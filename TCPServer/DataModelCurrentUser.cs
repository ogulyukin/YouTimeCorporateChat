namespace TCPServer
{
    public class DataModelCurrentUser
    {
        public int UserId { get; set; }
        public int UserChatId { get; set; }
        public int UserLastMessageId { get; set; }
        public int UserLastContactId { get; set; }
        public int UserLastChatId { get; set; }

        public DataModelCurrentUser()
        {
            UserId = UserChatId = UserLastContactId = UserLastMessageId = UserLastChatId = 0;
        }
    }
}
