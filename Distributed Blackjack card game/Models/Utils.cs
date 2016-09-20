using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Models
{
    public class Utils
    {
        public static IPAddress GetLocalIp4Address()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(ipa => ipa.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
