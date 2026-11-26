//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Data;
using Il2CppSystem.Linq;
using InnerNet;
using Rewired.Utils.Platforms.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static BanMod.ExtendedPlayerControl;
using static BanMod.Translator;
using static BanMod.Utils;
using Math = System.Math;

namespace BanMod;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class GameEndPatch
{
    private static void Postfix()
    {
        if (BanMod.IsBanModDisabled) return;

        VoteBanTracker.Reset();

        if (!AmongUsClient.Instance.AmHost) return;
        JesterWinState.Reset();
        UnifiedRPCHandlerPatch.ModdedClients.Clear();
        UnifiedRPCHandlerPatch.ResetModdedNotifications();
        Guesser.ResetSpecialKiller();
        Jester.ResetJester();
        Exiler.ResetExiler();
        MurderPlayerCombinedPatch.misfireCountShape.Clear();
        MurderPlayerCombinedPatch.ClearTemporaryNames();
        BanMod.RoomZoneManagerInstance.ClearAllData();
        GuessManager.ResetForNewGame();
        ExilerManager.ResetForNewGame();
        CloseMeetingManager.ResetForNewGame();
        BanMod.playerDeathTimes.Clear();
        BanMod.ShieldedPlayers.Clear();
        BanMod.UnreportableBodies.Clear();
        ChatCommands.ComandoExeUsed = false;
        ChatCommands.ComandoRoomUsed = false;
        LobbyStartPatch.hasSentSummary = false;
        LobbyStartPatch.hasSentSummary1 = false;
        BanMod.forcedImpostorIds.Clear();
        ForcedRoleSystem.Clear();
        BanMod.forceImpostor = false;
        BanMod.hasSentHackWarning = false;
        BanMod.hasKilled = false;
        RpcSetTasksPatch.ResetGeneratedCommonTasks();
    }
}
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public static class GameStartPatch
{
    private static void Postfix()
    {
        if (BanMod.IsBanModDisabled) return;
        if (!AmongUsClient.Instance.AmHost) return;
        if (FakeMapLobbyUtility.Active) return;
        FirstMeetingProtectionManager.ResetForNewGame();

        GameModeType gameMode = Options.GameMode.Selected;

        if (Options.Jester.GetBool())
        {
            Jester.SelectJester();
        }
        if (gameMode == GameModeType.TaskRun)
        {
            TaskManager.GameStartTime = Time.time;
        }
        JesterWinState.endgame = false;
        AbortAllMessages();
        ImmortalManager.ResetImmortal();
        TaskManager.ResetTaskManager();
        MatchSummary1.Reset();
        EndCriteriaPatch.ResetSnsEndDelay();
        ImpostorTracker.ResetSnsSnapshot();
        TaskTracker.Clear();
        KillTracker.Clear();
        BanMod.EndGameForced = false;
        BanMod.InitiallyProtectedFriendCode = null;

        BanMod.IsFirstRound = true;
        BanMod.ProtectedPlayerIdThisMatch = 255;
    
        if (Options.ProtectFirstHost.GetBool())
        {
            if (PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (!BanMod.ShieldedPlayers.Contains(PlayerControl.LocalPlayer.PlayerId))
                {
                    BanMod.ShieldedPlayers.Add(PlayerControl.LocalPlayer.PlayerId);
                }
            }
        }
        MatchSummary1.StartMatchTimer();
        GameTimeLimit.Start();
        AmongUsClient.Instance.StartCoroutine(WaitForLocalPlayerAndExecute());
    }
    private static void ApplyProtectFirst()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        string previousFirstDeadFriendCode =
            BanMod.FirstDeadFriendCode;

        BanMod.FirstDeadFriendCode = null;

        if (!Options.ProtectFirstDead.GetBool())
        {
            BMLogger.Info(
                "[ProtectFirst] Opzione disabilitata."
            );

            return;
        }

        if (string.IsNullOrEmpty(
                previousFirstDeadFriendCode))
        {
            BMLogger.Info(
                "[ProtectFirst] Nessun primo morto registrato " +
                "nella partita precedente."
            );

            return;
        }

        PlayerControl playerToProtect =
            BanMod.AllPlayerControls.FirstOrDefault(player =>
                player != null &&
                player.Data != null &&
                !player.Data.IsDead &&
                !player.Data.Disconnected &&
                player != PlayerControl.LocalPlayer &&
                player.Data.FriendCode ==
                    previousFirstDeadFriendCode);

        if (playerToProtect == null)
        {
            BMLogger.Info(
                $"[ProtectFirst] Il giocatore non è presente " +
                $"nella nuova partita. FriendCode: " +
                $"{previousFirstDeadFriendCode}"
            );

            return;
        }

        bool applied =
            FirstMeetingProtectionManager.AddPlayer(
                playerToProtect,
                "ProtectFirst"
            );

        if (!applied)
            return;

        BanMod.InitiallyProtectedFriendCode =
            playerToProtect.Data.FriendCode;

        BanMod.ProtectedPlayerIdThisMatch =
            playerToProtect.PlayerId;

        BMLogger.Info(
            $"[ProtectFirst] Selezionato " +
            $"{playerToProtect.Data.PlayerName}, " +
            $"PlayerId: {playerToProtect.PlayerId}"
        );
    }
    private static void ApplyManualFirstMeetingProtection()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (Options.ProtectFirstPlayer == null)
            return;

        string selectedPlayerName = "None";

        try
        {
            int selectedIndex =
                Options.ProtectFirstPlayer.GetValue();

            string[] selections =
                Options.ProtectFirstPlayer.Selections;

            if (selections != null &&
                selectedIndex >= 0 &&
                selectedIndex < selections.Length)
            {
                selectedPlayerName =
                    selections[selectedIndex];
            }
        }
        catch (System.Exception exception)
        {
            BMLogger.LogWarning(
                $"[ManualFirstProtection] Errore lettura selezione: " +
                $"{exception.Message}"
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(selectedPlayerName) ||
            string.Equals(
                selectedPlayerName,
                "None",
                System.StringComparison.OrdinalIgnoreCase
            ))
        {
            BMLogger.Info(
                "[ManualFirstProtection] Nessun giocatore selezionato."
            );

            return;
        }

        // Se il GM è attivo, il player locale non può
        // ricevere la protezione manuale.
        bool gmActive =
            BanMod.GM.Value ||
            ForcedRoleSystem.GM;

        if (gmActive &&
            PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data != null &&
            string.Equals(
                selectedPlayerName,
                PlayerControl.LocalPlayer.Data.PlayerName,
                System.StringComparison.Ordinal
            ))
        {
            BMLogger.Info(
                "[ManualFirstProtection] Player locale escluso perché GM attivo."
            );

            return;
        }

        bool applied =
            FirstMeetingProtectionManager.AddPlayerByName(
                selectedPlayerName,
                "ManualSelection"
            );

        if (!applied)
        {
            BMLogger.LogWarning(
                $"[ManualFirstProtection] Impossibile proteggere: " +
                $"{selectedPlayerName}"
            );

            return;
        }

        BMLogger.Info(
            $"[ManualFirstProtection] Protezione applicata a: " +
            $"{selectedPlayerName}"
        );
    }
    private static IEnumerator WaitForLocalPlayerAndExecute()
    {
        while (PlayerControl.LocalPlayer == null)
            yield return null;

        while (PlayerControl.LocalPlayer.Data == null)
            yield return null;

        while (!BanMod.AllPlayerControls
            .Where(p => p != null && p != PlayerControl.LocalPlayer)
            .All(p => p.Data != null &&
                      (p.roleAssigned || p.Data.Disconnected)))
        {
            yield return null;
        }
        while (!PlayerControl.LocalPlayer.roleAssigned ||
       PlayerControl.LocalPlayer.Data.Role == null)
        {
            yield return null;
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.IsHideAndSeek() &&
            Options.EnableSNS.GetBool())
        {
            ImpostorTracker.CaptureSnsSnapshot();
        }
        if (Options.Jester.GetBool())
        {
            Jester.SendJesterMessage();
        }
        if (BanMod.GM.Value || ForcedRoleSystem.GM)
        {
            while (!PlayerControl.LocalPlayer.roleAssigned || PlayerControl.LocalPlayer.Data.Role == null)
            {
                yield return null;
            }
            PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.CrewmateGhost, true);
            BMLogger.Info("[BANMOD] GM Mode");
        }

        ApplyProtectFirst();
        ApplyManualFirstMeetingProtection();

        if (Options.EngineerFixer.GetBool())
            Engineer.SendEngineerMessage();

        if (Options.PhantomGuess.GetBool())
            ImpostorGuesser.SendPhantomPlayerMessage();

        if (Options.ShapeGuess.GetBool())
            ImpostorGuesser.SendShapePlayerMessage();

        if (Options.ViperGuess.GetBool())
            ImpostorGuesser.SendViperPlayerMessage();

        if (Options.ImpostorGuess.GetBool())
            ImpostorGuesser.SendImpostorPlayerMessage();

        if (Options.ScientistTime.GetBool())
            Scientist.SendScientistMessage();

        if (Options.Guess.GetBool())
        {
            Guesser.OnStart();
            Guesser.SendKillerMessage();
        }
        if (Options.ExilerExe.GetBool())
        {
            Exiler.OnStart();
            Exiler.SendExilerMessage();
        }
        PreviousMatchPopupTracker.ResetCurrentMatch();
        PreviousMatchPopupTracker.CaptureInitialRoles();
        Jester.ForcedJesterSelected = false;
        if (AmongUsClient.Instance.AmHost) SendHostTripleBoolRpc();
        BMLogger.Info("[GamePatch] Operazioni completate dopo l'assegnazione dei ruoli!");

    }
    
}
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CheckEndCriteriaPatch
{
    public static bool Prefix(LogicGameFlowNormal __instance)
    {
        GameModeType gameMode = Options.GameMode.Selected;

        if (BanMod.IsBanModDisabled) return true;
        if (!AmongUsClient.Instance.AmHost) return true;
        PreviousMatchPopupTracker.CaptureFinalRole();

        if (gameMode == GameModeType.TaskRun)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.ZombieMode)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.HotPotatoModded)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.FFA)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.FFATeam)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.Assassin)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.KillRace)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.RoomRush)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.TargetRush)
        {
            {
                return false;
            }

        }
        if (gameMode == GameModeType.DeathRun)
        {
            {
                return false;
            }

        }
        TaskTracker.Clear();
        ImpostorTracker.Clear();
        ImpostorTracker.DetectImpostors();
        int impVivi = PlayerControl.AllPlayerControls.ToArray().Count(p =>
            p != null &&
            p.Data != null &&
            !p.Data.IsDead &&
            !p.Data.Disconnected &&
            p.Data.Role.IsImpostor);

        int crewVivi = PlayerControl.AllPlayerControls.ToArray().Count(p =>
            p != null &&
            p.Data != null &&
            !p.Data.IsDead &&
            !p.Data.Disconnected &&
            !p.Data.Role.IsImpostor);
        int impostoriVivi = 0;
        bool mutanteInPunizione = false;
        bool phantomInPunizione = false;
        bool showAd = !DataManager.Player.Ads.HasPurchasedAdRemoval;

        if (BanMod.NoGameEnd.Value) return false;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null) continue;

            TaskTracker.UpdatePlayerTask(player);

        }

        if (MeetingHud.Instance != null && impVivi == 0)
        {
            if (LastImpostorMeetingEndDelay.TryStart(__instance, showAd))
                return false;
        }

        if (gameMode == GameModeType.SnS) 
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        impostoriVivi++;
                    }
                    else if (player.isNew && gameMode == GameModeType.SnS)
                    {
                        mutanteInPunizione = true;
                        BMLogger.Info($"[CHECK_WIN] Trovato mutante in punizione ({player.Data.PlayerName}). Blocco fine game.");
                    }
                }
            }
            if (impostoriVivi == 0 && mutanteInPunizione)
            {
                return false;
            }
            if (impostoriVivi == 0 && !mutanteInPunizione)
            {
                return true;
            }
        }
        if (gameMode == GameModeType.PnS)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        impostoriVivi++;
                    }
                    else if (player.isNew && gameMode == GameModeType.PnS)
                    {
                        phantomInPunizione = true;
                        BMLogger.Info($"[CHECK_WIN] Trovato phantom in punizione ({player.Data.PlayerName}). Blocco fine game.");
                    }
                }
            }
            if (impostoriVivi == 0 && phantomInPunizione)
            {
                return false;
            }
            if (impostoriVivi == 0 && !phantomInPunizione)
            {
                return true;
            }
        }

        return true; 
    }
}

