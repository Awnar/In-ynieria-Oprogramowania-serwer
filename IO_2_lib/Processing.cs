using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IO_2_lib
{
    class Processing
    {
        public string Commands { private get; set; }
        public string IP { private get; set; }

        public Processing(byte[] data, string IP)
        {
            Commands = Encoding.UTF8.GetString(data);
            this.IP = IP;
        }

        public Processing(string data, string IP)
        {
            Commands = data;
            this.IP = IP;
        }

        public Processing()
        {
        }

        /// <summary>
        /// Wykonaj polecenie oraz zwróć dane w postaci tablicy byte
        /// </summary>
        public byte[] ByteOut
        {
            get
            {
                if (string.IsNullOrEmpty(Commands)) return null;
                var tmp = Encoding.UTF8.GetBytes(start() + "\n\r");
                Commands = null;
                return tmp;

            }
        }

        /// <summary>
        /// Wykonaj polecenie oraz zwróć dane w postaci string
        /// </summary>
        public string StringOut
        {
            get
            {
                if (string.IsNullOrEmpty(Commands)) return null;
                var tmp = start() + "\n\r";
                Commands = null;
                return tmp;

            }
        }

        private string start()
        {
            var command = new List<string> { Commands.Trim() };
            command.AddRange(command[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            if (command.Count > 1)
                switch (command[1].ToUpper())
                {
                    case "LOGIN":
                        if (command.Count >= 4)
                        {
                            var ID = SQLite.Login(command[2], command[3]);
                            if (ID <= 0)
                            {
                                Logger.Info?.Invoke($"Użytkownik wpisał zły login lub hasło");
                                return "Zły login lub hasło\n\r";
                            }
                            
                            var key = GetHash(SHA256.Create(), command[2] + DateTime.Now.ToString() + new Random().Next().ToString()).Substring(1, 6);//skracam token by nie trzeba było kopiować tak długiego ciągu
                            if (SQLite.UpdateAuthKey(ID, key, IP))
                            {
                                Logger.Info?.Invoke($"Użytkownik zalogował się w systemie");
                                return key;
                            }                               
                        }
                        return "ERROR";

                    case "REGISTER":
                    case "REG":
                        if (command.Count >= 4 && !SQLite.checkname(command[2]) && SQLite.Register(command[2], command[3]))
                        {
                            SQLite.AddAuthNullKey(command[2]);
                            Logger.Info?.Invoke($"Zarejestrowano nowego użytkownika.");
                            return "Użytkownik dodany";
                        }
                        return "ERROR";

                    case "ADDJOB":
                    case "ADD":
                        if (command.Count >= 7)
                        {
                            /** 2 => key
                             *  3 => 'name'
                             *  3+n => nazwa
                             *  3+n+1 => 'description'
                             *  ... => description
                             */
                            var id = VerifyKey(command[2], IP);
                            if (id >= 0)
                            {
                                string name = "";
                                int i = 3;
                                if (command[3].ToUpper() == "NAME")
                                    while (++i < command.Count && command[i].ToUpper() != "DESCRIPTION" && command[i].ToUpper() != "DESCRIPT" && command[i].ToUpper() != "DES")
                                        name += command[i] + " ";
                                else return "ERROR";
                                string des = "";
                                i++;
                                for (; command.Count > i; i++)
                                    des += command[i] + " ";
                                if (SQLite.AddTask(id, name, des))
                                {
                                    Logger.Info?.Invoke($"Dodano task z nazwą: {name}");
                                    return "SUCCES";
                                }
                                    
                            }
                            else return "Login again";
                        }
                        return "ERROR";

                    case "LISTJOB":
                    case "LIST":
                        if (command.Count >= 2)
                        {
                            var id = VerifyKey(command[2], IP);
                            if (id >= 0)
                            {
                                var res = SQLite.ListTasks(id);
                                var str = "";
                                foreach (var task in res)
                                    str += task.ToString() + "\n\r";
                                Logger.Info?.Invoke($"Pobrano wszystkie taski z bazy danych");
                                return str.Trim();
                            }
                        }
                        return "ERROR";

                    case "NAMELISTJOB":
                    case "NAMELIST":
                        if (command.Count >= 2)
                        {
                            var id = VerifyKey(command[2], IP);
                            if (id >= 0)
                            {
                                var res = SQLite.ListTasks(id);
                                var str = "";
                                foreach (var task in res)
                                    str += task.Name + "\n\r";
                                return str.Trim();
                            }
                        }
                        return "ERROR";

                    case "JOB":
                        if (command.Count >= 3)
                        {
                            var id = VerifyKey(command[2], IP);
                            if (id >= 0)
                            {
                                try
                                {
                                    var res = SQLite.GetTask(int.Parse(command[3]), id);
                                    if (res != null)
                                    {
                                        var str = res.ToString();
                                        Logger.Info?.Invoke($"Użytkownik pobrał task z bazy");
                                        return str.Trim();
                                    }
                                }
                                catch (Exception)
                                {
                                    return "ERROR";
                                }
                            }
                        }
                        return "ERROR";

                    case "DELJOB":
                    case "DEL":
                        if (command.Count >= 3)
                        {
                            var id = VerifyKey(command[2], IP);
                            if (id >= 0)
                            {
                                SQLite.DelTask(int.Parse(command[3]), id);
                                Logger.Info?.Invoke($"Usunięto wpis");
                                return "SUCCES";
                            }
                        }
                        return "ERROR";

                    case "UPDATEJOB":
                    case "UPDATE":
                        if (command.Count >= 8)
                        {
                            /** 2 => key
                             *  3 => 'name'
                             *  3+n => nazwa
                             *  3+n+1 => 'description'
                             *  ... => description
                             */
                            var id = VerifyKey(command[2], IP);
                            if (id >= 0)
                            {
                                string name = "";
                                int i = 4;
                                if (command[4].ToUpper() == "NAME")
                                    while (++i < command.Count && command[i].ToUpper() != "DESCRIPTION" && command[i].ToUpper() != "DESCRIPT" && command[i].ToUpper() != "DES")
                                        name += command[i] + " ";
                                else return "ERROR";
                                string des = "";
                                i++;
                                for (; command.Count > i; i++)
                                    des += command[i] + " ";

                                if (SQLite.UpdateTask(id, int.Parse(command[3]), name, des))
                                {
                                    Logger.Info?.Invoke($"Zaktualizowano task z nazwą: {name}");
                                    return "SUCCES";
                                }

                            }
                        }
                        return "ERROR";

                    case "TIME":
                        return DateTime.Now.ToString();

                    case "VERSION":
                    case "VER":
                    case "V":
                        Logger.Info?.Invoke($"Pobrano informacje o wersji");
                        var v = Assembly.GetExecutingAssembly().GetName().Version;
                        return string.Format(CultureInfo.InvariantCulture, "SERVER: {0}.{1}.{2} (r{3})\n\r", v.Major, v.Minor, v.Build, v.Revision);

                    default:
                        return "UNKNOWN COMMAND";
                }
            return "";
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }
        private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            var hashOfInput = GetHash(hashAlgorithm, input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }

        private static int VerifyKey(string key, string IP)
        {
            var res = SQLite.VerifyAuthKey(key);
            if (res != null && IP == res.LastAccessIP && res.Expiration > DateTime.Now)
            {
                SQLite.UpdateAuthKey(res.UserId, key, IP);
                return res.UserId;
            }
            return -1;
        }
    }
}