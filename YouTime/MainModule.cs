using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Networking;

namespace ChatClient
{
    public enum MessageType
    {
        info,
        error,
        warning,
        message
    }

    public class MainModule
    {
        public delegate void Updater();
        private Configuration.ConfigManager m_CManager;
        private List<MessageItem> m_ChatMsgs;
        private List<DataModelContact> m_ContactList;
        private Updater messageCallBack;
        private int m_CurrentChat;
        private int m_CurentUserId;
        List<DataModelChat> m_ChatList;
        private const string m_Connection = "Data Source=data.sqlite;Mode=ReadWrite;";
        private Queue<NetworkMessageItem> m_UserMessagesQueue;
        private Queue<NetworkMessageItem> m_ServerMessagesQueue;

        public MainModule(List<MessageItem> chatMsgs, Updater refreshMsg)
        {
            m_ChatMsgs = chatMsgs;
            m_CManager = new(Messager);
            messageCallBack = refreshMsg;
            m_ContactList = new();
            m_ContactList.Add(new DataModelContact()
            {
                ContactId = 0,
                Nickname = "System",
                RealName = "System",
                BackColor = new(Color.FromRgb(0, 255, 171))
            });
            m_UserMessagesQueue = new();
            m_ServerMessagesQueue = new();
            //TimerCallback callback = new TimerCallback(TimerMethod);
            //Timer timer = new(callback); //создаем объект таймера
            //timer.Change(1000, 2000); //через 1 сек каждые 2 сек
            //m_ChatList = DbWorker.getChatList(//TODO Add string connection here );
        }

        public void StartNetwork(string username, string password)
        {
            var network = new TCPClient(m_UserMessagesQueue, m_ServerMessagesQueue, Messager);
            Task task = new(() =>
            {
                network.ConnectToServer(m_CManager.AppConfig, username, password);
            });
            task.Start();
        }

        public void addUserMessage(string message)
        {
            //if (m_CurentUserId == 0) return; //Uncoment it
            var msg = new NetworkMessageItem()
            {
                type = NetworkMessageType.message,
                ChatId = m_CurrentChat,
                SenderId = m_CurentUserId,
                Message = message
            };
            m_UserMessagesQueue.Enqueue(msg);
        }

        public string GetCurrentUserNick()
        {
            foreach(var user in  m_ContactList)
            {
                if (user.ContactId == m_CurentUserId && m_CurentUserId != 0)
                    return user.Nickname;
            }
            return "User";
        }

        public string GetServerAdres()
        {
            return $"{m_CManager.AppConfig.GetServerIP()}:{m_CManager.AppConfig.GetServerPort()}";
        }

        public string GetCurrentChatName()
        {
            //ToDo add code to return string name of chat with m_CurrentChatId
            return "System chat";
        }

        public bool StartConfigMagager()
        {
            bool result = m_CManager.LoadConfig();
            m_CurrentChat = result ? m_CManager.AppConfig.CurrentChatId : 0;
            m_CurentUserId = m_CManager.AppConfig.UserId;
            return result;
        }

        public void AddNewConfig(string ip, string port)
        {
            m_CManager.AppConfig.CurrentChatId = 0;
            Messager(MessageType.info, 0, 0, $"Adding new server adress: {m_CManager.AppConfig.SetServerIp(ip)}");
            Messager(MessageType.info, 0, 0, $"Adding new server adress: {m_CManager.AppConfig.SetServerPort(port)}");
            m_CManager.AppConfig.SetLocalDB("data.sqlite");
            m_CManager.AppConfig.UserId = 0;
            m_CManager.SaveConfig();
        }

        public void Messager(MessageType type, int senderId, int chatId, string msg)
        {
            DataModelContact sender = GetContact(senderId);
            if(sender == null)
            {
                //Here will request to server for contact info
                return;
            }
            string messageTime = DateTime.Now.ToString();
            m_ChatMsgs.Add(new MessageItem(sender.Nickname, $"{sender.Nickname}:{msg}", 
                senderId == 0 ? SetMessageColor(type) : sender.BackColor, messageTime));
            int dbworkResult = DbWorker.AddMessage(chatId, messageTime, senderId, msg, m_Connection);
            if(dbworkResult == -1)
            {
                m_ChatMsgs.Add(new MessageItem("System", $"ERROR: Unable to access database!!!",
                SetMessageColor(MessageType.error), messageTime));
            }

            //messageCallBack();/*
            
        }

        private SolidColorBrush SetMessageColor(MessageType type)
        {
            switch (type)
            {
                case MessageType.info:
                    return new(Color.FromRgb(124, 195, 216));
                case MessageType.error:
                    return new(Color.FromRgb(255, 0, 15));
                case MessageType.warning:
                    return new(Color.FromRgb(254, 246, 1));
            }
            return new(Color.FromRgb(255, 255, 255));
        }

        private DataModelContact GetContact(int contact)
        {
            foreach (var c in m_ContactList)
            {
                if (c.ContactId == contact)
                    return c;
            }
            return null;
        }

        public void TimerMethod()
        {
            while (m_ServerMessagesQueue.Count > 0)
            {
                var msg = m_ServerMessagesQueue.Dequeue();
                Messager(MessageType.message, msg.SenderId, msg.ChatId, msg.Message);
            }
            //Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind,
            //        new Updater(messageCallBack));
        }
    }
}
