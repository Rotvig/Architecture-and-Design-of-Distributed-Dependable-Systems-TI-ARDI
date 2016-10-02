using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

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

        public void Publish(string topic, Event @event, string eventData = "", Guid? subscriptionId = null)
        {
            client.SendTo(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(
                new Message
                {
                    Command = Command.Publish,
                    Topic = topic,
                    Event = @event,
                    EventData = eventData,
                    SubscriptionId = subscriptionId
                })),
                remoteEndPoint);
        }
    }
}
