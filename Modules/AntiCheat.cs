//credits and licenses in the resources folder
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using BanMod;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using InnerNet;
using Internal.Threading.Tasks.Tracing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;
using static BanMod.AntiCheat;
using static BanMod.Translator;
using static BanMod.Utils;

namespace BanMod
{
    public class AntiCheat
    {
        private const string LogTag = "AntiCheat";

        private static readonly Dictionary<byte, int> taskRpcCount = new();
        private static readonly Dictionary<byte, DateTime> taskRpcLastTime = new();
        private static readonly Queue<string> pendingHudNotifications = new();

        private static string GetPlayerName(PlayerControl player)
            => player?.Data?.PlayerName ?? "Unknown";

        private static string GetPlayerLabel(PlayerControl player)
            => player == null ? "null-player" : $"{GetPlayerName(player)} [pid={player.PlayerId}]";

        private static void LogDebug(string text) => BMLogger.LogDebug(text, LogTag);
        private static void LogInfo(string text) => BMLogger.Info(text, LogTag);
        private static void LogWarn(string text) => BMLogger.Warn(text, LogTag);
        private static void LogError(string text) => BMLogger.Error(text, LogTag);

        internal static bool IsHostReady()
        {
            try
            {
                AmongUsClient client = AmongUsClient.Instance;
                return client != null && client.AmHost;
            }
            catch
            {
                return false;
            }
        }

        internal static bool IsGameplayReadyForPlayerScan()
        {
            try
            {
                AmongUsClient client = AmongUsClient.Instance;
                return client != null &&
                       client.AmHost &&
                       client.GameState == InnerNetClient.GameStates.Started &&
                       ShipStatus.Instance != null &&
                       PlayerControl.LocalPlayer != null;
            }
            catch
            {
                return false;
            }
        }

        public static void ShowHudNotification(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                if (HudManager.Instance?.Notifier != null)
                {
                    NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                        HudManager.Instance.Notifier,
                        message
                    );

                    return;
                }
            }
            catch (Exception ex)
            {
                LogWarn($"Unable to show HUD notification immediately: {ex.Message}");
            }

