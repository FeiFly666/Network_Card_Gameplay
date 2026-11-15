using Assets.Scripts.Manager;
using Common.Data;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Main
{
    public class UISelectCharacterInfo : MonoBehaviour
    {

        [SerializeField] private TMP_Text characterName;
        [SerializeField] private TMP_Text characterDescription;
        [SerializeField] private TMP_Text characterHP;
        [SerializeField] private TMP_Text characterMP;
        [SerializeField] private TMP_Text characterDef;
        [SerializeField] private TMP_Text characterAttack;
        [SerializeField] private TMP_Text characterSpeed;
        [SerializeField] private TMP_Text characterSkillName;
        [SerializeField] private TMP_Text characterSkillDes;
        [SerializeField] private TMP_Text characterSkillDamage;
        [SerializeField] private TMP_Text characterSkillHitCount;
        [SerializeField] private TMP_Text characterSkillTip;
        private void OnEnable()
        {
            InitText();
        }
        public void InitText()
        {
            characterName.text = string.Empty;
            characterDescription.text = string.Empty;
            characterHP.text = string.Empty;
            characterDef.text = string.Empty;
            characterAttack.text = string.Empty;
            characterSpeed.text = string.Empty;
            characterMP.text = string.Empty;
            characterSkillName.text = string.Empty;
            characterSkillDes.text = string.Empty;
            characterSkillDamage.text = string.Empty;
            characterSkillHitCount.text = string.Empty;
            characterSkillTip.text = string.Empty;
        }

        public void ChangeInfo(string characterID)
        {
            if (!DataManager.Instance.characterData.TryGetValue(characterID, out var data))
            {
                return;
            }
            characterName.text = data.Name;
            characterDescription.text = data.Description;
            characterHP.text = $"血量：{data.HP}";
            characterMP.text = $"技能消耗：{data.MP}";
            characterAttack.text = $"攻击力/回复量：{data.ATK}";
            characterDef.text = $"防御：{data.DEF}";
            characterSpeed.text = $"速度：{data.Speed}";

            if(!DataManager.Instance.skillData.TryGetValue(data.SkillID, out var skillData))
            {
                return;
            }

            characterSkillName.text = $"技能：{skillData.Name}";
            characterSkillDes.text = skillData.Description;
            characterSkillDamage.text = $"技能伤害：{skillData.Param}";
            characterSkillHitCount.text = $"技能段数：{skillData.HitCount}";
            characterSkillTip.text = "（单段伤害，基于攻击力增加）";
        }
    }
}