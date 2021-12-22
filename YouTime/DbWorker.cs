using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;
using System.Windows.Media;

namespace ChatClient
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
            return GetChatId(name, db);
        }

        private static int GetChatId(string name, SqliteConnection db)
        {
            int result = 0;
            var sql = $"SELECT id FROM 'Chat_Tab' WHERE name = '{name}'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                res.Read();
                result = res.GetInt32("id");
            }
            return result;
        }

        public static List<DataModelMessage> getMessageList(string connection, int chatId)
        {
            var result = new List<DataModelMessage>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = "SELECT * FROM 'Message_Tab'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    var message = new DataModelMessage();
                    message.Id = res.GetInt32("id");
                    message.ChatId = res.GetInt32("ChatId");
                    message.MyDateTime = res.GetString("DateTime");
                    message.Message = res.GetString("Message");
                    message.SenderId = res.GetInt32("SenderId");
                    result.Add(message);
                }
            }
            return result;
        }

        public static int AddMessage(int chatId, string dateTime, int senderId, string message, string connection )
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"INSERT INTO 'Message_Tab'(DateTime, SenderId, ChatId, Message) VALUES ('{dateTime}','{senderId}', '{chatId}', '{message}');";
            var query02 = new SqliteCommand(sql, db);
            query02.ExecuteNonQuery();
            return GetChatId(message, db);
        }

        public static List<DataModelContact> getContactList(string connection)
        {
            var result = new List<DataModelContact>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = "SELECT * FROM 'Chat_Tab'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    var contact = new DataModelContact();
                    contact.ContactId = res.GetInt32("id");
                    contact.Nickname = res.GetString("Name");
                    contact.RealName = res.GetString("Name");
                    contact.BackColor = new SolidColorBrush(Color.FromRgb(res.GetByte("R"), res.GetByte("G"), res.GetByte("B")));
                    result.Add(contact);
                }
            }
            return result;
        }

        public static int AddContact(string name, string connection)
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            int result = GetChatId(name, db);
            if (result != 0) return result;
            var sql = $"INSERT INTO 'Chat_Tab'(Name) VALUES ('{name}');";
            var query02 = new SqliteCommand(sql, db);
            query02.ExecuteNonQuery();
            return GetChatId(name, db);
        }
    }
}
