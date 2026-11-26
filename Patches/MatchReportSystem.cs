//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using BepInEx;
//using BepInEx.Unity.IL2CPP;
//using HarmonyLib;
//using Il2CppInterop.Runtime.Injection;
//using TMPro;
//using UnityEngine;

//namespace BanMod;

//internal static class MatchReportUiStyles
//{
//    private static GUIStyle _blackWindow;
//    private static GUIStyle _titleLabel;
//    private static GUIStyle _tintableButton;
//    private static GUIStyle _dangerButton;
//    private static Texture2D _windowTexture;
//    private static Texture2D _buttonTexture;
//    private static Texture2D _dangerTexture;

//    public static GUIStyle BlackWindow
//    {
//        get
//        {
//            if (_blackWindow == null)
//            {
//                _blackWindow = new GUIStyle(GUI.skin.window);
//                _blackWindow.padding = new RectOffset
//                {
//                    left = 14,
//                    right = 14,
//                    top = 14,
//                    bottom = 14
//                };
//                _windowTexture = MakeTexture(new Color(0.055f, 0.055f, 0.065f, 0.98f));
//                _blackWindow.normal.background = _windowTexture;
//                _blackWindow.hover.background = _windowTexture;
//                _blackWindow.active.background = _windowTexture;
//                _blackWindow.focused.background = _windowTexture;
//                _blackWindow.onNormal.background = _windowTexture;
//                _blackWindow.onHover.background = _windowTexture;
//                _blackWindow.onActive.background = _windowTexture;
//                _blackWindow.onFocused.background = _windowTexture;
//            }
//            return _blackWindow;
//        }
//    }

//    public static GUIStyle TitleLabel
//    {
//        get
//        {
//            if (_titleLabel == null)
//            {
//                _titleLabel = new GUIStyle(GUI.skin.label)
//                {
//                    alignment = TextAnchor.MiddleCenter,
//                    fontStyle = FontStyle.Bold
//                };
//                _titleLabel.normal.textColor = Color.white;
//            }
//            return _titleLabel;
//        }
//    }

//    public static GUIStyle TintableButton
//    {
//        get
//        {
//            if (_tintableButton == null)
//            {
//                _tintableButton = new GUIStyle(GUI.skin.button);
//                _tintableButton.normal.textColor = Color.white;
//                _tintableButton.hover.textColor = Color.white;
//                _tintableButton.active.textColor = Color.white;
//                _buttonTexture = MakeTexture(new Color(0.22f, 0.22f, 0.25f, 1f));
//                _tintableButton.normal.background = _buttonTexture;
//                _tintableButton.hover.background = _buttonTexture;
//                _tintableButton.active.background = _buttonTexture;
//                _tintableButton.focused.background = _buttonTexture;
//                _tintableButton.onNormal.background = _buttonTexture;
//                _tintableButton.onHover.background = _buttonTexture;
//                _tintableButton.onActive.background = _buttonTexture;
//                _tintableButton.onFocused.background = _buttonTexture;
//            }
//            return _tintableButton;
//        }
//    }

//    public static GUIStyle DangerButton
//    {
//        get
//        {
//            if (_dangerButton == null)
//            {
//                _dangerButton = new GUIStyle(GUI.skin.button);
//                _dangerButton.normal.textColor = Color.white;
//                _dangerButton.hover.textColor = Color.white;
//                _dangerButton.active.textColor = Color.white;
//                _dangerTexture = MakeTexture(new Color(0.62f, 0.12f, 0.12f, 1f));
//                _dangerButton.normal.background = _dangerTexture;
//                _dangerButton.hover.background = _dangerTexture;
//                _dangerButton.active.background = _dangerTexture;
//                _dangerButton.focused.background = _dangerTexture;
//                _dangerButton.onNormal.background = _dangerTexture;
//                _dangerButton.onHover.background = _dangerTexture;
//                _dangerButton.onActive.background = _dangerTexture;
//                _dangerButton.onFocused.background = _dangerTexture;
//            }
//            return _dangerButton;
//        }
//    }

//    private static Texture2D MakeTexture(Color color)
//    {
//        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
//        texture.hideFlags = HideFlags.HideAndDontSave;
//        texture.SetPixel(0, 0, color);
//        texture.Apply(false, true);
//        return texture;
//    }
//}
//public static class MapEvidenceRecorder
//{
//    private const int TargetWidth = 1100;
//    private const int MinHeight = 450;
//    private const int MaxHeight = 850;
//    private const int FooterPixelHeight = 180;
//    private const int FooterLayer = 30;

//    private static readonly BepInEx.Logging.ManualLogSource Log =
//        BepInEx.Logging.Logger.CreateLogSource("MatchReport");

//    private static readonly Dictionary<string, int> EventCounters =
//        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

//    private static readonly Dictionary<byte, byte> CurrentShapeshiftTarget =
//        new Dictionary<byte, byte>();

//    private static Bounds _mapBounds;
//    private static bool _hasMapBounds;
//    private static bool _matchActive;
//    private static DateTime _matchStartedUtc;
//    private static int _sequence;

//    private static readonly Queue<EventSnapshot> PendingEvents =
//        new Queue<EventSnapshot>();

//    private static bool _captureInProgress;
//    private static bool _completeRequested;
//    private static float _nextCaptureAt;

//    private static RenderTexture _captureRenderTexture;
//    private static Texture2D _captureTexture;
//    private static int _captureWidth;
//    private static int _captureHeight;

//    private static string _currentSessionDir;
//    private static byte[] _sessionKey;
//    private static bool? _recordingEnabled;
//    private const string RecordingEnabledPrefKey = "MatchReport.RecordingEnabled";

//    private static string EvidenceRoot =>
//    Path.Combine("./DATA/LOG/", "BanMod Match Png");

//    private static string CurrentRoot =>
//        Path.Combine(EvidenceRoot, "_CURRENT");

//    public static string CompletedRoot =>
//        EvidenceRoot;

//    public static bool IsMatchActive => _matchActive;

//    public static bool IsAllowedGameMode()
//    {
//        return !Options.GameMode.GetValue(GameModeType.SnS)
//            && !Options.GameMode.GetValue(GameModeType.TaskRun)
//            && !Options.GameMode.GetValue(GameModeType.FFA)
//            && !Options.GameMode.GetValue(GameModeType.FFATeam)
//            && !Options.GameMode.GetValue(GameModeType.ZombieMode)
//            && !Options.GameMode.GetValue(GameModeType.PnS)
//            && !Options.GameMode.GetValue(GameModeType.HotPotatoModded)
//            && !Options.GameMode.GetValue(GameModeType.RoomRush)
//            && !Options.GameMode.GetValue(GameModeType.TargetRush)
//            && !Options.GameMode.GetValue(GameModeType.Assassin)
//            && !Options.GameMode.GetValue(GameModeType.KillRace)
//            && !Options.GameMode.GetValue(GameModeType.DeathRun);
//    }

//    public static bool RecordingEnabled
//    {
//        get
//        {
//            if (!_recordingEnabled.HasValue)
//                _recordingEnabled = PlayerPrefs.GetInt(RecordingEnabledPrefKey, 1) != 0;

//            return _recordingEnabled.Value;
//        }
//    }

//    public static void SetRecordingEnabled(bool enabled)
//    {
//        _recordingEnabled = enabled;
//        PlayerPrefs.SetInt(RecordingEnabledPrefKey, enabled ? 1 : 0);
//        PlayerPrefs.Save();

//        if (!enabled && _matchActive)
//            CleanupActiveSession(deleteWorkingFiles: true);
//    }

//    public static void BeginMatch()
//    {
//        if (!RecordingEnabled)
//            return;

//        if (!IsAllowedGameMode())
//            return;

//        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
//            return;

//        try
//        {
//            CleanupActiveSession(deleteWorkingFiles: true);

//            Directory.CreateDirectory(CurrentRoot);
//            Directory.CreateDirectory(CompletedRoot);
//            PruneOldCompletedMatches();

//            foreach (string dir in Directory.GetDirectories(CurrentRoot))
//            {
//                try { Directory.Delete(dir, true); }
//                catch { }
//            }

//            _currentSessionDir = Path.Combine(
//                CurrentRoot,
//                Guid.NewGuid().ToString("N"));

//            Directory.CreateDirectory(_currentSessionDir);

//            _sessionKey = RandomBytes(64);

//            _matchStartedUtc = DateTime.UtcNow;
//            _matchActive = true;
//            _sequence = 0;
//            EventCounters.Clear();
//            CurrentShapeshiftTarget.Clear();
//            _hasMapBounds = false;

//            TryCalculateMapBounds();

//            Log.LogInfo("MapEvidence: recording started.");
//        }
//        catch (Exception ex)
//        {
//            Log.LogError(ex);
//            CleanupActiveSession(deleteWorkingFiles: true);
//        }
//    }

//    public static void Capture(
//        string eventType,
//        PlayerControl actor = null,
//        PlayerControl target = null)
//    {
//        if (!RecordingEnabled)
//            return;

//        if (!IsAllowedGameMode())
//            return;

//        if (!_matchActive)
//            return;

//        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
//            return;

//        if (ShipStatus.Instance == null)
//            return;

//        if (string.IsNullOrEmpty(_currentSessionDir) ||
//            !Directory.Exists(_currentSessionDir))
//            return;

//        try
//        {
//            if (!_hasMapBounds)
//                TryCalculateMapBounds();

//            if (!_hasMapBounds)
//                return;

//            var ev = SnapshotEvent(eventType, actor, target);

//            if (actor != null)
//            {
//                if (EqualsEvent(eventType, "SHAPESHIFT") && target != null)
//                {
//                    ev.ExtraId = target.PlayerId;
//                    ev.ExtraName = target.Data != null
//                        ? SanitizeText(target.Data.PlayerName)
//                        : "UNKNOWN";

//                    CurrentShapeshiftTarget[actor.PlayerId] = target.PlayerId;
//                }
//                else if (EqualsEvent(eventType, "SHAPESHIFT_REVERT"))
//                {
//                    byte oldTargetId;
//                    if (CurrentShapeshiftTarget.TryGetValue(actor.PlayerId, out oldTargetId))
//                    {
//                        ev.ExtraId = oldTargetId;
//                        ev.ExtraName = GetPlayerNameById(oldTargetId);
//                        CurrentShapeshiftTarget.Remove(actor.PlayerId);
//                    }
//                }
//                else if (EqualsEvent(eventType, "KILL"))
//                {
//                    byte formId;
//                    if (CurrentShapeshiftTarget.TryGetValue(actor.PlayerId, out formId))
//                    {
//                        ev.ExtraId = formId;
//                        ev.ExtraName = GetPlayerNameById(formId);
//                    }
//                }
//            }

//            int typeNumber = NextTypeNumber(eventType);
//            _sequence++;

//            ev.Sequence = _sequence;
//            ev.TypeNumber = typeNumber;
//            ev.FileBase = BuildEventFileBase(ev);

//            PendingEvents.Enqueue(ev);

//            if (_nextCaptureAt < Time.unscaledTime)
//                _nextCaptureAt = Time.unscaledTime + 0.08f;
//        }
//        catch (Exception ex)
//        {
//            Log.LogError("MapEvidence Capture failed: " + ex);
//        }
//    }

//    public static void CompleteMatch()
//    {
//        if (!_matchActive && !_completeRequested)
//            return;

//        _matchActive = false;
//        _completeRequested = true;

//        if (!_captureInProgress && PendingEvents.Count == 0)
//            FinalizeCompletedMatch();
//    }

//    public static void Tick()
//    {
//        if (_captureInProgress)
//            return;

//        if (PendingEvents.Count > 0 &&
//            Time.unscaledTime >= _nextCaptureAt &&
//            !string.IsNullOrEmpty(_currentSessionDir) &&
//            Directory.Exists(_currentSessionDir))
//        {
//            EventSnapshot ev = PendingEvents.Dequeue();
//            _captureInProgress = true;

//            byte[] pngBytes = null;
//            byte[] payload = null;
//            byte[] encrypted = null;

//            try
//            {
//                if (_sessionKey == null || _sessionKey.Length != 64)
//                    throw new InvalidOperationException("Match report session key is unavailable.");

//                pngBytes = RenderEventPng(ev);

//                if (pngBytes != null && pngBytes.Length > 0)
//                {
//                    payload = SerializePayload(ev, pngBytes);
//                    encrypted = Encrypt(payload, _sessionKey);

//                    string stagedPath = Path.Combine(
//                        _currentSessionDir,
//                        $"EVENT_{ev.Sequence:0000}.bmr");

//                    File.WriteAllBytes(stagedPath, encrypted);
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.LogError("MapEvidence processing failed: " + ex);
//            }
//            finally
//            {
//                ClearBytes(encrypted);
//                ClearBytes(payload);
//                ClearBytes(pngBytes);

//                _captureInProgress = false;
//                _nextCaptureAt = Time.unscaledTime + 0.06f;
//            }
//        }

//        if (_completeRequested &&
//            !_captureInProgress &&
//            PendingEvents.Count == 0)
//        {
//            FinalizeCompletedMatch();
//        }
//    }

//    private static void FinalizeCompletedMatch()
//    {
//        if (!_completeRequested)
//            return;

//        _completeRequested = false;

//        try
//        {
//            if (string.IsNullOrEmpty(_currentSessionDir) ||
//                !Directory.Exists(_currentSessionDir))
//            {
//                CleanupActiveSession(deleteWorkingFiles: true);
//                return;
//            }

//            if (_sessionKey == null || _sessionKey.Length != 64)
//            {
//                CleanupActiveSession(deleteWorkingFiles: true);
//                return;
//            }

//            string[] stagedFiles = Directory
//                .GetFiles(_currentSessionDir, "*.bmr", SearchOption.TopDirectoryOnly)
//                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
//                .ToArray();

//            if (stagedFiles.Length == 0)
//            {
//                CleanupActiveSession(deleteWorkingFiles: true);
//                return;
//            }

//            string sessionName = Path.GetFileName(_currentSessionDir);
//            string shortId = sessionName.Length >= 8
//                ? sessionName.Substring(0, 8)
//                : sessionName;

//            string completedDir = Path.Combine(
//                CompletedRoot,
//                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + shortId);

//            Directory.CreateDirectory(completedDir);

//            var index = new StringBuilder();
//            index.AppendLine("MATCHREPORT Evidence");
//            index.AppendLine("Completed match.");
//            index.AppendLine();

//            int exported = 0;

//            foreach (string stagedFile in stagedFiles)
//            {
//                byte[] encrypted = null;
//                byte[] payload = null;
//                EvidencePayload data = null;

//                try
//                {
//                    encrypted = File.ReadAllBytes(stagedFile);
//                    payload = Decrypt(encrypted, _sessionKey);
//                    data = DeserializePayload(payload);

//                    if (data == null ||
//                        data.PngBytes == null ||
//                        data.PngBytes.Length == 0)
//                        continue;

//                    string safeBase = SanitizeFileName(data.FileBase);
//                    string pngName = safeBase + ".png";
//                    string txtName = safeBase + ".txt";

//                    File.WriteAllBytes(
//                        Path.Combine(completedDir, pngName),
//                        data.PngBytes);

//                    File.WriteAllText(
//                        Path.Combine(completedDir, txtName),
//                        data.TextEvidence ?? string.Empty,
//                        Encoding.UTF8);

//                    index.AppendLine(pngName);
//                    exported++;
//                }
//                finally
//                {
//                    if (data != null)
//                        ClearBytes(data.PngBytes);

//                    ClearBytes(payload);
//                    ClearBytes(encrypted);
//                }
//            }

//            if (exported == 0)
//            {
//                try
//                {
//                    Directory.Delete(completedDir, true);
//                }
//                catch
//                {
//                }

//                CleanupActiveSession(deleteWorkingFiles: true);
//                return;
//            }

//            File.WriteAllText(
//                Path.Combine(completedDir, "INDEX.txt"),
//                index.ToString(),
//                Encoding.UTF8);

//            try
//            {
//                Directory.Delete(_currentSessionDir, true);
//            }
//            catch
//            {
//            }

//            PruneOldCompletedMatches();
//        }
//        catch (Exception ex)
//        {
//            Log.LogError(ex);
//        }
//        finally
//        {
//            CleanupActiveSession(deleteWorkingFiles: false);
//        }
//    }

//    public static void PruneOldCompletedMatches()
//    {
//        try
//        {
//            if (!Directory.Exists(CompletedRoot))
//                return;

//            DateTime oldestAllowedDate = DateTime.Today.AddDays(-1);

//            foreach (string dir in Directory.GetDirectories(CompletedRoot))
//            {
//                string name = Path.GetFileName(dir);

//                if (string.IsNullOrEmpty(name) || name.StartsWith("_", StringComparison.Ordinal))
//                    continue;

//                if (name.Length < 10)
//                    continue;

//                DateTime folderDate;
//                if (!DateTime.TryParseExact(
//                        name.Substring(0, 10),
//                        "yyyy-MM-dd",
//                        System.Globalization.CultureInfo.InvariantCulture,
//                        System.Globalization.DateTimeStyles.None,
//                        out folderDate))
//                    continue;

//                if (folderDate.Date < oldestAllowedDate)
//                {
//                    try { Directory.Delete(dir, true); }
//                    catch { }
//                }
//            }
//        }
//        catch { }
//    }

//    private static EventSnapshot SnapshotEvent(
//        string eventType,
//        PlayerControl actor,
//        PlayerControl target)
//    {
//        var ev = new EventSnapshot
//        {
//            EventType = eventType ?? "EVENT",
//            MatchSeconds = (float)(DateTime.UtcNow - _matchStartedUtc).TotalSeconds,
//            ActorId = actor != null ? actor.PlayerId : byte.MaxValue,
//            TargetId = target != null ? target.PlayerId : byte.MaxValue,
//            ActorName = actor != null && actor.Data != null
//                ? SanitizeText(actor.Data.PlayerName)
//                : "NONE",
//            TargetName = target != null && target.Data != null
//                ? SanitizeText(target.Data.PlayerName)
//                : "NONE"
//        };

//        foreach (var pc in PlayerControl.AllPlayerControls)
//        {
//            if (pc == null || pc.Data == null)
//                continue;

//            byte colorId = 0;
//            try { colorId = (byte)pc.Data.DefaultOutfit.ColorId; }
//            catch { }

//            Vector2 pos = ResolveSnapshotPosition(pc);

//            ev.Players.Add(new PlayerSnapshot
//            {
//                PlayerId = pc.PlayerId,
//                Name = SanitizeText(pc.Data.PlayerName),
//                X = pos.x,
//                Y = pos.y,
//                Dead = pc.Data.IsDead,
//                Disconnected = pc.Data.Disconnected,
//                InVent = pc.inVent,
//                ColorId = colorId
//            });
//        }

//        return ev;
//    }

//    private static Vector2 ResolveSnapshotPosition(PlayerControl pc)
//    {
//        if (pc == null)
//            return Vector2.zero;

//        Vector2 fallback = pc.transform.position;

//        if (pc.Data != null && pc.Data.IsDead)
//        {
//            DeadBody body = FindDeadBody(pc.PlayerId);
//            if (body != null)
//                return body.transform.position;
//        }

//        if (pc.inVent)
//        {
//            Vent vent = FindNearestVent(fallback);
//            if (vent != null)
//                return vent.transform.position;
//        }

//        return fallback;
//    }

//    private static DeadBody FindDeadBody(byte playerId)
//    {
//        try
//        {
//            DeadBody[] bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
//            foreach (DeadBody body in bodies)
//            {
//                if (body != null && body.ParentId == playerId)
//                    return body;
//            }
//        }
//        catch { }

//        return null;
//    }

