using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using SGF.Codec;

namespace SGF.Network.Core.RPCLite
{

    [ProtoContract]
    public class RPCMessage
    {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public List<RPCRawArg> raw_args = new List<RPCRawArg>();

        public object[] args
        {
            get
            {
                var list = new List<object>();
                for (int i = 0; i < raw_args.Count; i++)
                {
                    list.Add(raw_args[i].value);
                }
                return list.ToArray();
            }

            set
            {
                raw_args = new List<RPCRawArg>();
                object[] list = value;
                for (int i = 0; i < list.Length; i++)
                {
                    var raw_arg = new RPCRawArg();
                    raw_arg.value = list[i];
                    raw_args.Add(raw_arg);
                }
            }
        }


    }


    [ProtoContract]
    public class RPCRawArg
    {
        [ProtoMember(1)]
        public RPCArgType type;
        [ProtoMember(2)]
        public byte[] raw_value;

        public object value
        {
            get
            {
                if (raw_value == null || raw_value.Length == 0)
                {
                    return null;
                }

                NetBufferReader reader = new NetBufferReader(raw_value);
                switch (type)
                {
                    case RPCArgType.Int: return reader.ReadInt();
                    case RPCArgType.UInt: return reader.ReadUInt();
                    case RPCArgType.Long: return reader.ReadLong();
                    case RPCArgType.ULong: return reader.ReadULong();
                    case RPCArgType.Short: return reader.ReadShort();
                    case RPCArgType.UShort: return reader.ReadUShort();
                    case RPCArgType.Double: return reader.ReadDouble();
                    case RPCArgType.Float: return reader.ReadFloat();
                    case RPCArgType.String: return Encoding.UTF8.GetString(raw_value);
                    case RPCArgType.Byte: return reader.ReadByte();
                    case RPCArgType.Bool: return reader.ReadByte() != 0;
                    case RPCArgType.ByteArray: return raw_value;
                    case RPCArgType.PBObject: return raw_value;//由于数据层是不知道具体类型，由反射层去反序列化
                    default: return raw_value;
                }

            }
            set
            {
                NetBuffer writer;
                object v = value;
                if (v is int)
                {
                    type = RPCArgType.Int;
                    raw_value = BitConverter.GetBytes((int) v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is uint)
                {
                    type = RPCArgType.UInt;
                    raw_value = BitConverter.GetBytes((uint)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is long)
                {
                    type = RPCArgType.Long;
                    raw_value = BitConverter.GetBytes((long)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is ulong)
                {
                    type = RPCArgType.ULong;
                    raw_value = BitConverter.GetBytes((ulong)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is short)
                {
                    type = RPCArgType.Short;
                    raw_value = BitConverter.GetBytes((short)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is ushort)
                {
                    type = RPCArgType.UShort;
                    raw_value = BitConverter.GetBytes((ushort)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is double)
                {
                    type = RPCArgType.Double;
                    raw_value = BitConverter.GetBytes((double)v);
                }
                else if (v is float)
                {
                    type = RPCArgType.Float;
                    raw_value = BitConverter.GetBytes((float)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is string)
                {
                    type = RPCArgType.String;
                    raw_value = Encoding.UTF8.GetBytes((string)v);
                }
                else if (v is byte)
                {
                    type = RPCArgType.Byte;
                    raw_value = new[] { (byte)v };
                }
                else if (v is bool)
                {
                    type = RPCArgType.Bool;
                    raw_value = new[] { (bool)v ? (byte)1 : (byte)0 };
                }
                else if (v is byte[])
                {
                    type = RPCArgType.ByteArray;
                    raw_value = new byte[((byte[])v).Length];
                    Buffer.BlockCopy((byte[])v, 0, raw_value, 0, raw_value.Length);
                }
                else
                {
                    var bytes = PBSerializer.NSerialize(v);
                    if (bytes != null)
                    {
                        type = RPCArgType.PBObject;
                        raw_value = new byte[bytes.Length];
                        Buffer.BlockCopy(bytes, 0, raw_value, 0, raw_value.Length);
                    }
                    else
                    {
                        type = RPCArgType.Unkown;
                        Debuger.LogError("该参数无法序列化！value:{0}", v);
                    }
                }
            }
        }
    }


    public enum RPCArgType
    {
        Unkown = 0,
        Int = 1,
        UInt = 2,
        Long = 3,
        ULong = 4,
        Short = 5,
        UShort = 6,
        Double = 8,
        Float = 9,
        String = 10,
        Byte = 11,
        Bool = 12,
        ByteArray = 31,
        PBObject = 32
    }
}