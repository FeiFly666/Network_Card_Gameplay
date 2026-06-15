using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Player
    {
        public string id {  get; set; }
        public int socketID { get; set; }
        public bool ready { get; set; }
        public string mainCharcterID {  get; set; }
        public string nickname { get; set; }
        public PlayerFriendState friendState { get; set; } = new PlayerFriendState();

        public BattlePlayer battlePlayer { get; set; } = new BattlePlayer();
    }
    public class BattlePlayer
    {
        Player owner { get; set; }
        int battleRoomID { get; set; }//用于确认是否为断线重连
        public int mainCharacterID = -1;

        public Dictionary<int, Card> cards { get; set; } = new Dictionary<int, Card>();

        public Dictionary<int, Character> characters { get; set; } = new Dictionary<int, Character>();

    }
}
