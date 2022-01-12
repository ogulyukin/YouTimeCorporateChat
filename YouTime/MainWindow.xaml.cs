using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        public delegate void MessageUpdater();
        List<MessageItem> m_ChatMsgs;
        MainModule m_MainModule;

        public MainWindow()
        {
            InitializeComponent();
            m_ChatMsgs = new();
            m_MainModule = new(m_ChatMsgs, RefreshMessageBox);
            if (!m_MainModule.StartConfigMagager())
            {
                var dialog = new SettingsDialog("127.0.0.1", "8005");
                dialog.ShowDialog();
                m_MainModule.AddNewConfig(dialog.ipAdress.Text, dialog.portAdress.Text);
            }
            IPadressBlock.Text = $"Server: {m_MainModule.GetServerAdres()}";
            ChatName.Text = m_MainModule.GetCurrentChatName();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginDialog(m_MainModule.GetCurrentUserNick());
            dialog.ShowDialog();
            m_MainModule.StartNetwork(dialog.Username.Text, dialog.Password.Password);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            m_MainModule.addUserMessage(NewMessageBlock.Text);
            NewMessageBlock.Text = "";
        }

        private void RefreshMessageBox()
        {
            MyMessageBox.Items.Clear();
            foreach (var it in m_ChatMsgs)
            {
                MyMessageBox.Items.Add(it);
            }
        }
                
        private void NewMessageBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            if(NewMessageBlock.Text == "")
            {
                NewMessageBlock.Text = "Введите сообщение";
                NewMessageBlock.Foreground = Brushes.Gray;
            }
        }

        private void NewMessageBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            NewMessageBlock.Foreground = Brushes.Black;
            if (NewMessageBlock.Text == "Введите сообщение")
            {
                NewMessageBlock.Text = "";
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            m_MainModule.TimerMethod();
            RefreshMessageBox();
        }
    }
}
