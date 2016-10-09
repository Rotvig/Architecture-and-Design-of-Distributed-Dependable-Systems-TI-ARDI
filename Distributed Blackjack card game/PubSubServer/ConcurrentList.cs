using System.Collections.Generic;
using System.Linq;

namespace PubSubServer
{
    public class ConcurrentList
    {
        private int index;
        private readonly List<MessageServiceItem> list = new List<MessageServiceItem>();
        private readonly object Lock = new object();

        public void AddItem(MessageServiceItem item)
        {
            lock (Lock)
            {
                list.Add(item);
            }
        }

        public bool TryGetNextItem(out MessageServiceItem item)
        {
            lock (Lock)
            {
                item = default(MessageServiceItem);
                if (!list.Any())
                    return false;
                    
                index++;
                index = index%list.Count;
                item = list[index];
                return true;
            }
        }

        public bool RemoveItem(int messageNumber)
        {
            lock (Lock)
            {
                return list.Remove(list.Single(x => x.Message.Header.MessageNumber == messageNumber));
            }
        }
    }
}