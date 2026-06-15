using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using NetMessage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
namespace Assets.Scripts.Model
{
    public class MeleeView : BattleView
    {
        //[SerializeField] private GameObject bulletPrefab; 
        [SerializeField] private float rotateTime = 0.75f;
        [SerializeField] private float weaponRotateAngle = -60f;
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private bool isStatic = false;
        public override BattleAttackHandler PlayAttackAnim(BattleCharacter target)
        {
            BattleAttackHandler handler = new BattleAttackHandler();
            StartCoroutine(PlayAttackRoutine(target, handler));
            return handler;
        }

        IEnumerator PlayAttackRoutine(BattleCharacter target, BattleAttackHandler handler)
        {
            Vector2 targetPos = target.transform.position;
            Vector2 originPos = owner.transform.position;
            //目标是我方角色
            if(target.ownerID == PlayerInfoManager.Instance.userID)
            {
                targetPos.x += 1f;
            }
            else
            {
                targetPos.x -= 1f;
            }

            //移动到目标前
            if(!isStatic)
            {
                owner.anim.SetTrigger("Move");

                while (((Vector2)owner.transform.position - targetPos).sqrMagnitude > 0.25f)
                {
                    owner.transform.position = Vector3.MoveTowards(owner.transform.position, targetPos, moveSpeed * Time.deltaTime);

                    yield return null;
                }
            }

            //挥舞

            owner.anim.SetTrigger("Idle");

            Quaternion targetRotation = Quaternion.Euler(0, 0, weaponRotateAngle);

            Quaternion startRotation = weaponMoveRoot.localRotation;
            Quaternion originRotaion = startRotation;

            float t = 0f;
            while (t < rotateTime)
            {
                t += Time.deltaTime;
                weaponMoveRoot.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t / rotateTime);

                yield return null;
            }

            handler.MarkHit();

            startRotation = weaponMoveRoot.localRotation;

            targetRotation = originRotaion;

            t = 0f;
            while (t < rotateTime)
            {
                t += Time.deltaTime;
                weaponMoveRoot.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t / rotateTime);

                yield return null;
            }

            //挥舞完毕返回
            if(!isStatic)
            {
                owner.anim.SetTrigger("Move");

                if (owner.ownerID == PlayerInfoManager.Instance.userID)
                {
                    owner.SetFacing(false);
                }
                else
                {
                    owner.SetFacing(true);
                }

                while (((Vector2)owner.transform.position - originPos).sqrMagnitude > 0.01f)
                {
                    owner.transform.position = Vector3.MoveTowards(owner.transform.position, originPos, moveSpeed * Time.deltaTime);

                    yield return null;
                }
                owner.transform.position = originPos;

                owner.anim.SetTrigger("Idle");
                yield return null;

                if (owner.ownerID == PlayerInfoManager.Instance.userID)
                {
                    owner.SetFacing(true);
                }
                else
                {
                    owner.SetFacing(false);
                }
            }

            handler.MarkFinish();

            yield return null;
        }
    }
}
