using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Model
{
    public class BattleView : MonoBehaviour
    {
        protected BattleCharacter owner;

        [SerializeField] protected Transform characterRoot;
        [SerializeField] protected Transform weaponRoot;

        [SerializeField] protected float backAngle;
        [SerializeField] protected float hitDuration;

        private void Awake()
        {
            owner = GetComponent<BattleCharacter>();
        }
        public virtual BattleAttackHandler PlayAnim(BattleCharacter target)
        {

            return null;
        }
        public virtual IEnumerator PlayHit()
        {
            if (characterRoot == null) yield break;

            Quaternion originRotation = characterRoot.transform.localRotation;

            SpriteRenderer[] sprites = characterRoot.GetComponentsInChildren<SpriteRenderer>(true);

            Color[] originColors = new Color[sprites.Length];

            for(int i = 0; i< sprites.Length; i++)
            {
                originColors[i] = sprites[i].color;
                sprites[i].color = Color.red;
            }

            float t = 0f;

            float face = owner.transform.localScale.x < 0 ? -1 : 1f;

            Quaternion target = originRotation * Quaternion.Euler(0, 0, -backAngle * face);

            while(t < hitDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / hitDuration);
                characterRoot.transform.localRotation = Quaternion.Slerp(originRotation, target, Mathf.Sin(p * Mathf.PI));

                yield return null;
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].color = originColors[i];
            }
        }
    }
}

