using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Model
{
    public class UIFakeCard : UICard
    {
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
            }
            else
            {
                anim.SetTrigger("Use");
            };
        }
    }
}
