
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SGF.Network
{
    public class IPUtils
    {
        public static bool IOS_IPv6_First = true;
        //===========================================================

        public static readonly IPAddress IP_Broadcast = new IPAddress(0xffffffffL);
        public static IPEndPoint IPEP_Any = new IPEndPoint(IPAddress.Any, 0);
        public static IPEndPoint IPEP_IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);

        #region 工具函数:IsBroadcast,IsFatalException,GetIPEndPointAny
        public static bool IsBroadcast(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return false;
            }

            return ip.Equals(IP_Broadcast);
        }

        public static bool IsFatalException(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            return (((exception is OutOfMemoryException) ||
                (exception is StackOverflowException)) ||
                (exception is ThreadAbortException));
        }

        public static IPEndPoint GetIPEndPointAny(AddressFamily family, int port)
        {
            if (family == AddressFamily.InterNetwork)
            {
                if (port == 0)
                {
                    return IPEP_Any;
                }

                return new IPEndPoint(IPAddress.Any, port);
            }
            else if (family == AddressFamily.InterNetworkV6)
            {
                if (port == 0)
                {
                    return IPEP_IPv6Any;
                }

                return new IPEndPoint(IPAddress.IPv6Any, port);
            }
            return null;
        }
        #endregion

        #region 工具函数:获取远程主机的地址
        public static IPEndPoint GetHostEndPoint(string host, int port)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(host, out address))
            {
#if TestIPv6
                Debuger.Log("UdpSocket.GetHostEndPoint() TestIPv6: 如果已经是IP了，则封装成IPv6格式");
                //已经是一个IP了。在外网下不应该出现IP的情况
                if (!address.IsIPv6LinkLocal && !address.IsIPv6SiteLocal)
                {
                    string tmp = "64:ff9b::" + address.ToString();
                    IPAddress addr_v6 = null;
                    if (IPAddress.TryParse(tmp, out addr_v6))
                    {
                        return new IPEndPoint(addr_v6, port);
                    }
                    else
                    {
                        Debuger.LogError("UdpSocket.GetHostEndPoint() TestIPv6，封装IPv6失败:" + tmp);
                    }
                }

#endif
                return new IPEndPoint(address, port);
            }
            else
            {
                //如果不是IP，就当作域名来处理
                IPAddress[] ips = Dns.GetHostAddresses(host);
                //如果是IPv6环境，则会返回IPv6的地址，否则会返回IPv4的地址。

                if (Debuger.EnableLog)
                {
                    Debuger.Log("UdpSocket.GetHostEndPoint() Dns GetHostAddresses:");
                    for (int i = 0; i < ips.Length; i++)
                    {
                        Debuger.Log("[" + i + "] " + ips[i] + ", " + ips[i].AddressFamily);
                    }
                }


                List<IPAddress> listIPv4 = new List<IPAddress>();
                List<IPAddress> listIPv6 = new List<IPAddress>();

                for (int i = 0; i < ips.Length; i++)
                {
                    if (ips[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        listIPv4.Add(ips[i]);
                    }
                    else
                    {
                        listIPv6.Add(ips[i]);
                    }
                }


#if UNITY_ANDROID

                if (listIPv4.Count > 0)
                {
                    return new IPEndPoint(listIPv4[0], port);
                }

                if (listIPv6.Count > 0)
                {
                    return new IPEndPoint(listIPv6[0], port);
                }

#else
                //加一个静态变量开关，方便以后用Lua设置
                if (IOS_IPv6_First)
                {
                    //IPv6优先
                    if (listIPv6.Count > 0)
                    {
                        return new IPEndPoint(listIPv6[0], port);
                    }

                    if (listIPv4.Count > 0)
                    {
                        return new IPEndPoint(listIPv4[0], port);
                    }
                }
                else
                {
                    //IPv4优先
                    if (listIPv4.Count > 0)
                    {
                        return new IPEndPoint(listIPv4[0], port);
                    }

                    if (listIPv6.Count > 0)
                    {
                        return new IPEndPoint(listIPv6[0], port);
                    }
                }

#endif

            }

            return null;
        }
        #endregion


        public static string SelfIP
        {
            get
            {
                string hostName = Dns.GetHostName();//本机名   
                //System.Net.IPAddress[] addressList = Dns.GetHostByName(hostName).AddressList;//会警告GetHostByName()已过期，我运行时且只返回了一个IPv4的地址   
                IPAddress[] addressList = Dns.GetHostAddresses("127.0.0.1");//会返回所有地址，包括IPv4和IPv6  

                return addressList[0].ToString();
            }
        }

        public static string SelfIP2
        {
            get
            {
                string hostname = Dns.GetHostName();//得到本机名   
                IPHostEntry localhost = Dns.GetHostEntry(hostname);
                IPAddress localaddr = localhost.AddressList[0];
                return localaddr.ToString();
            }

        }



        public static Int64 IPStr2Int(string ip)
        {
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            return Int64.Parse(items[0]) << 24
                   | Int64.Parse(items[1]) << 16
                   | Int64.Parse(items[2]) << 8
                   | Int64.Parse(items[3]);
        }

        public static string IPInt2Str(Int64 ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }
    }
}
