////credits and licenses in the resources folder
//using BanMod;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEngine;
//using Il2CppInterop.Runtime.Attributes;
//using static BanMod.Translator;
//using static BanMod.Utils;

//namespace BanMod
//{
//    public class MsgMenu : MonoBehaviour
//    {
//        public static string buttonFilePath = "DATA/OTHER/MENU/buttonmessage.txt";

//        public static void Initialize()
//        {
//            try
//            {
//                Directory.CreateDirectory("DATA/SETTINGS_CONFIG/MENU/");
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError("[BanMod] Errore durante Initialize MsgMenu: " + ex.Message);
//            }
//        }

//        public bool showMenu = false;
//        private Rect windowRect;
//        private Vector2 windowSize = new Vector2(750, 600);
//        private Vector2 scrollPosition = Vector2.zero;

//        private GUIStyle titleStyle;
//        private GUIStyle buttonStyle;
//        private GUIStyle exitButtonStyle;

//        public static List<ButtonData> buttonDataList = new List<ButtonData>();
//        private int columns = 3;

//        public static MsgMenu Instance;

//        private float _lastFileCheckTime = 0f;
//        private DateTime _lastWriteTimeUtc = DateTime.MinValue;
//        private const float FileCheckInterval = 1.5f;

//        private void Awake()
//        {
//            Instance = this;
//        }

//        private void OnEnable()
//        {
//            MenuRouter.OnPanelChanged += HandlePanelChanged;
//        }

//        private void OnDisable()
//        {
//            MenuRouter.OnPanelChanged -= HandlePanelChanged;
//        }

//        private void HandlePanelChanged(MenuRouter.Panel p)
//        {
//            showMenu = (p == MenuRouter.Panel.MsgMenu);
//            if (showMenu)
//            {
//                LoadButtonData();
//                CenterWindow();
//            }
//        }

//        public void OpenMenu()
//        {
//            if (!showMenu) LoadButtonData();
//            ToggleMenu();
//        }

//        public void CloseMenu()
//        {
//            if (showMenu) ToggleMenu();
//        }

//        public bool IsOpen()
//        {
//            return showMenu;
//        }

//        void Update()
//        {
//            if (KeyBindOptions.IsBindingActive) return;

//            if (Input.GetKeyDown(KeyBindOptions.K13) && !BanMod.chatOpen)
//            {
//                if (MenuRouter.Current == MenuRouter.Panel.MsgMenu)
//                    MenuRouter.Open(MenuRouter.Panel.None);
//                else
//                    MenuRouter.Open(MenuRouter.Panel.MsgMenu);
//            }

//            if (showMenu)
//            {
//                AutoReloadIfChanged();
//            }
//        }

//        public void ToggleMenu()
//        {
//            showMenu = !showMenu;
//            if (showMenu)
//            {
//                LoadButtonData();
//                CenterWindow();
//            }
//        }

//        private void CenterWindow()
//        {
//            float centerX = Screen.width / 2f - windowSize.x / 2f;
//            float centerY = Screen.height / 2f - windowSize.y / 2f;
//            windowRect = new Rect(centerX, centerY, windowSize.x, windowSize.y);
//        }

//        void EnsureStyles()
//        {
//            if (titleStyle == null)
//            {
//                titleStyle = new GUIStyle(BanModUiStyles.TitleLabel)
//                {
//                    fontSize = 22,
//                    fontStyle = FontStyle.Bold,
//                    alignment = TextAnchor.MiddleCenter
//                };

//                buttonStyle = new GUIStyle(BanModUiStyles.TintableButton)
//                {
//                    alignment = TextAnchor.MiddleCenter,
//                    wordWrap = true,
//                    fontSize = 15,
//                    richText = true
//                };

//                exitButtonStyle = new GUIStyle(BanModUiStyles.DangerButton)
//                {
//                    fontSize = 18,
//                    fontStyle = FontStyle.Bold,
//                    alignment = TextAnchor.MiddleCenter
//                };
//            }
//        }

