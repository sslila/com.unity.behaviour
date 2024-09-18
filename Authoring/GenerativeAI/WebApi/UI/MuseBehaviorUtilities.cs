#if UNITY_EDITOR
using System;
#if ENABLE_MUSE_BEHAVIOR
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Account.UI;
#endif // ENABLE_MUSE_BEHAVIOR
using UnityEditor;


namespace Unity.Behavior.GenerativeAI
{
    internal static class MuseBehaviorUtilities
    {
        public static bool IsSessionUsable =>
#if ENABLE_MUSE_BEHAVIOR
            SessionStatus.IsSessionUsable;
#else
            false;
#endif

        public static void RegisterSessionStatusChangedCallback(Action<bool> callback)
        {
#if ENABLE_MUSE_BEHAVIOR
            SessionStatus.OnUsabilityChanged += callback;
#endif
        }

        public static void UpdateUsage()
        {
#if ENABLE_MUSE_BEHAVIOR
            AccountInfo.Instance.UpdateUsage();
#endif
        }

        public static void OpenMuseDropdown()
        {
#if ENABLE_MUSE_BEHAVIOR
            AccountDropdownWindow.ShowMuseAccountSettingsAsPopup();
#endif
        }
    }
}

#endif