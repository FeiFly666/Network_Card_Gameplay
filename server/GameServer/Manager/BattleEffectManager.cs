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
        public void ChangeCharacterAttribute(Character character, BuffData effect)
        {
            if (effect.Type != BuffType.Gain) return;

            int param = effect.Param;

            switch (effect.EffectType)
            {
                case EffectType.Health:
                    {
                        character.CurHP += effect.Param;
                        if(character.CurHP > character.CurMaxHP)
                        {
                            param = param - (character.CurHP - character.CurMaxHP);
                            character.CurHP = Math.Min(character.CurMaxHP, character.CurHP);
                        }
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
