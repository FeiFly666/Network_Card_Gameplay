using System;

namespace Common.Data
{
    public enum EffectType//作用于哪个属性
    {
        Health,
        MaxHealth,
        Power,
        Defence,
        Speed,
        MP
    }

    public enum ApplyType//具体类型
    {
        Gain,//直接获取
        ContinuousGain, //持续获取
        TempAdd, //限时增加
        TempAddPercent //限时百分比增加
    }
    public class BuffEffectData
    {
        public string DataID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ApplyType ApplyType { get; set; }
        public EffectType EffectType { get; set; }
        public int Param { get; set; }
        public string ParamStr { get; set; } = string.Empty;
        public int Duration { get; set; }
    }

    public class BuffData
    {
        public string DataID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> BuffEffects { get; set; } = new List<string>();

    }
}
