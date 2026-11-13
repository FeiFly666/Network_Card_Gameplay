using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.SO
{
    [CreateAssetMenu(fileName ="new Visual Container", menuName = "SO/SpecialEffectPicture")]
    public class SpecialEffectPicture : ScriptableObject
    {
        public List<SpecialEffectPictureData> pics = new List<SpecialEffectPictureData>();
    }
    [Serializable]
    public class SpecialEffectPictureData
    {
        public string name;
        public Sprite icon;
    }
}
