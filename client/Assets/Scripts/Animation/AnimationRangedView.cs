using Assets.Scripts.Model;
using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Animation
{
    public enum RangedAttackType
    {
        Instant,
        Bullet
    }
    public class AnimationRangedView : AnimationBattleView
    {
        [SerializeField] private RangedAttackType attackType = RangedAttackType.Instant;
        [Header("子弹配置")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject skillBulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private bool isNeedScale = false;
        public override BattleAttackHandler PlayAttackAnim(BattleCharacter target)
        {
            currentHandler.Refresh();
            StartCoroutine(PlayAttackRoutine(target));

            return currentHandler;
        }
        public override BattleAttackHandler PlaySelectSkillAnim(BattleCharacter target)
        {
            currentHandler.Refresh();
            StartCoroutine(SkillRoutine());

            return currentHandler;
        }
        public override BattleAttackHandler PlaySkillAnim()
        {
            currentHandler.Refresh();
            StartCoroutine(SkillRoutine());

            return currentHandler;
        }
        IEnumerator PlayAttackRoutine(BattleCharacter target)
        {
            BeginAttack(target);
            switch (attackType)
            {
                case RangedAttackType.Instant:
                    yield return InstantRoutine(target); break;
                case RangedAttackType.Bullet:
                    yield return BulletRoutine(target); break;
            }
            currentHandler.MarkFinish();
            yield return null;
        }

        IEnumerator InstantRoutine(BattleCharacter target)
        {
            yield return null;
            
            anim.SetTrigger(AttackTrigger);

            while(!animFinished)
            {
                yield return null;
            }
            currentHandler.MarkFinish();
            AnimationBackToIdle();
        }
        IEnumerator BulletRoutine(BattleCharacter target)
        {
            yield return null;

            anim.SetTrigger(AttackTrigger);

            while (!animFinished)
            {
                yield return null;
            }

            Quaternion originRotation = weaponMoveRoot.localRotation;

            Vector3 hitPos = target.transform.position;
            hitPos.y += 0.5f;

            Transform space = weaponMoveRoot.parent;

            Vector3 localHitPos = space.InverseTransformPoint(hitPos);
            Vector3 localWeaponPos = space.InverseTransformPoint(weaponMoveRoot.position);

            Vector3 dir = localHitPos - localWeaponPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            Quaternion startRotation = weaponMoveRoot.localRotation;


            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                weaponMoveRoot.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t / 0.2f);

                yield return null;
            }

            weaponMoveRoot.transform.localRotation = targetRotation;

            Transform bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity).transform;

            if (isNeedScale)
            {
                bullet.localScale = new Vector3(0, 0, 0);
                while (bullet.localScale.x < 1.25)
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
            currentHandler.MarkHit();

            t = 0f;
            startRotation = weaponMoveRoot.localRotation;

            while (t < 0.2f)
            {
                t += Time.deltaTime;
                weaponMoveRoot.localRotation = Quaternion.Slerp(startRotation, originRotation, t / 0.2f);

                yield return null;
            }

            weaponMoveRoot.localRotation = originRotation;

            currentHandler.MarkFinish();
        }
        IEnumerator SkillRoutine()
        {
            yield return null;
            BeginSkill();

            anim.SetTrigger(SkillTrigger);

            switch (attackType)
            {
                case RangedAttackType.Instant:
                    yield return InstantSkill(); break;
                case RangedAttackType.Bullet:
                    yield return BulletSkill(); break;
            }
            currentHandler.MarkFinish();
            AnimationBackToIdle();
        }
        IEnumerator BulletSkill()
        {
            while (!animFinished)
            {
                yield return null;
            }
            Vector3 firePointOriginPos = firePoint.position;

            Vector3 newPos = owner.targetPosition;
            newPos.y += 10;

            firePoint.position = newPos;

            Transform bullet = Instantiate(skillBulletPrefab, firePoint.position, Quaternion.identity).transform;

            while ((bullet.transform.position - (Vector3)owner.targetPosition).sqrMagnitude > 0.25f)
            {
                bullet.position = Vector3.MoveTowards(bullet.position, owner.targetPosition, bulletSpeed* 2 * Time.deltaTime);

                yield return null;
            }

            Destroy(bullet.gameObject);

            firePoint.position = firePointOriginPos;
            currentHandler.MarkHit();

        }
        IEnumerator InstantSkill()
        {
            while (!animFinished)
            {
                yield return null;
            }
            currentHandler.MarkFinish();
            AnimationBackToIdle();
        }

    }
}