//        void OnGUI()
//        {
//            if (!showMenu) return;

//            if (Event.current.isMouse)
//            {
//                Event.current.Use();
//            }

//            EnsureStyles();

//            GUI.backgroundColor = Color.white;
//            windowRect = GUI.Window(1, windowRect, (GUI.WindowFunction)WindowFunction, "", BanModUiStyles.BlackWindow);
//        }

//        void WindowFunction(int id)
//        {
//            GUILayout.Label(GetString("MENU_MSG"), titleStyle);
//            GUILayout.Space(10);

//            scrollPosition = GUILayout.BeginScrollView(
//                scrollPosition,
//                GUILayout.Width(windowSize.x - 20),
//                GUILayout.Height(windowSize.y - 120)
//            );

//            ShowButtonContent();

//            GUILayout.EndScrollView();

//            GUILayout.FlexibleSpace();

//            GUI.backgroundColor = Color.white;
//            if (GUILayout.Button(GetString("EXIT"), exitButtonStyle, GUILayout.Height(45)))
//            {
//                MenuRouter.Open(MenuRouter.Panel.None);
//            }
//            GUI.backgroundColor = Color.white;

//            GUI.DragWindow();
//        }

//        void ShowButtonContent()
//        {
//            UpdateColumns();

//            float availableWidth = windowSize.x - 40f;
//            float spacing = 10f;
//            float btnWidth = (availableWidth - ((columns - 1) * spacing)) / columns;
//            int total = buttonDataList.Count;

//            for (int i = 0; i < total; i += columns)
//            {
//                GUILayout.BeginHorizontal();
//                GUILayout.FlexibleSpace();

//                for (int col = 0; col < columns; col++)
//                {
//                    int index = i + col;

//                    if (index < total)
//                    {
//                        ButtonData data = buttonDataList[index];

//                        GUI.backgroundColor = data.ButtonColor;

//                        string buttonText = string.IsNullOrWhiteSpace(data.Icon)
//                            ? data.Title
//                            : $"{data.Icon} {data.Title}";

//                        if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Width(btnWidth), GUILayout.Height(50)))
//                        {
//                            Utils.SendMessage(data.Message.Replace("\\n", "\n"));
//                            MessageBlocker.UpdateLastMessageTime();
//                        }

//                        GUI.backgroundColor = Color.white;

//                        if (col < columns - 1)
//                            GUILayout.Space(spacing);
//                    }
//                    else
//                    {
//                        GUILayout.Space(btnWidth);

//                        if (col < columns - 1)
//                            GUILayout.Space(spacing);
//                    }
//                }

//                GUILayout.FlexibleSpace();
//                GUILayout.EndHorizontal();
//                GUILayout.Space(10);
//            }
//        }

//        private void UpdateColumns()
//        {
//            float width = Screen.width;

//            if (width < 1100f) columns = 2;
//            else if (width < 1600f) columns = 3;
//            else columns = 4;
//        }

//        private void AutoReloadIfChanged()
//        {
//            if (Time.unscaledTime - _lastFileCheckTime < FileCheckInterval)
//                return;

//            _lastFileCheckTime = Time.unscaledTime;

//            try
//            {
//                if (!File.Exists(buttonFilePath))
//                {
//                    CreateButtonExampleFile();
//                    LoadButtonData();
//                    return;
//                }

//                DateTime currentWrite = File.GetLastWriteTimeUtc(buttonFilePath);
//                if (currentWrite != _lastWriteTimeUtc)
//                {
//                    LoadButtonData();
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError("[BanMod] Errore AutoReload MsgMenu: " + ex.Message);
//            }
//        }

//        void LoadButtonData()
//        {
//            try
//            {
//                Directory.CreateDirectory("DATA/OTHER/MENU/");

//                if (!File.Exists(buttonFilePath))
//                {
//                    CreateButtonExampleFile();
//                }

