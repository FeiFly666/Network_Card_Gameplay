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
        [SerializeField] protected Transform weaponMoveRoot;
        [SerializeField] protected Transform weaponRoot;

        [SerializeField] protected float backAngle;
        [SerializeField] protected float hitDuration;

        [SerializeField] private bool isWeaponOutBody = false;

        private void Awake()
        {
            Init();
        }
        protected virtual void Init()
        {
            owner = GetComponentInParent<BattleCharacter>();
        }
        public virtual BattleAttackHandler PlayAttackAnim(BattleCharacter target)
        {

            return null;
        }
        public virtual BattleAttackHandler PlaySelectSkillAnim(BattleCharacter target)//选择目标的技能，即需要走到目标附近的技能
        {

            return null;
        }
        public virtual BattleAttackHandler PlaySkillAnim() { return null; }//非目标选择技能
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

            Transform originParent = null;

            if(isWeaponOutBody)
            {
                originParent = weaponRoot.transform.parent;

                weaponRoot.transform.SetParent(null);
            }

            while(t < hitDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / hitDuration);
                characterRoot.transform.localRotation = Quaternion.Slerp(originRotation, target, Mathf.Sin(p * Mathf.PI));

                yield return null;
            }

            if(isWeaponOutBody)
            {
                weaponRoot.transform.SetParent(originParent);
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].color = originColors[i];
            }
            
        }
    }
}

