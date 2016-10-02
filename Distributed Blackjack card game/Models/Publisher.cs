using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Shared
{
    public class Publisher
    {
        private readonly Socket client;
        private readonly EndPoint remoteEndPoint;
        private const int Port = 10002;

        public Publisher()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
        }

        public void Publish(string topic, string eventData)
        {
            client.SendTo(Encoding.ASCII.GetBytes("Publish" + "," + topic + "," + eventData), remoteEndPoint);
        }
    }
}
