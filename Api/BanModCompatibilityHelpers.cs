//credits and licenses in the resources folder/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace BanMod
{
    public static class BanModApiConfig
    {
        public const string ApiBaseUrl = BanModCore.PublicApiBaseUrl;
        public const string UpdateApiBaseUrl = ApiBaseUrl;
        public const string TokenRequestUrl = ApiBaseUrl + "/api/token/request";
        public const string PlayerStatusUrl = ApiBaseUrl + "/api/lobby/status";
        public const string LobbyStatusUrl = ApiBaseUrl + "/api/lobby/status";
        public const string LobbyUrl = ApiBaseUrl + "/api/lobbies/active";
        public const string PublicLobbyUrl = ApiBaseUrl + "/api/lobbies/active";
        public const string InternalLobbyUrl = ApiBaseUrl + "/api/lobbies/active";
        public const string CheatersUrl = ApiBaseUrl + "/api/cheaters/list";
        public const string TeamersUrl = ApiBaseUrl + "/api/teamers/list";
        public const string CheatersLegacyUrl = ApiBaseUrl + "/api/cheaters";
        public const string TeamersLegacyUrl = ApiBaseUrl + "/api/teamers";
        public const string CheatersAddUrl = ApiBaseUrl + "/api/cheaters/add";
        public const string TeamersAddUrl = ApiBaseUrl + "/api/teamers/add";
        public const string UpdateInfoUrl = ApiBaseUrl + "/api/update_info";
        public const string ModAccessUrl = ApiBaseUrl + "/api/access";
        public const string ExtraModsReportUrl = ApiBaseUrl + "/api/mod/extra-mods/report";
        public const string ModName = "BanMod";
        public const string FriendCodeOverride = "";
        public const float PlayerStatusIntervalSeconds = 30f;
        public const float AccessCheckIntervalSeconds = 30f;
    }

    public static class BanModApiTokenManager
    {
        public static string Token = "";
        public static readonly string ModId = "BanMod-" + BanMod.PluginVersion;
        public static bool LastTokenRequestWasBlocked { get; private set; }
        public static string LastTokenBlockReason { get; private set; } = "";

        public static void ClearLastTokenBlockState()
        {
            LastTokenRequestWasBlocked = false;
            LastTokenBlockReason = "";
        }

        public static IEnumerator EnsureTokenCoroutine(Action<bool, string> callback)
        {
            ClearLastTokenBlockState();

            bool ready = false;
            string token = "";
            yield return BanModCore.EnsureActivationTokenForApi((success, value) =>
            {
                ready = success;
                token = value ?? "";
            });

            Token = ready ? token : "";
            if (!ready || string.IsNullOrWhiteSpace(Token))
            {
                LastTokenRequestWasBlocked = true;
                LastTokenBlockReason = "BanMod client token unavailable.";
                callback?.Invoke(false, "");
                yield break;
            }

            callback?.Invoke(true, Token);
        }

        public static void ApplyAuthHeader(UnityWebRequest request)
        {
            if (request == null)
                return;

            string friendCode = BanModIdentity.GetFriendCode();
            string clientToken = BanModCore.GetCurrentClientToken();
            Token = clientToken ?? "";

            TrySetHeader(request, "X-BANMOD-FriendCode", MakeHeaderSafe(friendCode, "Unknown"));
            if (!string.IsNullOrWhiteSpace(clientToken))
                TrySetHeader(request, "X-BANMOD-Client-Token", MakeHeaderSafe(clientToken, ""));
        }

        private static string MakeHeaderSafe(string value, string fallback)
        {
            if (string.IsNullOrEmpty(value))
                return fallback;

            StringBuilder safe = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (c >= 0x20 && c <= 0x7E)
                    safe.Append(c);
            }

            string result = safe.ToString().Trim();
            return string.IsNullOrEmpty(result) ? fallback : result;
        }

        private static bool TrySetHeader(UnityWebRequest request, string name, string value)
        {
            if (request == null || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return false;

            try
            {
                request.SetRequestHeader(name, value);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BANMOD][AUTH] Header skipped (" + name + "): " +
                    ex.GetType().Name
                );
                return false;
            }
        }

        public static void ClearToken()
        {
            ClearToken(BanModCore.GetCurrentFriendCode(), BanModCore.GetCurrentClientToken());
        }

        public static void ClearToken(UnityWebRequest rejectedRequest)
        {
            if (rejectedRequest == null) return;
            try
            {
                ClearToken(rejectedRequest.GetRequestHeader("X-BANMOD-FriendCode") ?? "",
                           rejectedRequest.GetRequestHeader("X-BANMOD-Client-Token") ?? "");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[BANMOD][AUTH] Cannot inspect rejected request: " + ex.GetType().Name);
            }
        }

        public static void ClearToken(string friendCode, string rejectedToken)
        {
            BanModCore.RejectClientToken(friendCode, rejectedToken);
            if (!string.IsNullOrWhiteSpace(rejectedToken))
                System.Threading.Interlocked.CompareExchange(ref Token, "", rejectedToken);
        }

        public static string ExtractJsonString(string json, string key, string fallback = "")
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(key))
                return fallback;
            try
            {
                string search = "\"" + key + "\"";
                int keyIndex = json.IndexOf(search, StringComparison.OrdinalIgnoreCase);
                if (keyIndex < 0) return fallback;
                int colonIndex = json.IndexOf(':', keyIndex);
                if (colonIndex < 0) return fallback;
                int firstQuote = json.IndexOf('"', colonIndex + 1);
                if (firstQuote < 0) return fallback;
                int secondQuote = firstQuote + 1;
                bool escaped = false;
                while (secondQuote < json.Length)
                {
                    char c = json[secondQuote];
                    if (c == '\\' && !escaped) { escaped = true; secondQuote++; continue; }
                    if (c == '"' && !escaped) break;
                    escaped = false;
                    secondQuote++;
                }
                if (secondQuote >= json.Length) return fallback;
                return json.Substring(firstQuote + 1, secondQuote - firstQuote - 1)
                    .Replace("\\\"", "\"")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\\\", "\\");
            }
            catch { return fallback; }
        }
    }

    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
    internal static class BanModUnityWebRequestAuthPatch
    {
        [HarmonyPrefix]
        private static void Prefix(UnityWebRequest __instance)
        {
            if (__instance == null)
                return;

            try
            {
                string url = __instance.url ?? "";
                string apiBase = BanModApiConfig.ApiBaseUrl ?? "";
                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(apiBase))
                    return;

                if (!url.StartsWith(apiBase.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(url, apiBase, StringComparison.OrdinalIgnoreCase))
                    return;

                BanModApiTokenManager.ApplyAuthHeader(__instance);
            }
            catch { }
        }
    }

    public static class BanModIdentity
    {
        public static string GetFriendCode()
        {
            string overrideCode = BanModApiConfig.FriendCodeOverride;
            if (!string.IsNullOrWhiteSpace(overrideCode)) return overrideCode.Trim();
            string fc = BanModCore.GetCurrentFriendCode();
            if (!string.IsNullOrWhiteSpace(fc)) return fc;
            try { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null ? NormalizeFriendCode(PlayerControl.LocalPlayer.Data.FriendCode) : ""; } catch { return ""; }
        }

        public static string GetFriendCode(PlayerControl player)
        {
            try { return player != null && player.Data != null ? NormalizeFriendCode(player.Data.FriendCode) : ""; } catch { return ""; }
        }

        public static string GetFriendCode(ClientData player)
        {
            try { return player != null ? NormalizeFriendCode(player.FriendCode) : ""; } catch { return ""; }
        }

        public static string GetPlayerName()
        {
            string n = BanModCore.GetCurrentPlayerName();
            if (!string.IsNullOrWhiteSpace(n)) return n;
            try { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null ? (PlayerControl.LocalPlayer.Data.PlayerName ?? "").Trim() : ""; } catch { return ""; }
        }

        public static string GetPlayerName(PlayerControl player)
        {
            try { return player != null && player.Data != null ? (player.Data.PlayerName ?? "").Trim() : ""; } catch { return ""; }
        }

        public static string GetPlayerName(ClientData player)
        {
            try { return player != null ? (player.PlayerName ?? "").Trim() : ""; } catch { return ""; }
        }

        public static string GetPlatform()
        {
            try { return Application.platform.ToString(); } catch { return "Unknown"; }
        }

        public static string GetPlatform(ClientData player)
        {
            try { return player != null && player.PlatformData != null ? player.PlatformData.Platform.ToString() : GetPlatform(); }
            catch { return GetPlatform(); }
        }

        private static string NormalizeFriendCode(string fc)
        {
            return string.IsNullOrWhiteSpace(fc) ? "" : fc.Trim();
        }
    }

    public static class BanModJson
    {
        public static string Escape(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        public static string StringValue(string value)
        {
            return "\"" + Escape(value ?? "") + "\"";
        }

        public static string BoolValue(bool value)
        {
            return value ? "true" : "false";
        }
    }

    public sealed class BanModDeniedPlayer
    {
        public string friendCode { get; set; }
        public string hashedPuid { get; set; }
        public string accountId { get; set; }
        public string playerName { get; set; }
        public string name { get; set; }
        public string platform { get; set; }
        public string hackUsed { get; set; }
        public string Reason { get; set; }
    }

    internal sealed class BanModCheatersResponse
    {
        public bool success { get; set; }
        public List<BanModDeniedPlayer> cheaters { get; set; }
    }

    internal sealed class BanModTeamersResponse
    {
        public bool success { get; set; }
        public List<BanModDeniedPlayer> teamers { get; set; }
    }

    public static class BanModDeniedLists
    {

        public static List<BanModDeniedPlayer> Cheaters { get; private set; } =
            new List<BanModDeniedPlayer>();

        public static List<BanModDeniedPlayer> Teamers { get; private set; } =
            new List<BanModDeniedPlayer>();

        public static int LastRefreshUnix { get; private set; }
        public static string LastError { get; private set; } = "";

        private static bool refreshStarted;

        public static IEnumerator RefreshAllCoroutine()
        {
            if (refreshStarted)
                yield break;

            refreshStarted = true;
            LastError = "";

            Task cheaterTask = null;
            Task teamerTask = null;

            try
            {
                cheaterTask = CheaterManager.SyncFromServerAsync();
                teamerTask = TeamerManager.SyncFromServerAsync();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                yield break;
            }

            while ((cheaterTask != null && !cheaterTask.IsCompleted) ||
                   (teamerTask != null && !teamerTask.IsCompleted))
            {
                yield return null;
            }

            try
            {
                ReplaceRows(Cheaters, CheaterManager.GetSnapshot());
                ReplaceRows(Teamers, TeamerManager.GetSnapshot());

                if ((cheaterTask != null && cheaterTask.IsFaulted) ||
                    (teamerTask != null && teamerTask.IsFaulted))
                {
                    LastError = "An error occurred during the initial list synchronization.";
                }

                try { LastRefreshUnix = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
                catch { LastRefreshUnix = 0; }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        public static void ReportCheaterDetected(
            string friendCode,
            string hashedPuid = "",
            string playerName = "",
            string platform = "",
            string reason = "")
        {
            CheaterManager.AddDetected(friendCode, hashedPuid, playerName, platform, reason);
        }

        public static void ReportTeamerDetected(
            string friendCode,
            string hashedPuid = "",
            string playerName = "",
            string platform = "",
            string reason = "")
        {
            TeamerManager.AddDetected(friendCode, hashedPuid, playerName, platform, reason);
        }

        public static bool IsCheater(string friendCode, string hashedPuid = "")
        {
            return CheaterManager.CheckList(friendCode, hashedPuid);
        }

        public static bool IsTeamer(string friendCode, string hashedPuid = "")
        {
            return TeamerManager.CheckList(friendCode, hashedPuid);
        }

        private static void ReplaceRows(
            List<BanModDeniedPlayer> destination,
            List<BanModDeniedPlayer> source)
        {
            destination.Clear();

            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                BanModDeniedPlayer row = source[i];
                if (row != null)
                    destination.Add(row);
            }
        }
    }

    public enum ModFeature
    {
        ForceRole,
        Translate
    }

    public static class ModAccessGuard
    {
        private static bool _refreshRunning = false;
        public static bool ServerChecked { get; private set; }
        public static bool IsModBlocked { get; private set; }
        public static string ModBlockReason { get; private set; } = "";
        public static bool ServerReachable { get; private set; }
        public static float LastValidServerDataTime { get; private set; }

        public static bool IsHost()
        {
            try { return AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost; } catch { return false; }
        }

        public static bool CanUseMod()
        {
            return !IsModBlocked;
        }

        public static bool CanUse(ModFeature feature)
        {
            return CanUseFeatureCurrentStateFailClosedSilent(feature);
        }

        public static bool IsFeatureBlocked(ModFeature feature)
        {
            return IsFeaturePremiumLoaded(feature);
        }

        public static bool CanUseFeatureCurrentStateFailClosedSilent(ModFeature feature)
        {
            return IsFeaturePremiumLoaded(feature);
        }

        public static void StartRefreshIfPossible(bool force = false)
        {
            try
            {
                if (_refreshRunning || AmongUsClient.Instance == null) return;
                AmongUsClient.Instance.StartCoroutine(RefreshAccessWrapper(force).WrapToIl2Cpp());
            }
            catch { }
        }

        private static IEnumerator RefreshAccessWrapper(bool force)
        {
            _refreshRunning = true;
            yield return RefreshAccessFromServer(force);
            _refreshRunning = false;
        }

        public static IEnumerator RefreshAccessFromServer(bool force = false)
        {
            bool ok = false;
            string token = "";
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, value) => { ok = success; token = value ?? ""; });
            ServerChecked = true;
            ServerReachable = ok;
            if (ok) LastValidServerDataTime = Time.time;
            IsModBlocked = !ok;
            ModBlockReason = ok ? "" : "Premium access was not authorized by the server.";
        }

        public static IEnumerator RefreshFeatureDecisionCoroutine(ModFeature feature, Action<bool> callback)
        {
            yield return RefreshAccessFromServer(true);
            bool blocked = !IsFeaturePremiumLoaded(feature);
            callback?.Invoke(blocked);
        }

        public static void ApplyAccessJsonResponse(string responseText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responseText)) return;
                string lower = responseText.ToLowerInvariant();
                if (lower.Contains("premium_gate_ok\":false") || lower.Contains("access_denied") || lower.Contains("blocked") || lower.Contains("extra_report_invalid"))
                {
                    IsModBlocked = true;
                    ModBlockReason = "Premium Access denied from server.";
                    BanModCore.StopAllPremiumModules();
                }
            }
            catch { }
        }

        private static bool IsFeaturePremiumLoaded(ModFeature feature)
        {
            try
            {
                switch (feature)
                {
                    case ModFeature.ForceRole:
                        return true;
                    default:
                        return false;
                }
            }
            catch { return false; }
        }
    }

    public static class ModUpdater
    {
        private const int UpdateTimeoutSeconds = 60;
        private const string UpdateLatestUrl = BanModApiConfig.UpdateApiBaseUrl + "/public/update/latest";

        public static bool hasUpdate = false;
        public static bool hasOptionalUpdate = false;
        public static bool isBroken = false;
        public static bool isChecked = false;
        public static bool isMandatory = false;
        public static bool isInstalling = false;
        public static Version latestVersion = null;
        public static string latestTitle = "";
        public static string downloadUrl = "";
        public static string latestSha256 = "";
        public static string lastError = "";

        private static bool startupCheckRunning;
        private static bool startupCheckFinished;
        private static StartupUpdateResult cachedStartupResult;

        public sealed class StartupUpdateResult
        {
            public bool Checked { get; set; }
            public bool UpdateAvailable { get; set; }
            public bool Mandatory { get; set; }
            public bool UpdateStarted { get; set; }
            public bool BlockStartup { get; set; }
            public bool RepairRequired { get; set; }
            public string ReleaseNotes { get; set; } = "";
            public string Error { get; set; } = "";
        }

        private sealed class UpdateLatestResponse
        {
            public bool success { get; set; }
            public bool update_available { get; set; }
            public string latest_version { get; set; }
            public string release_version { get; set; }
            public bool? enabled { get; set; }
            public bool mandatory { get; set; }
            public bool required { get; set; }
            public bool force_update { get; set; }
            public bool auto_update { get; set; }
            public bool repair_required { get; set; }
            public bool integrity_mismatch { get; set; }
            public string sha256 { get; set; }
            public long size_bytes { get; set; }
            public string release_notes { get; set; }
            public string download_url { get; set; }
            public string reason { get; set; }
        }

        public static IEnumerator CheckAtStartupCoroutine(Action<StartupUpdateResult> callback)
        {
            yield return EnsureStartupCheckCoroutine(callback);
        }

        public static IEnumerator EnsureStartupCheckCoroutine(Action<StartupUpdateResult> callback)
        {
            if (startupCheckFinished && cachedStartupResult != null)
            {
                callback?.Invoke(cachedStartupResult);
                yield break;
            }

            if (startupCheckRunning)
            {
                while (startupCheckRunning)
                    yield return null;

                callback?.Invoke(cachedStartupResult);
                yield break;
            }

            startupCheckRunning = true;
            StartupUpdateResult result = null;

            yield return CheckAtStartupInternalCoroutine(value => result = value);

            cachedStartupResult = result;
            startupCheckFinished = result != null && result.Checked;
            startupCheckRunning = false;
            callback?.Invoke(result);
        }

        private static IEnumerator CheckAtStartupInternalCoroutine(Action<StartupUpdateResult> callback)
        {
            StartupUpdateResult result = new StartupUpdateResult();

            hasUpdate = false;
            hasOptionalUpdate = false;
            isBroken = false;
            isChecked = false;
            isMandatory = false;
            isInstalling = false;
            latestVersion = null;
            latestTitle = "";
            downloadUrl = "";
            latestSha256 = "";
            lastError = "";

            float updaterIdentityWaitStarted = Time.realtimeSinceStartup;
            string updaterFriendCode = BanModIdentity.GetFriendCode();
            while (string.IsNullOrWhiteSpace(updaterFriendCode) &&
                   Time.realtimeSinceStartup - updaterIdentityWaitStarted < 60f)
            {
                yield return null;
                updaterFriendCode = BanModIdentity.GetFriendCode();
            }

            if (string.IsNullOrWhiteSpace(updaterFriendCode))
            {
                result.Checked = false;
                result.Error = "FriendCode not available; update check deferred";
                lastError = result.Error;
                isBroken = false;
                isChecked = true;
                callback?.Invoke(result);
                yield break;
            }

            updaterFriendCode = updaterFriendCode.Replace("\r", "").Replace("\n", "").Trim();

            string currentVersion = "";
            try { currentVersion = Convert.ToString(BanMod.PluginVersion) ?? ""; } catch { }

            string currentSha256 = SafeGetOwnSha256();
            string url = UpdateLatestUrl
                + "?current_version=" + UnityWebRequest.EscapeURL(currentVersion ?? "")
                + "&current_sha256=" + UnityWebRequest.EscapeURL(currentSha256 ?? "");

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = UpdateTimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Cache-Control", "no-cache");
            request.SetRequestHeader("X-BANMOD-Version", currentVersion ?? "");
            request.SetRequestHeader("X-BANMOD-Sha256", currentSha256 ?? "");
            request.SetRequestHeader("X-BANMOD-Updater", "friendcode-release-v3");
            request.SetRequestHeader("X-BANMOD-FriendCode", updaterFriendCode);

            yield return request.SendWebRequest();

            string responseText = "";
            try { responseText = request.downloadHandler != null ? request.downloadHandler.text : ""; } catch { }

            if (request.result != UnityWebRequest.Result.Success ||
                request.responseCode < 200 || request.responseCode >= 300)
            {
                result.Checked = false;
                result.Error = "Update check HTTP=" + request.responseCode + " " + (request.error ?? "");
                lastError = result.Error;
                isBroken = true;
                isChecked = true;
                request.Dispose();
                callback?.Invoke(result);
                yield break;
            }

            UpdateLatestResponse response = null;
            try
            {
                response = JsonSerializer.Deserialize<UpdateLatestResponse>(
                    responseText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                result.Checked = false;
                result.Error = "Invalid update JSON: " + ex.Message;
                lastError = result.Error;
                isBroken = true;
                isChecked = true;
                request.Dispose();
                callback?.Invoke(result);
                yield break;
            }

            request.Dispose();

            if (response == null || !response.success)
            {
                result.Checked = false;
                result.Error = response != null && !string.IsNullOrWhiteSpace(response.reason)
                    ? response.reason
                    : "Invalid update response";
                lastError = result.Error;
                isBroken = true;
                isChecked = true;
                callback?.Invoke(result);
                yield break;
            }

            string serverVersion = !string.IsNullOrWhiteSpace(response.release_version)
                ? response.release_version.Trim()
                : (response.latest_version ?? "").Trim();

            bool releaseEnabled = !response.enabled.HasValue || response.enabled.Value;
            bool mandatoryByServer = response.mandatory || response.required ||
                                     response.force_update || response.auto_update;
            bool automaticMandatoryUpdate = response.update_available &&
                                            releaseEnabled &&
                                            mandatoryByServer;

            result.Checked = true;
            result.UpdateAvailable = response.update_available && releaseEnabled;
            result.Mandatory = automaticMandatoryUpdate;
            result.BlockStartup = automaticMandatoryUpdate;
            result.RepairRequired = response.repair_required || response.integrity_mismatch;
            result.ReleaseNotes = (response.release_notes ?? "").Trim();

            hasUpdate = result.UpdateAvailable;
            isMandatory = automaticMandatoryUpdate;
            hasOptionalUpdate = result.UpdateAvailable && !automaticMandatoryUpdate;
            downloadUrl = response.download_url ?? "";
            latestSha256 = (response.sha256 ?? "").Trim().ToLowerInvariant();
            latestTitle = string.IsNullOrWhiteSpace(serverVersion)
                ? "BanMod update"
                : "BanMod " + serverVersion;
            isChecked = true;

            Version parsedVersion;
            if (Version.TryParse(serverVersion, out parsedVersion))
                latestVersion = parsedVersion;

            result.UpdateStarted = false;
            result.Error = "";
            lastError = "";
            isBroken = false;
            callback?.Invoke(result);
            yield break;
        }

        private static IEnumerator DownloadStageAndLaunchCoroutine(
            string url,
            string expectedSha256,
            Action<bool, string> callback)
        {
            string downloadFriendCode = BanModIdentity.GetFriendCode();
            if (string.IsNullOrWhiteSpace(downloadFriendCode))
            {
                isInstalling = false;
                callback?.Invoke(false, "FriendCode unavailable; update cancelled");
                yield break;
            }
            downloadFriendCode = downloadFriendCode.Replace("\r", "").Replace("\n", "").Trim();

            isInstalling = true;

            byte[] bytes = null;
            Exception downloadError = null;
            bool downloadDone = false;

            try
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(UpdateTimeoutSeconds);
                            client.DefaultRequestHeaders.TryAddWithoutValidation(
                                "X-BANMOD-FriendCode", downloadFriendCode);

                            using (System.Net.Http.HttpResponseMessage response = await client.GetAsync(url))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    downloadError = new Exception(
                                        "HTTP " + (int)response.StatusCode + " " + (response.ReasonPhrase ?? ""));
                                    return;
                                }

                                bytes = await response.Content.ReadAsByteArrayAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        downloadError = ex;
                    }
                    finally
                    {
                        System.Threading.Volatile.Write(ref downloadDone, true);
                    }
                });
            }
            catch (Exception ex)
            {
                downloadError = ex;
                downloadDone = true;
            }

            while (!System.Threading.Volatile.Read(ref downloadDone))
                yield return null;

            if (downloadError != null)
            {
                isInstalling = false;
                callback?.Invoke(
                    false,
                    "Update download failed: " +
                    downloadError.GetType().Name + ": " +
                    downloadError.Message);
                yield break;
            }

            if (bytes == null || bytes.Length == 0)
            {
                isInstalling = false;
                callback?.Invoke(false, "The downloaded update DLL is empty");
                yield break;
            }

            string actualSha256 = Sha256Hex(bytes);
            if (!string.IsNullOrWhiteSpace(expectedSha256) &&
                !string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                isInstalling = false;
                callback?.Invoke(false,
                    "Invalid update SHA256. Expected=" + expectedSha256 + " Received=" + actualSha256);
                yield break;
            }

            string loadedDllPath = SafeGetOwnDllPath();
            if (string.IsNullOrWhiteSpace(loadedDllPath))
            {
                isInstalling = false;
                callback?.Invoke(false, "BanMod DLL path was not found");
                yield break;
            }

            string pluginDirectory = Path.GetDirectoryName(loadedDllPath);
            if (string.IsNullOrWhiteSpace(pluginDirectory) || !Directory.Exists(pluginDirectory))
            {
                isInstalling = false;
                callback?.Invoke(false, "BanMod plugins directory was not found");
                yield break;
            }

            string targetPath = Path.Combine(pluginDirectory, "BanMod.dll");

            bool launchSucceeded = false;
            string launchError = "";

            try
            {
                string stagedPath = targetPath + ".update";
                File.WriteAllBytes(stagedPath, bytes);

                string stagedSha256 = SafeSha256File(stagedPath);
                if (!string.Equals(stagedSha256, actualSha256, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(stagedPath); } catch { }
                    launchError = "Staged update file SHA256 verification failed";
                }
                else
                {
                    string scriptPath = CreateReplacementScript(targetPath, stagedPath);
                    if (string.IsNullOrWhiteSpace(scriptPath))
                    {
                        try { File.Delete(stagedPath); } catch { }
                        launchError = "Unable to create the update process";
                    }
                    else
                    {
                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = scriptPath,
                            UseShellExecute = true,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                        };
                        System.Diagnostics.Process.Start(psi);
                        launchSucceeded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                launchError = ex.GetType().Name + ": " + ex.Message;
            }

            if (!launchSucceeded)
            {
                isInstalling = false;
                callback?.Invoke(false, launchError);
                yield break;
            }

            callback?.Invoke(true, "");

            yield return new WaitForSeconds(0.25f);
            Application.Quit();
        }

        private static string CreateReplacementScript(string targetPath, string stagedPath)
        {
            try
            {
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                string scriptPath = Path.Combine(
                    Path.GetTempPath(),
                    "BanModUpdater_" + pid + "_" + DateTime.UtcNow.Ticks + ".bat");

                string target = EscapeBatchValue(targetPath);
                string staged = EscapeBatchValue(stagedPath);
                string backup = EscapeBatchValue(targetPath + ".backup");
                string pluginDirectory = EscapeBatchValue(
                    Path.GetDirectoryName(targetPath) ?? "");

                string updaterLog = EscapeBatchValue(
                    Path.Combine(
                        Path.GetDirectoryName(targetPath) ?? Path.GetTempPath(),
                        "BanModUpdater.log"));

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("@echo off");
                sb.AppendLine("setlocal");
                sb.AppendLine("set \"BANMOD_PID=" + pid + "\"");
                sb.AppendLine("set \"BANMOD_TRIES=0\"");
                sb.AppendLine("set \"BANMOD_DIRECTORY=" + pluginDirectory + "\"");
                sb.AppendLine("set \"BANMOD_TARGET=" + target + "\"");
                sb.AppendLine(
                    "echo [%date% %time%] Update BAT started. Waiting for shutdown.>>\"" +
                    updaterLog + "\"");

                sb.AppendLine(":wait_for_pid");
                sb.AppendLine(
                    "tasklist /FI \"PID eq %BANMOD_PID%\" /NH 2>NUL | " +
                    "find /I \"%BANMOD_PID%\" >NUL");
                sb.AppendLine("if errorlevel 1 goto wait_for_all_game");
                sb.AppendLine("timeout /t 1 /nobreak >NUL");
                sb.AppendLine("goto wait_for_pid");

                sb.AppendLine(":wait_for_all_game");
                sb.AppendLine(
                    "tasklist /FI \"IMAGENAME eq Among Us.exe\" /NH 2>NUL | " +
                    "find /I \"Among Us.exe\" >NUL");
                sb.AppendLine("if errorlevel 1 goto replace_file");
                sb.AppendLine("timeout /t 1 /nobreak >NUL");
                sb.AppendLine("goto wait_for_all_game");

                sb.AppendLine(":replace_file");
                sb.AppendLine(
                    "if exist \"" + target + "\" copy /Y \"" + target + "\" \"" +
                    backup + "\" >NUL 2>&1");
                sb.AppendLine(
                    "copy /Y \"" + staged + "\" \"" + target + "\" >NUL");

                sb.AppendLine("if not errorlevel 1 goto verify_replaced");
                sb.AppendLine("set /A BANMOD_TRIES+=1");
                sb.AppendLine("if %BANMOD_TRIES% GEQ 30 goto replace_failed");
                sb.AppendLine("timeout /t 1 /nobreak >NUL");
                sb.AppendLine("goto replace_file");

                sb.AppendLine(":verify_replaced");
                sb.AppendLine("if not exist \"" + target + "\" goto replace_failed");
                sb.AppendLine("del /F /Q \"" + staged + "\" >NUL 2>&1");
                sb.AppendLine(
                    "echo [%date% %time%] BanMod.dll replaced successfully.>>\"" +
                    updaterLog + "\"");

                sb.AppendLine(
                    "echo [%date% %time%] Removing obsolete BanMod DLLs.>>\"" +
                    updaterLog + "\"");
                sb.AppendLine("for %%F in (\"%BANMOD_DIRECTORY%\\BanMod*.dll\") do (");
                sb.AppendLine("    if /I not \"%%~fF\"==\"%BANMOD_TARGET%\" (");
                sb.AppendLine("        del /F /Q \"%%~fF\" >NUL 2>&1");
                sb.AppendLine("    )");
                sb.AppendLine(")");
                sb.AppendLine("del /F /Q \"" + backup + "\" >NUL 2>&1");

                sb.AppendLine("timeout /t 2 /nobreak >NUL");

                sb.AppendLine(
                    "echo [%date% %time%] Update installed and obsolete BanMod DLLs removed. Automatic game restart disabled.>>\"" +
                    updaterLog + "\"");

                sb.AppendLine("goto cleanup");

                sb.AppendLine(":replace_failed");
                sb.AppendLine(
                    "echo [%date% %time%] ERROR: BanMod.dll replacement failed.>>\"" +
                    updaterLog + "\"");
                sb.AppendLine("exit /b 1");

                sb.AppendLine(":cleanup");
                sb.AppendLine("timeout /t 1 /nobreak >NUL");
                sb.AppendLine("del /Q \"%~f0\" >NUL 2>&1");

                File.WriteAllText(scriptPath, sb.ToString(), Encoding.ASCII);
                return scriptPath;
            }
            catch
            {
                return "";
            }
        }

        private static string GetGameDirectoryFromTargetPath(string targetPath)
        {
            try
            {
                DirectoryInfo pluginDir =
                    new DirectoryInfo(Path.GetDirectoryName(targetPath) ?? "");

                DirectoryInfo bepinexDir = pluginDir.Parent;
                DirectoryInfo gameDir = bepinexDir != null ? bepinexDir.Parent : null;

                if (gameDir == null)
                    return "";

                string exe = Path.Combine(gameDir.FullName, "Among Us.exe");
                return File.Exists(exe) ? gameDir.FullName : "";
            }
            catch
            {
                return "";
            }
        }

        private static string GetPreferredRestartTarget(
            string gameDirectory,
            out bool isProtocol)
        {
            isProtocol = false;

            try
            {
                if (string.IsNullOrWhiteSpace(gameDirectory))
                    return "";

                string exe = Path.Combine(gameDirectory, "Among Us.exe");
                return File.Exists(exe) ? exe : "";
            }
            catch
            {
                return "";
            }
        }

        private static string EscapeBatchValue(string value)
        {
            return (value ?? "").Replace("%", "%%").Replace("\"", "\"\"");
        }

        private static string SafeGetOwnDllPath()
        {
            try
            {
                Assembly assembly = typeof(BanMod).Assembly;
                if (assembly != null &&
                    !string.IsNullOrWhiteSpace(assembly.Location) &&
                    File.Exists(assembly.Location))
                    return Path.GetFullPath(assembly.Location);
            }
            catch { }

            try
            {
                string fallback = Path.Combine(Paths.PluginPath, "BanMod.dll");
                if (File.Exists(fallback))
                    return Path.GetFullPath(fallback);
            }
            catch { }

            return "";
        }

        private static string SafeGetOwnSha256()
        {
            string path = SafeGetOwnDllPath();
            return string.IsNullOrWhiteSpace(path) ? "" : SafeSha256File(path);
        }

        private static string SafeSha256File(string path)
        {
            try
            {
                using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                using (FileStream stream = File.OpenRead(path))
                {
                    byte[] hash = sha.ComputeHash(stream);
                    StringBuilder sb = new StringBuilder(hash.Length * 2);
                    for (int i = 0; i < hash.Length; i++)
                        sb.Append(hash[i].ToString("x2"));
                    return sb.ToString();
                }
            }
            catch
            {
                return "";
            }
        }

        private static string Sha256Hex(byte[] bytes)
        {
            try
            {
                using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(bytes ?? Array.Empty<byte>());
                    StringBuilder sb = new StringBuilder(hash.Length * 2);
                    for (int i = 0; i < hash.Length; i++)
                        sb.Append(hash[i].ToString("x2"));
                    return sb.ToString();
                }
            }
            catch
            {
                return "";
            }
        }

        private static bool manualUpdateRunning;

        public static void StartUpdate(string ignoredUrl)
        {
            StartManualUpdate();
        }

        public static void StartManualUpdate()
        {
            if (manualUpdateRunning || isInstalling)
                return;

            lastError = "";
            isBroken = false;

            try
            {
                if (AmongUsClient.Instance == null)
                {
                    lastError = "Unable to start the manual update: game client unavailable";
                    isBroken = true;
                    return;
                }

                manualUpdateRunning = true;
                AmongUsClient.Instance.StartCoroutine(ManualUpdateCoroutine().WrapToIl2Cpp());
            }
            catch (Exception ex)
            {
                manualUpdateRunning = false;
                isBroken = true;
                lastError = "Failed to start the manual update: " + ex.GetType().Name + ": " + ex.Message;
                Debug.LogError("[BanMod Updater] " + lastError);
            }
        }

        private static IEnumerator ManualUpdateCoroutine()
        {
            StartupUpdateResult result = null;

            while (startupCheckRunning)
                yield return null;

            yield return CheckAtStartupInternalCoroutine(value => result = value);

            if (result == null || !result.Checked)
            {
                manualUpdateRunning = false;
                isBroken = true;
                if (string.IsNullOrWhiteSpace(lastError))
                    lastError = result != null ? (result.Error ?? "Update check failed") : "Update check returned no response";
                Debug.LogError("[BanMod Updater] Manual update unavailable: " + lastError);
                yield break;
            }

            if (result.UpdateStarted)
            {
                manualUpdateRunning = false;
                yield break;
            }

            if (!result.UpdateAvailable)
            {
                manualUpdateRunning = false;
                isBroken = false;
                lastError = "No update available";
                Debug.Log("[BanMod Updater] " + lastError);
                yield break;
            }

            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                manualUpdateRunning = false;
                isBroken = true;
                lastError = "Manual update response does not contain download_url";
                Debug.LogError("[BanMod Updater] " + lastError);
                yield break;
            }

            if (string.IsNullOrWhiteSpace(latestSha256) ||
                !Regex.IsMatch(latestSha256, "^[0-9a-f]{64}$", RegexOptions.IgnoreCase))
            {
                manualUpdateRunning = false;
                isBroken = true;
                lastError = "Manual update response does not contain a valid official SHA256";
                Debug.LogError("[BanMod Updater] " + lastError);
                yield break;
            }

            bool started = false;
            string installError = "";

            yield return DownloadStageAndLaunchCoroutine(
                downloadUrl,
                latestSha256,
                (ok, error) =>
                {
                    started = ok;
                    installError = error ?? "";
                });

            manualUpdateRunning = false;
            isBroken = !started;
            lastError = installError;

            if (!started)
                Debug.LogError("[BanMod Updater] Manual update failed: " + lastError);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), "Start")]
    internal static class BanModMandatoryUpdateBootstrapPatch
    {
        private static bool checkRunning;
        private static bool checkCompleted;

        private static void Postfix(MainMenuManager __instance)
        {
            if (__instance == null || checkRunning || checkCompleted)
                return;

            try
            {
                checkRunning = true;
                __instance.StartCoroutine(RunStartupUpdateCheck().WrapToIl2Cpp());
            }
            catch (Exception ex)
            {
                checkRunning = false;
                Debug.LogError("[BanMod Updater] Unable to start the update check: " + ex.Message);
            }
        }

        private static IEnumerator RunStartupUpdateCheck()
        {
            float identityWaitStarted = Time.realtimeSinceStartup;
            while (string.IsNullOrWhiteSpace(BanModIdentity.GetFriendCode()) &&
                   Time.realtimeSinceStartup - identityWaitStarted < 60f)
            {
                yield return null;
            }

            if (string.IsNullOrWhiteSpace(BanModIdentity.GetFriendCode()))
            {
                checkRunning = false;
                Debug.LogWarning("[BanMod Updater] FriendCode non disponibile; controllo rinviato.");
                yield break;
            }

            ModUpdater.StartupUpdateResult result = null;

            yield return ModUpdater.EnsureStartupCheckCoroutine(value => result = value);

            checkRunning = false;

            if (result == null)
            {
                Debug.LogError("[BanMod Updater] The update check returned no result.");
                yield break;
            }

            if (!result.Checked)
            {
                Debug.LogWarning("[BanMod Updater] Update check failed: " + (result.Error ?? "unknown error"));
                yield break;
            }

            checkCompleted = true;

            if (result.UpdateAvailable)
            {
                string releaseNotes = result.ReleaseNotes ?? "";

                Debug.Log(
                    "[BanMod Updater] " +
                    (result.Mandatory ? "Mandatory" : "Optional") +
                    " update available: " +
                    ModUpdater.latestTitle
                );

                BanModPopup.CreateUpdatePopup(
                    ModUpdater.latestTitle,
                    releaseNotes,
                    result.Mandatory,
                    ModUpdater.StartManualUpdate
                );
            }
        }
    }

}
