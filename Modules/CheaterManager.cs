using InnerNet;
using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BanMod
{
    public static class CheaterManager
    {
        private const string CheaterListPath = "./DATA/DENIED/Cheater.txt";

        private static readonly HttpClient httpClient = new HttpClient();

        private const int StartupTokenWaitAttempts = 240;
        private const int StartupTokenWaitDelayMs = 500;
        private const int StartupHttpAttempts = 3;

        private static readonly object cacheLock = new object();
        private static readonly object startupSyncLock = new object();
        private static Task startupSyncTask;

        private static readonly Dictionary<string, CheaterRecord> cachedCheaters =
            new Dictionary<string, CheaterRecord>(StringComparer.OrdinalIgnoreCase);

        private class CheaterRecord
        {
            public string FriendCode;
            public string HashedPuid;
            public string PlayerName;
            public string Platform;
            public string HackUsed;
        }

        public static void Initialize()
        {
            try
            {
                Directory.CreateDirectory("DATA/DENIED");

                if (!File.Exists(CheaterListPath))
                    File.Create(CheaterListPath).Close();

                LoadLocalCache();
            }
            catch (Exception e)
            {
                Debug.LogError("[BM_AntiCheat] Errore inizializzazione CheaterManager: " + e);
            }
        }

        private static void LoadLocalCache()
        {
            lock (cacheLock)
            {
                cachedCheaters.Clear();

                if (!File.Exists(CheaterListPath))
                    return;

                foreach (string line in File.ReadAllLines(CheaterListPath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    CheaterRecord record = ParseLocalLine(line);

                    if (record == null)
                        continue;

                    AddOrUpdateLocalRecordNoLock(record);
                }
            }
        }

        private static CheaterRecord ParseLocalLine(string line)
        {
            try
            {
                string[] parts = line.Split(',');

                if (parts.Length <= 0)
                    return null;

                string friendCode = parts.Length > 0 ? parts[0].Trim() : "";
                string hashedPuid = parts.Length > 1 ? parts[1].Trim() : "";
                string playerName = parts.Length > 2 ? parts[2].Trim() : "Unknown";
                string platform = "Unknown";
                string hackUsed = "Unknown";

                if (parts.Length > 4)
                {
                    platform = parts[3].Trim();
                    hackUsed = string.Join(",", parts, 4, parts.Length - 4).Trim();
                }
                else if (parts.Length > 3)
                {
                    hackUsed = string.Join(",", parts, 3, parts.Length - 3).Trim();
                }

                return new CheaterRecord
                {
                    FriendCode = friendCode,
                    HashedPuid = hashedPuid,
                    PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Unknown" : playerName,
                    Platform = string.IsNullOrWhiteSpace(platform) ? "Unknown" : platform,
                    HackUsed = string.IsNullOrWhiteSpace(hackUsed) ? "Unknown" : hackUsed
                };
            }
            catch
            {
                return null;
            }
        }

        public static void AddPlayer(ClientData player, string hackUsed)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || player == null)
                return;

            string friendCode = player.FriendCode?.Trim() ?? "";
            string hashedPuid = "";

            try
            {
                hashedPuid = player.GetHashedPuid();
            }
            catch
            {
                hashedPuid = "";
            }

            if (hashedPuid == "e3b0cb855")
                hashedPuid = "";

            string playerName = player.PlayerName?.Trim() ?? "Sconosciuto";
            string platform = BanModIdentity.GetPlatform(player);
            string reason = string.IsNullOrWhiteSpace(hackUsed) ? "Hack Unknown" : hackUsed;

            AddDetected(friendCode, hashedPuid, playerName, platform, reason);
        }

        public static void AddDetected(
            string friendCode,
            string hashedPuid = "",
            string playerName = "",
            string platform = "",
            string reason = "")
        {
            friendCode = friendCode?.Trim() ?? "";
            hashedPuid = hashedPuid?.Trim() ?? "";

            if (hashedPuid == "e3b0cb855")
                hashedPuid = "";

            if (string.IsNullOrWhiteSpace(friendCode) && string.IsNullOrWhiteSpace(hashedPuid))
                return;

            CheaterRecord record = new CheaterRecord
            {
                FriendCode = friendCode,
                HashedPuid = hashedPuid,
                PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Sconosciuto" : playerName.Trim(),
                Platform = string.IsNullOrWhiteSpace(platform) ? "Unknown" : platform.Trim(),
                HackUsed = string.IsNullOrWhiteSpace(reason) ? "Hack Unknown" : reason.Trim()
            };

            bool isNew;
            lock (cacheLock)
            {
                isNew = string.IsNullOrWhiteSpace(FindExistingKeyNoLock(friendCode, hashedPuid));
                AddOrUpdateLocalRecordNoLock(record);
            }

            SaveLocalCache();

            if (isNew)
                BMLogger.Info($"[CheaterManager] Nuovo cheater locale: {record.PlayerName} [{friendCode}] [{hashedPuid}]", "AntiCheat");
            else
                BMLogger.Info($"[CheaterManager] Cheater aggiornato localmente: {record.PlayerName} [{friendCode}] [{hashedPuid}]", "AntiCheat");

            _ = SendToServerWhenReadyAsync(
                record.FriendCode,
                record.HashedPuid,
                record.PlayerName,
                record.Platform,
                record.HackUsed
            );
        }

        public static bool CheckList(string friendCode)
        {
            if (string.IsNullOrWhiteSpace(friendCode))
                return false;

            lock (cacheLock)
            {
                foreach (CheaterRecord record in cachedCheaters.Values)
                {
                    if (!string.IsNullOrWhiteSpace(record.FriendCode) &&
                        record.FriendCode.Equals(friendCode.Trim(), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        public static bool CheckList(string friendCode, string hashedPuid)
        {
            friendCode = friendCode?.Trim() ?? "";
            hashedPuid = hashedPuid?.Trim() ?? "";

            lock (cacheLock)
                return !string.IsNullOrWhiteSpace(FindExistingKeyNoLock(friendCode, hashedPuid));
        }

        public static bool CheckList(ClientData player)
        {
            if (player == null)
                return false;

            string friendCode = player.FriendCode?.Trim() ?? "";
            string hashedPuid = "";

            try
            {
                hashedPuid = player.GetHashedPuid();
            }
            catch
            {
                hashedPuid = "";
            }

            if (hashedPuid == "e3b0cb855")
                hashedPuid = "";

            lock (cacheLock)
            {
                foreach (CheaterRecord record in cachedCheaters.Values)
                {
                    bool friendCodeMatch =
                        !string.IsNullOrWhiteSpace(friendCode) &&
                        !string.IsNullOrWhiteSpace(record.FriendCode) &&
                        record.FriendCode.Equals(friendCode, StringComparison.OrdinalIgnoreCase);

                    bool puidMatch =
                        !string.IsNullOrWhiteSpace(hashedPuid) &&
                        !string.IsNullOrWhiteSpace(record.HashedPuid) &&
                        record.HashedPuid.Equals(hashedPuid, StringComparison.OrdinalIgnoreCase);

                    if (friendCodeMatch || puidMatch)
                        return true;
                }
            }

            return false;
        }

        private static void AddOrUpdateLocalRecord(CheaterRecord incoming)
        {
            if (incoming == null)
                return;

            lock (cacheLock)
                AddOrUpdateLocalRecordNoLock(incoming);
        }

        private static void AddOrUpdateLocalRecordNoLock(CheaterRecord incoming)
        {
            if (incoming == null)
                return;

            string existingKey = FindExistingKeyNoLock(incoming.FriendCode, incoming.HashedPuid);

            if (!string.IsNullOrWhiteSpace(existingKey) && cachedCheaters.TryGetValue(existingKey, out CheaterRecord existing))
            {
                existing.FriendCode = ChooseBetterValue(incoming.FriendCode, existing.FriendCode);
                existing.HashedPuid = ChooseBetterValue(incoming.HashedPuid, existing.HashedPuid);
                existing.PlayerName = ChooseBetterValue(incoming.PlayerName, existing.PlayerName, "Unknown", "Sconosciuto", "Sincronizzato");
                existing.Platform = ChooseBetterValue(incoming.Platform, existing.Platform, "Unknown");
                existing.HackUsed = ChooseBetterValue(incoming.HackUsed, existing.HackUsed, "Unknown", "ServerSync");

                string canonicalKey = GetCacheKey(existing.FriendCode, existing.HashedPuid);
                if (!string.IsNullOrWhiteSpace(canonicalKey) && !canonicalKey.Equals(existingKey, StringComparison.OrdinalIgnoreCase))
                {
                    cachedCheaters.Remove(existingKey);
                    cachedCheaters[canonicalKey] = existing;
                }
                return;
            }

            string key = GetCacheKey(incoming.FriendCode, incoming.HashedPuid);
            if (string.IsNullOrWhiteSpace(key))
                return;

            cachedCheaters[key] = new CheaterRecord
            {
                FriendCode = incoming.FriendCode?.Trim() ?? "",
                HashedPuid = incoming.HashedPuid?.Trim() ?? "",
                PlayerName = ChooseBetterValue(incoming.PlayerName, "Unknown", "Unknown", "Sconosciuto", "Sincronizzato"),
                Platform = ChooseBetterValue(incoming.Platform, "Unknown", "Unknown"),
                HackUsed = ChooseBetterValue(incoming.HackUsed, "ServerSync", "Unknown", "ServerSync")
            };
        }

        private static string FindExistingKeyNoLock(string friendCode, string hashedPuid)
        {
            friendCode = friendCode?.Trim() ?? "";
            hashedPuid = hashedPuid?.Trim() ?? "";

            foreach (KeyValuePair<string, CheaterRecord> pair in cachedCheaters)
            {
                CheaterRecord record = pair.Value;
                if (record == null)
                    continue;

                bool friendCodeMatch =
                    !string.IsNullOrWhiteSpace(friendCode) &&
                    !string.IsNullOrWhiteSpace(record.FriendCode) &&
                    record.FriendCode.Equals(friendCode, StringComparison.OrdinalIgnoreCase);

                bool puidMatch =
                    !string.IsNullOrWhiteSpace(hashedPuid) &&
                    !string.IsNullOrWhiteSpace(record.HashedPuid) &&
                    record.HashedPuid.Equals(hashedPuid, StringComparison.OrdinalIgnoreCase);

                if (friendCodeMatch || puidMatch)
                    return pair.Key;
            }

            return "";
        }

        private static string GetCacheKey(string friendCode, string hashedPuid)
        {
            friendCode = friendCode?.Trim() ?? "";
            hashedPuid = hashedPuid?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(hashedPuid))
                return "puid:" + hashedPuid.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(friendCode))
                return "fc:" + friendCode.ToLowerInvariant();

            return "";
        }

        private static string ChooseBetterValue(string incoming, string current, params string[] badValues)
        {
            if (string.IsNullOrWhiteSpace(incoming))
                return current;

            foreach (string bad in badValues)
            {
                if (incoming.Equals(bad, StringComparison.OrdinalIgnoreCase))
                    return current;
            }

            return incoming.Trim();
        }

        private static void SaveLocalCache()
        {
            try
            {
                Directory.CreateDirectory("DATA/DENIED");

                List<CheaterRecord> snapshot;
                lock (cacheLock)
                    snapshot = new List<CheaterRecord>(cachedCheaters.Values);

                snapshot.Sort((a, b) => string.Compare(
                    a?.FriendCode ?? a?.HashedPuid ?? "",
                    b?.FriendCode ?? b?.HashedPuid ?? "",
                    StringComparison.OrdinalIgnoreCase));

                StringBuilder builder = new StringBuilder();
                foreach (CheaterRecord record in snapshot)
                {
                    if (record != null)
                        builder.AppendLine(ToLocalLine(record));
                }

                File.WriteAllText(CheaterListPath, builder.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheaterManager] Errore salvataggio cache locale: {ex.Message}");
            }
        }

        private static string ToLocalLine(CheaterRecord record)
        {
            string friendCode = SanitizeCsvField(record.FriendCode);
            string hashedPuid = SanitizeCsvField(record.HashedPuid);
            string playerName = SanitizeCsvField(record.PlayerName);
            string platform = SanitizeCsvField(record.Platform);
            string hackUsed = SanitizeCsvField(record.HackUsed);

            return $"{friendCode},{hashedPuid},{playerName},{platform},{hackUsed}";
        }

        private static string SanitizeCsvField(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value.Replace(",", " ")
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Trim();
        }

        private static async Task SendToServerWhenReadyAsync(string friendCode, string hashedPuid, string playerName, string platform, string hackUsed)
        {
            string apiFriendCode = await WaitForFriendCodeAsync();
            if (string.IsNullOrWhiteSpace(apiFriendCode))
            {
                BMLogger.Warn("[CheaterManager] FriendCode non disponibile: cheater mantenuto solo nel file locale.", "AntiCheat");
                return;
            }

            for (int attempt = 1; attempt <= StartupHttpAttempts; attempt++)
            {
                if (await SendToServerAsync(apiFriendCode, friendCode, hashedPuid, playerName, platform, hackUsed))
                    return;

                if (attempt < StartupHttpAttempts)
                    await Task.Delay(1000 * attempt);
            }
        }

        private static async Task<bool> SendToServerAsync(string apiFriendCode, string friendCode, string hashedPuid, string playerName, string platform, string hackUsed)
        {
            try
            {
                string jsonPayload = "{"
                    + "\"friendCode\":\"" + BanModJson.Escape(friendCode) + "\","
                    + "\"hashedPuid\":\"" + BanModJson.Escape(hashedPuid) + "\","
                    + "\"playerName\":\"" + BanModJson.Escape(playerName) + "\","
                    + "\"platform\":\"" + BanModJson.Escape(string.IsNullOrWhiteSpace(platform) ? "Unknown" : platform) + "\","
                    + "\"hackUsed\":\"" + BanModJson.Escape(hackUsed) + "\""
                    + "}";

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BanModApiConfig.CheatersAddUrl);
                request.Headers.Add("X-BANMOD-FriendCode", apiFriendCode);
                string clientToken = BanModCore.GetCurrentClientToken();
                if (string.IsNullOrWhiteSpace(clientToken))
                    return false;
                request.Headers.Add("X-BANMOD-Client-Token", clientToken);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await httpClient.SendAsync(request);

                if ((int)response.StatusCode == 401)
                {
                    BanModApiTokenManager.ClearToken(apiFriendCode, clientToken);
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    BMLogger.Warn($"[CheaterManager] Server non ha accettato cheater: HTTP {(int)response.StatusCode}", "AntiCheat");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                BMLogger.Error($"[CheaterManager] Errore invio al server: {ex.Message}", "AntiCheat");
                return false;
            }
        }

        public static Task SyncFromServerAsync()
        {
            lock (startupSyncLock)
            {
                if (startupSyncTask == null)
                    startupSyncTask = SyncFromServerAtStartupAsync();

                return startupSyncTask;
            }
        }

        private static async Task SyncFromServerAtStartupAsync()
        {
            string apiFriendCode = await WaitForFriendCodeAsync();

            if (string.IsNullOrWhiteSpace(apiFriendCode))
            {
                BMLogger.Warn("[CheaterManager] Sincronizzazione iniziale saltata: FriendCode non disponibile.", "AntiCheat");
                return;
            }

            for (int attempt = 1; attempt <= StartupHttpAttempts; attempt++)
            {
                try
                {
                    using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BanModApiConfig.CheatersUrl);
                    request.Headers.Add("X-BANMOD-FriendCode", apiFriendCode);
                    string clientToken = BanModCore.GetCurrentClientToken();
                    if (string.IsNullOrWhiteSpace(clientToken))
                    {
                        await Task.Delay(StartupTokenWaitDelayMs);
                        continue;
                    }
                    request.Headers.Add("X-BANMOD-Client-Token", clientToken);

                    using HttpResponseMessage response = await httpClient.SendAsync(request);

                    if ((int)response.StatusCode == 401)
                    {
                        BMLogger.Warn("[CheaterManager] FriendCode rifiutato durante la sincronizzazione iniziale.", "AntiCheat");
                        return;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        BMLogger.Warn($"[CheaterManager] Lista server non disponibile: HTTP {(int)response.StatusCode} (tentativo {attempt}/{StartupHttpAttempts}).", "AntiCheat");
                    }
                    else
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        List<CheaterRecord> serverRecords = ParseServerCheaters(jsonResponse);

                        int before;
                        int after;
                        lock (cacheLock)
                        {
                            before = cachedCheaters.Count;
                            foreach (CheaterRecord record in serverRecords)
                                AddOrUpdateLocalRecordNoLock(record);
                            after = cachedCheaters.Count;
                        }

                        SaveLocalCache();

                        BMLogger.Info(
                            $"[CheaterManager] Lista server caricata all'avvio: ricevuti={serverRecords.Count}, aggiunti={Math.Max(0, after - before)}, totali={after}, file={CheaterListPath}",
                            "AntiCheat"
                        );
                        return;
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Warn($"[CheaterManager] Errore sincronizzazione iniziale (tentativo {attempt}/{StartupHttpAttempts}): {ex.Message}", "AntiCheat");
                }

                if (attempt < StartupHttpAttempts)
                    await Task.Delay(1500 * attempt);
            }
        }

        private static async Task<string> WaitForFriendCodeAsync()
        {
            for (int i = 0; i < StartupTokenWaitAttempts; i++)
            {
                string friendCode = GetAvailableFriendCode();
                string clientToken = BanModCore.GetCurrentClientToken();
                if (!string.IsNullOrWhiteSpace(friendCode) && !string.IsNullOrWhiteSpace(clientToken))
                    return friendCode;

                await Task.Delay(StartupTokenWaitDelayMs);
            }

            return "";
        }

        private static string GetAvailableFriendCode()
        {
            try
            {
                string friendCode = BanModCore.GetCurrentFriendCode();
                if (!string.IsNullOrWhiteSpace(friendCode))
                    return friendCode;
            }
            catch { }

            return "";
        }

        public static List<BanModDeniedPlayer> GetSnapshot()
        {
            List<BanModDeniedPlayer> result = new List<BanModDeniedPlayer>();

            lock (cacheLock)
            {
                foreach (CheaterRecord record in cachedCheaters.Values)
                {
                    if (record == null)
                        continue;

                    result.Add(new BanModDeniedPlayer
                    {
                        friendCode = record.FriendCode ?? "",
                        hashedPuid = record.HashedPuid ?? "",
                        accountId = record.HashedPuid ?? "",
                        playerName = record.PlayerName ?? "",
                        name = record.PlayerName ?? "",
                        platform = record.Platform ?? "Unknown",
                        hackUsed = record.HackUsed ?? "",
                        Reason = record.HackUsed ?? ""
                    });
                }
            }

            return result;
        }

        private static List<CheaterRecord> ParseServerCheaters(string json)
        {
            List<CheaterRecord> records = new List<CheaterRecord>();

            if (string.IsNullOrWhiteSpace(json))
                return records;

            try
            {
                MatchCollection objects = Regex.Matches(json, "\\{[^\\{\\}]*\"friendCode\"[^\\{\\}]*\\}");

                foreach (Match objectMatch in objects)
                {
                    string obj = objectMatch.Value;

                    string friendCode = ExtractJsonString(obj, "friendCode", "");
                    if (string.IsNullOrWhiteSpace(friendCode))
                        friendCode = ExtractJsonString(obj, "friend_code", "");

                    string hashedPuid = ExtractJsonString(obj, "hashedPuid", "");
                    if (string.IsNullOrWhiteSpace(hashedPuid))
                        hashedPuid = ExtractJsonString(obj, "hashed_puid", "");

                    if (string.IsNullOrWhiteSpace(hashedPuid))
                        hashedPuid = ExtractJsonString(obj, "accountId", "");

                    if (string.IsNullOrWhiteSpace(hashedPuid))
                        hashedPuid = ExtractJsonString(obj, "puid", "");

                    string playerName = ExtractJsonString(obj, "playerName", "");
                    if (string.IsNullOrWhiteSpace(playerName))
                        playerName = ExtractJsonString(obj, "player_name", "");
                    if (string.IsNullOrWhiteSpace(playerName))
                        playerName = ExtractJsonString(obj, "name", "Unknown");

                    string platform = ExtractJsonString(obj, "platform", "");

                    if (string.IsNullOrWhiteSpace(platform))
                        platform = ExtractJsonString(obj, "Platform", "Unknown");

                    string hackUsed = ExtractJsonString(obj, "hackUsed", "");
                    if (string.IsNullOrWhiteSpace(hackUsed))
                        hackUsed = ExtractJsonString(obj, "reason", "");
                    if (string.IsNullOrWhiteSpace(hackUsed))
                        hackUsed = ExtractJsonString(obj, "Reason", "ServerSync");

                    if (string.IsNullOrWhiteSpace(friendCode) && string.IsNullOrWhiteSpace(hashedPuid))
                        continue;

                    records.Add(new CheaterRecord
                    {
                        FriendCode = friendCode.Trim(),
                        HashedPuid = hashedPuid.Trim(),
                        PlayerName = playerName.Trim(),
                        Platform = string.IsNullOrWhiteSpace(platform) ? "Unknown" : platform.Trim(),
                        HackUsed = hackUsed.Trim()
                    });
                }
            }
            catch
            {
            }

            return records;
        }

        private static string ExtractJsonString(string json, string key, string fallback = "")
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

                int firstQuote = json.IndexOf('"', colonIndex + 1);

                if (firstQuote < 0)
                    return fallback;

                int secondQuote = firstQuote + 1;
                bool escaped = false;

                while (secondQuote < json.Length)
                {
                    char c = json[secondQuote];

                    if (c == '\\' && !escaped)
                    {
                        escaped = true;
                        secondQuote++;
                        continue;
                    }

                    if (c == '"' && !escaped)
                        break;

                    escaped = false;
                    secondQuote++;
                }

                if (secondQuote >= json.Length)
                    return fallback;

                return json.Substring(firstQuote + 1, secondQuote - firstQuote - 1)
                    .Replace("\\\"", "\"")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\\\", "\\");
            }
            catch
            {
                return fallback;
            }
        }
    }
}
