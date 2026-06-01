using Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<Type, GameObject> m_typeUI = new Dictionary<Type, GameObject>();

    }
}
