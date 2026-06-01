using Common;
using Common.Model;
using Common.Network;
using GameServer.Model;
using GameServer.Network;
using NetMessage;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    public class BattleManager : Singleton <BattleManager>
    {
        private Dictionary<int,BattleRoom> m_battleRooms = new Dictionary<int,BattleRoom>();
        private Dictionary<string,BattleRoom> m_playerToRooms = new Dictionary<string,BattleRoom>();
        private int nextBattleRoomID = 0;
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserEnterBattleRequest>(OnPlayerEnterBattle);
            MessageDispatcher.Instance.Subscribe<BattleActionRequest>(OnPlayerSendBattleAction);
            MessageDispatcher.Instance.Subscribe<BattleStateSyncReq>(OnPlayerStateChangedFinished);
            MessageDispatcher.Instance.Subscribe<BattlePlayingFinishRequest>(OnPlayerBattlePlayFinished);
            MessageDispatcher.Instance.Subscribe<BattleEventPlayFinishRequest>(OnPlayerEventPlayFinish);
        }
        public void Tick()
        {
            foreach(var room in m_battleRooms.ToList())
            {
                BattleRoom theRoom = room.Value;
                if(theRoom != null)
                {
                    theRoom.Tick();
                }
            }
        }
        public void RemoveBattleRoom(BattleRoom room)
        {
            foreach(var playerkv in room.players)
            {
                Player player = playerkv.Value;
                if(m_playerToRooms.ContainsKey(player.id))
                    m_playerToRooms.Remove(player.id);
            }
            if(m_battleRooms.ContainsKey(room.Id))
            {
                m_battleRooms.Remove(room.Id);
            }
            Console.WriteLine($"battle manager : battle room {room.Id} removed");
        }
        public void RoomToBattleRoom(Room room)
        {
            BattleRoom newBattleRoom = new BattleRoom();
            newBattleRoom.Id = nextBattleRoomID++;
            newBattleRoom.players = new Dictionary<string, Player>();
            foreach(var player in room.Players)
            {
                string playerID = player.id;
                if(newBattleRoom.players.ContainsKey(playerID))
                {
                    newBattleRoom.players[playerID] = player;
                }
                else
                {
                    newBattleRoom.players.Add(playerID, player);
                }
                player.battlePlayer = new BattlePlayer();
            }

            newBattleRoom.fullPlayerNum = room.RoomPlayerNum;
            newBattleRoom.cardManager.InitPlayerSendCardNum();

            int id = newBattleRoom.Id;
            if(m_battleRooms.ContainsKey(id))
            {
                m_battleRooms[id] = newBattleRoom;
            }
            else
            {
                m_battleRooms.Add(id, newBattleRoom);
            }
            foreach(var pkv in newBattleRoom.players)
            {
                Player p = pkv.Value;

                Console.WriteLine($"battle manager : player {p.id} enter battle room");

                if (m_playerToRooms.ContainsKey(p.id))
                {
                    m_playerToRooms[p.id] = newBattleRoom;
                }
                else
                {
                    m_playerToRooms.Add(p.id, newBattleRoom);
                }
            }
            Console.WriteLine($"battle manager : room{id} change to battle room");
        }
        public bool IsPlayerInBattleRoom(string player)
        {
            return m_playerToRooms.ContainsKey(player);
        }
        public void EnterBattleRoom(string player)
        {
            if(m_playerToRooms.ContainsKey(player))
            {
                m_playerToRooms[player].NewPlayerLoaded(player);
            }
        }
        /// <summary>
        /// 等待所有玩家进入战斗场景
        /// </summary>
        private void OnPlayerEnterBattle(ISession session, UserEnterBattleRequest request)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.userEnterBattle = new UserEnterBattleResponse();

            ClientSession client = (ClientSession) session;

            string playerID = client.player.id;
            Console.WriteLine($"battle manager : player {playerID} try enter battle room");

            if (!m_playerToRooms.TryGetValue(playerID,out var room))
            {
                Console.WriteLine($"battle manager : can not find player {playerID} battle room");
                message.Response.userEnterBattle.Success = false;
                message.Response.userEnterBattle.Msg = "未找到玩家所在战斗房间";
                session.Send(message);
                return;
            }

            if (room.players.TryGetValue(playerID, out var battlePlayer))
            {
                battlePlayer.socketID = client.ClientID;
                client.player = battlePlayer;
            }

            room.NewPlayerLoaded(playerID);

            message.Response.userEnterBattle.Success = true;
            message.Response.userEnterBattle.Msg = "";
            session.Send(message);

        }

        private void OnPlayerSendBattleAction(ISession session, BattleActionRequest request)
        {
            ClientSession client = (ClientSession)session;
            string playerID = client.player.id;

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.battleAction = new BattleActionResponse();

            Console.WriteLine($"battle manager : player {playerID} try send battle action");

            if(!m_playerToRooms.TryGetValue(playerID, out var room))
            {
                Console.WriteLine($"battle manager : can not find player {playerID} battle room");
                message.Response.battleAction.Success = false;

                client.Send(message);
                return;
            }
            Console.WriteLine($"battle manager : player {playerID} send battle action successfully");

            //message.Response.battleAction.Success = true;

            room.messageSystem.RecieveBattleAction(client, request.Action);

            //client.Send(message);
        }
        private void OnPlayerStateChangedFinished(ISession session, BattleStateSyncReq request)
        {
            ClientSession client = (ClientSession)session;

            string playerID = client.player.id;

            Console.WriteLine($"battle manager : player {playerID} finish state change");

            if (!m_playerToRooms.TryGetValue(playerID, out var room))
            {
                Console.WriteLine($"battle manager : can not find player {playerID} battle room");
                return;
            }
            room.messageSystem.OnPlayerStateSyncFinished(client);
        }
        private void OnPlayerBattlePlayFinished(ISession session, BattlePlayingFinishRequest request)
        {
            ClientSession client = (ClientSession)session;

            string playerID = client.player.id;

            Console.WriteLine($"battle manager : player {playerID} finish state change");

            if (!m_playerToRooms.TryGetValue(playerID, out var room))
            {
                Console.WriteLine($"battle manager : can not find player {playerID} battle room");
                return;
            }
            room.messageSystem.OnPlayerBattlePlayingFinished(client);
        }

        private void OnPlayerEventPlayFinish(ISession session, BattleEventPlayFinishRequest finish)
        {
            ClientSession client = (ClientSession)session;

            string playerID = client.player.id;

            Console.WriteLine($"battle manager : player {playerID} finish play one battle event");
            if (!m_playerToRooms.TryGetValue(playerID, out var room))
            {
                Console.WriteLine($"battle manager : can not find player {playerID} battle room");
                return;
            }
            room.messageSystem.OnPlayerPlayOneBattleEventFinished(client, finish);
        }
    }
}
