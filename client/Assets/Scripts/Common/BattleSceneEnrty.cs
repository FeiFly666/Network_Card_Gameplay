using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.CommonScript
{
    public class BattleSceneEnrty : MonoBehaviour
    {
        [SerializeField] private GameObject bg;
        void Start()
        {
            //进入了，发送已经进入
            BattleService.Instance.Reset();
            BattleService.Instance.SendEnterRequest();

            BattleCardManager.Instance.Init();
            BattleManager.Instance.Init();
            BattleCharacterManager.Instance.Init();

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

    }
}