//credits and licenses in the resources folder
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using System.Linq;
using TMPro;
using UnityEngine;
using static BanMod.ChatCommands;

namespace BanMod;

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
class ChatBubbleSetRightPatch
{


    public static void Postfix(ChatBubble __instance)
    {
        if (Options.ChatLeft.GetBool()) __instance.SetLeft();
    }
}
[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
class ChatBubbleSetNamePatch
{
    public static void Postfix(ChatBubble __instance, [HarmonyArgument(1)] bool isDead)
    {
        if (BanMod.IsBanModDisabled) return;
        bool darkTheme = BanMod.DarkTheme.Value;
        bool hasCustomColor = ChatColorManager.currentChatColor.HasValue;

        if (hasCustomColor)
        {
            __instance.TextArea.color = ChatColorManager.currentChatColor.Value;
            __instance.Background.color = darkTheme ? new Color(0.1f, 0.1f, 0.1f, 1f) : Color.white;
        }
        else
        {
            __instance.TextArea.color = darkTheme ? Color.white : Color.black;
            __instance.Background.color = darkTheme ? new Color(0.1f, 0.1f, 0.1f, 1f) : Color.white;
        }

        if (isDead && darkTheme)
        {
            __instance.Background.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        }
        else if (isDead && !darkTheme)
        {
            __instance.Background.color = new Color(1f, 1f, 1f, 0.5f);


        }
    }
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
public static class ChatBubble_SetName
{
    public static void Postfix(ChatBubble __instance)
    {
        if (BanMod.IsBanModDisabled) return;

        if (PlayerControl.LocalPlayer.Data.IsDead && BanMod.SeeRoleMeeting.Value && PlayerControl.LocalPlayer.Data.RoleType != RoleTypes.GuardianAngel)
        {
            Utils.ChatNametags(__instance);
        }
    }
}