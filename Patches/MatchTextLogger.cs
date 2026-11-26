using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BanMod;

public static class MatchTextLogger
{
    private sealed class VoteEntry
    {
        public byte VoterId;
        public byte VotedForId;
        public bool Invalidated;
        public string Text;
    }

    private sealed class MeetingEntry
    {
        public int Number;
        public bool Finished;
        public float StartedAt;
        public float FinishedAt = -1f;
        public string CalledBy;
        public string CallType;
        public string ReportedPlayer;
        public string Outcome;
        public readonly Dictionary<byte, string> AlivePlayers = new();
        public readonly Dictionary<byte, string> PendingRevoteTargets = new();
        public readonly HashSet<byte> PlayersWhoVoted = new();
        public readonly List<VoteEntry> Votes = new();
        public readonly List<string> DeadPlayers = new();
    }

    private sealed class RoleEntry
    {
        public byte PlayerId;
        public int ColorId;
        public string PlayerLabel;
        public string RoleName;
    }

    private sealed class TaskEntry
    {
        public float CompletionTime;
        public string TaskName;
        public int Order;
    }

    private sealed class TaskTimelineEntry
    {
        public float Time;
        public int Order;
        public string Text;
        public bool IsMeeting;
        public bool IsGameStart;
    }

    private sealed class PlayerTaskEntry
    {
        public byte PlayerId;
        public string PlayerName;
        public string ColorName;
        public int TotalTasks;
        public readonly HashSet<uint> CompletedTaskIds = new();
        public readonly List<TaskEntry> Tasks = new();
    }

    private sealed class KillEntry
    {
        public byte KillerId;
        public string KillerLabel;
        public string VictimLabel;
        public string Location;
        public float Time;
    }

    private sealed class TimedTextEntry
    {
        public string Text;
        public string Location;
        public float Time;
        public int Order;
    }

    private sealed class DisconnectEntry
    {
        public string PlayerLabel;
        public string Reason;
        public float Time;
    }

    private static readonly List<MeetingEntry> Meetings = new();
    private static readonly List<RoleEntry> Roles = new();
    private static readonly List<KillEntry> Kills = new();
    private static readonly List<TimedTextEntry> Protections = new();
    private static readonly List<TimedTextEntry> Sabotages = new();
    private static readonly List<TimedTextEntry> Vents = new();
    private static readonly List<TimedTextEntry> NoisemakerEvents = new();
    private static readonly List<TimedTextEntry> TaskProgress = new();
    private static readonly List<DisconnectEntry> Disconnects = new();
    private static readonly Dictionary<byte, PlayerTaskEntry> PlayersTasks = new();
    private static readonly HashSet<byte> LoggedVictims = new();
    private static readonly HashSet<byte> LoggedDisconnects = new();
    private static readonly HashSet<byte> LoggedCompletedPlayers = new();
    private static MeetingEntry currentMeeting;
    private static float matchStartTime;
    private static DateTime matchStartDate;
    private static DateTime matchEndDate;
    private static string mapName;
    private static string gameModeName;
    private static string activeSabotage;
    private static string lastProtectionKey;
    private static float lastProtectionTime = -10f;
    private static int nextTaskProgressThreshold;
    private static int nextTaskEventOrder;
    private static int globalTotalTasks;
    private static bool matchActive;
    private static bool reportSaved;
    private static string pendingReportPath;

    private static readonly Dictionary<int, string> SkeldVents = new()
    {
        { 0, "Admin" },
        { 1, "Navigation Hallway" },
        { 2, "Cafeteria" },
        { 3, "Electrical" },
        { 4, "Upper Engine" },
        { 5, "Security" },
        { 6, "MedBay" },
        { 7, "Weapons" },
        { 8, "Lower Reactor" },
        { 9, "Lower Engine" },
        { 10, "Shields" },
        { 11, "Upper Reactor" },
        { 12, "Upper Navigation" },
        { 13, "Lower Navigation" }
    };

    private static readonly Dictionary<int, string> MiraVents = new()
    {
        { 0, "Admin" },
        { 1, "Balcony" },
        { 2, "Right Y Hallway" },
        { 3, "Reactor" },
        { 4, "Laboratory" },
        { 5, "Office" },
        { 6, "Admin" },
        { 7, "Greenhouse" },
        { 8, "MedBay" },
        { 9, "Decontamination" },
        { 10, "Locker Room" },
        { 11, "Launchpad" }
    };

    private static readonly Dictionary<int, string> PolusVents = new()
    {
        { 0, "Electrical" },
        { 1, "Under Electrical Cage" },
        { 2, "O2" },
        { 3, "Communications" },
        { 4, "Office" },
        { 5, "Admin" },
        { 6, "Laboratory" },
        { 7, "Under Laboratory" },
        { 8, "Storage" },
        { 9, "Right Reactor" },
        { 10, "Left Reactor" },
        { 11, "Outside Admin" }
    };

    private static readonly Dictionary<int, string> AirshipVents = new()
    {
        { 0, "Vault" },
        { 1, "Cockpit" },
        { 2, "Left Viewing Deck" },
        { 3, "Engine Room" },
        { 4, "Kitchen" },
        { 5, "Main Hall Bottom" },
        { 6, "Main Hall Trash" },
        { 7, "Right Gap Room" },
        { 8, "Left Gap Room" },
        { 9, "Showers" },
        { 10, "Records" },
        { 11, "Cargo Bay" }
    };

    private static readonly Dictionary<int, string> FungleVents = new()
    {
        { 0, "Communications" },
        { 1, "Kitchen" },
        { 2, "Lookout" },
        { 3, "Above Dorm Room" },
        { 4, "Laboratory" },
        { 5, "Reactor" },
        { 6, "Right of Laboratory" },
        { 7, "Mushroom Monitor" },
        { 8, "Splash Zone" },
        { 9, "Left of Dropship" }
    };

    private static bool IsHost =>
        AmongUsClient.Instance != null &&
        AmongUsClient.Instance.AmHost;

