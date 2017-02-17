using System.Net.Sockets;

namespace SocketLib.Models
{
    public class ClientModel
    {
        public string Id { get; set; }
        public TcpClient Client { get; set; } 
    }
}
