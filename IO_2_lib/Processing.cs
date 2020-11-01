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
                            if (ID <= 0) return "Zły login lub hasło\n\r";
                            var key = GetHash(SHA256.Create(), command[2] + DateTime.Now.ToString() + new Random().Next().ToString()).Substring(1, 6);//skracam token by nie trzeba było kopiować tak długiego ciągu
                            if (SQLite.UpdateAuthKey(ID, key, IP))
                                return key;
                        }
                        return "ERROR";

                    case "REGISTER":
                    case "REG":
                        if (command.Count >= 4 && !SQLite.checkname(command[2]) && SQLite.Register(command[2], command[3]))
                        {
                            SQLite.AddAuthNullKey(command[2]);
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
                                if (SQLite.AddJob(id, name, des))
                                    return "SUCCES";
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
                                var res = SQLite.listJob(id);
                                var str = "";
                                foreach (object[] o in res)
                                    str += o[0] + "\n\r";
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
                                var res = SQLite.listJob(id);
                                var str = "";
                                foreach (object[] o in res)
                                    str += o[0] + " " + o[1] + "\n\r";
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
                                    var res = SQLite.Job(int.Parse(command[3]), id);
                                    if (res != null)
                                    {
                                        var str = res[2] + "\n\r" + res[3];
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

                    case "TIME":
                        return DateTime.Now.ToString();

                    case "VERSION":
                    case "VER":
                    case "V":
                        var v = Assembly.GetExecutingAssembly().GetName().Version;
                        return string.Format(CultureInfo.InvariantCulture, "SERVER: {0}.{1}.{2} (r{3})\n\rDB: {4}", v.Major, v.Minor, v.Build, v.Revision, SQLite.Version());

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
            if (res != null && IP == (string)res[1] && (DateTime)res[2] > DateTime.Now)
            {
                SQLite.UpdateAuthKey((int)res[0], key, IP);
                return (int)res[0];
            }
            return -1;
        }
    }
}