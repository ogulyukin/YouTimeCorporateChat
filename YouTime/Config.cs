using System.Text.RegularExpressions;

namespace Configuration
{
    public class Config
    {
        public int UserId { get; set; }
        public int CurrentChatId { get; set; }
        private string m_ServerIp;
        private int m_ServerPort;
        private string m_LocalDB;

        public string GetServerIP() { return m_ServerIp; }
        public int GetServerPort() { return m_ServerPort; }
        public string GetDBPath() { return m_LocalDB; }

        public string SetServerIp(string ipstring)
        {
            if(ValidateIp(ipstring))
            {
                m_ServerIp = ipstring;
                return "OK";
            }

            return "ERROR: Incorrect Server IP Setting!";
        }

        public string SetServerPort(string strport)
        {
            m_ServerPort = ValidatePort(strport);
            return m_ServerPort > 0 ? "OK" : "ERROR: Incorrect Server Port Setting!";
        }

        public string SetLocalDB(string path)
        {
            if(ValidateDB(path))
            {
                m_LocalDB = path;
                return "OK";
            }

            return "ERROR: Database file is not exist!";
        }

        private bool ValidateDB(string path)
        {
            return System.IO.File.Exists(path);
        }
        
        private bool ValidateIp(string ipstring)
        {
            Regex regex = new(@"[0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}");
            return regex.IsMatch(ipstring);
        }
        
        private int ValidatePort(string strport)
        {
            return int.TryParse(strport, out int port) ? port : -1;
        }
    }
}
