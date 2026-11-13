using System;

namespace Assets.Scripts.UI.SpecialEffectUI
{
    public static class SpecialEffectInfoPanel
    {
        public static event Action OnPanelClosed;

        public static void OnPanelClose()
        {
            OnPanelClosed?.Invoke();
        }
    }

}