            pendingHudNotifications.Enqueue(message);
            LogWarn($"Notifier not available. HUD notification queued: {message}");
        }

        public static void FlushHudNotifications()
        {
            if (HudManager.Instance?.Notifier == null)
                return;

            while (pendingHudNotifications.Count > 0)
            {
                string message = pendingHudNotifications.Dequeue();

                try
                {
                    NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                        HudManager.Instance.Notifier,
                        message
                    );
                }
                catch (Exception ex)
                {
                    pendingHudNotifications.Enqueue(message);
                    LogWarn($"Unable to flush HUD notification: {ex.Message}");
                    break;
                }
            }
        }

        public static void ResetMatchState()
        {
            taskRpcCount.Clear();
            taskRpcLastTime.Clear();
            VentAntiCheat.warnedPlayersVent.Clear();
            VentStateMonitor.Reset();
            BanMod.hasSentHackWarning = false;
            LogInfo("AntiCheat state reset at match end.");
        }

        public static void GestisciRilevamentoCheat(
            PlayerControl player,
            string uiMessage,
            string reason,
            SystemTypes? systemType = null,
            byte? amount = null,
            bool addCheater = true,
            bool applyPunishment = true)
        {
            string details = $"Reason={reason} | Player={GetPlayerLabel(player)}";
            if (systemType.HasValue) details += $" | System={systemType.Value}";
            if (amount.HasValue) details += $" | Amount={amount.Value}";

            LogWarn($"Cheat detected. {details}");

            ShowHudNotification(uiMessage);
            SendGlobalHackWarning();

            ClientData client = ExtendedPlayerControl.GetClient(player);
            if (client == null)
            {
                LogWarn($"Null ClientData, cannot register or apply punishments. {details}");
                return;
            }

            if (addCheater)
            {
                CheaterManager.AddPlayer(client, reason);
                LogInfo($"Player added to cheater list: {GetPlayerLabel(player)} | ClientId={client.Id}");
            }

            if (!applyPunishment)
            {
                LogDebug($"Automatic punishment not applied for {GetPlayerLabel(player)}.");
                return;
            }

            int action = Options.ActionCheater.GetValue();

            if (action == 0)
            {
                LogDebug($"Punishment set to 0 (Warning Only) for {GetPlayerLabel(player)}. No kick or ban executed.");
            }
            else if (action == 1)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, false);
                LogWarn($"Kick executed for {GetPlayerLabel(player)} | ClientId={client.Id}");
            }
            else if (action == 2)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                LogWarn($"Ban executed for {GetPlayerLabel(player)} | ClientId={client.Id}");
            }
            else
            {
                LogDebug($"No automatic punishment configured for {GetPlayerLabel(player)}. Action={action}");
            }
        }

        public static class VentAntiCheat
        {
            private static readonly Dictionary<byte, int> ventRemovalsThisTick = new();
            private static float lastCheckTime = 0;

            public static readonly HashSet<byte> warnedPlayersVent = new();

            public static void RegisterVentBoot(PlayerControl player)
            {
                if (player == null || !AntiCheat.IsHostReady()) return;
                if (player == PlayerControl.LocalPlayer) return;
                if (Time.time - lastCheckTime > Time.fixedDeltaTime)
                {
                    ventRemovalsThisTick.Clear();
                    lastCheckTime = Time.time;
                    BMLogger.LogDebug("New vent check window started.", LogTag);
                }

                byte pid = player.PlayerId;
                if (!ventRemovalsThisTick.ContainsKey(pid))
                    ventRemovalsThisTick[pid] = 0;

                ventRemovalsThisTick[pid]++;
                BMLogger.LogDebug($"Vent boot registered for {GetPlayerLabel(player)} | TickCount={ventRemovalsThisTick[pid]}", LogTag);

                if (ventRemovalsThisTick[pid] > 3)
                {
                    if (!warnedPlayersVent.Contains(pid) && Options.KickVentCheat.GetBool() && Options.EnableAntiCheat.GetBool())
                    {
                        if (!AntiCheat.IsHostReady()) return;

                        warnedPlayersVent.Add(pid);
                        string msg = $"{GetPlayerName(player)}{GetAuto("VentCheat")}";

                        GestisciRilevamentoCheat(
                            player,
                            msg,
                            "Detected too many vent boots in the same tick",
                            SystemTypes.Ventilation,
                            null,
                            addCheater: true,
                            applyPunishment: false
                        );
                    }
                }
            }
        }

        public static class VentStateMonitor
        {
            private const float ScanInterval = 0.25f;
            private const float StateGraceSeconds = 1.75f;
            private const float PhysicalTransitionGraceSeconds = 1.25f;
            private const float HostVentActionGraceSeconds = 2.5f;
            private const float MovementThresholdSquared = 0.5625f;
            private const float AlertCooldownSeconds = 8f;

            private static readonly Dictionary<byte, int> ExpectedVentByPlayer = new();
            private static readonly Dictionary<byte, float> LastOperationTime = new();
            private static readonly Dictionary<byte, Vector2> VentAnchorPosition = new();
            private static readonly Dictionary<byte, bool> LastInVentState = new();
            private static readonly Dictionary<byte, float> LastInVentStateChangedAt = new();
            private static readonly Dictionary<string, float> ConditionStartedAt = new();
            private static readonly Dictionary<string, float> LastAlertAt = new();

            private static float nextScanTime;
            private static float hostVentActionGraceUntil;
            private static bool meetingWasActive;

            private static bool VentExists(int ventId)
            {
                if (ShipStatus.Instance?.AllVents == null)
                    return false;

                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    if (vent != null && vent.Id == ventId)
                        return true;
                }

                return false;
            }

            public static void Reset()
            {
                ExpectedVentByPlayer.Clear();
                LastOperationTime.Clear();
                VentAnchorPosition.Clear();
                LastInVentState.Clear();
                LastInVentStateChangedAt.Clear();
                ConditionStartedAt.Clear();
                LastAlertAt.Clear();
                nextScanTime = 0f;
                hostVentActionGraceUntil = 0f;
                meetingWasActive = false;
            }

            private static void ResetTransientVentState(float now)
            {
                ExpectedVentByPlayer.Clear();
                LastOperationTime.Clear();
                VentAnchorPosition.Clear();
                LastInVentState.Clear();
                LastInVentStateChangedAt.Clear();
                ConditionStartedAt.Clear();
                nextScanTime = 0f;
                hostVentActionGraceUntil = now + HostVentActionGraceSeconds;
            }

            public static void RegisterOperation(
                PlayerControl actor,
                VentilationSystem.Operation operation,
                int ventId)
            {
                if (actor == null)
                    return;

                float now = Time.realtimeSinceStartup;

                if (actor == PlayerControl.LocalPlayer)
                {
                    if (operation == VentilationSystem.Operation.StartCleaning ||
                        operation == VentilationSystem.Operation.BootImpostors)
                    {
                        foreach (byte playerId in new List<byte>(ExpectedVentByPlayer.Keys))
                        {
                            ExpectedVentByPlayer.Remove(playerId);
                            LastOperationTime[playerId] = now;
                            VentAnchorPosition.Remove(playerId);
                        }

                        hostVentActionGraceUntil = now + HostVentActionGraceSeconds;
                    }

                    return;
                }

                byte actorId = actor.PlayerId;
                LastOperationTime[actorId] = now;

                if (operation == VentilationSystem.Operation.Enter &&
                    !actor.inVent &&
                    !VentExists(ventId))
                {
                    Report(
                        actor,
                        "Vent_Immortality_Attempt",
                        $"ha dichiarato Enter nella vent inesistente {ventId} senza entrarci",
                        ventId >= byte.MinValue && ventId <= byte.MaxValue
                            ? (byte?)ventId
                            : null
                    );
                }

                switch (operation)
                {
                    case VentilationSystem.Operation.Enter:
                    case VentilationSystem.Operation.Move:
                        ExpectedVentByPlayer[actorId] = ventId;
                        break;

                    case VentilationSystem.Operation.Exit:
                        ExpectedVentByPlayer.Remove(actorId);
                        VentAnchorPosition.Remove(actorId);
                        break;

                    case VentilationSystem.Operation.StartCleaning:
                    case VentilationSystem.Operation.BootImpostors:
                        foreach (KeyValuePair<byte, int> entry in
                                 ExpectedVentByPlayer.ToArray())
                        {
                            if (entry.Value != ventId)
                                continue;

                            ExpectedVentByPlayer.Remove(entry.Key);
                            LastOperationTime[entry.Key] = now;
                            VentAnchorPosition.Remove(entry.Key);
                        }
                        break;
                }
            }

            public static void Scan()
            {
                if (!AntiCheat.IsGameplayReadyForPlayerScan() ||
                    !Options.EnableAntiCheat.GetBool() ||
                    !Options.KickVentCheat.GetBool())
                {
                    return;
                }

                try
                {
                    float now = Time.realtimeSinceStartup;
                    bool meetingActive = MeetingHud.Instance != null;

                    if (meetingActive)
                    {
                        if (!meetingWasActive)
                        {
                            ResetTransientVentState(now);
                            meetingWasActive = true;
                            LogDebug("Vent state cleared at meeting start.");
                        }

                        return;
                    }

                    if (meetingWasActive)
                    {
                        ResetTransientVentState(now);
                        meetingWasActive = false;
                        LogDebug("Vent state cleared at meeting end.");
                    }

                    if (now < nextScanTime)
                        return;

                    nextScanTime = now + ScanInterval;

                    var players = PlayerControl.AllPlayerControls;
                    if (players == null)
                        return;

                    PlayerControl local = PlayerControl.LocalPlayer;

                    foreach (PlayerControl player in players)
                    {
                        try
                        {
                            if (player == null ||
                                local == null ||
                                player == local ||
                                player.Data == null ||
                                player.Data.Disconnected ||
                                player.Data.IsDead)
                            {
                                continue;
                            }

                            byte playerId = player.PlayerId;

                            // transform can become invalid while Unity unloads a scene.
                            if (player.transform == null)
                                continue;

                            Vector2 position = player.transform.position;
                            bool inVent = player.inVent;

                            if (!LastInVentState.TryGetValue(
                                    playerId,
                                    out bool previousInVent))
                            {
                                LastInVentState[playerId] = inVent;
                                LastInVentStateChangedAt[playerId] = now;

                                if (inVent)
                                    VentAnchorPosition[playerId] = position;

                                continue;
                            }

                            if (previousInVent != inVent)
                            {
                                LastInVentState[playerId] = inVent;
                                LastInVentStateChangedAt[playerId] = now;

                                if (inVent)
                                    VentAnchorPosition[playerId] = position;
                                else
                                    VentAnchorPosition.Remove(playerId);
                            }

                            bool transitionGrace =
                                LastInVentStateChangedAt.TryGetValue(
                                    playerId,
                                    out float changedAt) &&
                                now - changedAt < PhysicalTransitionGraceSeconds;

                            bool operationGrace =
                                LastOperationTime.TryGetValue(
                                    playerId,
                                    out float operationAt) &&
                                now - operationAt < StateGraceSeconds;

                            bool suppressPhysicalChecks =
                                transitionGrace ||
                                operationGrace ||
                                now < hostVentActionGraceUntil;

                            bool expectedInside =
                                ExpectedVentByPlayer.TryGetValue(
                                    playerId,
                                    out int expectedVentId);

                            bool fakeInsideState =
                                expectedInside && !inVent;

                            bool fakeInsidePersisted = Persisted(
                                $"tracked-not-actual:{playerId}",
                                fakeInsideState,
                                now
                            );

                            if (!suppressPhysicalChecks && fakeInsidePersisted)
                            {
                                Report(
                                    player,
                                    "Vent_State_Tracked_Not_Actual",
                                    $"risulta nella vent {expectedVentId}, ma non è realmente entrato",
                                    expectedVentId >= byte.MinValue &&
                                    expectedVentId <= byte.MaxValue
                                        ? (byte?)expectedVentId
                                        : null
                                );
                            }
                        }
                        catch
                        {
                            // Destroyed IL2CPP PlayerControl during scene transition.
                            // Skip only this player and continue scanning.
                        }
                    }
                }
                catch
                {
                    // Fail-open while scene singletons/lists are being rebuilt.
                }
            }

            private static bool Persisted(string key, bool condition, float now)
            {
                if (!condition)
                {
                    ConditionStartedAt.Remove(key);
                    return false;
                }

                if (!ConditionStartedAt.TryGetValue(key, out float startedAt))
                {
                    ConditionStartedAt[key] = now;
                    return false;
                }

                return now - startedAt >= StateGraceSeconds;
            }

            private static void Report(
                PlayerControl player,
                string reasonCode,
                string details,
                byte? ventId = null)
            {
                if (player == null ||
                    player == PlayerControl.LocalPlayer ||
                    !Options.EnableAntiCheat.GetBool() ||
                    !Options.KickVentCheat.GetBool())
                {
                    return;
                }

                float now = Time.realtimeSinceStartup;
                string alertKey = $"{player.PlayerId}:{reasonCode}";

                if (LastAlertAt.TryGetValue(alertKey, out float lastAlert) &&
                    now - lastAlert < AlertCooldownSeconds)
                {
                    return;
                }

                LastAlertAt[alertKey] = now;

                string name = GetPlayerName(player);

                GestisciRilevamentoCheat(
                    player,
                    $"{name}: anomalia vent rilevata ({details})",
                    $"{reasonCode}: {details}",
                    SystemTypes.Ventilation,
                    ventId,
                    addCheater: false,
                    applyPunishment: false
                );
            }
        }

        public static void SendGlobalHackWarning(ulong friendCode = 0)
        {
            if (!AntiCheat.IsHostReady())
            {
                LogDebug("Global warning not sent: client is not host.");
                return;
            }

            if (BanMod.hasSentHackWarning)
            {
                LogDebug("Global warning not sent: already sent in this match.");
                return;
            }

            if (!Options.SentWarning.GetBool())
            {
                LogDebug("Global warning not sent: option disabled.");
                return;
            }

            BanMod.hasSentHackWarning = true;
            string msg = GetAuto("PrincipalMessage");

            if (MessageBlocker.CanSendMessage())
            {
                Utils.SendMessage(msg);
                MessageBlocker.UpdateLastMessageTime();
                LogInfo("Global hack warning sent in chat.");
            }
            else
            {
                LogDebug("Global warning blocked by MessageBlocker cooldown.");
            }
        }

        public static void PlayerControlReceiveRpc(PlayerControl pc, byte callId, MessageReader reader)
        {
            if (Enum.IsDefined(typeof(CustomRPC), callId))
                return;


            if (!AntiCheat.IsHostReady()) return;
            if (pc == null || reader == null) return;
            if (pc == PlayerControl.LocalPlayer) return;

            try
            {
                var rpc = (RpcCalls)callId;

                switch (rpc)
                {
                    case RpcCalls.CompleteTask:
                        DateTime nowTask = DateTime.UtcNow;
                        byte taskId = pc.PlayerId;

                        if (!taskRpcLastTime.ContainsKey(taskId))
                        {
                            taskRpcCount[taskId] = 0;
                            taskRpcLastTime[taskId] = nowTask;
                            LogDebug($"Initialized CompleteTask monitoring for {GetPlayerLabel(pc)}.");
                        }

                        double delta = (nowTask - taskRpcLastTime[taskId]).TotalSeconds;

                        if (delta < 1d)
                        {
                            taskRpcCount[taskId]++;
                            LogDebug($"CompleteTask RPC received from {GetPlayerLabel(pc)} | Interval={delta:F3}s | Count={taskRpcCount[taskId]}");

                            if (taskRpcCount[taskId] > 2 && Options.CompleteTaskCheat.GetBool() && Options.EnableAntiCheat.GetBool())
                            {
                                string msg = $"{GetPlayerName(pc)}{GetAuto("TaskCheat")}";

                                GestisciRilevamentoCheat(
                                    pc,
                                    msg,
                                    "CompleteTask RPC spam in too short an interval"
                                );

                                taskRpcCount[taskId] = 0;
                                LogDebug($"CompleteTask counter reset for {GetPlayerLabel(pc)} after detection.");
                            }
                        }
                        else
                        {
                            taskRpcCount[taskId] = 1;
                            LogDebug($"CompleteTask counter reset for {GetPlayerLabel(pc)} | Interval={delta:F3}s");
                        }

                        taskRpcLastTime[taskId] = nowTask;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling RPC {callId} for {GetPlayerLabel(pc)}: {ex}");
                ShowHudNotification($"AntiCheat Error: {ex.Message}");
                SendGlobalHackWarning();
            }
        }
    }

}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class AntiCheatHudUpdatePatch
{
    public static void Postfix()
    {
        try
        {
            AntiCheat.FlushHudNotifications();
        }
        catch
        {
            // HudManager/Notifier can disappear while changing scene.
        }

        try
        {
            AntiCheat.VentStateMonitor.Scan();
        }
        catch
        {
            // Never propagate anti-cheat exceptions through HudManager.Update.
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
public static class BlockNonHostKickExploitPatch
{
    public static bool Prefix(byte callId, MessageReader reader)
    {
        if (callId != (byte)RpcCalls.UpdateSystem || reader == null)
            return true;

        AmongUsClient client = AmongUsClient.Instance;

        if (client == null ||
            client.NetworkMode != NetworkModes.OnlineGame ||
            client.AmHost)
        {
            return true;
        }

        int originalPosition = reader.Position;

        try
        {
            SystemTypes systemType = (SystemTypes)reader.ReadByte();
            PlayerControl player = reader.ReadNetObject<PlayerControl>();

            if (systemType != SystemTypes.Ventilation || player == null)
                return true;

            string playerName = player.Data?.PlayerName ?? "Unknown";

            BMLogger.Warn(
                $"Blocked non-host VentilationSystem kick exploit | " +
                $"Player={playerName} | PlayerId={player.PlayerId} | " +
                $"ClientId={player.OwnerId}",
                "AntiCheat"
            );

            AntiCheat.ShowHudNotification(
                $"AntiCheat: {playerName} ha tentato di espellerti " +
                "con il kick VentilationSystem."
            );

            return false;
        }
        catch
        {
            return true;
        }
        finally
        {
            reader.Position = originalPosition;
        }
    }

}

[HarmonyPatch]
public static class ShipStatus_UpdateSystem_Patch
{
    private const string LogTag = "AntiCheat";

    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(ShipStatus), "UpdateSystem", new Type[] {
        typeof(SystemTypes),
        typeof(PlayerControl),
        typeof(Hazel.MessageReader)
    });
    }

    public static void Prefix(
        ShipStatus __instance,
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] Hazel.MessageReader reader)
    {
        if (!AntiCheat.IsHostReady() || player == null || reader == null) return;

        bool meetingActive = MeetingHud.Instance != null;

        if (meetingActive && systemType != SystemTypes.Ventilation) return;

        SystemTypes[] monitoredSystems = new SystemTypes[]
        {
        SystemTypes.Electrical,
        SystemTypes.LifeSupp,
        SystemTypes.Comms,
        SystemTypes.Doors,
        SystemTypes.Sabotage,
        SystemTypes.Laboratory,
        SystemTypes.HeliSabotage,
        SystemTypes.MushroomMixupSabotage,
        SystemTypes.Reactor,
        SystemTypes.Ventilation
        };

        if (Array.IndexOf(monitoredSystems, systemType) == -1) return;

        string playerName = player.Data?.PlayerName ?? "Unknown";
        bool isAuthorized = Utils.Impostor(player) || Utils.Phantom(player) || Utils.Shapeshifter(player) || Utils.ImpostorTeam(player);

        if (systemType == SystemTypes.Ventilation)
        {
            BMLogger.LogDebug($"Received UpdateSystem Ventilation from {playerName} [pid={player.PlayerId}]", LogTag);

            int originalVentReaderPosition = reader.Position;

            try
            {
                ushort sequenceId = reader.ReadUInt16();
                byte rawOperation = reader.ReadByte();
                byte ventId = reader.ReadByte();

                if (Enum.IsDefined(
                        typeof(VentilationSystem.Operation),
                        (VentilationSystem.Operation)rawOperation))
                {
                    VentilationSystem.Operation operation =
                        (VentilationSystem.Operation)rawOperation;

                    bool canRegisterOperation =
                        !meetingActive ||
                        operation == VentilationSystem.Operation.Exit ||
                        operation == VentilationSystem.Operation.StartCleaning ||
                        operation == VentilationSystem.Operation.BootImpostors;

                    if (canRegisterOperation)
                    {
                        AntiCheat.VentStateMonitor.RegisterOperation(
                            player,
                            operation,
                            ventId
                        );

                        BMLogger.LogDebug(
                            $"Vent state registered | Player={playerName} | " +
                            $"Sid={sequenceId} | Operation={operation} | Vent={ventId}",
                            LogTag
                        );
                    }
                    else
                    {
                        BMLogger.LogDebug(
                            $"Vent state ignored during meeting | Player={playerName} | " +
                            $"Sid={sequenceId} | Operation={operation} | Vent={ventId}",
                            LogTag
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.Warn(
                    $"Unable to inspect VentilationSystem update from {playerName}: {ex.Message}",
                    LogTag
                );
            }
            finally
            {
                reader.Position = originalVentReaderPosition;
            }

            if (!meetingActive)
                AntiCheat.VentAntiCheat.RegisterVentBoot(player);

            return;
        }

        int originalPos = reader.Position;
        byte amount = reader.ReadByte();
        reader.Position = originalPos;

        BMLogger.LogDebug($"UpdateSystem intercepted | Player={playerName} [pid={player.PlayerId}] | System={systemType} | Amount={amount} | Authorized={isAuthorized}", LogTag);

        if ((amount == 128) && !isAuthorized && Options.SabotageCheat2.GetBool() && Options.EnableAntiCheat.GetBool())
        {
            if (!AntiCheat.IsHostReady()) return;
            StartFixAfterDelay(playerName, player, systemType, amount, "Sabotage sent by unauthorized player");
        }

        if (systemType == SystemTypes.Sabotage)
        {
            if (((amount == 3) || (amount == 7) || (amount == 14) || (amount == 8) || (amount == 21) || (amount == 58) || (amount == 57))
                && !isAuthorized
                && Options.SabotageCheat2.GetBool()
                && Options.EnableAntiCheat.GetBool())
            {
                if (!AntiCheat.IsHostReady()) return;
                StartFixAfterDelay(playerName, player, systemType, amount, "Sabotage trigger sent by unauthorized player");
            }
        }
        else if ((amount == 69) && !isAuthorized && Options.SabotageCheat2.GetBool() && Options.EnableAntiCheat.GetBool())
        {
            if (!AntiCheat.IsHostReady()) return;

            string msg = $"{playerName}{GetAuto("SabotageCheat")} UnfixLight";

            AntiCheat.GestisciRilevamentoCheat(
                player,
                msg,
                "Unauthorized UnfixLight attempt",
                systemType,
                amount
            );
        }
        else if (systemType == SystemTypes.Electrical &&
                 amount <= 4 &&
                 Options.SabotageCheat2.GetBool() &&
                 Options.EnableAntiCheat.GetBool() &&
                 !IsNearElectricalPanel(player))
        {
            string msg = $"{playerName} {GetAuto("AutoFixCheat")}";

            AntiCheat.GestisciRilevamentoCheat(
                player,
                msg,
                "Lights fixed from invalid position",
                systemType,
                amount
            );
        }
        else if (systemType == SystemTypes.LifeSupp &&
                 amount == 16 &&
                 Options.SabotageCheat2.GetBool() &&
                 Options.EnableAntiCheat.GetBool() &&
                 !IsNearOxygenPanel(player))
        {
            string msg = $"{playerName} {GetAuto("OxygenFixCheat")}";

            AntiCheat.GestisciRilevamentoCheat(
                player,
                msg,
                "Oxygen fixed from invalid position",
                systemType,
                amount
            );
        }
        else if ((systemType == SystemTypes.Reactor || systemType == SystemTypes.Laboratory || systemType == SystemTypes.HeliSabotage) &&
                 amount == 16 &&
                 Options.SabotageCheat2.GetBool() &&
                 Options.EnableAntiCheat.GetBool() &&
                 !IsNearReactorPanel(player))
        {
            string msg = $"{playerName} {GetAuto("ReactorFixCheat")}";

            AntiCheat.GestisciRilevamentoCheat(
                player,
                msg,
                "Reactor/Laboratory/Helicopter fixed from invalid position",
                systemType,
                amount
            );
        }
        else if (systemType == SystemTypes.Comms &&
                 amount == 16 &&
                 Options.SabotageCheat2.GetBool() &&
                 Options.EnableAntiCheat.GetBool() &&
                 !IsNearCommsPanel(player))
        {
            string msg = $"{playerName} {GetAuto("CommsFixCheat")}";

            AntiCheat.GestisciRilevamentoCheat(
                player,
                msg,
                "Comms fixed from invalid position",
                systemType,
                amount
            );
        }
    }

    private static void StartFixAfterDelay(string playerName, PlayerControl player, SystemTypes systemType, byte amount, string reason)
    {
        if (ShipStatus.Instance == null)
        {
            BMLogger.Warn($"Cannot schedule sabotage fix: ShipStatus.Instance is null | Player={playerName} | System={systemType} | Amount={amount}", LogTag);
            return;
        }

        BMLogger.Warn($"Sabotage fix scheduled | Reason={reason} | Player={playerName} [pid={player.PlayerId}] | System={systemType} | Amount={amount}", LogTag);
        ShipStatus.Instance.StartCoroutine(FixSabotagesCoroutine(playerName, player, systemType, amount));
    }

    private static IEnumerator FixSabotagesCoroutine(string playerName, PlayerControl player, SystemTypes systemType, byte amount)
    {
        yield return new WaitForSeconds(0.1f);

        if (ShipStatus.Instance == null)
        {
            BMLogger.Warn($"Sabotage fix cancelled: ShipStatus.Instance is null at execution time | Player={playerName} | System={systemType}", LogTag);
            yield break;
        }

        string sabotageName;
        switch (systemType)
        {
            case SystemTypes.Reactor:
                sabotageName = " Reactor";
                break;
            case SystemTypes.LifeSupp:
                sabotageName = " Oxygen";
                break;
            case SystemTypes.Comms:
                sabotageName = " Comms";
                break;
            case SystemTypes.Electrical:
                sabotageName = " Lights";
                break;
            case SystemTypes.Laboratory:
                sabotageName = " Laboratory";
                break;
            case SystemTypes.HeliSabotage:
                sabotageName = " Helicopter";
                break;
            default:
                sabotageName = " Sabotage";
                break;
        }

        string msg = $"{playerName} {GetAuto("SabotageCheat")}{sabotageName}";

        AntiCheat.GestisciRilevamentoCheat(
            player,
            msg,
            $"Forced fix of sabotage{sabotageName}",
            systemType,
            amount
        );

        BMLogger.Info($"Starting automatic sabotage correction for {playerName} | Origin system={systemType} | Amount={amount}", LogTag);

        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16 | 0);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16 | 1);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Laboratory, 16);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);

        if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out var system))
        {
            var elecSys = system.Cast<SwitchSystem>();
            for (var i = 0; i < 5; i++)
            {
                int switchMask = 1 << (i & 0x1F);
                if ((elecSys.ActualSwitches & switchMask) != (elecSys.ExpectedSwitches & switchMask))
                {
                    BMLogger.LogDebug($"Automatic light switch correction index={i}", LogTag);
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
                }
            }
        }

        BMLogger.Info($"Automatic sabotage correction completed for {playerName}.", LogTag);
    }

    static bool IsNearElectricalPanel(PlayerControl player)
    {
        Vector2 pos = player.GetTruePosition();
        MapNames mapId = GetCurrentMap();
        if ((int)mapId == -1) return false;

        const float defaultElectricalRange = 3.2f;
        const float airshipElectricalRange = 3.4f;

        if (mapId == MapNames.Airship)
        {
            Vector2[] airshipPanels = new Vector2[]
            {
        new Vector2(-12.82f, -11.27f),
        new Vector2(30.66f, 2.08f),
        new Vector2(13.97f, 6.35f)
            };

            return airshipPanels.Any(p => Vector2.Distance(pos, p) < airshipElectricalRange);
        }

        return Vector2.Distance(pos, GetElectricalPanelPos(mapId)) < defaultElectricalRange;
    }

    static bool IsNearOxygenPanel(PlayerControl player)
    {
        Vector2 pos = player.GetTruePosition();
        MapNames mapId = GetCurrentMap();
        if ((int)mapId == -1) return false;

        Vector2[] panels = mapId switch
        {
            MapNames.Skeld => new Vector2[]
            {
            new Vector2(6.52f, -6.61f),
            new Vector2(6.81f, -3.07f)
            },
            MapNames.MiraHQ => new Vector2[]
            {
            new Vector2(17.57f, 24.22f),
            new Vector2(4.03f, -0.63f)
            },
            _ => Array.Empty<Vector2>()
        };
        return panels.Any(p => Vector2.Distance(pos, p) < 2f);
    }

    static bool IsNearReactorPanel(PlayerControl player)
    {
        Vector2 pos = player.GetTruePosition();
        MapNames mapId = GetCurrentMap();
        if ((int)mapId == -1) return false;

        Vector2[] panels = mapId switch
        {
            MapNames.Skeld => new Vector2[]
            {
            new Vector2(-21.28f, -1.69f),
            new Vector2(-21.28f, -8.62f)
            },
            MapNames.MiraHQ => new Vector2[]
            {
            new Vector2(4.67f, 14.47f),
            new Vector2(0.24f, 14.49f)
            },
            MapNames.Polus => new Vector2[]
            {
            new Vector2(24.40f, -3.05f),
            new Vector2(4.43f, -3.91f)
            },
            MapNames.Airship => new Vector2[]
            {
            new Vector2(3.89f, 9.76f),
            new Vector2(11.49f, 6.35f)
            },
            MapNames.Fungle => new Vector2[]
            {
            new Vector2(20.95f, -6.01f),
            new Vector2(23.98f, -7.67f)
            },
            _ => Array.Empty<Vector2>()
        };
        return panels.Any(p => Vector2.Distance(pos, p) < 2f);
    }

    static bool IsNearCommsPanel(PlayerControl player)
    {
        Vector2 pos = player.GetTruePosition();
        MapNames mapId = GetCurrentMap();
        if ((int)mapId == -1) return false;

        Vector2[] panels = mapId switch
        {
            MapNames.Skeld => new Vector2[]
            {
            new Vector2(4.27f, -16.39f)
            },
            MapNames.MiraHQ => new Vector2[]
            {
            new Vector2(15.11f, 5.17f),
            new Vector2(13.79f, 18.93f),
            new Vector2(13.68f, 19.97f),
            new Vector2(14.46f, 19.24f)
            },
            MapNames.Polus => new Vector2[]
            {
            new Vector2(13.86f, -15.30f)
            },
            MapNames.Airship => new Vector2[]
            {
            new Vector2(-13.98f, 2.17f)
            },
            MapNames.Fungle => new Vector2[]
            {
            new Vector2(24.57f, 13.76f),
            new Vector2(8.25f, 0.43f)
            },
            _ => Array.Empty<Vector2>()
        };
        return panels.Any(p => Vector2.Distance(pos, p) < 2f);
    }

    static Vector2 GetElectricalPanelPos(MapNames mapId)
    {
        return mapId switch
        {
            MapNames.Skeld => new Vector2(-10.05f, -10.24f),
            MapNames.MiraHQ => new Vector2(14.74f, 21.08f),
            MapNames.Polus => new Vector2(9.60f, -11.44f),
            _ => Vector2.zero
        };
    }

}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class GameEndPatchBM_AntiCheat
{
    private static void Postfix()
    {
        if (BanMod.BanMod.IsBanModDisabled) return;
        if (!AntiCheat.IsHostReady()) return;
        AntiCheat.ResetMatchState();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
public static class CloseDoorsOfTypePatch
{
    private const string LogTag = "AntiCheat";

    private static int closeDoorsCount = 0;
    private static float firstCallTime = 0f;
    private static bool warningSent = false;

    public static void Prefix(ShipStatus __instance, SystemTypes room)
    {
        if (BanMod.BanMod.IsBanModDisabled) return;
        if (!AntiCheat.IsHostReady()) return;

        float now = Time.time;

        if (firstCallTime == 0f || now - firstCallTime > 1f)
        {
            closeDoorsCount = 0;
            firstCallTime = now;
            warningSent = false;
            BMLogger.LogDebug("Door check window reset.", LogTag);
        }

        closeDoorsCount++;
        BMLogger.LogDebug($"CloseDoorsOfType intercepted | Room={room} | Count={closeDoorsCount}", LogTag);

        if (closeDoorsCount > 4 && !warningSent && (Options.ClooseDoorsCheat.GetBool() && Options.EnableAntiCheat.GetBool()))
        {
            if (!AntiCheat.IsHostReady()) return;

            string msg = GetAuto("AnonimousCheat");
            if (HudManager.Instance?.Notifier != null)
            {
                NotificationPopper_AddInfoMessagePatch.AddInfoMessage(HudManager.Instance.Notifier, msg);
            }
            else
            {
                BMLogger.Warn($"Notifier not available during door spam report. Message={msg}", LogTag);
            }

            warningSent = true;
            BMLogger.Warn($"Possible door close spam detected | Count={closeDoorsCount} | Room={room}", LogTag);
        }
    }

}
