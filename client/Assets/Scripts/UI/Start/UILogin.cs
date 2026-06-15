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

namespace Assets.Scripts.UI.Start
{
    public class UILogin : MonoBehaviour
    {
        [SerializeField] public TMP_InputField id;
        [SerializeField] public TMP_InputField password;

        [SerializeField] public Button loginButton;

        private void OnEnable()
        {
            loginButton.onClick.RemoveAllListeners();
         
            loginButton.onClick.AddListener(SendLogin);
            id.text = string.Empty;
            password.text = string.Empty;
        }

        private void SendLogin()
        {
            if(!NetClient.Instance.isConnected)
            {
                if(!NetClient.Instance.Reconnect())
                {
                    return;
                }
            }
            PlayerInfoManager.Instance.SendLogin(id.text, password.text);
            loginButton.interactable = false;
        }
        public void ClearPassword()
        {
            password.text = string.Empty;
        }
    }
}
