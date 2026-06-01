using Assets.Scripts.Manager;
using Assets.Scripts.UI.Battle;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class UIBuffCard : UICard
    {
        public override void HandleDrag(UIDragArea area, BattleCharacter character)
        {
            if(character != null)//角色操作
            {
                return;
            }

            if (area == null)
            {
                Debug.Log($"ui buff card {card.ID} : did not find area, back");
                ReturnToOrigin(); return;
            }

            switch (area.type)
            {
                case DragAreaType.Discard:
                    //TODO:弃牌,然后回调成功就销毁这个卡牌，失败则回原位
                    if(!BattleCardManager.Instance.SendDiscardRequest(this))
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
