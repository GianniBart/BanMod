//using AmongUs.GameOptions;
//using HarmonyLib;
//using System.Collections.Generic;
//using UnityEngine;
//using static BanMod.BanMod;

//namespace BanMod;

//internal static class PnsFlashUtility
//{
//    private const byte RedColorId = 0;
//    private const byte YellowColorId = 5;
//    private const float FlashInterval = 0.35f;

//    private sealed class FlashState
//    {
//        public PlayerControl Player;
//        public byte OriginalColor;
//        public float EndsAt;
//        public float FlashTimer;
//        public bool ShowingYellow;
//    }

//    private static readonly Dictionary<byte, FlashState> FlashingPlayers =
//        new Dictionary<byte, FlashState>();

//    public static bool IsFlashing(PlayerControl player)
//    {
//        if (player == null)
//        {
//            return false;
//        }

//        if (!FlashingPlayers.TryGetValue(
//                player.PlayerId,
//                out FlashState state))
//        {
//            return false;
//        }

//        return Time.realtimeSinceStartup < state.EndsAt;
//    }

//    public static bool Start(
//        PlayerControl player,
//        float duration)
//    {
//        if (player == null ||
//            player.Data == null ||
//            player.Data.DefaultOutfit == null ||
//            player.Data.Disconnected ||
//            duration <= 0f)
//        {
//            return false;
//        }

//        byte playerId = player.PlayerId;

//        Stop(playerId, true);

//        FlashState state = new FlashState
//        {
//            Player = player,
//            OriginalColor =
//                (byte)player.Data.DefaultOutfit.ColorId,
//            EndsAt =
//                Time.realtimeSinceStartup + duration,
//            FlashTimer = 0f,
//            ShowingYellow = false
//        };

//        FlashingPlayers[playerId] = state;

//        player.RpcSetColor(RedColorId);

//        FixedUpdateUnifiedPatch
//            .RefreshNameDisplay(player);

//        return true;
//    }

//    public static void FixedUpdate()
//    {
//        if (FlashingPlayers.Count == 0)
//        {
//            return;
//        }

//        List<byte> playerIds =
//            new List<byte>(FlashingPlayers.Keys);

//        foreach (byte playerId in playerIds)
//        {
//            if (!FlashingPlayers.TryGetValue(
//                    playerId,
//                    out FlashState state))
//            {
//                continue;
//            }

//            PlayerControl player = state.Player;

//            if (player == null ||
//                player.Data == null ||
//                player.Data.Disconnected)
//            {
//                Stop(playerId, false);
//                continue;
//            }

//            if (Time.realtimeSinceStartup >= state.EndsAt)
//            {
//                Stop(playerId, true);

//                PnsPhantomMovementPatch
//                    .BeginTrackingGrace(playerId);

//                continue;
//            }

//            state.FlashTimer += Time.fixedDeltaTime;

//            if (state.FlashTimer < FlashInterval)
//            {
//                continue;
//            }

//            state.FlashTimer -= FlashInterval;
//            state.ShowingYellow = !state.ShowingYellow;

//            player.RpcSetColor(
//                state.ShowingYellow
//                    ? YellowColorId
//                    : RedColorId);

//            FixedUpdateUnifiedPatch
//                .RefreshNameDisplay(player);
//        }
//    }

//    public static void Stop(
//        byte playerId,
//        bool restoreOriginalColor)
//    {
//        if (!FlashingPlayers.TryGetValue(
//                playerId,
//                out FlashState state))
//        {
//            return;
//        }

//        FlashingPlayers.Remove(playerId);

//        PlayerControl player = state.Player;

//        if (!restoreOriginalColor ||
//            player == null ||
//            player.Data == null ||
//            player.Data.Disconnected ||
//            AmongUsClient.Instance == null ||
//            !AmongUsClient.Instance.AmHost)
//        {
//            return;
//        }

//        player.RpcSetColor(state.OriginalColor);

//        FixedUpdateUnifiedPatch
//            .RefreshNameDisplay(player);
//    }

//    public static void StopAll(
//        bool restoreOriginalColors)
//    {
//        List<byte> playerIds =
//            new List<byte>(FlashingPlayers.Keys);

