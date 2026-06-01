using System;

namespace Common.Data
{
    public enum EffectType//作用于哪个属性
    {
        Health,
        MaxHealth,
        Power,
        Defence,
        Speed
    }

    public enum BuffType//具体类型
    {
        Gain,
        Buff
    }

    public class BuffData
    {
        public string DataID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BuffType Type { get; set; }
        public EffectType EffectType { get; set; }
        //数值可正可负
        public int Param{ get; set; }
        public int Duration { get; set; }

    }
}
