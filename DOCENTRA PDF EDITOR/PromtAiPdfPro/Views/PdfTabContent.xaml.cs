using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PdfiumViewer;
using PdfSharp.Drawing;
using PromtAiPdfPro.Services;
using Wpf.Ui.Controls;
using MsgBox = System.Windows.MessageBox;
using MsgBtn = System.Windows.MessageBoxButton;
using MsgImg = System.Windows.MessageBoxImage;

namespace PromtAiPdfPro.Views
{
    public partial class PdfTabContent : System.Windows.Controls.UserControl
    {
        private enum ViewerMode { View, Annotate }

        private enum AnnotTool { Pen, Highlighter, Eraser, Text, Stamp }

        private readonly record struct AnnotTextNote(double X, double Y, string Text, double FontSize, Color Color);
        private readonly record struct AnnotStampNote(double X, double Y, double Width, double Height, string StampId);
        private readonly record struct PageAnnotationData(
            StrokeCollection Strokes,
            List<AnnotTextNote> Texts,
            List<AnnotStampNote> Stamps,
            int Width,
            int Height);

        private static readonly Dictionary<string, (string LabelKey, string Display, Color Background, Color Foreground)> StampDefs =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["approved"] = ("Viewer_StampApproved", "ONAYLANDI", Color.FromRgb(200, 255, 200), Color.FromRgb(0, 100, 0)),
                ["draft"] = ("Viewer_StampDraft", "TASLAK", Color.FromRgb(255, 248, 200), Color.FromRgb(140, 90, 0)),
                ["confidential"] = ("Viewer_StampConfidential", "GİZLİ", Color.FromRgb(255, 210, 210), Color.FromRgb(140, 0, 0)),
                ["rejected"] = ("Viewer_StampRejected", "RED", Color.FromRgb(230, 230, 230), Color.FromRgb(80, 80, 80)),
            };

        private string _filePath;
        private readonly PdfService _pdfService = new PdfService();
        private readonly ObservableCollection<ThumbnailItem> _thumbnails = new();
        private readonly Dictionary<int, PageAnnotationData> _pageAnnotations = new();

        private PdfDocument? _pdfDoc;
        private ViewerMode _mode = ViewerMode.View;
        private AnnotTool _activeTool = AnnotTool.Pen;
        private int _currentPage = 1;
        private bool _suppressThumbnailSelection;
        private bool _webViewReady;
        private int _drawPixelWidth;
        private int _drawPixelHeight;
        private double _zoom = 1.0;
        private int _displayRequestId;
        private Color _inkColor = Colors.DodgerBlue;
        private bool _isInitialized;

        private const double BaseRenderDpi = 120;
        private const double ZoomStep = 0.2;
        private const double ZoomMin = 0.4;
        private const double ZoomMax = 3.0;

        public string FilePath => _filePath;

        public PdfTabContent(string filePath)
        {
            InitializeComponent();
            _filePath = filePath;
            ListThumbnails.ItemsSource = _thumbnails;

            Unloaded += (_, _) =>
            {
                DisposeDocument();
                _thumbnails.Clear();
                _pageAnnotations.Clear();
                try { PdfWebView?.Dispose(); } catch { }
            };

            Loaded += OnTabLoaded;
        }

