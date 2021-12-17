using System.Collections.Generic;

namespace TCPServer
{
    public class DataModelUser
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public string Realname { get; set; }
        public int SecurityLevel { get; set; }
        public string Password { get; set; }
        public List<DataModelChat> Chats { get; set; }
        public List<DataModelContact> Contacts { get; set; }
    }
}
