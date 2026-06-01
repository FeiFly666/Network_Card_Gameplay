using Assets.Scripts.Manager;
using Assets.Scripts.UI.Battle;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Model
{
    public class UIChaCard : UICard
    {
        public override void HandleDrag(UIDragArea area, BattleCharacter character)
        {
            if (area == null)
            {
                Debug.Log($"ui character card {card.ID} : did not find area, back");
                ReturnToOrigin(); return;
            }

            switch (area.type)
            {
                case DragAreaType.Battle:
                    //TODO：生成人物，然后回调成功就销毁这个卡牌，失败则回原位
                    if (!BattleCharacterManager.Instance.CanAddPlayerCha || !BattleCardManager.Instance.SendSpawnChaRequest(this))
                    {
                        ReturnToOrigin();
                    }
                    break;
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