//                string[] lines = File.ReadAllLines(buttonFilePath);
//                buttonDataList.Clear();

//                foreach (var rawLine in lines)
//                {
//                    if (string.IsNullOrWhiteSpace(rawLine)) continue;

//                    string line = rawLine.Trim();

//                    if (line.StartsWith("#")) continue;
//                    if (line.StartsWith("//")) continue;

//                    var data = ParseButtonLine(line);
//                    if (data != null)
//                    {
//                        buttonDataList.Add(data);
//                    }
//                }

//                _lastWriteTimeUtc = File.Exists(buttonFilePath)
//                    ? File.GetLastWriteTimeUtc(buttonFilePath)
//                    : DateTime.MinValue;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError("[BanMod] Errore LoadButtonData: " + ex.Message);
//            }
//        }
//        [HideFromIl2Cpp]
//        private ButtonData ParseButtonLine(string line)
//        {
//            try
//            {
//                var parts = line.Split('|');
//                if (parts.Length < 2) return null;

//                string title = parts[0].Trim();
//                string message = parts[1].Trim();

//                string colorText = parts.Length >= 3 ? parts[2].Trim() : "";

//                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
//                    return null;

//                Color buttonColor = ParseColorOrDefault(colorText, new Color(0.24f, 0.24f, 0.24f, 1f));

//                return new ButtonData
//                {
//                    Title = title,
//                    Message = message,
//                    ButtonColor = buttonColor
//                };
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError("[BanMod] Riga menu non valida: " + line + " | " + ex.Message);
//                return null;
//            }
//        }

//        private Color ParseColorOrDefault(string value, Color fallback)
//        {
//            if (string.IsNullOrWhiteSpace(value))
//                return fallback;

//            value = value.Trim();

//            try
//            {
//                switch (value.ToLowerInvariant())
//                {
//                    case "red": return new Color(0.80f, 0.15f, 0.15f, 1f);
//                    case "green": return new Color(0.20f, 0.65f, 0.20f, 1f);
//                    case "blue": return new Color(0.20f, 0.45f, 0.90f, 1f);
//                    case "yellow": return new Color(0.85f, 0.75f, 0.20f, 1f);
//                    case "orange": return new Color(0.90f, 0.50f, 0.15f, 1f);
//                    case "purple": return new Color(0.55f, 0.30f, 0.80f, 1f);
//                    case "pink": return new Color(0.90f, 0.35f, 0.65f, 1f);
//                    case "gray":
//                    case "grey": return new Color(0.35f, 0.35f, 0.35f, 1f);
//                    case "white": return new Color(0.85f, 0.85f, 0.85f, 1f);
//                    case "black": return new Color(0.15f, 0.15f, 0.15f, 1f);
//                }

//                if (!value.StartsWith("#"))
//                    value = "#" + value;

//                if (ColorUtility.TryParseHtmlString(value, out var parsed))
//                    return parsed;
//            }
//            catch
//            {
//            }

//            return fallback;
//        }

//        void CreateButtonExampleFile()
//        {
//            string[] exampleLines =
//            {
//                "# BanMod Button Message Menu",
//                "# Formato base:",
//                "# Titolo | Messaggio",
//                "#",
//                "# Formato completo:",
//                "# Titolo | Messaggio | Colore",
//                "#",
//                "# Note:",
//                "# - Colore può essere un nome (red, green, blue, yellow...) o un HEX (#FFAA00)",
//                "#",
//                "# Esempi:",
//                "Hello | Hello everyone!",
//                "GG | Good Game! | green ",
//                "Bye | See you later! | red ",
//                "Rules | Please follow the lobby rules!\\nHave fun! | #3A7BFF ",
//                "Discord | Join our Discord server! | purple ",
//                "Ready | Everyone ready? | yellow "
//            };

//            File.WriteAllLines(buttonFilePath, exampleLines);
//        }

