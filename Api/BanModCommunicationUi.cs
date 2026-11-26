//credits and licenses in the resources folder/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Attributes;

namespace BanMod
{
    public class BanModCommunicationUi : MonoBehaviour
    {
        public static BanModCommunicationUi Instance;

        private GameObject uiRoot;
        private GameObject inputBlocker;
        private GameObject mainPanel;
        private GameObject homePanel;
        private GameObject bugPanel;
        private GameObject playerReportPanel;
        private GameObject supportPanel;
        private GameObject sentReportsPanel;
        private GameObject sentReportsListPanel;
        private GameObject popupPanel;
        private GameObject playerListPanel;

        private TextMeshProUGUI mainTitleText;
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI popupTitleText;
        private TextMeshProUGUI popupBodyText;
        private ScrollRect popupBodyScrollRect;
        private GameObject popupReportDetailsPanel;
        private TextMeshProUGUI popupReportDetailsText;
        private TextMeshProUGUI popupReportChatText;
        private ScrollRect popupReportChatScrollRect;
        private TextMeshProUGUI popupStatusText;
        private TextMeshProUGUI popupReportInputLabel;
        private TextMeshProUGUI bugCustomRolesText;
        private TextMeshProUGUI bugOtherModsText;
        private TextMeshProUGUI selectedPlayerText;
        private TextMeshProUGUI sentReportsInfoText;
        private TextMeshProUGUI sentReportDetailsText;
        private TextMeshProUGUI sentReportChatText;
        private TextMeshProUGUI communicationsUnreadBadgeText;
        private TextMeshProUGUI sentReportsUnreadBadgeText;
        private TextMeshProUGUI floatingUnreadBadgeText;

        private TMP_InputField bugTitleInput;
        private TMP_InputField bugGameModeInput;
        private TMP_InputField bugCustomRolesWhichInput;
        private TMP_InputField bugOtherModsWhichInput;
        private TMP_InputField bugDescriptionInput;
        private TMP_InputField playerReasonInput;
        private TMP_InputField supportTitleInput;
        private TMP_InputField supportMessageInput;
        private TMP_InputField sentReportChatInput;
        private TMP_InputField popupReportChatInput;

        private Button sendBugButton;
        private Button sendPlayerButton;
        private Button sendSupportButton;
        private Button sentReportDeleteButton;
        private Button sentReportCloseButton;
        private Button sentReportChatSendButton;
        private Button sentReportRefreshButton;
        private Button popupCloseButton;
        private Button popupReportSendButton;
        private Button popupReportCloseButton;
        private Button popupReportDeleteButton;
        private Button floatingUnreadButton;
        private Button bugCustomYesButton;
        private Button bugCustomNoButton;
        private Button bugOtherYesButton;
        private Button bugOtherNoButton;

        private TextMeshProUGUI bugCustomYesText;
        private TextMeshProUGUI bugCustomNoText;
        private TextMeshProUGUI bugOtherYesText;
        private TextMeshProUGUI bugOtherNoText;

        private bool showMenu = false;
        private bool popupVisible = false;
        private bool acknowledgementPending;
        private int popupRevision;
        private bool popupIsReportChat = false;
        private bool isSending = false;
        private bool customRolesEnabled = false;
        private bool otherModsInstalled = false;

        private string targetFriendCode = "";
        private string targetName = "";
        private string targetHashedPuid = "";
        private string targetPlayerId = "";
        private string targetPlatform = "";
        private Action popupOnClose;
        private int popupReportId = 0;
        private bool popupReportIsOpen = false;
        private List<BanModCommunicationManager.ReportSummary> sentReports = new List<BanModCommunicationManager.ReportSummary>();
        private int selectedSentReportId = 0;
        private int pendingSelectSentReportId = 0;

        private bool draggingMainPanel = false;
        private bool draggingPopupPanel = false;
        private Vector2 lastDragMousePosition = Vector2.zero;
        private static int pendingUnreadCount = 0;

        private enum PanelMode
        {
            Home,
            BugReport,
            PlayerReport,
            Support,
            SentReports
        }

        private PanelMode currentPanel = PanelMode.Home;

        public static bool IsUiOpen
        {
            get
            {
                try { return Instance != null && (Instance.showMenu || Instance.popupVisible); }
                catch { return false; }
            }
        }

        public static void SetUnreadCount(int count)
        {
            pendingUnreadCount = Mathf.Max(0, count);
            try
            {
                if (pendingUnreadCount > 0)
                {
                    EnsureCreated();
                    Instance?.BuildUiIfNeeded();
                }

                Instance?.RefreshUnreadBadge();
                Instance?.RefreshVisibility();
            }
            catch { }
        }

        private static bool ensureCreateErrorLogged = false;

        public static void EnsureCreated()
        {
            try
            {
                if (Instance != null)
                    return;

                if (BanMod.Instance == null)
                    return;

                Instance = BanMod.Instance.AddComponent<BanModCommunicationUi>();
            }
            catch (Exception ex)
            {
                if (!ensureCreateErrorLogged)
                {
                    ensureCreateErrorLogged = true;
                    try { Debug.LogError("[BANMOD] Failed to create BanModCommunicationUi: " + ex.Message); } catch { }
                }
            }
        }

        private void Awake()
        {
            Instance = this;

        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            try
            {
                if (uiRoot != null)
                    UnityEngine.Object.Destroy(uiRoot);
            }
            catch { }
        }

        private void Update()
        {
            try
            {
                if (KeyBindOptions.IsBindingActive)
                    return;
            }
            catch { }


            HandleWindowDrag();
            HandlePopupBodyManualScroll();

        }

        public void ToggleMenu()
        {
            if (showMenu)
                CloseMenu();
            else
                OpenMenu();
        }

        public void OpenMenu()
        {
            BuildUiIfNeeded();
            showMenu = true;
            OpenHomePanel();
            RefreshTexts();
            RefreshVisibility();
        }

        public void CloseMenu()
        {
            showMenu = false;
            RefreshVisibility();
        }

        [HideFromIl2Cpp]
        internal bool TryShowBackgroundMessagePopup(string title, string content, Action onClose)
        {
            if (acknowledgementPending || popupVisible) return false;
            ShowMessagePopup(title, content, onClose);
            return popupVisible;
        }

        [HideFromIl2Cpp]
        public void ShowMessagePopup(string title, string content, Action onClose = null)
        {
            // A security acknowledgement must never be replaced by a background message.
            if (acknowledgementPending) return;
            BuildUiIfNeeded();
            popupRevision++;
            if (popupCloseButton != null)
                SetText(popupCloseButton.GetComponentInChildren<TextMeshProUGUI>(), T("Comm_ClosePopup", "Close popup"));

            InvokeCurrentPopupCallbackBeforeReplace();
            popupOnClose = onClose;
            popupVisible = true;
            popupIsReportChat = false;
            popupReportId = 0;
            popupReportIsOpen = false;

            string safeTitle = string.IsNullOrWhiteSpace(title)
                ? T("Comm_ServerMessageTitle", "BANMOD Message")
                : title.Trim();
            string safeContent = content ?? "";

            PrepareSimplePopupVisuals(safeTitle, safeContent);
            RefreshVisibility();
            PrepareSimplePopupVisuals(safeTitle, safeContent);

            RunCommunicationCoroutine(RefreshSimplePopupNextFrames(safeTitle, safeContent, popupRevision));
        }

        [HideFromIl2Cpp]
        public bool ShowSecurityAcknowledgement(string title, string content, Action onOk)
        {
            if (acknowledgementPending) return false;
            ShowMessagePopup(title, content, onOk);
            if (popupPanel == null || popupCloseButton == null) return false;
            acknowledgementPending = true;
            SetText(popupCloseButton.GetComponentInChildren<TextMeshProUGUI>(), "OK");
            return true;
        }

        private void PrepareSimplePopupVisuals(string title, string content)
        {
            try
            {
                GameObject bodyRoot = GetScrollableRoot(popupBodyText);
                SetActive(bodyRoot, true);

                SetText(popupTitleText, title ?? "");
                SetText(popupReportDetailsText, "");
                SetScrollableText(popupReportChatText, "", true);
                SetText(popupStatusText, "");
                SetInputText(popupReportChatInput, "");
                SetScrollableText(popupBodyText, content ?? "", true);

                if (popupBodyText != null)
                {
                    popupBodyText.enabled = true;
                    popupBodyText.SetLayoutDirty();
                    popupBodyText.SetVerticesDirty();
                    popupBodyText.ForceMeshUpdate(true, true);
                }

                if (bodyRoot != null)
                {
                    RectMask2D mask = bodyRoot.GetComponentInChildren<RectMask2D>(true);
                    if (mask != null)
                    {
                        mask.enabled = false;
                        mask.enabled = true;
                    }
                }

                if (popupBodyScrollRect != null)
                {
                    popupBodyScrollRect.enabled = false;
                    popupBodyScrollRect.enabled = true;
                    popupBodyScrollRect.StopMovement();
                    popupBodyScrollRect.verticalNormalizedPosition = 1f;
                }

                Canvas.ForceUpdateCanvases();
            }
            catch { }
        }

        [HideFromIl2Cpp]
        private IEnumerator RefreshSimplePopupNextFrames(string title, string content, int revision)
        {
            yield return null;

            if (!popupVisible || popupIsReportChat || revision != popupRevision)
                yield break;

            PrepareSimplePopupVisuals(title, content);

            yield return null;

            if (!popupVisible || popupIsReportChat || revision != popupRevision)
                yield break;

            PrepareSimplePopupVisuals(title, content);
        }

        [HideFromIl2Cpp]
        public void ShowReportChatPopup(BanModCommunicationManager.ReportSummary report, Action onClose = null)
        {
            if (acknowledgementPending || report == null || report.Id <= 0)
                return;

            BuildUiIfNeeded();
            popupRevision++;

            InvokeCurrentPopupCallbackBeforeReplace();
            popupOnClose = onClose;
            popupVisible = true;
            popupIsReportChat = true;
            popupReportId = report.Id;
            popupReportIsOpen = IsReportOpen(report);

            RefreshVisibility();

            UpsertCachedReport(report);

            string typeLabel = ReportTypeLabel(report);

            string titleText = "#" + report.Id + " - " + typeLabel;
            if (!string.IsNullOrWhiteSpace(report.Title))
                titleText += " — " + ShortText(report.Title, 48);

            SetText(popupTitleText, titleText);
            SetScrollableText(popupBodyText, "", true);
            SetText(popupReportDetailsText, BuildReportDetailsText(report));
            SetScrollableText(popupReportChatText, BuildReportChatText(report), false);
            SetText(popupStatusText, popupReportIsOpen
                ? T("Comm_ReportPopupHint", "Write a reply below. This chat is only for this report.")
                : T("Comm_ReportClosedHint", "This report is closed/resolved. You can delete it from your list or close the popup."));
            SetInputText(popupReportChatInput, "");

            report.IsUnread = false;
            report.UnreadCount = 0;
            BanModMessagePoller.MarkReportReadFromUi(report.Id);
            RefreshUnreadBadge();

            Canvas.ForceUpdateCanvases();
            FocusPopupReportInput();
        }

        private void InvokeCurrentPopupCallbackBeforeReplace()
        {
            if (!popupVisible || popupOnClose == null)
                return;

            Action callback = popupOnClose;
            popupOnClose = null;

            try { callback?.Invoke(); } catch { }
        }

