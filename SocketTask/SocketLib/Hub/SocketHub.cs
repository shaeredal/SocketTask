using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class SocketHub
    {
        //TEST
        //TODO: Remove as soon as possible

        public ClientModel Client { get; set; }
        public void SendMessage(object[] parameters)
        {
            Client.Client.SendMessage(JsonConvert.SerializeObject(new CallFunctonModel
            {
                functionName = "test",
                parameters = parameters
            }));
        }
    }
}
