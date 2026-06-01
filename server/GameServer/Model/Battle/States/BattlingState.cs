using Common.Model;
using GameServer.Manager;
using GameServer.Utils;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.States
{
    class BattleEventRecord
    {
        public BattleEvent Event;
        public List<BattleCharacterSnapshot> BeforeCharacters;
        public List<BattleCardSnapshot> BeforeCards;
    }
    public class BattlingState : IState, IResumeState
    {
        protected BattleRoom room;
        public BattleState stateType { get; }
        public float timeLeft { get; set; }

        private float maxTime = 2f;

        private List<((string, int)key, BattleAction)> m_actions = new List<((string, int) key, BattleAction)>();

        private List<BattleEvent> m_processEvents = new List<BattleEvent>();
        private List<BattleEventRecord> m_records = new List<BattleEventRecord> ();

        private Message broadCastPackage;

        private HashSet<string> m_finishedPlayers = new HashSet<string>();

        private HashSet<int> m_DefendCharacters = new HashSet<int>();

        private int currentBatchID = 0;
        private int eventID = 0;
        private int maxPlayEventID = -1;
        public BattlingState(BattleRoom room)
        {
            stateType = BattleState.Battle;
            this.room = room;

            broadCastPackage = new Message();
            broadCastPackage.Response = new ResponseMessage();
            broadCastPackage.Response.battleEvent = new BattleEventBroadcast();
        }
        public void Enter()
        {
            m_finishedPlayers.Clear();
            m_DefendCharacters.Clear();
            m_records.Clear();

            currentBatchID = room.round;
            eventID = 0;
            maxPlayEventID = -1;

            CalculateStateTime();
            timeLeft = maxTime;
            HandlePendingActions();
        }
        public void Resume()
        {
            int startID = maxPlayEventID + 1;

            string resumePlayer = room.lastPlayerId;

            var newEventList = new List<BattleEvent>();

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.Reconnect = room.BuildReconnectSnapshot(room.players[resumePlayer]);

            message.Response.Reconnect.enemyCardCount = 0;
            message.Response.Reconnect.selfCards.Clear();

            Console.WriteLine($"battle room {room.Id} : battle state resume  startEventID: {startID}");

            if (startID >= m_processEvents.Count)
            {
                room.messageSystem.SendToPlayer(room.players[resumePlayer], message);
                AddNewFinishPlayer(resumePlayer);
                return;
            }

            for(int i = startID; i< m_processEvents.Count; i++)
            {
                //if (m_processEvents[i].Type == BattleEvent.EventType.EventCharacterAttributechange) continue;
                newEventList.Add(m_processEvents[i]);
            }

            BattleEventRecord record = m_records.Find(r => r.Event == m_processEvents[startID]);

            message.Response.Reconnect.Characters.Clear();
            message.Response.Reconnect.Characters.AddRange(record.BeforeCharacters);

            room.messageSystem.SendToPlayer(room.players[resumePlayer], message);

            broadCastPackage.Response.battleEvent.Events.Clear();
            broadCastPackage.Response.battleEvent.Events.AddRange(newEventList);

            room.messageSystem.SendToPlayer(room.players[resumePlayer], broadCastPackage);
        }
        private void CalculateStateTime()
        {
            float totalTime = 0f;
            foreach(var actionkv in room.pendingBattleActions)
            {
                BattleAction action = actionkv.Value;
                if (action.chaActionType == CharacterActionType.ChaActionTypeAttack)
                {
                    totalTime += 5f;
                }
                else
                {
                    totalTime += 1f;
                }
            }
            maxTime = totalTime;
        }
        public void Update()
        {
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0 || m_finishedPlayers.Count == room.loadedPlayers.Count)
            {
                //room.ChangeState(BattleState.SubmitBattle);
                //room.NewRoundStart();
                room.ChangeState(BattleState.RoundEnd);
                return;
            }
        }
        private void HandlePendingActions()
        {
            m_actions.Clear();

            InitActions();

            SortActions();

            SimulateProcess();

            broadCastPackage.Response.battleEvent.Events.AddRange(m_processEvents);

            room.messageSystem.BroadCast(broadCastPackage);
        }
        private void InitActions()
        {
            foreach(var actionkv in room.pendingBattleActions)
            {
                m_actions.Add((actionkv.Key, actionkv.Value));
            }

            room.pendingBattleActions.Clear();
            m_processEvents.Clear();
        }
        private void SortActions()
        {
            m_actions.Sort((a, b) =>
            {
                string playerAID = a.key.Item1;
                int characterAID = a.key.Item2;
                BattleAction actionA = a.Item2;

                string playerBID = b.key.Item1;
                int characterBID = b.key.Item2;
                BattleAction actionB = b.Item2;

                int aPriority = BattleUtil.GetActionPriority(actionA.chaActionType);
                int bPriority = BattleUtil.GetActionPriority(actionB.chaActionType);
                if (aPriority != bPriority)
                {
                    return aPriority.CompareTo(bPriority);
                }

                int aSpeed = room.players[playerAID].battlePlayer.characters[characterAID].CurSpeed;
                int bSpeed = room.players[playerBID].battlePlayer.characters[characterBID].CurSpeed;

                if(aSpeed != bSpeed)
                {
                    return bSpeed.CompareTo(aSpeed);
                }

                return characterAID.CompareTo(characterBID);
            });
        }

        private void SimulateProcess()
        {
            broadCastPackage.Response.battleEvent.Events.Clear();
            foreach(var ctx in m_actions)
            {
                string playerID = ctx.key.Item1;
                int characterID = ctx.key.Item2;

                BattleAction action = ctx.Item2;

                //到这个行动的时候这个角色可能已经死了
                if(!room.characterManager.IsCharacterExist(characterID))
                {
                    continue;
                }

                switch (action.chaActionType)
                {
                    case CharacterActionType.ChaActionTypeDefend:
                        {
                            room.characterManager.CharacterDefend(characterID);
                            //角色防御事件
                            BattleEvent e1 = new BattleEvent();
                            e1.actionID = -1;
                            e1.Id = characterID;
                            e1.Type = BattleEvent.EventType.EventCharacterDefend;

                            AddNewEvents(e1);

                            room.players[playerID].battlePlayer.characters[characterID].isDefend = true;
                        }
                        break;
                    case CharacterActionType.ChaActionTypeAttack:
                        {
                            Character attacker = room.players[playerID].battlePlayer.characters[characterID];
                            Character target = null;
                            if (room.characterManager.GetCharacterOwner(action.Target.Id, out var targetPlayer))
                            {
                                if (room.players[targetPlayer].battlePlayer.characters.ContainsKey(action.Target.Id))
                                {
                                    target = room.players[targetPlayer].battlePlayer.characters[action.Target.Id];
                                }
                                else//目标已死亡
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                            room.characterManager.GetCharacterData(attacker.ID, out var attackerData);
                            //首先播放攻击
                            BattleEvent e1 = new BattleEvent();
                            //协议有点少，因为这是系统自动生成的不需要actionid，所以把actionid当成被攻击对象的id
                            e1.actionID = action.Target.Id;
                            e1.Id = characterID;
                            e1.Type = BattleEvent.EventType.EventCharacterAttack;

                            AddNewEvents(e1);
                            if(attackerData.CharacterType == Common.Data.CharacterType.Attack)
                            {
                                //播放攻击后角色血量减少

                                int defendDecrease = 0;

                                int damage = BattleUtil.CalculateDamage(attacker, target, out defendDecrease);
                                if (defendDecrease > 0)
                                {
                                    BattleEvent e2 = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Defence, -defendDecrease);
                                    AddNewEvents(e2);
                                    target.CurDEF -= defendDecrease;
                                }
                                if (damage > 0)
                                {
                                    damage = Math.Min(damage, target.CurHP);
                                    BattleEvent e2 = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Health, -damage);
                                    AddNewEvents(e2);
                                    target.CurHP -= damage;
                                    if (target.CurHP < 0)
                                    {
                                        //damage -= Math.Abs(target.CurHP);
                                        target.CurHP = 0;
                                    }
                                }
                                if (target.isDefend)
                                {
                                    room.characterManager.CharacterCantAttack(attacker.ID);
                                    m_DefendCharacters.Add(target.ID);
                                    //target.isDefend = false;
                                }
                                //如果target死亡则remove
                                if (target.CurHP > 0) continue;


                                BattleEvent e3 = new BattleEvent();

                                e3.Type = BattleEvent.EventType.EventCharacterDie;
                                e3.Id = action.Target.Id;
                                e3.Param = 1;
                                e3.paramS = targetPlayer;
                                e3.actionID = -1;

                                AddNewEvents(e3);

                                room.characterManager.RemoveCharacter(targetPlayer, action.Target.Id);
                            }
                            else
                            {
                                int healNum = BattleUtil.CalculateHealNum(attacker, target);
                                if(healNum > 0)
                                {
                                    BattleEvent e2 = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Health, healNum);
                                    AddNewEvents(e2);
                                    target.CurHP += healNum;
                                }

                            }
                            
                        }
                        break;
                }
            }
        }
        private void AddNewEvents(BattleEvent e)
        {
            e.eventBatchID = this.currentBatchID;
            e.eventID = this.eventID++;

            BattleEventRecord record = new BattleEventRecord();

            record.Event = e;
            record.BeforeCharacters = new List<BattleCharacterSnapshot>();

            foreach(var player in room.players)
            {
                foreach(var character in player.Value.battlePlayer.characters)
                {
                    record.BeforeCharacters.Add(BattleUtil.GenerateCharacterSnapShot(player.Value, character.Value));
                }
            }
            m_records.Add(record);
            m_processEvents.Add(e);
        }
        public void Exit()
        {
            RefreshCharacterStatus();
        }

        private void RefreshCharacterStatus()
        {
            foreach (var characterID in m_DefendCharacters)
            { 
                //到这个行动的时候这个角色可能已经死了
                if (!room.characterManager.IsCharacterExist(characterID))
                {
                    continue;
                }
                if (!room.characterManager.GetCharacterOwner(characterID, out var playerID))
                {
                    continue;
                }
                room.players[playerID].battlePlayer.characters[characterID].isDefend = false;
            }
        }

        public void AddNewFinishPlayer(string  playerID)
        {
            if(!m_finishedPlayers.Contains(playerID))
            {
                Console.WriteLine($"battle room {room.Id} : new player {playerID} battle animation finish play");
                m_finishedPlayers.Add(playerID);
            }
        }
        public void NewEventPlayFinish(int batchID, int eventID)
        {
            if (batchID != this.currentBatchID)
            {
                Console.WriteLine($"battle room {room.Id} : battle event finish request invaild");
                return;
            }

            maxPlayEventID = Math.Max(maxPlayEventID, eventID);
            Console.WriteLine($"battle room {room.Id} : battle event finish request recieve  [current maxPlayID {maxPlayEventID}]");
        }
    }
}
