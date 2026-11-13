using Assets.Scripts.Manager;
using NetMessage;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class UIBattleRoundInfo : UIWindow
    {
        [SerializeField] private TMP_Text roundNum;
        [SerializeField] private TMP_Text roundLeftNum;
        [SerializeField] private TMP_Text roundPhase;

        public override void OnOpen()
        {
            base.OnOpen();
            BattleManager.Instance.OnRoundChanged += UpdateRoundInfo;
            BattleManager.Instance.OnRoundSync += UpdateRoundInfo;
        }
        private void UpdateRoundInfo(int roundNum)
        {
            this.roundNum.text = $"Round {roundNum}";
        }
        private void UpdateRoundInfo(int roundNum, float leftTime, BattlePhase currentPhase)
        {
            this.roundNum.text = $"Round {roundNum}";

            if (currentPhase == BattlePhase.PhasePrepare)
            {
                this.roundLeftNum.text = $"{leftTime}";
            }
            else
            {
                this.roundLeftNum.text = string.Empty;
            }

            string phase = string.Empty;
            switch (currentPhase)
            {
                case BattlePhase.PhaseNone:
                    phase = "等待玩家连接";
                    break;
                case BattlePhase.PhasePrepare:
                    phase = "行动阶段";
                    break;
                case BattlePhase.PhaseAction:
                    phase = "战斗";
                    break;
                case BattlePhase.PhaseSettle:
                    phase = "结算";
                    break;
            }
            this.roundPhase.text = phase;
        }

        public override void OnClose()
        {
            base.OnClose();
            BattleManager.Instance.OnRoundChanged -= UpdateRoundInfo;
            BattleManager.Instance.OnRoundSync -= UpdateRoundInfo;
        }
    }
}