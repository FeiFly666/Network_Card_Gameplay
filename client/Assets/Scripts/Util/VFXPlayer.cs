using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Common.Data;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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
        public static void Play(Vector2 playPosition, bool needFlip, int id)
        {
            EffectObject effectPrefab = DataManager.Instance.VFXs[id];
            if (effectPrefab == null) { return; }
            EffectObject effect = PoolManager.Instance.GetGameObject<EffectObject>(effectPrefab.gameObject.GetInstanceID());
            ParticleSystemRenderer ps = effect.GetComponent<ParticleSystemRenderer>();
            if (effect == null) { return; }

            effect.transform.position = playPosition;
            ps.flip = needFlip ? new Vector3(1, ps.flip.y, 0) : new Vector3(0, ps.flip.y, 0);

            effect.Play();
        }
    }
}
