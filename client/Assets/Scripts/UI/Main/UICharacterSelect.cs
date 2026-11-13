using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Assets.Scripts.Service;
using Assets.Scripts.UI.SpecialEffectUI;
using Common.Data;
using Common.Model;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main
{
    public class UICharacterSelect :UIWindow
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
        [Header("当前角色被动显示")]
        [SerializeField] private UIChaSpecialEffectSlot slotOne;
        [SerializeField] private UIChaSpecialEffectSlot slotTwo;
        [SerializeField] private UISpecialEffectSelectPanel effectSelectPanel;
        private string currentCharacterID;
        private void Awake()
        {
            submitButton.onClick.AddListener(SendChangeMainCharRequest);

            showCha = GameObject.Find("ShowCharacterROOT").GetComponent<UICharacterShow>();

            InitSlot();
        }
        public void OnEnable()
        {
            InitShowCards();
            currentCharacterID = string.Empty;
            PlayerInfoManager.Instance.OnMainCharacterChanged += ChangeButtonUI;

            SpecialEffectsManager.Instance.OnEffectChanged += HandleEffectChange;

            SpecialEffectsService.Instance.SendGetSpecialEffectLists();

            SpecialEffectInfoPanel.OnPanelClosed += HideLockObj;
        }
        private void InitShowCards()
        {
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
        private void InitSlot()
        {
            slotOne.currentSlot = 0;
            slotTwo.currentSlot = 1;

            slotOne.OnClicked += OnSlotClicked;
            slotTwo.OnClicked += OnSlotClicked;
        }
        private void OnSlotClicked(int index, string effectID)//显示被动选择UI并传入当前槽位数字
        {
            if (string.IsNullOrEmpty(currentCharacterID))
                return;

            if (effectSelectPanel.ToggleForSlot(index, currentCharacterID, effectID))
            {
                slotOne.SetSelected(index == 0);
                slotTwo.SetSelected(index == 1);
            }
        }
        public void HideLockObj()
        {
            slotOne.lockObj.SetActive(false);
            slotTwo.lockObj.SetActive(false);
        }

        private void HandleEffectChange(SpecialEffect effect)
        {
            if(SpecialEffectsManager.Instance.GetSlotEffect(currentCharacterID, 0, out var effect1))
            {
                slotOne.UpdateIcon(effect1.Data.DataID);
            }
            else
            {
                slotOne.UpdateIcon("");
            }

            if(SpecialEffectsManager.Instance.GetSlotEffect(currentCharacterID, 1, out var effect2))
            {
                slotTwo.UpdateIcon(effect2.Data.DataID);
            }
            else
            {
                slotTwo.UpdateIcon("");
            }

        }

        private void HandleClickCard(string cardCharacterID)
        {
            showCha.ChangeShowPrefab(cardCharacterID);

            selectInfo.ChangeInfo(cardCharacterID);
            currentCharacterID = cardCharacterID;

            ChangeButtonUI();

            slotOne.gameObject.SetActive(true);
            slotTwo.gameObject.SetActive(true);
            HandleEffectChange(null);

            effectSelectPanel.Hide();
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
            PlayerInfoManager.Instance.SendChangeMainCharRequest(currentCharacterID);
        }
        private void OnDisable()
        {
            ClearCards();
            HideAllPrefabs();
            PlayerInfoManager.Instance.OnMainCharacterChanged -= ChangeButtonUI;
            
            SpecialEffectsManager.Instance.OnEffectChanged -= HandleEffectChange;

            SpecialEffectInfoPanel.OnPanelClosed -= HideLockObj;

            slotOne.gameObject.SetActive(false);
            slotTwo.gameObject.SetActive(false);

            submitButton.gameObject.SetActive(false);
            alreadySubmitButton.gameObject.SetActive(false);

            effectSelectPanel.gameObject.SetActive(false);
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