//        foreach (byte playerId in playerIds)
//        {
//            Stop(
//                playerId,
//                restoreOriginalColors);
//        }

//        FlashingPlayers.Clear();
//    }
//}

//[HarmonyPatch(
//    typeof(PlayerControl),
//    nameof(PlayerControl.FixedUpdate))]
//internal static class PnsPhantomMovementPatch
//{
//    private const float AllowedVisibleMovement = 1f;
//    private const float MinimumMovement = 0.001f;
//    private const float TeleportDistance = 2f;
//    private const float TrackingResumeDelay = 1.5f;

//    private sealed class MovementState
//    {
//        public Vector2 LastPosition;
//        public float VisibleDistance;
//    }

//    private static readonly Dictionary<byte, MovementState> States =
//        new Dictionary<byte, MovementState>();

//    private static readonly Dictionary<byte, int> MisfireCounts =
//        new Dictionary<byte, int>();

//    private static readonly Dictionary<byte, float> TrackingGraceEndsAt =
//        new Dictionary<byte, float>();

//    private static readonly HashSet<byte> ResolvedPlayers =
//        new HashSet<byte>();

//    [HarmonyPostfix]
//    public static void Postfix(PlayerControl __instance)
//    {
//        if (AmongUsClient.Instance == null ||
//            !AmongUsClient.Instance.AmHost ||
//            __instance == null ||
//            __instance.Data == null)
//        {
//            return;
//        }

//        if (__instance == PlayerControl.LocalPlayer)
//        {
//            if (IsBanModDisabled ||
//                Options.GameMode == null ||
//                !Options.GameMode.GetValue(GameModeType.PnS) ||
//                !AmongUsClient.Instance.IsGameStarted ||
//                AmongUsClient.Instance.IsGameOver)
//            {
//                PnsFlashUtility.StopAll(true);
//            }
//            else
//            {
//                PnsFlashUtility.FixedUpdate();
//            }
//        }

//        if (IsBanModDisabled)
//        {
//            return;
//        }

//        byte playerId = __instance.PlayerId;

//        if (Options.GameMode == null ||
//            !Options.GameMode.GetValue(GameModeType.PnS) ||
//            !AmongUsClient.Instance.IsGameStarted ||
//            AmongUsClient.Instance.IsGameOver ||
//            GameStates.isHideNSeek ||
//            __instance.Data.IsDead ||
//            __instance.Data.Disconnected)
//        {
//            Clear(__instance);
//            return;
//        }

//        if (__instance.Data.Role == null ||
//            __instance.Data.Role.Role != RoleTypes.Phantom)
//        {
//            Clear(__instance);
//            return;
//        }

//        Vector2 currentPosition =
//            __instance.GetTruePosition();

//        if (!States.TryGetValue(
//                playerId,
//                out MovementState state))
//        {
//            state = new MovementState
//            {
//                LastPosition = currentPosition,
//                VisibleDistance = 0f
//            };

//            States[playerId] = state;
//            return;
//        }

//        float movement = Vector2.Distance(
//            state.LastPosition,
//            currentPosition);

//        state.LastPosition = currentPosition;

//        if (PnsFlashUtility.IsFlashing(__instance))
//        {
//            state.VisibleDistance = 0f;
//            return;
//        }

//        if (TrackingGraceEndsAt.TryGetValue(
//                playerId,
//                out float graceEndsAt))
//        {
//            state.VisibleDistance = 0f;

//            if (Time.realtimeSinceStartup < graceEndsAt)
//            {
//                return;
//            }

//            TrackingGraceEndsAt.Remove(playerId);
//            state.LastPosition = currentPosition;
//            return;
//        }

//        if (IntroCutscene.Instance != null ||
//            MeetingHud.Instance != null ||
//            __instance.inVent ||
//            __instance.walkingToVent ||
//            __instance.inMovingPlat ||
//            __instance.onLadder)
//        {
//            state.VisibleDistance = 0f;
//            return;
//        }

//        if (__instance.PhantomFadeActive)
//        {
//            state.VisibleDistance = 0f;
//            return;
//        }