        private void ClosePopup()
        {
            if (!popupVisible)
                return;

            Action callback = popupOnClose;
            popupVisible = false;
            acknowledgementPending = false;
            popupRevision++;
            popupIsReportChat = false;
            popupReportId = 0;
            popupReportIsOpen = false;
            popupOnClose = null;
            SetInputText(popupReportChatInput, "");
            RefreshVisibility();

            try { callback?.Invoke(); } catch { }
        }

        private void BuildUiIfNeeded()
        {
            if (uiRoot != null)
                return;

            uiRoot = new GameObject("BanMod_CommunicationCanvas");
            UnityEngine.Object.DontDestroyOnLoad(uiRoot);

            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30000;

            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            uiRoot.AddComponent<GraphicRaycaster>();

            CreateInputBlocker();
            CreateFloatingUnreadBadge();
            CreateMainPanel();
            CreatePopupPanel();
        }

        private void CreateInputBlocker()
        {
            try
            {
                inputBlocker = new GameObject("InputBlocker");
                inputBlocker.transform.SetParent(uiRoot.transform, false);

                RectTransform rect = inputBlocker.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;

                Image image = inputBlocker.AddComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0.001f);
                image.raycastTarget = true;

                Button blockerButton = inputBlocker.AddComponent<Button>();
                blockerButton.targetGraphic = image;
                blockerButton.transition = Selectable.Transition.None;
                Action value = () => { };
                blockerButton.onClick.AddListener(value);

                CanvasGroup group = inputBlocker.AddComponent<CanvasGroup>();
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;

                inputBlocker.transform.SetAsFirstSibling();
                try { Debug.Log("[BANMOD] Communication UI input blocker created."); } catch { }
            }
            catch (Exception ex)
            {
                try { Debug.LogError("[BANMOD] CreateInputBlocker failed: " + ex.Message); } catch { }
            }
        }

        private void CreateFloatingUnreadBadge()
        {
            floatingUnreadButton = CreateButton(
                uiRoot.transform,
                "FloatingUnreadBadge",
                "",
                new Vector2(92f, 48f),
                new Vector2(850f, 485f),
                new Color(0.92f, 0.04f, 0.04f, 0.96f),
                OpenMenu
            );
            floatingUnreadBadgeText = floatingUnreadButton != null
                ? floatingUnreadButton.GetComponentInChildren<TextMeshProUGUI>(true)
                : null;
            SetActive(floatingUnreadButton != null ? floatingUnreadButton.gameObject : null, false);
        }

        private void CreateMainPanel()
        {
            mainPanel = CreatePanel(uiRoot.transform, "MainPanel", new Vector2(900f, 820f), Vector2.zero, BanModUiStyles.WindowColor);
            mainTitleText = CreateLabel(mainPanel.transform, "MainTitle", "", 30, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(820f, 50f), new Vector2(0f, 360f));
            communicationsUnreadBadgeText = CreateLabel(
                mainPanel.transform,
                "CommunicationsUnreadBadge",
                "",
                23,
                TextAlignmentOptions.Center,
                new Color(1f, 0.12f, 0.12f, 1f),
                new Vector2(105f, 38f),
                new Vector2(352f, 360f)
            );

            homePanel = CreateEmpty(mainPanel.transform, "HomePanel", new Vector2(820f, 620f), new Vector2(0f, 25f));
            CreateHomePanel();

            bugPanel = CreateEmpty(mainPanel.transform, "BugPanel", new Vector2(820f, 660f), new Vector2(0f, 5f));
            CreateBugPanel();

            playerReportPanel = CreateEmpty(mainPanel.transform, "PlayerReportPanel", new Vector2(820f, 660f), new Vector2(0f, 5f));
            CreatePlayerReportPanel();

            supportPanel = CreateEmpty(mainPanel.transform, "SupportPanel", new Vector2(820f, 660f), new Vector2(0f, 5f));
            CreateSupportPanel();

            sentReportsPanel = CreateEmpty(mainPanel.transform, "SentReportsPanel", new Vector2(820f, 660f), new Vector2(0f, 5f));
            CreateSentReportsPanel();

            statusText = CreateLabel(mainPanel.transform, "StatusText", "", 16, TextAlignmentOptions.Center, Color.white, new Vector2(820f, 28f), new Vector2(0f, -338f));
            CreateButton(mainPanel.transform, "CloseButton", T("Close", "Close"), new Vector2(220f, 46f), new Vector2(0f, -378f), new Color(0.65f, 0f, 0f, 1f), CloseMenu);
        }

        private void CreateHomePanel()
        {
            CreateButton(homePanel.transform, "BugReportButton", T("Comm_BugReport", "Bug Report"), new Vector2(330f, 60f), new Vector2(-185f, 145f), new Color(0.12f, 0.36f, 0.58f, 1f), () => OpenPanel(PanelMode.BugReport));
            CreateButton(homePanel.transform, "PlayerReportButton", T("Comm_ReportPlayer", "Report Player"), new Vector2(330f, 60f), new Vector2(185f, 145f), new Color(0.45f, 0.22f, 0.12f, 1f), () => OpenPanel(PanelMode.PlayerReport));
            CreateButton(homePanel.transform, "SupportButton", T("Comm_SupportRequest", "Support Request"), new Vector2(330f, 60f), new Vector2(0f, 60f), new Color(0.18f, 0.42f, 0.18f, 1f), () => OpenPanel(PanelMode.Support));
            CreateButton(homePanel.transform, "SentReportsButton", T("Comm_SentReports", "Sent Reports"), new Vector2(330f, 60f), new Vector2(0f, -25f), new Color(0.22f, 0.22f, 0.50f, 1f), () => OpenPanel(PanelMode.SentReports));
            sentReportsUnreadBadgeText = CreateLabel(
                homePanel.transform,
                "SentReportsUnreadBadge",
                "",
                22,
                TextAlignmentOptions.Center,
                new Color(1f, 0.12f, 0.12f, 1f),
                new Vector2(90f, 34f),
                new Vector2(142f, -6f)
            );
            CreateLabel(homePanel.transform, "Info", T("Comm_LogsAutoInfo", "Full BepInEx logs will be sent automatically with every report: LogOutput.log and ErrorLog.log."), 18, TextAlignmentOptions.Center, new Color(0.86f, 0.86f, 0.86f, 1f), new Vector2(760f, 100f), new Vector2(0f, -135f));
        }

        private void CreateBugPanel()
        {
            CreateLabel(bugPanel.transform, "BugTitleHeader", T("Comm_BugReport", "Bug Report"), 25, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(780f, 36f), new Vector2(0f, 300f));
            CreateLabel(bugPanel.transform, "BugInfo", T("Comm_LogsAutoInfo", "Full BepInEx logs will be sent automatically with every report: LogOutput.log and ErrorLog.log."), 15, TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 0.9f, 1f), new Vector2(780f, 34f), new Vector2(0f, 268f));

            CreateLabel(bugPanel.transform, "TitleLabel", T("Comm_FieldTitleRequired", "TITLE: (required)"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 24f), new Vector2(0f, 228f));
            bugTitleInput = CreateInput(bugPanel.transform, "BugTitleInput", T("Comm_TitlePlaceholder", "Write a short title..."), false, new Vector2(780f, 38f), new Vector2(0f, 200f));

            CreateLabel(bugPanel.transform, "GameModeLabel", T("Comm_FieldGameModeRequired", "GAMEMODE: (required)"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 24f), new Vector2(0f, 162f));
            bugGameModeInput = CreateInput(bugPanel.transform, "BugGameModeInput", T("Comm_GameModePlaceholder", "Example: Classic, Hide N Seek, Custom mode..."), false, new Vector2(780f, 38f), new Vector2(0f, 134f));

            bugCustomRolesText = CreateLabel(bugPanel.transform, "CustomRolesLabel", T("Comm_FieldCustomRoles", "CUSTOM ROLE:"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(410f, 28f), new Vector2(-185f, 90f));
            bugCustomYesButton = CreateButton(bugPanel.transform, "CustomRoleYes", "", new Vector2(90f, 34f), new Vector2(130f, 90f), new Color(0.25f, 0.25f, 0.25f, 1f), () => SetCustomRoles(true));
            bugCustomNoButton = CreateButton(bugPanel.transform, "CustomRoleNo", "", new Vector2(90f, 34f), new Vector2(230f, 90f), new Color(0.25f, 0.25f, 0.25f, 1f), () => SetCustomRoles(false));
            bugCustomYesText = bugCustomYesButton.GetComponentInChildren<TextMeshProUGUI>(true);
            bugCustomNoText = bugCustomNoButton.GetComponentInChildren<TextMeshProUGUI>(true);
            bugCustomRolesWhichInput = CreateInput(bugPanel.transform, "CustomRolesWhich", T("Comm_WhichRequiredPlaceholder", "Which? (required)"), false, new Vector2(780f, 38f), new Vector2(0f, 50f));

            bugOtherModsText = CreateLabel(bugPanel.transform, "OtherModsLabel", T("Comm_FieldOtherMods", "Other mods installed?"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(410f, 28f), new Vector2(-185f, 8f));
            bugOtherYesButton = CreateButton(bugPanel.transform, "OtherModsYes", "", new Vector2(90f, 34f), new Vector2(130f, 8f), new Color(0.25f, 0.25f, 0.25f, 1f), () => SetOtherMods(true));
            bugOtherNoButton = CreateButton(bugPanel.transform, "OtherModsNo", "", new Vector2(90f, 34f), new Vector2(230f, 8f), new Color(0.25f, 0.25f, 0.25f, 1f), () => SetOtherMods(false));
            bugOtherYesText = bugOtherYesButton.GetComponentInChildren<TextMeshProUGUI>(true);
            bugOtherNoText = bugOtherNoButton.GetComponentInChildren<TextMeshProUGUI>(true);
            bugOtherModsWhichInput = CreateInput(bugPanel.transform, "OtherModsWhich", T("Comm_WhichRequiredPlaceholder", "Which? (required)"), false, new Vector2(780f, 38f), new Vector2(0f, -32f));

            CreateLabel(bugPanel.transform, "DescriptionLabel", T("Comm_FieldBugDescriptionRequired", "Bug description: (required)"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 24f), new Vector2(0f, -78f));
            bugDescriptionInput = CreateInput(bugPanel.transform, "BugDescriptionInput", T("Comm_BugDescriptionPlaceholder", "Describe the bug in detail..."), true, new Vector2(780f, 140f), new Vector2(0f, -160f));

            sendBugButton = CreateButton(bugPanel.transform, "SendBugButton", T("Comm_SendBugReport", "Send Bug Report"), new Vector2(260f, 44f), new Vector2(155f, -252f), new Color(0.1f, 0.45f, 0.1f, 1f), SendBugReport);
            CreateButton(bugPanel.transform, "BackBugButton", T("Back", "Back"), new Vector2(180f, 42f), new Vector2(-155f, -252f), new Color(0.25f, 0.25f, 0.25f, 1f), OpenHomePanel);
        }

        private void CreatePlayerReportPanel()
        {
            CreateLabel(playerReportPanel.transform, "PlayerHeader", T("Comm_ReportPlayer", "Report Player"), 25, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(780f, 36f), new Vector2(0f, 300f));
            CreateLabel(playerReportPanel.transform, "PlayerInfo", T("Comm_LogsAutoInfo", "Full BepInEx logs will be sent automatically with every report: LogOutput.log and ErrorLog.log."), 15, TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 0.9f, 1f), new Vector2(780f, 34f), new Vector2(0f, 268f));
            selectedPlayerText = CreateLabel(playerReportPanel.transform, "SelectedPlayer", T("Comm_SelectedPlayerNone", "Selected player: none"), 16, TextAlignmentOptions.Center, Color.white, new Vector2(780f, 30f), new Vector2(0f, 228f));
            playerListPanel = CreateEmpty(playerReportPanel.transform, "PlayerListPanel", new Vector2(780f, 160f), new Vector2(0f, 130f));
            CreateLabel(playerReportPanel.transform, "ReasonLabel", T("Comm_FieldReasonRequired", "Reason: (required)"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 24f), new Vector2(0f, 20f));
            playerReasonInput = CreateInput(playerReportPanel.transform, "PlayerReasonInput", T("Comm_ReasonPlaceholder", "Write the reason for the report..."), true, new Vector2(780f, 180f), new Vector2(0f, -90f));
            sendPlayerButton = CreateButton(playerReportPanel.transform, "SendPlayerButton", T("Comm_SendPlayerReport", "Send Player Report"), new Vector2(280f, 44f), new Vector2(165f, -240f), new Color(0.1f, 0.45f, 0.1f, 1f), SendPlayerReport);
            CreateButton(playerReportPanel.transform, "BackPlayerButton", T("Back", "Back"), new Vector2(180f, 42f), new Vector2(-165f, -240f), new Color(0.25f, 0.25f, 0.25f, 1f), OpenHomePanel);
        }

