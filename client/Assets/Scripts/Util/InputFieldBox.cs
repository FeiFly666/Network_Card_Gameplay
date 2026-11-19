using Assets.Scripts.Manager;
using Assets.Scripts.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class InputFieldBox
    {
        private static UIInputFieldBox boxPrefab;
        private static UIInputFieldBox showedBox;
        public static void Init()
        {
            UIManager.Instance.GetUIPrefab<UIInputFieldBox>("Share/UIInputFieldBox", prefab =>
            {
                boxPrefab = prefab;
            });
        }

        public static void Show(string msg, Action<string> buttonFunc, Action backFunc)
        {
            UIInputFieldBox box = UIManager.Instance.CreatePopup<UIInputFieldBox>(boxPrefab.gameObject);
            if (showedBox != null && showedBox != box)
            {
                showedBox.HideThisBox();
            }
            showedBox = box;

            box.ShowInputField(msg, buttonFunc, backFunc);
        }
        public static void Close()
        {
            showedBox?.HideThisBox();
        }
    }
}
