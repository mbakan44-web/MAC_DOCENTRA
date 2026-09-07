using System.Windows;
using Wpf.Ui.Controls;
using PromtAiPdfPro.Services;
using System;
using System.Threading.Tasks;

namespace PromtAiPdfPro
{
    public partial class MainView : Wpf.Ui.Controls.FluentWindow
    {
        public Wpf.Ui.SnackbarService SnackbarService { get; } = new();
        private readonly LicenseService _licenseService = new LicenseService();
        private readonly UpdateService _updateService = new UpdateService();
        private bool _hasNavigatedInitially = false;

        public MainView()
        {
            InitializeComponent();
            
            SnackbarService.SetSnackbarPresenter(RootSnackbar);
            InitializeLicenseStatus();

            RootNavigation.Navigating += RootNavigation_Navigating;

            Loaded += async (s, e) =>
            {
                if (!_hasNavigatedInitially)
                {
                    RootNavigation.Navigate(typeof(Views.ControlCenterPage));
                }

                // Kayıt Defteri Entegrasyonlarını Güncelle
                _ = Task.Run(() => Helpers.FileAssociationHelper.RegisterContextMenu());

                try {
                    // Profesyonel Güncelleme Kontrolü
                    var (isAvailable, newVersion, downloadUrl) = await _updateService.CheckForUpdatesAsync();
                    if (isAvailable)
                    {
                        string title = (string)Application.Current.FindResource("Update_AvailableTitle") ?? "Update Available";
                        string msg = (string)Application.Current.FindResource("Msg_UpdateAvailable") ?? "New version {0} is available!";
                        
                        SnackbarService.Show(
                            title,
                            string.Format(msg, newVersion),
                            Wpf.Ui.Controls.ControlAppearance.Primary,
                            new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ArrowDownload24),
                            TimeSpan.FromSeconds(15)
                        );
                    }
                } catch { }
            };

            Closing += (s, e) =>
            {
                bool isPdfReaderOpen = false;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.PdfReaderWindow && window.IsVisible)
                    {
                        isPdfReaderOpen = true;
                        break;
                    }
                }

                if (isPdfReaderOpen)
                {
                    e.Cancel = true;
                    this.Hide();
                }
                else
                {
                    Services.IpcService.StopServer();
                    Application.Current.Shutdown();
                }
            };
        }

        private async void InitializeLicenseStatus()
        {
            var status = await _licenseService.CheckLicenseAsync();
            UpdateTrialBanner(status);
        }

        public void RefreshLocalization()
        {
            _ = UpdateLicenseStatusAsync();
        }

        private async Task UpdateLicenseStatusAsync()
        {
            var status = await _licenseService.CheckLicenseAsync();
            UpdateTrialBanner(status);
        }

        private void UpdateTrialBanner(LicenseService.LicenseStatus status)
        {
            if (status == null || TxtTrialTitle == null) return;

            string daysMsg = Application.Current.TryFindResource("Prem_DaysRemaining") as string ?? "{0} Days Remaining";
            
            if (status.Status == LicenseService.AppStatus.FullTrial)
            {
                TxtTrialTitle.Text = Application.Current.TryFindResource("Prem_TrialBanner") as string ?? "Premium Trial";
                TxtTrialDays.Text = string.Format(daysMsg, status.DaysRemaining);
                TrialBanner.Visibility = Visibility.Visible;
            }
            else if (status.Status == LicenseService.AppStatus.DiscountTrial || status.Status == LicenseService.AppStatus.RestrictedFree)
            {
                TxtTrialTitle.Text = status.Status == LicenseService.AppStatus.DiscountTrial ? 
                    (Application.Current.TryFindResource("Prem_LimitedMode") as string ?? "Limited Mode") :
                    (Application.Current.TryFindResource("Prem_RestrictedMode") as string ?? "Restricted Mode");

                TxtTrialDays.Text = string.Format(daysMsg, status.DaysRemaining);
                TxtLockStatus.Text = status.LockReason ?? "Locked";
                TxtLockStatus.Visibility = Visibility.Visible;
                TrialBanner.Visibility = Visibility.Visible;
                LockPremiumFeatures(status.LockReason);
            }
            
            if (status.IsPremium)
            {
                TrialBanner.Visibility = Visibility.Collapsed;
            }
        }

        private void RootNavigation_Navigating(Wpf.Ui.Controls.NavigationView sender, Wpf.Ui.Controls.NavigatingCancelEventArgs args)
        {
            _hasNavigatedInitially = true;
            // Senkron olay içerisinde asla .Result veya .Wait kullanma! (Deadlock önleyici)
            var targetPage = args.Page?.GetType().Name;
            
            // Ayarlar sayfasına her zaman izin ver
            if (targetPage == "SettingsPage") return;

            if (targetPage != null && !_licenseService.ValidateAccess(targetPage))
            {
                args.Cancel = true; 
                
                // Asenkron işlemi UI'ı dondurmadan ayrı bir Task olarak başlat
                Dispatcher.BeginInvoke(new Action(async () => {
                    string msg = (string)Application.Current.FindResource("Prem_PurchaseRequired_Msg") ?? "Premium Required";
                    string title = (string)Application.Current.FindResource("Prem_PurchaseRequired_Title") ?? "Upgrade";
                    
                    var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = title,
                        Content = msg,
                        PrimaryButtonText = (string)Application.Current.FindResource("Prem_GoToPremium"),
                        CloseButtonText = (string)Application.Current.FindResource("Prem_Close"),
                        MaxWidth = 450,
                        Owner = this
                    };

                    await uiMessageBox.ShowDialogAsync();
                    RootNavigation.Navigate(typeof(Views.PremiumPage));
                }));
            }
        }

        private void LockPremiumFeatures(string reason)
        {
            foreach (var item in RootNavigation.MenuItems)
            {
                if (item is Wpf.Ui.Controls.NavigationViewItem navItem)
                {
                    var target = navItem.TargetPageType?.Name;
                    if (target != null && !_licenseService.ValidateAccess(target))
                    {
                        navItem.Foreground = System.Windows.Media.Brushes.Gray;
                        navItem.ToolTip = reason;
                    }
                }
            }
        }

        private void RootNavigation_BackRequested(Wpf.Ui.Controls.NavigationView sender, RoutedEventArgs args)
        {
            if (RootNavigation.CanGoBack) RootNavigation.GoBack();
        }
    }
}
