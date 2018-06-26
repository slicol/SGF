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


namespace SGF.Network.Core
{
    public class ProtocolHead
    {
        public const int Length = 16;
        public uint uid = 0;
        public uint cmd = 0;
        public uint index = 0;
        public ushort dataSize = 0;
        public ushort checksum = 0;

        public ProtocolHead Deserialize(NetBuffer buffer)
        {
            ProtocolHead head = this;
            head.uid = buffer.ReadUInt();
            head.cmd = buffer.ReadUInt();
            head.index = buffer.ReadUInt();
            head.dataSize = buffer.ReadUShort();
            head.checksum = buffer.ReadUShort();
            return head;
        }

        public NetBuffer Serialize(NetBuffer buffer)
        {
            buffer.WriteUInt(uid);
            buffer.WriteUInt(cmd);
            buffer.WriteUInt(index);
            buffer.WriteUShort(dataSize);
            buffer.WriteUShort(checksum);
            return buffer;
        }

    }
}