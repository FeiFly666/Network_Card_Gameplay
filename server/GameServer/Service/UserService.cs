using Common;
using Common.Model;
using Common.Network;
using GameServer.DataBase.Service;
using GameServer.Manager;
using GameServer.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    public class UserService : Singleton<UserService>
    {
        private UserAccountService accountService = new UserAccountService();
        public UserService() { }
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserLoginRequest>(OnPlayerLogin);
            MessageDispatcher.Instance.Subscribe<UserRegisterRequest>(OnPlayerRegister);
            MessageDispatcher.Instance.Subscribe<UserChangeMainChaRequest>(OnPlayerChangeMainCharacter);
            MessageDispatcher.Instance.Subscribe<UserChangeNicknameRequest>(OnPlayerChangeNickname);
        }
        private async void OnPlayerRegister(ISession session, UserRegisterRequest request)
        {
            if (request == null) return;
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.Register = new UserRegisterResponse();

            string playerID = request.userID;
            string password = request.Password;

            Console.WriteLine($"UserService : player {request.userID} try regist");
            var registResult = await accountService.RegisterAsync(playerID, password, null);
            message.Response.Register.Success = registResult.success;
            message.Response.Register.Msg = registResult.msg;
            session.Send(message);

        }
        private async void OnPlayerLogin(ISession session, UserLoginRequest request)
        {
            if (request == null) return;
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.Login = new UserLoginResponse();

            string playerID = request.userID;
            string password = request.Password;

            Console.WriteLine($"UserService : player {request.userID} try login");

            var loginResult = await accountService.LoginAsync(playerID, password);
            if(!loginResult.success)
            {
                message.Response.Login.Success = false;
                message.Response.Login.Msg = loginResult.msg;
                session.Send(message);
                return;
            }

            ClientSession client = OnlinePlayerManager.Instance.OnPlayerLogin(session, request);
            FriendShipManager.Instance.OnUserLogin((ClientSession)session);

            SpecialEffectManager.Instance.UnlockAllEffects(client.player.id);

            client.player.mainCharcterID = loginResult.user!.MainCharacterDataID;
            client.player.nickname = loginResult.user!.Nickname;

            message.Response.Login.Success = true;
            message.Response.Login.playerData = new PlayerData();
            message.Response.Login.playerData.mainCharacterID = client.player.mainCharcterID;
            message.Response.Login.playerData.nickName = client.player.nickname;
            message.Response.Login.playerData.hasSetNickName = loginResult.user!.HasSetNickname;

            session.Send(message);

        }
        private async void OnPlayerChangeMainCharacter(ISession session, UserChangeMainChaRequest request)
        {
            string newCharacterID = request.mainCharacterID;
            ClientSession client = (ClientSession)session;
            if (client.player == null) return;

            string playerID = client.player.id;

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.changeMainCha = new UserChangeMainChaResponse();

            if (!DataManager.Instance.m_characters.ContainsKey(newCharacterID))
            {
                message.Response.changeMainCha.Success = false;
                message.Response.changeMainCha.Msg = "未发现请求的角色";
            }
            else
            {
                var changeResult = await accountService.SetMainCharacterAsync(playerID, request.mainCharacterID);
                if(!changeResult)
                {
                    message.Response.changeMainCha.Success = false;
                    message.Response.changeMainCha.Msg = "未找到指定用户";
                    return;
                }

                client.player.mainCharcterID = newCharacterID;
                message.Response.changeMainCha.Success = true;
                message.Response.changeMainCha.playerData = new PlayerData();
                message.Response.changeMainCha.playerData.mainCharacterID = newCharacterID;

                

                foreach(var friend in client.player.friendState.friends)
                {
                    string friendID = friend.Key;
                    if(OnlinePlayerManager.Instance.TryGetClientSession(friendID, out var friendSession))
                    {
                        var oldInfo = friendSession.player.friendState.friends[playerID].friendInfo;
                        oldInfo.mainCharacterID = client.player.mainCharcterID;
                        friendSession.player.friendState.SetFriendInfo(oldInfo);
                    }
                }
            }
            session.Send(message);
        }
        private async void OnPlayerChangeNickname(ISession session, UserChangeNicknameRequest request)
        {
            string newNickname = request.nickName;
            ClientSession client = (ClientSession)session;
            if (client.player == null) return;

            string playerID = client.player.id;

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.changeNickname = new UserChangeNicknameResponse();

            var changeResult = await accountService.SetNickname(playerID, request.nickName);
            if (!changeResult)
            {
                message.Response.changeNickname.Success = false;
                message.Response.changeNickname.Msg = "未找到指定用户或昵称不合法";
                session.Send(message);
                return;
            }

            client.player.nickname = newNickname;
            message.Response.changeNickname.Success = true;
            message.Response.changeNickname.Msg = "昵称修改成功";
            message.Response.changeNickname.playerData = new PlayerData();
            message.Response.changeNickname.playerData.nickName = newNickname;
            session.Send(message);

            foreach (var friend in client.player.friendState.friends)
            {
                string friendID = friend.Key;
                if (OnlinePlayerManager.Instance.TryGetClientSession(friendID, out var friendSession))
                {
                    var oldInfo = friendSession.player.friendState.friends[playerID].friendInfo;
                    oldInfo.Nickname = client.player.nickname;
                    friendSession.player.friendState.SetFriendInfo(oldInfo);
                }
            }
        }
    }
}
