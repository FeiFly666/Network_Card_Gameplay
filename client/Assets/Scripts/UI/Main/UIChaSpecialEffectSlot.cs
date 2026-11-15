using System;
using Assets.Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main
{
    public class UIChaSpecialEffectSlot : MonoBehaviour,IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public int currentSlot = -1;
        public string currentSlotEffect = "";
        public float infoShowDelayTime = 0.5f;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite noIcon;
        [SerializeField] public GameObject lockObj;
        private RectTransform rect_Transform => GetComponent<RectTransform>();
        public event Action<int, string> OnClicked;
        //private bool isLocked = false;

        public void UpdateIcon(string effectID)
        {
            if(DataManager.Instance.specialEffectPictures.TryGetValue(effectID, out var newIcon))
            {
                icon.sprite = newIcon;
                currentSlotEffect = effectID;
            }
            else
            {
                icon.sprite = noIcon;
                currentSlotEffect = "";
            }
        }
        public void SetSelected(bool selected)
        {
            lockObj.gameObject.SetActive(selected);
        }
        public void OnPointerClick(PointerEventData data)
        {
            OnClicked?.Invoke(currentSlot,currentSlotEffect);
        }
        public void OnPointerEnter(PointerEventData data)
        {
            if(!DataManager.Instance.specialEffectData.TryGetValue(this.currentSlotEffect, out var effectData))
            {
                return;
            }
            SpecialEffectsManager.Instance.OnPointerEnter(effectData, rect_Transform.position, infoShowDelayTime);
        }
        public void OnPointerExit(PointerEventData data)
        {
            if(!DataManager.Instance.specialEffectData.TryGetValue(this.currentSlotEffect, out var effectData))
            {
                return;
            }
            SpecialEffectsManager.Instance.OnPointerExit();
        }
    }

}

