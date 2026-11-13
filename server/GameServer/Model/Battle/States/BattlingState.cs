using Common.Data;
using Common.Model;
using GameServer.Manager;
using GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts;
using GameServer.Model.Battle.SkillHandler;
using GameServer.Utils;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace GameServer.Model.Battle.States
{
    class BattleEventRecord
    {
        public BattleEvent Event;
        public List<BattleCharacterSnapshot> BeforeCharacters;
        public List<BattleCardSnapshot> BeforeCards;
    }
    public enum AttackOrigin
    { 
        Player,
        FollowUp,
        Reflect,
        Skill
    }
    public class BattlingState : IState, IResumeState
    {
        protected BattleRoom room;
        public BattleState stateType { get; }
        public float timeLeft { get; set; }

        private float maxTime = 2f;

        private List<((string, int)key, BattleAction)> m_actions = new List<((string, int) key, BattleAction)>();
        private Dictionary<SkillType, SkillHandlerBase> m_SkillHandlers = new Dictionary<SkillType, SkillHandlerBase>();

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

            ChangeHPHandler hpHandHandler = new ChangeHPHandler(this);
            m_SkillHandlers[SkillType.Heal] = hpHandHandler;
            m_SkillHandlers[SkillType.Damage] = hpHandHandler;


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
                else if(action.chaActionType == CharacterActionType.ChaActionTypeSkill)
                {
                    totalTime += 8f;
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
                //只要行动了就增加一点MP
                if(action.chaActionType != CharacterActionType.ChaActionTypeSkill &&
                room.players[playerID].battlePlayer.characters[characterID].CurMP < room.players[playerID].battlePlayer.characters[characterID].data.MP)
                {
                    BattleEvent mpIncrease = BattleUtil.GenerateAttributeChangeEvent(characterID, EffectType.MP, 1);
                    AddNewEvents(mpIncrease);
                    room.players[playerID].battlePlayer.characters[characterID].CurMP += 1;
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
                            m_DefendCharacters.Add(characterID);
                        }
                        break;
                    case CharacterActionType.ChaActionTypeAttack:
                        {
                            ExecuteAttack(characterID, action.Target.Id, AttackOrigin.Player);
                        }
                        break;
                    case CharacterActionType.ChaActionTypeSkill:
                        {
                            if (!room.characterManager.GetCharacterData(characterID, out var data))
                            {
                                continue;
                            }
                            if (!DataManager.Instance.m_Skills.TryGetValue(data.SkillID, out var skill))
                            {
                                continue;
                            }
                            switch(skill.TargetType)
                            {
                                case SkillTargetType.SelectEnemy:
                                    {
                                        HandleSelectTargetSkill(action, skill);
                                        break;
                                    }
                                case SkillTargetType.SelectAlly:
                                    {
                                        HandleSelectTargetSkill(action, skill);
                                        break;
                                    }
                                case SkillTargetType.AllAllies:
                                    {
                                        HandleAllSkill(action, skill);
                                        break;
                                    }
                                case SkillTargetType.AllEnemies:
                                    {
                                        HandleAllSkill(action, skill);
                                        break;
                                    }
                            }
                            BattleEvent mpIncrease = BattleUtil.GenerateAttributeChangeEvent(characterID, EffectType.MP, -skill.Cost);
                            AddNewEvents(mpIncrease);
                            room.players[playerID].battlePlayer.characters[characterID].CurMP -= skill.Cost;
                            break;
                        }
                }
            }
        }
        private void HandleSelectTargetSkill(BattleAction action, SkillData skill)
        {
            Character attacker = null;
            
            Character target = null;

            room.characterManager.GetCharacter(action.Source.Id,out attacker);

            if(!room.characterManager.GetCharacter(action.Target.Id,out target))
            {
                return;
            }

            //首先播放技能动画
            BattleEvent playSkill = new BattleEvent();
            playSkill.actionID = target.ID;
            playSkill.Id = attacker.ID;
            playSkill.Type = BattleEvent.EventType.EventCharacterSkill;

            AddNewEvents(playSkill);

            //之后根据技能类型具体操作
            m_SkillHandlers[skill.Type].ReHandle();
            for(int i = 1; i <= skill.HitCount; i++)
            {
                m_SkillHandlers[skill.Type].HandleSkill(attacker, target, skill, i);
                if(target.isDefend)
                {
                    AttackBeDefended(attacker);
                    DefendSuccess(target, i);
                }
                
            }
            if (target.CurHP <= 0)//完成技能流程后判断是否死亡
            {
                CharacterDie(target, skill.HitCount);
            }
        }
        private void HandleAllSkill(BattleAction action,SkillData skill)
        {
            Character attacker = null;

            room.characterManager.GetCharacter(action.Source.Id, out attacker);
            room.characterManager.GetCharacterOwner(action.Source.Id, out var player);

            //首先播放技能动画
            BattleEvent playSkill = new BattleEvent();
            playSkill.actionID = -1;
            playSkill.Id = attacker.ID;
            playSkill.Type = BattleEvent.EventType.EventCharacterSkill;

            AddNewEvents(playSkill);

            //之后根据技能类型具体操作
            m_SkillHandlers[skill.Type].ReHandle();
            var targets = skill.TargetType == SkillTargetType.AllAllies? room.players[player].battlePlayer.characters 
                                              : room.players[room.GetEnemyID(player)].battlePlayer.characters;
            for (int i = 1; i <= skill.HitCount; i++)
            {
                foreach(var targetKv in targets)
                {
                    Character target = targetKv.Value;
                    m_SkillHandlers[skill.Type].HandleSkill(attacker, target, skill, i);
                    if (target.isDefend)
                    {
                        AttackBeDefended(attacker);
                        DefendSuccess(target, i);
                    }
                }

            }
            foreach (var targetKv in targets)
            {
                Character target = targetKv.Value;
                if (target.CurHP <= 0)//完成技能流程后判断是否死亡
                {
                    CharacterDie(target, skill.HitCount);
                }
            }
        }
        private bool ExecuteAttack(int attackerID, int targetID, AttackOrigin origin, int specialAttack = -100)
        {
            // 每次真正执行时重新获取，不能沿用排队时的对象状态
            if (!room.characterManager.GetCharacter(attackerID, out var attacker) || attacker == null || attacker.CurHP <= 0)
                return false;

            if (!room.characterManager.GetCharacter(targetID, out var target) || target == null || target.CurHP <= 0)
                return false;

            room.characterManager.GetCharacterData(attackerID, out var attackerData);
            if(origin != AttackOrigin.Reflect)
            {
                //首先播放攻击
                BattleEvent playAttack = new BattleEvent();
                //协议有点少，因为这是系统自动生成的不需要actionid，所以把actionid当成被攻击对象的id
                playAttack.actionID = targetID;
                playAttack.Id = attackerID;
                playAttack.Type = BattleEvent.EventType.EventCharacterAttack;

                AddNewEvents(playAttack);
                if (origin == AttackOrigin.FollowUp)
                {
                    maxTime += 5f;
                    timeLeft += 5f;
                }
            }

            if(origin == AttackOrigin.Reflect)
            {
                int damage = specialAttack;
                damage = Math.Min(damage, target.CurHP);

                BattleEvent changeHP = BattleUtil.GenerateAttributeChangeEvent(target.ID, EffectType.Health, -damage);
                AddNewEvents(changeHP);
                target.CurHP -= damage;
                if (target.CurHP < 0)
                {
                    target.CurHP = 0;
                }

                //如果target死亡则remove
                if (target.CurHP <= 0)
                {
                    CharacterDie(target);
                }
                return true;
            }

            if (attackerData.CharacterType == CharacterType.Attack)
            {

                int damage = BattleUtil.CalculateDamage(attacker, target, out var defendDecrease);

                if (defendDecrease > 0)
                {
                    BattleEvent changeDef = BattleUtil.GenerateAttributeChangeEvent(target.ID, EffectType.Defence, -defendDecrease);
                    AddNewEvents(changeDef);
                    target.CurDEF -= defendDecrease;
                }

                if (damage > 0)
                {
                    damage = Math.Min(damage, target.CurHP);
                    BattleEvent changeHP = BattleUtil.GenerateAttributeChangeEvent(target.ID, EffectType.Health, -damage);
                    AddNewEvents(changeHP);
                    target.CurHP -= damage;
                    if (target.CurHP < 0)
                    {
                        target.CurHP = 0;
                    }
                }

                if (target.isDefend)
                {
                    AttackBeDefended(attacker);
                    DefendSuccess(target);
                }
                //触发受击特殊效果
                NotifyAfterDamage(attacker, target, origin);

                //如果target死亡则remove
                if (target.CurHP <= 0)
                {
                    CharacterDie(target);
                }
                //触发攻击效果
                NotifyAfterAttack(attacker, target, origin);
            }
            else
            {
                int healNum = BattleUtil.CalculateHealNum(attacker, target);
                if (healNum > 0)
                {
                    BattleEvent increaseHP = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Health, healNum);
                    AddNewEvents(increaseHP);
                    target.CurHP += healNum;
                }

            }
            return true;
        }
        private void CharacterDie(Character target, int battleGroup = -1)
        {
            BattleEvent targetDie = new BattleEvent();
            room.characterManager.GetCharacterOwner(target.ID, out var targetPlayer);

            targetDie.Type = BattleEvent.EventType.EventCharacterDie;
            targetDie.Id = target.ID;
            targetDie.Param = 1;
            targetDie.paramS = targetPlayer;
            targetDie.actionID = -1;
            targetDie.battleGroup = battleGroup;
            AddNewEvents(targetDie);

            NotifyDeath(target, battleGroup);

            room.characterManager.RemoveCharacter(targetPlayer, target.ID);

        }
        public void AddNewEvents(BattleEvent e)
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
                    BattleCharacterSnapshot snapshot = BattleUtil.GenerateCharacterSnapShot(player.Value, character.Value);
                    snapshot.canDefend = room.characterManager.CanCharacterDefend(character.Value.ID);
                    snapshot.canAttack = room.characterManager.CanCharacterAttack(character.Value.ID);
                    snapshot.canUseSkill = room.characterManager.CanCharacterUseSkill(character.Value.ID);

                    record.BeforeCharacters.Add(snapshot);
                }
            }
            m_records.Add(record);
            m_processEvents.Add(e);
        }
        private void AttackBeDefended(Character attacker)
        {
            room.characterManager.CharacterCantAttack(attacker.ID);
            room.characterManager.CharacterCantSkill(attacker.ID);
        }
        private void DefendSuccess(Character character, int battleGroup = -1)//格挡成功
        {
            if (character.CurMP >= character.data.MP) return;

            //格挡成功增加MP
            BattleEvent mpIncrease = BattleUtil.GenerateAttributeChangeEvent(character.ID, EffectType.MP, 1);
            if(battleGroup != -1)mpIncrease.battleGroup = battleGroup;

            AddNewEvents(mpIncrease);
            character.CurMP += 1;
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

        #region 特殊效果广播区域
        public void NotifyAfterAttack(Character attacker, Character target, AttackOrigin origin)
        {
            if (!room.characterManager.GetBattleCharacter(attacker.ID, out var attackerCha) || attackerCha == null)
                return;

            var context = new AfterAttackContext
            {
                targetID = target.ID,
                isTargetDefended = target.isDefend,
                origin = origin,
                ExecuteAttack = id => ExecuteAttack(attacker.ID, id, AttackOrigin.FollowUp)
            };

            attackerCha.ApplyAfterAttackEffects(context);
        }
        //触发受击
        public void NotifyAfterDamage(Character attacker, Character target, AttackOrigin origion, int battleGroup = -1)
        {
            if (!room.characterManager.GetBattleCharacter(target.ID, out var targetCha) || targetCha == null)
                return;

            room.characterManager.GetBattleCharacter(attacker.ID, out var attackerCha);

            var context = new AfterDamageContext
            {
                attacker = attackerCha,
                isDefended = target.isDefend,
                battleGroup = battleGroup,
                AddEvent = AddNewEvents,
                origin = origion,
                ExecuteAttack = (origin, damage) => ExecuteAttack(target.ID, attacker.ID, origin, damage)
            };

            targetCha.ApplyAfterDamageEffects(context);
        }
        //触发死亡
        private void NotifyDeath(Character deather, int battleGroup = -1)
        {
            if (!room.characterManager.GetBattleCharacter(deather.ID, out var targetCha) || targetCha == null)
                return;

            var context = new DeathContext
            {
                allies = room.characterManager.GetPlayerCharacters(deather.ID),
                battleGroup = battleGroup,
                AddEvent = AddNewEvents
            };

           targetCha.ApplyDeathEffects(context);
        }
        #endregion
    }
}
