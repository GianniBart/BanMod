//using AmongUs.GameOptions;
//using HarmonyLib;
//using Il2CppInterop.Runtime.Injection;
//using InnerNet;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace BanMod;

//public sealed class DiscordAutomuteManager : MonoBehaviour
//{
//    public enum MatchPhase
//    {
//        Lobby,
//        Tasks,
//        Meeting
//    }

//    private enum RequestKind
//    {
//        State,
//        Test
//    }

//    private sealed class PlayerState
//    {
//        public int ClientId;
//        public string Name;
//        public string FriendCode;
//        public bool Alive;
//        public string Team;
//    }

//    private sealed class PendingRequest
//    {
//        public UnityWebRequest Request;
//        public UnityWebRequestAsyncOperation Operation;
//        public RequestKind Kind;
//        public long Sequence;
//        public string Reason;
//    }

//    public static DiscordAutomuteManager Instance { get; private set; }

//    private const string AutomuteApiUrl = BanModCore.PublicApiBaseUrl + "/api/discord/automute";

//    private const float StateCheckInterval = 0.20f;
//    private const float HeartbeatInterval = 20f;
//    private const float DebounceDelay = 0.25f;
//    private const float DefaultRateLimitRetry = 1.0f;

//    private static bool _registered;

//    private string _status = "Ready";

//    private MatchPhase _phase = MatchPhase.Lobby;

//    private float _stateCheckTimer;
//    private float _heartbeatTimer;
//    private long _sequence;

//    private string _lastStateSignature = "";

//    private bool _stateSendPending;
//    private float _stateSendTimer;
//    private string _pendingReason = "";

//    private float _rateLimitTimer;
//    private bool _retryLatestState;

//    private readonly List<PendingRequest> _requests = new();


//    public DiscordAutomuteManager(IntPtr ptr) : base(ptr) { }

//    public static void EnsureCreated()
//    {
//        if (Instance != null)
//            return;

//        try
//        {
//            if (!_registered)
//            {
//                ClassInjector.RegisterTypeInIl2Cpp<DiscordAutomuteManager>();
//                _registered = true;
//            }

//            GameObject obj = new("BanMod_DiscordAutomuteManager");
//            obj.hideFlags = HideFlags.HideAndDontSave;
//            DontDestroyOnLoad(obj);

//            Instance = obj.AddComponent<DiscordAutomuteManager>();
//        }
//        catch (Exception ex)
//        {
//            try
//            {
//                Debug.LogError("[DiscordAutomute] EnsureCreated failed: " + ex);
//            }
//            catch { }
//        }
//    }

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;

//        _stateCheckTimer = StateCheckInterval;
//    }

//    private void Update()
//    {
//        try
//        {
//            PollRequests();

//            UpdateRateLimit();
//            UpdateQueuedStateSend();

//            if (!IsEnabled() || !IsLocalHost() || !HasValidLobby())
//            {
//                _stateCheckTimer = 0f;
//                _heartbeatTimer = 0f;
//                _lastStateSignature = "";
//                _stateSendPending = false;
//                return;
//            }

//            _stateCheckTimer += Time.unscaledDeltaTime;

//            if (_stateCheckTimer >= StateCheckInterval)
//            {
//                _stateCheckTimer = 0f;
//                CheckForStateChange();
//            }

//            _heartbeatTimer += Time.unscaledDeltaTime;

//            if (_heartbeatTimer >= HeartbeatInterval)
//            {
//                _heartbeatTimer = 0f;
//                PublishCurrentState("heartbeat");
//            }
//        }
//        catch (Exception ex)
//        {
//            _status = "Automute update error";

//            try
//            {
//                Debug.LogError("[DiscordAutomute] Update failed: " + ex);
//            }
//            catch { }
//        }
//    }

//    private void UpdateQueuedStateSend()
//    {
//        if (!_stateSendPending)
//            return;

//        _stateSendTimer += Time.unscaledDeltaTime;

//        if (_stateSendTimer < DebounceDelay)
//            return;

//        _stateSendPending = false;
//        _stateSendTimer = 0f;

//        string reason = string.IsNullOrWhiteSpace(_pendingReason)
//            ? "state-change"
//            : _pendingReason;

//        _pendingReason = "";
//        PublishCurrentState(reason);
//    }

//    private void UpdateRateLimit()
//    {
//        if (_rateLimitTimer <= 0f)
//            return;

//        _rateLimitTimer -= Time.unscaledDeltaTime;

//        if (_rateLimitTimer > 0f)
//            return;

