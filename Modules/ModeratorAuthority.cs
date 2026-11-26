// credits and licenses in the resources folder
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BanMod
{
    public enum ModeratorAction : byte
    {
        TogglePublicPrivate = 1,
        StartGame = 2,
        InstantStart = 3,
        CallMeeting = 4,
        EndMeeting = 5,
        EndGame = 6,
        Kick = 7,
        Ban = 8,
        DestroyLobby = 9,
        SpawnLobby = 10,
        ChangeBody = 11,
        RandomFreeColor = 12,
        ToggleLobbyObject = 13
    }

    public static class ModeratorAuthority
    {
        private const string LogTag = "ModeratorAuthority";

        public static bool CanUseLocal
        {
            get
            {
                try
                {
                    if (AmongUsClient.Instance == null ||
                        PlayerControl.LocalPlayer?.Data == null)
                        return false;

                    if (AmongUsClient.Instance.AmHost)
                        return true;

                    PlayerControl local = PlayerControl.LocalPlayer;

                    string friendCode = local.FriendCode;

                    if (string.IsNullOrWhiteSpace(friendCode))
                    {
                        int clientId = local.GetClientId();

                        if (clientId >= 0)
                        {
                            ClientData client =
                                AmongUsClient.Instance.GetClient(clientId);

                            friendCode = client?.FriendCode;
                        }
                    }

                    return AllowedManager.IsModerator(friendCode);
                }
                catch (Exception ex)
                {
                    BMLogger.Warn(
                        $"CanUseLocal error: {ex}",
                        LogTag
                    );

                    return false;
                }
            }
        }

        public static void Request(
            ModeratorAction action,
            byte targetPlayerId = byte.MaxValue)
        {
            if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer?.Data == null)
                return;

            if (!CanUseLocal)
                return;

            if (AmongUsClient.Instance.AmHost)
            {
                ExecuteHost(
                    PlayerControl.LocalPlayer,
                    action,
                    targetPlayerId,
                    false);
                return;
            }

            BanModSyncApi.SendModeratorAction(
                action,
                targetPlayerId);
        }

        public static void ReceiveServer(
            byte senderPlayerId,
            ModeratorAction action,
            byte targetPlayerId)
        {
            if (AmongUsClient.Instance == null ||
                !AmongUsClient.Instance.AmHost)
            {
                return;
            }

            PlayerControl sender = BanMod.AllPlayerControls.FirstOrDefault(
                p => p != null &&
                     p.Data != null &&
                     !p.Data.Disconnected &&
                     p.PlayerId == senderPlayerId);

            if (sender == null)
                return;

            if (!IsActualModerator(sender))
            {
                BMLogger.Warn(
                    $"Moderator server request rejected: unauthorized sender | PlayerId={senderPlayerId}",
                    LogTag);
                return;
            }

            if (!Enum.IsDefined(typeof(ModeratorAction), action))
            {
                BMLogger.Warn(
                    $"Moderator server request rejected: unknown action {(byte)action}",
                    LogTag);
                return;
            }

            ExecuteHost(
                sender,
                action,
                targetPlayerId,
                true);
        }

        public static void Receive(PlayerControl sender, MessageReader reader)
        {
            if (AmongUsClient.Instance == null ||
                !AmongUsClient.Instance.AmHost ||
                sender?.Data == null ||
                reader == null)
            {
                return;
            }

            int senderClientId = sender.GetClientId();
            if (senderClientId < 0)
                return;


            ClientData senderClient = AmongUsClient.Instance.GetClient(senderClientId);
            if (senderClient == null)
                return;

            string friendCode = sender.Data.FriendCode;
            if (string.IsNullOrWhiteSpace(friendCode))
                friendCode = senderClient.FriendCode;

            if (string.IsNullOrWhiteSpace(friendCode) ||
                !AllowedManager.IsModerator(friendCode))
            {
                BMLogger.Warn(
                    $"Moderator RPC rejected: unauthorized sender | " +
                    $"Player={sender.Data.PlayerName} | " +
                    $"ClientId={senderClientId} | " +
                    $"FriendCode={friendCode}",
                    LogTag
                );

                return;
            }

            ModeratorAction action = (ModeratorAction)reader.ReadByte();
            byte targetPlayerId = reader.ReadByte();

            if (!Enum.IsDefined(typeof(ModeratorAction), action))
            {
                BMLogger.Warn(
                    $"Moderator RPC rejected: unknown action {(byte)action} | Player={sender.Data.PlayerName}",
                    LogTag);
                return;
            }

            ExecuteHost(
                sender,
                action,
                targetPlayerId,
                true);
        }

        private static PlayerControl GetTarget(byte playerId)
        {
            if (playerId == byte.MaxValue)
                return null;

            return BanMod.AllPlayerControls.FirstOrDefault(p =>
                p != null &&
                p.Data != null &&
                !p.Data.Disconnected &&
                p.PlayerId == playerId);
        }

        private static bool TryGetTargetClient(
            byte targetPlayerId,
            out PlayerControl target,
            out ClientData targetClient)
        {
            target = GetTarget(targetPlayerId);
            targetClient = null;

            if (target?.Data == null)
                return false;

            targetClient = AmongUsClient.Instance.GetClient(target.OwnerId);
            return targetClient != null;
        }

        private static bool IsProtectedTarget(ClientData client)
        {
            if (client == null)
                return true;

            if (AllowedManager.IsModCreator(client.FriendCode))
                return true;

            if (BanMod.IsProtected(client))
                return true;

            return false;
        }

        private static bool IsActualModerator(PlayerControl player)
        {
            if (player?.Data == null || AmongUsClient.Instance == null)
                return false;

            string friendCode = player.Data.FriendCode;

            if (string.IsNullOrWhiteSpace(friendCode))
            {
                int clientId = player.GetClientId();

                if (clientId >= 0)
                {
                    ClientData client = AmongUsClient.Instance.GetClient(clientId);
                    friendCode = client?.FriendCode;
                }
            }

            return !string.IsNullOrWhiteSpace(friendCode) &&
                   AllowedManager.IsModerator(friendCode);
        }

        public static bool CanManuallyRemove(
    PlayerControl actor,
    ClientData target)
        {
            var instance = AmongUsClient.Instance;

            if (instance == null ||
                !instance.AmHost ||
                actor?.Data == null ||
                target == null)
                return false;

            if (target.Id == instance.HostId ||
                AllowedManager.IsModCreator(target.FriendCode))
                return false;

            ClientData actorClient = instance.GetClient(actor.OwnerId);

            if (actorClient == null)
                return false;

            bool actorIsCreator =
                AllowedManager.IsModCreator(actorClient.FriendCode);

            bool actorIsHost =
                actorClient.Id == instance.HostId;

            bool actorIsModerator =
                AllowedManager.IsModerator(actorClient.FriendCode);

            if (!actorIsHost && !actorIsModerator && !actorIsCreator)
                return false;

            return actorIsCreator || !BanMod.IsProtected(target);
        }

        private static string GetModeratorActionName(ModeratorAction action)
        {
            return action switch
            {
                ModeratorAction.TogglePublicPrivate => "Toggle Public/Private",
                ModeratorAction.StartGame => "Start Game",
                ModeratorAction.InstantStart => "Instant Start",
                ModeratorAction.CallMeeting => "Call Meeting",
                ModeratorAction.EndMeeting => "End Meeting",
                ModeratorAction.EndGame => "End Game",
                ModeratorAction.Kick => "Kick",
                ModeratorAction.Ban => "Ban",
                ModeratorAction.DestroyLobby => "Destroy Lobby",
                ModeratorAction.SpawnLobby => "Spawn Lobby",
                ModeratorAction.ChangeBody => "Change Body",
                ModeratorAction.RandomFreeColor => "Random Free Color",
                ModeratorAction.ToggleLobbyObject => "Toggle Lobby Object",
                _ => action.ToString()
            };
        }

        public static void ReportModeratorUsage(
            PlayerControl moderator,
            string actionName,
            byte targetPlayerId = byte.MaxValue,
            string source = "UNKNOWN")
        {
            try
            {
                if (AmongUsClient.Instance == null ||
                    !AmongUsClient.Instance.AmHost)
                    return;

                if (moderator?.Data == null)
                    return;

                if (moderator == PlayerControl.LocalPlayer ||
                    moderator.AmOwner)
                    return;

                if (!IsActualModerator(moderator))
                    return;

                string moderatorName =
                    moderator.Data.PlayerName ?? "Unknown";

                PlayerControl target = GetTarget(targetPlayerId);

                string targetLog = "";
                string targetHud = "";

                if (target?.Data != null)
                {
                    targetLog =
                        $" | Target={target.Data.PlayerName}" +
                        $" | TargetPlayerId={target.PlayerId}";

                    targetHud =
                        $" -> <color=#FFFFFF>{target.Data.PlayerName}</color>";
                }

                BMLogger.Info(
                    $"MODERATOR ACTION | " +
                    $"Source={source} | " +
                    $"Moderator={moderatorName} | " +
                    $"ModeratorPlayerId={moderator.PlayerId} | " +
                    $"Action={actionName}" +
                    targetLog,
                    LogTag
                );

                if (HudManager.Instance?.Notifier != null)
                {
                    NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                        HudManager.Instance.Notifier,
                        $"<color=#00FFFF>[MOD]</color> " +
                        $"<color=#FFD700>{moderatorName}</color> " +
                        $"used <color=#FFAA00>{actionName}</color>" +
                        targetHud
                    );
                }
            }
            catch (Exception ex)
            {
                BMLogger.Warn(
                    $"ReportModeratorUsage error: {ex}",
                    LogTag
                );
            }
        }

        private static void LogModeratorAction(
            PlayerControl moderator,
            ModeratorAction action,
            byte targetPlayerId,
            bool fromModeratorCommand)
        {
            if (!fromModeratorCommand)
                return;

            ReportModeratorUsage(
                moderator,
                GetModeratorActionName(action),
                targetPlayerId,
                "MENU"
            );
        }

        private static void ExecuteHost(
            PlayerControl moderator,
            ModeratorAction action,
            byte targetPlayerId,
            bool fromModeratorCommand)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                return;

            switch (action)
            {
                case ModeratorAction.TogglePublicPrivate:
                    {
                        if (!GameStates.isLobby)
                            return;

                        var manager = DestroyableSingleton<GameStartManager>.Instance;

                        if (manager == null)
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        manager.MakePublic();
                        break;
                    }

                case ModeratorAction.StartGame:
                    {
                        if (!GameStates.isLobby)
                            return;

                        var manager = UnityEngine.Object.FindObjectOfType<GameStartManager>();
                        if (manager == null)
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        manager.BeginGame();
                        break;
                    }

                case ModeratorAction.InstantStart:
                    {
                        if (!GameStates.isLobby)
                            return;

                        var manager = UnityEngine.Object.FindObjectOfType<GameStartManager>();
                        if (manager == null)
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        manager.BeginGame();
                        break;
                    }

                case ModeratorAction.CallMeeting:
                    {
                        if (GameStates.isLobby || moderator?.Data == null || moderator.Data.IsDead)
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        moderator.CmdReportDeadBody(null);
                        break;
                    }

                case ModeratorAction.EndMeeting:
                    {
                        if (MeetingHud.Instance == null)
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        PlayerControl.LocalPlayer.StartCoroutine(Utils.DelayedCloseMeeting());
                        break;
                    }

                case ModeratorAction.EndGame:
                    {
                        if (!GameStates.IsInGameplay || GameManager.Instance == null)
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        BanMod.EndGameForced = true;
                        GameManager.Instance.RpcEndGame(
                            GameOverReason.CrewmatesByTask,
                            false);
                        break;
                    }

                case ModeratorAction.Kick:
                    {
                        if (!TryGetTargetClient(targetPlayerId, out PlayerControl target, out ClientData client))
                            return;

                        if (!CanManuallyRemove(moderator, client))
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        AmongUsClient.Instance.KickPlayer(client.Id, false);
                        break;
                    }

                case ModeratorAction.Ban:
                    {
                        if (!TryGetTargetClient(targetPlayerId, out PlayerControl target, out ClientData client))
                            return;

                        if (!CanManuallyRemove(moderator, client))
                            return;

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        BanManager.AddBanPlayer(
                            client,
                            fromModeratorCommand ? "Moderator" : "ManualBan",
                            fromModeratorCommand);
                        AmongUsClient.Instance.KickPlayer(client.Id, true);
                        break;
                    }


                case ModeratorAction.ChangeBody:
                    {
                        PlayerControl target = GetTarget(targetPlayerId);
                        if (target?.Data == null)
                            return;

                        PlayerBodyTypes nextBody = Utils.GetNextBodyType(target);
                        float scale = Mathf.Clamp(target.transform.localScale.x, 0.25f, 2.0f);

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        target.transform.localScale = new Vector3(scale, scale, 1f);
                        Utils.SetPlayerBodyType(target, nextBody);

                        BanModSyncApi.SendSyncPlayerVisual(
                            target.PlayerId,
                            scale,
                            (byte)nextBody,
                            false);
                        break;
                    }

                case ModeratorAction.RandomFreeColor:
                    {
                        PlayerControl target = GetTarget(targetPlayerId);
                        if (target?.Data == null)
                            return;

                        List<byte> usedColors = new();
                        foreach (var info in GameData.Instance.AllPlayers)
                        {
                            if (info?.DefaultOutfit != null)
                                usedColors.Add((byte)info.DefaultOutfit.ColorId);
                        }

                        List<byte> freeColors = Enumerable
                            .Range(0, Palette.PlayerColors.Length)
                            .Select(i => (byte)i)
                            .Where(c => !usedColors.Contains(c))
                            .ToList();

                        if (freeColors.Count == 0)
                            freeColors.Add(0);

                        byte color = freeColors[new System.Random().Next(freeColors.Count)];

                        LogModeratorAction(
                            moderator,
                            action,
                            targetPlayerId,
                            fromModeratorCommand);

                        target.RpcSetColor(color);
                        break;
                    }
            }
        }
    }
}
