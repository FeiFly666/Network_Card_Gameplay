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

        public void MarkHit()
        {
            Hit = true;
        }
        public void MarkFinish()
        {
            Finish = true;
        }
        public IEnumerator WaitHit()
        {
            while (!Hit)
            {
                yield return null;
            }
        }
        public IEnumerator WaitFinish()
        {
            while (!Finish)
            {
                yield return null;
            }
        }
    }
}
