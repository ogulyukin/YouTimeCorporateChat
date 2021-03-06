using System;

namespace TCPServer
{
    public class LogToConsoleFull : ILogMessage
    {
        public bool StartLog()
        {
            return true;
        }

        public void AddMessage(MessageType type, int senderId, int chatId, string message)
        {
            ChangeConsoleColor(type);
            if (type == MessageType.message)
            {
                Console.WriteLine($"{DateTime.UtcNow} Sender: {senderId} Chat: {chatId} Message: {message}");
            }else
            {
                Console.WriteLine($"{DateTime.UtcNow} {type}: {message}");
            }
            Console.ResetColor();
        }

        private void ChangeConsoleColor(MessageType type)
        {
            switch(type)
            {
                case MessageType.info:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case MessageType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageType.warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MessageType.message:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MessageType.file:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case MessageType.image:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
        }
    }
}
