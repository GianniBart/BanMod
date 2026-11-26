
//credits and licenses in the resources folder
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BanMod.Utils;

namespace BanMod
{
    public enum CustomRPC : byte
    {
        ModdedHandshake = 212,
        ProxySendChat = 213,
        SetSpecialKiller = 214,
        SetExiler = 215,
        SetJester = 216,
        ModeratorAction = 227,
        HostTripleBoolUpdate = 217,
        RoleCommandAction = 218,
        HandshakeModded = 219,
        ModdedAllChat = 220,
        HostRoleOptionsUpdate = 221,
        SyncPlayerVisual = 223,
        FfaSettings = 253
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    internal class UnifiedRPCHandlerPatch
    {
        private const string CurrentModVersion = BanMod.PluginVersion;
        private const string LogTag = "RPCHandler";
        private const int MaxRpcDataSize = 256;

        private const int MaxTotalRpcPerWindow = 25;
        private const float TotalRpcWindowSeconds = 0.35f;

        private const int MaxSameRpcPerWindow = 15;
        private const float SameRpcWindowSeconds = 0.15f;

        private const int MaxPlayAnimationPerWindow = 10;
        private const float PlayAnimationWindowSeconds = 0.40f;

        private const int MaxChatMessagesPerWindow = 5;
        private const float ChatWindowSeconds = 2.0f;

        public static readonly Dictionary<byte, string> ModdedClients = new();
        public static readonly HashSet<byte> AlreadyHandledCheaters = new();
        private static readonly HashSet<byte> AlreadyNotifiedModdedClients = new();

        private static readonly Dictionary<int, Queue<float>> GlobalRpcRateByClient = new();
        private static readonly Dictionary<int, Dictionary<int, Queue<float>>> RpcRateByClientAndCall = new();
        private static readonly Dictionary<int, Queue<float>> ChatRateByClient = new();
        private static readonly Dictionary<byte, string> KnownCheatRpcSignatures = new()
        {
            { 121, "Chocoo" },
            { 167, "TuffMenu" },
            { 164, "Hydra / Sicko / SickoMenu" },
            { 176, "HostGuard / TOH" },
            { 195, "Polar" },
            { 204, "Polar" },
            { 154, "GNC" },
            { 250, "KillNet" },
            { 80,  "KillNet" },
            { 85,  "KillNet" },
            { 150, "BetterAmongUs / GreaterAmongUs" },
            { 103, "Askinchik" },
            { 144, "Gaff Menu" },
            { 145, "Gaff Menu" },
            { 188, "GMM" },
            { 189, "GMM" },
            { 169, "Malum Menu" },
            { 133, "Lunar / NjordMenu" },
            { 210, "Lunar / NjordMenu" },
            { 202, "CrewCore / ModMenuCrew" },
            { 89, "NjordMenu (Old)" }
        };
        private static string GetPlayerName(PlayerControl player)
            => player?.Data?.PlayerName ?? player?.name ?? "Unknown";

        private static string GetPlayerLabel(PlayerControl player)
            => player == null ? "null-player" : $"{GetPlayerName(player)} [pid={player.PlayerId}]";

        public static void ResetModdedNotifications()
        {
            AlreadyNotifiedModdedClients.Clear();
        }

        private static bool IsHostPlayer(PlayerControl player)
        {
            if (player == null || AmongUsClient.Instance == null)
                return false;

            try
            {
                if (player.OwnerId == AmongUsClient.Instance.HostId)
                    return true;
            }
            catch
            {
            }

            try
            {
                if (player.Data != null && player.Data.ClientId == AmongUsClient.Instance.HostId)
                    return true;
            }
            catch
            {
            }

            return false;
        }

        private static void LogToFileOnly(string text)
        {
            BMLogger.LogDebug(text, "RPC-VERBOSE");
        }

        private static string CleanLogText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static int GetConfiguredAction()
        {
            try
            {
                return Options.ActionCheater.GetValue();
            }
            catch
            {
                return 2;
            }
        }

        private static string GetActionLabel(int action)
        {
            if (action == 0) return "warned";
            return action == 2 ? "banned" : "kicked";
        }

        private static string GetRpcName(int callId, bool hasByteId, byte callIdByte)
        {
            if (!hasByteId)
                return $"RPC_INT_{callId}";

            if (Enum.IsDefined(typeof(CustomRPC), callIdByte))
                return $"CustomRPC.{(CustomRPC)callIdByte}";

            if (Enum.IsDefined(typeof(RpcCalls), callIdByte))
                return $"RpcCalls.{(RpcCalls)callIdByte}";

            try
            {
                string mapped = RpcMap.GetRpcName(callIdByte);
                if (!string.IsNullOrWhiteSpace(mapped))
                    return mapped;
            }
            catch
            {
            }

            return $"RPC_BYTE_{callIdByte}";
        }

        private static void RegisterReceivedRpc(PlayerControl player, int callId, bool hasByteId, byte callIdByte, MessageReader reader)
        {
            string rpcName = GetRpcName(callId, hasByteId, callIdByte);
            int length = reader?.Length ?? -1;
            int position = reader?.Position ?? -1;
            int remaining = reader == null ? -1 : length - position;
            int clientId = player != null ? player.GetClientId() : -1;

            LogToFileOnly(
                $"RPC received | Player={GetPlayerLabel(player)} | ClientId={clientId} | " +
                $"CallIdInt={callId} | CallIdByte={(hasByteId ? callIdByte.ToString() : "N/A")} | " +
                $"Name={rpcName} | Position={position} | Length={length} | Remaining={remaining}"
            );
        }

        private static void RegisterReceivedMessage(PlayerControl player, string type, string content)
        {
            LogToFileOnly(
                $"Message received | Type={type} | Player={GetPlayerLabel(player)} | " +
                $"Text={CleanLogText(content)}"
            );
        }

        private static bool RegisterRateHit(
            Dictionary<int, Queue<float>> store,
            int key,
            float now,
            float windowSeconds,
            int maxAllowed,
            out int currentCount)
        {
            if (!store.TryGetValue(key, out var queue))
            {
                queue = new Queue<float>();
                store[key] = queue;
            }

            while (queue.Count > 0 && now - queue.Peek() > windowSeconds)
                queue.Dequeue();

            queue.Enqueue(now);
            currentCount = queue.Count;

            return currentCount > maxAllowed;
        }

        private static bool RegisterPerRpcRateHit(
            int clientId,
            int callId,
            float now,
            float windowSeconds,
            int maxAllowed,
            out int currentCount)
        {
            if (!RpcRateByClientAndCall.TryGetValue(clientId, out var rpcMap))
            {
                rpcMap = new Dictionary<int, Queue<float>>();
                RpcRateByClientAndCall[clientId] = rpcMap;
            }

            if (!rpcMap.TryGetValue(callId, out var queue))
            {
                queue = new Queue<float>();
                rpcMap[callId] = queue;
            }

            while (queue.Count > 0 && now - queue.Peek() > windowSeconds)
                queue.Dequeue();

            queue.Enqueue(now);
            currentCount = queue.Count;

            return currentCount > maxAllowed;
        }

        private static void ShowScreenMessage(string message)
        {
            try
            {
                AntiCheat.ShowHudNotification(message);
            }
            catch (Exception ex)
            {
                BMLogger.Warn($"Cannot show screen notification: {ex.Message}", LogTag);
            }
        }

        private static void NotifyUnauthorizedHostRpc(
            PlayerControl sender,
            string rpcName)
        {
            string senderName = GetPlayerName(sender);

            ShowScreenMessage(
                $"AntiCheat: {senderName} ha inviato {rpcName} " +
                "senza essere host. RPC bloccato."
            );
        }

        private static void ApplySuspicionAction(
            PlayerControl player,
            ClientData client,
            int cheaterClientId,
            string banReason,
            string screenMessage,
            bool addBan = true,
            bool addCheater = true,
            bool sendWarning = true)
        {
            if (player == null)
                return;

            int action = GetConfiguredAction();

            byte playerId = player.PlayerId;
            string name = GetPlayerName(player);
            bool firstHandle = AlreadyHandledCheaters.Add(playerId);

            BMLogger.Warn(
                $"Anti-cheat action applied | Player={name} | ClientId={cheaterClientId} | Action={action} | Reason={banReason}",
                LogTag);

            ShowScreenMessage(screenMessage);

            if (client != null && firstHandle)
            {
                try
                {
                    if (addCheater)
                    {
                        CheaterManager.AddPlayer(client, banReason);
                        BMLogger.Info($"Player added to cheater list: {name} | ClientId={cheaterClientId}", LogTag);
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Error($"Error adding to cheater list: {ex}", LogTag);
                }

                try
                {
                    if (addBan && action == 2)
                    {
                        BanManager.AddBanPlayer(client, banReason);
                        BMLogger.Info($"Player added to ban list: {name} | ClientId={cheaterClientId}", LogTag);
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Error($"Error adding to ban list: {ex}", LogTag);
                }
            }

            if (firstHandle && sendWarning)
            {
                try
                {
                    AntiCheat.SendGlobalHackWarning();
                }
                catch (Exception ex)
                {
                    BMLogger.Warn($"Cannot send global anti-cheat warning: {ex.Message}", LogTag);
                }
            }

            if (action > 0)
            {
                try
                {
                    if (cheaterClientId >= 0 && AmongUsClient.Instance != null)
                    {
                        AmongUsClient.Instance.KickPlayer(cheaterClientId, action == 2);
                        BMLogger.Warn(
                            $"{(action == 2 ? "Ban" : "Kick")} sent to client | Player={name} | ClientId={cheaterClientId}",
                            LogTag);
                    }
                    else
                    {
                        BMLogger.Warn(
                            $"Cannot apply kick/ban: Invalid ClientId ({cheaterClientId}) | Player={name}",
                            LogTag);
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Error($"Error executing kick/ban for {name} | ClientId={cheaterClientId}: {ex}", LogTag);
                }
            }
            else
            {
                BMLogger.LogDebug($"Action is 0 (Warning Only) for {name}. No kick/ban applied.", LogTag);
            }
        }

        private static bool IsRpcDataValid(MessageReader reader, int callId, bool isCustom, string playerName)
        {
            if (reader == null)
            {
                BMLogger.Warn($"Invalid RPC: reader is null | Player={playerName} | CallId={callId}", LogTag);
                return false;
            }

            if (reader.Position < 0 || reader.Position > reader.Length)
            {
                BMLogger.Warn(
                    $"Corrupted RPC detected | Player={playerName} | CallId={callId} | Position={reader.Position} | Length={reader.Length}",
                    LogTag);
                return false;
            }

            if (reader.Length > MaxRpcDataSize && !isCustom)
            {
                BMLogger.Error(
                    $"RPC blocked due to excessive size | Player={playerName} | CallId={callId} | Size={reader.Length} | Limit={MaxRpcDataSize}",
                    LogTag);
                return false;
            }

            return true;
        }

        private static bool TryDetectForeignRpc(
    PlayerControl player,
    ClientData client,
    int clientId,
    byte callId,
    string playerName)
        {
            if (Enum.IsDefined(typeof(RpcCalls), callId))
                return false;

            if (Enum.IsDefined(typeof(CustomRPC), callId))
                return false;

            int action = GetConfiguredAction();

            if (KnownCheatRpcSignatures.TryGetValue(callId, out string probableMenu))
            {
                BMLogger.Warn(
                    $"Foreign custom RPC detected | Player={playerName} | RPC={callId} | Probability={probableMenu}",
                    LogTag
                );

                ApplySuspicionAction(
                    player,
                    client,
                    clientId,
                    $"Custom_RPC_{callId}_{probableMenu}",
                    $"{playerName} {GetActionLabel(action)} | " +
                    $"Detected Custom RPC | Probability: {probableMenu} | RPC: {callId}"
                );

                return true;
            }

            BMLogger.Warn(
                $"Unknown foreign custom RPC detected | Player={playerName} | RPC={callId}",
                LogTag
            );

            ApplySuspicionAction(
                player,
                client,
                clientId,
                $"Unknown_Custom_RPC_{callId}",
                $"{playerName} {GetActionLabel(action)} | " +
                $"Detected Custom RPC | Unknown | RPC: {callId}"
            );

            return true;
        }

        private static bool TryHandleAllowedVisualRpc(
            PlayerControl player,
            MessageReader reader,
            int originalReaderPosition,
            int senderClientId)
        {
            try
            {
                reader.Position = originalReaderPosition;
                MessageReader visualReader = MessageReader.Get(reader);

                PlayerMouseController.ReceiveSyncPlayerVisual(visualReader, senderClientId);

                LogToFileOnly(
                    $"Allowed SyncPlayerVisual RPC handled without anti-cheat rate checks | Player={GetPlayerLabel(player)} | ClientId={senderClientId}");
            }
            catch (Exception ex)
            {
                BMLogger.Warn($"Cannot handle allowed SyncPlayerVisual RPC: {ex}", LogTag);
            }
            finally
            {
                try
                {
                    reader.Position = originalReaderPosition;
                }
                catch
                {
                }
            }

            return true;
        }

        private static bool IsChatDangerous(string text, bool allowLongModdedText = false)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (!allowLongModdedText && text.Length > 200)
                return true;

            if (allowLongModdedText && text.Length > 4000)
                return true;

            string lowerText = text.ToLowerInvariant();

            if (lowerText.Contains("<size") ||
                lowerText.Contains("voffset") ||
                lowerText.Contains("<mark") ||
                lowerText.Contains("<material") ||
                lowerText.Contains("<quad"))
            {
                return true;
            }

            int spriteCount = lowerText.Split(new[] { "<sprite" }, StringSplitOptions.None).Length - 1;
            if (spriteCount > 3)
                return true;

            return false;
        }

        private static bool HandleLegacyExploitRpc(
            PlayerControl player,
            ClientData client,
            int cheaterClientId,
            int callId,
            string playerName)
        {
            int action = GetConfiguredAction();

            switch (callId)
            {
                case 101:
                    BMLogger.Warn($"Suspicious legacy RPC 101 detected from {playerName}.", LogTag);
                    ApplySuspicionAction(
                        player, client, cheaterClientId,
                        "Legacy_Mod_Detected_101",
                        $"{playerName} {GetActionLabel(action)} for legacy mod/HostGuard (RPC 101).");
                    return true;

                case 420:
                    BMLogger.Warn($"Suspicious RPC 420 detected from {playerName}.", LogTag);
                    ApplySuspicionAction(
                        player, client, cheaterClientId,
                        "SickoMenu_RPC_420",
                        $"{playerName} {GetActionLabel(action)} for SickoMenu (RPC 420).");
                    return true;

                case 666:
                    BMLogger.Warn($"Suspicious RPC 666 detected from {playerName}.", LogTag);
                    ApplySuspicionAction(
                        player, client, cheaterClientId,
                        "GoatNetClient_RPC_666",
                        $"{playerName} {GetActionLabel(action)} for GoatNetClient (RPC 666).");
                    return true;

                case 42069:
                    BMLogger.Warn($"Suspicious RPC 42069 detected from {playerName}.", LogTag);
                    ApplySuspicionAction(
                        player, client, cheaterClientId,
                        "AmongUsMenu_RPC_42069",
                        $"{playerName} {GetActionLabel(action)} for AmongUsMenu (RPC 42069).");
                    return true;
            }

            return false;
        }

        private static string NormalizeModVersion(string modInfo)
        {
            if (string.IsNullOrWhiteSpace(modInfo))
                return "";

            string clean = modInfo.Trim();

            if (clean.Contains("|"))
            {
                string[] parts = clean.Split('|');
                if (parts.Length >= 2)
                    return parts[1].Trim();
            }

            string[] tokens = clean.Split(' ');
            foreach (string token in tokens)
            {
                string t = token.Trim();
                if (System.Text.RegularExpressions.Regex.IsMatch(t, @"^\d+\.\d+(\.\d+)?"))
                    return t;
            }

            return "";
        }

        private static bool IsSameModVersion(string remoteModInfo)
        {
            string remoteVersion = NormalizeModVersion(remoteModInfo);

            if (string.IsNullOrWhiteSpace(remoteVersion)) return false;

            string[] localParts = CurrentModVersion.Split('.');
            string[] remoteParts = remoteVersion.Split('.');

            int compareLength = Math.Min(3, Math.Min(localParts.Length, remoteParts.Length));

            for (int i = 0; i < compareLength; i++)
            {
                if (localParts[i] != remoteParts[i])
                    return false;
            }

            return true;
        }





        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] int callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (AmongUsClient.Instance == null)
                return true;

            if (BanMod.IsBanModDisabled) return true;

            if (__instance == null)
            {
                BMLogger.Warn("HandleRpc intercepted with null PlayerControl.", LogTag);
                return true;
            }

            if (reader == null)
            {
                BMLogger.Warn($"HandleRpc intercepted with null reader | Player={GetPlayerLabel(__instance)} | CallIdInt={callId}", LogTag);
                return false;
            }

            bool hasByteId = callId >= byte.MinValue && callId <= byte.MaxValue;
            byte callIdByte = hasByteId ? (byte)callId : byte.MaxValue;

            RegisterReceivedRpc(__instance, callId, hasByteId, callIdByte, reader);

            if (PlayerControl.LocalPlayer == __instance)
                return true;

            string name = GetPlayerName(__instance);
            int cheaterClientId = __instance.GetClientId();
            ClientData client = ExtendedPlayerControl.GetClient(__instance);
            int action = GetConfiguredAction();
            float now = Time.realtimeSinceStartup;

            int originalReaderPosition = reader.Position;

            try
            {
                if (HandleLegacyExploitRpc(__instance, client, cheaterClientId, callId, name))
                    return false;

                if (!hasByteId)
                {
                    BMLogger.Warn(
                        $"RPC with CallId out of byte range detected | Player={name} | CallIdInt={callId}",
                        LogTag);

                    ApplySuspicionAction(
                        __instance,
                        client,
                        cheaterClientId,
                        "RPC_CallId_Out_Of_Range",
                        $"{name} {GetActionLabel(action)} for RPC with invalid identifier."
                    );

                    return false;
                }

                bool isCustom = Enum.IsDefined(typeof(CustomRPC), callIdByte);
                bool isStandard = Enum.IsDefined(typeof(RpcCalls), callIdByte);

                if (!IsRpcDataValid(reader, callId, isCustom, name))
                {
                    ApplySuspicionAction(
                        __instance,
                        client,
                        cheaterClientId,
                        "Suspect_Corrupted_RPC",
                        $"{name} {GetActionLabel(action)} for malformed RPC.",
                        addBan: true,
                        addCheater: true,
                        sendWarning: true
                    );

                    return false;
                }

                if (callIdByte == (byte)CustomRPC.SyncPlayerVisual)
                {
                    TryHandleAllowedVisualRpc(__instance, reader, originalReaderPosition, cheaterClientId);
                    return false;
                }

                if (callIdByte == (byte)CustomRPC.FfaSettings)
                {
                    // FfaSettings non viaggia più tramite Among Us RPC.
                    // Blocchiamo il legacy 253: il nuovo percorso è /api/sync/ws.
                    LogToFileOnly(
                        $"Legacy FfaSettings RPC 253 blocked | Player={GetPlayerLabel(__instance)}");
                    return false;
                }

                if (callIdByte == (byte)RpcCalls.LobbyTimeExpiring)
                {
                    if (!AmongUsClient.Instance.AmHost || !IsHostPlayer(__instance))
                    {
                        BMLogger.Warn($"Tentativo di manipolazione timer bloccato da {name}.", "RPCHandler");
                        ApplySuspicionAction(__instance, client, cheaterClientId,
                            "Timer_Manipulation_Attempt",
                            $"{name} {GetActionLabel(action)} per manipolazione timer.");
                        return false;
                    }
                }

                if (!isStandard &&
                    !isCustom)
                {
                    if (TryDetectForeignRpc(
                        __instance,
                        client,
                        cheaterClientId,
                        callIdByte,
                        name))
                    {
                        return false;
                    }
                }

                if (cheaterClientId >= 0 &&
                    RegisterRateHit(
                        GlobalRpcRateByClient,
                        cheaterClientId,
                        now,
                        TotalRpcWindowSeconds,
                        MaxTotalRpcPerWindow,
                        out int totalRpcCount))
                {
                    BMLogger.Warn(
                        $"Total RPC flood detected | Player={name} | ClientId={cheaterClientId} | " +
                        $"Count={totalRpcCount} in {TotalRpcWindowSeconds:0.00}s",
                        LogTag);

                    ApplySuspicionAction(
                        __instance,
                        client,
                        cheaterClientId,
                        "Global_RPC_Flood_Window",
                        $"{name} {GetActionLabel(action)} for massive RPC flood."
                    );

                    return false;
                }

                if (cheaterClientId >= 0)
                {
                    int sameRpcLimit = callIdByte == (byte)RpcCalls.PlayAnimation
                        ? MaxPlayAnimationPerWindow
                        : MaxSameRpcPerWindow;

                    float sameRpcWindow = callIdByte == (byte)RpcCalls.PlayAnimation
                        ? PlayAnimationWindowSeconds
                        : SameRpcWindowSeconds;

                    bool sameRpcFlood = RegisterPerRpcRateHit(
                        cheaterClientId,
                        callId,
                        now,
                        sameRpcWindow,
                        sameRpcLimit,
                        out int sameRpcCount);

                    LogToFileOnly(
                        $"RPC history | Player={GetPlayerLabel(__instance)} | CallIdInt={callId} | " +
                        $"WindowCount={sameRpcCount} | Window={sameRpcWindow:0.00}s"
                    );

                    if (sameRpcFlood)
                    {
                        string reasonCode = callIdByte == (byte)RpcCalls.PlayAnimation
                            ? "RPC_PlayAnimation_Abuse"
                            : "Suspect_Cheater_RPCFlood";

                        string reasonText = callIdByte == (byte)RpcCalls.PlayAnimation
                            ? "PlayAnimation abuse"
                            : $"RPC flood {GetRpcName(callId, true, callIdByte)}";

                        BMLogger.Warn(
                            $"Specific RPC flood detected | Player={name} | CallId={callId} | Count={sameRpcCount}",
                            LogTag);

                        ApplySuspicionAction(
                            __instance,
                            client,
                            cheaterClientId,
                            reasonCode,
                            $"{name} {GetActionLabel(action)} for {reasonText}.",
                            addBan: true,
                            addCheater: true,
                            sendWarning: true
                        );

                        return false;
                    }
                }

                if (callIdByte != (byte)CustomRPC.ModdedAllChat &&
                    callIdByte != (byte)CustomRPC.ProxySendChat &&
                    callIdByte != (byte)CustomRPC.RoleCommandAction &&
                    callIdByte != (byte)CustomRPC.SetSpecialKiller &&
                    callIdByte != (byte)CustomRPC.SetExiler &&
                    callIdByte != (byte)CustomRPC.SetJester &&
                    callIdByte != (byte)CustomRPC.ModeratorAction &&
                    callIdByte != (byte)CustomRPC.SyncPlayerVisual &&
                    callIdByte != (byte)CustomRPC.HostTripleBoolUpdate &&
                    callIdByte != (byte)CustomRPC.HostRoleOptionsUpdate &&
                    callIdByte != (byte)CustomRPC.HandshakeModded &&
                    callIdByte != (byte)CustomRPC.FfaSettings &&
                    callIdByte != (byte)CustomRPC.ModdedHandshake)
                {
                    try
                    {
                        reader.Position = originalReaderPosition;
                        AntiCheat.PlayerControlReceiveRpc(__instance, callIdByte, reader);
                    }
                    finally
                    {
                        reader.Position = originalReaderPosition;
                    }
                }

                MessageReader subReader = MessageReader.Get(reader);

                LogToFileOnly(
                    $"RPC details | Player={GetPlayerLabel(__instance)} | RpcName={GetRpcName(callId, true, callIdByte)}"
                );

                if (AmongUsClient.Instance.AmHost &&
                    callIdByte == (byte)RpcCalls.Exiled &&
                    MeetingHud.Instance == null &&
                    !__instance.AmOwner)
                {
                    BMLogger.Warn($"Blocked Exiled RPC outside meeting from {name}.", LogTag);

                    ApplySuspicionAction(
                        __instance,
                        client,
                        cheaterClientId,
                        "Crash_RPC_Exiled",
                        $"{name} {GetActionLabel(action)} for suspicious Exiled RPC."
                    );

                    return false;
                }

                if (AmongUsClient.Instance.AmHost &&
                    callIdByte == (byte)RpcCalls.VotingComplete &&
                    GameStates.isLobby &&
                    MeetingHud.Instance == null)
                {
                    BMLogger.Warn($"Blocked VotingComplete RPC in lobby from {name}.", LogTag);

                    ApplySuspicionAction(
                        __instance,
                        client,
                        cheaterClientId,
                        "Crash_VotingComplete_Lobby",
                        $"{name} {GetActionLabel(action)} for suspicious VotingComplete in lobby."
                    );

                    return false;
                }

                if (isCustom)
                {
                    var customType = (CustomRPC)callIdByte;
                    LogToFileOnly($"Handling CustomRPC | Player={GetPlayerLabel(__instance)} | Type={customType}");

                    switch (customType)
                    {
                        case CustomRPC.HandshakeModded:
                            {
                                RegisterModdedClient(__instance.PlayerId, "BanMod");

                                LogToFileOnly($"HandshakeModded registered for {GetPlayerLabel(__instance)}.");
                                return false;
                            }

                        case CustomRPC.ModdedHandshake:
                            {
                                string modInfo = subReader.ReadString();
                                bool isRemoteModded = subReader.ReadBoolean();
                                byte playerId = __instance.PlayerId;

                                string cleanModInfo = string.IsNullOrWhiteSpace(modInfo) ? "BanMod" : modInfo;

                                if (isRemoteModded)
                                {
                                    RegisterModdedClient(playerId, cleanModInfo);
                                }

                                LogToFileOnly(
                                    $"ModdedHandshake received | Player={GetPlayerLabel(__instance)} | ModInfo={CleanLogText(cleanModInfo)} | FlagModded={isRemoteModded}"
                                );

                                if (isRemoteModded &&
                                    AlreadyNotifiedModdedClients.Add(playerId) &&
                                    HudManager.Instance?.Notifier != null)
                                {
                                    HudManager.Instance.Notifier.AddDisconnectMessage(
                                        $"Modded client connected: {GetPlayerName(__instance)} | Mod: {cleanModInfo}"
                                    );
                                }

                                return false;
                            }

                        case CustomRPC.HostTripleBoolUpdate:
                            {
                                PlayerControl sender = __instance;

                                if (!IsHostPlayer(sender))
                                {
                                    LogToFileOnly(
                                        $"HostTripleBoolUpdate BLOCKED: sender not host | Sender={GetPlayerLabel(sender)} | SenderPlayerId={sender?.PlayerId} | SenderClientId={sender?.GetClientId()} | HostId={AmongUsClient.Instance?.HostId}");

                                    NotifyUnauthorizedHostRpc(sender, "HostTripleBoolUpdate");

                                    return false;
                                }
                                bool remoteBool1ImmortalAdded = subReader.ReadBoolean();
                                bool remoteBool2ImmortalEnabled = subReader.ReadBoolean();
                                bool remoteBool3EngineerEnabled = subReader.ReadBoolean();

                                HostOptionStatus.UpdateHostRules(
                                    remoteBool1ImmortalAdded,
                                    remoteBool2ImmortalEnabled,
                                    remoteBool3EngineerEnabled
                                );

                                LogToFileOnly(
                                    $"HostTripleBoolUpdate received | Added={remoteBool1ImmortalAdded} | Enabled={remoteBool2ImmortalEnabled} | Engineer={remoteBool3EngineerEnabled}"
                                );

                                return false;
                            }

                        case CustomRPC.ProxySendChat:
                            {
                                PlayerControl sender = __instance;

                                if (!IsHostPlayer(sender))
                                {
                                    LogToFileOnly(
                                        $"ProxySendChat BLOCKED: sender not host | Sender={GetPlayerLabel(sender)} | SenderPlayerId={sender?.PlayerId} | SenderClientId={sender?.GetClientId()} | HostId={AmongUsClient.Instance?.HostId}");

                                    NotifyUnauthorizedHostRpc(sender, "ProxySendChat");

                                    return false;
                                }
                                string message = subReader.ReadString();
                                string ignoredTitle = subReader.ReadString();
                                byte target = subReader.ReadByte();

                                RegisterReceivedMessage(__instance, "ProxySendChat", message);

                                LogToFileOnly(
                                    $"ProxySendChat received | IgnoredTitle={CleanLogText(ignoredTitle)} | TargetPlayerId={target}");

                                PlayerControl localPlayer = PlayerControl.LocalPlayer;

                                if (localPlayer == null || localPlayer.Data == null)
                                    return false;

                                if (localPlayer.Data.IsDead || localPlayer.Data.Disconnected)
                                    return false;

                                if (string.IsNullOrWhiteSpace(message))
                                    return false;

                                message = message
                                    .RemoveHtmlTags()
                                    .Replace("\\r\\n", "\n")
                                    .Replace("\\n", "\n")
                                    .Replace("\\r", "\n")
                                    .Replace("\r\n", "\n")
                                    .Replace("\r", "\n")
                                    .Trim();

                                if (string.IsNullOrWhiteSpace(message))
                                    return false;

                                if (message.Length > 120)
                                    message = message.Substring(0, 120);

                                bool sendToAll = target == byte.MaxValue || target == 255;
                                bool targetIsProxySelf = target == localPlayer.PlayerId;

                                int targetClientId = -1;

                                if (!sendToAll && !targetIsProxySelf)
                                {
                                    PlayerControl targetPlayer = Utils.GetPlayerById(target);

                                    if (targetPlayer == null || targetPlayer.Data == null || targetPlayer.Data.Disconnected)
                                        return false;

                                    targetClientId = targetPlayer.GetClientId();

                                    if (targetClientId < 0)
                                        return false;
                                }

                                if (targetIsProxySelf)
                                {
                                    try
                                    {
                                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(localPlayer, message, false);
                                        MessageBlocker.UpdateLastMessageTime();

                                        LogToFileOnly(
                                            $"ProxySendChat shown locally to the proxy itself | Proxy={GetPlayerLabel(localPlayer)} | TargetPlayerId={target} | Text={CleanLogText(message)}");
                                    }
                                    catch (Exception ex)
                                    {
                                        BMLogger.Warn($"ProxySendChat local AddChat failed: {ex}", LogTag);
                                    }

                                    return false;
                                }

                                if (!MessageBlocker.CanSendMessage())
                                {
                                    ProxyMessageQueue.Enqueue(message, targetClientId, sendToAll);

                                    LogToFileOnly(
                                        $"ProxySendChat queued for cooldown | TargetPlayerId={target} | TargetClientId={targetClientId} | SendToAll={sendToAll} | Text={CleanLogText(message)}");

                                    return false;
                                }

                                try
                                {
                                    var writer = CustomRpcSender.Create("ProxySendChatDirect", SendOption.Reliable);

                                    writer.StartMessage(targetClientId);
                                    writer.StartRpc(localPlayer.NetId, (byte)RpcCalls.SendChat)
                                        .Write(message)
                                        .EndRpc();
                                    writer.EndMessage();
                                    writer.SendMessage();

                                    if (sendToAll)
                                    {
                                        try
                                        {
                                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(localPlayer, message, false);
                                        }
                                        catch (Exception ex)
                                        {
                                            BMLogger.Warn($"ProxySendChat local broadcast AddChat failed: {ex}", LogTag);
                                        }
                                    }

                                    MessageBlocker.UpdateLastMessageTime();

                                    LogToFileOnly(
                                        $"ProxySendChat sent | Proxy={GetPlayerLabel(localPlayer)} | TargetPlayerId={target} | TargetClientId={targetClientId} | SendToAll={sendToAll} | Text={CleanLogText(message)}");
                                }
                                catch (Exception ex)
                                {
                                    BMLogger.Warn($"ProxySendChat send failed: {ex}", LogTag);
                                }

                                return false;
                            }
                        case CustomRPC.ModeratorAction:
                            {
                                ModeratorAuthority.Receive(__instance, subReader);
                                return false;
                            }

                        case CustomRPC.SyncPlayerVisual:
                            {
                                PlayerMouseController.ReceiveSyncPlayerVisual(subReader, cheaterClientId);
                                return false;
                            }
                        case CustomRPC.ModdedAllChat:
                            {
                                byte senderId = subReader.ReadByte();
                                string text = subReader.ReadString();

                                RegisterReceivedMessage(__instance, "ModdedAllChat", text);

                                if (IsChatDangerous(text, true))
                                {
                                    BMLogger.Warn(
                                        $"Dangerous ModdedAllChat message blocked from {name}: {CleanLogText(text)}",
                                        LogTag);

                                    return false;
                                }

                                if (senderId != __instance.PlayerId)
                                {
                                    BMLogger.Warn(
                                        $"ModdedAllChat senderId mismatch | RPC owner={__instance.PlayerId} | senderId={senderId}",
                                        LogTag);

                                    return false;
                                }

                                ModdedOriginalChatManager.ReceiveAll(senderId, text);

                                try
                                {
                                    if (HudManager.Instance != null &&
                                        HudManager.Instance.Chat != null &&
                                        !HudManager.Instance.Chat.IsOpenOrOpening)
                                    {
                                        ChatControllerUpdatePatch.TryFlashChatNotifyDot();
                                    }
                                }
                                catch
                                {
                                }

                                return false;
                            }
                        case CustomRPC.HostRoleOptionsUpdate:
                            {
                                PlayerControl sender = __instance;

                                if (!IsHostPlayer(sender))
                                {
                                    LogToFileOnly(
                                        $"HostRoleOptionsUpdate BLOCKED: sender not host | Sender={GetPlayerLabel(sender)} | SenderPlayerId={sender?.PlayerId} | SenderClientId={sender?.GetClientId()} | HostId={AmongUsClient.Instance?.HostId}");

                                    NotifyUnauthorizedHostRpc(sender, "HostRoleOptionsUpdate");

                                    return false;
                                }
                                HostRoleOptionsRpc.Receive(__instance, subReader);
                                return false;
                            }

                        case CustomRPC.RoleCommandAction:
                            {
                                RoleCommandActionRpc.Receive(__instance, subReader);
                                return false;
                            }

                        case CustomRPC.SetSpecialKiller:
                            {
                                PlayerControl sender = __instance;

                                if (!IsHostPlayer(sender))
                                {
                                    LogToFileOnly(
                                        $"SetSpecialKiller BLOCKED: sender not host | Sender={GetPlayerLabel(sender)} | SenderPlayerId={sender?.PlayerId} | SenderClientId={sender?.GetClientId()} | HostId={AmongUsClient.Instance?.HostId}");

                                    NotifyUnauthorizedHostRpc(sender, "SetSpecialKiller");

                                    return false;
                                }
                                byte killerId = subReader.ReadByte();

                                Guesser.SpecialKillerId = killerId;
                                Guesser.SpecialKillerSelected = killerId != byte.MaxValue && killerId != 255;

                                LogToFileOnly($"SetSpecialKiller received | KillerId={killerId}");

                                try
                                {
                                    RoleButtonRefresh.RefreshNow();
                                }
                                catch
                                {
                                }

                                return false;
                            }

                        case CustomRPC.SetJester:
                            {
                                PlayerControl sender = __instance;

                                if (!IsHostPlayer(sender))
                                {
                                    LogToFileOnly(
                                        $"SetJester BLOCKED: sender not host | Sender={GetPlayerLabel(sender)} | SenderPlayerId={sender?.PlayerId} | SenderClientId={sender?.GetClientId()} | HostId={AmongUsClient.Instance?.HostId}");

                                    NotifyUnauthorizedHostRpc(sender, "SetJester");

                                    return false;
                                }
                                byte jesterId = subReader.ReadByte();

                                Jester.JesterId = jesterId;
                                Jester.JesterSelected = jesterId != byte.MaxValue && jesterId != 255;

                                try
                                {
                                    Jester.ForcedJesterId = jesterId;
                                    Jester.ForcedJesterSelected = jesterId != byte.MaxValue && jesterId != 255;
                                }
                                catch
                                {
                                }

                                LogToFileOnly($"SetJester received | JesterId={jesterId}");

                                try
                                {
                                    RoleButtonRefresh.RefreshNow();
                                }
                                catch
                                {
                                }

                                return false;
                            }

                        case CustomRPC.SetExiler:
                            {
                                PlayerControl sender = __instance;

                                if (!IsHostPlayer(sender))
                                {
                                    LogToFileOnly(
                                        $"SetExiler BLOCKED: sender not host | Sender={GetPlayerLabel(sender)} | SenderPlayerId={sender?.PlayerId} | SenderClientId={sender?.GetClientId()} | HostId={AmongUsClient.Instance?.HostId}");

                                    NotifyUnauthorizedHostRpc(sender, "SetExiler");

                                    return false;
                                }
                                byte exilerId = subReader.ReadByte();

                                Exiler.ExilerId = exilerId;
                                Exiler.ExilerSelected = exilerId != byte.MaxValue && exilerId != 255;

                                LogToFileOnly($"SetExiler received | ExilerId={exilerId}");

                                try
                                {
                                    RoleButtonRefresh.RefreshNow();
                                }
                                catch
                                {
                                }

                                return false;
                            }

                    }

                    return false;
                }

                var rpcType = (RpcCalls)callIdByte;
                LogToFileOnly($"Handling standard RpcCalls | Player={GetPlayerLabel(__instance)} | Type={rpcType}");

                switch (rpcType)
                {
                    case RpcCalls.SendChat:
                        {
                            string text = subReader.ReadString();
                            if (text.Contains("hacked by") || text.Contains("discord.gg") || text.Contains("bombsaddicts"))
                            {
                                ApplySuspicionAction(__instance, client, cheaterClientId,
                                    "Chat_Spam_Hack",
                                    $"{name} {GetActionLabel(action)} per spam in chat.");
                                return false;
                            }
                            RegisterReceivedMessage(__instance, "SendChat", text);

                            if (cheaterClientId >= 0 &&
                                RegisterRateHit(
                                    ChatRateByClient,
                                    cheaterClientId,
                                    now,
                                    ChatWindowSeconds,
                                    MaxChatMessagesPerWindow,
                                    out int chatCount))
                            {
                                BMLogger.Warn(
                                    $"Chat flood detected | Player={name} | ClientId={cheaterClientId} | Count={chatCount} in {ChatWindowSeconds:0.0}s",
                                    LogTag);

                                ApplySuspicionAction(
                                    __instance,
                                    client,
                                    cheaterClientId,
                                    "Chat_Flood",
                                    $"{name} {GetActionLabel(action)} for chat flood."
                                );

                                return false;
                            }

                            if (IsChatDangerous(text))
                            {
                                BMLogger.Warn($"Dangerous chat message blocked from {name}: {CleanLogText(text)}", LogTag);

                                ApplySuspicionAction(
                                    __instance,
                                    client,
                                    cheaterClientId,
                                    "Crash_Chat_Formatting",
                                    $"{name} {GetActionLabel(action)} for dangerous chat message."
                                );

                                return false;
                            }

                            ChatCommands.OnReceiveChat(__instance, text, out var canceled);
                            if (canceled)
                            {
                                LogToFileOnly($"Chat message cancelled by commands | Player={GetPlayerLabel(__instance)}");
                                return false;
                            }

                            break;
                        }
                    case RpcCalls.SendQuickChat:
                        {
                            if (cheaterClientId >= 0 &&
                                RegisterRateHit(
                                    ChatRateByClient,
                                    cheaterClientId,
                                    now,
                                    ChatWindowSeconds,
                                    MaxChatMessagesPerWindow,
                                    out int quickChatCount))
                            {
                                BMLogger.Warn($"QuickChat flood rilevato da {name}", LogTag);
                                ApplySuspicionAction(__instance, client, cheaterClientId,
                                    "QuickChat_Flood",
                                    $"{name} {GetActionLabel(action)} per flood QuickChat.");
                                return false;
                            }

                            RegisterReceivedMessage(__instance, "SendQuickChat", "Quick chat");
                            ChatCommands.OnReceiveChat(__instance, "Quick chat", out var canceledQuick);

                            if (canceledQuick)
                            {
                                LogToFileOnly($"Quick chat cancelled by commands | Player={GetPlayerLabel(__instance)}");
                                return false;
                            }

                            break;
                        }

                    default:
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                BMLogger.Error(
                    $"Error during RPC handling | Player={name} | CallId={callId} | Error={ex}",
                    LogTag);

                ApplySuspicionAction(
                    __instance,
                    client,
                    cheaterClientId,
                    "RPC_Exception",
                    $"{name} {GetActionLabel(action)} for malformed RPC or crash attempt."
                );

                return false;
            }
            finally
            {
                try
                {
                    reader.Position = originalReaderPosition;
                }
                catch (Exception ex)
                {
                    BMLogger.Warn($"Cannot restore reader position: {ex.Message}", LogTag);
                }
            }
        }

        public static void RegisterModdedClient(byte playerId, string modInfo = "BanMod")
        {
            try
            {
                if (!ModdedRegistry.ModdedPlayers.Contains(playerId))
                    ModdedRegistry.ModdedPlayers.Add(playerId);
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(modInfo))
                modInfo = "BanMod";

            ModdedClients[playerId] = modInfo;
        }

        public static void RegisterLocalAsModded()
        {
            try
            {
                if (PlayerControl.LocalPlayer == null)
                    return;

                string modInfo = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost
                    ? "BanMod Host"
                    : "BanMod";

                RegisterModdedClient(PlayerControl.LocalPlayer.PlayerId, modInfo);
            }
            catch (Exception)
            {

            }
        }

        public static void SendLocalModdedHandshake()
        {
            try
            {
                if (PlayerControl.LocalPlayer != null)
                    RegisterLocalAsModded();
                BanModSyncApi.SendModdedHandshake();
            }
            catch
            {
            }
        }

        public static bool IsClientModded(byte playerId)
        {
            if (ModdedClients.ContainsKey(playerId))
                return true;

            try
            {
                if (ModdedRegistry.ModdedPlayers.Contains(playerId))
                    return true;
            }
            catch
            {
            }

            return false;
        }

        public static List<(PlayerControl player, string modInfo)> GetModdedPlayersWithInfo()
        {
            return PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => p != null && IsClientModded(p.PlayerId))
                .Select(p =>
                {
                    string modInfo = "BanMod";

                    if (ModdedClients.TryGetValue(p.PlayerId, out string savedInfo) &&
                        !string.IsNullOrWhiteSpace(savedInfo))
                    {
                        modInfo = savedInfo;
                    }

                    return (p, modInfo);
                })
                .ToList();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    internal static class PlayerControl_Start_ModdedHandshakePatch
    {
        private static bool SentHandshake;

        public static void Postfix(PlayerControl __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                if (PlayerControl.LocalPlayer == null)
                    return;

                if (__instance != PlayerControl.LocalPlayer)
                    return;

                if (SentHandshake)
                    return;

                SentHandshake = true;

                ModdedHandshakeAutoSender.ResetHandshakeBurst();
                ModdedHandshakeAutoSender.TrySendHandshakeNow(true);
            }
            catch (Exception)
            {

            }
        }

        public static void Reset()
        {
            SentHandshake = false;
        }
    }

    internal static class ModdedHandshakeAutoSender
    {
        private const float HandshakeIntervalSeconds = 3f;
        private const int MaxHandshakeBurstSends = 8;

        private static float _nextHandshakeTime = 0f;
        private static int _handshakeSends = 0;

        public static void ResetHandshakeBurst()
        {
            _nextHandshakeTime = 0f;
            _handshakeSends = 0;
        }

        public static void TrySendHandshakeNow(bool force = false)
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return;

                if (PlayerControl.LocalPlayer == null)
                    return;

                if (!force)
                {
                    if (_handshakeSends >= MaxHandshakeBurstSends)
                        return;

                    if (Time.realtimeSinceStartup < _nextHandshakeTime)
                        return;
                }

                _nextHandshakeTime = Time.realtimeSinceStartup + HandshakeIntervalSeconds;
                _handshakeSends++;

                UnifiedRPCHandlerPatch.SendLocalModdedHandshake();
            }
            catch (Exception ex)
            {
                BMLogger.Warn($"[Handshake] TrySendHandshakeNow failed: {ex}", "RPCHandler");
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    internal static class AmongUsClient_Update_ModdedHandshakePatch
    {
        public static void Postfix()
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return;

                if (PlayerControl.LocalPlayer == null)
                    return;

                if (!GameStates.isOnlineGame)
                    return;

                ModdedHandshakeAutoSender.TrySendHandshakeNow(false);
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    internal static class AmongUsClient_OnGameJoined_ModdedHandshakePatch
    {
        public static void Postfix()
        {
            try
            {
                UnifiedRPCHandlerPatch.ModdedClients.Clear();

                try
                {
                    ModdedRegistry.ModdedPlayers.Clear();
                    UnifiedRPCHandlerPatch.ResetModdedNotifications();
                    HostRoleOptionsStatus.Reset();
                }
                catch
                {
                }

                PlayerControl_Start_ModdedHandshakePatch.Reset();

                UnifiedRPCHandlerPatch.RegisterLocalAsModded();

                ModdedHandshakeAutoSender.ResetHandshakeBurst();
                ModdedHandshakeAutoSender.TrySendHandshakeNow(true);
            }
            catch (Exception)
            {

            }
        }
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    internal class ExiledLobbyGuardPatch
    {
        static bool Prefix(PlayerControl __instance)
        {
            if (GameStates.isLobby)
            {
                BMLogger.Error("Exiled() called in lobby. Security block applied.", "RPCHandler");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    internal static class VoteKickHudNotificationPatch
    {
        public static void Prefix(int srcClient, int clientId)
        {
            try
            {
                if (AmongUsClient.Instance == null ||
                    PlayerControl.LocalPlayer == null ||
                    clientId != PlayerControl.LocalPlayer.OwnerId)
                {
                    return;
                }

                ClientData voter = AmongUsClient.Instance.FindClientById(srcClient);
                string voterName = voter?.PlayerName ?? $"Client {srcClient}";

                AntiCheat.ShowHudNotification(
                    $"Votekick: {voterName} ha votato per espellerti."
                );
            }
            catch (Exception ex)
            {
                BMLogger.Warn(
                    $"Unable to show votekick notification: {ex.Message}",
                    "RPCHandler"
                );
            }
        }
    }
}