    public static void StartMatch()
    {
        if (!IsHost)
            return;

        Meetings.Clear();
        Roles.Clear();
        Kills.Clear();
        Protections.Clear();
        Sabotages.Clear();
        Vents.Clear();
        NoisemakerEvents.Clear();
        TaskProgress.Clear();
        Disconnects.Clear();
        PlayersTasks.Clear();
        LoggedVictims.Clear();
        LoggedDisconnects.Clear();
        LoggedCompletedPlayers.Clear();
        currentMeeting = null;
        activeSabotage = null;
        lastProtectionKey = null;
        lastProtectionTime = -10f;
        nextTaskProgressThreshold = 25;
        nextTaskEventOrder = 1;
        globalTotalTasks = 0;
        matchStartTime = Time.realtimeSinceStartup;
        matchStartDate = DateTime.Now;
        matchEndDate = default;
        mapName = GetMapName();
        gameModeName = GetGameModeName();
        matchActive = true;
        reportSaved = false;

        BMLogger.Info("[MatchTextLogger] Match recording started.");
    }

    public static void CaptureInitialRoles()
    {
        if (!IsHost || !matchActive || PlayerControl.AllPlayerControls == null)
            return;

        Roles.Clear();

        if (GameData.Instance != null && GameData.Instance.TotalTasks > 0)
            globalTotalTasks = GameData.Instance.TotalTasks;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null)
                continue;

            Roles.Add(new RoleEntry
            {
                PlayerId = player.PlayerId,
                ColorId = GetColorId(player.Data),
                PlayerLabel = GetPlayerLabel(player.Data),
                RoleName = GetRoleName(player)
            });

