using Assets.Scripts.Util;
using Common;
using Common.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Manager
{
    public class PlayerInfoManager : Singleton<PlayerInfoManager>
    {
        public string userID;

        public string password;

        private string pendingID;
        private string pendingPassword;

        public string mainCharacterID;
        public string nickname;
        public bool hasSetNickname;

        public Action OnMainCharacterChanged;
        public Action<bool> OnLoginResponse;
        public Action<bool> OnRegistResponse;
        public Action<bool> NicknameResponse;

        public bool isLogin = false;

        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserLoginResponse>(OnRecieveLoginResponse);
            MessageDispatcher.Instance.Subscribe<UserRegisterResponse>(OnRecieveRegistResponse);
            MessageDispatcher.Instance.Subscribe<UserForceLeaveReponse>(OnRecieveForceLeaveResponse);
            MessageDispatcher.Instance.Subscribe<UserChangeMainChaResponse>(OnRecieveChangeMainChaResponse);
            MessageDispatcher.Instance.Subscribe<UserChangeNicknameResponse>(OnRecieveChangeNickName);

            userID = string.Empty;
            isLogin = false;
        }
        public void SendLogin(string username, string password)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.Login = new UserLoginRequest();
            message.Request.Login.userID = username;
            message.Request.Login.Password = password;
            pendingID = username;
            pendingPassword = password;
            NetClient.Instance.Send(message);
        }
        public void SendRegist(string username, string password)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.Register = new UserRegisterRequest();
            message.Request.Register.userID = username;
            message.Request.Register.Password = password;

            NetClient.Instance.Send(message);
        }
        public void SendNewNickname(string newName)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.changeNickname = new UserChangeNicknameRequest();
            message.Request.changeNickname.nickName = newName;

            NetClient.Instance.Send(message);
        }
        public void SendChangeMainCharRequest(string currentCharacterID)
        {
            Message submitMainCha = new Message();
            submitMainCha.Request = new RequestMessage();
            submitMainCha.Request.changeMainCha = new UserChangeMainChaRequest();
            submitMainCha.Request.changeMainCha.mainCharacterID = currentCharacterID;
            NetClient.Instance.Send(submitMainCha);
        }
        private void OnRecieveLoginResponse(ISession session, UserLoginResponse response)
        {
            if (response == null) { return; }
            OnLoginResponse?.Invoke(response.Success);
            if (response.Success)
            {
                userID = pendingID;
                password = pendingPassword;
                mainCharacterID = response.playerData.mainCharacterID;
                nickname = response.playerData.nickName;
                hasSetNickname = response.playerData.hasSetNickName;
                isLogin = true;
                MySceneManager.Instance.EnterScene("MainScene");
            }
            else
            {
                pendingID = null;
                pendingPassword = null;
                hasSetNickname = false;
                isLogin = false;
                MessageBox.Show(UI.Common.MessageType.Error, response.Msg);
            }
        }
        private void OnRecieveRegistResponse(ISession session, UserRegisterResponse response)
        {
            if (response == null) { return; }
            OnRegistResponse?.Invoke(response.Success);
            if (response.Success)
            {
                MessageBox.Show(UI.Common.MessageType.Normal, response.Msg);
            }
            else
            {
                MessageBox.Show(UI.Common.MessageType.Error, response.Msg);
            }
        }
        private void OnRecieveForceLeaveResponse(ISession session, UserForceLeaveReponse response)
        {
            MessageBox.Show(UI.Common.MessageType.Error, response.Msg,
                () =>
                {
                    NetClient.Instance.Close(true);
                    isLogin = false;
                    MySceneManager.Instance.EnterScene("HotUpdate");
                }

                );

        }
        private void OnRecieveChangeMainChaResponse(ISession session, UserChangeMainChaResponse response)
        {
            if(response == null) { return; }
            if (response.Success)
            {
                this.mainCharacterID = response.playerData.mainCharacterID;
                OnMainCharacterChanged?.Invoke();
            }
        }
        private void OnRecieveChangeNickName(ISession session, UserChangeNicknameResponse response)
        {
            if (response == null) { return; }

            if(response.Success)
            {
                this.nickname = response.playerData.nickName;
                hasSetNickname = true;
            }
            else
            {
                MessageBox.Show(UI.Common.MessageType.Error, response.Msg);
            }
            NicknameResponse?.Invoke(response.Success);
        }
    }

}
