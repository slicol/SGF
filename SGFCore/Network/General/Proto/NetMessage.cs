/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 封装了网络消息
 * Encapsulates network messages
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
    public class NetMessage
    {
        private static NetBuffer DefaultWriter = new NetBuffer(4096);
        private static NetBuffer DefaultReader = new NetBuffer(4096);

        public ProtocolHead head = new ProtocolHead();
        public byte[] content;

        public uint Length{get { return head.dataSize + ProtocolHead.Length; } }

        public bool Deserialize(NetBuffer buffer)
        {
            if (head.Deserialize(buffer))
            {
                if (buffer.BytesAvailable >= head.dataSize)
                {
                    content = new byte[head.dataSize];
                    buffer.ReadBytes(content, 0, (int)head.dataSize);
                    return true;
                }
            }
            return false;
        }

        public NetBuffer Serialize(NetBuffer buffer)
        {
            head.Serialize(buffer);
            buffer.WriteBytes(content, 0, (int)head.dataSize);
            return buffer;
        }

        public bool Deserialize(byte[] buffer, int size)
        {
            lock (DefaultReader)
            {
                DefaultReader.Attach(buffer, size);
                return Deserialize(DefaultReader);
            }
        }

        public int Serialize(out byte[] tempBuffer)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Clear();
                this.Serialize(DefaultWriter);
                tempBuffer = DefaultWriter.GetBytes();
                return DefaultWriter.Length;
            }

        }

    }
}