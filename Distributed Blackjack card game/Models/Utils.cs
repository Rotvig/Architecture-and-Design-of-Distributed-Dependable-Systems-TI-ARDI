﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Shared
{
    public static class Utils
    {
        public static IPAddress GetLocalIp4Address()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(ipa => ipa.AddressFamily == AddressFamily.InterNetwork);
        }

        private static readonly Random Rng = new Random();

        /// <summary>
        ///  Fisher-Yates shuffles and returns a Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static Queue<T> Shuffle<T>(this IList<T> list)
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

            var queue = new Queue<T>();
            foreach (var item in list)
            {
                queue.Enqueue(item);
            }
            return queue;
        }
    }
}
