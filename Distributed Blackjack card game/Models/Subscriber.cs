using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

        public Subscriber()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(Utils.GetLocalIp4Address(), Port);
        }

        public void Subscribe(string topic)
        {
            currentTopic = topic;

            if (string.IsNullOrEmpty(currentTopic))
            {
                throw new ArgumentException("Please Enter a Topic Name");
            }

            var Command = "Subscribe";

            var message = Command + "," + currentTopic;
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
            var command = "UnSubscribe";

            var message = command + "," + currentTopic;
            client.SendTo(Encoding.ASCII.GetBytes(message), remoteEndPoint);
        }

        private void ReceiveDataFromServer()
        {
            var publisherEndPoint = client.LocalEndPoint;
            while (true)
            {
                recv = client.ReceiveFrom(data, ref publisherEndPoint);
                var msg = Encoding.ASCII.GetString(data, 0, recv) + "," + publisherEndPoint;
                if (!string.IsNullOrEmpty(msg))
                {
                    NewMessage?.Invoke(this, new NewMessageEvent(msg));
                }
            }
        }

        public class NewMessageEvent : EventArgs
        {
            public string Message { get; set; }

            public NewMessageEvent(string message)
            {
                Message = message;
            }
        }
    }
}