        private void CreateSupportPanel()
        {
            CreateLabel(supportPanel.transform, "SupportHeader", T("Comm_SupportRequest", "Support Request"), 25, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(780f, 36f), new Vector2(0f, 300f));
            CreateLabel(supportPanel.transform, "SupportInfo", T("Comm_LogsAutoInfo", "Full BepInEx logs will be sent automatically with every report: LogOutput.log and ErrorLog.log."), 15, TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 0.9f, 1f), new Vector2(780f, 34f), new Vector2(0f, 260f));
            CreateLabel(supportPanel.transform, "SupportTitleLabel", T("Comm_FieldTitleRequired", "TITLE: (required)"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 24f), new Vector2(0f, 200f));
            supportTitleInput = CreateInput(supportPanel.transform, "SupportTitleInput", T("Comm_TitlePlaceholder", "Write a short title..."), false, new Vector2(780f, 42f), new Vector2(0f, 168f));
            CreateLabel(supportPanel.transform, "SupportMessageLabel", T("Comm_FieldDescriptionRequired", "Description: (required)"), 16, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 24f), new Vector2(0f, 112f));
            supportMessageInput = CreateInput(supportPanel.transform, "SupportMessageInput", T("Comm_DescriptionPlaceholder", "Write your message here..."), true, new Vector2(780f, 240f), new Vector2(0f, -25f));
            sendSupportButton = CreateButton(supportPanel.transform, "SendSupportButton", T("Comm_SendSupportRequest", "Send Support Request"), new Vector2(300f, 44f), new Vector2(170f, -220f), new Color(0.1f, 0.45f, 0.1f, 1f), SendSupportRequest);
            CreateButton(supportPanel.transform, "BackSupportButton", T("Back", "Back"), new Vector2(180f, 42f), new Vector2(-170f, -220f), new Color(0.25f, 0.25f, 0.25f, 1f), OpenHomePanel);
        }


        private void CreateSentReportsPanel()
        {
            CreateLabel(sentReportsPanel.transform, "SentHeader", T("Comm_SentReports", "Sent Reports"), 25, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(780f, 36f), new Vector2(0f, 300f));
            sentReportsInfoText = CreateLabel(sentReportsPanel.transform, "SentInfo", T("Comm_SentReportsInfo", "Select one of your reports to chat with admin, close it as resolved, or delete it from your list."), 15, TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 0.9f, 1f), new Vector2(780f, 34f), new Vector2(0f, 264f));
            sentReportRefreshButton = CreateButton(sentReportsPanel.transform, "RefreshSentReports", T("Comm_Refresh", "Refresh"), new Vector2(170f, 40f), new Vector2(225f, 220f), new Color(0.18f, 0.36f, 0.55f, 1f), LoadSentReports);
            CreateButton(sentReportsPanel.transform, "BackSentReports", T("Back", "Back"), new Vector2(170f, 40f), new Vector2(-225f, 220f), new Color(0.25f, 0.25f, 0.25f, 1f), OpenHomePanel);

            sentReportsListPanel = CreateEmpty(sentReportsPanel.transform, "SentReportsList", new Vector2(780f, 170f), new Vector2(0f, 70f));
            sentReportDetailsText = CreateLabel(sentReportsPanel.transform, "SentReportDetails", T("Comm_SelectReport", "Select a report to view details."), 14, TextAlignmentOptions.TopLeft, Color.white, new Vector2(780f, 85f), new Vector2(0f, 10f));
            sentReportChatText = CreateLabel(sentReportsPanel.transform, "SentReportChat", "", 14, TextAlignmentOptions.TopLeft, new Color(0.92f, 0.92f, 0.92f, 1f), new Vector2(780f, 165f), new Vector2(0f, -125f));

            CreateLabel(sentReportsPanel.transform, "ChatInputLabel", T("Comm_ReportChatInput", "Message to admin:"), 14, TextAlignmentOptions.Left, Color.white, new Vector2(780f, 20f), new Vector2(0f, -224f));
            sentReportChatInput = CreateInput(sentReportsPanel.transform, "SentReportChatInput", T("Comm_ReportChatPlaceholder", "Write a message for this report..."), true, new Vector2(560f, 58f), new Vector2(-110f, -266f));
            sentReportChatSendButton = CreateButton(sentReportsPanel.transform, "SendReportChat", T("Comm_Send", "Send"), new Vector2(170f, 46f), new Vector2(305f, -266f), new Color(0.1f, 0.45f, 0.1f, 1f), SendSelectedReportMessage);

            sentReportCloseButton = CreateButton(sentReportsPanel.transform, "CloseSentReport", T("Comm_CloseResolved", "Close Resolved"), new Vector2(240f, 42f), new Vector2(-140f, -315f), new Color(0.45f, 0.32f, 0.08f, 1f), CloseSelectedReport);
            sentReportDeleteButton = CreateButton(sentReportsPanel.transform, "DeleteSentReport", T("Comm_DeleteReport", "Delete Report"), new Vector2(220f, 42f), new Vector2(140f, -315f), new Color(0.65f, 0.05f, 0.05f, 1f), DeleteSelectedReport);
        }

        private void CreatePopupPanel()
        {
            popupPanel = CreatePanel(uiRoot.transform, "MessagePopup", new Vector2(1180f, 760f), Vector2.zero, new Color(BanModUiStyles.WindowColor.r, BanModUiStyles.WindowColor.g, BanModUiStyles.WindowColor.b, 0.985f));

            CanvasGroup popupGroup = popupPanel.GetComponent<CanvasGroup>();
            if (popupGroup == null)
                popupGroup = popupPanel.AddComponent<CanvasGroup>();
            popupGroup.alpha = 1f;
            popupGroup.interactable = true;
            popupGroup.blocksRaycasts = true;
            popupGroup.ignoreParentGroups = true;

            popupTitleText = CreateLabel(
                popupPanel.transform,
                "PopupTitle",
                T("Comm_ServerMessageTitle", "BANMOD Message"),
                27,
                TextAlignmentOptions.Center,
                BanModUiStyles.AccentHoverColor,
                new Vector2(1120f, 62f),
                new Vector2(0f, 334f)
            );

            popupBodyText = CreateScrollableLabel(
                popupPanel.transform,
                "PopupBody",
                "",
                18,
                TextAlignmentOptions.TopLeft,
                Color.white,
                new Vector2(1080f, 450f),
                new Vector2(0f, 92f),
                out popupBodyScrollRect
            );

            popupReportDetailsPanel = CreatePanel(
                popupPanel.transform,
                "PopupReportDetailsPanel",
                new Vector2(330f, 430f),
                new Vector2(-395f, 82f),
                BanModUiStyles.PanelColor
            );

            popupReportDetailsText = CreateLabel(
                popupReportDetailsPanel.transform,
                "PopupReportDetailsText",
                "",
                15,
                TextAlignmentOptions.TopLeft,
                Color.white,
                new Vector2(292f, 388f),
                new Vector2(0f, -2f)
            );

            popupReportChatText = CreateScrollableLabel(
                popupPanel.transform,
                "PopupReportChat",
                "",
                16,
                TextAlignmentOptions.TopLeft,
                Color.white,
                new Vector2(720f, 430f),
                new Vector2(200f, 82f),
                out popupReportChatScrollRect
            );

            popupStatusText = CreateLabel(
                popupPanel.transform,
                "PopupStatus",
                "",
                15,
                TextAlignmentOptions.Center,
                new Color(0.9f, 0.9f, 0.9f, 1f),
                new Vector2(1080f, 34f),
                new Vector2(0f, -158f)
            );

            popupReportInputLabel = CreateLabel(
                popupPanel.transform,
                "PopupReportInputLabel",
                T("Comm_ReportChatInput", "Message to admin:"),
                14,
                TextAlignmentOptions.Left,
                Color.white,
                new Vector2(720f, 22f),
                new Vector2(200f, -194f)
            );

            popupReportChatInput = CreateInput(
                popupPanel.transform,
                "PopupReportChatInput",
                T("Comm_ReportChatPlaceholder", "Write a reply for this report..."),
                true,
                new Vector2(560f, 70f),
                new Vector2(120f, -240f)
            );

            popupReportSendButton = CreateButton(
                popupPanel.transform,
                "PopupReportSend",
                T("Comm_Send", "Send"),
                new Vector2(155f, 48f),
                new Vector2(510f, -240f),
                new Color(0.1f, 0.45f, 0.1f, 1f),
                SendPopupReportMessage
            );

            popupReportCloseButton = CreateButton(
                popupPanel.transform,
                "PopupReportCloseReport",
                T("Comm_CloseReport", "Close report"),
                new Vector2(220f, 46f),
                new Vector2(-255f, -326f),
                new Color(0.45f, 0.32f, 0.08f, 1f),
                ClosePopupReport
            );

            popupReportDeleteButton = CreateButton(
                popupPanel.transform,
                "PopupReportDelete",
                T("Comm_DeleteReport", "Delete report"),
                new Vector2(220f, 46f),
                new Vector2(0f, -326f),
                new Color(0.65f, 0.05f, 0.05f, 1f),
                DeletePopupReport
            );

            popupCloseButton = CreateButton(
                popupPanel.transform,
                "PopupClose",
                T("Comm_ClosePopup", "Close popup"),
                new Vector2(220f, 46f),
                new Vector2(255f, -326f),
                new Color(0.28f, 0.28f, 0.28f, 1f),
                ClosePopup
            );
        }

        private void HandleWindowDrag()
        {
            try
            {
                if (!showMenu && !popupVisible)
                {
                    draggingMainPanel = false;
                    draggingPopupPanel = false;
                    return;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    draggingMainPanel = false;
                    draggingPopupPanel = false;

                    if (popupVisible && IsMouseInsideDragBand(popupPanel, 110f))
                    {
                        draggingPopupPanel = true;
                        lastDragMousePosition = Input.mousePosition;
                        return;
                    }

                    if (showMenu && IsMouseInsideDragBand(mainPanel, 110f))
                    {
                        draggingMainPanel = true;
                        lastDragMousePosition = Input.mousePosition;
                        return;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    draggingMainPanel = false;
                    draggingPopupPanel = false;
                    return;
                }

                if (!Input.GetMouseButton(0))
                    return;

                if (!draggingMainPanel && !draggingPopupPanel)
                    return;

                GameObject target = draggingPopupPanel ? popupPanel : mainPanel;
                if (target == null)
                    return;

                RectTransform rect = target.GetComponent<RectTransform>();
                if (rect == null)
                    return;

                Vector2 currentMousePosition = Input.mousePosition;
                Vector2 delta = currentMousePosition - lastDragMousePosition;
                lastDragMousePosition = currentMousePosition;

                float scaleFactor = 1f;
                try
                {
                    Canvas canvas = uiRoot != null ? uiRoot.GetComponent<Canvas>() : null;
                    if (canvas != null && canvas.scaleFactor > 0f)
                        scaleFactor = canvas.scaleFactor;
                }
                catch { }

                rect.anchoredPosition += delta / scaleFactor;
            }
            catch
            {
                draggingMainPanel = false;
                draggingPopupPanel = false;
            }
        }

        private void HandlePopupBodyManualScroll()
        {
            try
            {
                if (!popupVisible)
                    return;

                ScrollRect activeScrollRect = popupIsReportChat && popupReportId > 0 && popupReportChatScrollRect != null
                    ? popupReportChatScrollRect
                    : popupBodyScrollRect;

                if (activeScrollRect == null)
                    return;

                if (activeScrollRect.content == null || activeScrollRect.viewport == null)
                    return;

                if (!activeScrollRect.gameObject.activeInHierarchy)
                    return;

                RectTransform scrollRectTransform = activeScrollRect.GetComponent<RectTransform>();
                if (scrollRectTransform == null)
                    return;

                if (!IsMouseInsideRect(scrollRectTransform))
                    return;

                float wheel = Input.mouseScrollDelta.y;

                if (Mathf.Abs(wheel) < 0.001f)
                {
                    try
                    {
                        wheel = Input.GetAxis("Mouse ScrollWheel") * 10f;
                    }
                    catch { }
                }

                if (Mathf.Abs(wheel) < 0.001f)
                    return;

                float viewportHeight = activeScrollRect.viewport.rect.height;
                float contentHeight = activeScrollRect.content.rect.height;

                if (contentHeight <= viewportHeight + 1f)
                    return;

                activeScrollRect.StopMovement();

                float step = 0.12f;
                float next = activeScrollRect.verticalNormalizedPosition + wheel * step;

                activeScrollRect.verticalNormalizedPosition = Mathf.Clamp01(next);

                Canvas.ForceUpdateCanvases();
            }
            catch { }
        }

        private static bool IsMouseInsideRect(RectTransform rect)
        {
            try
            {
                if (rect == null)
                    return false;

                Vector2 localPoint;
                bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect,
                    Input.mousePosition,
                    null,
                    out localPoint
                );

                if (!converted)
                    return false;

                Rect r = rect.rect;

                return localPoint.x >= r.xMin &&
                       localPoint.x <= r.xMax &&
                       localPoint.y >= r.yMin &&
                       localPoint.y <= r.yMax;
            }
            catch
            {
                return false;
            }
        }


        private static bool IsMouseInsideDragBand(GameObject panel, float dragBandHeight)
        {
            try
            {
                if (panel == null)
                    return false;

                RectTransform rect = panel.GetComponent<RectTransform>();
                if (rect == null)
                    return false;

                Vector2 localPoint;
                bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect,
                    Input.mousePosition,
                    null,
                    out localPoint
                );

                if (!converted)
                    return false;

                Rect r = rect.rect;
                if (localPoint.x < r.xMin || localPoint.x > r.xMax || localPoint.y < r.yMin || localPoint.y > r.yMax)
                    return false;

                return localPoint.y >= r.yMax - dragBandHeight;
            }
            catch
            {
                return false;
            }
        }

        private void OpenHomePanel()
        {
            currentPanel = PanelMode.Home;
            ClearForm();
            RefreshVisibility();
        }

        private void OpenPanel(PanelMode panel)
        {
            currentPanel = panel;
            ClearForm();

            if (panel == PanelMode.PlayerReport)
                RefreshPlayerButtons();

            if (panel == PanelMode.SentReports)
                LoadSentReports();

            RefreshTexts();
            RefreshVisibility();
            FocusFirstInput();
        }

        private void ClearForm()
        {
            isSending = false;
            customRolesEnabled = false;
            otherModsInstalled = false;
            targetFriendCode = "";
            targetName = "";
            targetHashedPuid = "";
            targetPlayerId = "";
            targetPlatform = "";
            selectedSentReportId = 0;

            SetInputText(bugTitleInput, "");
            SetInputText(bugGameModeInput, "");
            SetInputText(bugCustomRolesWhichInput, "");
            SetInputText(bugOtherModsWhichInput, "");
            SetInputText(bugDescriptionInput, "");
            SetInputText(playerReasonInput, "");
            SetInputText(supportTitleInput, "");
            SetInputText(supportMessageInput, "");
            SetInputText(sentReportChatInput, "");
            SetText(sentReportDetailsText, T("Comm_SelectReport", "Select a report to view details."));
            SetText(sentReportChatText, "");
            SetText(statusText, "");
            UpdateToggleTexts();
            UpdateSelectedPlayerText();
            SetSending(false);
        }

        private void RefreshUnreadBadge()
        {
            try
            {
                int count = Mathf.Max(0, pendingUnreadCount);
                bool visible = count > 0;
                string label = count > 99 ? "● 99+" : "● " + count.ToString();

                SetText(communicationsUnreadBadgeText, visible ? label : "");
                SetText(sentReportsUnreadBadgeText, visible ? label : "");
                SetText(floatingUnreadBadgeText, visible ? label : "");
                SetActive(communicationsUnreadBadgeText != null ? communicationsUnreadBadgeText.gameObject : null, visible);
                SetActive(sentReportsUnreadBadgeText != null ? sentReportsUnreadBadgeText.gameObject : null, visible);
            }
            catch { }
        }

        private void RefreshVisibility()
        {
            if (uiRoot == null)
                return;

            RefreshUnreadBadge();

            bool reportLayout = popupIsReportChat && popupReportId > 0;

            SetActive(GetScrollableRoot(popupBodyText), !reportLayout);
            SetActive(popupReportDetailsPanel, reportLayout);
            SetActive(GetScrollableRoot(popupReportChatText), reportLayout);

            SetActive(popupStatusText != null ? popupStatusText.gameObject : null, true);
            SetActive(popupReportInputLabel != null ? popupReportInputLabel.gameObject : null, reportLayout);
            SetActive(popupReportChatInput != null ? popupReportChatInput.gameObject : null, reportLayout);
            SetActive(popupReportSendButton != null ? popupReportSendButton.gameObject : null, reportLayout);
            SetActive(popupReportCloseButton != null ? popupReportCloseButton.gameObject : null, reportLayout);
            SetActive(popupReportDeleteButton != null ? popupReportDeleteButton.gameObject : null, reportLayout);

            bool showFloatingUnread = !showMenu && !popupVisible && pendingUnreadCount > 0;
            uiRoot.SetActive(showMenu || popupVisible || showFloatingUnread);
            SetActive(inputBlocker, showMenu || popupVisible);
            SetActive(floatingUnreadButton != null ? floatingUnreadButton.gameObject : null, showFloatingUnread);
            SetActive(mainPanel, showMenu);
            SetActive(homePanel, showMenu && currentPanel == PanelMode.Home);
            SetActive(bugPanel, showMenu && currentPanel == PanelMode.BugReport);
            SetActive(playerReportPanel, showMenu && currentPanel == PanelMode.PlayerReport);
            SetActive(supportPanel, showMenu && currentPanel == PanelMode.Support);
            SetActive(sentReportsPanel, showMenu && currentPanel == PanelMode.SentReports);
            SetActive(popupPanel, popupVisible);

            bool reportPopup = popupVisible && reportLayout;
            SetInteractable(popupReportSendButton, reportPopup && popupReportIsOpen && !isSending);
            SetInteractable(popupReportCloseButton, reportPopup && popupReportIsOpen && !isSending);
            SetInteractable(popupReportDeleteButton, reportPopup && !isSending);

            SetActive(bugCustomRolesWhichInput != null ? bugCustomRolesWhichInput.gameObject : null, showMenu && currentPanel == PanelMode.BugReport && customRolesEnabled);
            SetActive(bugOtherModsWhichInput != null ? bugOtherModsWhichInput.gameObject : null, showMenu && currentPanel == PanelMode.BugReport && otherModsInstalled);

            bool selectedReport = showMenu && currentPanel == PanelMode.SentReports && selectedSentReportId > 0;
            BanModCommunicationManager.ReportSummary activeReport = selectedReport ? FindSentReport(selectedSentReportId) : null;
            bool selectedOpenReport = activeReport != null && IsReportOpen(activeReport);

            SetActive(sentReportChatInput != null ? sentReportChatInput.gameObject : null, selectedReport);
            SetActive(sentReportChatSendButton != null ? sentReportChatSendButton.gameObject : null, selectedReport);
            SetActive(sentReportCloseButton != null ? sentReportCloseButton.gameObject : null, selectedReport);
            SetActive(sentReportDeleteButton != null ? sentReportDeleteButton.gameObject : null, selectedReport);
            SetInteractable(sentReportChatSendButton, selectedOpenReport && !isSending);
            SetInteractable(sentReportCloseButton, selectedOpenReport && !isSending);
            SetInteractable(sentReportDeleteButton, selectedReport && !isSending);

            try
            {
                CanvasGroup mainGroup = mainPanel != null ? mainPanel.GetComponent<CanvasGroup>() : null;
                if (mainPanel != null && mainGroup == null)
                    mainGroup = mainPanel.AddComponent<CanvasGroup>();

                if (mainGroup != null)
                {
                    mainGroup.alpha = 1f;
                    mainGroup.interactable = showMenu && !popupVisible;
                    mainGroup.blocksRaycasts = showMenu && !popupVisible;
                }

                CanvasGroup popupGroup = popupPanel != null ? popupPanel.GetComponent<CanvasGroup>() : null;
                if (popupPanel != null && popupGroup == null)
                    popupGroup = popupPanel.AddComponent<CanvasGroup>();

                if (popupGroup != null)
                {
                    popupGroup.alpha = 1f;
                    popupGroup.interactable = popupVisible;
                    popupGroup.blocksRaycasts = popupVisible;
                    popupGroup.ignoreParentGroups = true;
                }

                CanvasGroup blockerGroup = inputBlocker != null ? inputBlocker.GetComponent<CanvasGroup>() : null;
                if (blockerGroup != null)
                {
                    blockerGroup.interactable = showMenu || popupVisible;
                    blockerGroup.blocksRaycasts = showMenu || popupVisible;
                }

                if (inputBlocker != null)
                    inputBlocker.transform.SetAsLastSibling();

                if (showMenu && mainPanel != null)
                    mainPanel.transform.SetAsLastSibling();

                if (popupVisible && popupPanel != null)
                    popupPanel.transform.SetAsLastSibling();
            }
            catch { }
        }

        private void RefreshTexts()
        {
            SetText(mainTitleText, T("Comm_Title", "BANMOD - Communication"));
            UpdateToggleTexts();
            UpdateSelectedPlayerText();
        }

        private void SetCustomRoles(bool enabled)
        {
            customRolesEnabled = enabled;
            UpdateToggleTexts();
            RefreshVisibility();
        }

        private void SetOtherMods(bool enabled)
        {
            otherModsInstalled = enabled;
            UpdateToggleTexts();
            RefreshVisibility();
        }

        private void UpdateToggleTexts()
        {
            SetText(bugCustomYesText, customRolesEnabled ? T("Comm_YesSelected", "Yes ✓") : T("Comm_Yes", "Yes"));
            SetText(bugCustomNoText, !customRolesEnabled ? T("Comm_NoSelected", "No ✓") : T("Comm_No", "No"));
            SetText(bugOtherYesText, otherModsInstalled ? T("Comm_YesSelected", "Yes ✓") : T("Comm_Yes", "Yes"));
            SetText(bugOtherNoText, !otherModsInstalled ? T("Comm_NoSelected", "No ✓") : T("Comm_No", "No"));
        }

        private void UpdateSelectedPlayerText()
        {
            string text = T("Comm_SelectedPlayerNone", "Selected player: none");
            if (!string.IsNullOrWhiteSpace(targetName) || !string.IsNullOrWhiteSpace(targetFriendCode))
            {
                text = T("Comm_SelectedPlayer", "Selected player:") + " " + (targetName ?? "");
                if (!string.IsNullOrWhiteSpace(targetFriendCode))
                    text += " [" + targetFriendCode + "]";
                if (!string.IsNullOrWhiteSpace(targetHashedPuid))
                    text += " [PUID: " + targetHashedPuid + "]";
            }

            SetText(selectedPlayerText, text.Trim());
        }

        private void RefreshPlayerButtons()
        {
            ClearPlayerButtons();

            if (playerListPanel == null)
                return;

            List<PlayerControl> players = new List<PlayerControl>();

            try
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p != null && p.Data != null && PlayerControl.LocalPlayer != null && p.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                        players.Add(p);
                }
            }
            catch { }

            if (players.Count <= 0)
            {
                CreateLabel(playerListPanel.transform, "NoPlayers", T("Comm_NoPlayers", "No selectable players found in lobby."), 16, TextAlignmentOptions.Center, Color.white, new Vector2(760f, 36f), new Vector2(0f, 35f));
                return;
            }

            int maxButtons = Math.Min(players.Count, 9);
            float startX = -260f;
            float startY = 55f;

            for (int i = 0; i < maxButtons; i++)
            {
                PlayerControl player = players[i];
                string playerName = SafePlayerName(player);
                string friendCode = SafeTargetFriendCode(player);
                string hashedPuid = SafeTargetHashedPuid(player);
                string playerId = SafeTargetPlayerId(player);
                string platform = SafeTargetPlatform(player);

                int col = i % 3;
                int row = i / 3;

                string buttonLabel = playerName;
                if (!string.IsNullOrWhiteSpace(friendCode))
                    buttonLabel += "\n" + friendCode;

                CreateButton(playerListPanel.transform, "PlayerButton_" + i, buttonLabel, new Vector2(240f, 42f), new Vector2(startX + col * 260f, startY - row * 50f), new Color(0.18f, 0.28f, 0.42f, 1f), () =>
                {
                    targetName = playerName;
                    targetFriendCode = friendCode;
                    targetHashedPuid = hashedPuid;
                    targetPlayerId = playerId;
                    targetPlatform = platform;
                    UpdateSelectedPlayerText();
                });
            }
        }

        private void ClearPlayerButtons()
        {
            if (playerListPanel == null)
                return;

            try
            {
                for (int i = playerListPanel.transform.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.Destroy(playerListPanel.transform.GetChild(i).gameObject);
            }
            catch { }
        }


        private void LoadSentReportsKeepingSelection(int reportId)
        {
            pendingSelectSentReportId = reportId;
            LoadSentReports();
        }

        private void LoadSentReports()
        {
            if (isSending)
                return;

            SetSending(true);
            SetText(statusText, T("Comm_LoadingReports", "Loading reports..."));

            try
            {
                LoadSentReportsCallback callback = new LoadSentReportsCallback(this);
                RunCommunicationCoroutine(
                    BanModCommunicationManager.GetMyReportsCoroutine(callback.Invoke)
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                SetText(statusText, ex.Message);
            }
        }
        private sealed class LoadSentReportsCallback
        {
            private readonly BanModCommunicationUi ui;

            public LoadSentReportsCallback(BanModCommunicationUi ui)
            {
                this.ui = ui;
            }

            public void Invoke(List<BanModCommunicationManager.ReportSummary> reports, string error)
            {
                if (ui == null)
                    return;

                ui.OnSentReportsLoaded(reports, error);
            }
        }

        [HideFromIl2Cpp]
        private void OnSentReportsLoaded(List<BanModCommunicationManager.ReportSummary> reports, string error)
        {
            SetSending(false);

            if (!string.IsNullOrWhiteSpace(error))
            {
                SetText(statusText, error);
                sentReports = new List<BanModCommunicationManager.ReportSummary>();
            }
            else
            {
                SetText(statusText, "");
                sentReports = reports ?? new List<BanModCommunicationManager.ReportSummary>();
                BanModMessagePoller.RefreshUnreadReportsFromUi(sentReports);
            }

            int keepId = pendingSelectSentReportId;
            pendingSelectSentReportId = 0;

            selectedSentReportId = 0;
            SetInputText(sentReportChatInput, "");
            SetText(sentReportDetailsText, T("Comm_SelectReport", "Select a report to view details."));
            SetText(sentReportChatText, "");
            RenderSentReportsList();

            if (keepId > 0 && FindSentReport(keepId) != null)
                SelectSentReport(keepId);

            RefreshVisibility();
        }
        private void RenderSentReportsList()
        {
            try
            {
                if (sentReportsListPanel == null)
                    return;

                for (int i = sentReportsListPanel.transform.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.Destroy(sentReportsListPanel.transform.GetChild(i).gameObject);

                if (sentReports == null || sentReports.Count <= 0)
                {
                    CreateLabel(sentReportsListPanel.transform, "NoReports", T("Comm_NoSentReports", "No sent reports."), 18, TextAlignmentOptions.Center, Color.white, new Vector2(760f, 50f), Vector2.zero);
                    return;
                }

                int max = Math.Min(sentReports.Count, 8);
                for (int i = 0; i < max; i++)
                {
                    BanModCommunicationManager.ReportSummary report = sentReports[i];
                    int id = report.Id;
                    int row = i / 2;
                    int col = i % 2;
                    float x = col == 0 ? -200f : 200f;
                    float y = 50f - row * 48f;
                    string unreadPrefix = report.IsUnread ? "<color=#ff3030>●</color> " : "";
                    string label = unreadPrefix + "#" + report.Id + " " + ShortText(report.Title, 28) + " [" + report.Status + "]";
                    CreateButton(sentReportsListPanel.transform, "SentReport_" + id, label, new Vector2(370f, 40f), new Vector2(x, y), new Color(0.18f, 0.28f, 0.42f, 1f), () => SelectSentReport(id));
                }
            }
            catch { }
        }

        private void SelectSentReport(int reportId)
        {
            BanModCommunicationManager.ReportSummary report = FindSentReport(reportId);
            if (report == null)
                return;

            selectedSentReportId = 0;
            SetInputText(sentReportChatInput, "");
            SetText(sentReportDetailsText, T("Comm_SelectReport", "Select a report to view details."));
            SetText(sentReportChatText, "");
            ShowReportChatPopup(report);
            RefreshVisibility();
        }
        [HideFromIl2Cpp]
        private void UpsertCachedReport(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null || report.Id <= 0)
                return;

            if (sentReports == null)
                sentReports = new List<BanModCommunicationManager.ReportSummary>();

            for (int i = 0; i < sentReports.Count; i++)
            {
                if (sentReports[i] != null && sentReports[i].Id == report.Id)
                {
                    sentReports[i] = report;
                    return;
                }
            }

            sentReports.Insert(0, report);
        }

        [HideFromIl2Cpp]
        private string BuildReportPopupText(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null)
                return "";

            return BuildReportDetailsText(report) + "\n\n" + BuildReportChatText(report);
        }
        [HideFromIl2Cpp]
        private string BuildReportDetailsText(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null)
                return "";

            string typeLabel = ReportTypeLabel(report);
            string status = string.IsNullOrWhiteSpace(report.Status) ? "open" : report.Status;

            string text =
                "<b>" + T("Comm_Details", "Details") + "</b>\n\n" +
                "<b>" + T("Comm_Report", "Report") + ":</b>\n" +
                "#" + report.Id + " — " + RichEscape(typeLabel) + "\n\n" +
                "<b>" + T("Comm_Status", "Status") + ":</b>\n" +
                RichEscape(status) + "\n\n";

            if (!string.IsNullOrWhiteSpace(report.Title))
            {
                text +=
                    "<b>" + T("Comm_Title", "Title") + ":</b>\n" +
                    RichEscape(report.Title) + "\n\n";
            }

            if (!string.IsNullOrWhiteSpace(report.TargetName) || !string.IsNullOrWhiteSpace(report.TargetFriendCode))
            {
                text += "<b>" + T("Comm_Target", "Reported player") + ":</b>\n";

                if (!string.IsNullOrWhiteSpace(report.TargetName))
                    text += RichEscape(report.TargetName.Trim());

                if (!string.IsNullOrWhiteSpace(report.TargetFriendCode))
                    text += " [" + RichEscape(report.TargetFriendCode.Trim()) + "]";

                text += "\n\n";
            }

            if (!string.IsNullOrWhiteSpace(report.GameMode))
            {
                text +=
                    "<b>" + T("Comm_GameMode", "Game mode") + ":</b>\n" +
                    RichEscape(report.GameMode) + "\n\n";
            }

            BanModCommunicationManager.ReportChatMessage lastAdmin = GetLatestAdminMessage(report);
            if (lastAdmin != null && !string.IsNullOrWhiteSpace(lastAdmin.Message))
            {
                text +=
                    "<b>" + T("Comm_LatestAdminReply", "Latest admin reply") + ":</b>\n" +
                    "<color=#8fd3ff>" + RichEscape(ShortText(lastAdmin.Message.Trim(), 120)) + "</color>\n\n";
            }

            text += "<color=#aaaaaa>" + T("Comm_ChatScrollHint", "Use the mouse wheel over the chat to scroll.") + "</color>";

            return text.Trim();
        }
        [HideFromIl2Cpp]
        private string ReportTypeLabel(BanModCommunicationManager.ReportSummary report)
        {
            string type = report != null ? (report.Type ?? "") : "";

            if (string.Equals(type, "player_report", StringComparison.OrdinalIgnoreCase))
                return T("Comm_ReportPlayer", "Player report");

            if (string.Equals(type, "bug_report", StringComparison.OrdinalIgnoreCase))
                return T("Comm_BugReport", "Bug report");

            return T("Comm_SupportRequest", "Communication");
        }

        private static bool IsReportOpen(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null)
                return false;

            string status = report.Status ?? "";

            return !string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, "ignored", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, "deleted", StringComparison.OrdinalIgnoreCase);
        }

        private static string RichEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private static string FormatReportChatTime(long unixTime)
        {
            try
            {
                if (unixTime <= 0)
                    return "";

                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime();
                return dto.ToString("dd/MM HH:mm");
            }
            catch
            {
                return "";
            }
        }

        private static string FormatReportChatTime(double unixTime)
        {
            try
            {
                return FormatReportChatTime((long)unixTime);
            }
            catch
            {
                return "";
            }
        }

        private static BanModCommunicationManager.ReportChatMessage GetLatestAdminMessage(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null || report.Chat == null)
                return null;

            BanModCommunicationManager.ReportChatMessage latest = null;
            for (int i = 0; i < report.Chat.Count; i++)
            {
                BanModCommunicationManager.ReportChatMessage msg = report.Chat[i];
                if (msg == null || !string.Equals(msg.AuthorType ?? "", "admin", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (latest == null || msg.CreatedAt > latest.CreatedAt || msg.Id > latest.Id)
                    latest = msg;
            }

            return latest;
        }
        [HideFromIl2Cpp]
        private BanModCommunicationManager.ReportSummary FindSentReport(int reportId)
        {
            if (sentReports == null)
                return null;

            for (int i = 0; i < sentReports.Count; i++)
            {
                if (sentReports[i] != null && sentReports[i].Id == reportId)
                    return sentReports[i];
            }

            return null;
        }

        [HideFromIl2Cpp]
        private string BuildReportChatText(BanModCommunicationManager.ReportSummary report)
        {
            if (report == null || report.Chat == null || report.Chat.Count <= 0)
                return "\n\n<align=center><color=#aaaaaa>" + T("Comm_NoReportChat", "No chat messages yet.") + "</color></align>";

            string text = "";

            for (int i = 0; i < report.Chat.Count; i++)
            {
                BanModCommunicationManager.ReportChatMessage message = report.Chat[i];
                if (message == null || string.IsNullOrWhiteSpace(message.Message))
                    continue;

                bool isAdmin = string.Equals(message.AuthorType ?? "", "admin", StringComparison.OrdinalIgnoreCase);

                string who = isAdmin
                    ? T("Admin", "Admin")
                    : T("Comm_Player", "You");

                if (!string.IsNullOrWhiteSpace(message.AuthorName))
                    who += " - " + message.AuthorName.Trim();

                string time = FormatReportChatTime(message.CreatedAt);
                string body = RichEscape(message.Message ?? "").Trim();

                if (isAdmin)
                {
                    text +=
                        "<align=left>" +
                        "<color=#8fd3ff><b>" + RichEscape(who) + "</b></color> " +
                        "<color=#888888>" + RichEscape(time) + "</color>\n" +
                        "<mark=#15324Aaa><color=#ffffff>  " + body + "  </color></mark>" +
                        "</align>\n\n";
                }
                else
                {
                    text +=
                        "<align=right>" +
                        "<color=#9dffb0><b>" + RichEscape(who) + "</b></color> " +
                        "<color=#888888>" + RichEscape(time) + "</color>\n" +
                        "<mark=#164A22aa><color=#ffffff>  " + body + "  </color></mark>" +
                        "</align>\n\n";
                }
            }

            if (string.IsNullOrWhiteSpace(text))
                return "\n\n<align=center><color=#aaaaaa>" + T("Comm_NoReportChat", "No chat messages yet.") + "</color></align>";

            return text.TrimEnd();
        }

        private void SendPopupReportMessage()
        {
            if (!popupVisible || !popupIsReportChat || popupReportId <= 0 || isSending)
                return;

            string message = TextOf(popupReportChatInput);
            if (string.IsNullOrWhiteSpace(message))
            {
                SetText(popupStatusText, T("Comm_ErrorMessageRequired", "Il messaggio è obbligatorio."));
                return;
            }

            int id = popupReportId;
            SetSending(true);
            SetText(popupStatusText, T("Comm_Sending", "Invio in corso..."));

            try
            {
                RunCommunicationCoroutine(
                    BanModCommunicationManager.SendReportMessageCoroutine(id, message, (success, result) =>
                    {
                        SetSending(false);

                        if (success)
                        {
                            SetInputText(popupReportChatInput, "");
                            AppendLocalPlayerMessageToPopup(id, message);

                            SetText(
                                popupStatusText,
                                string.IsNullOrWhiteSpace(result)
                                    ? T("Comm_MessageSent", "Messaggio inviato.")
                                    : result
                            );

                            TryRequestImmediateReportPoll();
                            RefreshVisibility();
                        }
                        else
                        {
                            SetText(
                                popupStatusText,
                                string.IsNullOrWhiteSpace(result)
                                    ? T("Comm_SendFailedBody", "Impossibile inviare il messaggio.")
                                    : result
                            );

                            RefreshVisibility();
                        }
                    })
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                SetText(popupStatusText, ex.Message);
                RefreshVisibility();
            }
        }

        [HideFromIl2Cpp]
        private void AppendLocalPlayerMessageToPopup(int reportId, string message)
        {
            try
            {
                if (reportId <= 0 || string.IsNullOrWhiteSpace(message))
                    return;

                BanModCommunicationManager.ReportSummary report = FindSentReport(reportId);

                if (report == null)
                {
                    report = new BanModCommunicationManager.ReportSummary();
                    report.Id = reportId;
                    report.Status = popupReportIsOpen ? "open" : "closed";
                    report.Chat = new List<BanModCommunicationManager.ReportChatMessage>();

                    if (sentReports == null)
                        sentReports = new List<BanModCommunicationManager.ReportSummary>();

                    sentReports.Insert(0, report);
                }

                if (report.Chat == null)
                    report.Chat = new List<BanModCommunicationManager.ReportChatMessage>();

                string playerName = "";
                try
                {
                    if (PlayerControl.LocalPlayer != null)
                        playerName = SafePlayerName(PlayerControl.LocalPlayer);
                }
                catch { }

                int nextId = report.Chat.Count + 1;
                try
                {
                    for (int i = 0; i < report.Chat.Count; i++)
                    {
                        BanModCommunicationManager.ReportChatMessage existing = report.Chat[i];
                        if (existing != null && existing.Id >= nextId)
                            nextId = existing.Id + 1;
                    }
                }
                catch { }

                report.Chat.Add(new BanModCommunicationManager.ReportChatMessage
                {
                    Id = nextId,
                    AuthorType = "player",
                    AuthorName = playerName,
                    Message = message.Trim(),
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

                UpsertCachedReport(report);
                SetText(popupReportDetailsText, BuildReportDetailsText(report));
                SetScrollableText(popupReportChatText, BuildReportChatText(report), false);
            }
            catch { }
        }

        private void ClosePopupReport()
        {
            if (!popupVisible || !popupIsReportChat || popupReportId <= 0 || isSending)
                return;

            int id = popupReportId;
            SetSending(true);
            SetText(popupStatusText, T("Comm_Closing", "Chiusura in corso..."));

            try
            {
                RunCommunicationCoroutine(
                    BanModCommunicationManager.CloseReportCoroutine(id, (success, result) =>
                    {
                        SetSending(false);

                        if (success)
                        {
                            popupReportIsOpen = false;
                            SetText(popupStatusText, string.IsNullOrWhiteSpace(result) ? T("Comm_ReportClosed", "Report chiuso.") : result);
                            RefreshPopupReport(id, string.IsNullOrWhiteSpace(result) ? T("Comm_ReportClosed", "Report chiuso.") : result);
                        }
                        else
                        {
                            SetText(popupStatusText, string.IsNullOrWhiteSpace(result) ? T("Comm_CouldNotCloseReport", "Impossibile chiudere il report.") : result);
                            RefreshVisibility();
                        }
                    })
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                SetText(popupStatusText, ex.Message);
            }
        }

        private void DeletePopupReport()
        {
            if (!popupVisible || !popupIsReportChat || popupReportId <= 0 || isSending)
                return;

            int id = popupReportId;
            SetSending(true);
            SetText(popupStatusText, T("Comm_Deleting", "Cancellazione in corso..."));

            try
            {
                RunCommunicationCoroutine(
                    BanModCommunicationManager.DeleteReportCoroutine(id, (success, result) =>
                    {
                        SetSending(false);

                        if (success)
                        {
                            RemoveCachedReport(id);
                            ShowMessagePopup(T("Comm_ReportDeleted", "Report cancellato"), string.IsNullOrWhiteSpace(result) ? T("Comm_ReportDeleted", "Report cancellato dalla tua lista.") : result);
                        }
                        else
                        {
                            SetText(popupStatusText, string.IsNullOrWhiteSpace(result) ? T("Comm_SendFailedBody", "Operazione non riuscita.") : result);
                            RefreshVisibility();
                        }
                    })
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                SetText(popupStatusText, ex.Message);
            }
        }

        private void RefreshPopupReport(int reportId, string statusMessage)
        {
            if (reportId <= 0)
                return;

            try
            {
                RunCommunicationCoroutine(
                    BanModCommunicationManager.GetMyReportsCoroutine((reports, error) =>
                    {
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            SetText(popupStatusText, error);
                            RefreshVisibility();
                            return;
                        }

                        sentReports = reports ?? new List<BanModCommunicationManager.ReportSummary>();
                        BanModCommunicationManager.ReportSummary updated = FindSentReport(reportId);
                        if (updated != null)
                        {
                            popupReportIsOpen = IsReportOpen(updated);
                            SetText(popupReportDetailsText, BuildReportDetailsText(updated));
                            SetScrollableText(popupReportChatText, BuildReportChatText(updated), false);
                        }

                        SetText(popupStatusText, statusMessage ?? "");
                        RefreshVisibility();
                    })
                );
            }
            catch (Exception ex)
            {
                SetText(popupStatusText, ex.Message);
                RefreshVisibility();
            }
        }

        private void RemoveCachedReport(int reportId)
        {
            if (sentReports == null)
                return;

            for (int i = sentReports.Count - 1; i >= 0; i--)
            {
                if (sentReports[i] != null && sentReports[i].Id == reportId)
                    sentReports.RemoveAt(i);
            }
        }

        private void FocusPopupReportInput()
        {
            try
            {
                if (popupReportChatInput == null || !popupReportIsOpen)
                    return;

                popupReportChatInput.Select();
                popupReportChatInput.ActivateInputField();

                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(popupReportChatInput.gameObject);
            }
            catch { }
        }

        private void SendSelectedReportMessage()
        {
            if (selectedSentReportId <= 0 || isSending)
                return;

            string message = TextOf(sentReportChatInput);
            if (!Require(message, T("Comm_ErrorMessageRequired", "Message is required.")))
                return;

            SetSending(true);
            SetText(statusText, T("Comm_Sending", "Sending..."));

            try
            {
                int id = selectedSentReportId;
                RunCommunicationCoroutine(
                    BanModCommunicationManager.SendReportMessageCoroutine(id, message, (success, result) =>
                    {
                        SetSending(false);
                        SetText(statusText, result ?? "");
                        if (success)
                        {
                            SetInputText(sentReportChatInput, "");
                            SetText(statusText, string.IsNullOrWhiteSpace(result) ? T("Comm_MessageSent", "Message sent.") : result);
                            LoadSentReportsKeepingSelection(id);
                        }
                        else
                        {
                            ShowMessagePopup(T("Comm_SendFailed", "Send Failed"), result ?? "");
                        }
                    })
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                SetText(statusText, ex.Message);
            }
        }

        private void CloseSelectedReport()
        {
            try { Debug.Log("[BANMOD] CloseSelectedReport clicked. selected=" + selectedSentReportId + " sending=" + isSending); } catch { }

            if (selectedSentReportId <= 0)
            {
                SetText(statusText, T("Comm_SelectReport", "Select a report to view details."));
                ShowMessagePopup(T("Comm_Error", "BANMOD"), T("Comm_SelectReport", "Select a report to view details."));
                return;
            }

            if (isSending)
            {
                SetText(statusText, T("Comm_Wait", "Please wait..."));
                return;
            }

            BanModCommunicationManager.ReportSummary current = FindSentReport(selectedSentReportId);
            if (current != null && string.Equals(current.Status ?? "", "closed", StringComparison.OrdinalIgnoreCase))
            {
                SetText(statusText, T("Comm_ReportAlreadyClosed", "Report already closed."));
                RefreshVisibility();
                return;
            }

            int id = selectedSentReportId;

            SetText(statusText, T("Comm_Closing", "Closing..."));

            SetSending(true);

            try
            {
                RunCommunicationCoroutine(
                    BanModCommunicationManager.CloseReportCoroutine(id, (success, result) =>
                    {
                        SetSending(false);
                        SetText(statusText, result ?? "");

                        if (success)
                        {
                            BanModCommunicationManager.ReportSummary report = FindSentReport(id);
                            if (report != null)
                            {
                                report.Status = "closed";
                                SelectSentReport(id);
                                RefreshVisibility();
                            }

                            ShowMessagePopup(T("Comm_ReportClosed", "Report Closed"), string.IsNullOrWhiteSpace(result) ? T("Comm_ReportClosed", "Report Closed") : result);
                            LoadSentReportsKeepingSelection(id);
                        }
                        else
                        {
                            string msg = string.IsNullOrWhiteSpace(result) ? T("Comm_CouldNotCloseReport", "Could not close report.") : result;
                            SetText(statusText, msg);
                            ShowMessagePopup(T("Comm_SendFailed", "Send Failed"), msg);
                        }
                    })
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                string msg = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
                SetText(statusText, msg);
                ShowMessagePopup(T("Comm_SendFailed", "Send Failed"), msg);
            }
        }

        private void DeleteSelectedReport()
        {
            if (selectedSentReportId <= 0 || isSending)
                return;

            SetSending(true);
            SetText(statusText, T("Comm_Deleting", "Deleting..."));

            try
            {
                int id = selectedSentReportId;
                RunCommunicationCoroutine(
                    BanModCommunicationManager.DeleteReportCoroutine(id, (success, result) =>
                    {
                        SetSending(false);
                        SetText(statusText, result ?? "");
                        ShowMessagePopup(success ? T("Comm_ReportDeleted", "Report Deleted") : T("Comm_SendFailed", "Send Failed"), result ?? "");
                        if (success)
                            LoadSentReports();
                    })
                );
            }
            catch (Exception ex)
            {
                SetSending(false);
                SetText(statusText, ex.Message);
            }
        }

        private static string ShortText(string value, int max)
        {
            value = value ?? "";
            if (value.Length <= max)
                return value;
            return value.Substring(0, Math.Max(0, max - 3)) + "...";
        }

        private void SendBugReport()
        {
            if (isSending)
                return;

            string title = TextOf(bugTitleInput);
            string gameMode = TextOf(bugGameModeInput);
            string customWhich = TextOf(bugCustomRolesWhichInput);
            string otherWhich = TextOf(bugOtherModsWhichInput);
            string description = TextOf(bugDescriptionInput);

            if (!Require(title, T("Comm_ErrorTitleRequired", "Title is required."))) return;
            if (!Require(gameMode, T("Comm_ErrorGameModeRequired", "Game mode is required."))) return;
            if (customRolesEnabled && !Require(customWhich, T("Comm_ErrorCustomRolesWhichRequired", "Please specify which custom roles are enabled."))) return;
            if (otherModsInstalled && !Require(otherWhich, T("Comm_ErrorOtherModsWhichRequired", "Please specify which other mods are installed."))) return;
            if (!Require(description, T("Comm_ErrorBugDescriptionRequired", "Bug description is required."))) return;
            if (!EnsureClientReady()) return;

            SetSending(true);
            RunCommunicationCoroutine(
                BanModCommunicationManager.SendBugReportCoroutine(
                    title,
                    gameMode,
                    customRolesEnabled,
                    customWhich,
                    otherModsInstalled,
                    otherWhich,
                    description,
                    HandleSendResult
                )
            );
        }

        private void SendPlayerReport()
        {
            if (isSending)
                return;

            string reason = TextOf(playerReasonInput);

            if (string.IsNullOrWhiteSpace(targetName) && string.IsNullOrWhiteSpace(targetFriendCode) && string.IsNullOrWhiteSpace(targetHashedPuid))
            {
                ShowMessagePopup(T("Comm_Error", "BANMOD"), T("Comm_ErrorSelectPlayer", "Please select the player you want to report."));
                return;
            }

            if (!Require(reason, T("Comm_ErrorReasonRequired", "Reason is required."))) return;
            if (!EnsureClientReady()) return;

            SetSending(true);
            RunCommunicationCoroutine(
                BanModCommunicationManager.SendPlayerReportCoroutine(
                    reason,
                    targetFriendCode,
                    targetName,
                    targetHashedPuid,
                    targetPlayerId,
                    targetPlatform,
                    HandleSendResult
                )
            );
        }

        private void SendSupportRequest()
        {
            if (isSending)
                return;

            string title = TextOf(supportTitleInput);
            string message = TextOf(supportMessageInput);

            if (!Require(title, T("Comm_ErrorTitleRequired", "Title is required."))) return;
            if (!Require(message, T("Comm_ErrorDescriptionRequired", "Description is required."))) return;
            if (!EnsureClientReady()) return;

            SetSending(true);
            RunCommunicationCoroutine(
                BanModCommunicationManager.SendSupportMessageCoroutine(
                    title,
                    message,
                    true,
                    HandleSendResult
                )
            );
        }

        private bool Require(string value, string error)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return true;

            ShowMessagePopup(T("Comm_Error", "BANMOD"), error);
            return false;
        }

        private void TryRequestImmediateReportPoll()
        {
            try { BanModMessagePoller.RequestImmediateReportPoll(); } catch { }
        }
        [HideFromIl2Cpp]
        private void RunCommunicationCoroutine(IEnumerator coroutine)
        {
            try
            {
                if (coroutine == null)
                {
                    SetSending(false);
                    SetText(statusText, T("Comm_Error", "Error"));
                    return;
                }

                try
                {
                    StartCoroutine(coroutine.WrapToIl2Cpp());
                }
                catch
                {
                    if (AmongUsClient.Instance != null)
                        AmongUsClient.Instance.StartCoroutine(coroutine.WrapToIl2Cpp());
                    else
                        throw;
                }
            }
            catch (Exception ex)
            {
                SetSending(false);
                string msg = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
                SetText(statusText, msg);
                try { ShowMessagePopup(T("Comm_Error", "BANMOD"), msg); } catch { }
            }
        }

        private bool EnsureClientReady()
        {
            if (AmongUsClient.Instance != null)
                return true;

            ShowMessagePopup(T("Comm_Error", "BANMOD"), T("Comm_ErrorClientNotReady", "Client is not ready. Try again in a few seconds."));
            return false;
        }

        private void SetSending(bool sending)
        {
            isSending = sending;
            SetText(statusText, sending ? T("Comm_Sending", "Sending...") : "");
            SetInteractable(sendBugButton, !sending);
            SetInteractable(sendPlayerButton, !sending);
            SetInteractable(sendSupportButton, !sending);
            BanModCommunicationManager.ReportSummary activeReport = selectedSentReportId > 0 ? FindSentReport(selectedSentReportId) : null;
            bool selectedOpenReport = activeReport != null && IsReportOpen(activeReport);
            SetInteractable(sentReportChatSendButton, !sending && selectedOpenReport);
            SetInteractable(sentReportCloseButton, !sending && selectedOpenReport);
            SetInteractable(sentReportDeleteButton, !sending && selectedSentReportId > 0);
            SetInteractable(sentReportRefreshButton, !sending);
            SetInteractable(popupReportSendButton, !sending && popupVisible && popupIsReportChat && popupReportIsOpen);
            SetInteractable(popupReportCloseButton, !sending && popupVisible && popupIsReportChat && popupReportIsOpen);
            SetInteractable(popupReportDeleteButton, !sending && popupVisible && popupIsReportChat && popupReportId > 0);
        }

        private void HandleSendResult(bool success, string message)
        {
            SetSending(false);

            if (success)
            {
                SetText(statusText, T("Comm_SentSuccessfully", "Sent successfully."));
                TryRequestImmediateReportPoll();
                ClearForm();
                OpenHomePanel();
                ShowMessagePopup(T("Comm_ReportSent", "Report Sent"), string.IsNullOrWhiteSpace(message) ? T("Comm_ReportSentBody", "Your report was sent successfully.") : message);
            }
            else
            {
                SetText(statusText, T("Comm_SendFailed", "Send failed."));
                ShowMessagePopup(T("Comm_SendFailed", "Send Failed"), string.IsNullOrWhiteSpace(message) ? T("Comm_SendFailedBody", "Could not send the report.") : message);
            }
        }

        private void FocusFirstInput()
        {
            try
            {
                TMP_InputField target = null;

                if (currentPanel == PanelMode.BugReport)
                    target = bugTitleInput;
                else if (currentPanel == PanelMode.PlayerReport)
                    target = playerReasonInput;
                else if (currentPanel == PanelMode.Support)
                    target = supportTitleInput;
                else if (currentPanel == PanelMode.SentReports)
                    target = sentReportChatInput;

                if (target == null)
                    return;

                target.Select();
                target.ActivateInputField();

                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(target.gameObject);
            }
            catch { }
        }

        private static void ApplyModernImage(Image image, Color color, bool addOutline)
        {
            if (image == null)
                return;

            image.sprite = BanModUiStyles.RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;

            if (addOutline)
            {
                Outline outline = image.GetComponent<Outline>();
                if (outline == null)
                    outline = image.gameObject.AddComponent<Outline>();

                outline.effectColor = new Color(
                    BanModUiStyles.AccentColor.r,
                    BanModUiStyles.AccentColor.g,
                    BanModUiStyles.AccentColor.b,
                    0.20f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = true;
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Vector2 pos, Color color)
        {
            GameObject go = CreateEmpty(parent, name, size, pos);
            Image image = go.AddComponent<Image>();
            ApplyModernImage(image, color, true);
            image.raycastTarget = true;
            return go;
        }

        private static GameObject CreateEmpty(Transform parent, string name, Vector2 size, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            return go;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment, Color color, Vector2 size, Vector2 pos)
        {
            GameObject go = CreateEmpty(parent, name, size, pos);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.enableWordWrapping = true;
            return label;
        }

        private static TextMeshProUGUI CreateScrollableLabel(
            Transform parent,
            string name,
            string text,
            int fontSize,
            TextAlignmentOptions alignment,
            Color color,
            Vector2 size,
            Vector2 pos,
            out ScrollRect scrollRect)
        {
            GameObject root = CreateEmpty(parent, name + "Scroll", size, pos);

            Image background = root.AddComponent<Image>();
            ApplyModernImage(background, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
            background.raycastTarget = true;

            scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.inertia = false;
            scrollRect.scrollSensitivity = 45f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = CreateEmpty(root.transform, "Viewport", size, Vector2.zero);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.offsetMin = new Vector2(10f, 10f);
            viewportRect.offsetMax = new Vector2(-24f, -10f);

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportImage.raycastTarget = true;

            viewport.AddComponent<RectMask2D>();

            GameObject content = CreateEmpty(viewport.transform, "Content", new Vector2(0f, size.y - 20f), Vector2.zero);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, size.y - 20f);

            TextMeshProUGUI label = CreateLabel(
                content.transform,
                name,
                text,
                fontSize,
                alignment,
                color,
                new Vector2(size.x - 44f, size.y - 20f),
                Vector2.zero
            );

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(4f, 4f);
            labelRect.offsetMax = new Vector2(-4f, -4f);

            label.raycastTarget = false;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Overflow;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.verticalNormalizedPosition = 1f;

            GameObject scrollHint = CreateEmpty(root.transform, "ScrollHint", new Vector2(6f, size.y - 28f), Vector2.zero);
            RectTransform hintRect = scrollHint.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(1f, 0.5f);
            hintRect.anchorMax = new Vector2(1f, 0.5f);
            hintRect.pivot = new Vector2(1f, 0.5f);
            hintRect.anchoredPosition = new Vector2(-8f, 0f);

            Image hintImage = scrollHint.AddComponent<Image>();
            hintImage.color = new Color(
                BanModUiStyles.AccentColor.r,
                BanModUiStyles.AccentColor.g,
                BanModUiStyles.AccentColor.b,
                0.45f);
            hintImage.raycastTarget = false;

            return label;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 pos, Color color, Action onClick)
        {
            GameObject go = CreateEmpty(parent, name, size, pos);
            Image image = go.AddComponent<Image>();
            ApplyModernImage(image, color, false);
            image.raycastTarget = true;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            if (onClick != null)
                button.onClick.AddListener(onClick);

            TextMeshProUGUI text = CreateLabel(go.transform, "Text", label, 17, TextAlignmentOptions.Center, Color.white, size, Vector2.zero);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static TMP_InputField CreateInput(Transform parent, string name, string placeholderText, bool multiline, Vector2 size, Vector2 pos)
        {
            GameObject go = CreateEmpty(parent, name, size, pos);

            Image image = go.AddComponent<Image>();
            ApplyModernImage(image, new Color(0.10f, 0.11f, 0.14f, 0.98f), true);
            image.raycastTarget = true;

            TMP_InputField input = go.AddComponent<TMP_InputField>();
            input.targetGraphic = image;
            input.characterLimit = multiline ? 6000 : 180;
            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Standard;
            input.inputType = TMP_InputField.InputType.Standard;
            input.richText = false;

            GameObject viewport = CreateEmpty(go.transform, "Viewport", size, Vector2.zero);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.offsetMin = new Vector2(10f, 6f);
            viewportRect.offsetMax = new Vector2(-10f, -6f);

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = false;

            viewport.AddComponent<RectMask2D>();

            TextMeshProUGUI text = CreateLabel(
                viewport.transform,
                "Text",
                "",
                multiline ? 16 : 18,
                multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft,
                Color.white,
                new Vector2(size.x - 20f, size.y - 12f),
                Vector2.zero
            );

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            text.enableWordWrapping = multiline;
            text.overflowMode = multiline ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            text.raycastTarget = false;

            TextMeshProUGUI placeholder = CreateLabel(
                viewport.transform,
                "Placeholder",
                placeholderText,
                multiline ? 16 : 18,
                multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft,
                new Color(1f, 1f, 1f, 0.45f),
                new Vector2(size.x - 20f, size.y - 12f),
                Vector2.zero
            );

            RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            placeholder.enableWordWrapping = multiline;
            placeholder.overflowMode = multiline ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            placeholder.raycastTarget = false;

            input.textViewport = viewportRect;
            input.textComponent = text;
            input.placeholder = placeholder;

            return input;
        }

        private static string TextOf(TMP_InputField input)
        {
            try { return input != null ? (input.text ?? "").Trim() : ""; }
            catch { return ""; }
        }

        private static void SetInputText(TMP_InputField input, string value)
        {
            try
            {
                if (input != null)
                    input.text = value ?? "";
            }
            catch { }
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            try
            {
                if (text != null)
                    text.text = value ?? "";
            }
            catch { }
        }

        private static void SetScrollableText(TextMeshProUGUI text, string value, bool scrollToTop)
        {
            SetText(text, value);

            try
            {
                if (text == null)
                    return;

                RectTransform textRect = text.GetComponent<RectTransform>();
                RectTransform contentRect = textRect != null ? textRect.parent as RectTransform : null;
                RectTransform viewportRect = contentRect != null ? contentRect.parent as RectTransform : null;
                ScrollRect scroll = viewportRect != null && viewportRect.parent != null
                    ? viewportRect.parent.GetComponent<ScrollRect>()
                    : null;

                text.gameObject.SetActive(true);
                text.enabled = true;
                text.enableWordWrapping = true;
                text.overflowMode = TextOverflowModes.Overflow;
                text.SetLayoutDirty();
                text.SetVerticesDirty();

                Canvas.ForceUpdateCanvases();
                text.ForceMeshUpdate(true, true);

                float viewportHeight = 410f;
                if (viewportRect != null && viewportRect.rect.height > 1f)
                    viewportHeight = viewportRect.rect.height;

                float textWidth = 740f;
                if (textRect != null && textRect.rect.width > 1f)
                    textWidth = textRect.rect.width;

                Vector2 preferred = text.GetPreferredValues(value ?? "", textWidth, 0f);
                float preferredHeight = Mathf.Max(viewportHeight + 1f, preferred.y + 48f);

                if (contentRect != null)
                {
                    contentRect.anchorMin = new Vector2(0f, 1f);
                    contentRect.anchorMax = new Vector2(1f, 1f);
                    contentRect.pivot = new Vector2(0.5f, 1f);
                    contentRect.sizeDelta = new Vector2(0f, preferredHeight);
                    contentRect.anchoredPosition = Vector2.zero;
                }

                if (textRect != null)
                {
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(4f, 4f);
                    textRect.offsetMax = new Vector2(-4f, -4f);
                }

                Canvas.ForceUpdateCanvases();

                if (contentRect != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

                if (scroll != null)
                {
                    scroll.enabled = false;
                    scroll.content = contentRect;
                    scroll.viewport = viewportRect;
                    scroll.enabled = true;
                    scroll.StopMovement();
                    scroll.velocity = Vector2.zero;
                    scroll.vertical = true;
                    scroll.horizontal = false;
                    scroll.inertia = false;
                    scroll.movementType = ScrollRect.MovementType.Clamped;
                    scroll.scrollSensitivity = 45f;
                    scroll.verticalNormalizedPosition = scrollToTop ? 1f : 0f;
                }

                text.SetLayoutDirty();
                text.SetVerticesDirty();
                text.ForceMeshUpdate(true, true);
                Canvas.ForceUpdateCanvases();
            }
            catch { }
        }
        private static GameObject GetScrollableRoot(TextMeshProUGUI text)
        {
            try
            {
                if (text == null)
                    return null;

                RectTransform textRect = text.GetComponent<RectTransform>();
                RectTransform contentRect = textRect != null ? textRect.parent as RectTransform : null;
                RectTransform viewportRect = contentRect != null ? contentRect.parent as RectTransform : null;

                if (viewportRect != null && viewportRect.parent != null)
                    return viewportRect.parent.gameObject;
            }
            catch { }

            return text != null ? text.gameObject : null;
        }

        private static void SetActive(GameObject go, bool active)
        {
            try
            {
                if (go != null && go.activeSelf != active)
                    go.SetActive(active);
            }
            catch { }
        }

        private static void SetInteractable(Button button, bool value)
        {
            try
            {
                if (button != null)
                    button.interactable = value;
            }
            catch { }
        }

        private static string T(string key, string fallback)
        {
            try
            {
                string value = Translator.GetString(key);
                if (!string.IsNullOrWhiteSpace(value) && value != key)
                    return value;
            }
            catch { }

            return fallback;
        }

        private static string SafePlayerName(PlayerControl p)
        {
            try
            {
                if (p != null && p.Data != null && !string.IsNullOrWhiteSpace(p.Data.PlayerName))
                    return p.Data.PlayerName;
            }
            catch { }

            return "Unknown";
        }

        private static string SafeTargetFriendCode(PlayerControl p)
        {
            try
            {
                if (p != null &&
                    p.Data != null &&
                    !string.IsNullOrWhiteSpace(p.Data.FriendCode))
                {
                    return p.Data.FriendCode.Trim().ToLowerInvariant();
                }
            }
            catch { }

            return "";
        }

        private static string SafeTargetHashedPuid(PlayerControl p)
        {
            try
            {
                object data = p != null ? (object)p.Data : null;
                if (data == null)
                    return "";

                MethodInfo method = data.GetType().GetMethod("GetHashedPuid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object value = method != null ? method.Invoke(data, null) : null;
                string text = value != null ? value.ToString() : "";

                if (text == "e3b0cb855")
                    return "";

                return text;
            }
            catch { return ""; }
        }

        private static string SafeTargetPlayerId(PlayerControl p)
        {
            try { return p != null ? p.PlayerId.ToString() : ""; }
            catch { return ""; }
        }

        private static string SafeTargetPlatform(PlayerControl p)
        {
            try
            {
                object data = p != null ? (object)p.Data : null;
                string value = ReadStringMember(data, "Platform");
                if (!string.IsNullOrWhiteSpace(value))
                    return value;

                value = ReadStringMember(data, "platform");
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            catch { }

            return "";
        }

        private static string ReadStringMember(object instance, string memberName)
        {
            if (instance == null)
                return "";

            Type type = instance.GetType();

            try
            {
                PropertyInfo prop = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object value = prop != null ? prop.GetValue(instance, null) : null;
                if (value != null)
                    return value.ToString();
            }
            catch { }

            try
            {
                FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object value = field != null ? field.GetValue(instance) : null;
                if (value != null)
                    return value.ToString();
            }
            catch { }

            return "";
        }
    }

    [HarmonyPatch]
    public static class BanModCommunicationUiBlockPassiveButtonsPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            string[] names =
            {
                "ReceiveClickDown",
                "ReceiveClickUp",
                "ReceiveClick",
                "ReceiveClickUpHandler",
                "DoClick",
                "OnClick",
                "OnMouseDown",
                "OnMouseUp",
                "OnMouseUpAsButton"
            };

            Type passiveButtonType = typeof(PassiveButton);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo[] methods = passiveButtonType.GetMethods(flags);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];

                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo method = methods[j];
                    if (method != null && method.Name == name)
                        yield return method;
                }
            }
        }

        public static bool Prefix()
        {
            try
            {
                if (BanModCommunicationUi.IsUiOpen)
                    return false;
            }
            catch { }

            return true;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    public static class BanModCommunicationUiBootstrapPatch
    {
        public static void Postfix()
        {
            try { BanModCommunicationUi.EnsureCreated(); }
            catch { }
        }
    }
}
