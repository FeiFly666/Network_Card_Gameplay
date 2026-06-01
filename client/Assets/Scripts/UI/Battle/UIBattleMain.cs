using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using NetMessage;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Battle
{
    public class UIBattleMain : MonoBehaviour
    {

        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text roundNum;
        [SerializeField] private TMP_Text roundLeftNum;
        [SerializeField] private TMP_Text roundPhase;

        [SerializeReference] private GameObject discardArea;
        [SerializeReference] private GameObject battleArea;

        [SerializeField] private GameObject bg;
        private void Awake()
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(SendExit);
        }
        private void OnEnable()
        {
            BattleManager.Instance.OnRoundChanged += UpdateRoundInfo;
            BattleManager.Instance.OnRoundSync += UpdateRoundInfo;

            BattleCardManager.Instance.OnStartDragCard += DragAreaActive;
            BattleCardManager.Instance.OnStopDragCard += DragAreaDeactive;
        }
        private void Start()
        {
            //进入了，发送已经进入
            BattleService.Instance.Reset();
            BattleService.Instance.SendEnterRequest();

            BattleCardManager.Instance.Init();
            BattleManager.Instance.Init();
            BattleCharacterManager.Instance.Init();

            int playerPos = RoomManager.Instance.GetPlayerRoomPos();
            if (playerPos == -1) ;
            else if(playerPos == 0)
            {
                bg.transform.localScale = new Vector3(1, 1, 1);
            }
            else if(playerPos == 1)
            {
                bg.transform.localScale = new Vector3(-1, 1, 1);
            }

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void DragAreaActive()
        {
            ShowDiscardArea();
            ShowBattleArea();
        }
        private void DragAreaDeactive()
        {
            HideDiscardArea();
            HideBattleArea();
        }
        private void ShowDiscardArea()
        {
            discardArea.SetActive(true);
        }
        private void ShowBattleArea()
        {
            battleArea.SetActive(true);
        }
        private void HideDiscardArea()
        {
            discardArea.SetActive(false);
        }
        private void HideBattleArea()
        {
            battleArea.SetActive(false);
        }
        private void UpdateRoundInfo(int roundNum)
        {
            this.roundNum.text = $"Round {roundNum}";
        }
        private void UpdateRoundInfo(int roundNum, float leftTime, BattlePhase currentPhase)
        {
            this.roundNum.text = $"Round {roundNum}";

            if(currentPhase == BattlePhase.PhasePrepare)
            {
                this.roundLeftNum.text = $"{leftTime}";
            }
            else
            {
                this.roundLeftNum.text = string.Empty;
            }

            string phase = string.Empty;
            switch(currentPhase)
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
        private void SendExit()
        {
            BattleService.Instance.SendExitBattleRequest();
        }
        private void OnDisable()
        {
            BattleManager.Instance.OnRoundChanged -= UpdateRoundInfo;
            BattleManager.Instance.OnRoundSync -= UpdateRoundInfo;

            BattleCardManager.Instance.OnStartDragCard -= DragAreaActive;
            BattleCardManager.Instance.OnStopDragCard -= DragAreaDeactive;
        }
    }
}