//        if (movement >= TeleportDistance)
//        {
//            state.VisibleDistance = 0f;
//            return;
//        }

//        if (movement < MinimumMovement)
//        {
//            return;
//        }

//        state.VisibleDistance += movement;

//        if (state.VisibleDistance <= AllowedVisibleMovement)
//        {
//            return;
//        }

//        state.VisibleDistance = 0f;

//        RegisterMisfire(__instance);
//    }

//    private static void RegisterMisfire(
//        PlayerControl player)
//    {
//        if (player == null ||
//            player.Data == null ||
//            ResolvedPlayers.Contains(player.PlayerId))
//        {
//            return;
//        }

//        byte playerId = player.PlayerId;

//        if (!MisfireCounts.TryGetValue(
//                playerId,
//                out int misfireCount))
//        {
//            misfireCount = 0;
//        }

//        misfireCount++;
//        MisfireCounts[playerId] = misfireCount;

//        int maximumMisfires = Mathf.Max(
//            1,
//            Options.MisfiresToSuicidePns.GetInt());

//        BMLogger.Info(
//            $"{player.Data.PlayerName} received PnS misfire " +
//            $"{misfireCount}/{maximumMisfires}.");

//        if (misfireCount >= maximumMisfires)
//        {
//            TurnIntoGhost(
//                player,
//                "maximum PnS misfires reached");

//            return;
//        }

//        float punishmentDuration = Mathf.Max(
//            0f,
//            Options.CantKillTimePns.GetFloat());

//        bool punishmentStarted =
//            PnsFlashUtility.Start(
//                player,
//                punishmentDuration);

//        if (!punishmentStarted)
//        {
//            BeginTrackingGrace(playerId);
//        }
//    }

//    public static void BeginTrackingGrace(
//        byte playerId)
//    {
//        TrackingGraceEndsAt[playerId] =
//            Time.realtimeSinceStartup +
//            TrackingResumeDelay;

//        if (States.TryGetValue(
//                playerId,
//                out MovementState state))
//        {
//            state.VisibleDistance = 0f;
//        }

//        BMLogger.Info(
//            $"PnS tracking for player {playerId} will resume in " +
//            $"{TrackingResumeDelay} seconds.");
//    }

//    public static void HandleKillDuringPunishment(
//        PlayerControl killer)
//    {
//        if (killer == null ||
//            killer.Data == null ||
//            !PnsFlashUtility.IsFlashing(killer))
//        {
//            return;
//        }

//        TurnIntoGhost(
//            killer,
//            "kill performed during PnS punishment");
//    }

//    private static void TurnIntoGhost(
//        PlayerControl player,
//        string reason)
//    {
//        if (player == null ||
//            player.Data == null ||
//            !ResolvedPlayers.Add(player.PlayerId))
//        {
//            return;
//        }

//        byte playerId = player.PlayerId;

//        PnsFlashUtility.Stop(
//            playerId,
//            true);

//        States.Remove(playerId);
//        TrackingGraceEndsAt.Remove(playerId);

//        BMLogger.Info(
//            $"{player.Data.PlayerName} became ImpostorGhost: {reason}.");

//        if (BanModServerSelection.IsModded25)
//        {
//            player.RpcMurderPlayer(player, true);
//        }
//        else
//        {
//            player.RpcSetRole(RoleTypes.ImpostorGhost, false);
//        }
//    }

//    private static void Clear(
//        PlayerControl player)
//    {
//        if (player == null)
//        {
//            return;
//        }

//        byte playerId = player.PlayerId;

//        PnsFlashUtility.Stop(
//            playerId,
//            true);

//        States.Remove(playerId);
//        MisfireCounts.Remove(playerId);
//        TrackingGraceEndsAt.Remove(playerId);
//        ResolvedPlayers.Remove(playerId);
//    }

//    public static void ClearAll()
//    {
//        PnsFlashUtility.StopAll(true);

//        States.Clear();
//        MisfireCounts.Clear();
//        TrackingGraceEndsAt.Clear();
//        ResolvedPlayers.Clear();
//    }
//}

