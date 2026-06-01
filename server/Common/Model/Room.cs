using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Room
    {
        public int ID { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

        public int RoomPlayerNum { get; set; }
    }
}
