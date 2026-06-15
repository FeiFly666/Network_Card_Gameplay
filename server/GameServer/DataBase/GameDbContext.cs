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
        }
    }
}
