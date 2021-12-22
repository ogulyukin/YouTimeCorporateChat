using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace ChatClient
{
    public class ServerConnector
    { 
        private TcpClient client;
        SslStream sslStream;
        private static Hashtable _certificateErrors = new Hashtable();

        public string ConnectToServer(string ip, int port)
        {
            try
            {
                var ipServer = new IPEndPoint(IPAddress.Parse(ip), port);
                client = new();
                client.Connect(ipServer);
            }
            catch (Exception exc)
            {
                return exc.Message;
            }

            try
            {
                sslStream = new SslStream(client.GetStream(), true, ValidateServerCertificate, null);
                sslStream.AuthenticateAsClient("Host name with SSL cert");
            }catch(Exception exc)
            {
                client.Close();
                return exc.Message;
            }

            return "Соединение установлено";
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void SendMessage(string message)
        {
            sslStream.WriteAsync(Encoding.Unicode.GetBytes(message));           
        }

        public async string GetNewMessages()
        {
            var buffer = new byte[256];
            var data = new List<byte>();
            CancellationTokenSource source;
            CancellationToken token = (source = new()).Token;
            do
            {
                sslStream.Read(buffer);
                data.AddRange(buffer);
            } while (client.Available > 0);

            byte[] dataArray = data.ToArray();

            return Encoding.Unicode.GetString(dataArray, 0, dataArray.Length);
        }

        public bool isConnectedToServer()
        {
            return client != null && client.Connected;
        }

        private void CloseConnection()
        {
            client.Close();
        }

        private void WaitForNewMessages(SslStream sslStream)
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
                        sslStream.Read(buffer, 0, buffer.Length);
                        data.AddRange(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    catch (Exception exc)
                    {
                        if (exc.HResult != -2146232800)
                        {
                            m_Msg(MessageType.error, sendeId, 0, exc.Message);
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
                    msg = Encoding.Unicode.GetString(dataArray, 0, dataArray.Length);
                    m_Msg(MessageType.message, sendeId, 0, $"Connection: {m_ConnectionNumber} Client: {msg}");
                    data.Clear();
                }
            }
            ClosingConnection();
        }

        private void SendNewMessages(SslStream sslStream)
        {
            sslStream.WriteAsync(Encoding.Unicode.GetBytes("New messages from server"));
            m_Msg(MessageType.info, 0, 0, $"Connection: {m_ConnectionNumber} Server: new messages sent");
        }

        private void ClosingConnection()
        {
            m_Msg(MessageType.info, sendeId, 0, $"Connection {m_ConnectionNumber} in thread: {Thread.CurrentThread.GetHashCode()} Closing connection...");
            m_Connection.Close();
        }

    }
}
