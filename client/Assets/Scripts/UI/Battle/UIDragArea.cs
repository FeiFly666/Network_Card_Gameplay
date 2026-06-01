using Common.Model;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public enum DragAreaType
    {
        None = 0,
        Discard = 1, //弃牌区
        Battle = 2 //战斗区域
    }

    public class UIDragArea : MonoBehaviour
    {
        [SerializeField] public DragAreaType type;
    }
}