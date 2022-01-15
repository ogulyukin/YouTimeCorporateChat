using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace TCPServer
{
    public class NewConnection
    {
        private Message m_Msg;
        private int m_ConnectionNumber;
        private TcpClient m_Connection;
        private DataModelCurrentUser m_User;
        private Queue<MessageItem> m_Messages4User;
        private const string m_DBConnection = "Data Source=data.sqlite;Mode=ReadWrite;";

        public NewConnection(Message msg, int number)
        {
            m_Msg = msg;
            m_ConnectionNumber = number;
            m_User = new();
            m_Msg(MessageType.info, m_User.UserId, 0, $"New connection created: {m_ConnectionNumber}...");
            m_Messages4User = new();
        }

        public void ProceedNewConnection(TcpClient connection, X509Certificate2 serverCertificate)
        {
            m_Connection = connection;
            m_Msg(MessageType.info, m_User.UserId, 0, $"Accept new connection: {m_ConnectionNumber} in thread: {Thread.CurrentThread.GetHashCode()}...");           
            
            var sslStream = new SslStream(connection.GetStream(), false);

            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;
            }catch(Exception exc)
            {
                m_Msg(MessageType.error, m_User.UserId, 0, exc.Message);
                ClosingConnection();
                return;
            }
            
            m_Msg(MessageType.info, m_User.UserId, 0, $"SSL Autotentification result: {sslStream.IsAuthenticated}");
            if (sslStream.IsAuthenticated)
            {
                WaitForNewMessages(sslStream);
            }else
            {
                ClosingConnection();
            }
        }

        private void WaitForNewMessages(SslStream sslStream)
        {
            bool isConnected = m_Connection.Connected;
            var buffer = new byte[256];
            var data = new List<byte>();
            string msg = "";
            int byteReceived = 0;

            while (isConnected)
            {
                do
                {
                    try
                    {
                        byteReceived += sslStream.Read(buffer, 0, buffer.Length);
                        data.AddRange(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    catch (Exception exc)
                    {
                        if (exc.HResult != -2146232800)
                        {
                            m_Msg(MessageType.error, m_User.UserId, 0, exc.Message);
                            isConnected = false;
                            break;
                        }
                        else
                        {
                            if (m_Connection.Connected) SendNewMessages(sslStream);
                        }
                    }

                } while (m_Connection.Available > 0);
                isConnected = m_Connection.Connected;
                if (isConnected)
                {
                    var dataArray = data.ToArray();
                    msg = Encoding.Unicode.GetString(dataArray, 0, byteReceived);
                    
                    if(msg != String.Empty && !msg.StartsWith("0000"))
                    {
                        ProceedNewMessage(msg);
                    }
                    var newUserContactLastId = UserRequests.RequestContacts(m_Messages4User, m_User.UserLastContactId, m_DBConnection);
                    if (newUserContactLastId > m_User.UserLastContactId) m_User.UserLastContactId = newUserContactLastId;
                    UserRequests.RequestMessages(m_Messages4User, m_User.UserChatId, m_User.UserLastMessageId, m_DBConnection);
                    data.Clear();
                    byteReceived = 0;
                }
            }
            ClosingConnection();
        }

        private void ProceedNewMessage(string msg)
        {
            var newMessage = MessageParse.GetMessageFromString(msg);
            
            if(newMessage.type == MessageType.autorisation)
            {
                if (!UserAutorization.ProceedAutorization(m_User, msg, m_DBConnection))
                {
                    m_Msg(MessageType.error, m_User.UserId, 0, $"Connection: {m_ConnectionNumber} bad autorization request.");
                    ClosingConnection();
                    return;
                }
                m_Msg(MessageType.info, m_User.UserId, 0, $"Connection: {m_ConnectionNumber} autorization of User: {m_User.UserId} sucesfull");
                m_Messages4User.Enqueue(new() { type = MessageType.autorisation, SenderId = m_User.UserId, ChatId = m_User.UserChatId, MessageId = 0, MessageTime = DateTime.Now.ToString(), Message = "-" });
                var newUserContactLastId =  UserRequests.RequestContacts(m_Messages4User, m_User.UserLastContactId, m_DBConnection);
                if (newUserContactLastId > m_User.UserLastContactId) m_User.UserLastContactId = newUserContactLastId;
                UserRequests.RequestMessages(m_Messages4User, m_User.UserChatId , m_User.UserLastMessageId, m_DBConnection);
            }
            else if(newMessage.type == MessageType.requestContact && m_User.UserId > 0)
            {
                var newUserContactLastId = UserRequests.RequestContacts(m_Messages4User, newMessage.SenderId, m_DBConnection);
                if (newUserContactLastId > m_User.UserLastContactId) m_User.UserLastContactId = newUserContactLastId;
            }
            else if(newMessage.type == MessageType.requestMessages && m_User.UserId > 0)
            {
                UserRequests.RequestMessages(m_Messages4User, newMessage.ChatId, newMessage.SenderId, m_DBConnection);
            }else if(newMessage.type == MessageType.message && m_User.UserId > 0)
            {
                DbWorker.AddMessage(newMessage, m_DBConnection);
            }else
            {
                m_Msg(MessageType.warning, m_User.UserId, 0, $"Warning: Reseived unexpected type of message!");
            }
        }

        private void SendNewMessages(SslStream sslStream)
        {
            UserRequests.RequestMessages(m_Messages4User, m_User.UserChatId, m_User.UserLastMessageId, m_DBConnection);
            while(m_Messages4User.Count > 0)
            {
                var msg = m_Messages4User.Dequeue();
                sslStream.WriteAsync(Encoding.Unicode.GetBytes($"{GetTypeId(msg.type)}|{msg.SenderId}|{msg.ChatId}|{msg.Message}|{msg.MessageId}|{msg.MessageTime}"));
                sslStream.WriteAsync(Encoding.Unicode.GetBytes($"0000"));
                if (msg.MessageId > m_User.UserLastMessageId) m_User.UserLastMessageId = msg.MessageId;
            }
            sslStream.WriteAsync(Encoding.Unicode.GetBytes($"0000"));
            m_Msg(MessageType.info, 0, 0, $"Connection: {m_ConnectionNumber} Server: new messages sent");
        }

        private string GetTypeId(MessageType type)
        {
            switch (type)
            {
                case MessageType.message:
                    return "001";
                case MessageType.requestContact:
                    return "002";
                case MessageType.requestMessages:
                    return "003";
                case MessageType.autorisation:
                    return "004";
            }
            return "000";
        }

        private void ClosingConnection()
        {
            m_Msg(MessageType.info, m_User.UserId, 0, $"Connection {m_ConnectionNumber} in thread: {Thread.CurrentThread.GetHashCode()} Closing connection...");
            m_Connection.Close();
        }
    }
}
