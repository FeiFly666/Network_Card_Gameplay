using Assets.Scripts.Service;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Chat
{
    public class UIChat : MonoBehaviour
    {
        [SerializeField] private TMP_InputField input;
        [SerializeField] private Button sendButton;
        [SerializeField] private UIChatItem messagePrefab;
        [SerializeField] private Transform root;
        // Use this for initialization
        private void OnEnable()
        {
            ChatManager.Instance.OnNewChatRecieve += ShowNewChat;
            sendButton.onClick.RemoveAllListeners();

            sendButton.onClick.AddListener(SendNewMessage);
        }

        private void ShowNewChat(string id, string msg)
        {
            GameObject go = Instantiate(messagePrefab.gameObject, root, false);

            UIChatItem newItem = go.GetComponent<UIChatItem>();
            newItem.SetMessage(id, msg);
        }
       

        private void SendNewMessage()
        {
            string newMessage = input.text;
            if (!string.IsNullOrEmpty(newMessage))
            {
                ChatService.Instance.SendChat(newMessage);
                input.text = string.Empty;
            }
        }
        private void OnDisable()
        {
            ChatManager.Instance.OnNewChatRecieve -= ShowNewChat;
        }
    }
}