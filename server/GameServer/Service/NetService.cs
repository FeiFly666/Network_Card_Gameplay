using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Network;
using GameServer.Manager;
using GameServer.Network;
using NetMessage;

namespace GameServer.Service
{
    public class NetService : Singleton<NetService>
    {
        public TcpSocketListener m_listener;
        public NetService()
        {
            m_listener = new TcpSocketListener();
        }
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<HeartCheckRequest>(OnClientSendHeart);
        }
        public void Start()
        {
            m_listener.Start("127.0.0.1", 8080);
        }
        public void Update()
        {
            m_listener.CheckSession();
        }
        private void OnClientSendHeart(ISession session, HeartCheckRequest request)
        {
            if (session is not ClientSession client || client.IsClosed)
                return;

            Message heartMessage = new Message();
            heartMessage.Response = new ResponseMessage();
            heartMessage.Response.Heart = new HeartCheckResponse();

            FriendShipManager.Instance.AppendDirtyFriendData(client, heartMessage);
            client.Send(heartMessage);
            heartMessage.Response.friendList = null;
        }

    }
}
