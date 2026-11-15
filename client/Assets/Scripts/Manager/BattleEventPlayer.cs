using Assets.Scripts.Util;
using Common.Data;
using NetMessage;
using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Manager
{
    public class BattleEventPlayer : MonoBehaviour
    {
        public static BattleEventPlayer Instance;
        private bool isPlaying = false;

        private Message playFinished;
        private Message playOneEventFinished;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                playFinished = new Message();
                playFinished.Request = new RequestMessage();
                playFinished.Request.battlePlayFinish = new BattlePlayingFinishRequest();

                playOneEventFinished = new Message();
                playOneEventFinished.Request = new RequestMessage();
                playOneEventFinished.Request.eventPlayFinish = new BattleEventPlayFinishRequest();
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }
        public void StartPlaying()
        {
            if (!isPlaying)
            {
                StartCoroutine(PlayQueue());
            }

        }
        IEnumerator PlayQueue()
        {
            Debug.Log("开始播放事件");
            isPlaying = true;
            while (BattleManager.Instance.eventQueue.Count > 0)
            {
                BattleEvent e = BattleManager.Instance.eventQueue.Dequeue();
                yield return PlayEvent(e);
            }

            isPlaying = false;

            NetClient.Instance.Send(playFinished);
        }
        IEnumerator PlayEvent(BattleEvent e)
        {
            switch (e.Type)
            {
                case BattleEvent.EventType.EventCharacterDefend:
                    if (BattleCharacterManager.Instance.TryGetCharacter(e.Id, out var defender))
                    {
                        defender.ShowDefend();
                    }

                    yield return new WaitForSeconds(1f);
                    break;

                case BattleEvent.EventType.EventCharacterAttack:
                    {
                        int attackerID = e.Id;
                        int targetID = e.actionID;

                        if (BattleCharacterManager.Instance.TryGetCharacter(attackerID, out var attacker)
                            && BattleCharacterManager.Instance.TryGetCharacter(targetID, out var target))
                        {
                            BattleAttackHandler handler = attacker.PlayAttack(target);

                            yield return handler.WaitHit(1);
                            if(handler.Cancelled)break;

                            while (BattleManager.Instance.eventQueue.Count > 0 && IsResultEvent(BattleManager.Instance.eventQueue.Peek()))
                            {
                                BattleEvent result = BattleManager.Instance.eventQueue.Dequeue();
                                yield return PlayEvent(result);
                            }

                            yield return handler.WaitFinish();

                        }
                    }
                    break;
                case BattleEvent.EventType.EventCharacterAttributechange:
                    if (BattleCharacterManager.Instance.TryGetCharacter(e.Id, out var hurt))
                    {
                        EffectType effect = (EffectType)e.effectType;
                        BattleCharacterManager.Instance.HandleCharacterAttributeChange(e.Id, effect, e.Param);

                        if(effect == EffectType.Health && e.Param <= 0)
                        {
                            yield return hurt.PlayHit();
                        }
                    }
                    break;
                case BattleEvent.EventType.EventCharacterSkill:
                    {
                        int attackerID = e.Id;
                        
                        if (BattleCharacterManager.Instance.TryGetCharacter(attackerID, out var skiller))
                        {
                            DataManager.Instance.skillData.TryGetValue(skiller.character.data.SkillID, out var skill);
                            BattleCharacterManager.Instance.TryGetCharacter(e.actionID, out var target);
                            if (skiller.ownerID != PlayerInfoManager.Instance.userID)
                            {
                                if (skill.TargetType != Common.Data.SkillTargetType.AllEnemies) skiller.targetPosition = BattleCharacterManager.Instance.enemyCenter;
                                else skiller.targetPosition = BattleCharacterManager.Instance.playerCenter;
                            }
                            BattleAttackHandler handler = skiller.PlaySkill(target);
                            int currentBattleGroup = 1;
                            while(currentBattleGroup <=skiller.skillBattleGroup)
                            {
                                yield return handler.WaitHit(currentBattleGroup);

                                while (BattleManager.Instance.eventQueue.Count > 0 && IsResultEvent(BattleManager.Instance.eventQueue.Peek()) && BattleManager.Instance.eventQueue.Peek().battleGroup == currentBattleGroup)
                                {
                                    BattleEvent currentEvent = BattleManager.Instance.eventQueue.Dequeue();
                                    if(BattleManager.Instance.eventQueue.Count == 0)
                                    {
                                        yield return PlayEvent(currentEvent);
                                        break;
                                    }
                                    BattleEvent nextEvent = BattleManager.Instance.eventQueue.Peek();

                                    if(!IsResultEvent(nextEvent) || nextEvent.battleGroup != currentBattleGroup)
                                    {
                                        yield return PlayEvent(currentEvent);
                                    }
                                    else
                                    {
                                        StartCoroutine(PlayEvent(currentEvent));
                                    }
                                }
                                currentBattleGroup++;
                            }
                            
                            yield return handler.WaitFinish();

                        }
                    }
                    break;
                case BattleEvent.EventType.EventCharacterDie:
                    BattleCharacterManager.Instance.RemoveCharacter(e.Id, e.paramS, e.Param == 1);
                    yield return new WaitForSeconds(1f);
                    break;
                case BattleEvent.EventType.EventSpawnCharacter:
                    BattleEventHandler.HandleBattleEvent(e);
                    yield return new WaitForSeconds(0.01f);
                    break;
                    
            }
            playOneEventFinished.Request.eventPlayFinish.eventID = e.eventID;
            playOneEventFinished.Request.eventPlayFinish.eventBatchID = e.eventBatchID;

            NetClient.Instance.Send(playOneEventFinished);
        }
        private bool IsResultEvent(BattleEvent e)
        {
            return e.Type == BattleEvent.EventType.EventCharacterDie
                || e.Type == BattleEvent.EventType.EventCharacterHpDecrease || e.Type == BattleEvent.EventType.EventCharacterHpIncrease
                || e.Type == BattleEvent.EventType.EventCharacterAddBuff || e.Type == BattleEvent.EventType.EventCharacterRemoveBuff
                || e.Type == BattleEvent.EventType.EventCharacterAttributechange || e.Type == BattleEvent.EventType.EventSpawnCharacter;
        }
    }
}
