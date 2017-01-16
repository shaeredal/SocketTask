using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Models;

namespace SocketLib
{
    public class WebSocketServer
    {
        private static WebSocketServer instance;

        private TcpListener listener;
        private Task listenTask;

        private Dictionary<string, ClientModel> clients;
        private readonly object locker = new object();

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

        private WebSocketServer()
        {
            clients = new Dictionary<string, ClientModel>();
        }

        private void Start()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            listener.Start();

            listenTask = new Task(Listen);
            listenTask.Start();
        }

        private void Listen()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                var processTask = new Task(() => AddClient(client));
                processTask.Start();
            }
        }

        private void AddClient(TcpClient client)
        {
            var hubName = Handshake(client);
            var id = Guid.NewGuid().ToString();
            var isSet = SetId(client, id);

            if (!isSet)
            {
                client.Close();
                return;
            };

            var clientModel = new ClientModel
            {
                Id = id,
                HubName = hubName,
                Client = client,
            };

            lock (locker)
            {
                clients.Add(id, clientModel);
            }
            var listenClientTask = new Task(() => ListenClient(client));
            listenClientTask.Start();
        }

        private string Handshake(TcpClient client)
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
                    var hubName = new Regex(@"(?<=^GET) \/(.+) (?=HTTP)").Match(data).Groups[1].Value.Trim();
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
                    return hubName;
                }
            }
        }

        private bool SetId(TcpClient client, string id)
        {
            var jsonId = $"{{\"setId\": \"{id}\"}}";
            client.SendMessage(jsonId);
            var responseJson = client.ReceiveMessage();

            SetIdResponse responseObj;
            try
            {
                responseObj = JsonConvert.DeserializeObject<SetIdResponse>(responseJson);
            }
            catch (Exception)
            {
                return false;
            }

            if (responseObj.Result == "IsSet" && responseObj.Id == id)
            {
                return true;
            }
            return false;
        }

        private void ListenClient(TcpClient client)
        {
            while (true)
            {
                var result = client.ReceiveMessage();
                ProcessClientRequest(result);
                //TODO: Close connection and remove client on close 
            }
        }

        private void ProcessClientRequest(string requestString)
        {
            //TODO: Implement
        }
    }
}
