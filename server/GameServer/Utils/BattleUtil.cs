using Common.Data;
using Common.Model;
using GameServer.Manager;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Utils
{
    public static class BattleUtil
    {
        public static Card CreateNewCard(string cardData)
        {
            Card card = new Card();

            CardData data = null;
            if (!DataManager.Instance.m_cards.TryGetValue(cardData, out data))
            {
                return null;
            }
            card.Data = data;
            card.ID = -1;

            return card;
        }
        public static Character CreateNewCharacter(string characterData)
        {
            Character character = new Character();
            CharacterData data = null;
            if(!DataManager.Instance.m_characters.TryGetValue(characterData, out data))
            {
                return null;
            }
            character.data = data;
            character.ID = -1;

            return character;
        }
        public static int GetActionPriority(CharacterActionType actionType)
        {
            switch (actionType)
            {
                case CharacterActionType.ChaActionTypeSkill: return 1;
                case CharacterActionType.ChaActionTypeAttack: return 1; 
                case CharacterActionType.ChaActionTypeDefend: return 0;

            }
            return -1;
        }

        public static int CalculateDamage(Character attack, Character target, out int defenceDamage)
        {
            int totalDamage = attack.CurATK;

            int defendDecrease = Math.Max(0, target.CurDEF);

            defendDecrease = Math.Min(totalDamage, defendDecrease);

            totalDamage = Math.Max(totalDamage - defendDecrease, 0);

            if (target.isDefend)
            {
                defendDecrease = 0;
            }
            defenceDamage = defendDecrease;

            return totalDamage;
        }
        public static int CalculateSkillDamage(Character attack, Character target, out int defenceDamage)
        {
            DataManager.Instance.m_Skills.TryGetValue(attack.data.SkillID, out var skill);
            int totalDamage = attack.CurATK + skill.Param;

            int defendDecrease = Math.Max(0, target.CurDEF);

            defendDecrease = Math.Min(totalDamage, defendDecrease);

            totalDamage = Math.Max(totalDamage - defendDecrease, 0);

            if (target.isDefend)
            {
                defendDecrease = 0;
            }
            defenceDamage = defendDecrease;

            return totalDamage;
        }

        public static int CalculateHealNum(Character attack, Character target)
        {
            int totalHealNum = attack.CurATK;

            int damagedNum = target.CurMaxHP - target.CurHP;

            totalHealNum = Math.Min(totalHealNum, damagedNum);

            return totalHealNum;
        }
        public static int CalculateSkillHealNum(Character attack, Character target)
        {
            DataManager.Instance.m_Skills.TryGetValue(attack.data.SkillID, out var skill);
            int totalHealNum = attack.CurATK + skill.Param;

            int damagedNum = target.CurMaxHP - target.CurHP;

            totalHealNum = Math.Min(totalHealNum, damagedNum);

            return totalHealNum;
        }

        public static BattleEvent GenerateAttributeChangeEvent(int characterID, EffectType type, int param)
        {
            BattleEvent e = new BattleEvent();
            e.Type = BattleEvent.EventType.EventCharacterAttributechange;
            e.effectType = (NEffectType)type;
            Console.WriteLine($"genearte attribute event : type :{type} change to Ntype {e.effectType}");
            e.Id = characterID;
            e.actionID = -1;
            e.Param = param;

            return e;
        }

        public static BattleCharacterSnapshot GenerateCharacterSnapShot(Player player, Character character)
        {
            BattleCharacterSnapshot snapshot = new BattleCharacterSnapshot();

            snapshot.Id = character.ID;
            snapshot.dataID = character.data.DataID;

            snapshot.maxHp = character.CurMaxHP;
            snapshot.Hp = character.CurHP;
            snapshot.Atk = character.CurATK;
            snapshot.Def = character.CurDEF;
            snapshot.Speed = character.CurSpeed;
            snapshot.Mp = character.CurMP;

            snapshot.isMain = character.isMain;
            snapshot.ownerID = player.id;

            return snapshot;
        }
        
    }
}
