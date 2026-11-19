using Assets.Scripts.Manager;
using Assets.Scripts.UI.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Util
{ 
    public static class MessageBox
    {
       private static UIMessageBox boxPrefab;
       public static void Init(Action callback = null)
       {
            UIManager.Instance.GetUIPrefab<UIMessageBox>("Share/UIMessageBox", prefab =>
            {
                boxPrefab = prefab;

                if (prefab == null) return;

                callback?.Invoke();
            });
       }

       public static void Show(MessageType type, string msg)
       {
            UIMessageBox box = UIManager.Instance.CreatePopup<UIMessageBox>(boxPrefab.gameObject);

            box.ShowMessage(type, msg);
       }

        public static void Show(MessageType type, string msg, Action buttonFunc)
        {
            UIMessageBox box = UIManager.Instance.CreatePopup<UIMessageBox>(boxPrefab.gameObject);

            box.ShowMessage(type, msg, buttonFunc);
        }

        public static void Show(MessageType type, string msg, Action buttonFunc, Action backFunc)
        {
            UIMessageBox box = UIManager.Instance.CreatePopup<UIMessageBox>(boxPrefab.gameObject);

            box.ShowInteractionMessage(type, msg, buttonFunc, backFunc);
        }
    }

}

