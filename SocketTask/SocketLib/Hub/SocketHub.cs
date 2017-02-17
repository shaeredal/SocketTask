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
    }
}
