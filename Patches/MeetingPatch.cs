//credits and licenses in the resources folder
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppSystem.Linq;
using InnerNet;
using MS.Internal.Xml.XPath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder;
using static BanMod.Translator;
using static BanMod.Utils;
using static UnityEngine.GraphicsBuffer;

namespace BanMod;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHudStartPatch
{
    private static void Postfix(MeetingHud __instance)
    {

        if (!AmongUsClient.Instance.AmHost)
            return;

        if (Options.GameTimerMessage.GetBool())
        {
            GameTimeLimit.SendTimeMessage();
        }
        FirstMeetingProtectionManager.EndAtFirstMeeting();

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            HostRoleOptionsRpc.SendToAll();
        }
        int impostorsAlive = BanMod.AllAlivePlayerControls.Count(p => p.Data.Role.IsImpostor);
        var player = PlayerControl.LocalPlayer;

        if (AmongUsClient.Instance.AmHost && Options.sendInfocomand.GetBool() && PlayerControl.LocalPlayer.Data.IsDead)
        {
            Utils.RequestProxyMessage(GetString("ComandInfo"), 255);
            MessageBlocker.UpdateLastMessageTime();
        }
        else if (AmongUsClient.Instance.AmHost && Options.sendInfocomand.GetBool() && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            Utils.SendMessage(GetString("ComandInfo"), 255);
            MessageBlocker.UpdateLastMessageTime();
        }

        if (Options.Guess.GetBool() && player.PlayerId == Guesser.SpecialKillerId)
        {
            __instance.StartCoroutine(GuessManager.WaitForButtonsAndCreate(__instance));
        }
        if (Options.ExilerExe.GetBool() && player.PlayerId == Exiler.ExilerId)
        {
            __instance.StartCoroutine(ExilerManager.WaitForButtonsAndCreate(__instance));
        }
        if (Utils.Phantom(player) && Options.PhantomGuess.GetBool() || Utils.Shapeshifter(player) && Options.ShapeGuess.GetBool() || Utils.Cobra(player) && Options.ViperGuess.GetBool())
        {
            __instance.StartCoroutine(ImpGuessManager.WaitForButtonsAndCreate(__instance));
        }

        if (!AmongUsClient.Instance.AmHost)
            return;

        if (HostAfkManager.IsHostAfk)
        {
            HostAfkManager.SendAfkNotification();
        }
        __instance.StartCoroutine(CloseMeetingManager.WaitForCloseMeetingButton(__instance));

        if (Options.ScientistTime.GetBool())
        {
            Scientist.OnMeetingStarted();
        }
        ProximityMonitor.EnsureTrackedPlayers();

    }

}


[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDisable))]
public static class MeetingHudClosePatch
{
    private static void Postfix()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
        if (Options.ProtectFirstHost.GetBool() || AmongUsClient.Instance.AmHost)
        {
            if (PlayerControl.LocalPlayer != null &&
                PlayerControl.LocalPlayer.Data != null &&
                !PlayerControl.LocalPlayer.Data.IsDead &&
                !ImmortalManager.IsImmortal(PlayerControl.LocalPlayer.PlayerId))
            {
                BanMod.ShieldedPlayers.Remove(PlayerControl.LocalPlayer.PlayerId);
                PlayerControl.LocalPlayer.RemoveProtection();
                PlayerControl.LocalPlayer.protectedByGuardianId = -1;
                PlayerControl.LocalPlayer.Data.MarkDirty();
            }
        }
        BanMod.playerDeathTimes.Clear();
        GuessManager.CleanupAfterMeeting();
        ExilerManager.CleanupAfterMeeting();
        CloseMeetingManager.CleanupAfterMeeting();
        BanMod.RoomZoneManagerInstance.ClearAllData();
        ChatCommands.ComandoRoomUsed = false;
        MessageRetryHandler.ClearQueue();
        DoorsReset.ResetDoors();
        if (AmongUsClient.Instance.IsGameOver) return;
        if (GameStates.isLobby) return;
        GameModeType gameMode = Options.GameMode.Selected;
        if (Options.Protection10Sec.GetBool())
        {
            Block.StartShieldTimer(PlayerControl.LocalPlayer, 15);
            NotificationPopper_AddInfoMessagePatch.AddInfoMessage(HudManager.Instance.Notifier, "KillBlock for 10S Added");
        }
        if (AmongUsClient.Instance.AmHost) SendHostTripleBoolRpc();
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class CombinedReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        GameModeType gameMode = Options.GameMode.Selected;

