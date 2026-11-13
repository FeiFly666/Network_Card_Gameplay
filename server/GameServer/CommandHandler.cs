using GameServer.Manager;
using GameServer.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class CommandHandler
    {
        internal MonitorChannel? Monitoring { get; init; }
        public bool run = false;
        public CommandHandler()
        {
            run = true;
        }

        public void Update()
        {
            if (Monitoring is null)
                Console.Write(">");

            string? line = Console.ReadLine();
            if (line == null)
            {
                if (Monitoring is not null)
                    run = false;
                return;
            }

            switch (line.ToLower().Trim())
            {
                case "exit":
                    run = false;
                    break;
                case MonitorChannel.SnapshotCommand when Monitoring is { } monitoring:
                    monitoring.WriteSnapshot();
                    break;
                case "remove client":
                    HandleRemoveClientCommand();
                    break;
                default:
                    break;
            }
        }
        private void HandleRemoveClientCommand()
        {
            Console.Write(">>Remove Client [Enter Client Player ID] : ");
            string playerid = Console.ReadLine();
            if(playerid == null) return;

            if (OnlinePlayerManager.Instance.IsPlayerOnline(playerid))
            {
                if (OnlinePlayerManager.Instance.TryGetClientSession(playerid, out var client))
                {
                    client.Close();
                }
            }
        }
    }
}
