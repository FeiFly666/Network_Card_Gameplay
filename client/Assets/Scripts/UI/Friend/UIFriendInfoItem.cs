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
        [SerializeField] TMP_Text online;
        [SerializeField] Button greenButton;
        [SerializeField] Button redButton;

        private string friendID;
        public string UserID { set; get; }
        public bool IsOnline { set; get; }
        public void SetInfo(string id, string nickname, bool isOnline)
        {
            ID.text = id;
            friendID = id;
            Nickname.text = nickname;

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
    }
}
