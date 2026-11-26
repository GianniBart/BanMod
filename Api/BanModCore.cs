//credits and licenses in the resources folder/
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
using InnerNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using static BanMod.Utils.CheatUtils;

namespace BanMod
{
    public static class BanModCore
    {
        private const string ApiBaseUrl = "https://banmodapi.banmod.online";
        public const string PublicApiBaseUrl = ApiBaseUrl;
        private const string ActivationChallengeUrl = ApiBaseUrl + "/api/activation/challenge";
        private const string ActivationVerifyUrl = ApiBaseUrl + "/api/activation/verify";
        private const string AccessUrl = ApiBaseUrl + "/api/access";
        private const string LobbyStatusUrl = ApiBaseUrl + "/api/lobby/status";
        private const string ActiveLobbiesUrl = ApiBaseUrl + "/api/lobbies/active";
        private const string LoginManifestUrl = ApiBaseUrl + "/api/login/manifest";

        private const int RequestTimeoutSeconds = 15;
        private const int IdentityMaxAttempts = 2147483647;
        private const float IdentityRetrySeconds = 1f;
        private const float LobbyStatusIntervalSeconds = 20f;
        private const float LobbyListIntervalSeconds = 30f;
        private const float PremiumRefreshIntervalSeconds = 120f;

        private static bool _startupRequested = true;
        private static bool _startupStarted;
        private static bool _loopStarted;
        private static bool _statusRunning;
        private static bool _lobbyListRunning;

        private static float _nextLobbyStatusRealtime;
        private static string _lastLobbyStatusSnapshot = "";
        private static bool _premiumRefreshLoopStarted;
        private static bool _premiumRefreshRunning;
        private static bool _premiumAccessFailureLogged;
        private static bool _premiumRefreshRequested;
        private static float _statusStartedRealtime;
        private static int _lastLobbySceneHandle = int.MinValue;

        private static string _friendCode = "";
        private static string _playerName = "";
        private static string _activationToken = "";
        private static long _activationTokenExpiresAtUnix = 0;
        private static bool _activationTokenRecoveryOnly;
        private static readonly string SyncSessionId = Guid.NewGuid().ToString("N");
        private static string _clientToken = "";
        private static string _clientTokenFriendCode = "";
        private const string ClientTokenPrefsPrefix = "BANMOD_CLIENT_TOKEN_V1_";
        private static string _capturedFriendCode = "";
        private static string _capturedPlayerName = "";
        private static int _capturedClientId = -1;

        public static List<ModdedLobbyInfo> LastModdedLobbies { get; private set; } = new List<ModdedLobbyInfo>();

        private static readonly Dictionary<string, LoadedPremiumModule> LoadedPremiumModules =
            new Dictionary<string, LoadedPremiumModule>(StringComparer.OrdinalIgnoreCase);

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static void CaptureClientData(ClientData client)
        {
            if (client == null)
                return;

            try
            {
                string friendCode = Norm(client.FriendCode);
                string playerName = Norm(client.PlayerName);

                if (!string.IsNullOrWhiteSpace(friendCode))
                {
                    _capturedFriendCode = friendCode;
                    _capturedPlayerName = string.IsNullOrWhiteSpace(playerName) ? _capturedPlayerName : playerName;
                    _capturedClientId = client.Id;

                }
            }
            catch { }
        }

        public static void CaptureClientName(ClientData client)
        {
            if (client == null)
                return;

            try
            {
                string playerName = Norm(client.PlayerName);

                if (!string.IsNullOrWhiteSpace(playerName) && client.Id == _capturedClientId)
                    _capturedPlayerName = playerName;
            }
            catch { }
        }

        private static string ExtractNameFromFriendCode(string friendCode)
        {
            if (string.IsNullOrWhiteSpace(friendCode))
                return "";

            int idx = friendCode.IndexOf('#');

            if (idx > 0)
                return friendCode.Substring(0, idx).Trim();

            return friendCode.Trim();
        }

        public static void TryCaptureFriendCodeFromOriginalAccountObject(object source)
        {
            if (!string.IsNullOrWhiteSpace(_friendCode))
                return;

            try
            {
                string found = ScanOriginalDataObjectForFriendCode(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));

                if (!string.IsNullOrWhiteSpace(found))
                {
                    _capturedFriendCode = found.Trim();

                    if (string.IsNullOrWhiteSpace(_capturedPlayerName))
                        _capturedPlayerName = ExtractNameFromFriendCode(_capturedFriendCode);

                }
            }
            catch { }
        }

        private static string ScanOriginalDataObjectForFriendCode(object obj, int depth, HashSet<object> visited)
        {
            if (obj == null || depth > 3)
                return "";

            Type t;
            try
            {
                t = obj.GetType();

                if (t.IsPrimitive || t.IsEnum)
                    return "";

                if (obj is string s)
                    return LooksLikeFriendCode(s) ? s.Trim() : "";
            }
            catch
            {
                return "";
            }

            try
            {
                if (!visited.Add(obj))
                    return "";
            }
            catch { }

            try
            {
                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    string fname = f.Name ?? "";

                    if (fname.IndexOf("friend", StringComparison.OrdinalIgnoreCase) < 0 &&
                        fname.IndexOf("code", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    object value = null;
                    try { value = f.GetValue(obj); } catch { }

                    string found = ExtractFriendCodeFromDataValue(value, depth, visited);

                    if (!string.IsNullOrWhiteSpace(found))
                        return found;
                }

                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!p.CanRead)
                        continue;

                    string pname = p.Name ?? "";

                    if (pname.IndexOf("friend", StringComparison.OrdinalIgnoreCase) < 0 &&
                        pname.IndexOf("code", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    object value = null;
                    try { value = p.GetValue(obj, null); } catch { }

                    string found = ExtractFriendCodeFromDataValue(value, depth, visited);

                    if (!string.IsNullOrWhiteSpace(found))
                        return found;
                }
            }
            catch { }

            return "";
        }

        private static string ExtractFriendCodeFromDataValue(object value, int depth, HashSet<object> visited)
        {
            if (value == null)
                return "";

            if (value is string s)
                return LooksLikeFriendCode(s) ? s.Trim() : "";

            if (depth >= 3)
                return "";

            return ScanOriginalDataObjectForFriendCode(value, depth + 1, visited);
        }

        private static bool LooksLikeFriendCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            int hash = value.IndexOf('#');
            if (hash <= 0 || hash >= value.Length - 1)
                return false;

            string left = value.Substring(0, hash);
            string right = value.Substring(hash + 1);

            if (left.Length < 2 || left.Length > 32)
                return false;

            if (right.Length < 4 || right.Length > 8)
                return false;

            for (int i = 0; i < right.Length; i++)
            {
                if (!char.IsDigit(right[i]))
                    return false;
            }

            return true;
        }

