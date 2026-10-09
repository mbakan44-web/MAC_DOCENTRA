using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Controls;

namespace PromtAiPdfPro.Views
{
    public partial class PdfReaderWindow : FluentWindow
    {
        private static PdfReaderWindow? _instance;
        public static PdfReaderWindow? Instance => _instance;

        private ObservableCollection<PdfTabItem> _tabs = new ObservableCollection<PdfTabItem>();

        public PdfReaderWindow(string? initialFilePath = null)
        {
            InitializeComponent();
            _instance = this;
            this.Closed += (s, e) =>
            {
                _instance = null;

                bool isDashboardVisible = false;
                foreach (Window win in Application.Current.Windows)
                {
                    if (win is MainView mv && win.IsVisible)
                    {
                        isDashboardVisible = true;
                        break;
                    }
                }

                if (!isDashboardVisible)
                {
                    Application.Current.Shutdown();
                }
            };

            if (Application.Current is App app)
            {
                app.ApplyTheme(Helpers.ThemeManager.CurrentTheme);
            }

            if (!string.IsNullOrEmpty(initialFilePath))
            {
                AddNewTab(initialFilePath);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Windows mesajı ile gelen yeni dosya yolu (WM_COPYDATA simülasyonu veya özel mesaj)
            // Basitlik adına App.xaml.cs'ten gelen direkt çağrıları bekliyoruz.
            return IntPtr.Zero;
        }

        public void AddNewTab(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            try {
                Dispatcher.Invoke(() => {
                    var existingTab = _tabs.FirstOrDefault(t => t.FilePath == filePath);
                    if (existingTab != null)
                    {
                        PdfTabView.SelectedItem = existingTab.ViewItem;
                        return;
                    }

                    var fileName = Path.GetFileName(filePath);
                    var content = new PdfTabContent(filePath);
                    
                    var tabItem = new TabViewItem
                    {
                        Header = CreateTabHeader(fileName, filePath),
                        Tag = filePath
                    };

                    var pdfTab = new PdfTabItem
                    {
                        FilePath = filePath,
                        Content = content,
                        ViewItem = tabItem
                    };

                    _tabs.Add(pdfTab);
                    PdfTabView.Items.Add(tabItem);
                    
                    // Seçimi tetikle
                    PdfTabView.SelectedItem = tabItem;

                    Services.RecentFilesService.AddFile(filePath);
                });
            } catch (Exception ex) {
                System.Windows.MessageBox.Show(
                    "Tab Error: " + ex.Message + (ex.InnerException != null ? "\n" + ex.InnerException.Message : ""),
                    "Docentra",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private UIElement CreateTabHeader(string fileName, string filePath)
        {
            var stack = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            
            var text = new System.Windows.Controls.TextBlock { 
                Text = fileName, 
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var closeBtn = new Wpf.Ui.Controls.Button {
                Icon = new SymbolIcon { Symbol = SymbolRegular.Dismiss24, FontSize = 12 },
                Appearance = ControlAppearance.Transparent,
                Padding = new Thickness(2),
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            closeBtn.Click += (s, e) => {
                e.Handled = true;
                CloseTab(filePath);
            };
            
            stack.Children.Add(text);
            stack.Children.Add(closeBtn);
            
            return stack;
        }

        private void UpdateTabContent(PdfTabItem tab)
        {
            if (tab == null || tab.Content == null) return;

            // Performans ve WindowsFormsHost uyumluluğu için Sil-Ekle yerine Görünürlük kullanıyoruz
            foreach (UIElement child in TabContentGrid.Children)
            {
                child.Visibility = Visibility.Collapsed;
            }

            if (!TabContentGrid.Children.Contains(tab.Content))
            {
                TabContentGrid.Children.Add(tab.Content);
            }

            tab.Content.Visibility = Visibility.Visible;
            this.Title = "Docentra PDF Reader - " + Path.GetFileName(tab.FilePath);
        }

        private void PdfTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PdfTabView.SelectedItem is TabViewItem selectedItem && selectedItem.Tag is string path)
            {
                var tab = _tabs.FirstOrDefault(t => t.FilePath == path);
                if (tab != null)
                {
                    UpdateTabContent(tab);
                }
            }
        }

        private void CloseTab(string path)
        {
            var tab = _tabs.FirstOrDefault(t => t.FilePath == path);
            if (tab != null)
            {
                _tabs.Remove(tab);
                PdfTabView.Items.Remove(tab.ViewItem);
                
                // Görsel ağaçtan temizle
                if (TabContentGrid.Children.Contains(tab.Content))
                {
                    TabContentGrid.Children.Remove(tab.Content);
                }

                if (_tabs.Count == 0)
                {
                    this.Close();
                }
                else if (PdfTabView.Items.Count > 0)
                {
                    if (PdfTabView.SelectedItem == null)
                        PdfTabView.SelectedIndex = 0;
                }
            }
        }

        private void BtnNewTab_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Select a PDF file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddNewTab(openFileDialog.FileName);
            }
        }

        private void BtnMainDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainView main)
            {
                main.Show();
                main.Activate();
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase)))
                {
                    e.Effects = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        AddNewTab(file);
                    }
                }
            }
        }
    }

    public class PdfTabItem
    {
        public string FilePath { get; set; } = string.Empty;
        public PdfTabContent Content { get; set; } = null!;
        public TabViewItem ViewItem { get; set; } = null!;
    }
}
