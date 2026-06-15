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
        private static GameObject prefab;
        private static UIInputFieldBox showedBox;

        public static void Show(string msg, Action<string> buttonFunc, Action backFunc)
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("UI/UIInputFieldBox");
            }

            UIInputFieldBox box = GameObject.Instantiate(prefab).GetComponent<UIInputFieldBox>();
            if(showedBox != null && showedBox != box)
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
