using NetMessage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
{
    public class MessageDispatcher : Singleton<MessageDispatcher>
    {
        public Action<string> Onlog;

        public delegate void MessageDelegate<T>(ISession session, T message);

        Dictionary<Type, Delegate> m_MessageSubscribleInfo = new Dictionary<Type, Delegate>();

        private readonly ConcurrentQueue<Action> m_TaskQueue = new ConcurrentQueue<Action>();


        private void DispatchMessage(ISession session, RequestMessage message)
        {
            if (message != null)
            {
                if (message.userTalk != null) Execute(session, message.userTalk);
                if (message.userMatch != null) Execute(session, message.userMatch);
                if (message.userReady != null) Execute(session, message.userReady);
                if (message.userExit != null) Execute(session, message.userExit);

                if (message.userEnterBattle != null) Execute(session, message.userEnterBattle);
                if (message.userLeaveBattle != null) Execute(session, message.userLeaveBattle);

                if (message.battleAction != null) Execute(session, message.battleAction);

                if (message.Heart != null) Execute(session, message.Heart);
                if (message.Login != null) Execute(session, message.Login);
                if (message.Register != null) Execute(session, message.Register);

                if (message.changeMainCha != null) Execute(session, message.changeMainCha);

                if (message.battlePlayFinish != null) Execute(session, message.battlePlayFinish);
                if (message.eventPlayFinish != null) Execute(session, message.eventPlayFinish);

                if (message.Sync != null) Execute(session, message.Sync);

                if (message.changeNickname != null) Execute(session, message.changeNickname);

                if (message.addFriend != null) Execute(session, message.addFriend);
                if (message.handleFriendRequest != null) Execute(session, message.handleFriendRequest);
                if (message.deleteFriend != null) Execute(session, message.deleteFriend);
                if (message.getFriendList != null) Execute(session, message.getFriendList);
                if (message.searchPlayer != null) Execute(session, message.searchPlayer);

                if (message.getSpecialEffectList != null) Execute(session, message.getSpecialEffectList);
                if (message.equipSpecialEffect != null) Execute(session, message.equipSpecialEffect);
                if (message.unEquipSpecialEffect != null) Execute(session, message.unEquipSpecialEffect);
            }
        }

        private void DispatchMessage(ISession session, ResponseMessage message)
        {
            if (message != null)
            {
                if (message.userTalk != null) Execute(session, message.userTalk);
                if (message.userTalkBroadCast != null) Execute(session, message.userTalkBroadCast);
                if (message.userMatch != null) Execute(session, message.userMatch);
                if (message.userReady != null) Execute(session, message.userReady);
                if (message.userExit != null) Execute(session, message.userExit);
                if (message.roomInfo != null) Execute(session, message.roomInfo);

                if (message.enterBattle != null) Execute(session, message.enterBattle);
                if (message.userEnterBattle != null) Execute(session, message.userEnterBattle);
                if (message.userLeaveBattle != null) Execute(session, message.userLeaveBattle);

                if (message.roundStart != null) Execute(session, message.roundStart);

                if (message.battleEvent != null) Execute(session, message.battleEvent);
                if (message.battleActionBroadcast != null) Execute(session, message.battleActionBroadcast);
                if (message.battleAction != null) Execute(session, message.battleAction);
                if (message.battleSync != null) Execute(session, message.battleSync);

                if (message.Heart != null) Execute(session, message.Heart);
                if (message.Login != null) Execute(session, message.Login);
                if (message.Register != null) Execute(session, message.Register);
                if (message.forceLeave != null) Execute(session, message.forceLeave);

                if (message.battleTimedOut != null) Execute(session, message.battleTimedOut);

                if (message.Gameover != null) Execute(session, message.Gameover);

                if (message.Reconnect != null) Execute(session, message.Reconnect);

                if (message.changeMainCha != null) Execute(session, message.changeMainCha);
                if (message.changeNickname != null) Execute(session, message.changeNickname);

                if (message.addFriend != null) Execute(session, message.addFriend);
                if (message.handleFriendRequest != null) Execute(session, message.handleFriendRequest);
                if (message.deleteFriend != null) Execute(session, message.deleteFriend);
                if (message.getFriendList != null) Execute(session, message.getFriendList);
                if (message.friendList != null) Execute(session, message.friendList);
                if (message.searchPlayer != null) Execute(session, message.searchPlayer);

                if (message.getSpecialEffectList != null) Execute(session, message.getSpecialEffectList);
                if (message.equipSpecialEffect != null) Execute(session, message.equipSpecialEffect);
                if (message.unEquipSpecialEffect != null) Execute(session, message.unEquipSpecialEffect);
            }
        }
        public void Enqueue(Action action)
        {
            m_TaskQueue.Enqueue(action);
        }
        public void Update()
        {
            while (m_TaskQueue.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }
        public void Dispatch(ISession session, Message message)
        {
            
            if (message == null)
            {
                Onlog?.Invoke("消息为空");
                return;
            }

            if (message.Response != null)
            {
                DispatchMessage(session, message.Response);
            }
            else if (message.Request != null)
            {
                DispatchMessage(session, message.Request);
            }
            else
            {
                Onlog?.Invoke("消息没有请求或访问");
            }
        }

        private void Execute<T>(ISession session, T message)
        {
            Type type = typeof(T);

            if (m_MessageSubscribleInfo.ContainsKey(type))
            {
                MessageDelegate<T> callback = m_MessageSubscribleInfo[type] as MessageDelegate<T>;

                m_TaskQueue.Enqueue(() => {
                    try
                    {
                        callback?.Invoke(session, message);
                    }
                    catch (Exception ex)
                    {
                        Onlog?.Invoke($"回调执行异常: {ex.Message}");
                    }
                });
            }
        }

        public void Subscribe<T>(MessageDelegate<T> subscriber) where T : class
        {
            Type subscriberType = typeof(T);

            if (!m_MessageSubscribleInfo.ContainsKey(subscriberType))
            {
                MessageDelegate<T> newDelegate = subscriber;

                m_MessageSubscribleInfo.Add(subscriberType, newDelegate);
            }
            else
            {
                m_MessageSubscribleInfo[subscriberType] = Delegate.Combine(m_MessageSubscribleInfo[subscriberType], subscriber);
            }
        }
        public void Unsubscribe<T>(MessageDelegate<T> subscriber) where T : class
        {
            if (subscriber == null) return;
            Type subscriberType = typeof(T);

            if (m_MessageSubscribleInfo.ContainsKey(subscriberType))
            {
                m_MessageSubscribleInfo[subscriberType] = Delegate.Remove(m_MessageSubscribleInfo[subscriberType], subscriber);

                if (m_MessageSubscribleInfo[subscriberType] == null)
                {
                    m_MessageSubscribleInfo.Remove(subscriberType);
                }
            }
        }
    }
}
