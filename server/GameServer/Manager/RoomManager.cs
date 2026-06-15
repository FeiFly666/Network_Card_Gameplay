using Common;
using Common.Model;
using Common.Network;
using GameServer.Network;
using GameServer.Service;
using GameServer.Model;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    public class RoomManager : Singleton<RoomManager>
    {
        private Dictionary<int,Room> m_Rooms = new Dictionary<int,Room>();
        private Dictionary<string,Room> m_PlayInRoom = new Dictionary<string,Room>();
        private int nextRoomID = 0;
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserMatchRequest>(OnUserTryMatch);
            MessageDispatcher.Instance.Subscribe<UserReadyRequest>(OnUserChangeReadyStatus);
            MessageDispatcher.Instance.Subscribe<UserExitRequest>(OnUserExit);
        }
        /// <summary>
        /// 玩家尝试匹配房间
        /// </summary>
        private void OnUserTryMatch(ISession session, UserMatchRequest request)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();

            message.Response.userMatch = new UserMatchResponse();

            ClientSession client = (ClientSession)session;

            string userID = client.player.id;

            Room matchRoom = null;
            Console.WriteLine($"match request : {userID} try match room");
            lock (m_Rooms)
            {
                if (m_PlayInRoom.ContainsKey(userID))//已经在房间中
                {
                    Console.WriteLine($"match request : {userID} has in a room");
                    message.Response.userMatch.Success = false;
                    
                }
                else if(BattleManager.Instance.IsPlayerInBattleRoom(userID))//有战斗房间
                {
                    //BattleManager.Instance.EnterBattleRoom(userID);
                    message.Response.userMatch.Success = true;
                    message.Response.userMatch.roomID = -1;
                }
                else
                {
                    foreach (var room in m_Rooms.Values)
                    {
                        if (room.Players.Count > 0 && room.Players.Count < 2)
                        {
                            Console.WriteLine($"match request : {userID} match room successfully roomID:{room.ID}");
                            matchRoom = room;
                            break;
                        }
                    }
                    //找不到找空位的
                    if (matchRoom == null)
                    {
                        foreach (var room in m_Rooms.Values)
                        {
                            if (room.Players.Count == 0)
                            {
                                Console.WriteLine($"match request : {userID} match room successfully roomID:{room.ID}");
                                matchRoom = room;
                                break;
                            }
                        }
                    }
                    //无可用房间创建
                    if (matchRoom == null)
                    {
                        Console.WriteLine($"match request : {userID} match room no avialable room ,start [create]");
                        Room room = new Room();
                        room.RoomPlayerNum = 2;
                        room.ID = nextRoomID++;
                        m_Rooms.Add(room.ID, room);
                        matchRoom = room;
                    }

                    Console.WriteLine($"match request : {userID} join room {matchRoom.ID}");
                   

                    matchRoom.Players.Add(client.player);

                    m_PlayInRoom.Add(client.player.id, matchRoom);

                    client.player.ready = false;
                    message.Response.userMatch.Success = true;
                }
            }

            session.Send(message);
            if(message.Response.userMatch.Success)
            {
                OnRoomPlayerInfoChanged(matchRoom);
            }
        }
        /// <summary>
        /// 玩家更新准备状态
        /// </summary>
        private void OnUserChangeReadyStatus(ISession session, UserReadyRequest request)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.userReady = new UserReadyResponse();

            ClientSession client = (ClientSession)session;

            string userID = client.player.id;
            if(!m_PlayInRoom.TryGetValue(userID,out var room))
            {
                message.Response.userReady.Success = false;
                message.Response.userReady.Msg = "未发现用户所在房间";

                session.Send(message);
                return;
            }

            Player playerInfo = room.Players.Find(p => p.id == userID);
            if(playerInfo == null)
            {
                message.Response.userReady.Success = false;
                message.Response.userReady.Msg = "未在房间中发现用户";

                session.Send(message);
                return;
            }

            if(client.ClientID != playerInfo.socketID)
            {
                message.Response.userReady.Success = false;
                message.Response.userReady.Msg = "无效旧信息";

                session.Send(message);
                return;
            }

            playerInfo.ready = !playerInfo.ready;

            message.Response.userReady.Success = true;
            message.Response.userReady.Msg = "已修改用户状态";

            session.Send(message);

            OnRoomPlayerInfoChanged(room);
        }
        /// <summary>
        /// 玩家退出房间
        /// </summary>
        public void OnUserExit(ISession session, UserExitRequest request)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.userExit = new UserExitResponse();

            ClientSession client = (ClientSession)session;
            string userID = client.player.id;

            if (!m_PlayInRoom.TryGetValue(userID, out var room))
            {
                message.Response.userExit.Success = false;
                message.Response.userExit.Msg = "玩家未进入房间";
                session.Send(message);
                return;
            }

            Player player = room.Players.Find( p => p.id == userID);
            if (player == null)
            {
                message.Response.userExit.Success = false;
                message.Response.userExit.Msg = "玩家已退出房间";
                session.Send(message);
                return;
            }

            if (client.ClientID != player.socketID)
            {
                message.Response.userReady.Success = false;
                message.Response.userReady.Msg = "无效旧信息";

                session.Send(message);
            }

            room.Players.Remove(player);

            m_PlayInRoom.Remove(userID);

            Console.WriteLine($"user exit: user{userID} exit room successfully");

            message.Response.userExit.Success = true;
            message.Response.userExit.Msg = "玩家成功退出房间";
            session.Send(message);

            OnRoomPlayerInfoChanged(room);
        }

        /// <summary>
        /// 房间信息更新
        /// </summary>
        private void OnRoomPlayerInfoChanged(Room room)
        {
            if (room == null) return;
            Console.WriteLine($"RoomInfoChanged : room:{room.ID}");

            if (room.Players.Count == 0)//没有玩家需要关注这个房间了
            {
                return;
            }


            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.roomInfo = new RoomInfoBroadcast();
            message.Response.roomInfo.roomInfo = new RoomInfo();

            message.Response.roomInfo.roomInfo.roomID = room.ID;

            bool isAllPlayerReady = true;

            foreach(var player in room.Players)
            {
                PlayerInfo newPlayerInfo = new PlayerInfo();
                newPlayerInfo.userID = player.id;
                newPlayerInfo.Ready = player.ready;

                newPlayerInfo.playerData = new NetMessage.PlayerData();
                newPlayerInfo.playerData.mainCharacterID = player.mainCharcterID;
                newPlayerInfo.playerData.nickName = player.nickname;
                if (!player.ready)
                {
                    isAllPlayerReady = false;
                }

                message.Response.roomInfo.roomInfo.Players.Add(newPlayerInfo);
            }

            foreach (var player in room.Players)
            {
                ClientSession client = null;
                if(NetService.Instance.m_listener.TryGetClient(player.socketID, out client))
                {
                    client.Send(message);
                }
            }

            if(isAllPlayerReady && room.Players.Count == room.RoomPlayerNum)//所有玩家准备完毕
            {
                Console.WriteLine($"room {room.ID} ready to enter battle");
                message.Response = new ResponseMessage();
                message.Response.enterBattle = new BattleEnter();

                message.Response.enterBattle.Success = true;

                BattleManager.Instance.RoomToBattleRoom(room);

                foreach (var player in room.Players)
                {
                    ClientSession client = null;
                    if (NetService.Instance.m_listener.TryGetClient(player.socketID, out client))
                    {
                        client.Send(message);
                    }
                    m_PlayInRoom.Remove(player.id);
                }

                room.Players.Clear();

            }
            

        }

        public bool IsPlayerInRoom(string userID, int clientID)
        {
            if(!m_PlayInRoom.ContainsKey(userID))return false;

            Room room = m_PlayInRoom[userID];

            Player player = room.Players.Find(p => p.id == userID);
            return player != null && player.socketID == clientID;
        }
        public void RemovePlayerFromRoomBySession(ClientSession client)
        {
            if (client.player == null) return;

            string userID = client.player.id;

            if (!m_PlayInRoom.TryGetValue(userID, out var room)) return;

            Player player = room.Players.Find(p => p.id == userID);
            if (player == null) return;

            if (player.socketID != client.ClientID) return;

            room.Players.Remove(player);
            m_PlayInRoom.Remove(userID);

            Console.WriteLine($"room manager : remove old session player {userID} from room {room.ID}");

            OnRoomPlayerInfoChanged(room);
        }
    }
}
