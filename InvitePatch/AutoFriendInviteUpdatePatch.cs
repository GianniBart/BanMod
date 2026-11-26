// credits and licenses in the resources folder
using HarmonyLib;
using System;
using UnityEngine;

namespace BanMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class AutoFriendInviteUpdatePatch
    {
        public static void Postfix()
        {
            try
            {
                AutoFriendInviteManager.Update();
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInvite] Update failed: " + ex);
            }
        }
    }
}