using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public class UIInputFieldBox : MonoBehaviour
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
            Destroy(this.gameObject);
        }
    }
}