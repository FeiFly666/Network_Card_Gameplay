using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GameServer.Model.CharacterSpecialEffect
{
    public class CharacterSpecialEffectData
    {   public string DataID { get; set; } = "";
        public string LogicType { get; set; } = "";
        public JObject Params { get; set; } = new JObject();
    }
    //反弹效果参数
    public class ReflectParams
    {
        public int ReflectPercent { get; set; }
        public int MaxTriggersPerRound { get; set; }
    }
    //防御成功改变数值 参数
    public class DefendAddParams
    {
        public int HealAmount { get; set; }
        public string Type { get; set; } = "";
    }


}
