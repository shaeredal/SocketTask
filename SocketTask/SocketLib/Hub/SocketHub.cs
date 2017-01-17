using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class SocketHub
    {
        private static SocketHub instance;

        private Dictionary<string, ClientModel> clients;

        private readonly object locker = new object();

        public Dictionary<string, ClientModel> Clients => clients;
        public static SocketHub GetInstance()
        {
            if (instance == null)
            {
                instance = new SocketHub();
            }
            return instance;
        }

        public SocketHub()
        {
            clients = new Dictionary<string, ClientModel>();
        }

        //TODO:Refactor
        public void AddClient(TcpClient client)
        {
            string id;
            do
            {
                id = Guid.NewGuid().ToString();
            } while (clients.ContainsKey(id)); 

            if (!SetId(client, id))
            {
                client.Close();
                return;
            };

            var clientModel = new ClientModel
            {
                Id = id,
                Client = client,
            };
            lock (locker)
            {
                clients.Add(id, clientModel);
            }
            var listenClientTask = new Task(() => ListenClient(clientModel));
            listenClientTask.Start();
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
            //TODO: Remove
            client.Client.SendMessage(JsonConvert.SerializeObject(new CallFunctonModel
            {
                callFunction = "test",
                parameters = new object[] { "Hello, World!", 42, 0}
            }));
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

        private ClientModel GetClient(string id)
        {
            ClientModel result;
            lock (locker)
            {
                result = clients.ContainsKey(id) ? clients[id] : null;
            }
            return result;
        }
    }
}
