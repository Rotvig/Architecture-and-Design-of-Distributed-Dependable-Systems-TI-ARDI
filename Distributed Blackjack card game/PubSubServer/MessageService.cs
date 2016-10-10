using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Shared;

namespace PubSubServer
{
    internal class MessageService
    {
        private Socket socket;
        private const int Port = 10003;
        private int messageNumber = 0;
        private readonly ConcurrentList MessageList;

        public MessageService()
        {
            MessageList = new ConcurrentList();

            var consumerThread = new Thread(MessageQeueConsumer) { IsBackground = true };
            var listenerThread = new Thread(HostListener) { IsBackground = true };
            listenerThread.Start();
            consumerThread.Start();
        }


        public void AddItemToList(Message message, EndPoint endpoint)
        {
            messageNumber ++;
            message.Header.MessageNumber = messageNumber;
            MessageList.AddItem(new MessageServiceItem
            {
                Message = message,
                EndPoint = endpoint
            });
        }

        private void HostListener()
        {
            var localEp = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEp);

            StartListening();
        }

        private void StartListening()
        {
            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    var recv = 0;
                    var data = new byte[1024];
                    recv = socket.ReceiveFrom(data, ref remoteEp);
                    var serializedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    var deserializedMessage = JsonConvert.DeserializeObject<Message>(serializedMessage);

                    if (deserializedMessage.Header.Command != Command.Ack) continue;

                    MessageList.RemoveItem(deserializedMessage.Header.MessageNumber);
                }
                catch
                {
                    // ignored
                }
            }
        }


        private void MessageQeueConsumer()
        {
            while (true)
            {
                MessageServiceItem item;
                if (!MessageList.TryGetNextItem(out item)) continue;

                if (item.Message.Header.Timeout.HasValue && item.Message.Header.Timeout <= DateTime.Now)
                {
                    if (item.Message.Header.PublishToSubscriptionId && item.Message.Content.SubscriptionId.HasValue)
                    {
                        MessageList.RemoveItem(item.Message.Header.MessageNumber);
                        Subscribers.RemoveSubscriber(item.Message.Header.Topic, item.Message.Content.SubscriptionId.Value);
                    }
                    continue;
                }

                TryPublish(item);
            }
        }


        private void TryPublish(MessageServiceItem item)
        {
            var serializeObject = JsonConvert.SerializeObject(item.Message);
            socket.SendTo(Encoding.ASCII.GetBytes(serializeObject), serializeObject.Length, SocketFlags.None, item.EndPoint);
            item.Message.Header.PublishTries ++;
        }
    }

    public class MessageServiceItem
    {
        public EndPoint EndPoint { get; set; }
        public Message Message { get; set; } 
    }
}
