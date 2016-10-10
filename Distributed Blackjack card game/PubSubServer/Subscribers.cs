using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Shared;

namespace PubSubServer
{
    class Subscribers
    {
        static readonly Dictionary<string, List<SubscriberTuple>> _subscribersList = new Dictionary<string, List<SubscriberTuple>>();

        static public Dictionary<string, List<SubscriberTuple>> SubscribersList
        {
            get
            {
                lock (typeof(Subscribers))
                {
                    return _subscribersList;
                }
            }
        }

        static public List<SubscriberTuple> GetSubscribers(String topicName)
        {
            lock (typeof(Subscribers))
            {
                if (SubscribersList.ContainsKey(topicName))
                {
                    return SubscribersList[topicName];
                }
                else
                    return null;
            }
        }

        static public void AddSubscriber(string topicName, Guid subscriptionId, EndPoint subscriberEndPoint)
        {
            lock (typeof(Subscribers))
            {
                if (SubscribersList.ContainsKey(topicName))
                {
                    if (!SubscribersList[topicName].Contains(SubscribersList[topicName].Find(x => x.Endpoint == subscriberEndPoint)))
                    {
                        SubscribersList[topicName].Add(new SubscriberTuple(subscriberEndPoint, subscriptionId));
                    }
                }
                else
                {
                    var newSubscribersList = new List<SubscriberTuple>
                    {
                        new SubscriberTuple(subscriberEndPoint, subscriptionId)
                    };

                    SubscribersList.Add(topicName, newSubscribersList);
                }
            }

        }

        static public void RemoveSubscriber(String topicName, Guid SubscriptionId)
        {
            lock (typeof(Subscribers))
            {
                if (!SubscribersList.ContainsKey(topicName)) return;

                if (SubscribersList[topicName].Contains(SubscribersList[topicName].First(x => x.SubscriptionId == SubscriptionId)))
                {
                    SubscribersList[topicName].Remove(SubscribersList[topicName].First(x => x.SubscriptionId == SubscriptionId));
                }
            }
        }

    }
}
