using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data
{
    public enum CharacterType//角色类型
    {
        Attack,//攻击的
        Heal//给队友回血的
    }
    public enum AttackType//攻击类型
    {
        SingleTarget,
        MultiTarget
    }
    [Serializable]
    public class CharacterData
    {
        public string DataID {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CharacterType CharacterType { get; set; }
        public AttackType AttackType { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public int ATK { get; set; }
        public int DEF { get; set; }
        public int Speed { get; set; }
        public string SkillID { get; set; }
        public string PrefabPath { get; set; }

    }
}
