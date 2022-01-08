using System.Windows;

namespace ChatClient
{
    public partial class LoginDialog : Window
    {
        public LoginDialog(string user)
        {
            InitializeComponent();
            Username.Text = user;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
