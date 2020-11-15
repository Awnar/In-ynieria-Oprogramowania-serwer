using IO_2_lib.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace IO_2_lib
{
    class SQLite
    {
        static DatabaseContext _database;
        static SQLiteConnection con;
        static TimeSpan validity = new TimeSpan(5, 0, 0);

        public static void Init()
        {
            _database = new DatabaseContext();
            _database.Database.EnsureCreated();
        }

        public static int Login(string name, string pass)
        {
            var user =_database.Users.FirstOrDefault(x => x.Name == name && x.Password == pass);
            return user == null ? -1 : 1;
        }

        public static bool checkname(string name)
        {
            return _database.Users.Count(x => x.Name == name) > 0;
        }

        public static bool Register(string name, string pass)
        {
            _database.Users.Add(new User() 
            { 
                Name = name,
                Password = pass
            });
            _database.SaveChanges();
            return true;
        }

        public static bool AddTask(int user_id, string name, string description)
        {
            var user = _database.Users.Find(user_id);
            if (user == null)
                return false;

            user.Tasks.Add(new Task()
            {
                Name = name,
                Description = description
            });
            _database.SaveChanges();
            return true;
        }
        public static List<Task> ListTasks(int user_ID)
        {
            return _database.Tasks.Where(x=> x.User.Id == user_ID).ToList();
        }

        public static Task GetTask(int id, int user_ID)
        {
            return _database.Tasks.FirstOrDefault(x => x.Id == id && x.User.Id == user_ID);
        }

        public static AuthKey VerifyAuthKey(string key)
        {
            return _database.AuthKeys.FirstOrDefault(x => x.Key == key);
        }
        public static bool UpdateAuthKey(int id, string key, string ip)
        {
            var user = _database.Users.Find(id);
            if (user == null)
                return false;

            user.AuthKey.Key = key;
            user.AuthKey.LastAccessIP = ip;
            user.AuthKey.Expiration = DateTime.Now.Add(validity);
            _database.SaveChanges();
            return true;
        }

        public static bool AddAuthNullKey(string name)
        {
            var user = _database.Users.FirstOrDefault(x => x.Name == name);
            if (user == null)
                return false;
            user.AuthKey = new AuthKey();
            _database.SaveChanges();
            return true;
        }

        public static void Disconnect()
        {
            con.Close();
        }
    }
}