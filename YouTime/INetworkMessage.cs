using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public enum NetworkMessageType
    {
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

    public interface INetworkMessage
    {
        public void AddMessage(int senderId, int chatId, string message);
    }
}
