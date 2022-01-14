using ChatClient;

namespace Networking
{
    public static class MessageParser
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

        public static NetworkMessageItem GetMessageFromString(string msg)
        {
            var messageContent =  msg.Split('|');
            int.TryParse(messageContent[1], out int sender);
            int.TryParse(messageContent[2], out int chat);
            var result = new NetworkMessageItem() {
                type = GetMessageType(messageContent[0]),
                SenderId = sender,
                ChatId = chat,
                Message = messageContent[3]
            };
            return result;
        }
    }
}
