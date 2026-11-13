using Common.Model;
using GameServer.Manager;
using GameServer.Model.CharacterSpecialEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.BattleCharaccterEffects
{
    public static class BattleChaSpecialEffectFactory
    {
        public static void EffectBuild(Player owner, BattleCharacter cha)
        {
            string characterID = cha.character.data.DataID;
            for(int i = 0; i <= 1; i++)
            {
                if(SpecialEffectManager.Instance.GetSlotEffect((owner.id ,characterID, i), out var specialEffectID))
                {
                    if (DataManager.Instance.m_chaSpeicalEffect.TryGetValue(specialEffectID, out var data))
                    {
                        BuildSpecialEffects(cha, data);
                    }
                }
            }

        }
        private static void BuildSpecialEffects(BattleCharacter cha, CharacterSpecialEffectData data)
        {
            switch(data.LogicType)
            {
                case "defend_add"://防御时受击->增加盾值
                    {
                        var args = data.Params.ToObject<DefendAddParams>() ?? throw new InvalidOperationException("防御受击改变数值 -- 参数无效");
                        DefendAddEffect effect = new DefendAddEffect();
                        effect.InitEffect(cha, args);

                        cha.AddAfterDamageEffect(effect, data.DataID);
                        break;
                    }
                case "death_mp_to_hp"://死亡时mp转换为友军hp
                    {
                        DeathMpToHpEffect effect = new DeathMpToHpEffect();
                        effect.InitEffect(cha);

                        cha.AddDeathEffect(effect, data.DataID);
                        break;
                    }
                case "attack_follow_up"://攻击后追加连击 连击次数随攻击次数提升
                    {
                        AttackFollowUpEffect effect = new AttackFollowUpEffect();
                        effect.InitEffect(cha);

                        cha.AddAfterAttackEffect(effect, data.DataID);
                        break;
                    }
                case "damage_reflect": //无视防御值，受击反伤
                    {
                        DamageReflectEffect effect = new DamageReflectEffect();
                        effect.InitEffect(cha);

                        cha.AddAfterDamageEffect(effect, data.DataID);
                        break;
                    }
            }
        }
    }
}
