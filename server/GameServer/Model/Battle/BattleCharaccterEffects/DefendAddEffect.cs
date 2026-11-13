using Common.Data;
using GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts;
using GameServer.Model.Battle.BattleInterface.IBattleCharacterEffect;
using GameServer.Model.CharacterSpecialEffect;
using GameServer.Utils;


namespace GameServer.Model.Battle.BattleCharaccterEffects
{
    public class DefendAddEffect : IAfterDamage
    {
        private BattleCharacter owner;
        private int addCount;
        private string type;

        public void InitEffect(BattleCharacter cha, DefendAddParams param)
        {
            owner = cha;
            addCount = param.HealAmount;
            type = param.Type;
        }
        public void OnAfterDamage(AfterDamageContext context)
        {
            if(!CanTrigger(context))
            {
                return;
            }

            switch(type)
            {
                case "def":
                    {
                        var e = BattleUtil.GenerateAttributeChangeEvent(owner.character.ID, EffectType.Defence, addCount);
                        e.battleGroup = context.battleGroup;
                        context.AddEvent(e);
                    }
                    owner.character.CurDEF += addCount;
                    break;
                case "mp": 
                    {
                        var e = BattleUtil.GenerateAttributeChangeEvent(owner.character.ID, EffectType.MP, addCount);
                        e.battleGroup = context.battleGroup;
                        context.AddEvent(e);
                    }
                    owner.character.CurMP += addCount;
                    break;
            }
        }
        private bool CanTrigger(AfterDamageContext context)
        {
            return context.isDefended && owner.character.CurHP > 0;
        }
    }
}