//        public class ButtonData
//        {
//            public string Title { get; set; }
//            public string Message { get; set; }
//            public Color ButtonColor { get; set; } = new Color(0.24f, 0.24f, 0.24f, 1f);
//            public string Icon { get; set; } = "";
//        }
//    }
//}
//credits and licenses in the resources folder
using BanMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using static BanMod.Translator;
using static BanMod.Utils;

namespace BanMod
{
    public class MsgMenu : MonoBehaviour
    {
        public static string buttonFilePath = "DATA/OTHER/MENU/buttonmessage.txt";

        public static void Initialize()
        {
            try
            {
                Directory.CreateDirectory("DATA/SETTINGS_CONFIG/MENU/");
            }
            catch (Exception ex)
            {
                Debug.LogError("[BanMod] Errore durante Initialize MsgMenu: " + ex.Message);
            }
        }

        public bool showMenu = false;
        private Rect windowRect;
        private Vector2 windowSize = new Vector2(750, 600);
        private Vector2 scrollPosition = Vector2.zero;

        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle exitButtonStyle;

        public static List<ButtonData> buttonDataList = new List<ButtonData>();
        private int columns = 3;

        // Una sola coda per tutti i pulsanti, senza cooldown o deduplicazione.
        private readonly Queue<string> pendingMessages = new Queue<string>();
        private readonly Dictionary<ButtonData, float> clickFeedback = new Dictionary<ButtonData, float>();
        private const float ClickFeedbackDuration = 0.12f;

        private void SendNextQueuedMessage()
        {
            if (pendingMessages.Count == 0) return;

            string message = pendingMessages.Peek();
            try
            {
                Utils.SendMessage(message);
                pendingMessages.Dequeue();
                MessageBlocker.UpdateLastMessageTime();
            }
            catch (Exception ex)
            {
                // Nessun retry automatico: SendMessage potrebbe aver gia inviato.
                Debug.LogError("[BanMod] Invio messaggio fallito; coda sospesa: " + ex.Message);
                sendingFailed = true;
            }
        }

        private bool sendingFailed;


        public static MsgMenu Instance;

        private float _lastFileCheckTime = 0f;
        private DateTime _lastWriteTimeUtc = DateTime.MinValue;
        private const float FileCheckInterval = 1.5f;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            MenuRouter.OnPanelChanged += HandlePanelChanged;
        }

        private void OnDisable()
        {
            MenuRouter.OnPanelChanged -= HandlePanelChanged;
        }

        private void HandlePanelChanged(MenuRouter.Panel p)
        {
            showMenu = (p == MenuRouter.Panel.MsgMenu);
            if (showMenu)
            {
                LoadButtonData();
                CenterWindow();
            }
        }

        public void OpenMenu()
        {
            if (!showMenu) LoadButtonData();
            ToggleMenu();
        }

        public void CloseMenu()
        {
            if (showMenu) ToggleMenu();
        }

        public bool IsOpen()
        {
            return showMenu;
        }

        void Update()
        {
            // Continua a inviare anche quando il menu e chiuso o si cambia keybind.
            // Un messaggio per frame: preserva l'ordine senza attese artificiali.
            if (!sendingFailed) SendNextQueuedMessage();

            if (KeyBindOptions.IsBindingActive) return;

            if (Input.GetKeyDown(KeyBindOptions.K13) && !BanMod.chatOpen)
            {
                if (MenuRouter.Current == MenuRouter.Panel.MsgMenu)
                    MenuRouter.Open(MenuRouter.Panel.None);
                else
                    MenuRouter.Open(MenuRouter.Panel.MsgMenu);
            }

            if (showMenu)
            {
                AutoReloadIfChanged();
            }
        }

        public void ToggleMenu()
        {
            showMenu = !showMenu;
            if (showMenu)
            {
                LoadButtonData();
                CenterWindow();
            }
        }

