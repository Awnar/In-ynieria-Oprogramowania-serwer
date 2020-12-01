using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IO_2_lib
{
    public class Serwer : AbstractSerwer
    {
        public Serwer(string IP = "127.0.0.1", int port = 9999) : base(IP, port) { }

        public delegate void TransmissionDataDelegate(NetworkStream stream, string IP);

        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient = TcpListener.AcceptTcpClient();
                Stream = TcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                transmissionDelegate.BeginInvoke(Stream, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString(), TransmissionCallback, TcpClient);
                Logger.Info?.Invoke($"Nowe przychodzące połączenie");
                //BeginDataTransmission(Stream, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());
            }
        }

        protected void BeginDataTransmission(NetworkStream stream, string IP)
        {
            byte[] buffer = new byte[Buffer_size];
            while (true)
            {
                try
                {
                    StringBuilder data = new StringBuilder();
                    do //pętla zczytująca całą zawartość w sokecie
                    {
                        var numberOfBytes = stream.Read(buffer, 0, Buffer_size);
                        data.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, numberOfBytes));

                    } while (stream.DataAvailable);

                    var execute = new Processing(data.ToString(), IP);

                    var dataOut = execute.ByteOut;

                    if (dataOut != null) stream.Write(dataOut, 0, dataOut.Length);
                }
                catch (Exception e)
                {
                     Logger.Info?.Invoke("ERROR");
                     Logger.Info?.Invoke(e.Message);
                    var dataOut = Encoding.UTF8.GetBytes("ERROR");
                    stream.Write(dataOut, 0, dataOut.Length);
                }
            }
        }

        protected override void BeginDataTransmission(NetworkStream stream) { }
        public override void Start()
        {
            Logger.Info?.Invoke("Serwer wystartował");
            SQLite.Init();
            StartListening();
            AcceptClient();
             Logger.Info?.Invoke("Koniec");
        }

        private void TransmissionCallback(IAsyncResult ar)
        {
        }
    }
}