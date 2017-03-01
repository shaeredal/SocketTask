using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class UserProxy : DynamicObject
    {
        private List<ClientModel> users;
        public string HubName { get; private set; }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Invoke(binder.Name, args);
            return true;
        }

        public Task Invoke(string method, params object[] args)
        {
            users.ForEach(u => new Task(() => SendMessage(u.Client, new CallFunctonModel
                {
                    functionName = method,
                    parameters = args,
                    hubName = HubName
                }
            )).Start());
            return Task.FromResult(0);
        }

        private void SendMessage(TcpClient client, CallFunctonModel callFunctonModel)
        {
            client.SendMessage(JsonConvert.SerializeObject(callFunctonModel));
        }

        public UserProxy(string hubName, IEnumerable<ClientModel> users)
        {
            HubName = hubName;
            this.users = users.ToList();
        }
    }
}
