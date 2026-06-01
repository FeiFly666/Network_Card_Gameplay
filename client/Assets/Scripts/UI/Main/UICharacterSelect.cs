using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Common.Data;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main
{
    public class UICharacterSelect :MonoBehaviour
    {
        //private Dictionary<string, GameObject> m_characterPrefabs = new Dictionary<string, GameObject>();
        private List<UIShowCard> m_generatedCards = new List<UIShowCard>();
        [Header("UI相关配置")]
        [SerializeField] private UICharacterShow showCha;
        [SerializeField] private UIShowCard cardPrefab;
        [SerializeField] private Transform cardRoot;
        [SerializeField] UISelectCharacterInfo selectInfo;
        [Header("按钮配置")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button alreadySubmitButton;

        private Message submitMainCha;
        private string currentCharacterID;
        private void Awake()
        {
            //DataManager.Instance.Init();

            submitMainCha = new Message();
            submitMainCha.Request = new RequestMessage();
            submitMainCha.Request.changeMainCha = new UserChangeMainChaRequest();

            exitButton.onClick.AddListener(() => { this.gameObject.SetActive(false); });
            submitButton.onClick.AddListener(SendChangeMainCharRequest);
        }
        public void OnEnable()
        {
            InitShowCards();
            currentCharacterID = string.Empty;
            PlayerInfoManager.Instance.OnMainCharacterChanged += ChangeButtonUI;
        }
        private void InitShowCards()
        {
            //m_characterPrefabs.Clear();
            m_generatedCards.Clear();
            foreach(var cardData in DataManager.Instance.cardData)
            {
                if(cardData.Value.Type == CardType.Character)
                {
                    UIShowCard newShowCard = Instantiate(cardPrefab, cardRoot, false);

                    newShowCard.Init(cardData.Key);
                    m_generatedCards.Add(newShowCard);

                    newShowCard.OnClicked += HandleClickCard;
                }
            }
        }
        private void HandleClickCard(string cardCharacterID)
        {
            showCha.ChangeShowPrefab(cardCharacterID);

            selectInfo.ChangeInfo(cardCharacterID);
            currentCharacterID = cardCharacterID;

            ChangeButtonUI();
        }
        private void ChangeButtonUI()
        {
            if(currentCharacterID == PlayerInfoManager.Instance.mainCharacterID)
            {
                submitButton.gameObject.SetActive(false);
                alreadySubmitButton.gameObject.SetActive(true);
            }
            else
            {
                submitButton.gameObject.SetActive(true);
                alreadySubmitButton.gameObject.SetActive(false);
            }
        }
        private void SendChangeMainCharRequest()
        {
            submitMainCha.Request.changeMainCha.mainCharacterID = currentCharacterID;
            NetClient.Instance.Send(submitMainCha);
        }
        private void OnDisable()
        {
            ClearCards();
            HideAllPrefabs();
            PlayerInfoManager.Instance.OnMainCharacterChanged -= ChangeButtonUI;
        }
        private void ClearCards()
        {
            foreach(var card in m_generatedCards.ToList())
            {
                Destroy(card.gameObject);
            }
        }
        private void HideAllPrefabs()
        {
            showCha.HideAllPrefabs();
        }
    }
}
