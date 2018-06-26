/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * 基于LCG的随机数生成算法
 * https://en.wikipedia.org/wiki/Linear_congruential_generator
 * https://stackoverflow.com/questions/4768180/rand-implementation
 * LCG-based random number generation algorithm
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


namespace SGF.MathLite
{
    public class SGFRandom
    {
        #region 默认实例
        public static SGFRandom Default = new SGFRandom();
        #endregion

        #region 线性同余参数

        //线性同余随机数生成算法
        private const int PrimeA = 214013;
        private const int PrimeB = 2531011;        

        #endregion

        //归一化
        private const float Mask15Bit_1 = 1.0f / 0x7fff;
        private const int Mask15Bit = 0x7fff;

        private int m_Value = 0;

        public int Seed
        {
            set { m_Value = value; }
            get { return m_Value; }
        }

        /// <summary>
        /// 采用线性同余算法产生一个0~1之间的随机小数
        /// </summary>
        /// <returns></returns>
        public float Rnd()
        {
            float val = ((((m_Value = m_Value * PrimeA + PrimeB) >> 16) & Mask15Bit) - 1) * Mask15Bit_1;
            return (val > 0.99999f ? 0.99999f : val);
        }

        public float Range(float min, float max)
        {
            return min + Rnd() * (max - min);
        }

        public int Range(int min, int max)
        {
            return (int)(min + Rnd() * (max - min));
        }


        public float Range(float max)
        {
            return Range(0, max);
        }

        public int Range(int max)
        {
            return Range(0, max);
        }



    }
}
