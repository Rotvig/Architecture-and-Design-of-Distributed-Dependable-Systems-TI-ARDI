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
                    var serializedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    var deserializedMessage = JsonConvert.DeserializeObject<Message>(serializedMessage);

                    if (deserializedMessage.Command != Command.Publish) continue;
                    if (string.IsNullOrEmpty(deserializedMessage.Topic)) continue;

                    var subscriberListForThisTopic = Filter.GetSubscribers(deserializedMessage.Topic);
                    var workerThreadParameters = new WorkerThreadParameters
                    {
                        Server = server,
                        SerializedMessage = serializedMessage,
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
            var server = workerThreadParameters.Server;
            var message = workerThreadParameters.DeserializedMessage.EventData;
            var subscriberListForThisTopic = workerThreadParameters.SubscriberListForThisTopic;
            var messagelength = workerThreadParameters.SerializedMessage.Length;

            if (subscriberListForThisTopic == null) return;

            if (workerThreadParameters.DeserializedMessage.SubscriptionId != null)
            {
                var subscriber = subscriberListForThisTopic.Single(x => x.SubscriptionId == workerThreadParameters.DeserializedMessage.SubscriptionId);
                server.SendTo(Encoding.ASCII.GetBytes(workerThreadParameters.SerializedMessage), messagelength, SocketFlags.None, subscriber.Endpoint);
                return;
            }

            foreach (var subscriber in subscriberListForThisTopic)
            {
                server.SendTo(Encoding.ASCII.GetBytes(workerThreadParameters.SerializedMessage), messagelength, SocketFlags.None, subscriber.Endpoint);
            }
        }
    }

    internal class WorkerThreadParameters
    {
        public Socket Server { get; set; }

        public string SerializedMessage { get; set; }
        public Message DeserializedMessage { get; set; }


        public List<SubscriberTuple> SubscriberListForThisTopic { get; set; }
    }
}
