using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Animation
{
    public class AnimationMeleeView : AnimationBattleView
    {
        [SerializeField] private float attackDistance = 1f;
        [SerializeField] private float skillDistance = 1f;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private string moveTrigger = "Move";

        public override BattleAttackHandler PlayAttackAnim(BattleCharacter target)
        {
            currentHandler.Refresh();
            StartCoroutine(PlayAttackRoutine(target));

            return currentHandler;
        }

        public override BattleAttackHandler PlaySelectSkillAnim(BattleCharacter target)
        {
            currentHandler.Refresh();
            StartCoroutine(PlaySelectSkillRoutine(target));

            return currentHandler;
        }
        public override BattleAttackHandler PlaySkillAnim()
        {
            currentHandler.Refresh();
            StartCoroutine(PlaySkillRoutine());

            return currentHandler;
        }

        //攻击协程
        private IEnumerator PlayAttackRoutine(BattleCharacter target)
        {
            BeginAttack(target);

            Vector2 targetPos = target.transform.position;
            Vector2 originPos = owner.transform.position;
            //目标是我方角色
            if (target.ownerID == PlayerInfoManager.Instance.userID)
            {
                targetPos.x += attackDistance;
            }
            else
            {
                targetPos.x -= attackDistance;
            }

            //移动到目标前
            anim.SetTrigger(moveTrigger);

            while (((Vector2)owner.transform.position - targetPos).sqrMagnitude > 0.01f)
            {
                owner.transform.position = Vector3.MoveTowards(owner.transform.position, targetPos, moveSpeed * Time.deltaTime);

                yield return null;
            }

            owner.transform.position = targetPos;

            anim.SetTrigger(AttackTrigger);

            while (!animFinished)
            {
                yield return null;
            }

            anim.SetTrigger(moveTrigger);

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

            AnimationBackToIdle();

            if (owner.ownerID == PlayerInfoManager.Instance.userID)
            {
                owner.SetFacing(true);
            }
            else
            {
                owner.SetFacing(false);
            }
            currentHandler.MarkFinish();

            ClearAttack();

            yield return null;
        }
        private IEnumerator PlaySelectSkillRoutine(BattleCharacter target)
        {
            BeginAttack(target);

            Vector2 targetPos = target.transform.position;
            Vector2 originPos = owner.transform.position;
            //目标是我方角色
            if (target.ownerID == PlayerInfoManager.Instance.userID)
            {
                targetPos.x += skillDistance;
            }
            else
            {
                targetPos.x -= skillDistance;
            }

            //移动到目标前
            anim.SetTrigger(moveTrigger);

            while (((Vector2)owner.transform.position - targetPos).sqrMagnitude > 0.01f)
            {
                owner.transform.position = Vector3.MoveTowards(owner.transform.position, targetPos, moveSpeed * Time.deltaTime);

                yield return null;
            }

            owner.transform.position = targetPos;

            anim.SetTrigger(SkillTrigger);

            while (!animFinished)
            {
                yield return null;
            }

            anim.SetTrigger(moveTrigger);

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

            AnimationBackToIdle();

            if (owner.ownerID == PlayerInfoManager.Instance.userID)
            {
                owner.SetFacing(true);
            }
            else
            {
                owner.SetFacing(false);
            }
            currentHandler.MarkFinish();

            ClearAttack();

            yield return null;
        }
        private IEnumerator PlaySkillRoutine()
        {
            yield return null;

            anim.SetTrigger(SkillTrigger);

            while (!animFinished)
            {
                yield return null;
            }
            currentHandler.MarkFinish();
            AnimationBackToIdle();
        }
    }
}