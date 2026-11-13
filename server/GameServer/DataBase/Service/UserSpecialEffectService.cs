using Common.Model;
using GameServer.DataBase.Entity;
using GameServer.Manager;
using Microsoft.EntityFrameworkCore;
using NetMessage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase.Service
{
    public enum SpecialEffectHandle
    {
        Equip = 0,
        UnEquip = 1
    }
    public class UserSpecialEffectService
    {
        private readonly DbContextOptions<GameDbContext> options;

        public UserSpecialEffectService()
        {
            options = new DbContextOptionsBuilder<GameDbContext>().UseSqlServer(DataManager.Instance.DbConnectionString).Options;
        }
        public async Task<bool> InitManagerData()
        {
            await using var db = new GameDbContext(options);
            await db.SpecialEffects.ForEachAsync(entity =>
            {
                UpdateServerData(entity);
            });
            return true;
        }
        public void UpdateServerData(UserSpecialEffectEntity? entity)
        {
            if (entity == null) return;
            string dataID = entity.SpecialEffectDataID;
            if (entity.EquippedCharacterDataID != null)
            {
                string userID = entity.UserID;
                string characterID = entity.EquippedCharacterDataID;
                int slotIndex = entity.SlotIndex ?? 0;
                SpecialEffectManager.Instance.SetSlotEffect((userID, characterID, slotIndex), dataID);
            }
        }
        //目前默认全部解锁，在注册账号的时候或者更新旧帐号数据库信息调用
        public async Task<bool> UnlockAllSpecialEffects(string userID)
        {
            await using var db = new GameDbContext(options);

            UserEntity? User = await db.Users.FirstOrDefaultAsync(u => u.UserID == userID);
            if (User == null)
            {
                return false;
            }
            bool isChanged = false;
            foreach(var dataID in DataManager.Instance.m_chaSpeicalEffect.Keys)
            {
                if (await db.SpecialEffects.AnyAsync(u => u.UserID == userID && u.SpecialEffectDataID == dataID)) continue;

                isChanged = true;

                UserSpecialEffectEntity addData = new UserSpecialEffectEntity();
                addData.UserID = userID;
                addData.UnlockedAtUtc = DateTime.UtcNow;
                addData.SpecialEffectDataID = dataID;

                db.SpecialEffects.Add(addData);
            }
            if(isChanged) await db.SaveChangesAsync();

            return true;
        }

        public async Task<(bool success, string msg, HashSet<UserSpecialEffectInfo>? effects)> GetListAsync(string userID)
        {
            await using var db = new GameDbContext(options);

            UserEntity? User = await db.Users.FirstOrDefaultAsync(u => u.UserID == userID);
            if (User == null)
            {
                return (false, "未找到玩家数据", null);
            }

            HashSet<UserSpecialEffectInfo> specialEffects = new HashSet<UserSpecialEffectInfo>();

            foreach(var effect in await db.SpecialEffects.Where(u => u.UserID == userID).ToListAsync())
            {
                if (!DataManager.Instance.m_chaSpeicalEffect.ContainsKey(effect.SpecialEffectDataID)) continue;

                UserSpecialEffectInfo info = new UserSpecialEffectInfo();
                info.specialEffectDataID = effect.SpecialEffectDataID;
                info.equippedCharacterDataID = effect.EquippedCharacterDataID;
                info.slotIndex = effect.SlotIndex ?? 0;

                specialEffects.Add(info);
            }

            return (true,"获取完成", specialEffects);
        }
        public async Task<(bool success, string msg)> ChangeSpeicalEffectStatus(SpecialEffectHandle option, string userID, string specialEffectID, string? equipCharacterID, int? slotIndex)
        {
            await using var db = new GameDbContext(options);

            UserEntity? User = await db.Users.FirstOrDefaultAsync(u => u.UserID == userID);
            if (User == null)
            {
                return (false, "未找到玩家数据");
            }

            UserSpecialEffectEntity? entity = await db.SpecialEffects.FirstOrDefaultAsync(u => u.UserID == userID && u.SpecialEffectDataID == specialEffectID);
            UserSpecialEffectEntity? slotEffect = null;

            string? oldCharacterIDForSlot = null;
            int? oldSlotIndexForSlot = null;

            if (entity == null)
            {
                return (false, "未找到该玩家持有的此特殊效果");
            }

            string? oldCharacterIDForEntity = entity.EquippedCharacterDataID;
            int? oldSlotIndexForEntity = entity.SlotIndex;


            if (option == SpecialEffectHandle.Equip)
            {
                if(equipCharacterID == null || slotIndex == null)
                {
                    return (false, "未输入装备角色ID或槽位ID");
                }

                //【注意】此处以后如改角色为解锁形式，则此处需要判断玩家是否拥有该角色。

                if(!DataManager.Instance.m_characters.ContainsKey(equipCharacterID)) { return (false, "未找到指定角色ID"); }
                if(slotIndex > 1 || slotIndex < 0) return (false, "指定槽位无效");

                slotEffect = await db.SpecialEffects.FirstOrDefaultAsync(x => x.UserID == userID && x.EquippedCharacterDataID == equipCharacterID && x.SlotIndex == slotIndex && x.SpecialEffectDataID != specialEffectID);

                if (slotEffect != null)//卸载已装备的在该槽位的被动
                {
                    oldCharacterIDForSlot = slotEffect.EquippedCharacterDataID;
                    oldSlotIndexForSlot = slotEffect.SlotIndex;

                    slotEffect.EquippedCharacterDataID = null;
                    slotEffect.SlotIndex = null;
                }

                entity.EquippedCharacterDataID = equipCharacterID;
                entity.SlotIndex = slotIndex;

            }
            else if(option == SpecialEffectHandle.UnEquip)
            {
                entity.EquippedCharacterDataID = null;
                entity.SlotIndex = null;
            }
            await db.SaveChangesAsync();

            if (oldCharacterIDForEntity != null && oldSlotIndexForEntity.HasValue &&(oldCharacterIDForEntity != entity.EquippedCharacterDataID || oldSlotIndexForEntity != entity.SlotIndex))
            {
                SpecialEffectManager.Instance.RemoveSlotEffect((userID, oldCharacterIDForEntity, oldSlotIndexForEntity.Value),specialEffectID);
            }

            if(slotEffect != null && oldCharacterIDForSlot != null && oldSlotIndexForSlot.HasValue)
            {
                SpecialEffectManager.Instance.RemoveSlotEffect((userID, oldCharacterIDForSlot, oldSlotIndexForSlot.Value), slotEffect.SpecialEffectDataID);
            }


            UpdateServerData(entity);
            UpdateServerData(slotEffect);

            return (true, "修改特殊效果成功");
        }
    }
}
