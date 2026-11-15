using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using Common.Data;
using Common.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Model
{
    public enum BattleCharacterState
    {
        None,
        Defend,
        Attack,
        Skill
    }
    public class BattleCharacter : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler ,IPointerClickHandler
    {
        //public SpriteRenderer spriteRenderer;
        public Character character;
        public Animator anim;
        public string ownerID;

        [SerializeField] private Sprite[] stateIcons;
        [SerializeField] private Sprite deathSprite;
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField] public Transform VisualRoot;

        public BattleView battleView;

        [SerializeField] private GameObject defendObj;
        [SerializeField] private GameObject weaponObj;
        [SerializeField] public GameObject InfoPos;

        public int targetID = -1;
        public Vector2 targetPosition;
        public bool canDefend = true;
        public bool canAttack = true;
        public bool canUseSkill = false;

        private BattleAttackHandler currentActionHandler;

        public Action OnCharacterDead;
        public Action OnCharacterValueChanged;

        private BattleCharacterState m_state;
        public BattleCharacterState state
        {
            get { return m_state; } 
            set
            {
                if (m_state == value) return;
                m_state = value;
                OnStateChanged();
            }
        }

        public bool isDead = false;

        private SkillTargetType skillTargetType;
        public int skillBattleGroup;
        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            battleView = GetComponentInChildren<BattleView>();
            
        }
        private void OnEnable()
        {
            BattleManager.Instance.OnSubmitPhaseChanged += ForceClearState;
            BattleManager.Instance.OnBattlePhaseChanged += HideDefend;
            BattleCharacterManager.Instance.OnCharacterRemoved += CheckIsTargetDead;
        }
        private void Init()
        {
            character.CurHP = character.data.HP;
            character.CurMaxHP = character.data.HP;
            character.CurATK = character.data.ATK;
            character.CurDEF = character.data.DEF;
            character.CurSpeed = character.data.Speed;
            character.Buffs = new List<Buff>();
            character.CurMP = 0;

            if(character.isMain)
            {
                character.CurMaxHP *= 2;
                character.CurHP *= 2;
            }
            canAttack = true;
            canDefend = true;
            canUseSkill = false;

            if (this.character.data.SkillID == string.Empty) return;
            if(!DataManager.Instance.skillData.TryGetValue(this.character.data.SkillID, out var skill))return;
            skillTargetType = skill.TargetType;
            skillBattleGroup = skill.HitCount;
        }
        public void ChangeValue(EffectType effectType, int param)
        {
            bool needVFX = false;
            if (param > 0 && effectType != EffectType.MP) needVFX = true;
            switch (effectType)
            {
                case EffectType.Health:
                    character.CurHP += param;
                    break;
                case EffectType.Defence:
                    character.CurDEF += param;
                    break;
                case EffectType.Speed:
                    character.CurSpeed += param;
                    break;
                case EffectType.Power:
                    character.CurATK += param;
                    break;
                case EffectType.MaxHealth:
                    character.CurMaxHP += param;
                    break;
                case EffectType.MP:
                    character.CurMP += param;
                    break;
            }
            if (needVFX)
            {
                Vector2 playPos = this.transform.position;
                playPos.y += 0.25f;

                VFXPlayer.Play(playPos, effectType);
            }
            OnCharacterValueChanged?.Invoke();
        }
        public void SetCharacterOwner(string playerID)
        {
            ownerID = playerID;
            Init();
        }
        public void SetFacing(bool faceRight)
        {
            Vector3 originScale = VisualRoot.transform.localScale;
            VisualRoot.localScale = new Vector3(Mathf.Abs(originScale.x) * (faceRight ? 1 : -1),originScale.y,originScale.z);
        }
        public void OnPointerEnter(PointerEventData data)
        {
            if (UnityEngine.Device.Application.isMobilePlatform && data.pointerId < 0)
                return;
            BattleCharacterManager.Instance.HandlePointerEnterCharacter(this);
            //Debug.LogWarning($"触发进入 {data.pointerId}");
        }
        public void OnPointerExit(PointerEventData data)
        {
            if (UnityEngine.Device.Application.isMobilePlatform && data.pointerId < 0)
                return;
            BattleCharacterManager.Instance.HandlePointerExitCharacter(this);
            //Debug.LogWarning($"触发退出 {data.pointerId}");
        }
        public void OnPointerClick(PointerEventData data)
        {
            BattleCharacterManager.Instance.HandleCharacterClicked(this);
        }
        public void RemoveThisCharacter(bool isDead)
        {
            this.isDead = true;
            currentActionHandler?.Close();
            OnCharacterDead?.Invoke();
            if (isDead)
            {
                StartCoroutine(Death());
                return;
            }

            Destroy(this.gameObject);
        }
        private IEnumerator Death()
        {
            weaponObj.SetActive(false);
            anim.enabled = false;
            characterRenderer.sprite = deathSprite;
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
        public void ForceClearState()
        {
            //HideDefend();
            if(BattleManager.Instance.CurrentPhase == NetMessage.BattlePhase.PhaseNone) { return; }
            ChangeState(BattleCharacterState.None);
            targetID = -1;
        }
        public void ChangeState(BattleCharacterState state)
        {
            this.state = state;
        }
        private void OnStateChanged()
        {
            iconRenderer.gameObject.SetActive(m_state != BattleCharacterState.None);

            iconRenderer.sprite = stateIcons[(int)m_state];
            if(state == BattleCharacterState.Defend)
            {
                targetID = -1;
            }
        }
        public void ShowDefend()
        {
            defendObj.SetActive(true);
        }
        public void HideDefend()
        {
            defendObj.SetActive(false);
            //ChangeState(BattleCharacterState.None);
        }
        private void CheckIsTargetDead(BattleCharacter removedCharacter)
        {
            if (this.targetID == -1 || this.targetID != removedCharacter.character.ID) return;

            ForceClearState();
        }
        public BattleAttackHandler PlayAttack(BattleCharacter target)
        {
            if (battleView == null)
            {
                BattleAttackHandler handle = new BattleAttackHandler();
                handle.MarkHit();
                handle.MarkFinish();

                //currentActionHandler = handle;
                //return handle;
            }
            else
                currentActionHandler = battleView.PlayAttackAnim(target);
            
            return currentActionHandler;

        }
        public BattleAttackHandler PlaySkill(BattleCharacter target = null)
        {
            if (battleView == null)
            {
                BattleAttackHandler handle = new BattleAttackHandler();
                handle.MarkHit();
                handle.MarkFinish();

                //currentActionHandler = handle;
                //return handle;
            }
            else if(skillTargetType == SkillTargetType.SelectEnemy )currentActionHandler = battleView.PlaySelectSkillAnim(target);
            else currentActionHandler = battleView.PlaySkillAnim();

            return currentActionHandler;
        }
        public IEnumerator PlayHit()
        {
            if(battleView == null)
            {
                yield break;
            }
            yield return battleView.PlayHit();
        }
        private void OnDisable()
        {
            BattleManager.Instance.OnSubmitPhaseChanged -= ForceClearState;
            BattleManager.Instance.OnBattlePhaseChanged -= HideDefend;
            BattleCharacterManager.Instance.OnCharacterRemoved -= CheckIsTargetDead;
        }

    }
}
