/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 帧同步模块
 * Frame synchronization module
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

namespace SGF.Network.FSPLite
{
    //由于以上有多种可能发GameEnd的情况，所以这里有一个GameEnd的原因定义
    public enum FSPGameEndReason
    {
        Normal = 0, //正常结束
        AllOtherExit = 1, //所有其他人都主动退出了
        AllOtherLost = 2,  //所有其他人都掉线了
    }
}
