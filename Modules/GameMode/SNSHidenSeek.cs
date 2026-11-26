//using AmongUs.GameOptions;
//using HarmonyLib;
//using Hazel;
//using InnerNet;
//using System.Collections.Generic;
//using UnityEngine;

//namespace BanMod
//{
//    public static class HnSHydraShapeshift
//    {
//        private const float ManualReverseDelay = 3f;

//        private class ShiftState
//        {
//            public PlayerControl Source;
//            public PlayerControl Target;
//            public float StartTime;
//        }

//        private static readonly Dictionary<byte, ShiftState> ActiveShifts = new();
//        private static readonly Dictionary<byte, int> Mistakes = new();
//        private static readonly Dictionary<byte, float> PunishedUntil = new();

//        private static bool ApplyingPenaltyKill;

//        private static bool IsEnabled()
//        {
//            return GameStates.isHideNSeek &&
//                   Options.EnableSNS.GetBool();
//        }

//        private static float GetMaxDuration()
//        {
//            float value = Options.Shapetimer.GetFloat();

//            if (value < ManualReverseDelay)
//                value = ManualReverseDelay;

//            return value;
//        }

//        private static int GetAllowedMistakes()
//        {
//            return Mathf.Clamp(
//                Options.SnSError.GetInt(),
//                0,
//                2
//            );
//        }

//        private static float GetPunishmentDuration()
//        {
//            float value = Options.SnSPunizione.GetInt();

//            if (value < 0f)
//                value = 0f;

//            return value;
//        }

//        private static bool IsPunished(byte playerId)
//        {
//            if (!PunishedUntil.TryGetValue(
//                    playerId,
//                    out float until))
//                return false;

//            if (Time.time >= until)
//            {
//                PunishedUntil.Remove(playerId);
//                return false;
//            }

//            return true;
//        }

//        public static void ForceShapeshift(
//            PlayerControl source,
//            PlayerControl target,
//            bool shouldAnimate)
//        {
//            if (!IsEnabled())
//                return;

//            if (source == null || target == null)
//                return;

//            if (AmongUsClient.Instance == null ||
//                !AmongUsClient.Instance.AmHost)
//                return;

//            source.Shapeshift(target, shouldAnimate);

//            MessageWriter writer =
//                AmongUsClient.Instance.StartRpcImmediately(
//                    source.NetId,
//                    (byte)RpcCalls.Shapeshift,
//                    SendOption.Reliable,
//                    -1
//                );

//            writer.WriteNetObject(target);
//            writer.Write(shouldAnimate);

//            AmongUsClient.Instance.FinishRpcImmediately(writer);
//        }

//        private static void StartShift(
//            PlayerControl source,
//            PlayerControl target)
//        {
//            ActiveShifts[source.PlayerId] = new ShiftState
//            {
//                Source = source,
//                Target = target,
//                StartTime = Time.time
//            };
//        }

//        private static bool TryGetShift(
//            byte playerId,
//            out ShiftState state)
//        {
//            return ActiveShifts.TryGetValue(
//                playerId,
//                out state
//            );
//        }

//        private static void RemoveShift(byte playerId)
//        {
//            ActiveShifts.Remove(playerId);
//        }

//        private static void ForceReverse(ShiftState state)
//        {
//            if (state == null ||
//                state.Source == null)
//                return;

//            PlayerControl source = state.Source;

//            RemoveShift(source.PlayerId);

//            ForceShapeshift(
//                source,
//                source,
//                true
//            );
//        }

//        private static void ApplyPunishment(
//            PlayerControl player)
//        {
//            if (player == null)
//                return;

//            float duration = GetPunishmentDuration();

//            if (duration <= 0f)
//                return;

//            PunishedUntil[player.PlayerId] =
//                Time.time + duration;
//        }

//        private static void EliminateForMistakes(
//            PlayerControl player)
//        {
//            if (player == null ||
//                player.Data == null ||
//                player.Data.IsDead)
//                return;

//            RemoveShift(player.PlayerId);
//            PunishedUntil.Remove(player.PlayerId);

