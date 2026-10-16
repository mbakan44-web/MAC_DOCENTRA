using System;
using System.Linq;
using System.Windows;
using PromtAiPdfPro.Views;

namespace PromtAiPdfPro.Helpers
{
    public static class AppMessageBox
    {
        public static Window? GetActiveOwner(Window? preferred = null)
        {
            if (preferred != null && preferred.IsVisible)
                return preferred;

            if (Application.Current != null)
            {
                var active = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w.IsVisible && !(w is DocentraDialog));
                if (active != null) return active;

                if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible && !(Application.Current.MainWindow is DocentraDialog))
                    return Application.Current.MainWindow;

                var anyVisible = Application.Current.Windows.OfType<Window>().LastOrDefault(w => w.IsVisible && !(w is DocentraDialog));
                if (anyVisible != null) return anyVisible;
            }

            return null;
        }

        public static MessageBoxResult Show(
            string messageBoxText,
            string caption = "Docentra",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            System.Windows.MessageBoxOptions options = System.Windows.MessageBoxOptions.None)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                return Application.Current.Dispatcher.Invoke(() => Show(messageBoxText, caption, button, icon, defaultResult, options));
            }

            try
            {
                var winOwner = GetActiveOwner();
                if (winOwner != null)
                {
                    try
                    {
                        if (winOwner.WindowState == WindowState.Minimized)
                            winOwner.WindowState = WindowState.Normal;
                        winOwner.Activate();
                        winOwner.Focus();
                    }
                    catch { }
                }

                var dialog = new DocentraDialog(messageBoxText, caption, button, icon);
                if (winOwner != null)
                {
                    dialog.Owner = winOwner;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                dialog.Topmost = true;
                dialog.ShowDialog();
                return dialog.Result;
            }
            catch
            {
                // Güvenli yedek fallback
                var owner = GetActiveOwner();
                if (owner != null)
                    return System.Windows.MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options);

                return System.Windows.MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            }
        }

        public static MessageBoxResult Show(string messageBoxText)
        {
            return Show(messageBoxText, "Docentra", MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption = "Docentra", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            return Show(messageBoxText, caption, button, icon);
        }
    }
}
