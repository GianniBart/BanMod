//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
using Innersloth.IO;
using Rewired.Utils.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TMPro;
using static BanMod.Utils;

namespace BanMod;



[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class DefaultModdedChatResetPatch
{
    public static void Postfix()
    {
        if (BanMod.IsBanModDisabled) return;
        ModdedOriginalChatManager.Reset();
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
class ChatControllerUpdatePatch
{
    private static SpriteRenderer QuickChatIcon;
    private static SpriteRenderer OpenBanMenuIcon;
    private static SpriteRenderer OpenKeyboardIcon;

    public static int CurrentHistorySelection = -1;
    public static bool timelastmessage;
    public static ChatController Instance;


    private static readonly FieldInfo ChatNotifyDotField =
        typeof(ChatController).GetField(
            "chatNotifyDot",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

    private static readonly FieldInfo ChatNotificationField =
        typeof(ChatController).GetField(
            "chatNotification",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );




    public static void Prefix()
    {
        if (BanMod.IsBanModDisabled) return;
        if (AmongUsClient.Instance != null &&
            AmongUsClient.Instance.AmHost &&
            DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
        {
            DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
        }

    }

    public static void Postfix(ChatController __instance)
    {
        if (__instance == null)
            return;

        Instance = __instance;

        if (BanModServerSelection.IsVanilla)
        {
            if (__instance.timeSinceLastMessage < 1.5f)
            {
                __instance.timeSinceLastMessage = 1.5f;
            }
        }
        else if (BanModServerSelection.IsModded25)
        {
            {
                __instance.timeSinceLastMessage = 3f;
            }
        }

        timelastmessage = __instance.timeSinceLastMessage >= (BanModServerSelection.IsVanilla ? 1.5f : 0f);
        if (__instance.freeChatField != null && __instance.freeChatField.textArea != null && BanModServerSelection.IsVanilla)
        {
            __instance.freeChatField.textArea.characterLimit = 120;
            __instance.freeChatField.textArea.AllowSymbols = true;
            __instance.freeChatField.textArea.AllowEmail = true;
            __instance.freeChatField.textArea.allowAllCharacters = true;
        }
        else
        {
            __instance.freeChatField.textArea.characterLimit = 1200;
            __instance.freeChatField.textArea.AllowSymbols = true;
            __instance.freeChatField.textArea.AllowEmail = true;
            __instance.freeChatField.textArea.allowAllCharacters = true;
        }

        ApplyChatTheme(__instance);

        HandleClipboardAndHistory(__instance);

    }

    private static void ApplyChatTheme(ChatController chat)
    {
        if (chat == null)
            return;

        if (BanMod.DarkTheme.Value)
        {
            if (chat.freeChatField != null)
            {
                chat.freeChatField.background.color = new Color(0.1f, 0.1f, 0.1f, 1f);

                if (chat.freeChatField.textArea != null)
                {
                    chat.freeChatField.textArea.compoText.Color(Color.white);
                    chat.freeChatField.textArea.outputText.color = Color.white;
                }
            }

            if (chat.quickChatField != null)
            {
                chat.quickChatField.background.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                chat.quickChatField.text.color = Color.white;
            }

            if (QuickChatIcon == null)
                QuickChatIcon = GameObject.Find("QuickChatIcon")?.transform.GetComponent<SpriteRenderer>();
            else
                QuickChatIcon.sprite = Utils.LoadSprite("BanMod.Resources.image.DarkQuickChat.png", 100f);

            if (OpenBanMenuIcon == null)
                OpenBanMenuIcon = GameObject.Find("OpenBanMenuIcon")?.transform.GetComponent<SpriteRenderer>();
            else
                OpenBanMenuIcon.sprite = Utils.LoadSprite("BanMod.Resources.image.DarkReport.png", 100f);

            if (OpenKeyboardIcon == null)
                OpenKeyboardIcon = GameObject.Find("OpenKeyboardIcon")?.transform.GetComponent<SpriteRenderer>();
            else
                OpenKeyboardIcon.sprite = Utils.LoadSprite("BanMod.Resources.image.DarkKeyboard.png", 100f);

            if (GameStates.IsDead)
            {
                if (chat.freeChatField != null)
                    chat.freeChatField.background.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

                if (chat.quickChatField != null)
                    chat.quickChatField.background.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            }
        }
        else
        {
            if (chat.freeChatField != null)
            {
                chat.freeChatField.background.color = Color.white;

                if (chat.freeChatField.textArea != null)
                {
                    chat.freeChatField.textArea.compoText.Color(Color.white);
                    chat.freeChatField.textArea.outputText.color = Color.black;
                }
            }

            if (QuickChatIcon == null)
                QuickChatIcon = GameObject.Find("QuickChatIcon")?.transform.GetComponent<SpriteRenderer>();
            else
                QuickChatIcon.sprite = Utils.LoadSprite("BanMod.Resources.image.QuickChat.png", 100f);

            if (OpenBanMenuIcon == null)
                OpenBanMenuIcon = GameObject.Find("OpenBanMenuIcon")?.transform.GetComponent<SpriteRenderer>();
            else
                OpenBanMenuIcon.sprite = Utils.LoadSprite("BanMod.Resources.image.Report.png", 100f);

            if (OpenKeyboardIcon == null)
                OpenKeyboardIcon = GameObject.Find("OpenKeyboardIcon")?.transform.GetComponent<SpriteRenderer>();
            else
                OpenKeyboardIcon.sprite = Utils.LoadSprite("BanMod.Resources.image.Keyboard.png", 100f);

            if (GameStates.IsDead)
            {
                if (chat.freeChatField != null)
                    chat.freeChatField.background.color = new Color(1f, 1f, 1f, 0.5f);

                if (chat.quickChatField != null)
                    chat.quickChatField.background.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }

    private static void HandleClipboardAndHistory(ChatController chat)
    {
        if (chat == null || chat.freeChatField == null || chat.freeChatField.textArea == null)
            return;

        if (!chat.freeChatField.textArea.hasFocus)
            return;

        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
            ClipboardHelper.PutClipboardString(chat.freeChatField.textArea.text);

        //if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
        //    chat.freeChatField.textArea.SetText(chat.freeChatField.textArea.text + GUIUtility.systemCopyBuffer);
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
        {
            if (BanModServerSelection.IsModded25)
            {
                BanModSymbolPicker.PasteUnicodeFromClipboard(chat.freeChatField.textArea, GUIUtility.systemCopyBuffer);
            }
            else
            {
                chat.freeChatField.textArea.SetText(chat.freeChatField.textArea.text + GUIUtility.systemCopyBuffer);
            }
        }

        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
        {
            ClipboardHelper.PutClipboardString(chat.freeChatField.textArea.text);
            chat.freeChatField.textArea.SetText("");
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && ChatCommands.ChatHistory.Any())
        {
            CurrentHistorySelection = Mathf.Clamp(--CurrentHistorySelection, 0, ChatCommands.ChatHistory.Count - 1);
            chat.freeChatField.textArea.SetText(ChatCommands.ChatHistory[CurrentHistorySelection]);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && ChatCommands.ChatHistory.Any())
        {
            CurrentHistorySelection++;

            if (CurrentHistorySelection < ChatCommands.ChatHistory.Count)
                chat.freeChatField.textArea.SetText(ChatCommands.ChatHistory[CurrentHistorySelection]);
            else
                chat.freeChatField.textArea.SetText("");
        }
    }

    public static void TryFlashChatNotifyDot()
    {
        try
        {
            if (Instance == null)
                return;

            ChatNotification notification = null;
            SpriteRenderer dot = null;

            try
            {
                if (ChatNotificationField != null)
                {
                    object raw = ChatNotificationField.GetValue(Instance);
                    notification = (raw as Il2CppObjectBase)?.TryCast<ChatNotification>();
                }
            }
            catch
            {
            }

            try
            {
                if (ChatNotifyDotField != null)
                {
                    object raw = ChatNotifyDotField.GetValue(Instance);
                    dot = (raw as Il2CppObjectBase)?.TryCast<SpriteRenderer>();
                }
            }
            catch
            {
            }

            notification?.Close();

            if (dot != null)
                dot.enabled = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[BanMod] TryFlashChatNotifyDot error: " + ex);
        }
    }

    public static void TrySetChatNotification(PlayerControl sourcePlayer, string text)
    {
        try
        {
            if (Instance == null || sourcePlayer == null)
                return;

            ChatNotification notification = null;

            if (ChatNotificationField != null)
            {
                object raw = ChatNotificationField.GetValue(Instance);
                notification = (raw as Il2CppObjectBase)?.TryCast<ChatNotification>();
            }

            notification?.SetUp(sourcePlayer, text);

            AudioClip messageSound = null;

            if (messageSound != null)
                SoundManager.Instance.PlaySound(messageSound, false, 1f, null).pitch = 0.5f + sourcePlayer.PlayerId / 15f;

            TryFlashChatNotifyDot();
        }
        catch
        {
        }
    }

    public static PassiveButton SafeGetPassiveButton(Component source)
    {
        if (source == null)
            return null;

        try
        {
            Component c = source.GetComponent("PassiveButton");

            if (c == null)
                return null;

            return (c as Il2CppObjectBase)?.TryCast<PassiveButton>();
        }
        catch
        {
            return null;
        }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
class ChatSendPatch
{
    static bool Prefix(ChatController __instance)
    {
        if (__instance == null)
            return false;

        string freeMessage = __instance.freeChatField?.textArea?.text;
        bool quickChatReady = __instance.quickChatMenu != null && __instance.quickChatMenu.CanSend;

        bool isModdedChat =
            ModdedOriginalChatManager.Enabled &&
            !quickChatReady &&
            !string.IsNullOrWhiteSpace(freeMessage);

        if (isModdedChat)
            return true;

        if (!string.IsNullOrEmpty(freeMessage) && freeMessage.Length > 120 && BanModServerSelection.IsVanilla)
        {
            NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                HudManager.Instance.Notifier,
                $"Messaggio troppo lungo! Max 120 caratteri. Messaggio digitato: \"{freeMessage}\" (Lunghezza: {freeMessage.Length})");

            return false;
        }

        if (!quickChatReady && string.IsNullOrWhiteSpace(freeMessage))
            return false;

        if (__instance.timeSinceLastMessage < 1.5f && BanModServerSelection.IsVanilla)
        {
            if (HudManager.Instance?.Notifier != null)
            {
                NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                    HudManager.Instance.Notifier,
                    Translator.GetString("Waitasecond"));
            }

            return false;
        }

        // Manual chat uses the game's own cooldown above. MessageBlocker belongs
        // to automatic messages: poll announcements must not lock out human votes.
        return true;
    }
}
[HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
internal class UpdateCharCountPatch
{
    public static void Postfix(FreeChatInputField __instance)
    {
        if (__instance == null || __instance.textArea == null || __instance.charCountText == null)
            return;

        int length = __instance.textArea.text.Length;

        __instance.charCountText.SetText(
            length <= 0
                ? Translator.GetString("BANMOD")
                : $"{length}/{__instance.textArea.characterLimit}");

        __instance.charCountText.enableWordWrapping = false;

        if (BanModServerSelection.IsVanilla)
        {
            if (length < (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost ? 80 : 100))
                __instance.charCountText.color = Color.cyan;
            else if (length < (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost ? 101 : 120))
                __instance.charCountText.color = new Color(1f, 1f, 0f, 1f);
            else
                __instance.charCountText.color = Color.red;
        }
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Start))]
public static class AllowPaste_TextBoxTMP_Start_Postfix
{
    public static void Postfix(TextBoxTMP __instance)
    {
        if (__instance == null)
            return;

        __instance.allowAllCharacters = true;
        __instance.AllowEmail = true;
        __instance.AllowSymbols = true;
    }
}

public static class ChatCopyData
{
    public static readonly Dictionary<ChatBubble, string> Messages =
        new Dictionary<ChatBubble, string>();
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
public static class ChatBubblePatch
{
    public static void Postfix(ChatBubble __instance, string chatText)
    {
        if (__instance == null)
            return;

        ChatCopyData.Messages[__instance] = chatText;
    }
}


[HarmonyPatch(typeof(ChatController), "Update")]
public static class ChatCopyPatch
{
    public static void Postfix()
    {
        bool ctrl =
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl);

        if (!ctrl || !Input.GetKeyDown(KeyCode.C))
            return;

        ChatBubble bubble = FindBubbleUnderMouse();

        if (bubble == null)
            return;

        if (ChatCopyData.Messages.TryGetValue(
            bubble,
            out string text))
        {
            GUIUtility.systemCopyBuffer = text;

            Debug.Log(
                $"[ChatCopy] Copied: {text}");
        }
    }

    private static ChatBubble FindBubbleUnderMouse()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return null;

        Vector3 mousePos =
            Input.mousePosition;

        ChatBubble nearestBubble = null;
        float nearestDistance =
            float.MaxValue;

        foreach (KeyValuePair<ChatBubble, string> pair
                 in ChatCopyData.Messages)
        {
            ChatBubble bubble = pair.Key;

            if (bubble == null)
                continue;

            if (bubble.TextArea == null)
                continue;

            Vector3 screenPos =
                cam.WorldToScreenPoint(
                    bubble.TextArea.transform.position);

            float distance =
                Vector2.Distance(
                    mousePos,
                    screenPos);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBubble = bubble;
            }
        }

        if (nearestDistance > 120f)
            return null;

        return nearestBubble;
    }
}
[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public static class TextBoxTMP_IsCharAllowed_Patch
{
    public static bool Prefix(char __0, ref bool __result)
    {
        if (BanModServerSelection.IsModded25)
            return true;

        if ((__0 == '<' || __0 == '>' || __0 == '[' || __0 == ']') && (BanModServerSelection.IsVanilla))
        {
            __result = false;
            return false;
        }

        if (char.IsControl(__0))
            return true;

        __result = true;
        return false;
    }
}
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatController_AddChat_GhostChatPatch
{
    private static bool CanSeeGhostChat()
    {
        if (!BanMod.EnableGhostChat.Value)
            return false;

        if (GameManager.Instance.IsHideAndSeek())
            return true;

        switch (Options.GameMode.Selected)
        {
            case GameModeType.SnS:
            case GameModeType.TaskRun:
            case GameModeType.FFA:
            case GameModeType.FFATeam:
            case GameModeType.ZombieMode:
            case GameModeType.PnS:
            case GameModeType.HotPotatoModded:
            case GameModeType.RoomRush:
            case GameModeType.Assassin:
            case GameModeType.KillRace:
            case GameModeType.DeathRun:
                return true;

            default:
                return false;
        }
    }

    public static bool Prefix(
        PlayerControl sourcePlayer,
        string chatText,
        bool censor,
        ChatController __instance)
    {
        if (!CanSeeGhostChat())
            return true;

        if (!sourcePlayer || !PlayerControl.LocalPlayer)
            return true;

        NetworkedPlayerInfo localData = PlayerControl.LocalPlayer.Data;
        NetworkedPlayerInfo sourceData = sourcePlayer.Data;

        if (sourceData == null || localData == null)
            return true;

        if (localData.IsDead)
            return true;

        if (!sourceData.IsDead)
            return true;


        ChatBubble pooledBubble = __instance.GetPooledBubble();

        try
        {
            pooledBubble.transform.SetParent(__instance.scroller.Inner);
            pooledBubble.transform.localScale = Vector3.one;

            bool isLocal = sourcePlayer == PlayerControl.LocalPlayer;

            if (isLocal)
            {
                pooledBubble.SetRight();
            }
            else
            {
                pooledBubble.SetLeft();
            }

            bool didVote =
                MeetingHud.Instance &&
                MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);

            pooledBubble.SetCosmetics(sourceData);

            __instance.SetChatBubbleName(
                pooledBubble,
                sourceData,
                sourceData.IsDead,
                didVote,
                PlayerNameColor.Get(sourceData),
                null
            );

            if (censor &&
                AmongUs.Data.DataManager.Settings.Multiplayer.CensorChat)
            {
                chatText = BlockedWords.CensorWords(chatText, false);
            }

            pooledBubble.SetText(chatText);
            pooledBubble.AlignChildren();

            __instance.AlignAllBubbles();

            if (!__instance.IsOpenOrOpening &&
                __instance.notificationRoutine == null)
            {
                __instance.notificationRoutine =
                    __instance.StartCoroutine(__instance.BounceDot());
            }

            if (!isLocal && !__instance.IsOpenOrOpening)
            {
                SoundManager.Instance
                    .PlaySound(__instance.messageSound, false)
                    .pitch = 0.5f + sourcePlayer.PlayerId / 15f;

                __instance.chatNotification.SetUp(
                    sourcePlayer,
                    chatText
                );
            }
        }
        catch (Exception ex)
        {
            try
            {
                ChatController.Logger.Error(ex.ToString(), null);
            }
            catch
            {
            }

            try
            {
                __instance.chatBubblePool.Reclaim(pooledBubble);
            }
            catch
            {
            }
        }

        return false;
    }
}