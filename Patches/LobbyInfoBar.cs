//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Text.RegularExpressions;
//using BepInEx.Configuration;
//using HarmonyLib;
//using TMPro;
//using UnityEngine;

//namespace BanMod
//{
//    public static class LobbyInfoBar
//    {
//        private sealed class Item
//        {
//            public string Id;
//            public TMP_Text Text;
//            public ConfigEntry<bool> LobbyVisible, TaskVisible;
//            public ConfigEntry<string> LobbyColor, TaskColor;
//            public ConfigEntry<bool> LobbyBold, TaskBold;
//            public ConfigEntry<bool> LobbyAutoColor, TaskAutoColor;
//            public ConfigEntry<float> LobbySize, TaskSize;
//            public ConfigEntry<string> LobbyPosition, TaskPosition;
//            public ConfigEntry<bool> Visible => inTaskLayout ? TaskVisible : LobbyVisible;
//            public ConfigEntry<string> Color => inTaskLayout ? TaskColor : LobbyColor;
//            public ConfigEntry<bool> Bold => inTaskLayout ? TaskBold : LobbyBold;
//            public ConfigEntry<bool> AutoColor => inTaskLayout ? TaskAutoColor : LobbyAutoColor;
//            public ConfigEntry<float> Size => inTaskLayout ? TaskSize : LobbySize;
//            public ConfigEntry<string> Position => inTaskLayout ? TaskPosition : LobbyPosition;
//            public bool Positioned;
//            public Vector2 ScreenPosition;
//        }

//        private static readonly List<Item> Items = new List<Item>();
//        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;
//        private static ConfigFile config;
//        private static TMP_Text source;
//        private static PingTracker trackedPing;
//        private static int lastBarFrame = -1;
//        private static TMP_Text timer;
//        private static Item dragging;
//        private static Canvas overlayCanvas;
//        private static Vector2 screenGrabOffset;
//        private static float baseUiFontRatio;
//        private static float smoothedDelta;
//        private static bool inTaskLayout;
//        private static bool IsBarContext => ShipStatus.Instance != null || GameStates.isLobby;

//        private static void HideBar()
//        {
//            foreach (var item in Items)
//                if (item.Text != null) item.Text.gameObject.SetActive(false);
//        }
//        private static ConfigFile lobbyFile, gameFile;
//        private static ConfigFile CurrentFile => inTaskLayout ? gameFile : lobbyFile;
//        private static ConfigEntry<bool> lobbyEnabled, gameEnabled, lobbyDragging, gameDragging, lobbyZeroKicks, gameZeroKicks;
//        private static ConfigEntry<bool> barEnabled => inTaskLayout ? gameEnabled : lobbyEnabled;
//        private static ConfigEntry<bool> allowDragging => inTaskLayout ? gameDragging : lobbyDragging;
//        private static ConfigEntry<bool> showZeroKicks => inTaskLayout ? gameZeroKicks : lobbyZeroKicks;
//        private static InfoBarMiniMenu menu;
//        private static float nextMenuCreate;
//        private static int lastMenuFrame = -1;
//        private static int selected;
//        private static bool editTask;
//        private static string editX = "", editY = "", editColor = "", editSize = "";
//        private static string menuMessage = "";
//        private static string activeField = "";
//        private static string activeSlider = "";

//        private static string CodeLobbyPosition(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return "0.005833328,0.04917556";
//                case "Ping": return "0.4365625,0.06719139";
//                case "Kicks": return "0.043854132,0.969837";
//                case "FPS": return "0.44906253,0.97639215";
//                case "Timer": return "0.005312379,0.909419";
//                default: return "";
//            }
//        }

//        private static string CodeTaskPosition(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return "0.0073958337,0.027437896";
//                case "Ping": return "0.45062497,0.04393828";
//                case "Kicks": return "0.008437365,0.056544393";
//                case "FPS": return "0.4615625,0.95324403";
//                case "Timer": return "";
//                default: return "";
//            }
//        }

//        private static string CodeDefaultPosition(string id, bool inGame) => inGame ? CodeTaskPosition(id) : CodeLobbyPosition(id);

//        private static float CodeLobbySize(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return 250f;
//                case "Ping": return 125f;
//                case "Kicks": return 100f;
//                case "FPS": return 150f;
//                case "Timer": return 250f;
//                default: return 100f;
//            }
//        }

//        private static float CodeTaskSize(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return 75f;
//                case "Ping": return 100f;
//                case "Kicks": return 75f;
//                case "FPS": return 150f;
//                case "Timer": return 250f;
//                default: return 100f;
//            }
//        }

//        private static string CodeColor(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return "#A54AFFFF";
//                case "Ping": return "#FFFFFF";
//                case "Kicks": return "#FFFFFF";
//                case "FPS": return "#B6DF2F";
//                case "Timer": return "#FFFFFFFF";
//                default: return "";
//            }
//        }

//        private static bool CodeBold(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return true;
//                case "Ping": return true;
//                case "Kicks": return false;
//                case "FPS": return true;
//                case "Timer": return true;
//                default: return false;
//            }
//        }

//        private static bool CodeAutoColor(string id)
//        {
//            switch (id)
//            {
//                case "Firma": return false;
//                case "Ping": return true;
//                case "Kicks": return true;
//                case "FPS": return false;
//                case "Timer": return false;
//                default: return false;
//            }
//        }

//        public static void Init(ConfigFile file)
//        {
//            config = file;
//            string folder = Path.GetDirectoryName(file.ConfigFilePath);
//            lobbyFile = new ConfigFile(Path.Combine(folder, "BanMod.InfoBar.Lobby.cfg"), false);
//            gameFile = new ConfigFile(Path.Combine(folder, "BanMod.InfoBar.InGame.cfg"), false);
//            lobbyFile.SaveOnConfigSet = false;
//            gameFile.SaveOnConfigSet = false;
//            lobbyEnabled = lobbyFile.Bind("General", "Enabled", true);
//            gameEnabled = gameFile.Bind("General", "Enabled", true);
//            lobbyDragging = lobbyFile.Bind("General", "AllowDragging", true);
//            gameDragging = gameFile.Bind("General", "AllowDragging", true);
//            lobbyZeroKicks = lobbyFile.Bind("General", "ShowKicksAtZero", false);
//            gameZeroKicks = gameFile.Bind("General", "ShowKicksAtZero", false);
//            lobbyFile.Bind("Profile", "Mode", "Lobby");
//            gameFile.Bind("Profile", "Mode", "InGame");
//            foreach (var item in Items)
//                if (item.Text != null) UnityEngine.Object.Destroy(item.Text.gameObject);
//            Items.Clear();
//            source = null;
//            dragging = null;
//            inTaskLayout = ShipStatus.Instance != null;
//            editTask = inTaskLayout;
//            foreach (string id in new[] { "Firma", "Ping", "Kicks", "FPS", "Timer" })
//            {
//                Items.Add(new Item
//                {
//                    Id = id,
//                    LobbyColor = lobbyFile.Bind(id, "Color", CodeColor(id)),
//                    TaskColor = gameFile.Bind(id, "Color", CodeColor(id)),
//                    LobbyBold = lobbyFile.Bind(id, "Bold", CodeBold(id)),
//                    TaskBold = gameFile.Bind(id, "Bold", CodeBold(id)),
//                    LobbyAutoColor = lobbyFile.Bind(id, "AutomaticColor", CodeAutoColor(id)),
//                    TaskAutoColor = gameFile.Bind(id, "AutomaticColor", CodeAutoColor(id)),
//                    LobbyVisible = lobbyFile.Bind(id, "Visible", true),
//                    TaskVisible = gameFile.Bind(id, "Visible", true),
//                    LobbySize = lobbyFile.Bind(id, "SizePercent", CodeLobbySize(id)),
//                    TaskSize = gameFile.Bind(id, "SizePercent", CodeTaskSize(id)),
//                    LobbyPosition = lobbyFile.Bind(id, "Position", CodeLobbyPosition(id)),
//                    TaskPosition = gameFile.Bind(id, "Position", CodeTaskPosition(id))
//                });
//            }
//            foreach (var item in Items)
//            {
//                item.LobbySize.Value = NormalizeSize(item.LobbySize.Value);
//                item.TaskSize.Value = NormalizeSize(item.TaskSize.Value);
//            }
//            lobbyFile.Save();
//            gameFile.Save();
//            Debug.Log("[BanMod Layout] PROFILE FILES: " + Path.GetFullPath(lobbyFile.ConfigFilePath) +
//                " | " + Path.GetFullPath(gameFile.ConfigFilePath));
//            EnsureMenu();
//            RefreshFields();
//        }

//        public static void CaptureTimer(GameStartManager manager)
//        {
//            if (config == null) return;
//            if (timer == null)
//            {
//                var target = manager.transform.Find(
//                    "StartGameArea/StartButton (Host)/LobbyTimer");
//                if (target != null) timer = target.GetComponent<TMP_Text>();
//            }
//            if (timer != null) timer.enabled = false;
//        }

//        public static void UpdateBar(PingTracker tracker)
//        {
//            if (tracker == null) return;
//            trackedPing = tracker;
//            if (lastBarFrame == Time.frameCount) return;
//            lastBarFrame = Time.frameCount;
//            MenuTick();
//            if (config == null && BanMod.Instance != null)
//                Init(BanMod.Instance.Config);
//            if (!IsBarContext)
//            {
//                HideBar();
//                return;
//            }
//            var client = AmongUsClient.Instance;
//            if (config == null || tracker.text == null || client == null) return;
//            bool taskNow = ShipStatus.Instance != null;
//            if (taskNow != inTaskLayout)
//            {
//                SaveDraggedPosition();
//                inTaskLayout = taskNow;
//                CurrentFile.Reload();
//                editTask = taskNow;
//                activeField = "";
//                activeSlider = "";
//                foreach (var item in Items)
//                {
//                    item.Positioned = false;
//                    if (TryReadPosition(item.Position.Value, out Vector2 saved))
//                    {
//                        item.ScreenPosition = saved;
//                        item.Positioned = true;
//                    }
//                }
//                RefreshFields();
//                Debug.Log("[BanMod Layout] Position profile: " +
//                    (inTaskLayout ? "InTask" : "Lobby"));
//            }
//            if (source != tracker.text || Items[0].Text == null) CreateItems(tracker.text);
//            tracker.text.enabled = false;

//            smoothedDelta = Mathf.Lerp(smoothedDelta > 0f ? smoothedDelta :
//                Time.unscaledDeltaTime, Time.unscaledDeltaTime, 0.1f);
//            int ping = client.Ping;
//            int fps = Mathf.RoundToInt(1f / Mathf.Max(smoothedDelta, 0.0001f));
//            int votes = client.AmHost ? 0 : VoteBanTracker.GetVoteCount(client.ClientId);
//            string pingColor = ping <= 100 ? "#00FF00" : ping < 400 ? "#FFFF00" : "#FF0000";
//            string fpsColor = fps < 20 ? "#FF0000" : fps < 40 ? "#FFFF00" : "#00FF00";
//            string voteColor = votes >= 2 && Mathf.FloorToInt(Time.time * 4f) % 2 == 0
//                ? "#FF0000" : "#FFFF00";

