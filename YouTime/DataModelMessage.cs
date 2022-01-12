namespace ChatClient
{
    public class DataModelMessage
    {
        public int Id { get; set; }
        public string MyDateTime { get; set; }
        public int SenderId { get; set; }
        public int ChatId { get; set; }
        public string Message { get; set; }
    }
}
