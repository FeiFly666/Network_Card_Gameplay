using Assets.Scripts.Manager;
using Assets.Scripts.UI.Battle;
using Common.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Model
{
    public class UIEffectCard : UICard
    {
        public override void HandleDrag(UIDragArea area, BattleCharacter character)
        {
            if(character != null)
            {
                if(character.ownerID == PlayerInfoManager.Instance.userID)
                {
                    if (!BattleCardManager.Instance.SendAttributeChangeRequest(this, character))
                    {
                        ReturnToOrigin();
                    }
                }
                else
                {
                    ReturnToOrigin();
                }
                return;
            }

            if (area == null)
            {
                Debug.Log($"ui character card {card.ID} : did not find area, back");
                ReturnToOrigin(); return;
            }

            switch (area.type)
            {
                case DragAreaType.Discard:
                    //TODO:弃牌,然后回调成功就销毁这个卡牌，失败则回原位
                    if (!BattleCardManager.Instance.SendDiscardRequest(this))
                    {
                        ReturnToOrigin();
                    }
                    break;
                default:
                    ReturnToOrigin();
                    break;
            }
        }
    }
}