        private void CenterWindow()
        {
            float centerX = Screen.width / 2f - windowSize.x / 2f;
            float centerY = Screen.height / 2f - windowSize.y / 2f;
            windowRect = new Rect(centerX, centerY, windowSize.x, windowSize.y);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(BanModUiStyles.TitleLabel)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                buttonStyle = new GUIStyle(BanModUiStyles.TintableButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    fontSize = 15,
                    richText = true
                };

                exitButtonStyle = new GUIStyle(BanModUiStyles.DangerButton)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        void OnGUI()
        {
            if (!showMenu) return;

            EnsureStyles();

            GUI.backgroundColor = Color.white;
            windowRect = GUI.Window(1, windowRect, (GUI.WindowFunction)WindowFunction, "", BanModUiStyles.BlackWindow);

            // I controlli devono ricevere prima gli eventi di pressione/rilascio.
            if (Event.current.isMouse && windowRect.Contains(Event.current.mousePosition))
                Event.current.Use();
        }

        void WindowFunction(int id)
        {
            GUILayout.Label(GetString("MENU_MSG"), titleStyle);
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.Width(windowSize.x - 20),
                GUILayout.Height(windowSize.y - 120)
            );

            ShowButtonContent();

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(GetString("EXIT"), exitButtonStyle, GUILayout.Height(45)))
            {
                MenuRouter.Open(MenuRouter.Panel.None);
            }
            GUI.backgroundColor = Color.white;

