using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Util
{
    public class BattleAttackHandler
    {
        public bool Hit {  get; set; }
        public bool Finish {  get; set; }
        public bool Cancelled { get; set;}
        private int hitNum = 0;
        public void Refresh()
        {
            Hit = false;
            Finish = false;
            Cancelled = false;
            hitNum = 0;
        }

        public void MarkHit()
        {
            hitNum++;
            
        }
        public void MarkFinish()
        {
            Finish = true;
        }
        public IEnumerator WaitHit(int current)
        {
            while (hitNum < current && !Cancelled)
            {
                yield return null;
            }
        }
        public IEnumerator WaitFinish()
        {
            while (!Finish && !Cancelled)
            {
                yield return null;
            }
        }

        public void Close()
        {
            Cancelled = true;
            Finish = true;
        }
    }
}
