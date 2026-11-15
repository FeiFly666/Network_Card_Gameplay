using Assets.Scripts.Manager;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UISceneTag : MonoBehaviour
    {

        [SerializeField] public string tip = "";
        private void Start()
        {
            UIManager.Instance.CloseAllUI();

            UIManager.Instance.SetSceneStaticUI(tip);
            if(tip == "start")
            {
                UIManager.Instance.ShowUI("UIStartPanel");
            }
            else if(tip == "battle")
            {
                UIManager.Instance.ShowUI("UIBattleInfoHUD");
                UIManager.Instance.ShowUI("UICardContainer");
                UIManager.Instance.ShowUI("UICharacterPanel");
            }
        }
    }
}