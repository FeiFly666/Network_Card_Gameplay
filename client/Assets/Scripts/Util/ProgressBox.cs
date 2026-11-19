using Assets.Scripts.Manager;
using Assets.Scripts.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Util
{
    public static class ProgressBox
    {
        private static UIProgressBox boxPrefab;
        public static void Init(Action callback = null)
        {
            UIManager.Instance.GetUIPrefab<UIProgressBox>("Share/UIProgressBox", prefab =>
            {
                boxPrefab = prefab;

                if (prefab == null) return;

                callback?.Invoke();
            });
        }
        public static UIProgressBox GetProgressBox()
        {
            UIProgressBox box = UIManager.Instance.CreatePopup<UIProgressBox>(boxPrefab.gameObject);
            box.CloseDescription();
            box.CloseProgress();
            return box;
        }
    }
}
