using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    public class AnimationBattleView : BattleView
    {
        [HideInInspector] protected Animator anim;
        [SerializeField] protected Transform vfxSpawnPoint;

        [SerializeField] protected string AttackTrigger;
        [SerializeField] protected string SkillTrigger;
        [SerializeField] protected string idleTrigger = "Idle";

        [Header("技能物体")]
        [SerializeField] protected GameObject Skill;
        protected BattleAttackHandler currentHandler;
        protected BattleCharacter currentTarget;

        protected bool animFinished = true;

        protected override void Init()
        {
            base.Init();
            anim = owner.anim;
            currentHandler = new BattleAttackHandler();
        }

        public void BeginAttack(BattleCharacter target)
        {
            //currentHandler.Refresh();
            currentTarget = target;

            animFinished = false;
        }
        public void BeginSkill()
        {
            animFinished = false;
        }
        public void AnimMarkHit()
        {
            currentHandler?.MarkHit();
        }

        public void AnimationFinish()
        {
            animFinished = true;
        }

        public void AnimationBackToIdle()
        {
            anim.SetTrigger(idleTrigger);
        }

        public void ShowVFX(int vfxID)
        {
            VFXPlayer.Play(vfxSpawnPoint.position, owner.ownerID != PlayerInfoManager.Instance.userID ,vfxID);
        }
        public void ShowSkillObject()
        {
            Skill?.SetActive(true);
        }
        public void HideSkillObject()
        {
            Skill.SetActive(false);
        }
        public void ClearAttack()
        {
            currentTarget = null;
            
        }
    }
}