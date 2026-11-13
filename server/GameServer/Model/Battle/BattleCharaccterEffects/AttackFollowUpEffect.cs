using GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts;
using GameServer.Model.Battle.BattleInterface.IBattleCharacterEffect;
using GameServer.Model.Battle.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.BattleCharaccterEffects
{
    public class AttackFollowUpEffect : IAfterAttack
    {
        private BattleCharacter owner;
        private int followUpAddNum = 0;
        public void InitEffect(BattleCharacter cha)
        {
            followUpAddNum = 0;
            owner = cha;
        }
        private bool CanTrigger(AfterAttackContext context)
        {
            return context.origin == AttackOrigin.Player;
        }
        public void OnAfterAttack(AfterAttackContext context)
        {
            if (context == null || !CanTrigger(context))
                return;

            for(int i = 1; i <= followUpAddNum; i++)
            {
                if (!context.ExecuteAttack(context.targetID))
                    break;
            }

            followUpAddNum++;

            if (context.isTargetDefended)
            {
                followUpAddNum = 0;
            }
        }
    }
}
