using Assets.Scripts.Model;
using Assets.Scripts.SO;
using Common;
using Common.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class DataManager:Singleton<DataManager>
    {
        private string configPath = "Config/";
        public Dictionary<string, CardData> cardData = new Dictionary<string, CardData>();
        public Dictionary<string, CharacterData> characterData = new Dictionary<string,CharacterData>();
        public Dictionary<string, BuffData> effectData = new Dictionary<string, BuffData>();

        private string sOPath = "SO/";
        //public VisualContainer VisualContianer { get; set; }
        public Dictionary<string, Sprite> cardImages = new Dictionary<string, Sprite>();
        public Dictionary<string, Color> cardBgColors = new Dictionary<string, Color>();
        public Dictionary<EffectType, Sprite> buffImages = new Dictionary<EffectType, Sprite>();
        public Dictionary<EffectType, EffectObject> buffVFXs = new Dictionary<EffectType, EffectObject>();

        private string characterPath = "Prefab/Character";

        public Dictionary<string, BattleCharacter> characterPrefabs = new Dictionary<string, BattleCharacter>();

        public void Init()
        {
            LoadCardData();
            LoadVisualContainuer();
            LoadCharacterPrefab();
        }
        private void LoadCardData()
        {
            TextAsset text = Resources.Load<TextAsset>(configPath + "Card");

            cardData = JsonConvert.DeserializeObject<Dictionary<string, CardData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "Character");

            characterData = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "Effect");

            effectData = JsonConvert.DeserializeObject<Dictionary<string, BuffData>>(text.text);
        }
        private void LoadVisualContainuer()
        {
            VisualContainer VisualContianer = Resources.Load<VisualContainer>(sOPath + "VisualContainer");

            cardImages.Clear();
            buffImages.Clear();

            foreach (var cardInfo in VisualContianer.CardImages)
            {
                if (!cardImages.ContainsKey(cardInfo.id))
                {
                    cardImages.Add(cardInfo.id, cardInfo.icon);
                }
                else
                {
                    //Debug.LogWarning($"Duplicate card id: {cardImg.id}");
                    cardImages[cardInfo.id] = cardInfo.icon;
                }
                if(!cardBgColors.ContainsKey (cardInfo.id))
                {
                    cardBgColors.Add(cardInfo.id, cardInfo.bgColor);
                }
                else
                {
                    cardBgColors[cardInfo.id] = cardInfo.bgColor;
                }
            }

            foreach (var buffImg in VisualContianer.BuffImages)
            {
                if (!buffImages.ContainsKey(buffImg.type))
                {
                    buffImages.Add(buffImg.type, buffImg.icon);
                }
                else
                {
                    //Debug.LogWarning($"Duplicate buff id: {buffImg.id}");
                    buffImages[buffImg.type] = buffImg.icon;
                }
            }

            foreach(var buffVFX in  VisualContianer.BuffVFXs)
            {
                if (!buffImages.ContainsKey(buffVFX.type))
                {
                    buffVFXs.Add(buffVFX.type, buffVFX.effect.GetComponent<EffectObject>());
                }
                else
                {
                    //Debug.LogWarning($"Duplicate buff id: {buffImg.id}");
                    buffVFXs[buffVFX.type] = buffVFX.effect.GetComponent<EffectObject>();
                }
            }

        }
        public void Start()
        {
            InitPools();
        }
        private void InitPools()
        {
            foreach(var vfx in buffVFXs.Values)
            {
                PoolManager.Instance.CreateGameObjectPool<EffectObject>(vfx, 5, 3);
            }
        }
        private void LoadCharacterPrefab()
        {
            BattleCharacter[] gameObjects = Resources.LoadAll<BattleCharacter>(characterPath);

            foreach (var gameObject in gameObjects)
            {
                string key = gameObject.name;

                characterPrefabs.Add(key, gameObject);

            }
        }
    }
}