//[HarmonyPatch(
//    typeof(PlayerControl),
//    nameof(PlayerControl.MurderPlayer))]
//internal static class PnsPunishmentKillPatch
//{
//    [HarmonyPostfix]
//    public static void Postfix(
//        PlayerControl __instance,
//        [HarmonyArgument(1)]
//        MurderResultFlags resultFlags)
//    {
//        if (AmongUsClient.Instance == null ||
//            !AmongUsClient.Instance.AmHost ||
//            Options.GameMode == null ||
//            !Options.GameMode.GetValue(GameModeType.PnS))
//        {
//            return;
//        }

//        if (!resultFlags.HasFlag(
//                MurderResultFlags.Succeeded))
//        {
//            return;
//        }

//        PnsPhantomMovementPatch
//            .HandleKillDuringPunishment(__instance);
//    }
//}
using AmongUs.GameOptions;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static BanMod.BanMod;
using static UnityEngine.GraphicsBuffer;

namespace BanMod;

internal static class PnsFlashUtility
{
    private const byte RedColorId = 0;
    private const byte YellowColorId = 5;
    private const float FlashInterval = 0.35f;

    private sealed class FlashState
    {
        public PlayerControl Player;
        public byte OriginalColor;
        public float EndsAt;
        public float FlashTimer;
        public bool ShowingYellow;
    }

    private static readonly Dictionary<byte, FlashState> FlashingPlayers =
        new Dictionary<byte, FlashState>();

    public static bool IsFlashing(PlayerControl player)
    {
        if (player == null)
        {
            return false;
        }

        if (!FlashingPlayers.TryGetValue(
                player.PlayerId,
                out FlashState state))
        {
            return false;
        }

        return Time.realtimeSinceStartup < state.EndsAt;
    }

    public static bool Start(
        PlayerControl player,
        float duration)
    {
        if (player == null ||
            player.Data == null ||
            player.Data.DefaultOutfit == null ||
            player.Data.Disconnected ||
            duration <= 0f)
        {
            return false;
        }

        byte playerId = player.PlayerId;

        Stop(playerId, true);

        FlashState state = new FlashState
        {
            Player = player,
            OriginalColor =
                (byte)player.Data.DefaultOutfit.ColorId,
            EndsAt =
                Time.realtimeSinceStartup + duration,
            FlashTimer = 0f,
            ShowingYellow = false
        };

        FlashingPlayers[playerId] = state;

        player.RpcSetColor(RedColorId);

        FixedUpdateUnifiedPatch
            .RefreshNameDisplay(player);

        PnsPhantomMovementPatch
            .RefreshTemporaryName(player);

        return true;
    }

    public static void FixedUpdate()
    {
        if (FlashingPlayers.Count == 0)
        {
            return;
        }

        List<byte> playerIds =
            new List<byte>(FlashingPlayers.Keys);

        foreach (byte playerId in playerIds)
        {
            if (!FlashingPlayers.TryGetValue(
                    playerId,
                    out FlashState state))
            {
                continue;
            }

            PlayerControl player = state.Player;

            if (player == null ||
                player.Data == null ||
                player.Data.Disconnected)
            {
                Stop(playerId, false);
                continue;
            }

            if (Time.realtimeSinceStartup >= state.EndsAt)
            {
                Stop(playerId, true);

                PnsPhantomMovementPatch
                    .BeginTrackingGrace(playerId);

                continue;
            }

            state.FlashTimer += Time.fixedDeltaTime;

            if (state.FlashTimer < FlashInterval)
            {
                continue;
            }

            state.FlashTimer -= FlashInterval;
            state.ShowingYellow = !state.ShowingYellow;

            player.RpcSetColor(
                state.ShowingYellow
                    ? YellowColorId
                    : RedColorId);

            FixedUpdateUnifiedPatch
                .RefreshNameDisplay(player);

            PnsPhantomMovementPatch
                .RefreshTemporaryName(player);
        }
    }

