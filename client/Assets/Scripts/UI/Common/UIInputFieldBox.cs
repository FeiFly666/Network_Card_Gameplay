using Assets.Scripts.Manager;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public class UIInputFieldBox : UIWindow
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] protected TMP_InputField input;

        [SerializeField] private Button conformButton;
        [SerializeField] private Button backButton;

        public void ShowInputField(string message, Action<string> buttonFunc, Action backFunc)
        {
            messageText.text = message;

            conformButton.onClick.AddListener(() => buttonFunc?.Invoke(input.text));

            backButton.onClick.AddListener(() => backFunc?.Invoke());
        }
        public void HideThisBox()
        {
            CloseUI();
        }

        public override void CloseUI()
        {
            //UIManager.Instance.ClosePopup(gameObject);
            UIManager.Instance.ClosePopup(gameObject);
        }

        protected override void OnCloseAnimFinish()
        {
            base.OnCloseAnimFinish();

            if (!gameObject.activeSelf)
                UIManager.Instance.DestroyPopup(gameObject);
        }
    }
}