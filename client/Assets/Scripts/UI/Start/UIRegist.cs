using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Start
{
    public class UIRegist : MonoBehaviour
    {

        [SerializeField] public TMP_InputField id;
        [SerializeField] public TMP_InputField password;
        [SerializeField] public TMP_InputField repeatPassword;
        [SerializeField] public Button submit;

        private void OnEnable()
        {
            submit.onClick.RemoveAllListeners();
            submit.onClick.AddListener(SendRegist);

            id.text = string.Empty;
            password.text = string.Empty;
            repeatPassword.text = string.Empty;
        }
        private void SendRegist()
        {
            if (!NetClient.Instance.isConnected)
            {
                if (!NetClient.Instance.Reconnect())
                {
                    return;
                }
            }
            if (password.text != repeatPassword.text)
            {
                MessageBox.Show(Common.MessageType.Error, "密码不一致");
                return;
            }

            PlayerInfoManager.Instance.SendRegist(id.text, password.text);
            submit.interactable = false;
        }
    }
}