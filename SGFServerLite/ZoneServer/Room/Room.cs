using System.Collections.Generic;
using System.Text;
using SGF.Extension;
using SGF.Network.FSPLite;
using SGF.Network.General.Server;
using SGF.Utils;
using SGFAppDemo.Common.Data;


namespace SGFServerDemo.ZoneServer
{
    public class Room
    {
        private static uint ms_lastRid = 0;
        public static uint NewRoomID()
        {
            return ++ms_lastRid;
        }

        private RoomData m_data;
        public RoomData data{get { return m_data; }}


        private DictionarySafe<uint, ISession> m_mapSession;

        public override string ToString()
        {
            return string.Format("<data:{0}, sessions:{1}>",m_data, m_mapSession.ToListString());
        }

        public string DumpString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<id:{0}, name:{1}, owner:{2}, players_count:{3}>\n", 
                m_data.id, m_data.name, m_data.owner, m_data.players.Count);

            sb.AppendLine(prefix + "\tPlayerList:");
            for (int i = 0; i < m_data.players.Count; i++)
            {
                sb.AppendLine(prefix + "\t" + m_data.players[i].ToString());
            }

            sb.AppendLine(prefix + "\tSessionList:");
            foreach (var session in m_mapSession)
            {
                sb.AppendLine(prefix + "\t" + session.Value.ToString());
            }

            return sb.ToString();
        }

        public void Create(uint userId, string userName, ISession session, string roomName)
        {
            m_mapSession = new DictionarySafe<uint, ISession>();

            m_data = new RoomData();
            m_data.id = NewRoomID();
            m_data.name = roomName;
            m_data.owner = userId;
            AddPlayer(userId, userName, session);
        }

        public void AddPlayer(uint userId, string userName, ISession session)
        {
            PlayerData data = GetPlayerInfoByUserId(userId);
            if (data == null)
            {
                data = new PlayerData();
                m_data.players.Add(data);
                data.id = (uint)m_data.players.Count;
                data.sid = session.Id;
            }
            data.isReady = false;
            data.userId = userId;
            data.name = userName;

            m_mapSession[userId] = session;
        }


        public void RemovePlayer(uint userId)
        {
            int i = GetPlayerIndexByUserId(userId);
            if (i >= 0)
            {
                m_data.players.RemoveAt(i);
            }

            m_mapSession.Remove(userId);
            
            if (userId == m_data.owner)
            {
                if (m_data.players.Count > 0)
                {
                    m_data.owner = m_data.players[0].userId;
                }
            }
        }

        public int GetPlayerCount()
        {
            return m_data.players.Count;
        }
        


        private int GetPlayerIndexByUserId(uint userId)
        {
            for (int i = 0; i < m_data.players.Count; i++)
            {
                if (m_data.players[i].userId == userId)
                {
                    return i;
                }
            }
            return -1;
        }


        private PlayerData GetPlayerInfoByUserId(uint userId)
        {
            for (int i = 0; i < m_data.players.Count; i++)
            {
                if (m_data.players[i].userId == userId)
                {
                    return m_data.players[i];
                }
            }
            return null;
        }

        public ISession[] GetSessionList()
        {
            List<ISession> list = new List<ISession>();
            for (int i = 0; i < m_data.players.Count; i++)
            {
                uint userId = m_data.players[i].userId;
                list.Add(m_mapSession[userId]);
            }

            return list.ToArray();
        }

        public bool CanStartGame()
        {
            if (m_data.players.Count > 1 && IsAllReady())
            {
                return true;
            }
            return false;
        }

        public bool IsAllReady()
        {
            bool isAllReady = true;
            for (int i = 0; i < m_data.players.Count; i++)
            {
                if (!m_data.players[i].isReady)
                {
                    isAllReady = false;
                    break;
                }
            }
            return isAllReady;
        }


        public void SetReady(uint userId, bool value)
        {
            var info = GetPlayerInfoByUserId(userId);
            if (info != null)
            {
                info.isReady = value;
            }

        }


        public FSPParam GetGameStartParam()
        {
            FSPParam param = new FSPParam();
            return param;
        }

    }
}