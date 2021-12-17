using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public class Starter
    {
        private int m_Port = 8005;
        private ILogMessage m_Logger;
        private ILogMessage m_ConsoleLogger;
        private TCPServer m_Server;

        public Starter()
        {
            m_ConsoleLogger = new LogToConsoleFull();
            m_Logger = new LogToFileFull();
            m_Server = new(Logger, m_Port);
            Logger(MessageType.info, 0, 0, $"Starting server at port {m_Port}...");
        }

        public void StartServer()
        {
            m_Server.StartServer();
        }

        private void Logger(MessageType type, int senderId, int chatId, string message)
        {
            m_Logger.AddMessage(type, senderId, chatId, message);
            m_ConsoleLogger.AddMessage(type, senderId, chatId, message);
        }
    }
}
