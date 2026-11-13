using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using Common.Network;
using NetMessage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class LoadingService : MonoBehaviour
    {
        public static LoadingService Instance;
        private WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(2);
        private Coroutine heartCoroutine = null;
        private Message hearMessage;
        [SerializeField] private bool isRelease = false;

        [SerializeField] public string ServerIP => isRelease ? "49.233.150.200" : "127.0.0.1";
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;

                DontDestroyOnLoad(this.gameObject);
                NetClient.Instance.Init();

                DataManager.Instance.Init();
                PlayerInfoManager.Instance.Init();

                ChatService.Instance.Init();
                ChatManager.Instance.Init();

                RoomService.Instance.Init();
                BattleService.Instance.Init();

                FriendService.Instance.Init();

                SpecialEffectsService.Instance.Init();
                SpecialEffectsManager.Instance.Init();

                NetClient.Instance.OnConnected += StartHeart;

                SettingManager.Instance.Init();

                

                hearMessage = new Message();
                hearMessage.Request = new RequestMessage();
                hearMessage.Request.Heart = new HeartCheckRequest();
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                    return;
                }
            }
        }
        private void Start()
        {
            NetClient.Instance.Start();
            DataManager.Instance.Start();
            //StartHeart();
        }

        private void Update()
        {
            MessageDispatcher.Instance.Update();

        }
        private void StartHeart()
        {
            if (heartCoroutine != null)
            {
                StopCoroutine(heartCoroutine);
            }
            NetClient.Instance.lastCommunicateTime = Time.time;
            heartCoroutine = StartCoroutine(Heart());
        }
        IEnumerator Heart()
        {
            while (NetClient.Instance.isConnected)
            {
                yield return waitTime;

                NetClient.Instance.Send(hearMessage);

                if(Time.time - NetClient.Instance.lastCommunicateTime > 10f)
                {
                    Debug.Log("服务器超时");
                    NetClient.Instance.Close();

                    yield break;
                }

            }
            yield return null;
        }
    }
}
