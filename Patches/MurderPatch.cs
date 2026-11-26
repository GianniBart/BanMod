//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Linq;
using Rewired.Utils.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BanMod.Translator;
using static BanMod.Utils;

namespace BanMod;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
internal static class CmdCheckMurder
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (__instance == PlayerControl.LocalPlayer && MurderPlayerCombinedPatch.isShapeBlocked)
        {
            return false;
        }
        return true;
    }
}

//[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
//internal static class MurderPlayerCombinedPatch
//{
//    public static readonly Dictionary<byte, int> misfireCountShape = new();
//    public static bool isShapeBlocked = false;
//    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags)
//    {
//        GameModeType gameMode = Options.GameMode.Selected;

//        if (!AmongUsClient.Instance.AmHost)
//        {
//            return;
//        }

//        if (target == null || target.Data == null)
//        {
//            return;
//        }

//        bool failedProtected =
//    resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
//    (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) &&
//     target.protectedByGuardianId > -1 &&
//     !resultFlags.HasFlag(MurderResultFlags.Succeeded));
//        bool succeeded = resultFlags.HasFlag(MurderResultFlags.Succeeded);

//        if (gameMode == GameModeType.SnS && !GameStates.isHideNSeek && IntroCutscene.Instance == null)
//        {
//            if (succeeded)
//            {
//                byte playerId = __instance.Data.PlayerId;
//                if (target.Data.PlayerId != __instance.shapeshiftTargetPlayerId)
//                {
//                    if (!misfireCountShape.ContainsKey(playerId))
//                        misfireCountShape[playerId] = 0;

//                    misfireCountShape[playerId]++;
//                    int currentMisfires = misfireCountShape[playerId];
//                    float maxAllowed = Options.MisfiresToSuicideSns.GetFloat();

//                    if (currentMisfires < maxAllowed)
//                    {
//                        float penaltyTime = Options.CantKillTimeSns.GetFloat();

//                        if (__instance == PlayerControl.LocalPlayer)
//                        {
//                            isShapeBlocked = true;
//                            LateTask.New(() =>
//                            {
//                                isShapeBlocked = false;
//                            }, (int)penaltyTime, "SNSResetRole");
//                        }
//                        else
//                        {
//                            __instance.RpcSetRole(RoleTypes.Crewmate);
//                            __instance.isNew = true;
//                            LateTask.New(() =>
//                            {
//                                __instance.isNew = false;
//                                if (!__instance.Data.IsDead) __instance.RpcSetRole(RoleTypes.Shapeshifter, false);
//                            }, (int)penaltyTime, "SNSResetRole");
//                        }
//                    }
//                    else
//                    {
//                        if (BanModServerSelection.IsModded25)
//                        {
//                            __instance.RpcMurderPlayer(__instance, true);
//                        }
//                        else
//                        {
//                            __instance.RpcSetRole(RoleTypes.ImpostorGhost);
//                        }
//                    }
//                }
//            }
//        }
//        if (failedProtected)
//        {
//            PreviousMatchPopupTracker.RegisterEffectiveProtectionSave(target);

//            FirstMeetingProtectionManager
//                .ReapplyAfterProtectedKill(target);
//        }
//        if (succeeded)
//        {
//            if (BanMod.FirstDeadFriendCode == null)
//            {
//                var friendCode = target.Data.FriendCode;
//                if (!string.IsNullOrEmpty(friendCode))
//                {
//                    BanMod.FirstDeadFriendCode = friendCode;
//                }
//            }

//            BanMod.hasKilled = true;
//            bool ignoreForStats = PreviousMatchPopupTracker.ConsumeIgnoredMirrorKill(target);

//            if (!ignoreForStats)
//            {
//                PreviousMatchPopupTracker.RegisterRealKill(__instance, target);
//                KillTracker.RegisterKill(__instance.PlayerId);
//            }

