using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Shared;

namespace PubSubServer
{
    public class SubscriberService
    {
        private const int Port = 10001;

        public void StartSubscriberService()
        {
            var th = new Thread(HostSubscriberService) {IsBackground = true};
            th.Start();
        }

        private void HostSubscriberService()
        {
            var localEP = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(localEP);


            StartListening(server);
        }

        private static void StartListening(Socket server)
        {
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var recv = 0;
            var data = new byte[1024];
            while (true)
            {
                recv = 0;
                data = new byte[1024];
                recv = server.ReceiveFrom(data, ref remoteEP);
                var messageSendFromClient = Encoding.ASCII.GetString(data, 0, recv);
                var messageParts = messageSendFromClient.Split(",".ToCharArray());

                if (!string.IsNullOrEmpty(messageParts[0]))
                {
                    switch (messageParts[0])
                    {
                        case "Subscribe":

                            if (!string.IsNullOrEmpty(messageParts[1]))
                            {
                                Filter.AddSubscriber(messageParts[1], remoteEP);
                            }


                            break;
                        case "UnSubscribe":

                            if (!string.IsNullOrEmpty(messageParts[1]))
                            {
                                Filter.RemoveSubscriber(messageParts[1], remoteEP);
                            }
                            break;
                    }
                }
            }
        }
    }
}
