using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase.Entity
{
    public class UserSpecialEffectEntity
    {
        public string UserID { get; set; } = "";
        public string SpecialEffectDataID { get; set; } = "";
        public string? EquippedCharacterDataID { get; set; }
        public int? SlotIndex { get; set; }
        public DateTime UnlockedAtUtc { get; set; } = DateTime.UtcNow;

    }
}