//    private static Vent FindNearestVent(Vector2 position)
//    {
//        try
//        {
//            Vent[] vents = UnityEngine.Object.FindObjectsOfType<Vent>();
//            Vent best = null;
//            float bestDistance = float.MaxValue;

//            foreach (Vent vent in vents)
//            {
//                if (vent == null)
//                    continue;

//                Vector2 ventPosition = vent.transform.position;
//                float distance = (ventPosition - position).sqrMagnitude;

//                if (distance < bestDistance)
//                {
//                    bestDistance = distance;
//                    best = vent;
//                }
//            }

//            return best;
//        }
//        catch
//        {
//            return null;
//        }
//    }

//    private static string GetPlayerNameById(byte playerId)
//    {
//        foreach (var pc in PlayerControl.AllPlayerControls)
//        {
//            if (pc == null || pc.Data == null)
//                continue;

//            if (pc.PlayerId == playerId)
//                return SanitizeText(pc.Data.PlayerName);
//        }

//        return "UNKNOWN";
//    }

//    private static int NextTypeNumber(string eventType)
//    {
//        int n;
//        if (!EventCounters.TryGetValue(eventType, out n))
//            n = 0;

//        n++;
//        EventCounters[eventType] = n;
//        return n;
//    }

//    private static string BuildEventFileBase(EventSnapshot ev)
//    {
//        if (EqualsEvent(ev.EventType, "KILL"))
//        {
//            if (ev.ExtraId != byte.MaxValue)
//                return $"KILL_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_AS_{SanitizeFileName(ev.ExtraName)}_KILLED_{SanitizeFileName(ev.TargetName)}";

//            return $"KILL_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_KILLED_{SanitizeFileName(ev.TargetName)}";
//        }

//        if (EqualsEvent(ev.EventType, "SHAPESHIFT"))
//            return $"SHAPESHIFT_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_TO_{SanitizeFileName(ev.TargetName)}";

//        if (EqualsEvent(ev.EventType, "SHAPESHIFT_REVERT"))
//            return $"SHAPESHIFT_REVERT_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_FROM_{SanitizeFileName(ev.ExtraName)}";

//        if (EqualsEvent(ev.EventType, "VANISH"))
//            return $"VANISH_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}";

//        if (EqualsEvent(ev.EventType, "APPEAR"))
//            return $"REVERT_VANISH_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}";

//        return $"{SanitizeFileName(ev.EventType).ToUpperInvariant()}_{ev.TypeNumber:000}";
//    }

//    private static void TryCalculateMapBounds()
//    {
//        var ship = ShipStatus.Instance;
//        if (ship == null)
//            return;

//        Renderer[] renderers = ship.GetComponentsInChildren<Renderer>(true);
//        bool hasBounds = false;
//        Bounds bounds = default;

//        foreach (var renderer in renderers)
//        {
//            if (renderer == null || !renderer.enabled)
//                continue;

//            if (IsDarknessOrOverlayRenderer(renderer))
//                continue;

//            Bounds b = renderer.bounds;
//            if (b.size.x <= 0.001f || b.size.y <= 0.001f)
//                continue;

//            if (!hasBounds)
//            {
//                bounds = b;
//                hasBounds = true;
//            }
//            else
//            {
//                bounds.Encapsulate(b);
//            }
//        }

//        if (!hasBounds)
//            return;

//        bounds.Expand(new Vector3(1.2f, 1.2f, 0f));

//        _mapBounds = bounds;
//        _hasMapBounds = true;
//    }

//    private static byte[] RenderEventPng(EventSnapshot ev)
//    {
//        GameObject mapCameraObject = null;
//        GameObject footerCameraObject = null;
//        GameObject footerRoot = null;

//        RenderTexture previous = RenderTexture.active;
//        List<RendererState> disabled = null;
//        List<BehaviourState> disabledVisionBehaviours = null;
//        List<RendererState> hiddenPlayers = null;

//        try
//        {
//            int width;
//            int mapHeight;
//            CalculateOutputSize(out width, out mapHeight);

//            int totalHeight = mapHeight + FooterPixelHeight;

//            EnsureCaptureBuffers(width, totalHeight);

//            RenderTexture rt = _captureRenderTexture;
//            Texture2D finalImage = _captureTexture;

//            RenderTexture.active = rt;
//            GL.Clear(true, true, Color.black);
//            RenderTexture.active = previous;

//            mapCameraObject = new GameObject("MatchReport_EvidenceCamera");
//            mapCameraObject.hideFlags = HideFlags.HideAndDontSave;

//            Camera mapCamera = mapCameraObject.AddComponent<Camera>();
//            mapCamera.orthographic = true;
//            mapCamera.clearFlags = CameraClearFlags.SolidColor;
//            mapCamera.backgroundColor = Color.black;
//            mapCamera.nearClipPlane = 0.01f;
//            mapCamera.farClipPlane = 300f;
//            mapCamera.rect = new Rect(
//                0f,
//                FooterPixelHeight / (float)totalHeight,
//                1f,
//                mapHeight / (float)totalHeight);
//            mapCamera.aspect = width / (float)mapHeight;
//            mapCamera.transform.position =
//                new Vector3(_mapBounds.center.x, _mapBounds.center.y, -50f);
//            mapCamera.transform.rotation = Quaternion.identity;

//            float byHeight = _mapBounds.size.y * 0.5f;
//            float byWidth = (_mapBounds.size.x * 0.5f) / mapCamera.aspect;
//            mapCamera.orthographicSize = Mathf.Max(byHeight, byWidth);

//            if (Camera.main != null)
//                mapCamera.cullingMask =
//                    Camera.main.cullingMask & ~(1 << FooterLayer);

//            mapCamera.targetTexture = rt;

//            disabled = DisableDarknessRenderers();
//            disabledVisionBehaviours = DisableVisionBehaviours();
//            hiddenPlayers = DisablePlayerAndBodyRenderers();

//            try
//            {
//                mapCamera.Render();
//            }
//            finally
//            {
//                RestoreRenderers(hiddenPlayers);
//                RestoreBehaviours(disabledVisionBehaviours);
//                RestoreRenderers(disabled);

//                hiddenPlayers = null;
//                disabledVisionBehaviours = null;
//                disabled = null;
//            }

//            footerRoot = new GameObject("MatchReport_FooterRoot");
//            footerRoot.hideFlags = HideFlags.HideAndDontSave;
//            SetLayerRecursively(footerRoot, FooterLayer);

//            footerCameraObject = new GameObject("MatchReport_FooterCamera");
//            footerCameraObject.hideFlags = HideFlags.HideAndDontSave;
//            footerCameraObject.layer = FooterLayer;

//            Camera footerCamera = footerCameraObject.AddComponent<Camera>();
//            footerCamera.orthographic = true;
//            footerCamera.clearFlags = CameraClearFlags.SolidColor;
//            footerCamera.backgroundColor = Color.black;
//            footerCamera.nearClipPlane = 0.01f;
//            footerCamera.farClipPlane = 100f;
//            footerCamera.rect = new Rect(
//                0f,
//                0f,
//                1f,
//                FooterPixelHeight / (float)totalHeight);
//            footerCamera.aspect = width / (float)FooterPixelHeight;
//            footerCamera.orthographicSize = 5f;
//            footerCamera.transform.position = new Vector3(0f, 0f, -20f);
//            footerCamera.transform.rotation = Quaternion.identity;
//            footerCamera.cullingMask = 1 << FooterLayer;
//            footerCamera.targetTexture = rt;

//            float halfWidth = footerCamera.orthographicSize * footerCamera.aspect;
//            float left = -halfWidth + 0.85f;
//            float textWidth = halfWidth * 2f - 1.7f;

//            CreateFooterText(
//                footerRoot.transform,
//                new Vector3(left, 4.20f, 0f),
//                BuildEventDescription(ev),
//                6.4f,
//                true,
//                textWidth,
//                2.5f);

//            CreateFooterText(
//                footerRoot.transform,
//                new Vector3(left, 1.55f, 0f),
//                "●  Alive player     ■  Dead player     ▲  Player in vent",
//                6.4f,
//                true,
//                textWidth,
//                1.8f);

//            ForceUpdateAllText(footerRoot);
//            footerCamera.Render();

//            RenderTexture.active = rt;

//            finalImage.ReadPixels(
//                new Rect(0, 0, width, totalHeight),
//                0,
//                0,
//                false);

//            finalImage.Apply(false, false);

//            DrawAllMarkers(finalImage, ev);

//            return finalImage.EncodeToPNG();
//        }
//        finally
//        {
//            if (hiddenPlayers != null)
//                RestoreRenderers(hiddenPlayers);

//            if (disabledVisionBehaviours != null)
//                RestoreBehaviours(disabledVisionBehaviours);

//            if (disabled != null)
//                RestoreRenderers(disabled);

//            RenderTexture.active = previous;

//            if (footerCameraObject != null)
//                UnityEngine.Object.Destroy(footerCameraObject);

//            if (footerRoot != null)
//                UnityEngine.Object.Destroy(footerRoot);

//            if (mapCameraObject != null)
//                UnityEngine.Object.Destroy(mapCameraObject);
//        }
//    }

//    private static void EnsureCaptureBuffers(int width, int height)
//    {
//        if (_captureRenderTexture != null &&
//            _captureTexture != null &&
//            _captureWidth == width &&
//            _captureHeight == height)
//        {
//            return;
//        }

//        ReleaseCaptureBuffers();

//        _captureRenderTexture = new RenderTexture(
//            width,
//            height,
//            24,
//            RenderTextureFormat.ARGB32);

//        _captureRenderTexture.hideFlags = HideFlags.HideAndDontSave;
//        _captureRenderTexture.Create();

//        _captureTexture = new Texture2D(
//            width,
//            height,
//            TextureFormat.RGB24,
//            false);

//        _captureTexture.hideFlags = HideFlags.HideAndDontSave;

//        _captureWidth = width;
//        _captureHeight = height;
//    }

//    private static void ReleaseCaptureBuffers()
//    {
//        if (_captureRenderTexture != null)
//        {
//            _captureRenderTexture.Release();
//            UnityEngine.Object.Destroy(_captureRenderTexture);
//            _captureRenderTexture = null;
//        }

//        if (_captureTexture != null)
//        {
//            UnityEngine.Object.Destroy(_captureTexture);
//            _captureTexture = null;
//        }

//        _captureWidth = 0;
//        _captureHeight = 0;
//    }

//    private static void CalculateOutputSize(out int width, out int height)
//    {
//        float ratio = _mapBounds.size.x / Mathf.Max(0.001f, _mapBounds.size.y);
//        width = TargetWidth;
//        height = Mathf.RoundToInt(width / Mathf.Max(0.1f, ratio));
//        height = Mathf.Clamp(height, MinHeight, MaxHeight);
//    }

//    private static void DrawAllMarkers(Texture2D image, EventSnapshot ev)
//    {
//        if (image == null || ev == null)
//            return;

//        int mapHeight = image.height - FooterPixelHeight;

//        foreach (PlayerSnapshot p in ev.Players)
//        {
//            int px;
//            int py;

//            WorldToMapPixel(
//                p.X,
//                p.Y,
//                image.width,
//                mapHeight,
//                out px,
//                out py);

//            py += FooterPixelHeight;

//            Color32 fill = GetMarkerPixelColor(p);
//            Color32 outline = new Color32(0, 0, 0, 255);

//            if (p.InVent)
//            {
//                DrawFilledTriangle(image, px, py, 16, outline);
//                DrawFilledTriangle(image, px, py, 11, fill);
//            }
//            else if (p.Dead)
//            {
//                DrawFilledSquare(image, px, py, 15, outline);
//                DrawFilledSquare(image, px, py, 10, fill);
//            }
//            else
//            {
//                DrawFilledCircle(image, px, py, 16, outline);
//                DrawFilledCircle(image, px, py, 11, fill);
//            }
//        }

//        image.Apply(false, false);
//    }

//    private static Color32 GetMarkerPixelColor(PlayerSnapshot p)
//    {
//        Color32 baseColor = GetPlayerColor(p.ColorId);

//        if (!p.Dead)
//            return baseColor;

//        byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(baseColor.r * 0.48f + 105f), 0, 255);
//        byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(baseColor.g * 0.48f + 105f), 0, 255);
//        byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(baseColor.b * 0.48f + 105f), 0, 255);
//        return new Color32(r, g, b, 255);
//    }

//    private static void WorldToMapPixel(float x, float y, int width, int height, out int px, out int py)
//    {
//        float nx = Mathf.InverseLerp(_mapBounds.min.x, _mapBounds.max.x, x);
//        float ny = Mathf.InverseLerp(_mapBounds.min.y, _mapBounds.max.y, y);
//        px = Mathf.Clamp(Mathf.RoundToInt(nx * (width - 1)), 0, width - 1);
//        py = Mathf.Clamp(Mathf.RoundToInt(ny * (height - 1)), 0, height - 1);
//    }

//    private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int radius, Color32 color)
//    {
//        int r2 = radius * radius;
//        for (int y = -radius; y <= radius; y++)
//        {
//            for (int x = -radius; x <= radius; x++)
//            {
//                if (x * x + y * y <= r2)
//                    SetPixelSafe(tex, cx + x, cy + y, color);
//            }
//        }
//    }

//    private static void DrawFilledSquare(Texture2D tex, int cx, int cy, int radius, Color32 color)
//    {
//        for (int y = -radius; y <= radius; y++)
//            for (int x = -radius; x <= radius; x++)
//                SetPixelSafe(tex, cx + x, cy + y, color);
//    }

//    private static void DrawFilledTriangle(Texture2D tex, int cx, int cy, int radius, Color32 color)
//    {
//        int topY = cy + radius;
//        int bottomY = cy - radius;
//        int total = Mathf.Max(1, topY - bottomY);

//        for (int y = bottomY; y <= topY; y++)
//        {
//            float t = (topY - y) / (float)total;
//            int halfWidth = Mathf.RoundToInt(radius * t);
//            for (int x = -halfWidth; x <= halfWidth; x++)
//                SetPixelSafe(tex, cx + x, y, color);
//        }
//    }

//    private static void SetPixelSafe(Texture2D tex, int x, int y, Color32 color)
//    {
//        if ((uint)x >= (uint)tex.width || (uint)y >= (uint)tex.height)
//            return;

//        tex.SetPixel(x, y, color);
//    }

//    private static string GetMarkerGlyph(PlayerSnapshot p)
//    {
//        if (p.InVent)
//            return "▲";

//        if (p.Dead)
//            return "■";

//        return "●";
//    }

//    private static Color GetMarkerColor(PlayerSnapshot p)
//    {
//        Color c = GetPlayerColor(p.ColorId);

//        if (p.Dead)
//        {
//            c = Color.Lerp(c, Color.gray, 0.62f);
//            c.a = 0.72f;
//        }

//        return c;
//    }

//    private static void CreateExactPlayerMarker(Transform parent, PlayerSnapshot p)
//    {
//        var marker = new GameObject("EvidencePlayerMarker_" + p.PlayerId);
//        marker.hideFlags = HideFlags.HideAndDontSave;
//        marker.transform.SetParent(parent, false);
//        marker.transform.position = new Vector3(p.X, p.Y, -8f);

//        var tmp = marker.AddComponent<TextMeshPro>();
//        if (TMP_Settings.defaultFontAsset != null)
//            tmp.font = TMP_Settings.defaultFontAsset;

//        tmp.fontSize = 6.4f;
//        tmp.alignment = TextAlignmentOptions.Center;
//        tmp.enableWordWrapping = false;
//        tmp.richText = false;
//        tmp.text = GetMarkerGlyph(p);
//        tmp.color = GetMarkerColor(p);
//        tmp.outlineWidth = 0.48f;
//        tmp.outlineColor = Color.black;
//        tmp.ForceMeshUpdate(true, true);

//        var renderer = marker.GetComponent<MeshRenderer>();
//        if (renderer != null)
//            renderer.sortingOrder = 5002;
//    }

//    private static void ForceUpdateAllText(GameObject root)
//    {
//        if (root == null)
//            return;

//        foreach (TextMeshPro tmp in root.GetComponentsInChildren<TextMeshPro>(true))
//        {
//            if (tmp != null)
//                tmp.ForceMeshUpdate(true, true);
//        }
//    }

//    private static void SetLayerRecursively(GameObject go, int layer)
//    {
//        if (go == null)
//            return;

//        go.layer = layer;
//        foreach (Transform child in go.transform)
//        {
//            if (child != null)
//                SetLayerRecursively(child.gameObject, layer);
//        }
//    }

//    private static Texture2D RenderFooter(int width, int height, EventSnapshot ev)
//    {
//        GameObject cameraObject = null;
//        GameObject root = null;
//        RenderTexture rt = null;
//        Texture2D footer = null;
//        RenderTexture previous = RenderTexture.active;

//        try
//        {
//            root = new GameObject("MatchReport_FooterRoot");
//            root.hideFlags = HideFlags.HideAndDontSave;
//            SetLayerRecursively(root, FooterLayer);

//            cameraObject = new GameObject("MatchReport_FooterCamera");
//            cameraObject.hideFlags = HideFlags.HideAndDontSave;
//            cameraObject.layer = FooterLayer;

//            Camera cam = cameraObject.AddComponent<Camera>();
//            cam.orthographic = true;
//            cam.clearFlags = CameraClearFlags.SolidColor;
//            cam.backgroundColor = Color.black;
//            cam.nearClipPlane = 0.01f;
//            cam.farClipPlane = 100f;
//            cam.aspect = width / (float)height;
//            cam.orthographicSize = 5f;
//            cam.transform.position = new Vector3(0f, 0f, -20f);
//            cam.transform.rotation = Quaternion.identity;
//            cam.cullingMask = 1 << FooterLayer;

//            float halfWidth = cam.orthographicSize * cam.aspect;
//            float left = -halfWidth + 0.85f;
//            float textWidth = halfWidth * 2f - 1.7f;

//            CreateFooterText(
//                root.transform,
//                new Vector3(left, 4.15f, 0f),
//                BuildEventDescription(ev),
//                6.2f,
//                true,
//                textWidth,
//                2.4f);

//            CreateFooterText(
//                root.transform,
//                new Vector3(left, 1.65f, 0f),
//                "●  Alive player     ■  Dead player     ▲  Player in vent",
//                6.2f,
//                true,
//                textWidth,
//                1.7f);

//            ForceUpdateAllText(root);

//            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//            rt.hideFlags = HideFlags.HideAndDontSave;
//            cam.targetTexture = rt;
//            cam.Render();

//            RenderTexture.active = rt;
//            footer = new Texture2D(width, height, TextureFormat.RGB24, false);
//            footer.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
//            footer.Apply(false, false);
//            return footer;
//        }
//        finally
//        {
//            RenderTexture.active = previous;

//            if (rt != null)
//            {
//                rt.Release();
//                UnityEngine.Object.Destroy(rt);
//            }

//            if (cameraObject != null)
//                UnityEngine.Object.Destroy(cameraObject);

//            if (root != null)
//                UnityEngine.Object.Destroy(root);
//        }
//    }

//    private static void CreateFooterText(
//        Transform parent,
//        Vector3 position,
//        string value,
//        float fontSize,
//        bool bold,
//        float width,
//        float height)
//    {
//        var go = new GameObject("MatchReport_FooterText");
//        go.hideFlags = HideFlags.HideAndDontSave;
//        go.layer = FooterLayer;
//        go.transform.SetParent(parent, false);
//        go.transform.position = position;

//        var tmp = go.AddComponent<TextMeshPro>();
//        if (TMP_Settings.defaultFontAsset != null)
//            tmp.font = TMP_Settings.defaultFontAsset;

