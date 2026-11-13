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
        public Dictionary<string, BuffData> buffData = new Dictionary<string, BuffData>();
        public Dictionary<string, BuffEffectData> buffEffectData = new Dictionary<string, BuffEffectData>();
        public Dictionary<string, SkillData> skillData = new Dictionary<string, SkillData>();
        public Dictionary<string, SpecialEffectData> specialEffectData = new Dictionary<string, SpecialEffectData>();

        private string sOPath = "SO/";
        //public VisualContainer VisualContianer { get; set; }
        public Dictionary<string, Sprite> cardImages = new Dictionary<string, Sprite>();
        public Dictionary<string, Color> cardBgColors = new Dictionary<string, Color>();
        public Dictionary<EffectType, Sprite> buffImages = new Dictionary<EffectType, Sprite>();
        public Dictionary<EffectType, EffectObject> buffVFXs = new Dictionary<EffectType, EffectObject>();
        public Dictionary<int, EffectObject> VFXs = new Dictionary<int, EffectObject>();
        public Dictionary<string, Sprite> profilePictures = new Dictionary<string, Sprite>();
        public Dictionary<string, Sprite> specialEffectPictures = new Dictionary<string, Sprite>();

        private string characterPath = "Prefab/Character";

        public Dictionary<string, BattleCharacter> characterPrefabs = new Dictionary<string, BattleCharacter>();

        public void Init()
        {
            LoadCardData();
            LoadVisualContainuer();
            LoadSpecialEffectPictures();
            LoadCharacterPrefab();
        }
        private void LoadCardData()
        {
            TextAsset text = Resources.Load<TextAsset>(configPath + "Card");

            cardData = JsonConvert.DeserializeObject<Dictionary<string, CardData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "Character");

            characterData = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "Buff");

            buffData = JsonConvert.DeserializeObject<Dictionary<string, BuffData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "BuffEffect");

            buffEffectData = JsonConvert.DeserializeObject<Dictionary<string, BuffEffectData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "Skill");

            skillData = JsonConvert.DeserializeObject<Dictionary<string, SkillData>>(text.text);

            text = Resources.Load<TextAsset>(configPath + "SpecialEffectInfos");

            specialEffectData = JsonConvert.DeserializeObject<Dictionary<string, SpecialEffectData>>(text.text);
        }
        private void LoadSpecialEffectPictures()
        {
            SpecialEffectPicture pictures = Resources.Load<SpecialEffectPicture>(sOPath + "SpecialEffectPicture");

            specialEffectPictures.Clear();

            foreach(var pic in pictures.pics)
            {
                if(!specialEffectPictures.ContainsKey(pic.name))
                {
                    specialEffectPictures.Add(pic.name, pic.icon);
                }
                else
                {
                    specialEffectPictures[pic.name] = pic.icon;
                }
            }
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

            foreach (var vfx in VisualContianer.VFXs) 
            {
                if(!VFXs.ContainsKey(vfx.id))
                {
                    VFXs.Add(vfx.id, vfx.effect.GetComponent<EffectObject>());
                }
                else
                {
                    VFXs[vfx.id] = vfx.effect.GetComponent<EffectObject>();
                }
            }

            foreach(var picture in VisualContianer.profilePics)
            {
                if(!profilePictures.ContainsKey(picture.characterID))
                {
                    profilePictures.Add(picture.characterID, picture.picture);
                }
                else
                {
                    profilePictures[picture.characterID] = picture.picture;
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
            foreach(var vfx in VFXs.Values)
            {
                PoolManager.Instance.CreateGameObjectPool<EffectObject>(vfx, 2, 1);
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
