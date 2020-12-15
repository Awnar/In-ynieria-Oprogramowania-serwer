using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Klient
{
    static class SynchronousSocketClient
    {
        private static readonly int buffer_size = 1024;
        private static TcpClient tcpClient = new TcpClient();
        private static NetworkStream networkStream;

        static public void Init()
        {
            tcpClient.Connect("127.0.0.1", 9999);
            networkStream = tcpClient.GetStream();
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
                data.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, numberOfBytes));

            } while (networkStream.DataAvailable);

            return data.ToString();
        }

        static public void Login(string name, string pass)
        {
            var tmp = Send("LOGIN " + name + " " + pass);
        }
    }
}
