using Assets.Scripts.Manager;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIButton : MonoBehaviour
    {
        public void OpenUI(string id)
        {
            UIManager.Instance.ShowUI(id);
        }
        public void CloseUI(string id)
        {
            UIManager.Instance.CloseUI(id);
        }
    }
}