using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Shared
{
    public class Subscriber
    {
        public event EventHandler<NewMessageEvent> NewMessage;
        public Guid? SubscriptionId;
        private readonly Socket client;
        private readonly EndPoint remoteEndPoint;
        private byte[] data;
        private int recv;
        private bool isReceivingStarted;
        private const int Port = 10001;
        private string currentTopic;
        

        public Subscriber()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
        }

        public void Subscribe(string topic)
        {
            currentTopic = topic;
            SubscriptionId = Guid.NewGuid();

            if (string.IsNullOrEmpty(currentTopic))
            {
                throw new ArgumentException("Please Enter a Topic Name");
            }

            var message = JsonConvert.SerializeObject(new Message
            {
                Command = Command.Subscribe,
                SubscriptionId = SubscriptionId,
                Topic = currentTopic
            });

            client.SendTo(Encoding.ASCII.GetBytes(message), remoteEndPoint);

            if (isReceivingStarted) return;

            isReceivingStarted = true;
            data = new byte[1024];
            var thread1 = new Thread(ReceiveDataFromServer) {IsBackground = true};
            thread1.Start();
        }

        public void Unsubscribe()
        {
            if (string.IsNullOrEmpty(currentTopic))
            {
                throw new ArgumentException("Please Enter a Topic Name");
            }

            var message = JsonConvert.SerializeObject(new Message
            {
                Command = Command.Unsubscribe,
                SubscriptionId = SubscriptionId,
                Topic = currentTopic
            });

            client.SendTo(Encoding.ASCII.GetBytes(message), remoteEndPoint);
            SubscriptionId = null;
        }

        private void ReceiveDataFromServer()
        {
            var publisherEndPoint = client.LocalEndPoint;
            while (true)
            {
                recv = client.ReceiveFrom(data, ref publisherEndPoint);
                var msg = Encoding.ASCII.GetString(data, 0, recv);
                if (!string.IsNullOrEmpty(msg))
                {
                    NewMessage?.Invoke(this, new NewMessageEvent(JsonConvert.DeserializeObject<Message>(msg)));
                }
            }
        }

        public class NewMessageEvent : EventArgs
        {
            public Message Message { get; set; }

            public NewMessageEvent(Message message)
            {
                Message = message;
            }
        }
    }
}