        private void OnTabLoaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                InitColorPicker();
                InitStampPicker();
                LoadDocument();
                InitializeWebViewAsync();
                SetViewerMode(ViewerMode.View, reloadPage: true);
            }
            catch (Exception ex)
            {
                MsgBox.Show(
                    GetString("Msg_Error") + ": " + ex.Message,
                    GetString("Msg_Error"),
                    MsgBtn.OK,
                    MsgImg.Error);
            }
        }

        private static string GetString(string key, string fallback = "")
        {
            try
            {
                if (Application.Current?.TryFindResource(key) is string s && !string.IsNullOrEmpty(s))
                    return s;
            }
            catch { }
            return fallback;
        }

        private void InitColorPicker()
        {
            if (CmbInkColor == null) return;
            CmbInkColor.SelectionChanged -= CmbInkColor_SelectionChanged;
            CmbInkColor.Items.Clear();
            AddColorItem("Viewer_ColorBlue", Colors.DodgerBlue);
            AddColorItem("Viewer_ColorRed", Colors.Crimson);
            AddColorItem("Viewer_ColorBlack", Colors.Black);
            AddColorItem("Viewer_ColorGreen", Colors.ForestGreen);
            if (CmbInkColor.Items.Count > 0)
                CmbInkColor.SelectedIndex = 0;
            CmbInkColor.SelectionChanged += CmbInkColor_SelectionChanged;
        }

        private void AddColorItem(string resourceKey, Color color)
        {
            var item = new ComboBoxItem
            {
                Content = GetString(resourceKey, resourceKey),
                Tag = color
            };
            CmbInkColor.Items.Add(item);
        }

        private void InitStampPicker()
        {
            if (CmbStampType == null) return;
            CmbStampType.Items.Clear();
            foreach (var kv in StampDefs)
            {
                string label = GetString(kv.Value.LabelKey, kv.Value.Display);
                CmbStampType.Items.Add(new ComboBoxItem { Content = label, Tag = kv.Key });
            }
            if (CmbStampType.Items.Count > 0)
                CmbStampType.SelectedIndex = 0;
        }

        private async void InitializeWebViewAsync()
        {
            try
            {
                string userData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Docentra", "WebView2Data");
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userData);
                await PdfWebView.EnsureCoreWebView2Async(env);

                var core = PdfWebView.CoreWebView2;
                if (core == null) return;

                core.Settings.HiddenPdfToolbarItems =
                    Microsoft.Web.WebView2.Core.CoreWebView2PdfToolbarItems.MoreSettings;

                if (File.Exists(_filePath))
                {
                    string uri = new Uri(Path.GetFullPath(_filePath)).AbsoluteUri + "#page=" + _currentPage;
                    PdfWebView.Source = new Uri(uri);
                }

                core.NavigationCompleted += (_, _) =>
                {
                    _webViewReady = true;
                    if (_mode == ViewerMode.View)
                        LoadingRing.Visibility = Visibility.Collapsed;
                };

                core.NewWindowRequested += (s, e) => { e.Handled = true; OpenNativeSettings(); };
                core.WebMessageReceived += (s, e) =>
                {
                    if (e.TryGetWebMessageAsString() == "OPEN_SETTINGS") OpenNativeSettings();
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("WebView2: " + ex.Message);
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenNativeSettings()
        {
            if (Application.Current.MainWindow is MainView mainView)
            {
                mainView.RootNavigation.Navigate(typeof(SettingsPage));
                mainView.Show();
                mainView.Activate();
            }
        }

        private void DisposeDocument()
        {
            if (_pdfDoc == null) return;
            _pdfDoc.Dispose();
            _pdfDoc = null;
        }

        private void LoadDocument()
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                DisposeDocument();
                _pdfDoc = PdfDocument.Load(_filePath);
                GenerateThumbnails();
                UpdatePageIndicators();
            }
            catch (Exception ex)
            {
                MsgBox.Show("PDF Load Error: " + ex.Message);
            }
        }

        private void UpdatePageIndicators()
        {
            if (TxtAnnotPage == null) return;
            int total = _pdfDoc?.PageCount ?? 0;
            string fmt = GetString("Viewer_PageOf", "{0} / {1}");
            TxtAnnotPage.Text = string.Format(fmt, _currentPage, total);
        }

        private void GenerateThumbnails()
        {
            _thumbnails.Clear();
            if (_pdfDoc == null) return;

            var doc = _pdfDoc;
            _ = Task.Run(async () =>
            {
                for (int i = 0; i < doc.PageCount; i++)
                {
                    if (i >= 5) await Task.Delay(80);
                    int pageNum = i + 1;
                    using var image = doc.Render(i, 120, 160, true);
                    using var ms = new MemoryStream();
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    Dispatcher.Invoke(() => _thumbnails.Add(new ThumbnailItem { PageNumber = pageNum, Image = bitmap }));
                }

                Dispatcher.Invoke(() =>
                {
                    if (ListThumbnails.SelectedItem == null && _thumbnails.Count > 0)
                    {
                        _suppressThumbnailSelection = true;
                        ListThumbnails.SelectedIndex = 0;
                        _suppressThumbnailSelection = false;
                    }
                });
            });
        }

        private void BtnModeView_Click(object sender, RoutedEventArgs e) => SetViewerMode(ViewerMode.View, reloadPage: true);

        private void BtnModeAnnotate_Click(object sender, RoutedEventArgs e) => SetViewerMode(ViewerMode.Annotate, reloadPage: true);

        private void SetViewerMode(ViewerMode mode, bool reloadPage)
        {
            if (PdfWebView == null || AnnotateSurface == null) return;

            if (mode == ViewerMode.View && _mode == ViewerMode.Annotate)
                CommitCurrentPageAnnotations();

            _mode = mode;
            bool isView = mode == ViewerMode.View;

            PdfWebView.Visibility = isView ? Visibility.Visible : Visibility.Collapsed;
            AnnotateSurface.Visibility = isView ? Visibility.Collapsed : Visibility.Visible;
            if (PanelAnnotateToolbar != null)
                PanelAnnotateToolbar.Visibility = isView ? Visibility.Collapsed : Visibility.Visible;

            if (BtnModeView != null)
                BtnModeView.Appearance = isView ? ControlAppearance.Primary : ControlAppearance.Secondary;
            if (BtnModeAnnotate != null)
                BtnModeAnnotate.Appearance = isView ? ControlAppearance.Secondary : ControlAppearance.Primary;

            if (TxtModeHint != null)
            {
                TxtModeHint.Text = isView
                    ? GetString("Viewer_ViewModeHint")
                    : GetString("Viewer_AnnotateModeHint");
            }

            if (!reloadPage) return;

            if (isView)
            {
                ApplyZoom(1);
                NavigateWebViewToPage(_currentPage);
            }
            else
            {
                SetAnnotTool(AnnotTool.Pen);
                _ = DisplayAnnotatePageAsync(_currentPage);
            }
        }

        private void NavigateWebViewToPage(int page)
        {
            if (!_webViewReady || PdfWebView.CoreWebView2 == null || !File.Exists(_filePath)) return;
            LoadingRing.Visibility = Visibility.Visible;

            string targetUri = new Uri(Path.GetFullPath(_filePath)).AbsoluteUri + "#page=" + page;

            EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>? handler = null;
            handler = (s, e) =>
            {
                PdfWebView.CoreWebView2.NavigationCompleted -= handler;
                PdfWebView.CoreWebView2.Navigate(targetUri);
            };

            PdfWebView.CoreWebView2.NavigationCompleted += handler;
            PdfWebView.CoreWebView2.Navigate("about:blank");
        }

        private void ListThumbnails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressThumbnailSelection || ListThumbnails.SelectedItem is not ThumbnailItem item)
                return;
            GoToPage(item.PageNumber);
        }

        private void GoToPage(int pageNumber)
        {
            if (_pdfDoc == null || pageNumber < 1 || pageNumber > _pdfDoc.PageCount || pageNumber == _currentPage)
                return;

            if (_mode == ViewerMode.Annotate)
                CommitCurrentPageAnnotations();

            _currentPage = pageNumber;
            UpdatePageIndicators();
            SyncThumbnailSelection(pageNumber);

            if (_mode == ViewerMode.View)
                NavigateWebViewToPage(pageNumber);
            else
                _ = DisplayAnnotatePageAsync(pageNumber);
        }

        private void BtnAnnotPrev_Click(object sender, RoutedEventArgs e) => GoToPage(_currentPage - 1);
        private void BtnAnnotNext_Click(object sender, RoutedEventArgs e) => GoToPage(_currentPage + 1);

        private void SyncThumbnailSelection(int pageNumber)
        {
            foreach (var t in _thumbnails)
            {
                if (t.PageNumber != pageNumber) continue;
                _suppressThumbnailSelection = true;
                ListThumbnails.SelectedItem = t;
                ListThumbnails.ScrollIntoView(t);
                _suppressThumbnailSelection = false;
                return;
            }
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e) => ApplyZoom(_zoom + ZoomStep);
        private void BtnZoomOut_Click(object sender, RoutedEventArgs e) => ApplyZoom(_zoom - ZoomStep);

        private void ApplyZoom(double zoom)
        {
            _zoom = Math.Clamp(zoom, ZoomMin, ZoomMax);
            if (PageScale != null)
            {
                PageScale.ScaleX = _zoom;
                PageScale.ScaleY = _zoom;
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_filePath)) return;
            try
            {
                var printDoc = PdfDocument.Load(_filePath);
                var win = new PrintPreviewWindow(printDoc) { Owner = Window.GetWindow(this) };
                win.Closed += (_, _) => printDoc.Dispose();
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message, Application.Current.FindResource("Msg_Error") as string ?? "Error",
                    MsgBtn.OK, MsgImg.Error);
            }
        }

        #region Annotation tools

        private void BtnToolPen_Click(object sender, RoutedEventArgs e) => SetAnnotTool(AnnotTool.Pen);
        private void BtnToolHighlighter_Click(object sender, RoutedEventArgs e) => SetAnnotTool(AnnotTool.Highlighter);
        private void BtnToolEraser_Click(object sender, RoutedEventArgs e) => SetAnnotTool(AnnotTool.Eraser);
        private void BtnToolText_Click(object sender, RoutedEventArgs e) => SetAnnotTool(AnnotTool.Text);
        private void BtnToolStamp_Click(object sender, RoutedEventArgs e) => SetAnnotTool(AnnotTool.Stamp);

        private void SetAnnotTool(AnnotTool tool)
        {
            if (DrawInkCanvas == null || DrawPageHost == null) return;

            _activeTool = tool;
            if (CmbStampType != null)
                CmbStampType.Visibility = tool == AnnotTool.Stamp ? Visibility.Visible : Visibility.Collapsed;

            if (BtnToolPen != null)
                BtnToolPen.Appearance = tool == AnnotTool.Pen ? ControlAppearance.Primary : ControlAppearance.Secondary;
            if (BtnToolHighlighter != null)
                BtnToolHighlighter.Appearance = tool == AnnotTool.Highlighter ? ControlAppearance.Primary : ControlAppearance.Secondary;
            if (BtnToolEraser != null)
                BtnToolEraser.Appearance = tool == AnnotTool.Eraser ? ControlAppearance.Primary : ControlAppearance.Secondary;

            bool inkTool = tool is AnnotTool.Pen or AnnotTool.Highlighter or AnnotTool.Eraser;
            DrawInkCanvas.IsHitTestVisible = inkTool;
            if (OverlayCanvas != null)
                OverlayCanvas.IsHitTestVisible = false;
            DrawPageHost.Cursor = inkTool ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Cross;

            if (tool == AnnotTool.Eraser)
            {
                DrawInkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                return;
            }

            if (tool is AnnotTool.Pen or AnnotTool.Highlighter)
            {
                DrawInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                ApplyInkAttributes();
            }
            else
            {
                DrawInkCanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void ApplyInkAttributes()
        {
            if (DrawInkCanvas == null) return;
            if (_activeTool == AnnotTool.Highlighter)
            {
                var c = _inkColor;
                DrawInkCanvas.DefaultDrawingAttributes.Color = Color.FromArgb(110, c.R, c.G, c.B);
                double w = Math.Max(8, SliderThickness.Value * 2.5);
                DrawInkCanvas.DefaultDrawingAttributes.Width = w;
                DrawInkCanvas.DefaultDrawingAttributes.Height = w;
            }
            else
            {
                DrawInkCanvas.DefaultDrawingAttributes.Color = _inkColor;
                DrawInkCanvas.DefaultDrawingAttributes.Width = SliderThickness.Value;
                DrawInkCanvas.DefaultDrawingAttributes.Height = SliderThickness.Value;
            }
        }

        private void CmbInkColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbInkColor.SelectedItem is ComboBoxItem { Tag: Color c })
            {
                _inkColor = c;
                ApplyInkAttributes();
            }
        }

        private void SliderThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyInkAttributes();

        private void CmbStampType_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void DrawInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e) { }

        private void DrawPageHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_mode != ViewerMode.Annotate || _drawPixelWidth <= 0) return;

            var pos = e.GetPosition(DrawPageHost);
            if (pos.X < 0 || pos.Y < 0 || pos.X > _drawPixelWidth || pos.Y > _drawPixelHeight) return;

            if (_activeTool == AnnotTool.Text)
            {
                string? text = PromptTextInput();
                if (!string.IsNullOrWhiteSpace(text))
                    AddTextNote(pos.X, pos.Y, text.Trim());
                return;
            }

            if (_activeTool == AnnotTool.Stamp)
            {
                string stampId = GetSelectedStampId();
                double w = Math.Min(200, _drawPixelWidth * 0.35);
                double h = w * 0.35;
                AddStampNote(pos.X - w / 2, pos.Y - h / 2, w, h, stampId);
            }
        }

        private string GetSelectedStampId()
        {
            if (CmbStampType.SelectedItem is ComboBoxItem { Tag: string id })
                return id;
            return "approved";
        }

        private string? PromptTextInput()
        {
            var dlg = new Window
            {
                Title = Application.Current.FindResource("Viewer_ToolText") as string ?? "Text",
                Width = 420,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize
            };
            var panel = new System.Windows.Controls.StackPanel { Margin = new Thickness(16) };
            var box = new System.Windows.Controls.TextBox { Height = 32, Margin = new Thickness(0, 0, 0, 12) };
            var ok = new System.Windows.Controls.Button
            {
                Content = Application.Current.FindResource("Common_OK") as string ?? "OK",
                Width = 90,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };
            ok.Click += (_, _) => { dlg.DialogResult = true; dlg.Close(); };
            panel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = Application.Current.FindResource("Viewer_TextPrompt") as string ?? "Enter note:",
                Margin = new Thickness(0, 0, 0, 8)
            });
            panel.Children.Add(box);
            panel.Children.Add(ok);
            dlg.Content = panel;
            return dlg.ShowDialog() == true ? box.Text : null;
        }

        private void AddTextNote(double x, double y, string text)
        {
            var tb = new System.Windows.Controls.TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(_inkColor),
                FontSize = 14 + SliderThickness.Value,
                FontWeight = FontWeights.SemiBold,
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                Padding = new Thickness(4, 2, 4, 2),
                Tag = "text"
            };
            System.Windows.Controls.Canvas.SetLeft(tb, x);
            System.Windows.Controls.Canvas.SetTop(tb, y);
            OverlayCanvas.Children.Add(tb);
        }

        private static string GetStampDisplayText(string stampId)
        {
            if (!StampDefs.TryGetValue(stampId, out var def)) return stampId;
            return Application.Current.FindResource(def.LabelKey) as string ?? def.Display;
        }

        private void AddStampNote(double x, double y, double w, double h, string stampId)
        {
            if (!StampDefs.TryGetValue(stampId, out var def)) return;

            var border = new Border
            {
                Width = w,
                Height = h,
                Background = new SolidColorBrush(def.Background),
                BorderBrush = new SolidColorBrush(def.Foreground),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Child = new System.Windows.Controls.TextBlock
                {
                    Text = GetStampDisplayText(stampId),
                    Foreground = new SolidColorBrush(def.Foreground),
                    FontWeight = FontWeights.Bold,
                    FontSize = Math.Max(12, h * 0.35),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Tag = stampId
            };
            System.Windows.Controls.Canvas.SetLeft(border, Math.Clamp(x, 0, Math.Max(0, _drawPixelWidth - w)));
            System.Windows.Controls.Canvas.SetTop(border, Math.Clamp(y, 0, Math.Max(0, _drawPixelHeight - h)));
            OverlayCanvas.Children.Add(border);
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (DrawInkCanvas.Strokes.Count > 0)
            {
                DrawInkCanvas.Strokes.RemoveAt(DrawInkCanvas.Strokes.Count - 1);
                return;
            }
            if (OverlayCanvas.Children.Count > 0)
                OverlayCanvas.Children.RemoveAt(OverlayCanvas.Children.Count - 1);
        }

        private void BtnClearPage_Click(object sender, RoutedEventArgs e)
        {
            DrawInkCanvas.Strokes.Clear();
            OverlayCanvas.Children.Clear();
            _pageAnnotations.Remove(_currentPage);
        }

        #endregion

        #region Page state

        private static StrokeCollection CloneStrokes(StrokeCollection source)
        {
            var copy = new StrokeCollection();
            foreach (Stroke stroke in source)
                copy.Add(stroke.Clone());
            return copy;
        }

        private PageAnnotationData CaptureCurrentPageAnnotations()
        {
            var texts = new List<AnnotTextNote>();
            foreach (UIElement child in OverlayCanvas.Children)
            {
                if (child is System.Windows.Controls.TextBlock tb)
                {
                    texts.Add(new AnnotTextNote(
                        System.Windows.Controls.Canvas.GetLeft(tb),
                        System.Windows.Controls.Canvas.GetTop(tb),
                        tb.Text,
                        tb.FontSize,
                        (tb.Foreground as SolidColorBrush)?.Color ?? Colors.Black));
                }
            }

            var stamps = new List<AnnotStampNote>();
            foreach (UIElement child in OverlayCanvas.Children)
            {
                if (child is Border b && b.Tag is string stampId)
                {
                    stamps.Add(new AnnotStampNote(
                        Canvas.GetLeft(b), Canvas.GetTop(b), b.Width, b.Height, stampId));
                }
            }

            return new PageAnnotationData(
                CloneStrokes(DrawInkCanvas.Strokes),
                texts,
                stamps,
                _drawPixelWidth,
                _drawPixelHeight);
        }

        private void CommitCurrentPageAnnotations()
        {
            if (_drawPixelWidth <= 0 || _drawPixelHeight <= 0) return;

            var data = CaptureCurrentPageAnnotations();
            if (data.Strokes.Count == 0 && data.Texts.Count == 0 && data.Stamps.Count == 0)
            {
                _pageAnnotations.Remove(_currentPage);
                return;
            }
            _pageAnnotations[_currentPage] = data;
        }

        private void RestorePageAnnotations(PageAnnotationData data)
        {
            DrawInkCanvas.Strokes.Clear();
            OverlayCanvas.Children.Clear();

            foreach (Stroke s in data.Strokes)
                DrawInkCanvas.Strokes.Add(s.Clone());

            foreach (var t in data.Texts)
            {
                var tb = new System.Windows.Controls.TextBlock
                {
                    Text = t.Text,
                    Foreground = new SolidColorBrush(t.Color),
                    FontSize = t.FontSize,
                    FontWeight = FontWeights.SemiBold,
                    Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    Padding = new Thickness(4, 2, 4, 2),
                    Tag = "text"
                };
                System.Windows.Controls.Canvas.SetLeft(tb, t.X);
                System.Windows.Controls.Canvas.SetTop(tb, t.Y);
                OverlayCanvas.Children.Add(tb);
            }

            foreach (var s in data.Stamps)
                AddStampNote(s.X, s.Y, s.Width, s.Height, s.StampId);
        }

        private void ClearAnnotationSurface()
        {
            DrawInkCanvas.Strokes.Clear();
            OverlayCanvas.Children.Clear();
        }

        private async Task DisplayAnnotatePageAsync(int pageNumber)
        {
            if (_pdfDoc == null || pageNumber < 1 || pageNumber > _pdfDoc.PageCount) return;

            int requestId = ++_displayRequestId;
            LoadingRing.Visibility = Visibility.Visible;

            try
            {
                var doc = _pdfDoc;
                int idx = pageNumber - 1;

                byte[] pngBytes = await Task.Run(() =>
                {
                    var size = doc.PageSizes[idx];
                    int rw = Math.Max(1, (int)(size.Width * (BaseRenderDpi / 72.0)));
                    int rh = Math.Max(1, (int)(size.Height * (BaseRenderDpi / 72.0)));
                    using var img = doc.Render(idx, rw, rh, (int)BaseRenderDpi, (int)BaseRenderDpi, false);
                    using var ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    if (requestId != _displayRequestId || pageNumber != _currentPage) return;

                    using var ms = new MemoryStream(pngBytes);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    _drawPixelWidth = bitmap.PixelWidth;
                    _drawPixelHeight = bitmap.PixelHeight;
                    DrawPageHost.Width = _drawPixelWidth;
                    DrawPageHost.Height = _drawPixelHeight;
                    DrawPageImage.Width = _drawPixelWidth;
                    DrawPageImage.Height = _drawPixelHeight;
                    DrawPageImage.Source = bitmap;
                    DrawInkCanvas.Width = _drawPixelWidth;
                    DrawInkCanvas.Height = _drawPixelHeight;
                    OverlayCanvas.Width = _drawPixelWidth;
                    OverlayCanvas.Height = _drawPixelHeight;

                    ClearAnnotationSurface();
                    if (_pageAnnotations.TryGetValue(pageNumber, out var saved))
                        RestorePageAnnotations(saved);

                    ApplyInkAttributes();
                    SetAnnotTool(_activeTool);
                    ApplyZoom(_zoom);
                });
            }
            finally
            {
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private static byte[] RenderAnnotationOverlay(PageAnnotationData data)
        {
            int w = data.Width;
            int h = data.Height;
            var host = new Grid { Width = w, Height = h, Background = Brushes.Transparent };

            var ink = new InkCanvas { Width = w, Height = h, Background = Brushes.Transparent };
            foreach (Stroke stroke in data.Strokes)
                ink.Strokes.Add(stroke.Clone());

            var overlay = new Canvas { Width = w, Height = h };
            foreach (var t in data.Texts)
            {
                var tb = new System.Windows.Controls.TextBlock
                {
                    Text = t.Text,
                    Foreground = new SolidColorBrush(t.Color),
                    FontSize = t.FontSize,
                    FontWeight = FontWeights.SemiBold
                };
                Canvas.SetLeft(tb, t.X);
                Canvas.SetTop(tb, t.Y);
                overlay.Children.Add(tb);
            }
            foreach (var s in data.Stamps)
            {
                if (!StampDefs.TryGetValue(s.StampId, out var def)) continue;
                var border = new Border
                {
                    Width = s.Width,
                    Height = s.Height,
                    Background = new SolidColorBrush(def.Background),
                    BorderBrush = new SolidColorBrush(def.Foreground),
                    BorderThickness = new Thickness(2),
                    Child = new System.Windows.Controls.TextBlock
                    {
                        Text = GetStampDisplayText(s.StampId),
                        Foreground = new SolidColorBrush(def.Foreground),
                        FontWeight = FontWeights.Bold,
                        FontSize = Math.Max(12, s.Height * 0.35),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Canvas.SetLeft(border, s.X);
                Canvas.SetTop(border, s.Y);
                overlay.Children.Add(border);
            }

            host.Children.Add(ink);
            host.Children.Add(overlay);
            host.Measure(new System.Windows.Size(w, h));
            host.Arrange(new System.Windows.Rect(0, 0, w, h));
            host.UpdateLayout();

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(host);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var outMs = new MemoryStream();
            encoder.Save(outMs);
            return outMs.ToArray();
        }

        private bool PageHasAnnotations(PageAnnotationData d) =>
            d.Strokes.Count > 0 || d.Texts.Count > 0 || d.Stamps.Count > 0;

        private async void BtnSaveDrawings_Click(object sender, RoutedEventArgs e)
        {
            CommitCurrentPageAnnotations();

            if (_pageAnnotations.Count == 0)
            {
                MsgBox.Show(
                    Application.Current.FindResource("Viewer_NoInk") as string ?? "No annotations.",
                    Application.Current.FindResource("Msg_Warning") as string ?? "Warning",
                    MsgBtn.OK, MsgImg.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = Application.Current.FindResource("Common_PdfFilter") as string ?? "PDF|*.pdf",
                FileName = Path.GetFileNameWithoutExtension(_filePath) + "_annotated.pdf"
            };
            if (dlg.ShowDialog() != true) return;

            LoadingRing.Visibility = Visibility.Visible;
            var tempFiles = new List<string>();

            try
            {
                DisposeDocument();
                string workingPath = _filePath;

                foreach (int page in _pageAnnotations.Keys.OrderBy(p => p))
                {
                    var data = _pageAnnotations[page];
                    if (!PageHasAnnotations(data)) continue;

                    string pngPath = Path.Combine(Path.GetTempPath(), "docentra_ann_" + Guid.NewGuid().ToString("N") + ".png");
                    tempFiles.Add(pngPath);
                    File.WriteAllBytes(pngPath, RenderAnnotationOverlay(data));

                    string outPdf = Path.Combine(Path.GetTempPath(), "docentra_ann_" + Guid.NewGuid().ToString("N") + ".pdf");
                    tempFiles.Add(outPdf);

                    bool ok = await _pdfService.AddImageWatermarkAsync(
                        workingPath, outPdf, pngPath,
                        new XRect(0, 0, 1, 1), 1.0, page.ToString(CultureInfo.InvariantCulture), true);

                    if (!ok)
                    {
                        MsgBox.Show(
                            Application.Current.FindResource("Msg_ProcessError") as string ?? "Error",
                            Application.Current.FindResource("Msg_Error") as string ?? "Error",
                            MsgBtn.OK, MsgImg.Error);
                        LoadDocument();
                        if (_mode == ViewerMode.Annotate)
                            await DisplayAnnotatePageAsync(_currentPage);
                        else
                            NavigateWebViewToPage(_currentPage);
                        return;
                    }

                    if (!string.Equals(workingPath, _filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(workingPath); } catch { }
                    }
                    workingPath = outPdf;
                }

                File.Copy(workingPath, dlg.FileName, true);
                _filePath = dlg.FileName;
                _pageAnnotations.Clear();

                LoadDocument();
                if (_mode == ViewerMode.Annotate)
                {
                    ClearAnnotationSurface();
                    await DisplayAnnotatePageAsync(_currentPage);
                }
                else
                    NavigateWebViewToPage(_currentPage);

                MsgBox.Show(
                    Application.Current.FindResource("Viewer_DrawSaveOk") as string ?? "OK",
                    Application.Current.FindResource("Msg_Success") as string ?? "Success",
                    MsgBtn.OK, MsgImg.Information);
            }
            catch (Exception ex)
            {
                if (_pdfDoc == null) LoadDocument();
                MsgBox.Show(ex.Message, Application.Current.FindResource("Msg_Error") as string ?? "Error",
                    MsgBtn.OK, MsgImg.Error);
            }
            finally
            {
                LoadingRing.Visibility = Visibility.Collapsed;
                foreach (string f in tempFiles)
                {
                    try { if (File.Exists(f)) File.Delete(f); } catch { }
                }
            }
        }

        #endregion

        private void BtnEditPdf_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTool(typeof(ControlCenterPage), "Edit");
        }

        private void NavigateToTool(Type pageType, string action)
        {
            MainView? mainView = Application.Current.Windows.OfType<MainView>().FirstOrDefault();

            if (mainView == null)
            {
                // MainView henüz oluşturulmamış, yeni bir tane oluştur
                mainView = new MainView();
                Application.Current.MainWindow = mainView;
            }

            // Mevcut dosyayı navigasyon yardımcısına kaydet
            Helpers.NavigationHelper.PendingFilePath = _filePath;
            Helpers.NavigationHelper.TargetAction = action;

            // Ana pencereyi göster ve öne getir
            mainView.Show();
            mainView.Activate();
            mainView.WindowState = WindowState.Normal;

            // Pencere oluştuktan sonra Navigasyonu tetikle
            mainView.RootNavigation.Navigate(pageType);

            // Reader penceresini simge durumuna küçültebiliriz
            if (Window.GetWindow(this) is Window readerWin)
            {
                readerWin.WindowState = WindowState.Minimized;
            }
        }

        private void BtnConvertToWord_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTool(typeof(ConvertPage), "PdfToWord");
        }

        private void BtnProtect_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTool(typeof(PasswordSecurityPage), "Protect");
        }

        private void BtnSign_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTool(typeof(SignPage), "Sign");
        }

        private void BtnCompress_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTool(typeof(CompressPage), "Compress");
        }

        private void BtnMetadata_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTool(typeof(MetadataEditorPage), "Metadata");
        }
    }

    public class ThumbnailItem
    {
        public int PageNumber { get; set; }
        public ImageSource Image { get; set; } = null!;
    }
}