//            if (BanModServerSelection.IsModded25)
//            {
//                ApplyingPenaltyKill = true;

//                try
//                {
//                    player.RpcMurderPlayer(
//                        player,
//                        true
//                    );
//                }
//                finally
//                {
//                    ApplyingPenaltyKill = false;
//                }
//            }
//            else
//            {
//                player.RpcSetRole(
//                    RoleTypes.ImpostorGhost,
//                    true
//                );
//            }
//        }

//        private static void RegisterWrongKill(
//            PlayerControl killer)
//        {
//            if (killer == null)
//                return;

//            int allowed =
//                GetAllowedMistakes();

//            Mistakes.TryGetValue(
//                killer.PlayerId,
//                out int used
//            );

//            used++;

//            Mistakes[killer.PlayerId] = used;

//            if (used > allowed)
//            {
//                EliminateForMistakes(killer);
//                return;
//            }

//            ApplyPunishment(killer);
//        }

//        private static void UpdateTimers()
//        {
//            if (!IsEnabled())
//                return;

//            if (AmongUsClient.Instance == null ||
//                !AmongUsClient.Instance.AmHost)
//                return;

//            if (ActiveShifts.Count == 0)
//                return;

//            float maxDuration =
//                GetMaxDuration();

//            List<byte> ids =
//                new List<byte>(ActiveShifts.Keys);

//            foreach (byte id in ids)
//            {
//                if (!ActiveShifts.TryGetValue(
//                        id,
//                        out ShiftState state))
//                    continue;

//                if (state == null ||
//                    state.Source == null ||
//                    state.Target == null ||
//                    state.Source.Data == null ||
//                    state.Source.Data.IsDead ||
//                    state.Source.Data.Disconnected)
//                {
//                    RemoveShift(id);
//                    continue;
//                }

//                float elapsed =
//                    Time.time - state.StartTime;

//                if (elapsed >= maxDuration)
//                    ForceReverse(state);
//            }
//        }

//        [HarmonyPatch]
//        public static class CheckShapeshiftPatch
//        {
//            static System.Reflection.MethodBase TargetMethod()
//            {
//                return AccessTools.Method(
//                    typeof(PlayerControl),
//                    "CheckShapeshift",
//                    new[]
//                    {
//                        typeof(PlayerControl),
//                        typeof(bool)
//                    }
//                );
//            }

//            public static bool Prefix(
//                PlayerControl __instance,
//                PlayerControl __0,
//                bool __1)
//            {
//                if (!IsEnabled())
//                    return true;

//                if (AmongUsClient.Instance == null ||
//                    !AmongUsClient.Instance.AmHost)
//                    return true;

//                PlayerControl target = __0;
//                bool shouldAnimate = __1;

//                if (__instance == null ||
//                    target == null ||
//                    __instance.Data == null)
//                    return true;

//                byte playerId =
//                    __instance.PlayerId;

//                if (target.PlayerId != playerId)
//                {
//                    if (IsPunished(playerId))
//                    {
//                        __instance.RpcRejectShapeshift();
//                        return false;
//                    }

//                    StartShift(
//                        __instance,
//                        target
//                    );

//                    ForceShapeshift(
//                        __instance,
//                        target,
//                        shouldAnimate
//                    );

//                    return false;
//                }

//                if (!TryGetShift(
//                        playerId,
//                        out ShiftState state))
//                    return true;

//                if (state.Target == null)
//                {
//                    RemoveShift(playerId);
//                    return true;
//                }

//                float elapsed =
//                    Time.time - state.StartTime;

//                if (elapsed < ManualReverseDelay)
//                {
//                    ForceShapeshift(
//                        __instance,
//                        state.Target,
//                        false
//                    );

//                    return false;
//                }

//                RemoveShift(playerId);

//                return true;
//            }
//        }

//        [HarmonyPatch(
//    typeof(PlayerControl),
//    nameof(PlayerControl.MurderPlayer)
//)]
//        public static class WrongKillPatch
//        {
//            public static void Postfix(
//                PlayerControl __instance,
//                PlayerControl target,
//                MurderResultFlags resultFlags)
//            {
//                if (!IsEnabled())
//                    return;

