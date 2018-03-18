////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 作者：slicol
*/

using System;



namespace SGF.Codec
{
    public class SGFEncoding
    {
        public static char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        public static String BytesToHex(byte[] bytes, int size = 0)
        {
            
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            if (size <= 0 || size > bytes.Length)
            {
                size = bytes.Length;
            }

            char[] buf = new char[2 * size];
            for (int i = 0; i < size; i++)
            {
                byte b = bytes[i];
                buf[2 * i + 1] = digits[b & 0xF];
                b = (byte)(b >> 4);
                buf[2 * i + 0] = digits[b & 0xF];
            }
            return new String(buf);
        }

        public static byte[] HexToBytes(String s)
        {

            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {

                data[i / 2] = (byte)((CharToValue(s[i]) << 4) + (CharToValue(s[i + 1])));
            }
            return data;
        }

        public static string Number2BinString(long value, int bitCnt)
        {
            return Number2BinString((ulong) value, bitCnt);
        }

        public static string Number2BinString(ulong value, int bitCnt)
        {
            string str = "";
            for (int i = 0; i < bitCnt; i++)
            {
                if ((value & (ulong) (1 << i)) != 0)
                {
                    str = "1" + str;
                }
                else
                {
                    str = "0" + str;
                }
            }
            return str;
        }

        private static byte CharToValue(char ch)
        {
            if (ch >= '0' && ch <= '9')
            {
                return (byte)(ch - '0');
            }
            else if (ch >= 'a' && ch <= 'f')
            {
                return (byte)(ch - 'a' + 10);
            }
            else if (ch >= 'A' && ch <= 'F')
            {
                return (byte)(ch - 'A' + 10);
            }

            return 0;
        }


        public static int XORCodec(byte[] buffer, int begin, int len,  byte[] key)
        {
            if (buffer == null || key == null || key.Length == 0)
            {
                return -1;
            }

            if (begin + len >= buffer.Length)
            {
                return -1;
            }

            int blockSize = key.Length;
            int j = 0;
            for (j = begin; j < begin + len; j++)
            {
                buffer[j] = (byte)(buffer[j] ^ key[(j - begin) % blockSize]);
            }

            return j;
        }

        public static int XORCodec(byte[] inBytes, byte[] outBytes, byte[] keyBytes)
        {
            if (inBytes == null || outBytes == null || keyBytes == null || keyBytes.Length == 0)
            {
                return -1;
            }

            if (outBytes.Length < inBytes.Length)
            {
                return -1;
            }

            int blockSize = keyBytes.Length;
            int j = 0;
            for (j = 0; j < inBytes.Length; j++)
            {
                outBytes[j] = (byte)(inBytes[j] ^ keyBytes[j % blockSize]);
            }

            return j;
        }



        public static ushort CheckSum(byte[] buffer, int size)
        {
            ulong sum = 0;
            int i = 0;
            while (size > 1)
            {
                sum = sum + BitConverter.ToUInt16(buffer, i);
                size -= 2;
                i += 2;
            }
            if (size > 0)
            {
                sum += buffer[i];
            }

            while ((sum >> 16) != 0)
            {
                sum = (sum >> 16) + (sum & 0xffff);
            }

            return (ushort)(~sum);
        }

    }
}
