using System;
using System.Collections;
using Assets.Scripts.Manager;
using Common.Data;
using Common.Model;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SpecialEffectUI
{
    public class UISpecialEffectInfoPanel : UIWindow
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text descirption;

        [SerializeField] private float rightXLimit = -100f;
        [SerializeField] private float upperHeight = 5f;
        private Coroutine currentRoutine;
        private string currentEffectID = "";
        private string pendingEffectID;

        private Vector2 originPos;

        private RectTransform rect_Transfrom => GetComponent<RectTransform>();

        public override void OnOpen()
        {
            base.OnOpen();

            SpecialEffectsManager.Instance.OnPointerExited += CloseUI;

            originPos = new Vector2(5000, 0);
            this.rect_Transfrom.position = originPos;
        }
        /// <summary>
        /// 显示被动信息
        /// </summary>
        /// <param name="specialEffect">需要显示的被动</param>
        /// <param name="callPos">唤醒位置</param>
        /// <param name="delayTime">延迟显示时间</param>
        public void ShowUp(SpecialEffectData specialEffect, Vector2 callPos, float delayTime)
        {
            if(currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            if(specialEffect == null || callPos == null || delayTime < 0)
            {
                CloseUI();
                return;
            }
            pendingEffectID = specialEffect.DataID;

            currentRoutine = StartCoroutine(Show(specialEffect,callPos,delayTime));
        }
        IEnumerator Show(SpecialEffectData specialEffect, Vector2 callPos, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            if(pendingEffectID != specialEffect.DataID) yield break;

            title.text = specialEffect.Name;
            descirption.text = specialEffect.Description;

            Vector2 newPos = new Vector2(callPos.x, callPos.y + upperHeight);
            if(!UnityEngine.Device.Application.isMobilePlatform && newPos.x > rightXLimit) newPos.x = rightXLimit;

            this.rect_Transfrom.position = newPos;

            currentEffectID = specialEffect.DataID;

            //this.gameObject.SetActive(true);
        }
        public override void CloseUI()
        {
            base.CloseUI();
            Hide();
        }

        public void Hide()
        {
            if(currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            //this.gameObject.SetActive(false);
            this.rect_Transfrom.position = originPos;
        }

        public override void OnClose()
        {
            base.OnClose();
            SpecialEffectsManager.Instance.OnPointerExited -= CloseUI;
        }
    }

}