//        tmp.fontSize = fontSize;
//        tmp.alignment = TextAlignmentOptions.TopLeft;
//        tmp.enableWordWrapping = true;
//        tmp.overflowMode = TextOverflowModes.Overflow;
//        tmp.richText = true;
//        tmp.color = Color.white;
//        tmp.outlineWidth = 0.18f;
//        tmp.outlineColor = Color.black;
//        tmp.rectTransform.pivot = new Vector2(0f, 1f);
//        tmp.rectTransform.sizeDelta = new Vector2(width, height);
//        tmp.text = bold ? "<b>" + value + "</b>" : value;
//        tmp.ForceMeshUpdate(true, true);

//        var renderer = go.GetComponent<MeshRenderer>();
//        if (renderer != null)
//            renderer.sortingOrder = 6000;
//    }

//    private static Texture2D CombineMapAndFooter(Texture2D map, Texture2D footer)
//    {
//        int width = map.width;
//        int totalHeight = map.height + footer.height;

//        var result = new Texture2D(width, totalHeight, TextureFormat.RGB24, false);
//        Color32[] pixels = new Color32[width * totalHeight];
//        Color32 black = new Color32(0, 0, 0, 255);

//        for (int i = 0; i < pixels.Length; i++)
//            pixels[i] = black;

//        Color32[] footerPixels = footer.GetPixels32();
//        for (int y = 0; y < footer.height; y++)
//        {
//            int src = y * width;
//            int dst = y * width;
//            Array.Copy(footerPixels, src, pixels, dst, width);
//        }

//        Color32[] mapPixels = map.GetPixels32();
//        for (int y = 0; y < map.height; y++)
//        {
//            int src = y * width;
//            int dst = (y + footer.height) * width;
//            Array.Copy(mapPixels, src, pixels, dst, width);
//        }

//        result.SetPixels32(pixels);
//        result.Apply(false, false);
//        return result;
//    }

//    private static string BuildActionPlayer(string playerName, byte colorId)
//    {
//        Color32 color = GetPlayerColor(colorId);
//        string symbol = "<color=#" + ColorToHex(color) + ">●</color>";
//        return EscapeRichText(playerName ?? "UNKNOWN") + " (" + symbol + ")";
//    }

//    private static string BuildEventDescription(EventSnapshot ev)
//    {
//        string actor = BuildActionPlayer(ev.ActorName, FindColorId(ev, ev.ActorId));
//        string target = BuildActionPlayer(ev.TargetName, FindColorId(ev, ev.TargetId));

//        if (EqualsEvent(ev.EventType, "KILL"))
//        {
//            if (ev.ExtraId != byte.MaxValue)
//            {
//                string form = BuildActionPlayer(ev.ExtraName, FindColorId(ev, ev.ExtraId));
//                return actor + " shapeshifted to " + form + " killed " + target;
//            }

//            return actor + " killed " + target;
//        }

//        if (EqualsEvent(ev.EventType, "SHAPESHIFT"))
//            return actor + " shapeshifted to " + target;

//        if (EqualsEvent(ev.EventType, "SHAPESHIFT_REVERT"))
//        {
//            if (ev.ExtraId != byte.MaxValue)
//            {
//                string previousForm = BuildActionPlayer(ev.ExtraName, FindColorId(ev, ev.ExtraId));
//                return actor + " reverted shapeshift from " + previousForm;
//            }

//            return actor + " reverted shapeshift";
//        }

//        if (EqualsEvent(ev.EventType, "VANISH"))
//            return actor + " vanished";

//        if (EqualsEvent(ev.EventType, "APPEAR"))
//            return actor + " reverted vanish";

//        return ev.EventType;
//    }

//    private static List<RendererState> DisablePlayerAndBodyRenderers()
//    {
//        var result = new List<RendererState>();

//        try
//        {
//            foreach (var pc in PlayerControl.AllPlayerControls)
//            {
//                if (pc == null)
//                    continue;

//                foreach (Renderer renderer in pc.GetComponentsInChildren<Renderer>(true))
//                {
//                    if (renderer == null || !renderer.enabled)
//                        continue;

//                    result.Add(new RendererState { Renderer = renderer, WasEnabled = true });
//                    renderer.enabled = false;
//                }
//            }

//            DeadBody[] bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
//            foreach (DeadBody body in bodies)
//            {
//                if (body == null)
//                    continue;

//                foreach (Renderer renderer in body.GetComponentsInChildren<Renderer>(true))
//                {
//                    if (renderer == null || !renderer.enabled)
//                        continue;

//                    result.Add(new RendererState { Renderer = renderer, WasEnabled = true });
//                    renderer.enabled = false;
//                }
//            }
//        }
//        catch { }

//        return result;
//    }

//    private static List<BehaviourState> DisableVisionBehaviours()
//    {
//        var result = new List<BehaviourState>();

//        foreach (ShadowCamera behaviour in UnityEngine.Object.FindObjectsOfType<ShadowCamera>())
//            DisableBehaviour(behaviour, result);

//        foreach (ShadowCollab behaviour in UnityEngine.Object.FindObjectsOfType<ShadowCollab>())
//            DisableBehaviour(behaviour, result);

//        foreach (LightSource behaviour in UnityEngine.Object.FindObjectsOfType<LightSource>())
//            DisableBehaviour(behaviour, result);

//        foreach (OneWayShadows behaviour in UnityEngine.Object.FindObjectsOfType<OneWayShadows>())
//            DisableBehaviour(behaviour, result);

//        return result;
//    }

//    private static void DisableBehaviour(Behaviour behaviour, List<BehaviourState> result)
//    {
//        if (behaviour == null || !behaviour.enabled)
//            return;

//        result.Add(new BehaviourState
//        {
//            Behaviour = behaviour,
//            WasEnabled = behaviour.enabled
//        });

//        behaviour.enabled = false;
//    }

//    private static void RestoreBehaviours(List<BehaviourState> behaviours)
//    {
//        if (behaviours == null)
//            return;

//        foreach (BehaviourState state in behaviours)
//        {
//            if (state != null && state.Behaviour != null)
//                state.Behaviour.enabled = state.WasEnabled;
//        }
//    }

//    private static List<RendererState> DisableDarknessRenderers()
//    {
//        var result = new List<RendererState>();

//        try
//        {
//            Renderer[] allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();

//            foreach (Renderer renderer in allRenderers)
//            {
//                if (renderer == null || !renderer.enabled)
//                    continue;

//                if (!IsDarknessOrOverlayRenderer(renderer))
//                    continue;

//                result.Add(new RendererState
//                {
//                    Renderer = renderer,
//                    WasEnabled = renderer.enabled
//                });

//                renderer.enabled = false;
//            }
//        }
//        catch { }

//        return result;
//    }

//    private static bool IsDarknessOrOverlayRenderer(Renderer renderer)
//    {
//        if (renderer == null)
//            return false;

//        string objectName = renderer.name == null
//            ? string.Empty
//            : renderer.name.ToLowerInvariant();

//        if (ContainsVisionOverlayName(objectName))
//            return true;

//        try
//        {
//            Transform t = renderer.transform;
//            for (int i = 0; i < 6 && t != null; i++, t = t.parent)
//            {
//                string n = t.name == null ? string.Empty : t.name.ToLowerInvariant();
//                if (ContainsVisionOverlayName(n))
//                    return true;
//            }
//        }
//        catch { }

//        try
//        {
//            Material material = renderer.sharedMaterial;
//            if (material != null)
//            {
//                string materialName = material.name == null
//                    ? string.Empty
//                    : material.name.ToLowerInvariant();

//                if (ContainsVisionOverlayName(materialName))
//                    return true;

//                if (material.shader != null)
//                {
//                    string shaderName = material.shader.name == null
//                        ? string.Empty
//                        : material.shader.name.ToLowerInvariant();

//                    if (ContainsVisionOverlayName(shaderName))
//                        return true;
//                }
//            }
//        }
//        catch { }

//        return false;
//    }

//    private static bool ContainsVisionOverlayName(string value)
//    {
//        if (string.IsNullOrEmpty(value))
//            return false;

//        return value.Contains("shadowquad") ||
//               value.Contains("shadowmask") ||
//               value.Contains("shadowcamera") ||
//               value.Contains("shadowcollab") ||
//               value.Contains("lightsource") ||
//               value.Contains("light source") ||
//               value.Contains("lightmask") ||
//               value.Contains("light mask") ||
//               value.Contains("fov") ||
//               value.Contains("vision") ||
//               value.Contains("vignette") ||
//               value.Contains("fog") ||
//               value.Contains("onewayshadow") ||
//               value.Contains("one way shadow") ||
//               value.Contains("shadow mask") ||
//               value.Contains("shadowmask");
//    }

//    private static void RestoreRenderers(List<RendererState> renderers)
//    {
//        if (renderers == null)
//            return;

//        foreach (RendererState state in renderers)
//        {
//            if (state != null && state.Renderer != null)
//                state.Renderer.enabled = state.WasEnabled;
//        }
//    }

//    private static byte[] SerializePayload(EventSnapshot ev, byte[] png)
//    {
//        using (var ms = new MemoryStream())
//        using (var bw = new BinaryWriter(ms, Encoding.UTF8))
//        {
//            bw.Write(2);
//            bw.Write(ev.Sequence);
//            bw.Write(ev.TypeNumber);
//            bw.Write(ev.EventType ?? string.Empty);
//            bw.Write(ev.MatchSeconds);
//            bw.Write(ev.ActorName ?? string.Empty);
//            bw.Write(ev.TargetName ?? string.Empty);
//            bw.Write(ev.FileBase ?? string.Empty);
//            bw.Write(BuildTextEvidence(ev));
//            bw.Write(png.Length);
//            bw.Write(png);
//            bw.Flush();
//            return ms.ToArray();
//        }
//    }

//    private static EvidencePayload DeserializePayload(byte[] payload)
//    {
//        using (var ms = new MemoryStream(payload, false))
//        using (var br = new BinaryReader(ms, Encoding.UTF8))
//        {
//            int version = br.ReadInt32();

//            if (version != 2)
//                throw new InvalidDataException("Unsupported evidence payload version.");

//            var data = new EvidencePayload();
//            data.Sequence = br.ReadInt32();
//            data.TypeNumber = br.ReadInt32();
//            data.EventType = br.ReadString();
//            data.MatchSeconds = br.ReadSingle();
//            data.ActorName = br.ReadString();
//            data.TargetName = br.ReadString();
//            data.FileBase = br.ReadString();
//            data.TextEvidence = br.ReadString();

//            int pngLength = br.ReadInt32();

//            if (pngLength < 0 || pngLength > 64 * 1024 * 1024)
//                throw new InvalidDataException("Invalid evidence image length.");

//            data.PngBytes = br.ReadBytes(pngLength);

//            if (data.PngBytes.Length != pngLength)
//                throw new EndOfStreamException();

//            return data;
//        }
//    }

//    private static byte[] Encrypt(byte[] plain, byte[] keyMaterial)
//    {
//        byte[] aesKey = new byte[32];
//        byte[] macKey = new byte[32];
//        Buffer.BlockCopy(keyMaterial, 0, aesKey, 0, 32);
//        Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

//        byte[] iv = RandomBytes(16);
//        byte[] cipher;

//        try
//        {
//            using (Aes aes = Aes.Create())
//            {
//                aes.KeySize = 256;
//                aes.BlockSize = 128;
//                aes.Mode = CipherMode.CBC;
//                aes.Padding = PaddingMode.PKCS7;
//                aes.Key = aesKey;
//                aes.IV = iv;

//                using (ICryptoTransform enc = aes.CreateEncryptor())
//                {
//                    cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
//                }
//            }

//            byte[] authenticated = new byte[4 + iv.Length + cipher.Length];
//            authenticated[0] = (byte)'B';
//            authenticated[1] = (byte)'M';
//            authenticated[2] = (byte)'E';
//            authenticated[3] = (byte)'1';
//            Buffer.BlockCopy(iv, 0, authenticated, 4, iv.Length);
//            Buffer.BlockCopy(cipher, 0, authenticated, 4 + iv.Length, cipher.Length);

//            byte[] tag;
//            using (var hmac = new HMACSHA256(macKey))
//                tag = hmac.ComputeHash(authenticated);

//            byte[] output = new byte[authenticated.Length + tag.Length];
//            Buffer.BlockCopy(authenticated, 0, output, 0, authenticated.Length);
//            Buffer.BlockCopy(tag, 0, output, authenticated.Length, tag.Length);

//            ClearBytes(authenticated);
//            ClearBytes(tag);
//            ClearBytes(cipher);

//            return output;
//        }
//        finally
//        {
//            ClearBytes(aesKey);
//            ClearBytes(macKey);
//            ClearBytes(iv);
//        }
//    }

//    private static byte[] Decrypt(byte[] encrypted, byte[] keyMaterial)
//    {
//        if (encrypted == null || encrypted.Length < 4 + 16 + 32 + 16)
//            throw new InvalidDataException("Invalid evidence file.");

//        if (encrypted[0] != (byte)'B' ||
//            encrypted[1] != (byte)'M' ||
//            encrypted[2] != (byte)'E' ||
//            encrypted[3] != (byte)'1')
//            throw new InvalidDataException("Invalid evidence magic.");

//        byte[] aesKey = new byte[32];
//        byte[] macKey = new byte[32];
//        Buffer.BlockCopy(keyMaterial, 0, aesKey, 0, 32);
//        Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

//        int tagOffset = encrypted.Length - 32;
//        byte[] expectedTag = null;

//        try
//        {
//            using (var hmac = new HMACSHA256(macKey))
//                expectedTag = hmac.ComputeHash(encrypted, 0, tagOffset);

//            if (!ConstantTimeEquals(encrypted, tagOffset, expectedTag))
//                throw new CryptographicException("Evidence integrity check failed.");

//            byte[] iv = new byte[16];
//            Buffer.BlockCopy(encrypted, 4, iv, 0, 16);

//            int cipherOffset = 20;
//            int cipherLength = tagOffset - cipherOffset;
//            byte[] cipher = new byte[cipherLength];
//            Buffer.BlockCopy(encrypted, cipherOffset, cipher, 0, cipherLength);

//            try
//            {
//                using (Aes aes = Aes.Create())
//                {
//                    aes.KeySize = 256;
//                    aes.BlockSize = 128;
//                    aes.Mode = CipherMode.CBC;
//                    aes.Padding = PaddingMode.PKCS7;
//                    aes.Key = aesKey;
//                    aes.IV = iv;

//                    using (ICryptoTransform dec = aes.CreateDecryptor())
//                        return dec.TransformFinalBlock(cipher, 0, cipher.Length);
//                }
//            }
//            finally
//            {
//                ClearBytes(iv);
//                ClearBytes(cipher);
//            }
//        }
//        finally
//        {
//            ClearBytes(aesKey);
//            ClearBytes(macKey);
//            ClearBytes(expectedTag);
//        }
//    }

//    private static bool ConstantTimeEquals(byte[] source, int sourceOffset, byte[] expected)
//    {
//        if (source == null || expected == null ||
//            sourceOffset < 0 || sourceOffset + expected.Length > source.Length)
//            return false;

//        int diff = 0;
//        for (int i = 0; i < expected.Length; i++)
//            diff |= source[sourceOffset + i] ^ expected[i];

//        return diff == 0;
//    }

//    private static byte[] RandomBytes(int count)
//    {
//        byte[] bytes = new byte[count];
//        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
//            rng.GetBytes(bytes);
//        return bytes;
//    }

//    private static string BuildTextEvidence(EventSnapshot data)
//    {
//        var sb = new StringBuilder();
//        sb.AppendLine(BuildEventDescription(data));
//        sb.AppendLine();
//        sb.AppendLine("Legend:");
//        sb.AppendLine("● Alive player");
//        sb.AppendLine("■ Dead player");
//        sb.AppendLine("▲ Player in vent");
//        return sb.ToString();
//    }

//    private static string BuildPlainPlayerLine(PlayerSnapshot p, EventSnapshot ev)
//    {
//        string me = PlainPlayerDisplayName(p.Name, p.ColorId);

//        if (p.PlayerId == ev.ActorId)
//        {
//            if (EqualsEvent(ev.EventType, "KILL"))
//            {
//                string victim = PlainPlayerDisplayName(ev.TargetName, FindColorId(ev, ev.TargetId));
//                if (ev.ExtraId != byte.MaxValue)
//                {
//                    string form = PlainPlayerDisplayName(ev.ExtraName, FindColorId(ev, ev.ExtraId));
//                    return me + " shapeshifted to " + form + " killed " + victim;
//                }
//                return me + " killed " + victim;
//            }

//            if (EqualsEvent(ev.EventType, "SHAPESHIFT"))
//                return me + " shapeshifted to " + PlainPlayerDisplayName(ev.TargetName, FindColorId(ev, ev.TargetId));

//            if (EqualsEvent(ev.EventType, "SHAPESHIFT_REVERT"))
//            {
//                if (ev.ExtraId != byte.MaxValue)
//                    return me + " reverted shapeshift from " + PlainPlayerDisplayName(ev.ExtraName, FindColorId(ev, ev.ExtraId));
//                return me + " reverted shapeshift";
//            }

//            if (EqualsEvent(ev.EventType, "VANISH"))
//                return me + " vanished";

//            if (EqualsEvent(ev.EventType, "APPEAR"))
//                return me + " reverted vanish";
//        }

//        if (p.InVent)
//            return me + " is in vent";
//        if (p.Dead)
//            return me + " is dead";
//        if (p.Disconnected)
//            return me + " is disconnected";

//        return me;
//    }

//    private static void CleanupActiveSession(bool deleteWorkingFiles)
//    {
//        _matchActive = false;
//        _hasMapBounds = false;
//        _sequence = 0;
//        _captureInProgress = false;
//        _completeRequested = false;
//        _nextCaptureAt = 0f;

//        PendingEvents.Clear();
//        EventCounters.Clear();
//        CurrentShapeshiftTarget.Clear();

//        if (deleteWorkingFiles &&
//            !string.IsNullOrEmpty(_currentSessionDir) &&
//            Directory.Exists(_currentSessionDir))
//        {
//            try
//            {
//                Directory.Delete(_currentSessionDir, true);
//            }
//            catch
//            {
//            }
//        }

//        ClearBytes(_sessionKey);
//        _sessionKey = null;
//        _currentSessionDir = null;

//        ReleaseCaptureBuffers();
//    }

//    private static void ClearBytes(byte[] data)
//    {
//        if (data != null)
//            Array.Clear(data, 0, data.Length);
//    }

//    private static bool EqualsEvent(string a, string b)
//    {
//        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
//    }

//    private static Color32 GetPlayerColor(byte colorId)
//    {
//        try
//        {
//            if (colorId < Palette.PlayerColors.Length)
//                return Palette.PlayerColors[colorId];
//        }
//        catch { }

//        return new Color32(255, 255, 255, 255);
//    }

//    private static string GetColorName(byte colorId)
//    {
//        switch (colorId)
//        {
//            case 0: return "red";
//            case 1: return "blue";
//            case 2: return "green";
//            case 3: return "pink";
//            case 4: return "orange";
//            case 5: return "yellow";
//            case 6: return "black";
//            case 7: return "white";
//            case 8: return "purple";
//            case 9: return "brown";
//            case 10: return "cyan";
//            case 11: return "lime";
//            case 12: return "maroon";
//            case 13: return "rose";
//            case 14: return "banana";
//            case 15: return "gray";
//            case 16: return "tan";
//            case 17: return "coral";
//            default: return "unknown";
//        }
//    }

