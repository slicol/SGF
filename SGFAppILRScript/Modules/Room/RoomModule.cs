using System;
using System.Collections.Generic;
using SGF;
using SGF.Extension;
using SGF.Module;
using SGF.Network.Core;
using SGF.Network.Core.RPCLite;
using SGF.Network.FSPLite;
using SGF.Utils;
using SGFAppDemo.Common.Data;
using SGFAppDemo.Services;

namespace SGFAppDemo.Modules
{
    public class RoomModule : GeneralModule
    {
        private List<RoomData> m_listRoom = null;
        private RoomData m_currRoom = new RoomData();

        protected override void Show(object arg)
        {
            base.Show(arg);
            Debuger.Log("显示房间界面!(模拟一下)");
            Debuger.Log("显示登录界面!(模拟一下)");
            Console.WriteLine("输入命令进行操作");
            Console.WriteLine("命令格式：ShowRoomList");
            Console.WriteLine("命令格式：CreateRoom %roomname%");
            Console.WriteLine("命令格式：JoinRoom %roomid%");
            Console.WriteLine("命令格式：ExitRoom");
            Console.WriteLine("命令格式：RoomReady %ready%");
            ConsoleInput.onInputLine.AddListener(OnInputLine);

            OnlineManager.Instance.Net.Rpc.RegisterListener(this);
        }


        private void OnInputLine(string line)
        {
            if (line == "ShowRoomList")
            {
                OnlineManager.Instance.Net.Rpc.Invoke("GetRoomList");
            }
            else if(line.StartsWith("CreateRoom"))
            {
                string[] args = line.Split(' ');
                if (args.Length > 1)
                {
                    UserData ud = OnlineManager.Instance.MainUserData;
                    string name = args[1];
                    OnlineManager.Instance.Net.Rpc.Invoke("CreateRoom", ud.id, name);
                }
                else
                {
                    Debuger.LogError("输入格式错误!");
                }
            }
            else if (line.StartsWith("JoinRoom"))
            {
                string[] args = line.Split(' ');
                if (args.Length > 1)
                {
                    UserData ud = OnlineManager.Instance.MainUserData;
                    uint id = uint.Parse(args[1]);
                    
                    OnlineManager.Instance.Net.Rpc.Invoke("JoinRoom", ud.id, id);
                    
                }
                else
                {
                    Debuger.LogError("输入格式错误!");
                }
            }
            else if (line == "ExitRoom")
            {
                UserData ud = OnlineManager.Instance.MainUserData;
                OnlineManager.Instance.Net.Rpc.Invoke("ExitRoom", ud.id, m_currRoom.id);
            }
            else if (line.StartsWith("RoomReady"))
            {
                string[] args = line.Split(' ');
                if (args.Length > 1)
                {
                    bool ready = args[1] == "true";
                    UserData ud = OnlineManager.Instance.MainUserData;
                    OnlineManager.Instance.Net.Rpc.Invoke("RoomReady", ud.id, m_currRoom.id, ready);
                }
                else
                {
                    Debuger.LogError("输入格式错误!");
                }
            }
            else if (line == "StartGame")
            {
                UserData ud = OnlineManager.Instance.MainUserData;
                OnlineManager.Instance.Net.Rpc.Invoke("StartGame", ud.id, m_currRoom.id);
            }

        }


        [RPCResponse]
        private void OnGetRoomList(RoomListData data)
        {
            if (data != null)
            {
                m_listRoom = data.rooms;
                for (int i = 0; i < m_listRoom.Count; i++)
                {
                    Debuger.LogWarning(m_listRoom[i].ToString());
                }
            }
            else
            {
                Debuger.LogWarning("房间列表为空！");
            }
        }


        [RPCResponse]
        private void OnCreateRoom(RoomData data)
        {
            Debuger.Log(data.ToString());
            m_currRoom = data;
        }

        [RPCResponse]
        private void OnJoinRoom(RoomData data)
        {
            Debuger.Log(data.ToString());
            m_currRoom = data;
        }

        [RPCNotify]
        private void NotifyRoomUpdate(RoomData data)
        {
            Debuger.Log(data.ToString());
            m_currRoom = data;
        }

        [RPCNotify]
        private void NotifyGameStart(FSPParam param)
        {
            Debuger.LogWarning(param.ToString());
        }
    }
}