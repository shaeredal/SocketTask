using System;
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

        //private Dictionary<string, ClientModel> clients;
        private Dictionary<string, SocketHub> hubs;

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

//        public static ClientModel GetClient(string Id)
//        {
//            return instance == null ? null : instance.GetClient(Id);
//        }

        private WebSocketServer()
        {
//            clients = new Dictionary<string, ClientModel>();
            hubs = new Dictionary<string, Hub.SocketHub>();
        }

        private void Start()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            listener.Start();

            FindHubs();

            var listenTask = new Task(ListenConnections);
            listenTask.Start();
        }

        private void FindHubs()
        {
            var hubTypes = new List<Type>();
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                hubTypes.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(SocketHub))));
            }
            foreach (var hubType in hubTypes)
            {
                var getInstance = hubType.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (getInstance != null)
                {
                    if (hubs.ContainsKey(hubType.Name))
                    {
                        throw new Exception("Two Hubs must not share the same name");
                    }
                    hubs.Add(hubType.Name.ToLowerInvariant() ,(SocketHub)getInstance.Invoke(null, null));
                }
            }
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

        private void AddClient(TcpClient client)
        {
            var hubName = client.Handshake();
            if (!hubs.ContainsKey(hubName))
            {
                client.Close();
                return;
            }
            var hub = hubs[hubName];

            hub.AddClient(client);
        }
    }
}
