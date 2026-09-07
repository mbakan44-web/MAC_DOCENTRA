using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Docentra_Mac.Views.Dialogs
{
    public partial class MessageDialog : Window
    {
        public bool Result { get; private set; }

        public MessageDialog()
        {
            InitializeComponent();
        }

        public MessageDialog(string message) : this()
        {
            this.FindControl<TextBlock>("MessageText")!.Text = message;
        }

        private void Yes_Click(object? sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void No_Click(object? sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
