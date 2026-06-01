using GameServer.Model.Battle.States;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle
{
    public class BattleStateMachine
    {
        public BattleRoom room;
        private IState m_currentState;
        private IState m_previousState;

        private IState pendingState;
        private bool pendingReconnect;
        private Dictionary<BattleState, IState> m_states = new Dictionary<BattleState, IState>();

        public IState CurrentState
        {
            get { return m_currentState; }
        }
        public IState PreviousState
        {
            get { return m_previousState; }
        }
        public BattleStateMachine(BattleRoom room)
        {
            this.room = room;

            m_states[BattleState.Loading] = new LoadingState(room);
            m_states[BattleState.SubmitBattle]  = new SubmitBattleState(room);
            m_states[BattleState.Battle] = new BattlingState(room);
            m_states[BattleState.RoundEnd] = new RoundEndState(room);

            room.syncSystem.OnAllPlayerSync += ApplyState;
        }
        public void InitState(BattleState inital)
        {
            m_currentState = m_states[inital];
            m_currentState.Enter();
        }

        public IState GetState(BattleState stateType)
        {
            return m_states[stateType];
        }

        public void ChangeState(BattleState newState, bool reconnect = false)
        {

            if(CurrentState != null && CurrentState.stateType == newState) return;

            Console.WriteLine($"battle room {room.Id} : try enter new state {newState}");

            if (room.syncSystem.isWaitingStateSync && newState != BattleState.Loading)
            {
                return;
            }

            pendingState = m_states[newState];

            pendingReconnect = reconnect;

            room.syncSystem.RequestChangeState(newState);
        }
        private void ApplyState()
        {
            CurrentState?.Exit();

            Console.WriteLine($"battle room {room.Id} : enter new state {pendingState.stateType} sucessfully");

            m_previousState = m_currentState;
            m_currentState = pendingState;

            if ((CurrentState != null && CurrentState is IResumeState state))
            {
                if (!pendingReconnect)
                    CurrentState?.Enter();
                else
                    state.Resume();
            }
            else
            {
                CurrentState?.Enter();
            }
        }
        public void Update()
        {
            CurrentState?.Update();
        }
    }
}
