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
            var hubName = client.Handshake();
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
            var listenClientTask = new Task(() => ListenClient(clientModel));
            listenClientTask.Start();
        }

        private void RemoveClient(ClientModel client)
        {
            client.Client.Close();
            lock (locker)
            {
                if (clients.ContainsKey(client.Id))
                {
                    clients.Remove(client.Id);
                }
            }
        }

        private bool SetId(TcpClient client, string id)
        {
            var jsonId = $"{{\"setId\": \"{id}\"}}";
            client.SendMessage(jsonId);
            var response = client.ReceiveMessage();

            SetIdResponseModel responseObj;
            try
            {
                responseObj = JsonConvert.DeserializeObject<SetIdResponseModel>(response.Message);
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

        private void ListenClient(ClientModel client)
        {
            while (true)
            {
                var result = client.Client.ReceiveMessage();
                if (result.Disconnected)
                {
                    break;
                }
                ProcessClientRequest(client, result.Message);
            }
            RemoveClient(client);
        }

        private void ProcessClientRequest(ClientModel client, string requestString)
        {
            //TODO: Implement
        }
    }
}
