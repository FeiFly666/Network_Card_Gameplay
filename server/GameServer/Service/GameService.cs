using Common.Network;
using GameServer.Manager;
using GameServer.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    

    public class GameService
    {
        internal MonitorChannel? Monitoring { get; init; }
        Thread? thread;
        bool isRunning;
        ChatService? chatService;
        public void Init()
        {
            chatService = new ChatService();
            thread = new Thread(new ThreadStart(this.Update));

            DataManager.Instance.Init();
            RoomManager.Instance.Init();
            BattleManager.Instance.Init();
            UserService.Instance.Init();
            FriendShipManager.Instance.Init();
            NetService.Instance.Init();

            SpecialEffectManager.Instance.Init();
        }

        public void Start()
        {
            isRunning = true;
            NetService.Instance.Start();
            thread?.Start();
        }
        public void Stop()
        {
            isRunning = false;
            thread?.Join();
        }
        private void Update()
        {
            while(isRunning)
            {
                Time.Tick();
                BattleManager.Instance.Tick();
                MessageDispatcher.Instance.Update();
                NetService.Instance.Update();
                Monitoring?.CompletePendingSnapshot(static () => new ServerSnapshot(
                    DateTimeOffset.UtcNow,
                    OnlinePlayerManager.Instance.CaptureOnlinePlayers(),
                    BattleManager.Instance.CaptureBattleRooms()));
                Thread.Sleep(100);
            }
        }
    }
}
