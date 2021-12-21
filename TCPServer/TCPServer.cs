using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace TCPServer
{
    public delegate void Message(MessageType type, int senderId, int chatId, string msg);

    public class TCPServer
    {
        private Message m_Msg;
        private TcpListener m_Listener;
        private int m_Port;
        private int m_ConnectionCount;
        private X509Certificate2 m_ServerCertificate;

        public TCPServer(Message log, int port)
        {
            m_Msg = log;
            this.m_Port = port;
            m_ServerCertificate = null;
        }

       
        public void StartServer(string certificate)
        {
            try
            {
                m_ServerCertificate = new X509Certificate2(certificate);
                m_Msg(MessageType.info, 0, 0, $"Certificate loaded: {m_ServerCertificate.ToString(true)}");
                m_Listener = new TcpListener(IPAddress.Any, m_Port);
                m_Listener.Start();
                m_Msg(MessageType.info, 0, 0, "Begin listening...");
            }
            catch (Exception exc)
            {
                m_Msg(MessageType.error, 0, 0, exc.Message);
                return;
            }

            while (true)
            {
                TcpClient client = m_Listener.AcceptTcpClient();
                NewConnection connection = new(m_Msg, m_ConnectionCount);

                Task task = new(() =>
                {
                    connection.ProceedNewConnection(client, m_ServerCertificate);
                });
                m_ConnectionCount++;
                task.Start();
            }
        }
    }
}
