using Assets.Scripts.UI.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Util
{ 
    public static class MessageBox
    {
       private static GameObject prefab;
       public static void Show(MessageType type, string msg)
       {
            if(prefab == null)
            {
                prefab = Resources.Load<GameObject>("UI/UIMessageBox");
            }

            UIMessageBox box = GameObject.Instantiate(prefab).GetComponent<UIMessageBox>();

            box.ShowMessage(type, msg);
       }

        public static void Show(MessageType type, string msg, Action buttonFunc)
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("UI/UIMessageBox");
            }

            UIMessageBox box = GameObject.Instantiate(prefab).GetComponent<UIMessageBox>();

            box.ShowMessage(type, msg, buttonFunc);
        }

        public static void Show(MessageType type, string msg, Action buttonFunc, Action backFunc)
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("UI/UIMessageBox");
            }

            UIMessageBox box = GameObject.Instantiate(prefab).GetComponent<UIMessageBox>();

            box.ShowInteractionMessage(type, msg, buttonFunc, backFunc);
        }
    }

}