//    private static string BuildPlayerDisplayName(string playerName, byte colorId)
//    {
//        string safeName = EscapeRichText(playerName ?? "UNKNOWN");
//        return safeName + " (" + GetColorName(colorId) + ")";
//    }

//    private static string PlainPlayerDisplayName(string playerName, byte colorId)
//    {
//        return (playerName ?? "UNKNOWN") + " (" + GetColorName(colorId) + ")";
//    }

//    private static byte FindColorId(EventSnapshot ev, byte playerId)
//    {
//        if (ev == null)
//            return 255;

//        foreach (var p in ev.Players)
//        {
//            if (p.PlayerId == playerId)
//                return p.ColorId;
//        }

//        return 255;
//    }

//    private static string ColorToHex(Color32 c)
//    {
//        return $"{c.r:X2}{c.g:X2}{c.b:X2}";
//    }

//    private static string EscapeRichText(string value)
//    {
//        if (string.IsNullOrEmpty(value))
//            return string.Empty;

//        return value
//            .Replace("&", "&amp;")
//            .Replace("<", "&lt;")
//            .Replace(">", "&gt;");
//    }

//    private static string FormatDisplayTime(float seconds)
//    {
//        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
//        return $"{total / 60:00}:{total % 60:00}";
//    }

//    private static string SanitizeText(string value)
//    {
//        if (string.IsNullOrEmpty(value))
//            return string.Empty;

//        return value
//            .Replace("\t", " ")
//            .Replace("\r", " ")
//            .Replace("\n", " ");
//    }

//    private static string SanitizeFileName(string value)
//    {
//        if (string.IsNullOrEmpty(value))
//            return "UNKNOWN";

//        foreach (char c in Path.GetInvalidFileNameChars())
//            value = value.Replace(c, '_');

//        return value.Replace(' ', '_');
//    }

//    private sealed class BehaviourState
//    {
//        public Behaviour Behaviour;
//        public bool WasEnabled;
//    }

//    private sealed class RendererState
//    {
//        public Renderer Renderer;
//        public bool WasEnabled;
//    }

//    private sealed class EventSnapshot
//    {
//        public int Sequence;
//        public int TypeNumber;
//        public string EventType = string.Empty;
//        public float MatchSeconds;
//        public byte ActorId = byte.MaxValue;
//        public byte TargetId = byte.MaxValue;
//        public string ActorName = "NONE";
//        public string TargetName = "NONE";
//        public byte ExtraId = byte.MaxValue;
//        public string ExtraName = "NONE";
//        public string FileBase = string.Empty;
//        public readonly List<PlayerSnapshot> Players = new List<PlayerSnapshot>();
//    }

//    private sealed class PlayerSnapshot
//    {
//        public byte PlayerId;
//        public string Name = string.Empty;
//        public float X;
//        public float Y;
//        public bool Dead;
//        public bool Disconnected;
//        public bool InVent;
//        public byte ColorId;
//    }

//    private sealed class EvidencePayload
//    {
//        public int Sequence;
//        public int TypeNumber;
//        public string EventType;
//        public float MatchSeconds;
//        public string ActorName;
//        public string TargetName;
//        public string FileBase;
//        public string TextEvidence;
//        public byte[] PngBytes;
//    }
//}

//public class EvidenceMenu : MonoBehaviour
//{
//    public EvidenceMenu(IntPtr ptr) : base(ptr) { }

//    public static EvidenceMenu Instance;

//    private bool _showMenu;
//    private Rect _windowRect;
//    private readonly Vector2 _windowSize = new Vector2(1040f, 700f);

//    private GUIStyle _titleStyle;
//    private GUIStyle _buttonStyle;
//    private GUIStyle _selectedButtonStyle;
//    private GUIStyle _exitButtonStyle;
//    private GUIStyle _labelStyle;
//    private GUIStyle _smallLabelStyle;
//    private GUIStyle _panelStyle;
//    private GUIStyle _previewImageStyle;

//    private Vector2 _matchScroll;
//    private Vector2 _eventScroll;

//    private readonly List<string> _matches = new List<string>();
//    private readonly List<string> _events = new List<string>();

//    private int _selectedMatch = -1;
//    private int _selectedEvent = -1;
//    private Texture2D _previewTexture;
//    private string _previewPath;
//    private float _lastRefresh;
//    private bool _zoomOpen;
//    private float _zoomScale = 1f;

//    private void Awake()
//    {
//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//        CenterWindow();
//    }

//    private void OnDestroy()
//    {
//        if (Instance == this)
//            Instance = null;

//        DestroyPreview();
//    }

//    private void Update()
//    {
//        MapEvidenceRecorder.Tick();

//        if (_zoomOpen && Input.GetKeyDown(KeyCode.Escape))
//        {
//            _zoomOpen = false;
//            _zoomScale = 1f;
//        }

//        if (Input.GetKeyDown(KeyCode.F8) &&
//            IsInLobby())
//        {
//            _showMenu = !_showMenu;

//            if (_showMenu)
//            {
//                RefreshMatches(true);
//                CenterWindow();
//            }
//            else
//            {
//                DestroyPreview();
//            }
//        }

//        if (_showMenu && !IsInLobby())
//        {
//            _showMenu = false;
//            DestroyPreview();
//        }

//        if (_showMenu && Time.unscaledTime - _lastRefresh > 2f)
//            RefreshMatches(false);
//    }

//    private static bool IsInLobby()
//    {
//        return ShipStatus.Instance == null &&
//               !MapEvidenceRecorder.IsMatchActive &&
//               (LobbyBehaviour.Instance != null || AmongUsClient.Instance != null);
//    }

//    private void CenterWindow()
//    {
//        float width = Mathf.Min(_windowSize.x, Screen.width - 40f);
//        float height = Mathf.Min(_windowSize.y, Screen.height - 40f);

//        _windowRect = new Rect(
//            Screen.width / 2f - width / 2f,
//            Screen.height / 2f - height / 2f,
//            width,
//            height);
//    }

//    private void EnsureStyles()
//    {
//        if (_titleStyle != null)
//            return;

//        _titleStyle = new GUIStyle(MatchReportUiStyles.TitleLabel)
//        {
//            fontSize = 22,
//            fontStyle = FontStyle.Bold,
//            alignment = TextAnchor.MiddleCenter
//        };

//        _buttonStyle = new GUIStyle(MatchReportUiStyles.TintableButton)
//        {
//            alignment = TextAnchor.MiddleLeft,
//            wordWrap = false,
//            fontSize = 14,
//            richText = true,
//            padding = new RectOffset { left = 12, right = 12, top = 7, bottom = 7 }
//        };

//        _selectedButtonStyle = new GUIStyle(MatchReportUiStyles.TintableButton)
//        {
//            alignment = TextAnchor.MiddleLeft,
//            wordWrap = false,
//            fontSize = 14,
//            fontStyle = FontStyle.Bold,
//            richText = true,
//            padding = new RectOffset { left = 12, right = 12, top = 7, bottom = 7 }
//        };

//        _exitButtonStyle = new GUIStyle(MatchReportUiStyles.DangerButton)
//        {
//            fontSize = 17,
//            fontStyle = FontStyle.Bold,
//            alignment = TextAnchor.MiddleCenter
//        };

//        _labelStyle = new GUIStyle(MatchReportUiStyles.TitleLabel)
//        {
//            fontSize = 15,
//            alignment = TextAnchor.MiddleLeft,
//            wordWrap = true
//        };

//        _smallLabelStyle = new GUIStyle(_labelStyle)
//        {
//            fontSize = 12,
//            fontStyle = FontStyle.Normal
//        };

//        _panelStyle = new GUIStyle(GUI.skin.box)
//        {
//            padding = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
//        };
//        _panelStyle.normal.background = MatchReportUiStyles.BlackWindow.normal.background;
//        _panelStyle.hover.background = MatchReportUiStyles.BlackWindow.normal.background;
//        _panelStyle.active.background = MatchReportUiStyles.BlackWindow.normal.background;

//        _previewImageStyle = new GUIStyle(GUIStyle.none);
//    }

//    private void OnGUI()
//    {
//        if (!_showMenu)
//            return;

//        EnsureStyles();

//        GUI.color = Color.white;
//        GUI.contentColor = Color.white;
//        GUI.backgroundColor = Color.white;

//        if (_zoomOpen && _previewTexture != null)
//        {
//            DrawZoomOverlay();
//            return;
//        }

//        _windowRect = GUI.Window(
//            87231,
//            _windowRect,
//            (GUI.WindowFunction)WindowFunction,
//            string.Empty,
//            MatchReportUiStyles.BlackWindow);
//    }

//    private void WindowFunction(int id)
//    {
//        GUI.color = Color.white;
//        GUI.contentColor = Color.white;
//        GUI.backgroundColor = Color.white;

//        GUILayout.Label("TODAY MATCH REPORT", _titleStyle);
//        GUILayout.Space(8f);

//        bool recordingEnabled = MapEvidenceRecorder.RecordingEnabled;

//        GUI.backgroundColor = recordingEnabled
//            ? new Color(0.20f, 0.65f, 0.28f, 1f)
//            : new Color(0.65f, 0.18f, 0.18f, 1f);

//        if (GUILayout.Button(
//            recordingEnabled
//                ? "MATCH REPORT: ENABLED"
//                : "MATCH REPORT: DISABLED",
//            _selectedButtonStyle,
//            GUILayout.Height(42f)))
//        {
//            MapEvidenceRecorder.SetRecordingEnabled(!recordingEnabled);
//        }

//        GUI.backgroundColor = Color.white;
//        GUILayout.Space(10f);

//        GUILayout.BeginHorizontal();

//        DrawMatchesColumn();
//        GUILayout.Space(8f);
//        DrawEventsColumn();
//        GUILayout.Space(8f);
//        DrawPreviewColumn();

//        GUILayout.EndHorizontal();

//        GUILayout.FlexibleSpace();
//        GUILayout.Space(8f);

//        GUI.backgroundColor = Color.white;

//        if (GUILayout.Button(
//            "EXIT",
//            _exitButtonStyle,
//            GUILayout.Height(42f)))
//        {
//            _showMenu = false;
//            DestroyPreview();
//        }

//        GUI.DragWindow();
//    }

//    private void DrawMatchesColumn()
//    {
//        GUILayout.BeginVertical(_panelStyle, GUILayout.Width(210f));
//        GUILayout.Label("Completed matches", _labelStyle);
//        GUILayout.Space(5f);

//        _matchScroll = GUILayout.BeginScrollView(_matchScroll);

//        if (_matches.Count == 0)
//        {
//            GUILayout.Label("No completed match evidence.", _smallLabelStyle);
//        }
//        else
//        {
//            for (int i = 0; i < _matches.Count; i++)
//            {
//                string label = Path.GetFileName(_matches[i]);
//                GUI.backgroundColor = i == _selectedMatch
//                    ? new Color(0.30f, 0.55f, 0.90f, 1f)
//                    : Color.white;

//                if (GUILayout.Button(
//                    label,
//                    i == _selectedMatch ? _selectedButtonStyle : _buttonStyle,
//                    GUILayout.Height(38f)))
//                {
//                    SelectMatch(i);
//                }
//            }
//        }

//        GUI.backgroundColor = Color.white;
//        GUILayout.EndScrollView();
//        GUILayout.EndVertical();
//    }

//    private void DrawEventsColumn()
//    {
//        GUILayout.BeginVertical(_panelStyle, GUILayout.Width(330f));
//        GUILayout.Label("Today's events", _labelStyle);
//        GUILayout.Space(5f);

//        _eventScroll = GUILayout.BeginScrollView(_eventScroll);

//        if (_events.Count == 0)
//        {
//            GUILayout.Label("No events in this match.", _smallLabelStyle);
//        }
//        else
//        {
//            for (int i = 0; i < _events.Count; i++)
//            {
//                string matchName = Path.GetFileName(Path.GetDirectoryName(_events[i]));
//                string matchTime = matchName.Length >= 19 ? matchName.Substring(11, 8).Replace('-', ':') : matchName;
//                string label = matchTime + "  |  " + HumanizeEventName(Path.GetFileNameWithoutExtension(_events[i]));

//                GUI.backgroundColor = i == _selectedEvent
//                    ? new Color(0.30f, 0.55f, 0.90f, 1f)
//                    : Color.white;

//                if (GUILayout.Button(
//                    label,
//                    i == _selectedEvent ? _selectedButtonStyle : _buttonStyle,
//                    GUILayout.Height(40f)))
//                {
//                    SelectEvent(i);
//                }
//            }
//        }

//        GUI.backgroundColor = Color.white;
//        GUILayout.EndScrollView();
//        GUILayout.EndVertical();
//    }

//    private void DrawPreviewColumn()
//    {
//        GUILayout.BeginVertical(_panelStyle, GUILayout.ExpandWidth(true));
//        GUILayout.Label("Event preview", _labelStyle);
//        GUILayout.Space(5f);

//        Rect area = GUILayoutUtility.GetRect(
//            100f,
//            10000f,
//            100f,
//            10000f,
//            GUILayout.ExpandWidth(true),
//            GUILayout.ExpandHeight(true));

//        GUI.Box(area, GUIContent.none);

//        if (_previewTexture != null)
//        {
//            float textureWidth = Mathf.Max(1f, _previewTexture.width);
//            float textureHeight = Mathf.Max(1f, _previewTexture.height);
//            float textureAspect = textureWidth / textureHeight;

//            float areaWidth = Mathf.Max(1f, area.width);
//            float areaHeight = Mathf.Max(1f, area.height);
//            float areaAspect = areaWidth / areaHeight;

//            Rect imageRect;

//            if (textureAspect > areaAspect)
//            {
//                float fittedHeight = areaWidth / textureAspect;

//                imageRect = new Rect(
//                    area.x,
//                    area.y + (areaHeight - fittedHeight) * 0.5f,
//                    areaWidth,
//                    fittedHeight);
//            }
//            else
//            {
//                float fittedWidth = areaHeight * textureAspect;

//                imageRect = new Rect(
//                    area.x + (areaWidth - fittedWidth) * 0.5f,
//                    area.y,
//                    fittedWidth,
//                    areaHeight);
//            }

//            _previewImageStyle.normal.background = _previewTexture;
//            GUI.Box(
//                imageRect,
//                GUIContent.none,
//                _previewImageStyle);
//            _previewImageStyle.normal.background = null;

//            if (GUI.Button(
//                imageRect,
//                GUIContent.none,
//                GUIStyle.none))
//            {
//                _zoomOpen = true;
//                _zoomScale = 1f;
//            }
//        }
//        else
//        {
//            GUI.Label(
//                area,
//                "Select an event to open its image.",
//                _smallLabelStyle);
//        }

//        if (!string.IsNullOrEmpty(_previewPath))
//        {
//            GUILayout.Space(5f);

//            GUILayout.Label(
//                Path.GetFileName(_previewPath),
//                _smallLabelStyle);
//        }

//        GUILayout.EndVertical();
//    }

//    private void DrawZoomOverlay()
//    {
//        GUI.color = Color.white;
//        GUI.contentColor = Color.white;
//        GUI.backgroundColor = Color.white;

//        Rect full = new Rect(
//            0f,
//            0f,
//            Screen.width,
//            Screen.height);

//        GUI.Box(
//            full,
//            GUIContent.none,
//            MatchReportUiStyles.BlackWindow);

//        float toolbarHeight = 58f;

//        GUILayout.BeginArea(
//            new Rect(
//                14f,
//                10f,
//                Screen.width - 28f,
//                toolbarHeight));

//        GUILayout.BeginHorizontal();

//        GUILayout.Label(
//            string.IsNullOrEmpty(_previewPath)
//                ? "MATCH REPORT"
//                : Path.GetFileName(_previewPath),
//            _labelStyle,
//            GUILayout.ExpandWidth(true));

//        if (GUILayout.Button(
//            "-",
//            _selectedButtonStyle,
//            GUILayout.Width(48f),
//            GUILayout.Height(40f)))
//        {
//            _zoomScale = Mathf.Clamp(
//                _zoomScale - 0.25f,
//                0.5f,
//                5f);
//        }

//        GUILayout.Label(
//            Mathf.RoundToInt(_zoomScale * 100f) + "%",
//            _labelStyle,
//            GUILayout.Width(72f),
//            GUILayout.Height(40f));

//        if (GUILayout.Button(
//            "+",
//            _selectedButtonStyle,
//            GUILayout.Width(48f),
//            GUILayout.Height(40f)))
//        {
//            _zoomScale = Mathf.Clamp(
//                _zoomScale + 0.25f,
//                0.5f,
//                5f);
//        }

//        if (GUILayout.Button(
//            "100%",
//            _buttonStyle,
//            GUILayout.Width(72f),
//            GUILayout.Height(40f)))
//        {
//            _zoomScale = 1f;
//        }

//        if (GUILayout.Button(
//            "CLOSE",
//            _exitButtonStyle,
//            GUILayout.Width(100f),
//            GUILayout.Height(40f)))
//        {
//            _zoomOpen = false;
//            _zoomScale = 1f;
//        }

//        GUILayout.EndHorizontal();
//        GUILayout.EndArea();

//        Rect viewport = new Rect(
//            20f,
//            toolbarHeight + 16f,
//            Screen.width - 40f,
//            Screen.height - toolbarHeight - 36f);

//        Event ev = Event.current;

//        if (ev != null &&
//            ev.type == EventType.ScrollWheel &&
//            viewport.Contains(ev.mousePosition))
//        {
//            float direction = ev.delta.y > 0f ? -1f : 1f;

//            _zoomScale = Mathf.Clamp(
//                _zoomScale + direction * 0.15f,
//                0.5f,
//                5f);
//        }

//        GUI.Box(
//            viewport,
//            GUIContent.none,
//            _panelStyle);

//        float textureWidth = Mathf.Max(
//            1f,
//            _previewTexture.width);

//        float textureHeight = Mathf.Max(
//            1f,
//            _previewTexture.height);

//        float textureAspect =
//            textureWidth / textureHeight;

//        float availableWidth =
//            Mathf.Max(1f, viewport.width - 20f);

//        float availableHeight =
//            Mathf.Max(1f, viewport.height - 20f);

//        float availableAspect =
//            availableWidth / availableHeight;

//        float baseWidth;
//        float baseHeight;

//        if (textureAspect > availableAspect)
//        {
//            baseWidth = availableWidth;
//            baseHeight = baseWidth / textureAspect;
//        }
//        else
//        {
//            baseHeight = availableHeight;
//            baseWidth = baseHeight * textureAspect;
//        }

//        float zoomedWidth =
//            baseWidth * _zoomScale;

//        float zoomedHeight =
//            baseHeight * _zoomScale;

//        Rect imageRect = new Rect(
//            (viewport.width - zoomedWidth) * 0.5f,
//            (viewport.height - zoomedHeight) * 0.5f,
//            zoomedWidth,
//            zoomedHeight);

//        GUI.BeginGroup(viewport);

//        _previewImageStyle.normal.background =
//            _previewTexture;

//        GUI.Box(
//            imageRect,
//            GUIContent.none,
//            _previewImageStyle);

//        _previewImageStyle.normal.background = null;

//        GUI.EndGroup();
//    }

//    private void RefreshMatches(bool forceSelectLatest)
//    {
//        _lastRefresh = Time.unscaledTime;

//        string previousMatch =
//            _selectedMatch >= 0 &&
//            _selectedMatch < _matches.Count
//                ? _matches[_selectedMatch]
//                : null;

//        string previousEvent =
//            _selectedEvent >= 0 &&
//            _selectedEvent < _events.Count
//                ? _events[_selectedEvent]
//                : null;

//        _matches.Clear();

//        try
//        {
//            MapEvidenceRecorder.PruneOldCompletedMatches();

