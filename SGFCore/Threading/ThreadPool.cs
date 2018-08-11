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


using System;
using System.Collections.Generic;
using System.Threading;

namespace SGF.Threading
{
    public class ThreadPool
    {
        class ThreadTaskState
        {
            public object[] args;
            public Delegate func;
        }

        public static int maxThreads = 8;
        private static int numThreads = 0;
        private static List<ThreadTaskState> ms_listFuncOnMainThread = new List<ThreadTaskState>();
        private static List<ThreadTaskState> ms_currentInvokingFuncs = new List<ThreadTaskState>();
        private static Delegate ms_funcThreadWorker;

        public static void QueueOnMainThread(Delegate func, params object[] args)
        {
            lock (ms_listFuncOnMainThread)
            {
                var state = new ThreadTaskState() { args = args, func = func };
                ms_listFuncOnMainThread.Add(state);
            }
        }

        public static Thread QueueUserWorkItem(Delegate func, params object[] args)
        {
            if (ms_funcThreadWorker == null)
            {
                ms_funcThreadWorker = (WaitCallback)ThreadWorker;
            }

            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }

            Interlocked.Increment(ref numThreads);
            try
            {
                //如果线程不够了，就直接执行
                var state = new ThreadTaskState() { args = args, func = func };
                System.Threading.ThreadPool.QueueUserWorkItem((WaitCallback)(ms_funcThreadWorker), state);

            }
            catch (Exception e)
            {
                ((Action<object[]>)func)(args);
                Interlocked.Decrement(ref numThreads);
            }
            finally
            {

            }

            return null;
        }

        private static void ThreadWorker(object obj)
        {
            try
            {
                ThreadTaskState state = obj as ThreadTaskState;

                ((Action<object[]>)state.func)(state.args);
            }
            catch
            {
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }

        }

        public static void TickOnMainThread()
        {
            lock (ms_listFuncOnMainThread)
            {
                ms_currentInvokingFuncs.Clear();
                ms_currentInvokingFuncs.AddRange(ms_listFuncOnMainThread);
                ms_listFuncOnMainThread.Clear();
            }
            foreach (var state in ms_currentInvokingFuncs)
            {
                try
                {
                    ((Action<object[]>)state.func)(state.args);
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message);
                }
            }
        }
    }
}