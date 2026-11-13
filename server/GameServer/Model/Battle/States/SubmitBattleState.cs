using Common.Data;
using Common.Model;
using GameServer.Manager;
using GameServer.Utils;
using Microsoft.EntityFrameworkCore.Query;
using NetMessage;
using System;
using System.Net.Http.Headers;


namespace GameServer.Model.Battle.States
{
    public class SubmitBattleState : IState,ITimedState, IResumeState,IBattleActionHandleState
    {
        protected BattleRoom room;
        public BattleState stateType { get; }

        public float timeLeft { get; set; }
        private float maxTime = 20;

        public SubmitBattleState(BattleRoom room)
        {
            stateType = BattleState.SubmitBattle;
            this.room = room;
        }
        public void Enter()
        {
            timeLeft = maxTime;
        }
        public void Resume()
        {
            ;
        }
        public void Update()
        {
            timeLeft -= Time.deltaTime;
            if(timeLeft <=0)
            {
                //TODO：切换到战斗阶段
                room.ChangeState(BattleState.Battle);
                return;
            }
            //HandleBattleAction();
        }
        public bool HandleBattleAction(BattleActonContext ctx)
        {
            BattleAction action = ctx.action;
            if(HandleAction(ctx, out var success))//返回的结果：是否立即反馈行动 /success：是否成功（判断立即反馈是否成功）
            {
                if(!success)//是立即反馈的行动但失败了
                {
                    return false;
                }
            }
            else//说明是不是实时反馈的行动
            {
                if(CheckBattleAction(ctx))
                {
                    room.pendingBattleActions[(ctx.playerID, action.Source.Id)] = action;
                }
                else
                {
                    return false;
                }
            }
            return true;

        }
        /// <summary>
        /// 处理行动阶段发送的BattleAction
        /// </summary>
        private bool HandleAction(BattleActonContext ctx, out bool success)
        {
            bool res = true;
            bool result = true;
            string player = ctx.playerID;
            BattleAction action = ctx.action;

            switch (action.Type)
            {
                case BattleActionType.TypeDropCard:
                    {
                        res = true;
                        if (action.dropCardInfo == null)
                        {
                            result = false;
                            break;
                        }
                        int cardID = action.dropCardInfo.Id;
                        if (room.players[player].battlePlayer.cards.ContainsKey(cardID))
                        {
                            Console.WriteLine($"battle room {room.Id} : player {player} remove card {cardID} successfully");
                            room.cardManager.RemoveCard(player, cardID, action.actionID, true);
                            room.cardManager.AddPlayerSendCardNum(player, 1);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    break;
                case BattleActionType.TypePlaceCharacter:
                    {
                        res = true;
                        if (action.Source == null)
                        {
                            result = false;
                            break;
                        }
                        int cardID = action.Source.Id;
                        if (room.players[player].battlePlayer.cards.TryGetValue(cardID, out var card))
                        {
                            Console.WriteLine($"battle room {room.Id} : player {player} use card {cardID} successfully");
                            room.cardManager.RemoveCard(player, cardID, action.actionID);

                            if (action.Target == null) action.Target = new ActionTarget();
                            action.Target.dataID = card.Data.TargetID[0];

                            room.characterManager.SpawnCharacter(player, action);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    break;
                case BattleActionType.TypeCharacterRemove:
                    {
                        res = true;
                        if (action.Source == null)
                        {
                            result = false;
                            break;
                        }
                        int characterID = action.Source.Id;
                        if (room.players[player].battlePlayer.characters.ContainsKey(characterID))
                        {
                            Console.WriteLine($"battle room {room.Id} : player {player} remove character {characterID} successfully");
                            room.characterManager.RemoveCharacter(player, characterID, action.actionID, false);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    break;
                case BattleActionType.TypeChangeAttribute:
                    {
                        res = true;
                        if (action.Source == null || action.Target == null)
                        {
                            result = false;
                            break;
                        }
                        int cardID = action.Source.Id;
                        int characterID = action.Target.Id;
                        if (room.players[player].battlePlayer.cards.ContainsKey(cardID) && room.players[player].battlePlayer.characters.ContainsKey(characterID))
                        {
                            Character target = room.players[player].battlePlayer.characters[characterID];
                            Console.WriteLine($"battle room {room.Id} : player {player} use card {cardID} successfully");

                            foreach (var effectID in room.players[player].battlePlayer.cards[cardID].Data.TargetID)
                            {
                                if(!DataManager.Instance.m_buffs.TryGetValue(effectID, out var effect))
                                {
                                    Console.WriteLine($"battle room {room.Id} : player {player} use card {cardID} effect {effectID} not exist");
                                    continue;
                                }
                                room.effectManager.ChangeCharacterAttribute(target, effect);
                            }

                            room.cardManager.RemoveCard(player, cardID, action.actionID);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    break;
                case BattleActionType.TypeCharacterAction:
                    res = false;
                    break;
            }
            success = result;
            return res;
        }
        private bool CheckBattleAction(BattleActonContext ctx)
        {
            string playerID = ctx.playerID;

            BattleAction action = ctx.action;

            if (action.Source == null) return false;

            switch(action.chaActionType)
            {
                case CharacterActionType.ChaActionTypeNone:
                    {
                        
                        int characterID = action.Source.Id;
                        Console.WriteLine($"battle room {room.Id} : character {characterID} send cancel action request");
                        if(room.pendingBattleActions.ContainsKey((playerID, characterID)))
                        {
                            room.pendingBattleActions.Remove((playerID, characterID));
                            return true;
                        }
                        return false;
                    }
                case CharacterActionType.ChaActionTypeDefend:
                    {
                        int characterID = action.Source.Id;
                        if (!room.players[playerID].battlePlayer.characters.ContainsKey(characterID))
                        {
                            return false;
                        }
                        if (!room.characterManager.CanCharacterDefend(characterID)) return false;
                        Console.WriteLine($"battle room {room.Id} : character {characterID} send defend request");
                    }
                    break;
                case CharacterActionType.ChaActionTypeAttack:
                    {
                        int attackerID = action.Source.Id;

                        if(action.Target == null) return false;

                        int targetID = action.Target.Id;

                        if (!room.players[playerID].battlePlayer.characters.ContainsKey(attackerID))
                        {
                            return false;
                        }
                        if (!room.characterManager.GetCharacterOwner(targetID, out var targetOwnerID)
                        || !room.characterManager.GetCharacterOwner(attackerID, out var attackerOwnerID))
                        {
                            return false;
                        }

                        if (!room.characterManager.GetCharacterData(attackerID, out var data))
                        {
                            return false;
                        }

                        if (data.CharacterType == CharacterType.Attack)
                        {
                            if (attackerOwnerID == targetOwnerID) return false;
                            if (targetID == room.players[targetOwnerID].battlePlayer.mainCharacterID &&
                                   room.players[targetOwnerID].battlePlayer.characters.Count > 1)
                            {
                                return false;
                            }

                        }
                        else
                        {
                            if (attackerOwnerID != targetOwnerID) return false;
                        }

                        Console.WriteLine($"battle room {room.Id} : character {attackerID} send attack to {targetID} request");
                    }
                    break;
                case CharacterActionType.ChaActionTypeSkill:
                    {
                        int characterID = action.Source.Id;
                        if(!room.characterManager.GetCharacterData(characterID, out var data))
                        {
                            return false;
                        }
                        if(!DataManager.Instance.m_Skills.TryGetValue(data.SkillID, out var skill))
                        {
                            return false;
                        }
                        return HandleSkillRequest(action, skill);
                    }
            }
            return true;
        }
        private bool HandleSkillRequest(BattleAction action, SkillData skill)
        {
            if(!room.characterManager.GetCharacterOwner(action.Source.Id, out var sourceOwner)) return false;
            Console.WriteLine($"battle room {room.Id} : character{action.Source.Id} try to use skill");

            if(!room.characterManager.CanCharacterUseSkill(action.Source.Id)) return false;

            switch (skill.TargetType)
            {
                case SkillTargetType.Self:
                    {
                        return true;
                    }
                case SkillTargetType.SelectEnemy:
                    {
                        if(action.Target == null)return false;

                        if (!room.characterManager.GetCharacterOwner(action.Target.Id, out var targetOwner)) return false;

                        if(sourceOwner == targetOwner) return false;

                        return true;
                    }

                case SkillTargetType.SelectAlly:
                    {
                        if (action.Target == null) return false;

                        if (!room.characterManager.GetCharacterOwner(action.Target.Id, out var targetOwner)) return false;

                        if (sourceOwner != targetOwner) return false;

                        return true;
                    }
                case SkillTargetType.AllEnemies:
                    {
                        return true;
                    }
                case SkillTargetType.AllAllies:
                    {
                        return true;
                    }
            }
            return false;
        }
        public void Exit()
        {

        }
    }
}
