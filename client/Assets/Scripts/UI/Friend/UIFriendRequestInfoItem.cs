using Assets.Scripts.Service;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Friend
{
    public class UIFriendRequestInfoItem : MonoBehaviour
    {

        [SerializeField] TMP_Text ID;
        [SerializeField] TMP_Text Nickname;
        [SerializeField] TMP_Text time;
        [SerializeField] Button greenButton;
        [SerializeField] Button redButton;

        private long requestID;
        private void Awake()
        {
            greenButton.onClick.RemoveAllListeners();
            greenButton.onClick.AddListener(AcceptRequest);

            redButton.onClick.RemoveAllListeners();
            redButton.onClick.AddListener(RejectRequest);
        }
        public void SetInfo(long requestID, string id, string nickname, long sendTime)
        {
            this.requestID = requestID;
            ID.text = id;
            Nickname.text = nickname;

            DateTime sendTimeLocal = DateTimeOffset.FromUnixTimeMilliseconds(sendTime).LocalDateTime;

            float year = sendTimeLocal.Year;
            float month = sendTimeLocal.Month;
            float day = sendTimeLocal.Day;
            float hour = sendTimeLocal.Hour;
            float minute = sendTimeLocal.Minute;

            time.text = $"{year}.{month}.{day} {hour}:{minute}";
        }
        public void SetButtonFunction(Action greenFunc, Action redFunc)
        {
            greenButton.onClick.RemoveAllListeners();
            greenButton.onClick.AddListener(() => { greenFunc(); });

            redButton.onClick.RemoveAllListeners();
            redButton.onClick.AddListener(() => { redFunc(); });
        }
        private void AcceptRequest()
        {
            FriendService.Instance.SendHandleFriendRequest(this.requestID, true);
        }
        private void RejectRequest()
        {
            FriendService.Instance.SendHandleFriendRequest(this.requestID, false);
        }
    }
}