            if (player.Data.Role != null &&
                player.Data.Role.TeamType != RoleTeamTypes.Impostor &&
                GetTotalTasks(player) > 0)
            {
                GetOrCreateTaskPlayer(player);
            }
        }
    }

    public static void StartMeeting(
        MeetingHud meetingHud,
        NetworkedPlayerInfo reporter,
        NetworkedPlayerInfo reportedBody,
        Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
    {
        if (!IsHost || !matchActive || meetingHud == null)
            return;

        FinishMeeting();

        if (Roles.Count == 0)
            CaptureInitialRoles();

        currentMeeting = new MeetingEntry
        {
            Number = Meetings.Count + 1,
            StartedAt = GetElapsedTime(),
            CalledBy = GetPlayerLabel(reporter),
            CallType = reportedBody == null ? "Emergency meeting" : "Body report",
            ReportedPlayer = reportedBody == null ? "" : GetPlayerLabel(reportedBody)
        };

        if (deadBodies != null)
        {
            foreach (var deadPlayer in deadBodies)
            {
                if (deadPlayer == null)
                    continue;

                string label = GetPlayerLabel(deadPlayer);

                if (!currentMeeting.DeadPlayers.Contains(label))
                    currentMeeting.DeadPlayers.Add(label);
            }
        }

        if (meetingHud.playerStates != null && GameData.Instance != null)
        {
            foreach (var voteArea in meetingHud.playerStates)
            {
                if (!voteArea)
                    continue;

                byte playerId = voteArea.PlayerId.Value;
                var playerData = GameData.Instance.GetPlayerById(voteArea.PlayerId);

                if (playerData == null || playerData.Disconnected || playerData.IsDead)
                    continue;

                currentMeeting.AlivePlayers[playerId] = GetColorName(playerData);
            }
        }

        Meetings.Add(currentMeeting);
    }

    public static void RecordVote(MeetingHud meetingHud, byte voterId)
    {
        if (!IsHost ||
            !matchActive ||
            currentMeeting == null ||
            currentMeeting.Finished ||
            meetingHud == null ||
            meetingHud.playerStates == null)
        {
            return;
        }

        if (!currentMeeting.AlivePlayers.ContainsKey(voterId) ||
            currentMeeting.PlayersWhoVoted.Contains(voterId))
        {
            return;
        }

        PlayerVoteArea voterArea = null;

        foreach (var area in meetingHud.playerStates)
        {
            if (area && area.PlayerId.Value == voterId)
            {
                voterArea = area;
                break;
            }
        }


        if (!voterArea)
            return;

        byte votedForId = voterArea.VotedForId;

        if (votedForId == PlayerVoteArea.HasNotVoted ||
            votedForId == PlayerVoteArea.MissedVote ||
            votedForId == PlayerVoteArea.DeadVote)
        {
            return;
        }

        string voterColor = currentMeeting.AlivePlayers[voterId];
        string voteText;

        if (votedForId == PlayerVoteArea.SkippedVote)
        {
            voteText = $"{voterColor} skipped";
        }
        else
        {
            if (!currentMeeting.AlivePlayers.TryGetValue(votedForId, out string targetColor))
            {
                var target = GameData.Instance?.GetPlayerById(votedForId);
                targetColor = target != null ? GetColorName(target) : $"Player {votedForId}";
            }

            voteText = $"{voterColor} voted {targetColor}";
        }

        if (currentMeeting.PendingRevoteTargets.TryGetValue(
                voterId,
                out string disconnectedTarget))
        {
            voteText +=
                $" (revoted because {disconnectedTarget} disconnected)";

            currentMeeting.PendingRevoteTargets.Remove(voterId);
        }

        currentMeeting.PlayersWhoVoted.Add(voterId);
        currentMeeting.Votes.Add(new VoteEntry
        {
            VoterId = voterId,
            VotedForId = votedForId,
            Text = voteText
        });
    }
    public static void RecordFinalVotes(
    Il2CppStructArray<MeetingHud.VoterState> states)
    {
        if (!IsHost ||
            !matchActive ||
            currentMeeting == null ||
            currentMeeting.Finished ||
            states == null ||
            states.Length == 0)
        {
            return;
        }

        for (int i = 0; i < states.Length; i++)
        {
            MeetingHud.VoterState state = states[i];

            byte voterId = state.VoterId;
            byte votedForId = state.VotedForId;

            if (votedForId == PlayerVoteArea.HasNotVoted ||
                votedForId == PlayerVoteArea.MissedVote ||
                votedForId == PlayerVoteArea.DeadVote)
            {
                continue;
            }

            if (currentMeeting.PlayersWhoVoted.Contains(voterId))
                continue;

            if (!currentMeeting.AlivePlayers.TryGetValue(
                    voterId,
                    out string voterColor))
            {
                var voter =
                    GameData.Instance?.GetPlayerById(voterId);

                if (voter == null)
                    continue;

                voterColor = GetColorName(voter);
            }

            string voteText;

            if (votedForId == PlayerVoteArea.SkippedVote)
            {
                voteText = $"{voterColor} skipped";
            }
            else
            {
                if (!currentMeeting.AlivePlayers.TryGetValue(
                        votedForId,
                        out string targetColor))
                {
                    var target =
                        GameData.Instance?.GetPlayerById(votedForId);

                    targetColor = target != null
                        ? GetColorName(target)
                        : $"Player {votedForId}";
                }

                voteText =
                    $"{voterColor} voted {targetColor}";
            }

            if (currentMeeting.PendingRevoteTargets.TryGetValue(
                    voterId,
                    out string disconnectedTarget))
            {
                voteText +=
                    $" (revoted because {disconnectedTarget} disconnected)";

                currentMeeting.PendingRevoteTargets.Remove(voterId);
            }

            currentMeeting.PlayersWhoVoted.Add(voterId);

            currentMeeting.Votes.Add(new VoteEntry
            {
                VoterId = voterId,
                VotedForId = votedForId,
                Text = voteText
            });
        }
    }
    public static void FinishMeeting(
        NetworkedPlayerInfo exiled = null,
        bool isTie = false,
        bool hasResult = false)
    {
        if (currentMeeting == null || currentMeeting.Finished)
            return;

        if (hasResult)
        {
            if (isTie)
                currentMeeting.Outcome = "Meeting ended in a tie.";
            else if (exiled != null)
                currentMeeting.Outcome = $"{GetPlayerLabel(exiled)} was ejected.";
            else
                currentMeeting.Outcome = "Meeting was skipped; no player was ejected.";
        }
        else if (string.IsNullOrWhiteSpace(currentMeeting.Outcome))
        {
            currentMeeting.Outcome = "Meeting ended without a recorded result.";
        }

        currentMeeting.Finished = true;
        currentMeeting.FinishedAt = GetElapsedTime();
        currentMeeting = null;
    }

    public static void RecordTask(PlayerControl player, uint taskId)
    {
        if (!IsHost || !matchActive || player == null || player.Data == null)
            return;

        if (player.Data.Role != null &&
            player.Data.Role.TeamType == RoleTeamTypes.Impostor)
        {
            return;
        }

        PlayerTaskEntry playerEntry = GetOrCreateTaskPlayer(player);

        if (!playerEntry.CompletedTaskIds.Add(taskId))
            return;

        float elapsed = GetElapsedTime();

        playerEntry.Tasks.Add(new TaskEntry
        {
            CompletionTime = elapsed,
            TaskName = GetTaskName(player, taskId),
            Order = nextTaskEventOrder++
        });

        playerEntry.TotalTasks = Math.Max(playerEntry.TotalTasks, GetTotalTasks(player));
        RecordGlobalTaskProgress(GameData.Instance);

        if (playerEntry.TotalTasks > 0 &&
            playerEntry.Tasks.Count >= playerEntry.TotalTasks &&
            LoggedCompletedPlayers.Add(player.PlayerId))
        {
            TaskProgress.Add(new TimedTextEntry
            {
                Text = $"{GetPlayerLabel(player.Data)} completed all assigned tasks",
                Time = elapsed,
                Order = nextTaskEventOrder++
            });
        }
    }

    public static void RecordGlobalTaskProgress(GameData gameData)
    {
        if (!IsHost || !matchActive || gameData == null || gameData.TotalTasks <= 0)
            return;

        globalTotalTasks = gameData.TotalTasks;

        double percentage =
            (double)gameData.CompletedTasks / gameData.TotalTasks * 100d;

        while (nextTaskProgressThreshold <= 75 &&
               percentage >= nextTaskProgressThreshold)
        {
            TaskProgress.Add(new TimedTextEntry
            {
                Text =
                    $"Global task completion reached {nextTaskProgressThreshold}% " +
                    $"({gameData.CompletedTasks}/{gameData.TotalTasks} tasks)",
                Time = GetElapsedTime(),
                Order = nextTaskEventOrder++
            });

            nextTaskProgressThreshold += 25;
        }
    }

    public static void RecordMurder(
        PlayerControl killer,
        PlayerControl victim,
        MurderResultFlags resultFlags)
    {
        if (!IsHost ||
            !matchActive ||
            killer == null ||
            killer.Data == null ||
            victim == null ||
            victim.Data == null)
        {
            return;
        }

        float elapsed = GetElapsedTime();
        string location = GetPlayerLocation(victim);
        bool failedProtected =
            resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
            (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) &&
             victim.protectedByGuardianId > -1 &&
             !resultFlags.HasFlag(MurderResultFlags.Succeeded));

        if (failedProtected)
        {
            string protector = GetPlayerLabel((byte)victim.protectedByGuardianId);

            Protections.Add(new TimedTextEntry
            {
                Text =
                    $"{GetPlayerLabel(killer.Data)} attempted to kill " +
                    $"{GetPlayerLabel(victim.Data)}\n" +
                    $"   Protection provided by: {protector}\n" +
                    "   Result: Kill blocked",
                Location = location,
                Time = elapsed
            });

            return;
        }

        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded) ||
            !LoggedVictims.Add(victim.PlayerId))
        {
            return;
        }

        Kills.Add(new KillEntry
        {
            KillerId = killer.PlayerId,
            KillerLabel = GetPlayerLabel(killer.Data),
            VictimLabel = GetPlayerLabel(victim.Data),
            Location = location,
            Time = elapsed
        });

        if (currentMeeting != null)
            currentMeeting.AlivePlayers.Remove(victim.PlayerId);
    }

    public static void RecordProtection(PlayerControl protector, PlayerControl target)
    {
        if (!IsHost ||
            !matchActive ||
            protector == null ||
            protector.Data == null ||
            target == null ||
            target.Data == null)
        {
            return;
        }

        float elapsed = GetElapsedTime();
        string key = $"{protector.PlayerId}:{target.PlayerId}";

        if (lastProtectionKey == key && elapsed - lastProtectionTime < 0.5f)
            return;

        lastProtectionKey = key;
        lastProtectionTime = elapsed;

        Protections.Add(new TimedTextEntry
        {
            Text =
                $"{GetPlayerLabel(protector.Data)} protected " +
                GetPlayerLabel(target.Data),
            Time = elapsed
        });
    }

    public static void RecordVent(PlayerPhysics physics, int ventId, bool entered)
    {
        if (!IsHost ||
            !matchActive ||
            physics == null ||
            physics.myPlayer == null ||
            physics.myPlayer.Data == null)
        {
            return;
        }

        Vents.Add(new TimedTextEntry
        {
            Text =
                $"{GetPlayerLabel(physics.myPlayer.Data)} " +
                $"{(entered ? "entered" : "exited")} a vent",
            Location = GetVentName(ventId),
            Time = GetElapsedTime()
        });
    }

    public static void RecordSystemUpdate(
        PlayerControl player,
        SystemTypes systemType,
        byte amount)
    {
        if (!IsHost ||
            !matchActive ||
            player == null ||
            player.Data == null ||
            MeetingHud.Instance != null)
        {
            return;
        }

        SystemTypes sabotageSystem;

        if (systemType == SystemTypes.Sabotage)
        {
            sabotageSystem = (SystemTypes)amount;
        }
        else if (amount == 128 && IsSabotageSystem(systemType))
        {
            sabotageSystem = systemType;
        }
        else
        {
            return;
        }

        string sabotageName = GetSabotageName(sabotageSystem);

        if (activeSabotage == sabotageName)
            return;

        activeSabotage = sabotageName;

        Sabotages.Add(new TimedTextEntry
        {
            Text =
                $"{GetPlayerLabel(player.Data)} started " +
                $"{sabotageName} sabotage",
            Time = GetElapsedTime()
        });
    }

    public static void RecordSabotageFixed(PlayerTask task)
    {
        if (!IsHost || !matchActive || task == null || activeSabotage == null)
            return;

        if (!IsSabotageRepairTask(task.TaskType))
            return;

        Sabotages.Add(new TimedTextEntry
        {
            Text = $"{activeSabotage} sabotage was fixed",
            Time = GetElapsedTime()
        });

        activeSabotage = null;
    }

    public static void RecordNoisemaker(NoisemakerRole role)
    {
        if (!IsHost ||
            !matchActive ||
            role == null ||
            role.Player == null ||
            role.Player.Data == null)
        {
            return;
        }

        bool commsAffected =
            PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.AreCommsAffected();

        string playerLabel = GetPlayerLabel(role.Player.Data);

        NoisemakerEvents.Add(new TimedTextEntry
        {
            Text = commsAffected
                ? $"Comms sabotage prevented {playerLabel} from alerting the lobby about their death"
                : $"{playerLabel} alerted the lobby about their death",
            Time = GetElapsedTime()
        });
    }

    public static void RecordDisconnect(PlayerControl player, DisconnectReasons reason)
    {
        if (!IsHost ||
            !matchActive ||
            player == null ||
            player.Data == null ||
            !LoggedDisconnects.Add(player.PlayerId))
        {
            return;
        }

        Disconnects.Add(new DisconnectEntry
        {
            PlayerLabel = GetPlayerLabel(player.Data),
            Reason = FormatDisconnectReason(reason),
            Time = GetElapsedTime()
        });

        if (currentMeeting != null)
        {
            byte disconnectedId = player.PlayerId;
            string disconnectedColor = GetColorName(player.Data);

            currentMeeting.AlivePlayers.Remove(disconnectedId);

            foreach (VoteEntry vote in currentMeeting.Votes)
            {
                if (vote.Invalidated || vote.VotedForId != disconnectedId)
                    continue;

                vote.Invalidated = true;
                vote.Text +=
                    $" (invalidated: {disconnectedColor} disconnected)";

                currentMeeting.PlayersWhoVoted.Remove(vote.VoterId);
                currentMeeting.PendingRevoteTargets[vote.VoterId] =
                    disconnectedColor;
            }
        }
    }

    public static void SaveReport()
    {
        if (!IsHost || !matchActive || reportSaved)
            return;

        try
        {
            FinishMeeting();

            if (Roles.Count == 0)
                CaptureInitialRoles();

            CapturePlayersWithoutCompletedTasks();
            matchEndDate = DateTime.Now;

            string directory = GetLogDirectory();
            Directory.CreateDirectory(directory);

            string fileName = $"Match_{matchStartDate:yyyy-MM-dd_HH-mm-ss-fff}.txt";
            string filePath = Path.Combine(directory, fileName);

            File.WriteAllText(filePath, BuildReport(), new UTF8Encoding(false));

            reportSaved = true;
            matchActive = false;
            pendingReportPath = filePath;

            BMLogger.Info($"[MatchTextLogger] Match report saved: {filePath}");
        }
        catch (Exception exception)
        {
            BMLogger.Error(
                $"[MatchTextLogger] Failed to save report: {exception}",
                "MatchTextLogger"
            );
        }
    }

    public static string OpenLatestReport()
    {
        try
        {
            if (matchActive)
                return "The match report is available only after the match ends.";

            string directory = GetLogDirectory();

            if (!Directory.Exists(directory))
                return "No match logs were found.";

            string latestReport = Directory
                .GetFiles(directory, "Match_*.txt")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(latestReport))
                return "No match logs were found.";

            OpenReport(latestReport);
            return $"Opened match log: {Path.GetFileName(latestReport)}";
        }
        catch (Exception exception)
        {
            BMLogger.Error(
                $"[MatchTextLogger] Failed to open report: {exception}",
                "MatchTextLogger"
            );

            return "The match log could not be opened.";
        }
    }

    private static string BuildReport()
    {
        var text = new StringBuilder();
        float duration = GetElapsedTime();
        GameOverReason reason = EndGameResult.CachedGameOverReason;

        text.AppendLine("========================================");
        text.AppendLine("              MATCH REPORT");
        text.AppendLine("========================================");
        text.AppendLine();
        text.AppendLine($"Started: {matchStartDate:yyyy-MM-dd HH:mm:ss}");
        text.AppendLine($"Ended:   {matchEndDate:yyyy-MM-dd HH:mm:ss}");
        text.AppendLine($"Duration: {FormatTime(duration)}");
        text.AppendLine($"Map: {mapName}");
        text.AppendLine($"Game mode: {gameModeName}");
        text.AppendLine($"Result: {GetWinnerText(reason)}");
        text.AppendLine($"Win reason: {GetWinReason(reason)}");
        text.AppendLine();

        AppendRoleSummary(text);
        AppendTaskRanking(text);
        AppendMeetings(text);
        AppendKills(text);
        AppendTimedSection(text, "PROTECTIONS", Protections, "No protections were recorded.", "Location");
        AppendTimedSection(text, "SABOTAGES", Sabotages, "No sabotages were recorded.");
        AppendTimedSection(text, "VENTS", Vents, "No vent activity was recorded.", "Vent location");
        AppendTimedSection(text, "NOISEMAKER", NoisemakerEvents, "No Noisemaker events were recorded.");
        AppendDisconnections(text);
        AppendKillSummary(text);
        AppendTaskProgress(text);

        text.AppendLine("========================================");
        text.AppendLine("              END OF REPORT");
        text.AppendLine("========================================");

        return text.ToString();
    }

    private static void AppendRoleSummary(StringBuilder text)
    {
        AppendSectionHeader(text, "ROLE SUMMARY");

        if (Roles.Count == 0)
        {
            text.AppendLine("No role information was recorded.");
            text.AppendLine();
            return;
        }

        foreach (var role in Roles.OrderBy(entry => entry.ColorId).ThenBy(entry => entry.PlayerLabel))
            text.AppendLine($"{role.PlayerLabel} - {role.RoleName}");

        text.AppendLine();
    }

    private static void AppendTaskRanking(StringBuilder text)
    {
        AppendSectionHeader(text, "TASK RANKING");

        var ranking = PlayersTasks.Values
            .OrderByDescending(player => player.Tasks.Count)
            .ThenBy(player =>
                player.Tasks.Count > 0
                    ? player.Tasks[player.Tasks.Count - 1].CompletionTime
                    : float.MaxValue)
            .ThenBy(player => player.PlayerName)
            .ToList();

        if (ranking.Count == 0)
        {
            text.AppendLine("No players had assigned tasks.");
            text.AppendLine();
            return;
        }

        for (int i = 0; i < ranking.Count; i++)
        {
            PlayerTaskEntry player = ranking[i];
            string lastTaskTime = player.Tasks.Count > 0
                ? FormatTime(player.Tasks[player.Tasks.Count - 1].CompletionTime)
                : "--:--.---";

            text.AppendLine(
                $"{i + 1}. {player.ColorName} ({player.PlayerName}) - " +
                $"{player.Tasks.Count}/{player.TotalTasks} tasks - " +
                $"last task: {lastTaskTime}"
            );
        }

        text.AppendLine();
    }

    private static void AppendMeetings(StringBuilder text)
    {
        if (Meetings.Count == 0)
        {
            AppendSectionHeader(text, "MEETINGS");
            text.AppendLine("No meetings were held.");
            text.AppendLine();
            return;
        }

        foreach (var meeting in Meetings)
        {
            AppendSectionHeader(text, $"VOTING - MEETING {meeting.Number}");
            text.AppendLine($"Meeting started: {FormatTime(meeting.StartedAt)}");
            text.AppendLine($"Meeting called by: {meeting.CalledBy}");
            text.AppendLine($"Call type: {meeting.CallType}");

            if (!string.IsNullOrWhiteSpace(meeting.ReportedPlayer))
                text.AppendLine($"Reported body: {meeting.ReportedPlayer}");

            text.AppendLine();
            text.AppendLine(meeting.Number == 1
                ? "Players who died before this meeting:"
                : "Players who died since the previous meeting:");

            if (meeting.DeadPlayers.Count == 0)
                text.AppendLine("- No players died.");
            else
                foreach (string player in meeting.DeadPlayers)
                    text.AppendLine($"- {player}");

            text.AppendLine();
            text.AppendLine("Votes, in chronological order:");
            text.AppendLine();

            for (int i = 0; i < meeting.Votes.Count; i++)
                text.AppendLine($"{i + 1}. {meeting.Votes[i].Text}");

            var nonVoters = meeting.AlivePlayers
                .Where(player => !meeting.PlayersWhoVoted.Contains(player.Key))
                .OrderBy(player => player.Value)
                .ToList();

            if (meeting.Votes.Count == 0 && nonVoters.Count == 0)
                text.AppendLine("No living players were available to vote.");

            if (meeting.Votes.Count > 0 && nonVoters.Count > 0)
                text.AppendLine("----------------------------------------");

            foreach (var player in nonVoters)
                text.AppendLine($"   {player.Value} did not vote");

            text.AppendLine();
            text.AppendLine($"Meeting outcome: {meeting.Outcome}");
            text.AppendLine();
        }
    }

    private static void AppendKills(StringBuilder text)
    {
        AppendSectionHeader(text, "KILLS");

        if (Kills.Count == 0)
        {
            text.AppendLine("No successful kills were recorded.");
            text.AppendLine();
            return;
        }

        for (int i = 0; i < Kills.Count; i++)
        {
            KillEntry entry = Kills[i];
            text.AppendLine(
                $"{i + 1}. [{FormatTime(entry.Time)}] " +
                $"{entry.KillerLabel} killed {entry.VictimLabel}"
            );
            text.AppendLine($"   Location: {entry.Location}");
            text.AppendLine($"   {GetEventContext(entry.Time)}");
            text.AppendLine();
        }
    }

    private static void AppendTimedSection(
        StringBuilder text,
        string title,
        List<TimedTextEntry> entries,
        string emptyText,
        string locationLabel = null)
    {
        AppendSectionHeader(text, title);

        if (entries.Count == 0)
        {
            text.AppendLine(emptyText);
            text.AppendLine();
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            TimedTextEntry entry = entries[i];
            string[] lines = (entry.Text ?? "").Split('\n');

            text.AppendLine($"{i + 1}. [{FormatTime(entry.Time)}] {lines[0]}");

            for (int line = 1; line < lines.Length; line++)
                text.AppendLine(lines[line]);

            if (!string.IsNullOrWhiteSpace(entry.Location) &&
                !string.IsNullOrWhiteSpace(locationLabel))
            {
                text.AppendLine($"   {locationLabel}: {entry.Location}");
            }

            text.AppendLine($"   {GetEventContext(entry.Time)}");
            text.AppendLine();
        }
    }

    private static void AppendDisconnections(StringBuilder text)
    {
        AppendSectionHeader(text, "DISCONNECTIONS");

        if (Disconnects.Count == 0)
        {
            text.AppendLine("No players disconnected.");
            text.AppendLine();
            return;
        }

        for (int i = 0; i < Disconnects.Count; i++)
        {
            DisconnectEntry entry = Disconnects[i];
            text.AppendLine(
                $"{i + 1}. [{FormatTime(entry.Time)}] " +
                $"{entry.PlayerLabel} disconnected"
            );
            text.AppendLine($"   Reason: {entry.Reason}");
            text.AppendLine($"   {GetEventContext(entry.Time)}");
            text.AppendLine();
        }
    }

    private static void AppendKillSummary(StringBuilder text)
    {
        AppendSectionHeader(text, "KILL SUMMARY");

        if (Kills.Count == 0)
        {
            text.AppendLine("No successful kills were recorded.");
            text.AppendLine();
            return;
        }

        foreach (var killer in Kills
                     .GroupBy(entry => new { entry.KillerId, entry.KillerLabel })
                     .OrderByDescending(group => group.Count())
                     .ThenBy(group => group.Key.KillerLabel))
        {
            int count = killer.Count();
            text.AppendLine(
                $"{killer.Key.KillerLabel} - {count} " +
                $"{(count == 1 ? "kill" : "kills")}"
            );
        }

        text.AppendLine();
    }

    private static void AppendTaskProgress(StringBuilder text)
    {
        AppendSectionHeader(text, "TASK PROGRESS");

        int totalTasks = globalTotalTasks > 0
            ? globalTotalTasks
            : PlayersTasks.Values.Sum(player => player.TotalTasks);

        var timeline = new List<TaskTimelineEntry>
        {
            new()
            {
                Time = 0f,
                Order = int.MinValue,
                Text = $"Game Start\n   Global task completion reached 0% (0/{totalTasks} tasks)",
                IsGameStart = true
            }
        };

        foreach (PlayerTaskEntry player in PlayersTasks.Values)
        {
            for (int i = 0; i < player.Tasks.Count; i++)
            {
                TaskEntry task = player.Tasks[i];
                timeline.Add(new TaskTimelineEntry
                {
                    Time = task.CompletionTime,
                    Order = task.Order,
                    Text =
                        $"{player.ColorName} ({player.PlayerName}) completed " +
                        $"{task.TaskName} ({i + 1}/{player.TotalTasks})"
                });
            }
        }

        foreach (TimedTextEntry progress in TaskProgress)
        {
            timeline.Add(new TaskTimelineEntry
            {
                Time = progress.Time,
                Order = progress.Order,
                Text = progress.Text
            });
        }

        foreach (MeetingEntry meeting in Meetings)
        {
            timeline.Add(new TaskTimelineEntry
            {
                Time = meeting.StartedAt,
                Order = int.MinValue + meeting.Number,
                Text = $"{GetOrdinal(meeting.Number)} Meeting",
                IsMeeting = true
            });
        }

        var ordered = timeline
            .OrderBy(entry => entry.Time)
            .ThenBy(entry => entry.Order)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            TaskTimelineEntry entry = ordered[i];
            string[] lines = (entry.Text ?? "").Split('\n');

            text.AppendLine($"{i + 1}. [{FormatTime(entry.Time)}] {lines[0]}");

            for (int line = 1; line < lines.Length; line++)
                text.AppendLine(lines[line]);

            if (!entry.IsMeeting)
            {
                text.AppendLine(entry.IsGameStart
                    ? "   Before any meeting"
                    : $"   {GetTaskEventContext(entry.Time)}");
            }

            text.AppendLine();
        }
    }

    private static void AppendSectionHeader(StringBuilder text, string title)
    {
        text.AppendLine($"================ {title} ================");
        text.AppendLine();
    }

    private static PlayerTaskEntry GetOrCreateTaskPlayer(PlayerControl player)
    {
        if (PlayersTasks.TryGetValue(player.PlayerId, out PlayerTaskEntry entry))
            return entry;

        entry = new PlayerTaskEntry
        {
            PlayerId = player.PlayerId,
            PlayerName = CleanText(player.Data.PlayerName),
            ColorName = GetColorName(player.Data),
            TotalTasks = GetTotalTasks(player)
        };

        PlayersTasks[player.PlayerId] = entry;
        return entry;
    }

    private static void CapturePlayersWithoutCompletedTasks()
    {
        if (PlayerControl.AllPlayerControls == null)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null)
                continue;

            if (player.Data.Role != null &&
                player.Data.Role.TeamType == RoleTeamTypes.Impostor)
            {
                continue;
            }

            if (GetTotalTasks(player) <= 0)
                continue;

            GetOrCreateTaskPlayer(player);
        }
    }

    private static int GetTotalTasks(PlayerControl player)
    {
        return player?.Data?.Tasks == null ? 0 : player.Data.Tasks.Count;
    }

    private static string GetTaskName(PlayerControl player, uint taskId)
    {
        if (player?.myTasks != null)
        {
            foreach (PlayerTask task in player.myTasks)
            {
                if (task != null && task.Id == taskId)
                    return CleanText(task.TaskType.ToString());
            }
        }

        return $"Task {taskId}";
    }

    private static float GetElapsedTime()
    {
        return Math.Max(0f, Time.realtimeSinceStartup - matchStartTime);
    }

    private static string GetEventContext(float eventTime)
    {
        if (Meetings.Count == 0)
            return "Before any meeting";

        for (int i = 0; i < Meetings.Count; i++)
        {
            MeetingEntry meeting = Meetings[i];

            if (eventTime < meeting.StartedAt)
            {
                return i == 0
                    ? "Before Meeting 1"
                    : $"Between Meeting {Meetings[i - 1].Number} and Meeting {meeting.Number}";
            }

            float finishedAt = meeting.FinishedAt >= 0f
                ? meeting.FinishedAt
                : meeting.StartedAt;

            if (eventTime <= finishedAt)
                return $"During Meeting {meeting.Number}";
        }

        return $"After Meeting {Meetings[Meetings.Count - 1].Number}";
    }

    private static string GetTaskEventContext(float eventTime)
    {
        if (Meetings.Count == 0 || eventTime < Meetings[0].StartedAt)
            return "Before any meeting";

        return GetEventContext(eventTime);
    }

    private static string GetOrdinal(int number)
    {
        return number switch
        {
            1 => "First",
            2 => "Second",
            3 => "Third",
            4 => "Fourth",
            5 => "Fifth",
            6 => "Sixth",
            7 => "Seventh",
            8 => "Eighth",
            9 => "Ninth",
            10 => "Tenth",
            _ => $"{number}th"
        };
    }

    private static string GetPlayerLabel(byte playerId)
    {
        var player = GameData.Instance?.GetPlayerById(playerId);
        return player == null ? $"Player {playerId}" : GetPlayerLabel(player);
    }

    private static string GetPlayerLabel(NetworkedPlayerInfo player)
    {
        if (player == null)
            return "Unknown";

        return $"{GetColorName(player)} ({CleanText(player.PlayerName)})";
    }

    private static string GetRoleName(PlayerControl player)
    {
        if (player == null || player.Data == null)
            return "Unknown";

        byte playerId = player.PlayerId;

        if (Jester.JesterId != byte.MaxValue && playerId == Jester.JesterId)
            return "Jester";
        if (Guesser.SpecialKillerId != byte.MaxValue && playerId == Guesser.SpecialKillerId)
            return "Guesser";
        if (Exiler.ExilerId != byte.MaxValue && playerId == Exiler.ExilerId)
            return "Exiler";
        if (ImmortalManager.ImmortalPlayerId.HasValue &&
            playerId == ImmortalManager.ImmortalPlayerId.Value)
        {
            return "Immortal";
        }

        try
        {
            if (player.Data.Role != null)
                return player.Data.Role.Role.ToString();
        }
        catch
        {
        }

        return "Unknown";
    }

    private static string GetPlayerLocation(PlayerControl player)
    {
        try
        {
            if (player != null && BanMod.RoomZoneManagerInstance != null)
            {
                var room = BanMod.RoomZoneManagerInstance.GetCurrentRoom(
                    player.GetTruePosition(),
                    GetCurrentMap()
                );

                if (room != null && !string.IsNullOrWhiteSpace(room.RoomName))
                    return room.RoomName;
            }
        }
        catch
        {
        }

        return "Outside / hallway";
    }

    private static string GetVentName(int ventId)
    {
        Dictionary<int, string> vents = GetCurrentMap() switch
        {
            MapNames.Skeld => SkeldVents,
            MapNames.MiraHQ => MiraVents,
            MapNames.Polus => PolusVents,
            MapNames.Airship => AirshipVents,
            MapNames.Fungle => FungleVents,
            _ => null
        };

        return vents != null && vents.TryGetValue(ventId, out string name)
            ? name
            : $"Unknown Vent {ventId}";
    }

    private static bool IsSabotageSystem(SystemTypes systemType)
    {
        return systemType == SystemTypes.Reactor ||
               systemType == SystemTypes.Laboratory ||
               systemType == SystemTypes.Electrical ||
               systemType == SystemTypes.LifeSupp ||
               systemType == SystemTypes.Comms ||
               systemType == SystemTypes.HeliSabotage ||
               systemType == SystemTypes.MushroomMixupSabotage;
    }

    private static string GetSabotageName(SystemTypes systemType)
    {
        return systemType switch
        {
            SystemTypes.Reactor => "Reactor",
            SystemTypes.Laboratory => "Reactor",
            SystemTypes.Electrical => "Lights",
            SystemTypes.LifeSupp => "Oxygen",
            SystemTypes.Comms => "Comms",
            SystemTypes.HeliSabotage => "Helicopter",
            SystemTypes.MushroomMixupSabotage => "Mushroom Mixup",
            _ => $"Unknown ({(byte)systemType})"
        };
    }

    private static bool IsSabotageRepairTask(TaskTypes taskType)
    {
        return taskType == TaskTypes.ResetReactor ||
               taskType == TaskTypes.ResetSeismic ||
               taskType == TaskTypes.RestoreOxy ||
               taskType == TaskTypes.FixComms ||
               taskType == TaskTypes.FixLights ||
               taskType == TaskTypes.StopCharles ||
               taskType == TaskTypes.MushroomMixupSabotage;
    }

    private static MapNames GetCurrentMap()
    {
        try
        {
            return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
        }
        catch
        {
            return (MapNames)(-1);
        }
    }

    private static string GetMapName()
    {
        return GetCurrentMap() switch
        {
            MapNames.Skeld => "The Skeld",
            MapNames.MiraHQ => "MIRA HQ",
            MapNames.Polus => "Polus",
            MapNames.Airship => "The Airship",
            MapNames.Fungle => "The Fungle",
            _ => GetCurrentMap().ToString()
        };
    }

    private static string GetGameModeName()
    {
        try
        {
            return (Options.GameMode.Selected).ToString();
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string GetWinnerText(GameOverReason reason)
    {
        if (JesterWinState.IsActive())
            return $"{GetPlayerLabel(JesterWinState.WinnerId)} won as Jester";

        try
        {
            return GameManager.Instance != null && GameManager.Instance.DidHumansWin(reason)
                ? "Crewmates won"
                : "Impostors won";
        }
        catch
        {
            return reason.ToString();
        }
    }

    private static string GetWinReason(GameOverReason reason)
    {
        if (JesterWinState.IsActive())
            return "The Jester was ejected";

        return reason switch
        {
            GameOverReason.CrewmatesByVote => "All Impostors were ejected",
            GameOverReason.CrewmatesByTask => "All required tasks were completed",
            GameOverReason.ImpostorsByVote => "A Crewmate was ejected",
            GameOverReason.ImpostorsByKill => "Impostors reached kill parity",
            GameOverReason.HideAndSeek_ImpostorsByKills => "Impostors eliminated the Crewmates",
            GameOverReason.ImpostorsBySabotage => "A critical sabotage was not fixed",
            GameOverReason.CrewmateDisconnect => "A Crewmate disconnected",
            GameOverReason.ImpostorDisconnect => "An Impostor disconnected",
            GameOverReason.HideAndSeek_CrewmatesByTimer => "Crewmates survived until the timer expired",
            _ => reason.ToString()
        };
    }

    private static string FormatDisconnectReason(DisconnectReasons reason)
    {
        string value = reason.ToString();

        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        var result = new StringBuilder();

        for (int i = 0; i < value.Length; i++)
        {
            if (i > 0 && char.IsUpper(value[i]) && !char.IsUpper(value[i - 1]))
                result.Append(' ');

            result.Append(value[i]);
        }

        return result.ToString();
    }

    private static int GetColorId(NetworkedPlayerInfo player)
    {
        try
        {
            return player?.DefaultOutfit == null ? int.MaxValue : player.DefaultOutfit.ColorId;
        }
        catch
        {
            return int.MaxValue;
        }
    }

    private static string GetColorName(NetworkedPlayerInfo player)
    {
        int colorId = GetColorId(player);
        return colorId == int.MaxValue
            ? CleanText(player?.PlayerName ?? "Unknown")
            : GetEnglishColorName(colorId);
    }

    private static string GetEnglishColorName(int colorId)
    {
        return colorId switch
        {
            0 => "Red",
            1 => "Blue",
            2 => "Green",
            3 => "Pink",
            4 => "Orange",
            5 => "Yellow",
            6 => "Black",
            7 => "White",
            8 => "Purple",
            9 => "Brown",
            10 => "Cyan",
            11 => "Lime",
            12 => "Maroon",
            13 => "Rose",
            14 => "Banana",
            15 => "Gray",
            16 => "Tan",
            17 => "Coral",
            18 => "Hidden",
            _ => $"Color {colorId}"
        };
    }

    private static string CleanText(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Unknown"
            : value.Replace("\r", " ").Replace("\n", " ").Trim();
    }

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int remainingSeconds = (int)(seconds % 60f);
        int milliseconds = (int)((seconds * 1000f) % 1000f);

        return $"{minutes:D2}:{remainingSeconds:D2}.{milliseconds:D3}";
    }

    private static string GetLogDirectory()
    {
        const string MatchPath = "./DATA/LOG/";
        return Path.Combine(MatchPath, "BanMod Match Logs");
    }

    private static void OpenReport(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                }
            );
        }
        catch (Exception exception)
        {
            BMLogger.Error(
                $"[MatchTextLogger] Report was saved but could not be opened: {exception}",
                "MatchTextLogger"
            );
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
internal static class MatchTextLoggerStartPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (!FakeMapLobbyUtility.Active && BanMod.EnableMatchLog.Value)
            MatchTextLogger.StartMatch();
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
internal static class MatchTextLoggerRolePatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && BanMod.EnableMatchLog.Value)
        {
            LateTask.New(
                MatchTextLogger.CaptureInitialRoles,
                2f,
                "Capture match logger roles",
                false
            );
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
internal static class MatchTextLoggerTaskPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] uint idx)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordTask(__instance, idx);
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
internal static class MatchTextLoggerMeetingStartPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        MeetingHud __instance,
        [HarmonyArgument(0)] NetworkedPlayerInfo reporter,
        [HarmonyArgument(1)] NetworkedPlayerInfo reportedBody,
        [HarmonyArgument(2)] Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.StartMeeting(__instance, reporter, reportedBody, deadBodies);
        }
    }
}


