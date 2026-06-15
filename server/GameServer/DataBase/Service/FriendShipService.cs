using GameServer.DataBase.Entity;
using GameServer.Manager;
using Microsoft.EntityFrameworkCore;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase.Service
{
    public class FriendShipService
    {
        private readonly DbContextOptions<GameDbContext> options;
        private const int rejectTimeLimit = 60;
        private const int deleteTimeLimit = 60 * 24 * 7;
        public FriendShipService()
        {
            options = new DbContextOptionsBuilder<GameDbContext>().UseSqlServer(DataManager.Instance.DbConnectionString).Options;
        }

        public async Task<(bool success, string msg, FriendRequestInfo? friend)> AddFriendAsync(string fromID, string toID)
        {
            await using var db = new GameDbContext(options);

            UserEntity? requester = await db.Users.FirstOrDefaultAsync(u => u.UserID == fromID);
            if(requester == null)
            {
                return (false, "请求方不存在", null);
            }

            UserEntity? target = await db.Users.FirstOrDefaultAsync(u => u.UserID == toID);
            if(target == null)
            {
                return (false, "目标不存在", null);
            }
            string requesterID = fromID;
            if(fromID.Equals(toID))
            {
                return (false, "不能自己添加自己", null);
            }
            string userAID = string.CompareOrdinal(fromID, toID) < 0 ? fromID : toID;
            string userBID = string.CompareOrdinal(fromID, toID) < 0 ? toID : fromID;
            FriendShipEntity? friendShip = await db.FriendShips.FirstOrDefaultAsync(f => f.UserAID == userAID && f.UserBID == userBID);
            bool isNew = false;
            if(friendShip != null)
            {
                if(friendShip.Status == FriendStatus.FriendStatusPending)
                {
                    return (false, $"不可重复申请", null);
                }
                else if(friendShip.Status == FriendStatus.FriendStatusAccepted)
                {
                    return (false, $"已添加该玩家为好友", null);
                }
                else if(friendShip.Status == FriendStatus.FriendStatusRejected || friendShip.Status == FriendStatus.FriendStatusDelete)
                {
                    DateTime nextAvailableTime = friendShip.UpdateAtUtc.AddMinutes(friendShip.Status == FriendStatus.FriendStatusRejected ? rejectTimeLimit : deleteTimeLimit);
                    //未到达时限不可重复申请
                    if (DateTime.UtcNow < nextAvailableTime )
                    {
                        TimeSpan leftMinutes = nextAvailableTime - DateTime.UtcNow;

                        int leftMinutesNum = (int)leftMinutes.TotalMinutes;

                        return (false, $"请在{leftMinutesNum}分钟后再尝试添加该玩家", null);
                    }
                }
            }
            else
            {
                isNew = true;
                friendShip = new FriendShipEntity();
                friendShip.UserAID = userAID;
                friendShip.UserBID = userBID;
            }

            friendShip.UpdateAtUtc = DateTime.UtcNow;
            friendShip.CreateAtUtc = DateTime.UtcNow;
            friendShip.RequiredByID = requesterID;
            friendShip.Status = FriendStatus.FriendStatusPending;

            if(isNew)
            {
                db.FriendShips.Add(friendShip);
            }

            await db.SaveChangesAsync();

            FriendRequestInfo res = new FriendRequestInfo();
            res.Id = friendShip.ID;
            res.requesterID = requesterID;
            res.requesterNickname = requester.Nickname;
            res.requestMainCharacterID = requester.MainCharacterDataID;
            res.createTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return (true, "已向该玩家发送好友请求", res);
        }

        public async Task<(bool success, string msg ,HashSet<FriendInfo>? friends, HashSet<FriendRequestInfo>? requests)> GetFriendInfo(string userID)
        {
            await using var db = new GameDbContext(options);

            UserEntity? User = await db.Users.FirstOrDefaultAsync(u => u.UserID == userID);
            if (User == null)
            {
                return (false, "未找到玩家数据", null, null);
            }

            HashSet<FriendInfo> friends = new HashSet<FriendInfo>();
            HashSet<FriendRequestInfo> requests = new HashSet<FriendRequestInfo>();

            foreach(var ship in await db.FriendShips.Where(s => s.UserAID == userID || s.UserBID == userID).ToListAsync())
            {
                UserEntity? friend = await db.Users.FirstOrDefaultAsync(u => u.UserID == (ship.UserAID == userID ? ship.UserBID : ship.UserAID));
                if (friend != null)
                {
                    if (ship.Status == FriendStatus.FriendStatusPending)
                    {
                        if (ship.RequiredByID == userID) continue;
                        FriendRequestInfo requestInfo = new FriendRequestInfo();
                        requestInfo.Id = ship.ID;
                        requestInfo.requesterID = ship.RequiredByID;
                        requestInfo.requesterNickname = friend.Nickname;
                        requestInfo.requestMainCharacterID = friend.MainCharacterDataID;
                        DateTime utcCreateTime = DateTime.SpecifyKind(ship.CreateAtUtc, DateTimeKind.Utc);
                        requestInfo.createTime = new DateTimeOffset(utcCreateTime).ToUnixTimeMilliseconds();

                        requests.Add(requestInfo);
                    }
                    else if (ship.Status == FriendStatus.FriendStatusAccepted)
                    {
                        FriendInfo friendInfo = new FriendInfo();

                        friendInfo.Id = friend.UserID;
                        friendInfo.Nickname = friend.Nickname;
                        friendInfo.mainCharacterID = friend.MainCharacterDataID;
                        friendInfo.Online = false;

                        friends.Add(friendInfo);
                    }
                }
            }

            
            return (true, "", friends, requests);
        }

        public async Task<(bool success, string msg)> SearchPlayer(string selfID, string keyword, List<PlayerSearchInfo> search)
        {
            await using var db = new GameDbContext(options);

            foreach(var player in await db.Users.Where(u => u.UserID == keyword || u.Nickname == keyword).ToListAsync())
            {
                string userAID = string.CompareOrdinal(selfID, player.UserID) < 0 ? selfID : player.UserID;
                string userBID = string.CompareOrdinal(selfID, player.UserID) < 0 ? player.UserID : selfID;
                FriendShipEntity? friendShip = await db.FriendShips.FirstOrDefaultAsync(f => f.UserAID == userAID && f.UserBID == userBID);
                PlayerSearchInfo newSearchInfo = new PlayerSearchInfo();

                newSearchInfo.userID = player.UserID;
                newSearchInfo.Nickname = player.Nickname;
                newSearchInfo.mainCharacterID = player.MainCharacterDataID;
                newSearchInfo.Online = OnlinePlayerManager.Instance.IsPlayerOnline(player.UserID);
                newSearchInfo.Relation = GetRelation(selfID, player.UserID, friendShip);

                search.Add(newSearchInfo);
            }

            return (true, "");
        }
        private FriendRelationState GetRelation(string selfID, string targetID, FriendShipEntity? ship)
        {
            if (selfID == targetID)
                return FriendRelationState.FriendRelationSelf;

            if (ship == null)
                return FriendRelationState.FriendRelationNone;

            if (ship.Status == FriendStatus.FriendStatusAccepted)
                return FriendRelationState.FriendRelationFriend;

            if (ship.Status == FriendStatus.FriendStatusPending)
            {
                if (ship.RequiredByID == selfID)
                    return FriendRelationState.FriendRelationRequestSent;

                return FriendRelationState.FriendRelationRequestReceived;
            }

            return FriendRelationState.FriendRelationNone;
        }

        public async Task<(bool success, string msg)> HandleFriend(long id, bool accept)
        {
            await using var db = new GameDbContext(options);

            FriendShipEntity? friendShip = await db.FriendShips.FirstOrDefaultAsync(f => f.ID == id);

            if(friendShip == null)
            {
                return (false, "请求不存在");
            }

            if(friendShip.Status != FriendStatus.FriendStatusPending)
            {
                return (false, "请求已答复");
            }

            friendShip.Status = accept ? FriendStatus.FriendStatusAccepted : FriendStatus.FriendStatusRejected;

            await db.SaveChangesAsync();

            if(friendShip.Status == FriendStatus.FriendStatusAccepted)
            {
                string userAID = friendShip.UserAID;
                string userBID = friendShip.UserBID;
                //玩家A在线则向其发送玩家B信息
                if(OnlinePlayerManager.Instance.TryGetClientSession(userAID, out var sessionA))
                {
                    UserEntity? userB = await db.Users.FirstOrDefaultAsync(u => u.UserID ==  userBID);
                    if(userB == null)
                    {
                        return (false, "请求包含不存在用户");
                    }
                    FriendInfo infoB = new FriendInfo();
                    infoB.Id = userBID;
                    infoB.Nickname = userB.Nickname;
                    infoB.mainCharacterID = userB.MainCharacterDataID;
                    infoB.Online = OnlinePlayerManager.Instance.IsPlayerOnline(userBID);
                    sessionA.player.friendState.SetFriendInfo(infoB);
                }
                //玩家B在线则向其发送玩家A信息
                if (OnlinePlayerManager.Instance.TryGetClientSession(userBID, out var sessionB))
                {
                    UserEntity? userA = await db.Users.FirstOrDefaultAsync(u => u.UserID == userAID);
                    if(userA == null)
                    {
                        return (false, "请求包含不存在用户");
                    }
                    FriendInfo infoA = new FriendInfo();
                    infoA.Id = userAID;
                    infoA.Nickname = userA.Nickname;
                    infoA.mainCharacterID = userA.MainCharacterDataID;
                    infoA.Online = OnlinePlayerManager.Instance.IsPlayerOnline(userAID);

                    sessionB.player.friendState.SetFriendInfo(infoA);
                }
                //删除接收方的请求
                string targetID = friendShip.RequiredByID == userAID? userBID : userAID;
                if (OnlinePlayerManager.Instance.TryGetClientSession(targetID, out var targetSession))
                {
                    targetSession.player.friendState.MarkRemoveRequest(id);
                }
            }

            return (true, "请求答复成功");
        }
    }
}
