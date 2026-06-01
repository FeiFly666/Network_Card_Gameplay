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

        private string pendingID;

        public string mainCharacterID;

        public Action OnMainCharacterChanged;

        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserLoginResponse>(OnRecieveLoginResponse);
            MessageDispatcher.Instance.Subscribe<UserForceLeaveReponse>(OnRecieveForceLeaveResponse);
            MessageDispatcher.Instance.Subscribe<UserChangeMainChaResponse>(OnRecieveChangeMainChaResponse);

            userID = string.Empty;

        }
        public void SendLogin(string username)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.Login = new UserLoginRequest();
            message.Request.Login.userID = username;
            pendingID = username;
            NetClient.Instance.Send(message);
        }
        private void OnRecieveLoginResponse(ISession session, UserLoginResponse response)
        {
            if (response == null) { return; }
            if (response.Success)
            {
                userID = pendingID;
                mainCharacterID = response.playerData.mainCharacterID;
                MySceneManager.Instance.EnterScene("MainScene");
            }
            else
            {
                pendingID = null;
            }
        }
        private void OnRecieveForceLeaveResponse(ISession session, UserForceLeaveReponse response)
        {
            MessageBox.Show(UI.Common.MessageType.Error, response.Msg,
                () =>
                {
                    NetClient.Instance.Close(true);
                    MySceneManager.Instance.EnterScene("StartScene");
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
    }

}
