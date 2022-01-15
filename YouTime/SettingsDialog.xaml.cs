using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {

        public SettingsDialog(string ip, string port)
        {
            InitializeComponent();
            ipAdress.Text = ip;
            portAdress.Text = port;
        }


        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
