//credits and licenses in the resources folder
using System;
using UnityEngine;
using System.Collections.Generic;
using static BanMod.Translator;

namespace BanMod
{
    public class HostControl : MonoBehaviour
    {
        public bool showMenu = false;
        private Rect windowRect;
        private Vector2 windowSize = new Vector2(750, 600);
        private Vector2 scrollPosition = Vector2.zero;
        private bool showBotList;
        private Vector2 botScroll;
        private string botSearch = "";
        private string botStatus = "";

        public enum ItemType { Button, Toggle }

        public class MenuItem
        {
            public string Label;
            public ItemType Type;
            public bool ToggleValue;
            public System.Action OnClick;
        }

        private List<MenuItem> menuItems = new List<MenuItem>();
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle exitButtonStyle;

        public static HostControl Instance;
        void Awake()
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
            showMenu = (p == MenuRouter.Panel.Host);
            showBotList = false;
            if (showMenu)
            {
                SetupMenuContent();
                CenterWindow();
            }
        }

        void Update()
        {
            if (BanMod.IsBanModDisabled) return;

            SharedBotList.Tick();

            if (KeyBindOptions.IsBindingActive) return;

            if (Input.GetKeyDown(KeyBindOptions.K12) && !BanMod.chatOpen)
            {
                if (MenuRouter.Current == MenuRouter.Panel.Host)
                    MenuRouter.Open(MenuRouter.Panel.None);
                else
                    MenuRouter.Open(MenuRouter.Panel.Host);
            }
        }

