using Assets.Scripts.Manager;
using Common;
using Common.Network;
using NetMessage;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Service
{
    public class RoomService : Singleton<RoomService>
    {
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<RoomInfoBroadcast>(OnRoomInfoChanged);
            MessageDispatcher.Instance.Subscribe<UserMatchResponse>(OnUserMatchResponse);
            MessageDispatcher.Instance.Subscribe<UserReadyResponse>(OnUserReadyResponse);
            MessageDispatcher.Instance.Subscribe<UserExitResponse>(OnUserExitResponse);
        }

        public void SendMatchRequest()
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.userMatch = new UserMatchRequest();

            NetClient.Instance.Send(message);
        }

        public void SendReadyRequest()
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.userReady = new UserReadyRequest();

            NetClient.Instance.Send(message);
        }
        public void SendExitRequest()
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.userExit = new UserExitRequest();

            NetClient.Instance.Send(message);
        }

        private void OnRoomInfoChanged(ISession session, RoomInfoBroadcast response)
        {
            Debug.Log("收到房间更新信息");
            RoomInfo newInfo = response.roomInfo;

            if (newInfo != null)
            {
                RoomManager.Instance.RefreshRoom(newInfo);
            }
        }
        private void OnUserMatchResponse(ISession session, UserMatchResponse response)
        {
            Debug.Log("收到匹配信息反馈");
            if (response == null) return;
            if(response.roomID == -1)
            {
                MySceneManager.Instance.EnterScene("BattleScene");
            }
        }

        private void OnUserReadyResponse(ISession session, UserReadyResponse response)
        {
            Debug.Log("收到准备反馈");
            if(!response.Success)
                Debug.LogWarning($"服务端准备反馈 ：{response.Success}  {response.Msg}");
        }
        private void OnUserExitResponse(ISession session, UserExitResponse response)
        {
            Debug.Log("收到退出房间反馈");
            if(response.Success)
            {
                RoomManager.Instance.RefreshRoom(null);
            }
        }
    }
}
