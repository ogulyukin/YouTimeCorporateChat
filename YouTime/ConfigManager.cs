using System;
using System.IO;
using System.Text.Json;
using ChatClient;

namespace Configuration
{
    public class ConfigManager
    {
        public Config AppConfig;
        public delegate void Message(MessageType type, int senderId, int chatId, string msg);
        Message m_Msg;

        public ConfigManager(Message msg)
        {
            m_Msg = msg;
            AppConfig = new();
        }

        public bool LoadConfig()
        {
            TempConfig tempConfig = new();
            try
            {
                using (FileStream fileStream = new("Config.json", FileMode.Open))
                {
                    tempConfig = JsonSerializer.DeserializeAsync<TempConfig>(fileStream).Result;
                }
            }
            catch (Exception exc)
            {
                m_Msg(MessageType.error, 0, 0, $"ERROR: {exc.Message}");
                return false;
            }
            m_Msg(MessageType.info, 0, 0, $"Loading server ip adress: {AppConfig.SetServerIp(tempConfig.IP)}");
            m_Msg(MessageType.info,0, 0, $"Loading server port: {AppConfig.SetServerPort(tempConfig.Port)}");
            m_Msg(MessageType.info, 0, 0, $"Local database check: {AppConfig.SetLocalDB(tempConfig.DBPath)}");
            AppConfig.UserId = int.TryParse(tempConfig.UserId, out int value0) ? value0 : 0;
            m_Msg(MessageType.info, 0, 0, $"Loading User ID: {AppConfig.UserId}");
            AppConfig.CurrentChatId = int.TryParse(tempConfig.UserId, out int value1) ? value1 : 0;
            m_Msg(MessageType.info, 0, 0, $"Current chat: {AppConfig.CurrentChatId}");
            return true;
        }

        public void SaveConfig()
        {
            TempConfig tempConfig = new(AppConfig.GetServerIP(), Convert.ToString(AppConfig.GetServerPort()), 
                AppConfig.GetDBPath(), AppConfig.UserId.ToString(), AppConfig.CurrentChatId.ToString());
            using (var file = new FileStream("Config.json", FileMode.Truncate, FileAccess.Write))
            {
                JsonSerializer.SerializeAsync(file, tempConfig);
            }
        }

        class TempConfig
        {
            public string IP {get; set;}
            public string Port { get; set; }
            public string DBPath { get; set; }
            public string UserId { get; set; }
            public string ChatId { get; set; }

            public TempConfig(string ip = "", string port = "", string dbpath = "", string userId = "", string chatId = "")
            {
                IP = ip;
                Port = port;
                DBPath = dbpath;
                UserId = userId;
                ChatId = chatId;
            }
        }
    }
}
