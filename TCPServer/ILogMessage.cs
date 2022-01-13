﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public enum MessageType
    {
        none,
        info,
        error,
        warning,
        message,
        file,
        image,
        autorisation,
        requestMessages,
        requestContact
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
        public void AddMessage(MessageType type, int senderId, int chatId, string message);
    }
}
