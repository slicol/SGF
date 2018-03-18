using System.Net;
using System.Net.NetworkInformation;

namespace SGF.Network
{
    public class NetUtils
    {
        public static bool GetIsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        
    }
}