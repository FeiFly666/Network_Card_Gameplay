using GameServer.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Network;
using System.Runtime.InteropServices;

namespace GameServer.Service
{
    public class ChatService
    {
        public ChatService()
        {
            MessageDispatcher.Instance.Subscribe<UserTalkRequest>(OnClientSendChat);
            Console.WriteLine("chatservice has subscribed");
        }

        private void OnClientSendChat(ISession session, UserTalkRequest request)
        {
            Message reply = new Message();
            reply.Response = new ResponseMessage();
            reply.Response.userTalk = new UserTalkResponse();

            if(request == null)
            {
                reply.Response.userTalk.Success = false;
                reply.Response.userTalk.Msg = "服务端接收失败";
                session.Send(reply);
                return;
            }

            string chatMsg = request.Msg;
            string senderID = request.userID;

            reply.Response.userTalk.Success = true;
            reply.Response.userTalk.Msg = string.Empty;

            //TODO:向其他所有在线客户端发送UserTalkBroadcast,同时向发送方发送UserTalkResponse

            session.Send(reply);

            Message broadcast = new Message();
            broadcast.Response = new ResponseMessage();
            broadcast.Response.userTalkBroadCast = new UserTalkBroadcast();

            var clients = NetService.Instance.m_listener.Clients;

            if (session is not ClientSession) return;

            int clientID = (session as ClientSession).ClientID;

            broadcast.Response.userTalkBroadCast.fromID = clientID.ToString();
            broadcast.Response.userTalkBroadCast.Msg = chatMsg;

            Console.WriteLine($"chat found : from:{clientID} msg:{request.Msg}");


            foreach (var client in clients)
            {
                //if (client.Key == clientID) continue;

                Console.WriteLine($"chat broadcast : from:{clientID} to:{client.Key}");
                client.Value.Send(broadcast);
            }

        }
    }
}
