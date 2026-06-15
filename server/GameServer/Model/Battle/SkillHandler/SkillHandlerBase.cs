using Common.Data;
using Common.Model;
using GameServer.Model.Battle.States;
using NetMessage;

namespace GameServer.Model.Battle.SkillHandler
{
    public abstract class SkillHandlerBase
    {
        protected BattlingState owner;
        public SkillHandlerBase(BattlingState state)
        {
            owner = state;
        }
        public abstract void HandleSkill(Character attacker, Character target, SkillData skillData, int hitGroup);
        
        protected void AddNewEvent(BattleEvent e)
        {
            owner?.AddNewEvents(e);
        }
        public void ReHandle()
        {
            
        }
    }
}
