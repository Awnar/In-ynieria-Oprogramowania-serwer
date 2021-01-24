using IO_2_lib.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace IO_2_lib
{
    public class SQLite
    {
        static DatabaseContext _database;
        static SQLiteConnection con;
        static TimeSpan validity = new TimeSpan(5, 0, 0);

        public static void Init()
        {
            _database = new DatabaseContext();
            _database.Database.EnsureCreated();
        }

        public static void Init(DatabaseContext databaseContext)
        {
            _database = databaseContext;
        }

        public static int Login(string name, string pass)
        {
            var user =_database.Users.FirstOrDefault(x => x.Name == name && x.Password == CreateMD5(pass));
            return user == null ? -1 : user.Id;
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
                Password = CreateMD5(pass)
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
                Description = description,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
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

        public static void DelTask(int id, int user_ID)
        {
            var task = _database.Tasks.FirstOrDefault(x => x.Id == id && x.User.Id == user_ID);
            if (task == null)
                return;
            _database.Tasks.Remove(task);
            _database.SaveChanges();
        }

        public static bool UpdateTask(int user_ID, int id, string name, string description)
        {
            var task = _database.Tasks.FirstOrDefault(x => x.Id == id && x.User.Id == user_ID);
            if (task == null)
                return false;

            task.Description = description;
            task.Name = name;
            task.UpdateTime = DateTime.Now;
            _database.SaveChanges();
            return true;
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

        private static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}