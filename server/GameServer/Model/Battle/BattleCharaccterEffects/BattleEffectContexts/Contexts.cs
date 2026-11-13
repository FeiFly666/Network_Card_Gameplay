using Common.Model;
using GameServer.Model.Battle.States;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.BattleCharaccterEffects.BattleEffectContexts
{
    //死亡效果上下文
    public class DeathContext
    {
        public IReadOnlyList<Character> allies; //友军列表
        public IReadOnlyList<Character> enemies; //敌人列表

        public int battleGroup = -1;//攻击所属group
        public Action<BattleEvent> AddEvent;//创建新事件所需函数
    }
    //受击效果上下文
    public class AfterDamageContext
    {
        public bool isDefended = false; //是否成功防御
        public BattleCharacter? attacker; //攻击方是谁
        public int battleGroup = -1; //攻击所属group
        public Action<BattleEvent> AddEvent; //创建新事件所需函数
        public AttackOrigin origin; //攻击来源
        public Action<AttackOrigin, int> ExecuteAttack; //攻击所需事件
    }
    //攻击后上下文
    public class AfterAttackContext
    {
        public int targetID; //目标ID
        public bool isTargetDefended = false;//目标是否防御成功
        public AttackOrigin origin; //攻击来源
        public Func<int, bool> ExecuteAttack; //攻击所需事件
    }
    //击杀敌人上下文
    public class OnKillContext
    {

    }

}
