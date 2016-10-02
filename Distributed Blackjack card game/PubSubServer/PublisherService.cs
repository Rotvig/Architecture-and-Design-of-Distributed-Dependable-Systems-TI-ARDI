using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var recv = 0;
            var data = new byte[1024];
            while (true)
            {
                try
                {
                    recv = 0;
                    data = new byte[1024];
                    recv = server.ReceiveFrom(data, ref remoteEP);
                    var messageSendFromClient = Encoding.ASCII.GetString(data, 0, recv);
                    var messageParts = messageSendFromClient.Split(",".ToCharArray());
                    var command = messageParts[0];
                    var topicName = messageParts[1];
                    if (!string.IsNullOrEmpty(command))
                    {
                        if (messageParts[0] == "Publish")
                        {
                            if (!string.IsNullOrEmpty(topicName))
                            {
                                var eventParts = new List<string>(messageParts);
                                eventParts.RemoveRange(0, 1);
                                var message = MakeCommaSeparatedString(eventParts);
                                List<EndPoint> subscriberListForThisTopic = Filter.GetSubscribers(topicName);
                                var workerThreadParameters = new WorkerThreadParameters();
                                workerThreadParameters.Server = server;
                                workerThreadParameters.Message = message;
                                workerThreadParameters.SubscriberListForThisTopic = subscriberListForThisTopic;

                                ThreadPool.QueueUserWorkItem(Publish, workerThreadParameters);
                            }
                        }
                    }
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
            var message = workerThreadParameters.Message;
            var subscriberListForThisTopic = workerThreadParameters.SubscriberListForThisTopic;
            var messagelength = message.Length;

            if (subscriberListForThisTopic != null)
            {
                foreach (var endPoint in subscriberListForThisTopic)
                {
                    server.SendTo(Encoding.ASCII.GetBytes(message), messagelength, SocketFlags.None, endPoint);
                }
            }
        }

        private static string MakeCommaSeparatedString(List<string> eventParts)
        {
            var message = string.Empty;
            foreach (var item in eventParts)
            {
                message = message + item + ",";
            }
            if (message.Length != 0)
            {
                message = message.Remove(message.Length - 1, 1);
            }
            return message;
        }
    }

    internal class WorkerThreadParameters
    {
        public Socket Server { get; set; }

        public string Message { get; set; }

        public List<EndPoint> SubscriberListForThisTopic { get; set; }
    }
}
