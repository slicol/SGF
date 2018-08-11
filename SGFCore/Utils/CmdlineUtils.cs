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

using SGF.Extension;

namespace SGF.Utils
{
    public class CmdlineUtils
    {
        public static string GetArgValue(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var token = args[i];
                if (token.ToLower() == name)
                {
                    if (i < args.Length - 1)
                    {
                        return args[i + 1];
                    }
                }
            }
            return "";
        }

        public static bool HasArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var token = args[i];
                if (token.ToLower() == name)
                {
                    return true;
                }
            }
            return false;
        }



        public static int GetArgInt(string[] args, string name)
        {
            return GetArgValue(args, name).ToInt();
        }

        public static float GetArgFloat(string[] args, string name)
        {
            return GetArgValue(args, name).ToFloat();
        }

        public static bool GetArgBool(string[] args, string name)
        {
            return GetArgValue(args, name) == "true";
        }


    }
}