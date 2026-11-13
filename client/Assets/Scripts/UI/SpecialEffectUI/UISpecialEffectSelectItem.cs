using System;
using Assets.Scripts.Manager;
using Common.Model;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SpecialEffectUI
{
    public class UISpecialEffectSelectItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject equipIcon;
        [SerializeField] private GameObject lockObj;
        [SerializeField] private Image icon;

        private RectTransform rect_Transform => GetComponent<RectTransform>();
        public float infoShowDelayTime = 0.1f;
        public event Action<string, bool> OnClicked;
        private string thisEffect;
        private bool isEquipped;
        private bool PointerStay = false;
        public bool pointerStay
        {
            get{return PointerStay;}
            set
            {
                PointerStay = value;
                lockObj?.SetActive(value);
            }
        }
        public void SetData(SpecialEffect data)
        {
            isEquipped = data.equippedCharacterID != null && data.equippedCharacterID != "";
            equipIcon.gameObject.SetActive(isEquipped);

            thisEffect = data.Data.DataID;
            if(DataManager.Instance.specialEffectPictures.TryGetValue(data.Data.DataID, out var sprite))
            {
                icon.overrideSprite = sprite;
            }
        }

        public void OnPointerClick(PointerEventData data)
        {
            Debug.Log($"被动按钮点击事件触发 [{thisEffect}]");
            OnClicked?.Invoke(thisEffect, isEquipped);
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if(!DataManager.Instance.specialEffectData.TryGetValue(this.thisEffect, out var effectData))
            {
                return;
            }
            pointerStay = true;
            SpecialEffectsManager.Instance.OnPointerEnter(effectData, rect_Transform.position, infoShowDelayTime);
        }
        public void OnPointerExit(PointerEventData data)
        {
            if(!DataManager.Instance.specialEffectData.TryGetValue(this.thisEffect, out var effectData))
            {
                return;
            }
            pointerStay = false;
            SpecialEffectsManager.Instance.OnPointerExit();
        }
    }

}
