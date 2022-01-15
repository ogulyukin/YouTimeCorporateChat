using ChatClient;

namespace Networking
{
    public class NetworkMessageItem
    {
        public MessageType type { get; init; }
        public int SenderId { get; init; }
        public int ChatId { get; init; }
        public string Message { get; init; }
        public int MessageId { get; init; }
        public string MessageTime { get; init; }
    }
}
