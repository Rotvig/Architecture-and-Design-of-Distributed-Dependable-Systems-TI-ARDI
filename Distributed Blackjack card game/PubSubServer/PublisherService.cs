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

        public void StartPublisherService()
        {
            var th = new Thread(HostPublisherService) {IsBackground = true};
            th.Start();
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
                    var message = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(data, 0, recv));

                    if (message.Command != Command.Publish) continue;
                    if (string.IsNullOrEmpty(message.Topic)) continue;

                    var subscriberListForThisTopic = Filter.GetSubscribers(message.Topic);
                    var workerThreadParameters = new WorkerThreadParameters
                    {
                        Server = server,
                        EventData = message.EventData,
                        SubscriberListForThisTopic = subscriberListForThisTopic,
                        SubscriptionId = message.SubscriptionId
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
            var server = workerThreadParameters.Server;
            var message = workerThreadParameters.EventData;
            var subscriberListForThisTopic = workerThreadParameters.SubscriberListForThisTopic;
            var messagelength = message.Length;

            if (subscriberListForThisTopic == null) return;

            if (workerThreadParameters.SubscriptionId != null)
            {
                var subscriber = subscriberListForThisTopic.Single(x => x.SubscriptionId == workerThreadParameters.SubscriptionId);
                server.SendTo(Encoding.ASCII.GetBytes(message + "," + subscriber.SubscriptionId), messagelength, SocketFlags.None, subscriber.Endpoint);
                return;
            }

            foreach (var subscriber in subscriberListForThisTopic)
            {

                server.SendTo(Encoding.ASCII.GetBytes(message + "," + subscriber.SubscriptionId), messagelength, SocketFlags.None, subscriber.Endpoint);
            }
        }
    }

    internal class WorkerThreadParameters
    {
        public Socket Server { get; set; }

        public string EventData { get; set; }

        public List<SubscriberTuple> SubscriberListForThisTopic { get; set; }

        public Guid? SubscriptionId;
    }
}
