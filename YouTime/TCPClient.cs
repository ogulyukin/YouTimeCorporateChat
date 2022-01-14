using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ChatClient;

namespace Networking
{
    public class TCPClient
    {
        private TcpClient m_Connection;
        private SslStream m_SslStream;
        private static Hashtable m_CertificateErrors = new Hashtable();
        private Queue<NetworkMessageItem> m_UserMessagesQueue;
        private Queue<NetworkMessageItem> m_ServerMessagesQueue;
        private DataModelCurrentUser m_User;

        public TCPClient(Queue<NetworkMessageItem> userMessagesQueue, Queue<NetworkMessageItem> serverMessagesQueue, DataModelCurrentUser user)
        {
            m_UserMessagesQueue = userMessagesQueue;
            m_ServerMessagesQueue = serverMessagesQueue;
            m_User = user;
        }

        public void ConnectToServer(Configuration.Config config, string username, string password)
        {
            m_ServerMessagesQueue.Enqueue(new() {  ChatId = 0, SenderId = 0, type = MessageType.info, Message = "System: Starting server...", MessageId = 0 });

            try
            {
                var ipServer = new IPEndPoint(IPAddress.Parse(config.GetServerIP()), config.GetServerPort());
                m_Connection = new();
                m_Connection.Connect(ipServer);
            }
            catch (Exception exc)
            {
                m_ServerMessagesQueue.Enqueue(new() { ChatId = 0, SenderId = 0, type = MessageType.error, Message = $"ERROR:{exc.Message}", MessageId = 0 });
                return;
            }

            try
            {
                m_SslStream = new SslStream(m_Connection.GetStream(), true, ValidateServerCertificate, null);
                m_SslStream.AuthenticateAsClient("Host name with SSL cert");
            }catch(Exception exc)
            {
                m_Connection.Close();
                m_ServerMessagesQueue.Enqueue(new() { ChatId = 0, SenderId = 0, type = MessageType.error, Message = $"ERROR:{exc.Message}", MessageId = 0 });
                return;
            }
            m_ServerMessagesQueue.Enqueue(new() { ChatId = 0, SenderId = 0, type = MessageType.info, Message = "System: Connection to Sever established.", MessageId = 0 });
            SendNewMessages();
            WaitForNewMessages();
            CloseConnection();
            return;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        
        public bool isConnectedToServer()
        {
            return m_Connection != null && m_Connection.Connected;
        }
        
        private void WaitForNewMessages()
        {
            bool isConnected = m_Connection.Connected;
            var buffer = new byte[256];
            var data = new List<byte>();
            string msg = "";
            int bytesReceived = 0;
            while (isConnected)
            {
                do
                {
                    try
                    {
                        bytesReceived += m_SslStream.Read(buffer, 0, buffer.Length);
                        data.AddRange(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    catch (Exception exc)
                    {
                        if (exc.HResult != -2146232800)
                        {
                            m_ServerMessagesQueue.Enqueue(new() { ChatId = 0, SenderId = 0, type = MessageType.error, Message = $"ERROR:{exc.Message}", MessageId = 0 });
                            isConnected = false;
                            break;
                        }
                        else
                        {
                            if (m_Connection.Connected) SendNewMessages();
                        }
                    }
                } while (m_Connection.Available > 0);
                SendNewMessages();
                isConnected = m_Connection.Connected;
                if (isConnected)
                {
                    var dataArray = data.ToArray();
                    msg = Encoding.Unicode.GetString(dataArray, 0, bytesReceived);
                    if (msg != String.Empty && !msg.StartsWith("0000"))
                    {
                        ProceedNewMessage(msg);
                    }
                    data.Clear();
                    bytesReceived = 0;
                }
            }
            CloseConnection();
        }
        
        private void ProceedNewMessage(string msg)
        {
            var newMessage = MessageParser.GetMessageFromString(msg);
            if (newMessage.type == MessageType.autorisation)
            {
                m_User.UserId = newMessage.SenderId;
                m_User.UserIsConnected = true;
            }
            else 
            {
                m_ServerMessagesQueue.Enqueue(new()
                {
                    ChatId = newMessage.ChatId,
                    SenderId = newMessage.SenderId,
                    type = newMessage.type,
                    Message = newMessage.Message,
                    MessageId = newMessage.MessageId,
                    MessageTime = newMessage.MessageTime
                });
            }
        }

        private void SendNewMessages()
        {
            while(m_UserMessagesQueue.Count > 0)
            {
                var msg = m_UserMessagesQueue.Dequeue();
                m_SslStream.WriteAsync(Encoding.Unicode.GetBytes($"{GetTypeId(msg.type)}|{msg.SenderId}|{msg.ChatId}|{msg.Message}|"));
                m_SslStream.WriteAsync(Encoding.Unicode.GetBytes($"0000"));
            }
        }

        private string GetTypeId(MessageType type)
        {
            switch(type)
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

        private void CloseConnection()
        {
            m_Connection.Close();
            m_User.UserIsConnected = false;
            m_ServerMessagesQueue.Enqueue(new() { ChatId = 0, SenderId = 0, type = MessageType.warning, Message = "SERVER: Connection to server Closed", MessageId = 0 });
        }
    }
}
