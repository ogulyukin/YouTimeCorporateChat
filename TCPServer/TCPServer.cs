using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;


namespace TCPServer
{
    public delegate void Message(MessageType type, int senderId, int chatId, string msg);

    public class TCPServer
    {
        private Message m_Msg;
        private TcpListener m_Listener;
        int port;
        private int m_ConnectionCount = 0;

        public TCPServer(Message log, int port)
        {
            m_Msg = log;
            this.port = port;
        }

        ~TCPServer()
        {
            if (m_Listener != null) m_Listener.Stop();
        }
        
        public void StartServer()
        {
            try
            {
                m_Listener = new TcpListener(IPAddress.Any, port);
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
                    connection.ProceedNewConnection(client);
                });
                m_ConnectionCount++;
                task.Start();
            }
        }
    }
}
