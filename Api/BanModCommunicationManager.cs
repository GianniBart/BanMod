//credits and licenses in the resources folder/
using InnerNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace BanMod
{
    public static class BanModCommunicationManager
    {
        public static IEnumerator SendBugReportCoroutine(
            string title,
            string gameMode,
            bool customRolesEnabled,
            string customRolesWhich,
            bool otherModsInstalled,
            string otherModsWhich,
            string bugDescription,
            Action<bool, string> callback
        )
        {
            List<KeyValuePair<string, string>> extra = new List<KeyValuePair<string, string>>();
            AddField(extra, "gameMode", gameMode);
            AddField(extra, "customRolesEnabled", customRolesEnabled ? "true" : "false");
            AddField(extra, "customRolesWhich", customRolesWhich);
            AddField(extra, "otherModsInstalled", otherModsInstalled ? "true" : "false");
            AddField(extra, "otherModsWhich", otherModsWhich);
            AddField(extra, "bugDescription", bugDescription);

            yield return SendReportCoroutine(
                "bug_report",
                title,
                bugDescription,
                "",
                "",
                true,
                extra,
                callback
            );
        }

        public static IEnumerator SendBugReportCoroutine(string title, string message, bool includeFullLogs, Action<bool, string> callback)
        {
            yield return SendBugReportCoroutine(
                title,
                SafeGetLobbyMode(),
                false,
                "",
                false,
                "",
                message,
                callback
            );
        }

        public static IEnumerator SendPlayerReportCoroutine(
            string reason,
            string targetFriendCode,
            string targetName,
            string targetHashedPuid,
            string targetPlayerId,
            string targetPlatform,
            Action<bool, string> callback
        )
        {
            List<KeyValuePair<string, string>> extra = new List<KeyValuePair<string, string>>();
            AddField(extra, "Reason", reason);
            AddField(extra, "targetHashedPuid", targetHashedPuid);
            AddField(extra, "targetPlayerId", targetPlayerId);
            AddField(extra, "targetPlatform", targetPlatform);

            string cleanTarget = string.IsNullOrWhiteSpace(targetName) ? targetFriendCode : targetName;
            string title = string.IsNullOrWhiteSpace(cleanTarget) ? "Player Report" : "Player Report: " + cleanTarget;

            yield return SendReportCoroutine(
                "player_report",
                title,
                reason,
                targetFriendCode,
                targetName,
                true,
                extra,
                callback
            );
        }

        public static IEnumerator SendPlayerReportCoroutine(string title, string message, string targetFriendCode, string targetName, bool includeFullLogs, Action<bool, string> callback)
        {
            yield return SendPlayerReportCoroutine(
                message,
                targetFriendCode,
                targetName,
                "",
                "",
                "",
                callback
            );
        }

        public static IEnumerator SendSupportMessageCoroutine(string title, string message, bool includeFullLogs, Action<bool, string> callback)
        {
            yield return SendReportCoroutine(
                "support",
                title,
                message,
                "",
                "",
                true,
                new List<KeyValuePair<string, string>>(),
                callback
            );
        }

        public static IEnumerator SendReportCoroutine(
            string reportType,
            string title,
            string message,
            string targetFriendCode,
            string targetName,
            bool includeFullLogs,
            Action<bool, string> callback
        )
        {
            yield return SendReportCoroutine(
                reportType,
                title,
                message,
                targetFriendCode,
                targetName,
                true,
                new List<KeyValuePair<string, string>>(),
                callback
            );
        }

        public static IEnumerator SendReportCoroutine(
            string reportType,
            string title,
            string message,
            string targetFriendCode,
            string targetName,
            bool includeFullLogs,
            List<KeyValuePair<string, string>> extraFields,
            Action<bool, string> callback
        )
        {
            bool hasToken = false;

            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) =>
            {
                hasToken = success;
            });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            string zipPath = "";
            byte[] zipBytes = null;
            string logError;
            zipPath = BanModLogZipCollector.CreateFullLogsZip(out logError);

            if (string.IsNullOrWhiteSpace(zipPath) || !File.Exists(zipPath))
            {
                callback?.Invoke(false, "Could not create log ZIP: " + logError);
                yield break;
            }

            try
            {
                zipBytes = File.ReadAllBytes(zipPath);
            }
            catch (Exception ex)
            {
                BanModLogZipCollector.TryDeleteTempZip(zipPath);
                callback?.Invoke(false, "Could not read log ZIP: " + ex.Message);
                yield break;
            }

            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            AddField(fields, "type", reportType);
            AddField(fields, "title", title);
            AddField(fields, "message", message);
            AddField(fields, "senderFriendCode", SafeGetFriendCode());
            AddField(fields, "senderName", SafeGetPlayerName());
            AddField(fields, "modId", SafeGetModId());
            AddField(fields, "gameCode", SafeGetGameCode());
            AddField(fields, "region", SafeGetRegion());
            AddField(fields, "language", SafeGetLanguage());
            AddField(fields, "targetFriendCode", targetFriendCode);
            AddField(fields, "targetName", targetName);
            AddField(fields, "includeLogs", "true");

            if (extraFields != null)
            {
                for (int i = 0; i < extraFields.Count; i++)
                    AddField(fields, extraFields[i].Key, extraFields[i].Value);
            }

            string boundary;
            byte[] body = BuildMultipartBody(fields, zipBytes, out boundary);

            UnityWebRequest request = new UnityWebRequest(BanModCommunicationConfig.ReportUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + boundary);
            request.timeout = 120;
            BanModApiTokenManager.ApplyAuthHeader(request);

            yield return request.SendWebRequest();

            bool success = false;
            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            string resultMessage = "";

            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                resultMessage = "Unauthorized. Token cleared, try again.";
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                success = ExtractJsonBool(responseText, "success", false);
                resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "message", "Report sent.");

                if (!success)
                    resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "error", "Could not send report.");
            }
            else
            {
                resultMessage = request.error;
            }

            request.Dispose();
            BanModLogZipCollector.TryDeleteTempZip(zipPath);

            callback?.Invoke(success, resultMessage);
        }



        public class ReportChatMessage
        {
            public int Id;
            public string AuthorType;
            public string AuthorName;
            public string Message;
            public bool ReadByPlayer;
            public double CreatedAt;
        }

        public class ReportSummary
        {
            public int Id;
            public string Type;
            public string Title;
            public string Message;
            public string Status;
            public string AdminReply;
            public string TargetName;
            public string TargetFriendCode;
            public string GameMode;
            public double CreatedAt;
            public double UpdatedAt;
            public bool DeletedByPlayer;
            public double ClosedByPlayerAt;
            public bool IsUnread;
            public bool UnreadKnown;
            public int UnreadCount;
            public List<ReportChatMessage> Chat = new List<ReportChatMessage>();
        }

        private static UnityWebRequest CreateJsonRequest(string url, string method, string json)
        {
            UnityWebRequest request = new UnityWebRequest(url, method);
            if (json != null)
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;
            BanModApiTokenManager.ApplyAuthHeader(request);
            return request;
        }

        private static string BuildHttpResultMessage(UnityWebRequest request, string responseText, string fallback)
        {
            string msg = BanModApiTokenManager.ExtractJsonString(responseText, "message", "");
            if (string.IsNullOrWhiteSpace(msg))
                msg = BanModApiTokenManager.ExtractJsonString(responseText, "error", "");
            if (string.IsNullOrWhiteSpace(msg) && request != null)
                msg = request.error;
            if (string.IsNullOrWhiteSpace(msg))
                msg = fallback;

            try
            {
                if (request != null && request.responseCode > 0 && !msg.StartsWith("HTTP ", StringComparison.OrdinalIgnoreCase))
                    msg = "HTTP " + request.responseCode + ": " + msg;
            }
            catch { }

            return msg;
        }

        private static void ApplyAccessBlockIfPresent(string responseText)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(responseText))
                    ModAccessGuard.ApplyAccessJsonResponse(responseText);
            }
            catch { }
        }

        public static IEnumerator GetMyReportsCoroutine(Action<List<ReportSummary>, string> callback)
        {
            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) => { hasToken = success; });

            if (!hasToken)
            {
                callback?.Invoke(new List<ReportSummary>(), "Token unavailable.");
                yield break;
            }

            UnityWebRequest request = UnityWebRequest.Get(BanModCommunicationConfig.MyReportsUrl);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;
            BanModApiTokenManager.ApplyAuthHeader(request);

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                request.Dispose();
                callback?.Invoke(new List<ReportSummary>(), "Unauthorized. Token cleared, try again.");
                yield break;
            }

            if (request.responseCode == 403)
                ApplyAccessBlockIfPresent(responseText);

            if (request.result != UnityWebRequest.Result.Success)
            {
                string err = BuildHttpResultMessage(request, responseText, "Could not load reports.");
                request.Dispose();
                callback?.Invoke(new List<ReportSummary>(), err);
                yield break;
            }

            request.Dispose();

            bool success = ExtractJsonBool(responseText, "success", false);
            if (!success)
            {
                string err = BanModApiTokenManager.ExtractJsonString(responseText, "error", "Could not load reports.");
                callback?.Invoke(new List<ReportSummary>(), err);
                yield break;
            }

            callback?.Invoke(ParseReports(responseText), "");
        }

        public static IEnumerator MarkReportReadCoroutine(int reportId, Action<bool, string> callback = null)
        {
            if (reportId <= 0)
            {
                callback?.Invoke(false, "Invalid report id.");
                yield break;
            }

            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) => { hasToken = success; });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            UnityWebRequest request = CreateJsonRequest(
                BanModCommunicationConfig.ReportReadUrl(reportId),
                "POST",
                "{}"
            );

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            bool success = request.result == UnityWebRequest.Result.Success &&
                           request.responseCode >= 200 && request.responseCode < 300 &&
                           ExtractJsonBool(responseText, "success", true);

            if (request.responseCode == 401)
                BanModApiTokenManager.ClearToken(request);

            string message = success
                ? "Read."
                : BuildHttpResultMessage(request, responseText, "Could not mark report as read.");

            request.Dispose();
            callback?.Invoke(success, message);
        }

        public static IEnumerator UpdateReportCoroutine(int reportId, string title, string message, Action<bool, string> callback)
        {
            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) => { hasToken = success; });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            string json = "{"
                + "\"title\":" + BanModJson.StringValue(title) + ","
                + "\"message\":" + BanModJson.StringValue(message)
                + "}";

            UnityWebRequest request = new UnityWebRequest(BanModCommunicationConfig.ReportItemUrl(reportId), "PUT");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;
            BanModApiTokenManager.ApplyAuthHeader(request);

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            bool success = false;
            string resultMessage = "";

            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                resultMessage = "Unauthorized. Token cleared, try again.";
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                success = ExtractJsonBool(responseText, "success", false);
                resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "message", success ? "Report updated." : "Could not update report.");
                if (!success)
                    resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "error", resultMessage);
            }
            else
            {
                resultMessage = request.error;
            }

            request.Dispose();
            callback?.Invoke(success, resultMessage);
        }

        public static IEnumerator DeleteReportCoroutine(int reportId, Action<bool, string> callback)
        {
            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) => { hasToken = success; });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            string json = "{"
                + "\"action\":\"delete\","
                + "\"modId\":" + BanModJson.StringValue(SafeGetModId()) + ","
                + "\"senderFriendCode\":" + BanModJson.StringValue(SafeGetFriendCode())
                + "}";
            UnityWebRequest request = CreateJsonRequest(BanModCommunicationConfig.ReportDeleteUrl(reportId), "POST", json);

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (request.responseCode == 404 || request.responseCode == 405)
            {
                request.Dispose();
                request = new UnityWebRequest(BanModCommunicationConfig.ReportItemUrl(reportId), "DELETE");
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 30;
                BanModApiTokenManager.ApplyAuthHeader(request);

                yield return request.SendWebRequest();
                responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            }

            bool success = false;
            string resultMessage = "";

            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                resultMessage = "Unauthorized. Token cleared, try again.";
            }
            else if (request.responseCode == 403)
            {
                ApplyAccessBlockIfPresent(responseText);
                resultMessage = BuildHttpResultMessage(request, responseText, "Forbidden.");
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                success = ExtractJsonBool(responseText, "success", false);
                resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "message", success ? "Report deleted." : "Could not delete report.");
                if (!success)
                    resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "error", resultMessage);
            }
            else
            {
                resultMessage = BuildHttpResultMessage(request, responseText, "Could not delete report.");
            }

            request.Dispose();
            callback?.Invoke(success, resultMessage);
        }

        public static IEnumerator CloseReportCoroutine(int reportId, Action<bool, string> callback)
        {
            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) => { hasToken = success; });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            string json = "{"
                + "\"action\":\"close\","
                + "\"status\":\"closed\","
                + "\"resolved\":true,"
                + "\"modId\":" + BanModJson.StringValue(SafeGetModId()) + ","
                + "\"senderFriendCode\":" + BanModJson.StringValue(SafeGetFriendCode())
                + "}";

            string[] urls = new string[]
            {
                BanModCommunicationConfig.ReportCloseUrl(reportId),
                BanModCommunicationConfig.ReportResolveUrl(reportId),
                BanModCommunicationConfig.ReportItemUrl(reportId)
            };

            string[] methods = new string[] { "POST", "POST", "POST", "PATCH", "PUT" };
            string lastResponseText = "";
            long lastCode = 0;
            string lastError = "";

            for (int attempt = 0; attempt < 5; attempt++)
            {
                string url = attempt == 0 ? urls[0] : (attempt == 1 ? urls[1] : urls[2]);
                string method = methods[attempt];

                UnityWebRequest request = CreateJsonRequest(url, method, json);
                yield return request.SendWebRequest();

                lastResponseText = request.downloadHandler != null ? request.downloadHandler.text : "";
                lastCode = request.responseCode;
                lastError = request.error ?? "";

                if (lastCode == 401)
                {
                    BanModApiTokenManager.ClearToken(request);
                    request.Dispose();
                    callback?.Invoke(false, "Unauthorized. Token cleared, try again.");
                    yield break;
                }

                if (lastCode == 403)
                {
                    ApplyAccessBlockIfPresent(lastResponseText);
                    string err403 = BuildHttpResultMessage(request, lastResponseText, "Forbidden.");
                    request.Dispose();
                    callback?.Invoke(false, err403);
                    yield break;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    bool success = ExtractJsonBool(lastResponseText, "success", false);
                    string msg = BanModApiTokenManager.ExtractJsonString(lastResponseText, "message", success ? "Report closed as resolved." : "Could not close report.");

                    if (success)
                    {
                        request.Dispose();
                        callback?.Invoke(true, msg);
                        yield break;
                    }

                    string err = BanModApiTokenManager.ExtractJsonString(lastResponseText, "error", "");
                    if (!string.IsNullOrWhiteSpace(err) && err.IndexOf("already", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        request.Dispose();
                        callback?.Invoke(true, msg);
                        yield break;
                    }
                }

                request.Dispose();

                yield return null;
            }

            string fallback = "Could not close report.";
            if (!string.IsNullOrWhiteSpace(lastResponseText))
                fallback = BanModApiTokenManager.ExtractJsonString(lastResponseText, "error", BanModApiTokenManager.ExtractJsonString(lastResponseText, "message", fallback));
            if (!string.IsNullOrWhiteSpace(lastError))
                fallback = lastError;
            if (lastCode > 0)
                fallback = "HTTP " + lastCode + ": " + fallback;

            callback?.Invoke(false, fallback);
        }

        public static IEnumerator SendReportMessageCoroutine(int reportId, string message, Action<bool, string> callback)
        {
            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) => { hasToken = success; });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            string replyFriendCode = SafeGetFriendCode();
            if (string.IsNullOrWhiteSpace(replyFriendCode))
            {
                callback?.Invoke(false, "FriendCode unavailable.");
                yield break;
            }

            string json = "{"
                + "\"action\":\"message\","
                + "\"FriendCode\":" + BanModJson.StringValue(replyFriendCode) + ","
                + "\"senderFriendCode\":" + BanModJson.StringValue(replyFriendCode) + ","
                + "\"modId\":" + BanModJson.StringValue(SafeGetModId()) + ","
                + "\"message\":" + BanModJson.StringValue(message)
                + "}";

            UnityWebRequest request = CreateJsonRequest(BanModCommunicationConfig.ReportMessageUrl(reportId), "POST", json);
            try { request.SetRequestHeader("X-BANMOD-FriendCode", replyFriendCode); } catch { }

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (request.responseCode == 404 || request.responseCode == 405)
            {
                request.Dispose();
                request = CreateJsonRequest(BanModCommunicationConfig.ReportMessagesUrl(reportId), "POST", json);
                try { request.SetRequestHeader("X-BANMOD-FriendCode", replyFriendCode); } catch { }
                yield return request.SendWebRequest();
                responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            }

            if (request.responseCode == 404 || request.responseCode == 405)
            {
                request.Dispose();
                request = CreateJsonRequest(BanModCommunicationConfig.ReportItemUrl(reportId), "POST", json);
                try { request.SetRequestHeader("X-BANMOD-FriendCode", replyFriendCode); } catch { }
                yield return request.SendWebRequest();
                responseText = request.downloadHandler != null ? request.downloadHandler.text : "";
            }

            bool success = false;
            string resultMessage = "";

            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                resultMessage = "Unauthorized. Token cleared, try again.";
            }
            else if (request.responseCode == 403)
            {
                ApplyAccessBlockIfPresent(responseText);
                resultMessage = BuildHttpResultMessage(request, responseText, "Forbidden.");
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                success = ExtractJsonBool(responseText, "success", false);
                resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "message", success ? "Message sent." : "Could not send message.");
                if (!success)
                    resultMessage = BanModApiTokenManager.ExtractJsonString(responseText, "error", resultMessage);
            }
            else
            {
                resultMessage = BuildHttpResultMessage(request, responseText, "Could not send message.");
            }

            request.Dispose();
            callback?.Invoke(success, resultMessage);
        }

        private static List<ReportSummary> ParseReports(string json)
        {
            List<ReportSummary> reports = new List<ReportSummary>();

            if (string.IsNullOrWhiteSpace(json))
                return reports;

            try
            {
                int reportsIndex = json.IndexOf("\"reports\"", StringComparison.OrdinalIgnoreCase);
                if (reportsIndex < 0)
                    return reports;

                int arrayStart = json.IndexOf('[', reportsIndex);
                if (arrayStart < 0)
                    return reports;

                int arrayEnd = FindMatchingBracket(json, arrayStart, '[', ']');
                if (arrayEnd <= arrayStart)
                    return reports;

                string array = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
                List<string> objects = ExtractJsonObjects(array);

                for (int i = 0; i < objects.Count; i++)
                {
                    string obj = objects[i];
                    ReportSummary report = new ReportSummary();
                    report.Id = ExtractTopLevelJsonInt(obj, "id", 0);
                    report.Type = BanModApiTokenManager.ExtractJsonString(obj, "type", "");
                    report.Title = BanModApiTokenManager.ExtractJsonString(obj, "title", "");
                    report.Message = BanModApiTokenManager.ExtractJsonString(obj, "message", "");
                    report.Status = BanModApiTokenManager.ExtractJsonString(obj, "status", "");
                    report.AdminReply = BanModApiTokenManager.ExtractJsonString(obj, "admin_reply", "");
                    report.TargetName = BanModApiTokenManager.ExtractJsonString(obj, "target_name", "");
                    report.TargetFriendCode = BanModApiTokenManager.ExtractJsonString(obj, "target_friend_code", "");
                    report.GameMode = BanModApiTokenManager.ExtractJsonString(obj, "game_mode", "");
                    report.CreatedAt = ExtractJsonDouble(obj, "created_at", 0);
                    report.UpdatedAt = ExtractJsonDouble(obj, "updated_at", 0);
                    report.DeletedByPlayer = ExtractJsonBool(obj, "deleted_by_player", false);
                    report.ClosedByPlayerAt = ExtractJsonDouble(obj, "closed_by_player_at", 0);
                    report.UnreadKnown = obj.IndexOf("\"is_unread\"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         obj.IndexOf("\"unread\"", StringComparison.OrdinalIgnoreCase) >= 0;
                    report.IsUnread = ExtractJsonBool(obj, "is_unread", ExtractJsonBool(obj, "unread", false));
                    report.UnreadCount = ExtractJsonInt(obj, "unread_count", report.IsUnread ? 1 : 0);
                    report.Chat = ParseReportChatMessages(obj);

                    if (report.Chat == null || report.Chat.Count <= 0)
                    {
                        report.Chat = new List<ReportChatMessage>();
                        if (!string.IsNullOrWhiteSpace(report.Message))
                        {
                            report.Chat.Add(new ReportChatMessage
                            {
                                Id = 1,
                                AuthorType = "player",
                                AuthorName = "",
                                Message = report.Message,
                                CreatedAt = report.CreatedAt
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(report.AdminReply))
                        {
                            report.Chat.Add(new ReportChatMessage
                            {
                                Id = report.Chat.Count + 1,
                                AuthorType = "admin",
                                AuthorName = "Admin",
                                Message = report.AdminReply,
                                CreatedAt = report.UpdatedAt
                            });
                        }
                    }

                    if (report.Id > 0)
                        reports.Add(report);
                }
            }
            catch { }

            return reports;
        }

        private static List<ReportChatMessage> ParseReportChatMessages(string reportJson)
        {
            List<ReportChatMessage> result = new List<ReportChatMessage>();

            if (string.IsNullOrWhiteSpace(reportJson))
                return result;

            try
            {
                int chatIndex = reportJson.IndexOf("\"chat\"", StringComparison.OrdinalIgnoreCase);
                if (chatIndex < 0)
                    return result;

                int arrayStart = reportJson.IndexOf('[', chatIndex);
                if (arrayStart < 0)
                    return result;

                int arrayEnd = FindMatchingBracket(reportJson, arrayStart, '[', ']');
                if (arrayEnd <= arrayStart)
                    return result;

                string array = reportJson.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
                List<string> objects = ExtractJsonObjects(array);

                for (int i = 0; i < objects.Count; i++)
                {
                    string obj = objects[i];
                    ReportChatMessage message = new ReportChatMessage();
                    message.Id = ExtractJsonInt(obj, "id", i + 1);
                    message.AuthorType = BanModApiTokenManager.ExtractJsonString(obj, "author_type", "player");
                    message.AuthorName = BanModApiTokenManager.ExtractJsonString(obj, "author_name", "");
                    message.Message = BanModApiTokenManager.ExtractJsonString(obj, "message", "");
                    message.ReadByPlayer = ExtractJsonBool(obj, "read_by_player", false);
                    message.CreatedAt = ExtractJsonDouble(obj, "created_at", 0);

                    if (!string.IsNullOrWhiteSpace(message.Message))
                        result.Add(message);
                }
            }
            catch { }

            return result;
        }

        private static List<string> ExtractJsonObjects(string text)
        {
            List<string> objects = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return objects;

            int depth = 0;
            int start = -1;
            bool inString = false;
            bool escaped = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (inString)
                {
                    if (c == '\\' && !escaped)
                    {
                        escaped = true;
                        continue;
                    }

                    if (c == '"' && !escaped)
                        inString = false;

                    escaped = false;
                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == '{')
                {
                    if (depth == 0)
                        start = i;
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        objects.Add(text.Substring(start, i - start + 1));
                        start = -1;
                    }
                }
            }

            return objects;
        }

        private static int FindMatchingBracket(string text, int openIndex, char openChar, char closeChar)
        {
            int depth = 0;
            bool inString = false;
            bool escaped = false;

            for (int i = openIndex; i < text.Length; i++)
            {
                char c = text[i];

                if (inString)
                {
                    if (c == '\\' && !escaped)
                    {
                        escaped = true;
                        continue;
                    }

                    if (c == '"' && !escaped)
                        inString = false;

                    escaped = false;
                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == openChar)
                    depth++;
                else if (c == closeChar)
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }

            return -1;
        }

        private static int ExtractTopLevelJsonInt(string json, string key, int fallback)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(key))
                return fallback;

            try
            {
                int depth = 0;
                bool inString = false;
                bool escaped = false;

                for (int i = 0; i < json.Length; i++)
                {
                    char c = json[i];

                    if (inString)
                    {
                        if (c == '\\' && !escaped)
                        {
                            escaped = true;
                            continue;
                        }

                        if (c == '"' && !escaped)
                            inString = false;

                        escaped = false;
                        continue;
                    }

                    if (c == '{' || c == '[')
                    {
                        depth++;
                        continue;
                    }

                    if (c == '}' || c == ']')
                    {
                        depth--;
                        continue;
                    }

                    if (c != '"' || depth != 1)
                        continue;

                    int nameStart = i + 1;
                    int nameEnd = nameStart;
                    bool nameEscaped = false;
                    while (nameEnd < json.Length)
                    {
                        char nc = json[nameEnd];
                        if (nc == '\\' && !nameEscaped)
                        {
                            nameEscaped = true;
                            nameEnd++;
                            continue;
                        }
                        if (nc == '"' && !nameEscaped)
                            break;
                        nameEscaped = false;
                        nameEnd++;
                    }

                    if (nameEnd >= json.Length)
                        return fallback;

                    string propertyName = json.Substring(nameStart, nameEnd - nameStart);
                    int colon = nameEnd + 1;
                    while (colon < json.Length && char.IsWhiteSpace(json[colon]))
                        colon++;

                    if (colon >= json.Length || json[colon] != ':')
                    {
                        i = nameEnd;
                        continue;
                    }

                    if (!string.Equals(propertyName, key, StringComparison.OrdinalIgnoreCase))
                    {
                        i = nameEnd;
                        continue;
                    }

                    int start = colon + 1;
                    while (start < json.Length && char.IsWhiteSpace(json[start]))
                        start++;

                    int end = start;
                    while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-' || json[end] == '+'))
                        end++;

                    if (end <= start)
                        return fallback;

                    string number = json.Substring(start, end - start);
                    if (int.TryParse(number, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int value))
                        return value;

                    return fallback;
                }
            }
            catch { }

            return fallback;
        }

        private static int ExtractJsonInt(string json, string key, int fallback)
        {
            try { return (int)ExtractJsonDouble(json, key, fallback); }
            catch { return fallback; }
        }

        private static double ExtractJsonDouble(string json, string key, double fallback)
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
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '.' || json[end] == '-' || json[end] == '+'))
                end++;

            string text = json.Substring(start, end - start);
            if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                return value;

            return fallback;
        }

        private static void AddField(List<KeyValuePair<string, string>> fields, string name, string value)
        {
            fields.Add(new KeyValuePair<string, string>(name, value ?? ""));
        }

        private static byte[] BuildMultipartBody(List<KeyValuePair<string, string>> fields, byte[] zipBytes, out string boundary)
        {
            boundary = "----BanModBoundary" + DateTime.UtcNow.Ticks.ToString("x");

            using (MemoryStream stream = new MemoryStream())
            {
                for (int i = 0; i < fields.Count; i++)
                {
                    KeyValuePair<string, string> field = fields[i];

                    WriteUtf8(stream, "--" + boundary + "\r\n");
                    WriteUtf8(stream, "Content-Disposition: form-data; name=\"" + EscapeMultipartName(field.Key) + "\"\r\n\r\n");
                    WriteUtf8(stream, field.Value ?? "");
                    WriteUtf8(stream, "\r\n");
                }

                if (zipBytes != null && zipBytes.Length > 0)
                {
                    WriteUtf8(stream, "--" + boundary + "\r\n");
                    WriteUtf8(stream, "Content-Disposition: form-data; name=\"logsZip\"; filename=\"banmod_logs.zip\"\r\n");
                    WriteUtf8(stream, "Content-Type: application/zip\r\n\r\n");
                    stream.Write(zipBytes, 0, zipBytes.Length);
                    WriteUtf8(stream, "\r\n");
                }

                WriteUtf8(stream, "--" + boundary + "--\r\n");
                return stream.ToArray();
            }
        }

        private static void WriteUtf8(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text ?? "");
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string EscapeMultipartName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");
        }

        private static string SafeGetModId()
        {
            try { return BanModApiTokenManager.ModId; }
            catch { return ""; }
        }

        private static string SafeGetPlayerName()
        {
            try
            {
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
                    return PlayerControl.LocalPlayer.Data.PlayerName;
            }
            catch { }

            return "";
        }

        private static string SafeGetFriendCode()
        {
            return BanModIdentity.GetFriendCode();
        }

        private static string SafeGetGameCode()
        {
            try
            {
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameId != 0)
                    return GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            }
            catch { }

            return "";
        }

        private static string SafeGetLobbyMode()
        {
            try { return Utils.GetCurrentLobbyMode(); }
            catch { return ""; }
        }

        private static string SafeGetRegion()
        {
            try { return Utils.GetRegionName(); }
            catch { return "Unknown"; }
        }

        private static string SafeGetLanguage()
        {
            try { return Utils.LanguageUtils.GetLanguageName(Utils.LanguageUtils.GetCurrentGameOptions()); }
            catch { return "Unknown"; }
        }

        private static bool ExtractJsonBool(string json, string key, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(json))
                return fallback;

            string compact = json.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
            string search = "\"" + key + "\":";
            int index = compact.IndexOf(search, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                return fallback;

            int start = index + search.Length;

            if (compact.IndexOf("true", start, StringComparison.OrdinalIgnoreCase) == start)
                return true;

            if (compact.IndexOf("false", start, StringComparison.OrdinalIgnoreCase) == start)
                return false;

            return fallback;
        }
    }
}
