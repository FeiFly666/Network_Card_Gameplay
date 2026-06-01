using Assets.Scripts.Manager;
using Assets.Scripts.UI.Battle;
using Common.Model;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Model
{
    public class UICard : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler,IPointerEnterHandler, IPointerExitHandler
    {
        public Card card;

        [SerializeField] private Image img;//卡牌图片
        [SerializeField] private Image bg;//卡牌背景
        [SerializeField] private TMP_Text cardName;//卡牌名称
        [SerializeField] protected GameObject root;
        private RectTransform rTransform;

        private Vector3 originPos;

        private int siblingIndex;
        private Transform layoutParent;
        public Transform dragRoot;

        private CanvasGroup canvasGroup;
        protected Animator anim;
        //private Camera mainCam;

        private bool canDrag;
        private bool isDragging;

        private bool moving = true;
        private bool isRemoving = false;
        private bool isHovering = false;
        private void Awake()
        {
            this.rTransform = GetComponent<RectTransform>();
            this.canvasGroup = this.AddComponent<CanvasGroup>();

            this.canvasGroup.blocksRaycasts = true;
            this.anim = this.GetComponent<Animator>();
            //mainCam = Camera.main;
        }
        public virtual void SetCardInfo()
        {
            if(!DataManager.Instance.cardImages.TryGetValue(card.Data.DataID, out var sprite))
            {
                ;
            }
            else
            {
                img.overrideSprite = sprite;
            }

            if (!DataManager.Instance.cardBgColors.TryGetValue(card.Data.DataID, out var color))
            {
                ;
            }
            else
            {
                bg.color = color;
            }

            cardName.text = card.Data.Name;
        }

        public virtual void OnBeginDrag(PointerEventData data)
        {
            //originPos = this.rTransform.position;

            canDrag = BattleCardManager.Instance.CanDragThisCard(this) && !moving && !isRemoving;

            if(canDrag)
            {
                layoutParent = this.transform.parent;

                siblingIndex = this.transform.GetSiblingIndex();
                this.transform.SetParent(dragRoot, false);

                BattleCardManager.Instance.UpdateUsingCard(this);

                this.canvasGroup.blocksRaycasts = false;
                isDragging = true;

                root.transform.localScale = Vector3.one;
                root.transform.localPosition = Vector3.zero;

                BattleCardManager.Instance.SetDraggingCard(true);
            }
        }
        public virtual void OnDrag(PointerEventData data)
        {
            if (!canDrag) return;
            rTransform.position = data.position;

        }
        public virtual void OnEndDrag(PointerEventData data)
        {
            if (!canDrag) return;

            BattleCardManager.Instance.SetDraggingCard(false);

            UIDragArea area = null;
            BattleCharacter character = null;

            if(data.pointerEnter != null)
            {
                area = data.pointerEnter.GetComponentInParent<UIDragArea>();
            }


            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f);

            if(hit != null)
            {
                character = hit.GetComponentInParent<BattleCharacter>();
            }

            HandleDrag(area,character);

            this.canvasGroup.blocksRaycasts = true;
            isDragging = false;
        }

        public virtual void OnPointerEnter(PointerEventData data)
        {
            if (moving || isRemoving) return;

            if (BattleCardManager.Instance.HasDraggingCard) return;
            if (isHovering) return;

            isHovering = true;
            anim.ResetTrigger("Down");
            anim.SetTrigger("Up");
        }
        public virtual void OnPointerExit(PointerEventData data)
        {
            if (moving || isRemoving) return;
            if (BattleCardManager.Instance.HasDraggingCard && !isDragging) return;
            if (!isHovering) return;

            isHovering = false;
            anim.ResetTrigger("Up");
            anim.SetTrigger("Down");
        }
        public virtual void HandleDrag(UIDragArea area, BattleCharacter character)
        {

        }
        public void ReturnToOrigin()
        {
            BattleCardManager.Instance.UpdateUsingCard(null);
            this.transform.SetParent(layoutParent,false);

            this.transform.SetSiblingIndex(siblingIndex);

            root.transform.localScale = Vector3.one;
            root.transform.position = Vector3.zero;
            //this.rTransform.position = originPos;
        }
        public virtual void RemoveCard(bool isDrop)
        {
            isRemoving = true;
            canvasGroup.interactable = false;
            anim.ResetTrigger("Up");
            anim.ResetTrigger("Down");
            if (isDrop)
            {
                anim.Play("Card_Drop", 0, 0f);
            }
            else
            {
                RemoveThisCard();
            }
        }
        public void RemoveThisCard()
        {
            Destroy(this.gameObject);
        }
        public void Moving() => moving = true;
        public void MovingFinish() => moving = false;
    }
}