//                if (ApplyingPenaltyKill)
//                    return;

//                if (AmongUsClient.Instance == null ||
//                    !AmongUsClient.Instance.AmHost)
//                    return;

//                if (__instance == null ||
//                    target == null ||
//                    __instance == target)
//                    return;

//                if (__instance.Data == null ||
//                    target.Data == null)
//                    return;

//                if (!target.Data.IsDead)
//                    return;

//                if (!resultFlags.HasFlag(
//                        MurderResultFlags.Succeeded) &&
//                    !resultFlags.HasFlag(
//                        MurderResultFlags.DecisionByHost))
//                    return;

//                if (IsPunished(__instance.PlayerId))
//                {
//                    EliminateForMistakes(__instance);
//                    return;
//                }

//                bool correctDisguise = false;

//                if (TryGetShift(
//                        __instance.PlayerId,
//                        out ShiftState state))
//                {
//                    correctDisguise =
//                        state != null &&
//                        state.Target != null &&
//                        state.Target.PlayerId ==
//                            target.PlayerId &&
//                        __instance.CurrentOutfitType ==
//                            PlayerOutfitType.Shapeshifted;
//                }

//                if (correctDisguise)
//                    return;

//                RegisterWrongKill(__instance);
//            }
//        }

//        [HarmonyPatch]
//        public static class ShapeshiftTimerPatch
//        {
//            static System.Reflection.MethodBase TargetMethod()
//            {
//                return AccessTools.Method(
//                    typeof(AmongUsClient),
//                    "Update"
//                );
//            }

//            public static void Postfix()
//            {
//                UpdateTimers();
//            }
//        }

//        [HarmonyPatch(
//            typeof(PlayerControl),
//            nameof(PlayerControl.OnGameEnd)
//        )]
//        public static class GameEndPatch
//        {
//            public static void Postfix()
//            {
//                ActiveShifts.Clear();
//                Mistakes.Clear();
//                PunishedUntil.Clear();
//                ApplyingPenaltyKill = false;
//            }
//        }
//    }
//}
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using UnityEngine;
using static BanMod.Translator;

namespace BanMod
{
    public static class HnSHydraShapeshift
    {
        private const float ManualReverseDelay = 3f;

        private class ShiftState
        {
            public PlayerControl Source;
            public PlayerControl Target;
            public float StartTime;
        }

        private static readonly Dictionary<byte, ShiftState> ActiveShifts = new();
        private static readonly Dictionary<byte, int> Mistakes = new();
        private static readonly Dictionary<byte, float> PunishedUntil = new();
        private static readonly Dictionary<byte, int> TemporaryNameVersions =
            new Dictionary<byte, int>();


        private static bool ApplyingPenaltyKill;

        private static bool IsEnabled()
        {
            return GameStates.isHideNSeek &&
                   Options.EnableSNS.GetBool();
        }

        private static float GetMaxDuration()
        {
            float value = Options.Shapetimer.GetFloat();

            if (value < ManualReverseDelay)
                value = ManualReverseDelay;

            return value;
        }

        private static int GetAllowedMistakes()
        {
            return Mathf.Clamp(
                Options.SnSError.GetInt(),
                0,
                2
            );
        }

        private static float GetPunishmentDuration()
        {
            float value = Options.SnSPunizione.GetInt();

            if (value < 0f)
                value = 0f;

            return value;
        }

        private static bool IsPunished(byte playerId)
        {
            if (!PunishedUntil.TryGetValue(
                    playerId,
                    out float until))
                return false;

            if (Time.time >= until)
            {
                PunishedUntil.Remove(playerId);
                return false;
            }

            return true;
        }

