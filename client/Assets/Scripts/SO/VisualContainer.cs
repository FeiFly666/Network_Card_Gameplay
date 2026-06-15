using Assets.Scripts.Model;
using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SO
{
    [CreateAssetMenu(fileName ="new Visual Container", menuName = "SO/VisualContainer")]
    public class VisualContainer : ScriptableObject
    {
        public List<CardImage> CardImages = new List<CardImage>();
        public List<BuffImage> BuffImages = new List<BuffImage>();
        public List<BuffVFX> BuffVFXs = new List<BuffVFX>();
        public List<VFX> VFXs = new List<VFX>();
    }
    [Serializable]
    public class CardImage
    {
        public string id;
        public Sprite icon;
        public Color bgColor;
    }

    [Serializable]
    public class BuffImage
    {
        public EffectType type;
        public Sprite icon;
    }
    [Serializable]
    public class BuffVFX
    {
        public EffectType type;
        public GameObject effect;
    }
    [Serializable]
    public class VFX
    {
        public int id;
        public GameObject effect;
    }
}
