//credits and licenses in the resources folder/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace BanMod
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    public static class BanModMessagePoller
    {
        private static float _nextMessagePollTime = 0f;
        private static float _nextReportPollTime = 0f;
        private static bool _pollRunning = false;
        private static bool _pollMessagesRequested = false;
        private static bool _pollReportsRequested = false;
        private const float FastReportPollIntervalSeconds = 15f;

        private const double DefaultMessageAckCooldownSeconds = 24d * 60d * 60d;
        private const string LocalAckPrefsPrefix = "BANMOD_COMM_MESSAGE_ACK_";
        private const string LocalReportReplyAckPrefsPrefix = "BANMOD_REPORT_REPLY_ACK_";

        private static readonly HashSet<string> _visibleMessageKeys = new HashSet<string>();
        private static readonly HashSet<string> _visibleReportReplyKeys = new HashSet<string>();
        private static readonly HashSet<string> _unreadMessageKeys = new HashSet<string>();
        private static readonly HashSet<int> _unreadReportIds = new HashSet<int>();

        public static int UnreadCount
        {
            get
            {
                try { return _unreadMessageKeys.Count + _unreadReportIds.Count; }
                catch { return 0; }
            }
        }

        public static void MarkReportReadFromUi(int reportId)
        {
            if (reportId <= 0)
                return;

            try
            {
                _unreadReportIds.Remove(reportId);
                PublishUnreadCount();

                if (AmongUsClient.Instance != null)
                {
                    AmongUsClient.Instance.StartCoroutine(
                        BanModCommunicationManager.MarkReportReadCoroutine(reportId).WrapToIl2Cpp()
                    );
                }
            }
            catch { }
        }

        private static void PublishUnreadCount()
        {
            try { BanModCommunicationUi.SetUnreadCount(UnreadCount); }
            catch { }
        }

        private static void UpdateUnreadMessages(List<ServerMessage> messages)
        {
            try
            {
                _unreadMessageKeys.Clear();
                if (messages != null)
                {
                    for (int i = 0; i < messages.Count; i++)
                    {
                        ServerMessage msg = messages[i];
                        if (msg == null || !ShouldShowMessage(msg))
                            continue;

                        string key = GetMessageAckKey(msg);
                        if (!string.IsNullOrWhiteSpace(key))
                            _unreadMessageKeys.Add(key);
                    }
                }
                PublishUnreadCount();
            }
            catch { }
        }

        private static void UpdateUnreadReports(List<BanModCommunicationManager.ReportSummary> reports)
        {
            try
            {
                _unreadReportIds.Clear();
                if (reports != null)
                {
                    for (int i = 0; i < reports.Count; i++)
                    {
                        BanModCommunicationManager.ReportSummary report = reports[i];
                        if (report == null || report.Id <= 0 || report.DeletedByPlayer)
                            continue;

                        bool unread = report.UnreadKnown && report.IsUnread;
                        if (!report.UnreadKnown)
                        {
                            BanModCommunicationManager.ReportChatMessage latest = GetLatestAdminReportMessage(report);
                            if (latest != null)
                            {
                                string signature = BuildReportReplySignature(report, latest);
                                unread = !string.IsNullOrWhiteSpace(signature) &&
                                         !string.Equals(GetLocalReportReplyAck(report.Id), signature, StringComparison.Ordinal);
                            }
                        }

                        if (unread)
                            _unreadReportIds.Add(report.Id);
                    }
                }
                PublishUnreadCount();
            }
            catch { }
        }

        public static void RefreshUnreadReportsFromUi(List<BanModCommunicationManager.ReportSummary> reports)
        {
            UpdateUnreadReports(reports);
        }

        public static void RequestImmediateReportPoll()
        {
            try { _nextReportPollTime = 0f; } catch { }
        }

        public static void RequestImmediatePoll()
        {
            try
            {
                _nextMessagePollTime = 0f;
                _nextReportPollTime = 0f;
            }
            catch { }
        }

        public static void Postfix()
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return;

                bool pollMessages = Time.time >= _nextMessagePollTime;
                bool pollReports = Time.time >= _nextReportPollTime;

                if ((!pollMessages && !pollReports) || _pollRunning)
                    return;

                if (pollMessages)
                    _nextMessagePollTime = Time.time + BanModCommunicationConfig.MessagePollIntervalSeconds;

                if (pollReports)
                    _nextReportPollTime = Time.time + FastReportPollIntervalSeconds;

                _pollMessagesRequested = pollMessages;
                _pollReportsRequested = pollReports;
                AmongUsClient.Instance.StartCoroutine(PollWrapper().WrapToIl2Cpp());
            }
            catch { }
        }

        private static IEnumerator PollWrapper()
        {
            _pollRunning = true;

            bool pollMessages = _pollMessagesRequested;
            bool pollReports = _pollReportsRequested;
            _pollMessagesRequested = false;
            _pollReportsRequested = false;

            try
            {
                yield return PollServerMessages(pollMessages, pollReports);
            }
            finally
            {
                _pollRunning = false;
            }
        }

        private static IEnumerator PollServerMessages(bool pollMessages, bool pollReports)
        {
            if (!pollMessages && !pollReports)
                yield break;

            if (!pollMessages)
            {
                if (pollReports)
                    yield return PollReportReplies();
                yield break;
            }

            bool hasToken = false;

            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) =>
            {
                hasToken = success;
            });

            if (!hasToken)
                yield break;

            UnityWebRequest request = UnityWebRequest.Get(BanModCommunicationConfig.MessagesUrl);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;
            BanModApiTokenManager.ApplyAuthHeader(request);

            yield return request.SendWebRequest();

            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                request.Dispose();
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                BMLogger.LogWarning("[COMM] Message poll failed HTTP " + request.responseCode + " result=" + request.result);
                request.Dispose();
                yield break;
            }

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            request.Dispose();

            if (BanModCore.TryApplyServerForceDisable(responseText))
                yield break;

            List<ServerMessage> messages = ParseMessages(responseText);
            UpdateUnreadMessages(messages);

            foreach (ServerMessage msg in messages)
            {
                if (msg == null || msg.Id <= 0)
                    continue;

                if (!ShouldShowMessage(msg))
                    continue;

                if (ShowServerMessagePopup(msg))
                    yield break;
            }

            if (pollReports)
                yield return PollReportReplies();
        }

        private static IEnumerator PollReportReplies()
        {
            List<BanModCommunicationManager.ReportSummary> reports = null;
            string error = "";

            yield return BanModCommunicationManager.GetMyReportsCoroutine((items, err) =>
            {
                reports = items;
                error = err ?? "";
            });

            if (!string.IsNullOrWhiteSpace(error))
                yield break;

            if (reports == null)
                reports = new List<BanModCommunicationManager.ReportSummary>();

            UpdateUnreadReports(reports);

            // A background report must not replace another popup and mark it read.
            if (BanModCommunicationUi.IsUiOpen)
                yield break;

            if (reports.Count <= 0)
                yield break;

            for (int i = 0; i < reports.Count; i++)
            {
                BanModCommunicationManager.ReportSummary report = reports[i];
                if (report == null || report.Id <= 0 || report.DeletedByPlayer)
                    continue;
                if (report.UnreadKnown && !report.IsUnread)
                    continue;

                BanModCommunicationManager.ReportChatMessage adminMessage = GetLatestAdminReportMessage(report);
                if (adminMessage == null || string.IsNullOrWhiteSpace(adminMessage.Message))
                    continue;

                string signature = BuildReportReplySignature(report, adminMessage);
                if (string.IsNullOrWhiteSpace(signature))
                    continue;

                string visibleKey = report.Id.ToString() + "_" + signature;
                if (_visibleReportReplyKeys.Contains(visibleKey))
                    continue;

                string lastSeenSignature = GetLocalReportReplyAck(report.Id);
                if (!string.IsNullOrWhiteSpace(lastSeenSignature) && lastSeenSignature == signature)
                    continue;

                ShowReportReplyPopup(report, signature, visibleKey);
                yield break;
            }
        }

        private static bool ShouldShowMessage(ServerMessage msg)
        {
            if (msg == null || msg.Id <= 0)
                return false;

            string key = GetMessageAckKey(msg);
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (_visibleMessageKeys.Contains(key))
                return false;

            double lastAck = GetLocalAckUnixSeconds(key);
            double cooldownSeconds = GetReminderCooldownSeconds(msg);
            if (lastAck > 0 && UnixNowSeconds() - lastAck < cooldownSeconds)
                return false;

            return true;
        }

        private static void AcknowledgeMessage(ServerMessage msg)
        {
            if (msg == null || msg.Id <= 0)
                return;

            string key = GetMessageAckKey(msg);
            if (!string.IsNullOrWhiteSpace(key))
            {
                SaveLocalAck(key);
                _visibleMessageKeys.Remove(key);
                _unreadMessageKeys.Remove(key);
                PublishUnreadCount();
            }

            try
            {
                if (AmongUsClient.Instance != null)
                    AmongUsClient.Instance.StartCoroutine(MarkMessageRead(msg.Id, msg.Signature).WrapToIl2Cpp());
            }
            catch { }
        }

        private static IEnumerator MarkMessageRead(int messageId, string signature)
        {
            string json = "{\"signature\":" + BanModJson.StringValue(signature ?? "") + "}";

            UnityWebRequest request = new UnityWebRequest(BanModCommunicationConfig.MessageReadUrl(messageId), "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;
            BanModApiTokenManager.ApplyAuthHeader(request);

            yield return request.SendWebRequest();
            request.Dispose();
        }

        private static bool ShowServerMessagePopup(ServerMessage msg)
        {
            string title = string.IsNullOrWhiteSpace(msg.Title) ? "Messaggio BANMOD" : msg.Title.Trim();
            string body = msg.Message ?? "";
            string key = GetMessageAckKey(msg);
            if (!string.IsNullOrWhiteSpace(key)) _visibleMessageKeys.Add(key);
            try
            {
                BanModCommunicationUi.EnsureCreated();
                var ui = BanModCommunicationUi.Instance;
                if (ui != null && ui.TryShowBackgroundMessagePopup(title, body, () => AcknowledgeMessage(msg)))
                {
                    BMLogger.LogInfo("[COMM] Message popup opened id=" + msg.Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                BMLogger.LogWarning("[COMM] Message popup failed id=" + msg.Id + " exception=" + ex.GetType().Name);
            }
            // Failed/deferred display is not an acknowledgement. Allow the next poll to retry.
            if (!string.IsNullOrWhiteSpace(key)) _visibleMessageKeys.Remove(key);
            return false;
        }

        private static BanModCommunicationManager.ReportChatMessage GetLatestAdminReportMessage(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null || report.Chat == null)
                return null;

            BanModCommunicationManager.ReportChatMessage latest = null;
            for (int i = 0; i < report.Chat.Count; i++)
            {
                BanModCommunicationManager.ReportChatMessage msg = report.Chat[i];
                if (msg == null || !string.Equals(msg.AuthorType ?? "", "admin", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (latest == null || msg.CreatedAt > latest.CreatedAt || msg.Id > latest.Id)
                    latest = msg;
            }

            return latest;
        }

        private static string BuildReportReplySignature(BanModCommunicationManager.ReportSummary report, BanModCommunicationManager.ReportChatMessage msg)
        {
            if (report == null || msg == null)
                return "";

            string raw = report.Id.ToString() + "|"
                + msg.Id.ToString() + "|"
                + msg.CreatedAt.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|"
                + (msg.AuthorName ?? "") + "|"
                + (msg.Message ?? "");

            return StableHash(raw);
        }

        private static void ShowReportReplyPopup(BanModCommunicationManager.ReportSummary report, string signature, string visibleKey)
        {
            if (report == null || report.Id <= 0)
                return;

            if (!string.IsNullOrWhiteSpace(visibleKey))
                _visibleReportReplyKeys.Add(visibleKey);

            try
            {
                BanModCommunicationUi.EnsureCreated();

                if (BanModCommunicationUi.Instance != null)
                {
                    BanModCommunicationUi.Instance.ShowReportChatPopup(report, () => AcknowledgeReportReply(report.Id, signature, visibleKey));
                    return;
                }
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(visibleKey))
                    _visibleReportReplyKeys.Remove(visibleKey);
            }

            try
            {
                SaveLocalReportReplyAck(report.Id, signature);
                Debug.LogWarning("[BANMOD REPORT] #" + report.Id + " " + (report.Title ?? "") + "\n" + (report.AdminReply ?? ""));
            }
            catch { }
        }

        private static void AcknowledgeReportReply(int reportId, string signature, string visibleKey)
        {
            SaveLocalReportReplyAck(reportId, signature);
            MarkReportReadFromUi(reportId);

            if (!string.IsNullOrWhiteSpace(visibleKey))
                _visibleReportReplyKeys.Remove(visibleKey);
        }

        private static void SaveLocalReportReplyAck(int reportId, string signature)
        {
            try
            {
                if (reportId <= 0 || string.IsNullOrWhiteSpace(signature))
                    return;

                PlayerPrefs.SetString(LocalReportReplyAckPrefsPrefix + reportId.ToString(), signature);
                PlayerPrefs.Save();
            }
            catch { }
        }

        private static string GetLocalReportReplyAck(int reportId)
        {
            try
            {
                if (reportId <= 0)
                    return "";

                return PlayerPrefs.GetString(LocalReportReplyAckPrefsPrefix + reportId.ToString(), "");
            }
            catch
            {
                return "";
            }
        }

        private static string GetMessageAckKey(ServerMessage msg)
        {
            if (msg == null || msg.Id <= 0)
                return "";

            string signature = msg.Signature;
            if (string.IsNullOrWhiteSpace(signature))
                signature = BuildLocalSignature(msg);

            return msg.Id.ToString() + "_" + signature;
        }

        private static string BuildLocalSignature(ServerMessage msg)
        {
            string raw = (msg.Id.ToString() + "|" + (msg.Title ?? "") + "|" + (msg.Message ?? "") + "|" + (msg.Severity ?? "") + "|" + msg.ReminderHours.ToString()).Trim();
            return StableHash(raw);
        }

        private static double GetReminderCooldownSeconds(ServerMessage msg)
        {
            int hours = msg != null ? msg.ReminderHours : 24;

            if (hours != 1 && hours != 3 && hours != 5 && hours != 9 && hours != 12 && hours != 24)
                hours = 24;

            double seconds = hours * 60d * 60d;
            return seconds > 0 ? seconds : DefaultMessageAckCooldownSeconds;
        }

        private static string StableHash(string value)
        {
            try
            {
                unchecked
                {
                    uint hash = 2166136261;
                    string text = value ?? "";

                    for (int i = 0; i < text.Length; i++)
                    {
                        hash ^= text[i];
                        hash *= 16777619;
                    }

                    return hash.ToString("x8");
                }
            }
            catch
            {
                return "fallback";
            }
        }

        private static void SaveLocalAck(string key)
        {
            try
            {
                PlayerPrefs.SetString(LocalAckPrefsPrefix + key, UnixNowSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));
                PlayerPrefs.Save();
            }
            catch { }
        }

        private static double GetLocalAckUnixSeconds(string key)
        {
            try
            {
                string raw = PlayerPrefs.GetString(LocalAckPrefsPrefix + key, "");
                if (double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
                    return value;
            }
            catch { }

            return 0;
        }

        private static double UnixNowSeconds()
        {
            try
            {
                return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }

        private static List<ServerMessage> ParseMessages(string json)
        {
            var result = new List<ServerMessage>();
            if (string.IsNullOrWhiteSpace(json)) return result;
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Object ||
                    !document.RootElement.TryGetProperty("messages", out var items) ||
                    items.ValueKind != JsonValueKind.Array) return result;
                foreach (var item in items.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    int id = JsonInt(item, "id", 0);
                    if (id <= 0) continue;
                    var msg = new ServerMessage
                    {
                        Id = id,
                        Title = JsonText(item, "title", ""),
                        Message = JsonText(item, "message", ""),
                        Severity = JsonText(item, "severity", "info"),
                        Signature = JsonText(item, "signature", ""),
                        ReminderHours = JsonInt(item, "reminder_hours", JsonInt(item, "reminderHours", 24))
                    };
                    if (string.IsNullOrWhiteSpace(msg.Signature)) msg.Signature = BuildLocalSignature(msg);
                    result.Add(msg);
                }
            }
            catch (JsonException)
            {
                BMLogger.LogWarning("[COMM] Invalid JSON in message response.");
            }
            return result;
        }

        private static string JsonText(JsonElement item, string key, string fallback) =>
            item.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? fallback : fallback;

        private static int JsonInt(JsonElement item, string key, int fallback) =>
            item.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out int number) ? number : fallback;

        private static bool ExtractJsonBool(string json, string key, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(key))
                return fallback;

            try
            {
                string search = "\"" + key + "\"";
                int keyIndex = json.IndexOf(search, StringComparison.OrdinalIgnoreCase);
                if (keyIndex < 0)
                    return fallback;

                int colonIndex = json.IndexOf(':', keyIndex);
                if (colonIndex < 0)
                    return fallback;

                int start = colonIndex + 1;
                while (start < json.Length && char.IsWhiteSpace(json[start]))
                    start++;

                if (start + 4 <= json.Length &&
                    string.Compare(json, start, "true", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

                if (start + 5 <= json.Length &&
                    string.Compare(json, start, "false", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
                    return false;

                if (start < json.Length && json[start] == '1')
                    return true;

                if (start < json.Length && json[start] == '0')
                    return false;
            }
            catch { }

            return fallback;
        }

        private static int ExtractJsonInt(string json, string key, int fallback)
        {
            if (string.IsNullOrWhiteSpace(json))
                return fallback;

            string search = "\"" + key + "\"";
            int keyIndex = json.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
                return fallback;

            int colonIndex = json.IndexOf(':', keyIndex);
            if (colonIndex < 0)
                return fallback;

            int start = colonIndex + 1;
            while (start < json.Length && char.IsWhiteSpace(json[start]))
                start++;

            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-'))
                end++;

            if (int.TryParse(json.Substring(start, end - start), out int value))
                return value;

            return fallback;
        }

        private class ServerMessage
        {
            public int Id;
            public string Title;
            public string Message;
            public string Severity;
            public string Signature;
            public int ReminderHours;
        }
    }
}
