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
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using SGF.Codec;
using SGF.Extension;

namespace SGF.Network.FSPLite
{
    //==========================================================
    #region FSP启动参数定义

    [ProtoContract]
    public class FSPParam
    {
        [ProtoMember(1)]
        public string host;
        [ProtoMember(2)]
        public int port;
        [ProtoMember(3)]
        public uint sid;
        [ProtoMember(4)]
        public int serverFrameInterval = 66;
        [ProtoMember(5)]
        public int serverTimeout = 15000;//ms
        [ProtoMember(6)]
        public int clientFrameRateMultiple = 2;
        [ProtoMember(7)]
        public int authId = 0;
        [ProtoMember(8)]
        public bool useLocal = false;
        [ProtoMember(9)]
        public int maxFrameId = 1800;

        [ProtoMember(10)]
        public bool enableSpeedUp = true;
        [ProtoMember(11)]
        public int defaultSpeed = 1;
        [ProtoMember(12)]
        public int jitterBufferSize = 0;//缓冲大小
        [ProtoMember(13)]
        public bool enableAutoBuffer = true;






        public FSPParam Clone()
        {
            byte[] buffer = PBSerializer.NSerialize(this);
            return (FSPParam)PBSerializer.NDeserialize(buffer, typeof(FSPParam));
        }

        public string ToString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\n{0}host:{1}:{2}", prefix, host, port);
            sb.AppendFormat("\n{0}serverFrameInterval:{1}", prefix, serverFrameInterval);
            sb.AppendFormat("\n{0}clientFrameRateMultiple:{1}", prefix, clientFrameRateMultiple);
            sb.AppendFormat("\n{0}serverTimeout:{1}", prefix, serverTimeout);
            sb.AppendFormat("\n{0}maxFrameId:{1}", prefix, maxFrameId);

            return sb.ToString();
        }
    }
	#endregion

    [ProtoContract]
    public class FSPMessage
    {
        [ProtoMember(1)] public int cmd;
        [ProtoMember(2)] public int[] args;
        [ProtoMember(3)] public int custom;

        public uint playerId
        {
            get { return (uint)custom; }
            set { custom = (int)value; }
        }

        public int clientFrameId
        {
            get { return custom; }
            set { custom = value; }
        }

        public override string ToString()
        {
            if (args != null)
            {
                return string.Format("cmd:{0}, args:{1}, custom:{2}", cmd, args.ToListString(), custom);
            }
            else
            {
                return string.Format("cmd:{0}, args:{1}, custom:{2}", cmd, "[]", custom);
            }
        }
        
    }


    [ProtoContract]
    public class FSPDataC2S
    {
        [ProtoMember(1)] public uint sid = 0;
        [ProtoMember(2)] public List<FSPMessage> msgs = new List<FSPMessage>();
    }

    [ProtoContract]
    public class FSPDataS2C
    {
        [ProtoMember(1)] public List<FSPFrame> frames = new List<FSPFrame>();
    }

    [ProtoContract]
    public class FSPFrame
    {
        [ProtoMember(1)] public int frameId;
        [ProtoMember(2)] public List<FSPMessage> msgs = new List<FSPMessage>();


        public bool IsEmpty()
        {
            return (msgs == null || msgs.Count == 0);
        }


        public bool Contains(int cmd)
        {
            if (!IsEmpty())
            {
                for (int i = 0; i < msgs.Count; i++)
                {
                    if (msgs[i].cmd == cmd)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override string ToString()
        {
            string tmp = "";
            for (int i = 0; i < msgs.Count - 1; i++)
            {
                tmp += msgs[i] + ",";
            }

            if (msgs.Count > 0)
            {
                tmp += msgs[msgs.Count - 1].ToString();
            }

            return string.Format("frameId:{0}, msgs:[{1}]", frameId, tmp);
        }

    }







}