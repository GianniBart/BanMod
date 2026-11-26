using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using UnityEngine;

namespace BanMod;

internal static class SharedBotList
{
    internal sealed class Entry
    {
        [JsonPropertyName("friendCode")]
        public string FriendCode { get; set; }

        [JsonPropertyName("hashedPuid")]
        public string HashedPuid { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("unlockedByMe")]
        public bool UnlockedByMe { get; set; }

        [JsonPropertyName("flaggedByOthers")]
        public bool FlaggedByOthers { get; set; }
    }

    private sealed class ListResponse
    {
        public ListResponse() { }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("bots")]
        public List<Entry> Bots { get; set; }
    }

    private sealed class ReportResponse
    {
        public ReportResponse() { }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("protected")]
        public bool Protected { get; set; }
    }

    private sealed class ExceptionResponse
    {
        public ExceptionResponse() { }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("unlockedByMe")]
        public bool UnlockedByMe { get; set; }
    }

    private const float RefreshInterval = 30f;
    private static readonly HttpClient httpClient = new(
        new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly Dictionary<string, Entry> Entries =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> PendingReports =
        new(StringComparer.OrdinalIgnoreCase);

    private static string owner = "";
    private static bool wasHost;
    private static bool refreshing;
    private static bool cacheWriteWarning;
    private static float nextRefresh;

    internal static bool IsBusy => refreshing;

    private static string CurrentOwner() =>
        BanModCore.GetCurrentFriendCode()?.Trim() ?? "";

    private static bool SameFriendCode(string first, string second) =>
        string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

    internal static bool CanManage()
    {
        var client = AmongUsClient.Instance;
        return client != null && client.AmHost && !BanMod.IsBanModDisabled &&
            GameStates.isOnlineGame && GameStates.isLobby;
    }

    private const string BotListPath = "./DATA/DENIED/Bots.txt";

    private static bool textListInitialized;

    private static void EnsureTextList()
    {
        if (textListInitialized)
            return;

        textListInitialized = true;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BotListPath));
            if (!File.Exists(BotListPath))
            {
                File.WriteAllText(BotListPath, "");
                return;
            }

            var rows = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            bool legacyFormat = false;
            foreach (string line in File.ReadLines(BotListPath))
            {
                string value = line.Trim();
                Entry row;
                if (value.StartsWith("{", StringComparison.Ordinal))
                {
                    legacyFormat = true;
                    row = ReadLegacyBot(value);
                }
                else
                {
                    string[] fields = value.Split(',', 3);
                    legacyFormat |= fields.Length != 3;
                    row = new Entry
                    {
                        FriendCode = fields[0],
                        HashedPuid = fields.Length > 1 ? fields[1] : "",
                        PlayerName = fields.Length > 2 ? fields[2] : ""
                    };
                }

                if (row != null && IsBotCode(row.FriendCode))
                    rows[row.FriendCode] = row;
            }

            if (legacyFormat)
                WriteBotRows(rows.Values);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            WarnCacheWriteFailure();
        }
    }

    private static Entry ReadLegacyBot(string line)
    {
        try
        {
            using var json = JsonDocument.Parse(line);
            if (json.RootElement.ValueKind == JsonValueKind.Object &&
                json.RootElement.TryGetProperty("Bot", out var bot) &&
                bot.ValueKind == JsonValueKind.Object)
                return JsonSerializer.Deserialize<Entry>(bot.GetRawText());
        }
        catch (JsonException) { }

        return null;
    }

    private static bool IsBotCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length > 64)
            return false;

        int separator = code.IndexOf('#');
        if (separator <= 0 || separator == code.Length - 1 || separator != code.LastIndexOf('#'))
            return false;

        for (int i = 0; i < code.Length; i++)
        {
            if (i != separator && !char.IsLetterOrDigit(code[i]))
                return false;
        }

        return true;
    }

    private static void WriteBotRows(IEnumerable<Entry> rows)
    {
        var lines = new List<string>();
        foreach (Entry row in rows)
        {
            if (!IsBotCode(row.FriendCode))
                continue;

            string name = (row.PlayerName ?? "").Replace(',', ' ')
                .Replace('\r', ' ').Replace('\n', ' ').Trim();
            string puid = row.HashedPuid ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(puid, @"\A[0-9a-fA-F]{9}\z"))
                puid = "";
            lines.Add($"{row.FriendCode},{puid},{name}");
        }

        lines.Sort(StringComparer.OrdinalIgnoreCase);
        string temporaryPath = BotListPath + ".tmp";
        File.WriteAllLines(temporaryPath, lines);
        File.Move(temporaryPath, BotListPath, overwrite: true);
        cacheWriteWarning = false;
    }

    private static void WarnCacheWriteFailure()
    {
        if (!cacheWriteWarning)
            BMLogger.LogWarning("[BanMod][Bots] Impossibile aggiornare Bots.txt.");

        cacheWriteWarning = true;
    }

    private static void ReplaceEntries(IEnumerable<Entry> entries)
    {
        Entries.Clear();
        foreach (Entry entry in entries)
        {
            if (entry != null && !string.IsNullOrWhiteSpace(entry.FriendCode))
                Entries[entry.FriendCode.Trim()] = entry;
        }
    }

    private static void SaveCache()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BotListPath));
            WriteBotRows(Entries.Values);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            WarnCacheWriteFailure();
        }
    }

    private static void SyncOwner()
    {
        string current = CurrentOwner();
        if (SameFriendCode(owner, current))
            return;

        owner = current;
        Entries.Clear();
        nextRefresh = 0f;
        cacheWriteWarning = false;
    }

    private static bool IsCurrentOwner(string requestedOwner) =>
        SameFriendCode(requestedOwner, owner) &&
        SameFriendCode(requestedOwner, CurrentOwner());

    private static async Task<T> SendAsync<T>(
        HttpMethod method, string route, string requester, string token,
        string json = null) where T : class
    {
        try
        {
            using var request = new HttpRequestMessage(method, BanModCore.PublicApiBaseUrl + route);
            request.Headers.TryAddWithoutValidation("X-BANMOD-FriendCode", requester);
            request.Headers.TryAddWithoutValidation("X-BANMOD-Client-Token", token);
            if (json != null)
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(body);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return null;
        }
    }

    internal static void Tick()
    {
        EnsureTextList();
        SyncOwner();
        bool isHost = CanManage();

        if (isHost && !wasHost)
            nextRefresh = 0f;

        wasHost = isHost;

        if (!isHost || refreshing || owner.Length == 0 ||
            Time.realtimeSinceStartup < nextRefresh)
            return;

        string token = BanModCore.GetCurrentClientToken();
        if (string.IsNullOrWhiteSpace(token))
            return;

        refreshing = true;
        nextRefresh = Time.realtimeSinceStartup + RefreshInterval;
        try
        {
            AmongUsClient.Instance.StartCoroutine(Refresh(owner, token).WrapToIl2Cpp());
        }
        catch (Exception)
        {
            refreshing = false;
        }
    }

    private static IEnumerator Refresh(string requester, string token, Action onSuccess = null)
    {
        try
        {
            var task = SendAsync<ListResponse>(HttpMethod.Get, "/api/bots/list", requester, token);
            while (!task.IsCompleted)
                yield return null;

            if (task.Status != TaskStatus.RanToCompletion || !IsCurrentOwner(requester))
                yield break;

            var response = task.Result;
            if (response == null || !response.Success || response.Bots == null)
                yield break;

            ReplaceEntries(response.Bots);
            SaveCache();
            onSuccess?.Invoke();
        }
        finally
        {
            refreshing = false;
        }
    }

    internal static bool TryGet(string friendCode, out Entry entry)
    {
        SyncOwner();
        entry = null;
        return !string.IsNullOrWhiteSpace(friendCode) &&
            Entries.TryGetValue(friendCode.Trim(), out entry);
    }

    internal static List<Entry> GetEntries()
    {
        SyncOwner();
        var rows = new List<Entry>(Entries.Values);
        rows.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(
            a.PlayerName ?? a.FriendCode, b.PlayerName ?? b.FriendCode));
        return rows;
    }

    internal static void Report(ClientData player, string reason)
    {
        if (!CanManage() || player == null || string.IsNullOrWhiteSpace(player.FriendCode))
            return;

        SyncOwner();
        string token = BanModCore.GetCurrentClientToken();
        if (owner.Length == 0 || string.IsNullOrWhiteSpace(token))
            return;

        string friendCode = player.FriendCode.Trim();
        string reportKey = owner + "|" + friendCode;
        if (!PendingReports.Add(reportKey))
            return;

        var client = AmongUsClient.Instance;
        try
        {
            client.StartCoroutine(SendReport(client.GameId, player.Id, owner, token,
                friendCode, player.GetHashedPuid(), BanMod.GetRealPlayerName(player), reason, reportKey).WrapToIl2Cpp());
        }
        catch (Exception)
        {
            PendingReports.Remove(reportKey);
        }
    }

    private static IEnumerator SendReport(int gameId, int clientId, string requester,
        string token, string friendCode, string hashedPuid, string playerName, string reason, string reportKey)
    {
        try
        {
            string json = JsonSerializer.Serialize(new { friendCode, hashedPuid, playerName, reason });
            var task = SendAsync<ReportResponse>(
                HttpMethod.Post, "/api/bots/add", requester, token, json);
            while (!task.IsCompleted)
                yield return null;

            if (task.Status != TaskStatus.RanToCompletion || !IsCurrentOwner(requester))
                yield break;

            var response = task.Result;
            if (response == null || !response.Success || response.Protected)
                yield break;

            while (refreshing && IsCurrentOwner(requester) && CanManage())
                yield return null;

            if (!IsCurrentOwner(requester) || !CanManage())
                yield break;

            bool refreshed = false;
            refreshing = true;
            nextRefresh = Time.realtimeSinceStartup + RefreshInterval;
            var refresh = Refresh(requester, token, () => refreshed = true);
            try
            {
                while (refresh.MoveNext())
                    yield return refresh.Current;
            }
            finally
            {
                (refresh as IDisposable)?.Dispose();
            }

            if (!refreshed || !CanManage() || !IsCurrentOwner(requester) ||
                !TryGet(friendCode, out var entry) || entry.UnlockedByMe)
                yield break;

            var client = AmongUsClient.Instance;
            if (client.GameId != gameId)
                yield break;

            var current = client.GetClient(clientId);
            if (current != null && !BanMod.IsProtected(current) &&
                SameFriendCode(current.FriendCode?.Trim(), friendCode))
                client.KickPlayer(clientId, true);
        }
        finally
        {
            PendingReports.Remove(reportKey);
        }
    }

    internal static void SetException(string friendCode, bool unlocked, Action<bool> onComplete)
    {
        if (refreshing || !CanManage() || !TryGet(friendCode, out var entry))
        {
            onComplete?.Invoke(false);
            return;
        }

        string token = BanModCore.GetCurrentClientToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            onComplete?.Invoke(false);
            return;
        }

        refreshing = true;
        try
        {
            AmongUsClient.Instance.StartCoroutine(ChangeException(
                owner, token, entry.FriendCode.Trim(), unlocked, onComplete).WrapToIl2Cpp());
        }
        catch (Exception)
        {
            refreshing = false;
            onComplete?.Invoke(false);
        }
    }

    private static IEnumerator ChangeException(string requester, string token,
        string friendCode, bool unlocked, Action<bool> onComplete)
    {
        bool success = false;
        try
        {
            string json = JsonSerializer.Serialize(new { friendCode, unlocked });
            var task = SendAsync<ExceptionResponse>(
                HttpMethod.Post, "/api/bots/exception", requester, token, json);
            while (!task.IsCompleted)
                yield return null;

            if (task.Status != TaskStatus.RanToCompletion || !IsCurrentOwner(requester))
                yield break;

            var response = task.Result;
            if (response == null || !response.Success || response.UnlockedByMe != unlocked)
                yield break;

            if (Entries.TryGetValue(friendCode, out var entry))
                entry.UnlockedByMe = response.UnlockedByMe;

            nextRefresh = 0f;
            success = true;
        }
        finally
        {
            refreshing = false;
            onComplete?.Invoke(success);
        }
    }
}
