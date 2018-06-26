/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
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


namespace SGF.Unity.UI
{
    public enum UITypeDef
    {
        Unkown = 0,
        Page = 1,
        Window=2,
        Widget = 3,
        Loading =4
    }


    public class UILayerDef
    {
        public const int Background = 0;
        public const int Page = 1000;//-1999
        public const int NormalWindow = 2000;//-2999
        public const int TopWindow = 3000;//-3999
        public const int Widget = 4000;//-4999
        public const int Loading = 5000;
        public const int Unkown = 9999;

        public static int GetDefaultLayer(UITypeDef type)
        {
            switch (type)
            {
                case UITypeDef.Loading: return Loading;
                case UITypeDef.Widget: return Widget;
                case UITypeDef.Window: return NormalWindow;
                case UITypeDef.Page: return Page;
                case UITypeDef.Unkown: return Unkown;
                default: return Unkown;
            }
        }

    }


}