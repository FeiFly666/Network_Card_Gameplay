using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Buff
    {
        public BuffData Data { get; set; }
        public Dictionary<BuffData, int> RemainRound { get; set; } = new Dictionary<BuffData, int>();
        public int OwnerID { get; set; }
    }
}
