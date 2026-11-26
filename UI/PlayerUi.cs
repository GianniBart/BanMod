//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using static BanMod.Translator;
using static BanMod.Utils;

namespace BanMod
{
    public class PlayerUI : MonoBehaviour
    {
        public static PlayerUI Instance;

        private Rect windowRect;
        private Vector2 windowSize = new Vector2(1450, 820);

        public PlayerControl CurrentSelectedPlayer => selectedPlayer;

        private GUIStyle buttonStyle;
        private GUIStyle buttonStyle1;
        private GUIStyle titleStyle;
        private GUIStyle titleStyle1;
        private GUIStyle titleStyle2;
        private GUIStyle titleStyle3;
        private GUIStyle titleStyle4;
        private GUIStyle textBoxStyle;
        private GUIStyle textInsideBoxStyle;

        public bool showActionMenu = false;
        public bool open = false;
        private PlayerControl selectedPlayer;
        private Vector2 scrollPos;

        private string inputText = "";
        public bool editingInput = false;
        private float caretTimer = 0f;
        private bool caretVisible = true;
        public static Vector3? originalScale = null;

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
            if (p == MenuRouter.Panel.PlayerUI)
            {
                if (!open) OpenPPM();
            }
            else
            {
                if (open || showActionMenu) ClosePPM();
            }
        }

        private void Start()
        {
            if (BanMod.IsBanModDisabled) return;
            windowRect = new Rect(
                Screen.width / 2f - windowSize.x / 2f,
                Screen.height / 2f - windowSize.y / 2f,
                windowSize.x,
                windowSize.y
            );
        }

        public void OpenSetRolesForCurrentPlayer()
        {
            if (selectedPlayer == null || SetPlayerUi.Instance == null)
                return;

            PlayerControl target = selectedPlayer;

            CloseActionMenu();
            SetPlayerUi.Instance.OpenActionMenu(target);
        }

        public void ReopenActionMenu(PlayerControl pc)
        {
            if (pc == null) return;

            selectedPlayer = pc;
            showActionMenu = true;
            open = false;
        }

        private void Update()
        {
            if (BanMod.IsBanModDisabled) return;

            if (KeyBindOptions.IsBindingActive) return;

            if (Input.GetKeyDown(KeyBindOptions.K16) && !BanMod.chatOpen)
            {
                ResetInputFocus();

                if (MenuRouter.Current == MenuRouter.Panel.PlayerUI)
                    MenuRouter.Open(MenuRouter.Panel.None);
                else
                    MenuRouter.Open(MenuRouter.Panel.PlayerUI);

                return;
            }

            HandleInlineTextInput();

            caretTimer += Time.unscaledDeltaTime;
            if (caretTimer >= 0.5f)
            {
                caretTimer = 0f;
                caretVisible = !caretVisible;
            }
        }

        private void HandleInlineTextInput()
        {
            if (!editingInput) return;

            string input = Input.inputString;

            if (!string.IsNullOrEmpty(input))
            {
                foreach (char c in input)
                {
                    if (c == '\b' || c == '\n' || c == '\r') continue;
                    if (inputText.Length >= 200) break;
                    inputText += c;
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && inputText.Length > 0)
                inputText = inputText.Substring(0, inputText.Length - 1);

            if (Input.GetKeyDown(KeyCode.Escape))
                editingInput = false;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (AmongUsClient.Instance.AmHost && inputText.Length > 0)
                {
                    Utils.SendMessage(inputText, 255);
                    inputText = "";
                }

                editingInput = false;
            }
        }

