using Common.Model;
using GameServer.Model.Battle.States;
using GameServer.Network;
using GameServer.Service;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle
{
    public class BattleMessageSystem
    {
        private BattleRoom room;
        public BattleMessageSystem(BattleRoom room)
        {
            this.room = room;
        }

        /// <summary>
        /// 广播信息给所有玩家
        /// </summary>
        public void BroadCast(Message message, Player except = null)
        {
            ClientSession session = null;
            foreach (var playerkv in room.players)
            {
                Player player = playerkv.Value;
                if(message.Response.battleSync == null)
                    Console.WriteLine($"broadcast drop room={room.Id}, to player={player.id}, socket={player.socketID}, ok={NetService.Instance.m_listener.TryGetClient(player.socketID, out session)}");
                if (NetService.Instance.m_listener.TryGetClient(player.socketID, out session))
                {
                    if (except != null)
                    {
                        if (player.id == except.id) continue;
                    }
                    session.Send(message);
                }
            }
        }
        public void BroadCastEvent(BattleEvent e)
        {
            Message msg = new Message();
            msg.Response = new ResponseMessage();
            msg.Response.battleEvent = new BattleEventBroadcast();
            msg.Response.battleEvent.Events.Add(e);

            BroadCast(msg);
        }

        /// <summary>
        /// 发给单个玩家
        /// </summary>
        public void SendToPlayer(Player player, Message message)
        {
            if (NetService.Instance.m_listener.TryGetClient(player.socketID, out var session))
            {
                if(session != null)
                    session.Send(message);
            }
        }
        //超时结束游戏发送
        public void SendTimeOutRoomDisband()
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.battleTimedOut = new BattleTimedoutResponse();

            BroadCast(message);
        }
        //正常游戏结束发送
        public void SendGameOver()
        {

        }

        public void RecieveBattleAction(ClientSession session, BattleAction action)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.battleAction = new BattleActionResponse();

            if (room.stateMachine.CurrentState?.stateType != BattleState.SubmitBattle)
            {
                Console.WriteLine($"battle room {room.Id} : player {session.player.id} battle action timeout");

                message.Response.battleAction.actionID = action.actionID;
                message.Response.battleAction.Success = false;
                message.Response.battleAction.Msg = "发送超时：未在行动阶段发送该动作";

                session.Send(message);
                return;
            }
            bool result = false;
            //room.submitQueue.Enqueue(new BattleActonContext(session.player.id, action));
            if(room.stateMachine.CurrentState != null && room.stateMachine.CurrentState is IBattleActionHandleState handleState)
            {
                BattleActonContext newCtx = new BattleActonContext(session.player.id, action);
                result = handleState.HandleBattleAction(newCtx);
            }

            message.Response.battleAction.actionID = action.actionID;
            message.Response.battleAction.Success = result;
            message.Response.battleAction.Msg = "";

            session.Send(message);
        }

        public void OnPlayerStateSyncFinished(ClientSession session)
        {
            string player = session.player.id;

            room.syncSystem.NewPlayerStateSyncFinish(player);
        }

        public void OnPlayerBattlePlayingFinished(ClientSession session)
        {
            if (room.stateMachine.CurrentState?.stateType == BattleState.Battle)
            {
                BattlingState currentSatate = (BattlingState)(room.stateMachine.CurrentState);
                currentSatate.AddNewFinishPlayer(session.player.id);
            }
        }
        public void OnPlayerPlayOneBattleEventFinished(ClientSession session, BattleEventPlayFinishRequest request)
        {
            if (room.stateMachine.CurrentState?.stateType == BattleState.Battle)
            {
                BattlingState currentSatate = (BattlingState)(room.stateMachine.CurrentState);
                currentSatate.NewEventPlayFinish(request.eventBatchID, request.eventID);
            }
            else
            {
                BattlingState battleState = (BattlingState)(room.stateMachine.GetState(BattleState.Battle));
                battleState.NewEventPlayFinish(request.eventBatchID, request.eventID);
            }
        }
    }
}
