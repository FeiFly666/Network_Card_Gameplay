using Common.Model;
using NetMessage;


namespace GameServer.Model.Battle.States
{
    public interface IBattleActionHandleState
    {
        bool HandleBattleAction(BattleActonContext action);
    }
}
