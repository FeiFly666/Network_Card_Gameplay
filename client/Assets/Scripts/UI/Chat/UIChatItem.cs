using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Chat
{
    public class UIChatItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text id;
        [SerializeField] private TMP_Text msg;

        public void SetMessage(string id, string message)
        {
            this.id.text = id;
            this.msg.text = message;
        }
    }
}
