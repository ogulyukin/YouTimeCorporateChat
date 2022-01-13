using System;

namespace TCPServer
{
    public class MessageItem
    {
        public MessageType type { get; init; }
        public int SenderId { get; init; }
        public int ChatId { get; init; }
        public string Message { get; init; }
        public int MessageId { get; set; }
        public string MessageTime { get; init; }
    }
}
