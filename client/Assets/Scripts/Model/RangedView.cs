using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class RangedView:BattleView
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float moveTime = 0.1f;
        [SerializeField] private bool isNeedScale = false;
        public override BattleAttackHandler PlayAnim(BattleCharacter target)
        {
            BattleAttackHandler handler = new BattleAttackHandler();
            StartCoroutine(PlayAttackRoutine(target, handler));
            return handler;
        }

        IEnumerator PlayAttackRoutine(BattleCharacter target, BattleAttackHandler handler)
        {
            Quaternion originRotation = weaponRoot.localRotation;

            Vector3 hitPos = target.transform.position;
            hitPos.y += 0.5f;

            Transform space = weaponRoot.parent;

            Vector3 localHitPos = space.InverseTransformPoint(hitPos);
            Vector3 localWeaponPos = space.InverseTransformPoint(weaponRoot.position);

            Vector3 dir  = localHitPos - localWeaponPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //weaponRoot.rotation = Quaternion.Euler(0, 0, angle);

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            Quaternion startRotation = weaponRoot.localRotation;


            float t = 0f;
            while (t < moveTime)
            {
                t += Time.deltaTime;
                weaponRoot.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation,t / moveTime);

                yield return null;
            }

            weaponRoot.transform.localRotation = targetRotation;

            Transform bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity).transform;

            if (isNeedScale)
            {
                bullet.localScale = new Vector3(0, 0, 0);
                while(bullet.localScale.x <1)
                {
                    bullet.localScale = bullet.localScale + new Vector3(Time.deltaTime, Time.deltaTime, 0);
                    yield return null;
                }
            }

            while ((bullet.transform.position - hitPos).sqrMagnitude > 0.25f)
            {
                bullet.position = Vector3.MoveTowards(bullet.position, hitPos, bulletSpeed * Time.deltaTime);

                yield return null;
            }

            Destroy(bullet.gameObject);
            handler.MarkHit();

            t = 0f;
            startRotation = weaponRoot.localRotation;

            while (t < moveTime)
            {
                t += Time.deltaTime;
                weaponRoot.localRotation = Quaternion.Slerp(startRotation,originRotation,t / moveTime);

                yield return null;
            }

            weaponRoot.localRotation = originRotation;

            handler.MarkFinish();
        }
    }
}
