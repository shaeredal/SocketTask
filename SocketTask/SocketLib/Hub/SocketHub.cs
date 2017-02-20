using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SocketLib.Extensions;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class SocketHub
    {
        private static SocketHub instance;

        private Dictionary<MethodKeyModel, MethodInfo> methods;

        public static SocketHub GetInstance()
        {
            return instance ?? (instance = new SocketHub());
        }

        protected SocketHub()
        {
            methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(m => new MethodKeyModel {Name = m.Name, ParametersCount = m.GetParameters().Length});
        }

        public void ProcessRequest(ClientModel client, CallFunctonModel model)
        {
            var key = new MethodKeyModel
            {
                Name = model.functionName,
                ParametersCount = model.parameters.Length
            };
            if (methods.ContainsKey(key))
            {
                this.client = client;
                methods[key].Invoke(this, model.parameters);
            }
        }


        //TEST
        //TODO: Remove as soon as possible

        private ClientModel client;
        public void SendMessage(object[] parameters)
        {
            client.Client.SendMessage(JsonConvert.SerializeObject(new CallFunctonModel
            {
                functionName = "test",
                parameters = parameters
            }));
        }
    }
}