//            SetText(0, $"<color={BanMod.ModColor}>BanMod</color> " +
//                "<color=#A54AFF>By</color> <color=#F34C50>Bart</color>");
//            SetText(1, $"Ping: {ping} ms", true, pingColor);
//            SetText(2, $"Kicks: {votes}/3", votes > 0 || showZeroKicks.Value, voteColor);
//            SetText(3, $"FPS: {fps}", BanMod.ShowFPS != null && BanMod.ShowFPS.Value, fpsColor);
//            bool hasTimer = ShipStatus.Instance == null && timer != null && timer.gameObject.activeInHierarchy;
//            string remaining = hasTimer ? Regex.Replace(timer.text ?? "", "<[^>]*>", "").Trim() : "";
//            SetText(4, remaining, hasTimer && remaining.Length > 0);

//            PlaceUnpositionedItems();
//            HandleDrag();
//        }

//        public static void RefreshBar()
//        {
//            if (trackedPing != null) UpdateBar(trackedPing);
//        }

//        private static void CreateItems(TMP_Text original)
//        {
//            if (overlayCanvas != null)
//                UnityEngine.Object.Destroy(overlayCanvas.gameObject);
//            foreach (var item in Items)
//                if (item.Text != null) UnityEngine.Object.Destroy(item.Text.gameObject);
//            source = original;
//            dragging = null;
//            var root = new GameObject("BanModInfoBarOverlay");
//            overlayCanvas = root.AddComponent<Canvas>();
//            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//            overlayCanvas.sortingOrder = 32760;
//            UnityEngine.Object.DontDestroyOnLoad(root);
//            foreach (var item in Items)
//            {
//                var obj = new GameObject("BanModInfo_" + item.Id);
//                obj.transform.SetParent(root.transform, false);
//                var copy = obj.AddComponent<TextMeshProUGUI>();
//                copy.font = original.font;
//                copy.fontSharedMaterial = original.fontSharedMaterial;
//                copy.color = Color.white;
//                copy.richText = true;
//                copy.raycastTarget = false;
//                copy.rectTransform.anchorMin = Vector2.zero;
//                copy.rectTransform.anchorMax = Vector2.zero;
//                copy.rectTransform.pivot = new Vector2(0f, 0.5f);
//                copy.rectTransform.sizeDelta = new Vector2(Screen.width * 2f, 200f);
//                copy.transform.localScale = Vector3.one;
//                copy.enabled = true;
//                copy.enableAutoSizing = false;
//                copy.alignment = TextAlignmentOptions.MidlineLeft;
//                copy.enableWordWrapping = false;
//                copy.overflowMode = TextOverflowModes.Overflow;
//                copy.fontSize = 100f;
//                if (baseUiFontRatio <= 0f)
//                {
//                    Vector2 uiMeasure = copy.GetPreferredValues("Ag");
//                    Vector2 sourceMeasure = original.GetPreferredValues("Ag");
//                    Camera cam = Camera.main;
//                    float pixelWidth;
//                    if (original.GetComponent<CanvasRenderer>() != null)
//                    {
//                        pixelWidth = sourceMeasure.x * Mathf.Abs(original.transform.lossyScale.x);
//                    }
//                    else if (cam != null)
//                    {
//                        Vector3 start = cam.WorldToScreenPoint(original.transform.position);
//                        Vector3 end = cam.WorldToScreenPoint(original.transform.TransformPoint(new Vector3(sourceMeasure.x, 0f, 0f)));
//                        pixelWidth = Vector2.Distance(new Vector2(start.x, start.y), new Vector2(end.x, end.y));
//                    }
//                    else pixelWidth = uiMeasure.x * 0.2f;
//                    baseUiFontRatio = Mathf.Max(1f, 100f * pixelWidth / Mathf.Max(uiMeasure.x, 0.001f)) / Mathf.Max(1, Screen.height);
//                }
//                copy.text = "";
//                item.Text = copy;
//                item.Positioned = TryReadPosition(item.Position.Value, out Vector2 position);
//                item.ScreenPosition = position;
//            }
//            Debug.Log("[BanMod Layout] Screen-space overlay created. Profile=" +
//                (inTaskLayout ? "InGame" : "Lobby"));
//        }

//        private static void SetText(int index, string value, bool available = true, string automaticColor = null)
//        {
//            var item = Items[index];
//            item.Text.gameObject.SetActive(IsBarContext && barEnabled.Value && (item.Id == "Firma" || item.Visible.Value) && available);
//            string color = item.AutoColor.Value && automaticColor != null ? automaticColor : item.Color.Value;
//            if (!ColorUtility.TryParseHtmlString(color, out _)) color = "#FFFFFF";
//            float size = NormalizeSize(item.Size.Value);
//            item.Text.enableAutoSizing = false;
//            item.Text.fontSize = baseUiFontRatio * Screen.height * size / 100f;
//            string content = value.Replace("\r", "").Replace("\n", " ");
//            if (item.Bold.Value) content = "<b>" + content + "</b>";
//            item.Text.text = "<color=" + color + ">" + content + "</color>";
//        }

//        private static Vector2 TextAnchorOffset(Item item)
//        {
//            Bounds bounds = item.Text.textBounds;
//            return new Vector2(bounds.min.x, bounds.center.y);
//        }

//        private static void ApplyScreenPosition(Item item)
//        {
//            item.Text.rectTransform.anchoredPosition = new Vector2(
//                item.ScreenPosition.x * Screen.width, item.ScreenPosition.y * Screen.height) - TextAnchorOffset(item);
//        }

//        private static void PlaceUnpositionedItems()
//        {
//            if (overlayCanvas == null) return;
//            float cursor = Screen.width * 0.03f;
//            foreach (var item in Items)
//            {
//                if (item.Text == null || !item.Text.gameObject.activeInHierarchy) continue;
//                item.Text.fontSize = baseUiFontRatio * Screen.height * NormalizeSize(item.Size.Value) / 100f;
//                item.Text.rectTransform.sizeDelta = new Vector2(Screen.width * 2f, Screen.height);
//                item.Text.ForceMeshUpdate();
//                if (dragging != item && TryReadPosition(item.Position.Value, out Vector2 configured))
//                {
//                    item.ScreenPosition = configured;
//                    item.Positioned = true;
//                }
//                if (!item.Positioned)
//                {
//                    item.ScreenPosition = new Vector2(cursor / Screen.width, 0.055f);
//                    item.Positioned = true;
//                }
//                ApplyScreenPosition(item);
//                cursor += item.Text.textBounds.size.x + 12f;
//            }
//        }

//        private static void HandleDrag()
//        {
//            if (!allowDragging.Value || !barEnabled.Value)
//            { SaveDraggedPosition(); return; }
//            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
//            Vector3 mouse3 = Input.mousePosition;
//            Vector2 mouse = new Vector2(mouse3.x, mouse3.y);
//            if (dragging == null && alt && Input.GetMouseButtonDown(0) && !MouseOverMenu())
//            {
//                for (int i = Items.Count - 1; i >= 0; i--)
//                {
//                    var item = Items[i];
//                    if (item.Text == null || !item.Text.gameObject.activeInHierarchy) continue;
//                    Bounds b = item.Text.textBounds;
//                    Vector2 pivot = item.Text.rectTransform.anchoredPosition;
//                    Rect hit = new Rect(pivot.x + b.min.x - 6f, pivot.y + b.min.y - 6f,
//                        b.size.x + 12f, b.size.y + 12f);
//                    if (!hit.Contains(mouse)) continue;
//                    dragging = item;
//                    screenGrabOffset = new Vector2(item.ScreenPosition.x * Screen.width,
//                        item.ScreenPosition.y * Screen.height) - mouse;
//                    break;
//                }
//            }
//            if (dragging == null) return;
//            Vector2 anchor = mouse + screenGrabOffset;
//            dragging.ScreenPosition = new Vector2(anchor.x / Screen.width, anchor.y / Screen.height);
//            dragging.Positioned = true;
//            ApplyScreenPosition(dragging);
//            if (Input.GetMouseButtonUp(0) || !alt || !Input.GetMouseButton(0)) SaveDraggedPosition();
//        }

//        private static void SaveDraggedPosition()
//        {
//            if (dragging == null) return;
//            dragging.Position.Value = string.Format(CI, "{0:R},{1:R}", dragging.ScreenPosition.x, dragging.ScreenPosition.y);
//            Debug.Log(string.Format(CI, "[BanMod Layout] SCREEN [{0}] {1}: {2}",
//                inTaskLayout ? "InGame" : "Lobby", dragging.Id, dragging.Position.Value));
//            dragging = null;
//            PersistAndVerifyProfile();
//        }

//        private static Rect windowRect = new Rect(20f, 20f, 610f, 690f);
//        private static GUIStyle menuButton, menuTitle, menuLabel, menuField, previewStyle;
//        private static float MenuScale => Mathf.Min(1f, Mathf.Min(Screen.width / 650f, Screen.height / 730f));
//        private static Rect MenuRect => new Rect(windowRect.x * MenuScale, windowRect.y * MenuScale,
//            windowRect.width * MenuScale, windowRect.height * MenuScale);

//        private static bool MouseOverMenu()
//        {
//            Vector3 mouse = Input.mousePosition;
//            return InfoBarMiniMenu.Open && MenuRect.Contains(new Vector2(mouse.x, Screen.height - mouse.y));
//        }

//        private static void EnsureMenu()
//        {
//            if (menu != null || BanMod.Instance == null || Time.realtimeSinceStartup < nextMenuCreate) return;
//            nextMenuCreate = Time.realtimeSinceStartup + 5f;
//            try
//            {
//                menu = BanMod.Instance.AddComponent<InfoBarMiniMenu>();
//                if (menu != null) menu.enabled = true;
//                Debug.Log("[BanMod Layout] Menu component: " + (menu != null ? "OK" : "MISSING"));
//            }
//            catch (Exception ex) { Debug.LogError("[BanMod Layout] Menu creation failed: " + ex); }
//        }

//        public static void MenuTick()
//        {
//            if (lastMenuFrame == Time.frameCount) return;
//            lastMenuFrame = Time.frameCount;
//            if (config == null && BanMod.Instance != null) Init(BanMod.Instance.Config);
//            if (config == null) return;
//            EnsureMenu();
//            if (menu != null) { menu.gameObject.SetActive(true); menu.enabled = true; }
//            if (InfoBarMiniMenu.Open) HandleFieldInput();
//            if (!Input.GetKeyDown(KeyCode.F12) || BanMod.IsBanModDisabled) return;
//            InfoBarMiniMenu.Open = !InfoBarMiniMenu.Open;
//            if (InfoBarMiniMenu.Open) OpenMenu();
//            Debug.Log("[BanMod Layout] F12: menu " + (InfoBarMiniMenu.Open ? "OPEN" : "CLOSED"));
//        }

