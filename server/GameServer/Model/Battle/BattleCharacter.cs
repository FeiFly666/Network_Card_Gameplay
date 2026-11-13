using Common.Model;
using GameServer.Manager;
using GameServer.Model.Battle.BattleCharaccterEffects;
using GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts;
using GameServer.Model.Battle.BattleInterface.IBattleCharacterEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle
{
    public class BattleCharacter
    {
        public Character character;

        public int characterSpawnRound;

        private List<IOnDeath> deathEffects = new List<IOnDeath>();
        private List<IAfterDamage> afterDamageEffects = new List<IAfterDamage>();
        private List<IAfterAttack> afterAttackEffects = new List<IAfterAttack>();

        private List<string> specialEffects = new List<string>();
        public void InitBattleCharacter(Player owner, Character cha, int spawnRound)
        {
            this.character = cha;
            this.characterSpawnRound = spawnRound;

            BattleChaSpecialEffectFactory.EffectBuild(owner, this);
        }
        #region 被动相关
        public void AddDeathEffect(IOnDeath newEffect, string effectID)
        {
            if(!specialEffects.Contains(effectID))
            {
                deathEffects.Add(newEffect);
                specialEffects.Add(effectID);
            }
        }
        public void AddAfterDamageEffect(IAfterDamage newEffect, string effectID)
        {
            if(!specialEffects.Contains(effectID))
            {
                afterDamageEffects.Add(newEffect);
                specialEffects.Add(effectID);
            }
        }
        public void AddAfterAttackEffect(IAfterAttack newEffect, string effectID)
        {
            if(!specialEffects.Contains(effectID))
            {
                afterAttackEffects.Add(newEffect);
                specialEffects.Add(effectID);
            }
        }

        public void ApplyDeathEffects(DeathContext args)
        {
            foreach(var deathEffect in  deathEffects)
            {
                deathEffect?.OnDeath(args);
            }

        }
        public void ApplyAfterDamageEffects(AfterDamageContext args)
        {
            foreach(var afterDamageEffect in afterDamageEffects)
            {
                afterDamageEffect?.OnAfterDamage(args);
            }
        }
        public void ApplyAfterAttackEffects(AfterAttackContext args)
        {
            foreach(var afterAttackEffect in afterAttackEffects)
            {
                afterAttackEffect?.OnAfterAttack(args);
            }
        }
        #endregion
    }
}
