/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 封装了协议头
 * Encapsulates protocol head
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

using SGF.Network.Core;

namespace SGF.Network.General.Proto
{
    public class ProtocolHead
    {
        public const int Length = 44;
        public uint sid = 0;
        public uint token = 0;
        public uint cmd = 0;
        public uint index = 0;
        public ulong player = 0;
        public int zone = 0;
        public ulong runner = 0; 
        public uint dataSize = 0;
        public uint checksum = 0;

        public override string ToString()
        {
            return string.Format("sid:{0}, token:{1}, cmd:{2}, index:{3}, player:{4}, zone:{5}, runner:{6}, dataSize:{7}, checksum:{8}", sid, token, cmd,
                index, player,zone,runner, dataSize, checksum);
        }

        public bool Deserialize(NetBuffer buffer)
        {
            if (buffer.BytesAvailable >= Length)
            {
                ProtocolHead head = this;
                head.sid = buffer.ReadUInt();
                head.token = buffer.ReadUInt();
                head.cmd = buffer.ReadUInt();
                head.index = buffer.ReadUInt();
                head.player = buffer.ReadULong();
                head.zone = buffer.ReadInt();
                head.runner = buffer.ReadULong();
                head.dataSize = buffer.ReadUInt();
                head.checksum = buffer.ReadUInt();
                return true;
            }

            return false;
        }

        public NetBuffer Serialize(NetBuffer buffer)
        {
            buffer.WriteUInt(sid);
            buffer.WriteUInt(token);
            buffer.WriteUInt(cmd);
            buffer.WriteUInt(index);
            buffer.WriteULong(player);
            buffer.WriteInt(zone);
            buffer.WriteULong(runner);
            buffer.WriteUInt(dataSize);
            buffer.WriteUInt(checksum);
            return buffer;
        }



    }
}