using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Shared;

namespace PubSubServer
{
    internal class Filter
    {
        private static readonly Dictionary<string, List<SubscriberTuple>> _subscribersList =
            new Dictionary<string, List<SubscriberTuple>>();

        public static Dictionary<string, List<SubscriberTuple>> SubscribersList
        {
            get
            {
                lock (typeof (Filter))
                {
                    return _subscribersList;
                }
            }
        }

        public static List<SubscriberTuple> GetSubscribers(string topicName)
        {
            lock (typeof (Filter))
            {
                if (SubscribersList.ContainsKey(topicName))
                {
                    return SubscribersList[topicName];
                }
                return null;
            }
        }

        public static void AddSubscriber(string topicName, Guid subscriptionId, EndPoint subscriberEndPoint)
        {
            lock (typeof (Filter))
            {
                if (SubscribersList.ContainsKey(topicName))
                {
                    if (
                        !SubscribersList[topicName].Contains(
                            SubscribersList[topicName].Find(x => x.Endpoint == subscriberEndPoint)))
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

        public static void RemoveSubscriber(string topicName, Guid SubscriptionId)
        {
            lock (typeof (Filter))
            {
                if (!SubscribersList.ContainsKey(topicName)) return;

                if (
                    SubscribersList[topicName].Contains(
                        SubscribersList[topicName].First(x => x.SubscriptionId == SubscriptionId)))
                {
                    SubscribersList[topicName].Remove(
                        SubscribersList[topicName].First(x => x.SubscriptionId == SubscriptionId));
                }
            }
        }
    }
}
