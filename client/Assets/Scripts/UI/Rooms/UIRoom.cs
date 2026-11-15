using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Common.Model;
using Assets.Scripts.Service;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.UI.Rooms
{
    public class UIRoom : UIWindow
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private TMP_Text readyText;
        [SerializeField] private Button exitButton;

        [SerializeField] private TMP_Text player1Name;
        [SerializeField] private TMP_Text player2Name;

        [SerializeField] private Image player1ReadyStatus;
        [SerializeField] private Image player2ReadyStatus;

        [SerializeField] private UICharacterShow character1Show;
        [SerializeField] private UICharacterShow character2Show;

        private void Awake()
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(SendReady);

            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(SendExit);
            //RoomManager.Instance.OnRoomInfoChanged += RefreshUI;
        }
        private void OnEnable()
        {
           /* isInit = false;
            RoomManager.Instance.OnRoomInfoChanged += RefreshUI;*/
        }
        public override void OnOpen()
        {
            base.OnOpen();
            RoomManager.Instance.OnRoomInfoChanged += RefreshUI;
            character1Show = GameObject.Find("ShowCharacterROOT")?.GetComponent<UICharacterShow>();
            character2Show = GameObject.Find("ShowCharacterROOT2")?.GetComponent<UICharacterShow>();
        }
        private void Start()
        {

        }

        private void RefreshUI(Room room)
        {
            if(room == null)
            {
                CloseUI();
                return;
            }
            this.gameObject.SetActive(true);
            //硬编码，目前只支持两个人
            if(player1Name.text != room.Players[0].nickname)
            {
                player1Name.text = room.Players[0].nickname;
            }
            player1ReadyStatus.gameObject.SetActive(room.Players[0].ready);
            string player1MainCha = room.Players[0].mainCharcterID;

            character1Show.ChangeShowPrefab(player1MainCha);

            if (room.Players.Count > 1)
            {
                if (player2Name.text != room.Players[1].nickname)
                {
                    player2Name.text = room.Players[1].nickname;
                }
                player2ReadyStatus.gameObject.SetActive(room.Players[1].ready);
                string player2MainCha = room.Players[1].mainCharcterID;

                character2Show.ChangeShowPrefab(player2MainCha);
            }
            else
            {
                player2Name.text = string.Empty;
                player2ReadyStatus.gameObject.SetActive(false);
                character2Show.HideAllPrefabs();
            }
            foreach(var player in room.Players)
            {
                if(player.id == PlayerInfoManager.Instance.userID)
                {
                    readyText.text = player.ready ? "取消" : "准备";
                }
            }
        }
        private void SendReady()
        {
            RoomService.Instance.SendReadyRequest();
        }
        private void SendExit()
        {
            RoomService.Instance.SendExitRequest();
        }
        private void OnDisable()
        {

        }
        public override void OnClose()
        {
            base.OnClose();
            character1Show?.HideAllPrefabs();
            character2Show?.HideAllPrefabs();
            RoomManager.Instance.OnRoomInfoChanged -= RefreshUI;
        }
    }
}
