using GameServer.DataBase.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase
{
    public class GameDbContext : DbContext
    {
        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<FriendShipEntity> FriendShips => Set<FriendShipEntity>();
        public DbSet<UserSpecialEffectEntity> SpecialEffects => Set<UserSpecialEffectEntity>();

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.UserID);
                entity.Property(u=>u.UserID).IsRequired().HasMaxLength(64);
                entity.Property(u=>u.Nickname).IsRequired().HasMaxLength(32).IsUnicode(true);
                entity.Property(u => u.HasSetNickname).IsRequired().HasDefaultValue(false);
                entity.Property(u=>u.PasswordHash).IsRequired().HasMaxLength(128);
                entity.Property(u=>u.PasswordSalt).IsRequired().HasMaxLength(64);
                entity.Property(u=>u.MainCharacterDataID).IsRequired().HasMaxLength(64);
            });

            modelBuilder.Entity<FriendShipEntity>(entity =>
            {
                entity.ToTable("FriendShips");
                entity.HasKey(f => f.ID);
                entity.Property(f => f.UserAID).IsRequired().HasMaxLength(64);
                entity.Property(f => f.UserBID).IsRequired().HasMaxLength(64);
                entity.Property(f => f.RequiredByID).IsRequired().HasMaxLength(64);
                entity.Property(f => f.Status).IsRequired();
                entity.HasIndex(f => new { f.UserAID, f.UserBID }).IsUnique();
            });

            modelBuilder.Entity<UserSpecialEffectEntity>(entity =>
            {
                entity.ToTable("UserSpecialEffects");

                //一个玩家的一个被动只能有一条记录
                entity.HasKey(x => new
                {
                    x.UserID,
                    x.SpecialEffectDataID
                });
                entity.Property(x => x.UserID).IsRequired().HasMaxLength(64);
                entity.Property(x => x.SpecialEffectDataID).IsRequired().HasMaxLength(64);
                entity.Property(x => x.EquippedCharacterDataID).HasMaxLength(64).IsRequired(false);

                // 一个角色的一个槽位只能装备一个被动,未装备的记录不参与唯一性检查
                entity.HasIndex(x => new
                {
                    x.UserID,
                    x.EquippedCharacterDataID,
                    x.SlotIndex
                }).IsUnique() .HasFilter( "[EquippedCharacterDataID] IS NOT NULL " + "AND [SlotIndex] IS NOT NULL");

                // 未装备：角色和槽位都为空,已装备：角色和槽位都有值，槽位只能是 0、1
                entity.HasCheckConstraint(
                    "CK_UserSpecialEffects_Equipment",
                    "([EquippedCharacterDataID] IS NULL AND [SlotIndex] IS NULL) OR " +
                    "([EquippedCharacterDataID] IS NOT NULL " +
                    "AND [SlotIndex] IS NOT NULL " +
                    "AND [SlotIndex] IN (0, 1))");

                // 关联账号；删除账号时一并删除其被动记录
                entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
