using GameServer.DataBase.Entity;
using GameServer.Manager;
using GameServer.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase.Service
{
    public class UserAccountService
    {
        private readonly DbContextOptions<GameDbContext> options;

        public UserAccountService()
        {
            options = new DbContextOptionsBuilder<GameDbContext>().UseSqlServer(DataManager.Instance.DbConnectionString).Options;
        }
        public async Task<(bool success, string msg)> RegisterAsync(string userID, string password, string? mainCharacterDataID)
        {
            if(string.IsNullOrEmpty(userID))
            {
                return (false, "用户ID不能为空");
            }
            if(!IsValidUserID(userID))
            {
                return (false, "用户ID格式有误，[长度不超过18且为纯数字]");
            }
            if(string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }
            if(string.IsNullOrEmpty(mainCharacterDataID))
            {
                mainCharacterDataID = "character1";
            }

            await using var db = new GameDbContext(options);

            bool exists = await db.Users.AnyAsync(u => u.UserID == userID);
            if (exists)
                return (false, "用户已存在");

            var passwordData = PasswordHasher.HashPassword(password);
            var user = new UserEntity
            {
                UserID = userID,
                Nickname = userID,
                HasSetNickname = false,
                PasswordHash = passwordData.hash,
                PasswordSalt = passwordData.salt,
                MainCharacterDataID = mainCharacterDataID
            };

            db.Users.Add(user);

            await db.SaveChangesAsync();

            return (true, "注册成功");
        }

        public async Task<(bool success, string msg, UserEntity? user)> LoginAsync(string userID, string password)
        {
            await using var db = new GameDbContext(options);

            UserEntity? user = await db.Users.FirstOrDefaultAsync(u => u.UserID == userID);

            if (user == null)
            {
                return (false, "用户不存在或密码错误", null);
            }

            bool isPasswordCorrect = PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

            if(!isPasswordCorrect)
            {
                return (false, "用户不存在或密码错误", null);
            }
            return (true, "", user);
        }
        public async Task<bool> SetMainCharacterAsync(string userId, string mainCharacterDataId)
        {
            await using var db = new GameDbContext(options);

            UserEntity? user = await db.Users.FindAsync(userId);

            if (user == null)
                return false;

            user.MainCharacterDataID = mainCharacterDataId;

            await db.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SetNickname(string userID, string Nickname)
        {
            await using var db = new GameDbContext(options);

            UserEntity? user = await db.Users.FindAsync(userID);
            if (user == null) return false;
            if(!IsValidNickname(Nickname)) { return false; }
            user.Nickname = Nickname;
            user.HasSetNickname = true;

            await db.SaveChangesAsync();
            return true;
        }
        private static bool IsValidUserID(string userID)
        {
            return !string.IsNullOrWhiteSpace(userID)
                && userID.Length <= 18
                && userID.All(char.IsDigit);
        }
        private static bool IsValidNickname(string nickname)
        {
            return !string.IsNullOrWhiteSpace(nickname)
                && nickname.Length <= 16
                && !nickname.Any(char.IsControl);
        }
    }
}
