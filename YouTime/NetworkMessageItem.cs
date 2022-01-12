using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatClient;

namespace Networking
{
    public class NetworkMessageItem
    {
        public MessageType type { get; init; }
        public int SenderId { get; init; }
        public int ChatId { get; init; }
        public string Message { get; init; }
        public bool StoreToDataBase { get; init; }
    }
}
