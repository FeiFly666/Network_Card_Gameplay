using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data
{
    public enum SkillType
    {
        Damage,
        Heal,
        AttributeChange,
        AddBuff
    }
    public enum SkillTargetType
    {
        Self,
        SelectEnemy,
        SelectAlly,
        AllEnemies,
        AllAllies
    }
    public class SkillData
    {
        public string DataID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillType Type { get; set; }
        public SkillTargetType TargetType { get; set; }
        public int Param {  get; set; }
        public int HitCount { get; set; }
        public int Cost { get; set; }
        public string BuffID { get; set; }

    }
}
