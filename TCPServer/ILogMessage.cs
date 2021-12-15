using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public enum MessageType
    {
        info,
        error,
        warning,
        message,
        file,
        image
    }

    public enum LogLevel
    {
        full,
        info,
        messages
    }

    public interface ILogMessage
    {
        public bool StartLog();
        public void AddMessage(MessageType type, string message);
    }
}
