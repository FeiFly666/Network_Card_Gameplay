using Common.Data;
using Common.Model;
using GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts;
using GameServer.Model.Battle.BattleInterface.IBattleCharacterEffect;
using GameServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.BattleCharaccterEffects
{
    public class DeathMpToHpEffect : IOnDeath
    {
        private BattleCharacter owner;
        public void InitEffect(BattleCharacter cha)
        {
            owner = cha;
        }
        public void OnDeath(DeathContext context)
        {
            if (owner.character.CurMP <= 0) return;
            var candidates = context.allies.Where(ally => ally.ID != owner.character.ID && ally.CurHP > 0).ToList();
            if (candidates == null || candidates.Count() == 0)
                return;


            int index = Random.Shared.Next(candidates.Count);

            Character theHealAlly = candidates[index];

            var e = BattleUtil.GenerateAttributeChangeEvent(theHealAlly.ID, EffectType.Health, owner.character.CurMP);
            e.battleGroup = context.battleGroup;
            context.AddEvent(e);

            theHealAlly.CurHP += owner.character.CurMP;
        }
    }
}
