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
    public class UILogin : MonoBehaviour
    {
        [SerializeField] private TMP_InputField input;
        [SerializeField] private Button loginButton;

        private void OnEnable()
        {
            loginButton.onClick.RemoveAllListeners();
         
            loginButton.onClick.AddListener(SendLogin);
        }

        private void SendLogin()
        {
            /*if(NetClient.Instance.isConnected)
            {
                PlayerInfoManager.Instance.SendLogin(input.text);
            }
            else
            {
                NetClient.Instance
            }*/
            if(!NetClient.Instance.isConnected)
            {
                if(!NetClient.Instance.Reconnect())
                {
                    //MessageBox.Show(Common.MessageType.Error, "网络连接失败，请重新登录");
                    return;
                }
            }
            PlayerInfoManager.Instance.SendLogin(input.text);
        }
    }
}
