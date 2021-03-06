﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Hub;
using SocketLib.Models;

namespace SocketLib
{
    public class WebSocketServer
    {
        private static WebSocketServer instance;

        private Dictionary<string, ClientModel> clients;
        private Dictionary<string, HubDescriptor> hubs;

        private TcpListener listener;

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

        public static ClientModel GetClient(string id)
        {
            return instance?.FindClient(id);
        }

        private WebSocketServer()
        {
            clients = new Dictionary<string, ClientModel>();
            hubs = new Dictionary<string, HubDescriptor>();
        }

        private void Start()
        {
            //TODO: Make more flexible (remove hardcode)
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            listener.Start();

            FindHubs();

            var listenTask = new Task(ListenConnections);
            listenTask.Start();
        }

        private void FindHubs()
        {
            var hubTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                hubTypes.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(SocketHub))));
            }

            foreach (var hubType in hubTypes)
            {
                var descriptor = CreateDesctiptor(hubType);
                hubs.Add(descriptor.Name, descriptor);
                //var getInstance = hubType.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            }
            //TODO: Create hubs script
        }

        private HubDescriptor CreateDesctiptor(Type hubType)
        {
            var name = char.ToLowerInvariant(hubType.Name[0]) + hubType.Name.Substring(1);
            if (hubs.ContainsKey(name))
            {
                throw new Exception("Two Hubs must not share the same name");
            }
            var methods = hubType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(m => new MethodKeyModel { Name = m.Name, ParametersCount = m.GetParameters().Length });
            return new HubDescriptor(hubType, name, methods);
        }

        private void ListenConnections()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                var processTask = new Task(() => AddClient(client));
                processTask.Start();
            }
        }

        //TODO:Refactor
        private void AddClient(TcpClient client)
        {
            client.Handshake();
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
                Client = client
            };

            var getHubsResponse = GetHubs(clientModel);
            if (getHubsResponse.Error)
            {
                client.Close();
                return;
            }

            lock (locker)
            {
                clients.Add(id, clientModel);
            }
            var listenClientTask = new Task(() => ListenClient(clientModel));
            listenClientTask.Start();
        }

        private bool SetId(TcpClient client, string id)
        {
            //TODO: Make better or something
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

        private GetHubsModel GetHubs(ClientModel client)
        {
            var result = GetHubNames(client.Client);
            if (result.Error)
            {
                return result;
            }
            result.HubNames = result.HubNames.Where(hubName => hubs.ContainsKey(hubName)).ToArray();
            foreach (var hubName in result.HubNames)
            {
                //TODO: if OnConnected is realized, call it here
                hubs[hubName].Clients.Add(client);
            }
            return result;
        }

        private GetHubsModel GetHubNames(TcpClient client)
        {
            //TODO: Make better or something
            var jsonHubs = JsonConvert.SerializeObject(new GetHubsModel {HubNames = new string[0]});
            client.SendMessage(jsonHubs);
            var response = client.ReceiveMessage();

            var responseObj = new GetHubsModel();
            try
            {
                responseObj = JsonConvert.DeserializeObject<GetHubsModel>(response.Message);
                return responseObj;
            }
            catch(Exception)
            {
                responseObj.Error = true;
                return responseObj;
            }
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
                new Task(() => ProcessRequest(client, result.Message)).Start();
            }
            RemoveClient(client);
        }

        private void ProcessRequest(ClientModel client, string request)
        {
            try
            {
                var callFunctionModel = JsonConvert.DeserializeObject<CallFunctonModel>(request);
                if (!string.IsNullOrEmpty(callFunctionModel.hubName) && hubs.ContainsKey(callFunctionModel.hubName))
                {
                    var hub = hubs[callFunctionModel.hubName];
                    var key = new MethodKeyModel
                    {
                        Name = callFunctionModel.functionName,
                        ParametersCount = callFunctionModel.parameters.Length
                    };
                    var hubInstance = (SocketHub)Activator.CreateInstance(hub.HubType);
                    hubInstance.HubName = hub.Name;
                    hubInstance.Clients = new ClientsProxy(hub.Name, hub.Clients, client);
                    if (hub.Methods.ContainsKey(key))
                    {
                        //TODO: Maybe make it possible to pass non-string parameters
                        hub.Methods[key].Invoke(hubInstance, callFunctionModel.parameters.Select(x => (object)x?.ToString()).ToArray());
                    }
                }
            }
            catch(Exception e)
            {
                //TODO: Implement
                // ignored
            }
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

        private ClientModel FindClient(string id)
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
