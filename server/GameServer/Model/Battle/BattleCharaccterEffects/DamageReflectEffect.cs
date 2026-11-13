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
    class DamageReflectEffect:IAfterDamage
    {
        private BattleCharacter owner;

        private int damage = 1;

        public void InitEffect(BattleCharacter owner)
        {
            this.owner = owner;
        }
        private bool CanTrigger(AfterDamageContext context)
        {
            return context.attacker != null && context.origin == States.AttackOrigin.Player
                || context.origin == States.AttackOrigin.FollowUp;
        }

        public void OnAfterDamage(AfterDamageContext context)
        {
            if(context == null ||!CanTrigger(context)) return;

            context.ExecuteAttack(States.AttackOrigin.Reflect, damage);
        }
    }
}
