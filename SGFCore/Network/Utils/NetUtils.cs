/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * NetUtils
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/


using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SGF.Network
{
    public class NetUtils
    {
        public static bool GetIsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public static int GetAvaliablePort()
        {
            int port = 0;
            try
            {
                var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Bind(new IPEndPoint(IPAddress.Any, 0));
                port = (s.LocalEndPoint as IPEndPoint).Port;
                s.Close();
            }
            catch (Exception e)
            {
            }

            return port;
        }
    }
}