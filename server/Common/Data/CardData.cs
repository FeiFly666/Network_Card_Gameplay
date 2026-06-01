using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data
{
    public enum CardType
    {
        Character,//放置角色
        Buff, //给角色buff
        Gain // 增加角色属性
    }
    public class CardData
    {
        public string DataID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public CardType Type { get; set; }
        public int Quality { get; set; }
        //生成对象的数据ID
        public List<string> TargetID { get; set; }
    }
}
