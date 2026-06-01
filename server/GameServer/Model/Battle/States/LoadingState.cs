using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.States
{
    public class LoadingState : IState,ITimedState
    {
        protected BattleRoom room;
        public BattleState stateType { get;}
        public float timeLeft { get; set; }

        private float maxLoadingTime = 30f;
        public LoadingState(BattleRoom room)
        {
            stateType = BattleState.Loading;
            this.room = room;
        }
        public void Enter()
        {
            timeLeft = maxLoadingTime;
        }
        public void Resume()
        {

        }
        public void Update()
        {
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0)
            {
                //room.DisbandRoom();
                room.HandleLoadTimedOut();
                return;
            }
        }
        public void Exit()
        {

        }
    }
}
