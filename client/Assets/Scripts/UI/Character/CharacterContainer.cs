using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.UI.Characters
{
    public class CharacterContainer : MonoBehaviour
    {
        [SerializeField] private Vector2[] playerCharacterSlots;
        [SerializeField] private Vector2[] enemyCharacterSlots;
        [SerializeField] private Vector2 playerMain;
        [SerializeField] private Vector2 enemyMain;
        //角色ID确认对应槽位
        private Dictionary<int,int> idToSlot = new Dictionary<int,int>();
        //槽位ID确认角色
        private Dictionary<int, BattleCharacter> idToCharacter = new Dictionary<int, BattleCharacter>();

        private Dictionary<int, bool> playerAvialableSlot = new Dictionary<int, bool>();
        private Dictionary<int, bool> enemyAvialableSlot = new Dictionary<int, bool>();

        private readonly Dictionary<BattleCharacter, UICharacterValue> activeValueUIs = new();

        private UICharacterValue UIChaValPrefab;
        [SerializeField] private string UIChaValPrefabID;
        [SerializeField] private Transform uiRoot;
        bool isLoadPrefab = false;

        // Use this for initialization
        public void LoadPrefab(Action<bool> onComplete)
        {
            uiRoot = UIManager.Instance.uiChaValRoot;

            UIManager.Instance.GetUIPrefab<UICharacterValue>(UIChaValPrefabID, prefab =>
            {
                if (this == null)
                {
                    if(prefab != null)
                    {
                        AssetBundleManager.instance.ReleaseAsset("UI/Item/" + UIChaValPrefabID);
                    }
                    return;
                }
                if(prefab == null)
                {
                    onComplete?.Invoke(false);
                    return;
                }

                UIChaValPrefab = prefab;

                PoolManager.Instance.CreateGameObjectPool(UIChaValPrefab, maxNum: 8, preWarmNum: 0);
                isLoadPrefab = true;

                Init();
                onComplete?.Invoke(true);
            });
        }
        
        public void Init()
        {
            BattleCharacterManager.Instance.OnNewCharacterSpawn += SetNewBattleCharacterPosition;
            BattleCharacterManager.Instance.OnCharacterRemoved += RemoveBattleCharacter;
            for (int i = 0; i < playerCharacterSlots.Length; i++)
            {
                playerAvialableSlot[i] = true;
            }
            for (int i = 0; i < enemyCharacterSlots.Length; i++)
            {
                enemyAvialableSlot[i] = true;
            }
            BattleCharacterManager.Instance.playerCenter = (playerMain + playerCharacterSlots[1]) / 2;
            BattleCharacterManager.Instance.enemyCenter = (enemyMain + enemyCharacterSlots[1]) / 2;
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void SetNewBattleCharacterPosition(BattleCharacter character)
        {
            if(character.character.isMain)
            {
                if (character.ownerID == PlayerInfoManager.Instance.userID)
                {
                    character.gameObject.transform.position = playerMain;
                }
                else
                {
                    character.gameObject.transform.position = enemyMain;
                    character.SetFacing(false);
                }
                GiveCharacterValUI(character);
                return;
            }
            //先找到合适的槽位
            int posID = GetAvailablePosition(character.ownerID == PlayerInfoManager.Instance.userID);
            if (posID == -1) return;

            //槽位转换一下，如果是敌人，槽位+100
            if (character.ownerID != PlayerInfoManager.Instance.userID)
            {
                //character.VisualRoot.Rotate(new Vector3(0, 180, 0));
                character.SetFacing(false);
                posID += 100;
            }

            //角色ID对应槽位,槽位对应角色
            idToSlot[character.character.ID] = posID;
            idToCharacter[posID] = character;

            //放置
            character.gameObject.transform.position = posID >= 100 ? enemyCharacterSlots[posID - 100] : playerCharacterSlots[posID];
            
            if(posID >= 100)
            {
                enemyAvialableSlot[posID - 100] = false;
            }
            else
            {
                playerAvialableSlot[posID] = false;
            }

            GiveCharacterValUI(character);
        }
        private int GetAvailablePosition(bool isPlayer)
        {
            int position = -1;
            if (isPlayer)
            {
                foreach (var available in playerAvialableSlot)
                {
                    if (available.Value)
                    {
                        position = available.Key;
                        return position;
                    }
                }
            }
            else
            {
                foreach (var available in enemyAvialableSlot)
                {
                    if (available.Value)
                    {
                        position = available.Key;
                        return position;
                    }
                }
            }

            return position;
        }

        private void GiveCharacterValUI(BattleCharacter character)
        {
            if (activeValueUIs.ContainsKey(character)) return;

            int prefabId = UIChaValPrefab.gameObject.GetInstanceID();
            var ui = PoolManager.Instance.GetGameObject<UICharacterValue>(prefabId);

            ui.transform.SetParent(uiRoot, false);
            ui.transform.localScale = UIChaValPrefab.transform.localScale;
            ui.SetOwner(character);
            activeValueUIs.Add(character, ui);
        }

        private void ReleaseCharacterValUI(BattleCharacter character)
        {
            if (!activeValueUIs.TryGetValue(character, out var ui)) return;

            activeValueUIs.Remove(character);
            ui.ClearOwner();
            PoolManager.Instance.ReleaseGameObject(ui);
        }

        private void ReleaseAllCharacterValUIs()
        {
            foreach (var ui in activeValueUIs.Values)
            {
                ui.ClearOwner();
                PoolManager.Instance.ReleaseGameObject(ui);
            }

            activeValueUIs.Clear();
        }
        private void RemoveBattleCharacter(BattleCharacter character)
        {
            Debug.Log("删除角色位置信息");
            ReleaseCharacterValUI(character);
            if (character.character.isMain)
            {
                return;
            }
            int posID = idToSlot[character.character.ID];

            idToCharacter.Remove(posID);
            idToSlot.Remove(character.character.ID);

            if(posID >=100)
            {
                posID -= 100;
                enemyAvialableSlot[posID] = true;
            }
            else
            {
                playerAvialableSlot[posID] = true;
            }
        }
        private void OnDisable()
        {
            if (this == null) return;
            if(isLoadPrefab)
            {
                BattleCharacterManager.Instance.OnNewCharacterSpawn -= SetNewBattleCharacterPosition;
                BattleCharacterManager.Instance.OnCharacterRemoved -= RemoveBattleCharacter;
            }
            ReleaseAllCharacterValUIs();
        }
    }
}