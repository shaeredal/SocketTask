using System;
using System.Collections.Generic;
using System.Linq;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class ClientsProxy
    {
        private readonly ClientModel caller;
        private readonly List<ClientModel> clients;
        public string HubName { get; }
        public dynamic Caller { get; private set; }
        public dynamic All { get; private set; }
        public dynamic Other { get; private set; }

        public dynamic User(string id)
        {
            return new UserProxy(HubName, clients.FindAll(c => c.Id == id));
        }

        public dynamic Users(IEnumerable<string> ids)
        {
            return new UserProxy(HubName, clients.FindAll(c => ids.Contains(c.Id)));
        }

        public dynamic AllExcept(string id)
        {
            return new UserProxy(HubName, clients.Where(c => c.Id != id));
        }

        public ClientsProxy(string hubName, List<ClientModel> clients, ClientModel caller)
        {
            HubName = hubName;
            this.clients = clients;
            this.caller = caller;
            Caller = new UserProxy(hubName, new List<ClientModel> {caller});
            All = new UserProxy(hubName, clients);
            Other = AllExcept(caller.Id);
        }
    }
}