//        public static void OpenMenu()
//        {
//            activeField = "";
//            activeSlider = "";
//            editTask = inTaskLayout;
//            float scale = MenuScale;
//            windowRect.x = Mathf.Max(0f, (Screen.width / scale - windowRect.width) / 2f);
//            windowRect.y = Mathf.Max(0f, (Screen.height / scale - windowRect.height) / 2f);
//            RefreshFields();
//        }

//        private static ConfigEntry<string> EditedPosition(Item item) => editTask ? item.TaskPosition : item.LobbyPosition;
//        private static ConfigEntry<float> EditedSize(Item item) => editTask ? item.TaskSize : item.LobbySize;
//        private static float NormalizeSize(float value) => Mathf.Clamp(Mathf.Round(value / 25f) * 25f, 25f, 250f);
//        private static string DisplayName(Item item) => item.Id == "Firma" ? "BanMod By Bart" : item.Id;

//        private static void RefreshFields()
//        {
//            if (Items.Count == 0) return;
//            activeField = "";
//            var item = Items[selected];
//            if (!TryReadPosition(EditedPosition(item).Value, out Vector2 pos))
//                pos = editTask == inTaskLayout && item.Positioned ? item.ScreenPosition : new Vector2(0.05f, 0.055f);
//            editX = (pos.x * 100f).ToString("0.###", CI);
//            editY = (pos.y * 100f).ToString("0.###", CI);
//            editColor = item.Color.Value;
//            editSize = EditedSize(item).Value.ToString("0.##", CI);
//            menuMessage = "";
//        }

//        private static void EnsureStyles()
//        {
//            if (menuButton != null) return;
//            menuButton = new GUIStyle(BanModUiStyles.ButtonDark)
//            { fontSize = 17, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
//            menuTitle = new GUIStyle(BanModUiStyles.TitleLabel)
//            { fontSize = 23, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
//            menuTitle.normal.textColor = BanModUiStyles.AccentColor;
//            menuLabel = new GUIStyle(BanModUiStyles.BodyLabel)
//            { fontSize = 16, alignment = TextAnchor.MiddleLeft, wordWrap = true };
//            menuField = new GUIStyle(BanModUiStyles.DarkBox)
//            { fontSize = 17, alignment = TextAnchor.MiddleLeft };
//            menuField.normal.textColor = Color.white;
//            previewStyle = new GUIStyle(menuTitle) { fontSize = 28, richText = true };
//        }

//        private static bool Button(float x, float y, float w, string label)
//            => GUI.Button(new Rect(x, y, w, 32f), label, menuButton);
//        private static bool Option(float x, float y, float w, string label, bool value)
//            => Button(x, y, w, label + (value ? ": ON" : ": OFF")) ? !value : value;
//        private static void Label(float x, float y, float w, string label)
//            => GUI.Label(new Rect(x, y, w, 27f), label, menuLabel);
//        private static string Field(float x, float y, float w, string value, string id)
//        {
//            bool editing = activeField == id;
//            string caret = editing && Mathf.FloorToInt(Time.unscaledTime * 2f) % 2 == 0 ? "|" : "";
//            Color previous = GUI.backgroundColor;
//            if (editing) GUI.backgroundColor = BanModUiStyles.AccentColor;
//            if (GUI.Button(new Rect(x, y, w, 30f), value + caret, menuField))
//            {
//                activeField = id;
//                activeSlider = "";
//            }
//            GUI.backgroundColor = previous;
//            return value;
//        }

//        private static void HandleFieldInput()
//        {
//            if (string.IsNullOrEmpty(activeField)) return;
//            string value = activeField == "size" ? editSize : activeField == "color" ? editColor :
//                activeField == "x" ? editX : editY;
//            bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
//            if (control && Input.GetKeyDown(KeyCode.A)) value = "";
//            bool backspaceHandled = false;
//            foreach (char c in Input.inputString ?? "")
//            {
//                if (c == '\b')
//                {
//                    if (value.Length > 0) value = value.Substring(0, value.Length - 1);
//                    backspaceHandled = true;
//                    continue;
//                }
//                if (c == '\n' || c == '\r') continue;
//                string allowed = activeField == "color" ? "#0123456789abcdefABCDEF" : "0123456789.,-";
//                if (allowed.IndexOf(c) >= 0 && value.Length < 20 && !control) value += c;
//            }
//            if (!backspaceHandled && Input.GetKeyDown(KeyCode.Backspace) && value.Length > 0)
//                value = value.Substring(0, value.Length - 1);
//            if (activeField == "size") editSize = value;
//            else if (activeField == "color") editColor = value;
//            else if (activeField == "x") editX = value;
//            else editY = value;
//            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
//                Input.GetKeyDown(KeyCode.Escape)) activeField = "";
//        }

//        private static float ManualSlider(Rect rect, float value, float min, float max)
//        {
//            string id = rect.x.ToString(CI) + ":" + rect.y.ToString(CI);
//            Event e = Event.current;
//            bool mouseDown = e.type == EventType.MouseDown && e.button == 0;
//            if (mouseDown && rect.Contains(e.mousePosition))
//            {
//                activeSlider = id;
//                activeField = "";
//            }
//            if (activeSlider == id && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
//            {
//                value = Mathf.Lerp(min, max, Mathf.Clamp01((e.mousePosition.x - rect.x) / rect.width));
//                e.Use();
//            }
//            if (activeSlider == id && (e.type == EventType.MouseUp || !Input.GetMouseButton(0)))
//                activeSlider = "";
//            GUI.Box(new Rect(rect.x, rect.y + 6f, rect.width, 8f), "", BanModUiStyles.DarkBox);
//            float fraction = Mathf.InverseLerp(min, max, value);
//            Color previous = GUI.backgroundColor;
//            GUI.backgroundColor = BanModUiStyles.AccentColor;
//            GUI.Box(new Rect(rect.x + fraction * (rect.width - 14f), rect.y, 14f, 20f), "", menuButton);
//            GUI.backgroundColor = previous;
//            return value;
//        }

//        public static void DrawMenu()
//        {
//            if (config == null || Items.Count == 0 || BanMod.IsBanModDisabled) return;
//            EnsureStyles();
//            Matrix4x4 oldMatrix = GUI.matrix;
//            Color previousGuiColor = GUI.color;
//            try
//            {
//                GUI.color = Color.white;
//                GUI.matrix = Matrix4x4.Scale(new Vector3(MenuScale, MenuScale, 1f));
//                windowRect = GUI.Window(812453, windowRect, (GUI.WindowFunction)DrawStyledWindow,
//                    "", BanModUiStyles.BlackWindow);
//            }
//            finally { GUI.matrix = oldMatrix; GUI.color = previousGuiColor; }
//        }

//        private static bool windowErrorLogged;

//        private static void DrawStyledWindow(int id)
//        {
//            try { DrawStyledWindowContent(id); }
//            catch (Exception ex)
//            {
//                if (!windowErrorLogged)
//                {
//                    windowErrorLogged = true;
//                    Debug.LogError("[BanMod Layout] Window rendering failed: " + ex);
//                }
//            }
//        }

//        private static void DrawStyledWindowContent(int id)
//        {
//            editTask = inTaskLayout;
//            GUI.Label(new Rect(20f, 12f, 520f, 34f),
//                "INFO BAR / " + (inTaskLayout ? "IN GAME" : "LOBBY"), menuTitle);
//            if (Button(552f, 12f, 40f, "X")) InfoBarMiniMenu.Open = false;
//            GUI.Box(new Rect(16f, 58f, 210f, 544f), "", BanModUiStyles.DarkBox);
//            GUI.Box(new Rect(238f, 58f, 356f, 544f), "", BanModUiStyles.DarkBox);
//            GUI.Label(new Rect(26f, 68f, 190f, 30f), "ELEMENTS", menuTitle);
//            for (int i = 0; i < Items.Count; i++)
//                if (Button(28f, 108f + i * 42f, 186f, (selected == i ? "> " : "") + DisplayName(Items[i])))
//                { selected = i; RefreshFields(); }
//            barEnabled.Value = Option(28f, 416f, 186f, "Info bar", barEnabled.Value);
//            allowDragging.Value = Option(28f, 458f, 186f, "Alt + drag", allowDragging.Value);
//            showZeroKicks.Value = Option(28f, 500f, 186f, "Kicks at zero", showZeroKicks.Value);
//            Label(28f, 546f, 182f, "F12: open / close");

//            var item = Items[selected];
//            GUI.Label(new Rect(250f, 68f, 332f, 30f), "STYLE / " + DisplayName(item), menuTitle);
//            if (item.Id != "Firma")
//                item.Visible.Value = Option(252f, 108f, 154f, "Visible", item.Visible.Value);
//            else Label(252f, 110f, 160f, "Original colors");
//            item.Bold.Value = Option(418f, 108f, 160f, "Bold", item.Bold.Value);
//            Label(252f, 149f, 320f, "SIZE (%)");
//            for (int i = 0; i < 10; i++)
//            {
//                int size = (i + 1) * 25;
//                string caption = (Mathf.Approximately(EditedSize(item).Value, size) ? ">" : "") + size + "%";
//                if (Button(252f + (i % 5) * 66f, 181f + (i / 5) * 38f, 62f, caption))
//                {
//                    EditedSize(item).Value = size;
//                    menuMessage = "Size changed. Use SAVE PROFILE to save all settings.";
//                }
//            }
//            if (item.Id == "Firma")
//            {
//                GUI.Label(new Rect(254f, 282f, 322f, 90f),
//                    "BanMod By Bart is a single element.\nIts original colors are fixed.\nMove with Alt + drag. Adjust bold and size here.", menuLabel);
//                previewStyle.normal.textColor = Color.white;
//                previewStyle.fontStyle = item.Bold.Value ? FontStyle.Bold : FontStyle.Normal;
//                previewStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(20f * EditedSize(item).Value / 100f), 8, 40);
//                GUI.Label(new Rect(252f, 403f, 326f, 100f),
//                    $"<color={BanMod.ModColor}>BanMod</color> <color=#A54AFF>By</color> <color=#F34C50>Bart</color>", previewStyle);
//            }
//            else
//            {
//                DrawColorPicker(item);
//                ColorUtility.TryParseHtmlString(item.Color.Value, out Color previewColor);
//                previewStyle.normal.textColor = previewColor;
//                previewStyle.fontStyle = item.Bold.Value ? FontStyle.Bold : FontStyle.Normal;
//                previewStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(20f * EditedSize(item).Value / 100f), 8, 40);
//                GUI.Label(new Rect(252f, 553f, 326f, 44f), PreviewText(item), previewStyle);
//            }

//            if (Button(20f, 615f, 180f, "SAVE PROFILE")) SaveCurrentProfile();
//            if (Button(215f, 615f, 180f, "RELOAD PROFILE")) ReloadCurrentProfile();
//            if (Button(410f, 615f, 180f, "CLOSE")) InfoBarMiniMenu.Open = false;
//            GUI.Label(new Rect(22f, 655f, 565f, 28f), menuMessage, menuLabel);
//            GUI.DragWindow(new Rect(0f, 0f, 545f, 52f));
//        }


