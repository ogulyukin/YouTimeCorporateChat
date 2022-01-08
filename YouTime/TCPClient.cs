using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ChatClient;

namespace Networking
{
    public class TCPClient
    {
        public delegate void Message(MessageType type, int senderId, int chatId, string msg);
        private Message m_Msg;
        private TcpClient m_Connection;
        private SslStream m_SslStream;
        private static Hashtable m_CertificateErrors = new Hashtable();
        private int m_UserId = 0;

        public TCPClient(Message msg)
        {
            m_Msg = msg;
        }

        public void ConnectToServer(Configuration.Config config, string username, string password)
        {
            try
            {
                var ipServer = new IPEndPoint(IPAddress.Parse(config.GetServerIP()), config.GetServerPort());
                m_Connection = new();
                m_Connection.Connect(ipServer);
            }
            catch (Exception exc)
            {
                m_Msg(MessageType.error, 0, 0, $"ERROR:{exc.Message}");
                return;
            }

            try
            {
                m_SslStream = new SslStream(m_Connection.GetStream(), true, ValidateServerCertificate, null);
                m_SslStream.AuthenticateAsClient("Host name with SSL cert");
            }catch(Exception exc)
            {
                m_Connection.Close();
                m_Msg(MessageType.error, 0, 0, $"ERROR:{exc.Message}");
                return;
            }
            m_Msg(MessageType.info, 0, 0, "System: Connection to Sever established.");
            WaitForNewMessages();
            CloseConnection();
            return;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void SendMessage(string message)
        {
            m_SslStream.WriteAsync(Encoding.Unicode.GetBytes(message));           
        }

        public string GetNewMessages()
        {
            var buffer = new byte[256];
            var data = new List<byte>();
            CancellationTokenSource source;
            CancellationToken token = (source = new()).Token;
            do
            {
                m_SslStream.Read(buffer);
                data.AddRange(buffer);
            } while (m_Connection.Available > 0);

            byte[] dataArray = data.ToArray();

            return Encoding.Unicode.GetString(dataArray, 0, dataArray.Length);
        }

        public bool isConnectedToServer()
        {
            return m_Connection != null && m_Connection.Connected;
        }

        private void CloseConnection()
        {
            m_Connection.Close();
        }
        
        private void WaitForNewMessages()
        {
            bool isConnected = m_Connection.Connected;
            var buffer = new byte[256];
            var data = new List<byte>();
            string msg = "";

            while (isConnected)
            {
                do
                {
                    try
                    {
                        m_SslStream.Read(buffer, 0, buffer.Length);
                        data.AddRange(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    catch (Exception exc)
                    {
                        if (exc.HResult != -2146232800)
                        {
                            m_Msg(MessageType.error, m_UserId, 0, exc.Message);
                            isConnected = false;
                            break;
                        }
                        else
                        {
                            if (m_Connection.Connected) SendNewMessages();
                        }
                    }

                } while (m_Connection.Available > 0);
                isConnected = m_Connection.Connected;
                if (isConnected)
                {
                    var dataArray = data.ToArray();
                    msg = Encoding.Unicode.GetString(dataArray, 0, dataArray.Length);
                    m_Msg(MessageType.message, m_UserId, 0, $"{msg}");
                    data.Clear();
                }
            }
            CloseConnection();
        }
        
        private void SendNewMessages()
        {
            m_SslStream.WriteAsync(Encoding.Unicode.GetBytes("New messages from server"));
            m_Msg(MessageType.info, 0, 0, $"Connection:  Server: new messages sent");
        }

    }
}