    public static void Stop(
        byte playerId,
        bool restoreOriginalColor)
    {
        if (!FlashingPlayers.TryGetValue(
                playerId,
                out FlashState state))
        {
            return;
        }

        FlashingPlayers.Remove(playerId);

        PlayerControl player = state.Player;

        if (!restoreOriginalColor ||
            player == null ||
            player.Data == null ||
            player.Data.Disconnected ||
            AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        player.RpcSetColor(state.OriginalColor);

        FixedUpdateUnifiedPatch
            .RefreshNameDisplay(player);

        PnsPhantomMovementPatch
            .RefreshTemporaryName(player);
    }

    public static void StopAll(
        bool restoreOriginalColors)
    {
        List<byte> playerIds =
            new List<byte>(FlashingPlayers.Keys);

        foreach (byte playerId in playerIds)
        {
            Stop(
                playerId,
                restoreOriginalColors);
        }

        FlashingPlayers.Clear();
    }
}

[HarmonyPatch(
    typeof(PlayerControl),
    nameof(PlayerControl.FixedUpdate))]
internal static class PnsPhantomMovementPatch
{
    private const float AllowedVisibleMovement = 1f;
    private const float MinimumMovement = 0.001f;
    private const float TeleportDistance = 2f;
    private const float TrackingResumeDelay = 1.5f;
    private sealed class MovementState
    {
        public Vector2 LastPosition;
        public float VisibleDistance;
    }

    private static readonly Dictionary<byte, MovementState> States =
        new Dictionary<byte, MovementState>();

    private static readonly Dictionary<byte, int> MisfireCounts =
        new Dictionary<byte, int>();

    private static readonly Dictionary<byte, float> TrackingGraceEndsAt =
        new Dictionary<byte, float>();

    private static readonly HashSet<byte> ResolvedPlayers =
        new HashSet<byte>();

    private static readonly Dictionary<byte, float> PendingGhostKills =
        new Dictionary<byte, float>();

    private sealed class TemporaryNameState
    {
        public PlayerControl Player;
        public string Text;
        public float EndsAt;
    }

    private static readonly Dictionary<byte, TemporaryNameState> TemporaryNames =
        new Dictionary<byte, TemporaryNameState>();
    private static void SetTemporaryName(
    PlayerControl player,
    string text,
    float duration)
    {
        if (player == null ||
            player.Data == null ||
            player.Data.Disconnected ||
            !BanModServerSelection.IsModded25)
        {
            return;
        }

        byte playerId = player.PlayerId;

        TemporaryNames[playerId] =
            new TemporaryNameState
            {
                Player = player,
                Text = text,
                EndsAt = Time.realtimeSinceStartup + duration
            };

        player.RpcSetName(text);
    }

