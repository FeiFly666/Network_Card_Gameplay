using Common.Model;
using GameServer.Model.Battle.States;
using NetMessage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle
{
    public class BattleSyncSystem
    {
        private BattleRoom room;

        public Action OnAllPlayerSync;
        public Action OnSyncTimedout;

        public bool isWaitingStateSync = false;

        private float waitStateSyncTime = 0f;
        private int sendCount = 0;

        private const float maxWaitSyncTime = 5f;
        private const int maxSendCount = 5;

        private HashSet<string> syncedPlayers = new HashSet<string>();

        private BattleState syncState = BattleState.Loading;

        private float roundSyncTimer = 0f;
        private Message roundSyncMessage;
        public BattleSyncSystem(BattleRoom room)
        {
            this.room = room;
            roundSyncMessage = new Message();
            roundSyncMessage.Response = new ResponseMessage();
            roundSyncMessage.Response.battleSync = new BattleStateSync();
        }

        public void Update()
        {
            if(isWaitingStateSync)
            {
                //Console.WriteLine($"battle room {room.Id} : waiting for player response for change state");
                HandleStateSyncWaiting();
            }
            else
            {
                if(room.stateMachine.CurrentState?.stateType == BattleState.SubmitBattle)
                {
                    HandleRoundSync();
                }
            }
        }
        private void HandleRoundSync()
        {
            roundSyncTimer -= Time.deltaTime;
            if (roundSyncTimer<=0)
            {
                roundSyncTimer = 0.25f;
                int timeLeft = room.stateMachine.CurrentState is ITimedState timedState ? (int)timedState.timeLeft : -1;
                //Console.WriteLine($"battle room : room {this.room.Id} infoSync");
                //TODO:发包，广播剩余时间
                roundSyncMessage.Response.battleSync.remainTime = timeLeft;
                roundSyncMessage.Response.battleSync.State = (BattlePhase)room.stateMachine.CurrentState.stateType;

                roundSyncMessage.Response.battleSync.Round = room.round;
                roundSyncMessage.Response.battleSync.needResponse = false;

                room.messageSystem.BroadCast(roundSyncMessage);
            }
        }

        private void HandleStateSyncWaiting()
        {
            waitStateSyncTime += Time.deltaTime;

            if (syncedPlayers.Count == room.fullPlayerNum)
            {
                Console.WriteLine($"battle room {this.room.Id} : all player change state finished, apply new state");
                //ApplyState();
                AllPlayerSyncStateFinish();
                return;
            }
            else if (syncState == BattleState.Loading && syncedPlayers.Count == this.room.loadedPlayers.Count)
            {
                Console.WriteLine($"battle room {this.room.Id} : all player change state finished, apply new state");
                //ApplyState();
                AllPlayerSyncStateFinish();
                return;
            }

            if (waitStateSyncTime >= maxWaitSyncTime)
            {
                if (sendCount >= maxSendCount)
                {
                    Console.WriteLine($"battle room {this.room.Id} : change state timeout, game over");
                    //TODO：直接结算，等结算阶段完成添加

                    //DisbandRoom();
                    OnSyncTimedout?.Invoke();

                    return;
                }

                BroadCastStateSync();
            }

        }
        private void AllPlayerSyncStateFinish()
        {
            isWaitingStateSync = false;
            OnAllPlayerSync?.Invoke();
        }
        public void RequestChangeState(BattleState newState)
        {
            sendCount = 0;
            syncedPlayers.Clear();

            isWaitingStateSync = true;

            syncState = newState;

            BroadCastStateSync();
        }

        private void BroadCastStateSync()
        {
            roundSyncMessage.Response.battleSync.Round = room.round;
            roundSyncMessage.Response.battleSync.State = (BattlePhase)syncState;
            roundSyncMessage.Response.battleSync.needResponse = true;


            room.messageSystem.BroadCast(roundSyncMessage);

            sendCount++;
            waitStateSyncTime = 0;

            Console.WriteLine($"battle room {this.room.Id} : send state change sync   send time : {sendCount}");
        }

        public void NewPlayerStateSyncFinish(string player)
        {
            if (!syncedPlayers.Contains(player))
            {
                Console.WriteLine($"battle room {this.room.Id} : new player {player} change state finished");
                syncedPlayers.Add(player);
            }
        }
    }
}