//            if (Directory.Exists(MapEvidenceRecorder.CompletedRoot))
//            {
//                string todayPrefix =
//                    DateTime.Now.ToString("yyyy-MM-dd_");

//                _matches.AddRange(
//                    Directory
//                        .GetDirectories(MapEvidenceRecorder.CompletedRoot)
//                        .Where(x =>
//                            !Path.GetFileName(x)
//                                .StartsWith(
//                                    "_",
//                                    StringComparison.Ordinal))
//                        .Where(x =>
//                            Path.GetFileName(x)
//                                .StartsWith(
//                                    todayPrefix,
//                                    StringComparison.OrdinalIgnoreCase))
//                        .OrderByDescending(
//                            x => x,
//                            StringComparer.OrdinalIgnoreCase));
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError(
//                "[MatchReport] EvidenceMenu RefreshMatches: " +
//                ex.Message);
//        }

//        int newMatch = -1;

//        if (!string.IsNullOrEmpty(previousMatch))
//        {
//            newMatch = _matches.FindIndex(
//                x => string.Equals(
//                    x,
//                    previousMatch,
//                    StringComparison.OrdinalIgnoreCase));
//        }

//        if (newMatch < 0 && _matches.Count > 0)
//            newMatch = 0;

//        _selectedMatch = newMatch;

//        RefreshEventsForSelectedMatch(
//            previousEvent,
//            forceSelectLatest);
//    }

//    private void SelectMatch(int index)
//    {
//        if (index < 0 || index >= _matches.Count)
//            return;

//        if (_selectedMatch == index)
//            return;

//        _selectedMatch = index;
//        _selectedEvent = -1;

//        DestroyPreview();

//        RefreshEventsForSelectedMatch(
//            null,
//            true);
//    }

//    private void RefreshEventsForSelectedMatch(
//        string previousEvent,
//        bool forceSelectLatest)
//    {
//        _events.Clear();

//        if (_selectedMatch < 0 ||
//            _selectedMatch >= _matches.Count)
//        {
//            _selectedEvent = -1;
//            DestroyPreview();
//            return;
//        }

//        string matchDirectory = _matches[_selectedMatch];

//        try
//        {
//            if (Directory.Exists(matchDirectory))
//            {
//                _events.AddRange(
//                    Directory
//                        .GetFiles(
//                            matchDirectory,
//                            "*.png",
//                            SearchOption.TopDirectoryOnly)
//                        .OrderByDescending(
//                            File.GetLastWriteTimeUtc));
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError(
//                "[MatchReport] EvidenceMenu RefreshEvents: " +
//                ex.Message);
//        }

//        int newEvent = -1;

//        if (!string.IsNullOrEmpty(previousEvent))
//        {
//            newEvent = _events.FindIndex(
//                x => string.Equals(
//                    x,
//                    previousEvent,
//                    StringComparison.OrdinalIgnoreCase));
//        }

//        if (newEvent < 0 &&
//            forceSelectLatest &&
//            _events.Count > 0)
//        {
//            newEvent = 0;
//        }

//        if (newEvent < 0 && _events.Count > 0)
//            newEvent = 0;

//        if (newEvent != _selectedEvent ||
//            _previewTexture == null)
//        {
//            SelectEvent(newEvent);
//        }
//    }

//    private void SelectEvent(int index)
//    {
//        DestroyPreview();

//        if (index < 0 || index >= _events.Count)
//        {
//            _selectedEvent = -1;
//            return;
//        }

//        _selectedEvent = index;

//        string path = _events[index];

//        try
//        {
//            if (!File.Exists(path))
//                return;

//            byte[] data = File.ReadAllBytes(path);

//            if (data == null || data.Length == 0)
//                return;

//            var tex = new Texture2D(
//                2,
//                2,
//                TextureFormat.RGBA32,
//                false);

//            tex.hideFlags = HideFlags.HideAndDontSave;

//            if (!ImageConversion.LoadImage(tex, data))
//            {
//                UnityEngine.Object.Destroy(tex);
//                return;
//            }

//            tex.wrapMode = TextureWrapMode.Clamp;
//            tex.filterMode = FilterMode.Bilinear;

//            _previewTexture = tex;
//            _previewPath = path;
//        }
//        catch (Exception ex)
//        {
//            DestroyPreview();

//            Debug.LogError(
//                "[MatchReport] EvidenceMenu SelectEvent: " +
//                ex.Message);
//        }
//    }

//    private void DestroyPreview()
//    {
//        if (_previewTexture != null)
//        {
//            UnityEngine.Object.Destroy(_previewTexture);
//            _previewTexture = null;
//        }

//        _previewPath = null;
//        _zoomOpen = false;
//        _zoomScale = 1f;
//    }

//    private static string HumanizeEventName(string value)
//    {
//        if (string.IsNullOrEmpty(value))
//            return "Event";

//        return value.Replace('_', ' ');
//    }
//}


//[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
//internal static class MapEvidence_ShipStartPatch
//{
//    [HarmonyPostfix]
//    private static void Postfix()
//    {
//        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
//            MapEvidenceRecorder.BeginMatch();
//    }
//}

//[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
//internal static class MapEvidence_MurderPatch
//{
//    [HarmonyPostfix]
//    private static void Postfix(PlayerControl __instance, PlayerControl __0)
//    {
//        MapEvidenceRecorder.Capture("KILL", __instance, __0);
//    }
//}

//[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
//internal static class MapEvidence_ShapeshiftPatch
//{
//    [HarmonyPostfix]
//    private static void Postfix(PlayerControl __instance, PlayerControl __0)
//    {
//        bool revert = __0 == null || __0.PlayerId == __instance.PlayerId;

//        MapEvidenceRecorder.Capture(
//            revert ? "SHAPESHIFT_REVERT" : "SHAPESHIFT",
//            __instance,
//            __0);
//    }
//}

//[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckVanish))]
//internal static class MapEvidence_VanishPatch
//{
//    [HarmonyPostfix]
//    private static void Postfix(PlayerControl __instance)
//    {
//        MapEvidenceRecorder.Capture("VANISH", __instance, null);
//    }
//}

//[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckAppear))]
//internal static class MapEvidence_AppearPatch
//{
//    [HarmonyPostfix]
//    private static void Postfix(PlayerControl __instance)
//    {
//        MapEvidenceRecorder.Capture("APPEAR", __instance, null);
//    }
//}

//[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
//internal static class MapEvidence_EndGamePatch
//{
//    [HarmonyPrefix]
//    private static void Prefix()
//    {
//        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
//            MapEvidenceRecorder.CompleteMatch();
//    }
//}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;

namespace BanMod;

internal static class MatchReportUiStyles
{
    private static GUIStyle _blackWindow;
    private static GUIStyle _titleLabel;
    private static GUIStyle _tintableButton;
    private static GUIStyle _dangerButton;
    private static Texture2D _windowTexture;
    private static Texture2D _buttonTexture;
    private static Texture2D _dangerTexture;

    public static GUIStyle BlackWindow
    {
        get
        {
            if (_blackWindow == null)
            {
                _blackWindow = new GUIStyle(GUI.skin.window);
                _blackWindow.padding = new RectOffset
                {
                    left = 14,
                    right = 14,
                    top = 14,
                    bottom = 14
                };
                _windowTexture = MakeTexture(new Color(0.055f, 0.055f, 0.065f, 0.98f));
                _blackWindow.normal.background = _windowTexture;
                _blackWindow.hover.background = _windowTexture;
                _blackWindow.active.background = _windowTexture;
                _blackWindow.focused.background = _windowTexture;
                _blackWindow.onNormal.background = _windowTexture;
                _blackWindow.onHover.background = _windowTexture;
                _blackWindow.onActive.background = _windowTexture;
                _blackWindow.onFocused.background = _windowTexture;
            }
            return _blackWindow;
        }
    }

    public static GUIStyle TitleLabel
    {
        get
        {
            if (_titleLabel == null)
            {
                _titleLabel = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                _titleLabel.normal.textColor = Color.white;
            }
            return _titleLabel;
        }
    }

    public static GUIStyle TintableButton
    {
        get
        {
            if (_tintableButton == null)
            {
                _tintableButton = new GUIStyle(GUI.skin.button);
                _tintableButton.normal.textColor = Color.white;
                _tintableButton.hover.textColor = Color.white;
                _tintableButton.active.textColor = Color.white;
                _buttonTexture = MakeTexture(new Color(0.22f, 0.22f, 0.25f, 1f));
                _tintableButton.normal.background = _buttonTexture;
                _tintableButton.hover.background = _buttonTexture;
                _tintableButton.active.background = _buttonTexture;
                _tintableButton.focused.background = _buttonTexture;
                _tintableButton.onNormal.background = _buttonTexture;
                _tintableButton.onHover.background = _buttonTexture;
                _tintableButton.onActive.background = _buttonTexture;
                _tintableButton.onFocused.background = _buttonTexture;
            }
            return _tintableButton;
        }
    }

    public static GUIStyle DangerButton
    {
        get
        {
            if (_dangerButton == null)
            {
                _dangerButton = new GUIStyle(GUI.skin.button);
                _dangerButton.normal.textColor = Color.white;
                _dangerButton.hover.textColor = Color.white;
                _dangerButton.active.textColor = Color.white;
                _dangerTexture = MakeTexture(new Color(0.62f, 0.12f, 0.12f, 1f));
                _dangerButton.normal.background = _dangerTexture;
                _dangerButton.hover.background = _dangerTexture;
                _dangerButton.active.background = _dangerTexture;
                _dangerButton.focused.background = _dangerTexture;
                _dangerButton.onNormal.background = _dangerTexture;
                _dangerButton.onHover.background = _dangerTexture;
                _dangerButton.onActive.background = _dangerTexture;
                _dangerButton.onFocused.background = _dangerTexture;
            }
            return _dangerButton;
        }
    }

    private static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, color);
        texture.Apply(false, true);
        return texture;
    }
}
public static class MapEvidenceRecorder
{
    private const int TargetWidth = 1100;
    private const int MinHeight = 450;
    private const int MaxHeight = 850;
    private const int FooterPixelHeight = 180;
    private const int FooterLayer = 30;

    private static readonly BepInEx.Logging.ManualLogSource Log =
        BepInEx.Logging.Logger.CreateLogSource("MatchReport");

    private static readonly Dictionary<string, int> EventCounters =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<byte, byte> CurrentShapeshiftTarget =
        new Dictionary<byte, byte>();

    private static Bounds _mapBounds;
    private static bool _hasMapBounds;
    private static bool _matchActive;
    private static DateTime _matchStartedUtc;
    private static int _sequence;

    private static readonly Queue<EventSnapshot> PendingEvents =
        new Queue<EventSnapshot>();

    private static bool _captureInProgress;
    private static bool _completeRequested;
    private static float _nextCaptureAt;

    private static RenderTexture _captureRenderTexture;
    private static Texture2D _captureTexture;
    private static int _captureWidth;
    private static int _captureHeight;

    private static string _currentSessionDir;
    private static byte[] _sessionKey;
    private static bool? _recordingEnabled;
    private const string RecordingEnabledPrefKey = "MatchReport.RecordingEnabled";

    private static string EvidenceRoot =>
    Path.Combine("./DATA/LOG/", "BanMod Match Png");

    private static string CurrentRoot =>
        Path.Combine(EvidenceRoot, "_CURRENT");

    public static string CompletedRoot =>
        EvidenceRoot;

    public static bool IsMatchActive => _matchActive;

    public static bool IsAllowedGameMode()
    {
        return !Options.GameMode.GetValue(GameModeType.SnS)
            && !Options.GameMode.GetValue(GameModeType.TaskRun)
            && !Options.GameMode.GetValue(GameModeType.FFA)
            && !Options.GameMode.GetValue(GameModeType.FFATeam)
            && !Options.GameMode.GetValue(GameModeType.ZombieMode)
            && !Options.GameMode.GetValue(GameModeType.PnS)
            && !Options.GameMode.GetValue(GameModeType.HotPotatoModded)
            && !Options.GameMode.GetValue(GameModeType.RoomRush)
            && !Options.GameMode.GetValue(GameModeType.TargetRush)
            && !Options.GameMode.GetValue(GameModeType.Assassin)
            && !Options.GameMode.GetValue(GameModeType.KillRace)
            && !Options.GameMode.GetValue(GameModeType.DeathRun);
    }

    public static bool RecordingEnabled
    {
        get
        {
            if (!_recordingEnabled.HasValue)
                _recordingEnabled = PlayerPrefs.GetInt(RecordingEnabledPrefKey, 1) != 0;

            return _recordingEnabled.Value;
        }
    }

    public static void SetRecordingEnabled(bool enabled)
    {
        _recordingEnabled = enabled;
        PlayerPrefs.SetInt(RecordingEnabledPrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled && _matchActive)
            CleanupActiveSession(deleteWorkingFiles: true);
    }

    public static void BeginMatch()
    {
        if (!RecordingEnabled)
            return;

        if (!IsAllowedGameMode())
            return;

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            return;

        try
        {
            CleanupActiveSession(deleteWorkingFiles: true);

            Directory.CreateDirectory(CurrentRoot);
            Directory.CreateDirectory(CompletedRoot);
            PruneOldCompletedMatches();

            foreach (string dir in Directory.GetDirectories(CurrentRoot))
            {
                try { Directory.Delete(dir, true); }
                catch { }
            }

            _currentSessionDir = Path.Combine(
                CurrentRoot,
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_currentSessionDir);

            _sessionKey = RandomBytes(64);

            _matchStartedUtc = DateTime.UtcNow;
            _matchActive = true;
            _sequence = 0;
            EventCounters.Clear();
            CurrentShapeshiftTarget.Clear();
            _hasMapBounds = false;

            TryCalculateMapBounds();

            Log.LogInfo("MapEvidence: recording started.");
        }
        catch (Exception ex)
        {
            Log.LogError(ex);
            CleanupActiveSession(deleteWorkingFiles: true);
        }
    }

    public static void Capture(
        string eventType,
        PlayerControl actor = null,
        PlayerControl target = null)
    {
        if (!RecordingEnabled)
            return;

        if (!IsAllowedGameMode())
            return;

        if (!_matchActive)
            return;

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            return;

        if (ShipStatus.Instance == null)
            return;

        if (string.IsNullOrEmpty(_currentSessionDir) ||
            !Directory.Exists(_currentSessionDir))
            return;

        try
        {
            if (!_hasMapBounds)
                TryCalculateMapBounds();

            if (!_hasMapBounds)
                return;

            var ev = SnapshotEvent(eventType, actor, target);

            if (actor != null)
            {
                if (EqualsEvent(eventType, "SHAPESHIFT") && target != null)
                {
                    ev.ExtraId = target.PlayerId;
                    ev.ExtraName = target.Data != null
                        ? SanitizeText(target.Data.PlayerName)
                        : "UNKNOWN";

                    CurrentShapeshiftTarget[actor.PlayerId] = target.PlayerId;
                }
                else if (EqualsEvent(eventType, "SHAPESHIFT_REVERT"))
                {
                    byte oldTargetId;
                    if (CurrentShapeshiftTarget.TryGetValue(actor.PlayerId, out oldTargetId))
                    {
                        ev.ExtraId = oldTargetId;
                        ev.ExtraName = GetPlayerNameById(oldTargetId);
                        CurrentShapeshiftTarget.Remove(actor.PlayerId);
                    }
                }
                else if (EqualsEvent(eventType, "KILL"))
                {
                    byte formId;
                    if (CurrentShapeshiftTarget.TryGetValue(actor.PlayerId, out formId))
                    {
                        ev.ExtraId = formId;
                        ev.ExtraName = GetPlayerNameById(formId);
                    }
                }
            }

            int typeNumber = NextTypeNumber(eventType);
            _sequence++;

            ev.Sequence = _sequence;
            ev.TypeNumber = typeNumber;
            ev.FileBase = BuildEventFileBase(ev);

            PendingEvents.Enqueue(ev);

            if (_nextCaptureAt < Time.unscaledTime)
                _nextCaptureAt = Time.unscaledTime + 0.08f;
        }
        catch (Exception ex)
        {
            Log.LogError("MapEvidence Capture failed: " + ex);
        }
    }

    public static void CompleteMatch()
    {
        if (!_matchActive && !_completeRequested)
            return;

        _matchActive = false;
        _completeRequested = true;

        if (!_captureInProgress && PendingEvents.Count == 0)
            FinalizeCompletedMatch();
    }

