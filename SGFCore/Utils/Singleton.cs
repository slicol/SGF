/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * Singleton
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
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T ms_instance = default(T);

        public static T Instance
        {
            get
            {
                if (ms_instance == null)
                {
                    ms_instance = new T();
                    ms_instance.InitSingleton();
                }
                return ms_instance;
            }
        }

        protected virtual void InitSingleton()
        {
            
        }
    }

    //**********************************************************************

}
