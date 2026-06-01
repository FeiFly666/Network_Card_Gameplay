using Common.Data;
using Common.Model;
using GameServer.Manager;
using GameServer.Model.Battle;
using GameServer.Network;
using GameServer.Service;
using GameServer.Utils;
using NetMessage;

namespace GameServer.Model
{
    public enum BattleState
    {
        Loading,//等待玩家加载（可在断线重连时复用？）
        SubmitBattle,//玩家进行使用卡牌和角色状态设置（除了角色目标即角色要进攻还是防御外实时同步）
        Battle,//显示角色目标，并按在场角色速度先后执行角色攻击（防御进入该状态后就执行）
        RoundEnd//结算，未结束进入下一回合，即新的submitbattle
    }
    public class BattleRoom
    {
        public int Id { get; set; }

        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public HashSet<string> loadedPlayers { get; set; } = new HashSet<string>();

        public BattleSyncSystem syncSystem;

        public BattleStateMachine stateMachine;

        public BattleMessageSystem messageSystem;

        public BattleCardManager cardManager;

        public BattleCharacterManager characterManager;

        public BattleEffectManager effectManager;

        public int fullPlayerNum;

        public bool isActive => stateMachine.CurrentState?.stateType != BattleState.Loading;
        public bool isStarted = false;
        public int round = 0;

        public int currentFreeID = 0;//card和character的id都用这个

        public string lastPlayerId;

        public List<BattleActonContext> battleActions = new List<BattleActonContext>();

        public Dictionary<(string, int), BattleAction> pendingBattleActions = new Dictionary<(string, int), BattleAction>();

        public BattleRoom()
        {
            syncSystem = new BattleSyncSystem(this);
            stateMachine = new BattleStateMachine(this);
            messageSystem = new BattleMessageSystem(this);
            cardManager = new BattleCardManager(this);
            characterManager = new BattleCharacterManager(this);
            effectManager = new BattleEffectManager(this);

            syncSystem.OnSyncTimedout += HandleSyncTimedOut;

            stateMachine.InitState(BattleState.Loading);
        }

        //进入唯一接口，包括断线重连
        public void NewPlayerLoaded(string playerID)
        {
            if (!loadedPlayers.Contains(playerID))
            {
                loadedPlayers.Add(playerID);
            }

            if (loadedPlayers.Count == fullPlayerNum && !isActive)
            {
                Console.WriteLine($"battle room : room {this.Id} start game");
                //todo:开始游戏
                if(!isStarted)
                {
                    isStarted = true;
                    NewRoundStart();
                }
                else //断线重连
                {
                    lastPlayerId = playerID;

                    BattleState resumeState = stateMachine.PreviousState.stateType;

                    SendReconnectSnapshot(players[playerID], resumeState);

                    if (loadedPlayers.Count == fullPlayerNum && !isActive)
                    {

                        ChangeState(resumeState, true);

                        /*if (resumeState == BattleState.SubmitBattle)
                        {
                            ChangeState(BattleState.SubmitBattle, true);
                        }
                        else
                        {
                            ChangeState(BattleState.RoundEnd);
                        }*/
                    }
                }
            }
        }

        public void ChangeState(BattleState newState, bool reconnect = false)
        {
            stateMachine.ChangeState(newState, reconnect);
        }

        public void Tick()
        {
            if (!isStarted) return;
            syncSystem.Update();

            if (isActive)
            {
                UpdateActivePlayer();

                if (syncSystem.isWaitingStateSync)
                {
                    
                    return;
                }
            }
            stateMachine.Update();
        }

