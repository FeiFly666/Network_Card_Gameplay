using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.States
{
    public interface IState
    {
        public BattleState stateType { get; }

        public void Enter();
        public void Update();
        public void Exit();
    }
}
