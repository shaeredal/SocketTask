using System;
using System.Net.Sockets;

namespace SocketLib.Models
{
    public class ClientModel
    {
        public string Id { get; set; }
        public string HubName { get; set; }
        public TcpClient Client { get; set; } 
    }
}
