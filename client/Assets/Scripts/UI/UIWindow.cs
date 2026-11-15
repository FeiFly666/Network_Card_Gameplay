using Assets.Scripts.Manager;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

namespace Assets.Scripts.UI
{
    public class UIWindow : MonoBehaviour
    {
        [SerializeField] public string id;
        [SerializeField] public float sleepTime;
        [SerializeField] public UILayer layer;
        [SerializeField] private Transform hoverRoot;
        [SerializeField] private bool UseOpenAnim = true;
        [SerializeField] private bool UseCloseAnim = true;

        private Vector2 originScale;
        private Vector2 minScale = new Vector2(0.02f, 0.02f);
        private CanvasGroup canvasGroup;

        private Tween hoverTween;
        public void Init()
        {
            canvasGroup = this.AddComponent<CanvasGroup>();

            if (hoverRoot == null) return;

            originScale = hoverRoot.localScale;

            if(UseOpenAnim && UseCloseAnim )
            {
                hoverRoot.localScale = minScale;
            }
        }

        public virtual void OnOpen() 
        { 
            this.gameObject.SetActive(true);
            this.canvasGroup.alpha = 1.0f;

            if(hoverTween != null )
            {
                OnCloseAnimFinish();
            }

            if (hoverRoot == null) return;

            if (UseOpenAnim)
            {
                OpenAnim();
            }
            else
            {
                hoverRoot.localScale = originScale;
            }
        }
        protected virtual void OpenAnim()
        {
            if (hoverRoot == null) return;

            hoverTween?.Kill();
            hoverTween = null;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(hoverRoot.DOScale(originScale * 1.05f, 0.25f)).SetEase(Ease.OutQuad);
            sequence.Append(hoverRoot.DOScale(originScale, 0.15f)).SetEase(Ease.OutQuad);

            hoverTween = sequence.SetUpdate(true).SetLink(this.gameObject);
        }

        public virtual void OnClose()
        {
            if (UseCloseAnim && hoverRoot != null)
            {
                CloseAnim();
                return;
            }

            gameObject.SetActive(false);
            OnCloseAnimFinish();
        }
        protected virtual void CloseAnim()
        {
            if (hoverRoot == null) return;

            hoverTween?.Kill();
            hoverTween = null;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(hoverRoot.DOScale(originScale * 1.05f, 0.25f)).SetEase(Ease.OutQuad);
            sequence.Append(hoverRoot.DOScale(minScale, 0.25f)).SetEase(Ease.OutQuad);

            sequence.Insert(0.25f, canvasGroup.DOFade(0f, 0.15f));

            hoverTween = sequence.SetUpdate(true).SetLink(this.gameObject).OnComplete(()=>
                {
                    this.gameObject.SetActive(false);
                    OnCloseAnimFinish();
                });
        }
        protected virtual void OnCloseAnimFinish()
        {
            hoverTween?.Kill();
            hoverTween = null;
        }
        public virtual void CloseUI()
        {
            UIManager.Instance.CloseUI(id);
        }
        public void ActiveWindow()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        public void DeAcviveWindow()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}