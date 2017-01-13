using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SocketLib
{
    public class WebSocketServer
    {
        private static WebSocketServer instance;

        private TcpListener listener;
        private Task listenTask;

        private List<TcpClient> clients;
        private object locker = new object();

        public static bool Create()
        {
            if (instance != null)
            {
                return false;
            }
            instance = new WebSocketServer();
            instance.Start();
            return true;
        }

        private void Start()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            listener.Start();

            listenTask = new Task(Listen);
            listenTask.Start();
        }

        private void AddClient(TcpClient client)
        {
            lock (locker) { 
                clients.Add(client);
            }
        }

        private void Listen()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                var processTask = new Task(() => Handshake(client));
                processTask.Start();

                //                var connectionTask = new Task(() => AddClient(client));
                //                connectionTask.Start();
            }
        }

        private void Handshake(TcpClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                while (!stream.DataAvailable) ;

                var bytes = new byte[client.Available];
                stream.Read(bytes, 0, bytes.Length);
                var data = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(data))
                {
                    var key = Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() +
                                "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    );
                    var response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                            + "Connection: Upgrade" + Environment.NewLine
                            + "Upgrade: websocket" + Environment.NewLine
                            + "Sec-WebSocket-Accept: " + key + Environment.NewLine
                            + Environment.NewLine);

                    stream.Write(response, 0, response.Length);


                    //TODO: Remove
                    var testMessage = "azazazazazqweqweqweqweqweqweqweqweqweqweqqqqqqqweqwerqwerqwerqwer" +
                                      "qwerqwrqwerqwerqwerqwerqwerqwerqwerqwerqwerwqerqwerwqrqwrqwerqwer" +
                                      "qwerqwerqwerqwerqwerqwerqwerqwerqwerqwerqwerqwerqwerwqerwqerqwerw" +
                                      "qerwqerwqerqwerqwerwqerqwrqerqwerwerqwerqwerqwerqwerqwerqerqerqwer";
                    SendMessage(stream, testMessage);
                    ListenClient(client);

                    return;
                }
            }
        }

        private void SendMessage(NetworkStream stream, string message)
        {
            var result = EncodeMessage(message);
            stream.Write(result.ToArray(), 0, result.Count);
        }

        private List<byte> EncodeMessage(string message)
        {
            var bytesRaw = Encoding.UTF8.GetBytes(message);

            var result = new List<byte>();
            result.Add(0x81);

            if (bytesRaw.Length <= 125)
            {
                result.Add((byte)bytesRaw.Length);
            }
            else if (bytesRaw.Length >= 126 && bytesRaw.Length <= 65535)
            {
                result.Add(126);
                result.Add((byte)((bytesRaw.Length >> 8) & 255));
                result.Add((byte)((bytesRaw.Length) & 255));
            }
            else
            {
                result.Add(127);
                result.Add((byte)((bytesRaw.Length >> 56) & 255));
                result.Add((byte)((bytesRaw.Length >> 48) & 255));
                result.Add((byte)((bytesRaw.Length >> 40) & 255));
                result.Add((byte)((bytesRaw.Length >> 32) & 255));
                result.Add((byte)((bytesRaw.Length >> 24) & 255));
                result.Add((byte)((bytesRaw.Length >> 16) & 255));
                result.Add((byte)((bytesRaw.Length >> 8) & 255));
                result.Add((byte)((bytesRaw.Length) & 255));
            }

            result.AddRange(bytesRaw);

            return result;
        }

        private string DecodeMessage(byte[] bytes)
        {
            var secondByte = bytes[1];
            var dataLength = secondByte & 127;
            var indexFirstMask = 2;

            if (dataLength == 126)
            {
                indexFirstMask = 4;
            }
            else if (dataLength == 127)
            {
                indexFirstMask = 10;
            }

            var masks = bytes.Skip(indexFirstMask).Take(4).ToList();
            var indexFirstDataByte = indexFirstMask + 4;
            var decoded = new byte[bytes.Length - indexFirstDataByte];
            
            for (int i = indexFirstDataByte,  j = 0; i < bytes.Length; i++, j++)
            {
                decoded[j] = (byte)(bytes[i] ^ masks[j % 4]);
            }

            return Encoding.UTF8.GetString(decoded);
        }

        private void ListenClient(TcpClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                while (!stream.DataAvailable) ;
                var bytes = new byte[client.Available];
                stream.Read(bytes, 0, bytes.Length);
                var result = DecodeMessage(bytes);
                //TODO: Close connection and remove client on close 
            }
        }

        //        public static bool Stop()
        //        {
        //            if (!IsWorking)
        //            {
        //                return false;
        //            }
        //            instance.StopServer();
        //            instance = null;
        //            return true;
        //        }
        //
        //        private void StopServer()
        //        {
        //            isListen = false;
        //        }
    }
}
