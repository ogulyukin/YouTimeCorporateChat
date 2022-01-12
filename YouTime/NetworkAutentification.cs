using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatClient;

namespace Networking
{
    class NetworkAutentification : INetworkMessage
    {
        public void AddMessage(int senderId, int chatId, string message, bool storeToDataBase)
        {
            var newMessage = new NetworkMessageItem()
            {
                type = MessageType.autorisation,
                SenderId = senderId,
                ChatId = chatId,
                Message = message,
                StoreToDataBase = storeToDataBase
            }; 
        }
    }
}
