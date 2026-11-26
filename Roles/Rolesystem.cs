////credits and licenses in the res
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace BanMod
{
    public static class ForcedRoleSystem
    {
        public static readonly Dictionary<byte, RoleTypes> ForcedRoles =
            new Dictionary<byte, RoleTypes>();

        public static bool GM = false;

        private const string ForceRoleAuthorizeUrl =
            BanModCore.PublicApiBaseUrl + "/api/forcerole/authorize";

        private const string ForceRoleCommitUrl =
            BanModCore.PublicApiBaseUrl + "/api/forcerole/commit";

        private const string ForceRoleStatusUrl =
            BanModCore.PublicApiBaseUrl + "/api/forcerole/status";

        private const string ForceRoleEventUrl =
            BanModCore.PublicApiBaseUrl + "/api/forcerole/event";

        private const int RequestTimeoutSeconds = 10;

        private static readonly System.Random Random = new System.Random();

        private static bool _accessRefreshRunning = false;
        private static int _stateGeneration = 0;

        private static readonly Dictionary<byte, int> PendingRoleRequests =
            new Dictionary<byte, int>();
        private static int _nextRoleRequestId = 0;

        private static bool _selfRoleRequestRunning = false;
        private static int _queuedSelfRoleRequestId = 0;
        private static byte _queuedSelfRolePlayerId = byte.MaxValue;
        private static RoleTypes _queuedSelfRole = default;

        private static string _selfRoleAttemptId = "";
        private static string _selfRoleAuthorizationId = "";
        private static byte _authorizedSelfRolePlayerId = byte.MaxValue;
        private static RoleTypes _authorizedSelfRole = default;
        private static bool _selfRoleCommitStarted = false;

        private static bool _serverAccessKnown = false;
        private static bool _serverAccessAllowed = false;
        private static bool _serverSelfRoleAllowed = false;
        private static bool _serverPermanentUnlock = false;

        private static bool _selfRoleAssignedInCurrentGame = false;
        private static byte _assignedSelfRolePlayerId = byte.MaxValue;
        private static RoleTypes _assignedSelfRole = default;
        private static void ClearGameAssignmentState()
        {
            _selfRoleAssignedInCurrentGame = false;
            _assignedSelfRolePlayerId = byte.MaxValue;
            _assignedSelfRole = default;
            _selfRoleCommitStarted = false;
        }

        private static void ClearSelfRoleAuthorizationState(bool clearAttemptId)
        {
            _selfRoleAuthorizationId = "";
            _authorizedSelfRolePlayerId = byte.MaxValue;
            _authorizedSelfRole = default;

            if (clearAttemptId)
                _selfRoleAttemptId = "";
        }

        public sealed class ForceRoleLimitDecision
        {
            public bool success { get; set; }
            public bool allowed { get; set; }
            public bool premium { get; set; }

            [JsonPropertyName("self_allowed")]
            public bool self_allowed { get; set; }

            public int remaining { get; set; }

            [JsonPropertyName("reset_seconds")]
            public int reset_seconds { get; set; }

            public string reason { get; set; }

            [JsonPropertyName("authorization_id")]
            public string authorization_id { get; set; }

            [JsonPropertyName("attempt_id")]
            public string attempt_id { get; set; }

            public bool committed { get; set; }

            [JsonIgnore]
            public bool no_response { get; set; }
        }

        public static bool IsForcedImpostorRole(RoleTypes role)
        {
            return role == RoleTypes.Impostor ||
                   role == RoleTypes.Viper ||
                   role == RoleTypes.Shapeshifter ||
                   role == RoleTypes.Phantom;
        }

        public static bool IsLocalPlayerId(byte playerId)
        {
            try
            {
                return PlayerControl.LocalPlayer != null &&
                       PlayerControl.LocalPlayer.PlayerId == playerId;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasForcedSelfImpostorRole()
        {
            try
            {
                if (PlayerControl.LocalPlayer == null)
                    return false;

                return ForcedRoles.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out RoleTypes role) &&
                       IsForcedImpostorRole(role);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAmongUsNormalMode()
        {
            try
            {
                return GameOptionsManager.Instance != null &&
                       GameOptionsManager.Instance.CurrentGameOptions != null &&
                       GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsFfaModeActive()
        {
            try
            {
                if (Options.GameMode == null)
                    return false;

                string modeName = Options.GameMode.GetString();
                if (string.Equals(modeName, "FFA", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(modeName, "Free For All", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(modeName, "FreeForAll", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if (Options.GameMode.GetValue(GameModeType.FFA))
                    return true;
            }
            catch { }

            return false;
        }

        public static void DisableForFfa()
        {
            _stateGeneration++;
            PendingRoleRequests.Clear();
            _queuedSelfRoleRequestId = 0;
            _selfRoleRequestRunning = false;
            ForcedRoles.Clear();
            ClearGameAssignmentState();
            ClearSelfRoleAuthorizationState(true);
            _accessRefreshRunning = false;
            _serverAccessKnown = false;
            _serverAccessAllowed = false;
            _serverSelfRoleAllowed = false;
            _serverPermanentUnlock = false;
            RestoreOriginalGM("FFA attivo");
        }

        public static void ForcedRoleLog(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return;

                if (!message.Contains("ForceRole ON") &&
                    !message.Contains("ForceRole OFF") &&
                    !message.Contains("ForcedRole") &&
                    !message.Contains("PrimaryLimited") &&
                    !message.Contains("SetRole") &&
                    !message.Contains("ApplyExactRole"))
                    return;

                BMLogger.LogWarning("[ForcedRoleBlock] " + message);
            }
            catch
            {
                try
                {
                    if (!string.IsNullOrEmpty(message))
                        Debug.LogWarning("[ForcedRoleBlock] " + message);
                }
                catch { }
            }
        }

        private static void ShowSetRoleResult(string title, string message)
        {
            message = string.IsNullOrWhiteSpace(message) ? "Risposta server non disponibile." : message;
            ForcedRoleLog("SetRole UI | " + title + " | " + message);

            try
            {
                BanModCommunicationUi.EnsureCreated();
                if (BanModCommunicationUi.Instance != null)
                    BanModCommunicationUi.Instance.ShowMessagePopup(title, message);
            }
            catch { }
        }

        private static string FriendlySetRoleReason(ForceRoleLimitDecision decision)
        {
            string reason = decision != null ? (decision.reason ?? "") : "no_response";

            switch (reason)
            {
                case "limit_reached":
                    return "Limite SetRole raggiunto. Riprova più tardi.";
                case "friend_code_missing":
                case "missing_friend_code":
                case "friend_code_invalid":
                    return "FriendCode non disponibile. SetRole non autorizzato.";
                case "activation_token_missing":
                case "missing_activation_token":
                case "activation_token_invalid":
                case "activation_expired":
                    return "Sessione scaduta. Il client proverà a rinnovare il token.";
                case "player_security_blocked":
                case "ip_blocked":
                    return "Accesso SetRole bloccato dal server.";
                case "no_response":
                case "connection_error":
                case "timeout":
                    return "Il server SetRole non ha risposto. Il ruolo non è stato impostato.";
                default:
                    return string.IsNullOrWhiteSpace(reason)
                        ? "SetRole non autorizzato dal server."
                        : "SetRole non autorizzato: " + reason;
            }
        }

        private static IEnumerator SendOtherImpostorRoleEvent(
    byte targetPlayerId,
    RoleTypes role)
        {
            if (!IsForcedImpostorRole(role))
                yield break;

            bool identityOk = false;
            string friendCode = "";
            string playerName = "";
            string activationToken = "";

            yield return GetFreshApiIdentity((ok, fc, pn, token) =>
            {
                identityOk = ok;
                friendCode = fc;
                playerName = pn;
                activationToken = token;
            });

            if (!identityOk)
                yield break;

            PlayerControl target = FindPlayerById(targetPlayerId);

            string targetPlayerName =
                target != null && target.Data != null
                    ? target.Data.PlayerName ?? ""
                    : "";

            int requesterPlayerId =
                PlayerControl.LocalPlayer != null
                    ? PlayerControl.LocalPlayer.PlayerId
                    : -1;

            string eventId = Guid.NewGuid().ToString("N");

            string body = "{"
                + "\"FriendCode\":" + JsonString(friendCode) + ","
                + "\"PlayerName\":" + JsonString(playerName) + ","
                + "\"PlayerId\":" + requesterPlayerId + ","
                + "\"TargetIsSelf\":false,"
                + "\"TargetPlayerId\":" + targetPlayerId + ","
                + "\"TargetPlayerName\":" + JsonString(targetPlayerName) + ","
                + "\"Role\":" + JsonString(role.ToString()) + ","
                + "\"AttemptId\":" + JsonString(eventId)
                + "}";

            ForceRoleLimitDecision ignored = null;

            yield return SendForceRoleRequest(
                ForceRoleEventUrl,
                friendCode,
                body,
                value => ignored = value
            );
        }

        private static IEnumerator RequestAndApplyOtherForcedRole(
            byte playerId,
            RoleTypes role,
            int requestId)
        {
            ForceRoleLimitDecision decision = null;
            yield return CheckForcedRoleStatusFromServer(value => decision = value);

            if (!PendingRoleRequests.TryGetValue(playerId, out int currentRequestId) ||
                currentRequestId != requestId)
            {
                yield break;
            }

            PendingRoleRequests.Remove(playerId);

            if (IsFfaModeActive())
                yield break;

            if (decision == null || decision.no_response)
            {
                ShowSetRoleResult("SetRole", FriendlySetRoleReason(decision));
                yield break;
            }

            _serverAccessKnown = true;
            _serverAccessAllowed = decision.success && decision.allowed;
            _serverSelfRoleAllowed = decision.self_allowed || decision.premium;
            _serverPermanentUnlock = decision.premium;

            if (!_serverAccessAllowed)
            {
                ShowSetRoleResult("SetRole Denied", FriendlySetRoleReason(decision));
                yield break;
            }

            ForcedRoles[playerId] = role;

            if (IsForcedImpostorRole(role))
            {
                yield return SendOtherImpostorRoleEvent(
                    playerId,
                    role
                );
            }

            UpdateGMState("SetForcedRole server approved for other player");
        }

        private static void QueueSelfRoleAuthorization(
            byte playerId,
            RoleTypes role,
            int requestId)
        {
            _queuedSelfRolePlayerId = playerId;
            _queuedSelfRole = role;
            _queuedSelfRoleRequestId = requestId;

            if (_selfRoleRequestRunning || AmongUsClient.Instance == null)
                return;

            _selfRoleRequestRunning = true;
            AmongUsClient.Instance.StartCoroutine(
                ProcessSelfRoleAuthorizationQueue().WrapToIl2Cpp()
            );
        }

        private static IEnumerator ProcessSelfRoleAuthorizationQueue()
        {
            while (_queuedSelfRoleRequestId > 0 && !IsFfaModeActive())
            {
                int requestId = _queuedSelfRoleRequestId;
                byte playerId = _queuedSelfRolePlayerId;
                RoleTypes role = _queuedSelfRole;

                if (string.IsNullOrWhiteSpace(_selfRoleAttemptId))
                    _selfRoleAttemptId = Guid.NewGuid().ToString("N");

                string attemptId = _selfRoleAttemptId;
                ForceRoleLimitDecision decision = null;

                yield return AuthorizeSelfRoleFromServer(
                    playerId,
                    role,
                    attemptId,
                    value => decision = value
                );

                if (requestId != _queuedSelfRoleRequestId)
                    continue;

                _queuedSelfRoleRequestId = 0;

                if (decision == null || decision.no_response)
                {
                    ClearSelfRoleAuthorizationState(false);
                    ShowSetRoleResult("SetRole", FriendlySetRoleReason(decision));
                    continue;
                }

                _serverAccessKnown = true;
                _serverAccessAllowed = decision.success && decision.allowed;
                _serverSelfRoleAllowed = decision.self_allowed || decision.premium;
                _serverPermanentUnlock = decision.premium;

                if (!_serverAccessAllowed ||
                    (!_serverPermanentUnlock && !_serverSelfRoleAllowed))
                {
                    ClearSelfRoleAuthorizationState(false);
                    ForcedRoles.Remove(playerId);
                    ShowSetRoleResult("SetRole Denied", FriendlySetRoleReason(decision));
                    continue;
                }

                string authorizationId = decision.authorization_id ?? "";
                string returnedAttemptId = string.IsNullOrWhiteSpace(decision.attempt_id)
                    ? attemptId
                    : decision.attempt_id;

                if (!decision.premium && string.IsNullOrWhiteSpace(authorizationId))
                {
                    ClearSelfRoleAuthorizationState(false);
                    ForcedRoles.Remove(playerId);
                    ShowSetRoleResult(
                        "SetRole Denied",
                        "The server did not return the SetRole authorization."
                    );
                    continue;
                }

                bool alreadyHadRole = ForcedRoles.TryGetValue(
                    playerId,
                    out RoleTypes previousRole
                );
                bool roleActuallyChanged = !alreadyHadRole || previousRole != role;

                if (roleActuallyChanged)
                    ClearGameAssignmentState();

                _selfRoleAttemptId = returnedAttemptId;
                _selfRoleAuthorizationId = authorizationId;
                _authorizedSelfRolePlayerId = playerId;
                _authorizedSelfRole = role;

                ForcedRoles[playerId] = role;

                ForcedRoleLog(
                    "SetRole locale prenotato dal server" +
                    " | playerId=" + playerId +
                    " | previousRole=" + (alreadyHadRole ? previousRole.ToString() : "none") +
                    " | newRole=" + role +
                    " | remaining=" + decision.remaining +
                    " | premium=" + decision.premium
                );

                UpdateGMState("SetForcedRole self authorization ready");
            }

            _selfRoleRequestRunning = false;
        }

        public static void RequestAccessRefresh(string reason = "")
        {
            if (IsFfaModeActive())
                return;

            try
            {
                if (_accessRefreshRunning || AmongUsClient.Instance == null)
                    return;

                _accessRefreshRunning = true;
                AmongUsClient.Instance.StartCoroutine(
                    RefreshForcedRoleAccessFromServer(reason).WrapToIl2Cpp()
                );
            }
            catch (Exception ex)
            {
                _accessRefreshRunning = false;
                ForcedRoleLog("SetRole access refresh error: " + ex.Message);
            }
        }

        public static bool CanUseForcedRoleEffects()
        {
            return !IsFfaModeActive() && _serverAccessKnown && _serverAccessAllowed;
        }

        public static bool ShouldRunForcedRoleEffects()
        {
            return CanUseForcedRoleEffects();
        }

        private static bool HasForcedRoleOnLocalPlayer()
        {
            try
            {
                return PlayerControl.LocalPlayer != null &&
                       ForcedRoles.ContainsKey(PlayerControl.LocalPlayer.PlayerId);
            }
            catch
            {
                return false;
            }
        }

        private static bool HasForcedRoleOnOtherPlayer()
        {
            try
            {
                byte localId = PlayerControl.LocalPlayer != null
                    ? PlayerControl.LocalPlayer.PlayerId
                    : byte.MaxValue;

                return ForcedRoles.Keys.Any(playerId => playerId != localId);
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateGMState(string reason = "")
        {
            if (IsFfaModeActive())
            {
                RestoreOriginalGM("FFA attivo");
                return;
            }

            try
            {
                bool shouldEnable =
                    _serverAccessKnown &&
                    _serverAccessAllowed &&
                    !_serverPermanentUnlock &&
                    HasForcedRoleOnOtherPlayer() &&
                    !HasForcedRoleOnLocalPlayer();

                if (shouldEnable)
                    EnableGMForForcedRoleWithoutCheck(reason);
                else
                    RestoreOriginalGM(reason);
            }
            catch (Exception ex)
            {
                RestoreOriginalGM("GM state error: " + ex.Message);
            }
        }

        public static void EnableGMForForcedRole(string reason = "")
        {
            UpdateGMState(reason);
        }

        public static void EnableGMForForcedRoleWithoutCheck(string reason = "")
        {
            if (IsFfaModeActive())
                return;

            try
            {
                if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                    return;

                if (!GM)
                {
                    GM = true;
                    ForcedRoleLog("ForcedRole GM locale impostato su true. Motivo: " + reason);
                }
            }
            catch (Exception e)
            {
                ForcedRoleLog("ForcedRole ERRORE impostazione GM=true: " + e);
            }
        }

        public static void RestoreOriginalGM(string reason = "")
        {
            try
            {
                if (GM)
                {
                    GM = false;
                    ForcedRoleLog("ForcedRole GM locale ripristinato a false. Motivo: " + reason);
                }
            }
            catch (Exception e)
            {
                ForcedRoleLog("ForcedRole ERRORE ripristino GM=false: " + e);
            }
        }

        private static IEnumerator GetFreshApiIdentity(
            Action<bool, string, string, string> callback)
        {
            string friendCode = "";
            string playerName = "";

            try
            {
                friendCode = BanModCore.GetCurrentFriendCode() ?? "";
                playerName = BanModCore.GetCurrentPlayerName() ?? "";
            }
            catch (Exception ex)
            {
                ForcedRoleLog("SetRole errore identità locale: " + ex.Message);
                callback?.Invoke(false, "", "", "");
                yield break;
            }

            bool tokenReady = false;
            string clientToken = "";
            yield return BanModCore.EnsureActivationTokenForApi((success, token) =>
            {
                tokenReady = success;
                clientToken = token ?? "";
            });

            bool identityOk = !string.IsNullOrWhiteSpace(friendCode) &&
                              tokenReady &&
                              !string.IsNullOrWhiteSpace(clientToken);
            callback?.Invoke(identityOk, friendCode, playerName, clientToken);
        }

        private static IEnumerator SendForceRoleRequest(
            string url,
            string friendCode,
            string body,
            Action<ForceRoleLimitDecision> callback)
        {
            ForceRoleLimitDecision denied = new ForceRoleLimitDecision
            {
                success = false,
                allowed = false,
                premium = false,
                self_allowed = false,
                remaining = 0,
                reset_seconds = 0,
                reason = "server_error",
                authorization_id = "",
                attempt_id = "",
                committed = false,
                no_response = false
            };

            UnityWebRequest req = new UnityWebRequest(url, "POST");
            req.timeout = RequestTimeoutSeconds;
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            BanModApiTokenManager.ApplyAuthHeader(req);

            yield return req.SendWebRequest();

            string text = "";
            string requestError = "";
            UnityWebRequest.Result requestResult = req.result;
            long responseCode = req.responseCode;

            try
            {
                text = req.downloadHandler != null ? req.downloadHandler.text : "";
                requestError = req.error ?? "";
            }
            catch { }

            ForceRoleLimitDecision parsed = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                    parsed = JsonSerializer.Deserialize<ForceRoleLimitDecision>(text);
            }
            catch
            {
                parsed = null;
            }

            try { req.Dispose(); } catch { }

            if (parsed != null)
            {
                parsed.no_response = false;
                callback?.Invoke(parsed);
                yield break;
            }

            if (responseCode == 204)
            {
                denied.reason = "client_not_trusted";
                denied.no_response = false;
                callback?.Invoke(denied);
                yield break;
            }

            bool timedOut = requestError.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            requestError.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;

            if (requestResult != UnityWebRequest.Result.Success)
            {
                denied.no_response = timedOut || responseCode == 0 || responseCode >= 500;
                denied.reason = timedOut
                    ? "timeout"
                    : responseCode > 0
                        ? "http_" + responseCode
                        : "connection_error";
                callback?.Invoke(denied);
                yield break;
            }

            denied.reason = string.IsNullOrWhiteSpace(text)
                ? "empty_response"
                : "json_parse_error";
            denied.no_response = true;
            callback?.Invoke(denied);
        }

        private static IEnumerator CheckForcedRoleStatusFromServer(
            Action<ForceRoleLimitDecision> callback)
        {
            bool identityOk = false;
            string friendCode = "";
            string playerName = "";
            string activationToken = "";

            yield return GetFreshApiIdentity((ok, fc, pn, token) =>
            {
                identityOk = ok;
                friendCode = fc;
                playerName = pn;
                activationToken = token;
            });

            if (!identityOk)
            {
                callback?.Invoke(new ForceRoleLimitDecision
                {
                    success = false,
                    allowed = false,
                    self_allowed = false,
                    reason = "friend_code_missing",
                    no_response = false
                });
                yield break;
            }

            string body = "{"
                + "\"FriendCode\":" + JsonString(friendCode)
                + "}";

            yield return SendForceRoleRequest(
                ForceRoleStatusUrl,
                friendCode,
                body,
                callback
            );
        }

        private static IEnumerator AuthorizeSelfRoleFromServer(
            byte playerId,
            RoleTypes role,
            string attemptId,
            Action<ForceRoleLimitDecision> callback)
        {
            bool identityOk = false;
            string friendCode = "";
            string playerName = "";
            string activationToken = "";

            yield return GetFreshApiIdentity((ok, fc, pn, token) =>
            {
                identityOk = ok;
                friendCode = fc;
                playerName = pn;
                activationToken = token;
            });

            if (!identityOk)
            {
                callback?.Invoke(new ForceRoleLimitDecision
                {
                    success = false,
                    allowed = false,
                    self_allowed = false,
                    reason = "friend_code_missing",
                    no_response = false
                });
                yield break;
            }

            if (string.IsNullOrWhiteSpace(attemptId))
                attemptId = Guid.NewGuid().ToString("N");

            string body;

            if (IsForcedImpostorRole(role))
            {
                body = "{"
                    + "\"FriendCode\":" + JsonString(friendCode) + ","
                    + "\"PlayerName\":" + JsonString(playerName) + ","
                    + "\"AttemptId\":" + JsonString(attemptId) + ","
                    + "\"TargetIsSelf\":true,"
                    + "\"PlayerId\":" + playerId + ","
                    + "\"Role\":" + JsonString(role.ToString())
                    + "}";
            }
            else
            {
                body = "{"
                    + "\"FriendCode\":" + JsonString(friendCode) + ","
                    + "\"AttemptId\":" + JsonString(attemptId) + ","
                    + "\"TargetIsSelf\":true,"
                    + "\"PlayerId\":" + playerId + ","
                    + "\"Role\":" + JsonString(role.ToString())
                    + "}";
            }

            yield return SendForceRoleRequest(
                ForceRoleAuthorizeUrl,
                friendCode,
                body,
                callback
            );
        }

        private static IEnumerator CommitSelfRoleUseToServer(
            byte playerId,
            RoleTypes role,
            string authorizationId,
            string attemptId,
            Action<ForceRoleLimitDecision> callback)
        {
            ForceRoleLimitDecision finalDecision = null;

            for (int requestAttempt = 1; requestAttempt <= 3; requestAttempt++)
            {
                bool identityOk = false;
                string friendCode = "";
                string playerName = "";
                string activationToken = "";

                yield return GetFreshApiIdentity((ok, fc, pn, token) =>
                {
                    identityOk = ok;
                    friendCode = fc;
                    playerName = pn;
                    activationToken = token;
                });

                if (!identityOk)
                {
                    finalDecision = new ForceRoleLimitDecision
                    {
                        success = false,
                        allowed = false,
                        reason = "friend_code_missing",
                        no_response = false
                    };
                    break;
                }

                string body = "{"
                    + "\"FriendCode\":" + JsonString(friendCode) + ","
                    + "\"AuthorizationId\":" + JsonString(authorizationId) + ","
                    + "\"AttemptId\":" + JsonString(attemptId) + ","
                    + "\"PlayerId\":" + playerId + ","
                    + "\"Role\":" + JsonString(role.ToString()) + ","
                    + "\"Assigned\":true,"
                    + "\"AssignedRole\":" + JsonString(role.ToString())
                    + "}";

                ForceRoleLimitDecision decision = null;
                yield return SendForceRoleRequest(
                    ForceRoleCommitUrl,
                    friendCode,
                    body,
                    value => decision = value
                );

                finalDecision = decision;
                if (decision != null && !decision.no_response)
                    break;

                if (requestAttempt < 3)
                    yield return new WaitForSeconds(1.5f);
            }

            callback?.Invoke(finalDecision);
        }

        private static void ClearForcedRolesForServerDenial(string reason)
        {
            _stateGeneration++;
            ForcedRoles.Clear();
            ClearGameAssignmentState();
            RestoreOriginalGM(reason);
            ForcedRoleLog("SetRole rimossi dal server | reason=" + reason);
        }

        private static IEnumerator RefreshForcedRoleAccessFromServer(string reason)
        {
            int requestGeneration = _stateGeneration;

            if (IsFfaModeActive())
            {
                _accessRefreshRunning = false;
                yield break;
            }

            ForceRoleLimitDecision decision = null;
            yield return CheckForcedRoleStatusFromServer(value => decision = value);

            _accessRefreshRunning = false;

            if (requestGeneration != _stateGeneration || IsFfaModeActive())
                yield break;

            if (decision == null || decision.no_response)
            {
                ForcedRoleLog(
                    "SetRole status non disponibile; selezione mantenuta" +
                    " | reason=" + (decision != null ? decision.reason : "no_response")
                );
                yield break;
            }

            _serverAccessKnown = true;

            if (!decision.allowed)
            {
                _serverAccessAllowed = false;
                _serverSelfRoleAllowed = false;
                _serverPermanentUnlock = false;
                ClearForcedRolesForServerDenial(decision.reason ?? "server_denied");
                yield break;
            }

            _serverAccessAllowed = true;
            _serverSelfRoleAllowed = decision.self_allowed || decision.premium;
            _serverPermanentUnlock = decision.premium;

            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local != null &&
                    !_serverPermanentUnlock &&
                    !_serverSelfRoleAllowed &&
                    ForcedRoles.ContainsKey(local.PlayerId))
                {
                    ForcedRoles.Remove(local.PlayerId);
                    ClearGameAssignmentState();
                    ForcedRoleLog("SetRole locale rimosso: limite orario raggiunto");
                }
            }
            catch { }

            UpdateGMState("Access refresh: " + reason);
        }

        public static void MarkSelfRoleAssignmentForEndGame()
        {
            if (IsFfaModeActive())
                return;

            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null ||
                    local.Data == null ||
                    local.Data.Role == null ||
                    !ForcedRoles.TryGetValue(local.PlayerId, out RoleTypes requestedRole))
                {
                    ClearGameAssignmentState();
                    return;
                }

                if (local.Data.RoleType != requestedRole)
                {
                    ClearGameAssignmentState();
                    ForcedRoleLog(
                        "SetRole locale non confermato" +
                        " | requested=" + requestedRole +
                        " | actual=" + local.Data.RoleType
                    );
                    return;
                }

                _selfRoleAssignedInCurrentGame = true;
                _assignedSelfRolePlayerId = local.PlayerId;
                _assignedSelfRole = requestedRole;

                ForcedRoleLog(
                    "SetRole locale assegnato; commit immediato" +
                    " | playerId=" + local.PlayerId +
                    " | role=" + requestedRole
                );


                bool authorizationMatches =
                    _authorizedSelfRolePlayerId == local.PlayerId &&
                    _authorizedSelfRole == requestedRole &&
                    !string.IsNullOrWhiteSpace(_selfRoleAuthorizationId) &&
                    !string.IsNullOrWhiteSpace(_selfRoleAttemptId);

                if (!authorizationMatches)
                {
                    ForcedRoleLog(
                        "SetRole assegnato senza autorizzazione corrispondente" +
                        " | playerId=" + local.PlayerId +
                        " | role=" + requestedRole
                    );
                    return;
                }

                if (_selfRoleCommitStarted)
                    return;

                _selfRoleCommitStarted = true;

                string authorizationId = _selfRoleAuthorizationId;
                string attemptId = _selfRoleAttemptId;

                if (AmongUsClient.Instance != null)
                {
                    AmongUsClient.Instance.StartCoroutine(
                        CommitAssignedSelfRoleUse(
                            local.PlayerId,
                            requestedRole,
                            authorizationId,
                            attemptId
                        ).WrapToIl2Cpp()
                    );
                }
            }
            catch (Exception ex)
            {
                ForcedRoleLog("SetRole assignment/commit start error: " + ex.Message);
            }
        }

        private static IEnumerator CommitAssignedSelfRoleUse(
            byte playerId,
            RoleTypes role,
            string authorizationId,
            string attemptId)
        {
            ForceRoleLimitDecision commit = null;

            yield return CommitSelfRoleUseToServer(
                playerId,
                role,
                authorizationId,
                attemptId,
                value => commit = value
            );

            bool committed = commit != null &&
                             commit.success &&
                             (commit.committed || commit.premium);

            if (committed)
            {
                _serverSelfRoleAllowed = commit.self_allowed || commit.premium;
                _serverPermanentUnlock = commit.premium;

                ForcedRoleLog(
                    "SetRole utilizzo registrato all'inizio partita" +
                    " | remaining=" + commit.remaining +
                    " | reason=" + commit.reason
                );

                if (string.Equals(
                    _selfRoleAuthorizationId,
                    authorizationId,
                    StringComparison.Ordinal))
                {
                    ClearSelfRoleAuthorizationState(true);
                }
            }
            else
            {
                _selfRoleCommitStarted = false;
                ForcedRoleLog(
                    "SetRole commit non confermato" +
                    " | reason=" + (commit != null ? commit.reason : "no_response")
                );
            }
        }

        public static void FinishSetRoleGame()
        {
            _stateGeneration++;
            PendingRoleRequests.Clear();
            _queuedSelfRoleRequestId = 0;
            ForcedRoles.Clear();
            ClearGameAssignmentState();
            ClearSelfRoleAuthorizationState(true);
            _accessRefreshRunning = false;
            _serverAccessKnown = false;
            _serverAccessAllowed = false;
            _serverSelfRoleAllowed = false;
            _serverPermanentUnlock = false;
            RestoreOriginalGM("Fine match");
        }

        public static void SetForcedRole(byte playerId, RoleTypes role)
        {
            if (IsFfaModeActive())
            {
                DisableForFfa();
                return;
            }
            if (AmongUsClient.Instance == null)
            {
                ShowSetRoleResult("SetRole", "Client non pronto. Riprova tra pochi secondi.");
                return;
            }

            int requestId = ++_nextRoleRequestId;

            ForcedRoleLog(
                "SetRole richiesta al server" +
                " | playerId=" + playerId +
                " | role=" + role +
                " | requestId=" + requestId
            );

            if (IsLocalPlayerId(playerId))
            {
                QueueSelfRoleAuthorization(playerId, role, requestId);
                return;
            }

            PendingRoleRequests[playerId] = requestId;
            AmongUsClient.Instance.StartCoroutine(
                RequestAndApplyOtherForcedRole(playerId, role, requestId).WrapToIl2Cpp()
            );
        }

        public static void SetForcedRoleNoLimitForPremium(byte playerId, RoleTypes role)
        {
            SetForcedRole(playerId, role);
        }

        public static void Clear()
        {
            _stateGeneration++;
            PendingRoleRequests.Clear();
            _queuedSelfRoleRequestId = 0;
            ForcedRoles.Clear();

            if (!_selfRoleAssignedInCurrentGame)
            {
                ClearGameAssignmentState();
                ClearSelfRoleAuthorizationState(true);
            }

            _accessRefreshRunning = false;
            RestoreOriginalGM("ForcedRoles.Clear");
            ForcedRoleLog("ForcedRoles.Clear()");
        }

        public static void ClearForcedRole(byte playerId)
        {
            PendingRoleRequests.Remove(playerId);

            if (IsLocalPlayerId(playerId) || _assignedSelfRolePlayerId == playerId)
            {
                _queuedSelfRoleRequestId = 0;
                ClearGameAssignmentState();
                ClearSelfRoleAuthorizationState(false);
            }

            if (ForcedRoles.Remove(playerId))
            {
                ForcedRoleLog(
                    "ClearForcedRole | playerId=" + playerId +
                    " | count=" + ForcedRoles.Count
                );
            }

            UpdateGMState("ClearForcedRole");
        }

        public static PlayerControl FindPlayerById(byte playerId)
        {
            try
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.PlayerId == playerId)
                        return player;
                }
            }
            catch { }

            try
            {
                return BanMod.AllAlivePlayerControls
                    .FirstOrDefault(p => p != null && p.PlayerId == playerId);
            }
            catch
            {
                return null;
            }
        }


        public static bool TryGetForcedRole(byte playerId, out RoleTypes role)
        {
            return ForcedRoles.TryGetValue(playerId, out role);
        }

        public static bool TrySetRandomAvailableSpecialImpostorRole(
            byte playerId,
            out RoleTypes selectedRole)
        {
            selectedRole = default;

            if (IsFfaModeActive())
                return false;

            try
            {
                if (_serverAccessKnown && !_serverAccessAllowed)
                    return false;

                HashSet<RoleTypes> alreadyChosenRoles = ForcedRoles
                    .Where(x => x.Key != playerId)
                    .Select(x => x.Value)
                    .Where(role => ForcedRoleHelpers.SpecialImpostorRoles.Contains(role))
                    .ToHashSet();

                RoleTypes[] possibleRoles = ForcedRoleHelpers.SpecialImpostorRoles
                    .Where(ForcedRoleHelpers.IsEnabledSpecialImpostorRole)
                    .Where(role => !alreadyChosenRoles.Contains(role))
                    .ToArray();

                if (possibleRoles.Length == 0)
                    return false;

                selectedRole = possibleRoles[Random.Next(possibleRoles.Length)];

                SetForcedRole(playerId, selectedRole);
                return true;
            }
            catch
            {
                selectedRole = default;
                return false;
            }
        }

        private static string JsonString(string value)
        {
            if (value == null)
                value = "";

            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") + "\"";
        }
    }

    public static class ForcedRoleHelpers
    {
        public class RoleRateBackup
        {
            public int Count;
            public int Chance;

            public RoleRateBackup(int count, int chance)
            {
                Count = count;
                Chance = chance;
            }
        }

        public static readonly RoleTypes[] VanillaSpecialAssignableRoles =
        {
            RoleTypes.Scientist,
            RoleTypes.Engineer,
            RoleTypes.Shapeshifter,
            RoleTypes.Noisemaker,
            RoleTypes.Phantom,
            RoleTypes.Tracker,
            RoleTypes.Detective,
            RoleTypes.SpiritGuide,
            RoleTypes.Viper
        };

        public static readonly RoleTypes[] SpecialImpostorRoles =
        {
            RoleTypes.Viper,
            RoleTypes.Shapeshifter,
            RoleTypes.Phantom
        };

        public static bool IsAliveValidPlayer(PlayerControl p)
        {
            return p != null &&
                   p.Data != null &&
                   !p.Data.Disconnected &&
                   !p.Data.IsDead;
        }

        public static bool IsAssignableAtGameStart(RoleTypes role)
        {
            switch (role)
            {
                case RoleTypes.Crewmate:
                case RoleTypes.Impostor:
                case RoleTypes.Scientist:
                case RoleTypes.Engineer:
                case RoleTypes.Shapeshifter:
                case RoleTypes.Noisemaker:
                case RoleTypes.Phantom:
                case RoleTypes.Tracker:
                case RoleTypes.Detective:
                case RoleTypes.SpiritGuide:
                case RoleTypes.Viper:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsVanillaSpecialAssignableRole(RoleTypes role)
        {
            return VanillaSpecialAssignableRoles.Contains(role);
        }

        public static int GetConfiguredRoleCount(RoleTypes role)
        {
            try
            {
                return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
            }
            catch
            {
                return 0;
            }
        }

        public static int GetConfiguredRoleChance(RoleTypes role)
        {
            try
            {
                return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(role);
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsRoleAvailableInOptions(RoleTypes role)
        {
            if (role == RoleTypes.Crewmate || role == RoleTypes.Impostor)
                return true;

            return GetConfiguredRoleCount(role) > 0 &&
                   GetConfiguredRoleChance(role) > 0;
        }

        public static bool IsFourImpEnabled()
        {
            try
            {
                return Options.MoreImp.GetBool();
            }
            catch
            {
                return false;
            }
        }

        public static bool ShouldForceFourImpostors(int alivePlayersCount)
        {
            return IsFourImpEnabled() && alivePlayersCount > 12;
        }

        public static bool IsEnabledSpecialImpostorRole(RoleTypes role)
        {
            if (!SpecialImpostorRoles.Contains(role))
                return false;

            int count = GetConfiguredRoleCount(role);
            int chance = GetConfiguredRoleChance(role);

            return count > 0 && chance > 0;
        }

        public static Dictionary<RoleTypes, RoleRateBackup> ReduceRoleOptionsForForcedExactRoles(
            Dictionary<RoleTypes, int> forcedCountByRole)
        {
            Dictionary<RoleTypes, RoleRateBackup> backup = new Dictionary<RoleTypes, RoleRateBackup>();

            if (forcedCountByRole == null || forcedCountByRole.Count == 0)
                return backup;

            var options = GameOptionsManager.Instance.CurrentGameOptions;
            if (options == null || options.RoleOptions == null)
                return backup;

            foreach (var kvp in forcedCountByRole)
            {
                RoleTypes role = kvp.Key;
                int forcedCount = kvp.Value;

                if (forcedCount <= 0)
                    continue;

                if (!IsVanillaSpecialAssignableRole(role))
                    continue;

                int originalCount = GetConfiguredRoleCount(role);
                int originalChance = GetConfiguredRoleChance(role);

                backup[role] = new RoleRateBackup(originalCount, originalChance);

                int newCount = Math.Max(0, originalCount - forcedCount);
                int newChance = newCount <= 0 ? 0 : originalChance;

                try
                {
                    options.RoleOptions.SetRoleRate(role, newCount, newChance);

                }
                catch (Exception)
                {
                }
            }

            return backup;
        }
        public static Dictionary<RoleTypes, RoleRateBackup> ForceFourImpSpecialRoleRates()
        {
            Dictionary<RoleTypes, RoleRateBackup> backup =
                new Dictionary<RoleTypes, RoleRateBackup>();

            try
            {
                var options = GameOptionsManager.Instance.CurrentGameOptions;

                if (options == null || options.RoleOptions == null)
                    return backup;

                BackupAndSetRoleRate(
                    options,
                    backup,
                    RoleTypes.Viper,
                    0,
                    0
                );

                BackupAndSetRoleRate(
                    options,
                    backup,
                    RoleTypes.Phantom,
                    0,
                    0
                );

                BackupAndSetRoleRate(
                    options,
                    backup,
                    RoleTypes.Shapeshifter,
                    0,
                    0
                );
            }
            catch
            {
            }

            return backup;
        }

        private static void BackupAndSetRoleRate(
            IGameOptions options,
            Dictionary<RoleTypes, RoleRateBackup> backup,
            RoleTypes role,
            int count,
            int chance)
        {
            try
            {
                if (!backup.ContainsKey(role))
                {
                    backup[role] = new RoleRateBackup(
                        GetConfiguredRoleCount(role),
                        GetConfiguredRoleChance(role)
                    );
                }

                options.RoleOptions.SetRoleRate(role, count, chance);
            }
            catch
            {
            }
        }
        public static void RestoreRoleOptions(Dictionary<RoleTypes, RoleRateBackup> backup)
        {
            if (backup == null || backup.Count == 0)
                return;

            var options = GameOptionsManager.Instance.CurrentGameOptions;
            if (options == null || options.RoleOptions == null)
                return;

            foreach (var kvp in backup)
            {
                RoleTypes role = kvp.Key;
                RoleRateBackup data = kvp.Value;

                try
                {
                    options.RoleOptions.SetRoleRate(role, data.Count, data.Chance);

                }
                catch (Exception)
                {
                }
            }
        }

        public static List<RoleTypes> BuildConfiguredSpecialImpostorPool(
    System.Random rng)
        {
            List<RoleTypes> pool = new List<RoleTypes>();

            foreach (RoleTypes role in SpecialImpostorRoles)
            {
                int count = GetConfiguredRoleCount(role);
                int chance = GetConfiguredRoleChance(role);

                if (count <= 0 || chance <= 0)
                    continue;

                for (int i = 0; i < count; i++)
                    pool.Add(role);
            }

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);

                RoleTypes temp = pool[i];
                pool[i] = pool[j];
                pool[j] = temp;
            }

            if (pool.Count > 4)
                pool = pool.Take(4).ToList();

            return pool;
        }

        public static void RemoveAlreadyUsedSpecialImpostorRolesFromPool(
            List<RoleTypes> pool,
            List<PlayerControl> allPlayers)
        {
            if (pool == null || allPlayers == null)
                return;

            foreach (var player in allPlayers)
            {
                if (player == null || player.Data == null || player.Data.Role == null)
                    continue;

                RoleTypes currentRole = player.Data.RoleType;

                if (!SpecialImpostorRoles.Contains(currentRole))
                    continue;

                int index = pool.IndexOf(currentRole);

                if (index >= 0)
                    pool.RemoveAt(index);
            }
        }

        public static void ApplyFourImpSpecialFill(
    List<PlayerControl> allPlayers,
    HashSet<byte> exactAssignedPlayers,
    List<RoleTypes> configuredPool,
    System.Random rng)
        {
            if (allPlayers == null || rng == null)
                return;

            if (!ShouldForceFourImpostors(allPlayers.Count))
                return;

            if (configuredPool == null)
                return;

            configuredPool = new List<RoleTypes>(configuredPool);

            RemoveAlreadyUsedSpecialImpostorRolesFromPool(
                configuredPool,
                allPlayers
            );

            if (configuredPool.Count == 0)
                return;

            var baseImpostors = allPlayers
                .Where(p => p != null && p.Data != null)
                .Where(p => p.Data.RoleType == RoleTypes.Impostor)
                .Where(p =>
                {
                    if (exactAssignedPlayers == null)
                        return true;

                    if (exactAssignedPlayers.Contains(p.PlayerId) &&
                        ForcedRoleSystem.TryGetForcedRole(
                            p.PlayerId,
                            out RoleTypes forcedRole) &&
                        forcedRole == RoleTypes.Impostor)
                    {
                        return false;
                    }

                    return true;
                })
                .ToList();

            foreach (var player in baseImpostors)
            {
                if (configuredPool.Count == 0)
                    break;

                int index = rng.Next(configuredPool.Count);

                RoleTypes selectedRole = configuredPool[index];
                configuredPool.RemoveAt(index);

                ApplyExactRole(player, selectedRole);
            }
        }

        public static void SendForcedRoleChatMessage(PlayerControl player, RoleTypes role)
        {
            try
            {
                if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                    return;

                if (PlayerControl.LocalPlayer == null)
                    return;

                string message = Translator.GetString("Roleset");
                PlayerControl.LocalPlayer.RpcSendChat(message);
            }
            catch
            {
            }
        }

        public static void ApplyExactRole(PlayerControl player, RoleTypes role)
        {
            ForcedRoleSystem.ForcedRoleLog($"ApplyExactRole SOLO ruolo | player={player?.Data?.PlayerName ?? "null"} | role={role}");
            if (player == null || player.Data == null)
                return;

            try
            {
                RoleManager.Instance.SetRole(player, role);
            }
            catch
            {
            }

            try
            {
                player.RpcSetRole(role, false);
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class RoleManager_SelectRoles_Patch
    {
        public static bool Prefix()
        {
            ForcedRoleSystem.ForcedRoleLog("RoleManager.SelectRoles Prefix chiamato");
            GameModeType gameMode = Options.GameMode.Selected;

            if (AmongUsClient.Instance == null)
            {
                ForcedRoleSystem.ForcedRoleLog("SelectRoles stop: AmongUsClient.Instance null");
                return true;
            }

            if (!AmongUsClient.Instance.AmHost)
            {
                ForcedRoleSystem.ForcedRoleLog("SelectRoles stop: non host");
                return true;
            }

            if (ForcedRoleSystem.IsFfaModeActive())
            {
                ForcedRoleSystem.DisableForFfa();
                return true;
            }

            if (Options.EnableFFA.GetBool())
            {
                return true;
            }

            if (gameMode == GameModeType.TaskRun ||
                gameMode == GameModeType.ZombieMode ||
                gameMode == GameModeType.RoomRush ||
                gameMode == GameModeType.TargetRush ||
                gameMode == GameModeType.KillRace ||
                gameMode == GameModeType.Assassin ||
                gameMode == GameModeType.FFA ||
                gameMode == GameModeType.FFATeam ||
                gameMode == GameModeType.DeathRun ||
                gameMode == GameModeType.HotPotatoModded)
            {
                return true;
            }

            if (Options.Jester.GetBool() && !Jester.JesterSelected)
            {
                Jester.SelectJester();
            }

            var allPlayersList = BanMod.AllAlivePlayerControls
                .Where(ForcedRoleHelpers.IsAliveValidPlayer)
                .ToList();

            PlayerControl localPlayer = PlayerControl.LocalPlayer;

            bool localIsGM =
                localPlayer != null &&
                (BanMod.GM.Value || ForcedRoleSystem.GM);

            bool hasForcedExactRoles = ForcedRoleSystem.ForcedRoles.Count > 0;
            bool hasForcedImpostors = BanMod.forcedImpostorIds.Count > 0;
            bool jesterActive = Options.Jester.GetBool() || Jester.ForcedJesterSelected;
            bool hideAndSeek = GameManager.Instance.IsHideAndSeek();
            bool taskRun = gameMode == GameModeType.TaskRun;
            bool zombieMode = gameMode == GameModeType.ZombieMode;
            bool hotpotatoMode = gameMode == GameModeType.HotPotatoModded;
            bool roomrushMode = gameMode == GameModeType.RoomRush;
            bool targetrushMode = gameMode == GameModeType.TargetRush;
            bool deathrunMode = gameMode == GameModeType.DeathRun;
            bool zeroImpostorsMode = taskRun || zombieMode || roomrushMode || targetrushMode || deathrunMode || hotpotatoMode;
            bool fourImpActive = !zeroImpostorsMode && !hideAndSeek && ForcedRoleHelpers.ShouldForceFourImpostors(allPlayersList.Count);

            ForcedRoleSystem.ForcedRoleLog($"SelectRoles flags | taskRun={taskRun} hideAndSeek={hideAndSeek} jesterActive={jesterActive} forceImpostor={BanMod.forceImpostor} hasForcedImpostors={hasForcedImpostors} hasForcedExactRoles={hasForcedExactRoles} fourImpActive={fourImpActive} forcedRolesCount={ForcedRoleSystem.ForcedRoles.Count}");

            if (!zeroImpostorsMode &&
                !hideAndSeek &&
                !jesterActive &&
                !BanMod.forceImpostor &&
                !hasForcedImpostors &&
                !hasForcedExactRoles &&
                !fourImpActive &&
                !localIsGM)
            {
                ForcedRoleSystem.ForcedRoleLog("SelectRoles usa vanilla: nessuna condizione attiva");
                return true;
            }

            var gameOptions = GameOptionsManager.Instance.CurrentGameOptions;

            if (GameManager.Instance != null && GameManager.Instance.IsHideAndSeek())
            {
                PrepareHnsSeekers();

                var hnsOptions =
                    GameOptionsManager.Instance?
                        .CurrentGameOptions?
                        .Cast<HideNSeekGameOptionsV12>();

                if (hnsOptions != null)
                {
                    int seekerCount = Math.Max(1, HnsSeekers.Count);

                    hnsOptions.NumImpostors = seekerCount;

                    if (HnsSeekers.Count > 0)
                    {
                        hnsOptions.ImpostorPlayerID = HnsSeekers[0];
                    }
                }


                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == null ||
                        player.Data == null ||
                        player.Data.Disconnected)
                    {
                        continue;
                    }

                    try
                    {
                        bool isSeeker =
                            HnsSeekers.Contains(player.PlayerId);

                        if (Options.EnableSNS.GetBool())
                        {
                            if (isSeeker)
                            {
                                RoleManager.Instance.SetRole(
                                    player,
                                    RoleTypes.Shapeshifter);

                                player.RpcSetRole(
                                    RoleTypes.Shapeshifter,
                                    false);

                                BMLogger.Info(
                                    $"[HnS] Shapeshifter assegnato: " +
                                    $"{player.Data.PlayerName} " +
                                    $"PlayerId={player.PlayerId}");
                            }
                            else
                            {
                                RoleManager.Instance.SetRole(
                                    player,
                                    RoleTypes.Engineer);

                                player.RpcSetRole(
                                    RoleTypes.Engineer,
                                    false);
                            }
                        }
                        else
                        {
                            if (isSeeker)
                            {
                                RoleManager.Instance.SetRole(
                                    player,
                                    RoleTypes.Impostor);

                                player.RpcSetRole(
                                    RoleTypes.Impostor,
                                    false);

                                BMLogger.Info(
                                    $"[HnS] Seeker assegnato: " +
                                    $"{player.Data.PlayerName} " +
                                    $"PlayerId={player.PlayerId}");
                            }
                            else
                            {
                                RoleManager.Instance.SetRole(
                                    player,
                                    RoleTypes.Engineer);

                                player.RpcSetRole(
                                    RoleTypes.Engineer,
                                    false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        BMLogger.LogError(
                            $"[HnS] Errore assegnazione ruolo " +
                            $"PlayerId={player.PlayerId}: {ex}");
                    }
                }

                return false;
            }

            bool hasRealJester =
                Options.Jester.GetBool() &&
                Jester.JesterSelected &&
                Jester.JesterId != 255;


            Dictionary<RoleTypes, ForcedRoleHelpers.RoleRateBackup> roleOptionBackup = null;

            try
            {
                int adjustedNumImpostors = zeroImpostorsMode
                    ? 0
                    : gameOptions.GetAdjustedNumImpostors(allPlayersList.Count);

                if (!zeroImpostorsMode && ForcedRoleHelpers.ShouldForceFourImpostors(allPlayersList.Count))
                {
                    adjustedNumImpostors = Math.Min(Options.NumImpostor.GetInt(), allPlayersList.Count);
                }


                int crewSlotsTotal = Math.Max(0, allPlayersList.Count - adjustedNumImpostors);

                var rng = new System.Random();

                List<RoleTypes> fourImpSpecialPool = null;

                if (fourImpActive)
                {
                    fourImpSpecialPool =
                        ForcedRoleHelpers.BuildConfiguredSpecialImpostorPool(rng);
                }

                var exactAssignedPlayers = new HashSet<byte>();
                var appliedForcedCountByRole = new Dictionary<RoleTypes, int>();

                int usedImpostorSlots = 0;
                int usedCrewSlots = 0;

                foreach (var kvp in ForcedRoleSystem.ForcedRoles.ToList())
                {
                    byte playerId = kvp.Key;
                    RoleTypes forcedRole = kvp.Value;

                    var player = allPlayersList.FirstOrDefault(p => p.PlayerId == playerId);
                    if (player == null)
                    {
                        continue;
                    }
                    if (localIsGM && localPlayer != null && player.PlayerId == localPlayer.PlayerId)
                    {
                        continue;
                    }
                    if (hasRealJester && Jester.IsJester(player))
                    {
                        continue;
                    }

                    if (exactAssignedPlayers.Contains(player.PlayerId))
                        continue;

                    if (!ForcedRoleHelpers.IsAssignableAtGameStart(forcedRole))
                    {
                        continue;
                    }

                    bool isImpostorRole = RoleManager.IsImpostorRole(forcedRole);
                    bool isSpecial = forcedRole != RoleTypes.Crewmate && forcedRole != RoleTypes.Impostor;

                    if (isImpostorRole)
                    {
                        if (usedImpostorSlots >= adjustedNumImpostors)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (usedCrewSlots >= crewSlotsTotal)
                        {
                            continue;
                        }
                    }

                    if (isSpecial && !fourImpActive)
                    {
                        if (!ForcedRoleHelpers.IsRoleAvailableInOptions(forcedRole))
                        {
                            continue;
                        }

                        int configuredCount = ForcedRoleHelpers.GetConfiguredRoleCount(forcedRole);
                        int alreadyApplied = appliedForcedCountByRole.TryGetValue(forcedRole, out var n) ? n : 0;

                        if (configuredCount > 0 && alreadyApplied >= configuredCount)
                        {
                            continue;
                        }
                    }

                    exactAssignedPlayers.Add(player.PlayerId);

                    if (!appliedForcedCountByRole.ContainsKey(forcedRole))
                        appliedForcedCountByRole[forcedRole] = 0;

                    appliedForcedCountByRole[forcedRole]++;

                    if (isImpostorRole)
                        usedImpostorSlots++;
                    else
                        usedCrewSlots++;
                }

                if (fourImpActive)
                {
                    roleOptionBackup = ForcedRoleHelpers.ForceFourImpSpecialRoleRates();
                }
                else
                {
                    roleOptionBackup = ForcedRoleHelpers.ReduceRoleOptionsForForcedExactRoles(
                        appliedForcedCountByRole
                    );
                }

                int impostorSlotsRemaining = Math.Max(0, adjustedNumImpostors - usedImpostorSlots);

                var impostorsToAssign = new List<PlayerControl>();

                var forcedImpostorCandidates = allPlayersList
                    .Where(p => !localIsGM || localPlayer == null || p.PlayerId != localPlayer.PlayerId)
                    .Where(p => BanMod.forcedImpostorIds.Contains(p.PlayerId))
                    .Where(p => !exactAssignedPlayers.Contains(p.PlayerId))
                    .Where(p => !(hasRealJester && Jester.IsJester(p)))
                    .ToList();

                if (forcedImpostorCandidates.Count > impostorSlotsRemaining)
                    forcedImpostorCandidates = forcedImpostorCandidates.Take(impostorSlotsRemaining).ToList();

                impostorsToAssign.AddRange(forcedImpostorCandidates);

                if (impostorsToAssign.Count < impostorSlotsRemaining)
                {
                    int needed = impostorSlotsRemaining - impostorsToAssign.Count;

                    var randomImpostorCandidates = allPlayersList
                        .Where(p => !localIsGM || localPlayer == null || p.PlayerId != localPlayer.PlayerId)
                        .Where(p => !exactAssignedPlayers.Contains(p.PlayerId))
                        .Where(p => !impostorsToAssign.Contains(p))
                        .Where(p => !(hasRealJester && Jester.IsJester(p)))
                        .OrderBy(_ => rng.Next())
                        .Take(needed)
                        .ToList();

                    impostorsToAssign.AddRange(randomImpostorCandidates);
                }

                var crewmatesToAssign = allPlayersList
                    .Where(p => !exactAssignedPlayers.Contains(p.PlayerId))
                    .Where(p => !impostorsToAssign.Contains(p))
                    .Where(p => !(hasRealJester && Jester.IsJester(p)))
                    .ToList();

                if (localIsGM && localPlayer != null)
                {
                    var gmPlayer = crewmatesToAssign
                        .FirstOrDefault(p => p.PlayerId == localPlayer.PlayerId);

                    if (gmPlayer != null)
                    {
                        ForcedRoleHelpers.ApplyExactRole(
                            gmPlayer,
                            RoleTypes.Crewmate
                        );

                        crewmatesToAssign.Remove(gmPlayer);
                    }
                }

                if (impostorsToAssign.Count > 0)
                {
                    if (fourImpActive)
                    {
                        foreach (var imp in impostorsToAssign)
                        {
                            if (imp == null || imp.Data == null)
                                continue;

                            ForcedRoleHelpers.ApplyExactRole(
                                imp,
                                RoleTypes.Impostor
                            );
                        }
                    }
                    else
                    {
                        var impostorInfos =
                            new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();

                        foreach (var imp in impostorsToAssign)
                            impostorInfos.Add(imp.Data);

                        GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(
                            impostorInfos,
                            gameOptions,
                            RoleTeamTypes.Impostor,
                            int.MaxValue,
                            new Il2CppSystem.Nullable<RoleTypes>(RoleTypes.Impostor)
                        );
                    }
                }

                if (crewmatesToAssign.Count > 0)
                {
                    var crewmateInfos = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();
                    foreach (var cm in crewmatesToAssign)
                        crewmateInfos.Add(cm.Data);

                    GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(
                        crewmateInfos,
                        gameOptions,
                        RoleTeamTypes.Crewmate,
                        int.MaxValue,
                        new Il2CppSystem.Nullable<RoleTypes>(RoleTypes.Crewmate)
                    );
                }

                foreach (var kvp in ForcedRoleSystem.ForcedRoles.ToList())
                {
                    byte playerId = kvp.Key;
                    RoleTypes forcedRole = kvp.Value;

                    var player = allPlayersList.FirstOrDefault(p => p.PlayerId == playerId);
                    if (player == null)
                    {
                        continue;
                    }
                    if (localIsGM && localPlayer != null && player.PlayerId == localPlayer.PlayerId)
                    {
                        continue;
                    }
                    if (!exactAssignedPlayers.Contains(player.PlayerId))
                        continue;

                    if (hasRealJester && Jester.IsJester(player))
                        continue;

                    ForcedRoleHelpers.ApplyExactRole(player, forcedRole);
                }

                ForcedRoleHelpers.ApplyFourImpSpecialFill(
                    allPlayersList,
                    exactAssignedPlayers,
                    fourImpSpecialPool,
                    rng
                );

                if (hasRealJester)
                {
                    BanMod.forcedImpostorIds.Remove(Jester.JesterId);
                    ForcedRoleSystem.ClearForcedRole(Jester.JesterId);

                    var jesterPlayer = allPlayersList.FirstOrDefault(p => p.PlayerId == Jester.JesterId);
                    if (jesterPlayer != null && jesterPlayer.Data != null)
                    {
                        ForcedRoleHelpers.ApplyExactRole(jesterPlayer, Jester.GetAssignedRole());
                    }
                }

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data != null)
                    {
                        if (pc.Data.Role == null)
                        {
                            RoleManager.Instance.SetRole(pc, RoleTypes.Crewmate);
                        }
                        pc.Data.Role?.Initialize(pc);
                    }
                }

                ForcedRoleSystem.MarkSelfRoleAssignmentForEndGame();
                return false;
            }
            finally
            {
                if (roleOptionBackup != null && roleOptionBackup.Count > 0)
                {
                    ForcedRoleHelpers.RestoreRoleOptions(roleOptionBackup);
                }
            }
        }
        private static readonly List<byte> HnsSeekers = new();

        private static void PrepareHnsSeekers()
        {
            HnsSeekers.Clear();

            if (GameData.Instance == null ||
                AmongUsClient.Instance == null)
                return;

            var available = GameData.Instance.AllPlayers
                .ToArray()
                .Where(p => p != null && !p.Disconnected)
                .ToList();

            if (available.Count == 0)
                return;

            int count = Options.NumSeekers != null
                ? Options.NumSeekers.GetInt()
                : 1;

            count = Math.Max(1, Math.Min(count, available.Count));

            for (int i = 0; i < count; i++)
            {
                string choice = "Round-robin";

                if (Options.SeekerSelections != null &&
                    i < Options.SeekerSelections.Count &&
                    Options.SeekerSelections[i] != null)
                {
                    try
                    {
                        choice = Options.SeekerSelections[i].GetString();
                    }
                    catch
                    {
                        choice = "Round-robin";
                    }
                }

                NetworkedPlayerInfo picked = null;

                if (!string.IsNullOrWhiteSpace(choice) &&
                    !string.Equals(
                        choice,
                        "Round-robin",
                        StringComparison.OrdinalIgnoreCase))
                {
                    picked = available.FirstOrDefault(p =>
                        string.Equals(
                            p.PlayerName,
                            choice,
                            StringComparison.Ordinal));
                }

                if (picked == null)
                {
                    var pseudo =
                        new PseudoRandomList<NetworkedPlayerInfo>(
                            AmongUsClient.Instance.GameId);

                    foreach (var player in available)
                        pseudo.Add(player);

                    int skips =
                        GameData.RoundsPlayedInSession + i;

                    for (int x = 0; x < skips; x++)
                        pseudo.PickRandom();

                    picked = pseudo.PickRandom();
                }

                if (picked == null)
                    continue;

                HnsSeekers.Add(picked.PlayerId);

                available.Remove(picked);
            }
        }
    }
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static class ForcedRole_EndGameRestoreGMPatch
    {
        public static void Postfix()
        {
            ForcedRoleSystem.FinishSetRoleGame();
        }
    }
}
