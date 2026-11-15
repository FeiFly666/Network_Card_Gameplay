using Assets.ObjectPool;
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
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sequence = DG.Tweening.Sequence;

namespace Assets.Scripts.Model
{
    public class UICard : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler,IPointerEnterHandler, IPointerExitHandler,Assets.ObjectPool.IPoolable
    {
        public Card card;
        [Header("基本UI配置")]
        [SerializeField] private Image img;//卡牌图片
        [SerializeField] private Image bg;//卡牌背景
        [SerializeField] private TMP_Text cardName;//卡牌名称
        [SerializeField] protected GameObject root;
        [SerializeField] private RectTransform hoverRoot;
        [SerializeField] protected LayerMask characterLayer;
        private RectTransform rTransform;

        private Vector3 originPos;

        private int siblingIndex;
        private Transform layoutParent;
        public Transform dragRoot;

        private CanvasGroup canvasGroup;
        protected Animator anim;
        //private Camera mainCam;
        private Vector3 initialLocalScale;

        //放大缩小效果
        private Tween hoverTween;

        //使用效果
        private Tween playCardTween;

        //晃动效果
        private Tween swayReturnTween;
        private float? previousSwayX;
        private float swayIdleTime;

        [Header("延迟移动用节点")]
        [SerializeField] private RectTransform followRoot;

        private Vector3? followTarget;
        private Vector3 shownPosition;
        private TweenerCore<Vector3, Vector3, VectorOptions> followTween;

        private Vector3 FollowOffset => followTarget.HasValue ? shownPosition - rTransform.position : Vector3.zero;

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

            initialLocalScale = transform.localScale;
            //mainCam = Camera.main;
        }
        public virtual void OnSpawn()
        {
            moving = true;
            isRemoving = false;
            isHovering = false;

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            transform.localScale = initialLocalScale;

            if (followRoot == null || root == null) return;

            // 让 UGUI 先注册重排回调
            _ = CanvasUpdateRegistry.instance;
            Canvas.willRenderCanvases += UpdatePositionFollow;
        }
        private void LateUpdate()
        {
            HandleSway();
        }
        /// <summary>
        /// 处理拖拽倾斜
        /// </summary>
        private void HandleSway()
        {
            if (root == null || hoverRoot == null || isRemoving) return;

            float delta = Time.unscaledDeltaTime;
            //float screenX = RectTransformUtility.WorldToScreenPoint(null, root.transform.position).x;
            float screenX = RectTransformUtility.WorldToScreenPoint(null, root.transform.position + FollowOffset).x;

            float? lastX = previousSwayX;
            previousSwayX = screenX;
            if (!lastX.HasValue || delta <= 0f) return;

            //按屏幕宽度归一化
            float speedX = (screenX - lastX.Value) / (Mathf.Max(1, Screen.width) * delta);
            float angle = Mathf.DeltaAngle(0f, hoverRoot.localEulerAngles.z);

            if (Mathf.Abs(speedX) >= 0.015f)
            {
                swayIdleTime = 0f;
                swayReturnTween?.Kill();

                float targetAngle = Mathf.Clamp(-speedX * 30f, -24f, 24f);
                float follow = 1f - Mathf.Exp(-25f * delta);
                hoverRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(angle, targetAngle, follow));
                return;
            }

            // 输入缓冲。
            swayIdleTime += delta;
            if (swayIdleTime < 0.03f || swayReturnTween != null || Mathf.Abs(angle) < 0.01f)
                return;

