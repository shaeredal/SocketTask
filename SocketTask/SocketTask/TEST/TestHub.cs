using SocketLib.Hub;

namespace SocketTask.TEST
{
    public class TestHub : SocketHub
    {
        public void ThisIsForReal(string message)
        {
            Clients.Caller.write(message);
        }
    }
}