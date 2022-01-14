using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Networking;

namespace ChatClient
{
    public enum MessageType
    {
        none,
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
        private DataModelCurrentUser m_User;
        //List<DataModelChat> m_ChatList; //Currently unused
        private const string m_Connection = "Data Source=data.sqlite;Mode=ReadWrite;";
        private Queue<NetworkMessageItem> m_UserMessagesQueue;
        private Queue<NetworkMessageItem> m_ServerMessagesQueue;
        private bool m_ConfigSaved = false;

        public MainModule(List<MessageItem> chatMsgs, Updater refreshMsg)
        {
            m_ChatMsgs = chatMsgs; // = DbWorker.getMessageList(m_Connection); !!!!!
            m_CManager = new(Messager);
            m_ContactList = DbWorker.getContactList(m_Connection);
            m_User = new();
            m_ContactList.Add(new DataModelContact()
            {
                ContactId = 0,
                Nickname = "System",
                RealName = "System",
                BackColor = new(Color.FromRgb(0, 255, 171))
            });
            m_UserMessagesQueue = new();
            m_ServerMessagesQueue = new();
            m_User.UserLastContactId = DbWorker.GetLastContactId(m_Connection);
            m_User.UserLastMessageId = DbWorker.GetLastMessageId(m_User.UserChatId, m_Connection);
            m_User.UserLastChatId = DbWorker.GetLastChatId(m_Connection);
            Messager(MessageType.info, 0, 0, $"LastContact = {m_User.UserLastContactId} LastMessage = {m_User.UserLastMessageId}", 0);
        }

        public void StartNetwork(string username, string password)
        {
            var network = new TCPClient(m_UserMessagesQueue, m_ServerMessagesQueue, m_User);
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
            if (!m_User.UserIsConnected) return;
            var userMessage = message.Replace('|', ' ');
            var msg = new NetworkMessageItem()
            {
                type = MessageType.message,
                ChatId = m_User.UserChatId,
                SenderId = m_User.UserId,
                Message = userMessage,
                MessageId = 0,
                MessageTime = "-"
            };
            m_UserMessagesQueue.Enqueue(msg);
        }

        public string GetCurrentUserNick()
        {
            foreach (var user in m_ContactList)
            {
                if (user.ContactId == m_User.UserId && m_User.UserId != 0)
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
            return "Common chat";
        }

        public bool StartConfigMagager()
        {
            bool result = m_CManager.LoadConfig();
            m_User.UserChatId = result ? m_CManager.AppConfig.CurrentChatId : 0;
            m_User.UserId = result ? m_CManager.AppConfig.UserId : 0;
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
            if (id > m_User.UserLastMessageId) m_User.UserLastMessageId = id;
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
            
            if (msg.type == MessageType.message && msg.MessageId > 0)
            {
                int dbworkResult = DbWorker.AddMessage(msg.MessageId, msg.ChatId, msg.MessageTime, msg.SenderId, msg.Message, m_Connection);
                if (dbworkResult == -1)
                {
                    m_ChatMsgs.Add(new MessageItem("System", $"ERROR: Unable to access database!!!",
                    SetMessageColor(MessageType.error), msg.MessageTime));
                    AddMessageToListBox(msg);
                    return;
                }
                AddMessageToListBox(msg);
            }
            else if (msg.type == MessageType.requestContact)
            {
                DbWorker.AddContact(msg.SenderId, msg.Message, m_Connection);
                var newcontact = DbWorker.GetContactById(msg.SenderId, m_Connection);
                if (newcontact.ContactId == msg.SenderId) m_ContactList.Add(newcontact);
            }
            else
            {
                AddMessageToListBox(msg);
            }
        }

        private void AddMessageToListBox(NetworkMessageItem msg)
        {
            if (msg.MessageId > m_User.UserLastMessageId) m_User.UserLastMessageId = msg.MessageId;
            DataModelContact sender;
            if (msg.SenderId != 0)
            {
                sender = GetContact(msg.SenderId);
                if (sender == null)
                {
                    sender = new() { Nickname = "Unknown", BackColor = SetMessageColor(MessageType.error) };
                }
            }else
            {
                sender = new() { Nickname = "System", BackColor = SetMessageColor(MessageType.info) };
            }
            

            m_ChatMsgs.Add(new MessageItem(sender.Nickname, $"{sender.Nickname}:{msg.Message}",
                msg.SenderId == 0 ? SetMessageColor(msg.type) : sender.BackColor, msg.MessageTime));
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
            if (!m_ConfigSaved && m_User.UserIsConnected)
            {
                m_CManager.AppConfig.CurrentChatId = m_User.UserChatId;
                m_CManager.AppConfig.UserId = m_User.UserId;
                m_CManager.SaveConfig();
            }

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
                    SenderId = m_User.UserLastContactId,
                    ChatId = m_User.UserChatId,
                    Message =  "-"
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
                    Message = $"{param1}|{param2}|{m_User.UserChatId}|{m_User.UserLastContactId}|{m_User.UserLastMessageId}|{m_User.UserLastChatId}"
                });
                return;
            }
            if(mtype == MessageType.requestMessages)
            {
                m_UserMessagesQueue.Enqueue(new()
                {
                    type = MessageType.requestMessages,
                    SenderId = m_User.UserLastMessageId,
                    ChatId = m_User.UserChatId,
                    Message = "-"
                });
                return;
            }
        }
    }
}