//        private static Texture2D colorSquare, hueStrip;
//        private static float renderedHue = -1f, pickerHue, pickerSaturation, pickerValue;
//        private static string pickerItem = "", pickerHex = "";

//        private static readonly Dictionary<int, GUIStyle> pickerTextureStyles = new Dictionary<int, GUIStyle>();

//        private static void DrawPickerTexture(Rect rect, Texture2D texture)
//        {
//            if (texture == null || Event.current.type != EventType.Repaint) return;
//            int key = texture.GetInstanceID();
//            if (!pickerTextureStyles.TryGetValue(key, out GUIStyle style))
//            {
//                style = new GUIStyle();
//                style.normal.background = texture;
//                style.hover.background = texture;
//                style.active.background = texture;
//                style.focused.background = texture;
//                style.border = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
//                style.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
//                pickerTextureStyles[key] = style;
//            }
//            Color oldBackground = GUI.backgroundColor;
//            GUI.backgroundColor = Color.white;
//            GUI.Box(rect, "", style);
//            GUI.backgroundColor = oldBackground;
//        }

//        private static void DrawColorPicker(Item item)
//        {
//            Color pickerOldColor = GUI.color;
//            Color pickerOldBackground = GUI.backgroundColor;
//            GUI.color = Color.white;
//            GUI.backgroundColor = Color.white;
//            try { DrawColorPickerContent(item); }
//            finally
//            {
//                GUI.color = pickerOldColor;
//                GUI.backgroundColor = pickerOldBackground;
//            }
//        }

//        private static void DrawColorPickerContent(Item item)
//        {
//            if (pickerItem != item.Id || pickerHex != item.Color.Value)
//            {
//                if (!ColorUtility.TryParseHtmlString(item.Color.Value, out Color original)) original = Color.white;
//                Color.RGBToHSV(original, out pickerHue, out pickerSaturation, out pickerValue);
//                pickerItem = item.Id; pickerHex = item.Color.Value;
//            }
//            Rect square = new Rect(254f, 270f, 220f, 190f);
//            Rect hue = new Rect(488f, 270f, 28f, 190f);
//            Event e = Event.current;
//            if (e.type == EventType.MouseDown && e.button == 0)
//            {
//                if (square.Contains(e.mousePosition)) activeSlider = "color-square";
//                else if (hue.Contains(e.mousePosition)) activeSlider = "color-hue";
//            }
//            bool changed = false;
//            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
//            {
//                if (activeSlider == "color-square")
//                {
//                    pickerSaturation = Mathf.Clamp01((e.mousePosition.x - square.x) / square.width);
//                    pickerValue = 1f - Mathf.Clamp01((e.mousePosition.y - square.y) / square.height);
//                    activeField = ""; changed = true; e.Use();
//                }
//                else if (activeSlider == "color-hue")
//                {
//                    pickerHue = 1f - Mathf.Clamp01((e.mousePosition.y - hue.y) / hue.height);
//                    activeField = ""; changed = true; e.Use();
//                }
//            }
//            if (e.type == EventType.MouseUp || !Input.GetMouseButton(0))
//                if (activeSlider == "color-square" || activeSlider == "color-hue") activeSlider = "";
//            if (changed)
//            {
//                Color chosen = Color.HSVToRGB(pickerHue, pickerSaturation, pickerValue);
//                item.Color.Value = "#" + ColorUtility.ToHtmlStringRGB(chosen);
//                pickerHex = item.Color.Value; editColor = pickerHex; item.AutoColor.Value = false;
//            }
//            if (colorSquare == null)
//            {
//                colorSquare = new Texture2D(64, 64, TextureFormat.RGBA32, false);
//                colorSquare.hideFlags = HideFlags.HideAndDontSave;
//                colorSquare.wrapMode = TextureWrapMode.Clamp;
//                hueStrip = new Texture2D(1, 128, TextureFormat.RGBA32, false);
//                hueStrip.hideFlags = HideFlags.HideAndDontSave;
//                hueStrip.wrapMode = TextureWrapMode.Clamp;
//                for (int y = 0; y < 128; y++) hueStrip.SetPixel(0, y, Color.HSVToRGB(y / 127f, 1f, 1f));
//                hueStrip.Apply(); renderedHue = -1f;
//            }
//            if (Mathf.Abs(renderedHue - pickerHue) > .001f)
//            {
//                for (int y = 0; y < 64; y++)
//                    for (int x = 0; x < 64; x++)
//                        colorSquare.SetPixel(x, y, Color.HSVToRGB(pickerHue, x / 63f, y / 63f));
//                colorSquare.Apply(); renderedHue = pickerHue;
//            }
//            DrawPickerTexture(square, colorSquare);
//            DrawPickerTexture(hue, hueStrip);
//            Color previous = GUI.color;
//            GUI.color = Color.black;
//            Rect marker = new Rect(square.x + pickerSaturation * square.width - 4f,
//                square.y + (1f - pickerValue) * square.height - 4f, 8f, 8f);
//            DrawPickerTexture(marker, Texture2D.whiteTexture);
//            GUI.color = Color.white;
//            DrawPickerTexture(new Rect(marker.x + 2f, marker.y + 2f, 4f, 4f), Texture2D.whiteTexture);
//            DrawPickerTexture(new Rect(hue.x - 3f, hue.y + (1f - pickerHue) * hue.height - 2f,
//                hue.width + 6f, 4f), Texture2D.whiteTexture);
//            GUI.color = Color.HSVToRGB(pickerHue, pickerSaturation, pickerValue);
//            DrawPickerTexture(new Rect(530f, 270f, 46f, 46f), Texture2D.whiteTexture);
//            GUI.color = previous;
//            Label(526f, 323f, 56f, "Hue");
//            editColor = Field(252f, 475f, 200f, editColor, "color");
//            if (Button(466f, 475f, 112f, "Apply HEX"))
//            {
//                string hex = editColor.Trim(); if (!hex.StartsWith("#")) hex = "#" + hex;
//                if (ColorUtility.TryParseHtmlString(hex, out _))
//                { item.Color.Value = hex; item.AutoColor.Value = false; menuMessage = "Color saved."; }
//                else menuMessage = "Use #RRGGBB or #RRGGBBAA.";
//            }
//            if (item.Id == "Ping" || item.Id == "FPS" || item.Id == "Kicks")
//                item.AutoColor.Value = Option(252f, 515f, 326f, "Automatic status color", item.AutoColor.Value);
//        }

//        private static string PreviewText(Item item)
//        {
//            if (item.Id == "Ping") return "Ping: 27 ms";
//            if (item.Id == "Kicks") return "Kicks: 1/3";
//            if (item.Id == "FPS") return "FPS: 60";
//            if (item.Id == "Timer") return "09:59";
//            return DisplayName(item);
//        }

//        private static void SaveCurrentProfile()
//        {
//            SaveDraggedPosition();
//            foreach (var item in Items)
//            {
//                if (!item.Positioned) continue;
//                item.Position.Value = string.Format(CI, "{0:R},{1:R}", item.ScreenPosition.x, item.ScreenPosition.y);
//            }
//            PersistAndVerifyProfile();
//        }

//        private static bool PersistAndVerifyProfile()
//        {
//            string path = CurrentFile.ConfigFilePath;
//            try
//            {
//                CurrentFile.Save();
//                var disk = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
//                string section = "";
//                foreach (string raw in File.ReadAllLines(path))
//                {
//                    string line = raw.Trim();
//                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
//                    if (line.StartsWith("[") && line.EndsWith("]"))
//                    { section = line.Substring(1, line.Length - 2); continue; }
//                    int equals = line.IndexOf('=');
//                    if (equals < 0) continue;
//                    disk[section + "/" + line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
//                }
//                foreach (var item in Items)
//                {
//                    VerifySaved(disk, item.Id + "/Position", item.Position.Value);
//                    VerifySaved(disk, item.Id + "/Color", item.Color.Value);
//                    VerifySaved(disk, item.Id + "/Bold", item.Bold.Value.ToString());
//                    VerifySaved(disk, item.Id + "/Visible", item.Visible.Value.ToString());
//                    VerifySaved(disk, item.Id + "/AutomaticColor", item.AutoColor.Value.ToString());
//                    if (!disk.TryGetValue(item.Id + "/SizePercent", out string size) ||
//                        !ReadNumber(size, out float parsed) || !Mathf.Approximately(parsed, item.Size.Value))
//                        throw new IOException("Size verification failed for " + item.Id);
//                }
//                VerifySaved(disk, "General/Enabled", barEnabled.Value.ToString());
//                VerifySaved(disk, "General/AllowDragging", allowDragging.Value.ToString());
//                VerifySaved(disk, "General/ShowKicksAtZero", showZeroKicks.Value.ToString());
//                menuMessage = "SAVED AND VERIFIED: " + Path.GetFileName(path);
//                Debug.Log("[BanMod Layout] SAVED AND VERIFIED: " + Path.GetFullPath(path));
//                foreach (var item in Items)
//                    Debug.Log(string.Format(CI,
//                        "[BanMod Layout] DISK [{0}] {1}: position={2}; size={3}%; color={4}; bold={5}; visible={6}",
//                        inTaskLayout ? "InGame" : "Lobby", item.Id, item.Position.Value,
//                        item.Size.Value, item.Color.Value, item.Bold.Value, item.Visible.Value));
//                return true;
//            }
//            catch (Exception ex)
//            {
//                menuMessage = "SAVE FAILED - see log";
//                Debug.LogError("[BanMod Layout] SAVE FAILED: " + path + "\n" + ex);
//                return false;
//            }
//        }

//        private static void VerifySaved(Dictionary<string, string> disk, string key, string expected)
//        {
//            if (!disk.TryGetValue(key, out string actual) ||
//                !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
//                throw new IOException("Disk verification failed: " + key + "; expected=" + expected + "; actual=" + actual);
//        }

//        public static void ReapplyLayout()
//        {
//            if (!IsBarContext)
//            {
//                HideBar();
//                return;
//            }
//            if (source != null && Items.Count > 0 && Items[0].Text != null)
//                PlaceUnpositionedItems();
//        }

//        private static void ReloadCurrentProfile()
//        {
//            dragging = null;
//            CurrentFile.Reload();
//            foreach (var item in Items)
//            {
//                item.Positioned = TryReadPosition(item.Position.Value, out Vector2 position);
//                item.ScreenPosition = position;
//            }
//            RefreshFields();
//            menuMessage = "Loaded: " + Path.GetFileName(CurrentFile.ConfigFilePath);
//        }

//        private static bool ReadNumber(string value, out float number)
//        {
//            return float.TryParse(value.Trim().Replace(',', '.'), NumberStyles.Float, CI, out number)
//                && !float.IsNaN(number) && !float.IsInfinity(number);
//        }

