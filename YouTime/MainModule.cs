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
        message,
        file,
        image,
        autorisation,
        requestMessages,
        requestContact
    }

    public class MainModule
    {
        public delegate void Updater();
        private Configuration.ConfigManager m_CManager;
        private List<MessageItem> m_ChatMsgs;
        private List<DataModelContact> m_ContactList;
        private int m_CurrentChat;
        private int m_CurentUserId;
        private int m_LastChatId;
        private int m_LastMessageId;
        private int m_LastContactId;
        //private bool isConnected;
        List<DataModelChat> m_ChatList;
        private const string m_Connection = "Data Source=data.sqlite;Mode=ReadWrite;";
        private Queue<NetworkMessageItem> m_UserMessagesQueue;
        private Queue<NetworkMessageItem> m_ServerMessagesQueue;

        public MainModule(List<MessageItem> chatMsgs, Updater refreshMsg)
        {
            m_ChatMsgs = chatMsgs;
            m_CManager = new(Messager);
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
            m_LastContactId = DbWorker.GetLastContactId(m_Connection);
            m_LastMessageId = DbWorker.GetLastMessageId(m_CurrentChat, m_Connection);
            m_LastChatId = DbWorker.GetLastChatId(m_Connection);
        }

        public void StartNetwork(string username, string password)
        {
            var network = new TCPClient(m_UserMessagesQueue, m_ServerMessagesQueue);
            Task task = new(() =>
            {
                network.ConnectToServer(m_CManager.AppConfig, username, password);
            });
            m_ServerMessagesQueue.Clear();
            CreateMessageToServer(MessageType.autorisation, username, password);
            task.Start();
        }

        public void addUserMessage(string message)
        {
            //if (m_CurentUserId == 0 || !isConnected) return; //Uncoment!!!!
            var userMessage = message.Replace('|', ' ');
            var msg = new NetworkMessageItem()
            {
                type = MessageType.message,
                ChatId = m_CurrentChat,
                SenderId = m_CurentUserId,
                Message = userMessage
            };
            m_UserMessagesQueue.Enqueue(msg);
        }

        public string GetCurrentUserNick()
        {
            foreach (var user in m_ContactList)
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
            return "Common chat";
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
            Messager(MessageType.info, 0, 0, $"Adding new server adress: {m_CManager.AppConfig.SetServerIp(ip)}", 0);
            Messager(MessageType.info, 0, 0, $"Adding new server adress: {m_CManager.AppConfig.SetServerPort(port)}", 0);
            m_CManager.AppConfig.SetLocalDB("data.sqlite");
            m_CManager.AppConfig.UserId = 0;
            m_CManager.SaveConfig();
        }

        public void Messager(MessageType type, int senderId, int chatId, string msg, int id)
        {
            if (id > m_LastMessageId) m_LastMessageId = id;
            DataModelContact sender = GetContact(senderId);
            if (sender == null)
            {
                CreateMessageToServer(MessageType.requestContact, "", "", 0);
            }
            string messageTime = DateTime.Now.ToString();
            m_ChatMsgs.Add(new MessageItem(sender.Nickname, $"{sender.Nickname}:{msg}",
                senderId == 0 ? SetMessageColor(type) : sender.BackColor, messageTime));
            if (type == MessageType.message && id > 0)
            {
                int dbworkResult = DbWorker.AddMessage(id, chatId, messageTime, senderId, msg, m_Connection);
                if (dbworkResult == -1)
                {
                    m_ChatMsgs.Add(new MessageItem("System", $"ERROR: Unable to access database!!!",
                    SetMessageColor(MessageType.error), messageTime));
                }
            }
        }

        public void Messager(NetworkMessageItem msg)
        {
            if (msg == null) return;
            if (msg.MessageId > m_LastMessageId) m_LastMessageId = msg.MessageId;
            DataModelContact sender = GetContact(msg.SenderId);
            if (sender == null)
            {
                CreateMessageToServer(MessageType.requestContact, "", "", 0);
            }
            
            m_ChatMsgs.Add(new MessageItem(sender.Nickname, $"{sender.Nickname}:{msg.Message}",
                msg.SenderId == 0 ? SetMessageColor(msg.type) : sender.BackColor, msg.MessageTime));
            if (msg.type == MessageType.message && msg.MessageId > 0)
            {
                int dbworkResult = DbWorker.AddMessage(msg.MessageId, msg.ChatId, msg.MessageTime, msg.SenderId, msg.Message, m_Connection);
                if (dbworkResult == -1)
                {
                    m_ChatMsgs.Add(new MessageItem("System", $"ERROR: Unable to access database!!!",
                    SetMessageColor(MessageType.error), msg.MessageTime));
                }
            }
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
                Messager(msg);
            }
        }

        private void CreateMessageToServer(MessageType mtype, string param1 = "", string param2 = "", int param3 = 0) 
        {
            if(mtype == MessageType.requestContact)
            {
                m_UserMessagesQueue.Enqueue(new()
                {
                    type = MessageType.requestContact,
                    SenderId = m_CurentUserId,
                    ChatId = m_CurrentChat,
                    Message = m_LastContactId.ToString()
                });
                return;
            }
            if(mtype == MessageType.autorisation)
            {
                m_UserMessagesQueue.Enqueue(new()
                {
                    type = MessageType.autorisation,
                    SenderId = 0,
                    ChatId = 0,
                    Message = $"{param1}|{param2}|{m_CurrentChat}|{m_LastContactId}|{m_LastMessageId}|{m_LastChatId}"
                });
                return;
            }
            if(mtype == MessageType.requestMessages)
            {
                m_UserMessagesQueue.Enqueue(new()
                {
                    type = MessageType.requestMessages,
                    SenderId = m_CurentUserId,
                    ChatId = m_CurrentChat,
                    Message = m_LastMessageId.ToString()
                });
                return;
            }
        }
    }
}
