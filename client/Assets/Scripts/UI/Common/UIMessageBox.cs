using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public enum UIMessageType
    {
        Normal,//普通的通知，只有一个确认按钮
        Interact//需要互动的，有确认按钮和取消按钮
    }
    public enum MessageType
    {
        Normal, //普通的消息
        Warning, //有点小问题
        Error // 发生错误
    }
    public class UIMessageBox : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private Sprite[] infoTypeBg;

        [SerializeField] private TMP_Text messageText;

        [SerializeField] private Button conformButton;
        [SerializeField] private Button funcButton;
        [SerializeField] private Button backButton;
        public void ShowMessage(MessageType messageType, string message)
        {
            ChangeInfoTypeImg(messageType);
            ChangeMessageType(UIMessageType.Normal);

            messageText.text = message;

            conformButton.onClick.AddListener(HideThisMessageBox);
        }
        public void ShowMessage(MessageType messageType, string message, Action function)
        {
            ChangeInfoTypeImg(messageType);
            ChangeMessageType(UIMessageType.Normal);

            messageText.text = message;

            conformButton.onClick.AddListener(HideThisMessageBox);
            conformButton.onClick.AddListener(() => function?.Invoke());
        }
        public void ShowInteractionMessage(MessageType messageType, string message, Action buttonFunc, Action backFunc)
        {
            ChangeInfoTypeImg(messageType);
            ChangeMessageType(UIMessageType.Interact);

            messageText.text = message;

            funcButton.onClick.AddListener(()=>buttonFunc?.Invoke());
            funcButton.onClick.AddListener(HideThisMessageBox);

            backButton.onClick.AddListener(HideThisMessageBox);
            backButton.onClick.AddListener(() => backFunc?.Invoke());
        }

        private void ChangeInfoTypeImg(MessageType type)
        {
            bg.overrideSprite = infoTypeBg[(int)type];
        }
        private void ChangeMessageType(UIMessageType type)
        {
            conformButton.gameObject.SetActive(false);
            funcButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(false);

            switch(type)
            {
                case UIMessageType.Normal:
                    conformButton.gameObject.SetActive(true);
                    break;
                case UIMessageType.Interact:
                    funcButton.gameObject.SetActive(true);
                    backButton.gameObject.SetActive(true);
                    break;
            }
        }

        private void HideThisMessageBox()
        {
            Destroy(this.gameObject);
        }

    }
}