        private void EnsureStyles()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(BanModUiStyles.ButtonDark)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            if (buttonStyle1 == null)
            {
                buttonStyle1 = new GUIStyle(BanModUiStyles.DangerButton)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(BanModUiStyles.TitleLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            if (titleStyle1 == null)
            {
                titleStyle1 = new GUIStyle(BanModUiStyles.TitleLabel)
                {
                    fontSize = 28,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                titleStyle1.normal.textColor = BanModUiStyles.AccentColor;
            }

            if (titleStyle2 == null)
            {
                titleStyle2 = new GUIStyle(BanModUiStyles.HeaderLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                titleStyle2.normal.textColor = BanModUiStyles.DangerColor;
            }

            if (titleStyle3 == null)
            {
                titleStyle3 = new GUIStyle(BanModUiStyles.HeaderLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                titleStyle3.normal.textColor = BanModUiStyles.AccentHoverColor;
            }

            if (titleStyle4 == null)
            {
                titleStyle4 = new GUIStyle(BanModUiStyles.HeaderLabel)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                titleStyle4.normal.textColor = BanModUiStyles.AccentHoverColor;
            }

            if (textBoxStyle == null)
            {
                textBoxStyle = new GUIStyle(BanModUiStyles.DarkBox)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 30,
                    fontStyle = FontStyle.Bold
                };

                textBoxStyle.padding = new RectOffset
                {
                    left = 10,
                    right = 10,
                    top = 10,
                    bottom = 10
                };
            }

            if (textInsideBoxStyle == null)
            {
                textInsideBoxStyle = new GUIStyle(BanModUiStyles.BodyLabel)
                {
                    fontSize = 30,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (showActionMenu)
            {
                GUI.color = Color.white;
                windowRect = GUI.Window(29292, windowRect, (GUI.WindowFunction)DrawActionWindow, "", BanModUiStyles.BlackWindow);
            }
        }

        private class CategoryBlock
        {
            public string Title;
            public Action DrawContent;

            public CategoryBlock(string title, Action draw)
            {
                Title = title;
                DrawContent = draw;
            }
        }
        [HideFromIl2Cpp]
        private void DrawCategoryColumn(List<CategoryBlock> column, float width)
        {
            GUILayout.BeginVertical(GUILayout.Width(width));

            foreach (var category in column)
            {
                GUILayout.Label(category.Title, titleStyle4);
                category.DrawContent();
                GUILayout.Space(12);
            }

            GUILayout.EndVertical();
        }

        private void DrawActionWindow(int id)
        {
            if (!CheckAndCloseIfPlayerInvalid())
                return;

            float scrollHeight = Mathf.Max(windowRect.height - 200f, 120f);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(scrollHeight));
            GUILayout.Space(5);

            bool selectedIsHost = selectedPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;

            GUILayout.BeginVertical();
            {
                Rect rectInput;
                if (Event.current.type == EventType.Layout)
                {
                    rectInput = new Rect(0, 0, 0, 0);
                    GUILayoutUtility.GetRect(windowRect.width - 60, 50);
                }
                else
                {
                    rectInput = GUILayoutUtility.GetRect(windowRect.width - 60, 50);
                }

                GUI.color = Color.white;
                GUI.Box(rectInput, "", textBoxStyle);

                Color borderColor = editingInput ? Color.cyan : Color.gray;
                GUI.color = borderColor;
                GUI.Box(new Rect(rectInput.x, rectInput.y, rectInput.width, 2), "");
                GUI.Box(new Rect(rectInput.x, rectInput.y + rectInput.height - 2, rectInput.width, 2), "");
                GUI.Box(new Rect(rectInput.x, rectInput.y, 2, rectInput.height), "");
                GUI.Box(new Rect(rectInput.x + rectInput.width - 2, rectInput.y, 2, rectInput.height), "");
                GUI.color = Color.white;

                string displayInput = inputText;

                if (string.IsNullOrEmpty(inputText) && !editingInput)
                {
                    displayInput = GetString("Typehere");
                    GUIStyle placeholderStyle = new GUIStyle(textInsideBoxStyle);
                    placeholderStyle.normal.textColor = new Color(1, 1, 1, 0.4f);

                    GUI.Label(
                        new Rect(rectInput.x + 15, rectInput.y + 5, rectInput.width - 20, rectInput.height - 10),
                        displayInput,
                        placeholderStyle
                    );
                }
                else
                {
                    if (editingInput && caretVisible)
                        displayInput += "|";

                    GUI.Label(
                        new Rect(rectInput.x + 15, rectInput.y + 5, rectInput.width - 20, rectInput.height - 10),
                        displayInput,
                        textInsideBoxStyle
                    );
                }

                if (Event.current.type == EventType.MouseDown && rectInput.Contains(Event.current.mousePosition))
                {
                    editingInput = true;
                    Event.current.Use();
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(15);

            CategoryBlock rolesCat = new CategoryBlock("ROLES", () =>
            {
                if (GUILayout.Button("SET ROLES", buttonStyle, GUILayout.Height(40)) && AmongUsClient.Instance.AmHost)
                {
                    OpenSetRolesForCurrentPlayer();
                }
                if (GUILayout.Button("REMOVE ALL ROLES", buttonStyle, GUILayout.Height(40)) && AmongUsClient.Instance.AmHost)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        BanMod.forcedImpostorIds.Remove(selectedPlayer.PlayerId);
                        BanMod.forceImpostor = false;
                        ForcedRoleSystem.Clear();
                        Guesser.SpecialKillerId = 255;
                        Guesser.SpecialKillerSelected = false;
                        Jester.JesterId = 255;
                        Jester.JesterSelected = false;
                        Jester.ForcedJesterSelected = false;
                        Exiler.ExilerId = 255;
                        Exiler.ExilerSelected = false;
                        ChatCommands.ShowChat($"Removed: ALL");
                    }
                }
            });
            CategoryBlock outfitCat = new CategoryBlock(GetString("PUI_Cat_Outfit"), () =>
            {
                if (GUILayout.Button(GetString("PUI_CopyOutfit"), buttonStyle, GUILayout.Height(45)))
                {
                    Utils.SaveOriginalOutfit(PlayerControl.LocalPlayer);
                    Utils.CopyOutfit(selectedPlayer, PlayerControl.LocalPlayer);
                    CloseActionMenu();
                }

                if (GUILayout.Button(GetString("PUI_RestoreOutfit"), buttonStyle, GUILayout.Height(45)))
                {
                    Utils.RestoreOriginalOutfit(PlayerControl.LocalPlayer);
                    CloseActionMenu();
                }

                if (GUILayout.Button(GetString("PUI_CycleOutfit"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        SkinUI.Instance.ForceCycleOutfit();
                    }
                }
            });

            CategoryBlock hostCat = new CategoryBlock(GetString("PUI_Cat_Host"), () =>
            {
                if (GUILayout.Button(GetString("PUI_Eject"), buttonStyle, GUILayout.Height(45)))
                {
                    if (Utils.SondaggioEspulsione(selectedPlayer))
                        CloseActionMenu();
                }

                if (GUILayout.Button(GetString("KickPlayerAction"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        AmongUsClient.Instance.KickPlayer(selectedPlayer.OwnerId, false);
                        CloseActionMenu();
                    }
                }

                if (GUILayout.Button(GetString("BanPlayerAction"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var client = AmongUsClient.Instance.allClients.ToArray()
                            .FirstOrDefault(c => c.Id == selectedPlayer.OwnerId);

                        if (client != null)
                        {
                            BanManager.AddBanPlayer(client, inputText);
                            AmongUsClient.Instance.KickPlayer(client.Id, true);
                        }

                        inputText = "";
                        CloseActionMenu();
                    }
                }
            });

            CategoryBlock teleportCat = null;
            if (!selectedIsHost)
            {
                teleportCat = new CategoryBlock("Teleport", () =>
                {
                    if (GUILayout.Button(GetString("PUI_TeleportTo"), buttonStyle, GUILayout.Height(45)))
                    {
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(selectedPlayer.transform.position);
                        CloseActionMenu();
                    }
                });
            }

            CategoryBlock colorCat = new CategoryBlock(GetString("PUI_Cat_Color"), () =>
            {
                if (GUILayout.Button(GetString("PUI_RandomColor"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        List<byte> usedColors = new();
                        foreach (var p in GameData.Instance.AllPlayers)
                            usedColors.Add((byte)p.DefaultOutfit.ColorId);

                        List<byte> allColors = Enumerable.Range(0, Palette.PlayerColors.Length).Select(i => (byte)i).ToList();
                        List<byte> freeColors = allColors.Where(c => !usedColors.Contains(c)).ToList();
                        if (freeColors.Count == 0) freeColors.Add(0);

                        System.Random rng = new();
                        byte color = freeColors[rng.Next(freeColors.Count)];

                        selectedPlayer.RpcSetColor(color);
                    }
                }

                if (GUILayout.Button(GetString("PUI_SetColor"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var color = Utils.MsgToColor(inputText, true);
                        selectedPlayer.RpcSetColor(color);
                    }
                }

                if (GUILayout.Button(GetString("PUI_SetColorAll"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var color = Utils.MsgToColor(inputText, true);
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            player.RpcSetColor(color);
                        }
                    }
                }

                string rainbowLabel = (BanMod.RainbowTarget == selectedPlayer) ? "<color=green>Rainbow: ON</color>" : "Rainbow: OFF";

                if (GUILayout.Button(rainbowLabel, buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost && selectedPlayer != null)
                    {
                        if (BanMod.RainbowTarget == selectedPlayer)
                        {
                            BanMod.RainbowTarget = null;
                        }
                        else
                        {
                            BanMod.RainbowTarget = selectedPlayer;
                            BanMod.EveryRandomActive = false;
                        }
                    }
                }

                string rainbowAllLabel = BanMod.EveryRandomActive ? "<color=green>Rainbow All: ON</color>" : "Rainbow All: OFF";

                if (GUILayout.Button(rainbowAllLabel, buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        BanMod.EveryRandomActive = !BanMod.EveryRandomActive;

                        if (BanMod.EveryRandomActive)
                        {
                            BanMod.RainbowTarget = null;
                        }
                    }
                }
            });

            CategoryBlock msgCat = new CategoryBlock(GetString("PUI_Cat_VanillaMsg"), () =>
            {
                if (GUILayout.Button(GetString("PUI_MsgPlayer"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        Utils.SendMessage(inputText, selectedPlayer.PlayerId);
                        inputText = "";
                    }
                }

                if (GUILayout.Button(GetString("PUI_MsgAll"), buttonStyle, GUILayout.Height(45)))
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        Utils.SendMessage(inputText);
                        inputText = "";
                    }
                }
            });

            CategoryBlock listsCat = new CategoryBlock(GetString("PUI_Cat_Lists"), () =>
            {
                if (GUILayout.Button(GetString("PUI_AddVip"), buttonStyle, GUILayout.Height(45)))
                {
                    AllowedManager.ManageVip(selectedPlayer.PlayerId.ToString(), true);
                    CloseActionMenu();
                }

                if (GUILayout.Button(GetString("PUI_RemoveVip"), buttonStyle, GUILayout.Height(45)))
                {
                    AllowedManager.ManageVip(selectedPlayer.PlayerId.ToString(), false);
                    CloseActionMenu();
                }

                if (GUILayout.Button(GetString("PUI_AddMod"), buttonStyle, GUILayout.Height(45)))
                {
                    AllowedManager.ManageModerator(selectedPlayer.PlayerId.ToString(), true);
                    CloseActionMenu();
                }

                if (GUILayout.Button(GetString("PUI_RemoveMod"), buttonStyle, GUILayout.Height(45)))
                {
                    AllowedManager.ManageModerator(selectedPlayer.PlayerId.ToString(), false);
                    CloseActionMenu();
                }
            });

            List<CategoryBlock> leftColumn = new();
            List<CategoryBlock> centerColumn = new();
            List<CategoryBlock> rightColumn = new();

            leftColumn.Add(rolesCat);
            leftColumn.Add(outfitCat);

            centerColumn.Add(hostCat);
            centerColumn.Add(colorCat);

            rightColumn.Add(msgCat);
            rightColumn.Add(listsCat);
            if (teleportCat != null) rightColumn.Add(teleportCat);

            float columnWidth = (windowRect.width - 70f) / 3f;

            GUILayout.BeginHorizontal();

            DrawCategoryColumn(leftColumn, columnWidth);
            GUILayout.Space(15);

            DrawCategoryColumn(centerColumn, columnWidth);
            GUILayout.Space(15);

            DrawCategoryColumn(rightColumn, columnWidth);

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            if (GUILayout.Button(GetString("ExitButton"), buttonStyle1, GUILayout.Height(45)))
                MenuRouter.Open(MenuRouter.Panel.None);

            GUI.DragWindow();
        }

        private bool CheckAndCloseIfPlayerInvalid()
        {
            if (selectedPlayer == null)
            {
                CloseActionMenu();
                return false;
            }

            var playerInfo = GameData.Instance?.GetPlayerById(selectedPlayer.PlayerId);

            if (playerInfo == null || playerInfo.Disconnected)
            {
                ChatCommands.ShowChat($"<color=#FF0000>Error: Player {selectedPlayer.name} (ID: {selectedPlayer.PlayerId}) is no longer available. Menu closed.</color>");
                CloseActionMenu();
                return false;
            }

            return true;
        }

        private ClientData GetClientDataForSelectedPlayer()
        {
            if (selectedPlayer == null) return null;
            return AmongUsClient.Instance?.GetClient(selectedPlayer.OwnerId);
        }

        private void ResetInputFocus()
        {
            editingInput = false;
            caretTimer = 0f;
            caretVisible = true;

            try
            {
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
            }
            catch { }
        }

        private void CloseActionMenu()
        {
            ResetInputFocus();
            ClosePPM();
            showActionMenu = false;
            selectedPlayer = null;
        }

        public void OpenActionMenu(PlayerControl pc)
        {
            selectedPlayer = pc;
            showActionMenu = true;
        }

        public void OpenPPM()
        {
            ResetInputFocus();

            var list = Utils.GetAllPlayerData();
            PlayerPickMenu.openPlayerPickMenu(list, PlayerPickMenuBridge.Action);
            open = true;
        }

        public void ClosePPM()
        {
            ResetInputFocus();

            showActionMenu = false;
            selectedPlayer = null;

            if (PlayerPickMenu.playerpickMenu != null)
            {
                try { PlayerPickMenu.playerpickMenu.Close(); }
                catch { }
            }

            PlayerPickMenu.playerpickMenu = null;
            PlayerPickMenu.IsActive = false;
            open = false;
        }

        public static class PlayerPickMenuBridge
        {
            public static System.Action<PlayerControl> pendingAction;
            public static readonly System.Action Action = (System.Action)(System.Action)OnPlayerChosen;

            public static void OnPlayerChosen()
            {
                try
                {
                    var data = PlayerPickMenu.targetPlayerData;

                    if (data == null)
                    {
                        ResetPPM();
                        return;
                    }

                    PlayerControl target = Utils.GetPlayerById(data.PlayerId);

                    if (target != null)
                    {
                        PlayerUI.Instance.OpenActionMenu(target);
                    }
                }
                finally
                {
                    ResetPPM();
                }
            }

            public static void ResetPPM()
            {
                pendingAction = null;
                PlayerPickMenu.targetPlayerData = null;

                if (PlayerPickMenu.playerpickMenu != null)
                {
                    try { PlayerPickMenu.playerpickMenu.Close(); }
                    catch { }
                }

                PlayerPickMenu.playerpickMenu = null;
                PlayerPickMenu.IsActive = false;
            }
        }
    }
}