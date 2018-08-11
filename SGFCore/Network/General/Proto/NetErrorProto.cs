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
    [ProtoContract]
    public class NetErrorMessage
    {
        [ProtoMember(1)]
        public int code;
        [ProtoMember(2)]
        public string info;
    }

    [ProtoContract]
    public class ReturnCode
    {
        public static ReturnCode Success = new ReturnCode();
        public static ReturnCode UnkownError = new ReturnCode(1, "UnkownError");

        public ReturnCode(int code, string info)
        {
            this.code = code;
            this.info = info;
        }

        public ReturnCode()
        {
            this.code = 0;
            this.info = "";
        }



        [ProtoMember(1)]
        public int code = 0;
        [ProtoMember(2)]
        public string info = "";

        public override string ToString()
        {
            return string.Format("ErrCode:{0}, Info:{1}", code, info);
        }
    }
}