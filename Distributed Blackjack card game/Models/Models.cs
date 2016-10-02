using System;
using System.Net;

namespace Shared
{
    public class SubscriberTuple : Tuple<EndPoint, Guid>
    {
        public SubscriberTuple(EndPoint endpoint, Guid subscriptionId)
            : base(endpoint, subscriptionId)
        {

        }

        public EndPoint Endpoint => Item1;
        public Guid SubscriptionId => Item2;
    }

    public class Message
    {
        public Command Command { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string Topic { get; set; }
        public string EventData { get; set; }
    }

    public enum Command
    {
        Publish,
        Subscribe,
        Unsubscribe
    }
}
