using GameServer.Manager;
using GameServer.Service;
using System.Text;

namespace GameServer;

class Program
{
    static void Main(string[] args)
    {
        GameService gameService = new GameService();
        CommandHandler commandHandler = new CommandHandler();

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