//        private static bool TryReadPosition(string value, out Vector2 position)
//        {
//            position = default;
//            string[] parts = value.Split(',');
//            if (parts.Length != 2) return false;
//            if (!float.TryParse(parts[0], NumberStyles.Float, CI, out float x) ||
//                !float.TryParse(parts[1], NumberStyles.Float, CI, out float y)) return false;
//            if (float.IsNaN(x) || float.IsNaN(y) ||
//                float.IsInfinity(x) || float.IsInfinity(y)) return false;
//            position = new Vector2(x, y);
//            return true;
//        }
//    }

//    public class InfoBarMiniMenu : MonoBehaviour
//    {
//        public InfoBarMiniMenu(IntPtr ptr) : base(ptr) { }
//        public static bool Open;
//        private bool guiErrorLogged;
//        public void LateUpdate()
//        {
//            LobbyInfoBar.ReapplyLayout();
//        }
//        public void OnGUI()
//        {
//            if (!Open) return;
//            try
//            {
//                GUI.depth = -10000;
//                LobbyInfoBar.DrawMenu();
//            }
//            catch (Exception ex)
//            {
//                if (!guiErrorLogged)
//                {
//                    guiErrorLogged = true;
//                    Debug.LogError("[BanMod Layout] Menu GUI error: " + ex);
//                }
//            }
//        }
//    }

//    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
//    public static class InfoBarMenuKeyboardPatch
//    {
//        private static void Postfix()
//        {
//            LobbyInfoBar.MenuTick();
//            LobbyInfoBar.RefreshBar();
//        }
//    }

//    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
//    public static class PingTracker_InfoBar
//    {
//        private static void Postfix(PingTracker __instance)
//        {
//            LobbyInfoBar.UpdateBar(__instance);
//        }
//    }

