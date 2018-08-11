/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 提供网络相关的调试功能
 * Provides network-related debugging features
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
using SGF.Codec;

using SGF.Network.protocol;
using SGF.Utils;


namespace SGF.Network
{
    public class NetDebugFile
    {
        private NetDebugFileData m_content;
        public NetDebugFileData Content { get { return m_content; } }


        private DictionarySafe<string, List<NetSampleItem>> m_mapAllSampleList;

        private List<string> m_listAllSampleName;
        public List<string> ListAllSampleName { get { return m_listAllSampleName; } }

        public NetDebugFile()
        {
            m_content = new NetDebugFileData();
            m_mapAllSampleList = new DictionarySafe<string, List<NetSampleItem>>();
            m_listAllSampleName = new List<string>();
        }

        public List<NetSampleItem> GetSampleList(string name)
        {
            return m_mapAllSampleList[name];
        }



        public bool Save(string dirname, string filename)
        {
            Debuger.Log("");
            
            if (string.IsNullOrEmpty(filename))
            {
                return false;
            }

            string errinfo = "";
            byte[] buffer = PBSerializer.NSerialize(m_content);
            return FileUtils.SaveFile(dirname + filename, buffer) > 0;
        }

        public bool Open(string dirname, string filename)
        {
            string fullpath = filename;
            if (!PathUtils.IsFullPath(filename))
            {
                fullpath = dirname + "/" + filename;
            }

            return Open(fullpath);
        }


        public bool Open(string fullpath)
        {
            Debuger.Log("fullpath = " + fullpath);

            Byte[] bytes = FileUtils.ReadFile(fullpath);
            if (bytes != null && bytes.Length > 0)
            {
                m_content = PBSerializer.NDeserialize<NetDebugFileData>(bytes);
                return ParserFile();
            }
            else
            {
                Debuger.LogError("File Is Not Exist, Or Open Wrong!");
                return false;
            }

            return true;
        }



        private bool ParserFile()
        {
            m_mapAllSampleList.Clear();
            m_listAllSampleName.Clear();

            if (m_content == null)
            {
                return false;
            }

            List<NetSampleItem> listAll = m_content.profiler_samples;
            for (int i = 0; i < listAll.Count; i++)
            {
                NetSampleItem item = listAll[i];
                List<NetSampleItem> list = m_mapAllSampleList[item.name];
                if (list == null)
                {
                    list = new List<NetSampleItem>();
                    m_mapAllSampleList.Add(item.name, list);
                    m_listAllSampleName.Add(item.name);
                }
                list.Add(item);
            }

            return true;
        }


    }
}
