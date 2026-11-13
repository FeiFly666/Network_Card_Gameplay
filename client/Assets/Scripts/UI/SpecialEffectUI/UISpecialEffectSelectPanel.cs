using System;
using System.Collections.Generic;
using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using Common.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SpecialEffectUI
{
    public class UISpecialEffectSelectPanel : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [SerializeField] private UISpecialEffectSelectItem itemPrefab;
        [SerializeField] private Dictionary<string, UISpecialEffectSelectItem> items = new Dictionary<string, UISpecialEffectSelectItem>();
        public bool isOpen = false;
        public int currentSlotID;
        private string characterID;
        private string currentEffectID;
        private void OnEnable()
        {
            SpecialEffectsManager.Instance.OnEffectChanged += HandleEffectChange;
        }
        public bool ToggleForSlot(int slotId, string characterId, string effectId)
        {
            if (isOpen && currentSlotID == slotId && characterID == characterId)
            {
                Hide();
                return false;
            }

            ClearItem();
            ShowUp(slotId, characterId, effectId);
            return true;
        }
        public void ShowUp(int slotID, string characterID, string currentEffectID = null)
        {
            currentSlotID = slotID;
            this.characterID = characterID;
            this.currentEffectID = currentEffectID;

            this.gameObject.SetActive(true);

            ShowItems();

            isOpen = true;
        }

        private void ShowItems()
        {
            var infos = SpecialEffectsManager.Instance.GetAllData();

            foreach(var info in infos)
            {
                var newItem = GameObject.Instantiate(itemPrefab, root);

                newItem.SetData(info);

                newItem.OnClicked += HandleClick;

                items[info.Data.DataID] = newItem;
            }
        }

        private void HandleEffectChange(SpecialEffect effect)
        {
            if(items.TryGetValue(effect.Data.DataID, out var item))
            {
                item.SetData(effect);
            }
        }

        private void HandleClick(string effectID, bool isEquipped)
        {
            if(isEquipped)
            {
                if(effectID != currentEffectID) return;
                SpecialEffectsService.Instance.SendUnEquipEffect(effectID);
            }
            else
            {
                SpecialEffectsService.Instance.SendEquipEffect(effectID, characterID, currentSlotID);
            }

            this.Hide();
        }

        public void ClearItem()
        {
            foreach(var item in items.Values)
            {
                item.OnClicked -= HandleClick;
                if(item.pointerStay)
                {
                    SpecialEffectsManager.Instance.OnPointerExit();
                }
                if(item.gameObject != null)
                    Destroy(item.gameObject);
            }

            items.Clear();

        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            ClearItem();

            isOpen = false;

            SpecialEffectInfoPanel.OnPanelClose();
        }
        private void OnDisable()
        {
            SpecialEffectsManager.Instance.OnEffectChanged -= HandleEffectChange;
        }
    }

}

