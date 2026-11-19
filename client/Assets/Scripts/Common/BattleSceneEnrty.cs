using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using Assets.Scripts.UI.Characters;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.CommonScript
{
    public class BattleSceneEnrty : MonoBehaviour
    {
        [SerializeField] private GameObject bg;
        [SerializeField] private CharacterContainer characterContainer;
        void Start()
        {
            //进入了，发送已经进入
            BattleService.Instance.Reset();

            BattleCardManager.Instance.Init();
            BattleManager.Instance.Init();
            BattleCharacterManager.Instance.Init();

            characterContainer.LoadPrefab(success =>
            {
                if (this == null)
                    return;

                if (!success)
                {
                    Debug.LogError("角色数值 UI 预制体加载失败");
                    return;
                }

                BattleService.Instance.SendEnterRequest();
            });

            int playerPos = RoomManager.Instance.GetPlayerRoomPos();
            if (playerPos == -1) ;
            else if (playerPos == 0)
            {
                bg.transform.localScale = new Vector3(1, 1, 1);
            }
            else if (playerPos == 1)
            {
                bg.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        private void OnDestroy()
        {
            if (BattleCardManager.Instance != null && PoolManager.Instance != null)
                BattleCardManager.Instance.Init();
        }
    }
}