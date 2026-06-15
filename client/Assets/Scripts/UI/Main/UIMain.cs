using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using Assets.Scripts.Util;
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

            PlayerInfoManager.Instance.NicknameResponse += SetNewNickNameCallback;
        }
        private void Start()
        {
            if(!PlayerInfoManager.Instance.hasSetNickname)
            {
                InputFieldBox.Show("请输入昵称，如取消则将ID设置为昵称，后续可修改", (nickname) =>
                {
                    PlayerInfoManager.Instance.SendNewNickname(nickname);
                },()=>
                {
                    PlayerInfoManager.Instance.SendNewNickname(PlayerInfoManager.Instance.userID);
                });
            }
            else
            {
                OnPlayerEnterScene();
            }
        }

        private void SendMatch()
        {
            RoomService.Instance.SendMatchRequest();
        }

        private void OnPlayerEnterScene()
        {
            playerID.text = PlayerInfoManager.Instance.nickname;
        }
        private void OnDisable()
        {
            PlayerInfoManager.Instance.NicknameResponse -= SetNewNickNameCallback;
        }
        private void SetNewNickNameCallback(bool success)
        {
            if(success)
            {
                OnPlayerEnterScene();
                InputFieldBox.Close();
            }
            else
            {
                
            }
        }
    }
}
