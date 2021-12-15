using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public class NewConnection
    {
        Message m_Msg;
        int m_ConnectionNumber;
        TcpClient m_Connection;

        public NewConnection(Message msg, int number)
        {
            m_Msg = msg;
            m_ConnectionNumber = number;
        }

        public void ProceedNewConnection(TcpClient connection)
        {
            m_Connection = connection;
            m_Msg(MessageType.info, $"Accept new Connection {m_ConnectionNumber} in {Thread.CurrentThread.GetHashCode()}...");           
            var buffer = new byte[256];
            var data = new List<byte>();
            string msg = "";
            bool isConnected = true;
            while (msg != "Bye" && isConnected)
            {
                do
                {
                    try
                    {
                        connection.GetStream().Read(buffer);
                        data.AddRange(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    catch (Exception exc)
                    {
                        m_Msg(MessageType.error, exc.Message);
                        isConnected = false;
                        break;
                    }

                } while (connection.GetStream().DataAvailable);
                if (isConnected)
                {
                    var t = data.ToArray();
                    msg = Encoding.Unicode.GetString(t, 0, t.Length);
                    m_Msg(MessageType.message, $"Connection {m_ConnectionNumber} Client: {msg}");
                    connection.GetStream().Write(Encoding.Unicode.GetBytes("Put something here!!!"));
                    m_Msg(MessageType.message, $"Connection {m_ConnectionNumber} Server: Put something here!!!");
                    data.Clear();
                }
            }
            ClosingConnection();
        }

        private void ClosingConnection()
        {
            m_Msg(MessageType.info, $"Connection {m_ConnectionNumber} Closing connection...");
            m_Connection.Close();
        }
    }
}
