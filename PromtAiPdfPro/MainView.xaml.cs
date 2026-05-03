using System.Windows;
using Wpf.Ui.Controls;
using PromtAiPdfPro.Services;
using System;
namespace PromtAiPdfPro
{
    public partial class MainView : Wpf.Ui.Controls.FluentWindow
    {
        public Wpf.Ui.SnackbarService SnackbarService { get; } = new();
        private readonly LicenseService _licenseService = new LicenseService();
        private readonly UpdateService _updateService = new UpdateService();

        public MainView()
        {
            InitializeComponent();
            
            SnackbarService.SetSnackbarPresenter(RootSnackbar);
            InitializeLicenseStatus();

            RootNavigation.Navigating += RootNavigation_Navigating;

            Loaded += async (s, e) =>
            {
                WindowBackdropType = WindowBackdropType.Mica;
                RootNavigation.Navigate(typeof(Views.ControlCenterPage));

                // Profesyonel Güncelleme Kontrolü
                var (isAvailable, newVersion, downloadUrl) = await _updateService.CheckForUpdatesAsync();
                if (isAvailable)
                {
                    var status = await _licenseService.CheckLicenseAsync();
                    
                    string title = (string)Application.Current.FindResource("Update_AvailableTitle") ?? "Update Available";
                    string msg = (string)Application.Current.FindResource("Msg_UpdateAvailable") ?? "New version {0} is available!";
                    string btnText = (string)Application.Current.FindResource("Btn_DownloadNow") ?? "Download Now";

                    if (status.IsPremium)
                    {
                        string premiumMsg = (string)Application.Current.FindResource("Msg_UpdatePremiumFree") ?? "This update is free for you as a Premium member!";
                        msg += "\n" + premiumMsg;
                    }

                    SnackbarService.Show(
                        title,
                        string.Format(msg, newVersion),
                        Wpf.Ui.Controls.ControlAppearance.Primary,
                        new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ArrowDownload24),
                        TimeSpan.FromSeconds(15)
                    );

                    // Add a button to the snackbar if possible, or just open URL on click
                    // Wpf.Ui Snackbar doesn't easily support buttons in the simple Show call, 
                    // but we can use the URL. For now, let's keep it simple or open on click if we had an event.
                    // Instead, let's just show the message and maybe open the link.
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
            // Lisans durumunu tekrar kontrol et ve UI'ı güncelle
            _ = UpdateLicenseStatusAsync();
        }

        private async Task UpdateLicenseStatusAsync()
        {
            var status = await _licenseService.CheckLicenseAsync();
            UpdateTrialBanner(status);
        }

        private void UpdateTrialBanner(LicenseService.LicenseStatus status)
        {
            if (status == null) return;

            string daysMsg = Application.Current.TryFindResource("Prem_DaysRemaining") as string ?? "{0} Days Remaining";
            
            if (status.Status == LicenseService.AppStatus.FullTrial)
            {
                TxtTrialTitle.Text = Application.Current.TryFindResource("Prem_TrialBanner") as string ?? "Premium Trial";
                TxtTrialDays.Text = string.Format(daysMsg, status.DaysRemaining);
                TrialBanner.Visibility = Visibility.Visible;
                TrialBanner.ToolTip = status.StatusMessage;
            }
            else if (status.Status == LicenseService.AppStatus.DiscountTrial)
            {
                TxtTrialTitle.Text = Application.Current.TryFindResource("Prem_LimitedMode") as string ?? "Limited Mode";
                TxtTrialDays.Text = string.Format(daysMsg, status.DaysRemaining);
                TxtLockStatus.Text = Application.Current.TryFindResource("Prem_SomePagesLocked") as string ?? "Some pages locked";
                TxtLockStatus.Visibility = Visibility.Visible;
                TrialBanner.Visibility = Visibility.Visible;
                TrialBanner.ToolTip = status.LockReason;
                LockPremiumFeatures(status.LockReason);
            }
            else if (status.Status == LicenseService.AppStatus.RestrictedFree)
            {
                TxtTrialTitle.Text = Application.Current.TryFindResource("Prem_RestrictedMode") as string ?? "Restricted Mode";
                TxtTrialDays.Text = Application.Current.TryFindResource("Prem_RestrictedMode") as string ?? "Restricted Mode"; 
                TxtLockStatus.Text = Application.Current.TryFindResource("Prem_PageLimitActive") as string ?? "5 Page Limit Active";
                TxtLockStatus.Visibility = Visibility.Visible;
                TrialBanner.Visibility = Visibility.Visible;
                TrialBanner.ToolTip = status.LockReason;
                LockPremiumFeatures(status.LockReason);
            }
            
            if (status.IsPremium)
            {
                TrialBanner.Visibility = Visibility.Collapsed;
            }
        }

        private void RootNavigation_Navigating(Wpf.Ui.Controls.NavigationView sender, Wpf.Ui.Controls.NavigatingCancelEventArgs args)
        {
            var targetPage = args.Page?.GetType().Name;
            if (targetPage != null && !_licenseService.ValidateAccess(targetPage))
            {
                args.Cancel = true; // Navigasyonu iptal et!
                
                var status = _licenseService.CheckLicenseAsync().Result; // Statik cache'den hemen döner
                
                // Kullanıcının istediği yerelleştirilmiş uyarı
                string msg = (string)Application.Current.FindResource("Prem_PurchaseRequired_Msg");
                string title = (string)Application.Current.FindResource("Prem_PurchaseRequired_Title");
                
                // Daha güzel ve premium bir MessageBox (Wpf.Ui)
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = title,
                    Content = msg,
                    PrimaryButtonText = (string)Application.Current.FindResource("Prem_GoToPremium"),
                    CloseButtonText = (string)Application.Current.FindResource("Prem_Close"),
                    MaxWidth = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var result = uiMessageBox.ShowDialogAsync().Result;

                // Eğer Premium'a git butonuna basıldıysa veya her durumda yönlendir (Kullanıcının önceki talebi doğrultusunda)
                RootNavigation.Navigate(typeof(Views.PremiumPage));
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
                        // navItem.IsEnabled = false; // Tıklanabilir kalsın!
                        navItem.ToolTip = reason;
                        navItem.Foreground = System.Windows.Media.Brushes.Gray; // Görsel olarak farklı olsun
                    }
                }
            }
        }

        private void RootNavigation_BackRequested(Wpf.Ui.Controls.NavigationView sender, RoutedEventArgs args)
        {
            if (RootNavigation.CanGoBack)
            {
                RootNavigation.GoBack();
            }
        }
    }
}