        public void SetupMenuContent()
        {
            menuItems.Clear();
            menuItems.Add(new MenuItem { Label = GetString("Host_Start"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/start", new string[] { "/start" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("InstantStartAction"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/instantstart", new string[] { "/instantstart" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("CallMeetingAction"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/meeting", new string[] { "/meeting" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("Host_SkipMeeting"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/skipmeeting", new string[] { "/skipmeeting" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("Host_EndMeeting"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/endmeeting", new string[] { "/endmeeting" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("Host_EndGame"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/endgame", new string[] { "/endgame" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("DestroyLobbyAction"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/destroy", new string[] { "/destroy" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("RecreateLobbyAction"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/lobby", new string[] { "/lobby" }, ""); } });
            menuItems.Add(new MenuItem { Label = GetString("Host_SendInfo"), Type = ItemType.Button, OnClick = () => { ChatCommands.HandleCommand("/m", new string[] { "/m" }, ""); } });

            if (SharedBotList.CanManage())
            {
                menuItems.Add(new MenuItem
                {
                    Label = "Lista bot",
                    Type = ItemType.Button,
                    OnClick = () =>
                    {
                        showBotList = true;
                        botScroll = Vector2.zero;
                        botStatus = "";
                    }
                });
            }
        }

        public void OpenMenu()
        {
            SetupMenuContent();
            showBotList = false;
            showMenu = true;
            CenterWindow();
        }

        public void CloseMenu()
        {
            showMenu = false;
            showBotList = false;
        }

        public bool IsOpen() => showMenu;

        void CenterWindow()
        {
            windowRect = new Rect(Screen.width / 2 - windowSize.x / 2, Screen.height / 2 - windowSize.y / 2, windowSize.x, windowSize.y);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(BanModUiStyles.TitleLabel)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter
                };

                buttonStyle = new GUIStyle(BanModUiStyles.ButtonDark)
                {
                    alignment = TextAnchor.MiddleCenter
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
            if (!showMenu || BanMod.IsBanModDisabled)
                return;
            EnsureStyles();

            GUI.backgroundColor = Color.white;
            windowRect = GUI.Window(4, windowRect, (GUI.WindowFunction)DrawWindow, "", BanModUiStyles.BlackWindow);

            if (Event.current.isMouse && windowRect.Contains(Event.current.mousePosition))
                Event.current.Use();
        }

        private void DrawBotList()
        {
            GUILayout.Label("Lista bot", titleStyle);
            GUILayout.Space(10);
            GUILayout.Label("Cerca per nome o Friend Code");
            botSearch = GUILayout.TextField(botSearch, 80);

            if (!string.IsNullOrEmpty(botStatus))
                GUILayout.Label(botStatus);

            bool canManage = SharedBotList.CanManage();
            if (!canManage)
                GUILayout.Label("Puoi gestire la lista quando ospiti una lobby online.");

            string query = botSearch.Trim();
            var entries = SharedBotList.GetEntries();
            bool found = false;

            botScroll = GUILayout.BeginScrollView(botScroll);
            foreach (var entry in entries)
            {
                if (query.Length > 0 &&
                    (entry.PlayerName ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    entry.FriendCode.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                found = true;
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"{entry.PlayerName} · {entry.FriendCode} · {entry.Reason}");

                if (entry.FlaggedByOthers)
                    GUILayout.Label("⚠ Segnalato come falso positivo da un altro utente");

                if (entry.UnlockedByMe)
                    GUILayout.Label("Sbloccato per me");

                bool wasEnabled = GUI.enabled;
                GUI.enabled = wasEnabled && canManage && !SharedBotList.IsBusy;
                string label = entry.UnlockedByMe
                    ? "Annulla sblocco"
                    : "Segnala falso positivo e sblocca per me";

                bool pressed = GUILayout.Button(label, buttonStyle, GUILayout.Height(34));
                GUI.enabled = wasEnabled;

                if (pressed)
                {
                    bool unlock = !entry.UnlockedByMe;
                    botStatus = "Invio in corso…";
                    SharedBotList.SetException(entry.FriendCode, unlock, success =>
                    {
                        botStatus = success
                            ? (unlock ? "Segnalazione inviata. Sbloccato per te." : "Sblocco annullato.")
                            : "Operazione non riuscita. Riprova tra poco.";
                    });
                }

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            if (!found)
                GUILayout.Label(SharedBotList.IsBusy ? "Caricamento…" : "Nessuna voce da mostrare.");

            GUILayout.EndScrollView();

            if (GUILayout.Button("Indietro", exitButtonStyle, GUILayout.Height(45)))
                showBotList = false;
        }

        void DrawWindow(int id)
        {
            if (showBotList)
            {
                DrawBotList();
                GUI.DragWindow();
                return;
            }

            GUILayout.Label(GetString("HostControlMenu"), titleStyle);
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            float btnWidth = (windowSize.x / 3) - 30;
            for (int i = 0; i < menuItems.Count; i += 3)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int col = 0; col < 3; col++)
                {
                    int index = i + col;
                    if (index < menuItems.Count)
                    {
                        var item = menuItems[index];
                        if (item.Type == ItemType.Button)
                        {
                            if (GUILayout.Button(item.Label, buttonStyle, GUILayout.Width(btnWidth), GUILayout.Height(50)))
                            {
                                item.OnClick?.Invoke();
                            }
                        }
                        else if (item.Type == ItemType.Toggle)
                        {
                            bool newValue = GUILayout.Toggle(item.ToggleValue, item.Label + (item.ToggleValue ? " [ON]" : " [OFF]"), item.ToggleValue ? BanModUiStyles.ToggleOnBlueOutline : BanModUiStyles.ToggleOffDark, GUILayout.Width(btnWidth), GUILayout.Height(50));
                            if (newValue != item.ToggleValue)
                            {
                                item.ToggleValue = newValue;
                                item.OnClick?.Invoke();
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Space(btnWidth);
                    }
                    if (col < 2) GUILayout.Space(10);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(GetString("ExitButton"), exitButtonStyle, GUILayout.Height(50)))
            {
                MenuRouter.Open(MenuRouter.Panel.None);
            }
            GUI.backgroundColor = Color.white;

            GUI.DragWindow();
        }
    }
}