using NetMessage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase.Entity
{
    public class FriendShipEntity
    {
        [Key]
        public long ID { get; set; }
        [Required]
        public string UserAID { get; set; } = string.Empty;
        [Required]
        public string UserBID { get; set; } = string.Empty;
        [Required]
        public string RequiredByID { get; set; } = string.Empty;
        [Required]
        public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
        [Required]
        public FriendStatus Status { get; set; }
        [Required]
        public DateTime UpdateAtUtc { get; set; } = DateTime.UtcNow;
    }
}
