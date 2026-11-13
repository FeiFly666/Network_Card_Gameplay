using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using Common;
using Common.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Service
{
    public class FriendService :Singleton<FriendService>
    {
        public Action<List<PlayerSearchInfo>> OnSearchFinished;
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserFriendListChangedBroadcast>(OnRecieveFriendList);
            MessageDispatcher.Instance.Subscribe<UserAddFriendResponse>(OnRecieveAddFriend);
            MessageDispatcher.Instance.Subscribe<UserSearchPlayerResponse>(OnRecieveSearchInfo);
            MessageDispatcher.Instance.Subscribe<UserDeleteFriendResponse>(OnRecieveRemoveFriend);
        }

        public void SendGetFriendList()
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.getFriendList = new UserGetFriendListRequest();

            NetClient.Instance.Send(message);
        }
        public void SendSearchRequest(string keyword)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.searchPlayer = new UserSearchPlayerRequest();
            message.Request.searchPlayer.Keyword = keyword;

            NetClient.Instance.Send(message);
        }
        public void SendAddFriendRequest(string targetID)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.addFriend = new UserAddFriendRequest();
            message.Request.addFriend.targetID = targetID;

            NetClient.Instance.Send(message);
        }
        public void SendHandleFriendRequest(long requestID, bool accept)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.handleFriendRequest = new UserHandleFriendRequest();
            message.Request.handleFriendRequest.requestID = requestID;
            message.Request.handleFriendRequest.Accept = accept;

            NetClient.Instance.Send(message);
        }
        public void SendRemoveFriendRequest(string targetID)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.deleteFriend = new UserDeleteFriendRequest();
            message.Request.deleteFriend.deleteID = targetID;

            NetClient.Instance.Send(message);
        }
        private void OnRecieveFriendList(ISession session, UserFriendListChangedBroadcast broadcast)
        {
            if (broadcast == null) return;
            Debug.Log($"接受到玩家好友信息： 好友数量：{broadcast.Friends.Count} 请求数量：{broadcast.Requests.Count} 删除好友数：{broadcast.removedFriendIDs.Count} 删除请求数：{broadcast.removedRequestIDs.Length}");

            FriendManager.Instance.UpdateInfo(broadcast);
        }
        private void OnRecieveAddFriend(ISession session, UserAddFriendResponse response)
        {
            if (response == null) return;
            if(!response.Success)
            {
                MessageBox.Show(UI.Common.MessageType.Warning, response.Msg);
            }
            else
            {
                FriendManager.Instance.UpdateSearchInfo(response.Info);
            }
        }
        private void OnRecieveSearchInfo(ISession session, UserSearchPlayerResponse response)
        {
            if (response == null) return;
            Debug.Log($"接受到玩家查询结果, 结果：{response.Success} 长度：{response.Players.Count}");
            if(!response.Success)
            {
                MessageBox.Show(UI.Common.MessageType.Warning, response.Msg);
                return;
            }
            OnSearchFinished?.Invoke(response.Players);
        }

        private void OnRecieveRemoveFriend(ISession session, UserDeleteFriendResponse response)
        {
            if (response == null) return;
            MessageBox.Show(UI.Common.MessageType.Warning, response.Msg);
        }

    }
}
