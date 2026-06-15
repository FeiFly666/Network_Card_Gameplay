using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class SettingPanel
    {
        private static GameObject prefab;
        public static void Show()
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("UI/UISettingsPanel");
            }

            UISettingPanel box = GameObject.Instantiate(prefab).GetComponent<UISettingPanel>();

            //box.ShowMessage(type, msg);
        }
    }
}
