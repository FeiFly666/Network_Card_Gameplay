using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Assets.Scripts.Model;
using Assets.ObjectPool;

namespace Assets.Scripts.UI.Characters
{
    public class UICharacterValue : MonoBehaviour, IPoolable
    {
        [SerializeField] private TMP_Text hp;
        [SerializeField] private TMP_Text defense;

        private BattleCharacter owner;
        private RectTransform rTransfrom;
        private void Awake()
        {
            rTransfrom = GetComponent<RectTransform>();
        }
        public void OnSpawn()
        {
            if(owner != null)
            {
                gameObject.SetActive(true);
            }
        }
        public void SetOwner(BattleCharacter owner)
        {
            this.owner = owner;

            owner.OnCharacterValueChanged += ChangeValue;
            //owner.OnCharacterDead += DestroyThisUI;

            ChangeValue();

            Vector2 pos = Camera.main.WorldToScreenPoint(owner.transform.position);

            this.rTransfrom.position = pos;
        }

        private void ChangeValue()
        {
            hp.text = owner.character.CurHP.ToString();
            defense.text = owner.character.CurDEF.ToString();
        }
        public void ClearOwner()
        {
            if (owner == null) return;

            owner.OnCharacterValueChanged -= ChangeValue;

            this.owner = null;
        }
        public void OnDeSpawn()
        {
            if (owner == null) return;

            owner.OnCharacterValueChanged -= ChangeValue;
        }
        private void OnDisable()
        {
            //if (owner == null) return;

            //owner.OnCharacterValueChanged -= ChangeValue;
            //owner.OnCharacterDead -= DestroyThisUI;
        }
    }
}
