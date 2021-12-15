using System;

namespace TCPServer
{
    class Program
    {
        static void PrintMessages(string msg)
        {
            Console.WriteLine(msg);
        }

        static void Main(string[] args)
        {
            var port = 8005;
            ILogMessage logger = new LogToConsoleFull();
            TCPServer server = new(logger.AddMessage, port);
            logger.AddMessage(MessageType.info, $"Starting server at port {port}...");
            server.StartServer();
        }
    }
}
