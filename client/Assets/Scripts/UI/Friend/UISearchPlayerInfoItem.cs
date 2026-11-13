using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using NetMessage;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Friend
{
    public class UISearchPlayerInfoItem : MonoBehaviour
    {
        [SerializeField] TMP_Text ID;
        [SerializeField] TMP_Text Nickname;
        [SerializeField] TMP_Text online;
        [SerializeField] Button button;
        [SerializeField] TMP_Text buttonText;
        [SerializeField] Image mainCharacterPic;
        private string playerID;
        private bool canSend = true;
        private void Awake()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SendAddFriendRequest);
        }
        public void SetInfo(PlayerSearchInfo info)
        {
            ID.text = info.userID;
            playerID = info.userID;
            Nickname.text = info.Nickname;
            mainCharacterPic.overrideSprite = DataManager.Instance.profilePictures[info.mainCharacterID];

            switch (info.Relation)
            {
                case FriendRelationState.FriendRelationFriend:
                    buttonText.text = "已添加";
                    canSend = false;
                    button.interactable = false;
                    break;
                case FriendRelationState.FriendRelationRequestSent:
                    buttonText.text = "已请求";
                    canSend = false;
                    button.interactable = false;
                    break;
                case FriendRelationState.FriendRelationRequestReceived:
                    buttonText.text = "待确认";
                    canSend = false;
                    button.interactable = false;
                    break;
                default:
                    buttonText.text = "添加";
                    canSend = true;
                    button.interactable = true;
                    break;
            }

        }
        private void SendAddFriendRequest()
        {
            if(canSend)
                FriendService.Instance.SendAddFriendRequest(this.playerID);
        }
    }
}