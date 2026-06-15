using Assets.Scripts.Manager;
using Assets.Scripts.UI.Main;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Start
{
    public class UIStartPanel : MonoBehaviour
    {
        public enum Panel
        {
            Login,
            Regist
        }
        [SerializeField] UILogin login;
        [SerializeField] UIRegist regist;
        [SerializeField] private Panel currentPanel = Panel.Login;

        private void Start()
        {
            PlayerInfoManager.Instance.OnLoginResponse += LoginButtonCanInteract;
            PlayerInfoManager.Instance.OnRegistResponse += RegistButtonCanInteract;
        }
        private void LoginButtonCanInteract(bool result)
        {
            login.loginButton.interactable = true;
            if(!result)
            {
                login.ClearPassword();
            }
        }
        private void RegistButtonCanInteract(bool result)
        {
            regist.submit.interactable = true;

            if(result)
            {
                login.id.text = regist.id.text;
                SetPanel(Panel.Login);
            }
        }
        public void SetPanel(int num)
        {
            SetPanel((Panel)num);
        }
        public void SetPanel(Panel newPanel)
        {
            if (newPanel == currentPanel) return;
            currentPanel = newPanel;
            login.gameObject.SetActive(newPanel == Panel.Login);
            regist.gameObject.SetActive(newPanel == Panel.Regist);
        }

        private void OnDestroy()
        {
            PlayerInfoManager.Instance.OnLoginResponse -= LoginButtonCanInteract;
            PlayerInfoManager.Instance.OnRegistResponse -= RegistButtonCanInteract;
        }
    }
}