[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
internal static class MatchTextLoggerMeetingVotePollPatch
{
    [HarmonyPostfix]
    private static void Postfix(MeetingHud __instance)
    {
        if (!BanMod.EnableMatchLog.Value ||
            __instance == null ||
            __instance.playerStates == null)
        {
            return;
        }

        foreach (var voteArea in __instance.playerStates)
        {
            if (!voteArea)
                continue;

            byte votedForId = voteArea.VotedForId;

            if (votedForId == PlayerVoteArea.HasNotVoted ||
                votedForId == PlayerVoteArea.MissedVote ||
                votedForId == PlayerVoteArea.DeadVote)
            {
                continue;
            }

            MatchTextLogger.RecordVote(
                __instance,
                voteArea.PlayerId.Value
            );
        }
    }
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetVote))]
internal static class MatchTextLoggerVotePatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerVoteArea __instance)
    {
        if (!BanMod.EnableMatchLog.Value ||
            !__instance ||
            !__instance.Parent)
        {
            return;
        }

        MatchTextLogger.RecordVote(
            __instance.Parent,
            __instance.PlayerId.Value
        );
    }
}

[HarmonyPatch(typeof(MeetingHud), "VotingComplete")]
internal static class MatchTextLoggerVotingCompletePatch
{
    [HarmonyPrefix]
    private static void Prefix(
        [HarmonyArgument(0)]
        Il2CppStructArray<MeetingHud.VoterState> states)
    {
        if (!BanMod.EnableMatchLog.Value)
            return;

        if (states == null)
            return;

        MatchTextLogger.RecordFinalVotes(states);
    }

