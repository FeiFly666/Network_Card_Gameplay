using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Manager;
using Assets.Scripts.UI.SpecialEffectUI;
using Common;
using Common.Data;
using Common.Model;
using NetMessage;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class SpecialEffectsManager : Singleton<SpecialEffectsManager>
    {
        private Dictionary<string, SpecialEffect> m_specialEffects = new Dictionary<string, SpecialEffect>();
        private Dictionary<(string, int), SpecialEffect> m_chaSlot = new Dictionary<(string, int), SpecialEffect>();

        public event Action<SpecialEffect>OnEffectChanged;

        public event Action<SpecialEffectData, Vector2, float> OnPointerEnterForInfo;
        public event Action OnPointerExited;

        public void Init()
        {
            OnPointerEnterForInfo += ShowSpecialEffectInfo;
        }
        private void ShowSpecialEffectInfo(SpecialEffectData data, Vector2 pos, float delayTime)
        {
            UIManager.Instance.ShowUI<UISpecialEffectInfoPanel>("UISpecialEffectInfoPanel", (ui) =>
            {
                ui.ShowUp(data, pos, delayTime);
            });
        }
        public List<SpecialEffect> GetAllData()
        {
            return m_specialEffects.Values.ToList();
        }
        public void DeleteAllData()
        {
            OnEffectChanged = null;
            m_specialEffects.Clear();
            m_chaSlot.Clear();
        }
        public bool GetSlotEffect(string characterID, int slotIndex, out SpecialEffect effect)
        {
            return m_chaSlot.TryGetValue((characterID, slotIndex), out effect);
        }
        public void SetData(UserSpecialEffectInfo info)
        {
            string id = info.specialEffectDataID;
            SpecialEffect effect = new SpecialEffect();

            if(!DataManager.Instance.specialEffectData.TryGetValue(id, out var data))
            {
                Debug.LogError($"被动返回失败：未找到该ID的被动信息 [{id}]");
                return;
            }

            if(!IsDataSame(id, info.equippedCharacterDataID, info.slotIndex,out var oldData))
            {
                if(oldData != null)
                {
                    m_chaSlot.Remove((oldData.equippedCharacterID, oldData.slotIndex));
                }
                effect.Data = data;
                effect.equippedCharacterID = info.equippedCharacterDataID;
                effect.slotIndex = info.slotIndex;

                m_specialEffects[id] = effect;
                m_chaSlot[(effect.equippedCharacterID, effect.slotIndex)] = effect;

                OnEffectChanged?.Invoke(effect);
            }
        }
        private bool IsDataSame(string id, string newDataCha, int newDataIndex, out SpecialEffect oldData)
        {
            if(!m_specialEffects.TryGetValue(id,out oldData))
            {
                return false;
            }
            return oldData.equippedCharacterID == newDataCha && oldData.slotIndex == newDataIndex;

        }
        public bool GetData(string id, out SpecialEffect info)
        {
            return m_specialEffects.TryGetValue(id, out info);
        }

        public void OnPointerEnter()
        {
            
        }

        public void OnPointerEnter(SpecialEffectData effect, Vector2 pos, float delayTime)
        {
            OnPointerEnterForInfo?.Invoke(effect, pos, delayTime);
        }
        public void OnPointerExit()
        {
            OnPointerExited?.Invoke();
        }
    }

}