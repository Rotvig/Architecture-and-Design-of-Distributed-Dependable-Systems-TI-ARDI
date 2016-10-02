using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Shared;
using Enumerable = System.Linq.Enumerable;

namespace PubSubServer
{
    class Filter
    {
        static readonly Dictionary<string, List<SubscriberTuple>> _subscribersList = new Dictionary<string, List<SubscriberTuple>>();

        static public Dictionary<string, List<SubscriberTuple>> SubscribersList
        {
            get
            {
                lock (typeof(Filter))
                {
                    return _subscribersList;
                }
            }

        }

        static public List<SubscriberTuple> GetSubscribers(String topicName)
        {
            lock (typeof(Filter))
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
            lock (typeof(Filter))
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

        static public void RemoveSubscriber(String topicName, Guid subscriptionId, EndPoint subscriberEndPoint)
        {
            lock (typeof(Filter))
            {
                if (!SubscribersList.ContainsKey(topicName)) return;

                if (SubscribersList[topicName].Contains(SubscribersList[topicName].First(x => x.Endpoint == subscriberEndPoint)))
                {
                    SubscribersList[topicName].Remove(SubscribersList[topicName].First(x => x.Endpoint == subscriberEndPoint));
                }
            }
        }

    }
}
