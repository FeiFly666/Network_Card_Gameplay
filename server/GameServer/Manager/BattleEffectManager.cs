using Common.Data;
using Common.Model;
using GameServer.Model;
using GameServer.Utils;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    public class BattleEffectManager
    {
        private BattleRoom room;

        public BattleEffectManager(BattleRoom room)
        {
            this.room = room;
        }
        public void ChangeCharacterAttribute(Character character, BuffData effectData)
        {
            foreach(var effectID in effectData.BuffEffects)
            {
                if (!DataManager.Instance.m_buffEffects.TryGetValue(effectID, out var effect))
                    continue;

                if (effect.ApplyType == ApplyType.Gain)
                    ApplyCharacterAttributeChange(character, effect);


            }

        }
        private void ApplyCharacterAttributeChange(Character character, BuffEffectData effect)
        {
            int param = effect.Param;

            switch (effect.EffectType)
            {
                case EffectType.Health:
                    {
                        if (param > 0)
                        {
                            int trueHealNum = Math.Max(0, character.CurMaxHP - character.CurHP);
                            param = Math.Min(trueHealNum, param);
                        }
                        character.CurHP += param;
                    }
                    break;
                case EffectType.Defence:
                    {
                        character.CurDEF += effect.Param;
                    }
                    break;
                case EffectType.Speed:
                    {
                        character.CurSpeed += effect.Param;
                    }
                    break;
                case EffectType.Power:
                    {
                        character.CurATK += effect.Param;
                    }
                    break;
                case EffectType.MaxHealth:
                    {
                        character.CurMaxHP += effect.Param;
                    }
                    break;
            }

            BattleEvent e = BattleUtil.GenerateAttributeChangeEvent(character.ID, effect.EffectType, param);

            e.paramS = effect.DataID;

            room.messageSystem.BroadCastEvent(e);
        }
    }
}
