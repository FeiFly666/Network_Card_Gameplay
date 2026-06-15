using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class DirtyFriendInfo
    {
        public FriendInfo friendInfo;
        public bool isDirty;
        public bool isRemove;
    }
    public class DirtyFriendRequestInfo
    {
        public FriendRequestInfo requestInfo;
        public bool isDirty;
        public bool isRemove;
    }
    public class PlayerFriendState
    {
        public Dictionary<string, DirtyFriendInfo> friends = new Dictionary<string, DirtyFriendInfo>();
        public Dictionary<long, DirtyFriendRequestInfo> requests = new Dictionary<long, DirtyFriendRequestInfo>();
        private List<long> pendingRequestID = new List<long>();
        public void ChangeFriendOnlineStatus(string friendID, bool online)
        {
            if(!friends.TryGetValue(friendID, out DirtyFriendInfo friendInfo)) {  return; }

            friendInfo.friendInfo.Online = online;
            friendInfo.isDirty = true;
        }
        public void AddDirtyInfo(Message message)
        {
            pendingRequestID.Clear();
            if (message.Response == null)
                message.Response = new ResponseMessage();
            if (message.Response.friendList == null)
                message.Response.friendList = new UserFriendListChangedBroadcast();
            foreach (var info in friends.Values)
            {
                if (info.isDirty)
                {
                    if (info.isRemove) message.Response.friendList.removedFriendIDs.Add(info.friendInfo.Id);
                    else message.Response.friendList.Friends.Add(info.friendInfo);

                    info.isDirty = false;
                }
            }
            foreach (var info in requests.Values)
            {
                if (info.isDirty)
                {
                    if(info.isRemove)
                    {
                        pendingRequestID.Add(info.requestInfo.Id);
                    }
                    else message.Response.friendList.Requests.Add(info.requestInfo);

                    info.isDirty = false;
                }
            }
            message.Response.friendList.removedRequestIDs = pendingRequestID.ToArray();
            ApplyRemove();
        }
        private void ApplyRemove()
        {
            foreach(var key in requests.Keys.ToList())
            {
                if (requests[key].isRemove) requests.Remove(key);
            }
            foreach (var key in friends.Keys.ToList())
            {
                if (friends[key].isRemove) friends.Remove(key);
            }
        }
        public void SetFriendInfo(FriendInfo info)
        {
            string friendID = info.Id;
            if(friends.TryGetValue(friendID, out var friend))
            {
                friend.friendInfo = info;
            }
            else
            {
                DirtyFriendInfo newInfo = new DirtyFriendInfo();
                newInfo.friendInfo = info;
                friends.Add(friendID, newInfo);
                friend = newInfo;
            }
            friend.isDirty = true;
            friend.isRemove = false;
        }
        public void SetFriendRequestInfo(FriendRequestInfo info)
        {
            long requestID = info.Id;
            if(requests.TryGetValue(requestID, out var request))
            {
                request.requestInfo = info;
            }
            else
            {
                DirtyFriendRequestInfo newInfo = new DirtyFriendRequestInfo();
                newInfo.requestInfo = info;
                requests.Add(requestID, newInfo);
                request = newInfo;
            }
            request.isDirty = true;
            request.isRemove = false;
        }
        public void MarkRemoveFriend(string friendID)
        {
            if(friends.TryGetValue(friendID, out var friendInfo))
            {
                friendInfo.isRemove = true;
                friendInfo.isDirty = true;
            }
        }
        public void MarkRemoveRequest(long id)
        {
            if(requests.TryGetValue(id, out var request))
            {
                request.isRemove = true;
                request.isDirty = true;
            }
        }
        public bool HasDirtyInfo()
        {
            foreach(var info in friends.Values)
            {
                if(info.isDirty)
                {
                    return true;
                }
            }
            foreach (var info in requests.Values)
            {
                if (info.isDirty)
                {
                    return true;
                }
            }
            return false;
        }
        public void Clear()
        {
            friends.Clear();
            requests.Clear();
            pendingRequestID.Clear();
        }

    }
}