//        _rateLimitTimer = 0f;

//        if (_retryLatestState)
//        {
//            _retryLatestState = false;
//            QueueStatePublish("rate-limit-retry");
//        }
//    }

//    private void QueueStatePublish(string reason)
//    {
//        _stateSendPending = true;
//        _stateSendTimer = 0f;
//        _pendingReason = reason ?? "state-change";
//    }

//    private void CheckForStateChange()
//    {
//        MatchPhase detected = DetectPhase();

//        if (detected != _phase)
//            _phase = detected;

//        string signature = BuildStateJson(0);

//        if (signature == _lastStateSignature)
//            return;

//        string oldSignature = _lastStateSignature;
//        _lastStateSignature = signature;

//        string reason = string.IsNullOrEmpty(oldSignature)
//            ? "initial-state"
//            : "state-change";

//        QueueStatePublish(reason);
//    }

//    private static MatchPhase DetectPhase()
//    {
//        if (!IsGameStarted())
//            return MatchPhase.Lobby;

//        try
//        {
//            if (MeetingHud.Instance != null)
//                return MatchPhase.Meeting;
//        }
//        catch { }

//        try
//        {
//            if (ExileController.Instance != null)
//                return MatchPhase.Meeting;
//        }
//        catch { }

//        return MatchPhase.Tasks;
//    }

//    private static bool IsGameStarted()
//    {
//        try
//        {
//            return AmongUsClient.Instance != null &&
//                   AmongUsClient.Instance.IsGameStarted;
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    private void PollRequests()
//    {
//        for (int i = _requests.Count - 1; i >= 0; i--)
//        {
//            PendingRequest pending = _requests[i];

//            if (pending == null ||
//                pending.Operation == null ||
//                !pending.Operation.isDone)
//            {
//                continue;
//            }

//            bool success = false;
//            long responseCode = 0;
//            string responseText = "";

//            try
//            {
//                UnityWebRequest req = pending.Request;

//                if (req != null)
//                {
//                    responseCode = req.responseCode;
//                    responseText = req.downloadHandler?.text ?? "";
//                }

//#if UNITY_2020_2_OR_NEWER
//                success = req != null &&
//                          req.result == UnityWebRequest.Result.Success;
//#else
//                success = req != null &&
//                          !req.isNetworkError &&
//                          !req.isHttpError;
//#endif

//                if (responseCode == 429)
//                {
//                    float retryAfter = ParseRetryAfterSeconds(responseText);

//                    if (retryAfter <= 0f)
//                        retryAfter = DefaultRateLimitRetry;

//                    _rateLimitTimer = Math.Max(_rateLimitTimer, retryAfter);

//                    if (pending.Kind == RequestKind.State)
//                        _retryLatestState = true;

//                    _status = $"Discord rate limit - retry in {retryAfter:0.##}s";
//                    success = false;
//                }
//                else if (!success && req != null)
//                {
//                    try
//                    {
//                        Debug.LogError(
//                            "[DiscordAutomute] Discord webhook HTTP " +
//                            req.responseCode + ": " +
//                            (req.error ?? "unknown error"));
//                    }
//                    catch { }
//                }
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogError("[DiscordAutomute] Request poll failed: " + ex);
//                }
//                catch { }
//            }

//            if (responseCode != 429)
//            {
//                if (pending.Kind == RequestKind.Test)
//                {
//                    _status = success
//                        ? "Webhook test successful"
//                        : "Webhook test failed";
//                }
//                else
//                {
//                    _status = success
//                        ? $"Sent #{pending.Sequence} ({pending.Reason})"
//                        : $"Send failed #{pending.Sequence}";
//                }
//            }

//            try
//            {
//                pending.Request?.Dispose();
//            }
//            catch { }

//            _requests.RemoveAt(i);
//        }
//    }

//    private static float ParseRetryAfterSeconds(string json)
//    {
//        if (string.IsNullOrWhiteSpace(json))
//            return DefaultRateLimitRetry;

//        try
//        {
//            const string key = "\"retry_after\"";
//            int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);

//            if (keyIndex < 0)
//                return DefaultRateLimitRetry;

//            int colon = json.IndexOf(':', keyIndex + key.Length);
//            if (colon < 0)
//                return DefaultRateLimitRetry;

//            int start = colon + 1;
//            while (start < json.Length && char.IsWhiteSpace(json[start]))
//                start++;

//            bool quoted = start < json.Length && json[start] == '"';
//            if (quoted)
//                start++;

