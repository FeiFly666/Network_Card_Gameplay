using System.Collections;
using UnityEngine;

namespace Assets.Scripts.CommonScript
{
    public class DontDestroyOnLoad : MonoBehaviour
    {

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}