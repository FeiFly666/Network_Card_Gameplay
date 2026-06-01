using Assets.Scripts.Manager;
using Common.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Assets.Scripts.UI.Main
{
    public class UIShowCard : MonoBehaviour,IPointerClickHandler
    {
        [SerializeField] private Image img;//卡牌图片
        [SerializeField] private Image bg;//卡牌背景
        [SerializeField] private TMP_Text cardName;//卡牌名称
        [SerializeField] protected string characterID;

        public Action<string> OnClicked;

        public void Init(string cardDataID)
        {
            if(!DataManager.Instance.cardData.TryGetValue(cardDataID, out var card))
            {
                return;
            }
            characterID = card.TargetID[0];

            img.sprite = DataManager.Instance.cardImages[cardDataID];
            bg.color = DataManager.Instance.cardBgColors[cardDataID];

            cardName.text = card.Name;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke(characterID);
        }
    }
}