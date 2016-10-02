using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Shared
{
    public class Utils
    {
        public static IPAddress GetLocalIp4Address()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(ipa => ipa.AddressFamily == AddressFamily.InterNetwork);
        }

        public const string TablePublishTopic = "Table 1";
        public const string TableSubscribeTopic = "Sub Table 1";
    }

    public static class Ext
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        ///  Fisher-Yates shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
