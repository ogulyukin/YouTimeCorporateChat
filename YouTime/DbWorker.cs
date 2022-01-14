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
            db.Close();
            return result;
        }

        public static int GetLastChatId(string connection)
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"select max(Id) from Chat_Tab";
            var query01 = new SqliteCommand(sql, db);
            var res = query01.ExecuteScalar();
            bool resId = int.TryParse(res.ToString(), out int resultId);
            db.Close();
            return resId ? resultId : -1;
        }

        public static int GetLastMessageId(int chatId, string connection)
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"select max(Id) from Message_Tab where ChatId = '{chatId}'";
            var query01 = new SqliteCommand(sql, db);
            var res = query01.ExecuteScalar();
            bool resId = int.TryParse(res.ToString(), out int resultId);
            db.Close();
            return resId ? resultId : -1;
        }


        public static int GetLastContactId(string connection)
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = "select max(UserId) from UserContacts_Tab";
            var query01 = new SqliteCommand(sql, db);
            var res = query01.ExecuteScalar();
            bool resId = int.TryParse(res.ToString(), out int resultId);
            db.Close();
            return resId ? resultId : -1;
        }

        public static DataModelContact GetContactById(int contactId, string connection)
        {
            var contact = new DataModelContact();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"SELECT * FROM 'UserContacts_Tab' WHERE UserId = {contactId}";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    
                    contact.ContactId = res.GetInt32("UserId");
                    contact.Nickname = res.GetString("Nickname");
                    contact.BackColor = new SolidColorBrush(Color.FromRgb(res.GetByte("R"), res.GetByte("G"), res.GetByte("B")));
                }
            }
            db.Close();
            return contact;
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

        public static List<DataModelMessage> getMessageList(string connection, int chatId)
        {
            var result = new List<DataModelMessage>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"SELECT * FROM 'Message_Tab' WHERE ChatId = {chatId}";
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
            db.Close();
            return result;
        }

        public static int AddMessage(int id, int chatId, string dateTime, int senderId, string message, string connection )
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"INSERT INTO 'Message_Tab'(id, DateTime, SenderId, ChatId, Message) VALUES ('{id}','{dateTime}','{senderId}', '{chatId}', '{message}');";
            var query01 = new SqliteCommand(sql, db);
            query01.ExecuteNonQuery();
            sql = "SELECT last_insert_rowid() from Message_Tab";
            var query02 = new SqliteCommand(sql, db);
            var res = query02.ExecuteScalar();
            bool resId = int.TryParse(res.ToString(), out int resultId);
            db.Close();
            return resId && resultId == id ? resultId : -1;
        }

        public static List<DataModelContact> getContactList(string connection)
        {
            var result = new List<DataModelContact>();

            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = "SELECT * FROM 'UserContacts_Tab'";
            using var query = new SqliteCommand(sql, db);
            using var res = query.ExecuteReader();
            if (res.HasRows)
            {
                while (res.Read())
                {
                    var contact = new DataModelContact();
                    contact.ContactId = res.GetInt32("Userid");
                    contact.Nickname = res.GetString("Nickname");
                    contact.BackColor = new SolidColorBrush(Color.FromRgb(res.GetByte("R"), res.GetByte("G"), res.GetByte("B")));
                    result.Add(contact);
                }
            }
            db.Close();
            return result;
        }

        public static int AddContact(int id, string name, string connection)
        {
            using var db = new SqliteConnection(connection);
            db.Open();
            var sql = $"INSERT INTO UserContacts_Tab (UserId,R,G,B,Nickname) VALUES ('{id}','{GenerageRGB()}','{GenerageRGB()}','{GenerageRGB()}','{name}')";
            var query01 = new SqliteCommand(sql, db);
            query01.ExecuteNonQuery();
            sql = "SELECT last_insert_rowid() from UserContacts_Tab";
            var query02 = new SqliteCommand(sql, db);
            var res = query02.ExecuteScalar();
            bool resId = int.TryParse(res.ToString(), out int resultId);
            db.Close();
            return resId && resultId == id ? resultId : -1;
        }

        private static string GenerageRGB()
        {
            var rand = new Random();
            return rand.Next(0, 255).ToString();
        }
    }
}