    [HarmonyPostfix]
    private static void Postfix(
        [HarmonyArgument(1)] NetworkedPlayerInfo exiled,
        [HarmonyArgument(2)] bool isTie)
    {
        if (!BanMod.EnableMatchLog.Value)
            return;

        MatchTextLogger.FinishMeeting(
            exiled,
            isTie,
            true
        );
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
internal static class MatchTextLoggerMeetingDestroyPatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.FinishMeeting();
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class MatchTextLoggerMurderPatch
{
    [HarmonyPrefix]
    private static void Prefix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target,
        [HarmonyArgument(1)] MurderResultFlags resultFlags)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordMurder(__instance, target, resultFlags);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ProtectPlayer))]
internal static class MatchTextLoggerProtectionPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordProtection(__instance, target);
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
internal static class MatchTextLoggerEnterVentPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        PlayerPhysics __instance,
        [HarmonyArgument(0)] int id)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordVent(__instance, id, true);
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
internal static class MatchTextLoggerExitVentPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        PlayerPhysics __instance,
        [HarmonyArgument(0)] int id)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordVent(__instance, id, false);
        }
    }
}

[HarmonyPatch]
internal static class MatchTextLoggerSystemPatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(ShipStatus),
            "UpdateSystem",
            new[]
            {
                typeof(SystemTypes),
                typeof(PlayerControl),
                typeof(Hazel.MessageReader)
            }
        );
    }

    [HarmonyPrefix]
    private static void Prefix(
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] Hazel.MessageReader reader)
    {
        if (reader == null)
            return;
        if (!BanMod.EnableMatchLog.Value) return;

        int position = reader.Position;
        byte amount = reader.ReadByte();
        reader.Position = position;

        MatchTextLogger.RecordSystemUpdate(player, systemType, amount);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveTask))]
