using SocketLib.Hub;

namespace SocketLib.TEST
{
    public class TestHub2 : SocketHub
    {
        public void TestFunction(string first, string second, string third)
        {
            SendMessage(new object[] {first, second, third});
        }
    }
}
