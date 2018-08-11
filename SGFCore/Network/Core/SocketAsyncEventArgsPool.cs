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


using System.Collections.Generic;
using System.Net.Sockets;


namespace SGF.Network.Core
{
    public class SocketAsyncEventArgsPool
    {
        private readonly int m_poolSize;
        private readonly int m_packedBufferSize;
        private readonly Stack<SocketAsyncEventArgs> m_pool = 
            new Stack<SocketAsyncEventArgs>();
        
        public SocketAsyncEventArgsPool(int packetBufferSize, int poolSize)
        {
            this.m_packedBufferSize = packetBufferSize;
            this.m_poolSize = poolSize;
        }

        public SocketAsyncEventArgs Acquire()
        {
            SocketAsyncEventArgs e = null;
            lock (m_pool)
            {
                if (m_pool.Count > 0)
                {
                    return m_pool.Pop();
                }
            }

            e = new SocketAsyncEventArgs();
            e.SetBuffer(new byte[this.m_packedBufferSize], 0, this.m_packedBufferSize);
            return e;
        }

        public void Release(SocketAsyncEventArgs e)
        {
            lock (m_pool)
            {
                if (this.m_pool.Count < m_poolSize)
                {
                    this.m_pool.Push(e);
                    return;
                }

            }

            e.Dispose();
        }

        public void Clear()
        {
            lock (m_pool)
            {
                SocketAsyncEventArgs e = null;
                while (m_pool.Count > 0)
                {
                    e = m_pool.Pop();
                    e.Dispose();
                }
            }
        }
    }
}