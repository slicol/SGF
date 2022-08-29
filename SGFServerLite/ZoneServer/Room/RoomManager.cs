using System.Collections.Generic;
using System.Text;
using SGF;
using SGF.Network.Core.RPCLite;
using SGF.Network.FSPLite;
using SGF.Network.General.Server;
using SGF.Utils;
using SGFAppDemo.Common.Data;
using SGF.Utils;


namespace SGFServerDemo.ZoneServer
{
    public class RoomManager:Singleton<RoomManager>
    {
        private NetManager m_net;
        private List<Room> m_listRoom = new List<Room>();

        public void Init(ServerContext context)
        {
            m_net = context.net;
            //m_net.RegisterRPCListener(this);
            m_net.Rpc.RegisterListener(this);
        }

        public void Dump()
        {
            
            StringBuilder sb = new StringBuilder();
            Room[] list = m_listRoom.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                sb.AppendLine("\t" + list[i].DumpString("\t"));
            }

            Debuger.LogWarning("\nRooms ({0}):\n{1}", m_listRoom.Count, sb);
            
        }

        [RPCRequest]
        private void GetRoomList(ISession session)
        {
            
            List<RoomData> list = new List<RoomData>();
            for (int i = 0; i < m_listRoom.Count; i++)
            {
                list.Add(m_listRoom[i].data);
            }
            RoomListData data = new RoomListData();
            data.rooms = list;
            m_net.Rpc.Return(data);
        }

        [RPCRequest]
        private void CreateRoom(ISession session, uint userId, string roomName)
        {
            Room room = new Room();
            UserData ud = OnlineManager.Instance.GetUserData(userId);
            room.Create(userId, ud.name, session, roomName);
            m_listRoom.Add(room);

            m_net.Rpc.Return(room.data);
        }

        [RPCRequest]
        private void JoinRoom(ISession session, uint userId, uint roomId)
        {
            Room room = GetRoom(roomId);
            if (room != null)
            {
                UserData ud = OnlineManager.Instance.GetUserData(userId);
                
                room.AddPlayer(userId, ud.name, session);
                ISession[] listSession = room.GetSessionList();
                m_net.Rpc.Invoke(listSession,"NotifyRoomUpdate", room.data);
            }
            else
            {
                m_net.Rpc.ReturnError("房间不存在", (int)roomId);
            }
        }


        [RPCRequest]
        private void ExitRoom(ISession session, uint userId, uint roomId)
        {
            Room room = GetRoom(roomId);
            if (room != null)
            {
                room.RemovePlayer(userId);

                if (room.GetPlayerCount() > 0)
                {
                    ISession[] listSession = room.GetSessionList();
                    m_net.Rpc.Invoke(listSession, "NotifyRoomUpdate", room.data);
                }
            }
        }

        [RPCRequest]
        private void RoomReady(ISession session, uint userId, uint roomId,  bool ready)
        {
            Room room = GetRoom(roomId);
            if (room != null)
            {
                room.SetReady(userId, ready);
                ISession[] listSession = room.GetSessionList();
                m_net.Rpc.Invoke(listSession, "NotifyRoomUpdate", room.data);
            }
            else
            {
                m_net.Rpc.ReturnError("房间不存在", (int)roomId);
            }
        }


        [RPCRequest]
        private void StartGame(ISession session, uint userId, uint roomId)
        {
            Room room = GetRoom(roomId);
            if (room != null)
            {
                if (room.data.owner == userId)
                {
                    if (room.CanStartGame())
                    {
                        FSPParam param = room.GetGameStartParam();
                        ISession[] listSession = room.GetSessionList();
                        m_net.Rpc.Invoke(listSession, "NotifyGameStart", param);
                    }
                }
            }
            else
            {
                m_net.Rpc.ReturnError("房间不存在", (int)roomId);
            }
        }


        private Room GetRoom(uint id)
        {
            for (int i = 0; i < m_listRoom.Count; i++)
            {
                if (m_listRoom[i].data.id == id)
                {
                    return m_listRoom[i];
                }
            }
            return null;
        }

    }
}
