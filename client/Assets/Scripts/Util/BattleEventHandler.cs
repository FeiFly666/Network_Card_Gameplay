using Assets.Scripts.Manager;
using Common.Data;
using Common.Model;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class BattleEventHandler
    {
        public static void HandleBattleEvent(BattleEvent e)
        {
            switch (e.Type)
            {
                case BattleEvent.EventType.EventDrawCard:
                    HandleDrawCardEvent(e);
                    break;
                case BattleEvent.EventType.EventDropCard:
                    HandleDropCardEvent(e);
                    break;
                case BattleEvent.EventType.EventSpawnCharacter:
                    HandleCreateCharacterEvent(e);
                    break;
                case BattleEvent.EventType.EventCharacterDie:
                    HandelRemoveCharacterEvent(e);
                    break;
                case BattleEvent.EventType.EventCharacterActioncooldown:
                    HandleCharacterActionCoolDown(e);
                    break;
                case BattleEvent.EventType.EventCharacterAttributechange:
                    HandleCharacterAttributeChange(e);
                    break;
            }
        }
        private static void HandleDrawCardEvent(BattleEvent e)
        {
            Card newCard = new Card();
            if (e.Id == -1)//对手的牌更新
            {
                string enemyID = e.dataID;

                BattleCardManager.Instance.SetNewCard(enemyID);

                return;
            }

            newCard.ID = e.Id;
            newCard.Data = null;

            if (!DataManager.Instance.cardData.TryGetValue(e.dataID, out var Data))
            {
                return;
            }

            newCard.Data = Data;
            BattleCardManager.Instance.SetNewCard(PlayerInfoManager.Instance.userID, newCard);
        }
        private static void HandleDropCardEvent(BattleEvent e)
        {
            BattleCardManager.Instance.RemoveCard(e.Id, e.Param == 1);
        }
        private static void HandleCreateCharacterEvent(BattleEvent e)
        {
            Character newCha = new Character();

            newCha.ID = e.Id;
            if (!DataManager.Instance.characterData.TryGetValue(e.dataID, out var Data))
            {
                return;
            }
            newCha.data = Data;
            BattleCharacterManager.Instance.AddNewCharacter(newCha, e);
        }
        private static void HandelRemoveCharacterEvent(BattleEvent e)
        {
            BattleCharacterManager.Instance.RemoveCharacter(e.Id, e.paramS, e.Param == 1);
        }
        private static void HandleCharacterActionCoolDown(BattleEvent e)
        {
            switch(e.actionCoolDownType)
            {
                case CharacterActionCoolDownType.ChaActionCooldownDefend:
                    BattleCharacterManager.Instance.HandleCharacterDefendCoolDownSync(e.Id, e.Param == 1);
                    break;
                case CharacterActionCoolDownType.ChaActionCooldownAttack:
                    BattleCharacterManager.Instance.HandleCharacterAttackCoolDownSync(e.Id, e.Param == 1);
                    break;
                case CharacterActionCoolDownType.ChaActionCooldownSkill:
                    BattleCharacterManager.Instance.HandleCharacterSkillCoolDownSync(e.Id, e.Param == 1);
                    break;
            }
        }
        private static void HandleCharacterAttributeChange(BattleEvent e)
        {
            BuffData effectData = DataManager.Instance.effectData[e.paramS];
            BattleCharacterManager.Instance.HandleCharacterAttributeChange(e.Id, effectData.EffectType, e.Param);
        }
    }
}
