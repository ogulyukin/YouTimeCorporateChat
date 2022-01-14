using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public static class MessageParse
    {
        private static MessageType GetMessageType(string msg)
        {
            bool result = int.TryParse(msg, out int type);
            if (!result) return MessageType.none;
            switch (type)
            {
                case 1:
                    return MessageType.message;
                case 2:
                    return MessageType.requestContact;
                case 3:
                    return MessageType.requestMessages;
                case 4:
                    return MessageType.autorisation;
            }
            return MessageType.none;
        }

        public static MessageItem GetMessageFromString(string msg)
        {
            var messageContent =  msg.Split('|');
            int.TryParse(messageContent[1], out int sender);
            int.TryParse(messageContent[2], out int chat);
            var result = new MessageItem() {
                type = GetMessageType(messageContent[0]),
                SenderId = sender,
                ChatId = chat,
                Message = StringChecker(messageContent[3])
            };
            return result;
        }

        private static string StringChecker(string str)
        {
            return str.Contains('\0') ? str.Remove(str.IndexOf('\0')) : str;
        }
    }
}
