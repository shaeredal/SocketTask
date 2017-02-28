using System;
using System.Collections.Generic;
using System.Reflection;

namespace SocketLib.Models
{
    public class HubDescriptor
    {
        public Type HubType { get; set; }
        public string Name { get; set; }
        public Dictionary<MethodKeyModel, MethodInfo> Methods { get; set; } 
    }
}