    public static void Tick()
    {
        if (_captureInProgress)
            return;

        if (PendingEvents.Count > 0 &&
            Time.unscaledTime >= _nextCaptureAt &&
            !string.IsNullOrEmpty(_currentSessionDir) &&
            Directory.Exists(_currentSessionDir))
        {
            EventSnapshot ev = PendingEvents.Dequeue();
            _captureInProgress = true;

            byte[] pngBytes = null;
            byte[] payload = null;
            byte[] encrypted = null;

            try
            {
                if (_sessionKey == null || _sessionKey.Length != 64)
                    throw new InvalidOperationException("Match report session key is unavailable.");

                pngBytes = RenderEventPng(ev);

                if (pngBytes != null && pngBytes.Length > 0)
                {
                    payload = SerializePayload(ev, pngBytes);
                    encrypted = Encrypt(payload, _sessionKey);

                    string stagedPath = Path.Combine(
                        _currentSessionDir,
                        $"EVENT_{ev.Sequence:0000}.bmr");

                    File.WriteAllBytes(stagedPath, encrypted);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("MapEvidence processing failed: " + ex);
            }
            finally
            {
                ClearBytes(encrypted);
                ClearBytes(payload);
                ClearBytes(pngBytes);

                _captureInProgress = false;
                _nextCaptureAt = Time.unscaledTime + 0.06f;
            }
        }

        if (_completeRequested &&
            !_captureInProgress &&
            PendingEvents.Count == 0)
        {
            FinalizeCompletedMatch();
        }
    }

    private static void FinalizeCompletedMatch()
    {
        if (!_completeRequested)
            return;

        _completeRequested = false;

        try
        {
            if (string.IsNullOrEmpty(_currentSessionDir) ||
                !Directory.Exists(_currentSessionDir))
            {
                CleanupActiveSession(deleteWorkingFiles: true);
                return;
            }

            if (_sessionKey == null || _sessionKey.Length != 64)
            {
                CleanupActiveSession(deleteWorkingFiles: true);
                return;
            }

            string[] stagedFiles = Directory
                .GetFiles(_currentSessionDir, "*.bmr", SearchOption.TopDirectoryOnly)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (stagedFiles.Length == 0)
            {
                CleanupActiveSession(deleteWorkingFiles: true);
                return;
            }

            string sessionName = Path.GetFileName(_currentSessionDir);
            string shortId = sessionName.Length >= 8
                ? sessionName.Substring(0, 8)
                : sessionName;

            string completedDir = Path.Combine(
                CompletedRoot,
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + shortId);

            Directory.CreateDirectory(completedDir);

            var index = new StringBuilder();
            index.AppendLine("MATCHREPORT Evidence");
            index.AppendLine("Completed match.");
            index.AppendLine();

            int exported = 0;

            foreach (string stagedFile in stagedFiles)
            {
                byte[] encrypted = null;
                byte[] payload = null;
                EvidencePayload data = null;

                try
                {
                    encrypted = File.ReadAllBytes(stagedFile);
                    payload = Decrypt(encrypted, _sessionKey);
                    data = DeserializePayload(payload);

                    if (data == null ||
                        data.PngBytes == null ||
                        data.PngBytes.Length == 0)
                        continue;

                    string safeBase = SanitizeFileName(data.FileBase);
                    string pngName = safeBase + ".png";
                    string txtName = safeBase + ".txt";

                    File.WriteAllBytes(
                        Path.Combine(completedDir, pngName),
                        data.PngBytes);

                    File.WriteAllText(
                        Path.Combine(completedDir, txtName),
                        data.TextEvidence ?? string.Empty,
                        Encoding.UTF8);

                    index.AppendLine(pngName);
                    exported++;
                }
                finally
                {
                    if (data != null)
                        ClearBytes(data.PngBytes);

                    ClearBytes(payload);
                    ClearBytes(encrypted);
                }
            }

            if (exported == 0)
            {
                try
                {
                    Directory.Delete(completedDir, true);
                }
                catch
                {
                }

                CleanupActiveSession(deleteWorkingFiles: true);
                return;
            }

            File.WriteAllText(
                Path.Combine(completedDir, "INDEX.txt"),
                index.ToString(),
                Encoding.UTF8);

            try
            {
                Directory.Delete(_currentSessionDir, true);
            }
            catch
            {
            }

            PruneOldCompletedMatches();
        }
        catch (Exception ex)
        {
            Log.LogError(ex);
        }
        finally
        {
            CleanupActiveSession(deleteWorkingFiles: false);
        }
    }

    public static void PruneOldCompletedMatches()
    {
        try
        {
            if (!Directory.Exists(CompletedRoot))
                return;

            DateTime oldestAllowedDate = DateTime.Today.AddDays(-1);

            foreach (string dir in Directory.GetDirectories(CompletedRoot))
            {
                string name = Path.GetFileName(dir);

                if (string.IsNullOrEmpty(name) || name.StartsWith("_", StringComparison.Ordinal))
                    continue;

                if (name.Length < 10)
                    continue;

                DateTime folderDate;
                if (!DateTime.TryParseExact(
                        name.Substring(0, 10),
                        "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out folderDate))
                    continue;

                if (folderDate.Date < oldestAllowedDate)
                {
                    try { Directory.Delete(dir, true); }
                    catch { }
                }
            }
        }
        catch { }
    }

    private static EventSnapshot SnapshotEvent(
        string eventType,
        PlayerControl actor,
        PlayerControl target)
    {
        var ev = new EventSnapshot
        {
            EventType = eventType ?? "EVENT",
            MatchSeconds = (float)(DateTime.UtcNow - _matchStartedUtc).TotalSeconds,
            ActorId = actor != null ? actor.PlayerId : byte.MaxValue,
            TargetId = target != null ? target.PlayerId : byte.MaxValue,
            ActorName = actor != null && actor.Data != null
                ? SanitizeText(actor.Data.PlayerName)
                : "NONE",
            TargetName = target != null && target.Data != null
                ? SanitizeText(target.Data.PlayerName)
                : "NONE"
        };

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null)
                continue;

            byte colorId = 0;
            try { colorId = (byte)pc.Data.DefaultOutfit.ColorId; }
            catch { }

            Vector2 pos = ResolveSnapshotPosition(pc);

            ev.Players.Add(new PlayerSnapshot
            {
                PlayerId = pc.PlayerId,
                Name = SanitizeText(pc.Data.PlayerName),
                X = pos.x,
                Y = pos.y,
                Dead = pc.Data.IsDead,
                Disconnected = pc.Data.Disconnected,
                InVent = pc.inVent,
                ColorId = colorId
            });
        }

        return ev;
    }

    private static Vector2 ResolveSnapshotPosition(PlayerControl pc)
    {
        if (pc == null)
            return Vector2.zero;

        Vector2 fallback = pc.transform.position;

        if (pc.Data != null && pc.Data.IsDead)
        {
            DeadBody body = FindDeadBody(pc.PlayerId);
            if (body != null)
                return body.transform.position;
        }

        if (pc.inVent)
        {
            Vent vent = FindNearestVent(fallback);
            if (vent != null)
                return vent.transform.position;
        }

        return fallback;
    }

    private static DeadBody FindDeadBody(byte playerId)
    {
        try
        {
            DeadBody[] bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            foreach (DeadBody body in bodies)
            {
                if (body != null && body.ParentId == playerId)
                    return body;
            }
        }
        catch { }

        return null;
    }

    private static Vent FindNearestVent(Vector2 position)
    {
        try
        {
            Vent[] vents = UnityEngine.Object.FindObjectsOfType<Vent>();
            Vent best = null;
            float bestDistance = float.MaxValue;

            foreach (Vent vent in vents)
            {
                if (vent == null)
                    continue;

                Vector2 ventPosition = vent.transform.position;
                float distance = (ventPosition - position).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = vent;
                }
            }

            return best;
        }
        catch
        {
            return null;
        }
    }