[HarmonyPatch(
    typeof(EndGameManager),
    nameof(EndGameManager.Start))]
public static class EndGameSavePatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        HostAfkManager.IsHostAfk = false;

        var reason =
            EndGameResult.CachedGameOverReason;

        PreviousMatchPopupTracker
            .SaveCurrentMatch();

        if (MatchSummary1.SnsHnsReason !=
    MatchSummary1.SnsHnsEndReason.None)
        {
            bool impostorsWon =
                MatchSummary1.SnsHnsReason ==
                MatchSummary1.SnsHnsEndReason.ImpostorsWin;

            MatchSummary1.ImpostorWin = impostorsWon;
            MatchSummary1.CrewmateWin = !impostorsWon;
        }
        else if (GameTimeLimit.EndedByTimer)
        {
            switch (GameTimeLimit.WinnerByTimer)
            {
                case GameTimeLimit.TimerWinner.Crewmates:
                    {
                        MatchSummary1.CrewmateWin = true;
                        MatchSummary1.ImpostorWin = false;
                        break;
                    }

                case GameTimeLimit.TimerWinner.Impostors:
                    {
                        MatchSummary1.CrewmateWin = false;
                        MatchSummary1.ImpostorWin = true;
                        break;
                    }

                default:
                    {
                        if (GameManager.Instance != null &&
                            GameManager.Instance.DidHumansWin(reason))
                        {
                            MatchSummary1.CrewmateWin = true;
                            MatchSummary1.ImpostorWin = false;
                        }
                        else
                        {
                            MatchSummary1.CrewmateWin = false;
                            MatchSummary1.ImpostorWin = true;
                        }

                        break;
                    }
            }
        }
        else
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.DidHumansWin(reason))
            {
                MatchSummary1.CrewmateWin = true;
                MatchSummary1.ImpostorWin = false;
            }
            else
            {
                MatchSummary1.CrewmateWin = false;
                MatchSummary1.ImpostorWin = true;
            }
        }

        MatchSummary1.StopMatchTimer();
        MatchSummary1.SaveToHistory();
        if (GameManager.Instance != null && GameManager.Instance.IsHideAndSeek())
        {
            var hnsOptions = GameOptionsManager.Instance?
                .CurrentGameOptions?
                .Cast<HideNSeekGameOptionsV12>();

            if (hnsOptions != null)
                hnsOptions.ImpostorPlayerID = -1;

            if (Options.SeekerSelections != null)
            {
                foreach (var option in Options.SeekerSelections)
                {
                    if (option != null)
                        option.SetValue(0);
                }
            }

            OptionItem.SyncAllOptions();
        }

        UnifiedRPCHandlerPatch
            .AlreadyHandledCheaters
            .Clear();

        GameTimeLimit.Stop();
    }
}
[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
class CheckTaskCompletionPatch
{
    public static bool Prefix(ref bool __result)
    {
        GameModeType gameMode = Options.GameMode.Selected;

        if (BanMod.NoGameEnd.Value)
        {
            __result = false;
            return false;
        }
        if (gameMode == GameModeType.HotPotatoModded)
        {
            __result = false;
            return false;
        }
        if (gameMode == GameModeType.FFA)
        {
            __result = false;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
public static class VictoryLogicPatch
{
    public static bool Prefix(LogicGameFlowHnS __instance, ref bool __result)
    {
        int totalSeekersNeeded = Options.NumSeekers.GetInt();

        if (totalSeekersNeeded == 1)
            return true;


        var counts = __instance.GetPlayerCounts();
        int seekers = counts.Item1;
        int crewmates = counts.Item2;
        int ghosts = counts.Item3;

        __result = crewmates <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || ghosts > 0)
                   || seekers <= 0;

        return false;
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
public static class EndCriteriaPatch
{
    private static float snsEndPendingSince = -1f;

    public static void ResetSnsEndDelay()
    {
        snsEndPendingSince = -1f;
    }
    public static bool Prefix(LogicGameFlowHnS __instance)
    {
        if (BanMod.NoGameEnd.Value)
            return false;

        if (GameData.Instance == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            return true;

        if (__instance == null || __instance.Manager == null || AmongUsClient.Instance.IsGameOver)
            return false;

        PreviousMatchPopupTracker.CaptureFinalRole();
        TaskTracker.Clear();
        ImpostorTracker.Clear();
        ImpostorTracker.DetectImpostors();

        int totalSeekersNeeded = Options.NumSeekers.GetInt();

        if (totalSeekersNeeded == 1 &&
            !Options.EnableSNS.GetBool())
        {
            return true;
        }

        bool showAd = false;


        try
        {
            showAd = !DataManager.Player.Ads.HasPurchasedAdRemoval;
        }
        catch
        {
            showAd = false;
        }


        int seekersAlive = 0;
        int crewmatesAlive = 0;
        int seekersConnected = 0;

        foreach (var player in GameData.Instance.AllPlayers)
        {
            if (player == null ||
                player.Disconnected ||
                player.Role == null)
            {
                continue;
            }

            if (player.Role.IsImpostor)
            {
                seekersConnected++;

                if (!player.IsDead)
                    seekersAlive++;
            }
            else if (!player.IsDead)
            {
                crewmatesAlive++;
            }
        }

        if (Options.EnableSNS.GetBool())
        {
            // La richiesta di fine partita è già stata inviata.
            if (MatchSummary1.SnsHnsReason !=
                MatchSummary1.SnsHnsEndReason.None)
            {
                return false;
            }
            if (seekersAlive <= 0 || crewmatesAlive <= 0)
            {
                if (snsEndPendingSince < 0f)
                    snsEndPendingSince = Time.realtimeSinceStartup;

                if (Time.realtimeSinceStartup - snsEndPendingSince < 0.5f)
                    return false;
            }
            else
            {
                snsEndPendingSince = -1f;
            }
            if (seekersAlive <= 0)
            {
                MatchSummary1.SnsHnsReason = seekersConnected > 0
                    ? MatchSummary1.SnsHnsEndReason.ImpostorsDead
                    : MatchSummary1.SnsHnsEndReason.ImpostorsDisconnected;

                __instance.Manager.RpcEndGame(
                    seekersConnected > 0
                        ? GameOverReason.HideAndSeek_CrewmatesByTimer
                        : GameOverReason.ImpostorDisconnect,
                    false);

                return false;
            }

            if (crewmatesAlive <= 0)
            {
                MatchSummary1.SnsHnsReason =
                    MatchSummary1.SnsHnsEndReason.ImpostorsWin;

                __instance.Manager.RpcEndGame(
                    GameOverReason.HideAndSeek_ImpostorsByKills,
                    false);

                return false;
            }

            GameData.Instance.RecomputeTaskCounts();

            if (GameData.Instance.TotalTasks > 0 &&
                GameData.Instance.CompletedTasks >=
                GameData.Instance.TotalTasks)
            {
                MatchSummary1.SnsHnsReason =
                    MatchSummary1.SnsHnsEndReason.TasksCompleted;

                __instance.Manager.RpcEndGame(
                    GameOverReason.HideAndSeek_CrewmatesByTimer,
                    false);

                return false;
            }

            if (__instance.AllTimersExpired())
            {
                MatchSummary1.SnsHnsReason =
                    MatchSummary1.SnsHnsEndReason.TimerExpired;

                __instance.Manager.RpcEndGame(
                    GameOverReason.HideAndSeek_CrewmatesByTimer,
                    false);

                return false;
            }

            return false;
        }

        if (crewmatesAlive <= 0)
        {
            __instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_ImpostorsByKills, false);
            return false;
        }

        if (seekersAlive <= 0)
        {
            GameOverReason reason = seekersConnected > 0
                ? GameOverReason.HideAndSeek_CrewmatesByTimer
                : GameOverReason.ImpostorDisconnect;

            __instance.Manager.RpcEndGame(reason, false);
            return false;
        }

        if (__instance.AllTimersExpired())
        {
            __instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, false);
            return false;
        }
        return false;
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
public static class UpdateSeekerNamesPatch
{
    private static float timer = 0f;

    public static void Postfix()
    {
        timer += Time.deltaTime;
        if (timer < 2f) return;
        timer = 0f;

        if (AmongUsClient.Instance == null || GameData.Instance == null) return;

        var namesList = new List<string> { "Round-robin" };
        foreach (var player in GameData.Instance.AllPlayers)
        {
            if (player != null && !player.Disconnected)
                namesList.Add(player.PlayerName);
        }

        string[] namesArray = namesList.ToArray();

        foreach (var opt in Options.SeekerSelections)
        {
            opt.Selections = namesArray;
            opt.Rule = (0, namesArray.Length - 1, 1);
        }
    }
}
