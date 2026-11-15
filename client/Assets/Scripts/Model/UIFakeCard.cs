using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Model
{
    public class UIFakeCard : UICard
    {
        private Tween useTween;
        public override void OnDrag(PointerEventData data)
        {
            ;
        }
        public override void OnPointerEnter(PointerEventData data)
        {
            ;
        }
        public override void OnPointerExit(PointerEventData data)
        {
            ;
        }
        public override void RemoveCard(bool isDrop)
        {
            if (isDrop)
            {
                anim.SetTrigger("Drop");
                return;
            }
            if (!(root != null && root.transform is RectTransform cardFace))
            {
                RemoveThisCard();
                return;
            }

            anim.enabled = false;

            useTween = DOTween.Sequence()
                .Append(cardFace.DOAnchorPosY(-101f, 0.42f).SetEase(Ease.InOutQuad))
                .Insert(0.12f, cardFace.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.23f)
                    .SetEase(Ease.InOutQuad))
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    useTween = null;
                    RemoveThisCard();
                });
        }
        public override void OnSpawn()
        {
            base.OnSpawn();
            anim.enabled = true;
        }
        public override void OnDeSpawn()
        {
            useTween?.Kill();
            useTween = null;

            if (root != null && root.transform is RectTransform cardFace)
            {
                cardFace.anchoredPosition = Vector2.zero;
                cardFace.localScale = Vector3.one;
                cardFace.localRotation = Quaternion.identity;
            }
            base.OnDeSpawn();
        }
    }
}
