using Common;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class FriendManager : Singleton<FriendManager>
    {
        private Dictionary<string, FriendInfo> friends = new Dictionary<string, FriendInfo>();
        private Dictionary<long, FriendRequestInfo> requests = new Dictionary<long, FriendRequestInfo>();

        public Action<FriendInfo> OnFriendChange;
        public Action<FriendRequestInfo> OnRequestChange;
        public Action<string> OnRemoveFriend;
        public Action<long> OnRemoveRequest;

        public Action<PlayerSearchInfo> OnUpdateSearchedInfo;
        public void Init()
        {

        }
        public void CallActionForCurrentData()
        {
            foreach(var friend in friends.Values)
            {
                OnFriendChange(friend);
            }
            foreach(var request in requests.Values)
            {
                OnRequestChange(request);
            }
        }
        public void UpdateInfo(UserFriendListChangedBroadcast message)
        {
            foreach(var friendInfo in message.Friends)
            {
                if(!friends.TryGetValue(friendInfo.Id, out var friend))
                {
                    FriendInfo newInfo = new FriendInfo();
                    friend = newInfo;
                    friends.Add(friendInfo.Id, friend);
                }
                friend.Id = friendInfo.Id;
                friend.Nickname = friendInfo.Nickname;
                friend.mainCharacterID = friendInfo.mainCharacterID;
                friend.Online = friendInfo.Online;

                Debug.Log($"处理玩家好友 {friend.Id}");
                OnFriendChange?.Invoke(friend);
            }

            foreach(var requestInfo in message.Requests)
            {
                if(!requests.TryGetValue(requestInfo.Id, out var request))
                {
                    FriendRequestInfo newInfo = new FriendRequestInfo();
                    request = newInfo;

                    requests.Add(requestInfo.Id, request);
                }
                request.Id = requestInfo.Id;
                request.requesterID = requestInfo.requesterID;
                request.requesterNickname = requestInfo.requesterNickname;
                request.requestMainCharacterID = requestInfo.requestMainCharacterID;
                request.createTime = requestInfo.createTime;

                Debug.Log($"处理玩家好友请求 {request.Id}");
                OnRequestChange?.Invoke(request);
            }

            foreach(var deleteFriend in message.removedFriendIDs)
            {
                if(friends.TryGetValue(deleteFriend,out var friend))
                {
                    OnRemoveFriend?.Invoke(deleteFriend);
                    friends.Remove(deleteFriend);
                }
            }
            if (message.removedRequestIDs == null) return;

            foreach(var deleteRequest in message.removedRequestIDs)
            {
                if(requests.TryGetValue(deleteRequest,out var request))
                {
                    OnRemoveRequest?.Invoke(deleteRequest);
                    requests.Remove(deleteRequest);
                }
            }
        }
        public void UpdateSearchInfo(PlayerSearchInfo info)
        {
            OnUpdateSearchedInfo?.Invoke(info);
        }
    }
}
