using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace IO_2_lib
{
    class SQLite
    {
        static SQLiteConnection con;
        static TimeSpan validity = new TimeSpan(5, 0, 0);

        public static void Init(string cs = "Data Source=:memory:")
        {
            con = new SQLiteConnection(cs);
            con.Open();

            if (cs == "Data Source=:memory:")
            {
                var cmd = new SQLiteCommand(con);

                cmd.CommandText = @"CREATE TABLE users (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL,
                        name STRING UNIQUE  NOT NULL,
                        pass NOT NULL
                    );";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE auth_key (
                        user_ID INTEGER REFERENCES users (ID) PRIMARY KEY NOT NULL,
                        [key] STRING UNIQUE,
                        validity_time TIME DEFAULT (date('now')),
                        last_IP STRING
                     );";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE ToDo (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL,
                        user_id INT REFERENCES users (ID) NOT NULL,
                        name STRING,
                        description TEXT
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public static int Login(string name, string pass)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT ID FROM users WHERE name=@name AND pass=@pass";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@pass", pass);
            cmd.Prepare();
            var tmp = cmd.ExecuteReader();
            if (tmp.Read())
            {
                int res = (int)(long)tmp["ID"];//domyślnie zwraca obiect który jest typen long, bezpośredne rzutowanie na int nie działa stąd ten dziwny zapis
                tmp.Close();
                return res;
            }
            return -1;
        }

        public static bool checkname(string name)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT name FROM users WHERE name=@name";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();
            var tmp = cmd.ExecuteReader();
            bool res = tmp.Read();
            tmp.Close();
            return res;
        }

        public static bool Register(string name, string pass)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "INSERT INTO users(name, pass) VALUES(@name, @pass)";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@pass", pass);
            cmd.Prepare();
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool AddJob(int user_id, string name, string description)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "INSERT INTO ToDo (user_id, name, description) VALUES (@user_id, @name, @description)";
            cmd.Parameters.AddWithValue("@user_id", user_id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Prepare();
            return cmd.ExecuteNonQuery() > 0;
        }
        public static List<object[]> listJob(int user_ID)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT ID, name FROM ToDO WHERE user_id=@user_id";
            cmd.Parameters.AddWithValue("@user_id", user_ID);
            cmd.Prepare();
            var exe = cmd.ExecuteReader();
            var res = new List<object[]>();
            while (exe.Read())
                res.Add(new object[2] { exe[0], exe[1] });
            exe.Close();
            return res;
        }

        public static object[] Job(int ID, int user_ID)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM ToDO WHERE user_id=@user_id AND ID=@ID";
            cmd.Parameters.AddWithValue("@user_id", user_ID);
            cmd.Parameters.AddWithValue("@ID", ID);
            cmd.Prepare();
            var exe = cmd.ExecuteReader();
            object[] res = null;
            while (exe.Read())
                res = new object[4] { exe[0], exe[1], exe[2], exe[3] };
            exe.Close();
            return res;
        }

        public static object[] VerifyAuthKey(string key)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM auth_key WHERE key=@key";
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Prepare();
            var tmp = cmd.ExecuteReader();
            if (tmp.Read())
            {
                var res = new object[] { tmp["user_ID"], tmp["last_IP"], tmp["validity_time"] };
                tmp.Close();
                return res;
            }
            return null;
        }
        public static bool UpdateAuthKey(int id, string key, string ip)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "UPDATE auth_key SET key=@key, validity_time=@time, last_IP=@IP WHERE user_ID=@ID";
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@time", DateTime.Now.Add(validity));
            cmd.Parameters.AddWithValue("@IP", ip);
            cmd.Prepare();
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool AddAuthNullKey(string name)
        {
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "INSERT INTO auth_key (user_ID, key, validity_time, last_IP) VALUES((SELECT ID FROM users WHERE name=@name), NULL, NULL, NULL)";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();
            return cmd.ExecuteNonQuery() > 0;
        }

        public static string Version()
        {
            var cmd = new SQLiteCommand("SELECT SQLITE_VERSION()", con);
            return cmd.ExecuteScalar().ToString();
        }

        public static void Disconnect()
        {
            con.Close();
        }
    }
}