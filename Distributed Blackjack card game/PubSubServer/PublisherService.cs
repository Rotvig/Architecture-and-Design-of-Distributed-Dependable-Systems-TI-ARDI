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

                    var subscriberListForThisTopic = Filter.GetSubscribers(deserializedMessage.Header.Topic);
                    var workerThreadParameters = new WorkerThreadParameters
                    {
                        MessageService = messageService,
                        SubscriberListForThisTopic = subscriberListForThisTopic,
                        DeserializedMessage = deserializedMessage
                    };

                    ThreadPool.QueueUserWorkItem(Publish, workerThreadParameters);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public static void Publish(object stateInfo)
        {
            var workerThreadParameters = (WorkerThreadParameters) stateInfo;
            var subscriberListForThisTopic = workerThreadParameters.SubscriberListForThisTopic;

            if (subscriberListForThisTopic == null) return;

            if (workerThreadParameters.DeserializedMessage.Content.SubscriptionId != null && workerThreadParameters.DeserializedMessage.Header.PublishToSubscriptionId)
            {
                var subscriber = subscriberListForThisTopic.Single(x => x.SubscriptionId == workerThreadParameters.DeserializedMessage.Content.SubscriptionId);
                PublishMessage(workerThreadParameters.MessageService, workerThreadParameters.DeserializedMessage, subscriber);
                return;
            }

            foreach (var subscriber in subscriberListForThisTopic)
            {
                PublishMessage(workerThreadParameters.MessageService, workerThreadParameters.DeserializedMessage, subscriber);
            }
        }

        private static void PublishMessage(MessageService service, Message message, SubscriberTuple subscriber)
        {
            service.AddItemToList(message, subscriber.Endpoint);
        }
    }

    internal class WorkerThreadParameters
    {
        public MessageService MessageService { get; set; }
        public Message DeserializedMessage { get; set; }
        public List<SubscriberTuple> SubscriberListForThisTopic { get; set; }
    }
}
