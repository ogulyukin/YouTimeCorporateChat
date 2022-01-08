using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    class NetworkAutentification : INetworkMessage
    {
        public void AddMessage(int senderId, int chatId, string message)
        {
            var newMessage = new NetworkMessageItem()
            {
                type = NetworkMessageType.autorisation,
                SenderId = senderId,
                ChatId = chatId,
                Message = message
            }; 
        }
    }
}