internal static class MatchTextLoggerSabotageFixedPatch
{
    [HarmonyPostfix]
    private static void Postfix([HarmonyArgument(0)] PlayerTask task)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordSabotageFixed(task);
        }
    }
}

[HarmonyPatch(typeof(NoisemakerRole), nameof(NoisemakerRole.NotifyOfDeath))]
internal static class MatchTextLoggerNoisemakerPatch
{
    [HarmonyPostfix]
    private static void Postfix(NoisemakerRole __instance)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordNoisemaker(__instance);
        }
    }
}

[HarmonyPatch(
    typeof(GameData),
    nameof(GameData.HandleDisconnect),
    typeof(PlayerControl),
    typeof(DisconnectReasons))]
internal static class MatchTextLoggerDisconnectPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        [HarmonyArgument(0)] PlayerControl player,
        [HarmonyArgument(1)] DisconnectReasons reason)
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.RecordDisconnect(player, reason);
        }
    }
}

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
internal static class MatchTextLoggerEndGamePatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (BanMod.EnableMatchLog.Value)
        {
            MatchTextLogger.SaveReport();
        }
    }
}


[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal static class MatchTextLoggerChatCommandPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(ChatController __instance)
    {
        if (__instance == null ||
            __instance.freeChatField == null ||
            __instance.freeChatField.textArea == null)
        {
            return true;
        }
        if (!BanMod.EnableMatchLog.Value)
        {
            return true;
        }
        string text = __instance.freeChatField.textArea.text;

        if (!string.Equals(
                text?.Trim(),
                "/matchlog",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        __instance.freeChatField.textArea.SetText("");
        ChatCommands.ShowChat(MatchTextLogger.OpenLatestReport());
        return false;
    }
}