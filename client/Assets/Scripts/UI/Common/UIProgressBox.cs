using Assets.Scripts.Manager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public class UIProgressBox : UIWindow
    {
        [SerializeField] private TMP_Text description;
        [SerializeField] private Image progress;
        [SerializeField] private Image progressObj;
        public override void OnOpen()
        {
            base.OnOpen();

        }
        public void ShowDescription()
        {
            description.text = "";
            description.gameObject.SetActive(true);
        }
        public void SetDescription(string sentence)
        {
            if(description.text != sentence)
                description.text = sentence;
        }
        public void CloseDescription()
        {
            description.gameObject.SetActive(false);
        }
        public void ShowProgress()
        {
            progress.fillAmount = 0;
            progressObj.gameObject?.SetActive(true);
        }
        public void UpdateProgress(float current, float max)
        {
            progress.fillAmount = current/max;
        }
        public void CloseProgress()
        {
            progressObj.gameObject?.SetActive(false);
        }

        public override void CloseUI()
        {
            UIManager.Instance.ClosePopup(gameObject);
        }

        protected override void OnCloseAnimFinish()
        {
            base.OnCloseAnimFinish();

            if (!gameObject.activeSelf)
                UIManager.Instance.DestroyPopup(gameObject);
        }
    }
}