    private static string GetPlayerNameById(byte playerId)
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null)
                continue;

            if (pc.PlayerId == playerId)
                return SanitizeText(pc.Data.PlayerName);
        }

        return "UNKNOWN";
    }

    private static int NextTypeNumber(string eventType)
    {
        int n;
        if (!EventCounters.TryGetValue(eventType, out n))
            n = 0;

        n++;
        EventCounters[eventType] = n;
        return n;
    }

    private static string BuildEventFileBase(EventSnapshot ev)
    {
        if (EqualsEvent(ev.EventType, "KILL"))
        {
            if (ev.ExtraId != byte.MaxValue)
                return $"KILL_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_AS_{SanitizeFileName(ev.ExtraName)}_KILLED_{SanitizeFileName(ev.TargetName)}";

            return $"KILL_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_KILLED_{SanitizeFileName(ev.TargetName)}";
        }

        if (EqualsEvent(ev.EventType, "SHAPESHIFT"))
            return $"SHAPESHIFT_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_TO_{SanitizeFileName(ev.TargetName)}";

        if (EqualsEvent(ev.EventType, "SHAPESHIFT_REVERT"))
            return $"SHAPESHIFT_REVERT_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}_FROM_{SanitizeFileName(ev.ExtraName)}";

        if (EqualsEvent(ev.EventType, "VANISH"))
            return $"VANISH_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}";

        if (EqualsEvent(ev.EventType, "APPEAR"))
            return $"REVERT_VANISH_{ev.TypeNumber:000}_{SanitizeFileName(ev.ActorName)}";

        return $"{SanitizeFileName(ev.EventType).ToUpperInvariant()}_{ev.TypeNumber:000}";
    }

    private static void TryCalculateMapBounds()
    {
        var ship = ShipStatus.Instance;
        if (ship == null)
            return;

        Renderer[] renderers = ship.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds bounds = default;

        foreach (var renderer in renderers)
        {
            if (renderer == null || !renderer.enabled)
                continue;

            if (IsDarknessOrOverlayRenderer(renderer))
                continue;

            Bounds b = renderer.bounds;
            if (b.size.x <= 0.001f || b.size.y <= 0.001f)
                continue;

            if (!hasBounds)
            {
                bounds = b;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(b);
            }
        }

        if (!hasBounds)
            return;

        bounds.Expand(new Vector3(1.2f, 1.2f, 0f));

        _mapBounds = bounds;
        _hasMapBounds = true;
    }

    private static byte[] RenderEventPng(EventSnapshot ev)
    {
        GameObject mapCameraObject = null;
        GameObject footerCameraObject = null;
        GameObject footerRoot = null;
        GameObject playerNamesRoot = null;

        RenderTexture previous = RenderTexture.active;
        List<RendererState> disabled = null;
        List<BehaviourState> disabledVisionBehaviours = null;
        List<RendererState> hiddenPlayers = null;

        try
        {
            int width;
            int mapHeight;
            CalculateOutputSize(out width, out mapHeight);

            int totalHeight = mapHeight + FooterPixelHeight;

            EnsureCaptureBuffers(width, totalHeight);

            RenderTexture rt = _captureRenderTexture;
            Texture2D finalImage = _captureTexture;

            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;

            mapCameraObject = new GameObject("MatchReport_EvidenceCamera");
            mapCameraObject.hideFlags = HideFlags.HideAndDontSave;

            Camera mapCamera = mapCameraObject.AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = Color.black;
            mapCamera.nearClipPlane = 0.01f;
            mapCamera.farClipPlane = 300f;
            mapCamera.rect = new Rect(
                0f,
                FooterPixelHeight / (float)totalHeight,
                1f,
                mapHeight / (float)totalHeight);
            mapCamera.aspect = width / (float)mapHeight;
            mapCamera.transform.position =
                new Vector3(_mapBounds.center.x, _mapBounds.center.y, -50f);
            mapCamera.transform.rotation = Quaternion.identity;

            float byHeight = _mapBounds.size.y * 0.5f;
            float byWidth = (_mapBounds.size.x * 0.5f) / mapCamera.aspect;
            mapCamera.orthographicSize = Mathf.Max(byHeight, byWidth);

            if (Camera.main != null)
                mapCamera.cullingMask =
                    Camera.main.cullingMask & ~(1 << FooterLayer);

            mapCamera.targetTexture = rt;

            disabled = DisableDarknessRenderers();
            disabledVisionBehaviours = DisableVisionBehaviours();
            hiddenPlayers = DisablePlayerAndBodyRenderers();

            playerNamesRoot = new GameObject("MatchReport_PlayerNames");
            playerNamesRoot.hideFlags = HideFlags.HideAndDontSave;

            float nameOffsetX = _mapBounds.size.x * 22f / Mathf.Max(1f, width);
            foreach (PlayerSnapshot p in ev.Players)
                CreatePlayerNameLabel(playerNamesRoot.transform, p, nameOffsetX);

            ForceUpdateAllText(playerNamesRoot);

            try
            {
                mapCamera.Render();
            }
            finally
            {
                RestoreRenderers(hiddenPlayers);
                RestoreBehaviours(disabledVisionBehaviours);
                RestoreRenderers(disabled);

                hiddenPlayers = null;
                disabledVisionBehaviours = null;
                disabled = null;
            }

            footerRoot = new GameObject("MatchReport_FooterRoot");
            footerRoot.hideFlags = HideFlags.HideAndDontSave;
            SetLayerRecursively(footerRoot, FooterLayer);

            footerCameraObject = new GameObject("MatchReport_FooterCamera");
            footerCameraObject.hideFlags = HideFlags.HideAndDontSave;
            footerCameraObject.layer = FooterLayer;

            Camera footerCamera = footerCameraObject.AddComponent<Camera>();
            footerCamera.orthographic = true;
            footerCamera.clearFlags = CameraClearFlags.SolidColor;
            footerCamera.backgroundColor = Color.black;
            footerCamera.nearClipPlane = 0.01f;
            footerCamera.farClipPlane = 100f;
            footerCamera.rect = new Rect(
                0f,
                0f,
                1f,
                FooterPixelHeight / (float)totalHeight);
            footerCamera.aspect = width / (float)FooterPixelHeight;
            footerCamera.orthographicSize = 5f;
            footerCamera.transform.position = new Vector3(0f, 0f, -20f);
            footerCamera.transform.rotation = Quaternion.identity;
            footerCamera.cullingMask = 1 << FooterLayer;
            footerCamera.targetTexture = rt;

            float halfWidth = footerCamera.orthographicSize * footerCamera.aspect;
            float left = -halfWidth + 0.85f;
            float textWidth = halfWidth * 2f - 1.7f;

            CreateFooterText(
                footerRoot.transform,
                new Vector3(left, 4.20f, 0f),
                BuildEventDescription(ev),
                6.4f,
                true,
                textWidth,
                2.5f);

            CreateFooterText(
                footerRoot.transform,
                new Vector3(left, 1.55f, 0f),
                "●  Alive player     ■  Dead player     ▲  Player in vent",
                6.4f,
                true,
                textWidth,
                1.8f);

            ForceUpdateAllText(footerRoot);
            footerCamera.Render();

            RenderTexture.active = rt;

            finalImage.ReadPixels(
                new Rect(0, 0, width, totalHeight),
                0,
                0,
                false);

            finalImage.Apply(false, false);

            DrawAllMarkers(finalImage, ev);

            return finalImage.EncodeToPNG();
        }
        finally
        {
            if (hiddenPlayers != null)
                RestoreRenderers(hiddenPlayers);

            if (disabledVisionBehaviours != null)
                RestoreBehaviours(disabledVisionBehaviours);

            if (disabled != null)
                RestoreRenderers(disabled);

            RenderTexture.active = previous;

            if (footerCameraObject != null)
                UnityEngine.Object.Destroy(footerCameraObject);

            if (footerRoot != null)
                UnityEngine.Object.Destroy(footerRoot);

            if (playerNamesRoot != null)
                UnityEngine.Object.Destroy(playerNamesRoot);

            if (mapCameraObject != null)
                UnityEngine.Object.Destroy(mapCameraObject);
        }
    }

    private static void EnsureCaptureBuffers(int width, int height)
    {
        if (_captureRenderTexture != null &&
            _captureTexture != null &&
            _captureWidth == width &&
            _captureHeight == height)
        {
            return;
        }

        ReleaseCaptureBuffers();

        _captureRenderTexture = new RenderTexture(
            width,
            height,
            24,
            RenderTextureFormat.ARGB32);

        _captureRenderTexture.hideFlags = HideFlags.HideAndDontSave;
        _captureRenderTexture.Create();

        _captureTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGB24,
            false);

        _captureTexture.hideFlags = HideFlags.HideAndDontSave;

        _captureWidth = width;
        _captureHeight = height;
    }

    private static void ReleaseCaptureBuffers()
    {
        if (_captureRenderTexture != null)
        {
            _captureRenderTexture.Release();
            UnityEngine.Object.Destroy(_captureRenderTexture);
            _captureRenderTexture = null;
        }

        if (_captureTexture != null)
        {
            UnityEngine.Object.Destroy(_captureTexture);
            _captureTexture = null;
        }

        _captureWidth = 0;
        _captureHeight = 0;
    }

    private static void CalculateOutputSize(out int width, out int height)
    {
        float ratio = _mapBounds.size.x / Mathf.Max(0.001f, _mapBounds.size.y);
        width = TargetWidth;
        height = Mathf.RoundToInt(width / Mathf.Max(0.1f, ratio));
        height = Mathf.Clamp(height, MinHeight, MaxHeight);
    }

    private static void DrawAllMarkers(Texture2D image, EventSnapshot ev)
    {
        if (image == null || ev == null)
            return;

        int mapHeight = image.height - FooterPixelHeight;

        foreach (PlayerSnapshot p in ev.Players)
        {
            int px;
            int py;

            WorldToMapPixel(
                p.X,
                p.Y,
                image.width,
                mapHeight,
                out px,
                out py);

            py += FooterPixelHeight;

            Color32 fill = GetMarkerPixelColor(p);
            Color32 outline = new Color32(0, 0, 0, 255);

            if (p.InVent)
            {
                DrawFilledTriangle(image, px, py, 16, outline);
                DrawFilledTriangle(image, px, py, 11, fill);
            }
            else if (p.Dead)
            {
                DrawFilledSquare(image, px, py, 15, outline);
                DrawFilledSquare(image, px, py, 10, fill);
            }
            else
            {
                DrawFilledCircle(image, px, py, 16, outline);
                DrawFilledCircle(image, px, py, 11, fill);
            }
        }

        image.Apply(false, false);
    }

    private static Color32 GetMarkerPixelColor(PlayerSnapshot p)
    {
        Color32 baseColor = GetPlayerColor(p.ColorId);

        if (!p.Dead)
            return baseColor;

        byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(baseColor.r * 0.48f + 105f), 0, 255);
        byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(baseColor.g * 0.48f + 105f), 0, 255);
        byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(baseColor.b * 0.48f + 105f), 0, 255);
        return new Color32(r, g, b, 255);
    }

    private static void WorldToMapPixel(float x, float y, int width, int height, out int px, out int py)
    {
        float nx = Mathf.InverseLerp(_mapBounds.min.x, _mapBounds.max.x, x);
        float ny = Mathf.InverseLerp(_mapBounds.min.y, _mapBounds.max.y, y);
        px = Mathf.Clamp(Mathf.RoundToInt(nx * (width - 1)), 0, width - 1);
        py = Mathf.Clamp(Mathf.RoundToInt(ny * (height - 1)), 0, height - 1);
    }

    private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int radius, Color32 color)
    {
        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= r2)
                    SetPixelSafe(tex, cx + x, cy + y, color);
            }
        }
    }

    private static void DrawFilledSquare(Texture2D tex, int cx, int cy, int radius, Color32 color)
    {
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                SetPixelSafe(tex, cx + x, cy + y, color);
    }

    private static void DrawFilledTriangle(Texture2D tex, int cx, int cy, int radius, Color32 color)
    {
        int topY = cy + radius;
        int bottomY = cy - radius;
        int total = Mathf.Max(1, topY - bottomY);

        for (int y = bottomY; y <= topY; y++)
        {
            float t = (topY - y) / (float)total;
            int halfWidth = Mathf.RoundToInt(radius * t);
            for (int x = -halfWidth; x <= halfWidth; x++)
                SetPixelSafe(tex, cx + x, y, color);
        }
    }

    private static void SetPixelSafe(Texture2D tex, int x, int y, Color32 color)
    {
        if ((uint)x >= (uint)tex.width || (uint)y >= (uint)tex.height)
            return;

        tex.SetPixel(x, y, color);
    }

    private static string GetMarkerGlyph(PlayerSnapshot p)
    {
        if (p.InVent)
            return "▲";

        if (p.Dead)
            return "■";

        return "●";
    }

    private static Color GetMarkerColor(PlayerSnapshot p)
    {
        Color c = GetPlayerColor(p.ColorId);

        if (p.Dead)
        {
            c = Color.Lerp(c, Color.gray, 0.62f);
            c.a = 0.72f;
        }

        return c;
    }

    private static void CreateExactPlayerMarker(Transform parent, PlayerSnapshot p)
    {
        var marker = new GameObject("EvidencePlayerMarker_" + p.PlayerId);
        marker.hideFlags = HideFlags.HideAndDontSave;
        marker.transform.SetParent(parent, false);
        marker.transform.position = new Vector3(p.X, p.Y, -8f);

        var tmp = marker.AddComponent<TextMeshPro>();
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        tmp.fontSize = 6.4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.richText = false;
        tmp.text = GetMarkerGlyph(p);
        tmp.color = GetMarkerColor(p);
        tmp.outlineWidth = 0.48f;
        tmp.outlineColor = Color.black;
        tmp.ForceMeshUpdate(true, true);

        var renderer = marker.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.sortingOrder = 5002;
    }


    private static void CreatePlayerNameLabel(Transform parent, PlayerSnapshot p, float offsetX)
    {
        var go = new GameObject("EvidencePlayerName_" + p.PlayerId);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(p.X + offsetX, p.Y, -8f);

        var tmp = go.AddComponent<TextMeshPro>();
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        tmp.fontSize = 2.6f;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.richText = false;
        tmp.text = p.Name ?? "UNKNOWN";
        tmp.color = Color.white;
        tmp.outlineWidth = 0.35f;
        tmp.outlineColor = Color.black;
        tmp.ForceMeshUpdate(true, true);

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.sortingOrder = 5003;
    }

    private static void ForceUpdateAllText(GameObject root)
    {
        if (root == null)
            return;

        foreach (TextMeshPro tmp in root.GetComponentsInChildren<TextMeshPro>(true))
        {
            if (tmp != null)
                tmp.ForceMeshUpdate(true, true);
        }
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null)
            return;

        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            if (child != null)
                SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static Texture2D RenderFooter(int width, int height, EventSnapshot ev)
    {
        GameObject cameraObject = null;
        GameObject root = null;
        RenderTexture rt = null;
        Texture2D footer = null;
        RenderTexture previous = RenderTexture.active;

        try
        {
            root = new GameObject("MatchReport_FooterRoot");
            root.hideFlags = HideFlags.HideAndDontSave;
            SetLayerRecursively(root, FooterLayer);

            cameraObject = new GameObject("MatchReport_FooterCamera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            cameraObject.layer = FooterLayer;

            Camera cam = cameraObject.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            cam.aspect = width / (float)height;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -20f);
            cam.transform.rotation = Quaternion.identity;
            cam.cullingMask = 1 << FooterLayer;

            float halfWidth = cam.orthographicSize * cam.aspect;
            float left = -halfWidth + 0.85f;
            float textWidth = halfWidth * 2f - 1.7f;

            CreateFooterText(
                root.transform,
                new Vector3(left, 4.15f, 0f),
                BuildEventDescription(ev),
                6.2f,
                true,
                textWidth,
                2.4f);

            CreateFooterText(
                root.transform,
                new Vector3(left, 1.65f, 0f),
                "●  Alive player     ■  Dead player     ▲  Player in vent",
                6.2f,
                true,
                textWidth,
                1.7f);

            ForceUpdateAllText(root);

            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.hideFlags = HideFlags.HideAndDontSave;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            footer = new Texture2D(width, height, TextureFormat.RGB24, false);
            footer.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            footer.Apply(false, false);
            return footer;
        }
        finally
        {
            RenderTexture.active = previous;

            if (rt != null)
            {
                rt.Release();
                UnityEngine.Object.Destroy(rt);
            }

            if (cameraObject != null)
                UnityEngine.Object.Destroy(cameraObject);

            if (root != null)
                UnityEngine.Object.Destroy(root);
        }
    }

    private static void CreateFooterText(
        Transform parent,
        Vector3 position,
        string value,
        float fontSize,
        bool bold,
        float width,
        float height)
    {
        var go = new GameObject("MatchReport_FooterText");
        go.hideFlags = HideFlags.HideAndDontSave;
        go.layer = FooterLayer;
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        var tmp = go.AddComponent<TextMeshPro>();
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.richText = true;
        tmp.color = Color.white;
        tmp.outlineWidth = 0.18f;
        tmp.outlineColor = Color.black;
        tmp.rectTransform.pivot = new Vector2(0f, 1f);
        tmp.rectTransform.sizeDelta = new Vector2(width, height);
        tmp.text = bold ? "<b>" + value + "</b>" : value;
        tmp.ForceMeshUpdate(true, true);

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.sortingOrder = 6000;
    }

    private static Texture2D CombineMapAndFooter(Texture2D map, Texture2D footer)
    {
        int width = map.width;
        int totalHeight = map.height + footer.height;

        var result = new Texture2D(width, totalHeight, TextureFormat.RGB24, false);
        Color32[] pixels = new Color32[width * totalHeight];
        Color32 black = new Color32(0, 0, 0, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = black;

        Color32[] footerPixels = footer.GetPixels32();
        for (int y = 0; y < footer.height; y++)
        {
            int src = y * width;
            int dst = y * width;
            Array.Copy(footerPixels, src, pixels, dst, width);
        }

        Color32[] mapPixels = map.GetPixels32();
        for (int y = 0; y < map.height; y++)
        {
            int src = y * width;
            int dst = (y + footer.height) * width;
            Array.Copy(mapPixels, src, pixels, dst, width);
        }

        result.SetPixels32(pixels);
        result.Apply(false, false);
        return result;
    }

    private static string BuildActionPlayer(string playerName, byte colorId)
    {
        Color32 color = GetPlayerColor(colorId);
        string symbol = "<color=#" + ColorToHex(color) + ">●</color>";
        return EscapeRichText(playerName ?? "UNKNOWN") + " (" + symbol + ")";
    }

    private static string BuildEventDescription(EventSnapshot ev)
    {
        string actor = BuildActionPlayer(ev.ActorName, FindColorId(ev, ev.ActorId));
        string target = BuildActionPlayer(ev.TargetName, FindColorId(ev, ev.TargetId));

        if (EqualsEvent(ev.EventType, "KILL"))
        {
            if (ev.ExtraId != byte.MaxValue)
            {
                string form = BuildActionPlayer(ev.ExtraName, FindColorId(ev, ev.ExtraId));
                return actor + " shapeshifted to " + form + " killed " + target;
            }

            return actor + " killed " + target;
        }

        if (EqualsEvent(ev.EventType, "SHAPESHIFT"))
            return actor + " shapeshifted to " + target;

        if (EqualsEvent(ev.EventType, "SHAPESHIFT_REVERT"))
        {
            if (ev.ExtraId != byte.MaxValue)
            {
                string previousForm = BuildActionPlayer(ev.ExtraName, FindColorId(ev, ev.ExtraId));
                return actor + " reverted shapeshift from " + previousForm;
            }

            return actor + " reverted shapeshift";
        }

        if (EqualsEvent(ev.EventType, "VANISH"))
            return actor + " vanished";

        if (EqualsEvent(ev.EventType, "APPEAR"))
            return actor + " reverted vanish";

        return ev.EventType;
    }

    private static List<RendererState> DisablePlayerAndBodyRenderers()
    {
        var result = new List<RendererState>();

        try
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null)
                    continue;

                foreach (Renderer renderer in pc.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null || !renderer.enabled)
                        continue;

                    result.Add(new RendererState { Renderer = renderer, WasEnabled = true });
                    renderer.enabled = false;
                }
            }

            DeadBody[] bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            foreach (DeadBody body in bodies)
            {
                if (body == null)
                    continue;

                foreach (Renderer renderer in body.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null || !renderer.enabled)
                        continue;

                    result.Add(new RendererState { Renderer = renderer, WasEnabled = true });
                    renderer.enabled = false;
                }
            }
        }
        catch { }

        return result;
    }

    private static List<BehaviourState> DisableVisionBehaviours()
    {
        var result = new List<BehaviourState>();

        foreach (ShadowCamera behaviour in UnityEngine.Object.FindObjectsOfType<ShadowCamera>())
            DisableBehaviour(behaviour, result);

        foreach (ShadowCollab behaviour in UnityEngine.Object.FindObjectsOfType<ShadowCollab>())
            DisableBehaviour(behaviour, result);

        foreach (LightSource behaviour in UnityEngine.Object.FindObjectsOfType<LightSource>())
            DisableBehaviour(behaviour, result);

        foreach (OneWayShadows behaviour in UnityEngine.Object.FindObjectsOfType<OneWayShadows>())
            DisableBehaviour(behaviour, result);

        return result;
    }

    private static void DisableBehaviour(Behaviour behaviour, List<BehaviourState> result)
    {
        if (behaviour == null || !behaviour.enabled)
            return;

        result.Add(new BehaviourState
        {
            Behaviour = behaviour,
            WasEnabled = behaviour.enabled
        });

        behaviour.enabled = false;
    }

    private static void RestoreBehaviours(List<BehaviourState> behaviours)
    {
        if (behaviours == null)
            return;

        foreach (BehaviourState state in behaviours)
        {
            if (state != null && state.Behaviour != null)
                state.Behaviour.enabled = state.WasEnabled;
        }
    }

    private static List<RendererState> DisableDarknessRenderers()
    {
        var result = new List<RendererState>();

        try
        {
            Renderer[] allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();

            foreach (Renderer renderer in allRenderers)
            {
                if (renderer == null || !renderer.enabled)
                    continue;

                if (!IsDarknessOrOverlayRenderer(renderer))
                    continue;

                result.Add(new RendererState
                {
                    Renderer = renderer,
                    WasEnabled = renderer.enabled
                });

                renderer.enabled = false;
            }
        }
        catch { }

        return result;
    }

    private static bool IsDarknessOrOverlayRenderer(Renderer renderer)
    {
        if (renderer == null)
            return false;

        string objectName = renderer.name == null
            ? string.Empty
            : renderer.name.ToLowerInvariant();

        if (ContainsVisionOverlayName(objectName))
            return true;

        try
        {
            Transform t = renderer.transform;
            for (int i = 0; i < 6 && t != null; i++, t = t.parent)
            {
                string n = t.name == null ? string.Empty : t.name.ToLowerInvariant();
                if (ContainsVisionOverlayName(n))
                    return true;
            }
        }
        catch { }

        try
        {
            Material material = renderer.sharedMaterial;
            if (material != null)
            {
                string materialName = material.name == null
                    ? string.Empty
                    : material.name.ToLowerInvariant();

                if (ContainsVisionOverlayName(materialName))
                    return true;

                if (material.shader != null)
                {
                    string shaderName = material.shader.name == null
                        ? string.Empty
                        : material.shader.name.ToLowerInvariant();

                    if (ContainsVisionOverlayName(shaderName))
                        return true;
                }
            }
        }
        catch { }

        return false;
    }

    private static bool ContainsVisionOverlayName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.Contains("shadowquad") ||
               value.Contains("shadowmask") ||
               value.Contains("shadowcamera") ||
               value.Contains("shadowcollab") ||
               value.Contains("lightsource") ||
               value.Contains("light source") ||
               value.Contains("lightmask") ||
               value.Contains("light mask") ||
               value.Contains("fov") ||
               value.Contains("vision") ||
               value.Contains("vignette") ||
               value.Contains("fog") ||
               value.Contains("onewayshadow") ||
               value.Contains("one way shadow") ||
               value.Contains("shadow mask") ||
               value.Contains("shadowmask");
    }

    private static void RestoreRenderers(List<RendererState> renderers)
    {
        if (renderers == null)
            return;

        foreach (RendererState state in renderers)
        {
            if (state != null && state.Renderer != null)
                state.Renderer.enabled = state.WasEnabled;
        }
    }

    private static byte[] SerializePayload(EventSnapshot ev, byte[] png)
    {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms, Encoding.UTF8))
        {
            bw.Write(2);
            bw.Write(ev.Sequence);
            bw.Write(ev.TypeNumber);
            bw.Write(ev.EventType ?? string.Empty);
            bw.Write(ev.MatchSeconds);
            bw.Write(ev.ActorName ?? string.Empty);
            bw.Write(ev.TargetName ?? string.Empty);
            bw.Write(ev.FileBase ?? string.Empty);
            bw.Write(BuildTextEvidence(ev));
            bw.Write(png.Length);
            bw.Write(png);
            bw.Flush();
            return ms.ToArray();
        }
    }

    private static EvidencePayload DeserializePayload(byte[] payload)
    {
        using (var ms = new MemoryStream(payload, false))
        using (var br = new BinaryReader(ms, Encoding.UTF8))
        {
            int version = br.ReadInt32();

            if (version != 2)
                throw new InvalidDataException("Unsupported evidence payload version.");

            var data = new EvidencePayload();
            data.Sequence = br.ReadInt32();
            data.TypeNumber = br.ReadInt32();
            data.EventType = br.ReadString();
            data.MatchSeconds = br.ReadSingle();
            data.ActorName = br.ReadString();
            data.TargetName = br.ReadString();
            data.FileBase = br.ReadString();
            data.TextEvidence = br.ReadString();

            int pngLength = br.ReadInt32();

            if (pngLength < 0 || pngLength > 64 * 1024 * 1024)
                throw new InvalidDataException("Invalid evidence image length.");

            data.PngBytes = br.ReadBytes(pngLength);

            if (data.PngBytes.Length != pngLength)
                throw new EndOfStreamException();

            return data;
        }
    }

    private static byte[] Encrypt(byte[] plain, byte[] keyMaterial)
    {
        byte[] aesKey = new byte[32];
        byte[] macKey = new byte[32];
        Buffer.BlockCopy(keyMaterial, 0, aesKey, 0, 32);
        Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

        byte[] iv = RandomBytes(16);
        byte[] cipher;

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = aesKey;
                aes.IV = iv;

                using (ICryptoTransform enc = aes.CreateEncryptor())
                {
                    cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
                }
            }

            byte[] authenticated = new byte[4 + iv.Length + cipher.Length];
            authenticated[0] = (byte)'B';
            authenticated[1] = (byte)'M';
            authenticated[2] = (byte)'E';
            authenticated[3] = (byte)'1';
            Buffer.BlockCopy(iv, 0, authenticated, 4, iv.Length);
            Buffer.BlockCopy(cipher, 0, authenticated, 4 + iv.Length, cipher.Length);

            byte[] tag;
            using (var hmac = new HMACSHA256(macKey))
                tag = hmac.ComputeHash(authenticated);

            byte[] output = new byte[authenticated.Length + tag.Length];
            Buffer.BlockCopy(authenticated, 0, output, 0, authenticated.Length);
            Buffer.BlockCopy(tag, 0, output, authenticated.Length, tag.Length);

            ClearBytes(authenticated);
            ClearBytes(tag);
            ClearBytes(cipher);

            return output;
        }
        finally
        {
            ClearBytes(aesKey);
            ClearBytes(macKey);
            ClearBytes(iv);
        }
    }

    private static byte[] Decrypt(byte[] encrypted, byte[] keyMaterial)
    {
        if (encrypted == null || encrypted.Length < 4 + 16 + 32 + 16)
            throw new InvalidDataException("Invalid evidence file.");

        if (encrypted[0] != (byte)'B' ||
            encrypted[1] != (byte)'M' ||
            encrypted[2] != (byte)'E' ||
            encrypted[3] != (byte)'1')
            throw new InvalidDataException("Invalid evidence magic.");

        byte[] aesKey = new byte[32];
        byte[] macKey = new byte[32];
        Buffer.BlockCopy(keyMaterial, 0, aesKey, 0, 32);
        Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

        int tagOffset = encrypted.Length - 32;
        byte[] expectedTag = null;

        try
        {
            using (var hmac = new HMACSHA256(macKey))
                expectedTag = hmac.ComputeHash(encrypted, 0, tagOffset);

            if (!ConstantTimeEquals(encrypted, tagOffset, expectedTag))
                throw new CryptographicException("Evidence integrity check failed.");

            byte[] iv = new byte[16];
            Buffer.BlockCopy(encrypted, 4, iv, 0, 16);

            int cipherOffset = 20;
            int cipherLength = tagOffset - cipherOffset;
            byte[] cipher = new byte[cipherLength];
            Buffer.BlockCopy(encrypted, cipherOffset, cipher, 0, cipherLength);

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = aesKey;
                    aes.IV = iv;

                    using (ICryptoTransform dec = aes.CreateDecryptor())
                        return dec.TransformFinalBlock(cipher, 0, cipher.Length);
                }
            }
            finally
            {
                ClearBytes(iv);
                ClearBytes(cipher);
            }
        }
        finally
        {
            ClearBytes(aesKey);
            ClearBytes(macKey);
            ClearBytes(expectedTag);
        }
    }

    private static bool ConstantTimeEquals(byte[] source, int sourceOffset, byte[] expected)
    {
        if (source == null || expected == null ||
            sourceOffset < 0 || sourceOffset + expected.Length > source.Length)
            return false;

        int diff = 0;
        for (int i = 0; i < expected.Length; i++)
            diff |= source[sourceOffset + i] ^ expected[i];

        return diff == 0;
    }

    private static byte[] RandomBytes(int count)
    {
        byte[] bytes = new byte[count];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            rng.GetBytes(bytes);
        return bytes;
    }

    private static string BuildTextEvidence(EventSnapshot data)
    {
        var sb = new StringBuilder();
        sb.AppendLine(BuildEventDescription(data));
        sb.AppendLine();
        sb.AppendLine("Legend:");
        sb.AppendLine("● Alive player");
        sb.AppendLine("■ Dead player");
        sb.AppendLine("▲ Player in vent");
        return sb.ToString();
    }

    private static string BuildPlainPlayerLine(PlayerSnapshot p, EventSnapshot ev)
    {
        string me = PlainPlayerDisplayName(p.Name, p.ColorId);

        if (p.PlayerId == ev.ActorId)
        {
            if (EqualsEvent(ev.EventType, "KILL"))
            {
                string victim = PlainPlayerDisplayName(ev.TargetName, FindColorId(ev, ev.TargetId));
                if (ev.ExtraId != byte.MaxValue)
                {
                    string form = PlainPlayerDisplayName(ev.ExtraName, FindColorId(ev, ev.ExtraId));
                    return me + " shapeshifted to " + form + " killed " + victim;
                }
                return me + " killed " + victim;
            }

            if (EqualsEvent(ev.EventType, "SHAPESHIFT"))
                return me + " shapeshifted to " + PlainPlayerDisplayName(ev.TargetName, FindColorId(ev, ev.TargetId));

            if (EqualsEvent(ev.EventType, "SHAPESHIFT_REVERT"))
            {
                if (ev.ExtraId != byte.MaxValue)
                    return me + " reverted shapeshift from " + PlainPlayerDisplayName(ev.ExtraName, FindColorId(ev, ev.ExtraId));
                return me + " reverted shapeshift";
            }

            if (EqualsEvent(ev.EventType, "VANISH"))
                return me + " vanished";

            if (EqualsEvent(ev.EventType, "APPEAR"))
                return me + " reverted vanish";
        }

        if (p.InVent)
            return me + " is in vent";
        if (p.Dead)
            return me + " is dead";
        if (p.Disconnected)
            return me + " is disconnected";

        return me;
    }

    private static void CleanupActiveSession(bool deleteWorkingFiles)
    {
        _matchActive = false;
        _hasMapBounds = false;
        _sequence = 0;
        _captureInProgress = false;
        _completeRequested = false;
        _nextCaptureAt = 0f;

        PendingEvents.Clear();
        EventCounters.Clear();
        CurrentShapeshiftTarget.Clear();

        if (deleteWorkingFiles &&
            !string.IsNullOrEmpty(_currentSessionDir) &&
            Directory.Exists(_currentSessionDir))
        {
            try
            {
                Directory.Delete(_currentSessionDir, true);
            }
            catch
            {
            }
        }

        ClearBytes(_sessionKey);
        _sessionKey = null;
        _currentSessionDir = null;

        ReleaseCaptureBuffers();
    }

    private static void ClearBytes(byte[] data)
    {
        if (data != null)
            Array.Clear(data, 0, data.Length);
    }

    private static bool EqualsEvent(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static Color32 GetPlayerColor(byte colorId)
    {
        try
        {
            if (colorId < Palette.PlayerColors.Length)
                return Palette.PlayerColors[colorId];
        }
        catch { }

        return new Color32(255, 255, 255, 255);
    }

    private static string GetColorName(byte colorId)
    {
        switch (colorId)
        {
            case 0: return "red";
            case 1: return "blue";
            case 2: return "green";
            case 3: return "pink";
            case 4: return "orange";
            case 5: return "yellow";
            case 6: return "black";
            case 7: return "white";
            case 8: return "purple";
            case 9: return "brown";
            case 10: return "cyan";
            case 11: return "lime";
            case 12: return "maroon";
            case 13: return "rose";
            case 14: return "banana";
            case 15: return "gray";
            case 16: return "tan";
            case 17: return "coral";
            default: return "unknown";
        }
    }

    private static string BuildPlayerDisplayName(string playerName, byte colorId)
    {
        string safeName = EscapeRichText(playerName ?? "UNKNOWN");
        return safeName + " (" + GetColorName(colorId) + ")";
    }

    private static string PlainPlayerDisplayName(string playerName, byte colorId)
    {
        return (playerName ?? "UNKNOWN") + " (" + GetColorName(colorId) + ")";
    }

    private static byte FindColorId(EventSnapshot ev, byte playerId)
    {
        if (ev == null)
            return 255;

        foreach (var p in ev.Players)
        {
            if (p.PlayerId == playerId)
                return p.ColorId;
        }

        return 255;
    }

    private static string ColorToHex(Color32 c)
    {
        return $"{c.r:X2}{c.g:X2}{c.b:X2}";
    }

    private static string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static string FormatDisplayTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }

    private static string SanitizeText(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\t", " ")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "UNKNOWN";

        foreach (char c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');

        return value.Replace(' ', '_');
    }

    private sealed class BehaviourState
    {
        public Behaviour Behaviour;
        public bool WasEnabled;
    }

    private sealed class RendererState
    {
        public Renderer Renderer;
        public bool WasEnabled;
    }

    private sealed class EventSnapshot
    {
        public int Sequence;
        public int TypeNumber;
        public string EventType = string.Empty;
        public float MatchSeconds;
        public byte ActorId = byte.MaxValue;
        public byte TargetId = byte.MaxValue;
        public string ActorName = "NONE";
        public string TargetName = "NONE";
        public byte ExtraId = byte.MaxValue;
        public string ExtraName = "NONE";
        public string FileBase = string.Empty;
        public readonly List<PlayerSnapshot> Players = new List<PlayerSnapshot>();
    }

    private sealed class PlayerSnapshot
    {
        public byte PlayerId;
        public string Name = string.Empty;
        public float X;
        public float Y;
        public bool Dead;
        public bool Disconnected;
        public bool InVent;
        public byte ColorId;
    }

    private sealed class EvidencePayload
    {
        public int Sequence;
        public int TypeNumber;
        public string EventType;
        public float MatchSeconds;
        public string ActorName;
        public string TargetName;
        public string FileBase;
        public string TextEvidence;
        public byte[] PngBytes;
    }
}