        public static void ForceShapeshift(
            PlayerControl source,
            PlayerControl target,
            bool shouldAnimate)
        {
            if (!IsEnabled())
                return;

            if (source == null || target == null)
                return;

            if (AmongUsClient.Instance == null ||
                !AmongUsClient.Instance.AmHost)
                return;

            source.Shapeshift(target, shouldAnimate);

            MessageWriter writer =
                AmongUsClient.Instance.StartRpcImmediately(
                    source.NetId,
                    (byte)RpcCalls.Shapeshift,
                    SendOption.Reliable,
                    -1
                );

            writer.WriteNetObject(target);
            writer.Write(shouldAnimate);

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        private static void StartShift(
            PlayerControl source,
            PlayerControl target)
        {
            ActiveShifts[source.PlayerId] = new ShiftState
            {
                Source = source,
                Target = target,
                StartTime = Time.time
            };
        }

        private static bool TryGetShift(
            byte playerId,
            out ShiftState state)
        {
            return ActiveShifts.TryGetValue(
                playerId,
                out state
            );
        }

        private static void RemoveShift(byte playerId)
        {
            ActiveShifts.Remove(playerId);
        }

        private static void ForceReverse(ShiftState state)
        {
            if (state == null ||
                state.Source == null)
                return;

            PlayerControl source = state.Source;

            RemoveShift(source.PlayerId);

            ForceShapeshift(
                source,
                source,
                true
            );
        }

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

            if (!TemporaryNameVersions.TryGetValue(
                    playerId,
                    out int version))
            {
                version = 0;
            }

            version++;

            TemporaryNameVersions[playerId] = version;

            int currentVersion = version;

            player.RpcSetName(text);

            LateTask.New(
                () =>
                {
                    if (!TemporaryNameVersions.TryGetValue(
                            playerId,
                            out int activeVersion) ||
                        activeVersion != currentVersion)
                    {
                        return;
                    }

                    TemporaryNameVersions.Remove(playerId);

                    if (player == null ||
                        player.Data == null ||
                        player.Data.Disconnected)
                    {
                        return;
                    }

                    FixedUpdateUnifiedPatch
                        .RefreshNameDisplay(player);
                },
                (int)duration,
                "HnSSnSTemporaryName");
        }

        private static void ApplyPunishment(
            PlayerControl player)
        {
            if (player == null)
                return;

            float duration = GetPunishmentDuration();

            if (duration <= 0f)
                return;

            PunishedUntil[player.PlayerId] =
                Time.time + duration;

            if (BanModServerSelection.IsModded25)
            {
                SetTemporaryName(
                    player,
                    GetString("SnsPunishmentWrongTarget"),
                    duration);
            }
        }

        private static void EliminateForMistakes(
            PlayerControl player)
        {
            if (player == null ||
                player.Data == null ||
                player.Data.IsDead)
                return;

            RemoveShift(player.PlayerId);
            PunishedUntil.Remove(player.PlayerId);

            if (BanModServerSelection.IsModded25)
            {
                SetTemporaryName(
                    player,
                    GetString("SnsDeathWrongTarget"),
                    15f);

                ApplyingPenaltyKill = true;

                try
                {
                    player.RpcMurderPlayer(
                        player,
                        true
                    );
                }
                finally
                {
                    ApplyingPenaltyKill = false;
                }
            }
            else
            {
                player.Data.IsDead = true;
                player.Data.MarkDirty();
                player.RpcSetRole(RoleTypes.ImpostorGhost, true);
            }
        }

        private static void RegisterWrongKill(
            PlayerControl killer)
        {
            if (killer == null)
                return;

            int allowed =
                GetAllowedMistakes();

            Mistakes.TryGetValue(
                killer.PlayerId,
                out int used
            );

            used++;

            Mistakes[killer.PlayerId] = used;

            if (used > allowed)
            {
                EliminateForMistakes(killer);
                return;
            }

            ApplyPunishment(killer);
        }

        private static void UpdateTimers()
        {
            if (!IsEnabled())
                return;

            if (AmongUsClient.Instance == null ||
                !AmongUsClient.Instance.AmHost)
                return;

            if (ActiveShifts.Count == 0)
                return;

            float maxDuration =
                GetMaxDuration();

            List<byte> ids =
                new List<byte>(ActiveShifts.Keys);

            foreach (byte id in ids)
            {
                if (!ActiveShifts.TryGetValue(
                        id,
                        out ShiftState state))
                    continue;

                if (state == null ||
                    state.Source == null ||
                    state.Target == null ||
                    state.Source.Data == null ||
                    state.Source.Data.IsDead ||
                    state.Source.Data.Disconnected)
                {
                    RemoveShift(id);
                    continue;
                }

                float elapsed =
                    Time.time - state.StartTime;

                if (elapsed >= maxDuration)
                    ForceReverse(state);
            }
        }

