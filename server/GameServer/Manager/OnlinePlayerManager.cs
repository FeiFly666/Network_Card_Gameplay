using Common;
using Common.Network;
using GameServer.Network;
using NetMessage;
using System;
using Common.Model;
using System.Net.Http.Headers;

namespace GameServer.Manager
{
    public class OnlinePlayerManager : Singleton<OnlinePlayerManager>
    {
        private Dictionary<string, ClientSession> m_playerToClient = new Dictionary<string, ClientSession>();

       
        public ClientSession OnPlayerLogin(ISession session, UserLoginRequest request)
        {
            string playerID = request.userID;

            if(m_playerToClient.ContainsKey(playerID))
            {
                ClientSession oldSession = m_playerToClient[playerID];

                RoomManager.Instance.RemovePlayerFromRoomBySession(oldSession);

                ForceLeave(oldSession, "您已在别处登录");

                Console.WriteLine($"online player manager : player {request.userID} old client will be closed");

            }
            ClientSession client = (ClientSession)session;

            client.player = new Player();
            client.player.ready = false;
            client.player.socketID = client.ClientID;
            client.player.id = playerID;

            m_playerToClient[playerID] = client;
            return client;
        }

        public void ForceLeave(ClientSession session, string msg)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.forceLeave = new UserForceLeaveReponse();
            message.Response.forceLeave.Msg = msg;

            session.Send(message);
            //session.Close();
        }
        public bool TryGetClientSession(string key, out ClientSession session)
        {
            return m_playerToClient.TryGetValue(key, out session);
        }

        public bool IsPlayerOnline(string key)
        {
            return m_playerToClient.ContainsKey(key);
        }
        public void RemoveOnlinePlayer(string playerID)
        {
            if(m_playerToClient.ContainsKey(playerID))
            {
                m_playerToClient.Remove(playerID);
            }
        }
    }
}