        public static bool TryCaptureEosFriendCode(string source)
        {
            try
            {
                if (!DestroyableSingleton<EOSManager>.InstanceExists)
                    return false;

                EOSManager eos = DestroyableSingleton<EOSManager>.Instance;

                if (eos == null)
                    return false;

                string friendCode = Norm(eos.FriendCode);

                if (string.IsNullOrWhiteSpace(friendCode))
                    return false;

                string playerName = ExtractNameFromFriendCode(friendCode);

                CaptureIdentity(friendCode, playerName, source);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CaptureIdentity(string friendCode, string playerName, string source)
        {
            friendCode = Norm(friendCode);
            playerName = Norm(playerName);

            if (string.IsNullOrWhiteSpace(friendCode))
                return;

            if (string.IsNullOrWhiteSpace(playerName))
                playerName = ExtractNameFromFriendCode(friendCode);


            _friendCode = friendCode;
            _playerName = string.IsNullOrWhiteSpace(playerName) ? "Unknown" : playerName;

        }


        public static string GetCurrentFriendCode()
        {
            try { return _friendCode ?? ""; } catch { return ""; }
        }

        public static string GetCurrentPlayerName()
        {
            try { return _playerName ?? ""; } catch { return ""; }
        }

        public static string GetCurrentActivationToken()
        {
            try { return _friendCode ?? ""; } catch { return ""; }
        }

        public static string GetCurrentBanModSha256()
        {
            try { return SafeGetOwnBanModSha256(); } catch { return ""; }
        }

        public static string GetCurrentBuildId()
        {
            try { return BanModBuildSecret.BuildId ?? ""; } catch { return ""; }
        }

        public static void RequestPremiumRefresh()
        {
            _premiumRefreshRequested = true;
        }

        public static bool IsPluginLoaded(string serviceKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serviceKey))
                    return false;

                return LoadedPremiumModules.ContainsKey(serviceKey.Trim());
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAnyPluginLoaded(params string[] serviceKeys)
        {
            try
            {
                if (serviceKeys == null || serviceKeys.Length == 0)
                    return false;

                for (int i = 0; i < serviceKeys.Length; i++)
                {
                    if (IsPluginLoaded(serviceKeys[i]))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string[] GetLoadedPluginKeys()
        {
            try
            {
                string[] keys = new string[LoadedPremiumModules.Keys.Count];
                LoadedPremiumModules.Keys.CopyTo(keys, 0);
                return keys;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public static void ClearCurrentActivationToken()
        {
            try
            {
                _activationToken = "";
                _activationTokenExpiresAtUnix = 0;
                _activationTokenRecoveryOnly = false;
            }
            catch { }
        }

        public static string GetCurrentClientToken()
        {
            try
            {
                string friendCode = Norm(_friendCode);
                if (string.IsNullOrWhiteSpace(friendCode))
                    return "";

                if (!string.Equals(
                        Norm(_clientTokenFriendCode),
                        friendCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _clientToken = LoadClientToken(friendCode);
                    _clientTokenFriendCode = friendCode;
                }

                return _clientToken ?? "";
            }
            catch
            {
                return "";
            }
        }

        // Safe on an HTTP worker: no Unity/PlayerPrefs access. Invalidate only
        // the exact credential rejected for the current account, never a newer
        // token installed while this request was in flight.
        public static void RejectClientToken(string friendCode, string rejectedToken)
        {
            if (string.IsNullOrWhiteSpace(rejectedToken) ||
                !string.Equals(_friendCode, friendCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(_clientTokenFriendCode, friendCode, StringComparison.OrdinalIgnoreCase))
                return;
            System.Threading.Interlocked.CompareExchange(ref _clientToken, "", rejectedToken);
        }

        private static string ClientTokenPrefsKey(string friendCode)
        {
            friendCode = Norm(friendCode);
            if (string.IsNullOrWhiteSpace(friendCode))
                return ClientTokenPrefsPrefix + "UNKNOWN";

            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(friendCode.ToLowerInvariant()));
            StringBuilder sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return ClientTokenPrefsPrefix + sb.ToString();
        }

        private static string LoadClientToken(string friendCode)
        {
            try
            {
                return (PlayerPrefs.GetString(ClientTokenPrefsKey(friendCode), "") ?? "").Trim();
            }
            catch
            {
                return "";
            }
        }

        private static void SaveClientToken(string friendCode, string token)
        {
            friendCode = Norm(friendCode);
            token = Norm(token);
            if (string.IsNullOrWhiteSpace(friendCode) || string.IsNullOrWhiteSpace(token))
                return;

            try
            {
                PlayerPrefs.SetString(ClientTokenPrefsKey(friendCode), token);
                PlayerPrefs.Save();
                _clientToken = token;
                _clientTokenFriendCode = friendCode;
                BanModApiTokenManager.Token = token;
            }
            catch { }
        }

        private static void ClearClientTokenForFriendCode(string friendCode)
        {
            friendCode = Norm(friendCode);
            if (string.IsNullOrWhiteSpace(friendCode))
                return;

            try
            {
                PlayerPrefs.DeleteKey(ClientTokenPrefsKey(friendCode));
                PlayerPrefs.Save();
            }
            catch { }

            if (string.Equals(
                    Norm(_clientTokenFriendCode),
                    friendCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                _clientToken = "";
                _clientTokenFriendCode = "";
                BanModApiTokenManager.Token = "";
            }
        }

        public static void ApplyClientAuthHeaders(UnityWebRequest request)
        {
            if (request == null)
                return;

            string friendCode = GetCurrentFriendCode();
            string token = GetCurrentClientToken();
            try { request.SetRequestHeader("X-BANMOD-FriendCode", MakeHeaderSafe(friendCode, "Unknown")); } catch { }
            if (!string.IsNullOrWhiteSpace(token))
            {
                try { request.SetRequestHeader("X-BANMOD-Client-Token", MakeHeaderSafe(token, "")); } catch { }
            }
        }

        public static IEnumerator EnsureActivationTokenForApi(Action<bool, string> callback)
        {
            TryCaptureEosFriendCode("EOSManager.FriendCode/api");
            string friendCode = Norm(_friendCode);
            if (string.IsNullOrWhiteSpace(friendCode) || BanMod.IsBanModDisabled)
            {
                callback?.Invoke(false, "");
                yield break;
            }

            string currentToken = GetCurrentClientToken();

            if (string.IsNullOrWhiteSpace(currentToken))
            {
                bool activationOk = false;
                yield return ActivationFlow(ok => activationOk = ok);
                if (!activationOk)
                {
                    callback?.Invoke(false, "");
                    yield break;
                }
                currentToken = GetCurrentClientToken();
            }

            bool ready = !string.IsNullOrWhiteSpace(currentToken);
            BanModApiTokenManager.Token = ready ? currentToken : "";
            callback?.Invoke(ready, ready ? currentToken : "");
        }

        public static bool TryApplyServerForceDisable(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return false;

            try
            {
                using JsonDocument document = JsonDocument.Parse(responseText);
                JsonElement root = document.RootElement;

                bool force = ReadJsonBool(root, "force_disable_mod") ||
                             ReadJsonBool(root, "force_disable");
                bool explicitlyForCurrentPlayer =
                    ReadJsonBool(root, "force_disable_for_current_player");

                if (!force || !explicitlyForCurrentPlayer)
                    return false;

                string targetFriendCode = ReadJsonString(root, "target_friend_code");
                string currentFriendCode = GetCurrentFriendCode();

                if (!string.IsNullOrWhiteSpace(targetFriendCode) &&
                    !string.Equals(
                        targetFriendCode.Trim(),
                        (currentFriendCode ?? "").Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                string reason = ReadJsonString(root, "force_disable_reason");
                if (string.IsNullOrWhiteSpace(reason))
                    reason = ReadJsonString(root, "reason");
                if (string.IsNullOrWhiteSpace(reason))
                    reason = "BanMod disabilitata temporaneamente dal server.";

                if (ReadJsonString(root, "force_disable_scope") == "extra_mod_blacklist")
                    ModCompatibilityNotices.DisableAfterNotice(BanMod.DisableMod);
                else
                    BanMod.DisableMod();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadJsonBool(JsonElement root, string name)
        {
            try
            {
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (property.Value.ValueKind == JsonValueKind.True) return true;
                    if (property.Value.ValueKind == JsonValueKind.False) return false;
                    if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out int n)) return n != 0;
                    if (property.Value.ValueKind == JsonValueKind.String && bool.TryParse(property.Value.GetString(), out bool b)) return b;
                }
            }
            catch { }
            return false;
        }

        private static string ReadJsonString(JsonElement root, string name)
        {
            try
            {
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                        return property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() ?? "" : property.Value.ToString();
                }
            }
            catch { }
            return "";
        }

        public static void Init(ManualLogSource log)
        {
            _ = log;
        }

        public static void RequestStartup()
        {
            _startupRequested = true;
        }

        internal static void TryStart()
        {

            if (!_startupRequested || _startupStarted)
                return;

            if (AmongUsClient.Instance == null)
                return;

            _startupStarted = true;
            Debug.Log("[BANMOD][LOGIN] Starting FriendCode startup flow.");
            AmongUsClient.Instance.StartCoroutine(StartupFlow().WrapToIl2Cpp());
        }

        private static IEnumerator EnsureLoginBinLoaded(Action<bool> callback)
        {
            if (BanModLoginRuntime.IsLoaded)
            {
                callback?.Invoke(true);
                yield break;
            }

            LoginManifest manifest = null;
            byte[] bytes = null;
            string expectedSha = "";
            string version = "";

            UnityWebRequest request = UnityWebRequest.Get(LoginManifestUrl);
            request.timeout = RequestTimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyClientAuthHeaders(request);

            yield return request.SendWebRequest();

            string text = request.downloadHandler != null ? request.downloadHandler.text : "";
            long manifestResponseCode = request.responseCode;
            UnityWebRequest.Result manifestResult = request.result;
            bool manifestOk = manifestResult == UnityWebRequest.Result.Success &&
                              manifestResponseCode >= 200 && manifestResponseCode < 300;
            try { request.Dispose(); } catch { }

            if (manifestOk)
            {
                try { manifest = JsonSerializer.Deserialize<LoginManifest>(text, JsonOptions); }
                catch { }

                if (manifest != null && manifest.success && manifest.can_download &&
                    !string.IsNullOrWhiteSpace(manifest.download_url))
                {
                    expectedSha = Norm(manifest.sha256).ToLowerInvariant();
                    version = Norm(manifest.version);
                    yield return DownloadBytes(manifest.download_url, value => bytes = value);

                    if (bytes != null && bytes.Length > 0)
                    {
                        string actualSha = Sha256Hex(bytes);
                        if (!string.IsNullOrWhiteSpace(expectedSha) &&
                            !string.Equals(actualSha, expectedSha, StringComparison.OrdinalIgnoreCase))
                        {
                            bytes = null;
                        }
                        else if (manifest.size_bytes > 0 && bytes.Length != manifest.size_bytes)
                        {
                            bytes = null;
                        }
                        else
                        {
                            expectedSha = actualSha;
                        }
                    }
                }
            }
            else
            {
            }

            if (bytes == null || bytes.Length == 0)
            {
                callback?.Invoke(false);
                yield break;
            }

            bool loaded = BanModLoginRuntime.LoadAndStart(bytes, expectedSha, version, out _);
            try { CryptographicOperations.ZeroMemory(bytes); } catch { }


            callback?.Invoke(loaded);
        }

        private static IEnumerator StartupFlow()
        {
            GameIdentity identity = null;

            for (int i = 1; i <= IdentityMaxAttempts; i++)
            {
                identity = ReadIdentity();

                if (identity != null && !string.IsNullOrWhiteSpace(identity.FriendCode))
                    break;

                yield return new WaitForSeconds(IdentityRetrySeconds);
            }

            if (identity == null || string.IsNullOrWhiteSpace(identity.FriendCode))
            {
                yield break;
            }

            string newFriendCode = identity.FriendCode.Trim();
            string newPlayerName = string.IsNullOrWhiteSpace(identity.PlayerName) ? "Unknown" : identity.PlayerName.Trim();

            if (!string.Equals(_friendCode, newFriendCode, StringComparison.OrdinalIgnoreCase))
            {
                _friendCode = newFriendCode;
                _playerName = newPlayerName;
                _clientToken = LoadClientToken(_friendCode);
                BanModApiTokenManager.Token = _clientToken;

                StopAllPremiumModules();

            }
            else
            {
                _playerName = newPlayerName;
            }

            if (string.IsNullOrWhiteSpace(_clientToken))
            {
                _clientToken = LoadClientToken(_friendCode);
                BanModApiTokenManager.Token = _clientToken;
            }

            ModUpdater.StartupUpdateResult updateResult = null;
            yield return ModUpdater.CheckAtStartupCoroutine(result => updateResult = result);

            if (updateResult != null && updateResult.BlockStartup)
                yield break;

            bool activationReady = false;
            yield return ActivationFlow(ok => activationReady = ok);
            if (!activationReady || string.IsNullOrWhiteSpace(GetCurrentClientToken()))
            {
                Debug.LogWarning("[BANMOD][AUTH] Client token unavailable; private services were not started.");
                StartLobbyLoops();
                yield break;
            }

            bool loginLoaded = false;
            yield return EnsureLoginBinLoaded(ok => loginLoaded = ok);

            if (!loginLoaded)
            {
                Debug.LogWarning("[BANMOD][LOGIN] login.bin non disponibile; nessun forcedisable applicato.");
                StartLobbyLoops();
                yield break;
            }

            yield return BanModDeniedLists.RefreshAllCoroutine();

            StartLobbyLoops();

            while (!BanModLoginRuntime.IsReady && !BanMod.IsBanModDisabled)
                yield return null;

            if (BanMod.IsBanModDisabled)
                yield break;

            try
            {
                BanModLoginUi.Instance?.ShowLoading("Loading compatible services...");
            }
            catch { }

            yield return RequestPremiumAndLoadBins();

            try { BanModLoginUi.Instance?.Close(); } catch { }

            if (BanMod.IsBanModDisabled)
                yield break;

            StartPremiumRefreshLoop();
        }

        private static void StartLobbyLoops()
        {
            if (AmongUsClient.Instance == null)
                return;

            if (_loopStarted)
                return;

            _loopStarted = true;

            _nextLobbyStatusRealtime = 0f;
            _lastLobbyStatusSnapshot = "";
            _lastLobbySceneHandle = SafeGetActiveSceneHandle();

            AmongUsClient.Instance.StartCoroutine(LobbyListLoop().WrapToIl2Cpp());
        }

        internal static void TickLobbyStatus()
        {
            try
            {
                if (!_loopStarted || BanMod.IsBanModDisabled)
                    return;

                AmongUsClient client = AmongUsClient.Instance;
                if (client == null)
                    return;

                int sceneHandle = SafeGetActiveSceneHandle();

                if (_lastLobbySceneHandle == int.MinValue)
                {
                    _lastLobbySceneHandle = sceneHandle;
                }
                else if (sceneHandle != int.MinValue &&
                         sceneHandle != _lastLobbySceneHandle)
                {
                    _lastLobbySceneHandle = sceneHandle;

                    _lastLobbyStatusSnapshot = "";
                    _nextLobbyStatusRealtime = 0f;

                    _statusRunning = false;
                    _statusStartedRealtime = 0f;

                    Debug.Log(
                        "[BANMOD][LOBBY] Cambio scena rilevato: heartbeat forzato."
                    );
                }

                if (!IsInLobbyOrGame())
                {
                    _lastLobbyStatusSnapshot = "";
                    _nextLobbyStatusRealtime = 0f;
                    _statusRunning = false;
                    _statusStartedRealtime = 0f;
                    return;
                }

                string lobbyCode = SafeGetOldGameCode();
                if (string.IsNullOrWhiteSpace(lobbyCode))
                    return;

                bool isPublic = SafeGetOldIsPublic();
                bool isPrivate = !isPublic;
                bool shareLobby = SafeGetOldShareLobby();
                bool isHost = SafeGetOldIsHost();

                BanModSyncState.CaptureMainThread();
                var syncState = BanModSyncState.Get();
                string snapshot =
                    syncState.Region + "|" + syncState.PlayerId + "|" + syncState.ClientId + "|" +
                    lobbyCode.Trim().ToUpperInvariant() + "|" +
                    (isPublic ? "1" : "0") + "|" +
                    (isPrivate ? "1" : "0") + "|" +
                    (shareLobby ? "1" : "0") + "|" +
                    (isHost ? "1" : "0");

                if (!string.Equals(
                        snapshot,
                        _lastLobbyStatusSnapshot,
                        StringComparison.Ordinal))
                {
                    _lastLobbyStatusSnapshot = snapshot;
                    _nextLobbyStatusRealtime = 0f;
                }

                float now = Time.realtimeSinceStartup;

                if (_statusRunning)
                {
                    if (_statusStartedRealtime > 0f &&
                        now - _statusStartedRealtime > RequestTimeoutSeconds + 20f)
                    {
                        Debug.LogWarning(
                            "[BANMOD][LOBBY] Watchdog Update: heartbeat precedente sbloccato.");
                        _statusRunning = false;
                        _statusStartedRealtime = 0f;
                        _nextLobbyStatusRealtime = 0f;
                    }
                    else
                    {
                        return;
                    }
                }

                if (now < _nextLobbyStatusRealtime)
                    return;

                _nextLobbyStatusRealtime = now + LobbyStatusIntervalSeconds;

                try
                {
                    client.StartCoroutine(SendLobbyStatus().WrapToIl2Cpp());
                }
                catch (Exception ex)
                {
                    _statusRunning = false;
                    _statusStartedRealtime = 0f;
                    _nextLobbyStatusRealtime = now + 1f;

                    Debug.LogError(
                        "[BANMOD][LOBBY] Impossibile avviare heartbeat da Update: " +
                        ex.GetType().Name + ": " + ex.Message
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[BANMOD][LOBBY] Tick heartbeat fallito: " +
                    ex.GetType().Name + ": " + ex.Message
                );
            }
        }

        private static void StartPremiumRefreshLoop()
        {
            if (_premiumRefreshLoopStarted || AmongUsClient.Instance == null)
                return;

            _premiumRefreshLoopStarted = true;
            _premiumRefreshRequested = false;
            AmongUsClient.Instance.StartCoroutine(PremiumRefreshLoop().WrapToIl2Cpp());
        }

        private static IEnumerator PremiumRefreshLoop()
        {
            while (!BanMod.IsBanModDisabled)
            {
                float elapsed = 0f;
                while (elapsed < PremiumRefreshIntervalSeconds && !_premiumRefreshRequested)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                _premiumRefreshRequested = false;

                if (_premiumRefreshRunning || !BanModLoginRuntime.IsReady ||
                    string.IsNullOrWhiteSpace(_activationToken))
                    continue;

                _premiumRefreshRunning = true;
                try
                {
                    yield return RequestPremiumAndLoadBins();
                }
                finally
                {
                    _premiumRefreshRunning = false;
                }
            }
        }


        private static IEnumerator LobbyListLoop()
        {
            while (true)
            {
                if (!_lobbyListRunning)
                    yield return FetchModdedLobbies();

                yield return new WaitForSeconds(LobbyListIntervalSeconds);
            }
        }

        private static IEnumerator ActivationFlow(Action<bool> callback)
        {
            string challengeUrl = ActivationChallengeUrl + "?friend_code=" + UnityWebRequest.EscapeURL(_friendCode);

            UnityWebRequest c = UnityWebRequest.Get(challengeUrl);
            c.timeout = RequestTimeoutSeconds;
            c.downloadHandler = new DownloadHandlerBuffer();

            yield return c.SendWebRequest();

            string challengeText = c.downloadHandler != null ? c.downloadHandler.text : "";
            UnityWebRequest.Result challengeResult = c.result;
            long challengeCode = c.responseCode;
            try { c.Dispose(); } catch { }

            if (challengeResult != UnityWebRequest.Result.Success || challengeCode < 200 || challengeCode >= 300)
            {
                callback(false);
                yield break;
            }

            ChallengeResponse challenge = null;
            try { challenge = JsonSerializer.Deserialize<ChallengeResponse>(challengeText, JsonOptions); } catch { }

            if (challenge == null || !challenge.success || string.IsNullOrWhiteSpace(challenge.nonce))
            {
                callback(false);
                yield break;
            }

            string buildId = BanModBuildSecret.BuildId ?? "";
            string banModSha256 = SafeGetOwnBanModSha256();
            string clientVersion = GetCurrentClientVersion();

            string existingClientToken = GetCurrentClientToken();

            string body = "{"
                + "\"FriendCode\":" + JsonString(_friendCode) + ","
                + "\"PlayerName\":" + JsonString(_playerName) + ","
                + "\"Nonce\":" + JsonString(challenge.nonce) + ","
                + "\"ClientVersion\":" + JsonString(clientVersion) + ","
                + "\"BuildId\":" + JsonString(buildId) + ","
                + "\"BanModSha256\":" + JsonString(banModSha256) + ","
                + "\"ClientToken\":" + JsonString(existingClientToken)
                + "}";

            UnityWebRequest v = new UnityWebRequest(ActivationVerifyUrl, "POST");
            v.timeout = RequestTimeoutSeconds;
            v.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            v.downloadHandler = new DownloadHandlerBuffer();
            v.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrWhiteSpace(existingClientToken))
            {
                try { v.SetRequestHeader("X-BANMOD-Client-Token", MakeHeaderSafe(existingClientToken, "")); } catch { }
            }

            yield return v.SendWebRequest();

            string verifyText = v.downloadHandler != null ? v.downloadHandler.text : "";
            UnityWebRequest.Result verifyResult = v.result;
            long verifyCode = v.responseCode;
            try { v.Dispose(); } catch { }

            if (verifyResult != UnityWebRequest.Result.Success || verifyCode < 200 || verifyCode >= 300)
            {
                callback(false);
                yield break;
            }

            ActivationResponse response = null;
            try { response = JsonSerializer.Deserialize<ActivationResponse>(verifyText, JsonOptions); } catch { }

            if (response == null || !response.success || string.IsNullOrWhiteSpace(response.activation_token))
            {
                callback(false);
                yield break;
            }

            if (TryApplyServerForceDisable(verifyText))
            {
                callback(false);
                yield break;
            }

            _activationToken = response.activation_token;
            _activationTokenRecoveryOnly = response.recovery_only;

            if (!string.IsNullOrWhiteSpace(response.client_token))
                SaveClientToken(_friendCode, response.client_token);
            else if (string.IsNullOrWhiteSpace(_clientToken))
                _clientToken = LoadClientToken(_friendCode);

            if (string.Equals(response.device_status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning(
                    "[BANMOD][AUTH] Device registered and pending admin approval. " +
                    "Base BanMod/login remain available; premium modules stay locked."
                );
            }
            else if (string.Equals(response.device_status, "revoked", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[BANMOD][AUTH] This BanMod device key was revoked by the server.");
            }

            int lifetime = response.expires_in_seconds > 0
                ? response.expires_in_seconds
                : 300;
            _activationTokenExpiresAtUnix =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(30, lifetime);
            callback(!_activationTokenRecoveryOnly && !string.IsNullOrWhiteSpace(_clientToken));
        }

        private static string GetCurrentClientVersion()
        {
            try
            {
                string value = Convert.ToString(BanMod.PluginVersion) ?? "";
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
            catch { }

            try
            {
                Version version = typeof(BanModCore).Assembly.GetName().Version;
                if (version != null)
                    return version.ToString();
            }
            catch { }

            return "3.6.9";
        }

        private static void LogPremiumAccessFailure(string reason)
        {
            if (_premiumAccessFailureLogged)
                return;

            _premiumAccessFailureLogged = true;
            Debug.LogWarning("[BANMOD][PREMIUM] /api/access non riuscito. " + reason);
        }

        private static IEnumerator RequestPremiumAndLoadBins()
        {
            bool identityReady = false;
            string clientToken = "";

            yield return EnsureActivationTokenForApi((success, value) =>
            {
                identityReady = success;
                clientToken = value ?? "";
            });

            if (!identityReady || string.IsNullOrWhiteSpace(clientToken))
                yield break;

            string friendCode = GetCurrentFriendCode();
            if (string.IsNullOrWhiteSpace(friendCode))
                yield break;

            _friendCode = friendCode;

            string body = "{"
                + "\"FriendCode\":" + JsonString(_friendCode) + ","
                + "\"PlayerName\":" + JsonString(_playerName) + ","
                + "\"BanModSha256\":" + JsonString(SafeGetOwnBanModSha256()) + ","
                + "\"BuildId\":" + JsonString(BanModBuildSecret.BuildId ?? "") + ","
                + "\"LoginBinSha256\":" + JsonString(BanModLoginRuntime.LoginBinSha256) + ","
                + "\"LoginBinVersion\":" + JsonString(BanModLoginRuntime.LoginBinVersion)
                + "}";

            string text = "";
            long responseCode = 0;
            UnityWebRequest.Result result = UnityWebRequest.Result.InProgress;

            for (int accessAttempt = 0; accessAttempt < 2; accessAttempt++)
            {
                UnityWebRequest req = new UnityWebRequest(AccessUrl, "POST");
                req.timeout = RequestTimeoutSeconds;
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                ApplyClientAuthHeaders(req);

                yield return req.SendWebRequest();

                text = req.downloadHandler != null ? req.downloadHandler.text : "";
                responseCode = req.responseCode;
                result = req.result;
                try { req.Dispose(); } catch { }
                bool temporaryFailure =
                    result == UnityWebRequest.Result.ConnectionError ||
                    responseCode == 502 ||
                    responseCode == 503 ||
                    responseCode == 504;

                if (temporaryFailure && accessAttempt == 0)
                {
                    yield return new WaitForSecondsRealtime(2f);
                    continue;
                }
                bool clientTokenInvalid = responseCode == 401 &&
                    text != null &&
                    text.IndexOf("client_token_invalid", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!clientTokenInvalid || accessAttempt > 0)
                    break;

                try
                {
                    Debug.LogWarning(
                        "[BANMOD][AUTH] Client token rejected. " +
                        "Requesting a new credential for this FriendCode/device."
                    );
                }
                catch { }

                ClearClientTokenForFriendCode(friendCode);

                bool recovered = false;
                yield return ActivationFlow(ok => recovered = ok);
                if (!recovered || string.IsNullOrWhiteSpace(GetCurrentClientToken()))
                    break;
            }

            if (result != UnityWebRequest.Result.Success || responseCode < 200 || responseCode >= 300)
            {
                LogPremiumAccessFailure("HTTP=" + responseCode + " result=" + result);
                yield break;
            }

            AccessResponse access;
            try
            {
                access = JsonSerializer.Deserialize<AccessResponse>(text, JsonOptions);
            }
            catch (JsonException)
            {
                access = null;
            }

            if (access == null || !access.success)
            {
                LogPremiumAccessFailure("risposta non valida o non riuscita.");
                yield break;
            }

            _premiumAccessFailureLogged = false;

            if (TryApplyServerForceDisable(text))
                yield break;

            if (!access.premium_gate_ok)
            {
                try
                {
                    Debug.LogWarning(
                        "[BANMOD][PREMIUM] Premium gate closed. reason=" +
                        (access.reason ?? ""));
                }
                catch { }
                StopAllPremiumModules();
                yield break;
            }

            HashSet<string> desired = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (access.enabled_services != null)
            {
                foreach (string key in access.enabled_services)
                {
                    if (!string.IsNullOrWhiteSpace(key))
                        desired.Add(key.Trim());
                }
            }

            if (desired.Count == 0 && access.services != null)
            {
                foreach (PremiumService service in access.services)
                {
                    if (service == null)
                        continue;

                    string serviceKey = service.GetKey();

                    if (string.IsNullOrWhiteSpace(serviceKey) ||
                        !service.allowed ||
                        !service.enabled ||
                        !service.selected)
                    {
                        try
                        {
                            Debug.Log(
                                "[BANMOD][PREMIUM] Skip " + (serviceKey ?? "<unknown>") +
                                " allowed=" + service.allowed +
                                " enabled=" + service.enabled +
                                " selected=" + service.selected +
                                " autoload=" + service.GetAutoload());
                        }
                        catch { }
                        continue;
                    }

                    desired.Add(serviceKey);
                }
            }

            string[] loadedKeys = GetLoadedPluginKeys();
            for (int i = 0; i < loadedKeys.Length; i++)
            {
                if (!desired.Contains(loadedKeys[i]))
                    UnloadPremiumModule(loadedKeys[i]);
            }

            try
            {
                Debug.Log(
                    "[BANMOD][PREMIUM] Access OK. server_count=" + access.premium_count +
                    " desired=" + desired.Count +
                    " developer=" + access.developer_override);
            }
            catch { }

            foreach (string serviceKey in desired)
            {
                if (LoadedPremiumModules.ContainsKey(serviceKey))
                {
                    try { Debug.Log("[BANMOD][PREMIUM] Already loaded: " + serviceKey); } catch { }
                    continue;
                }

                bool loaded = false;
                yield return LoadPremiumBin(serviceKey, ok => loaded = ok);

                try
                {
                    if (loaded)
                    {
                        Debug.Log("[BANMOD][PREMIUM] Loaded: " + serviceKey);
                        Utils.MainMenuInfo.Show(
                            $"\n<color=#ff5555>[PREMIUM]</color> Loaded: {serviceKey}"
                        );
                    }
                    else
                    {
                        Debug.LogError("[BANMOD][PREMIUM] Failed to load: " + serviceKey);
                        Utils.MainMenuInfo.Show(
                            $"\n<color=#ff5555>[PREMIUM]</color> Failed to load: {serviceKey}"
                        );
                    }
                }
                catch { }
            }

        }

        private static void UnloadPremiumModule(string serviceKey)
        {
            if (string.IsNullOrWhiteSpace(serviceKey) ||
                !LoadedPremiumModules.TryGetValue(serviceKey, out LoadedPremiumModule module))
                return;

            if (module.PluginInstances != null)
            {
                for (int i = module.PluginInstances.Count - 1; i >= 0; i--)
                {
                    object pluginInstance = module.PluginInstances[i];
                    if (pluginInstance == null)
                        continue;

                    try
                    {
                        MethodInfo unloadMethod = pluginInstance.GetType().GetMethod(
                            "Unload", BindingFlags.Public | BindingFlags.Instance);
                        unloadMethod?.Invoke(pluginInstance, null);
                    }
                    catch { }
                }
                module.PluginInstances.Clear();
            }

            LoadedPremiumModules.Remove(serviceKey);
        }

        private static IEnumerator LoadPremiumBin(string serviceKey, Action<bool> callback)
        {
            PremiumManifest manifest = null;

            string url = ApiBaseUrl + "/api/premium/" + UnityWebRequest.EscapeURL(serviceKey) + "/manifest";
            string body = "{"
                + "\"FriendCode\":" + JsonString(_friendCode)
                + "}";

            UnityWebRequest req = new UnityWebRequest(url, "POST");
            req.timeout = RequestTimeoutSeconds;
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            ApplyClientAuthHeaders(req);

            yield return req.SendWebRequest();

            string text = req.downloadHandler != null ? req.downloadHandler.text : "";
            long manifestCode = req.responseCode;
            UnityWebRequest.Result manifestResult = req.result;
            string manifestError = req.error ?? "";
            try { req.Dispose(); } catch { }

            if (manifestResult != UnityWebRequest.Result.Success || manifestCode < 200 || manifestCode >= 300)
            {
                try
                {
                    Debug.LogError(
                        "[BANMOD][PREMIUM] Manifest failed service=" + serviceKey +
                        " HTTP=" + manifestCode +
                        " error=" + manifestError +
                        " response=" + SafeLogText(text));
                }
                catch { }
                callback(false);
                yield break;
            }

            try { manifest = JsonSerializer.Deserialize<PremiumManifest>(text, JsonOptions); }
            catch (Exception ex)
            {
                try { Debug.LogError("[BANMOD][PREMIUM] Manifest JSON error service=" + serviceKey + ": " + ex.Message); } catch { }
            }

            if (manifest == null || !manifest.success || !manifest.can_download || string.IsNullOrWhiteSpace(manifest.download_url))
            {
                try { Debug.LogError("[BANMOD][PREMIUM] Manifest rejected service=" + serviceKey + " response=" + SafeLogText(text)); } catch { }
                callback(false);
                yield break;
            }

            byte[] binBytes = null;
            yield return DownloadBytes(manifest.download_url, value => binBytes = value);

            if (binBytes == null || binBytes.Length == 0)
            {
                try { Debug.LogError("[BANMOD][PREMIUM] Download returned no bytes service=" + serviceKey); } catch { }
                callback(false);
                yield break;
            }

            try
            {
                string actualSha = Sha256Hex(binBytes);

                if (!string.IsNullOrWhiteSpace(manifest.sha256) &&
                    !string.Equals(actualSha, manifest.sha256, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Debug.LogError(
                            "[BANMOD][PREMIUM] SHA256 mismatch service=" + serviceKey +
                            " expected=" + manifest.sha256 +
                            " actual=" + actualSha);
                    }
                    catch { }
                    callback(false);
                    yield break;
                }

                LoadedPremiumModule module = LoadManagedBinFromMemory(serviceKey, binBytes);
                LoadedPremiumModules[serviceKey] = module;
                callback(true);
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogError(
                        "[BANMOD][PREMIUM] Assembly load failed service=" + serviceKey +
                        " type=" + ex.GetType().Name +
                        " message=" + ex.Message);
                }
                catch { }
                callback(false);
            }
            finally
            {
                try { CryptographicOperations.ZeroMemory(binBytes); } catch { }
            }
        }

        private static IEnumerator SendLobbyStatus()
        {
            _statusRunning = true;
            _statusStartedRealtime = Time.realtimeSinceStartup;

            bool tokenReady = false;
            string validToken = "";

            yield return EnsureActivationTokenForApi((success, value) =>
            {
                tokenReady = success;
                validToken = value ?? "";
            });

            if (!tokenReady || string.IsNullOrWhiteSpace(validToken))
            {
                Debug.LogWarning("[BANMOD][LOBBY] Token di attivazione non disponibile; heartbeat non inviato.");
                _statusRunning = false;
                yield break;
            }

            BanModApiTokenManager.Token = validToken;

            string friendCode = _friendCode;
            string playerName = _playerName;
            string lobbyCode = "";
            string hostName = "";
            string gameMode = "";
            string status = "";
            string region = "";
            string language = "";
            string platform = "";
            bool isOnline = true;
            bool shareLobby = true;
            bool isHost = false;
            bool isPublic = false;
            bool isPrivate = false;
            int players = 0;
            int maxPlayers = 15;
            int kc = 0;
            int impostorCount = 0;

            bool buildFailed = false;

            try
            {
                PlayerControl player = PlayerControl.LocalPlayer;

                if (player != null)
                {
                    string oldName = SafeGetOldPlayerName(player);
                    if (!string.IsNullOrWhiteSpace(oldName))
                        playerName = oldName.Trim();
                }

                string oldFriendCode = SafeGetOldFriendCode(player);
                if (!string.IsNullOrWhiteSpace(oldFriendCode))
                    friendCode = oldFriendCode.Trim();

                lobbyCode = SafeGetOldGameCode();
                isHost = SafeGetOldIsHost();
                isPublic = !string.IsNullOrWhiteSpace(lobbyCode) && SafeGetOldIsPublic();
                isPrivate = !string.IsNullOrWhiteSpace(lobbyCode) && !isPublic;
                shareLobby = SafeGetOldShareLobby();

                players = SafeGetOldCurrentLobbyPlayerCount();
                maxPlayers = SafeGetOldMaxPlayers();
                if (maxPlayers <= 0)
                    maxPlayers = 15;

                gameMode = SafeGetOldCurrentLobbyMode();
                status = SafeGetOldCurrentStatus();
                region = SafeGetOldRegion();
                language = SafeGetOldLanguage();
                platform = SafeGetOldPlatform();
                hostName = SafeGetOldHostName();
                kc = SafeGetOldKillCooldown();
                impostorCount = SafeGetOldImpostorCount();

                if (string.IsNullOrWhiteSpace(friendCode))
                    friendCode = _friendCode;

                if (string.IsNullOrWhiteSpace(playerName))
                    playerName = _playerName;

                if (string.IsNullOrWhiteSpace(hostName))
                    hostName = playerName;

                if (!string.IsNullOrWhiteSpace(friendCode))
                    _friendCode = friendCode;

                if (!string.IsNullOrWhiteSpace(playerName))
                    _playerName = playerName;
            }
            catch
            {
                buildFailed = true;
            }

            if (buildFailed)
            {
                _statusRunning = false;
                yield break;
            }

            string playersText = players > 0 ? players + "/" + maxPlayers : "0/" + maxPlayers;
            BanModSyncState.CaptureMainThread();
            var syncState = BanModSyncState.Get();

            string body = "{"
                + "\"FriendCode\":" + JsonString(friendCode) + ","
                + "\"SyncSessionId\":" + JsonString(SyncSessionId) + ","
                + "\"SyncRegion\":" + JsonString(syncState.Region) + ","
                + "\"SyncPlayerId\":" + syncState.PlayerId + ","
                + "\"SyncClientId\":" + syncState.ClientId + ","
                + "\"PlayerName\":" + JsonString(playerName) + ","
                + "\"LobbyCode\":" + JsonString(lobbyCode) + ","
                + "\"HostName\":" + JsonString(hostName) + ","
                + "\"GameMode\":" + JsonString(gameMode) + ","
                + "\"Mode\":" + JsonString(gameMode) + ","
                + "\"Players\":" + players + ","
                + "\"MaxPlayers\":" + maxPlayers + ","
                + "\"PlayersText\":" + JsonString(playersText) + ","
                + "\"KC\":" + kc + ","
                + "\"KillCooldown\":" + kc + ","
                + "\"ImpostorCount\":" + impostorCount + ","
                + "\"Region\":" + JsonString(region) + ","
                + "\"Language\":" + JsonString(language) + ","
                + "\"Platform\":" + JsonString(platform) + ","
                + "\"Status\":" + JsonString(status) + ","
                + "\"IsOnline\":" + BoolJson(isOnline) + ","
                + "\"ShareLobby\":" + BoolJson(shareLobby) + ","
                + "\"share_lobby\":" + BoolJson(shareLobby) + ","
                + "\"IsHost\":" + BoolJson(isHost) + ","
                + "\"IsPublic\":" + BoolJson(isPublic) + ","
                + "\"IsPrivate\":" + BoolJson(isPrivate) + ","

                + "\"ModName\":" + JsonString("BanMod") + ","
                + "\"GameCode\":" + JsonString(lobbyCode) + ","
                + "\"PlayersInLobby\":" + players + ","
                + "\"KillCooldownOld\":" + kc + ","
                + "\"impostor_count\":" + impostorCount
                + "}";


            UnityWebRequest req = null;
            UnityWebRequestAsyncOperation operation = null;

            try
            {
                req = new UnityWebRequest(LobbyStatusUrl, "POST");
                req.timeout = RequestTimeoutSeconds;
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                BanModApiTokenManager.ApplyAuthHeader(req);

                operation = req.SendWebRequest();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[BANMOD][LOBBY] Errore preparazione richiesta: " +
                    ex.GetType().Name + ": " + ex.Message
                );

                try { req?.Dispose(); } catch { }
                _statusRunning = false;
                yield break;
            }

            float requestDeadline = Time.realtimeSinceStartup + RequestTimeoutSeconds + 5f;
            while (operation != null && !operation.isDone && Time.realtimeSinceStartup < requestDeadline)
                yield return null;

            if (operation != null && !operation.isDone)
            {
                try { req.Abort(); } catch { }
                Debug.LogWarning("[BANMOD][LOBBY] Heartbeat abortito dal watchdog.");
            }

            try
            {
                string responseText = "";

                try
                {
                    responseText = req.downloadHandler != null
                        ? req.downloadHandler.text
                        : "";
                }
                catch
                {
                    responseText = "";
                }

                if (TryApplyServerForceDisable(responseText))
                {
                }
                else if (req.responseCode == 401)
                {
                    Debug.LogWarning(
                        "[BANMOD][LOBBY] Token non valido o scaduto."
                    );

                    BanModApiTokenManager.ClearToken(req);
                }
                else if (
                    req.result != UnityWebRequest.Result.Success ||
                    req.responseCode < 200 ||
                    req.responseCode >= 300
                )
                {
                    Debug.LogError(
                        "[BANMOD][LOBBY] Invio fallito: HTTP=" +
                        req.responseCode +
                        " error=" + (req.error ?? "") +
                        " response=" + responseText
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[BANMOD][LOBBY] Errore lettura risposta: " +
                    ex.GetType().Name + ": " + ex.Message
                );
            }
            finally
            {
                try { req?.Dispose(); } catch { }
                _statusRunning = false;
                _statusStartedRealtime = 0f;
            }
        }

        private static string SafeGetOldFriendCode(PlayerControl player)
        {
            try
            {
                string value = BanModIdentity.GetFriendCode(player);
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
            catch { }

            try
            {
                string value = BanModIdentity.GetFriendCode();
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(_friendCode))
                    return _friendCode.Trim();
            }
            catch { }

            return "";
        }

        private static string SafeGetOldPlayerName(PlayerControl player)
        {
            try
            {
                string realName = ExtendedPlayerControl.GetRealName(player);
                if (!string.IsNullOrWhiteSpace(realName))
                    return realName.Trim();
            }
            catch { }

            try
            {
                string identityName = BanModIdentity.GetPlayerName(player);
                if (!string.IsNullOrWhiteSpace(identityName))
                    return identityName.Trim();
            }
            catch { }

            try
            {
                if (player != null && player.Data != null && !string.IsNullOrWhiteSpace(player.Data.PlayerName))
                    return player.Data.PlayerName.Trim();
            }
            catch { }

            return "Unknown";
        }

        private static string SafeGetOldGameCode()
        {
            try
            {
                if (AmongUsClient.Instance != null)
                    return GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            }
            catch { }

            return "";
        }

        private static bool SafeGetOldIsPublic()
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return false;

                if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
                    return false;

                return AmongUsClient.Instance.IsGamePublic;
            }
            catch
            {
                return false;
            }
        }

        private static bool SafeGetOldIsHost()
        {
            try { return AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost; }
            catch { return false; }
        }

        private static bool SafeGetOldShareLobby()
        {
            try
            {
                return Options.ShareLobbyCode != null && Options.ShareLobbyCode.GetBool();
            }
            catch
            {
                return false;
            }
        }

        private static int SafeGetOldCurrentLobbyPlayerCount()
        {
            try { return Utils.GetCurrentLobbyPlayerCount(); }
            catch { }

            try { return SafeGetPlayerCount(); }
            catch { return 0; }
        }

        private static string SafeGetOldCurrentLobbyMode()
        {
            try { return Utils.GetCurrentLobbyMode(); }
            catch { }

            try { return SafeGetGameMode(); }
            catch { return "Unknown"; }
        }

        private static string SafeGetOldCurrentStatus()
        {
            try { return Utils.GetCurrentStatus(); }
            catch { return "Unknown"; }
        }

        private static string SafeGetOldPlatform()
        {
            try { return BanModIdentity.GetPlatform(); }
            catch { return "Unknown"; }
        }

        private static string SafeGetOldRegion()
        {
            try { return Utils.GetRegionName(); }
            catch { return "Unknown"; }
        }

        private static string SafeGetOldLanguage()
        {
            try { return Utils.LanguageUtils.GetLanguageName(Utils.LanguageUtils.GetCurrentGameOptions()); }
            catch { return "Unknown"; }
        }

        private static string SafeGetOldHostName()
        {
            try
            {
                if (AmongUsClient.Instance != null)
                {
                    int hostId = ObjectToInt(ReadInstanceObject(AmongUsClient.Instance, "HostId"));
                    ClientData hostClient = GetClientDataById(hostId) ?? FindClientDataById(hostId);
                    if (hostClient != null && !string.IsNullOrWhiteSpace(hostClient.PlayerName))
                        return hostClient.PlayerName.Trim();
                }
            }
            catch { }

            try
            {
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer != null)
                    return SafeGetOldPlayerName(PlayerControl.LocalPlayer);
            }
            catch { }

            return "";
        }

        private static int SafeGetOldKillCooldown()
        {
            try
            {
                object logicOptions = GameManager.Instance != null ? ReadInstanceObject(GameManager.Instance, "LogicOptions") : null;
                if (logicOptions != null)
                {
                    MethodInfo m = logicOptions.GetType().GetMethod("GetKillCooldown", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (m != null)
                        return ObjectToInt(m.Invoke(logicOptions, null));
                }
            }
            catch { }

            try { return SafeGetKillCooldown(); }
            catch { return 0; }
        }

        private static int SafeGetOldImpostorCount()
        {
            try
            {
                object logicOptions = GameManager.Instance != null ? ReadInstanceObject(GameManager.Instance, "LogicOptions") : null;
                if (logicOptions != null)
                {
                    object value = ReadInstanceObject(logicOptions, "NumImpostors") ??
                                   ReadInstanceObject(logicOptions, "numImpostors") ??
                                   ReadInstanceObject(logicOptions, "ImpostorCount");
                    return ObjectToInt(value);
                }
            }
            catch { }

            return 0;
        }

        private static int SafeGetOldMaxPlayers()
        {
            try { return SafeGetMaxPlayers(); }
            catch { return 15; }
        }


        private static IEnumerator FetchModdedLobbies()
        {
            _lobbyListRunning = true;

            string[] urls = new string[]
            {
                ActiveLobbiesUrl,
                ApiBaseUrl + "/api/lobbies/public"
            };

            bool loaded = false;

            for (int attempt = 0; attempt < urls.Length && !loaded; attempt++)
            {
                UnityWebRequest req = UnityWebRequest.Get(urls[attempt]);
                req.timeout = RequestTimeoutSeconds;
                req.downloadHandler = new DownloadHandlerBuffer();

                yield return req.SendWebRequest();

                string text = "";
                try { text = req.downloadHandler != null ? req.downloadHandler.text : ""; }
                catch { text = ""; }

                if (req.result == UnityWebRequest.Result.Success &&
                    req.responseCode >= 200 && req.responseCode < 300)
                {
                    try
                    {
                        ActiveLobbiesResponse response = JsonSerializer.Deserialize<ActiveLobbiesResponse>(text, JsonOptions);
                        if (response != null && response.success && response.lobbies != null)
                        {
                            List<ModdedLobbyInfo> visible = new List<ModdedLobbyInfo>();
                            HashSet<string> seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            for (int i = 0; i < response.lobbies.Count; i++)
                            {
                                ModdedLobbyInfo lobby = response.lobbies[i];
                                if (lobby == null || lobby.is_private)
                                    continue;

                                if (!lobby.is_public && string.IsNullOrWhiteSpace(lobby.lobby_code))
                                    continue;

                                string code = lobby.lobby_code ?? "";
                                if (!string.IsNullOrWhiteSpace(code) && !seenCodes.Add(code.Trim()))
                                    continue;

                                visible.Add(lobby);
                            }

                            LastModdedLobbies = visible;
                            loaded = true;
                        }
                    }
                    catch { }
                }

                try { req.Dispose(); } catch { }
            }

            _lobbyListRunning = false;
        }

        private static string SafeLogText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            string clean = value.Replace("\r", " ").Replace("\n", " ").Trim();
            return clean.Length <= 400 ? clean : clean.Substring(0, 400);
        }

        private static IEnumerator DownloadBytes(string url, Action<byte[]> callback)
        {
            byte[] result = null;
            Exception error = null;
            bool done = false;

            try
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);
                            if (!string.IsNullOrWhiteSpace(_friendCode))
                                client.DefaultRequestHeaders.TryAddWithoutValidation("X-BANMOD-FriendCode", _friendCode);
                            string clientToken = GetCurrentClientToken();
                            if (!string.IsNullOrWhiteSpace(clientToken))
                                client.DefaultRequestHeaders.TryAddWithoutValidation("X-BANMOD-Client-Token", clientToken);

                            using (HttpResponseMessage response = await client.GetAsync(url))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    error = new Exception("HTTP " + (int)response.StatusCode + " " + response.ReasonPhrase);
                                    return;
                                }

                                result = await response.Content.ReadAsByteArrayAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        System.Threading.Volatile.Write(ref done, true);
                    }
                });
            }
            catch
            {
                callback(null);
                yield break;
            }

            while (!System.Threading.Volatile.Read(ref done))
                yield return null;

            if (error != null)
            {
                try { Debug.LogError("[BANMOD][PREMIUM] HTTP download failed: " + error.Message); } catch { }
                callback(null);
                yield break;
            }

            if (result == null || result.Length == 0)
            {
                try { Debug.LogError("[BANMOD][PREMIUM] HTTP download completed with zero bytes."); } catch { }
                callback(null);
                yield break;
            }

            callback(result);
        }

        private static LoadedPremiumModule LoadManagedBinFromMemory(string serviceKey, byte[] binBytes)
        {
            Assembly assembly = Assembly.Load(binBytes);
            Type basePluginType = typeof(BepInEx.Unity.IL2CPP.BasePlugin);

            Type[] assemblyTypes;
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                List<Type> validTypes = new List<Type>();
                if (ex.Types != null)
                {
                    for (int i = 0; i < ex.Types.Length; i++)
                    {
                        if (ex.Types[i] != null)
                            validTypes.Add(ex.Types[i]);
                    }
                }
                assemblyTypes = validTypes.ToArray();
            }

            List<object> pluginInstances = new List<object>();

            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                Type pluginType = assemblyTypes[i];

                if (pluginType == null || pluginType.IsAbstract || !basePluginType.IsAssignableFrom(pluginType))
                    continue;

                object pluginInstance = Activator.CreateInstance(pluginType, true);
                if (pluginInstance == null)
                    throw new InvalidOperationException("Impossibile creare il plugin BepInEx: " + pluginType.FullName);

                MethodInfo loadMethod = pluginType.GetMethod(
                    "Load",
                    BindingFlags.Public | BindingFlags.Instance);

                if (loadMethod == null || loadMethod.GetParameters().Length != 0)
                    throw new MissingMethodException("Load() non trovato nel plugin BepInEx: " + pluginType.FullName);

                loadMethod.Invoke(pluginInstance, null);
                pluginInstances.Add(pluginInstance);

            }

