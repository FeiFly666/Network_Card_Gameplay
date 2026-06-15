using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase.Entity
{
    public class UserEntity
    {
        [Key]
        [MaxLength(64)]
        public string UserID { get; set; } = string.Empty;
        [Required]
        [MaxLength(32)]
        public string Nickname { get; set; } = string.Empty;
        [Required]
        public bool HasSetNickname { get; set; } = false;
        [Required]
        [MaxLength(128)]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        [MaxLength(64)]
        public string PasswordSalt {  get; set; } = string.Empty;
        [Required]
        [MaxLength(64)]
        public string MainCharacterDataID {  get; set; } = string.Empty;

        public DateTime CreatAtUtc { get; set; } = DateTime.UtcNow;
    }
}
