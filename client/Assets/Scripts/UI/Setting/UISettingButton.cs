using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Setting
{
    public class UISettingButton : MonoBehaviour
    {
        Button button;
        private void Awake()
        {
            button = this.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ShowPanel);
        }
        private void ShowPanel()
        {
            SettingPanel.Show();
        }
    }
}
