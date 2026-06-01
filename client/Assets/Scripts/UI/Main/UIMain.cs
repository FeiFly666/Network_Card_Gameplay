using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main
{
    public class UIMain : MonoBehaviour
    {
        [SerializeField] private Button matchButton;
        [SerializeField] private TMP_Text playerID;

        private void OnEnable()
        {
            matchButton.onClick.RemoveAllListeners();
         
            matchButton.onClick.AddListener(SendMatch);
        }
        private void Start()
        {
            OnPlayerEnterScene();
        }

        private void SendMatch()
        {
            RoomService.Instance.SendMatchRequest();
        }

        private void OnPlayerEnterScene()
        {
            playerID.text = PlayerInfoManager.Instance.userID;
        }
    }
}