//            int end = start;
//            while (end < json.Length)
//            {
//                char c = json[end];
//                if ((c >= '0' && c <= '9') || c == '.' || c == ',')
//                {
//                    end++;
//                    continue;
//                }

//                break;
//            }

//            string number = json.Substring(start, end - start).Replace(',', '.');

//            if (float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
//                return Math.Max(0.1f, result);
//        }
//        catch { }

//        return DefaultRateLimitRetry;
//    }

//    private static bool IsEnabled()
//    {
//        try
//        {
//            return Options.AutoMuteForDiscord.GetBool();
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    private static bool HasValidLobby()
//    {
//        try
//        {
//            return AmongUsClient.Instance != null &&
//                   AmongUsClient.Instance.GameId != 0;
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    private static bool IsLocalHost()
//    {
//        try
//        {
//            return AmongUsClient.Instance != null &&
//                   AmongUsClient.Instance.AmHost;
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    private void PublishCurrentState(string reason)
//    {
//        if (!IsEnabled() || !IsLocalHost() || !HasValidLobby())
//            return;


//        if (_rateLimitTimer > 0f)
//        {
//            _retryLatestState = true;
//            _status = $"Rate limited - latest state queued ({_rateLimitTimer:0.##}s)";
//            return;
//        }

//        long seq = ++_sequence;
//        string stateJson;

//        try
//        {
//            _phase = DetectPhase();
//            stateJson = BuildStateJson(seq);
//            _lastStateSignature = BuildStateJson(0);
//        }
//        catch (Exception ex)
//        {
//            _status = "Could not build game state";

//            try
//            {
//                Debug.LogError("[DiscordAutomute] BuildState failed: " + ex);
//            }
//            catch { }

//            return;
//        }

//        if (stateJson.Length >= 1900)
//        {
//            _status = "State JSON is too large for Discord";

//            try
//            {
//                Debug.LogError(
//                    "[DiscordAutomute] State too large for Discord: " +
//                    stateJson.Length + " chars");
//            }
//            catch { }

//            return;
//        }

//        SendAutomuteState(
//            stateJson,
//            RequestKind.State,
//            seq,
//            reason);
//    }

//    private string BuildStateJson(long seq)
//    {
//        List<PlayerState> players = new();

//        foreach (ClientData client in BanMod.AllClients)
//        {
//            if (client == null || client.Character == null)
//                continue;

//            PlayerControl pc = client.Character;

//            if (pc.Data == null || pc.Data.Disconnected)
//                continue;

//            players.Add(
//                new PlayerState
//                {
//                    ClientId = client.Id,
//                    Name = BanMod.GetRealPlayerName(client),
//                    FriendCode = client.FriendCode ?? "",
//                    Alive = !pc.Data.IsDead,
//                    Team = GetPlayerTeam(pc)
//                });
//        }

//        players.Sort((a, b) => a.ClientId.CompareTo(b.ClientId));

//        StringBuilder sb = new();

//        sb.Append('{');
//        sb.Append("\"v\":2,");
//        sb.Append("\"seq\":").Append(seq).Append(',');
//        sb.Append("\"host\":\"")
//          .Append(EscapeJson(GetHostFriendCode()))
//          .Append("\",");
//        sb.Append("\"code\":\"")
//          .Append(EscapeJson(GetLobbyCode()))
//          .Append("\",");
//        sb.Append("\"phase\":\"")
//          .Append(PhaseToString(_phase))
//          .Append("\",");
//        sb.Append("\"players\":[");

//        for (int i = 0; i < players.Count; i++)
//        {
//            if (i > 0)
//                sb.Append(',');

//            PlayerState p = players[i];

//            sb.Append('{');
//            sb.Append("\"name\":\"")
//              .Append(EscapeJson(p.Name ?? ""))
//              .Append("\",");
//            sb.Append("\"fc\":\"")
//              .Append(EscapeJson(p.FriendCode ?? ""))
//              .Append("\",");
//            sb.Append("\"alive\":")
//              .Append(p.Alive ? "true" : "false")
//              .Append(',');
//            sb.Append("\"team\":\"")
//              .Append(EscapeJson(p.Team ?? "unknown"))
//              .Append("\"");
//            sb.Append('}');
//        }

//        sb.Append("]}");
//        return sb.ToString();
//    }

//    private string GetPlayerTeam(PlayerControl pc)
//    {
//        if (_phase != MatchPhase.Tasks || pc == null || pc.Data == null)
//            return "unknown";

//        try
//        {
//            if (pc.Data.Role == null)
//                return "unknown";

