using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;

namespace TCPServer
{
    class DbWorker
    {
        public static List<DataModelChat> getChatList(string connection)
        {
            var result = new List<DataModelChat>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = "SELECT * FROM 'Chat_Tab'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    var chat = new DataModelChat();
                    chat.Id = res.GetInt32("id");
                    chat.Name = res.GetString("Name");
                    result.Add(chat);
                }
            }
            db.Close();
            return result;
        }

        public static int AddChat(string name, string connection)
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            int result = GetChatId(name, db);
            if (result != 0) return result;
            var sql = $"INSERT INTO 'Chat_Tab'(Name) VALUES ('{name}');";
            var query02 = new SqliteCommand(sql, db);
            query02.ExecuteNonQuery();
            db.Close();
            return GetChatId(name, db);
        }

        private static int GetChatId(string name, SqliteConnection db)
        {
            int result = 0;
            db.Open();
            var sql = $"SELECT id FROM 'Chat_Tab' WHERE name = '{name}'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                res.Read();
                result = res.GetInt32("id");
            }
            db.Close();
            return result;
        }

        public static int GetUserId(string nickname, string password, string connection)
        {
            using var db = new SqliteConnection(connection);
            int result = -1;
            db.Open();
            var sql = $"SELECT id FROM User_Tab WHERE Nickname = '{nickname}' AND Password = '{password}'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                res.Read();
                result = res.GetInt32("id");
            }
            db.Close();
            return result;
        }

        public static List<MessageItem> getMessageList(string connection, int chatId, int startId)
        {
            var result = new List<MessageItem>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"SELECT * FROM 'Message_Tab' WHERE ChatId = '{chatId}' AND ID > '{startId}' ORDER BY ID";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    var message = new MessageItem()
                    {
                        SenderId = res.GetInt32("SenderId"),
                        ChatId = res.GetInt32("ChatId"),
                        MessageTime = res.GetString("DateTime"),
                        Message = res.GetString("Message"),
                        MessageId = res.GetInt32("id"),
                        type = MessageType.message
                    };                    
                    result.Add(message);
                }
            }
            db.Close();
            return result;
        }

        public static int AddMessage(MessageItem msg, string connection )
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var msgTime = DateTime.Now.ToString();
            var sql = $"INSERT INTO 'Message_Tab'(DateTime, SenderId, ChatId, Message) VALUES ('{msgTime}','{msg.SenderId}', '{msg.ChatId}', '{msg.Message}');";
            var query01 = new SqliteCommand(sql, db);
            query01.ExecuteNonQuery();
            sql = $"SELECT id from Message_Tab WHERE SenderId = '{msg.SenderId}' AND DateTime = '{msgTime}' AND Message = '{msg.Message}'";
            var query02 = new SqliteCommand(sql, db);
            var res = query02.ExecuteScalar();
            bool resId = int.TryParse(res.ToString(), out int resultId);
            db.Close();
            return resId ? resultId : -1;
        }

        public static List<DataModelContact> getContactList(int startId, string connection)
        {
            var result = new List<DataModelContact>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"SELECT * FROM 'User_Tab' WHERE id > '{startId}'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    var contact = new DataModelContact();
                    contact.ContactId = res.GetInt32("id");
                    contact.Nickname = res.GetString("Nickname");
                    contact.RealName = res.GetString("RealName");
                    result.Add(contact);
                }
            }
            db.Close();
            return result;
        }
    }
}