        if (!AmongUsClient.Instance.AmHost)
            return true;

        if (Options.DisableMeetingsAndReports.GetBool())
            return false;


        GameModeType gameMode1 = Options.GameMode.Selected;

        if (gameMode1 == GameModeType.FFA)
        {
            return false;
        }
        if (gameMode1 == GameModeType.FFATeam)
        {
            return false;
        }
        if (gameMode1 == GameModeType.Assassin)
        {
            return false;
        }
        if (gameMode1 == GameModeType.KillRace)
        {
            return false;
        }
        if (gameMode1 == GameModeType.RoomRush)
        {
            return false;
        }
        if (gameMode1 == GameModeType.TargetRush)
        {
            return false;
        }
        if (gameMode1 == GameModeType.DeathRun)
        {
            return false;
        }
        if (gameMode1 == GameModeType.SnS)
        {
            return false;
        }
        if (gameMode1 == GameModeType.PnS)
        {
            return false;
        }
        if (target == null)
        {
            if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId ||
                (BanMod.ExcludeFriends.Value && Utils.IsVip(__instance.FriendCode)))
                return true;

            if (!BanMod.hasKilled && Options.NoKillMeeting.GetBool())
                return false;

            return true;
        }

        if (__instance.Data?.Role?.TeamType == RoleTeamTypes.Impostor && gameMode == GameModeType.KaitoRun)
            return false;

        if (gameMode == GameModeType.SnS) return false;
        if (gameMode == GameModeType.PnS) return false;

        try
        {
            if (BanMod.UnreportableBodies.Contains(target.PlayerId))
            {
                DeadBody[] allBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                DeadBody body = allBodies.FirstOrDefault(b => b.ParentId == target.PlayerId);

                if (body != null)
                    UnityEngine.Object.Destroy(body.gameObject);

                return false;
            }
        }
        catch (Exception)
        {
        }

        return true;
    }
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaPatch
{
    private static void Postfix(PlayerVoteArea __instance, ref NetworkedPlayerInfo playerInfo)
    {
        var player = playerInfo.Object;
        if (player == null || player.Data == null || __instance.NameText == null || PlayerControl.LocalPlayer == null)
            return;

        var local = PlayerControl.LocalPlayer;

        string displayName = player.Data.PlayerName;

        if (BanMod.namewithid)
        {
            displayName = $"{player.Data.PlayerName} <color=#FFA500>(Id{player.PlayerId})</color>";
        }

        if (BanMod.Taskremain)
        {
            bool isLocalImpostor = local.Data.Role.TeamType == RoleTeamTypes.Impostor;
            bool isLocalDead = local.Data.IsDead;
            bool isLocalImpostorDead = local.Data.Role.TeamType == RoleTeamTypes.Impostor && local.Data.IsDead;
            bool isSamePlayer = local.PlayerId == player.PlayerId;
            bool isTargetCrewmate = player.Data.Role.TeamType != RoleTeamTypes.Impostor;

            bool showTasks = false;
            if (isLocalImpostorDead)
            {
                showTasks = isTargetCrewmate;
            }
            else
            {
                if (isLocalDead)
                {
                    showTasks = isTargetCrewmate;
                }
                else
                {
                    showTasks = isSamePlayer;
                }
            }

            if (showTasks)
            {
                int totalTasks = player.Data.Tasks.Count;
                int tasksDone = 0;
                foreach (var task in player.Data.Tasks)
                {
                    if (task.Complete)
                        tasksDone++;
                }

                displayName += $" <color=#00FFFF>({tasksDone}/{totalTasks})</color>";
            }
        }

        __instance.NameText.text = displayName;
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class AutoSkipVotePatch
{
    public static void Postfix(MeetingHud __instance)
    {
        if (__instance == null) return;

        if (PlayerControl.LocalPlayer.Data.IsDead && BanMod.SeeRoleMeeting.Value && PlayerControl.LocalPlayer.Data.RoleType != RoleTypes.GuardianAngel)
        {
            MeetingNametags(__instance);
        }

        if (!AmongUsClient.Instance.AmHost)
            return;

    }
}

public static class LastImpostorMeetingEndDelay
{
    private static bool delayRunning = false;

    private const float MeetingDeathDelaySeconds = 5f;

    public static bool IsRunning => delayRunning;

    public static bool TryStart(LogicGameFlowNormal flow, bool showAd)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            return false;

        if (flow == null || flow.Manager == null)
            return false;

        if (MeetingHud.Instance == null)
            return false;

        if (delayRunning)
            return true;

        var impostors = PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p =>
                p != null &&
                p.Data != null &&
                p.Data.Role != null &&
                p.Data.Role.IsImpostor)
            .ToList();

        int impostorsAliveConnected = impostors.Count(p =>
            !p.Data.IsDead &&
            !p.Data.Disconnected);

        if (impostorsAliveConnected > 0)
            return false;

        if (IsNormalImpostorExileInProgress(impostors))
            return false;

        PlayerControl disconnectedImpostor = impostors.FirstOrDefault(p =>
            p.Data.Disconnected);

        if (disconnectedImpostor != null)
        {
            string name = GetSafeName(disconnectedImpostor);

            string msg = string.Format(
                GetString("LastImpostorDisconnectedEndDelay"),
                name
            );

            SendDelayMessage(msg);
        }
       

        MeetingHud.Instance.StartCoroutine(DelayEndRoutine(flow, showAd));
        return true;
    }

    private static bool IsNormalImpostorExileInProgress(List<PlayerControl> impostors)
    {
        try
        {
            if (impostors == null || impostors.Count == 0)
                return false;

            if (MeetingHud.Instance == null)
                return false;

            NetworkedPlayerInfo exiledInfo = TryGetMeetingExiledPlayer();

            if (exiledInfo == null)
                return false;

            return impostors.Any(p =>
                p != null &&
                p.Data != null &&
                p.Data.PlayerId == exiledInfo.PlayerId);
        }
        catch
        {
            return false;
        }
    }

    private static NetworkedPlayerInfo TryGetMeetingExiledPlayer()
    {
        try
        {
            if (MeetingHud.Instance == null)
                return null;

            var field = AccessTools.Field(typeof(MeetingHud), "exiledPlayer");
            if (field != null)
            {
                object value = field.GetValue(MeetingHud.Instance);
                if (value is NetworkedPlayerInfo info)
                    return info;
            }

            var property = AccessTools.Property(typeof(MeetingHud), "exiledPlayer");
            if (property != null)
            {
                object value = property.GetValue(MeetingHud.Instance, null);
                if (value is NetworkedPlayerInfo info)
                    return info;
            }
        }
        catch
        {
        }

        return null;
    }

    private static IEnumerator DelayEndRoutine(LogicGameFlowNormal flow, bool showAd)
    {
        delayRunning = true;

        float endTime = Time.realtimeSinceStartup + MeetingDeathDelaySeconds;

        while (Time.realtimeSinceStartup < endTime)
        {
            if (BanMod.NoGameEnd.Value)
            {
                delayRunning = false;
                yield break;
            }

            yield return null;
        }

        if (AmongUsClient.Instance != null &&
            AmongUsClient.Instance.AmHost &&
            flow != null &&
            flow.Manager != null &&
            !BanMod.NoGameEnd.Value)
        {
            flow.Manager.RpcEndGame(GameOverReason.CrewmatesByVote, showAd);
        }

        delayRunning = false;
    }


    private static string GetSafeName(PlayerControl player)
    {
        try
        {
            if (player?.Data != null && !string.IsNullOrWhiteSpace(player.Data.PlayerName))
                return player.Data.PlayerName;
        }
        catch
        {
        }

        return "Unknown";
    }

    private static void SendDelayMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
            return;

        if (AmongUsClient.Instance.AmHost &&
            PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data != null &&
            PlayerControl.LocalPlayer.Data.IsDead)
        {
            Utils.RequestProxyMessage(msg);
            MessageBlocker.UpdateLastMessageTime();
        }
        else
        {
            Utils.SendMessage(msg);
            MessageBlocker.UpdateLastMessageTime();
        }
    }

    public static void Reset()
    {
        delayRunning = false;
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
public static class ExileControllerWrapUpPatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (BanMod.IsFirstRound)
        {
            BanMod.IsFirstRound = false;
        }
    }
}
[HarmonyPatch(
    typeof(ExileController),
    nameof(ExileController.ReEnableGameplay)
)]
public static class ReEnableGameplayChatPatch
{
    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        if (!Options.EnableAllChat.GetBool())
            return;

        if (!BanModServerSelection.IsModded25)
            return;

        LateTask.New(() =>
        {
            ChatUtils.EnableAllChat();

        }, 0.5f);
    }
}