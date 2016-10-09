using System;
using System.Collections.Generic;
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
        private readonly Socket client;
        private readonly EndPoint remoteEndPoint;
        private byte[] data;
        private int recv;
        private bool isReceivingStarted;
        private const int Port = 10001;
        private string currentTopic;
        private List<int> messageNumbersRecieved = new List<int>(); 

        public Subscriber()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
        }

        public Guid SubscriptionId { get; private set; }

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
                Header = new MessageHeader
                {
                    Command = Command.Subscribe,
                    Topic = currentTopic
                },
                Content = new MessageContent
                {
                    SubscriptionId = SubscriptionId
                }
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
                Header = new MessageHeader
                {
                    Command = Command.Unsubscribe,
                    Topic = currentTopic
                },
                Content = new MessageContent
                {
                    SubscriptionId = SubscriptionId
                }
            });

            client.SendTo(Encoding.ASCII.GetBytes(message), remoteEndPoint);
        }

        private void ReceiveDataFromServer()
        {
            var publisherEndPoint = client.LocalEndPoint;
            while (true)
            {
                recv = client.ReceiveFrom(data, ref publisherEndPoint);
                var msg = Encoding.ASCII.GetString(data, 0, recv);
                if (string.IsNullOrEmpty(msg)) continue;

                var message = JsonConvert.DeserializeObject<Message>(msg);
                if (messageNumbersRecieved.Contains(message.Header.MessageNumber)) continue;

                ReturnAck(message, publisherEndPoint);
                messageNumbersRecieved.Add(message.Header.MessageNumber);

                NewMessage?.Invoke(this, new NewMessageEvent(message.Content));
            }
        }

        private void ReturnAck(Message message, EndPoint endPoint)
        {
            message.Header.Command = Command.Ack;
            var serializedMessage = JsonConvert.SerializeObject(message);
            client.SendTo((Encoding.ASCII.GetBytes(serializedMessage)), serializedMessage.Length ,SocketFlags.None, endPoint);
        }

        public class NewMessageEvent : EventArgs
        {
            public MessageContent Message { get; set; }

            public NewMessageEvent(MessageContent message)
            {
                Message = message;
            }
        }
    }
}
