using Common.Data;
using Common.Model;
using GameServer.Model.Battle.States;
using GameServer.Utils;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.SkillHandler
{
    public class ChangeHPHandler : SkillHandlerBase
    {
        public ChangeHPHandler(BattlingState state):base(state)
        {

        }

        public override void HandleSkill(Character attacker, Character target, SkillData skill, int hitGroup)
        {
            int param = skill.Param;
            param *= skill.Type == SkillType.Heal ? 1 : -1;

            //治疗
            if (param >= 0)
            {
                int healNum = BattleUtil.CalculateSkillHealNum(attacker, target);
                if (healNum > 0)
                {
                    BattleEvent increaseHP = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Health, healNum);
                    increaseHP.battleGroup = hitGroup;
                    AddNewEvent(increaseHP);
                    target.CurHP += healNum;
                }
            }
            else//攻击
            {
                int defendDecrease = 0;

                int damage = BattleUtil.CalculateSkillDamage(attacker, target, out defendDecrease);
                if (defendDecrease > 0)
                {
                    BattleEvent changeDef = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Defence, -defendDecrease);
                    changeDef.battleGroup = hitGroup;
                    AddNewEvent(changeDef);
                    target.CurDEF -= defendDecrease;
                }
                if (damage > 0)
                {
                    damage = Math.Min(damage, target.CurHP);

                    BattleEvent changeHP = BattleUtil.GenerateAttributeChangeEvent(target.ID, Common.Data.EffectType.Health, -damage);
                    changeHP.battleGroup = hitGroup;
                    AddNewEvent(changeHP);
                    target.CurHP -= damage;
                    if (target.CurHP < 0)
                    {
                        target.CurHP = 0;
                    }
                }

                owner.NotifyAfterDamage(attacker, target, AttackOrigin.Skill ,hitGroup);
            }
        }
    }
}
