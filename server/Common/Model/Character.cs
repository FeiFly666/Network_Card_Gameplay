using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Character
    {
        public CharacterData data { get; set; }
        public int ID { get; set; }
        public bool isDefend { get; set; }
        public bool isMain { get; set; }
        public int CurMaxHP { get; set; }
        public int CurHP { get; set; }

        public int CurATK { get; set; }
        public int CurDEF{ get; set;}
        public int CurSpeed{ get; set;}

        public List<Buff> Buffs = new List<Buff>();

    }
}
