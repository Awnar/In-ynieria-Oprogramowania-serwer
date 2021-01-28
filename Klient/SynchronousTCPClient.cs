using System;
using System.Net.Sockets;
using System.Text;

namespace Klient
{
    static class SynchronousTCPClient
    {
        private static readonly int buffer_size = 1024;
        private static TcpClient tcpClient = new TcpClient();
        private static NetworkStream networkStream;
        private static string key = null;

        static public void Init()
        {
            tcpClient.Connect("127.0.0.1", 9999);
            networkStream = tcpClient.GetStream();
        }

        static public void Close()
        {
            tcpClient.Close();
            key = null;
        }

        static private string Send(string message)
        {
            var bits = Encoding.Default.GetBytes(message);
            networkStream.Write(bits, 0, bits.Length);

            byte[] buffer = new byte[buffer_size];
            StringBuilder data = new StringBuilder();
            do
            {
                var numberOfBytes = networkStream.Read(buffer, 0, buffer_size);
                data.AppendFormat("{0}", Encoding.UTF8.GetString(buffer, 0, numberOfBytes));

            } while (networkStream.DataAvailable);

            var tmp = data.ToString().Trim();

            if (tmp == "ERROR") throw new Exception();
            return tmp;
        }

        static public string Login(string name, string pass)
        {
            var answer = Send("LOGIN " + name + " " + pass).Replace("\n\r", " ");
            if (answer.Length > 8)
                return answer;
            key = answer;
            return null;
        }

        static public string Register(string name, string pass)
        {
            return Send("REG " + name + " " + pass).Replace("\n\r", " ");
        }

        static public string JobList()
        {
            if (key == null)
                return null;
            return Send("LIST " + key);
        }

        static public void SelectJob(int id)
        {
            if (key == null)
                return;
            var answer = Send("JOB " + key+" " +id);
        }

        static public string AddJob(string name, string descryption)
        {
            if (key == null)
                return "Nie jesteś zalogowany";
            var answer = Send("ADD " + key+ " NAME "+name+" DES "+descryption).Replace("\n\r", " ");
            if (answer == "SUCCES")
                return null;
            return answer;
        }

        static public bool DelJob(int id)
        {
            if (key == null)
                return false;
            var answer = Send("DEL " + key + " " + id).Replace("\n\r", " ");
            if (answer != "SUCCES")
                return false;
            return true;
        }

        static public bool UpdateJob(int id, string name, string descryption)
        {
            if (key == null)
                return false;
            var answer = Send("UPDATE " + key +" "+ id + " NAME " + name + " DES " + descryption).Replace("\n\r", " ");
            if (answer != "SUCCES")
                return false;
            return true;
        }
    }
}