        [HarmonyPatch]
        public static class CheckShapeshiftPatch
        {
            static System.Reflection.MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(PlayerControl),
                    "CheckShapeshift",
                    new[]
                    {
                        typeof(PlayerControl),
                        typeof(bool)
                    }
                );
            }

            public static bool Prefix(
                PlayerControl __instance,
                PlayerControl __0,
                bool __1)
            {
                if (!IsEnabled())
                    return true;

                if (AmongUsClient.Instance == null ||
                    !AmongUsClient.Instance.AmHost)
                    return true;

                PlayerControl target = __0;
                bool shouldAnimate = __1;

                if (__instance == null ||
                    target == null ||
                    __instance.Data == null)
                    return true;

                byte playerId =
                    __instance.PlayerId;

                if (target.PlayerId != playerId)
                {
                    if (IsPunished(playerId))
                    {
                        __instance.RpcRejectShapeshift();
                        return false;
                    }

                    StartShift(
                        __instance,
                        target
                    );

                    ForceShapeshift(
                        __instance,
                        target,
                        shouldAnimate
                    );

                    return false;
                }

                if (!TryGetShift(
                        playerId,
                        out ShiftState state))
                    return true;

                if (state.Target == null)
                {
                    RemoveShift(playerId);
                    return true;
                }

                float elapsed =
                    Time.time - state.StartTime;

                if (elapsed < ManualReverseDelay)
                {
                    ForceShapeshift(
                        __instance,
                        state.Target,
                        false
                    );

                    return false;
                }

                RemoveShift(playerId);

                return true;
            }
        }

        [HarmonyPatch(
    typeof(PlayerControl),
    nameof(PlayerControl.MurderPlayer)
)]
        public static class WrongKillPatch
        {
            public static void Postfix(
                PlayerControl __instance,
                PlayerControl target,
                MurderResultFlags resultFlags)
            {
                if (!IsEnabled())
                    return;

                if (ApplyingPenaltyKill)
                    return;

                if (AmongUsClient.Instance == null ||
                    !AmongUsClient.Instance.AmHost)
                    return;

                if (__instance == null ||
                    target == null ||
                    __instance == target)
                    return;

                if (__instance.Data == null ||
                    target.Data == null)
                    return;

                if (!target.Data.IsDead)
                    return;

                if (!resultFlags.HasFlag(
                        MurderResultFlags.Succeeded) &&
                    !resultFlags.HasFlag(
                        MurderResultFlags.DecisionByHost))
                    return;

                if (IsPunished(__instance.PlayerId))
                {
                    EliminateForMistakes(__instance);
                    return;
                }

                bool correctDisguise = false;

                if (TryGetShift(
                        __instance.PlayerId,
                        out ShiftState state))
                {
                    correctDisguise =
                        state != null &&
                        state.Target != null &&
                        state.Target.PlayerId ==
                            target.PlayerId &&
                        __instance.CurrentOutfitType ==
                            PlayerOutfitType.Shapeshifted;
                }

                if (correctDisguise)
                    return;

                RegisterWrongKill(__instance);
            }
        }

        [HarmonyPatch]
        public static class ShapeshiftTimerPatch
        {
            static System.Reflection.MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(AmongUsClient),
                    "Update"
                );
            }

            public static void Postfix()
            {
                UpdateTimers();
            }
        }

        [HarmonyPatch(
            typeof(PlayerControl),
            nameof(PlayerControl.OnGameEnd)
        )]
        public static class GameEndPatch
        {
            public static void Postfix()
            {
                ActiveShifts.Clear();
                Mistakes.Clear();
                PunishedUntil.Clear();
                TemporaryNameVersions.Clear();
                ApplyingPenaltyKill = false;
            }
        }
    }
}