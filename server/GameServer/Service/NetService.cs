using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
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
        public void Start()
        {
            m_listener.Start("127.0.0.1", 8080);
        }
        public void Update()
        {
            m_listener.CheckSession();
        }
    }
}
