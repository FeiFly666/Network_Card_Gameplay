using Assets.Scripts.Manager;
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
    public class UIPlayerInfo : UIWindow
    {
        [SerializeField] private TMP_Text playerID;
        [SerializeField] private TMP_Text playerNickname;
        [SerializeField] private Button changeNicknameButton;

        private void OnEnable()
        {
            changeNicknameButton.onClick.RemoveAllListeners();
            changeNicknameButton.onClick.AddListener(ChangeNickname);

            PlayerInfoManager.Instance.NicknameResponse += ChangeNickname;
            ShowPlayerInfo();
        }
        private void ShowPlayerInfo()
        {
            playerID.text = PlayerInfoManager.Instance.userID;
            playerNickname.text = PlayerInfoManager.Instance.nickname;
        }
        private void ChangeNickname()
        {
            InputFieldBox.Show("请输入新昵称",(newName)=>
            {
                PlayerInfoManager.Instance.SendNewNickname(newName);
            }, () =>
            {
                InputFieldBox.Close();
            });
        }
        private void ChangeNickname(bool success)
        {
            if(success)
            {
                playerNickname.text = PlayerInfoManager.Instance.nickname;
            }
        }
        private void OnDisable()
        {
            PlayerInfoManager.Instance.NicknameResponse -= ChangeNickname;
        }
    }
}
