using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class SpecialEffect
    {
        public SpecialEffectData Data { get; set; }
        public int slotIndex { get; set; }
        public string equippedCharacterID { get; set; } = "";
    }
}
