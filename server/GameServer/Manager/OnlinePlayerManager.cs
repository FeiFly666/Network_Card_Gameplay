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
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserLoginRequest>(OnPlayerLogin);
            MessageDispatcher.Instance.Subscribe<UserChangeMainChaRequest>(OnPlayerChanggMainCharacter);
        }
        private void OnPlayerChanggMainCharacter(ISession session, UserChangeMainChaRequest request)
        {
            string newCharacterID = request.mainCharacterID;
            ClientSession client = (ClientSession)session;

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.changeMainCha = new UserChangeMainChaResponse();

            if(!DataManager.Instance.m_characters.ContainsKey(newCharacterID))
            {
                message.Response.changeMainCha.Success = false;
                message.Response.changeMainCha.Msg = "未发现请求的角色";
            }
            else
            {
                client.player.mainCharcterID = newCharacterID;
                DataManager.Instance.SavePlayerInfo(client.player);

                message.Response.changeMainCha.Success = true;
                message.Response.changeMainCha.playerData = new PlayerData();
                message.Response.changeMainCha.playerData.mainCharacterID = newCharacterID;
            }

            session.Send(message);

        }
        private void OnPlayerLogin(ISession session, UserLoginRequest request)
        {
            if (request == null) return;
            Message message = new Message();

            string playerID = request.userID;

            Console.WriteLine($"online player manager : player {request.userID} try log in");

            if(m_playerToClient.ContainsKey(playerID))
            {
                ClientSession oldSession = m_playerToClient[playerID];

                RoomManager.Instance.RemovePlayerFromRoomBySession(oldSession);

                ForceLeave(oldSession, "您已在别处登录");

                Console.WriteLine($"online player manager : player {request.userID} old client will be closed");

                //m_playerToClient.Remove(playerID);
            }
            ClientSession client = (ClientSession)session;

            client.player = new Player();
            client.player.ready = false;
            client.player.socketID = client.ClientID;
            client.player.id = playerID;


            m_playerToClient[playerID] = client;

            Console.WriteLine($"online player manager : player {request.userID} login successfully");
            if(!DataManager.Instance.m_playerInfos.ContainsKey(playerID))
            {
                client.player.mainCharcterID = "character1";
                DataManager.Instance.SavePlayerInfo(client.player);
            }

            client.player.mainCharcterID = DataManager.Instance.m_playerInfos[playerID].mainCharacterID;

            message.Response = new ResponseMessage();
            message.Response.Login = new UserLoginResponse();
            message.Response.Login.Success = true;
            message.Response.Login.playerData = new PlayerData();
            message.Response.Login.playerData.mainCharacterID = client.player.mainCharcterID;
            session.Send(message);


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

        public void ForceLeave(ClientSession session,string msg)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.forceLeave = new UserForceLeaveReponse();
            message.Response.forceLeave.Msg = msg;

            session.Send(message);
            //session.Close();
        }
    }
}
