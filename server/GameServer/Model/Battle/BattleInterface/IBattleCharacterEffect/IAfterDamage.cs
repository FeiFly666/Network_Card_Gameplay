using Common.Model;
using GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.BattleInterface.IBattleCharacterEffect
{
    public interface IAfterDamage
    {
        void OnAfterDamage(AfterDamageContext context);
    }
}
