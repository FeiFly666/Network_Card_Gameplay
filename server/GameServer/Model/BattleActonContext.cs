using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    public class BattleActonContext
    {
        public string playerID;
        public BattleAction action;

        public BattleActonContext(string  playerID, BattleAction action)
        {
            this.playerID = playerID;
            this.action = action;
        }
    }
}