            GUI.DragWindow();
        }

        void ShowButtonContent()
        {
            UpdateColumns();

            float availableWidth = windowSize.x - 40f;
            float spacing = 10f;
            float btnWidth = (availableWidth - ((columns - 1) * spacing)) / columns;
            int total = buttonDataList.Count;

            for (int i = 0; i < total; i += columns)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int col = 0; col < columns; col++)
                {
                    int index = i + col;

                    if (index < total)
                    {
                        ButtonData data = buttonDataList[index];

                        bool recentlyClicked = clickFeedback.TryGetValue(data, out float clickedAt)
                            && Time.unscaledTime - clickedAt < ClickFeedbackDuration;
                        GUI.backgroundColor = recentlyClicked
                            ? Color.Lerp(data.ButtonColor, Color.white, 0.55f)
                            : data.ButtonColor;

                        string buttonText = string.IsNullOrWhiteSpace(data.Icon)
                            ? data.Title
                            : $"{data.Icon} {data.Title}";

                        if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Width(btnWidth), GUILayout.Height(50)))
                        {
                            clickFeedback[data] = Time.unscaledTime;
                            pendingMessages.Enqueue(data.Message.Replace("\\n", "\n"));
                        }

                        GUI.backgroundColor = Color.white;

                        if (col < columns - 1)
                            GUILayout.Space(spacing);
                    }
                    else
                    {
                        GUILayout.Space(btnWidth);

                        if (col < columns - 1)
                            GUILayout.Space(spacing);
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }

        private void UpdateColumns()
        {
            float width = Screen.width;

            if (width < 1100f) columns = 2;
            else if (width < 1600f) columns = 3;
            else columns = 4;
        }

        private void AutoReloadIfChanged()
        {
            if (Time.unscaledTime - _lastFileCheckTime < FileCheckInterval)
                return;

            _lastFileCheckTime = Time.unscaledTime;

            try
            {
                if (!File.Exists(buttonFilePath))
                {
                    CreateButtonExampleFile();
                    LoadButtonData();
                    return;
                }

                DateTime currentWrite = File.GetLastWriteTimeUtc(buttonFilePath);
                if (currentWrite != _lastWriteTimeUtc)
                {
                    LoadButtonData();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[BanMod] Errore AutoReload MsgMenu: " + ex.Message);
            }
        }

        void LoadButtonData()
        {
            try
            {
                Directory.CreateDirectory("DATA/OTHER/MENU/");

                if (!File.Exists(buttonFilePath))
                {
                    CreateButtonExampleFile();
                }

                string[] lines = File.ReadAllLines(buttonFilePath);
                buttonDataList.Clear();
                clickFeedback.Clear();

                foreach (var rawLine in lines)
                {
                    if (string.IsNullOrWhiteSpace(rawLine)) continue;

                    string line = rawLine.Trim();

                    if (line.StartsWith("#")) continue;
                    if (line.StartsWith("//")) continue;

                    var data = ParseButtonLine(line);
                    if (data != null)
                    {
                        buttonDataList.Add(data);
                    }
                }

                _lastWriteTimeUtc = File.Exists(buttonFilePath)
                    ? File.GetLastWriteTimeUtc(buttonFilePath)
                    : DateTime.MinValue;
            }
            catch (Exception ex)
            {
                Debug.LogError("[BanMod] Errore LoadButtonData: " + ex.Message);
            }
        }
        [HideFromIl2Cpp]
        private ButtonData ParseButtonLine(string line)
        {
            try
            {
                var parts = line.Split('|');
                if (parts.Length < 2) return null;

                string title = parts[0].Trim();
                string message = parts[1].Trim();

                string colorText = parts.Length >= 3 ? parts[2].Trim() : "";

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                    return null;

                Color buttonColor = ParseColorOrDefault(colorText, new Color(0.24f, 0.24f, 0.24f, 1f));

                return new ButtonData
                {
                    Title = title,
                    Message = message,
                    ButtonColor = buttonColor
                };
            }
            catch (Exception ex)
            {
                Debug.LogError("[BanMod] Riga menu non valida: " + line + " | " + ex.Message);
                return null;
            }
        }

        private Color ParseColorOrDefault(string value, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            value = value.Trim();

            try
            {
                switch (value.ToLowerInvariant())
                {
                    case "red": return new Color(0.80f, 0.15f, 0.15f, 1f);
                    case "green": return new Color(0.20f, 0.65f, 0.20f, 1f);
                    case "blue": return new Color(0.20f, 0.45f, 0.90f, 1f);
                    case "yellow": return new Color(0.85f, 0.75f, 0.20f, 1f);
                    case "orange": return new Color(0.90f, 0.50f, 0.15f, 1f);
                    case "purple": return new Color(0.55f, 0.30f, 0.80f, 1f);
                    case "pink": return new Color(0.90f, 0.35f, 0.65f, 1f);
                    case "gray":
                    case "grey": return new Color(0.35f, 0.35f, 0.35f, 1f);
                    case "white": return new Color(0.85f, 0.85f, 0.85f, 1f);
                    case "black": return new Color(0.15f, 0.15f, 0.15f, 1f);
                }

                if (!value.StartsWith("#"))
                    value = "#" + value;

                if (ColorUtility.TryParseHtmlString(value, out var parsed))
                    return parsed;
            }
            catch
            {
            }

            return fallback;
        }

        void CreateButtonExampleFile()
        {
            string[] exampleLines =
            {
                "# BanMod Button Message Menu",
                "# Formato base:",
                "# Titolo | Messaggio",
                "#",
                "# Formato completo:",
                "# Titolo | Messaggio | Colore",
                "#",
                "# Note:",
                "# - Colore puÃ² essere un nome (red, green, blue, yellow...) o un HEX (#FFAA00)",
                "#",
                "# Esempi:",
                "Hello | Hello everyone!",
                "GG | Good Game! | green ",
                "Bye | See you later! | red ",
                "Rules | Please follow the lobby rules!\\nHave fun! | #3A7BFF ",
                "Discord | Join our Discord server! | purple ",
                "Ready | Everyone ready? | yellow "
            };

            File.WriteAllLines(buttonFilePath, exampleLines);
        }

        public class ButtonData
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public Color ButtonColor { get; set; } = new Color(0.24f, 0.24f, 0.24f, 1f);
            public string Icon { get; set; } = "";
        }
    }
}