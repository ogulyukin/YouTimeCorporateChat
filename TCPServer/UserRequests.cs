using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public static class UserRequests
    {
        public static int RequestContacts(Queue<MessageItem> messages4User, int lastId, string dbConnection)
        {
            var contacts = DbWorker.getContactList(lastId, dbConnection);
            if (contacts.Count == 0) return -1;
            foreach(var it in contacts)
            {
                messages4User.Enqueue(new() { type = MessageType.requestContact, ChatId = 0, SenderId = it.ContactId, MessageId = 0, Message = it.Nickname, MessageTime = DateTime.Now.ToString() });
                if (it.ContactId > lastId) lastId = it.ContactId;
            }
            return lastId;
        }

        public static void RequestMessages(Queue<MessageItem> messages4User,int chatId, int lastId, string dbConnection)
        {
            var messages = DbWorker.getMessageList(dbConnection, chatId, lastId);
            if (messages.Count == 0) return;
            foreach (var it in messages)
            {
                messages4User.Enqueue(new() { type = MessageType.message, ChatId = chatId, SenderId = it.SenderId, MessageId = it.MessageId, Message = it.Message, MessageTime = it.MessageTime });
            }
        }
    }
}
