namespace TCPServer
{
    public static class UserAutorization
    {
        public static bool ProceedAutorization(DataModelCurrentUser user, string msg, string connection)
        {
            var userMsg = msg.Split('|');
            if (userMsg.Length < 9) return false;
            int userId = DbWorker.GetUserId(userMsg[3], userMsg[4], connection);
            if (userId <= 0) return false;
            user.UserId = userId;
            user.UserChatId = int.TryParse(userMsg[5], out int chatId) ? chatId : 0;
            user.UserLastContactId = int.TryParse(userMsg[6], out int contactId) ? contactId : 0;
            user.UserLastMessageId = int.TryParse(userMsg[7], out int messageId) ? messageId : 0;
            user.UserLastChatId = int.TryParse(userMsg[8], out int lastchatId) ? lastchatId : 0;
            return true;
        }
    }
}
