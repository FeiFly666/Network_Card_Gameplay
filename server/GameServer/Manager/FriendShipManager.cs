using Common;
using Common.Model;
using Common.Network;
using GameServer.DataBase.Entity;
using GameServer.DataBase.Service;
using GameServer.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    public class FriendShipManager : Singleton<FriendShipManager>
    {
        private FriendShipService friendShipService = new FriendShipService();

        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserAddFriendRequest>(OnRecieveUserAddFriendRequest);
            MessageDispatcher.Instance.Subscribe<UserGetFriendListRequest>(OnRecieveUserGetFriendRequest);
            MessageDispatcher.Instance.Subscribe<UserHandleFriendRequest>(OnRecieveUserHandleFriendRequest);
            MessageDispatcher.Instance.Subscribe<UserSearchPlayerRequest>(OnRecieveUserSearchPlayer);
            MessageDispatcher.Instance.Subscribe<UserDeleteFriendRequest>(OnRecieveUserRemoveFriendRequest);
        }
        public async void OnUserLogin(ClientSession client)
        {
            if (client.player == null) return;
            var info = await friendShipService.GetFriendInfo(client.player.id);

            if (!info.success)
            {
                return;
            }
            foreach(var friend in info.friends)
            {
                client.player.friendState.SetFriendInfo(friend);
                client.player.friendState.ChangeFriendOnlineStatus(friend.Id, OnlinePlayerManager.Instance.IsPlayerOnline(friend.Id));
            }
            foreach(var request in info.requests)
            {
                client.player.friendState.SetFriendRequestInfo(request);
            }
            foreach (var friendID in client.player.friendState.friends.Keys)
            {
                if (OnlinePlayerManager.Instance.TryGetClientSession(friendID, out var friendSession))
                {
                    friendSession.player.friendState.ChangeFriendOnlineStatus(client.player.id, true);
                }
            }
            Message message = new Message();
            message.Response = new ResponseMessage();

            this.AppendDirtyFriendData(client, message);
            client.Send(message);
        }
        public void OnUserExit(ClientSession client)
        {
            if (client.player == null) return;
            PlayerFriendState friendState = client.player.friendState;

            foreach(var friendID in friendState.friends.Keys)
            {
                if(OnlinePlayerManager.Instance.TryGetClientSession(friendID, out ClientSession friendSession))
                {
                    friendSession.player.friendState.ChangeFriendOnlineStatus(client.player.id, false);
                }
            }
        }
        public void AppendDirtyFriendData(ClientSession session, Message message)
        {
            if (session.player == null) return;
            if (!session.player.friendState.HasDirtyInfo()) return;

            if (message.Response == null)
                message.Response = new ResponseMessage();

            session.player.friendState.AddDirtyInfo(message);
        }
        private async void OnRecieveUserAddFriendRequest(ISession session, UserAddFriendRequest request)
        {
            if (request == null) return;
            ClientSession client = (ClientSession)session;
            if(client.player == null) return;

            string fromID = client.player.id;
            string toID = request.targetID;

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.addFriend = new UserAddFriendResponse();

            Console.WriteLine($"friend ship manager: {client.player.id} try add friend  target:{toID}");

            var addResult = await friendShipService.AddFriendAsync(fromID, toID);

            message.Response.addFriend.Success = addResult.success;
            message.Response.addFriend.Msg = addResult.msg;
            message.Response.addFriend.Info = addResult.info;

            session.Send(message);

            if (addResult.success)
            {
                if (OnlinePlayerManager.Instance.TryGetClientSession(toID, out var targetClient))
                {
                    targetClient.player.friendState.SetFriendRequestInfo(addResult.friend);

                    
                }
            }


        }
        private async void OnRecieveUserHandleFriendRequest(ISession session, UserHandleFriendRequest request)
        {
            if (request == null) return;
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.handleFriendRequest = new UserHandleFriendResponse();
            
            ClientSession client = (ClientSession)session;
            if (client.player == null) return;

            var result = await friendShipService.HandleFriend(client.player.id, request.requestID, request.Accept);

            message.Response.handleFriendRequest.Success = result.success;
            message.Response.handleFriendRequest.Msg = result.msg;

            session.Send(message);

            
        }
        private void OnRecieveUserGetFriendRequest(ISession session, UserGetFriendListRequest request)
        {
            if (request == null) return;
            ClientSession client = (ClientSession)session;
            if (client.player == null) return;

            Message message = new Message();
            message.Response = new ResponseMessage();

            this.AppendDirtyFriendData(client, message);
            client.Send(message);
        }

        private async void OnRecieveUserSearchPlayer(ISession session, UserSearchPlayerRequest request)
        {
            if (request == null) return;

            ClientSession client = (ClientSession)session;
            if (client.player == null) return;

            Message message = new Message();
            message.Response = new ResponseMessage();

            this.AppendDirtyFriendData(client, message);
            message.Response.searchPlayer = new UserSearchPlayerResponse();

            if (string.IsNullOrWhiteSpace(request.Keyword))
            {
                message.Response.searchPlayer.Success = false;
                message.Response.searchPlayer.Msg = "搜索词不能为空";

                session .Send(message);
                return;
            }
            Console.WriteLine($"friend ship manager: {client.player.id} try search player");

            message.Response.searchPlayer.Success = true;
            await friendShipService.SearchPlayer(client.player.id, request.Keyword, message.Response.searchPlayer.Players);

            session.Send(message);
        }
        private async void OnRecieveUserRemoveFriendRequest(ISession session, UserDeleteFriendRequest request)
        {
            if (request == null) return;
            ClientSession client = (ClientSession)session;
            if (client.player == null) return;

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.deleteFriend = new UserDeleteFriendResponse();

            var deleteResult = await friendShipService.RemoveFriendAsync(client.player.id, request.deleteID);

            message.Response.deleteFriend.Success = deleteResult.success;
            message.Response.deleteFriend.Msg = deleteResult.msg;

            session .Send(message);
        }
    }
}
