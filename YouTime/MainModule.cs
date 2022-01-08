using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private Configuration.ConfigManager m_CManager;
        private List<MessageItem> m_ChatMsgs;
        private List<DataModelContact> m_ContactList;
        private Action messageCallBack;
        private int m_CurrentChat;
        private int m_CurentUserId;
        List<DataModelChat> m_ChatList;
        private const string m_Connection = "Data Source=data.sqlite;Mode=ReadWrite;";

        public MainModule(List<MessageItem> chatMsgs, Action refreshMsg)
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
            //m_ChatList = DbWorker.getChatList(//TODO Add string connection here );
        }

        public void StartNetwork(string username, string password)
        {
            var network = new TCPClient(Messager);
            Task task = new(() =>
            {
                network.ConnectToServer(m_CManager.AppConfig, username, password);
            });
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
            messageCallBack();
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
    }
}