    private static void ProcessTemporaryNames()
    {
        if (TemporaryNames.Count == 0)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;

        List<byte> playerIds =
            new List<byte>(TemporaryNames.Keys);

        foreach (byte playerId in playerIds)
        {
            if (!TemporaryNames.TryGetValue(
                    playerId,
                    out TemporaryNameState state))
            {
                continue;
            }

            PlayerControl player = state.Player;

            if (player == null ||
                player.Data == null ||
                player.Data.Disconnected)
            {
                TemporaryNames.Remove(playerId);
                continue;
            }

            if (now >= state.EndsAt)
            {
                TemporaryNames.Remove(playerId);

                FixedUpdateUnifiedPatch
                    .RefreshNameDisplay(player);

                continue;
            }
        }
    }
    public static void RefreshTemporaryName(
        PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        if (!TemporaryNames.TryGetValue(
                player.PlayerId,
                out TemporaryNameState state))
        {
            return;
        }

        if (Time.realtimeSinceStartup >= state.EndsAt)
        {
            return;
        }

        player.RpcSetName(state.Text);
    }

    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance)
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost ||
            __instance == null ||
            __instance.Data == null)
        {
            return;
        }

        if (__instance == PlayerControl.LocalPlayer)
        {
            ProcessPendingGhostKills();

            if (IsBanModDisabled ||
                Options.GameMode == null ||
                !Options.GameMode.GetValue(GameModeType.PnS) ||
                !AmongUsClient.Instance.IsGameStarted ||
                AmongUsClient.Instance.IsGameOver)
            {
                PnsFlashUtility.StopAll(true);
            }
            else
            {
                PnsFlashUtility.FixedUpdate();
            }

            ProcessTemporaryNames();
        }

        if (IsBanModDisabled)
        {
            return;
        }

        byte playerId = __instance.PlayerId;

        if (Options.GameMode == null ||
            !Options.GameMode.GetValue(GameModeType.PnS) ||
            !AmongUsClient.Instance.IsGameStarted ||
            AmongUsClient.Instance.IsGameOver ||
            GameStates.isHideNSeek ||
            __instance.Data.IsDead ||
            __instance.Data.Disconnected)
        {
            Clear(__instance);
            return;
        }

        if (__instance.Data.Role == null ||
            __instance.Data.Role.Role != RoleTypes.Phantom)
        {
            Clear(__instance);
            return;
        }

        Vector2 currentPosition =
            __instance.GetTruePosition();

        if (!States.TryGetValue(
                playerId,
                out MovementState state))
        {
            state = new MovementState
            {
                LastPosition = currentPosition,
                VisibleDistance = 0f
            };

            States[playerId] = state;
            return;
        }

        float movement = Vector2.Distance(
            state.LastPosition,
            currentPosition);

        state.LastPosition = currentPosition;

        if (PnsFlashUtility.IsFlashing(__instance))
        {
            state.VisibleDistance = 0f;
            return;
        }

        if (TrackingGraceEndsAt.TryGetValue(
                playerId,
                out float graceEndsAt))
        {
            state.VisibleDistance = 0f;

            if (Time.realtimeSinceStartup < graceEndsAt)
            {
                return;
            }

            TrackingGraceEndsAt.Remove(playerId);
            state.LastPosition = currentPosition;
            return;
        }

        if (IntroCutscene.Instance != null ||
            MeetingHud.Instance != null ||
            __instance.inVent ||
            __instance.walkingToVent ||
            __instance.inMovingPlat ||
            __instance.onLadder)
        {
            state.VisibleDistance = 0f;
            return;
        }

        if (__instance.PhantomFadeActive)
        {
            state.VisibleDistance = 0f;
            return;
        }

        if (movement >= TeleportDistance)
        {
            state.VisibleDistance = 0f;
            return;
        }

        if (movement < MinimumMovement)
        {
            return;
        }

        state.VisibleDistance += movement;

        if (state.VisibleDistance <= AllowedVisibleMovement)
        {
            return;
        }

        state.VisibleDistance = 0f;

        RegisterMisfire(__instance);
    }

    private static void ProcessPendingGhostKills()
    {
        if (PendingGhostKills.Count == 0)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;

        List<byte> playerIds =
            new List<byte>(PendingGhostKills.Keys);

        foreach (byte playerId in playerIds)
        {
            if (!PendingGhostKills.TryGetValue(
                    playerId,
                    out float killAt) ||
                now < killAt)
            {
                continue;
            }

            PendingGhostKills.Remove(playerId);

            PlayerControl player = null;

            foreach (PlayerControl candidate
                     in PlayerControl.AllPlayerControls)
            {
                if (candidate != null &&
                    candidate.PlayerId == playerId)
                {
                    player = candidate;
                    break;
                }
            }

            if (player == null ||
                player.Data == null ||
                player.Data.Disconnected)
            {
                continue;
            }

            player.RpcMurderPlayer(player, true);
        }
    }
    public static void RegisterMisfire(
        PlayerControl player)
    {
        if (player == null ||
            player.Data == null ||
            ResolvedPlayers.Contains(player.PlayerId))
        {
            return;
        }

        byte playerId = player.PlayerId;

        if (!MisfireCounts.TryGetValue(
                playerId,
                out int misfireCount))
        {
            misfireCount = 0;
        }

        misfireCount++;
        MisfireCounts[playerId] = misfireCount;

        int maximumMisfires = Mathf.Max(
            1,
            Options.MisfiresToSuicidePns.GetInt());

        BMLogger.Info(
            $"{player.Data.PlayerName} received PnS misfire " +
            $"{misfireCount}/{maximumMisfires}.");

        if (misfireCount >= maximumMisfires)
        {
            TurnIntoGhost(
                player,
                "maximum PnS misfires reached");

            return;
        }

        float punishmentDuration = Mathf.Max(
            0f,
            Options.CantKillTimePns.GetFloat());

        bool punishmentStarted =
            PnsFlashUtility.Start(
                player,
                punishmentDuration);

        if (punishmentStarted &&
            BanModServerSelection.IsModded25)
        {
            SetTemporaryName(
                player,
                Translator.GetString("DontKillOrYouDie"),
                punishmentDuration);
        }

        if (!punishmentStarted)
        {
            BeginTrackingGrace(playerId);
        }
    }

    public static void BeginTrackingGrace(
        byte playerId)
    {
        TrackingGraceEndsAt[playerId] =
            Time.realtimeSinceStartup +
            TrackingResumeDelay;

        if (States.TryGetValue(
                playerId,
                out MovementState state))
        {
            state.VisibleDistance = 0f;
        }

        BMLogger.Info(
            $"PnS tracking for player {playerId} will resume in " +
            $"{TrackingResumeDelay} seconds.");
    }

    public static void HandleKillDuringPunishment(
        PlayerControl killer)
    {
        if (killer == null ||
            killer.Data == null ||
            !PnsFlashUtility.IsFlashing(killer))
        {
            return;
        }

        TurnIntoGhost(
            killer,
            "kill performed during PnS punishment");
    }

    private static void TurnIntoGhost(
        PlayerControl player,
        string reason)
    {
        if (player == null ||
            player.Data == null ||
            !ResolvedPlayers.Add(player.PlayerId))
        {
            return;
        }

        byte playerId = player.PlayerId;

        PnsFlashUtility.Stop(
            playerId,
            true);

        States.Remove(playerId);
        TrackingGraceEndsAt.Remove(playerId);

        BMLogger.Info(
            $"{player.Data.PlayerName} became ImpostorGhost: {reason}.");

        if (BanModServerSelection.IsModded25)
        {
            SetTemporaryName(
                player,
                Translator.GetString(
                    "YouDieBecauseMovementnotAllowedOrKillBetweenPunish"),
                15f);

            player.Explosion(3f);

            PendingGhostKills[playerId] =
                Time.realtimeSinceStartup + 3f;
        }
        else
        {
            player.RpcSetRole(RoleTypes.ImpostorGhost, false);
        }
    }

    private static void Clear(
        PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        byte playerId = player.PlayerId;

        PnsFlashUtility.Stop(
            playerId,
            true);

        States.Remove(playerId);
        MisfireCounts.Remove(playerId);
        TrackingGraceEndsAt.Remove(playerId);
        ResolvedPlayers.Remove(playerId);
        PendingGhostKills.Remove(playerId);
    }

    public static void ClearAll()
    {
        PnsFlashUtility.StopAll(true);

        States.Clear();
        MisfireCounts.Clear();
        TrackingGraceEndsAt.Clear();
        ResolvedPlayers.Clear();
        PendingGhostKills.Clear();
        TemporaryNames.Clear();
    }
}

[HarmonyPatch(
    typeof(PlayerControl),
    nameof(PlayerControl.MurderPlayer))]
internal static class PnsPunishmentKillPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target,
        [HarmonyArgument(1)] MurderResultFlags resultFlags)
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost ||
            BanMod.IsBanModDisabled ||
            Options.GameMode == null ||
            !Options.GameMode.GetValue(GameModeType.PnS) ||
            GameStates.isHideNSeek ||
            IntroCutscene.Instance != null)
        {
            return;
        }

        if (__instance == null ||
            __instance.Data == null ||
            target == null ||
            target.Data == null ||
            __instance.PlayerId == target.PlayerId ||
            !resultFlags.HasFlag(MurderResultFlags.Succeeded))
        {
            return;
        }

        if (PnsFlashUtility.IsFlashing(__instance))
        {
            PnsPhantomMovementPatch
                .HandleKillDuringPunishment(__instance);

            return;
        }

        if (NoKillCommsUtility.IsMisfire(__instance))
        {
            PnsPhantomMovementPatch
                .RegisterMisfire(__instance);
        }
    }
}