using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    public class MessageDispatcher : Singleton<MessageDispatcher>
    {
        public delegate void MessageDelegate(ISession session, object message);

        Dictionary<Type, MessageDelegate> m_MessageSubscribleInfo = new Dictionary<Type, MessageDelegate>();

        

        private void DispatchMessage(ISession session, RequestMessage message)
        {
            if(message != null)
            {
                if(message.userTalk != null) Execute(session, message.userTalk);
            }
        }

        private void DispatchMessage(ISession session, ResponseMessage message)
        {
            if(message != null)
            {
                if (message.userTalk != null) Execute(session, message.userTalk);
                if(message.userTalkBroadCast != null) Execute(session, message.userTalkBroadCast);
            }
        }
        public void Dispatch(ISession session, Message message)
        {
            if (message == null) return;

            if(message.Response != null)
            {
                DispatchMessage(session, message.Response);
            }
            else if(message.Request != null)
            {
                DispatchMessage(session, message.Request);
            }
        }

        private void Execute<T>(ISession session, T message)
        {
            Type type = typeof(T);

            if(m_MessageSubscribleInfo.ContainsKey(type))
            {
                m_MessageSubscribleInfo[type]?.Invoke(session, message);
            }
        }

        public void Subscribe<T>(MessageDelegate subscriber)where T : class
        {
            Type subscriberType = typeof(T);

            if(!m_MessageSubscribleInfo.ContainsKey(subscriberType))
            {
                MessageDelegate newDelegate = subscriber;

                m_MessageSubscribleInfo.Add(subscriberType, newDelegate);
            }
            else
            {
                m_MessageSubscribleInfo[subscriberType] += subscriber;
            }
        }
        public void Unsubscribe<T>(MessageDelegate subscriber) where T : class
        {
            if (subscriber == null) return;
            Type subscriberType = typeof(T);

            if (m_MessageSubscribleInfo.ContainsKey(subscriberType))
            {
                m_MessageSubscribleInfo[subscriberType] -= subscriber;

                if (m_MessageSubscribleInfo[subscriberType] == null)
                {
                    m_MessageSubscribleInfo.Remove(subscriberType);
                }
            }
        }
    }
}
