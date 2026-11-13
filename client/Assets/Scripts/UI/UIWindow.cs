using Assets.Scripts.Manager;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIWindow : MonoBehaviour
    {
        public string id;
        public float sleepTime;
        public UILayer layer;

        public virtual void OnOpen() 
        { 
            this.gameObject.SetActive(true);
        }

        public virtual void OnClose()
        {
            this.gameObject.SetActive(false);
        }
        public virtual void CloseUI()
        {
            UIManager.Instance.CloseUI(id);
        }
    }
}