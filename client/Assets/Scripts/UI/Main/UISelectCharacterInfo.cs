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
        [SerializeField] private TMP_Text characterDef;
        [SerializeField] private TMP_Text characterAttack;
        [SerializeField] private TMP_Text characterSpeed;
        private void OnEnable()
        {
            characterName.text = string.Empty;
            characterDescription.text = string.Empty;
            characterHP.text = string.Empty;
            characterDef.text = string.Empty;
            characterAttack.text = string.Empty;
            characterSpeed.text = string.Empty;
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
            characterAttack.text = $"攻击力/回复量：{data.ATK}";
            characterDef.text = $"防御：{data.DEF}";
            characterSpeed.text = $"速度：{data.Speed}";
        }
    }
}