            swayReturnTween = hoverRoot.DOLocalRotate(Vector3.zero, 0.18f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject).OnKill(() => swayReturnTween = null);
        }
        private void ResetSway()
        {
            previousSwayX = null;
            swayIdleTime = 0f;
            swayReturnTween?.Kill();

            if (hoverRoot != null)
                hoverRoot.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 处理延迟跟随
        /// </summary>
        private void UpdatePositionFollow()
        {
            if (rTransform == null) return;
            Vector3 target = rTransform.position;

            if (!followTarget.HasValue)
            {
                // 新卡从当前槽位开始，由 root 播原来的进场动画。
                shownPosition = target;
            }
            else if (target != followTarget.Value)
            {
                if (followTween != null)
                {
                    followTween.ChangeEndValue(target, snapStartValue: true);
                }
                else
                {
                    followTween = DOTween.To(() => shownPosition, position => shownPosition = position, target, 0.08f)
                        .SetEase(Ease.OutCubic)
                        .SetUpdate(true)
                        .OnKill(() => followTween = null);
                }
            }

            followTarget = target;
            followRoot.position = root.transform.position + FollowOffset;
        }
        public virtual void SetCardInfo()
        {
            if(DataManager.Instance.cardImages.TryGetValue(card.Data.DataID, out var sprite))
            {
                img.overrideSprite = sprite;
            }

            if (DataManager.Instance.cardBgColors.TryGetValue(card.Data.DataID, out var color))
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

                ResetHover();

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
            if (!canDrag || !(dragRoot is RectTransform dragRect)) return;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragRect, data.position, null, out Vector3 position))
            {
                rTransform.position = position;
            }

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
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f, characterLayer);

            if(hit != null)
            {
                character = hit.GetComponentInParent<BattleCharacter>();
            }

            HandleDrag(area,character);

            this.canvasGroup.blocksRaycasts = true;
            isDragging = false;
            BattleCardManager.Instance.UpdateUsingCard(null);
        }

        public virtual void OnPointerEnter(PointerEventData data)
        {
            if (moving || isRemoving) return;

            if (BattleCardManager.Instance.HasDraggingCard) return;
            if (isHovering) return;

            isHovering = true;
            /*anim.ResetTrigger("Down");
            anim.SetTrigger("Up");*/
            SetHover(true);
        }
        public virtual void OnPointerExit(PointerEventData data)
        {
            if (moving || isRemoving) return;
            if (BattleCardManager.Instance.HasDraggingCard && !isDragging) return;
            if (!isHovering) return;

            isHovering = false;
            /*anim.ResetTrigger("Up");
            anim.SetTrigger("Down");*/
            SetHover(false);
        }
        public virtual void HandleDrag(UIDragArea area, BattleCharacter character)
        {

        }
        public void ReturnToOrigin()
        {
            this.transform.SetParent(layoutParent,false);

            this.transform.SetSiblingIndex(siblingIndex);

            root.transform.localScale = Vector3.one;
            root.transform.localPosition = Vector3.zero;

            ResetHover();
            //this.rTransform.position = originPos;
        }
        public virtual void RemoveCard(bool isDrop)
        {
            if (isRemoving) return;

            isRemoving = true;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            /*anim.ResetTrigger("Up");
            anim.ResetTrigger("Down");*/
            //ResetHover();
            if (isDrop)
            {
                ResetHover();
                anim.Play("Card_Drop", 0, 0f);
            }
            else
            {
                //RemoveThisCard();
                HandleUsed();
            }
        }
        private void HandleUsed()
        {
            // 从当前显示的大小和角度接着播，避免先跳回原样。
            hoverTween?.Kill();
            hoverTween = null;
            swayReturnTween?.Kill();

            Sequence sequence = DOTween.Sequence();

            if (hoverRoot != null)
            {
                Vector3 startScale = hoverRoot.localScale;
                sequence.Append(hoverRoot.DOScale(startScale * 1.06f, 0.04f)
                    .SetEase(Ease.OutQuad));
                sequence.Append(hoverRoot.DOScale(startScale * 0.85f, 0.12f)
                    .SetEase(Ease.OutQuad));
            }

            sequence.Insert(0.04f, canvasGroup.DOFade(0f, 0.12f));

            playCardTween = sequence
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    playCardTween = null;
                    RemoveThisCard();
                });
        }
        private void ResetUsed()
        {
            playCardTween?.Kill();
            playCardTween = null;
            canvasGroup.alpha = 1f;
        }
        public void RemoveThisCard()
        {
            //Destroy(this.gameObject);
            //PoolManager.Instance.ReleaseGameObject<UICard>(this);
            BattleCardManager.Instance.ReleaseCard(this);
        }
        public virtual void OnDeSpawn()
        {
            card = null;

            canDrag = false;
            isDragging = false;
            layoutParent = null;
            dragRoot = null;
            siblingIndex = 0;

            ResetUsed();

            Canvas.willRenderCanvases -= UpdatePositionFollow;
            followTween?.Kill();
            followTarget = null;

            if (followRoot != null)
                followRoot.localPosition = Vector3.zero;

            ResetHover();
            ResetSway();
        }
        public void Moving() => moving = true;
        public void MovingFinish() => moving = false;

        private void SetHover(bool hovered)
        {
            if (hoverRoot == null) return;

            isHovering = hovered;
            hoverTween?.Kill();

            Vector3 scale = hovered ? new Vector3(1.2f, 1.2f, 1f) : Vector3.one;
            float y = hovered ? 56f : 0f;
            const float duration = 0.16f;

            hoverTween = DOTween.Sequence().Join(hoverRoot.DOScale(scale, duration).SetEase(Ease.OutQuad)).
                Join(hoverRoot.DOAnchorPosY(y, duration).SetEase(Ease.OutQuad)).SetLink(gameObject);
        }

        private void ResetHover()
        {
            hoverTween?.Kill();
            hoverTween = null;
            isHovering = false;

            if (hoverRoot == null) return;

            hoverRoot.anchoredPosition = Vector2.zero;
            hoverRoot.localScale = Vector3.one;
        }
    }
}