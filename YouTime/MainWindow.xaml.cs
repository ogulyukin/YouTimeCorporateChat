using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Threading;


namespace ChatClient
{
    public partial class MainWindow : Window
    {
        public delegate void ShowNewMessages();

        ServerConnector m_Connector;
        List<MessageItem> m_ChatMsgs;
        Timer m_Timer;

        public MainWindow()
        {
            InitializeComponent();
            NewMessageBlock.IsEnabled = false;
            m_Connector = new();
            m_ChatMsgs = new();
            TimerCallback callback = new(OnTimer);
            m_Timer = new(callback);
        }

        public void OnTimer(object obj)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind,
                    new ShowNewMessages(GetNewMessages));
            }
            catch (Exception exc)
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind,
                    new ShowNewMessages(GetErrorGettingData));
            }
        }

        private void GetNewMessages()
        {
            if (!m_Connector.isConnectedToServer()) return;
            m_ChatMsgs.Add(new(m_Connector.GetNewMessages(), new SolidColorBrush(Color.FromRgb(255, 0, 0))));
            RefreshMessageBox();
        }

        private void GetErrorGettingData()
        {
            m_ChatMsgs.Add(new("Uncnoun error", new SolidColorBrush(Color.FromRgb(255, 0, 0))));
            RefreshMessageBox();
        }


        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if(!Validator.ValidateIp(IPadressBlock.Text))
            {
                MessageBox.Show("Некорректный IP адрес сервера!", "Ошибка");
                return;
            }

            int port;
            string resultmesg = "";

            if(int.TryParse(PortBlock.Text, out port))
            {
                resultmesg = m_Connector.ConnectToServer(IPadressBlock.Text, port);
            }

            MessageBox.Show(resultmesg);

            if (resultmesg == "Соединение установлено")
            {
                NewMessageBlock.IsEnabled = true;
            }

            m_Timer.Change(3000, 3000);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

            if (!m_Connector.isConnectedToServer())
            {
                MessageBox.Show("Не установлено соединение\n с сервером", "Ошибка");
                return;
            }
            m_ChatMsgs.Add(new(NewMessageBlock.Text, new SolidColorBrush(Color.FromRgb(0, 255, 0))));
            m_Connector.SendMessage(NewMessageBlock.Text);
            RefreshMessageBox();
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

        private void IPadressBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IPadressBlock.Text == "")
            {
                IPadressBlock.Text = "Введите IP адресс";
                IPadressBlock.Foreground = Brushes.Gray;
            }
        }

        private void IPadressBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            if(IPadressBlock.Text == "Введите IP адресс")
            {
                IPadressBlock.Text = "";
                IPadressBlock.Foreground = Brushes.Black;
            }
        }

        private void PortBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PortBlock.Text == "")
            {
                PortBlock.Text = "Введите номер порта";
                PortBlock.Foreground = Brushes.Gray;
            }
        }

        private void PortBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            if(PortBlock.Text == "Введите номер порта")
            {
                PortBlock.Text = "";
                PortBlock.Foreground = Brushes.Black;
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
    }
}
