using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class VFXPlayer
    {
        public static void Play(Vector2 playPosition, EffectType effectType)
        {
            EffectObject effectPrefab = DataManager.Instance.buffVFXs[effectType];
            if (effectPrefab == null) { return; }
            EffectObject effect = PoolManager.Instance.GetGameObject<EffectObject>(effectPrefab.gameObject.GetInstanceID());

            if(effect == null) { return; }

            effect.transform.position = playPosition;

            effect.Play();
        }
    }
}