public class EvidenceMenu : MonoBehaviour
{
    public EvidenceMenu(IntPtr ptr) : base(ptr) { }

    public static EvidenceMenu Instance;

    private bool _showMenu;
    private Rect _windowRect;
    private readonly Vector2 _windowSize = new Vector2(1040f, 700f);

    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _selectedButtonStyle;
    private GUIStyle _exitButtonStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _smallLabelStyle;
    private GUIStyle _panelStyle;
    private GUIStyle _previewImageStyle;

    private Vector2 _matchScroll;
    private Vector2 _eventScroll;

    private readonly List<string> _matches = new List<string>();
    private readonly List<string> _events = new List<string>();

    private int _selectedMatch = -1;
    private int _selectedEvent = -1;
    private Texture2D _previewTexture;
    private string _previewPath;
    private float _lastRefresh;
    private bool _zoomOpen;
    private float _zoomScale = 1f;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CenterWindow();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        DestroyPreview();
    }

    private void Update()
    {
        MapEvidenceRecorder.Tick();

        if (_zoomOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            _zoomOpen = false;
            _zoomScale = 1f;
        }

        if (Input.GetKeyDown(KeyCode.F8) &&
            IsInLobby())
        {
            _showMenu = !_showMenu;

            if (_showMenu)
            {
                RefreshMatches(true);
                CenterWindow();
            }
            else
            {
                DestroyPreview();
            }
        }

        if (_showMenu && !IsInLobby())
        {
            _showMenu = false;
            DestroyPreview();
        }

        if (_showMenu && Time.unscaledTime - _lastRefresh > 2f)
            RefreshMatches(false);
    }

    private static bool IsInLobby()
    {
        return ShipStatus.Instance == null &&
               !MapEvidenceRecorder.IsMatchActive &&
               (LobbyBehaviour.Instance != null || AmongUsClient.Instance != null);
    }

    private void CenterWindow()
    {
        float width = Mathf.Min(_windowSize.x, Screen.width - 40f);
        float height = Mathf.Min(_windowSize.y, Screen.height - 40f);

        _windowRect = new Rect(
            Screen.width / 2f - width / 2f,
            Screen.height / 2f - height / 2f,
            width,
            height);
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
            return;

        _titleStyle = new GUIStyle(MatchReportUiStyles.TitleLabel)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _buttonStyle = new GUIStyle(MatchReportUiStyles.TintableButton)
        {
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            fontSize = 14,
            richText = true,
            padding = new RectOffset { left = 12, right = 12, top = 7, bottom = 7 }
        };

        _selectedButtonStyle = new GUIStyle(MatchReportUiStyles.TintableButton)
        {
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            richText = true,
            padding = new RectOffset { left = 12, right = 12, top = 7, bottom = 7 }
        };

        _exitButtonStyle = new GUIStyle(MatchReportUiStyles.DangerButton)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _labelStyle = new GUIStyle(MatchReportUiStyles.TitleLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        };

        _smallLabelStyle = new GUIStyle(_labelStyle)
        {
            fontSize = 12,
            fontStyle = FontStyle.Normal
        };

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };
        _panelStyle.normal.background = MatchReportUiStyles.BlackWindow.normal.background;
        _panelStyle.hover.background = MatchReportUiStyles.BlackWindow.normal.background;
        _panelStyle.active.background = MatchReportUiStyles.BlackWindow.normal.background;

        _previewImageStyle = new GUIStyle(GUIStyle.none);
    }

    private void OnGUI()
    {
        if (!_showMenu)
            return;

        EnsureStyles();

        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;

        if (_zoomOpen && _previewTexture != null)
        {
            DrawZoomOverlay();
            return;
        }

        _windowRect = GUI.Window(
            87231,
            _windowRect,
            (GUI.WindowFunction)WindowFunction,
            string.Empty,
            MatchReportUiStyles.BlackWindow);
    }

    private void WindowFunction(int id)
    {
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;

        GUILayout.Label("TODAY MATCH REPORT", _titleStyle);
        GUILayout.Space(8f);

        bool recordingEnabled = MapEvidenceRecorder.RecordingEnabled;

        GUI.backgroundColor = recordingEnabled
            ? new Color(0.20f, 0.65f, 0.28f, 1f)
            : new Color(0.65f, 0.18f, 0.18f, 1f);

        if (GUILayout.Button(
            recordingEnabled
                ? "MATCH REPORT: ENABLED"
                : "MATCH REPORT: DISABLED",
            _selectedButtonStyle,
            GUILayout.Height(42f)))
        {
            MapEvidenceRecorder.SetRecordingEnabled(!recordingEnabled);
        }

        GUI.backgroundColor = Color.white;
        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();

        DrawMatchesColumn();
        GUILayout.Space(8f);
        DrawEventsColumn();
        GUILayout.Space(8f);
        DrawPreviewColumn();

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.Space(8f);

        GUI.backgroundColor = Color.white;

        if (GUILayout.Button(
            "EXIT",
            _exitButtonStyle,
            GUILayout.Height(42f)))
        {
            _showMenu = false;
            DestroyPreview();
        }

        GUI.DragWindow();
    }

    private void DrawMatchesColumn()
    {
        GUILayout.BeginVertical(_panelStyle, GUILayout.Width(210f));
        GUILayout.Label("Completed matches", _labelStyle);
        GUILayout.Space(5f);

        _matchScroll = GUILayout.BeginScrollView(_matchScroll);

        if (_matches.Count == 0)
        {
            GUILayout.Label("No completed match evidence.", _smallLabelStyle);
        }
        else
        {
            for (int i = 0; i < _matches.Count; i++)
            {
                string label = Path.GetFileName(_matches[i]);
                GUI.backgroundColor = i == _selectedMatch
                    ? new Color(0.30f, 0.55f, 0.90f, 1f)
                    : Color.white;

                if (GUILayout.Button(
                    label,
                    i == _selectedMatch ? _selectedButtonStyle : _buttonStyle,
                    GUILayout.Height(38f)))
                {
                    SelectMatch(i);
                }
            }
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawEventsColumn()
    {
        GUILayout.BeginVertical(_panelStyle, GUILayout.Width(330f));
        GUILayout.Label("Today's events", _labelStyle);
        GUILayout.Space(5f);

        _eventScroll = GUILayout.BeginScrollView(_eventScroll);

        if (_events.Count == 0)
        {
            GUILayout.Label("No events in this match.", _smallLabelStyle);
        }
        else
        {
            for (int i = 0; i < _events.Count; i++)
            {
                string matchName = Path.GetFileName(Path.GetDirectoryName(_events[i]));
                string matchTime = matchName.Length >= 19 ? matchName.Substring(11, 8).Replace('-', ':') : matchName;
                string label = matchTime + "  |  " + HumanizeEventName(Path.GetFileNameWithoutExtension(_events[i]));

                GUI.backgroundColor = i == _selectedEvent
                    ? new Color(0.30f, 0.55f, 0.90f, 1f)
                    : Color.white;

                if (GUILayout.Button(
                    label,
                    i == _selectedEvent ? _selectedButtonStyle : _buttonStyle,
                    GUILayout.Height(40f)))
                {
                    SelectEvent(i);
                }
            }
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawPreviewColumn()
    {
        GUILayout.BeginVertical(_panelStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label("Event preview", _labelStyle);
        GUILayout.Space(5f);

        Rect area = GUILayoutUtility.GetRect(
            100f,
            10000f,
            100f,
            10000f,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        GUI.Box(area, GUIContent.none);

        if (_previewTexture != null)
        {
            float textureWidth = Mathf.Max(1f, _previewTexture.width);
            float textureHeight = Mathf.Max(1f, _previewTexture.height);
            float textureAspect = textureWidth / textureHeight;

            float areaWidth = Mathf.Max(1f, area.width);
            float areaHeight = Mathf.Max(1f, area.height);
            float areaAspect = areaWidth / areaHeight;

            Rect imageRect;

            if (textureAspect > areaAspect)
            {
                float fittedHeight = areaWidth / textureAspect;

                imageRect = new Rect(
                    area.x,
                    area.y + (areaHeight - fittedHeight) * 0.5f,
                    areaWidth,
                    fittedHeight);
            }
            else
            {
                float fittedWidth = areaHeight * textureAspect;

                imageRect = new Rect(
                    area.x + (areaWidth - fittedWidth) * 0.5f,
                    area.y,
                    fittedWidth,
                    areaHeight);
            }

            _previewImageStyle.normal.background = _previewTexture;
            GUI.Box(
                imageRect,
                GUIContent.none,
                _previewImageStyle);
            _previewImageStyle.normal.background = null;

            if (GUI.Button(
                imageRect,
                GUIContent.none,
                GUIStyle.none))
            {
                _zoomOpen = true;
                _zoomScale = 1f;
            }
        }
        else
        {
            GUI.Label(
                area,
                "Select an event to open its image.",
                _smallLabelStyle);
        }

        if (!string.IsNullOrEmpty(_previewPath))
        {
            GUILayout.Space(5f);

            GUILayout.Label(
                Path.GetFileName(_previewPath),
                _smallLabelStyle);
        }

        GUILayout.EndVertical();
    }

    private void DrawZoomOverlay()
    {
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;

        Rect full = new Rect(
            0f,
            0f,
            Screen.width,
            Screen.height);

        GUI.Box(
            full,
            GUIContent.none,
            MatchReportUiStyles.BlackWindow);

        float toolbarHeight = 58f;

        GUILayout.BeginArea(
            new Rect(
                14f,
                10f,
                Screen.width - 28f,
                toolbarHeight));

        GUILayout.BeginHorizontal();

        GUILayout.Label(
            string.IsNullOrEmpty(_previewPath)
                ? "MATCH REPORT"
                : Path.GetFileName(_previewPath),
            _labelStyle,
            GUILayout.ExpandWidth(true));

        if (GUILayout.Button(
            "-",
            _selectedButtonStyle,
            GUILayout.Width(48f),
            GUILayout.Height(40f)))
        {
            _zoomScale = Mathf.Clamp(
                _zoomScale - 0.25f,
                0.5f,
                5f);
        }

        GUILayout.Label(
            Mathf.RoundToInt(_zoomScale * 100f) + "%",
            _labelStyle,
            GUILayout.Width(72f),
            GUILayout.Height(40f));

        if (GUILayout.Button(
            "+",
            _selectedButtonStyle,
            GUILayout.Width(48f),
            GUILayout.Height(40f)))
        {
            _zoomScale = Mathf.Clamp(
                _zoomScale + 0.25f,
                0.5f,
                5f);
        }

        if (GUILayout.Button(
            "100%",
            _buttonStyle,
            GUILayout.Width(72f),
            GUILayout.Height(40f)))
        {
            _zoomScale = 1f;
        }

        if (GUILayout.Button(
            "CLOSE",
            _exitButtonStyle,
            GUILayout.Width(100f),
            GUILayout.Height(40f)))
        {
            _zoomOpen = false;
            _zoomScale = 1f;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        Rect viewport = new Rect(
            20f,
            toolbarHeight + 16f,
            Screen.width - 40f,
            Screen.height - toolbarHeight - 36f);

        Event ev = Event.current;

        if (ev != null &&
            ev.type == EventType.ScrollWheel &&
            viewport.Contains(ev.mousePosition))
        {
            float direction = ev.delta.y > 0f ? -1f : 1f;

            _zoomScale = Mathf.Clamp(
                _zoomScale + direction * 0.15f,
                0.5f,
                5f);
        }

        GUI.Box(
            viewport,
            GUIContent.none,
            _panelStyle);

        float textureWidth = Mathf.Max(
            1f,
            _previewTexture.width);

        float textureHeight = Mathf.Max(
            1f,
            _previewTexture.height);

        float textureAspect =
            textureWidth / textureHeight;

        float availableWidth =
            Mathf.Max(1f, viewport.width - 20f);

        float availableHeight =
            Mathf.Max(1f, viewport.height - 20f);

        float availableAspect =
            availableWidth / availableHeight;

        float baseWidth;
        float baseHeight;

        if (textureAspect > availableAspect)
        {
            baseWidth = availableWidth;
            baseHeight = baseWidth / textureAspect;
        }
        else
        {
            baseHeight = availableHeight;
            baseWidth = baseHeight * textureAspect;
        }

        float zoomedWidth =
            baseWidth * _zoomScale;

        float zoomedHeight =
            baseHeight * _zoomScale;

        Rect imageRect = new Rect(
            (viewport.width - zoomedWidth) * 0.5f,
            (viewport.height - zoomedHeight) * 0.5f,
            zoomedWidth,
            zoomedHeight);

        GUI.BeginGroup(viewport);

        _previewImageStyle.normal.background =
            _previewTexture;

        GUI.Box(
            imageRect,
            GUIContent.none,
            _previewImageStyle);

        _previewImageStyle.normal.background = null;

        GUI.EndGroup();
    }

    private void RefreshMatches(bool forceSelectLatest)
    {
        _lastRefresh = Time.unscaledTime;

        string previousMatch =
            _selectedMatch >= 0 &&
            _selectedMatch < _matches.Count
                ? _matches[_selectedMatch]
                : null;

        string previousEvent =
            _selectedEvent >= 0 &&
            _selectedEvent < _events.Count
                ? _events[_selectedEvent]
                : null;

        _matches.Clear();

        try
        {
            MapEvidenceRecorder.PruneOldCompletedMatches();

            if (Directory.Exists(MapEvidenceRecorder.CompletedRoot))
            {
                string todayPrefix =
                    DateTime.Now.ToString("yyyy-MM-dd_");

                _matches.AddRange(
                    Directory
                        .GetDirectories(MapEvidenceRecorder.CompletedRoot)
                        .Where(x =>
                            !Path.GetFileName(x)
                                .StartsWith(
                                    "_",
                                    StringComparison.Ordinal))
                        .Where(x =>
                            Path.GetFileName(x)
                                .StartsWith(
                                    todayPrefix,
                                    StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(
                            x => x,
                            StringComparer.OrdinalIgnoreCase));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[MatchReport] EvidenceMenu RefreshMatches: " +
                ex.Message);
        }

        int newMatch = -1;

        if (!string.IsNullOrEmpty(previousMatch))
        {
            newMatch = _matches.FindIndex(
                x => string.Equals(
                    x,
                    previousMatch,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (newMatch < 0 && _matches.Count > 0)
            newMatch = 0;

        _selectedMatch = newMatch;

        RefreshEventsForSelectedMatch(
            previousEvent,
            forceSelectLatest);
    }

    private void SelectMatch(int index)
    {
        if (index < 0 || index >= _matches.Count)
            return;

        if (_selectedMatch == index)
            return;

        _selectedMatch = index;
        _selectedEvent = -1;

        DestroyPreview();

        RefreshEventsForSelectedMatch(
            null,
            true);
    }

    private void RefreshEventsForSelectedMatch(
        string previousEvent,
        bool forceSelectLatest)
    {
        _events.Clear();

        if (_selectedMatch < 0 ||
            _selectedMatch >= _matches.Count)
        {
            _selectedEvent = -1;
            DestroyPreview();
            return;
        }

        string matchDirectory = _matches[_selectedMatch];

        try
        {
            if (Directory.Exists(matchDirectory))
            {
                _events.AddRange(
                    Directory
                        .GetFiles(
                            matchDirectory,
                            "*.png",
                            SearchOption.TopDirectoryOnly)
                        .OrderByDescending(
                            File.GetLastWriteTimeUtc));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[MatchReport] EvidenceMenu RefreshEvents: " +
                ex.Message);
        }

        int newEvent = -1;

        if (!string.IsNullOrEmpty(previousEvent))
        {
            newEvent = _events.FindIndex(
                x => string.Equals(
                    x,
                    previousEvent,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (newEvent < 0 &&
            forceSelectLatest &&
            _events.Count > 0)
        {
            newEvent = 0;
        }

        if (newEvent < 0 && _events.Count > 0)
            newEvent = 0;

        if (newEvent != _selectedEvent ||
            _previewTexture == null)
        {
            SelectEvent(newEvent);
        }
    }

    private void SelectEvent(int index)
    {
        DestroyPreview();

        if (index < 0 || index >= _events.Count)
        {
            _selectedEvent = -1;
            return;
        }

        _selectedEvent = index;

        string path = _events[index];

        try
        {
            if (!File.Exists(path))
                return;

            byte[] data = File.ReadAllBytes(path);

            if (data == null || data.Length == 0)
                return;

            var tex = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                false);

            tex.hideFlags = HideFlags.HideAndDontSave;

            if (!ImageConversion.LoadImage(tex, data))
            {
                UnityEngine.Object.Destroy(tex);
                return;
            }

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            _previewTexture = tex;
            _previewPath = path;
        }
        catch (Exception ex)
        {
            DestroyPreview();

            Debug.LogError(
                "[MatchReport] EvidenceMenu SelectEvent: " +
                ex.Message);
        }
    }

    private void DestroyPreview()
    {
        if (_previewTexture != null)
        {
            UnityEngine.Object.Destroy(_previewTexture);
            _previewTexture = null;
        }

        _previewPath = null;
        _zoomOpen = false;
        _zoomScale = 1f;
    }

    private static string HumanizeEventName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "Event";

        return value.Replace('_', ' ');
    }
}


[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
internal static class MapEvidence_ShipStartPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            MapEvidenceRecorder.BeginMatch();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class MapEvidence_MurderPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerControl __instance, PlayerControl __0)
    {
        MapEvidenceRecorder.Capture("KILL", __instance, __0);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
internal static class MapEvidence_ShapeshiftPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerControl __instance, PlayerControl __0)
    {
        bool revert = __0 == null || __0.PlayerId == __instance.PlayerId;

        MapEvidenceRecorder.Capture(
            revert ? "SHAPESHIFT_REVERT" : "SHAPESHIFT",
            __instance,
            __0);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckVanish))]
internal static class MapEvidence_VanishPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerControl __instance)
    {
        MapEvidenceRecorder.Capture("VANISH", __instance, null);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckAppear))]
internal static class MapEvidence_AppearPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerControl __instance)
    {
        MapEvidenceRecorder.Capture("APPEAR", __instance, null);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class MapEvidence_EndGamePatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            MapEvidenceRecorder.CompleteMatch();
    }
}
