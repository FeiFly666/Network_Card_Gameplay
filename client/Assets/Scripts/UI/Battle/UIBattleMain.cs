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

        [SerializeReference] private GameObject discardArea;
        [SerializeReference] private GameObject battleArea;
        private void Awake()
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(SendExit);
        }
        private void OnEnable()
        {

            BattleCardManager.Instance.OnStartDragCard += DragAreaActive;
            BattleCardManager.Instance.OnStopDragCard += DragAreaDeactive;
        }
        private void Start()
        {

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
        
        private void SendExit()
        {
            BattleService.Instance.SendExitBattleRequest();
        }
        private void OnDisable()
        {

            BattleCardManager.Instance.OnStartDragCard -= DragAreaActive;
            BattleCardManager.Instance.OnStopDragCard -= DragAreaDeactive;
        }
    }
}