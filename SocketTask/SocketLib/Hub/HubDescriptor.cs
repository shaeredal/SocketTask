using System;
using System.Collections.Generic;
using System.Reflection;
using SocketLib.Models;

namespace SocketLib.Hub
{
    public class HubDescriptor
    {
        public Type HubType { get; set; }
        public string Name { get; set; }
        public Dictionary<MethodKeyModel, MethodInfo> Methods { get; set; } 
        public List<ClientModel> Clients { get; set; }

        public HubDescriptor(Type hubType, string name, Dictionary<MethodKeyModel, MethodInfo> methods)
        {
            HubType = hubType;
            Name = name;
            Methods = methods;
            Clients = new List<ClientModel>();
        }
    }
}
