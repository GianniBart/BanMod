using System;
using BepInEx.Unity.IL2CPP.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace BanMod
{
    public static partial class Utils
    {
        public static void sondaggio(string domanda, string rispostaA, string rispostaB, string rispostaC,
            Action<PollResult> alTermine = null, float durataSecondi = float.PositiveInfinity)
        {
            BanModPoll.Start(domanda, new[] { rispostaA, rispostaB, rispostaC }, alTermine, durataSecondi);
        }

        // Variable-length API; the previous three-answer overload remains compatible.
        public static void sondaggio(string domanda, params string[] risposte)
        {
            BanModPoll.Start(domanda, risposte, null, float.PositiveInfinity);
        }

        public static void sondaggio(string domanda, string[] risposte,
            Action<PollResult> alTermine, float durataSecondi = float.PositiveInfinity)
        {
            BanModPoll.Start(domanda, risposte, alTermine, durataSecondi);
        }

        public static bool ChiudiSondaggio() => BanModPoll.Finish();
        public static bool AnnullaSondaggio() => BanModPoll.Cancel();
        public static bool SondaggioAttivo => BanModPoll.Active;
        public static bool PubblicaRisultatoSondaggio() => BanModPoll.PublishLast();

        // Both actions are tied to the current meeting, never to a later one.
        public static bool SondaggioEspulsione(PlayerControl target)
        {
            var meeting = MeetingHud.Instance;
            if (meeting == null || target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
            {
                ChatCommands.ShowChat(PollText.Get("PollNeedMeeting", "This action requires an active meeting and a living target."));
                return false;
            }
            uint netId = target.NetId;
            byte playerId = target.PlayerId;
            int ownerId = target.GetClientId();
            string name = PollText.Plain(target.Data.PlayerName ?? "Player");
            if (name.Length > 24) name = name.Substring(0, 24);
            return BanModPoll.StartDecision(
                PollText.Format("PollEjectQuestion", "Eject {0} immediately?", name),
                () => target != null && target.Data != null && !target.Data.IsDead && !target.Data.Disconnected &&
                    target.NetId == netId && target.PlayerId == playerId && target.GetClientId() == ownerId,
                () =>
                {
                    if (GameData.Instance == null) return;
                    var info = GameData.Instance.GetPlayerById(playerId);
                    if (info == null) return;
                    VoteContextManager.IsForcedVote = true;
                    try
                    {
                        meeting.RpcVotingComplete(new MeetingHud.VoterState[0], info, false, false, 0);
                        meeting.RpcClose();
                        meeting.Close();
                    }
                    finally { VoteContextManager.IsForcedVote = false; }
                });
        }

        public static bool SondaggioChiusuraMeeting(MeetingHud meeting, bool closeVotingOnly = false)
        {
            if (meeting == null || meeting != MeetingHud.Instance) return false;
            string question = closeVotingOnly
                ? PollText.Get("PollCloseVoteQuestion", "End voting immediately?")
                : PollText.Get("PollCloseMeetingQuestion", "Close this meeting immediately?");
            return BanModPoll.StartDecision(question, () => meeting != null && meeting == MeetingHud.Instance,
                () =>
                {
                    if (closeVotingOnly) MeetingVoteCloser.CloseVoteNow();
                    else CloseMeetingApproved(meeting);
                });
        }
        // Called only by the successful majority callback. Never starts another poll.
        private static void CloseMeetingApproved(MeetingHud meeting)
        {
            if (!BanModPoll.IsHost || meeting == null || meeting != MeetingHud.Instance)
                return;
            bool previousForcedVote = VoteContextManager.IsForcedVote;
            VoteContextManager.IsForcedVote = true;
            try
            {
                meeting.RpcVotingComplete(new MeetingHud.VoterState[0], null, true, false, 0);
                meeting.Close();
                meeting.RpcClose();
            }
            finally { VoteContextManager.IsForcedVote = previousForcedVote; }
            GuessManager.CleanupAfterMeeting();
            ExilerManager.CleanupAfterMeeting();
            CloseMeetingManager.CleanupAfterMeeting();
        }

    }

    internal static class PollText
    {
        internal static string Get(string key, string fallback)
        {
            string value;
            try { value = Translator.GetString(key); }
            catch { return fallback; }
            return string.IsNullOrWhiteSpace(value) || value == key || value.Contains("<INVALID") ? fallback : value;
        }
        internal static string Format(string key, string fallback, params object[] args)
        {
            try { return string.Format(Get(key, fallback), args); }
            catch (FormatException) { return string.Format(fallback, args); }
        }
        internal static string Plain(string value) => Regex.Replace(value ?? "", "<[^>]*>", "")
            .Replace('\r', ' ').Replace('\n', ' ').Trim();
    }

    public sealed class PollResult
    {
        public string Domanda { get; }
        public ReadOnlyCollection<string> Risposte { get; }
        public ReadOnlyCollection<int> Voti { get; }
        public int Partecipanti { get; }
        public int TotaleVoti => Voti.Sum();
        public int Astenuti => Partecipanti - TotaleVoti;
        public int SogliaSi => Partecipanti / 2 + 1;
        public bool MaggioranzaAssoluta { get; }
        public bool Approvato => MaggioranzaAssoluta && Voti[0] >= SogliaSi;
        public int Vincitore { get; }
        public bool Pareggio { get; }
        internal PollResult(string question, string[] options, int[] votes, int participants, bool majority)
        {
            Domanda = question;
            Risposte = Array.AsReadOnly((string[])options.Clone());
            Voti = Array.AsReadOnly((int[])votes.Clone());
            Partecipanti = participants;
            MaggioranzaAssoluta = majority;
            int max = votes.Max();
            Pareggio = max > 0 && votes.Count(n => n == max) > 1;
            Vincitore = max == 0 || Pareggio ? -1 : Array.IndexOf(votes, max);
        }
    }

    internal static class BanModPoll
    {
        private sealed class Elector
        {
            public uint NetId;
            public byte PlayerId;
            public int Vote = -1;
        }
        private sealed class Session
        {
            public int GameId;
            public uint HostNetId;
            public bool InLobby;
            public MeetingHud Meeting;
            public string Question;
            public string[] Options;
            public float Deadline;
            public bool Majority;
            public Func<bool> ActionValid;
            public readonly Dictionary<int, Elector> Electors = new();
            public Action<PollResult> Callback;
        }
        private static Session _session;
        private static PollResult _last;
        private static int _lastGameId;
        private static uint _lastHostNetId;
        internal static bool Active => _session != null;
        internal static bool IsHost => !BanMod.IsBanModDisabled && AmongUsClient.Instance != null &&
            AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameId != 0 && PlayerControl.LocalPlayer != null;
        private static bool Eligible(PlayerControl player) => player != null && player.Data != null &&
            !player.Data.Disconnected && !player.Data.IsDead;
        private static bool SameContext(Session s) => IsHost && s.GameId == AmongUsClient.Instance.GameId &&
            s.HostNetId == PlayerControl.LocalPlayer.NetId && s.InLobby == GameStates.isLobby && s.Meeting == MeetingHud.Instance;

        internal static void Start(string question, string[] options, Action<PollResult> callback, float seconds,
            bool majority = false, Func<bool> actionValid = null)
        {
            if (!IsHost) throw new InvalidOperationException(PollText.Get("PollHostOnly", "Only the host can start a poll."));
            if (_session != null && !SameContext(_session)) _session = null;
            if (_session != null) throw new InvalidOperationException(PollText.Get("PollAlreadyActive", "A poll is already active."));
            bool unlimited = !majority && float.IsPositiveInfinity(seconds);
            if (!unlimited && (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 5 || seconds > 600))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (options == null || options.Length < 2 || (majority && options.Length != 2))
                throw new ArgumentException(PollText.Get("PollNeedAnswers", "Enter a question and at least two answers separated by /."));
            var s = new Session
            {
                GameId = AmongUsClient.Instance.GameId,
                HostNetId = PlayerControl.LocalPlayer.NetId,
                InLobby = GameStates.isLobby,
                Meeting = MeetingHud.Instance,
                Question = Clean(question),
                Options = options.Select(Clean).ToArray(),
                Deadline = Time.realtimeSinceStartup + seconds,
                Callback = callback,
                Majority = majority,
                ActionValid = actionValid
            };
            foreach (var player in BanMod.AllPlayerControls)
            {
                if (!Eligible(player)) continue;
                int clientId = player.GetClientId();
                if (clientId >= 0) s.Electors[clientId] = new Elector { NetId = player.NetId, PlayerId = player.PlayerId };
            }
            if (s.Electors.Count == 0) throw new InvalidOperationException(PollText.Get("PollNoPlayers", "No players are available to vote."));
            _session = s;
            var lines = new List<string> { "BanMod Poll: " + s.Question };
            for (int i = 0; i < s.Options.Length; i++) lines.Add($"{OptionLabel(i)}) {s.Options[i]}");
            lines.Add(majority
                ? PollText.Format("PollYesNoInstructions", "Vote 1 = YES or 2 = NO. {0} YES votes required. Time: {1}s.", s.Electors.Count / 2 + 1, seconds)
                : unlimited
                    ? PollText.Get("PollManualInstructions", "Vote with the answer number. Ends when everyone votes or the host uses /endpoll.")
                    : PollText.Format("PollInstructions", "Vote with the answer number. One vote each. Time: {0}s.", seconds));
            Broadcast(string.Join("\n", lines));
        }

        internal static bool StartDecision(string question, Func<bool> valid, Action action)
        {
            try
            {
                if (MeetingHud.Instance == null) throw new InvalidOperationException(PollText.Get("PollNeedMeeting", "This action requires an active meeting and a living target."));
                Start(question, new[] { PollText.Get("YesButtonText", "YES"), PollText.Get("NoButtonText", "NO") },
                    result => { if (result.Approvato && valid()) action(); }, 45f, true, valid);
                return true;
            }
            catch (Exception ex) { ChatCommands.ShowChat(ex.Message); return false; }
        }
        private static string Clean(string value)
        {
            value = (value ?? "").Trim();
            if (value.Length == 0 || value.Length > 90 || value.Any(char.IsControl) || value.Contains('<') || value.Contains('>'))
                throw new ArgumentException(PollText.Get("PollInvalidText", "Use 1-90 characters per question/answer, without newlines or tags."));
            return value;
        }
        private static Dictionary<int, PlayerControl> Connected(Session s)
        {
            var result = new Dictionary<int, PlayerControl>();
            foreach (var player in BanMod.AllPlayerControls)
            {
                if (!Eligible(player)) continue;
                int id = player.GetClientId();
                if (s.Electors.TryGetValue(id, out var e) && e.NetId == player.NetId && e.PlayerId == player.PlayerId)
                    result[id] = player;
            }
            return result;
        }
        internal static void Tick()
        {
            var s = _session;
            if (s == null) return;
            if (!SameContext(s) || (s.ActionValid != null && !s.ActionValid())) { CancelInternal(); return; }
            int yes = s.Electors.Values.Count(e => e.Vote == 0);
            var connected = Connected(s);
            int remaining = s.Electors.Count(pair => pair.Value.Vote < 0 && connected.ContainsKey(pair.Key));
            int threshold = s.Electors.Count / 2 + 1; // Frozen electorate: leaving never lowers this threshold.
            if (s.Majority && (yes >= threshold || yes + remaining < threshold)) { Finish(); return; }
            // Manual polls have no deadline: close only after every original voter answered,
            // or through the host's /endpoll command. Leaving does not count as voting.
            bool everyoneVoted = s.Electors.Values.All(e => e.Vote >= 0);
            if (Time.realtimeSinceStartup >= s.Deadline || everyoneVoted ||
                (!float.IsPositiveInfinity(s.Deadline) && remaining == 0)) Finish();
        }
        internal static void Receive(PlayerControl player, string text)
        {
            var s = _session;
            if (s == null || !SameContext(s) || !Eligible(player) || Time.realtimeSinceStartup >= s.Deadline) return;
            text = (text ?? "").Trim();
            int choice = -1;
            if (int.TryParse(text, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int number) && number > 0 && number <= s.Options.Length)
                choice = number - 1;
            if (s.Majority)
            {
                if (text.Equals(s.Options[0], StringComparison.OrdinalIgnoreCase) ||
                    text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text.Equals("si", StringComparison.OrdinalIgnoreCase) ||
                    text.Equals("sì", StringComparison.OrdinalIgnoreCase)) choice = 0;
                if (text.Equals(s.Options[1], StringComparison.OrdinalIgnoreCase) || text.Equals("no", StringComparison.OrdinalIgnoreCase)) choice = 1;
            }
            if (choice < 0 || choice >= s.Options.Length) return;
            if (!s.Electors.TryGetValue(player.GetClientId(), out var elector) || elector.NetId != player.NetId ||
                elector.PlayerId != player.PlayerId || elector.Vote >= 0) return;
            elector.Vote = choice;
        }
        internal static bool Finish()
        {
            var s = _session;
            if (s == null) return false;
            _session = null; // Before callback: action may close the meeting or start another poll.
            if (!SameContext(s) || (s.ActionValid != null && !s.ActionValid())) return false;
            int[] votes = new int[s.Options.Length];
            foreach (var e in s.Electors.Values) if (e.Vote >= 0) votes[e.Vote]++;
            var result = new PollResult(s.Question, s.Options, votes, s.Electors.Count, s.Majority);
            _last = result; _lastGameId = s.GameId; _lastHostNetId = s.HostNetId;
            // Decision outcome is public; ordinary polls retain their local report until /pollresult.
            if (s.Majority) Report(result, true);
            else ChatCommands.ShowChat(string.Join("\n", ReportLines(result)));
            try { s.Callback?.Invoke(result); }
            catch (Exception ex) { Debug.LogError("[BANMOD][POLL] Result action failed: " + ex); }
            return true;
        }
        internal static bool Cancel()
        {
            if (_session == null || !IsHost) return false;
            CancelInternal(); return true;
        }
        private static void CancelInternal() { _session = null; }
        internal static void OnMeetingCompleted(MeetingHud meeting)
        {
            if (_session != null && _session.Meeting == meeting) CancelInternal();
        }
        internal static bool EndManual()
        {
            if (!IsHost) return false;
            if (_session == null || _session.Majority || !SameContext(_session))
            {
                ChatCommands.ShowChat(PollText.Get("PollNoManualActive", "There is no active manual poll to end."));
                return false;
            }
            return Finish();
        }

        internal static bool PublishLast()
        {
            if (!IsHost) { ChatCommands.ShowChat(PollText.Get("PollHostOnly", "Only the host can start a poll.")); return false; }
            if (_last == null || _lastGameId != AmongUsClient.Instance.GameId || _lastHostNetId != PlayerControl.LocalPlayer.NetId)
            { ChatCommands.ShowChat(PollText.Get("PollNoResult", "No completed poll is available in this lobby.")); return false; }
            Report(_last, true); return true;
        }
        private static string OptionLabel(int index) =>
            (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        private static IEnumerable<string> ReportLines(PollResult r)
        {
            yield return "BanMod Poll: " + r.Domanda;
            for (int i = 0; i < r.Voti.Count; i++) yield return $"{OptionLabel(i)}) {r.Risposte[i]}: {r.Voti[i]}";
            yield return PollText.Format("PollVoteCount", "Votes: {0}/{1}. Abstentions: {2}.", r.TotaleVoti, r.Partecipanti, r.Astenuti);
            if (r.MaggioranzaAssoluta)
                yield return PollText.Format(r.Approvato ? "PollApproved" : "PollRejected",
                    r.Approvato ? "Approved: {0} YES votes required." : "Not approved: {0} YES votes required.", r.SogliaSi);
            else yield return r.Vincitore >= 0
                ? PollText.Format("PollWinner", "Winner: {0}.", OptionLabel(r.Vincitore))
                : PollText.Get(r.Pareggio ? "PollTie" : "PollNoVotes", r.Pareggio ? "Tie." : "No votes.");
        }
        private static void Report(PollResult r, bool broadcast)
        {
            if (broadcast) Broadcast(string.Join("\n", ReportLines(r)));
        }
        private static void Broadcast(string text)
        {
            // Preserve our line breaks, remove square brackets and keep vanilla's limit.
            text = Regex.Replace(text ?? "", "<[^>]*>", "")
                .Replace("\r\n", "\n").Replace('\r', '\n')
                .Replace("[", "").Replace("]", "").Trim();
            text = Regex.Replace(text, @"[0-9]{4,}",
                match => Utils.ToFullWidthNumbers(match.Value));
            while (text.Length > 120)
            {
                // Prefer complete lines, then words. Never split a UTF-16 surrogate pair.
                int split = text.LastIndexOf('\n', 119, 120);
                if (split < 1) split = text.LastIndexOf(' ', 119, 120);
                if (split < 1) split = 120;
                if (char.IsHighSurrogate(text[split - 1]) && char.IsLowSurrogate(text[split])) split--;
                Utils.SendMessage(text.Substring(0, split).TrimEnd());
                text = text.Substring(split).TrimStart();
            }
            if (text.Length > 0) Utils.SendMessage(text);
        }

    }

    internal static class BanModPollCommands
    {
        internal static bool TryHandle(string text)
        {
            string command = (text ?? "").TrimStart().Split(new[] { ' ', '\t', '\r', '\n' }, 2)[0];
            bool start = command.Equals("/poll", StringComparison.OrdinalIgnoreCase);
            bool end = command.Equals("/endpoll", StringComparison.OrdinalIgnoreCase);
            if (!start && !end && !command.Equals("/pollresult", StringComparison.OrdinalIgnoreCase)) return false;
            if (!BanModPoll.IsHost) { ChatCommands.ShowChat(PollText.Get("PollHostOnly", "Only the host can start a poll.")); return true; }
            try
            {
                string body = text.Trim().Substring(command.Length).Trim();
                if (end)
                {
                    if (body.Length != 0) throw new ArgumentException("/endpoll");
                    BanModPoll.EndManual();
                }
                else if (!start)
                {
                    if (body.Length != 0) throw new ArgumentException("/pollresult");
                    BanModPoll.PublishLast();
                }
                else
                {
                    string[] fields = body.Split('/').Select(value => value.Trim()).ToArray();
                    if (fields.Length < 3 || fields.Any(string.IsNullOrWhiteSpace))
                        throw new ArgumentException(PollText.Get("PollUsage",
                            "Usage: /poll question/answer 1/answer 2/..."));
                    Utils.sondaggio(fields[0], fields.Skip(1).ToArray());
                }
            }
            catch (Exception ex) { ChatCommands.ShowChat(ex.Message); }
            return true;
        }

    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    internal static class BanModPollChatPatch
    {
        [HarmonyPostfix]
        // Postfix observer only: ordinary chat and votes remain visible and input stays enabled.
        private static void Postfix(PlayerControl sourcePlayer, string chatText) => BanModPoll.Receive(sourcePlayer, chatText);
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal static class BanModPollUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix() => BanModPoll.Tick();
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.RpcVotingComplete))]
    internal static class BanModPollMeetingCompletePatch
    {
        [HarmonyPrefix]
        private static void Prefix(MeetingHud __instance) => BanModPoll.OnMeetingCompleted(__instance);
    }
}
