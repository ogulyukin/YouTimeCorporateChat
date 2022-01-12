using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public interface INetworkMessage
    {
        public void AddMessage(int senderId, int chatId, string message, bool storeToDataBase);
    }
}
