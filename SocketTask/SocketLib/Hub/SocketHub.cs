using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class SocketHub
    {
        private static SocketHub instance;

        public static SocketHub GetInstance()
        {
            if (instance == null)
            {
                instance = new SocketHub();
            }
            return instance;
        }

        public void ProcessRequest(ClientModel client, CallFunctonModel model)
        {
            //TODO: Implement
            //TODO: Remove
            client.Client.SendMessage(JsonConvert.SerializeObject(new CallFunctonModel
            {
                callFunction = "test",
                parameters = new object[] { "Hello, World!", 42, model.callFunction }
            }));
        }
    }
}