//    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
//    public static class GameStartManager_InfoTimer
//    {
//        private static void Postfix(GameStartManager __instance)
//        {
//            LobbyInfoBar.CaptureTimer(__instance);
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BanMod
{
    public static class LobbyInfoBar
    {
        private sealed class Item
        {
            public string Id;
            public TMP_Text Text;
            public ConfigEntry<bool> LobbyVisible, TaskVisible;
            public ConfigEntry<string> LobbyColor, TaskColor;
            public ConfigEntry<bool> LobbyBold, TaskBold;
            public ConfigEntry<bool> LobbyAutoColor, TaskAutoColor;
            public ConfigEntry<float> LobbySize, TaskSize;
            public ConfigEntry<string> LobbyPosition, TaskPosition;
            public ConfigEntry<bool> Visible => inTaskLayout ? TaskVisible : LobbyVisible;
            public ConfigEntry<string> Color => inTaskLayout ? TaskColor : LobbyColor;
            public ConfigEntry<bool> Bold => inTaskLayout ? TaskBold : LobbyBold;
            public ConfigEntry<bool> AutoColor => inTaskLayout ? TaskAutoColor : LobbyAutoColor;
            public ConfigEntry<float> Size => inTaskLayout ? TaskSize : LobbySize;
            public ConfigEntry<string> Position => inTaskLayout ? TaskPosition : LobbyPosition;
            public bool Positioned;
            public Vector2 ScreenPosition;
        }

        private static readonly List<Item> Items = new List<Item>();
        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;
        private static ConfigFile config;
        private static TMP_Text source;
        private static PingTracker trackedPing;
        private static int lastBarFrame = -1;
        private static TMP_Text timer;
        private static Item dragging;
        private static Canvas overlayCanvas;
        private static Vector2 screenGrabOffset;
        private static float baseUiFontRatio;
        private static float smoothedDelta;
        private static bool inTaskLayout;
        private static bool IsBarContext => ShipStatus.Instance != null || GameStates.isLobby;

        private static void HideBar()
        {
            foreach (var item in Items)
                if (item.Text != null) item.Text.gameObject.SetActive(false);
        }
        private static ConfigFile lobbyFile, gameFile;
        private static ConfigFile CurrentFile => inTaskLayout ? gameFile : lobbyFile;
        private static ConfigEntry<bool> lobbyEnabled, gameEnabled, lobbyDragging, gameDragging, lobbyZeroKicks, gameZeroKicks;
        private static ConfigEntry<bool> barEnabled => inTaskLayout ? gameEnabled : lobbyEnabled;
        private static ConfigEntry<bool> allowDragging => inTaskLayout ? gameDragging : lobbyDragging;
        private static ConfigEntry<bool> showZeroKicks => inTaskLayout ? gameZeroKicks : lobbyZeroKicks;
        private static InfoBarMiniMenu menu;
        private static float nextMenuCreate;
        private static int lastMenuFrame = -1;
        private static int selected;
        private static bool editTask;
        private static string editX = "", editY = "", editColor = "", editSize = "";
        private static string menuMessage = "";
        private static string activeField = "";
        private static string activeSlider = "";

        private static string CodeLobbyPosition(string id)
        {
            switch (id)
            {
                case "Firma": return "0.41468748,0.026027411";
                case "Ping": return "0.4365625,0.06719139";
                case "Kicks": return "0.043854132,0.969837";
                case "FPS": return "0.44906253,0.97639215";
                case "Timer": return "0.43291655,0.9621969";
                default: return "";
            }
        }

        private static string CodeTaskPosition(string id)
        {
            switch (id)
            {
                case "Firma": return "0.0073958337,0.027437896";
                case "Ping": return "0.45062497,0.04393828";
                case "Kicks": return "0.008437365,0.056544393";
                case "FPS": return "0.4615625,0.95324403";
                case "Timer": return "";
                default: return "";
            }
        }

        private static string CodeDefaultPosition(string id, bool inGame) => inGame ? CodeTaskPosition(id) : CodeLobbyPosition(id);

        private static float CodeLobbySize(string id)
        {
            switch (id)
            {
                case "Firma": return 125f;
                case "Ping": return 125f;
                case "Kicks": return 100f;
                case "FPS": return 150f;
                case "Timer": return 250f;
                default: return 100f;
            }
        }

        private static float CodeTaskSize(string id)
        {
            switch (id)
            {
                case "Firma": return 75f;
                case "Ping": return 100f;
                case "Kicks": return 75f;
                case "FPS": return 150f;
                case "Timer": return 250f;
                default: return 100f;
            }
        }

        private static string CodeColor(string id)
        {
            switch (id)
            {
                case "Firma": return "#A54AFFFF";
                case "Ping": return "#FFFFFF";
                case "Kicks": return "#FFFFFF";
                case "FPS": return "#B6DF2F";
                case "Timer": return "#FFFFFFFF";
                default: return "";
            }
        }

        private static bool CodeBold(string id)
        {
            switch (id)
            {
                case "Firma": return true;
                case "Ping": return true;
                case "Kicks": return false;
                case "FPS": return true;
                case "Timer": return true;
                default: return false;
            }
        }

        private static bool CodeAutoColor(string id)
        {
            switch (id)
            {
                case "Firma": return false;
                case "Ping": return true;
                case "Kicks": return true;
                case "FPS": return false;
                case "Timer": return false;
                default: return false;
            }
        }

        public static void Init(ConfigFile file)
        {
            config = file;
            string folder = Path.GetDirectoryName(file.ConfigFilePath);
            lobbyFile = new ConfigFile(Path.Combine(folder, "BanMod.InfoBar.Lobby.cfg"), false);
            gameFile = new ConfigFile(Path.Combine(folder, "BanMod.InfoBar.InGame.cfg"), false);
            lobbyFile.SaveOnConfigSet = false;
            gameFile.SaveOnConfigSet = false;
            lobbyEnabled = lobbyFile.Bind("General", "Enabled", true);
            gameEnabled = gameFile.Bind("General", "Enabled", true);
            lobbyDragging = lobbyFile.Bind("General", "AllowDragging", true);
            gameDragging = gameFile.Bind("General", "AllowDragging", true);
            lobbyZeroKicks = lobbyFile.Bind("General", "ShowKicksAtZero", false);
            gameZeroKicks = gameFile.Bind("General", "ShowKicksAtZero", false);
            lobbyFile.Bind("Profile", "Mode", "Lobby");
            gameFile.Bind("Profile", "Mode", "InGame");
            foreach (var item in Items)
                if (item.Text != null) UnityEngine.Object.Destroy(item.Text.gameObject);
            Items.Clear();
            source = null;
            dragging = null;
            inTaskLayout = ShipStatus.Instance != null;
            editTask = inTaskLayout;
            foreach (string id in new[] { "Firma", "Ping", "Kicks", "FPS", "Timer" })
            {
                Items.Add(new Item
                {
                    Id = id,
                    LobbyColor = lobbyFile.Bind(id, "Color", CodeColor(id)),
                    TaskColor = gameFile.Bind(id, "Color", CodeColor(id)),
                    LobbyBold = lobbyFile.Bind(id, "Bold", CodeBold(id)),
                    TaskBold = gameFile.Bind(id, "Bold", CodeBold(id)),
                    LobbyAutoColor = lobbyFile.Bind(id, "AutomaticColor", CodeAutoColor(id)),
                    TaskAutoColor = gameFile.Bind(id, "AutomaticColor", CodeAutoColor(id)),
                    LobbyVisible = lobbyFile.Bind(id, "Visible", true),
                    TaskVisible = gameFile.Bind(id, "Visible", true),
                    LobbySize = lobbyFile.Bind(id, "SizePercent", CodeLobbySize(id)),
                    TaskSize = gameFile.Bind(id, "SizePercent", CodeTaskSize(id)),
                    LobbyPosition = lobbyFile.Bind(id, "Position", CodeLobbyPosition(id)),
                    TaskPosition = gameFile.Bind(id, "Position", CodeTaskPosition(id))
                });
            }
            foreach (var item in Items)
            {
                item.LobbySize.Value = NormalizeSize(item.LobbySize.Value);
                item.TaskSize.Value = NormalizeSize(item.TaskSize.Value);
            }
            lobbyFile.Save();
            gameFile.Save();
            Debug.Log("[BanMod Layout] PROFILE FILES: " + Path.GetFullPath(lobbyFile.ConfigFilePath) +
                " | " + Path.GetFullPath(gameFile.ConfigFilePath));
            EnsureMenu();
            RefreshFields();
        }

        public static void CaptureTimer(GameStartManager manager)
        {
            if (config == null) return;
            if (timer == null)
            {
                var target = manager.transform.Find(
                    "StartGameArea/StartButton (Host)/LobbyTimer");
                if (target != null) timer = target.GetComponent<TMP_Text>();
            }
            if (timer != null) timer.enabled = false;
        }

        public static void UpdateBar(PingTracker tracker)
        {
            if (tracker == null) return;
            trackedPing = tracker;
            if (lastBarFrame == Time.frameCount) return;
            lastBarFrame = Time.frameCount;
            MenuTick();
            if (config == null && BanMod.Instance != null)
                Init(BanMod.Instance.Config);
            if (!IsBarContext)
            {
                HideBar();
                return;
            }
            var client = AmongUsClient.Instance;
            if (config == null || tracker.text == null || client == null) return;
            bool taskNow = ShipStatus.Instance != null;
            if (taskNow != inTaskLayout)
            {
                SaveDraggedPosition();
                inTaskLayout = taskNow;
                CurrentFile.Reload();
                editTask = taskNow;
                activeField = "";
                activeSlider = "";
                foreach (var item in Items)
                {
                    item.Positioned = false;
                    if (TryReadPosition(item.Position.Value, out Vector2 saved))
                    {
                        item.ScreenPosition = saved;
                        item.Positioned = true;
                    }
                }
                RefreshFields();
                Debug.Log("[BanMod Layout] Position profile: " +
                    (inTaskLayout ? "InTask" : "Lobby"));
            }
            if (source != tracker.text || Items[0].Text == null) CreateItems(tracker.text);
            tracker.text.enabled = false;

            smoothedDelta = Mathf.Lerp(smoothedDelta > 0f ? smoothedDelta :
                Time.unscaledDeltaTime, Time.unscaledDeltaTime, 0.1f);
            int ping = client.Ping;
            int fps = Mathf.RoundToInt(1f / Mathf.Max(smoothedDelta, 0.0001f));
            int votes = client.AmHost ? 0 : VoteBanTracker.GetVoteCount(client.ClientId);
            string pingColor = ping <= 100 ? "#00FF00" : ping < 400 ? "#FFFF00" : "#FF0000";
            string fpsColor = fps < 20 ? "#FF0000" : fps < 40 ? "#FFFF00" : "#00FF00";
            string voteColor = votes >= 2 && Mathf.FloorToInt(Time.time * 4f) % 2 == 0
                ? "#FF0000" : "#FFFF00";

            SetText(0, $"<color={BanMod.ModColor}>BanMod</color> " +
                "<color=#A54AFF>By</color> <color=#F34C50>Bart</color>");
            SetText(1, $"Ping: {ping} ms", true, pingColor);
            SetText(2, $"Kicks: {votes}/3", votes > 0 || showZeroKicks.Value, voteColor);
            SetText(3, $"FPS: {fps}", true, fpsColor);
            bool hasTimer = ShipStatus.Instance == null && timer != null && timer.gameObject.activeInHierarchy;
            string remaining = hasTimer ? Regex.Replace(timer.text ?? "", "<[^>]*>", "").Trim() : "";
            SetText(4, remaining, hasTimer && remaining.Length > 0);

            PlaceUnpositionedItems();
            HandleDrag();
        }

        public static void RefreshBar()
        {
            if (trackedPing != null) UpdateBar(trackedPing);
        }

        private static void CreateItems(TMP_Text original)
        {
            if (overlayCanvas != null)
                UnityEngine.Object.Destroy(overlayCanvas.gameObject);
            foreach (var item in Items)
                if (item.Text != null) UnityEngine.Object.Destroy(item.Text.gameObject);
            source = original;
            dragging = null;
            var root = new GameObject("BanModInfoBarOverlay");
            overlayCanvas = root.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 32760;
            UnityEngine.Object.DontDestroyOnLoad(root);
            foreach (var item in Items)
            {
                var obj = new GameObject("BanModInfo_" + item.Id);
                obj.transform.SetParent(root.transform, false);
                var copy = obj.AddComponent<TextMeshProUGUI>();
                copy.font = original.font;
                copy.fontSharedMaterial = original.fontSharedMaterial;
                copy.color = Color.white;
                copy.richText = true;
                copy.raycastTarget = false;
                copy.rectTransform.anchorMin = Vector2.zero;
                copy.rectTransform.anchorMax = Vector2.zero;
                copy.rectTransform.pivot = new Vector2(0f, 0.5f);
                copy.rectTransform.sizeDelta = new Vector2(Screen.width * 2f, 200f);
                copy.transform.localScale = Vector3.one;
                copy.enabled = true;
                copy.enableAutoSizing = false;
                copy.alignment = TextAlignmentOptions.MidlineLeft;
                copy.enableWordWrapping = false;
                copy.overflowMode = TextOverflowModes.Overflow;
                copy.fontSize = 100f;
                if (baseUiFontRatio <= 0f)
                {
                    Vector2 uiMeasure = copy.GetPreferredValues("Ag");
                    Vector2 sourceMeasure = original.GetPreferredValues("Ag");
                    Camera cam = Camera.main;
                    float pixelWidth;
                    if (original.GetComponent<CanvasRenderer>() != null)
                    {
                        pixelWidth = sourceMeasure.x * Mathf.Abs(original.transform.lossyScale.x);
                    }
                    else if (cam != null)
                    {
                        Vector3 start = cam.WorldToScreenPoint(original.transform.position);
                        Vector3 end = cam.WorldToScreenPoint(original.transform.TransformPoint(new Vector3(sourceMeasure.x, 0f, 0f)));
                        pixelWidth = Vector2.Distance(new Vector2(start.x, start.y), new Vector2(end.x, end.y));
                    }
                    else pixelWidth = uiMeasure.x * 0.2f;
                    baseUiFontRatio = Mathf.Max(1f, 100f * pixelWidth / Mathf.Max(uiMeasure.x, 0.001f)) / Mathf.Max(1, Screen.height);
                }
                copy.text = "";
                item.Text = copy;
                item.Positioned = TryReadPosition(item.Position.Value, out Vector2 position);
                item.ScreenPosition = position;
            }
            Debug.Log("[BanMod Layout] Screen-space overlay created. Profile=" +
                (inTaskLayout ? "InGame" : "Lobby"));
        }

        private static void SetText(int index, string value, bool available = true, string automaticColor = null)
        {
            var item = Items[index];
            item.Text.gameObject.SetActive(IsBarContext && barEnabled.Value && item.Visible.Value && available);
            string color = item.AutoColor.Value && automaticColor != null ? automaticColor : item.Color.Value;
            if (!ColorUtility.TryParseHtmlString(color, out _)) color = "#FFFFFF";
            float size = NormalizeSize(item.Size.Value);
            item.Text.enableAutoSizing = false;
            item.Text.fontSize = baseUiFontRatio * Screen.height * size / 100f;
            string content = value.Replace("\r", "").Replace("\n", " ");
            if (item.Bold.Value) content = "<b>" + content + "</b>";
            item.Text.text = "<color=" + color + ">" + content + "</color>";
        }

        private static Vector2 TextAnchorOffset(Item item)
        {
            Bounds bounds = item.Text.textBounds;
            return new Vector2(bounds.min.x, bounds.center.y);
        }

        private static void ApplyScreenPosition(Item item)
        {
            item.Text.rectTransform.anchoredPosition = new Vector2(
                item.ScreenPosition.x * Screen.width, item.ScreenPosition.y * Screen.height) - TextAnchorOffset(item);
        }

        private static void PlaceUnpositionedItems()
        {
            if (overlayCanvas == null) return;
            float cursor = Screen.width * 0.03f;
            foreach (var item in Items)
            {
                if (item.Text == null || !item.Text.gameObject.activeInHierarchy) continue;
                item.Text.fontSize = baseUiFontRatio * Screen.height * NormalizeSize(item.Size.Value) / 100f;
                item.Text.rectTransform.sizeDelta = new Vector2(Screen.width * 2f, Screen.height);
                item.Text.ForceMeshUpdate();
                if (dragging != item && TryReadPosition(item.Position.Value, out Vector2 configured))
                {
                    item.ScreenPosition = configured;
                    item.Positioned = true;
                }
                if (!item.Positioned)
                {
                    item.ScreenPosition = new Vector2(cursor / Screen.width, 0.055f);
                    item.Positioned = true;
                }
                ApplyScreenPosition(item);
                cursor += item.Text.textBounds.size.x + 12f;
            }
        }

        private static void HandleDrag()
        {
            if (!allowDragging.Value || !barEnabled.Value)
            { SaveDraggedPosition(); return; }
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            Vector3 mouse3 = Input.mousePosition;
            Vector2 mouse = new Vector2(mouse3.x, mouse3.y);
            if (dragging == null && alt && Input.GetMouseButtonDown(0) && !MouseOverMenu())
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    var item = Items[i];
                    if (item.Text == null || !item.Text.gameObject.activeInHierarchy) continue;
                    Bounds b = item.Text.textBounds;
                    Vector2 pivot = item.Text.rectTransform.anchoredPosition;
                    Rect hit = new Rect(pivot.x + b.min.x - 6f, pivot.y + b.min.y - 6f,
                        b.size.x + 12f, b.size.y + 12f);
                    if (!hit.Contains(mouse)) continue;
                    dragging = item;
                    screenGrabOffset = new Vector2(item.ScreenPosition.x * Screen.width,
                        item.ScreenPosition.y * Screen.height) - mouse;
                    break;
                }
            }
            if (dragging == null) return;
            Vector2 anchor = mouse + screenGrabOffset;
            dragging.ScreenPosition = new Vector2(anchor.x / Screen.width, anchor.y / Screen.height);
            dragging.Positioned = true;
            ApplyScreenPosition(dragging);
            if (Input.GetMouseButtonUp(0) || !alt || !Input.GetMouseButton(0)) SaveDraggedPosition();
        }

        private static void SaveDraggedPosition()
        {
            if (dragging == null) return;
            dragging.Position.Value = string.Format(CI, "{0:R},{1:R}", dragging.ScreenPosition.x, dragging.ScreenPosition.y);
            Debug.Log(string.Format(CI, "[BanMod Layout] SCREEN [{0}] {1}: {2}",
                inTaskLayout ? "InGame" : "Lobby", dragging.Id, dragging.Position.Value));
            dragging = null;
            PersistAndVerifyProfile();
        }

        private static Rect windowRect = new Rect(20f, 20f, 610f, 690f);
        private static GUIStyle menuButton, menuTitle, menuLabel, menuField, previewStyle;
        private static float MenuScale => Mathf.Min(1f, Mathf.Min(Screen.width / 650f, Screen.height / 730f));
        private static Rect MenuRect => new Rect(windowRect.x * MenuScale, windowRect.y * MenuScale,
            windowRect.width * MenuScale, windowRect.height * MenuScale);

        private static bool MouseOverMenu()
        {
            Vector3 mouse = Input.mousePosition;
            return InfoBarMiniMenu.Open && MenuRect.Contains(new Vector2(mouse.x, Screen.height - mouse.y));
        }

        private static void EnsureMenu()
        {
            if (menu != null || BanMod.Instance == null || Time.realtimeSinceStartup < nextMenuCreate) return;
            nextMenuCreate = Time.realtimeSinceStartup + 5f;
            try
            {
                menu = BanMod.Instance.AddComponent<InfoBarMiniMenu>();
                if (menu != null) menu.enabled = true;
                Debug.Log("[BanMod Layout] Menu component: " + (menu != null ? "OK" : "MISSING"));
            }
            catch (Exception ex) { Debug.LogError("[BanMod Layout] Menu creation failed: " + ex); }
        }

        public static void MenuTick()
        {
            if (lastMenuFrame == Time.frameCount) return;
            lastMenuFrame = Time.frameCount;
            if (config == null && BanMod.Instance != null) Init(BanMod.Instance.Config);
            if (config == null) return;
            EnsureMenu();
            if (menu != null) { menu.gameObject.SetActive(true); menu.enabled = true; }
            if (InfoBarMiniMenu.Open) HandleFieldInput();
            if (!Input.GetKeyDown(KeyCode.F12) || BanMod.IsBanModDisabled) return;
            InfoBarMiniMenu.Open = !InfoBarMiniMenu.Open;
            if (InfoBarMiniMenu.Open) OpenMenu();
            Debug.Log("[BanMod Layout] F12: menu " + (InfoBarMiniMenu.Open ? "OPEN" : "CLOSED"));
        }

        public static void OpenMenu()
        {
            activeField = "";
            activeSlider = "";
            editTask = inTaskLayout;
            float scale = MenuScale;
            windowRect.x = Mathf.Max(0f, (Screen.width / scale - windowRect.width) / 2f);
            windowRect.y = Mathf.Max(0f, (Screen.height / scale - windowRect.height) / 2f);
            RefreshFields();
        }

        private static ConfigEntry<string> EditedPosition(Item item) => editTask ? item.TaskPosition : item.LobbyPosition;
        private static ConfigEntry<float> EditedSize(Item item) => editTask ? item.TaskSize : item.LobbySize;
        private static float NormalizeSize(float value) => Mathf.Clamp(Mathf.Round(value / 25f) * 25f, 25f, 250f);
        private static string DisplayName(Item item) => item.Id == "Firma" ? "BanMod By Bart" : item.Id;

        private static void RefreshFields()
        {
            if (Items.Count == 0) return;
            activeField = "";
            var item = Items[selected];
            if (!TryReadPosition(EditedPosition(item).Value, out Vector2 pos))
                pos = editTask == inTaskLayout && item.Positioned ? item.ScreenPosition : new Vector2(0.05f, 0.055f);
            editX = (pos.x * 100f).ToString("0.###", CI);
            editY = (pos.y * 100f).ToString("0.###", CI);
            editColor = item.Color.Value;
            editSize = EditedSize(item).Value.ToString("0.##", CI);
            menuMessage = "";
        }

        private static void EnsureStyles()
        {
            if (menuButton != null) return;
            menuButton = new GUIStyle(BanModUiStyles.ButtonDark)
            { fontSize = 17, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            menuTitle = new GUIStyle(BanModUiStyles.TitleLabel)
            { fontSize = 23, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            menuTitle.normal.textColor = BanModUiStyles.AccentColor;
            menuLabel = new GUIStyle(BanModUiStyles.BodyLabel)
            { fontSize = 16, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            menuField = new GUIStyle(BanModUiStyles.DarkBox)
            { fontSize = 17, alignment = TextAnchor.MiddleLeft };
            menuField.normal.textColor = Color.white;
            previewStyle = new GUIStyle(menuTitle) { fontSize = 28, richText = true };
        }

        private static bool Button(float x, float y, float w, string label)
            => GUI.Button(new Rect(x, y, w, 32f), label, menuButton);
        private static bool Option(float x, float y, float w, string label, bool value)
            => Button(x, y, w, label + (value ? ": ON" : ": OFF")) ? !value : value;
        private static void Label(float x, float y, float w, string label)
            => GUI.Label(new Rect(x, y, w, 27f), label, menuLabel);
        private static string Field(float x, float y, float w, string value, string id)
        {
            bool editing = activeField == id;
            string caret = editing && Mathf.FloorToInt(Time.unscaledTime * 2f) % 2 == 0 ? "|" : "";
            Color previous = GUI.backgroundColor;
            if (editing) GUI.backgroundColor = BanModUiStyles.AccentColor;
            if (GUI.Button(new Rect(x, y, w, 30f), value + caret, menuField))
            {
                activeField = id;
                activeSlider = "";
            }
            GUI.backgroundColor = previous;
            return value;
        }

        private static void HandleFieldInput()
        {
            if (string.IsNullOrEmpty(activeField)) return;
            string value = activeField == "size" ? editSize : activeField == "color" ? editColor :
                activeField == "x" ? editX : editY;
            bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (control && Input.GetKeyDown(KeyCode.A)) value = "";
            bool backspaceHandled = false;
            foreach (char c in Input.inputString ?? "")
            {
                if (c == '\b')
                {
                    if (value.Length > 0) value = value.Substring(0, value.Length - 1);
                    backspaceHandled = true;
                    continue;
                }
                if (c == '\n' || c == '\r') continue;
                string allowed = activeField == "color" ? "#0123456789abcdefABCDEF" : "0123456789.,-";
                if (allowed.IndexOf(c) >= 0 && value.Length < 20 && !control) value += c;
            }
            if (!backspaceHandled && Input.GetKeyDown(KeyCode.Backspace) && value.Length > 0)
                value = value.Substring(0, value.Length - 1);
            if (activeField == "size") editSize = value;
            else if (activeField == "color") editColor = value;
            else if (activeField == "x") editX = value;
            else editY = value;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Escape)) activeField = "";
        }

        private static float ManualSlider(Rect rect, float value, float min, float max)
        {
            string id = rect.x.ToString(CI) + ":" + rect.y.ToString(CI);
            Event e = Event.current;
            bool mouseDown = e.type == EventType.MouseDown && e.button == 0;
            if (mouseDown && rect.Contains(e.mousePosition))
            {
                activeSlider = id;
                activeField = "";
            }
            if (activeSlider == id && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
            {
                value = Mathf.Lerp(min, max, Mathf.Clamp01((e.mousePosition.x - rect.x) / rect.width));
                e.Use();
            }
            if (activeSlider == id && (e.type == EventType.MouseUp || !Input.GetMouseButton(0)))
                activeSlider = "";
            GUI.Box(new Rect(rect.x, rect.y + 6f, rect.width, 8f), "", BanModUiStyles.DarkBox);
            float fraction = Mathf.InverseLerp(min, max, value);
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = BanModUiStyles.AccentColor;
            GUI.Box(new Rect(rect.x + fraction * (rect.width - 14f), rect.y, 14f, 20f), "", menuButton);
            GUI.backgroundColor = previous;
            return value;
        }

        public static void DrawMenu()
        {
            if (config == null || Items.Count == 0 || BanMod.IsBanModDisabled) return;
            EnsureStyles();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color previousGuiColor = GUI.color;
            try
            {
                GUI.color = Color.white;
                GUI.matrix = Matrix4x4.Scale(new Vector3(MenuScale, MenuScale, 1f));
                windowRect = GUI.Window(812453, windowRect, (GUI.WindowFunction)DrawStyledWindow,
                    "", BanModUiStyles.BlackWindow);
            }
            finally { GUI.matrix = oldMatrix; GUI.color = previousGuiColor; }
        }

        private static bool windowErrorLogged;

        private static void DrawStyledWindow(int id)
        {
            try { DrawStyledWindowContent(id); }
            catch (Exception ex)
            {
                if (!windowErrorLogged)
                {
                    windowErrorLogged = true;
                    Debug.LogError("[BanMod Layout] Window rendering failed: " + ex);
                }
            }
        }

        private static void DrawStyledWindowContent(int id)
        {
            editTask = inTaskLayout;
            GUI.Label(new Rect(20f, 12f, 520f, 34f),
                "INFO BAR / " + (inTaskLayout ? "IN GAME" : "LOBBY"), menuTitle);
            if (Button(552f, 12f, 40f, "X")) InfoBarMiniMenu.Open = false;
            GUI.Box(new Rect(16f, 58f, 210f, 544f), "", BanModUiStyles.DarkBox);
            GUI.Box(new Rect(238f, 58f, 356f, 544f), "", BanModUiStyles.DarkBox);
            GUI.Label(new Rect(26f, 68f, 190f, 30f), "ELEMENTS", menuTitle);
            for (int i = 0; i < Items.Count; i++)
                if (Button(28f, 108f + i * 42f, 186f, (selected == i ? "> " : "") + DisplayName(Items[i])))
                { selected = i; RefreshFields(); }
            barEnabled.Value = Option(28f, 416f, 186f, "Info bar", barEnabled.Value);
            allowDragging.Value = Option(28f, 458f, 186f, "Alt + drag", allowDragging.Value);
            showZeroKicks.Value = Option(28f, 500f, 186f, "Kicks at zero", showZeroKicks.Value);
            Label(28f, 546f, 182f, "F12: open / close");

            var item = Items[selected];
            GUI.Label(new Rect(250f, 68f, 332f, 30f), "STYLE / " + DisplayName(item), menuTitle);
            item.Visible.Value = Option(252f, 108f, 154f, "Visible", item.Visible.Value);
            item.Bold.Value = Option(418f, 108f, 160f, "Bold", item.Bold.Value);
            Label(252f, 149f, 320f, "SIZE (%)");
            for (int i = 0; i < 10; i++)
            {
                int size = (i + 1) * 25;
                string caption = (Mathf.Approximately(EditedSize(item).Value, size) ? ">" : "") + size + "%";
                if (Button(252f + (i % 5) * 66f, 181f + (i / 5) * 38f, 62f, caption))
                {
                    EditedSize(item).Value = size;
                    menuMessage = "Size changed. Use SAVE PROFILE to save all settings.";
                }
            }
            if (item.Id == "Firma")
            {
                GUI.Label(new Rect(254f, 282f, 322f, 90f),
                    "BanMod By Bart is a single element.\nIts original colors are fixed.\nMove with Alt + drag. Adjust bold and size here.", menuLabel);
                previewStyle.normal.textColor = Color.white;
                previewStyle.fontStyle = item.Bold.Value ? FontStyle.Bold : FontStyle.Normal;
                previewStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(20f * EditedSize(item).Value / 100f), 8, 40);
                GUI.Label(new Rect(252f, 403f, 326f, 100f),
                    $"<color={BanMod.ModColor}>BanMod</color> <color=#A54AFF>By</color> <color=#F34C50>Bart</color>", previewStyle);
            }
            else
            {
                DrawColorPicker(item);
                ColorUtility.TryParseHtmlString(item.Color.Value, out Color previewColor);
                previewStyle.normal.textColor = previewColor;
                previewStyle.fontStyle = item.Bold.Value ? FontStyle.Bold : FontStyle.Normal;
                previewStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(20f * EditedSize(item).Value / 100f), 8, 40);
                GUI.Label(new Rect(252f, 553f, 326f, 44f), PreviewText(item), previewStyle);
            }

            if (Button(20f, 615f, 180f, "SAVE PROFILE")) SaveCurrentProfile();
            if (Button(215f, 615f, 180f, "RELOAD PROFILE")) ReloadCurrentProfile();
            if (Button(410f, 615f, 180f, "CLOSE")) InfoBarMiniMenu.Open = false;
            GUI.Label(new Rect(22f, 655f, 565f, 28f), menuMessage, menuLabel);
            GUI.DragWindow(new Rect(0f, 0f, 545f, 52f));
        }


        private static Texture2D colorSquare, hueStrip;
        private static float renderedHue = -1f, pickerHue, pickerSaturation, pickerValue;
        private static string pickerItem = "", pickerHex = "";

        private static readonly Dictionary<int, GUIStyle> pickerTextureStyles = new Dictionary<int, GUIStyle>();

        private static void DrawPickerTexture(Rect rect, Texture2D texture)
        {
            if (texture == null || Event.current.type != EventType.Repaint) return;
            int key = texture.GetInstanceID();
            if (!pickerTextureStyles.TryGetValue(key, out GUIStyle style))
            {
                style = new GUIStyle();
                style.normal.background = texture;
                style.hover.background = texture;
                style.active.background = texture;
                style.focused.background = texture;
                style.border = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                style.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                pickerTextureStyles[key] = style;
            }
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            GUI.Box(rect, "", style);
            GUI.backgroundColor = oldBackground;
        }

        private static void DrawColorPicker(Item item)
        {
            Color pickerOldColor = GUI.color;
            Color pickerOldBackground = GUI.backgroundColor;
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            try { DrawColorPickerContent(item); }
            finally
            {
                GUI.color = pickerOldColor;
                GUI.backgroundColor = pickerOldBackground;
            }
        }

        private static void DrawColorPickerContent(Item item)
        {
            if (pickerItem != item.Id || pickerHex != item.Color.Value)
            {
                if (!ColorUtility.TryParseHtmlString(item.Color.Value, out Color original)) original = Color.white;
                Color.RGBToHSV(original, out pickerHue, out pickerSaturation, out pickerValue);
                pickerItem = item.Id; pickerHex = item.Color.Value;
            }
            Rect square = new Rect(254f, 270f, 220f, 190f);
            Rect hue = new Rect(488f, 270f, 28f, 190f);
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (square.Contains(e.mousePosition)) activeSlider = "color-square";
                else if (hue.Contains(e.mousePosition)) activeSlider = "color-hue";
            }
            bool changed = false;
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (activeSlider == "color-square")
                {
                    pickerSaturation = Mathf.Clamp01((e.mousePosition.x - square.x) / square.width);
                    pickerValue = 1f - Mathf.Clamp01((e.mousePosition.y - square.y) / square.height);
                    activeField = ""; changed = true; e.Use();
                }
                else if (activeSlider == "color-hue")
                {
                    pickerHue = 1f - Mathf.Clamp01((e.mousePosition.y - hue.y) / hue.height);
                    activeField = ""; changed = true; e.Use();
                }
            }
            if (e.type == EventType.MouseUp || !Input.GetMouseButton(0))
                if (activeSlider == "color-square" || activeSlider == "color-hue") activeSlider = "";
            if (changed)
            {
                Color chosen = Color.HSVToRGB(pickerHue, pickerSaturation, pickerValue);
                item.Color.Value = "#" + ColorUtility.ToHtmlStringRGB(chosen);
                pickerHex = item.Color.Value; editColor = pickerHex; item.AutoColor.Value = false;
            }
            if (colorSquare == null)
            {
                colorSquare = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                colorSquare.hideFlags = HideFlags.HideAndDontSave;
                colorSquare.wrapMode = TextureWrapMode.Clamp;
                hueStrip = new Texture2D(1, 128, TextureFormat.RGBA32, false);
                hueStrip.hideFlags = HideFlags.HideAndDontSave;
                hueStrip.wrapMode = TextureWrapMode.Clamp;
                for (int y = 0; y < 128; y++) hueStrip.SetPixel(0, y, Color.HSVToRGB(y / 127f, 1f, 1f));
                hueStrip.Apply(); renderedHue = -1f;
            }
            if (Mathf.Abs(renderedHue - pickerHue) > .001f)
            {
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                        colorSquare.SetPixel(x, y, Color.HSVToRGB(pickerHue, x / 63f, y / 63f));
                colorSquare.Apply(); renderedHue = pickerHue;
            }
            DrawPickerTexture(square, colorSquare);
            DrawPickerTexture(hue, hueStrip);
            Color previous = GUI.color;
            GUI.color = Color.black;
            Rect marker = new Rect(square.x + pickerSaturation * square.width - 4f,
                square.y + (1f - pickerValue) * square.height - 4f, 8f, 8f);
            DrawPickerTexture(marker, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawPickerTexture(new Rect(marker.x + 2f, marker.y + 2f, 4f, 4f), Texture2D.whiteTexture);
            DrawPickerTexture(new Rect(hue.x - 3f, hue.y + (1f - pickerHue) * hue.height - 2f,
                hue.width + 6f, 4f), Texture2D.whiteTexture);
            GUI.color = Color.HSVToRGB(pickerHue, pickerSaturation, pickerValue);
            DrawPickerTexture(new Rect(530f, 270f, 46f, 46f), Texture2D.whiteTexture);
            GUI.color = previous;
            Label(526f, 323f, 56f, "Hue");
            editColor = Field(252f, 475f, 200f, editColor, "color");
            if (Button(466f, 475f, 112f, "Apply HEX"))
            {
                string hex = editColor.Trim(); if (!hex.StartsWith("#")) hex = "#" + hex;
                if (ColorUtility.TryParseHtmlString(hex, out _))
                { item.Color.Value = hex; item.AutoColor.Value = false; menuMessage = "Color saved."; }
                else menuMessage = "Use #RRGGBB or #RRGGBBAA.";
            }
            if (item.Id == "Ping" || item.Id == "FPS" || item.Id == "Kicks")
                item.AutoColor.Value = Option(252f, 515f, 326f, "Automatic status color", item.AutoColor.Value);
        }

        private static string PreviewText(Item item)
        {
            if (item.Id == "Ping") return "Ping: 27 ms";
            if (item.Id == "Kicks") return "Kicks: 1/3";
            if (item.Id == "FPS") return "FPS: 60";
            if (item.Id == "Timer") return "09:59";
            return DisplayName(item);
        }

        private static void SaveCurrentProfile()
        {
            SaveDraggedPosition();
            foreach (var item in Items)
            {
                if (!item.Positioned) continue;
                item.Position.Value = string.Format(CI, "{0:R},{1:R}", item.ScreenPosition.x, item.ScreenPosition.y);
            }
            PersistAndVerifyProfile();
        }

        private static bool PersistAndVerifyProfile()
        {
            string path = CurrentFile.ConfigFilePath;
            try
            {
                CurrentFile.Save();
                var disk = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string section = "";
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    { section = line.Substring(1, line.Length - 2); continue; }
                    int equals = line.IndexOf('=');
                    if (equals < 0) continue;
                    disk[section + "/" + line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
                }
                foreach (var item in Items)
                {
                    VerifySaved(disk, item.Id + "/Position", item.Position.Value);
                    VerifySaved(disk, item.Id + "/Color", item.Color.Value);
                    VerifySaved(disk, item.Id + "/Bold", item.Bold.Value.ToString());
                    VerifySaved(disk, item.Id + "/Visible", item.Visible.Value.ToString());
                    VerifySaved(disk, item.Id + "/AutomaticColor", item.AutoColor.Value.ToString());
                    if (!disk.TryGetValue(item.Id + "/SizePercent", out string size) ||
                        !ReadNumber(size, out float parsed) || !Mathf.Approximately(parsed, item.Size.Value))
                        throw new IOException("Size verification failed for " + item.Id);
                }
                VerifySaved(disk, "General/Enabled", barEnabled.Value.ToString());
                VerifySaved(disk, "General/AllowDragging", allowDragging.Value.ToString());
                VerifySaved(disk, "General/ShowKicksAtZero", showZeroKicks.Value.ToString());
                menuMessage = "SAVED AND VERIFIED: " + Path.GetFileName(path);
                Debug.Log("[BanMod Layout] SAVED AND VERIFIED: " + Path.GetFullPath(path));
                foreach (var item in Items)
                    Debug.Log(string.Format(CI,
                        "[BanMod Layout] DISK [{0}] {1}: position={2}; size={3}%; color={4}; bold={5}; visible={6}",
                        inTaskLayout ? "InGame" : "Lobby", item.Id, item.Position.Value,
                        item.Size.Value, item.Color.Value, item.Bold.Value, item.Visible.Value));
                return true;
            }
            catch (Exception ex)
            {
                menuMessage = "SAVE FAILED - see log";
                Debug.LogError("[BanMod Layout] SAVE FAILED: " + path + "\n" + ex);
                return false;
            }
        }

        private static void VerifySaved(Dictionary<string, string> disk, string key, string expected)
        {
            if (!disk.TryGetValue(key, out string actual) ||
                !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Disk verification failed: " + key + "; expected=" + expected + "; actual=" + actual);
        }

        public static void ReapplyLayout()
        {
            if (!IsBarContext)
            {
                HideBar();
                return;
            }
            if (source != null && Items.Count > 0 && Items[0].Text != null)
                PlaceUnpositionedItems();
        }

        private static void ReloadCurrentProfile()
        {
            dragging = null;
            CurrentFile.Reload();
            foreach (var item in Items)
            {
                item.Positioned = TryReadPosition(item.Position.Value, out Vector2 position);
                item.ScreenPosition = position;
            }
            RefreshFields();
            menuMessage = "Loaded: " + Path.GetFileName(CurrentFile.ConfigFilePath);
        }

        private static bool ReadNumber(string value, out float number)
        {
            return float.TryParse(value.Trim().Replace(',', '.'), NumberStyles.Float, CI, out number)
                && !float.IsNaN(number) && !float.IsInfinity(number);
        }

        private static bool TryReadPosition(string value, out Vector2 position)
        {
            position = default;
            string[] parts = value.Split(',');
            if (parts.Length != 2) return false;
            if (!float.TryParse(parts[0], NumberStyles.Float, CI, out float x) ||
                !float.TryParse(parts[1], NumberStyles.Float, CI, out float y)) return false;
            if (float.IsNaN(x) || float.IsNaN(y) ||
                float.IsInfinity(x) || float.IsInfinity(y)) return false;
            position = new Vector2(x, y);
            return true;
        }
    }

    public class InfoBarMiniMenu : MonoBehaviour
    {
        public InfoBarMiniMenu(IntPtr ptr) : base(ptr) { }
        public static bool Open;
        private bool guiErrorLogged;
        public void LateUpdate()
        {
            LobbyInfoBar.ReapplyLayout();
        }
        public void OnGUI()
        {
            if (!Open) return;
            try
            {
                GUI.depth = -10000;
                LobbyInfoBar.DrawMenu();
            }
            catch (Exception ex)
            {
                if (!guiErrorLogged)
                {
                    guiErrorLogged = true;
                    Debug.LogError("[BanMod Layout] Menu GUI error: " + ex);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    public static class InfoBarMenuKeyboardPatch
    {
        private static void Postfix()
        {
            LobbyInfoBar.MenuTick();
            LobbyInfoBar.RefreshBar();
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTracker_InfoBar
    {
        private static void Postfix(PingTracker __instance)
        {
            LobbyInfoBar.UpdateBar(__instance);
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManager_InfoTimer
    {
        private static void Postfix(GameStartManager __instance)
        {
            LobbyInfoBar.CaptureTimer(__instance);
        }
    }
}
