using Assets.Scripts.Model;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Manager
{
    public class BattleClickHandler : MonoBehaviour,IPointerClickHandler
    {
        private void Update()
        {
            /*if(Input.GetMouseButtonDown(0)&&!EventSystem.current.IsPointerOverGameObject())
            {
                BattleCharacter character = null;

                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(worldPos);

                if (hit != null)
                {
                    character = hit.GetComponentInParent<BattleCharacter>();
                }

                BattleCharacterManager.Instance.HandleCharacterClicked(character);
                *//*if (character != null)
                {
                    //Debug.Log("发现鼠标点击角色");
                    BattleCharacterManager.Instance.HandleCharacterClicked(character);
                }*//*
            }*/
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            BattleCharacterManager.Instance.HandleCharacterClicked(null);
        }
    }
}