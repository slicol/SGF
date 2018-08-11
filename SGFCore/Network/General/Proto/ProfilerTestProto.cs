/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
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

using ProtoBuf;

namespace SGF.Network.General.Proto
{
    internal class ProfilerTestProto
    {
        public class ProtoCmd
        {
            public static uint LoginReq = 1;
            public static uint LoginRsp = 2;
            public static uint HeartBeatReq = 3;
            public static uint HeartBeatRsp = 4;
        }


        [ProtoContract]
        public class LoginReq
        {
            [ProtoMember(1)]
            public uint id;
            [ProtoMember(2)]
            public string name;
        }

        [ProtoContract]
        public class LoginRsp
        {
            [ProtoMember(1)]
            public int ret;
            [ProtoMember(2)]
            public string name;
        }

        [ProtoContract]
        public class HeartBeatReq
        {
            [ProtoMember(1)]
            public ushort ping;
            [ProtoMember(2)]
            public uint timestamp;
        }

        [ProtoContract]
        public class HeartBeatRsp
        {
            [ProtoMember(1)]
            public int ret;
            [ProtoMember(2)]
            public uint timestamp;
        }
    }
    

}