        private void UpdateActivePlayer()//更新玩家在线状态，如果有不在线玩家则等待20s，20s后扔有不在线玩家则解散房间
        {
            foreach(var loaded in loadedPlayers.ToList())
            {
                if(loaded != null)
                {
                    bool success = false;
                    if (players.TryGetValue(loaded,out var player))
                    {
                        if(NetService.Instance.m_listener.TryGetClient(player.socketID, out var client))
                        {
                            if(client != null)
                            {
                                if(client.isOnline)
                                {
                                    success = true;
                                }
                            }
                        }
                    }
                    if (!success)
                    {
                        loadedPlayers.Remove(loaded);
                    }
                }
                
            }
            if(loadedPlayers.Count != fullPlayerNum)
            {
                ChangeState(BattleState.Loading);
            }
        }
        private void HandleSyncTimedOut()
        {
            messageSystem.SendTimeOutRoomDisband();
            DisbandRoom();
        }
        public void HandleLoadTimedOut()
        {
            messageSystem.SendTimeOutRoomDisband();
            DisbandRoom();
        }
        public void DisbandRoom()//解散房间
        {
            isStarted = false;
            BattleManager.Instance.RemoveBattleRoom(this);
        }

        private void RoundInit()
        {
            if(round == 1)
            {
                foreach(var playerkv in players)
                {
                    characterManager.InitMainCharacter(playerkv.Value);
                }
            }
            //每个玩家增添手牌
            //cardManager.DrawCardDemo();
            cardManager.DrawCard();

        }
        
        public void NewRoundStart()
        {
            //回合数增加并发送同步
            this.round++;
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.roundStart = new BattleRoundStartBroadcast();

            message.Response.roundStart.Round = round;

            messageSystem.BroadCast(message);

            //初始化回合
            RoundInit();

            //切换状态
            ChangeState(BattleState.SubmitBattle);
        }
        public void SendReconnectSnapshot(Player reconnectPlayer, BattleState state)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.Reconnect = BuildReconnectSnapshot(reconnectPlayer);

            if (state == BattleState.Battle) message.Response.Reconnect.Characters.Clear();

            messageSystem.SendToPlayer(reconnectPlayer, message);
        }
        public BattleReconnectSnapshot BuildReconnectSnapshot(Player self)
        {
            BattleReconnectSnapshot snapshot = new BattleReconnectSnapshot();

            snapshot.roomID = Id;
            snapshot.Round = round;
            snapshot.Phase = ToBattlePhase(GetSnapshotState());

            foreach (var playerKV in players)
            {
                string ownerID = playerKV.Key;
                Player player = playerKV.Value;

                foreach (var chaKV in player.battlePlayer.characters)
                {
                    Character cha = chaKV.Value;

                    BattleCharacterSnapshot c = BattleUtil.GenerateCharacterSnapShot(player, cha);

                    c.canDefend = characterManager.CanCharacterDefend(cha.ID);
                    c.canAttack = characterManager.CanCharacterAttack(cha.ID);

                    snapshot.Characters.Add(c);
                }
            }

            foreach (var cardKV in self.battlePlayer.cards)
            {
                Card card = cardKV.Value;

                BattleCardSnapshot c = new BattleCardSnapshot();
                c.Id = card.ID;
                c.dataID = card.Data.DataID;

                snapshot.selfCards.Add(c);
            }

            foreach (var playerKV in players)
            {
                if (playerKV.Key == self.id) continue;
                snapshot.enemyCardCount = playerKV.Value.battlePlayer.cards.Count;
            }

            foreach (var actionKV in pendingBattleActions)
            {
                string ownerID = actionKV.Key.Item1;
                int characterID = actionKV.Key.Item2;
                BattleAction action = actionKV.Value;

                BattlePendingActionSnapshot p = new BattlePendingActionSnapshot();
                p.ownerID = ownerID;
                p.characterID = characterID;
                p.actionType = action.chaActionType;
                p.targetID = action.Target != null ? action.Target.Id : -1;

                snapshot.pendingActions.Add(p);
            }

            return snapshot;
        }

        private BattleState GetSnapshotState()
        {
            if (stateMachine.CurrentState.stateType == BattleState.Loading && isStarted)
            {
                return stateMachine.PreviousState.stateType;
            }
            return stateMachine.CurrentState.stateType;
        }

        private BattlePhase ToBattlePhase(BattleState state)
        {
            switch (state)
            {
                case BattleState.SubmitBattle: return BattlePhase.PhasePrepare;
                case BattleState.Battle: return BattlePhase.PhaseAction;
                case BattleState.RoundEnd: return BattlePhase.PhaseSettle;
                default: return BattlePhase.PhaseNone;
            }
        }
    }
}
