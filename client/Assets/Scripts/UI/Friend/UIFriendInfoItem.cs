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

namespace Assets.Scripts.UI.Friend
{
    public class UIFriendInfoItem : MonoBehaviour
    {
        [SerializeField] TMP_Text ID;
        [SerializeField] TMP_Text Nickname;
        [SerializeField] Image mainCharacterPic;
        [SerializeField] TMP_Text online;
        [SerializeField] Button greenButton;
        [SerializeField] Button redButton;

        private string friendID;
        public string UserID { set; get; }
        public bool IsOnline { set; get; }
        private void Awake()
        {
            redButton.onClick.RemoveAllListeners();
            greenButton.onClick.RemoveAllListeners();

            redButton.onClick.AddListener(RemoveFriend);
        }
        public void SetInfo(string id, string nickname, bool isOnline, string mainCharacterID)
        {
            ID.text = id;
            friendID = id;
            Nickname.text = nickname;
            mainCharacterPic.overrideSprite = DataManager.Instance.profilePictures[mainCharacterID];

            if(isOnline)
            {
                online.text = "在线";
                online.color = Color.green;
            }
            else
            {
                online.text = "离线";
                online.color = Color.gray;
            }

            UserID = id;
            IsOnline = isOnline;
        }
        private void RemoveFriend()
        {
            MessageBox.Show(Common.MessageType.Warning, $"确定删除好友 {Nickname.text} 吗？", () =>
            {
                FriendService.Instance.SendRemoveFriendRequest(friendID);
            }, null);
        }
    }
}