            if (pluginInstances.Count == 0)
            {
                throw new MissingMethodException(
                    "Nessuna classe derivata da BepInEx.Unity.IL2CPP.BasePlugin trovata nel .bin.");
            }

            return new LoadedPremiumModule
            {
                ServiceKey = serviceKey,
                Assembly = assembly,
                PluginInstances = pluginInstances
            };
        }

        public static void StopAllPremiumModules()
        {
            string[] keys = new string[LoadedPremiumModules.Keys.Count];
            LoadedPremiumModules.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                if (!LoadedPremiumModules.TryGetValue(key, out LoadedPremiumModule module))
                    continue;

                if (module.PluginInstances != null)
                {
                    for (int i = module.PluginInstances.Count - 1; i >= 0; i--)
                    {
                        object pluginInstance = module.PluginInstances[i];
                        if (pluginInstance == null)
                            continue;

                        try
                        {
                            MethodInfo unloadMethod = pluginInstance.GetType().GetMethod(
                                "Unload",
                                BindingFlags.Public | BindingFlags.Instance);

                            unloadMethod?.Invoke(pluginInstance, null);
                        }
                        catch { }
                    }

                    module.PluginInstances.Clear();
                }

                try { module.Assembly = null; } catch { }

                LoadedPremiumModules.Remove(key);
            }

        }

        private static GameIdentity ReadIdentity()
        {
            GameIdentity identity = new GameIdentity();

            try
            {
                if (DestroyableSingleton<EOSManager>.InstanceExists)
                {
                    string eosFriendCode = Norm(DestroyableSingleton<EOSManager>.Instance.FriendCode);

                    if (!string.IsNullOrWhiteSpace(eosFriendCode))
                    {
                        identity.FriendCode = eosFriendCode;
                        identity.PlayerName = ExtractNameFromFriendCode(eosFriendCode);
                        return identity;
                    }
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(_capturedFriendCode))
            {
                identity.FriendCode = _capturedFriendCode;
                identity.PlayerName = string.IsNullOrWhiteSpace(_capturedPlayerName) ? "Unknown" : _capturedPlayerName;
                return identity;
            }

            try
            {
                ClientData localClient = GetLocalClientData();

                if (localClient != null)
                {
                    identity.FriendCode = Norm(localClient.FriendCode);
                    identity.PlayerName = Norm(localClient.PlayerName);

                    if (!string.IsNullOrWhiteSpace(identity.FriendCode))
                    {
                        CaptureClientData(localClient);
                        return identity;
                    }
                }
            }
            catch { }

            try
            {
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
                {
                    identity.FriendCode = Norm(PlayerControl.LocalPlayer.Data.FriendCode);
                    identity.PlayerName = Norm(PlayerControl.LocalPlayer.Data.PlayerName);

                    if (!string.IsNullOrWhiteSpace(identity.FriendCode))
                        return identity;
                }
            }
            catch { }

            return identity;
        }

        private static ClientData GetLocalClientData()
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return null;

                int localClientId = ObjectToInt(ReadInstanceObject(AmongUsClient.Instance, "ClientId"));

                ClientData direct = GetClientDataById(localClientId);

                if (direct != null)
                    return direct;

                return FindClientDataById(localClientId);
            }
            catch
            {
                return null;
            }
        }

        private static ClientData GetClientDataById(int clientId)
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return null;

                MethodInfo method = AmongUsClient.Instance.GetType().GetMethod(
                    "GetClient",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (method == null)
                    return null;

                object value = method.Invoke(AmongUsClient.Instance, new object[] { clientId });
                return value as ClientData;
            }
            catch
            {
                return null;
            }
        }

        private static ClientData FindClientDataById(int clientId)
        {
            try
            {
                if (AmongUsClient.Instance == null)
                    return null;

                object clients =
                    ReadInstanceObject(AmongUsClient.Instance, "allClients") ??
                    ReadInstanceObject(AmongUsClient.Instance, "AllClients") ??
                    ReadInstanceObject(AmongUsClient.Instance, "clients") ??
                    ReadInstanceObject(AmongUsClient.Instance, "Clients");

                if (clients == null)
                    return null;

                foreach (object item in EnumerateObjects(clients))
                {
                    ClientData client = item as ClientData;

                    if (client == null)
                        continue;

                    if (client.Id == clientId)
                        return client;
                }
            }
            catch { }

            return null;
        }

        private static IEnumerable<object> EnumerateObjects(object collection)
        {
            if (collection == null)
                yield break;

            IEnumerable enumerable = collection as IEnumerable;

            if (enumerable == null)
                yield break;

            foreach (object item in enumerable)
                yield return item;
        }

        private static int SafeGetActiveSceneHandle()
        {
            try
            {
                return UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene()
                    .handle;
            }
            catch
            {
                return int.MinValue;
            }
        }

        private static bool IsInLobbyOrGame()
        {
            try
            {
                AmongUsClient client = AmongUsClient.Instance;
                if (client == null)
                    return false;

                if (client.NetworkMode != NetworkModes.OnlineGame)
                    return false;

                if (client.GameId == 0)
                    return false;

                string code = GameCode.IntToGameName(client.GameId);
                return !string.IsNullOrWhiteSpace(code);
            }
            catch
            {
                return false;
            }
        }

        private static string SafeGetLobbyCode()
        {
            try
            {
                AmongUsClient client = AmongUsClient.Instance;

                if (client == null || client.GameId == 0)
                    return "";

                return GameCode.IntToGameName(client.GameId) ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static string SafeGetGameMode()
        {
            try
            {
                Type t = typeof(GameOptionsManager);
                object instance = t != null ? ReadStaticObject(t, "Instance") : null;
                object options = ReadInstanceObject(instance, "CurrentGameOptions") ?? ReadInstanceObject(instance, "GameOptions");
                object mode = ReadInstanceObject(options, "GameMode") ?? ReadInstanceObject(instance, "GameMode");
                return mode != null ? mode.ToString() : "";
            }
            catch
            {
                return "";
            }
        }

        private static int SafeGetKillCooldown()
        {
            object value = FindGameOptionValue("KillCooldown");
            return ObjectToInt(value);
        }

        private static int SafeGetMaxPlayers()
        {
            object value =
                FindGameOptionValue("MaxPlayers") ??
                FindGameOptionValue("maxPlayers") ??
                FindGameOptionValue("PlayerLimit") ??
                FindGameOptionValue("NumPlayers");

            int parsed = ObjectToInt(value);

            if (parsed <= 0)
                parsed = 15;

            return parsed;
        }

        private static object FindGameOptionValue(string contains)
        {
            try
            {
                Type t = typeof(GameOptionsManager);
                object instance = t != null ? ReadStaticObject(t, "Instance") : null;
                object options = ReadInstanceObject(instance, "CurrentGameOptions") ?? ReadInstanceObject(instance, "GameOptions");
                return FindNestedValue(options, contains, 3) ?? FindNestedValue(instance, contains, 3);
            }
            catch
            {
                return null;
            }
        }

        private static string SafeGetHostName()
        {
            try
            {
                if (AmongUsClient.Instance != null)
                {
                    int hostId = ObjectToInt(ReadInstanceObject(AmongUsClient.Instance, "HostId"));

                    ClientData hostClient = GetClientDataById(hostId) ?? FindClientDataById(hostId);

                    if (hostClient != null && !string.IsNullOrWhiteSpace(hostClient.PlayerName))
                        return hostClient.PlayerName.Trim();
                }
            }
            catch { }

            try
            {
                if (PlayerControl.AllPlayerControls == null)
                    return "";

                object hostId = ReadInstanceObject(AmongUsClient.Instance, "HostId");

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || p.Data == null)
                        continue;

                    object ownerId = ReadInstanceObject(p.Data, "OwnerId");

                    if (ownerId != null && hostId != null && ownerId.ToString() == hostId.ToString())
                        return p.Data.PlayerName;
                }
            }
            catch { }

            return "";
        }

        private static int SafeGetPlayerCount()
        {
            try
            {
                int count = 0;

                if (PlayerControl.AllPlayerControls != null)
                {
                    foreach (PlayerControl _ in PlayerControl.AllPlayerControls)
                        count++;

                    if (count > 0)
                        return count;
                }
            }
            catch { }

            try
            {
                object clients =
                    ReadInstanceObject(AmongUsClient.Instance, "allClients") ??
                    ReadInstanceObject(AmongUsClient.Instance, "AllClients") ??
                    ReadInstanceObject(AmongUsClient.Instance, "clients") ??
                    ReadInstanceObject(AmongUsClient.Instance, "Clients");

                int count = 0;

                foreach (object item in EnumerateObjects(clients))
                {
                    ClientData client = item as ClientData;

                    if (client != null)
                        count++;
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        private static bool IsKnownBaseAssembly(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            return name.Equals("BanMod", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("BanMod.Login", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Unity", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Il2Cpp", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("BepInEx", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Harmony", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Mono", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Assembly-CSharp", StringComparison.OrdinalIgnoreCase);
        }

        private static object ReadStaticObject(Type t, string name)
        {
            try
            {
                FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (f != null) return f.GetValue(null);

                PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (p != null && p.CanRead) return p.GetValue(null, null);
            }
            catch { }

            return null;
        }

        private static object ReadInstanceObject(object obj, string name)
        {
            if (obj == null)
                return null;

            try
            {
                Type t = obj.GetType();

                FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) return f.GetValue(obj);

                PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanRead) return p.GetValue(obj, null);
            }
            catch { }

            return null;
        }

        private static object FindNestedValue(object obj, string contains, int depth)
        {
            if (obj == null || depth <= 0)
                return null;

            try
            {
                Type t = obj.GetType();

                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (f.Name.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0)
                        return f.GetValue(obj);
                }

                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (p.CanRead && p.Name.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0)
                        return p.GetValue(obj, null);
                }

                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object child = null;
                    try { child = f.GetValue(obj); } catch { }
                    object found = FindNestedValue(child, contains, depth - 1);
                    if (found != null) return found;
                }
            }
            catch { }

            return null;
        }

        private static int ObjectToInt(object value)
        {
            if (value == null)
                return 0;

            if (value is int i) return i;
            if (value is float f) return Mathf.RoundToInt(f);
            if (value is double d) return (int)Math.Round(d);

            int parsed;
            return int.TryParse(value.ToString(), out parsed) ? parsed : 0;
        }

        private static string SafeGetOwnBanModSha256()
        {
            try
            {
                Assembly assembly = typeof(BanMod).Assembly;
                if (assembly != null && !string.IsNullOrWhiteSpace(assembly.Location) && File.Exists(assembly.Location))
                    return SafeSha256File(assembly.Location);
            }
            catch { }

            try
            {
                string fallback = Path.Combine(Paths.PluginPath, "BanMod.dll");
                if (File.Exists(fallback))
                    return SafeSha256File(fallback);
            }
            catch { }

            return "";
        }

        private static string SafeSha256File(string path)
        {
            try
            {
                using SHA256 sha = SHA256.Create();
                using FileStream fs = File.OpenRead(path);
                byte[] hash = sha.ComputeHash(fs);
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            catch { return ""; }
        }

        private static string SafeAssemblyFileName(Assembly asm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asm.Location))
                    return "";
                return Path.GetFileName(asm.Location);
            }
            catch { return ""; }
        }

        private static string SafeAssemblySha(Assembly asm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asm.Location) || !File.Exists(asm.Location))
                    return "";
                return SafeSha256File(asm.Location);
            }
            catch { return ""; }
        }

        private static string Norm(string v) => string.IsNullOrWhiteSpace(v) ? "" : v.Trim();

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

        private static void TrySetPlayerNameHeader(UnityWebRequest request)
        {
            if (request == null)
                return;

            try
            {
                request.SetRequestHeader(
                    "X-BANMOD-PlayerName",
                    MakeHeaderSafe(_playerName, "Unknown")
                );
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BANMOD] X-BANMOD-PlayerName header skipped: " +
                    ex.GetType().Name
                );
            }
        }

        private static string JsonString(string value)
        {
            if (value == null) value = "";
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
        }

        private static string BoolJson(bool value) => value ? "true" : "false";

        private static string Sha256Hex(byte[] data)
        {
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            StringBuilder sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }




        private sealed class GameIdentity { public string FriendCode; public string PlayerName; }
        public sealed class ChallengeResponse { public bool success { get; set; } public string nonce { get; set; } public int expires_in_seconds { get; set; } }
        public sealed class ActivationResponse
        {
            public bool success { get; set; }
            public string activation_token { get; set; }
            public string client_token { get; set; }
            public bool client_token_issued { get; set; }
            public bool client_token_required { get; set; }
            public int expires_in_seconds { get; set; }
            public bool recovery_only { get; set; }
            public string auth_level { get; set; }
            public bool device_required { get; set; }
            public string device_status { get; set; }
            public string device_key_id { get; set; }
            public string device_reason { get; set; }
            public bool update_required { get; set; }
            public string minimum_version { get; set; }
        }
        public sealed class ExtraReportResponse
        {
            public bool success { get; set; }
            public bool valid { get; set; }
            public bool accepted { get; set; }
            public bool report_authenticated { get; set; }
            public string reason { get; set; }
        }
        public sealed class AccessResponse
        {
            public bool success { get; set; }
            public bool premium_gate_ok { get; set; }
            public bool can_use_mod { get; set; }
            public bool blocked { get; set; }
            public bool force_disable_mod { get; set; }
            public bool force_disable { get; set; }
            public string force_disable_reason { get; set; }
            public string reason { get; set; }
            public bool developer_override { get; set; }
            public int premium_count { get; set; }
            public List<string> enabled_services { get; set; }
            public List<PremiumService> services { get; set; }
        }
        public sealed class PremiumService
        {
            public string key { get; set; }
            public string service_key { get; set; }
            public string serviceKey { get; set; }
            public string feature_key { get; set; }

            public string label { get; set; }
            public string version { get; set; }

            public bool enabled { get; set; }
            public bool allowed { get; set; }
            public bool autoload { get; set; }
            public bool auto_load { get; set; }
            public bool selected { get; set; }
            public bool server_allowed { get; set; }
            public bool integrable { get; set; }

            public bool has_payload { get; set; }
            public bool hasPayload { get; set; }
            public bool has_bin { get; set; }
            public bool hasBin { get; set; }
            public bool has_file { get; set; }
            public bool hasFile { get; set; }

            public string bin { get; set; }
            public string payload { get; set; }
            [JsonPropertyName("file")]
            public string json_file { get; set; }
            public string bin_file { get; set; }
            public string bin_filename { get; set; }
            public string filename { get; set; }
            public string file_name { get; set; }

            public string GetKey()
            {
                if (!string.IsNullOrWhiteSpace(key))
                    return key.Trim();

                if (!string.IsNullOrWhiteSpace(service_key))
                    return service_key.Trim();

                if (!string.IsNullOrWhiteSpace(serviceKey))
                    return serviceKey.Trim();

                if (!string.IsNullOrWhiteSpace(feature_key))
                    return feature_key.Trim();

                return "";
            }

            public bool GetAutoload()
            {
                return autoload || auto_load;
            }

            public string GetBinName()
            {
                if (!string.IsNullOrWhiteSpace(bin)) return bin.Trim();
                if (!string.IsNullOrWhiteSpace(payload)) return payload.Trim();
                if (!string.IsNullOrWhiteSpace(json_file)) return json_file.Trim();
                if (!string.IsNullOrWhiteSpace(bin_file)) return bin_file.Trim();
                if (!string.IsNullOrWhiteSpace(bin_filename)) return bin_filename.Trim();
                if (!string.IsNullOrWhiteSpace(filename)) return filename.Trim();
                if (!string.IsNullOrWhiteSpace(file_name)) return file_name.Trim();
                return "";
            }
        }
        public sealed class LoginManifest { public bool success { get; set; } public bool can_download { get; set; } public string version { get; set; } public string sha256 { get; set; } public int size_bytes { get; set; } public string download_url { get; set; } public string load_mode { get; set; } }
        public sealed class PremiumManifest { public bool success { get; set; } public bool can_download { get; set; } public string service_key { get; set; } public string feature_key { get; set; } public string version { get; set; } public string sha256 { get; set; } public int size_bytes { get; set; } public string download_url { get; set; } public string load_mode { get; set; } }
        public sealed class ActiveLobbiesResponse { public bool success { get; set; } public List<ModdedLobbyInfo> lobbies { get; set; } public int server_time { get; set; } }
        public sealed class ModdedLobbyInfo { public string lobby_code { get; set; } public string player_name { get; set; } public string friend_code { get; set; } public string host_friend_code { get; set; } public string host_name { get; set; } public string game_mode { get; set; } public int players { get; set; } public int kill_cooldown { get; set; } public int last_seen { get; set; } public bool is_public { get; set; } public bool is_private { get; set; } public bool share_lobby { get; set; } }
        private sealed class LoadedPremiumModule { public string ServiceKey; public Assembly Assembly; public List<object> PluginInstances; }
    }





    public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        public new bool Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }






    [HarmonyPatch(typeof(ClientData), nameof(ClientData.UpdatePlayerName))]
    public static class BanModClientDataNameOnlyPatch
    {
        public static void Postfix(ClientData __instance)
        {
            try
            {
                BanModCore.CaptureClientName(__instance);
            }
            catch { }
        }
    }


    [HarmonyPatch(typeof(FriendsListManager), nameof(FriendsListManager.CheckFriendCodeOnLogin))]
    public static class BanModFriendsListManagerCheckFriendCodePatch
    {
        public static void Postfix()
        {
            try
            {
                BanModCore.TryCaptureEosFriendCode("FriendsListManager.CheckFriendCodeOnLogin");
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.UpdateVisuals))]
    public static class BanModAccountManagerUpdateVisualsFriendCodePatch
    {
        public static void Postfix()
        {
            try
            {
                BanModCore.TryCaptureEosFriendCode("AccountManager.UpdateVisuals");
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    public static class BanModCoreStartupPatch
    {
        public static void Postfix()
        {
            try { BanModCore.TryStart(); } catch { }
            try { BanModCore.TickLobbyStatus(); } catch { }
        }
    }
}
