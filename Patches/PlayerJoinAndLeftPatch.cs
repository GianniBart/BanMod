//credits and licenses in the resources folder/

using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Data;
using InnerNet;
using Rewired.Utils.Platforms.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using static BanMod.ImmortalManager;
using static BanMod.Utils;

namespace BanMod;




[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
static class OnPlayerJoined_ClientData_Patch
{
    public static void Postfix([HarmonyArgument(0)] ClientData client)
    {
        if (client == null)
            return;

        if (AmongUsClient.Instance == null)
            return;

        if (AmongUsClient.Instance.AmHost &&
            Options.CheckBlockList.GetBool() &&
            DestroyableSingleton<FriendsListManager>.Instance != null &&
            DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            BanManager.AddBanPlayer(client, "Blocked List");

            if (HudManager.Instance?.Notifier != null)
            {
                NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                    HudManager.Instance.Notifier,
                    $"{client.PlayerName} {Translator.GetString("Blocked")}"
                );
            }

            return;
        }

        if (LobbyDepartureGuard.OnJoin(client))
            return;

        BanManager.CheckBanPlayer(client);

        if (AmongUsClient.Instance != null)
            AmongUsClient.Instance.StartCoroutine(BanManager.WaitAndCheckAll(client));

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            BanMod.SendModDetectionRPC();
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class PlayerControlStartUnifiedPatch
{
    public static System.Collections.Generic.Dictionary<string, string> PlayerNamesByFriendCode = new System.Collections.Generic.Dictionary<string, string>();

    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null) return;

        if (__instance.AmOwner && GameStates.isOnlineGame)
        {
            __instance.StartCoroutine(InitialHandshake());
        }
        if (AmongUsClient.Instance.AmHost && __instance.AmOwner && Options.GameMode.Selected == GameModeType.FFA)
        {
            FfaExternalBridge.SyncGameMode();
            FfaExternalBridge.SyncVentSeconds();
            FfaExternalBridge.SyncVentMode();
            FfaExternalBridge.SyncTeamMode();
            FfaExternalBridge.SyncTeamCount();
        }
    }
    private static IEnumerator InitialHandshake()
    {
        yield return new WaitForSeconds(5f);
        if (GameStates.isLobby)
        {
            Utils.SendModdedHandshake();
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal static class AntiLeaveSpamPatch
{
    private static void Prefix(
        [HarmonyArgument(0)] ClientData data,
        out string __state)
    {
        __state = null;

        try
        {
            if (data == null)
                return;

            LobbyDepartureGuard.OnLeave(data);

            __state = data.Character?.FriendCode;

            if (string.IsNullOrEmpty(__state))
                __state = data.FriendCode;
        }
        catch (Exception ex)
        {
            BMLogger.LogError(
                $"[Cleanup Error] OnPlayerLeft Prefix fallita: {ex}");
        }
    }

    private static void Postfix(
        [HarmonyArgument(0)] ClientData data,
        string __state)
    {
        try
        {
            string friendCode = __state;

            if (string.IsNullOrEmpty(friendCode))
                return;

            if (PlayerControlStartUnifiedPatch.PlayerNamesByFriendCode.Remove(friendCode))
            {
                BMLogger.LogInfo(
                    $"[Cleanup] Rimossi dati per friendCode {friendCode}");
            }

            ChatCommands.moddedNamesByFriendCode.Remove(friendCode);
            ChatCommands.originalNamesByFriendCode.Remove(friendCode);
        }
        catch (Exception ex)
        {
            BMLogger.LogError(
                $"[Cleanup Error] OnPlayerLeft cleanup fallita: {ex}");
        }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
public static class ExitGamePatch
{
    public static void Prefix()
    {
        LobbyDepartureGuard.Reset();
        ChatCommands.ClearLocalPlayerNameData();
    }
}
internal static class LobbyDepartureGuard
{
    private const long BotWindowMs = 30_000;
    private const int BotEventCount = 10; 
    private const string BotReason = "Likely a bot";
    private const string StartReason = "Repeated game exit during startup";

    private sealed class PlayerState
    {
        internal readonly System.Collections.Generic.Queue<(bool Join, long Time)> Events = new();
        internal int? FirstStartExit;
        internal bool Warned;
        internal bool Banned;
    }

    private static readonly System.Collections.Generic.Dictionary<string, PlayerState> Players =
        new(StringComparer.OrdinalIgnoreCase);

    private static int _gameId;
    private static int _startNumber;
    private static GameStartManager _startManager;

    internal static void Reset()
    {
        Players.Clear();
        _gameId = 0;
        _startNumber = 0;
        _startManager = null;
    }

    private static bool IsHostLobby()
    {
        var client = AmongUsClient.Instance;

        if (client == null || !client.AmHost || BanMod.IsBanModDisabled ||
            !GameStates.isOnlineGame || !GameStates.isLobby)
            return false;

        if (_gameId != client.GameId)
        {
            Reset();
            _gameId = client.GameId;
        }

        return true;
    }

    internal static void OnStart(GameStartManager manager)
    {
        if (!IsHostLobby())
            return;

        _startManager = manager;
        _startNumber++;
    }

    private static PlayerState GetState(ClientData player)
    {
        if (player == null || BanMod.IsProtected(player) ||
            AllowedManager.IsFriends(player.FriendCode))
            return null;

        string puid = player.ProductUserId?.Trim();
        if (!string.IsNullOrEmpty(puid) &&
            DestroyableSingleton<FriendsListManager>.InstanceExists)
        {
            var friends = DestroyableSingleton<FriendsListManager>.Instance?.Friends;
            if (friends != null)
            {
                for (int i = 0; i < friends.Count; i++)
                {
                    var friend = friends[i];
                    if (friend != null &&
                        string.Equals(friend.FriendPuid, puid, StringComparison.Ordinal))
                        return null;
                }
            }
        }

        string key;
        if (!string.IsNullOrEmpty(puid))
            key = "puid:" + puid;
        else if (!string.IsNullOrWhiteSpace(player.FriendCode))
            key = "fc:" + player.FriendCode.Trim();
        else
            return null;

        if (!Players.TryGetValue(key, out var state))
        {
            state = new PlayerState();
            Players.Add(key, state);
        }

        return state;
    }

    private static void RecordEvent(PlayerState state, bool joined)
    {
        if (!Options.EnableAntiBotSystem.GetBool())
        {
            state.Events.Clear();
            return;
        }

        long now = Environment.TickCount64;

        while (state.Events.Count > 0 &&
               now - state.Events.Peek().Time > BotWindowMs)
            state.Events.Dequeue();

        if (state.Events.Count > 0 && state.Events.Last().Join == joined)
            return;

        state.Events.Enqueue((joined, now));

        if (state.Events.Count > BotEventCount)
            state.Events.Dequeue();
    }

    internal static bool OnJoin(ClientData player)
    {
        if (!IsHostLobby())
            return false;

        var state = GetState(player);
        if (state == null)
            return false;

        if (state.Banned)
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            return true;
        }
        bool listed = SharedBotList.TryGet(player.FriendCode, out var entry);

        if (listed && !entry.UnlockedByMe)
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            return true;
        }

        string friendCode = player.FriendCode;
        int separator = friendCode?.IndexOf('#') ?? -1;

        if (!listed && Options.EnableAntiBotSystem.GetBool() &&
            separator > 0 && friendCode.Take(separator).Any(char.IsDigit))
        {
            SharedBotList.Report(player, "BOT");
        }
        RecordEvent(state, joined: true);

        if (!listed &&
            Options.EnableAntiBotSystem.GetBool() &&
            state.Events.Count == BotEventCount &&
            !state.Events.Peek().Join)
        {
            state.Events.Clear();
            SharedBotList.Report(player, BotReason);
        }

        if (state.FirstStartExit.HasValue && !state.Warned)
        {
            state.Warned = true;
            Notify(BanMod.GetRealPlayerName(player) +
                "returned after having left during the start.. " +
                "He will be banned if he leaves during a subsequent start..");
        }

        return false;
    }

    internal static void OnLeave(ClientData player)
    {
        if (!IsHostLobby())
            return;

        var state = GetState(player);
        if (state == null || state.Banned)
            return;

        RecordEvent(state, joined: false);

        if (_startManager == null ||
            _startManager.startState != GameStartManager.StartingStates.Countdown)
            return;

        if (!state.FirstStartExit.HasValue)
        {
            state.FirstStartExit = _startNumber;
            return;
        }

        if (state.Warned && state.FirstStartExit.Value != _startNumber)
        {
            state.Banned = true;
            SaveBan(player, StartReason);
        }
    }

    private static void SaveBan(ClientData player, string reason)
    {
        try
        {
            bool isBotBan = reason == BotReason;
            BanManager.AddBanPlayer(player, reason, forceSave: isBotBan);

            if (!BanMod.AddBanToList.Value && !isBotBan)
                Notify($"{BanMod.GetRealPlayerName(player)} ({reason})");
        }
        catch (Exception ex)
        {
            BMLogger.LogError(
                "[BanMod] Errore nel salvataggio del ban: " + ex.Message);
        }
    }

    private static void Notify(string message)
    {
        var notifier = HudManager.Instance?.Notifier;
        if (notifier != null)
            NotificationPopper_AddInfoMessagePatch.AddInfoMessage(notifier, message);
    }
}