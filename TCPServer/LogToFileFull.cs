using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace TCPServer
{
    class LogToFileFull : ILogMessage
    {      
        public void AddMessage(MessageType type, int senderId, int chatId, string message)
        { 
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream("log.txt", FileMode.Append), Encoding.UTF8, 512, false))
                {
                    if (type == MessageType.message)
                    {
                        writer.WriteLineAsync($"{DateTime.UtcNow} Sender: {senderId} Chat: {chatId} Message: {message}");
                    }
                    else
                    {
                        writer.WriteLineAsync($"{DateTime.UtcNow} {type}: {message}");
                    }
                }
            }catch(Exception exc)
            {
                Console.WriteLine(exc.Message); //Debug message don't forget to remove it!!!
            }
             
        }

        public bool StartLog()
        {
            return true;
        }
    }
}
