using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketLib.Models
{
    public class ClientModel
    {
        public string Id { get; set; }
        public TcpClient Client { get; set; } 
        public List<string> HubNames { get; set; }
    }
}
