using GameServer.Manager;
using GameServer.Monitoring;
using GameServer.Service;
using System.Text;

namespace GameServer;

class Program
{
    static void Main(string[] args)
    {
        var monitoring = MonitorChannel.Create(args);
        GameService gameService = new GameService { Monitoring = monitoring };
        CommandHandler commandHandler = new CommandHandler { Monitoring = monitoring };

        gameService.Init();

        gameService.Start();

        Console.WriteLine("all id is socket id in server, not real user id");
        while (commandHandler.run)
        {
            commandHandler.Update();
        }

        gameService.Stop();
    }
}
