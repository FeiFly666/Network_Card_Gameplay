using GameServer.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DataBase
{
    public class GameDbContextFactory :  IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<GameDbContext> builder = new DbContextOptionsBuilder<GameDbContext>();

            builder.UseSqlServer(DataManager.Instance.DbConnectionString);
            return new GameDbContext(builder.Options);
        }
    }
}
