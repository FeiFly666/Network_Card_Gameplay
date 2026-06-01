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
    internal class ChatService : Singleton<ChatService>
    {
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserTalkResponse>(OnServerSendResponse);
            MessageDispatcher.Instance.Subscribe<UserTalkBroadcast>(OnServerSendChat);
        }

        public void SendChat(string msg)
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.userTalk = new UserTalkRequest();
            message.Request.userTalk.Msg = msg;
            message.Request.userTalk.userID = "05";


            NetClient.Instance.Send(message);
        }

        private void OnServerSendResponse(ISession sender, UserTalkResponse response)
        {
            if (response == null) { return; }
        }

        private void OnServerSendChat(ISession sender, UserTalkBroadcast response)
        {
            if (response == null) { return; }
            Debug.Log($"收到广播信息 {response.Msg}");

            ChatManager.Instance.NewChatRecieve(response.fromID, response.Msg);
        }

    }
}
