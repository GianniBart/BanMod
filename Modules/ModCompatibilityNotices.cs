using System;
using UnityEngine;
using HarmonyLib;

namespace BanMod
{
    internal static class ModCompatibilityNotices
    {
        // Process lifetime, deliberately not reset on lobby/menu changes.
        private static bool _unknownShown;
        private static bool _cheatShown;
        private static Action _pendingDisable;
        internal static bool DisableAfterNotice(Action disable)
        {
            if (_pendingDisable != null || BanMod.IsBanModDisabled) return true;
            _pendingDisable = disable;
            // No unload, unpatch or timer here. Only the OK callback can disable.
            if (!Show(true))
            {
                _pendingDisable = null; // Next login/access poll can retry UI creation.
                return false;
            }
            return true;
        }

        private static void OnCheatAcknowledged()
        {
            var disable = _pendingDisable;
            _pendingDisable = null;
            // All teardown happens here, after OK, including dynamically loaded services.
            try { Utils.AnnullaSondaggio(); } catch { }
            try { Utils.AbortAllMessages(); } catch { }
            try { BanModSyncTransport.Stop(); } catch { }
            try { BanModCore.StopAllPremiumModules(); } catch { }
            try { BanModLoginRuntime.Shutdown(); } catch { }
            foreach (string id in new[] { "com.GianniBart.BanMod", "com.GianniBart", "com.GianniBart.invite",
                "com.GianniBart.ffa", "com.GianniBart.chat", "com.GianniBart.login" })
            {
                try { Harmony.UnpatchID(id); } catch { }
            }
            disable?.Invoke();
        }

        internal static bool Show(bool cheatDetected)
        {
            if (_cheatShown || (!cheatDetected && _unknownShown))
                return true;

            string message = cheatDetected
                ? "BanMod has detected a cheat modification on the blacklist. BanMod will be completely disabled when you press OK. " +
                  "To use BanMod, remove the cheat modifications and restart the game."
                : "BanMod has detected other mods that are not on its whitelist. For security reasons, " +
                  "some features, including Extra Services such as FFA, will be disabled. " +
                  "Remove the unapproved mods and restart the game to restore these features.";

            // Fixed text only: no mod names, tokens or server-supplied markup.
            BanModCommunicationUi.EnsureCreated();
            var ui = BanModCommunicationUi.Instance;
            if (ui == null) return false;
            if (cheatDetected)
            {
                if (!ui.ShowSecurityAcknowledgement("BanMod - Security Notice", message, OnCheatAcknowledged))
                    return false;
            }
            else
            {
                ui.ShowMessagePopup("BanMod - Security Notice", message);
            }

            if (cheatDetected) _cheatShown = true;
            else _unknownShown = true;
            Debug.LogWarning("[BANMOD] " + message);
            return true;
        }
    }

}
