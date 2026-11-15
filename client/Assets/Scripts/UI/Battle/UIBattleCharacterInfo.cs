using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class UIBattleCharacterInfo : UIWindow
    {
        [SerializeField] private TMP_Text hp;
        [SerializeField] private TMP_Text mp;
        [SerializeField] private TMP_Text def;
        [SerializeField] private TMP_Text atk;
        [SerializeField] private TMP_Text spd;
        [SerializeField] private TMP_Text actionRank;

        private BattleCharacter currentCharacter = null;

        [SerializeField] private float hoverDelay = 0.5f;

        private Coroutine showRoutine;
        private BattleCharacter pendingCharacter;

        [SerializeField] private Vector2 originPos;
        private RectTransform rTransform => GetComponent<RectTransform>();
        private void Awake()
        {
            //rTransform = GetComponent<RectTransform>();
        }

        public override void OnOpen()
        {
            rTransform.position = originPos;
            base.OnOpen();
            BattleCharacterManager.Instance.OnCharacterAttributeChanged += OnOneCharacterAttributeChanged;
            //BattleCharacterManager.Instance.OnPointerEnterCharacter += OnPointerInNewCharacter;
            //BattleCharacterManager.Instance.OnPointerExitCharacter += OnPointerOut;
        }
        public void OnPointerInNewCharacter(BattleCharacter character)
        {
            if (showRoutine != null)
            {
                StopCoroutine(showRoutine);
                showRoutine = null;
            }
            if (character == null)
            {
                currentCharacter = null;
                pendingCharacter = null;
                HideInfo();
                return;
            }
            if(currentCharacter == character)
            {
                UpdateInfo(currentCharacter);
                ShowInfo();
                return;
            }

            pendingCharacter = character;

            showRoutine = StartCoroutine(ShowInfoCoRoutine(character));
        }
/*        private void OnPointerIn(BattleCharacter character)
        {
            if (character == null)
            {
                currentCharacter = null;
                pendingCharacter = null;
                HideInfo();
                return;
            }
            currentCharacter = character;
            UpdateInfo(currentCharacter);
            ShowInfo();
        }*/
        private IEnumerator ShowInfoCoRoutine(BattleCharacter character)
        {
            
            yield return new WaitForSeconds(hoverDelay);

            if (pendingCharacter != character) yield break;

            currentCharacter = character;
            UpdateInfo(currentCharacter);
            ShowInfo();

            showRoutine = null;
        }
        public void OnPointerOut(BattleCharacter character)
        {
            if (showRoutine != null)
            {
                StopCoroutine(showRoutine);
                showRoutine = null;
            }

            if (pendingCharacter == character)
            {
                pendingCharacter = null;
            
            }
            if(currentCharacter == character)
            {
                currentCharacter = null;
                HideInfo();
            }
        }
        public void UpdateInfo(BattleCharacter character)
        {
            currentCharacter = character;

            hp.text = $"{character.character.CurHP} / {character.character.CurMaxHP}";
            mp.text = $"{character.character.CurMP} / {character.character.data.MP}";
            def.text = character.character.CurDEF.ToString();
            atk.text = character.character.CurATK.ToString();
            spd.text = character.character.CurSpeed.ToString();

            ChangeCurrentCharacterRank();
        }

        private void OnOneCharacterAttributeChanged(BattleCharacter changedCharacter)
        {
            if(changedCharacter == currentCharacter)
            {
                UpdateInfo(currentCharacter);
            }
            else if(currentCharacter != null)
            {
                ChangeCurrentCharacterRank();
            }
        }

        private void ChangeCurrentCharacterRank()
        {
            int rank = BattleCharacterManager.Instance.GetCharacterRank(currentCharacter.character.ID);
            if(rank > 0)
            {
                actionRank.text = rank.ToString();
            }

        }
        public override void OnClose()
        {
            base.OnClose();

            if (showRoutine != null)
                StopCoroutine(showRoutine);

            showRoutine = null;
            pendingCharacter = null;
            currentCharacter = null;
            BattleCharacterManager.Instance.OnCharacterAttributeChanged -= OnOneCharacterAttributeChanged;
            //BattleCharacterManager.Instance.OnPointerEnterCharacter -= OnPointerInNewCharacter;
            //BattleCharacterManager.Instance.OnPointerExitCharacter -= OnPointerOut;
        }
        private void HideInfo()
        {
            //rTransform.position = originPos;
            CloseUI();
        }
        private void ShowInfo()
        {
            if (currentCharacter == null)
            {
                CloseUI();
                return;
            }

            Vector2 pos = Camera.main.WorldToScreenPoint(currentCharacter.InfoPos.transform.position);

            rTransform.position = pos;

        }
    }
}