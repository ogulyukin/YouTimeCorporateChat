using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TCPServer
{
    public class NewConnection
    {
        Message m_Msg;
        int m_ConnectionNumber;
        TcpClient m_Connection;
        int sendeId;

        public NewConnection(Message msg, int number)
        {
            m_Msg = msg;
            m_ConnectionNumber = number;
            sendeId = 0;
            m_Msg(MessageType.info, sendeId, 0, $"New connection created: {m_ConnectionNumber}...");
        }

        public void ProceedNewConnection(TcpClient connection, X509Certificate2 serverCertificate)
        {
            m_Connection = connection;
            m_Msg(MessageType.info, sendeId, 0, $"Accept new connection: {m_ConnectionNumber} in thread: {Thread.CurrentThread.GetHashCode()}...");           
            
            var sslStream = new SslStream(connection.GetStream(), false);

            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;
            }catch(Exception exc)
            {
                m_Msg(MessageType.error, sendeId, 0, exc.Message);
                ClosingConnection();
                return;
            }
            
            m_Msg(MessageType.info, sendeId, 0, $"Autotentification result: {sslStream.IsAuthenticated}");
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

            while (isConnected)
            {
                do
                {
                    try
                    {
                        //connection.GetStream().Read(buffer);
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