//            return pc.Data.Role.TeamType == RoleTeamTypes.Impostor
//                ? "impostor"
//                : "crewmate";
//        }
//        catch (Exception ex)
//        {
//            try
//            {
//                Debug.LogWarning("[DiscordAutomute] Could not read team: " + ex.Message);
//            }
//            catch { }

//            return "unknown";
//        }
//    }

//    private static string GetHostFriendCode()
//    {
//        try
//        {
//            if (AmongUsClient.Instance == null)
//                return "";

//            int hostId = AmongUsClient.Instance.HostId;

//            foreach (ClientData client in BanMod.AllClients)
//            {
//                if (client != null && client.Id == hostId)
//                    return client.FriendCode ?? "";
//            }

//            if (PlayerControl.LocalPlayer != null)
//            {
//                int ownerId = PlayerControl.LocalPlayer.OwnerId;
//                ClientData local = AmongUsClient.Instance.GetClient(ownerId);

//                if (local != null)
//                    return local.FriendCode ?? "";
//            }
//        }
//        catch { }

//        return "";
//    }

//    private static string GetLobbyCode()
//    {
//        try
//        {
//            if (AmongUsClient.Instance == null)
//                return "";

//            return GameCode.IntToGameName(AmongUsClient.Instance.GameId);
//        }
//        catch
//        {
//            try
//            {
//                return AmongUsClient.Instance != null
//                    ? AmongUsClient.Instance.GameId.ToString()
//                    : "";
//            }
//            catch
//            {
//                return "";
//            }
//        }
//    }

//    private void SendAutomuteState(
//        string json,
//        RequestKind kind,
//        long sequence,
//        string reason)
//    {
//        try
//        {
//            if (kind == RequestKind.State && _rateLimitTimer > 0f)
//            {
//                _retryLatestState = true;
//                return;
//            }

//            byte[] body = Encoding.UTF8.GetBytes(json);

//            UnityWebRequest request = new(
//                AutomuteApiUrl,
//                UnityWebRequest.kHttpVerbPOST);

//            request.uploadHandler = new UploadHandlerRaw(body);
//            request.downloadHandler = new DownloadHandlerBuffer();

//            request.SetRequestHeader("Content-Type", "application/json");
//            request.SetRequestHeader("Accept", "application/json");
//            BanModApiTokenManager.ApplyAuthHeader(request);
//            request.timeout = 5;

//            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

//            _requests.Add(
//                new PendingRequest
//                {
//                    Request = request,
//                    Operation = operation,
//                    Kind = kind,
//                    Sequence = sequence,
//                    Reason = reason ?? ""
//                });
//        }
//        catch (Exception ex)
//        {
//            _status = kind == RequestKind.Test
//                ? "Webhook test failed"
//                : $"Send failed #{sequence}";

//            try
//            {
//                Debug.LogError("[DiscordAutomute] Automute API request failed: " + ex);
//            }
//            catch { }
//        }
//    }

//    private static string EscapeJson(string value)
//    {
//        if (string.IsNullOrEmpty(value))
//            return "";

//        StringBuilder sb = new(value.Length + 16);

//        foreach (char c in value)
//        {
//            switch (c)
//            {
//                case '"':
//                    sb.Append("\\\"");
//                    break;

//                case '\\':
//                    sb.Append("\\\\");
//                    break;

//                case '\b':
//                    sb.Append("\\b");
//                    break;

//                case '\f':
//                    sb.Append("\\f");
//                    break;

//                case '\n':
//                    sb.Append("\\n");
//                    break;

//                case '\r':
//                    sb.Append("\\r");
//                    break;

//                case '\t':
//                    sb.Append("\\t");
//                    break;

//                default:
//                    if (c < 32)
//                    {
//                        sb.Append("\\u");
//                        sb.Append(((int)c).ToString("x4"));
//                    }
//                    else
//                    {
//                        sb.Append(c);
//                    }
//                    break;
//            }
//        }

//        return sb.ToString();
//    }

//    private static string PhaseToString(MatchPhase phase)
//    {
//        return phase switch
//        {
//            MatchPhase.Tasks => "tasks",
//            MatchPhase.Meeting => "meeting",
//            _ => "lobby"
//        };
//    }
//}

//[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
//internal static class DiscordAutomuteBootstrapPatch
//{
//    private static bool _done;

//    [HarmonyPostfix]
//    private static void Postfix()
//    {
//        if (_done)
//            return;

//        _done = true;
//        DiscordAutomuteManager.EnsureCreated();
//    }
//}
