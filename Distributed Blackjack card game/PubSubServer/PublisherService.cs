using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Shared;

namespace PubSubServer
{
    public class PublisherService
    {
        private const int Port = 10002;
        private static MessageService messageService;

        public void StartPublisherService()
        {
            var th = new Thread(HostPublisherService) {IsBackground = true};
            th.Start();
            messageService = new MessageService();
        }

        private void HostPublisherService()
        {
            var localEp = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(localEp);

            StartListening(server);
        }

        private static void StartListening(Socket server)
        {
            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    var recv = 0;
                    var data = new byte[1024];
                    recv = server.ReceiveFrom(data, ref remoteEp);
                    var deserializedMessage = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(data, 0, recv));

                    if (deserializedMessage.Header.Command != Command.Publish) continue;
                    if (string.IsNullOrEmpty(deserializedMessage.Header.Topic)) continue;

                    var subscriberListForThisTopic = Subscribers.GetSubscribers(deserializedMessage.Header.Topic);

                    if (subscriberListForThisTopic == null) return;

                    if (deserializedMessage.Content.SubscriptionId != null &&
                        deserializedMessage.Header.PublishToSubscriptionId)
                    {
                        var subscriber =
                            subscriberListForThisTopic.Single(
                                x => x.SubscriptionId == deserializedMessage.Content.SubscriptionId);
                        PublishMessage(deserializedMessage, subscriber);

                    }
                    else
                    {
                        foreach (var subscriber in subscriberListForThisTopic)
                        {
                            PublishMessage(deserializedMessage, subscriber);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static void PublishMessage(Message message, SubscriberTuple subscriber)
        {
            messageService.AddItemToList(message, subscriber.Endpoint);
        }
    }

    internal class WorkerThreadParameters
    {
        public MessageService MessageService { get; set; }
        public Message DeserializedMessage { get; set; }
        public List<SubscriberTuple> SubscriberListForThisTopic { get; set; }
    }
}