//            if (!BanMod.playerDeathTimes.ContainsKey(target.PlayerId))
//            {
//                BanMod.playerDeathTimes[target.PlayerId] = Time.time;
//            }
//        }
//    }
//}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class MurderPlayerCombinedPatch
{
    public static readonly Dictionary<byte, int> misfireCountShape = new();

    private static readonly Dictionary<byte, int> temporaryNameVersions =
        new Dictionary<byte, int>();

    public static bool isShapeBlocked = false;

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

        if (!temporaryNameVersions.TryGetValue(
                playerId,
                out int version))
        {
            version = 0;
        }

        version++;

        temporaryNameVersions[playerId] = version;

        int currentVersion = version;

        player.RpcSetName(text);

        LateTask.New(
            () =>
            {
                if (!temporaryNameVersions.TryGetValue(
                        playerId,
                        out int activeVersion) ||
                    activeVersion != currentVersion)
                {
                    return;
                }

                temporaryNameVersions.Remove(playerId);

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
            "SnSTemporaryName");
    }

    public static void ClearTemporaryNames()
    {
        temporaryNameVersions.Clear();
    }

    public static void Postfix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target,
        [HarmonyArgument(1)] MurderResultFlags resultFlags)
    {
        GameModeType gameMode = Options.GameMode.Selected;

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (target == null ||
            target.Data == null)
        {
            return;
        }

        bool failedProtected =
            resultFlags.HasFlag(
                MurderResultFlags.FailedProtected) ||
            (resultFlags.HasFlag(
                 MurderResultFlags.DecisionByHost) &&
             target.protectedByGuardianId > -1 &&
             !resultFlags.HasFlag(
                 MurderResultFlags.Succeeded));

        bool succeeded =
            resultFlags.HasFlag(
                MurderResultFlags.Succeeded);

        if (gameMode == GameModeType.SnS &&
            !GameStates.isHideNSeek &&
            IntroCutscene.Instance == null)
        {
            if (succeeded)
            {
                byte playerId =
                    __instance.Data.PlayerId;

                if (__instance.PlayerId != target.PlayerId && 
                    (target.Data.PlayerId != __instance.shapeshiftTargetPlayerId ||
                    NoKillCommsUtility.IsMisfire(__instance)))
                {
                    if (!misfireCountShape.ContainsKey(
                            playerId))
                    {
                        misfireCountShape[playerId] = 0;
                    }

                    misfireCountShape[playerId]++;

                    int currentMisfires =
                        misfireCountShape[playerId];

                    float maxAllowed =
                        Options.MisfiresToSuicideSns
                            .GetFloat();

                    if (currentMisfires < maxAllowed)
                    {
                        float penaltyTime =
                            Options.CantKillTimeSns
                                .GetFloat();

                        if (BanModServerSelection.IsModded25)
                        {
                            SetTemporaryName(
                                __instance,
                                GetString(
                                    "SnsPunishmentWrongTarget"),
                                penaltyTime);
                        }

                        if (__instance ==
                            PlayerControl.LocalPlayer)
                        {
                            isShapeBlocked = true;

                            LateTask.New(
                                () =>
                                {
                                    isShapeBlocked = false;
                                },
                                (int)penaltyTime,
                                "SNSResetRole");
                        }
                        else
                        {
                            __instance.RpcSetRole(
                                RoleTypes.Crewmate);

                            __instance.isNew = true;

                            LateTask.New(
                                () =>
                                {
                                    __instance.isNew = false;

                                    if (!__instance.Data.IsDead)
                                    {
                                        __instance.RpcSetRole(
                                            RoleTypes.Shapeshifter,
                                            false);
                                    }
                                },
                                (int)penaltyTime,
                                "SNSResetRole");
                        }
                    }
                    else
                    {
                        if (BanModServerSelection.IsModded25)
                        {
                            SetTemporaryName(
                                __instance,
                                GetString(
                                    "SnsDeathWrongTarget"),
                                15f);

                            __instance.RpcMurderPlayer(
                                __instance,
                                true);
                        }
                        else
                        {
                            __instance.RpcSetRole(
                                RoleTypes.ImpostorGhost);
                        }
                    }
                }
            }
        }

        if (failedProtected)
        {
            PreviousMatchPopupTracker
                .RegisterEffectiveProtectionSave(
                    target);

            FirstMeetingProtectionManager
                .ReapplyAfterProtectedKill(
                    target);
        }

        if (succeeded)
        {
            if (BanMod.FirstDeadFriendCode == null)
            {
                var friendCode =
                    target.Data.FriendCode;

                if (!string.IsNullOrEmpty(
                        friendCode))
                {
                    BanMod.FirstDeadFriendCode =
                        friendCode;
                }
            }

            BanMod.hasKilled = true;

            bool ignoreForStats =
                PreviousMatchPopupTracker
                    .ConsumeIgnoredMirrorKill(
                        target);

            if (!ignoreForStats)
            {
                PreviousMatchPopupTracker
                    .RegisterRealKill(
                        __instance,
                        target);

                bool snsHnsSuicide =
                    GameManager.Instance != null &&
                    GameManager.Instance.IsHideAndSeek() &&
                    Options.EnableSNS.GetBool() &&
                    __instance.PlayerId == target.PlayerId;

                if (!snsHnsSuicide)
                {
                    KillTracker.RegisterKill(
                        __instance.PlayerId);
                }
            }

            if (!BanMod.playerDeathTimes
                .ContainsKey(target.PlayerId))
            {
                BanMod.playerDeathTimes[
                    target.PlayerId] =
                    Time.time;
            }
        }
    }
}