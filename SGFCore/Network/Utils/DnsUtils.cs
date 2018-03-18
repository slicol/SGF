////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 作者：slicol
*/
using System;
using System.Collections.Generic;
using System.Net;
using SGF.Utils;

namespace SGF.Network
{
    public class DnsUtils
    {
        public const string TAG = "DnsUtils";

        public static string[] GetHostByName(string hostNameOrAddress)
        {
            IPAddress ipaddr = null;
            if (IPAddress.TryParse(hostNameOrAddress, out ipaddr))
            {
                return new string[1] {ipaddr.ToString()};
            }



            IPAddress[] ipAddresses = null;

            try
            {
                ipAddresses = Dns.GetHostAddresses(hostNameOrAddress);
            }
            catch (Exception)
            {

            }

            if (ipAddresses != null && ipAddresses.Length > 0)
            {
                string[] ipstrs = new string[ipAddresses.Length];
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    ipstrs[i] = ipAddresses[i].ToString();
                }
                return ipstrs;
            }

            return new string[0];
        }


        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            string[] ipstrs = GetHostByName(hostNameOrAddress);
            if (ipstrs == null || ipstrs.Length == 0)
            {
                return new IPAddress[0];
            }
            List<IPAddress> listIPAddrs = new List<IPAddress>();
            for (int i = 0; i < ipstrs.Length; i++)
            {
                IPAddress ipAddress = null;
                if (IPAddress.TryParse(ipstrs[i], out ipAddress))
                {
                    listIPAddrs.Add(ipAddress);
                }
            }
            return listIPAddrs.ToArray();
        }



        public static string[] GetUrlWithIP(string url)
        {
            Debuger.Log(TAG, "GetUrlWithIP() old url=" + url);

            string head, hostname, port, path;
            UrlUtils.SplitUrl(url, out head, out hostname, out port, out path);


            string[] ipstrs = GetHostByName(hostname);
            if (ipstrs == null || ipstrs.Length == 0)
            {
                Debuger.Log(TAG, "GetUrlWithIP() domain cannot be parse! new url=" + url);
                return new string[1] {url};
            }

            Debuger.Log(TAG, "GetUrlWithIP() Can Parse Total " + ipstrs.Length + " url!");
            string[] urls = new string[ipstrs.Length];
            for (int i = 0; i < ipstrs.Length; i++)
            {
                if (string.IsNullOrEmpty(port))
                {
                    urls[i] = head + ipstrs[i] + path;
                }
                else
                {
                    urls[i] = head + ipstrs[i] + ":" + port + path;
                }

                Debuger.Log(TAG, "GetUrlWithIP() new url[" + (i + 1) + "]=" + urls[i]);
            }

            return urls;
        }




    }
}