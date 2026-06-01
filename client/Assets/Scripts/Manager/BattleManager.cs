using Assets.Scripts.Service;
using Assets.Scripts.Util;
using Common;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    class BattleManager : Singleton<BattleManager>
    {
        private int currentRound;
        private float leftTime;
        private BattlePhase currentPhase;
        public BattlePhase CurrentPhase { get { return currentPhase; } }
        public Action<int> OnRoundChanged;
        public Action<int, float, BattlePhase> OnRoundSync;
        public Action OnSubmitPhaseChanged;
        public Action OnBattlePhaseChanged;

        public Queue<BattleEvent> eventQueue = new Queue<BattleEvent>();
        
        public void Init()
        {
            currentPhase = BattlePhase.PhaseNone;
            currentRound = 0;

            eventQueue.Clear();
        }
        public void SetRoundInfo(BattleRoundStartBroadcast broadcast)
        {
            currentRound = broadcast.Round;

            OnRoundChanged?.Invoke(currentRound);
        }

        public void SetRoundInfo(BattleStateSync sync)
        {
            BattlePhase lastPhase = currentPhase;

            currentRound = sync.Round;

            leftTime = sync.remainTime;

            if(currentPhase != sync.State)
            {
                currentPhase = sync.State;
                if (lastPhase == BattlePhase.PhasePrepare)
                {
                    OnSubmitPhaseChanged?.Invoke();
                }
                else if(lastPhase == BattlePhase.PhaseAction)
                {
                    OnBattlePhaseChanged?.Invoke();
                }
            }
            if(sync.needResponse)
                BattleService.Instance.SendStateSyncBack();

            OnRoundSync?.Invoke(currentRound, leftTime, currentPhase);
        }

        public void RegistBattlingEvent(List<BattleEvent> es)
        {
            foreach(BattleEvent e in es)
            {
                if(IsBattlingEvent(e))
                {
                    eventQueue.Enqueue(e);
                }
                else
                {
                    BattleEventHandler.HandleBattleEvent(e);
                }
            }

            BattleEventPlayer.Instance.StartPlaying();
            
        }
        private bool IsBattlingEvent(BattleEvent e)
        {
            return e.Type != BattleEvent.EventType.EventDropCard && e.Type != BattleEvent.EventType.EventDrawCard;
        }

        public void ApplyReconnectSnapshot(BattleReconnectSnapshot snapshot)
        {
            Init();
            BattleCardManager.Instance.Init();
            BattleCharacterManager.Instance.Init();

            currentRound = snapshot.Round;
            currentPhase = snapshot.Phase;

            OnRoundChanged?.Invoke(currentRound);
            OnRoundSync?.Invoke(currentRound, snapshot.remainTime, currentPhase);

            BattleCharacterManager.Instance.ApplySnapshot(snapshot);
            BattleCardManager.Instance.ApplySnapshot(snapshot);
        }
    }
}
