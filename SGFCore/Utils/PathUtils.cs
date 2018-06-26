/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * PathUtils
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



namespace SGF.Utils
{
    public class PathUtils
    {
        public static readonly string[] PathHeadDefine = { "jar://", "jar:file:///", "file:///", "http://", "https://" };

        public static bool IsSureDir(string path)
        {
            int i = path.LastIndexOf("/");
            if (i >= 0)
            {
                return true;
            }
            i = path.LastIndexOf("\\");
            if (i >= 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsFullPath(string path)
        {
            int i = path.IndexOf(":/");
            if (i >= 0)
            {
                return true;
            }

            i = path.IndexOf(":\\");
            if (i >= 0)
            {
                return true;
            }

            return false;
        }


        public static string GetFileName(string path)
        {
            string parent = "", child = "";
            SplitPath(path, ref parent, ref child, true);
            return child;
        }

        public static string GetParentDir(string path)
        {
            string parent = "", child = "";
            SplitPath(path, ref parent, ref child, true);
            return parent;
        }

        public static string SplitPath(string path, ref string parent, ref string child, bool bSplitExt = false)
        {
            string ext = "";
            string head = SplitPath(path, ref parent, ref child, ref ext);
            if (bSplitExt)
            {
                return head;
            }
            if (!string.IsNullOrEmpty(ext))
            {
                child = child + "." + ext;
            }
            return head;
        }

        public static string SplitPath(string path, ref string parent, ref string child, ref string ext)
        {
            string head = GetPathHead(path);

            int index = path.LastIndexOf("/");
            int index2 = path.LastIndexOf("\\");
            index = System.Math.Max(index, index2);

            if (index == head.Length - 1)
            {
                parent = "";
                child = path;
            }
            else
            {
                parent = path.Substring(0, index);
                child = path.Substring(index + 1);
            }

            index = child.LastIndexOf(".");
            if (index >= 0)
            {
                ext = child.Substring(index + 1);
                child = child.Substring(0, index);

            }

            return head;
        }

        public static string GetPathHead(string path)
        {
            for (int i = 0; i < PathHeadDefine.Length; i++)
            {
                if (path.StartsWith(PathHeadDefine[i]))
                {
                    return PathHeadDefine[i];
                }
            }

            return "";
        }






    }



}
