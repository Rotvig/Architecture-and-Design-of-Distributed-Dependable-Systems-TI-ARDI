﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
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

        private static void HostSubscriberService()
        {
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(new IPEndPoint(Utils.GetLocalIp4Address(), Port));


            StartListening(server);
        }

        private static void StartListening(Socket server)
        {
            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                var receive = 0;
                var data = new byte[1024];
                receive = server.ReceiveFrom(data, ref remoteEp);
                var message = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(data, 0, receive));

                switch (message.Header.Command)
                {
                    case Command.Subscribe:
                        Filter.AddSubscriber(message.Header.Topic, message.Content.SubscriptionId.Value, remoteEp);
                        break;
                    case Command.Unsubscribe:
                        Filter.RemoveSubscriber(message.Header.Topic, message.Content.SubscriptionId.Value);
                        break;
                }
            }
        }
    }
}
