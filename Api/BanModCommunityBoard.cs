////credits and licenses in the resources folder/
//using BepInEx.Unity.IL2CPP.Utils;
//using BepInEx.Unity.IL2CPP.Utils.Collections;
//using HarmonyLib;
//using Il2CppInterop.Runtime;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using System.Text.Json;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//namespace BanMod
//{
//    internal static class BanModCommunityBoard
//    {
//        private const string RootName = "BanMod_StandaloneCommunity";
//        private const int PageSize = 6;
//        private const int CommunityChatCharacterLimit = 1000;


//        private const float CommunityButtonYOffsetFallback = 0.54f;
//        private const string CommunityLastSeenPostKey = "BANMOD_Community_LastSeenPostId_v2";
//        private const string CommunityLastSeenReplyKey = "BANMOD_Community_LastSeenReplyId_v2";
//        private const string CommunityLastSeenChatKey = "BANMOD_Community_LastSeenChatId_v1";
//        private const string CommunityDisplayNameCacheKey = "BANMOD_Community_DisplayName_v1";
//        private const float CommunityMentionPollIntervalSeconds = 10f;
//        private const string CommunityApiBase = BanModApiConfig.ApiBaseUrl;

//        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

//        private static MainMenuManager _mainMenuOwner;
//        private static GameObject _mainMenuButton;
//        private static GameObject _communityUnreadDot;
//        private static bool _mainMenuInstallRunning;
//        private static int _latestKnownCommunityPostId;
//        private static int _latestKnownCommunityReplyId;
//        private static int _latestKnownCommunityChatId;


//        private static readonly HashSet<int> CommunityMentionNotifiedIds = new HashSet<int>();
//        private static bool _communityMentionInitialSyncDone;
//        private static bool _communityMentionPollRunning;
//        private static int _communityMentionLastMessageId;
//        private static float _communityNextMentionPollTime;
//        private static float _communityMentionFailureBackoff = 10f;


//        private static GameObject _root;
//        private static GameObject _window;
//        private static GameObject _listPanel;
//        private static GameObject _detailPanel;
//        private static GameObject _composePanel;
//        private static GameObject _chatPanel;
//        private static RectTransform _chatContent;
//        private static ScrollRect _chatScroll;
//        private static GameObject _chatListRoot;
//        private static readonly List<GameObject> ChatRowObjects = new List<GameObject>();
//        private static TMP_InputField _chatInput;
//        private static Button _chatSendButton;
//        private static int _editingChatMessageId;
//        private static string _chatDraftBeforeEdit = "";
//        private static Button _chatTranslateButton;
//        private static Button _chatTagButton;
//        private static Button _chatReplyButton;
//        private static TMP_InputField _profileInput;
//        private static TextMeshProUGUI _profileNameText;
//        private static TextMeshProUGUI _profileHeaderText;
//        private static Button _profileActionButton;
//        private static Button _profileMenuButton;
//        private static GameObject _profileMenuPanel;
//        private static string _communityDisplayName = "";
//        private static bool _profileNameLocked;
//        private static string _profilePendingName = "";
//        private static bool _profileStateLoaded;
//        private static bool _profileDefaultAttempted;
//        private static bool _profileDefaultRequestRunning;
//        private static string _privateLoginUsername = "";
//        private static GameObject _managePanel;
//        private static TMP_InputField _manageTitleInput;
//        private static TMP_InputField _manageMessageInput;
//        private static TextMeshProUGUI _manageHeading;
//        private static string _manageMode = "";
//        private static int _manageTargetId;
//        private static Button _newPostButton;
//        private static Button _tabChatButton;
//        private static Button _tabPrivateChatButton;
//        private static Button _tabBugReportButton;
//        private static Button _tabSuggestionsButton;
//        private static TextMeshProUGUI _privateChatTabUnreadBadgeText;
//        private static TextMeshProUGUI _bugTabUnreadBadgeText;
//        private static bool _showingChat = true;
//        private static bool _showingPrivateChat;
//        private static bool _showingBugReport;
//        private static bool _chatPolling;
//        private static bool _backgroundProfileRequested;




//        private static GameObject _bugReportPanel;
//        private static RectTransform _bugReportsContent;
//        private static ScrollRect _bugReportsScroll;
//        private static TextMeshProUGUI _bugConversationText;
//        private static RectTransform _bugConversationContent;
//        private static ScrollRect _bugConversationScroll;
//        private static TextMeshProUGUI _bugSelectedHeader;
//        private static TMP_InputField _bugTitleInput;
//        private static TMP_InputField _bugGameModeInput;
//        private static TMP_InputField _bugCustomRolesWhichInput;
//        private static TMP_InputField _bugOtherModsWhichInput;
//        private static TMP_InputField _bugDescriptionInput;
//        private static TMP_InputField _bugReplyInput;
//        private static Button _bugCustomYesButton;
//        private static Button _bugCustomNoButton;
//        private static Button _bugOtherYesButton;
//        private static Button _bugOtherNoButton;
//        private static Button _bugSendButton;
//        private static Button _bugReplyButton;
//        private static Button _bugCloseButton;
//        private static Button _bugDeleteButton;
//        private static bool _bugCustomRolesEnabled;
//        private static bool _bugOtherModsInstalled;
//        private static bool _bugRequestRunning;
//        private static int _selectedBugReportId;
//        private static int _bugReportUnreadCount;
//        private static readonly List<BanModCommunicationManager.ReportSummary> BugReports =
//            new List<BanModCommunicationManager.ReportSummary>();



//        private const float PrivateChatPresenceIntervalSeconds = 20f;
//        private const float PrivateChatOpenPollIntervalSeconds = 2f;
//        private const float PrivateChatClosedPollIntervalSeconds = 5f;
//        private const float PrivateChatPlayersPollIntervalSeconds = 10f;
//        private const long PrivateChatOnlineThresholdSeconds = 90L;

//        private static GameObject _privateChatPanel;
//        private static RectTransform _privatePlayersContent;
//        private static ScrollRect _privatePlayersScroll;
//        private static RectTransform _privateMessagesContent;
//        private static ScrollRect _privateMessagesScroll;
//        private static TMP_InputField _privateChatInput;
//        private static TextMeshProUGUI _privateConversationTitle;
//        private static Button _privateReportButton;
//        private static Button _privateTranslateButton;
//        private static readonly List<PrivateChatPlayer> PrivateChatPlayers = new List<PrivateChatPlayer>();
//        private static readonly List<PrivateChatMessage> PrivateChatMessages = new List<PrivateChatMessage>();
//        private static readonly Dictionary<long, PrivateChatTranslationEntry> PrivateChatTranslations =
//            new Dictionary<long, PrivateChatTranslationEntry>();
//        private static readonly Dictionary<int, PrivateChatTranslationEntry> CommunityChatTranslations =
//            new Dictionary<int, PrivateChatTranslationEntry>();
//        private static readonly Dictionary<int, PrivateChatTranslationEntry> SuggestionTitleTranslations =
//            new Dictionary<int, PrivateChatTranslationEntry>();
//        private static readonly Dictionary<int, PrivateChatTranslationEntry> SuggestionMessageTranslations =
//            new Dictionary<int, PrivateChatTranslationEntry>();
//        private static readonly Dictionary<int, PrivateChatTranslationEntry> SuggestionReplyTranslations =
//            new Dictionary<int, PrivateChatTranslationEntry>();
//        private static readonly HashSet<long> PrivateKnownMessageIds = new HashSet<long>();
//        private static readonly HashSet<long> PrivateUnreadMessageIds = new HashSet<long>();
//        private static readonly HashSet<long> PrivatePendingDeliveryAckIds = new HashSet<long>();
//        private static string _privateActiveFriendCode = "";
//        private static string _privateActiveName = "";
//        private static int _selectedPrivateMessageIndex = -1;
//        private static string _privateBanModToken = "";
//        private static long _privateChatSessionStartedAt;
//        private static long _privateLastMessageId;
//        private static bool _privateInitialSyncDone;
//        private static bool _privateSessionRequestRunning;
//        private static bool _privatePresenceRequestRunning;
//        private static bool _privatePlayersRequestRunning;
//        private static bool _privateMessagesRequestRunning;
//        private static bool _privateAckRunning;
//        private static bool _privateSendRunning;
//        private static bool _privateReportRunning;
//        private static float _privateNextSessionAttemptTime;
//        private static float _privateNextPresenceTime;
//        private static float _privateNextPlayersPollTime;
//        private static float _privateNextMessagesPollTime;



//        private static GameObject _launcherRoot;
//        private static GameObject _launcherButton;
//        private static RectTransform _launcherButtonRect;
//        private static TextMeshProUGUI _launcherUnreadDot;
//        private static bool _launcherDragging;
//        private static bool _launcherMoved;
//        private static Vector2 _launcherDragOffset;
//        private static Vector2 _launcherDragStartMouse;
//        private static float _launcherSuppressClickUntil;
//        private const string LauncherPosXKey = "BANMOD_COMMUNITY_ICON_X_V2";
//        private const string LauncherPosYKey = "BANMOD_COMMUNITY_ICON_Y_V2";
//        private static bool _publicCommunityUnread;
//        private static readonly List<BoxCollider2D> BlockedMainMenuColliders = new List<BoxCollider2D>();
//        private static readonly List<bool> BlockedMainMenuColliderStates = new List<bool>();
//        private static readonly List<PassiveButton> BlockedMainMenuButtons = new List<PassiveButton>();
//        private static readonly List<bool> BlockedMainMenuButtonStates = new List<bool>();
//        private static bool _mainMenuInputBlocked;
//        private static TextMeshProUGUI _statusText;
//        private static TextMeshProUGUI _detailText;
//        private static RectTransform _detailContent;
//        private static ScrollRect _detailScroll;
//        private static RectTransform _suggestionsListContent;
//        private static ScrollRect _suggestionsListScroll;
//        private static TMP_InputField _titleInput;
//        private static TMP_InputField _messageInput;
//        private static TMP_InputField _replyInput;
//        private static Button _starButton;
//        private static Button _suggestionTranslateButton;
//        private static readonly List<GameObject> ListButtons = new List<GameObject>();
//        private static readonly List<CommunityPost> Posts = new List<CommunityPost>();
//        private static readonly List<CommunityReply> Replies = new List<CommunityReply>();
//        private static CommunityPost _selected;
//        private static bool _requestRunning;


//        private static bool _manageRequestRunning;

//        private sealed class CommunityPost
//        {
//            public int Id;
//            public string Kind = "suggestion";
//            public string Title = "";
//            public string Message = "";
//            public string Status = "open";
//            public long CreatedAt;
//            public long UpdatedAt;
//            public int Stars;
//            public int ReplyCount;
//            public bool MyStar;
//            public bool MyPost;
//            public string Author = "";
//        }

//        private sealed class CommunityReply
//        {
//            public int Id;
//            public int PostId;
//            public string Message = "";
//            public long CreatedAt;
//            public long UpdatedAt;
//            public bool MyReply;
//            public string Author = "";
//        }

//        private sealed class CommunityChatMessage
//        {
//            public int Id;
//            public string Message = "";
//            public long CreatedAt;
//            public long UpdatedAt;
//            public bool MyMessage;
//            public string Author = "";
//        }

//        private sealed class PrivateChatPlayer
//        {
//            public string FriendCode = "";
//            public string PlayerName = "";
//            public bool IsOnline;
//            public bool InLobby;
//            public long LastSeen;
//        }

//        private sealed class PrivateChatMessage
//        {
//            public long Id;
//            public string SenderFriendCode = "";
//            public string SenderName = "";
//            public string RecipientFriendCode = "";
//            public string RecipientName = "";
//            public bool IsPrivate;
//            public bool PendingDelivery;
//            public string Message = "";
//            public long CreatedAt;
//        }

//        private sealed class PrivateChatTranslationEntry
//        {
//            public string SettingsKey = "";
//            public string Original = "";
//            public string Translation = "";
//            public bool Attempted;
//            public bool Manual;
//        }

//        private static readonly List<CommunityChatMessage> ChatMessages = new List<CommunityChatMessage>();
//        private static int _selectedChatIndex = -1;
//        private static int _selectedReplyIndex = -1;
//        private static Button _reportChatButton;
//        private static Button _reportReplyButton;

//        internal static bool IsOpen { get { return _root != null && _root.activeSelf; } }

//        internal static void OpenFromMainMenu()
//        {
//            try
//            {
//                EnsureStandaloneUi();
//                if (_root == null) return;
//                BlockMainMenuInput(true);
//                _root.SetActive(true);
//                RefreshLauncherVisibility();
//                if (_composePanel != null) _composePanel.SetActive(false);
//                SetStatus("Connecting to Community...");
//                FetchProfile();
//                ShowChatTab();
//            }
//            catch (Exception ex)
//            {
//                BlockMainMenuInput(false);
//                Debug.LogError("[BANMOD Community] Cannot open standalone Community: " + ex);
//            }
//        }
//        internal static void InstallMainMenuButton(MainMenuManager menu)
//        {
//            if (menu == null) return;
//            if (_mainMenuButton != null) return;
//            if (_mainMenuInstallRunning) return;

//            _mainMenuInstallRunning = true;
//            _mainMenuOwner = menu;
//            menu.StartCoroutine(CoInstallMainMenuButton(menu));
//        }

//        private static IEnumerator CoInstallMainMenuButton(MainMenuManager menu)
//        {


//            PassiveButton stockNews = null;
//            for (int attempt = 0; attempt < 180 && menu != null; attempt++)
//            {
//                stockNews = FindStockNewsButton(menu);
//                if (stockNews != null && stockNews.gameObject != null && stockNews.gameObject.activeInHierarchy)
//                    break;

//                if (attempt == 0)
//                    Debug.Log("[BANMOD Community] Waiting for the stock News button to become available...");

//                yield return new WaitForSeconds(0.25f);
//            }

//            _mainMenuInstallRunning = false;
//            if (menu == null) yield break;

//            if (stockNews == null || stockNews.gameObject == null)
//            {
//                Debug.LogError("[BANMOD Community] Stock News button was still not found after MainMenu startup.");
//                DumpMainMenuButtons(menu);
//                yield break;
//            }

//            try
//            {
//                // BuildCommunityMainMenuButton(menu, stockNews); // disabilitato: niente pulsante COMMUNITY accanto a NEWS
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError("[BANMOD Community] Failed to build main-menu Community button: " + ex);
//                yield break;
//            }


//            if (_mainMenuButton != null)
//                menu.StartCoroutine(CommunityUnreadPollingLoop(menu));
//        }

//        private static PassiveButton FindStockNewsButton(MainMenuManager menu)
//        {
//            if (menu == null) return null;

//            try
//            {
//                var buttons = menu.GetComponentsInChildren<PassiveButton>(true);
//                if (buttons != null)
//                {

//                    for (int i = 0; i < buttons.Length; i++)
//                    {
//                        var button = buttons[i];
//                        if (button == null || button.gameObject == null) continue;
//                        string objectName = (button.gameObject.name ?? "").ToUpperInvariant();
//                        if (objectName.Contains("NEWS")) return button;
//                    }



//                    for (int i = 0; i < buttons.Length; i++)
//                    {
//                        var button = buttons[i];
//                        if (button == null || button.gameObject == null) continue;
//                        if (ButtonHasAnyText(button, "NOVIT", "NEWS", "NOUVE", "NOVED", "NOTIC", "NEUIG", "НОВОСТ"))
//                            return button;
//                    }
//                }
//            }
//            catch { }



//            try
//            {
//                var objects = Resources.FindObjectsOfTypeAll(Il2CppType.Of<PassiveButton>());
//                if (objects != null)
//                {
//                    for (int i = 0; i < objects.Length; i++)
//                    {
//                        var button = objects[i] as PassiveButton;
//                        if (button == null || button.gameObject == null || button.transform == null) continue;
//                        if (menu.transform != null && !button.transform.IsChildOf(menu.transform)) continue;

//                        string objectName = (button.gameObject.name ?? "").ToUpperInvariant();
//                        if (objectName.Contains("NEWS") ||
//                            ButtonHasAnyText(button, "NOVIT", "NEWS", "NOUVE", "NOVED", "NOTIC", "NEUIG", "НОВОСТ"))
//                            return button;
//                    }
//                }
//            }
//            catch { }

//            return null;
//        }

//        private static bool ButtonHasAnyText(PassiveButton button, params string[] tokens)
//        {
//            if (button == null) return false;
//            try
//            {
//                var labels = button.GetComponentsInChildren<TMP_Text>(true);
//                if (labels == null) return false;
//                for (int i = 0; i < labels.Length; i++)
//                {
//                    var label = labels[i];
//                    if (label == null) continue;
//                    string value = (label.text ?? "").Trim().ToUpperInvariant();
//                    if (value.Length == 0) continue;
//                    for (int t = 0; t < tokens.Length; t++)
//                    {
//                        if (value.Contains(tokens[t])) return true;
//                    }
//                }
//            }
//            catch { }
//            return false;
//        }

//        private static PassiveButton FindMenuButton(MainMenuManager menu, string[] objectTokens, string[] textTokens)
//        {
//            if (menu == null) return null;
//            try
//            {
//                var buttons = menu.GetComponentsInChildren<PassiveButton>(true);
//                if (buttons == null) return null;

//                for (int i = 0; i < buttons.Length; i++)
//                {
//                    var button = buttons[i];
//                    if (button == null || button.gameObject == null) continue;
//                    string name = (button.gameObject.name ?? "").ToUpperInvariant();
//                    for (int t = 0; t < objectTokens.Length; t++)
//                    {
//                        if (name.Contains(objectTokens[t])) return button;
//                    }
//                }

//                for (int i = 0; i < buttons.Length; i++)
//                {
//                    var button = buttons[i];
//                    if (button == null) continue;
//                    if (ButtonHasAnyText(button, textTokens)) return button;
//                }
//            }
//            catch { }
//            return null;
//        }

//        private static void BuildCommunityMainMenuButton(MainMenuManager menu, PassiveButton originalPassive)
//        {
//            if (menu == null || originalPassive == null || originalPassive.gameObject == null) return;
//            if (_mainMenuButton != null) return;

//            const float halfRatio = 0.475f;
//            const float gapRatio = 0.050f;
//            GameObject original = originalPassive.gameObject;
//            Transform parent = original.transform.parent;
//            if (parent == null) return;

//            Debug.Log("[BANMOD Community] Stock News button found: " + original.name);
//            Vector2 stockSize = GetPassiveButtonSize(originalPassive);
//            float fullWidthInParent = GetButtonWidthInParent(originalPassive, parent);
//            if (fullWidthInParent <= 0.05f)
//                fullWidthInParent = stockSize.x * Mathf.Abs(original.transform.localScale.x);
//            if (fullWidthInParent <= 0.05f)
//            {
//                Debug.LogError("[BANMOD Community] Could not determine the stock News button width.");
//                return;
//            }

//            Vector3 stockPosition = original.transform.localPosition;
//            Vector3 stockScale = original.transform.localScale;
//            float centerOffset = fullWidthInParent * ((halfRatio + gapRatio) * 0.5f);

//            GameObject clone = UnityEngine.Object.Instantiate(original, parent);
//            clone.name = "BanMod_CommunityNewsButton";
//            PassiveButton passive = clone.GetComponent<PassiveButton>();
//            if (passive == null) passive = clone.GetComponentInChildren<PassiveButton>(true);
//            if (passive == null)
//            {
//                UnityEngine.Object.Destroy(clone);
//                Debug.LogError("[BANMOD Community] Cloned News object has no PassiveButton.");
//                return;
//            }

//            DisableAspectPositionsUnder(original.transform);
//            DisableAspectPositionsUnder(clone.transform);




//            original.transform.localScale = new Vector3(stockScale.x * halfRatio, stockScale.y, stockScale.z);
//            clone.transform.localScale = new Vector3(stockScale.x * halfRatio, stockScale.y, stockScale.z);
//            original.transform.localPosition = stockPosition + new Vector3(-centerOffset, 0f, 0f);
//            clone.transform.localPosition = stockPosition + new Vector3(centerOffset, 0f, 0f);

//            try
//            {
//                NewsCountButton[] copiedNews = clone.GetComponentsInChildren<NewsCountButton>(true);
//                if (copiedNews != null)
//                    for (int i = 0; i < copiedNews.Length; i++)
//                        if (copiedNews[i] != null)
//                        {
//                            copiedNews[i].enabled = false;
//                            UnityEngine.Object.Destroy(copiedNews[i]);
//                        }
//            }
//            catch { }



//            TMP_Text stockNewsSource = FindNewsMainLabel(original);
//            string stockNewsText = stockNewsSource == null ? "NEWS" : (stockNewsSource.text ?? "NEWS").Trim();
//            if (string.IsNullOrWhiteSpace(stockNewsText)) stockNewsText = "NEWS";
//            TMP_Text newsLabel = InstallCleanSplitLabel(originalPassive, stockNewsText, 2f);


//            HideCommunityDecorativeSprites(passive);
//            TMP_Text communityLabel = InstallCleanSplitLabel(passive, "COMMUNITY", 2f);
//            RemoveCopiedNewsBadge(clone, communityLabel);
//            RepositionNumericBadge(original, 0.20f);

//            passive.OnClick = new Button.ButtonClickedEvent();
//            passive.OnClick.AddListener((UnityAction)delegate { OpenFromMainMenu(); });
//            _mainMenuButton = clone;
//            CreateCommunityUnreadDot(clone, communityLabel);
//            clone.SetActive(true);
//            EnforceCommunityButtonVisuals();
//            RefreshCombinedUnread();
//            Debug.Log("[BANMOD Community] News row split: separate scaled hit-boxes, 47.5% | 5% | 47.5%.");
//        }

//        private static float GetButtonWidthInParent(PassiveButton button, Transform parent)
//        {
//            if (button == null || parent == null) return 0f;
//            try
//            {
//                BoxCollider2D col = button.GetComponent<BoxCollider2D>();
//                if (col != null && col.size.x > 0.05f)
//                {
//                    Vector3 leftWorld = button.transform.TransformPoint(new Vector3(col.offset.x - col.size.x * 0.5f, col.offset.y, 0f));
//                    Vector3 rightWorld = button.transform.TransformPoint(new Vector3(col.offset.x + col.size.x * 0.5f, col.offset.y, 0f));
//                    float left = parent.InverseTransformPoint(leftWorld).x;
//                    float right = parent.InverseTransformPoint(rightWorld).x;
//                    return Mathf.Abs(right - left);
//                }
//            }
//            catch { }
//            return 0f;
//        }

//        private static void CounterScaleForegroundX(PassiveButton button, float ratio)
//        {
//            if (button == null || ratio <= 0.01f) return;
//            try
//            {
//                var labels = button.GetComponentsInChildren<TMP_Text>(true);
//                if (labels != null)
//                    for (int i = 0; i < labels.Length; i++)
//                        if (labels[i] != null)
//                        {
//                            Vector3 s = labels[i].transform.localScale;
//                            labels[i].transform.localScale = new Vector3(s.x / ratio, s.y, s.z);
//                        }
//            }
//            catch { }
//            try
//            {
//                Transform inactive = button.inactiveSprites == null ? null : button.inactiveSprites.transform;
//                Transform active = button.activeSprites == null ? null : button.activeSprites.transform;
//                var sprites = button.GetComponentsInChildren<SpriteRenderer>(true);
//                if (sprites != null)
//                    for (int i = 0; i < sprites.Length; i++)
//                    {
//                        var sr = sprites[i];
//                        if (sr == null) continue;
//                        Transform tr = sr.transform;
//                        if ((inactive != null && (tr == inactive || tr.IsChildOf(inactive))) ||
//                            (active != null && (tr == active || tr.IsChildOf(active)))) continue;
//                        Vector3 s = tr.localScale;
//                        tr.localScale = new Vector3(s.x / ratio, s.y, s.z);
//                    }
//            }
//            catch { }
//        }

//        private static void HideCommunityDecorativeSprites(PassiveButton button)
//        {
//            if (button == null) return;
//            try
//            {
//                var sprites = button.GetComponentsInChildren<SpriteRenderer>(true);
//                if (sprites == null || sprites.Length == 0) return;





//                float largestArea = 0f;
//                for (int i = 0; i < sprites.Length; i++)
//                {
//                    SpriteRenderer sr = sprites[i];
//                    if (sr == null) continue;
//                    float area = Mathf.Abs(sr.bounds.size.x * sr.bounds.size.y);
//                    if (area > largestArea) largestArea = area;
//                }

//                if (largestArea <= 0.0001f) return;
//                float keepThreshold = largestArea * 0.62f;
//                for (int i = 0; i < sprites.Length; i++)
//                {
//                    SpriteRenderer sr = sprites[i];
//                    if (sr == null) continue;
//                    float area = Mathf.Abs(sr.bounds.size.x * sr.bounds.size.y);
//                    if (area < keepThreshold) sr.enabled = false;
//                }
//            }
//            catch { }
//        }

//        private static TMP_Text InstallCleanSplitLabel(PassiveButton button, string text, float fontRatio)
//        {
//            if (button == null || button.gameObject == null) return null;
//            try
//            {
//                GameObject root = button.gameObject;
//                TMP_Text source = FindNewsMainLabel(root) ?? FindLargestTextLabelFallback(root);
//                if (source == null) return null;

//                Vector3 localPoint = root.transform.InverseTransformPoint(source.transform.position);
//                float sourceFontSize = Math.Max(1f, source.fontSize);
//                Color sourceColor = source.color;
//                RectTransform sourceRect = source.GetComponent<RectTransform>();
//                Vector2 sourceSize = sourceRect != null ? sourceRect.sizeDelta : Vector2.zero;

//                TMP_Text[] labels = root.GetComponentsInChildren<TMP_Text>(true);
//                if (labels != null)
//                {
//                    for (int i = 0; i < labels.Length; i++)
//                    {
//                        TMP_Text existing = labels[i];
//                        if (existing == null) continue;
//                        string value = (existing.text ?? "").Trim();
//                        bool numericBadge = value.Length > 0;
//                        for (int c = 0; c < value.Length && numericBadge; c++)
//                        {
//                            char ch = value[c];
//                            if (!char.IsDigit(ch) && ch != '+' && ch != ' ') numericBadge = false;
//                        }
//                        if (!numericBadge) existing.gameObject.SetActive(false);
//                    }
//                }

//                GameObject labelGo = UnityEngine.Object.Instantiate(source.gameObject, root.transform);
//                labelGo.name = "BanMod_SplitLabel_" + text;
//                labelGo.SetActive(true);
//                TMP_Text label = labelGo.GetComponent<TMP_Text>();
//                if (label == null) label = labelGo.GetComponentInChildren<TMP_Text>(true);
//                if (label == null) return null;

//                try { label.DestroyTranslator(); } catch { }
//                label.richText = false;
//                label.text = text;
//                label.alignment = TextAlignmentOptions.Center;
//                label.color = sourceColor;
//                label.enableAutoSizing = true;
//                label.fontSizeMax = Math.Max(1f, sourceFontSize * fontRatio);
//                label.fontSizeMin = Math.Max(1f, sourceFontSize * 0.28f);
//                label.enableWordWrapping = false;
//                label.overflowMode = TextOverflowModes.Truncate;
//                label.raycastTarget = false;

//                Transform tr = label.transform;
//                localPoint.x = 0f;
//                tr.localPosition = localPoint;
//                tr.localRotation = source.transform.localRotation;


//                Vector3 sourceScale = source.transform.localScale;
//                tr.localScale = new Vector3(sourceScale.x / 0.475f, sourceScale.y, sourceScale.z);

//                RectTransform rect = label.GetComponent<RectTransform>();
//                if (rect != null && sourceRect != null)
//                {
//                    rect.anchorMin = sourceRect.anchorMin;
//                    rect.anchorMax = sourceRect.anchorMax;
//                    rect.pivot = sourceRect.pivot;
//                    Vector2 size = sourceSize;
//                    if (Math.Abs(size.x) > 0.01f) size.x *= 0.46f;
//                    rect.sizeDelta = size;
//                }

//                return label;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogWarning("[BANMOD Community] Could not install split label '" + text + "': " + ex.Message);
//                return null;
//            }
//        }


//        private static void CenterBranchUnderRoot(Transform root, Transform leaf)
//        {
//            if (root == null || leaf == null) return;
//            try
//            {
//                Transform branch = leaf;
//                while (branch.parent != null && branch.parent != root) branch = branch.parent;
//                if (branch.parent == root)
//                {
//                    Vector3 p = branch.localPosition;
//                    p.x = 0f;
//                    branch.localPosition = p;
//                }
//            }
//            catch { }
//        }

//        private static Vector2 GetPassiveButtonSize(PassiveButton button)
//        {
//            if (button == null) return Vector2.zero;
//            try
//            {
//                var collider = button.GetComponent<BoxCollider2D>();
//                if (collider != null && collider.size.x > 0.05f && collider.size.y > 0.05f)
//                    return collider.size;
//            }
//            catch { }

//            try
//            {
//                if (button.inactiveSprites != null)
//                {
//                    var sr = button.inactiveSprites.GetComponent<SpriteRenderer>();
//                    if (sr != null && sr.size.x > 0.05f && sr.size.y > 0.05f)
//                        return sr.size;
//                }
//            }
//            catch { }

//            try
//            {
//                var sprites = button.GetComponentsInChildren<SpriteRenderer>(true);
//                SpriteRenderer best = null;
//                float area = 0f;
//                if (sprites != null)
//                {
//                    for (int i = 0; i < sprites.Length; i++)
//                    {
//                        var sr = sprites[i];
//                        if (sr == null) continue;
//                        float a = Mathf.Abs(sr.size.x * sr.size.y);
//                        if (a > area) { area = a; best = sr; }
//                    }
//                }
//                if (best != null) return best.size;
//            }
//            catch { }
//            return Vector2.zero;
//        }

//        private static void ResizePassiveButton(PassiveButton button, Vector2 stockSize, float widthRatio)
//        {
//            if (button == null) return;
//            float newWidth = stockSize.x * widthRatio;

//            try
//            {
//                BoxCollider2D[] colliders = button.GetComponentsInChildren<BoxCollider2D>(true);
//                if (colliders != null)
//                {
//                    for (int i = 0; i < colliders.Length; i++)
//                    {
//                        BoxCollider2D collider = colliders[i];
//                        if (collider == null) continue;
//                        if (collider.size.x >= stockSize.x * 0.70f)
//                            collider.size = new Vector2(newWidth, collider.size.y);
//                    }
//                }
//            }
//            catch { }

//            ResizeButtonBackgroundBranch(button.inactiveSprites, newWidth, widthRatio);
//            ResizeButtonBackgroundBranch(button.activeSprites, newWidth, widthRatio);
//        }

//        private static void ResizeButtonBackgroundBranch(GameObject branch, float newWidth, float widthRatio)
//        {
//            if (branch == null) return;
//            try
//            {
//                SpriteRenderer[] sprites = branch.GetComponentsInChildren<SpriteRenderer>(true);
//                if (sprites == null) return;
//                for (int i = 0; i < sprites.Length; i++)
//                {
//                    SpriteRenderer sr = sprites[i];
//                    if (sr == null) continue;
//                    try
//                    {
//                        if (sr.drawMode != SpriteDrawMode.Simple)
//                            sr.size = new Vector2(newWidth, sr.size.y);
//                        else
//                        {
//                            Vector3 scale = sr.transform.localScale;
//                            sr.transform.localScale = new Vector3(scale.x * widthRatio, scale.y, scale.z);
//                        }
//                    }
//                    catch { }
//                }
//            }
//            catch { }
//        }


//        private static void RepositionNumericBadge(GameObject root, float xRatio)
//        {
//            if (root == null) return;
//            try
//            {
//                TMP_Text main = FindNewsMainLabel(root);
//                var labels = root.GetComponentsInChildren<TMP_Text>(true);
//                if (labels == null) return;
//                for (int i = 0; i < labels.Length; i++)
//                {
//                    var label = labels[i];
//                    if (label == null || label == main) continue;
//                    string value = (label.text ?? "").Trim();
//                    if (value.Length == 0) continue;
//                    bool numeric = true;
//                    for (int c = 0; c < value.Length; c++)
//                    {
//                        char ch = value[c];
//                        if (!char.IsDigit(ch) && ch != '+' && ch != ' ') { numeric = false; break; }
//                    }
//                    if (!numeric) continue;
//                    Transform badge = label.transform.parent != null ? label.transform.parent : label.transform;
//                    Vector3 p = badge.localPosition;
//                    p.x *= xRatio;
//                    badge.localPosition = p;
//                }
//            }
//            catch { }
//        }

//        private static TMP_Text FindNewsMainLabel(GameObject root)
//        {
//            if (root == null) return null;
//            try
//            {
//                var labels = root.GetComponentsInChildren<TMP_Text>(true);
//                if (labels != null)
//                {
//                    string[] tokens = { "COMMUNITY", "NOVIT", "NEWS", "NOUVE", "NOVED", "NOTIC", "NEUIG", "НОВОСТ" };
//                    for (int i = 0; i < labels.Length; i++)
//                    {
//                        var label = labels[i];
//                        if (label == null) continue;
//                        string value = (label.text ?? "").Trim().ToUpperInvariant();
//                        for (int t = 0; t < tokens.Length; t++)
//                            if (value.Contains(tokens[t])) return label;
//                    }
//                }
//            }
//            catch { }
//            return FindLargestTextLabelFallback(root);
//        }

//        private static TMP_Text FindLargestTextLabelFallback(GameObject root)
//        {
//            if (root == null) return null;
//            try
//            {
//                var labels = root.GetComponentsInChildren<TMP_Text>(true);
//                TMP_Text best = null;
//                float bestScore = -1f;
//                if (labels != null)
//                {
//                    for (int i = 0; i < labels.Length; i++)
//                    {
//                        var label = labels[i];
//                        if (label == null) continue;
//                        string value = (label.text ?? "").Trim();
//                        if (value.Length == 0) continue;
//                        float score = label.fontSize + value.Length * 0.1f;
//                        if (score > bestScore)
//                        {
//                            bestScore = score;
//                            best = label;
//                        }
//                    }
//                }
//                return best;
//            }
//            catch { return null; }
//        }

//        private static void ShiftUpperMainMenuSection(MainMenuManager menu, PassiveButton newsButton, float shiftY)
//        {
//            if (menu == null || newsButton == null) return;

//            var play = FindMenuButton(menu,
//                new[] { "PLAYONLINEBUTTON", "PLAYONLINE" },
//                new[] { "GIOCA", "PLAY", "SPIELEN", "JOUER", "JUGAR" });
//            var inventory = FindMenuButton(menu,
//                new[] { "INVENTORY" },
//                new[] { "INVENTARIO", "INVENTORY", "INVENTAR", "INVENTAIRE" });
//            var shop = FindMenuButton(menu,
//                new[] { "SHOP", "STORE" },
//                new[] { "NEGOZIO", "SHOP", "STORE", "BOUTIQUE", "TIENDA" });






//            var movedRoots = new List<Transform>();
//            Transform upperRoot = FindUpperButtonsRoot(play, inventory, shop, newsButton);
//            if (upperRoot != null)
//            {
//                DisableAspectPosition(upperRoot);
//                upperRoot.localPosition += new Vector3(0f, shiftY, 0f);
//                movedRoots.Add(upperRoot);
//                Debug.Log("[BANMOD Community] Shifted upper-menu group once: " + upperRoot.gameObject.name);
//            }
//            else
//            {


//                AddIndependentMoveRoot(movedRoots, play == null ? null : play.transform);
//                AddIndependentMoveRoot(movedRoots, inventory == null ? null : inventory.transform);
//                AddIndependentMoveRoot(movedRoots, shop == null ? null : shop.transform);
//                for (int i = 0; i < movedRoots.Count; i++)
//                {
//                    DisableAspectPosition(movedRoots[i]);
//                    movedRoots[i].localPosition += new Vector3(0f, shiftY, 0f);
//                    Debug.Log("[BANMOD Community] Shifted independent upper root: " + movedRoots[i].gameObject.name);
//                }
//            }

//            var moved = new HashSet<int>();
//            var logo = FindMainMenuLogo(menu, play);
//            if (!IsInsideAnyRoot(logo, movedRoots))
//                MoveTransformOnce(logo, shiftY, moved, "Among Us logo");
//            TryMoveDivider(menu, newsButton, shop, moved, shiftY);
//        }

//        private static Transform FindUpperButtonsRoot(PassiveButton play, PassiveButton inventory, PassiveButton shop, PassiveButton news)
//        {
//            Transform[] items = { play == null ? null : play.transform, inventory == null ? null : inventory.transform, shop == null ? null : shop.transform };
//            Transform first = null;
//            for (int i = 0; i < items.Length; i++) if (items[i] != null) { first = items[i]; break; }
//            if (first == null) return null;

//            Transform candidate = first;
//            while (candidate != null)
//            {
//                bool containsAll = true;
//                for (int i = 0; i < items.Length; i++)
//                {
//                    if (items[i] == null) continue;
//                    if (items[i] != candidate && !items[i].IsChildOf(candidate)) { containsAll = false; break; }
//                }
//                bool containsNews = news != null && news.transform != null &&
//                    (news.transform == candidate || news.transform.IsChildOf(candidate));
//                if (containsAll && !containsNews) return candidate;
//                candidate = candidate.parent;
//            }



//            Transform common = CommonAncestor(items);
//            if (common == null) return null;
//            Transform branch = DirectChildUnder(common, first);
//            if (branch == null) return null;
//            for (int i = 0; i < items.Length; i++)
//            {
//                if (items[i] == null) continue;
//                Transform other = DirectChildUnder(common, items[i]);
//                if (other != branch) return null;
//            }
//            if (news != null && news.transform != null &&
//                (news.transform == branch || news.transform.IsChildOf(branch))) return null;
//            return branch;
//        }

//        private static Transform CommonAncestor(Transform[] items)
//        {
//            Transform first = null;
//            for (int i = 0; i < items.Length; i++) if (items[i] != null) { first = items[i]; break; }
//            if (first == null) return null;
//            Transform c = first;
//            while (c != null)
//            {
//                bool all = true;
//                for (int i = 0; i < items.Length; i++)
//                {
//                    if (items[i] == null) continue;
//                    if (items[i] != c && !items[i].IsChildOf(c)) { all = false; break; }
//                }
//                if (all) return c;
//                c = c.parent;
//            }
//            return null;
//        }

//        private static Transform DirectChildUnder(Transform ancestor, Transform item)
//        {
//            if (ancestor == null || item == null || item == ancestor) return item;
//            Transform current = item;
//            while (current.parent != null && current.parent != ancestor) current = current.parent;
//            return current.parent == ancestor ? current : null;
//        }

//        private static bool IsInsideAnyRoot(Transform candidate, List<Transform> roots)
//        {
//            if (candidate == null || roots == null) return false;
//            for (int i = 0; i < roots.Count; i++)
//            {
//                var root = roots[i];
//                if (root != null && (candidate == root || candidate.IsChildOf(root))) return true;
//            }
//            return false;
//        }

//        private static void AddIndependentMoveRoot(List<Transform> roots, Transform candidate)
//        {
//            if (candidate == null) return;


//            for (int i = 0; i < roots.Count; i++)
//                if (candidate == roots[i] || candidate.IsChildOf(roots[i])) return;

//            for (int i = roots.Count - 1; i >= 0; i--)
//                if (roots[i].IsChildOf(candidate)) roots.RemoveAt(i);
//            roots.Add(candidate);
//        }

//        private static float GetStockLowerButtonStep(MainMenuManager menu, PassiveButton news)
//        {
//            float fallback = CommunityButtonYOffsetFallback;
//            if (menu == null || news == null) return fallback;
//            try
//            {
//                var account = FindMenuButton(menu,
//                    new[] { "ACCOUNT" },
//                    new[] { "IL MIO ACCOUNT", "MY ACCOUNT", "ACCOUNT", "KONTO", "COMPTE", "CUENTA" });
//                if (account != null && account.transform != null && news.transform != null &&
//                    account.transform.parent == news.transform.parent)
//                {
//                    float step = Math.Abs(news.transform.localPosition.y - account.transform.localPosition.y);
//                    if (step >= 0.30f && step <= 0.90f)
//                    {
//                        Debug.Log("[BANMOD Community] Using stock News/Account spacing: " + step.ToString("0.000"));
//                        return step;
//                    }
//                }
//            }
//            catch { }
//            Debug.Log("[BANMOD Community] Using fallback Community spacing: " + fallback.ToString("0.000"));
//            return fallback;
//        }

//        private static void DisableAspectPosition(Transform transform)
//        {
//            if (transform == null) return;
//            try
//            {
//                var aspect = transform.GetComponent<AspectPosition>();
//                if (aspect != null)
//                {
//                    aspect.enabled = false;
//                    UnityEngine.Object.Destroy(aspect);
//                }
//            }
//            catch { }
//        }

//        private static void DisableAspectPositionsUnder(Transform root)
//        {
//            if (root == null) return;
//            try
//            {
//                var aspects = root.GetComponentsInChildren<AspectPosition>(true);
//                if (aspects == null) return;
//                for (int i = 0; i < aspects.Length; i++)
//                {
//                    var aspect = aspects[i];
//                    if (aspect == null) continue;
//                    aspect.enabled = false;
//                    UnityEngine.Object.Destroy(aspect);
//                }
//            }
//            catch { DisableAspectPosition(root); }
//        }

//        private static void MoveMainButtonOnce(PassiveButton button, float deltaY, HashSet<int> moved, string label)
//        {
//            if (button == null || button.transform == null) return;
//            DisableAspectPosition(button.transform);
//            MoveTransformOnce(button.transform, deltaY, moved, label);
//        }

//        private static void MoveTransformOnce(Transform transform, float deltaY, HashSet<int> moved, string label)
//        {
//            if (transform == null) return;
//            try
//            {
//                int id = transform.gameObject.GetInstanceID();
//                if (moved.Contains(id)) return;
//                transform.localPosition += new Vector3(0f, deltaY, 0f);
//                moved.Add(id);
//                Debug.Log("[BANMOD Community] Shifted " + label + " upward: " + transform.gameObject.name);
//            }
//            catch { }
//        }

//        private static Transform FindMainMenuLogo(MainMenuManager menu, PassiveButton play)
//        {
//            if (menu == null) return null;



//            Transform named = FindLikelyAmongUsLogoByName(menu.transform);
//            if (named != null) return named;



//            try
//            {
//                if (play != null)
//                {
//                    var menuSprites = menu.GetComponentsInChildren<SpriteRenderer>(true);
//                    SpriteRenderer bestLocal = null;
//                    float bestLocalScore = 0f;
//                    if (menuSprites != null)
//                    {
//                        for (int i = 0; i < menuSprites.Length; i++)
//                        {
//                            var sprite = menuSprites[i];
//                            if (sprite == null || sprite.gameObject == null) continue;
//                            if (sprite.transform.position.y <= play.transform.position.y + 0.25f) continue;
//                            float h = Math.Max(0.001f, sprite.bounds.size.y);
//                            float ratio = sprite.bounds.size.x / h;
//                            if (ratio < 1.4f) continue;
//                            float score = sprite.bounds.size.x * sprite.bounds.size.y * ratio;
//                            if (score > bestLocalScore)
//                            {
//                                bestLocalScore = score;
//                                bestLocal = sprite;
//                            }
//                        }
//                    }
//                    if (bestLocal != null)
//                    {
//                        Transform root = bestLocal.transform;
//                        while (root.parent != null && root.parent != menu.transform)
//                            root = root.parent;
//                        return root;
//                    }
//                }
//            }
//            catch { }


//            try
//            {
//                var sprites = Resources.FindObjectsOfTypeAll(Il2CppType.Of<SpriteRenderer>());
//                SpriteRenderer best = null;
//                float bestArea = 0f;
//                float playY = play == null ? float.MinValue : play.transform.position.y;
//                if (sprites != null)
//                {
//                    for (int i = 0; i < sprites.Length; i++)
//                    {
//                        var sprite = sprites[i] as SpriteRenderer;
//                        if (sprite == null || sprite.gameObject == null || !sprite.gameObject.activeInHierarchy) continue;
//                        string n = (sprite.gameObject.name ?? "").ToUpperInvariant();
//                        if (!n.Contains("LOGO") || n.Contains("LOGOUT")) continue;
//                        if (sprite.transform.position.y < playY) continue;
//                        float area = sprite.bounds.size.x * sprite.bounds.size.y;
//                        if (area > bestArea)
//                        {
//                            bestArea = area;
//                            best = sprite;
//                        }
//                    }
//                }
//                if (best != null) return best.transform;
//            }
//            catch { }

//            return null;
//        }

//        private static Transform FindLikelyAmongUsLogoByName(Transform root)
//        {
//            if (root == null) return null;
//            try
//            {
//                string own = (root.gameObject.name ?? "").ToUpperInvariant();
//                bool logoLike = own.Contains("LOGO") || own.Contains("TITLE");
//                bool rejected = own.Contains("DISCORD") || own.Contains("GITHUB") || own.Contains("STEAM") ||
//                                own.Contains("SOCIAL") || own.Contains("BANMOD") || own.Contains("LOGIN") ||
//                                own.Contains("LOGOUT");
//                if (logoLike && !rejected) return root;
//                for (int i = 0; i < root.childCount; i++)
//                {
//                    var found = FindLikelyAmongUsLogoByName(root.GetChild(i));
//                    if (found != null) return found;
//                }
//            }
//            catch { }
//            return null;
//        }

//        private static Transform FindTransformNameContains(Transform root, string token)
//        {
//            if (root == null) return null;
//            try
//            {
//                string own = (root.gameObject.name ?? "").ToUpperInvariant();
//                if (own.Contains(token)) return root;
//                for (int i = 0; i < root.childCount; i++)
//                {
//                    var result = FindTransformNameContains(root.GetChild(i), token);
//                    if (result != null) return result;
//                }
//            }
//            catch { }
//            return null;
//        }

//        private static void TryMoveDivider(MainMenuManager menu, PassiveButton news, PassiveButton shop, HashSet<int> moved, float shiftY)
//        {
//            if (menu == null || news == null) return;
//            try
//            {
//                float newsY = news.transform.position.y;
//                float shopY = shop == null ? newsY + 3f : shop.transform.position.y;
//                var renderers = menu.GetComponentsInChildren<SpriteRenderer>(true);
//                if (renderers == null) return;

//                SpriteRenderer best = null;
//                float bestRatio = 0f;
//                for (int i = 0; i < renderers.Length; i++)
//                {
//                    var sr = renderers[i];
//                    if (sr == null || sr.gameObject == null) continue;
//                    float y = sr.transform.position.y;
//                    if (y <= newsY || y >= shopY) continue;
//                    float h = Math.Max(0.001f, sr.bounds.size.y);
//                    float ratio = sr.bounds.size.x / h;
//                    if (ratio > 8f && ratio > bestRatio)
//                    {
//                        bestRatio = ratio;
//                        best = sr;
//                    }
//                }

//                if (best != null)
//                    MoveTransformOnce(best.transform, shiftY, moved, "separator");
//            }
//            catch { }
//        }

//        private static void RemoveCopiedNewsBadge(GameObject clone, TMP_Text mainLabel)
//        {
//            if (clone == null) return;
//            try
//            {
//                var labels = clone.GetComponentsInChildren<TMP_Text>(true);
//                if (labels == null) return;
//                for (int i = 0; i < labels.Length; i++)
//                {
//                    var label = labels[i];
//                    if (label == null || label == mainLabel) continue;
//                    string value = (label.text ?? "").Trim();
//                    bool numericBadge = value.Length > 0;
//                    for (int c = 0; c < value.Length && numericBadge; c++)
//                    {
//                        char ch = value[c];
//                        if (!char.IsDigit(ch) && ch != '+' && ch != ' ') numericBadge = false;
//                    }
//                    if (!numericBadge) continue;

//                    GameObject badgeRoot = label.gameObject;
//                    if (label.transform.parent != null && label.transform.parent.gameObject != clone)
//                        badgeRoot = label.transform.parent.gameObject;
//                    if (badgeRoot != null) badgeRoot.SetActive(false);
//                }
//            }
//            catch { }
//        }

//        private static void CreateCommunityUnreadDot(GameObject buttonRoot, TMP_Text sourceLabel)
//        {
//            if (buttonRoot == null || sourceLabel == null) return;
//            try
//            {
//                var dotObject = UnityEngine.Object.Instantiate(sourceLabel.gameObject, buttonRoot.transform);
//                dotObject.name = "BanMod_CommunityUnreadDot";
//                var dot = dotObject.GetComponent<TMP_Text>();
//                if (dot == null)
//                {
//                    UnityEngine.Object.Destroy(dotObject);
//                    return;
//                }

//                dot.richText = false;
//                dot.text = "●";
//                dot.color = new Color(0.96f, 0.06f, 0.06f, 1f);
//                dot.alignment = TextAlignmentOptions.Center;
//                dot.fontSize = Math.Max(18f, sourceLabel.fontSize * 0.58f);



//                Vector3 localPos = new Vector3(2.35f, 0.28f, sourceLabel.transform.localPosition.z - 0.1f);
//                var sprites = buttonRoot.GetComponentsInChildren<SpriteRenderer>(true);
//                SpriteRenderer largest = null;
//                float largestArea = 0f;
//                if (sprites != null)
//                {
//                    for (int i = 0; i < sprites.Length; i++)
//                    {
//                        var sr = sprites[i];
//                        if (sr == null) continue;
//                        float area = sr.bounds.size.x * sr.bounds.size.y;
//                        if (area > largestArea)
//                        {
//                            largestArea = area;
//                            largest = sr;
//                        }
//                    }
//                }
//                if (largest != null)
//                {
//                    Vector3 corner = new Vector3(largest.bounds.max.x, largest.bounds.max.y, largest.bounds.center.z);
//                    Vector3 converted = buttonRoot.transform.InverseTransformPoint(corner);
//                    localPos.x = converted.x - 0.18f;
//                    localPos.y = converted.y - 0.12f;
//                }

//                dotObject.transform.localPosition = localPos;
//                dotObject.transform.localRotation = Quaternion.identity;
//                dotObject.transform.localScale = sourceLabel.transform.localScale * 0.60f;
//                dotObject.SetActive(false);
//                _communityUnreadDot = dotObject;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogWarning("[BANMOD Community] Could not create unread dot: " + ex.Message);
//            }
//        }

//        private static void EnforceCommunityButtonVisuals()
//        {
//            if (_mainMenuButton == null) return;
//            try
//            {
//                TMP_Text mainLabel = null;
//                TMP_Text[] labels = _mainMenuButton.GetComponentsInChildren<TMP_Text>(true);
//                if (labels != null)
//                {
//                    for (int i = 0; i < labels.Length; i++)
//                    {
//                        TMP_Text label = labels[i];
//                        if (label == null) continue;

//                        if (string.Equals(label.gameObject.name, "BanMod_SplitLabel_COMMUNITY", StringComparison.Ordinal))
//                        {
//                            mainLabel = label;
//                            continue;
//                        }

//                        if (string.Equals(label.gameObject.name, "BanMod_CommunityUnreadDot", StringComparison.Ordinal))
//                            continue;

//                        string value = (label.text ?? "").Trim();
//                        bool numericBadge = value.Length > 0;
//                        for (int c = 0; c < value.Length && numericBadge; c++)
//                        {
//                            char ch = value[c];
//                            if (!char.IsDigit(ch) && ch != '+' && ch != ' ') numericBadge = false;
//                        }



//                        if (!numericBadge) label.gameObject.SetActive(false);
//                    }
//                }

//                if (mainLabel != null)
//                {
//                    try { mainLabel.DestroyTranslator(); } catch { }
//                    mainLabel.richText = false;
//                    mainLabel.text = "COMMUNITY";
//                    mainLabel.alignment = TextAlignmentOptions.Center;
//                    mainLabel.gameObject.SetActive(true);
//                }

//                RemoveCopiedNewsBadge(_mainMenuButton, mainLabel);
//            }
//            catch { }
//        }


//        private static IEnumerator CommunityUnreadPollingLoop(MainMenuManager menu)
//        {
//            yield return new WaitForSeconds(1f);
//            while (menu != null && _mainMenuButton != null)
//            {
//                EnforceCommunityButtonVisuals();
//                yield return CheckCommunityUnread();
//                yield return new WaitForSeconds(20f);
//            }
//        }

//        private static IEnumerator CheckCommunityUnread()
//        {
//            bool ok = false;
//            string body = "";
//            yield return SendRequest("GET", CommunityUrl("/activity"), null, delegate (bool success, string response)
//            {
//                ok = success;
//                body = response ?? "";
//            });
//            if (!ok) yield break;
//            try
//            {
//                using (var doc = JsonDocument.Parse(body))
//                {
//                    var root = doc.RootElement;
//                    int postId = root.TryGetProperty("latest_post_id", out var p) ? p.GetInt32() : 0;
//                    int replyId = root.TryGetProperty("latest_reply_id", out var r) ? r.GetInt32() : 0;
//                    int chatId = root.TryGetProperty("latest_chat_id", out var c) ? c.GetInt32() : 0;
//                    _latestKnownCommunityPostId = Math.Max(_latestKnownCommunityPostId, postId);
//                    _latestKnownCommunityReplyId = Math.Max(_latestKnownCommunityReplyId, replyId);
//                    _latestKnownCommunityChatId = Math.Max(_latestKnownCommunityChatId, chatId);

//                    if (!_communityMentionInitialSyncDone)
//                    {
//                        _communityMentionInitialSyncDone = true;
//                        _communityMentionLastMessageId = chatId;
//                    }

//                    int seenPost = PlayerPrefs.GetInt(CommunityLastSeenPostKey, 0);
//                    int seenReply = PlayerPrefs.GetInt(CommunityLastSeenReplyKey, 0);
//                    int seenChat = PlayerPrefs.GetInt(CommunityLastSeenChatKey, 0);
//                    SetCommunityUnreadVisible(postId > seenPost || replyId > seenReply || chatId > seenChat);
//                }
//            }
//            catch { }
//        }

//        private static void MarkCommunityActivitySeen()
//        {
//            try
//            {
//                if (_latestKnownCommunityPostId > PlayerPrefs.GetInt(CommunityLastSeenPostKey, 0))
//                    PlayerPrefs.SetInt(CommunityLastSeenPostKey, _latestKnownCommunityPostId);
//                if (_latestKnownCommunityReplyId > PlayerPrefs.GetInt(CommunityLastSeenReplyKey, 0))
//                    PlayerPrefs.SetInt(CommunityLastSeenReplyKey, _latestKnownCommunityReplyId);
//                if (_latestKnownCommunityChatId > PlayerPrefs.GetInt(CommunityLastSeenChatKey, 0))
//                    PlayerPrefs.SetInt(CommunityLastSeenChatKey, _latestKnownCommunityChatId);
//                PlayerPrefs.Save();
//            }
//            catch { }
//            SetCommunityUnreadVisible(false);
//        }

//        private static void SetCommunityUnreadVisible(bool visible)
//        {
//            _publicCommunityUnread = visible;
//            RefreshCombinedUnread();
//        }

//        private static void RefreshCombinedUnread()
//        {
//            bool visible = _publicCommunityUnread || PrivateUnreadMessageIds.Count > 0 || _bugReportUnreadCount > 0;
//            try { if (_communityUnreadDot != null) _communityUnreadDot.SetActive(visible); } catch { }
//            try { if (_launcherUnreadDot != null) _launcherUnreadDot.gameObject.SetActive(visible); } catch { }
//            RefreshPrivateChatUnreadVisuals();
//            RefreshBugUnreadVisuals();
//        }

//        private static void RefreshPrivateChatUnreadVisuals()
//        {
//            bool visible = PrivateUnreadMessageIds.Count > 0;
//            try
//            {
//                if (_privateChatTabUnreadBadgeText != null)
//                {
//                    _privateChatTabUnreadBadgeText.text = visible ? "●" : "";
//                    _privateChatTabUnreadBadgeText.gameObject.SetActive(visible);
//                }
//            }
//            catch { }
//        }
//        private static void DumpMainMenuButtons(MainMenuManager menu)
//        {
//            if (menu == null) return;
//            try
//            {
//                var buttons = menu.GetComponentsInChildren<PassiveButton>(true);
//                if (buttons == null) return;
//                for (int i = 0; i < buttons.Length; i++)
//                {
//                    var button = buttons[i];
//                    if (button == null || button.gameObject == null) continue;
//                    string text = "";
//                    var labels = button.GetComponentsInChildren<TMP_Text>(true);
//                    if (labels != null)
//                    {
//                        for (int j = 0; j < labels.Length; j++)
//                        {
//                            if (labels[j] == null || string.IsNullOrWhiteSpace(labels[j].text)) continue;
//                            if (text.Length > 0) text += " | ";
//                            text += labels[j].text.Replace("\n", " ").Replace("\r", " ");
//                        }
//                    }
//                    Debug.Log("[BANMOD Community] MainMenu PassiveButton: " + button.gameObject.name + " text=[" + text + "]");
//                }
//            }
//            catch { }
//        }


//        internal static void ForceClose()
//        {
//            _chatPolling = false;
//            _showingChat = false;
//            _showingPrivateChat = false;
//            _showingBugReport = false;

//            if (_composePanel != null) _composePanel.SetActive(false);
//            if (_managePanel != null) _managePanel.SetActive(false);
//            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);
//            if (_chatPanel != null) _chatPanel.SetActive(false);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
//            if (_listPanel != null) _listPanel.SetActive(false);
//            if (_detailPanel != null) _detailPanel.SetActive(false);

//            if (_root != null) _root.SetActive(false);

//            BlockMainMenuInput(false);
//            RefreshLauncherVisibility();
//        }

//        private static void BlockMainMenuInput(bool block)
//        {
//            try
//            {
//                if (block)
//                {
//                    if (_mainMenuInputBlocked)
//                        return;

//                    _mainMenuInputBlocked = true;
//                    BlockedMainMenuColliders.Clear();
//                    BlockedMainMenuColliderStates.Clear();
//                    BlockedMainMenuButtons.Clear();
//                    BlockedMainMenuButtonStates.Clear();
//                    if (_mainMenuOwner == null)
//                    {
//                        _mainMenuInputBlocked = false;
//                        return;
//                    }

//                    var buttons = _mainMenuOwner.GetComponentsInChildren<PassiveButton>(true);
//                    if (buttons != null)
//                    {
//                        for (int i = 0; i < buttons.Length; i++)
//                        {
//                            var b = buttons[i];
//                            if (b == null) continue;
//                            BlockedMainMenuButtons.Add(b);
//                            BlockedMainMenuButtonStates.Add(b.enabled);
//                            b.enabled = false;
//                        }
//                    }

//                    var colliders = _mainMenuOwner.GetComponentsInChildren<BoxCollider2D>(true);
//                    if (colliders != null)
//                    {
//                        for (int i = 0; i < colliders.Length; i++)
//                        {
//                            var c = colliders[i];
//                            if (c == null) continue;
//                            BlockedMainMenuColliders.Add(c);
//                            BlockedMainMenuColliderStates.Add(c.enabled);
//                            c.enabled = false;
//                        }
//                    }
//                }
//                else
//                {
//                    for (int i = 0; i < BlockedMainMenuButtons.Count; i++)
//                    {
//                        var b = BlockedMainMenuButtons[i];
//                        if (b != null && i < BlockedMainMenuButtonStates.Count)
//                            b.enabled = BlockedMainMenuButtonStates[i];
//                    }
//                    for (int i = 0; i < BlockedMainMenuColliders.Count; i++)
//                    {
//                        var c = BlockedMainMenuColliders[i];
//                        if (c != null && i < BlockedMainMenuColliderStates.Count)
//                            c.enabled = BlockedMainMenuColliderStates[i];
//                    }
//                    BlockedMainMenuButtons.Clear();
//                    BlockedMainMenuButtonStates.Clear();
//                    BlockedMainMenuColliders.Clear();
//                    BlockedMainMenuColliderStates.Clear();
//                    _mainMenuInputBlocked = false;
//                }
//            }
//            catch
//            {
//                if (!block)
//                    _mainMenuInputBlocked = false;
//            }
//        }

//        private static void EnsureStandaloneUi()
//        {
//            if (_root != null) return;

//            EnsureEventSystem();

//            _root = new GameObject(RootName);
//            UnityEngine.Object.DontDestroyOnLoad(_root);
//            Canvas canvas = _root.AddComponent<Canvas>();
//            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//            canvas.overrideSorting = true;
//            canvas.sortingOrder = 30000;
//            CanvasScaler scaler = _root.AddComponent<CanvasScaler>();
//            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//            scaler.referenceResolution = new Vector2(1920f, 1080f);
//            _root.AddComponent<GraphicRaycaster>();

//            CreateStandaloneInputBlocker();

//            _window = CreatePanel(_root.transform, "CommunityWindow", Vector2.zero, new Vector2(1460f, 920f), BanModUiStyles.WindowColor);
//            CreateLabel(_window.transform, "Title", "BANMOD COMMUNITY", 34, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(0f, 418f), new Vector2(980f, 48f));
//            CreateLabel(_window.transform, "Subtitle", "Private messages, mod help, bug reports, and suggestions in one place.", 15, TextAlignmentOptions.Center, new Color(0.82f, 0.84f, 0.90f, 1f), new Vector2(0f, 382f), new Vector2(960f, 28f));
//            CreateButton(_window.transform, "Close", "CLOSE", new Vector2(635f, 418f), new Vector2(150f, 46f), BanModUiStyles.DangerColor, ForceClose);



//            _tabPrivateChatButton = CreateButton(_window.transform, "TabPrivateChat", "PRIVATE CHAT", new Vector2(-465f, 318f), new Vector2(250f, 52f), new Color(0.22f, 0.25f, 0.48f, 1f), ShowPrivateChatTab);
//            _tabChatButton = CreateButton(_window.transform, "TabCommunity", "COMMUNITY", new Vector2(-155f, 318f), new Vector2(250f, 52f), new Color(0.12f, 0.36f, 0.58f, 1f), ShowChatTab);
//            _tabBugReportButton = CreateButton(_window.transform, "TabBugReport", "BUG REPORT", new Vector2(155f, 318f), new Vector2(250f, 52f), new Color(0.46f, 0.28f, 0.10f, 1f), ShowBugReportTab);
//            _tabSuggestionsButton = CreateButton(_window.transform, "TabSuggestions", "SUGGESTIONS", new Vector2(465f, 318f), new Vector2(250f, 52f), new Color(0.18f, 0.42f, 0.18f, 1f), ShowSuggestionsTab);
//            _privateChatTabUnreadBadgeText = CreateLabel(_window.transform, "PrivateChatTabUnread", "●", 18, TextAlignmentOptions.Center, new Color(1f, 0.12f, 0.12f, 1f), new Vector2(-360f, 336f), new Vector2(26f, 26f));
//            if (_privateChatTabUnreadBadgeText != null) _privateChatTabUnreadBadgeText.gameObject.SetActive(false);
//            _bugTabUnreadBadgeText = CreateLabel(_window.transform, "BugTabUnread", "●", 18, TextAlignmentOptions.Center, new Color(1f, 0.12f, 0.12f, 1f), new Vector2(260f, 336f), new Vector2(26f, 26f));
//            if (_bugTabUnreadBadgeText != null) _bugTabUnreadBadgeText.gameObject.SetActive(false);

//            _profileHeaderText = CreateLabel(_window.transform, "ProfileHeader", "", 15, TextAlignmentOptions.Right, new Color(0.78f, 0.82f, 0.92f, 1f), new Vector2(480f, 370f), new Vector2(300f, 30f));
//            _profileMenuButton = CreateButton(_window.transform, "ProfileMenuButton", "...", new Vector2(660f, 370f), new Vector2(62f, 38f), new Color(0.18f, 0.20f, 0.28f, 1f), ToggleProfileMenu);

//            _statusText = CreateLabel(_window.transform, "Status", "", 14, TextAlignmentOptions.Center, new Color(0.78f, 0.82f, 0.92f, 1f), new Vector2(0f, -431f), new Vector2(1180f, 26f));

//            BuildChatPanel();
//            BuildPrivateChatPanel();
//            BuildBugReportPanel();
//            BuildListPanel();
//            BuildDetailPanel();
//            BuildComposePanel();
//            BuildManagePanel();
//            BuildProfileMenu();

//            _showingChat = true;
//            _showingPrivateChat = false;
//            _showingBugReport = false;
//            if (_chatPanel != null) _chatPanel.SetActive(true);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
//            if (_listPanel != null) _listPanel.SetActive(false);
//            if (_detailPanel != null) _detailPanel.SetActive(false);
//            if (_composePanel != null) _composePanel.SetActive(false);
//            if (_managePanel != null) _managePanel.SetActive(false);
//            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);

//            _root.SetActive(false);
//            EnsureLauncherUi();
//            RefreshLauncherVisibility();
//            RefreshBugUnreadVisuals();
//        }


//        private static void EnsureEventSystem()
//        {



//        }


//        private static void CreateStandaloneInputBlocker()
//        {
//            GameObject blocker = new GameObject("InputBlocker");
//            blocker.transform.SetParent(_root.transform, false);
//            RectTransform rect = blocker.AddComponent<RectTransform>();
//            rect.anchorMin = Vector2.zero;
//            rect.anchorMax = Vector2.one;
//            rect.pivot = new Vector2(0.5f, 0.5f);
//            rect.offsetMin = Vector2.zero;
//            rect.offsetMax = Vector2.zero;
//            Image image = blocker.AddComponent<Image>();
//            image.color = new Color(0f, 0f, 0f, 0.70f);
//            image.raycastTarget = true;
//            Button button = blocker.AddComponent<Button>();
//            button.targetGraphic = image;
//            button.transition = Selectable.Transition.None;
//            button.onClick.AddListener((UnityAction)delegate { });
//            CanvasGroup group = blocker.AddComponent<CanvasGroup>();
//            group.alpha = 1f;
//            group.interactable = true;
//            group.blocksRaycasts = true;
//            blocker.transform.SetAsFirstSibling();
//        }


//        private static void BuildProfileMenu()
//        {
//            _profileMenuPanel = CreatePanel(_window.transform, "CommunityNameMenu", new Vector2(380f, 125f), new Vector2(610f, 290f), new Color(0.07f, 0.08f, 0.11f, 0.995f));
//            CreateLabel(_profileMenuPanel.transform, "NameMenuTitle", "COMMUNITY NAME", 24, TextAlignmentOptions.Left, Color.white, new Vector2(-145f, 105f), new Vector2(270f, 36f));
//            CreateButton(_profileMenuPanel.transform, "NameMenuClose", "×", new Vector2(270f, 108f), new Vector2(48f, 40f), new Color(0.28f, 0.28f, 0.34f, 1f), CloseProfileMenu);
//            _profileNameText = CreateLabel(_profileMenuPanel.transform, "ProfileNameLabel", "", 14, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, 57f), new Vector2(540f, 34f));
//            _profileInput = CreateInput(_profileMenuPanel.transform, "ProfileNameInput", "Choose / request a name...", false, new Vector2(-55f, 5f), new Vector2(390f, 48f), 20);
//            _profileActionButton = CreateButton(_profileMenuPanel.transform, "ProfileSave", "SET NAME", new Vector2(205f, 5f), new Vector2(130f, 48f), new Color(0.18f, 0.42f, 0.18f, 1f), SaveProfileName);
//            CreateLabel(_profileMenuPanel.transform, "ProfileRule", "After the first name is set, changes require admin approval.", 13, TextAlignmentOptions.Left, new Color(0.62f, 0.67f, 0.78f, 1f), new Vector2(0f, -48f), new Vector2(540f, 34f));
//            CreateLabel(_profileMenuPanel.transform, "ProfileLoginDefault", "Default: your BanMod Login username.", 13, TextAlignmentOptions.Left, new Color(0.62f, 0.67f, 0.78f, 1f), new Vector2(0f, -78f), new Vector2(540f, 30f));
//            _profileMenuPanel.SetActive(false);
//        }

//        private static void ToggleProfileMenu()
//        {
//            if (_profileMenuPanel == null) return;
//            bool show = !_profileMenuPanel.activeSelf;
//            _profileMenuPanel.SetActive(show);
//            if (show)
//            {
//                _profileMenuPanel.transform.SetAsLastSibling();
//                FetchProfile();
//            }
//        }

//        private static void CloseProfileMenu()
//        {
//            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);
//        }

//        private static void BuildBugReportPanel()
//        {
//            _bugReportPanel = CreatePanel(_window.transform, "BugReportPanel", new Vector2(0f, -55f), new Vector2(1320f, 660f), BanModUiStyles.PanelColor);

//            GameObject formPanel = CreatePanel(_bugReportPanel.transform, "NewBugPanel", new Vector2(-370f, 0f), new Vector2(540f, 610f), new Color(0.075f, 0.085f, 0.12f, 1f));
//            CreateLabel(formPanel.transform, "BugFormHeading", "NEW BUG REPORT", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-105f, 264f), new Vector2(280f, 36f));
//            CreateLabel(formPanel.transform, "BugPrivateInfo", "Visible only to you and BanMod administration.", 12, TextAlignmentOptions.Left, new Color(0.60f, 0.66f, 0.78f, 1f), new Vector2(0f, 235f), new Vector2(480f, 26f));

//            CreateLabel(formPanel.transform, "BugTitleLabel", "TITLE", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, 196f), new Vector2(480f, 24f));
//            _bugTitleInput = CreateInput(formPanel.transform, "BugTitleInput", "Short bug title...", false, new Vector2(0f, 162f), new Vector2(480f, 46f), 160);

//            CreateLabel(formPanel.transform, "BugGameModeLabel", "GAME MODE", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, 119f), new Vector2(480f, 24f));
//            _bugGameModeInput = CreateInput(formPanel.transform, "BugGameModeInput", "Classic, Hide N Seek, Custom...", false, new Vector2(0f, 85f), new Vector2(480f, 46f), 100);

//            CreateLabel(formPanel.transform, "BugCustomLabel", "CUSTOM ROLES?", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(-118f, 39f), new Vector2(245f, 24f));
//            _bugCustomYesButton = CreateButton(formPanel.transform, "BugCustomYes", "YES", new Vector2(100f, 39f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugCustomRoles(true); });
//            _bugCustomNoButton = CreateButton(formPanel.transform, "BugCustomNo", "NO ✓", new Vector2(202f, 39f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugCustomRoles(false); });
//            _bugCustomRolesWhichInput = CreateInput(formPanel.transform, "BugCustomWhich", "Which custom roles?", false, new Vector2(0f, -3f), new Vector2(480f, 42f), 180);
//            _bugCustomRolesWhichInput.gameObject.SetActive(false);

//            CreateLabel(formPanel.transform, "BugModsLabel", "OTHER MODS?", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(-118f, -50f), new Vector2(245f, 24f));
//            _bugOtherYesButton = CreateButton(formPanel.transform, "BugModsYes", "YES", new Vector2(100f, -50f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugOtherMods(true); });
//            _bugOtherNoButton = CreateButton(formPanel.transform, "BugModsNo", "NO ✓", new Vector2(202f, -50f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugOtherMods(false); });
//            _bugOtherModsWhichInput = CreateInput(formPanel.transform, "BugModsWhich", "Which other mods?", false, new Vector2(0f, -92f), new Vector2(480f, 42f), 180);
//            _bugOtherModsWhichInput.gameObject.SetActive(false);

//            CreateLabel(formPanel.transform, "BugDescLabel", "DESCRIPTION", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, -139f), new Vector2(480f, 24f));
//            _bugDescriptionInput = CreateInput(formPanel.transform, "BugDescriptionInput", "Describe what happened and how to reproduce it...", true, new Vector2(0f, -205f), new Vector2(480f, 100f), 4000);
//            _bugSendButton = CreateButton(formPanel.transform, "BugSend", "SEND BUG REPORT", new Vector2(0f, -270f), new Vector2(250f, 48f), new Color(0.14f, 0.44f, 0.25f, 1f), SubmitBugReport);

//            GameObject reportsPanel = CreatePanel(_bugReportPanel.transform, "BugReportsPanel", new Vector2(310f, 0f), new Vector2(740f, 610f), new Color(0.075f, 0.085f, 0.12f, 1f));
//            CreateLabel(reportsPanel.transform, "BugReportsHeading", "MY BUG REPORTS", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-170f, 264f), new Vector2(320f, 36f));
//            CreateButton(reportsPanel.transform, "BugRefresh", "REFRESH", new Vector2(285f, 264f), new Vector2(130f, 42f), BanModUiStyles.ButtonColor, LoadBugReports);

//            CreateBugReportsList(reportsPanel.transform, new Vector2(0f, 157f), new Vector2(680f, 155f));
//            _bugSelectedHeader = CreateLabel(reportsPanel.transform, "BugSelectedHeader", "Select a bug report to view the private conversation.", 14, TextAlignmentOptions.Left, new Color(0.70f, 0.75f, 0.86f, 1f), new Vector2(0f, 56f), new Vector2(680f, 34f));
//            _bugConversationText = CreateScrollableText(reportsPanel.transform, "BugConversation", new Vector2(0f, -72f), new Vector2(680f, 220f), out _bugConversationScroll, out _bugConversationContent);
//            _bugReplyInput = CreateInput(reportsPanel.transform, "BugReplyInput", "Message BanMod administration...", false, new Vector2(-80f, -222f), new Vector2(500f, 50f), 2500);
//            _bugReplyButton = CreateButton(reportsPanel.transform, "BugReplySend", "SEND", new Vector2(245f, -222f), new Vector2(130f, 50f), new Color(0.14f, 0.44f, 0.25f, 1f), SendBugReportReply);
//            _bugCloseButton = CreateButton(reportsPanel.transform, "BugClose", "CLOSE RESOLVED", new Vector2(-150f, -274f), new Vector2(210f, 42f), new Color(0.45f, 0.32f, 0.08f, 1f), CloseSelectedBugReport);
//            _bugDeleteButton = CreateButton(reportsPanel.transform, "BugDelete", "DELETE", new Vector2(95f, -274f), new Vector2(150f, 42f), BanModUiStyles.DangerColor, DeleteSelectedBugReport);

//            UpdateBugReportButtons();
//        }

//        private static void CreateBugReportsList(Transform parent, Vector2 pos, Vector2 size)
//        {
//            GameObject root = new GameObject("BugReportsList");
//            root.transform.SetParent(parent, false);
//            RectTransform rootRect = root.AddComponent<RectTransform>();
//            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
//            rootRect.pivot = new Vector2(0.5f, 0.5f);
//            rootRect.anchoredPosition = pos;
//            rootRect.sizeDelta = size;
//            Image bg = root.AddComponent<Image>();
//            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);

//            ScrollRect scroll = root.AddComponent<ScrollRect>();
//            scroll.horizontal = false;
//            scroll.vertical = true;
//            scroll.movementType = ScrollRect.MovementType.Clamped;
//            scroll.scrollSensitivity = 30f;

//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(root.transform, false);
//            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
//            StretchFull(viewportRect, new Vector4(8f, 8f, 8f, 8f));
//            Image vpImage = viewport.AddComponent<Image>();
//            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
//            vpImage.raycastTarget = true;
//            viewport.AddComponent<RectMask2D>();

//            GameObject contentGo = new GameObject("Content");
//            contentGo.transform.SetParent(viewport.transform, false);
//            _bugReportsContent = contentGo.AddComponent<RectTransform>();
//            _bugReportsContent.anchorMin = new Vector2(0f, 1f);
//            _bugReportsContent.anchorMax = new Vector2(1f, 1f);
//            _bugReportsContent.pivot = new Vector2(0.5f, 1f);
//            _bugReportsContent.anchoredPosition = Vector2.zero;
//            _bugReportsContent.sizeDelta = new Vector2(0f, 140f);

//            scroll.viewport = viewportRect;
//            scroll.content = _bugReportsContent;
//            _bugReportsScroll = scroll;
//        }

//        private static void SetBugCustomRoles(bool enabled)
//        {
//            _bugCustomRolesEnabled = enabled;
//            SetButtonText(_bugCustomYesButton, enabled ? "YES ✓" : "YES");
//            SetButtonText(_bugCustomNoButton, enabled ? "NO" : "NO ✓");
//            if (_bugCustomRolesWhichInput != null) _bugCustomRolesWhichInput.gameObject.SetActive(enabled);
//        }

//        private static void SetBugOtherMods(bool enabled)
//        {
//            _bugOtherModsInstalled = enabled;
//            SetButtonText(_bugOtherYesButton, enabled ? "YES ✓" : "YES");
//            SetButtonText(_bugOtherNoButton, enabled ? "NO" : "NO ✓");
//            if (_bugOtherModsWhichInput != null) _bugOtherModsWhichInput.gameObject.SetActive(enabled);
//        }

//        private static void SubmitBugReport()
//        {
//            if (_bugRequestRunning) return;

//            string title = _bugTitleInput != null ? (_bugTitleInput.text ?? "").Trim() : "";
//            string gameMode = _bugGameModeInput != null ? (_bugGameModeInput.text ?? "").Trim() : "";
//            string customWhich = _bugCustomRolesWhichInput != null ? (_bugCustomRolesWhichInput.text ?? "").Trim() : "";
//            string modsWhich = _bugOtherModsWhichInput != null ? (_bugOtherModsWhichInput.text ?? "").Trim() : "";
//            string description = _bugDescriptionInput != null ? (_bugDescriptionInput.text ?? "").Trim() : "";

//            if (title.Length == 0) { SetStatus("Bug report title is required."); return; }
//            if (gameMode.Length == 0) { SetStatus("Game mode is required."); return; }
//            if (_bugCustomRolesEnabled && customWhich.Length == 0) { SetStatus("Specify which custom roles are enabled."); return; }
//            if (_bugOtherModsInstalled && modsWhich.Length == 0) { SetStatus("Specify which other mods are installed."); return; }
//            if (description.Length == 0) { SetStatus("Bug description is required."); return; }

//            _bugRequestRunning = true;
//            UpdateBugReportButtons();
//            SetStatus("Sending bug report and logs...");

//            StartRoutine(BanModCommunicationManager.SendBugReportCoroutine(
//                title,
//                gameMode,
//                _bugCustomRolesEnabled,
//                customWhich,
//                _bugOtherModsInstalled,
//                modsWhich,
//                description,
//                delegate (bool success, string result)
//                {
//                    _bugRequestRunning = false;
//                    SetStatus(result ?? (success ? "Bug report sent." : "Could not send bug report."));
//                    if (success)
//                    {
//                        ClearBugReportForm();
//                        LoadBugReports();
//                    }
//                    UpdateBugReportButtons();
//                }));
//        }

//        private static void ClearBugReportForm()
//        {
//            SetInputText(_bugTitleInput, "");
//            SetInputText(_bugGameModeInput, "");
//            SetInputText(_bugCustomRolesWhichInput, "");
//            SetInputText(_bugOtherModsWhichInput, "");
//            SetInputText(_bugDescriptionInput, "");
//            SetBugCustomRoles(false);
//            SetBugOtherMods(false);
//        }

//        private static void LoadBugReports()
//        {
//            if (_bugRequestRunning) return;
//            _bugRequestRunning = true;
//            UpdateBugReportButtons();
//            SetStatus("Loading bug reports...");

//            int keepId = _selectedBugReportId;
//            StartRoutine(BanModCommunicationManager.GetMyReportsCoroutine(
//                delegate (List<BanModCommunicationManager.ReportSummary> reports, string error)
//                {
//                    _bugRequestRunning = false;
//                    BugReports.Clear();

//                    if (!string.IsNullOrWhiteSpace(error))
//                    {
//                        SetStatus(error);
//                        _selectedBugReportId = 0;
//                        RenderBugReportsList();
//                        RenderSelectedBugReport();
//                        UpdateBugReportButtons();
//                        return;
//                    }

//                    if (reports != null)
//                    {
//                        for (int i = 0; i < reports.Count; i++)
//                        {
//                            BanModCommunicationManager.ReportSummary report = reports[i];
//                            if (report == null) continue;
//                            string type = report.Type ?? "";
//                            if (type.IndexOf("bug", StringComparison.OrdinalIgnoreCase) >= 0)
//                                BugReports.Add(report);
//                        }
//                    }

//                    _bugReportUnreadCount = 0;
//                    for (int i = 0; i < BugReports.Count; i++)
//                    {
//                        BanModCommunicationManager.ReportSummary report = BugReports[i];
//                        if (report == null) continue;
//                        if (report.UnreadCount > 0) _bugReportUnreadCount += report.UnreadCount;
//                        else if (report.IsUnread) _bugReportUnreadCount++;
//                    }

//                    _selectedBugReportId = FindBugReport(keepId) != null
//                        ? keepId
//                        : (BugReports.Count > 0 ? BugReports[0].Id : 0);

//                    RenderBugReportsList();
//                    RenderSelectedBugReport();
//                    RefreshBugUnreadVisuals();
//                    RefreshCombinedUnread();
//                    SetStatus(BugReports.Count + " bug report" + (BugReports.Count == 1 ? "" : "s"));
//                    UpdateBugReportButtons();
//                }));
//        }

//        private static BanModCommunicationManager.ReportSummary FindBugReport(int reportId)
//        {
//            if (reportId <= 0) return null;
//            for (int i = 0; i < BugReports.Count; i++)
//            {
//                BanModCommunicationManager.ReportSummary report = BugReports[i];
//                if (report != null && report.Id == reportId) return report;
//            }
//            return null;
//        }

//        private static void RenderBugReportsList()
//        {
//            if (_bugReportsContent == null) return;
//            ClearRectChildren(_bugReportsContent);

//            if (BugReports.Count == 0)
//            {
//                CreateLabel(_bugReportsContent, "NoBugReports", "No bug reports yet.", 14, TextAlignmentOptions.Center,
//                    new Color(0.68f, 0.72f, 0.80f, 1f), new Vector2(0f, -28f), new Vector2(620f, 40f));
//                _bugReportsContent.sizeDelta = new Vector2(0f, 140f);
//                return;
//            }

//            float top = 6f;
//            for (int i = 0; i < BugReports.Count; i++)
//            {
//                BanModCommunicationManager.ReportSummary report = BugReports[i];
//                if (report == null) continue;
//                int id = report.Id;
//                bool selected = id == _selectedBugReportId;
//                bool unread = report.IsUnread || report.UnreadCount > 0;
//                string status = string.IsNullOrWhiteSpace(report.Status) ? "open" : report.Status;
//                string label = (unread ? "● " : "") + "#" + id + "  " + Short(report.Title, 38) + "  [" + status + "]";
//                Button row = CreateButton(
//                    _bugReportsContent,
//                    "BugReport_" + id,
//                    label,
//                    new Vector2(0f, -(top + 22f)),
//                    new Vector2(640f, 44f),
//                    selected ? new Color(0.12f, 0.34f, 0.50f, 0.98f) : new Color(0.13f, 0.16f, 0.22f, 0.96f),
//                    delegate { SelectBugReport(id); });
//                RectTransform rowRect = row != null ? row.GetComponent<RectTransform>() : null;
//                if (rowRect != null)
//                {
//                    rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 1f);
//                    rowRect.pivot = new Vector2(0.5f, 1f);
//                    rowRect.anchoredPosition = new Vector2(0f, -top);
//                }
//                TextMeshProUGUI rowText = row != null ? row.GetComponentInChildren<TextMeshProUGUI>(true) : null;
//                if (rowText != null)
//                {
//                    rowText.alignment = TextAlignmentOptions.MidlineLeft;
//                    rowText.margin = new Vector4(12f, 0f, 8f, 0f);
//                    rowText.fontSizeMax = 18f;
//                }
//                top += 48f;
//            }

//            _bugReportsContent.sizeDelta = new Vector2(0f, Math.Max(140f, top + 8f));
//        }

//        private static void SelectBugReport(int reportId)
//        {
//            BanModCommunicationManager.ReportSummary report = FindBugReport(reportId);
//            if (report == null) return;
//            _selectedBugReportId = reportId;
//            report.IsUnread = false;
//            report.UnreadCount = 0;
//            SetInputText(_bugReplyInput, "");
//            try { BanModMessagePoller.MarkReportReadFromUi(reportId); } catch { }
//            StartRoutine(BanModCommunicationManager.MarkReportReadCoroutine(reportId));
//            RecountBugUnread();
//            RenderBugReportsList();
//            RenderSelectedBugReport();
//            RefreshBugUnreadVisuals();
//            RefreshCombinedUnread();
//            UpdateBugReportButtons();
//        }

//        private static void RecountBugUnread()
//        {
//            int count = 0;
//            for (int i = 0; i < BugReports.Count; i++)
//            {
//                BanModCommunicationManager.ReportSummary report = BugReports[i];
//                if (report == null) continue;
//                if (report.UnreadCount > 0) count += report.UnreadCount;
//                else if (report.IsUnread) count++;
//            }
//            _bugReportUnreadCount = Math.Max(0, count);
//        }

//        private static void RenderSelectedBugReport()
//        {
//            BanModCommunicationManager.ReportSummary report = FindBugReport(_selectedBugReportId);
//            if (report == null)
//            {
//                if (_bugSelectedHeader != null) _bugSelectedHeader.text = "Select a bug report to view the private conversation.";
//                if (_bugConversationText != null) _bugConversationText.text = "";
//                if (_bugConversationContent != null) _bugConversationContent.sizeDelta = new Vector2(0f, 220f);
//                UpdateBugReportButtons();
//                return;
//            }

//            string status = string.IsNullOrWhiteSpace(report.Status) ? "open" : report.Status;
//            if (_bugSelectedHeader != null)
//                _bugSelectedHeader.text = "#" + report.Id + " · " + (report.Title ?? "") + " · " + status.ToUpperInvariant();

//            StringBuilder builder = new StringBuilder();
//            builder.Append("Private bug conversation — only you and BanMod administration.\n");
//            if (!string.IsNullOrWhiteSpace(report.GameMode))
//                builder.Append("Game mode: ").Append(report.GameMode).Append("\n");

//            if (report.Chat != null && report.Chat.Count > 0)
//            {
//                for (int i = 0; i < report.Chat.Count; i++)
//                {
//                    BanModCommunicationManager.ReportChatMessage message = report.Chat[i];
//                    if (message == null || string.IsNullOrWhiteSpace(message.Message)) continue;
//                    string author = string.Equals(message.AuthorType ?? "", "admin", StringComparison.OrdinalIgnoreCase)
//                        ? "BANMOD ADMIN"
//                        : "YOU";
//                    builder.Append("\n").Append(author);
//                    string when = FormatBugTimestamp(message.CreatedAt);
//                    if (!string.IsNullOrWhiteSpace(when)) builder.Append(" · ").Append(when);
//                    builder.Append("\n").Append(message.Message.Trim()).Append("\n");
//                }
//            }
//            else if (!string.IsNullOrWhiteSpace(report.Message))
//            {
//                builder.Append("\nYOU\n").Append(report.Message.Trim()).Append("\n");
//                if (!string.IsNullOrWhiteSpace(report.AdminReply))
//                    builder.Append("\nBANMOD ADMIN\n").Append(report.AdminReply.Trim()).Append("\n");
//            }

//            if (_bugConversationText != null)
//            {
//                _bugConversationText.text = builder.ToString();
//                try
//                {
//                    _bugConversationText.ForceMeshUpdate();
//                    float height = Math.Max(220f, _bugConversationText.preferredHeight + 28f);
//                    if (_bugConversationContent != null) _bugConversationContent.sizeDelta = new Vector2(0f, height);
//                    RectTransform rt = _bugConversationText.GetComponent<RectTransform>();
//                    if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
//                    Canvas.ForceUpdateCanvases();
//                    if (_bugConversationScroll != null) _bugConversationScroll.verticalNormalizedPosition = 0f;
//                }
//                catch { }
//            }

//            UpdateBugReportButtons();
//        }

//        private static string FormatBugTimestamp(double value)
//        {
//            try
//            {
//                long seconds = Convert.ToInt64(value);
//                if (seconds <= 0) return "";
//                return DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
//            }
//            catch { return ""; }
//        }

//        private static void SendBugReportReply()
//        {
//            if (_bugRequestRunning || _selectedBugReportId <= 0 || _bugReplyInput == null) return;
//            string message = (_bugReplyInput.text ?? "").Trim();
//            if (message.Length == 0) { SetStatus("Write a message first."); return; }

//            _bugRequestRunning = true;
//            UpdateBugReportButtons();
//            SetStatus("Sending message...");
//            int id = _selectedBugReportId;

//            StartRoutine(BanModCommunicationManager.SendReportMessageCoroutine(
//                id,
//                message,
//                delegate (bool success, string result)
//                {
//                    _bugRequestRunning = false;
//                    SetStatus(result ?? (success ? "Message sent." : "Could not send message."));
//                    if (success)
//                    {
//                        SetInputText(_bugReplyInput, "");
//                        _selectedBugReportId = id;
//                        LoadBugReports();
//                    }
//                    else
//                    {
//                        UpdateBugReportButtons();
//                    }
//                }));
//        }

//        private static void CloseSelectedBugReport()
//        {
//            if (_bugRequestRunning || _selectedBugReportId <= 0) return;
//            _bugRequestRunning = true;
//            UpdateBugReportButtons();
//            SetStatus("Closing bug report as resolved...");
//            int id = _selectedBugReportId;

//            StartRoutine(CloseBugReportDirectCoroutine(
//                id,
//                delegate (bool success, string result)
//                {
//                    _bugRequestRunning = false;
//                    SetStatus(result ?? (success ? "Bug report closed as resolved." : "Could not close bug report."));
//                    _selectedBugReportId = id;

//                    if (success)
//                    {
//                        BanModCommunicationManager.ReportSummary local = FindBugReport(id);
//                        if (local != null)
//                            local.Status = "closed";
//                        RenderBugReportsList();
//                        RenderSelectedBugReport();
//                        UpdateBugReportButtons();
//                        LoadBugReports();
//                    }
//                    else
//                    {
//                        UpdateBugReportButtons();
//                    }
//                }));
//        }

//        private static IEnumerator CloseBugReportDirectCoroutine(int reportId, Action<bool, string> callback)
//        {
//            bool hasToken = false;
//            yield return BanModApiTokenManager.EnsureTokenCoroutine(delegate (bool success, string token)
//            {
//                hasToken = success;
//            });

//            if (!hasToken)
//            {
//                callback?.Invoke(false, "Token unavailable.");
//                yield break;
//            }

//            UnityWebRequest request = new UnityWebRequest(BanModCommunicationConfig.ReportCloseUrl(reportId), "POST");
//            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
//            request.downloadHandler = new DownloadHandlerBuffer();
//            request.timeout = 30;
//            request.SetRequestHeader("Content-Type", "application/json");
//            BanModApiTokenManager.ApplyAuthHeader(request);

//            yield return request.SendWebRequest();

//            long code = request.responseCode;
//            string body = request.downloadHandler != null ? request.downloadHandler.text : "";
//            string transportError = request.error ?? "";

//            if (code == 401)
//            {
//                BanModApiTokenManager.ClearToken(request);
//                request.Dispose();
//                callback?.Invoke(false, "Unauthorized. Token cleared, try again.");
//                yield break;
//            }

//            if (code == 404 || code == 405)
//            {
//                request.Dispose();
//                yield return BanModCommunicationManager.CloseReportCoroutine(reportId, callback);
//                yield break;
//            }

//            bool successHttp = code >= 200 && code < 300;
//            bool success = successHttp;
//            string message = successHttp ? "Report closed as resolved." : "Could not close report.";

//            if (!string.IsNullOrWhiteSpace(body))
//            {
//                try
//                {
//                    using (JsonDocument doc = JsonDocument.Parse(body))
//                    {
//                        JsonElement root = doc.RootElement;
//                        if (root.ValueKind == JsonValueKind.Object)
//                        {
//                            if (root.TryGetProperty("success", out JsonElement successElement) &&
//                                (successElement.ValueKind == JsonValueKind.True || successElement.ValueKind == JsonValueKind.False))
//                                success = successHttp && successElement.GetBoolean();

//                            if (root.TryGetProperty("message", out JsonElement msgElement) && msgElement.ValueKind == JsonValueKind.String)
//                                message = msgElement.GetString() ?? message;
//                            else if (!success && root.TryGetProperty("error", out JsonElement errElement) && errElement.ValueKind == JsonValueKind.String)
//                                message = errElement.GetString() ?? message;
//                        }
//                    }
//                }
//                catch { }
//            }

//            if (!success && !string.IsNullOrWhiteSpace(transportError))
//                message = "HTTP " + code + ": " + transportError;
//            else if (!success && code > 0 && message.IndexOf("HTTP ", StringComparison.OrdinalIgnoreCase) < 0)
//                message = "HTTP " + code + ": " + message;

//            request.Dispose();
//            callback?.Invoke(success, message);
//        }

//        private static void DeleteSelectedBugReport()
//        {
//            if (_bugRequestRunning || _selectedBugReportId <= 0) return;
//            _bugRequestRunning = true;
//            UpdateBugReportButtons();
//            SetStatus("Deleting bug report...");
//            int id = _selectedBugReportId;

//            StartRoutine(BanModCommunicationManager.DeleteReportCoroutine(
//                id,
//                delegate (bool success, string result)
//                {
//                    _bugRequestRunning = false;
//                    SetStatus(result ?? (success ? "Bug report deleted." : "Could not delete bug report."));
//                    if (success) _selectedBugReportId = 0;
//                    LoadBugReports();
//                }));
//        }

//        private static void UpdateBugReportButtons()
//        {
//            BanModCommunicationManager.ReportSummary report = FindBugReport(_selectedBugReportId);
//            bool selected = report != null;
//            string status = selected ? (report.Status ?? "") : "";
//            bool open = selected &&
//                !string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase) &&
//                !string.Equals(status, "resolved", StringComparison.OrdinalIgnoreCase);

//            if (_bugSendButton != null) _bugSendButton.interactable = !_bugRequestRunning;
//            if (_bugReplyButton != null) _bugReplyButton.interactable = !_bugRequestRunning && open;
//            if (_bugCloseButton != null) _bugCloseButton.interactable = !_bugRequestRunning && open;
//            if (_bugDeleteButton != null) _bugDeleteButton.interactable = !_bugRequestRunning && selected;
//            if (_bugReplyInput != null) _bugReplyInput.interactable = !_bugRequestRunning && open;
//        }

//        internal static void SetBugReportUnreadCount(int count)
//        {
//            _bugReportUnreadCount = Math.Max(0, count);
//            RefreshBugUnreadVisuals();
//            RefreshCombinedUnread();
//        }

//        private static void RefreshBugUnreadVisuals()
//        {
//            bool visible = _bugReportUnreadCount > 0;
//            try
//            {
//                if (_bugTabUnreadBadgeText != null)
//                {
//                    _bugTabUnreadBadgeText.text = visible ? "●" : "";
//                    _bugTabUnreadBadgeText.gameObject.SetActive(visible);
//                }
//            }
//            catch { }
//        }

//        internal static void ToggleBugReportFromF3()
//        {
//            EnsureStandaloneUi();
//            if (_root == null) return;

//            if (_root.activeSelf && _showingBugReport)
//            {
//                ForceClose();
//                return;
//            }

//            BlockMainMenuInput(true);
//            _root.SetActive(true);
//            FetchProfile();
//            ShowBugReportTab();
//            RefreshLauncherVisibility();
//        }

//        internal static void OpenBugReportNotification(BanModCommunicationManager.ReportSummary report, Action onClose)
//        {
//            EnsureStandaloneUi();
//            if (_root == null) return;

//            BlockMainMenuInput(true);
//            _root.SetActive(true);
//            FetchProfile();

//            if (report != null && report.Id > 0)
//            {
//                bool replaced = false;
//                for (int i = 0; i < BugReports.Count; i++)
//                {
//                    if (BugReports[i] != null && BugReports[i].Id == report.Id)
//                    {
//                        BugReports[i] = report;
//                        replaced = true;
//                        break;
//                    }
//                }
//                if (!replaced) BugReports.Insert(0, report);
//                _selectedBugReportId = report.Id;
//            }

//            _showingChat = false;
//            _showingPrivateChat = false;
//            _showingBugReport = true;
//            HideSectionOverlays();
//            if (_chatPanel != null) _chatPanel.SetActive(false);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(true);
//            if (_listPanel != null) _listPanel.SetActive(false);
//            if (_detailPanel != null) _detailPanel.SetActive(false);

//            if (report != null)
//            {
//                report.IsUnread = false;
//                report.UnreadCount = 0;
//                try { BanModMessagePoller.MarkReportReadFromUi(report.Id); } catch { }
//                RecountBugUnread();
//                RenderBugReportsList();
//                RenderSelectedBugReport();
//            }

//            RefreshCombinedUnread();
//            RefreshLauncherVisibility();
//            LoadBugReports();

//            try { onClose?.Invoke(); } catch { }
//        }

//        private static void TryUseLoginUsernameAsDefault()
//        {
//            if (!_profileStateLoaded || _profileNameLocked || _profileDefaultAttempted || _profileDefaultRequestRunning)
//                return;

//            string loginName = (_privateLoginUsername ?? "").Trim();
//            if (loginName.Length < 3 || loginName.Length > 20)
//                return;

//            _profileDefaultAttempted = true;
//            _profileDefaultRequestRunning = true;
//            var payload = new Dictionary<string, string> { { "display_name", loginName } };

//            StartRoutine(SendRequest("POST", CommunityUrl("/profile"), payload, delegate (bool ok, string body)
//            {
//                _profileDefaultRequestRunning = false;
//                if (ok)
//                {
//                    SetStatus("Community name set from your BanMod Login username.");
//                    FetchProfile();
//                    if (_showingChat) FetchChat();
//                }
//                else
//                {


//                    SetStatus("Could not use your Login username as the Community name: " + ErrorText(body));
//                    FetchProfile();
//                }
//            }));
//        }


//        private static void BuildChatPanel()
//        {
//            _chatPanel = CreatePanel(_window.transform, "HelpDiscussionPanel", new Vector2(0f, -55f), new Vector2(1320f, 660f), BanModUiStyles.PanelColor);
//            CreateLabel(_chatPanel.transform, "ChatHeading", "MOD HELP & DISCUSSION", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-455f, 286f), new Vector2(360f, 36f));
//            CreateLabel(_chatPanel.transform, "ChatRetention", "Ask questions about BanMod, get help, and talk with the community. Messages are kept for 3 days.", 13, TextAlignmentOptions.Right, new Color(0.58f, 0.64f, 0.76f, 1f), new Vector2(235f, 286f), new Vector2(650f, 28f));
//            CreateButton(_chatPanel.transform, "CommunityRefresh", "REFRESH", new Vector2(545f, 286f), new Vector2(130f, 42f), BanModUiStyles.ButtonColor, FetchChat);
//            CreateChatMessageList(_chatPanel.transform, new Vector2(0f, 42f), new Vector2(1210f, 425f));
//            _reportChatButton = CreateButton(_chatPanel.transform, "ReportChat", "REPORT SELECTED", new Vector2(-485f, -208f), new Vector2(210f, 44f), new Color(0.45f, 0.14f, 0.16f, 1f), ReportSelectedChatMessage);
//            _chatTranslateButton = CreateButton(_chatPanel.transform, "TranslateChat", "TRANSLATE", new Vector2(-275f, -208f), new Vector2(180f, 44f), new Color(0.14f, 0.34f, 0.52f, 1f), TranslateSelectedCommunityChatMessage);
//            _chatTagButton = CreateButton(_chatPanel.transform, "TagChat", "TAG", new Vector2(-100f, -208f), new Vector2(130f, 44f), new Color(0.24f, 0.32f, 0.48f, 1f), TagSelectedChatMessage);
//            _chatReplyButton = CreateButton(_chatPanel.transform, "ReplyChat", "REPLY", new Vector2(45f, -208f), new Vector2(150f, 44f), new Color(0.18f, 0.38f, 0.28f, 1f), ReplySelectedChatMessage);
//            CreateLabel(_chatPanel.transform, "SelectHint", "Select a message, then TAG / REPLY / TRANSLATE.", 12, TextAlignmentOptions.Left, new Color(0.62f, 0.68f, 0.80f, 1f), new Vector2(350f, -208f), new Vector2(400f, 30f));
//            _chatInput = CreateInput(_chatPanel.transform, "ChatInput", "Write a message...", true, new Vector2(-110f, -274f), new Vector2(850f, 56f), CommunityChatCharacterLimit);
//            if (_chatInput != null)
//                _chatInput.lineType = TMP_InputField.LineType.MultiLineSubmit;
//            BindSubmitAction(_chatInput, SubmitChat);
//            BanModSymbolPicker.AttachToTmpInput(
//                _chatPanel.transform,
//                "CommunitySymbolPicker",
//                _chatInput,
//                new Vector2(350f, -274f),
//                new Vector2(56f, 56f),
//                true);
//            _chatSendButton = CreateButton(_chatPanel.transform, "ChatSend", "SEND", new Vector2(515f, -274f), new Vector2(150f, 56f), new Color(0.15f, 0.42f, 0.28f, 1f), SubmitChat);
//            if (_reportChatButton != null) _reportChatButton.interactable = false;
//            if (_chatTranslateButton != null) _chatTranslateButton.interactable = false;
//            if (_chatTagButton != null) _chatTagButton.interactable = false;
//            if (_chatReplyButton != null) _chatReplyButton.interactable = false;
//        }

//        private static void CreateChatMessageList(Transform parent, Vector2 pos, Vector2 size)
//        {
//            _chatListRoot = new GameObject("ChatMessageList");
//            _chatListRoot.transform.SetParent(parent, false);
//            RectTransform rootRect = _chatListRoot.AddComponent<RectTransform>();
//            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
//            rootRect.pivot = new Vector2(0.5f, 0.5f);
//            rootRect.anchoredPosition = pos;
//            rootRect.sizeDelta = size;
//            Image bg = _chatListRoot.AddComponent<Image>();
//            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
//            _chatScroll = _chatListRoot.AddComponent<ScrollRect>();
//            _chatScroll.horizontal = false;
//            _chatScroll.vertical = true;
//            _chatScroll.movementType = ScrollRect.MovementType.Clamped;
//            _chatScroll.inertia = true;
//            _chatScroll.decelerationRate = 0.12f;


//            _chatScroll.scrollSensitivity = 0f;

//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(_chatListRoot.transform, false);
//            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
//            StretchFull(viewportRect, new Vector4(10f, 10f, 10f, 10f));
//            Image vpImage = viewport.AddComponent<Image>();
//            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
//            vpImage.raycastTarget = true;
//            viewport.AddComponent<RectMask2D>();

//            GameObject contentGo = new GameObject("Content");
//            contentGo.transform.SetParent(viewport.transform, false);
//            _chatContent = contentGo.AddComponent<RectTransform>();
//            _chatContent.anchorMin = new Vector2(0f, 1f);
//            _chatContent.anchorMax = new Vector2(1f, 1f);
//            _chatContent.pivot = new Vector2(0.5f, 1f);
//            _chatContent.anchoredPosition = Vector2.zero;
//            _chatContent.sizeDelta = new Vector2(0f, 410f);
//            _chatScroll.viewport = viewportRect;
//            _chatScroll.content = _chatContent;
//        }

//        private static void BuildPrivateChatPanel()
//        {
//            _privateChatPanel = CreatePanel(_window.transform, "PrivateChatPanel", new Vector2(0f, -55f), new Vector2(1320f, 660f), BanModUiStyles.PanelColor);
//            CreateLabel(_privateChatPanel.transform, "PrivateHeading", "PRIVATE CHAT", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-500f, 286f), new Vector2(300f, 36f));
//            CreateLabel(_privateChatPanel.transform, "PrivateHint", "Select an online user. Conversations are private between the two users.", 13, TextAlignmentOptions.Right, new Color(0.58f, 0.64f, 0.76f, 1f), new Vector2(230f, 286f), new Vector2(610f, 28f));
//            CreateButton(_privateChatPanel.transform, "PrivateRefresh", "REFRESH", new Vector2(545f, 286f), new Vector2(130f, 42f), BanModUiStyles.ButtonColor, RefreshPrivateChat);

//            CreateLabel(_privateChatPanel.transform, "PrivatePlayersHeading", "ONLINE USERS", 15, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(-475f, 246f), new Vector2(250f, 28f));
//            CreatePrivatePlayersList(_privateChatPanel.transform, new Vector2(-465f, -15f), new Vector2(300f, 475f));

//            _privateConversationTitle = CreateLabel(_privateChatPanel.transform, "PrivateConversationTitle", "SELECT AN ONLINE USER", 17, TextAlignmentOptions.Left, Color.white, new Vector2(125f, 246f), new Vector2(760f, 30f));
//            CreatePrivateMessagesList(_privateChatPanel.transform, new Vector2(150f, 35f), new Vector2(800f, 370f));

//            _privateChatInput = CreateInput(_privateChatPanel.transform, "PrivateInput", "Select an online user...", false, new Vector2(35f, -205f), new Vector2(550f, 54f), 300);
//            BindSubmitAction(_privateChatInput, SubmitPrivateChatMessage);
//            BanModSymbolPicker.AttachToTmpInput(
//                _privateChatPanel.transform,
//                "PrivateSymbolPicker",
//                _privateChatInput,
//                new Vector2(350f, -205f),
//                new Vector2(54f, 54f),
//                true);
//            CreateButton(_privateChatPanel.transform, "PrivateSend", "SEND", new Vector2(490f, -205f), new Vector2(140f, 54f), new Color(0.15f, 0.42f, 0.28f, 1f), SubmitPrivateChatMessage);

//            CreateLabel(
//                _privateChatPanel.transform,
//                "PrivateSelectHint",
//                "Select a message. TRANSLATE also works when LiveTranslator is OFF.",
//                12,
//                TextAlignmentOptions.Left,
//                new Color(0.58f, 0.62f, 0.72f, 1f),
//                new Vector2(-90f, -270f),
//                new Vector2(430f, 28f));

//            _privateTranslateButton = CreateButton(
//                _privateChatPanel.transform,
//                "PrivateTranslate",
//                "TRANSLATE",
//                new Vector2(215f, -270f),
//                new Vector2(170f, 42f),
//                new Color(0.14f, 0.34f, 0.52f, 1f),
//                TranslateSelectedPrivateMessage);

//            _privateReportButton = CreateButton(
//                _privateChatPanel.transform,
//                "PrivateReport",
//                "REPORT",
//                new Vector2(430f, -270f),
//                new Vector2(190f, 42f),
//                new Color(0.45f, 0.14f, 0.16f, 1f),
//                ReportSelectedPrivateMessage);

//            if (_privateChatInput != null) _privateChatInput.interactable = false;
//            if (_privateTranslateButton != null) _privateTranslateButton.interactable = false;
//            if (_privateReportButton != null) _privateReportButton.interactable = false;
//        }

//        private static void RefreshPrivateChat()
//        {
//            RequestPrivateChatImmediateRefresh();
//            SetStatus("Refreshing Private Chat...");
//        }

//        private static void CreatePrivatePlayersList(Transform parent, Vector2 pos, Vector2 size)
//        {
//            GameObject root = new GameObject("PrivatePlayersList");
//            root.transform.SetParent(parent, false);
//            RectTransform rootRect = root.AddComponent<RectTransform>();
//            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
//            rootRect.pivot = new Vector2(0.5f, 0.5f);
//            rootRect.anchoredPosition = pos;
//            rootRect.sizeDelta = size;
//            Image bg = root.AddComponent<Image>();
//            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
//            ScrollRect scroll = root.AddComponent<ScrollRect>();
//            scroll.horizontal = false;
//            scroll.vertical = true;
//            scroll.movementType = ScrollRect.MovementType.Clamped;
//            scroll.scrollSensitivity = 30f;

//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(root.transform, false);
//            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
//            StretchFull(viewportRect, new Vector4(8f, 8f, 8f, 8f));
//            Image vpImage = viewport.AddComponent<Image>();
//            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
//            vpImage.raycastTarget = true;
//            viewport.AddComponent<RectMask2D>();

//            GameObject contentGo = new GameObject("Content");
//            contentGo.transform.SetParent(viewport.transform, false);
//            _privatePlayersContent = contentGo.AddComponent<RectTransform>();
//            _privatePlayersContent.anchorMin = new Vector2(0f, 1f);
//            _privatePlayersContent.anchorMax = new Vector2(1f, 1f);
//            _privatePlayersContent.pivot = new Vector2(0.5f, 1f);
//            _privatePlayersContent.anchoredPosition = Vector2.zero;
//            _privatePlayersContent.sizeDelta = new Vector2(0f, 440f);

//            scroll.viewport = viewportRect;
//            scroll.content = _privatePlayersContent;
//            _privatePlayersScroll = scroll;
//        }

//        private static void CreatePrivateMessagesList(Transform parent, Vector2 pos, Vector2 size)
//        {
//            GameObject root = new GameObject("PrivateMessagesList");
//            root.transform.SetParent(parent, false);
//            RectTransform rootRect = root.AddComponent<RectTransform>();
//            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
//            rootRect.pivot = new Vector2(0.5f, 0.5f);
//            rootRect.anchoredPosition = pos;
//            rootRect.sizeDelta = size;
//            Image bg = root.AddComponent<Image>();
//            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
//            ScrollRect scroll = root.AddComponent<ScrollRect>();
//            scroll.horizontal = false;
//            scroll.vertical = true;
//            scroll.movementType = ScrollRect.MovementType.Clamped;
//            scroll.scrollSensitivity = 35f;

//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(root.transform, false);
//            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
//            StretchFull(viewportRect, new Vector4(10f, 10f, 10f, 10f));
//            Image vpImage = viewport.AddComponent<Image>();
//            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
//            vpImage.raycastTarget = true;
//            viewport.AddComponent<RectMask2D>();

//            GameObject contentGo = new GameObject("Content");
//            contentGo.transform.SetParent(viewport.transform, false);
//            _privateMessagesContent = contentGo.AddComponent<RectTransform>();
//            _privateMessagesContent.anchorMin = new Vector2(0f, 1f);
//            _privateMessagesContent.anchorMax = new Vector2(1f, 1f);
//            _privateMessagesContent.pivot = new Vector2(0.5f, 1f);
//            _privateMessagesContent.anchoredPosition = Vector2.zero;
//            _privateMessagesContent.sizeDelta = new Vector2(0f, 340f);

//            scroll.viewport = viewportRect;
//            scroll.content = _privateMessagesContent;
//            _privateMessagesScroll = scroll;
//        }

//        private static void HideSectionOverlays()
//        {
//            if (_composePanel != null) _composePanel.SetActive(false);
//            if (_managePanel != null) _managePanel.SetActive(false);
//            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);
//        }

//        private static void ShowChatTab()
//        {
//            _showingChat = true;
//            _showingPrivateChat = false;
//            _showingBugReport = false;
//            HideSectionOverlays();
//            if (_chatPanel != null) _chatPanel.SetActive(true);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
//            if (_listPanel != null) _listPanel.SetActive(false);
//            if (_detailPanel != null) _detailPanel.SetActive(false);
//            FetchChat();
//        }

//        private static void ShowPrivateChatTab()
//        {
//            _showingChat = false;
//            _showingPrivateChat = true;
//            _showingBugReport = false;
//            HideSectionOverlays();
//            if (_chatPanel != null) _chatPanel.SetActive(false);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(true);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
//            if (_listPanel != null) _listPanel.SetActive(false);
//            if (_detailPanel != null) _detailPanel.SetActive(false);
//            RequestPrivateChatImmediateRefresh();
//            MarkActivePrivateConversationRead();
//            RenderPrivatePlayers();
//            RenderPrivateMessages(true);
//            SetStatus(string.IsNullOrWhiteSpace(_privateActiveFriendCode)
//                ? "Private Chat · select an online user."
//                : "Private Chat with " + _privateActiveName);
//        }

//        private static void ShowBugReportTab()
//        {
//            _showingChat = false;
//            _showingPrivateChat = false;
//            _showingBugReport = true;
//            HideSectionOverlays();
//            if (_chatPanel != null) _chatPanel.SetActive(false);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(true);
//            if (_listPanel != null) _listPanel.SetActive(false);
//            if (_detailPanel != null) _detailPanel.SetActive(false);
//            LoadBugReports();
//        }

//        private static void ShowSuggestionsTab()
//        {
//            _showingChat = false;
//            _showingPrivateChat = false;
//            _showingBugReport = false;
//            HideSectionOverlays();
//            if (_chatPanel != null) _chatPanel.SetActive(false);
//            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
//            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
//            if (_listPanel != null) _listPanel.SetActive(true);
//            if (_detailPanel != null) _detailPanel.SetActive(true);
//            FetchPosts();
//        }

//        private static void RefreshActive()
//        {
//            FetchProfile();
//            if (_showingPrivateChat)
//            {
//                RefreshPrivateChat();
//            }
//            else if (_showingBugReport)
//            {
//                LoadBugReports();
//            }
//            else if (_showingChat)
//            {
//                FetchChat();
//            }
//            else
//            {
//                FetchPosts();
//            }
//        }

//        private static void BuildListPanel()
//        {
//            _listPanel = CreatePanel(_window.transform, "ListPanel", new Vector2(-385f, -55f), new Vector2(520f, 660f), BanModUiStyles.PanelColor);
//            CreateLabel(_listPanel.transform, "ListHeading", "SUGGESTIONS", 22, TextAlignmentOptions.Left, Color.white, new Vector2(-135f, 286f), new Vector2(230f, 36f));
//            CreateButton(_listPanel.transform, "SuggestionsRefresh", "REFRESH", new Vector2(55f, 286f), new Vector2(110f, 42f), BanModUiStyles.ButtonColor, FetchPosts);
//            _newPostButton = CreateButton(_listPanel.transform, "NewSuggestion", "+ NEW", new Vector2(185f, 286f), new Vector2(110f, 42f), new Color(0.18f, 0.42f, 0.18f, 1f), ShowCompose);
//            CreateSuggestionsListScroll(_listPanel.transform, new Vector2(0f, -12f), new Vector2(470f, 520f));
//        }

//        private static void CreateSuggestionsListScroll(Transform parent, Vector2 pos, Vector2 size)
//        {
//            GameObject root = new GameObject("SuggestionsListScroll");
//            root.transform.SetParent(parent, false);
//            RectTransform rootRect = root.AddComponent<RectTransform>();
//            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
//            rootRect.pivot = new Vector2(0.5f, 0.5f);
//            rootRect.anchoredPosition = pos;
//            rootRect.sizeDelta = size;
//            Image bg = root.AddComponent<Image>();
//            ApplyModernImage(bg, new Color(0.075f, 0.085f, 0.12f, 0.96f), true);

//            ScrollRect scroll = root.AddComponent<ScrollRect>();
//            scroll.horizontal = false;
//            scroll.vertical = true;
//            scroll.movementType = ScrollRect.MovementType.Clamped;
//            scroll.scrollSensitivity = 34f;

//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(root.transform, false);
//            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
//            StretchFull(viewportRect, new Vector4(8f, 8f, 8f, 8f));
//            Image viewportImage = viewport.AddComponent<Image>();
//            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
//            viewportImage.raycastTarget = true;
//            viewport.AddComponent<RectMask2D>();

//            GameObject contentGo = new GameObject("Content");
//            contentGo.transform.SetParent(viewport.transform, false);
//            _suggestionsListContent = contentGo.AddComponent<RectTransform>();
//            _suggestionsListContent.anchorMin = new Vector2(0f, 1f);
//            _suggestionsListContent.anchorMax = new Vector2(1f, 1f);
//            _suggestionsListContent.pivot = new Vector2(0.5f, 1f);
//            _suggestionsListContent.anchoredPosition = Vector2.zero;
//            _suggestionsListContent.sizeDelta = new Vector2(0f, 500f);

//            scroll.viewport = viewportRect;
//            scroll.content = _suggestionsListContent;
//            _suggestionsListScroll = scroll;
//        }

//        private static void BuildDetailPanel()
//        {
//            _detailPanel = CreatePanel(_window.transform, "DetailPanel", new Vector2(305f, -55f), new Vector2(740f, 660f), BanModUiStyles.PanelColor);
//            CreateLabel(_detailPanel.transform, "DetailHeading", "DISCUSSION", 22, TextAlignmentOptions.Left, Color.white, new Vector2(-205f, 286f), new Vector2(280f, 36f));
//            _detailText = CreateScrollableText(_detailPanel.transform, "DiscussionScroll", new Vector2(0f, 80f), new Vector2(680f, 360f), out _detailScroll, out _detailContent);
//            _replyInput = CreateInput(_detailPanel.transform, "ReplyInput", "Write a reply...", true, new Vector2(-75f, -145f), new Vector2(480f, 82f), 2500);
//            CreateButton(_detailPanel.transform, "ReplySend", "REPLY", new Vector2(260f, -145f), new Vector2(130f, 54f), new Color(0.15f, 0.42f, 0.28f, 1f), SubmitReply);
//            CreateButton(_detailPanel.transform, "ReplyPrev", "<", new Vector2(-315f, -238f), new Vector2(54f, 42f), new Color(0.15f, 0.17f, 0.25f, 1f), SelectPreviousReply);
//            CreateButton(_detailPanel.transform, "ReplyNext", ">", new Vector2(-250f, -238f), new Vector2(54f, 42f), new Color(0.15f, 0.17f, 0.25f, 1f), SelectNextReply);
//            _reportReplyButton = CreateButton(_detailPanel.transform, "ReportReply", "REPORT COMMENT", new Vector2(-110f, -238f), new Vector2(185f, 42f), new Color(0.45f, 0.14f, 0.16f, 1f), ReportSelectedReply);
//            _starButton = CreateButton(_detailPanel.transform, "Star", "☆ STAR", new Vector2(105f, -238f), new Vector2(155f, 42f), new Color(0.46f, 0.36f, 0.10f, 1f), ToggleStar);
//            _suggestionTranslateButton = CreateButton(_detailPanel.transform, "TranslateSuggestion", "TRANSLATE POST", new Vector2(285f, -238f), new Vector2(160f, 42f), new Color(0.14f, 0.34f, 0.52f, 1f), TranslateSelectedSuggestionContent);
//            CreateLabel(_detailPanel.transform, "ReportHint", "Use < / > to select a comment. TRANSLATE works even when LiveTranslator is OFF.", 12, TextAlignmentOptions.Left, new Color(0.58f, 0.62f, 0.72f, 1f), new Vector2(0f, -294f), new Vector2(660f, 26f));
//            _starButton.interactable = false;
//            if (_reportReplyButton != null) _reportReplyButton.interactable = false;
//            if (_suggestionTranslateButton != null) _suggestionTranslateButton.interactable = false;
//        }

//        private static void BuildComposePanel()
//        {
//            _composePanel = CreatePanel(_window.transform, "ComposePanel", new Vector2(0f, -55f), new Vector2(1180f, 660f), BanModUiStyles.PanelColor);
//            CreateLabel(_composePanel.transform, "ComposeTitle", "NEW SUGGESTION", 30, TextAlignmentOptions.Left, Color.white, new Vector2(-325f, 280f), new Vector2(520f, 46f));
//            _titleInput = CreateInput(_composePanel.transform, "TitleInput", "Suggestion title...", false, new Vector2(0f, 195f), new Vector2(900f, 58f), 120);
//            _messageInput = CreateInput(_composePanel.transform, "MessageInput", "Describe your suggestion...", true, new Vector2(0f, 20f), new Vector2(900f, 240f), 4000);
//            CreateLabel(_composePanel.transform, "AnonInfo", "Your Community name is shown publicly. Your FriendCode/account identity is visible only to moderators.", 14, TextAlignmentOptions.Left, new Color(0.72f, 0.76f, 0.86f, 1f), new Vector2(0f, -135f), new Vector2(900f, 42f));
//            CreateButton(_composePanel.transform, "Publish", "PUBLISH", new Vector2(125f, -265f), new Vector2(210f, 54f), new Color(0.14f, 0.44f, 0.25f, 1f), SubmitPost);
//            CreateButton(_composePanel.transform, "Cancel", "CANCEL", new Vector2(-125f, -265f), new Vector2(210f, 54f), new Color(0.27f, 0.28f, 0.34f, 1f), delegate { _composePanel.SetActive(false); });
//            _composePanel.SetActive(false);
//        }

//        private static void BuildManagePanel()
//        {
//            _managePanel = CreatePanel(_window.transform, "ManageOwnContent", new Vector2(0f, -55f), new Vector2(980f, 620f), new Color(0.075f, 0.085f, 0.12f, 1f));
//            _manageHeading = CreateLabel(_managePanel.transform, "ManageHeading", "EDIT", 28, TextAlignmentOptions.Left, Color.white, new Vector2(-225f, 250f), new Vector2(420f, 44f));
//            _manageTitleInput = CreateInput(_managePanel.transform, "ManageTitle", "Title...", false, new Vector2(0f, 165f), new Vector2(720f, 56f), 120);
//            _manageMessageInput = CreateInput(_managePanel.transform, "ManageMessage", "Message...", true, new Vector2(0f, 10f), new Vector2(720f, 260f), 4000);
//            if (_manageMessageInput != null)
//            {
//                _manageMessageInput.onValueChanged.AddListener((UnityAction<string>)delegate (string value)
//                {
//                    if (_manageMode != "chat") return;
//                    int length = (value ?? "").Length;
//                    SetStatus("Editing message: " + length + "/" + CommunityChatCharacterLimit + " characters.");
//                });
//            }
//            CreateLabel(_managePanel.transform, "ManageInfo", "Only your messages can be edited or deleted. Community limit: 1000 characters.", 13, TextAlignmentOptions.Left, new Color(0.68f, 0.73f, 0.84f, 1f), new Vector2(0f, -155f), new Vector2(720f, 30f));
//            CreateButton(_managePanel.transform, "ManageSave", "SAVE", new Vector2(190f, -245f), new Vector2(170f, 52f), new Color(0.14f, 0.44f, 0.25f, 1f), SaveManagedContent);
//            CreateButton(_managePanel.transform, "ManageDelete", "DELETE", new Vector2(0f, -245f), new Vector2(170f, 52f), BanModUiStyles.DangerColor, DeleteManagedContent);
//            CreateButton(_managePanel.transform, "ManageCancel", "CANCEL", new Vector2(-190f, -245f), new Vector2(170f, 52f), new Color(0.27f, 0.28f, 0.34f, 1f), CloseManagePanel);
//            _managePanel.SetActive(false);
//        }

//        private static void ShowCompose()
//        {
//            if (_composePanel == null) return;
//            SetInputText(_titleInput, "");
//            SetInputText(_messageInput, "");
//            _composePanel.SetActive(true);
//        }

//        private static void FetchProfile()
//        {
//            StartRoutine(SendRequest("GET", CommunityUrl("/profile"), null, delegate (bool ok, string body)
//            {
//                if (!ok) return;
//                try
//                {
//                    using (var doc = JsonDocument.Parse(body))
//                    {
//                        JsonElement profile;
//                        if (!doc.RootElement.TryGetProperty("profile", out profile)) return;
//                        JsonElement name;
//                        _communityDisplayName = profile.TryGetProperty("display_name", out name) ? (name.GetString() ?? "") : "";

//                        try
//                        {
//                            if (!string.IsNullOrWhiteSpace(_communityDisplayName))
//                            {
//                                PlayerPrefs.SetString(CommunityDisplayNameCacheKey, _communityDisplayName.Trim());
//                                PlayerPrefs.Save();
//                            }
//                        }
//                        catch { }

//                        JsonElement locked;
//                        _profileNameLocked = profile.TryGetProperty("name_locked", out locked) && locked.ValueKind == JsonValueKind.True;
//                        JsonElement pending;
//                        _profilePendingName = profile.TryGetProperty("pending_display_name", out pending) && pending.ValueKind == JsonValueKind.String
//                            ? (pending.GetString() ?? "") : "";
//                        _profileStateLoaded = true;

//                        if (_profileHeaderText != null)
//                            _profileHeaderText.text = string.IsNullOrWhiteSpace(_communityDisplayName)
//                                ? "COMMUNITY NAME"
//                                : _communityDisplayName;

//                        if (!_profileNameLocked)
//                        {
//                            if (_profileNameText != null)
//                                _profileNameText.text = "Choose your Community name once. Your Login username is used by default.";
//                            if (_profileInput != null) _profileInput.interactable = true;
//                            SetInputText(_profileInput,
//                                !string.IsNullOrWhiteSpace(_privateLoginUsername) ? _privateLoginUsername : _communityDisplayName);
//                            SetButtonText(_profileActionButton, "SET NAME");
//                            if (_profileActionButton != null) _profileActionButton.interactable = true;
//                        }
//                        else if (!string.IsNullOrWhiteSpace(_profilePendingName))
//                        {
//                            if (_profileNameText != null)
//                                _profileNameText.text = "CURRENT: " + _communityDisplayName + " · CHANGE REQUEST PENDING";
//                            SetInputText(_profileInput, _profilePendingName);
//                            if (_profileInput != null) _profileInput.interactable = false;
//                            SetButtonText(_profileActionButton, "PENDING");
//                            if (_profileActionButton != null) _profileActionButton.interactable = false;
//                        }
//                        else
//                        {
//                            if (_profileNameText != null)
//                                _profileNameText.text = "CURRENT: " + _communityDisplayName + " · LOCKED";
//                            SetInputText(_profileInput, "");
//                            if (_profileInput != null) _profileInput.interactable = true;
//                            SetButtonText(_profileActionButton, "REQUEST CHANGE");
//                            if (_profileActionButton != null) _profileActionButton.interactable = true;
//                        }

//                        TryUseLoginUsernameAsDefault();
//                    }
//                }
//                catch { }
//            }));
//        }

//        private static void SaveProfileName()
//        {
//            if (_requestRunning || _profileInput == null) return;
//            string name = (_profileInput.text ?? "").Trim();
//            if (name.Length < 3) { SetStatus("Community name must be at least 3 characters."); return; }
//            _requestRunning = true;
//            var payload = new Dictionary<string, string> { { "display_name", name } };
//            string endpoint = _profileNameLocked ? "/profile/change-request" : "/profile";
//            StartRoutine(SendRequest("POST", CommunityUrl(endpoint), payload, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok)
//                {
//                    SetStatus((_profileNameLocked ? "Name-change request failed: " : "Name setup failed: ") + ErrorText(body));
//                    FetchProfile();
//                    return;
//                }
//                SetStatus(_profileNameLocked ? "Name-change request sent for admin approval." : "Community name saved and locked.");
//                FetchProfile();
//                if (_showingChat) FetchChat();
//                else if (!_showingPrivateChat && !_showingBugReport) FetchPosts();
//            }));
//        }

//        private static void SetChatSendButtonText(string text)
//        {
//            if (_chatSendButton == null) return;
//            try
//            {
//                TextMeshProUGUI label = _chatSendButton.GetComponentInChildren<TextMeshProUGUI>(true);
//                if (label != null) label.text = text ?? "SEND";
//            }
//            catch { }
//        }

//        private static void BeginInlineChatEdit(CommunityChatMessage message)
//        {
//            if (message == null || !message.MyMessage || _chatInput == null)
//                return;

//            if (_editingChatMessageId <= 0)
//                _chatDraftBeforeEdit = _chatInput.text ?? "";

//            _editingChatMessageId = message.Id;
//            _selectedChatIndex = -1;

//            _chatInput.characterLimit = CommunityChatCharacterLimit;
//            _chatInput.lineType = TMP_InputField.LineType.MultiLineSubmit;
//            _chatInput.contentType = TMP_InputField.ContentType.Standard;
//            _chatInput.inputType = TMP_InputField.InputType.Standard;
//            _chatInput.richText = false;
//            _chatInput.enabled = true;
//            _chatInput.interactable = true;
//            _chatInput.readOnly = false;

//            SetInputText(_chatInput, message.Message ?? "");
//            SetChatSendButtonText("SAVE");

//            if (_chatTagButton != null) _chatTagButton.interactable = false;
//            if (_chatReplyButton != null) _chatReplyButton.interactable = false;
//            if (_chatTranslateButton != null) _chatTranslateButton.interactable = false;

//            SetStatus("EDIT MODE · " +
//                (_chatInput.text ?? "").Length + "/" +
//                CommunityChatCharacterLimit +
//                " characters · SAVE to apply changes");

//            StartRoutine(FocusInlineChatEditRoutine());
//        }

//        private static IEnumerator FocusInlineChatEditRoutine()
//        {
//            yield return null;

//            if (_editingChatMessageId <= 0 || _chatInput == null)
//                yield break;

//            try
//            {
//                if (UnityEngine.EventSystems.EventSystem.current != null)
//                {
//                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
//                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_chatInput.gameObject);
//                }
//            }
//            catch { }

//            try
//            {
//                _chatInput.enabled = true;
//                _chatInput.interactable = true;
//                _chatInput.readOnly = false;
//                _chatInput.ActivateInputField();
//                _chatInput.ForceLabelUpdate();

//                int end = (_chatInput.text ?? "").Length;
//                _chatInput.caretPosition = end;
//                _chatInput.selectionAnchorPosition = end;
//                _chatInput.selectionFocusPosition = end;
//                try { _chatInput.MoveTextEnd(false); } catch { }
//            }
//            catch { }

//            yield return null;

//            if (_editingChatMessageId <= 0 || _chatInput == null)
//                yield break;

//            try
//            {
//                _chatInput.ActivateInputField();
//                int end = (_chatInput.text ?? "").Length;
//                _chatInput.caretPosition = end;
//                _chatInput.selectionAnchorPosition = end;
//                _chatInput.selectionFocusPosition = end;
//            }
//            catch { }
//        }

//        private static void EndInlineChatEdit(bool restoreDraft)
//        {
//            string draft = restoreDraft ? (_chatDraftBeforeEdit ?? "") : "";

//            _editingChatMessageId = 0;
//            _chatDraftBeforeEdit = "";

//            SetChatSendButtonText("SEND");

//            if (_chatInput != null)
//            {
//                _chatInput.characterLimit = CommunityChatCharacterLimit;
//                _chatInput.lineType = TMP_InputField.LineType.MultiLineSubmit;
//                SetInputText(_chatInput, draft);
//            }

//            RenderChat(false);
//        }

//        private static void SaveInlineChatEdit()
//        {
//            if (_editingChatMessageId <= 0 || _chatInput == null || _manageRequestRunning)
//                return;

//            string message = (_chatInput.text ?? "").Trim();

//            if (message.Length == 0)
//            {
//                SetStatus("Message cannot be empty.");
//                return;
//            }

//            if (message.Length > CommunityChatCharacterLimit)
//            {
//                SetStatus("Community messages are limited to " +
//                    CommunityChatCharacterLimit +
//                    " characters. Current length: " + message.Length + ".");
//                return;
//            }

//            int messageId = _editingChatMessageId;
//            var payload = new Dictionary<string, string> { { "message", message } };

//            _manageRequestRunning = true;
//            SetStatus("Saving edited message...");

//            StartRoutine(SendRequest(
//                "PATCH",
//                CommunityUrl("/chat/" + messageId),
//                payload,
//                delegate (bool ok, string body)
//                {
//                    _manageRequestRunning = false;

//                    if (!ok)
//                    {
//                        SetStatus("Edit failed: " + ErrorText(body));
//                        return;
//                    }

//                    EndInlineChatEdit(true);
//                    SetStatus("Message edited.");
//                    FetchChat(false);
//                }));
//        }

//        private static void OpenManageChat(CommunityChatMessage message)
//        {
//            if (message == null || !message.MyMessage || _managePanel == null)
//                return;

//            _manageMode = "chat";
//            _manageTargetId = message.Id;
//            _manageRequestRunning = false;

//            if (_manageHeading != null)
//                _manageHeading.text = "EDIT YOUR MESSAGE";

//            if (_manageTitleInput != null)
//                _manageTitleInput.gameObject.SetActive(false);

//            _managePanel.SetActive(true);
//            _managePanel.transform.SetAsLastSibling();

//            if (_manageMessageInput != null)
//            {
//                _manageMessageInput.gameObject.SetActive(true);
//                _manageMessageInput.enabled = true;
//                _manageMessageInput.interactable = true;
//                _manageMessageInput.readOnly = false;

//                _manageMessageInput.characterLimit = CommunityChatCharacterLimit;
//                _manageMessageInput.contentType = TMP_InputField.ContentType.Standard;
//                _manageMessageInput.inputType = TMP_InputField.InputType.Standard;
//                _manageMessageInput.lineType = TMP_InputField.LineType.MultiLineNewline;
//                _manageMessageInput.richText = false;

//                if (_manageMessageInput.textComponent != null)
//                {
//                    _manageMessageInput.textComponent.richText = false;
//                    _manageMessageInput.textComponent.enableWordWrapping = true;
//                    _manageMessageInput.textComponent.overflowMode = TextOverflowModes.Overflow;
//                }

//                SetInputText(_manageMessageInput, message.Message ?? "");
//                _manageMessageInput.ForceLabelUpdate();

//                SetStatus("Editing message: " +
//                    (_manageMessageInput.text ?? "").Length + "/" +
//                    CommunityChatCharacterLimit + " characters.");

//                StartRoutine(FocusManagePopupInputRoutine());
//            }
//        }

//        private static IEnumerator FocusManagePopupInputRoutine()
//        {
//            yield return null;
//            yield return null;

//            if (_managePanel == null ||
//                !_managePanel.activeSelf ||
//                _manageMode != "chat" ||
//                _manageMessageInput == null)
//                yield break;

//            try
//            {
//                if (UnityEngine.EventSystems.EventSystem.current != null)
//                {
//                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
//                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(
//                        _manageMessageInput.gameObject);
//                }
//            }
//            catch { }

//            try
//            {
//                _manageMessageInput.enabled = true;
//                _manageMessageInput.interactable = true;
//                _manageMessageInput.readOnly = false;
//                _manageMessageInput.ActivateInputField();
//                _manageMessageInput.ForceLabelUpdate();

//                int end = (_manageMessageInput.text ?? "").Length;
//                _manageMessageInput.caretPosition = end;
//                _manageMessageInput.selectionAnchorPosition = end;
//                _manageMessageInput.selectionFocusPosition = end;

//                try { _manageMessageInput.MoveTextEnd(false); } catch { }
//            }
//            catch { }
//        }

//        private static void OpenManagePost(CommunityPost post)
//        {
//            if (post == null || !post.MyPost || _managePanel == null) return;
//            _manageMode = "post";
//            _manageTargetId = post.Id;
//            if (_manageHeading != null) _manageHeading.text = "EDIT YOUR SUGGESTION";
//            if (_manageTitleInput != null)
//            {
//                _manageTitleInput.gameObject.SetActive(true);
//                SetInputText(_manageTitleInput, post.Title);
//            }
//            if (_manageMessageInput != null)
//            {
//                _manageMessageInput.gameObject.SetActive(true);
//                _manageMessageInput.characterLimit = 4000;
//                SetInputText(_manageMessageInput, post.Message);
//            }
//            _managePanel.SetActive(true);
//            _managePanel.transform.SetAsLastSibling();
//        }

//        private static void CloseManagePanel()
//        {
//            _manageMode = "";
//            _manageTargetId = 0;
//            _manageRequestRunning = false;
//            if (_managePanel != null) _managePanel.SetActive(false);
//        }

//        private static void SaveManagedContent()
//        {
//            if (_manageRequestRunning || _manageTargetId <= 0) return;
//            object payload;
//            string url;
//            if (_manageMode == "chat")
//            {
//                string message = _manageMessageInput == null ? "" : (_manageMessageInput.text ?? "").Trim();

//                if (message.Length == 0)
//                {
//                    SetStatus("Message cannot be empty.");
//                    return;
//                }

//                if (message.Length > CommunityChatCharacterLimit)
//                {
//                    SetStatus("Community messages are limited to " +
//                        CommunityChatCharacterLimit +
//                        " characters. Current length: " + message.Length + ".");
//                    return;
//                }

//                payload = new Dictionary<string, string> { { "message", message } };
//                url = CommunityUrl("/chat/" + _manageTargetId);
//            }
//            else if (_manageMode == "post")
//            {
//                string title = _manageTitleInput == null ? "" : (_manageTitleInput.text ?? "").Trim();
//                string message = _manageMessageInput == null ? "" : (_manageMessageInput.text ?? "").Trim();
//                if (title.Length == 0 || message.Length == 0) { SetStatus("Title and message are required."); return; }
//                payload = new Dictionary<string, string> { { "title", title }, { "message", message } };
//                url = CommunityUrl("/posts/" + _manageTargetId);
//            }
//            else return;

//            string mode = _manageMode;
//            string requestedChatText =
//                mode == "chat" && _manageMessageInput != null
//                    ? (_manageMessageInput.text ?? "").Trim()
//                    : "";

//            _manageRequestRunning = true;
//            SetStatus("Saving...");

//            StartRoutine(SendRequest("PATCH", url, payload, delegate (bool ok, string body)
//            {
//                _manageRequestRunning = false;

//                if (!ok)
//                {
//                    SetStatus("Edit failed: " + ErrorText(body));
//                    return;
//                }

//                if (mode == "chat")
//                {
//                    int serverLength = -1;
//                    string serverMessage = "";

//                    try
//                    {
//                        using (var doc = JsonDocument.Parse(body ?? "{}"))
//                        {
//                            JsonElement messageObj;
//                            if (doc.RootElement.TryGetProperty("message", out messageObj) &&
//                                messageObj.ValueKind == JsonValueKind.Object)
//                            {
//                                JsonElement messageText;
//                                if (messageObj.TryGetProperty("message", out messageText) &&
//                                    messageText.ValueKind == JsonValueKind.String)
//                                {
//                                    serverMessage = messageText.GetString() ?? "";
//                                    serverLength = serverMessage.Length;
//                                }
//                            }
//                        }
//                    }
//                    catch { }




//                    if (serverLength >= 0 &&
//                        requestedChatText.Length > serverLength &&
//                        !string.Equals(requestedChatText, serverMessage, StringComparison.Ordinal))
//                    {
//                        SetStatus(
//                            "SERVER TRUNCATED EDIT: sent " +
//                            requestedChatText.Length +
//                            " characters, saved " +
//                            serverLength +
//                            ". The Community PATCH backend limit must be raised to " +
//                            CommunityChatCharacterLimit + ".");


//                        if (_manageMessageInput != null)
//                        {
//                            SetInputText(_manageMessageInput, requestedChatText);
//                            StartRoutine(FocusManagePopupInputRoutine());
//                        }

//                        FetchChat(false);
//                        return;
//                    }
//                }

//                CloseManagePanel();
//                SetStatus("Saved.");

//                if (mode == "chat")
//                    FetchChat(false);
//                else
//                    FetchPosts();
//            }));
//        }

//        private static void DeleteManagedContent()
//        {
//            if (_manageRequestRunning || _manageTargetId <= 0) return;
//            string mode = _manageMode;
//            string url = mode == "chat" ? CommunityUrl("/chat/" + _manageTargetId) : CommunityUrl("/posts/" + _manageTargetId);
//            if (mode != "chat" && mode != "post") return;
//            _manageRequestRunning = true;
//            StartRoutine(SendRequest("DELETE", url, null, delegate (bool ok, string body)
//            {
//                _manageRequestRunning = false;
//                if (!ok) { SetStatus("Delete failed: " + ErrorText(body)); return; }
//                CloseManagePanel();
//                SetStatus("Deleted.");
//                if (mode == "chat") FetchChat(); else { _selected = null; FetchPosts(); }
//            }));
//        }

//        private static IEnumerator ChatPollingLoop()
//        {
//            _chatPolling = true;
//            while (_chatPolling && _root != null)
//            {
//                bool editingPublicMessage =
//                    _editingChatMessageId > 0 ||
//                    (_managePanel != null &&
//                     _managePanel.activeSelf &&
//                     _manageMode == "chat");

//                if (_root.activeSelf && _showingChat && !editingPublicMessage)
//                    FetchChat();

//                yield return new WaitForSeconds(4f);
//            }
//            _chatPolling = false;
//        }

//        private static bool IsScrollNearBottom(ScrollRect scroll)
//        {
//            if (scroll == null || scroll.content == null || scroll.viewport == null)
//                return true;

//            try
//            {
//                Canvas.ForceUpdateCanvases();


//                if (scroll.content.rect.height <= scroll.viewport.rect.height + 1f)
//                    return true;


//                return scroll.verticalNormalizedPosition <= 0.08f;
//            }
//            catch
//            {
//                return true;
//            }
//        }

//        private static void NotifyIncomingPrivateMessage()
//        {
//            try
//            {
//                ChatController chat = null;

//                try
//                {
//                    chat = HudManager.Instance != null
//                        ? HudManager.Instance.Chat
//                        : null;
//                }
//                catch { }

//                if (chat == null)
//                {
//                    try
//                    {
//                        var chats = Resources.FindObjectsOfTypeAll(
//                            Il2CppType.Of<ChatController>());

//                        if (chats != null)
//                        {
//                            for (int i = 0; i < chats.Length; i++)
//                            {
//                                ChatController candidate = chats[i] as ChatController;

//                                if (candidate == null)
//                                    continue;

//                                chat = candidate;
//                                break;
//                            }
//                        }
//                    }
//                    catch { }
//                }

//                if (chat != null &&
//                    chat.messageSound != null &&
//                    SoundManager.Instance != null)
//                {
//                    SoundManager.Instance.PlaySoundImmediate(
//                        chat.messageSound,
//                        false,
//                        1f,
//                        1f,
//                        SoundManager.Instance.SfxChannel
//                    );
//                }
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    BMLogger.Warn(
//                        $"[PrivateChat] NotifyIncomingPrivateMessage sound failed: {ex}",
//                        "PrivateChat");
//                }
//                catch { }
//            }

//            try
//            {
//                BanMod.FlashColor(
//                    new Color(0f, 0.45f, 1f, 0.30f),
//                    1.1f);
//            }
//            catch { }
//        }

//        private static string GetLocalCommunityNotificationName()
//        {
//            string name = (_communityDisplayName ?? "").Trim();
//            if (!string.IsNullOrWhiteSpace(name))
//                return name;

//            try
//            {
//                name = (PlayerPrefs.GetString(CommunityDisplayNameCacheKey, "") ?? "").Trim();
//                if (!string.IsNullOrWhiteSpace(name))
//                    return name;
//            }
//            catch { }



//            name = (_privateLoginUsername ?? "").Trim();
//            return name;
//        }

//        private static bool CommunityMessageMentionsName(string message, string displayName)
//        {
//            message = message ?? "";
//            displayName = (displayName ?? "").Trim();

//            if (message.Length == 0 || displayName.Length == 0)
//                return false;

//            string needle = "@" + displayName;
//            int start = 0;

//            while (start < message.Length)
//            {
//                int index = message.IndexOf(needle, start, StringComparison.OrdinalIgnoreCase);
//                if (index < 0)
//                    return false;

//                int after = index + needle.Length;

//                bool beforeOk =
//                    index == 0 ||
//                    char.IsWhiteSpace(message[index - 1]) ||
//                    "↪([{\"'".IndexOf(message[index - 1]) >= 0;

//                bool afterOk =
//                    after >= message.Length ||
//                    char.IsWhiteSpace(message[after]) ||
//                    ":,.;!?)]}\"'".IndexOf(message[after]) >= 0;

//                if (beforeOk && afterOk)
//                    return true;

//                start = index + 1;
//            }

//            return false;
//        }

//        private static void ProcessIncomingCommunityMentions(List<CommunityChatMessage> messages)
//        {
//            if (messages == null)
//                return;

//            int highestId = _communityMentionLastMessageId;
//            for (int i = 0; i < messages.Count; i++)
//            {
//                CommunityChatMessage message = messages[i];
//                if (message != null && message.Id > highestId)
//                    highestId = message.Id;
//            }


//            if (!_communityMentionInitialSyncDone)
//            {
//                _communityMentionInitialSyncDone = true;
//                _communityMentionLastMessageId = highestId;
//                return;
//            }

//            string localName = GetLocalCommunityNotificationName();
//            if (string.IsNullOrWhiteSpace(localName))
//            {
//                _communityMentionLastMessageId = Math.Max(_communityMentionLastMessageId, highestId);
//                return;
//            }

//            bool notify = false;

//            for (int i = 0; i < messages.Count; i++)
//            {
//                CommunityChatMessage message = messages[i];
//                if (message == null ||
//                    message.Id <= _communityMentionLastMessageId ||
//                    message.MyMessage)
//                    continue;



//                if (CommunityMessageMentionsName(message.Message, localName) &&
//                    !CommunityMentionNotifiedIds.Contains(message.Id))
//                {
//                    CommunityMentionNotifiedIds.Add(message.Id);
//                    notify = true;

//                }
//            }

//            _communityMentionLastMessageId = Math.Max(_communityMentionLastMessageId, highestId);


//            if (CommunityMentionNotifiedIds.Count > 512)
//            {
//                CommunityMentionNotifiedIds.Clear();
//            }

//            if (notify)
//            {


//                NotifyIncomingPrivateMessage();
//            }
//        }

//        private static void TickCommunityMentionNotifications()
//        {


//            if (_root != null && _root.activeSelf && _showingChat)
//                return;

//            float now = Time.unscaledTime;
//            if (_communityMentionPollRunning || now < _communityNextMentionPollTime)
//                return;

//            _communityNextMentionPollTime = now + CommunityMentionPollIntervalSeconds;
//            StartRuntimeRoutine(PollCommunityMentionNotificationsCoroutine());
//        }

//        private static IEnumerator PollCommunityMentionNotificationsCoroutine()
//        {
//            if (_communityMentionPollRunning)
//                yield break;

//            _communityMentionPollRunning = true;

//            bool ok = false;
//            string body = "";

//            yield return SendRequest(
//                "GET",
//                CommunityUrl("/chat?limit=20"),
//                null,
//                delegate (bool success, string response)
//                {
//                    ok = success;
//                    body = response ?? "";
//                });

//            if (!ok)
//            {
//                _communityNextMentionPollTime =
//                    Time.unscaledTime + _communityMentionFailureBackoff;

//                _communityMentionFailureBackoff =
//                    Mathf.Min(_communityMentionFailureBackoff * 2f, 60f);

//                _communityMentionPollRunning = false;
//                yield break;
//            }

//            _communityMentionFailureBackoff = 10f;

//            try
//            {
//                List<CommunityChatMessage> messages = ParseChatMessages(body);
//                ProcessIncomingCommunityMentions(messages);

//                int highest = HighestChatId(messages);
//                _latestKnownCommunityChatId =
//                    Math.Max(_latestKnownCommunityChatId, highest);

//                int seenChat = 0;
//                try
//                {
//                    seenChat = PlayerPrefs.GetInt(
//                        CommunityLastSeenChatKey,
//                        0
//                    );
//                }
//                catch { }

//                if (highest > seenChat)
//                    SetCommunityUnreadVisible(true);
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning(
//                        "[BANMOD Community] Mention poll parse failed: " +
//                        ex.Message
//                    );
//                }
//                catch { }
//            }

//            _communityMentionPollRunning = false;
//        }

//        private static void FetchChat()
//        {
//            FetchChat(false);
//        }

//        private static void FetchChat(bool forceScrollToBottom)
//        {
//            if (_requestRunning) return;
//            _requestRunning = true;
//            StartRoutine(SendRequest("GET", CommunityUrl("/chat?limit=120"), null, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok) { SetStatus("Chat load failed: " + ErrorText(body)); return; }
//                try
//                {



//                    bool keepAtBottom =
//                        forceScrollToBottom ||
//                        ChatMessages.Count == 0 ||
//                        IsScrollNearBottom(_chatScroll);

//                    ChatMessages.Clear();
//                    ChatMessages.AddRange(ParseChatMessages(body));

//                    ProcessIncomingCommunityMentions(ChatMessages);

//                    _latestKnownCommunityChatId = Math.Max(_latestKnownCommunityChatId, HighestChatId(ChatMessages));
//                    if (ChatMessages.Count == 0) _selectedChatIndex = -1;
//                    else if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count) _selectedChatIndex = ChatMessages.Count - 1;
//                    RenderChat(keepAtBottom);
//                    SetStatus("General Chat · " + ChatMessages.Count + " messages from the current 3-day window");
//                    MarkCommunityActivitySeen();
//                }
//                catch (Exception ex) { SetStatus("Invalid chat response: " + ex.Message); }
//            }));
//        }

//        private static void SubmitChat()
//        {
//            if (_requestRunning) return;
//            string message = (_chatInput == null ? "" : (_chatInput.text ?? "")).Trim();
//            if (message.Length == 0) { SetStatus("Write a message first."); return; }

//            message = TranslateOutgoingCommunityText(message, "Community Chat", CommunityChatCharacterLimit);

//            _requestRunning = true;
//            var payload = new Dictionary<string, string> { { "message", message } };
//            StartRoutine(SendRequest("POST", CommunityUrl("/chat"), payload, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok) { SetStatus("Send failed: " + ErrorText(body)); return; }
//                SetInputText(_chatInput, "");

//                FetchChat(true);
//            }));
//        }

//        private static List<CommunityChatMessage> ParseChatMessages(string json)
//        {
//            var result = new List<CommunityChatMessage>();
//            using (var doc = JsonDocument.Parse(json))
//            {
//                var root = doc.RootElement;
//                if (!root.TryGetProperty("messages", out var arr) || arr.ValueKind != JsonValueKind.Array) return result;
//                foreach (var item in arr.EnumerateArray())
//                {
//                    var m = new CommunityChatMessage();
//                    if (item.TryGetProperty("id", out var id)) m.Id = id.GetInt32();
//                    if (item.TryGetProperty("message", out var text)) m.Message = text.GetString() ?? "";
//                    if (item.TryGetProperty("created_at", out var created)) m.CreatedAt = created.GetInt64();
//                    if (item.TryGetProperty("updated_at", out var updated)) m.UpdatedAt = updated.GetInt64();
//                    if (item.TryGetProperty("my_message", out var mine)) m.MyMessage = mine.GetBoolean();
//                    if (item.TryGetProperty("author", out var author)) m.Author = author.GetString() ?? "BanModUser";
//                    if (m.Id > 0) result.Add(m);
//                }
//            }
//            return result;
//        }

//        private static int HighestChatId(List<CommunityChatMessage> messages)
//        {
//            int max = 0;
//            for (int i = 0; i < messages.Count; i++) if (messages[i] != null && messages[i].Id > max) max = messages[i].Id;
//            return max;
//        }

//        private static void RenderChat(bool scrollToBottom = false)
//        {
//            if (_chatContent == null) return;



//            float previousOffsetFromTop = 0f;
//            try
//            {
//                previousOffsetFromTop = Mathf.Max(0f, _chatContent.anchoredPosition.y);
//            }
//            catch { previousOffsetFromTop = 0f; }
//            for (int i = 0; i < ChatRowObjects.Count; i++)
//                if (ChatRowObjects[i] != null) UnityEngine.Object.Destroy(ChatRowObjects[i]);
//            ChatRowObjects.Clear();

//            if (ChatMessages.Count == 0)
//            {
//                TextMeshProUGUI empty = CreateLabel(_chatContent, "EmptyChat", "No messages yet. Start the conversation.", 17, TextAlignmentOptions.Center, new Color(0.76f, 0.80f, 0.90f, 1f), new Vector2(0f, -70f), new Vector2(900f, 60f));
//                RectTransform er = empty.GetComponent<RectTransform>();
//                er.anchorMin = er.anchorMax = new Vector2(0.5f, 1f); er.pivot = new Vector2(0.5f, 1f);
//                ChatRowObjects.Add(empty.gameObject);
//                _chatContent.sizeDelta = new Vector2(0f, 410f);
//            }
//            else
//            {
//                float top = 8f;
//                for (int i = 0; i < ChatMessages.Count; i++)
//                {
//                    int capturedIndex = i;
//                    CommunityChatMessage m = ChatMessages[i];
//                    bool selected = i == _selectedChatIndex;
//                    string author = string.IsNullOrWhiteSpace(m.Author) ? "BanModUser" : m.Author;
//                    string edited = m.UpdatedAt > m.CreatedAt ? " · edited" : "";

//                    if (!m.MyMessage && LiveTranslator.GetEnabled())
//                        CacheAutomaticCommunityTranslation(CommunityChatTranslations, m.Id, m.Message, "Community Chat");

//                    string displayedMessage = GetCommunityDisplayedText(CommunityChatTranslations, m.Id, m.Message);
//                    string rowTextRaw = author + (m.MyMessage ? " (you)" : "") + " · " + FormatTimestamp(m.CreatedAt) + edited + "\n" + displayedMessage;
//                    string rowText = BanModSymbolPicker.ToSafeRichText(rowTextRaw);

//                    float rowHeight = 70f;
//                    Button row = CreateButton(_chatContent, "ChatRow_" + m.Id, rowText, Vector2.zero, new Vector2(1010f, rowHeight), selected ? new Color(0.23f, 0.31f, 0.50f, 1f) : new Color(0.12f, 0.14f, 0.20f, 1f), delegate
//                    {
//                        _selectedChatIndex = capturedIndex;
//                        RenderChat(false);
//                    });

//                    TextMeshProUGUI label = row.GetComponentInChildren<TextMeshProUGUI>(true);
//                    if (label != null)
//                    {
//                        label.richText = true;
//                        label.alignment = TextAlignmentOptions.TopLeft;
//                        label.enableWordWrapping = true;



//                        label.enableAutoSizing = false;
//                        label.fontSize = 15f;
//                        label.overflowMode = TextOverflowModes.Overflow;

//                        RectTransform lr = label.GetComponent<RectTransform>();
//                        if (lr != null)
//                        {
//                            lr.offsetMin = new Vector2(14f, 10f);
//                            lr.offsetMax = new Vector2(-14f, -10f);
//                        }

//                        try
//                        {


//                            label.ForceMeshUpdate();
//                            float preferred = label.GetPreferredValues(rowText, 980f, 100000f).y;

//                            if (float.IsNaN(preferred) || float.IsInfinity(preferred) || preferred < 1f)
//                                preferred = 50f;

//                            rowHeight = Mathf.Max(70f, preferred + 28f);
//                        }
//                        catch
//                        {

//                            int explicitLines = 1;
//                            string rawText = rowTextRaw ?? "";
//                            for (int c = 0; c < rawText.Length; c++)
//                                if (rawText[c] == '\n') explicitLines++;

//                            rowHeight = Mathf.Max(70f, 28f + explicitLines * 24f);
//                        }
//                    }

//                    RectTransform rr = row.GetComponent<RectTransform>();
//                    rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 1f);
//                    rr.pivot = new Vector2(0.5f, 1f);
//                    rr.sizeDelta = new Vector2(1010f, rowHeight);
//                    rr.anchoredPosition = new Vector2(-28f, -top);
//                    ChatRowObjects.Add(row.gameObject);

//                    if (m.MyMessage)
//                    {
//                        CommunityChatMessage captured = m;
//                        Button more = CreateButton(_chatContent, "ChatMore_" + m.Id, "...", Vector2.zero, new Vector2(58f, rowHeight), new Color(0.22f, 0.23f, 0.30f, 1f), delegate
//                        {
//                            OpenManageChat(captured);
//                        });
//                        RectTransform mr = more.GetComponent<RectTransform>();
//                        mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 1f);
//                        mr.pivot = new Vector2(0.5f, 1f);
//                        mr.sizeDelta = new Vector2(58f, rowHeight);
//                        mr.anchoredPosition = new Vector2(520f, -top);
//                        ChatRowObjects.Add(more.gameObject);
//                    }

//                    top += rowHeight + 12f;
//                }

//                _chatContent.sizeDelta = new Vector2(0f, Math.Max(410f, top + 24f));
//            }

//            if (_reportChatButton != null)
//            {
//                bool canReport = _selectedChatIndex >= 0 && _selectedChatIndex < ChatMessages.Count && !ChatMessages[_selectedChatIndex].MyMessage;
//                _reportChatButton.interactable = canReport && !_requestRunning;
//            }

//            if (_chatTranslateButton != null)
//            {
//                bool canTranslate = _selectedChatIndex >= 0 && _selectedChatIndex < ChatMessages.Count;
//                _chatTranslateButton.interactable = canTranslate && !_requestRunning;
//            }

//            bool hasSelectedMessage = _selectedChatIndex >= 0 && _selectedChatIndex < ChatMessages.Count;

//            if (_chatTagButton != null)
//                _chatTagButton.interactable =
//                    _editingChatMessageId <= 0 &&
//                    hasSelectedMessage &&
//                    !_requestRunning;

//            if (_chatReplyButton != null)
//                _chatReplyButton.interactable =
//                    _editingChatMessageId <= 0 &&
//                    hasSelectedMessage &&
//                    !_requestRunning;

//            try
//            {
//                Canvas.ForceUpdateCanvases();

//                if (_chatScroll != null && _chatScroll.viewport != null && _chatScroll.content != null)
//                {
//                    _chatScroll.StopMovement();

//                    float viewportHeight = Mathf.Max(1f, _chatScroll.viewport.rect.height);
//                    float contentHeight = Mathf.Max(viewportHeight, _chatScroll.content.rect.height);
//                    float maxOffset = Mathf.Max(0f, contentHeight - viewportHeight);

//                    if (scrollToBottom)
//                    {
//                        _chatScroll.verticalNormalizedPosition = 0f;
//                    }
//                    else if (maxOffset <= 0.5f)
//                    {
//                        _chatScroll.verticalNormalizedPosition = 1f;
//                    }
//                    else
//                    {
//                        float restoredOffset = Mathf.Clamp(previousOffsetFromTop, 0f, maxOffset);
//                        _chatScroll.verticalNormalizedPosition = 1f - (restoredOffset / maxOffset);
//                    }

//                    Canvas.ForceUpdateCanvases();
//                }
//            }
//            catch { }
//        }

//        private static void SelectPreviousChatMessage()
//        {
//            if (ChatMessages.Count == 0) { _selectedChatIndex = -1; RenderChat(); return; }
//            if (_selectedChatIndex < 0) _selectedChatIndex = ChatMessages.Count - 1;
//            else _selectedChatIndex = Math.Max(0, _selectedChatIndex - 1);
//            RenderChat();
//        }

//        private static void SelectNextChatMessage()
//        {
//            if (ChatMessages.Count == 0) { _selectedChatIndex = -1; RenderChat(); return; }
//            if (_selectedChatIndex < 0) _selectedChatIndex = 0;
//            else _selectedChatIndex = Math.Min(ChatMessages.Count - 1, _selectedChatIndex + 1);
//            RenderChat();
//        }

//        private static void TagSelectedChatMessage()
//        {
//            if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count || _chatInput == null)
//            {
//                SetStatus("Select a message first.");
//                return;
//            }

//            CommunityChatMessage message = ChatMessages[_selectedChatIndex];
//            if (message == null)
//            {
//                SetStatus("Select a valid message first.");
//                return;
//            }

//            string author = string.IsNullOrWhiteSpace(message.Author) ? "BanModUser" : message.Author.Trim();
//            string current = _chatInput.text ?? "";
//            string separator = current.Length > 0 && !char.IsWhiteSpace(current[current.Length - 1]) ? " " : "";
//            string addition = separator + "@" + author + " ";

//            if (current.Length + addition.Length > CommunityChatCharacterLimit)
//            {
//                SetStatus("TAG would exceed the " + CommunityChatCharacterLimit + " character Community limit.");
//                return;
//            }

//            SetInputText(_chatInput, current + addition);
//            FocusChatInputAtEnd();
//        }

//        private static void ReplySelectedChatMessage()
//        {
//            if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count || _chatInput == null)
//            {
//                SetStatus("Select a message first.");
//                return;
//            }

//            CommunityChatMessage message = ChatMessages[_selectedChatIndex];
//            if (message == null)
//            {
//                SetStatus("Select a valid message first.");
//                return;
//            }

//            string author = string.IsNullOrWhiteSpace(message.Author) ? "BanModUser" : message.Author.Trim();
//            string preview = (message.Message ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
//            const int maxPreviewLength = 120;
//            if (preview.Length > maxPreviewLength)
//                preview = preview.Substring(0, maxPreviewLength) + "...";

//            string prefix = "↪ @" + author + ": \"" + preview + "\"\n";
//            string current = _chatInput.text ?? "";
//            int availableForCurrentText = CommunityChatCharacterLimit - prefix.Length;

//            if (availableForCurrentText < 0)
//            {
//                SetStatus("The selected message is too long to quote.");
//                return;
//            }

//            if (current.Length > availableForCurrentText)
//                current = current.Substring(0, availableForCurrentText);

//            SetInputText(_chatInput, prefix + current);
//            FocusChatInputAtEnd();
//        }

//        private static void FocusChatInputAtEnd()
//        {
//            if (_chatInput == null) return;
//            try
//            {
//                _chatInput.ActivateInputField();
//                int end = (_chatInput.text ?? "").Length;
//                _chatInput.caretPosition = end;
//                _chatInput.selectionAnchorPosition = end;
//                _chatInput.selectionFocusPosition = end;
//            }
//            catch { }
//        }

//        private static void ReportSelectedChatMessage()
//        {
//            if (_requestRunning || _selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count) return;
//            var message = ChatMessages[_selectedChatIndex];
//            if (message == null || message.MyMessage) { SetStatus("You cannot report your own message."); return; }
//            _requestRunning = true;
//            if (_reportChatButton != null) _reportChatButton.interactable = false;
//            int messageId = message.Id;
//            var payload = new Dictionary<string, string> { { "reason", "inappropriate" } };
//            StartRoutine(SendRequest("POST", CommunityUrl("/chat/" + messageId + "/report"), payload, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                SetStatus(ok ? "Message reported to moderators." : "Report failed: " + ErrorText(body));
//                RenderChat();
//            }));
//        }

//        private static void FetchPosts()
//        {
//            if (_requestRunning) return;
//            _requestRunning = true;
//            SetStatus("Loading...");
//            StartRoutine(SendRequest("GET", CommunityUrl("/posts?kind=suggestion&limit=100&sort=latest"), null, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok) { SetStatus("Load failed: " + ErrorText(body)); return; }
//                try
//                {
//                    Posts.Clear();
//                    Posts.AddRange(ParsePosts(body));
//                    _latestKnownCommunityPostId = Math.Max(_latestKnownCommunityPostId, HighestPostId(Posts));
//                    _selected = Posts.Count > 0 ? Posts[0] : null;
//                    RenderList();
//                    if (_selected != null) FetchReplies(_selected.Id); else { Replies.Clear(); RenderSelected(); }
//                    SetStatus(Posts.Count + " suggestions");
//                    MarkCommunityActivitySeen();
//                }
//                catch (Exception ex) { SetStatus("Invalid server response: " + ex.Message); }
//            }));
//        }

//        private static void FetchReplies(int postId)
//        {
//            Replies.Clear();
//            RenderSelected();
//            StartRoutine(SendRequest("GET", CommunityUrl("/posts/" + postId + "/replies"), null, delegate (bool ok, string body)
//            {
//                if (_selected == null || _selected.Id != postId) return;
//                if (!ok) { SetStatus("Reply load failed: " + ErrorText(body)); return; }
//                try
//                {
//                    Replies.Clear();
//                    Replies.AddRange(ParseReplies(body));
//                    for (int i = 0; i < Replies.Count; i++) _latestKnownCommunityReplyId = Math.Max(_latestKnownCommunityReplyId, Replies[i].Id);
//                    _selectedReplyIndex = -1;
//                    RenderSelected();
//                    MarkCommunityActivitySeen();
//                }
//                catch (Exception ex) { SetStatus("Invalid replies response: " + ex.Message); }
//            }));
//        }

//        private static void SelectPost(CommunityPost post)
//        {
//            _selected = post;
//            Replies.Clear();
//            _selectedReplyIndex = -1;
//            SetInputText(_replyInput, "");
//            RenderList();
//            RenderSelected();
//            if (post != null) FetchReplies(post.Id);
//        }

//        private static void SubmitPost()
//        {
//            if (_requestRunning) return;
//            string title = (_titleInput == null ? "" : (_titleInput.text ?? "")).Trim();
//            string message = (_messageInput == null ? "" : (_messageInput.text ?? "")).Trim();
//            if (title.Length == 0 || message.Length == 0) { SetStatus("Title and message are required."); return; }

//            title = TranslateOutgoingCommunityText(title, "Suggestion title", 120);
//            message = TranslateOutgoingCommunityText(message, "Suggestion", 4000);

//            var payload = new Dictionary<string, string> { { "kind", "suggestion" }, { "title", title }, { "message", message } };
//            _requestRunning = true;
//            StartRoutine(SendRequest("POST", CommunityUrl("/posts"), payload, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok) { SetStatus("Publish failed: " + ErrorText(body)); return; }
//                if (_composePanel != null) _composePanel.SetActive(false);
//                SetStatus("Suggestion published.");
//                FetchPosts();
//            }));
//        }

//        private static void SubmitReply()
//        {
//            if (_selected == null || _requestRunning) return;
//            string message = (_replyInput == null ? "" : (_replyInput.text ?? "")).Trim();
//            if (message.Length == 0) { SetStatus("Write a reply first."); return; }

//            message = TranslateOutgoingCommunityText(message, "Suggestion reply", 2500);

//            int postId = _selected.Id;
//            var payload = new Dictionary<string, string> { { "message", message } };
//            _requestRunning = true;
//            StartRoutine(SendRequest("POST", CommunityUrl("/posts/" + postId + "/replies"), payload, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok) { SetStatus("Reply failed: " + ErrorText(body)); return; }
//                SetInputText(_replyInput, "");
//                SetStatus("Reply published.");
//                FetchReplies(postId);
//                FetchPosts();
//            }));
//        }

//        private static void ToggleStar()
//        {
//            if (_selected == null || _selected.MyPost || _requestRunning) return;
//            int id = _selected.Id;
//            _requestRunning = true;
//            StartRoutine(SendRequest("POST", CommunityUrl("/posts/" + id + "/star"), new Dictionary<string, string>(), delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                if (!ok) { SetStatus("Star failed: " + ErrorText(body)); return; }
//                try
//                {
//                    using (var doc = JsonDocument.Parse(body))
//                    {
//                        var root = doc.RootElement;
//                        _selected.MyStar = root.TryGetProperty("starred", out var s) && s.GetBoolean();
//                        if (root.TryGetProperty("stars", out var n)) _selected.Stars = n.GetInt32();
//                    }
//                    RenderList(); RenderSelected();
//                }
//                catch { }
//            }));
//        }

//        private static void RenderList()
//        {
//            for (int i = 0; i < ListButtons.Count; i++)
//                if (ListButtons[i] != null) UnityEngine.Object.Destroy(ListButtons[i]);
//            ListButtons.Clear();
//            if (_suggestionsListContent == null) return;

//            ClearRectChildren(_suggestionsListContent);

//            if (Posts.Count == 0)
//            {
//                TextMeshProUGUI empty = CreateLabel(_suggestionsListContent, "NoSuggestions", "No suggestions yet.", 15,
//                    TextAlignmentOptions.Center, new Color(0.68f, 0.72f, 0.80f, 1f), Vector2.zero, new Vector2(420f, 50f));
//                RectTransform emptyRect = empty != null ? empty.GetComponent<RectTransform>() : null;
//                if (emptyRect != null)
//                {
//                    emptyRect.anchorMin = emptyRect.anchorMax = new Vector2(0.5f, 1f);
//                    emptyRect.pivot = new Vector2(0.5f, 1f);
//                    emptyRect.anchoredPosition = new Vector2(0f, -24f);
//                }
//                _suggestionsListContent.sizeDelta = new Vector2(0f, 500f);
//                return;
//            }

//            float top = 8f;
//            for (int i = 0; i < Posts.Count; i++)
//            {
//                CommunityPost post = Posts[i];
//                if (post == null) continue;
//                string resolved = post.Status == "resolved" ? " ✓" : "";
//                string author = string.IsNullOrWhiteSpace(post.Author) ? "BanModUser" : post.Author;
//                string text = author + " · " + Short(post.Title, 30) + resolved + "   ★" + post.Stars + "  💬" + post.ReplyCount;
//                CommunityPost captured = post;
//                float rowHeight = 68f;
//                float width = post.MyPost ? 370f : 430f;
//                float x = post.MyPost ? -28f : 0f;

//                Button b = CreateButton(_suggestionsListContent, "Post_" + post.Id, text, Vector2.zero,
//                    new Vector2(width, rowHeight), post == _selected
//                        ? new Color(0.24f, 0.31f, 0.52f, 1f)
//                        : new Color(0.13f, 0.15f, 0.22f, 1f),
//                    delegate { SelectPost(captured); });
//                RectTransform br = b != null ? b.GetComponent<RectTransform>() : null;
//                if (br != null)
//                {
//                    br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
//                    br.pivot = new Vector2(0.5f, 1f);
//                    br.anchoredPosition = new Vector2(x, -top);
//                }
//                TextMeshProUGUI label = b != null ? b.GetComponentInChildren<TextMeshProUGUI>(true) : null;
//                if (label != null)
//                {
//                    label.fontSize = 14f;
//                    label.enableAutoSizing = true;
//                    label.fontSizeMin = 10f;
//                    label.fontSizeMax = 14f;
//                }
//                if (b != null) ListButtons.Add(b.gameObject);

//                if (post.MyPost)
//                {
//                    Button more = CreateButton(_suggestionsListContent, "PostMore_" + post.Id, "...", Vector2.zero,
//                        new Vector2(56f, rowHeight), new Color(0.22f, 0.23f, 0.30f, 1f), delegate { OpenManagePost(captured); });
//                    RectTransform mr = more != null ? more.GetComponent<RectTransform>() : null;
//                    if (mr != null)
//                    {
//                        mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 1f);
//                        mr.pivot = new Vector2(0.5f, 1f);
//                        mr.anchoredPosition = new Vector2(190f, -top);
//                    }
//                    if (more != null) ListButtons.Add(more.gameObject);
//                }

//                top += rowHeight + 10f;
//            }

//            _suggestionsListContent.sizeDelta = new Vector2(0f, Math.Max(500f, top + 8f));
//            try
//            {
//                Canvas.ForceUpdateCanvases();
//                if (_suggestionsListScroll != null)
//                    _suggestionsListScroll.verticalNormalizedPosition = 1f;
//            }
//            catch { }
//        }

//        private static void RenderSelected()
//        {
//            if (_detailText == null) return;
//            if (_selected == null)
//            {
//                SetDetailText("No public posts yet.");
//                if (_starButton != null) _starButton.interactable = false;
//                if (_replyInput != null) _replyInput.interactable = false;
//                if (_reportReplyButton != null) _reportReplyButton.interactable = false;
//                if (_suggestionTranslateButton != null) _suggestionTranslateButton.interactable = false;
//                return;
//            }

//            if (_replyInput != null) _replyInput.interactable = true;

//            if (!_selected.MyPost && LiveTranslator.GetEnabled())
//            {
//                CacheAutomaticCommunityTranslation(SuggestionTitleTranslations, _selected.Id, _selected.Title, "Suggestion title");
//                CacheAutomaticCommunityTranslation(SuggestionMessageTranslations, _selected.Id, _selected.Message, "Suggestion");
//            }

//            string displayedTitle = GetCommunityDisplayedText(SuggestionTitleTranslations, _selected.Id, _selected.Title);
//            string displayedMessage = GetCommunityDisplayedText(SuggestionMessageTranslations, _selected.Id, _selected.Message);

//            string kind = _selected.Kind.ToUpperInvariant();
//            string state = _selected.Status == "resolved" ? " · RESOLVED" : "";
//            var sb = new System.Text.StringBuilder();
//            sb.Append(kind).Append(state).Append("\n").Append(displayedTitle).Append("\n\n").Append(displayedMessage);
//            sb.Append("\n\n").Append(string.IsNullOrWhiteSpace(_selected.Author) ? "BanModUser" : _selected.Author).Append(_selected.MyPost ? " (you)" : "").Append(" · ").Append(FormatTimestamp(_selected.CreatedAt)).Append(" · ★ ").Append(_selected.Stars).Append(" · 💬 ").Append(_selected.ReplyCount);
//            sb.Append("\n\n────────────────────────\nCOMMENTS\n");
//            if (Replies.Count == 0) sb.Append("\nNo comments yet. You can be the first to reply.");

//            for (int i = 0; i < Replies.Count; i++)
//            {
//                var r = Replies[i];

//                if (r != null && !r.MyReply && LiveTranslator.GetEnabled())
//                    CacheAutomaticCommunityTranslation(SuggestionReplyTranslations, r.Id, r.Message, "Suggestion comment");

//                string displayedReply = r == null ? "" : GetCommunityDisplayedText(SuggestionReplyTranslations, r.Id, r.Message);
//                bool selectedReply = i == _selectedReplyIndex;
//                if (selectedReply) sb.Append("\n▶ ");
//                else sb.Append("\n");
//                sb.Append("#").Append(i + 1).Append(" · ").Append(string.IsNullOrWhiteSpace(r.Author) ? "BanModUser" : r.Author).Append(r.MyReply ? " (you)" : "").Append(" · ").Append(FormatTimestamp(r.CreatedAt));
//                if (selectedReply) sb.Append(" · SELECTED");
//                sb.Append("\n").Append(displayedReply).Append("\n");
//            }

//            SetDetailText(sb.ToString());
//            bool canAct = !_selected.MyPost;
//            if (_starButton != null) { _starButton.interactable = canAct; SetButtonText(_starButton, (_selected.MyStar ? "★ STARRED" : "☆ STAR") + " (" + _selected.Stars + ")"); }
//            if (_reportReplyButton != null)
//            {
//                bool canReport = _selectedReplyIndex >= 0 && _selectedReplyIndex < Replies.Count && !Replies[_selectedReplyIndex].MyReply;
//                _reportReplyButton.interactable = canReport && !_requestRunning;
//            }
//            if (_suggestionTranslateButton != null)
//            {
//                bool commentSelected = _selectedReplyIndex >= 0 && _selectedReplyIndex < Replies.Count;
//                SetButtonText(_suggestionTranslateButton, commentSelected ? "TRANSLATE COMMENT" : "TRANSLATE POST");
//                _suggestionTranslateButton.interactable = !_requestRunning;
//            }
//        }

//        private static void SelectPreviousReply()
//        {
//            if (Replies.Count == 0) { _selectedReplyIndex = -1; RenderSelected(); return; }
//            if (_selectedReplyIndex < 0) _selectedReplyIndex = Replies.Count - 1;
//            else _selectedReplyIndex = Math.Max(0, _selectedReplyIndex - 1);
//            RenderSelected();
//        }

//        private static void SelectNextReply()
//        {
//            if (Replies.Count == 0) { _selectedReplyIndex = -1; RenderSelected(); return; }
//            if (_selectedReplyIndex < 0) _selectedReplyIndex = 0;
//            else _selectedReplyIndex = Math.Min(Replies.Count - 1, _selectedReplyIndex + 1);
//            RenderSelected();
//        }

//        private static void ReportSelectedReply()
//        {
//            if (_requestRunning || _selectedReplyIndex < 0 || _selectedReplyIndex >= Replies.Count) return;
//            var reply = Replies[_selectedReplyIndex];
//            if (reply == null || reply.MyReply) { SetStatus("You cannot report your own comment."); return; }
//            _requestRunning = true;
//            if (_reportReplyButton != null) _reportReplyButton.interactable = false;
//            int replyId = reply.Id;
//            var payload = new Dictionary<string, string> { { "reason", "inappropriate" } };
//            StartRoutine(SendRequest("POST", CommunityUrl("/replies/" + replyId + "/report"), payload, delegate (bool ok, string body)
//            {
//                _requestRunning = false;
//                SetStatus(ok ? "Comment reported to moderators." : "Report failed: " + ErrorText(body));
//                RenderSelected();
//            }));
//        }

//        private static void SetDetailText(string value)
//        {
//            if (_detailText == null) return;
//            _detailText.text = value ?? "";
//            try
//            {
//                _detailText.ForceMeshUpdate();
//                float h = Math.Max(300f, _detailText.preferredHeight + 30f);
//                if (_detailContent != null) _detailContent.sizeDelta = new Vector2(_detailContent.sizeDelta.x, h);
//                if (_detailScroll != null) _detailScroll.verticalNormalizedPosition = 1f;
//            }
//            catch { }
//        }


//        internal static void RuntimeTick()
//        {
//            try
//            {
//                EnsureLauncherUi();
//                RefreshLauncherVisibility();
//                HandleLauncherDrag();
//                HandleUnifiedScrollInput();

//                if (!_backgroundProfileRequested)
//                {
//                    string friendCode = "";

//                    try
//                    {
//                        friendCode = (BanModIdentity.GetFriendCode() ?? "").Trim();
//                    }
//                    catch { }

//                    if (friendCode.Length >= 3)
//                    {
//                        _backgroundProfileRequested = true;
//                        FetchProfile();
//                    }
//                }

//                TickCommunityMentionNotifications();
//                TickPrivateChat();
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning("[BANMOD Community] Runtime tick failed: " + ex.Message);
//                }
//                catch { }
//            }
//        }

//        private static void BindSubmitAction(TMP_InputField input, Action action)
//        {
//            if (input == null || action == null)
//                return;






//            try
//            {
//                input.onSubmit.AddListener((UnityAction<string>)delegate (string _)
//                {
//                    bool isCommunityChatInput = _chatInput != null && input == _chatInput;
//                    bool ctrlPressed = false;

//                    if (isCommunityChatInput)
//                    {
//                        try
//                        {
//                            ctrlPressed =
//                                Input.GetKey(KeyCode.LeftControl) ||
//                                Input.GetKey(KeyCode.RightControl);
//                        }
//                        catch { ctrlPressed = false; }
//                    }

//                    if (isCommunityChatInput && ctrlPressed)
//                    {
//                        InsertNewLineAtCaret(input);
//                        return;
//                    }

//                    action();
//                });
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning("[BANMOD Community] Could not bind Enter submit for " +
//                        (input.gameObject != null ? input.gameObject.name : "input") + ": " + ex.Message);
//                }
//                catch { }
//            }
//        }

//        private static void InsertNewLineAtCaret(TMP_InputField input)
//        {
//            if (input == null)
//                return;

//            try
//            {
//                string current = input.text ?? "";

//                int anchor = Mathf.Clamp(input.selectionAnchorPosition, 0, current.Length);
//                int focus = Mathf.Clamp(input.selectionFocusPosition, 0, current.Length);
//                int selectionStart = Math.Min(anchor, focus);
//                int selectionEnd = Math.Max(anchor, focus);

//                string result =
//                    current.Substring(0, selectionStart) +
//                    "\n" +
//                    current.Substring(selectionEnd);

//                int limit = input.characterLimit;
//                if (limit > 0 && result.Length > limit)
//                {
//                    SetStatus("Community messages are limited to " + limit + " characters.");
//                    input.ActivateInputField();
//                    return;
//                }

//                SetInputText(input, result);

//                int newCaret = Math.Min(selectionStart + 1, result.Length);
//                input.caretPosition = newCaret;
//                input.selectionAnchorPosition = newCaret;
//                input.selectionFocusPosition = newCaret;
//                input.ActivateInputField();
//            }
//            catch (Exception ex)
//            {
//                try { Debug.LogWarning("[BANMOD Community] Could not insert Community newline: " + ex.Message); }
//                catch { }
//            }
//        }

//        private static void HandleUnifiedScrollInput()
//        {
//            if (_root == null || !_root.activeSelf)
//                return;

//            float wheel = Input.mouseScrollDelta.y;
//            if (Mathf.Abs(wheel) < 0.001f)
//            {
//                try { wheel = Input.GetAxis("Mouse ScrollWheel") * 10f; }
//                catch { wheel = 0f; }
//            }
//            if (Mathf.Abs(wheel) < 0.001f)
//                return;

//            if (_showingPrivateChat)
//            {
//                if (TryManualScroll(_privatePlayersScroll, wheel)) return;
//                TryManualScroll(_privateMessagesScroll, wheel);
//                return;
//            }

//            if (_showingBugReport)
//            {
//                if (TryManualScroll(_bugReportsScroll, wheel)) return;
//                TryManualScroll(_bugConversationScroll, wheel);
//                return;
//            }

//            if (_showingChat)
//            {
//                TryManualScroll(_chatScroll, wheel);
//                return;
//            }

//            if (TryManualScroll(_suggestionsListScroll, wheel)) return;
//            TryManualScroll(_detailScroll, wheel);
//        }

//        private static bool TryManualScroll(ScrollRect scroll, float wheel)
//        {
//            if (scroll == null || scroll.content == null || scroll.viewport == null || !scroll.gameObject.activeInHierarchy)
//                return false;

//            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
//            if (scrollRect == null)
//                return false;

//            bool inside = false;
//            try { inside = RectTransformUtility.RectangleContainsScreenPoint(scrollRect, Input.mousePosition, null); }
//            catch { }
//            if (!inside)
//                return false;

//            float viewportHeight = scroll.viewport.rect.height;
//            float contentHeight = scroll.content.rect.height;
//            if (contentHeight <= viewportHeight + 1f)
//                return true;

//            try
//            {
//                scroll.StopMovement();

//                float scrollableHeight = Mathf.Max(1f, contentHeight - viewportHeight);



//                const float pixelsPerWheelUnit = 52f;
//                float pixelDelta = wheel * pixelsPerWheelUnit;
//                float normalizedDelta = pixelDelta / scrollableHeight;

//                scroll.verticalNormalizedPosition =
//                    Mathf.Clamp01(scroll.verticalNormalizedPosition + normalizedDelta);

//                Canvas.ForceUpdateCanvases();
//            }
//            catch { }
//            return true;
//        }

//        private static void EnsureLauncherUi()
//        {
//            if (_launcherRoot != null) return;

//            try
//            {
//                EnsureEventSystem();

//                _launcherRoot = new GameObject("BanMod_CommunityLauncher");
//                UnityEngine.Object.DontDestroyOnLoad(_launcherRoot);
//                RectTransform rootRect = _launcherRoot.AddComponent<RectTransform>();
//                rootRect.anchorMin = Vector2.zero;
//                rootRect.anchorMax = Vector2.one;
//                rootRect.offsetMin = Vector2.zero;
//                rootRect.offsetMax = Vector2.zero;

//                Canvas canvas = _launcherRoot.AddComponent<Canvas>();
//                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//                canvas.overrideSorting = true;
//                canvas.sortingOrder = 29950;

//                CanvasScaler scaler = _launcherRoot.AddComponent<CanvasScaler>();
//                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//                scaler.referenceResolution = new Vector2(1920f, 1080f);
//                _launcherRoot.AddComponent<GraphicRaycaster>();

//                Vector2 pos = LoadLauncherPosition();
//                Button launcherButton = CreateButton(
//                    _launcherRoot.transform,
//                    "CommunityLauncherButton",
//                    "C",
//                    pos,
//                    new Vector2(58f, 42f),
//                    new Color(1f, 1f, 1f, 0.34f),
//                    OnLauncherClicked);
//                _launcherButton = launcherButton != null ? launcherButton.gameObject : null;

//                _launcherButtonRect = _launcherButton == null ? null : _launcherButton.GetComponent<RectTransform>();
//                if (_launcherButton != null)
//                {
//                    TextMeshProUGUI textLabel = _launcherButton.GetComponentInChildren<TextMeshProUGUI>(true);
//                    if (textLabel != null)
//                    {
//                        textLabel.fontSize = 19f;
//                        textLabel.fontStyle = FontStyles.Bold;
//                        textLabel.color = new Color(0f, 0f, 0f, 0.78f);
//                    }

//                    _launcherUnreadDot = CreateLabel(
//                        _launcherButton.transform,
//                        "UnreadDot",
//                        "●",
//                        22f,
//                        TextAlignmentOptions.Center,
//                        new Color(0.96f, 0.06f, 0.06f, 1f),
//                        new Vector2(24f, 15f),
//                        new Vector2(26f, 26f));
//                    if (_launcherUnreadDot != null)
//                        _launcherUnreadDot.gameObject.SetActive(false);
//                }

//                RefreshCombinedUnread();
//                RefreshLauncherVisibility();
//            }
//            catch (Exception ex)
//            {
//                Debug.LogWarning("[BANMOD Community] Launcher creation failed: " + ex.Message);
//            }
//        }

//        private static void RefreshLauncherVisibility()
//        {
//            if (_launcherRoot == null || _launcherButton == null) return;
//            try
//            {
//                bool communityOpen = _root != null && _root.activeSelf;
//                _launcherRoot.SetActive(true);
//                _launcherButton.SetActive(!communityOpen);
//                if (!communityOpen)
//                    _launcherButton.transform.SetAsLastSibling();
//            }
//            catch { }
//        }

//        private static void OnLauncherClicked()
//        {
//            if (Time.unscaledTime < _launcherSuppressClickUntil)
//                return;
//            OpenFromMainMenu();
//        }

//        private static Vector2 LoadLauncherPosition()
//        {
//            try
//            {
//                return new Vector2(
//                    PlayerPrefs.GetFloat(LauncherPosXKey, -920f),
//                    PlayerPrefs.GetFloat(LauncherPosYKey, 508f));
//            }
//            catch
//            {
//                return new Vector2(-920f, 508f);
//            }
//        }

//        private static void SaveLauncherPosition()
//        {
//            if (_launcherButtonRect == null) return;
//            try
//            {
//                PlayerPrefs.SetFloat(LauncherPosXKey, _launcherButtonRect.anchoredPosition.x);
//                PlayerPrefs.SetFloat(LauncherPosYKey, _launcherButtonRect.anchoredPosition.y);
//                PlayerPrefs.Save();
//            }
//            catch { }
//        }

//        private static void HandleLauncherDrag()
//        {
//            if (_launcherButtonRect == null || _launcherButton == null || !_launcherButton.activeSelf)
//            {
//                _launcherDragging = false;
//                return;
//            }

//            Vector2 mouse = Input.mousePosition;
//            try
//            {
//                if (Input.GetMouseButtonDown(0) &&
//                    RectTransformUtility.RectangleContainsScreenPoint(_launcherButtonRect, mouse, null))
//                {
//                    RectTransform canvasRect = _launcherRoot == null ? null : _launcherRoot.GetComponent<RectTransform>();
//                    if (canvasRect != null &&
//                        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouse, null, out Vector2 local))
//                    {
//                        _launcherDragging = true;
//                        _launcherMoved = false;
//                        _launcherDragStartMouse = mouse;
//                        _launcherDragOffset = _launcherButtonRect.anchoredPosition - local;
//                    }
//                }

//                if (_launcherDragging && Input.GetMouseButton(0))
//                {
//                    RectTransform canvasRect = _launcherRoot == null ? null : _launcherRoot.GetComponent<RectTransform>();
//                    if (canvasRect != null &&
//                        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouse, null, out Vector2 local))
//                    {
//                        Vector2 target = local + _launcherDragOffset;
//                        Vector2 half = _launcherButtonRect.sizeDelta * 0.5f;
//                        Rect bounds = canvasRect.rect;
//                        target.x = Mathf.Clamp(target.x, bounds.xMin + half.x + 6f, bounds.xMax - half.x - 6f);
//                        target.y = Mathf.Clamp(target.y, bounds.yMin + half.y + 6f, bounds.yMax - half.y - 6f);
//                        _launcherButtonRect.anchoredPosition = target;

//                        if ((mouse - _launcherDragStartMouse).sqrMagnitude > 36f)
//                            _launcherMoved = true;
//                    }
//                }

//                if (_launcherDragging && Input.GetMouseButtonUp(0))
//                {
//                    _launcherDragging = false;
//                    if (_launcherMoved)
//                    {
//                        _launcherSuppressClickUntil = Time.unscaledTime + 0.30f;
//                        SaveLauncherPosition();
//                    }
//                }
//            }
//            catch
//            {
//                _launcherDragging = false;
//            }
//        }

//        private static void TickPrivateChat()
//        {
//            float now = Time.unscaledTime;

//            if (string.IsNullOrWhiteSpace(_privateBanModToken))
//            {
//                if (!_privateSessionRequestRunning && now >= _privateNextSessionAttemptTime)
//                {
//                    if (StartRuntimeRoutine(EnsurePrivateChatSessionCoroutine()))
//                        _privateNextSessionAttemptTime = now + 5f;
//                    else
//                        _privateNextSessionAttemptTime = now + 1f;
//                }

//                return;
//            }

//            if (!_privatePresenceRequestRunning && now >= _privateNextPresenceTime)
//            {
//                if (StartRuntimeRoutine(SendPrivatePresenceCoroutine()))
//                    _privateNextPresenceTime = now + PrivateChatPresenceIntervalSeconds;
//                else
//                    _privateNextPresenceTime = now + 1f;
//            }

//            if (!_privatePlayersRequestRunning && now >= _privateNextPlayersPollTime)
//            {
//                if (StartRuntimeRoutine(PollPrivatePlayersCoroutine()))
//                    _privateNextPlayersPollTime = now + PrivateChatPlayersPollIntervalSeconds;
//                else
//                    _privateNextPlayersPollTime = now + 1f;
//            }

//            float messageInterval =
//                (_root != null && _root.activeSelf && _showingPrivateChat)
//                ? PrivateChatOpenPollIntervalSeconds
//                : PrivateChatClosedPollIntervalSeconds;

//            if (!_privateMessagesRequestRunning && now >= _privateNextMessagesPollTime)
//            {
//                if (StartRuntimeRoutine(PollPrivateMessagesCoroutine()))
//                    _privateNextMessagesPollTime = now + messageInterval;
//                else
//                    _privateNextMessagesPollTime = now + 1f;
//            }
//        }

//        private static void RequestPrivateChatImmediateRefresh()
//        {
//            _privateNextPlayersPollTime = 0f;
//            _privateNextMessagesPollTime = 0f;
//            _privateNextPresenceTime = 0f;
//        }

//        private static bool StartRuntimeRoutine(IEnumerator routine)
//        {
//            if (routine == null) return false;
//            try
//            {
//                if (AmongUsClient.Instance != null)
//                {
//                    AmongUsClient.Instance.StartCoroutine(routine.WrapToIl2Cpp());
//                    return true;
//                }
//            }
//            catch { }

//            try
//            {
//                if (_mainMenuOwner != null && _mainMenuOwner.gameObject != null)
//                {
//                    _mainMenuOwner.StartCoroutine(routine);
//                    return true;
//                }
//            }
//            catch { }

//            return false;
//        }

//        private static IEnumerator EnsurePrivateChatSessionCoroutine()
//        {
//            try
//            {
//                if (PlayerPrefs.HasKey("CHAT_INSTALLATION_ID"))
//                {
//                    PlayerPrefs.DeleteKey("CHAT_INSTALLATION_ID");
//                    PlayerPrefs.Save();
//                }
//            }
//            catch { }

//            if (_privateSessionRequestRunning || !string.IsNullOrWhiteSpace(_privateBanModToken))
//                yield break;

//            string friendCode = "";
//            try { friendCode = BanModIdentity.GetFriendCode() ?? ""; } catch { }
//            friendCode = friendCode.Trim();

//            if (friendCode.Length < 3)
//            {
//                _privateNextSessionAttemptTime = Time.unscaledTime + 5f;
//                yield break;
//            }

//            bool tokenReady = false;
//            string clientToken = "";
//            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) =>
//            {
//                tokenReady = success;
//                clientToken = token ?? "";
//            });

//            if (!tokenReady || string.IsNullOrWhiteSpace(clientToken))
//            {
//                _privateNextSessionAttemptTime = Time.unscaledTime + 5f;
//                yield break;
//            }

//            _privateSessionRequestRunning = true;
//            string json = "{" +
//                "\"friend_code\":" + PrivateJsonString(friendCode) + "," +
//                "\"platform\":" + PrivateJsonString(Application.platform.ToString()) +
//                "}";

//            UnityWebRequest request = new UnityWebRequest(CommunityApiBase.TrimEnd('/') + "/api/chat/session", "POST");
//            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
//            request.downloadHandler = new DownloadHandlerBuffer();
//            request.timeout = 25;
//            request.SetRequestHeader("Content-Type", "application/json");
//            BanModApiTokenManager.ApplyAuthHeader(request);
//            yield return request.SendWebRequest();

//            string body = request.downloadHandler != null ? request.downloadHandler.text : "";
//            if (request.responseCode >= 200 && request.responseCode < 300)
//            {
//                try
//                {
//                    using (JsonDocument doc = JsonDocument.Parse(body))
//                    {
//                        JsonElement root = doc.RootElement;
//                        string loginUsername = PrivateReadString(root, "username");
//                        if (!string.IsNullOrWhiteSpace(loginUsername))
//                            _privateLoginUsername = loginUsername.Trim();
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogWarning("[BANMOD Community] Private Chat session parse failed: " + ex.Message);
//                }

//                _privateBanModToken = clientToken;
//                _privateChatSessionStartedAt = PrivateUnixNowSeconds();
//                _privateLastMessageId = 0;
//                _privateInitialSyncDone = false;

//                PrivateKnownMessageIds.Clear();
//                PrivateChatMessages.Clear();
//                PrivateUnreadMessageIds.Clear();
//                PrivatePendingDeliveryAckIds.Clear();
//                PrivateChatTranslations.Clear();

//                _privateNextPresenceTime = 0f;
//                _privateNextPlayersPollTime = 0f;
//                _privateNextMessagesPollTime = 0f;

//                RequestPrivateChatImmediateRefresh();
//                StartRuntimeRoutine(SendPrivatePresenceCoroutine());
//                StartRuntimeRoutine(PollPrivatePlayersCoroutine());
//                StartRuntimeRoutine(PollPrivateMessagesCoroutine());
//                RefreshCombinedUnread();
//                TryUseLoginUsernameAsDefault();
//            }
//            else
//            {
//                _privateNextSessionAttemptTime = Time.unscaledTime +
//                    ((request.responseCode == 403 || request.responseCode == 409) ? 15f : 5f);
//            }

//            request.Dispose();
//            _privateSessionRequestRunning = false;
//        }

//        private static void ClearPrivateChatSession()
//        {
//            _privateBanModToken = "";
//            _privateChatSessionStartedAt = 0;
//            _privateLastMessageId = 0;
//            _privateInitialSyncDone = false;
//            PrivatePendingDeliveryAckIds.Clear();
//            _privateAckRunning = false;
//            _privateNextSessionAttemptTime = 0f;
//        }

//        private static IEnumerator SendPrivatePresenceCoroutine()
//        {
//            if (_privatePresenceRequestRunning || string.IsNullOrWhiteSpace(_privateBanModToken))
//                yield break;

//            _privatePresenceRequestRunning = true;
//            string json = "{\"in_lobby\":" + (PrivateChatIsInLobbyOrGame() ? "true" : "false") + "}";
//            UnityWebRequest request = CreatePrivateChatRequest(
//                CommunityApiBase.TrimEnd('/') + "/api/chat/presence",
//                "POST",
//                json);

//            yield return request.SendWebRequest();
//            HandlePrivateChatAuthFailure(request);
//            request.Dispose();
//            _privatePresenceRequestRunning = false;
//        }

//        private static IEnumerator PollPrivatePlayersCoroutine()
//        {
//            if (_privatePlayersRequestRunning || string.IsNullOrWhiteSpace(_privateBanModToken))
//                yield break;

//            _privatePlayersRequestRunning = true;
//            UnityWebRequest request = CreatePrivateChatRequest(
//                CommunityApiBase.TrimEnd('/') + "/api/chat/online",
//                "GET",
//                null);

//            yield return request.SendWebRequest();
//            if (!HandlePrivateChatAuthFailure(request) &&
//                request.responseCode >= 200 && request.responseCode < 300)
//            {
//                string body = request.downloadHandler != null ? request.downloadHandler.text : "";
//                try
//                {
//                    List<PrivateChatPlayer> parsed = ParsePrivateChatPlayers(body);
//                    PrivateChatPlayers.Clear();
//                    PrivateChatPlayers.AddRange(parsed);
//                    RenderPrivatePlayers();
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogWarning("[BANMOD Community] Private players parse failed: " + ex.Message);
//                }
//            }

//            request.Dispose();
//            _privatePlayersRequestRunning = false;
//        }

//        private static IEnumerator PollPrivateMessagesCoroutine()
//        {
//            if (_privateMessagesRequestRunning || string.IsNullOrWhiteSpace(_privateBanModToken))
//                yield break;

//            _privateMessagesRequestRunning = true;

//            long now = PrivateUnixNowSeconds();
//            long privateSince = Math.Max(0L, _privateChatSessionStartedAt);
//            string url = CommunityApiBase.TrimEnd('/') + "/api/chat/messages" +
//                "?general_since=" + now +
//                "&private_since=" + privateSince +
//                "&after_id=" + Math.Max(0L, _privateLastMessageId);

//            UnityWebRequest request = CreatePrivateChatRequest(url, "GET", null);
//            yield return request.SendWebRequest();

//            if (!HandlePrivateChatAuthFailure(request) &&
//                request.responseCode >= 200 && request.responseCode < 300)
//            {
//                string body = request.downloadHandler != null ? request.downloadHandler.text : "";
//                try
//                {
//                    List<PrivateChatMessage> parsed = ParsePrivateChatMessages(body);
//                    string ownFriendCode = "";
//                    try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

//                    bool shouldNotify = _privateInitialSyncDone;
//                    bool playIncomingNotification = false;


//                    bool keepPrivateAtBottom =
//                        PrivateChatMessages.Count == 0 ||
//                        IsScrollNearBottom(_privateMessagesScroll);

//                    for (int i = 0; i < parsed.Count; i++)
//                    {
//                        PrivateChatMessage message = parsed[i];
//                        if (message == null || message.Id <= 0) continue;

//                        if (message.Id > _privateLastMessageId)
//                            _privateLastMessageId = message.Id;

//                        if (!message.IsPrivate)
//                            continue;

//                        bool fromMe = string.Equals(message.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
//                        bool toMe = string.Equals(message.RecipientFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
//                        if (!fromMe && !toMe)
//                            continue;

//                        bool queuedIncoming = toMe && !fromMe && message.PendingDelivery;
//                        if (queuedIncoming)
//                            PrivatePendingDeliveryAckIds.Add(message.Id);

//                        if (PrivateKnownMessageIds.Contains(message.Id))
//                            continue;

//                        PrivateKnownMessageIds.Add(message.Id);
//                        PrivateChatMessages.Add(message);

//                        if (!fromMe && LiveTranslator.GetEnabled())
//                            CacheAutomaticPrivateTranslation(message);



//                        if (!fromMe && (shouldNotify || queuedIncoming))
//                        {


//                            playIncomingNotification = true;

//                            bool conversationVisible =
//                                _root != null && _root.activeSelf && _showingPrivateChat &&
//                                string.Equals(_privateActiveFriendCode, message.SenderFriendCode, StringComparison.OrdinalIgnoreCase);

//                            if (!conversationVisible)
//                                PrivateUnreadMessageIds.Add(message.Id);
//                        }
//                    }

//                    PrivateChatMessages.Sort(delegate (PrivateChatMessage a, PrivateChatMessage b)
//                    {
//                        return a.Id.CompareTo(b.Id);
//                    });

//                    _privateInitialSyncDone = true;

//                    if (playIncomingNotification)
//                        NotifyIncomingPrivateMessage();

//                    if (_showingPrivateChat && _root != null && _root.activeSelf)
//                    {
//                        MarkActivePrivateConversationRead();
//                        RenderPrivateMessages(keepPrivateAtBottom);
//                        RenderPrivatePlayers();
//                    }
//                    RefreshCombinedUnread();
//                    StartPendingPrivateDeliveryAck();
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogWarning("[BANMOD Community] Private messages parse failed: " + ex.Message);
//                }
//            }

//            request.Dispose();
//            _privateMessagesRequestRunning = false;
//        }

//        private static void StartPendingPrivateDeliveryAck()
//        {
//            if (_privateAckRunning ||
//                PrivatePendingDeliveryAckIds.Count == 0 ||
//                string.IsNullOrWhiteSpace(_privateBanModToken))
//            {
//                return;
//            }

//            _privateAckRunning = true;
//            if (!StartRuntimeRoutine(AckPendingPrivateMessagesCoroutine()))
//                _privateAckRunning = false;
//        }

//        private static IEnumerator AckPendingPrivateMessagesCoroutine()
//        {
//            List<long> ids = new List<long>(PrivatePendingDeliveryAckIds);
//            if (ids.Count == 0)
//            {
//                _privateAckRunning = false;
//                yield break;
//            }

//            StringBuilder json = new StringBuilder();
//            json.Append("{\"message_ids\":[");
//            for (int i = 0; i < ids.Count; i++)
//            {
//                if (i > 0) json.Append(',');
//                json.Append(ids[i]);
//            }
//            json.Append("]}");

//            UnityWebRequest request = CreatePrivateChatRequest(
//                CommunityApiBase.TrimEnd('/') + "/api/chat/messages/ack",
//                "POST",
//                json.ToString());

//            yield return request.SendWebRequest();

//            bool authFailed = HandlePrivateChatAuthFailure(request);
//            if (!authFailed && request.responseCode >= 200 && request.responseCode < 300)
//            {
//                for (int i = 0; i < ids.Count; i++)
//                    PrivatePendingDeliveryAckIds.Remove(ids[i]);
//            }

//            request.Dispose();
//            _privateAckRunning = false;
//        }

//        private static void SelectPrivatePlayer(string friendCode, string playerName)
//        {
//            friendCode = (friendCode ?? "").Trim();
//            if (friendCode.Length == 0) return;

//            _privateActiveFriendCode = friendCode;
//            _privateActiveName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
//            _selectedPrivateMessageIndex = -1;
//            if (_privateChatInput != null)
//            {
//                _privateChatInput.interactable = true;
//                if (_privateChatInput.placeholder is TextMeshProUGUI placeholder)
//                    placeholder.text = "Message " + _privateActiveName + "...";
//            }
//            if (_privateConversationTitle != null)
//                _privateConversationTitle.text = "PRIVATE CHAT — " + _privateActiveName;

//            MarkActivePrivateConversationRead();
//            RenderPrivatePlayers();
//            RenderPrivateMessages(true);
//            SetStatus("Private Chat with " + _privateActiveName);
//        }

//        private static void MarkActivePrivateConversationRead()
//        {
//            if (string.IsNullOrWhiteSpace(_privateActiveFriendCode))
//                return;

//            string ownFriendCode = "";
//            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

//            for (int i = PrivateChatMessages.Count - 1; i >= 0; i--)
//            {
//                PrivateChatMessage message = PrivateChatMessages[i];
//                if (message == null) continue;

//                bool inboundFromActive =
//                    string.Equals(message.SenderFriendCode, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase) &&
//                    string.Equals(message.RecipientFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);

//                if (inboundFromActive)
//                    PrivateUnreadMessageIds.Remove(message.Id);
//            }

//            RefreshCombinedUnread();
//        }

//        private static int PrivateUnreadCountForPlayer(string friendCode)
//        {
//            int count = 0;
//            if (string.IsNullOrWhiteSpace(friendCode)) return 0;

//            for (int i = 0; i < PrivateChatMessages.Count; i++)
//            {
//                PrivateChatMessage message = PrivateChatMessages[i];
//                if (message == null || !PrivateUnreadMessageIds.Contains(message.Id)) continue;
//                if (string.Equals(message.SenderFriendCode, friendCode, StringComparison.OrdinalIgnoreCase))
//                    count++;
//            }
//            return count;
//        }

//        private static void RenderPrivatePlayers()
//        {
//            if (_privatePlayersContent == null) return;

//            ClearRectChildren(_privatePlayersContent);

//            List<PrivateChatPlayer> online = new List<PrivateChatPlayer>();
//            string ownFriendCode = "";
//            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

//            for (int i = 0; i < PrivateChatPlayers.Count; i++)
//            {
//                PrivateChatPlayer p = PrivateChatPlayers[i];
//                if (p == null || !p.IsOnline) continue;
//                if (string.Equals(p.FriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase)) continue;
//                online.Add(p);
//            }

//            online.Sort(delegate (PrivateChatPlayer a, PrivateChatPlayer b)
//            {
//                int unreadA = PrivateUnreadCountForPlayer(a == null ? "" : a.FriendCode);
//                int unreadB = PrivateUnreadCountForPlayer(b == null ? "" : b.FriendCode);
//                if ((unreadA > 0) != (unreadB > 0))
//                    return unreadA > 0 ? -1 : 1;
//                return string.Compare(a == null ? "" : a.PlayerName, b == null ? "" : b.PlayerName, StringComparison.OrdinalIgnoreCase);
//            });

//            float top = 8f;
//            if (online.Count == 0)
//            {
//                CreateLabel(_privatePlayersContent, "NoOnline", "No users online.", 14, TextAlignmentOptions.Center,
//                    new Color(0.68f, 0.72f, 0.80f, 1f), new Vector2(0f, -30f), new Vector2(240f, 40f));
//                _privatePlayersContent.sizeDelta = new Vector2(0f, 440f);
//                return;
//            }

//            for (int i = 0; i < online.Count; i++)
//            {
//                PrivateChatPlayer player = online[i];
//                string fc = player.FriendCode;
//                string name = string.IsNullOrWhiteSpace(player.PlayerName) ? "Player" : player.PlayerName;
//                int unread = PrivateUnreadCountForPlayer(fc);
//                string label = (unread > 0 ? "● " : "") + name;
//                if (unread > 1) label += "  (" + unread + ")";

//                bool selected = string.Equals(fc, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase);
//                Button row = CreateButton(
//                    _privatePlayersContent,
//                    "PrivatePlayer_" + i,
//                    label,
//                    new Vector2(0f, -(top + 22f)),
//                    new Vector2(240f, 44f),
//                    selected ? new Color(0.10f, 0.34f, 0.48f, 0.98f) : new Color(0.12f, 0.24f, 0.18f, 0.95f),
//                    delegate { SelectPrivatePlayer(fc, name); });

//                RectTransform playerRowRect = row == null ? null : row.GetComponent<RectTransform>();
//                if (playerRowRect != null)
//                {
//                    playerRowRect.anchorMin = playerRowRect.anchorMax = new Vector2(0.5f, 1f);
//                    playerRowRect.pivot = new Vector2(0.5f, 1f);
//                    playerRowRect.anchoredPosition = new Vector2(0f, -top);
//                }
//                TextMeshProUGUI labelText = row == null ? null : row.GetComponentInChildren<TextMeshProUGUI>(true);
//                if (labelText != null)
//                {
//                    labelText.alignment = TextAlignmentOptions.MidlineLeft;
//                    labelText.margin = new Vector4(12f, 0f, 8f, 0f);
//                }
//                top += 48f;
//            }

//            _privatePlayersContent.sizeDelta = new Vector2(0f, Math.Max(440f, top + 10f));
//        }

//        private static void RenderPrivateMessages(bool scrollToBottom = false)
//        {
//            if (_privateMessagesContent == null) return;

//            float previousScroll = 0f;
//            try
//            {
//                if (_privateMessagesScroll != null)
//                    previousScroll = _privateMessagesScroll.verticalNormalizedPosition;
//            }
//            catch { }

//            ClearRectChildren(_privateMessagesContent);

//            if (string.IsNullOrWhiteSpace(_privateActiveFriendCode))
//            {
//                CreateLabel(_privateMessagesContent, "SelectPlayerHint", "Select an online user from the list.", 16,
//                    TextAlignmentOptions.Center, new Color(0.72f, 0.76f, 0.86f, 1f),
//                    new Vector2(0f, -40f), new Vector2(680f, 50f));
//                _privateMessagesContent.sizeDelta = new Vector2(0f, 340f);
//                if (_privateTranslateButton != null) _privateTranslateButton.interactable = false;
//                if (_privateReportButton != null) _privateReportButton.interactable = false;
//                return;
//            }

//            string ownFriendCode = "";
//            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

//            float top = 8f;
//            int visibleIndex = 0;

//            for (int i = 0; i < PrivateChatMessages.Count; i++)
//            {
//                PrivateChatMessage message = PrivateChatMessages[i];
//                if (message == null || !message.IsPrivate) continue;

//                bool fromActiveToMe =
//                    string.Equals(message.SenderFriendCode, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase) &&
//                    string.Equals(message.RecipientFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
//                bool fromMeToActive =
//                    string.Equals(message.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase) &&
//                    string.Equals(message.RecipientFriendCode, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase);

//                if (!fromActiveToMe && !fromMeToActive)
//                    continue;

//                bool own = fromMeToActive;
//                string sender = own
//                    ? (string.IsNullOrWhiteSpace(_communityDisplayName) ? "You" : _communityDisplayName)
//                    : (string.IsNullOrWhiteSpace(message.SenderName) ? _privateActiveName : message.SenderName);




//                if (fromActiveToMe && LiveTranslator.GetEnabled())
//                    CacheAutomaticPrivateTranslation(message);

//                string displayedMessage = GetPrivateDisplayedMessage(message);
//                string lineRaw = sender + "  ·  " + FormatTimestamp(message.CreatedAt) + "\n" + displayedMessage;
//                string line = BanModSymbolPicker.ToSafeRichText(lineRaw);
//                int originalIndex = i;
//                bool selected = originalIndex == _selectedPrivateMessageIndex;

//                Button row = CreateButton(
//                    _privateMessagesContent,
//                    "PrivateMessage_" + visibleIndex,
//                    line,
//                    new Vector2(0f, -(top + 30f)),
//                    new Vector2(700f, 60f),
//                    selected
//                        ? new Color(0.25f, 0.30f, 0.44f, 0.98f)
//                        : (own ? new Color(0.10f, 0.24f, 0.34f, 0.95f) : new Color(0.16f, 0.16f, 0.18f, 0.95f)),
//                    delegate { SelectPrivateMessage(originalIndex); });

//                float rowHeight = 60f;
//                TextMeshProUGUI label = row == null ? null : row.GetComponentInChildren<TextMeshProUGUI>(true);
//                if (label != null)
//                {
//                    label.richText = true;
//                    label.alignment = TextAlignmentOptions.TopLeft;
//                    label.margin = new Vector4(10f, 7f, 10f, 7f);
//                    label.enableWordWrapping = true;
//                    label.overflowMode = TextOverflowModes.Overflow;
//                    try
//                    {
//                        float preferred = label.GetPreferredValues(line, 680f, 0f).y;
//                        rowHeight = Mathf.Clamp(preferred + 18f, 60f, 320f);
//                    }
//                    catch { }
//                }

//                RectTransform rowRect = row == null ? null : row.GetComponent<RectTransform>();
//                if (rowRect != null)
//                {
//                    rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 1f);
//                    rowRect.pivot = new Vector2(0.5f, 1f);
//                    rowRect.sizeDelta = new Vector2(700f, rowHeight);
//                    rowRect.anchoredPosition = new Vector2(0f, -top);
//                }

//                top += rowHeight + 6f;
//                visibleIndex++;
//            }

//            if (visibleIndex == 0)
//            {
//                CreateLabel(_privateMessagesContent, "NoMessages", "No private messages in this session yet.", 15,
//                    TextAlignmentOptions.Center, new Color(0.68f, 0.72f, 0.80f, 1f),
//                    new Vector2(0f, -40f), new Vector2(680f, 50f));
//                top = 330f;
//            }

//            _privateMessagesContent.sizeDelta = new Vector2(0f, Math.Max(340f, top + 12f));

//            bool hasSelectedMessage =
//                _selectedPrivateMessageIndex >= 0 &&
//                _selectedPrivateMessageIndex < PrivateChatMessages.Count &&
//                PrivateChatMessages[_selectedPrivateMessageIndex] != null;

//            if (_privateTranslateButton != null)
//                _privateTranslateButton.interactable = hasSelectedMessage;

//            if (_privateReportButton != null)
//            {
//                bool canReport = false;
//                if (_selectedPrivateMessageIndex >= 0 && _selectedPrivateMessageIndex < PrivateChatMessages.Count)
//                {
//                    PrivateChatMessage selected = PrivateChatMessages[_selectedPrivateMessageIndex];
//                    canReport = selected != null &&
//                        !string.Equals(selected.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
//                }
//                _privateReportButton.interactable = canReport;
//            }

//            if (_privateMessagesScroll != null)
//            {
//                try
//                {
//                    Canvas.ForceUpdateCanvases();
//                    _privateMessagesScroll.StopMovement();
//                    _privateMessagesScroll.verticalNormalizedPosition =
//                        scrollToBottom ? 0f : previousScroll;
//                }
//                catch { }
//            }
//        }

//        private static void SelectPrivateMessage(int index)
//        {
//            if (index < 0 || index >= PrivateChatMessages.Count) return;
//            _selectedPrivateMessageIndex = index;
//            RenderPrivateMessages(false);
//        }

//        private static void CacheAutomaticCommunityTranslation(
//            Dictionary<int, PrivateChatTranslationEntry> cache,
//            int id,
//            string original,
//            string context)
//        {
//            if (cache == null ||
//                id <= 0 ||
//                string.IsNullOrWhiteSpace(original) ||
//                !LiveTranslator.GetEnabled())
//            {
//                return;
//            }

//            string settingsKey = GetPrivateTranslationSettingsKey();

//            PrivateChatTranslationEntry existing;
//            if (cache.TryGetValue(id, out existing) &&
//                existing != null &&
//                existing.Attempted &&
//                string.Equals(existing.SettingsKey, settingsKey, StringComparison.OrdinalIgnoreCase) &&
//                string.Equals(existing.Original, original, StringComparison.Ordinal))
//            {
//                return;
//            }

//            PrivateChatTranslationEntry entry = new PrivateChatTranslationEntry
//            {
//                SettingsKey = settingsKey,
//                Original = original ?? "",
//                Translation = "",
//                Attempted = true,
//                Manual = false
//            };
//            cache[id] = entry;

//            try
//            {
//                string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
//                    original,
//                    delegate (string part) { return LiveTranslator.TranslateExternalIncomingForRead(part, false); });
//                if (!string.IsNullOrWhiteSpace(translated) &&
//                    !string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
//                {
//                    entry.Translation = translated;
//                }
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning("[BANMOD Community] " + context + " automatic translation failed: " + ex.Message);
//                }
//                catch { }
//            }
//        }

//        private static string GetCommunityDisplayedText(
//            Dictionary<int, PrivateChatTranslationEntry> cache,
//            int id,
//            string original)
//        {
//            original = original ?? "";
//            if (cache == null || id <= 0)
//                return original;

//            PrivateChatTranslationEntry entry;
//            if (!cache.TryGetValue(id, out entry) ||
//                entry == null ||
//                string.IsNullOrWhiteSpace(entry.Translation) ||
//                !string.Equals(entry.SettingsKey, GetPrivateTranslationSettingsKey(), StringComparison.OrdinalIgnoreCase) ||
//                !string.Equals(entry.Original, original, StringComparison.Ordinal))
//            {
//                return original;
//            }

//            if (string.Equals(entry.Translation, original, StringComparison.OrdinalIgnoreCase))
//                return original;

//            if (!LiveTranslator.GetEnabled() && !entry.Manual)
//                return original;

//            if (!LiveTranslator.GetShowOriginalIncoming())
//                return entry.Translation;

//            return original + "\n────────────\n[TR] " + entry.Translation;
//        }

//        private static bool TryCacheManualCommunityTranslation(
//            Dictionary<int, PrivateChatTranslationEntry> cache,
//            int id,
//            string original,
//            string context)
//        {
//            if (cache == null || id <= 0 || string.IsNullOrWhiteSpace(original))
//                return false;

//            string translated = "";
//            try
//            {
//                translated = BanModSymbolPicker.TranslatePreservingPickerColors(
//                    original,
//                    delegate (string part) { return LiveTranslator.TranslateExternalSelectedForRead(part); });
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning("[BANMOD Community] Manual " + context + " translation failed: " + ex.Message);
//                }
//                catch { }
//            }

//            if (string.IsNullOrWhiteSpace(translated) ||
//                string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
//            {
//                return false;
//            }

//            cache[id] = new PrivateChatTranslationEntry
//            {
//                SettingsKey = GetPrivateTranslationSettingsKey(),
//                Original = original,
//                Translation = translated,
//                Attempted = true,
//                Manual = true
//            };
//            return true;
//        }

//        private static string TranslateOutgoingCommunityText(string value, string context, int maxLength)
//        {
//            string result = (value ?? "").Trim();
//            if (result.Length == 0)
//                return result;

//            if (LiveTranslator.GetEnabled())
//            {
//                try
//                {
//                    string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
//                        result,
//                        delegate (string part) { return LiveTranslator.TranslateExternalOutgoingForSend(part); });
//                    if (!string.IsNullOrWhiteSpace(translated))
//                        result = translated.Trim();
//                }
//                catch (Exception ex)
//                {
//                    try
//                    {
//                        Debug.LogWarning("[BANMOD Community] " + context + " outgoing translation failed: " + ex.Message);
//                    }
//                    catch { }
//                }
//            }

//            if (maxLength > 0 && result.Length > maxLength)
//                result = result.Substring(0, maxLength);

//            return result;
//        }

//        private static void TranslateSelectedCommunityChatMessage()
//        {
//            if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count)
//            {
//                SetStatus("Select a Community Chat message first.");
//                return;
//            }

//            CommunityChatMessage message = ChatMessages[_selectedChatIndex];
//            if (message == null || string.IsNullOrWhiteSpace(message.Message))
//            {
//                SetStatus("The selected Community Chat message is empty.");
//                return;
//            }

//            if (!TryCacheManualCommunityTranslation(
//                    CommunityChatTranslations,
//                    message.Id,
//                    message.Message,
//                    "Community Chat"))
//            {
//                SetStatus("No translation available, or the message is already in the selected language.");
//                return;
//            }

//            RenderChat(false);
//            SetStatus("Selected Community Chat message translated.");
//        }

//        private static void TranslateSelectedSuggestionContent()
//        {
//            if (_selected == null || _selected.Id <= 0)
//            {
//                SetStatus("Select a suggestion first.");
//                return;
//            }

//            if (_selectedReplyIndex >= 0 && _selectedReplyIndex < Replies.Count)
//            {
//                CommunityReply reply = Replies[_selectedReplyIndex];
//                if (reply == null || string.IsNullOrWhiteSpace(reply.Message))
//                {
//                    SetStatus("The selected comment is empty.");
//                    return;
//                }

//                if (!TryCacheManualCommunityTranslation(
//                        SuggestionReplyTranslations,
//                        reply.Id,
//                        reply.Message,
//                        "suggestion comment"))
//                {
//                    SetStatus("No translation available, or the comment is already in the selected language.");
//                    return;
//                }

//                RenderSelected();
//                SetStatus("Selected suggestion comment translated.");
//                return;
//            }

//            bool translatedAnything = false;
//            if (!string.IsNullOrWhiteSpace(_selected.Title))
//            {
//                translatedAnything |= TryCacheManualCommunityTranslation(
//                    SuggestionTitleTranslations,
//                    _selected.Id,
//                    _selected.Title,
//                    "suggestion title");
//            }

//            if (!string.IsNullOrWhiteSpace(_selected.Message))
//            {
//                translatedAnything |= TryCacheManualCommunityTranslation(
//                    SuggestionMessageTranslations,
//                    _selected.Id,
//                    _selected.Message,
//                    "suggestion");
//            }

//            if (!translatedAnything)
//            {
//                SetStatus("No translation available, or the suggestion is already in the selected language.");
//                return;
//            }

//            RenderSelected();
//            SetStatus("Selected suggestion translated.");
//        }

//        private static string GetPrivateTranslationSettingsKey()
//        {
//            try
//            {
//                return
//                    (LiveTranslator.GetReadFromLang() ?? "auto").Trim().ToLowerInvariant() + "->" +
//                    (LiveTranslator.GetReadToLang() ?? "among").Trim().ToLowerInvariant() +
//                    "|among=" + (LiveTranslator.GetAmongUserLangCode() ?? "en").Trim().ToLowerInvariant() +
//                    "|provider=" + (LiveTranslator.GetProvider() ?? "").Trim().ToLowerInvariant();
//            }
//            catch
//            {
//                return "auto->among|among=en";
//            }
//        }

//        private static void CacheAutomaticPrivateTranslation(PrivateChatMessage message)
//        {
//            if (message == null ||
//                message.Id <= 0 ||
//                string.IsNullOrWhiteSpace(message.Message) ||
//                !LiveTranslator.GetEnabled())
//            {
//                return;
//            }

//            string settingsKey = GetPrivateTranslationSettingsKey();
//            string original = message.Message ?? "";

//            PrivateChatTranslationEntry existing;
//            if (PrivateChatTranslations.TryGetValue(message.Id, out existing) &&
//                existing != null &&
//                existing.Attempted &&
//                string.Equals(existing.SettingsKey, settingsKey, StringComparison.OrdinalIgnoreCase) &&
//                string.Equals(existing.Original, original, StringComparison.Ordinal))
//            {
//                return;
//            }



//            PrivateChatTranslationEntry entry = new PrivateChatTranslationEntry
//            {
//                SettingsKey = settingsKey,
//                Original = original,
//                Translation = "",
//                Attempted = true,
//                Manual = false
//            };
//            PrivateChatTranslations[message.Id] = entry;

//            try
//            {
//                string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
//                    original,
//                    delegate (string part) { return LiveTranslator.TranslateExternalIncomingForRead(part, false); });
//                if (!string.IsNullOrWhiteSpace(translated) &&
//                    !string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
//                {
//                    entry.Translation = translated;
//                }
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning(
//                        "[BANMOD Community] Private automatic translation failed: " + ex.Message);
//                }
//                catch { }
//            }
//        }

//        private static string GetPrivateDisplayedMessage(PrivateChatMessage message)
//        {
//            if (message == null)
//                return "";

//            string original = message.Message ?? "";
//            if (message.Id <= 0)
//                return original;

//            PrivateChatTranslationEntry entry;
//            if (!PrivateChatTranslations.TryGetValue(message.Id, out entry) ||
//                entry == null ||
//                string.IsNullOrWhiteSpace(entry.Translation) ||
//                !string.Equals(entry.SettingsKey, GetPrivateTranslationSettingsKey(), StringComparison.OrdinalIgnoreCase) ||
//                !string.Equals(entry.Original, original, StringComparison.Ordinal))
//            {
//                return original;
//            }

//            if (string.Equals(entry.Translation, original, StringComparison.OrdinalIgnoreCase))
//                return original;



//            if (!LiveTranslator.GetEnabled() && !entry.Manual)
//                return original;

//            if (!LiveTranslator.GetShowOriginalIncoming())
//                return entry.Translation;

//            return original + "\n────────────\n[TR] " + entry.Translation;
//        }

//        private static void TranslateSelectedPrivateMessage()
//        {
//            if (_selectedPrivateMessageIndex < 0 ||
//                _selectedPrivateMessageIndex >= PrivateChatMessages.Count)
//            {
//                SetStatus("Select a private message first.");
//                return;
//            }

//            PrivateChatMessage message = PrivateChatMessages[_selectedPrivateMessageIndex];
//            if (message == null || string.IsNullOrWhiteSpace(message.Message))
//            {
//                SetStatus("The selected private message is empty.");
//                return;
//            }

//            string original = message.Message ?? "";
//            string translated = "";

//            try
//            {

//                translated = BanModSymbolPicker.TranslatePreservingPickerColors(
//                    original,
//                    delegate (string part) { return LiveTranslator.TranslateExternalSelectedForRead(part); });
//            }
//            catch (Exception ex)
//            {
//                try
//                {
//                    Debug.LogWarning(
//                        "[BANMOD Community] Manual private translation failed: " + ex.Message);
//                }
//                catch { }
//            }

//            if (string.IsNullOrWhiteSpace(translated) ||
//                string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
//            {
//                SetStatus("No translation available, or the message is already in the selected language.");
//                return;
//            }

//            PrivateChatTranslations[message.Id] = new PrivateChatTranslationEntry
//            {
//                SettingsKey = GetPrivateTranslationSettingsKey(),
//                Original = original,
//                Translation = translated,
//                Attempted = true,
//                Manual = true
//            };

//            RenderPrivateMessages(false);
//            SetStatus("Selected private message translated.");
//        }

//        private static void SubmitPrivateChatMessage()
//        {
//            if (_privateSendRunning) return;
//            if (string.IsNullOrWhiteSpace(_privateActiveFriendCode))
//            {
//                SetStatus("Select an online user first.");
//                return;
//            }

//            string message = (_privateChatInput == null ? "" : (_privateChatInput.text ?? "")).Trim();
//            if (message.Length == 0)
//            {
//                SetStatus("Write a private message first.");
//                return;
//            }



//            if (LiveTranslator.GetEnabled())
//            {
//                try
//                {
//                    string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
//                        message,
//                        delegate (string part) { return LiveTranslator.TranslateExternalOutgoingForSend(part); });
//                    if (!string.IsNullOrWhiteSpace(translated))
//                        message = translated;
//                }
//                catch (Exception ex)
//                {
//                    try
//                    {
//                        Debug.LogWarning(
//                            "[BANMOD Community] Private outgoing translation failed: " + ex.Message);
//                    }
//                    catch { }
//                }
//            }



//            if (message.Length > 300)
//                message = message.Substring(0, 300);

//            if (!StartRuntimeRoutine(SendPrivateChatMessageCoroutine(message, _privateActiveFriendCode)))
//                SetStatus("Private Chat runtime is unavailable.");
//        }

//        private static IEnumerator SendPrivateChatMessageCoroutine(string message, string recipientFriendCode)
//        {
//            if (_privateSendRunning) yield break;
//            _privateSendRunning = true;

//            if (string.IsNullOrWhiteSpace(_privateBanModToken))
//            {
//                yield return EnsurePrivateChatSessionCoroutine();
//                if (string.IsNullOrWhiteSpace(_privateBanModToken))
//                {
//                    _privateSendRunning = false;
//                    SetStatus("Private Chat session is unavailable.");
//                    yield break;
//                }
//            }

//            string json = "{" +
//                "\"message\":" + PrivateJsonString(message) + "," +
//                "\"recipient_friend_code\":" + PrivateJsonString(recipientFriendCode) +
//                "}";

//            UnityWebRequest request = CreatePrivateChatRequest(
//                CommunityApiBase.TrimEnd('/') + "/api/chat/messages",
//                "POST",
//                json);

//            yield return request.SendWebRequest();
//            string body = request.downloadHandler != null ? request.downloadHandler.text : "";

//            if (HandlePrivateChatAuthFailure(request))
//            {
//                SetStatus("Private Chat session expired. Try again.");
//            }
//            else if (request.responseCode >= 200 && request.responseCode < 300)
//            {
//                if (_privateChatInput != null) _privateChatInput.text = "";
//                try
//                {
//                    using (JsonDocument doc = JsonDocument.Parse(body))
//                    {
//                        JsonElement root = doc.RootElement;
//                        if (root.TryGetProperty("message", out JsonElement msg) && msg.ValueKind == JsonValueKind.Object)
//                        {
//                            PrivateChatMessage parsed = ParsePrivateChatMessage(msg);
//                            if (parsed != null && parsed.Id > 0 && !PrivateKnownMessageIds.Contains(parsed.Id))
//                            {
//                                PrivateKnownMessageIds.Add(parsed.Id);
//                                PrivateChatMessages.Add(parsed);
//                                if (parsed.Id > _privateLastMessageId) _privateLastMessageId = parsed.Id;
//                            }
//                        }
//                    }
//                }
//                catch { }

//                PrivateChatMessages.Sort(delegate (PrivateChatMessage a, PrivateChatMessage b)
//                {
//                    return a.Id.CompareTo(b.Id);
//                });
//                RenderPrivateMessages(true);
//                SetStatus("Private message sent.");
//                _privateNextMessagesPollTime = 0f;
//                _privateNextPresenceTime = 0f;
//            }
//            else
//            {
//                SetStatus("Private send failed: " + PrivateChatError(body, "HTTP " + request.responseCode));
//            }

//            request.Dispose();
//            _privateSendRunning = false;
//        }

//        private static void ReportSelectedPrivateMessage()
//        {
//            if (_privateReportRunning) return;
//            if (_selectedPrivateMessageIndex < 0 || _selectedPrivateMessageIndex >= PrivateChatMessages.Count)
//            {
//                SetStatus("Select a private message first.");
//                return;
//            }

//            PrivateChatMessage message = PrivateChatMessages[_selectedPrivateMessageIndex];
//            string ownFriendCode = "";
//            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

//            if (message == null || string.Equals(message.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase))
//            {
//                SetStatus("You cannot report your own message.");
//                return;
//            }

//            StartRuntimeRoutine(ReportPrivateMessageCoroutine(message.Id));
//        }

//        private static IEnumerator ReportPrivateMessageCoroutine(long messageId)
//        {
//            if (_privateReportRunning || messageId <= 0) yield break;
//            _privateReportRunning = true;

//            string json = "{\"message_id\":" + messageId + ",\"reason\":\"inappropriate\"}";
//            UnityWebRequest request = CreatePrivateChatRequest(
//                CommunityApiBase.TrimEnd('/') + "/api/chat/report",
//                "POST",
//                json);

//            yield return request.SendWebRequest();
//            string body = request.downloadHandler != null ? request.downloadHandler.text : "";

//            if (HandlePrivateChatAuthFailure(request))
//                SetStatus("Private Chat session expired.");
//            else if (request.responseCode >= 200 && request.responseCode < 300)
//                SetStatus("Private message reported.");
//            else
//                SetStatus("Private report failed: " + PrivateChatError(body, "HTTP " + request.responseCode));

//            request.Dispose();
//            _privateReportRunning = false;
//        }

//        private static UnityWebRequest CreatePrivateChatRequest(string url, string method, string json)
//        {
//            UnityWebRequest request;
//            if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
//            {
//                request = UnityWebRequest.Get(url);
//                request.downloadHandler = new DownloadHandlerBuffer();
//            }
//            else
//            {
//                request = new UnityWebRequest(url, method);
//                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json ?? "{}"));
//                request.downloadHandler = new DownloadHandlerBuffer();
//                request.SetRequestHeader("Content-Type", "application/json");
//            }

//            request.timeout = 25;
//            BanModApiTokenManager.ApplyAuthHeader(request);
//            return request;
//        }

//        private static bool HandlePrivateChatAuthFailure(UnityWebRequest request)
//        {
//            if (request == null) return false;
//            if (request.responseCode == 401)
//            {
//                ClearPrivateChatSession();
//                return true;
//            }
//            if (request.responseCode == 403)
//            {
//                string body = request.downloadHandler != null ? request.downloadHandler.text : "";
//                SetStatus(PrivateChatError(body, "Private Chat access denied."));
//                return true;
//            }
//            return false;
//        }

//        private static List<PrivateChatPlayer> ParsePrivateChatPlayers(string json)
//        {
//            List<PrivateChatPlayer> players = new List<PrivateChatPlayer>();
//            if (string.IsNullOrWhiteSpace(json)) return players;

//            using (JsonDocument doc = JsonDocument.Parse(json))
//            {
//                JsonElement root = doc.RootElement;
//                if (!root.TryGetProperty("players", out JsonElement array) || array.ValueKind != JsonValueKind.Array)
//                    return players;

//                foreach (JsonElement element in array.EnumerateArray())
//                {
//                    if (element.ValueKind != JsonValueKind.Object) continue;
//                    long lastSeen = PrivateReadLong(element, "last_seen", 0L);
//                    bool inferredOnline = lastSeen > 0 && lastSeen >= PrivateUnixNowSeconds() - PrivateChatOnlineThresholdSeconds;
//                    players.Add(new PrivateChatPlayer
//                    {
//                        FriendCode = PrivateReadString(element, "friend_code"),
//                        PlayerName = PrivateReadString(element, "player_name"),
//                        IsOnline = PrivateReadBool(element, "online", inferredOnline),
//                        InLobby = PrivateReadBool(element, "in_lobby", false),
//                        LastSeen = lastSeen
//                    });
//                }
//            }

//            return players;
//        }

//        private static List<PrivateChatMessage> ParsePrivateChatMessages(string json)
//        {
//            List<PrivateChatMessage> messages = new List<PrivateChatMessage>();
//            if (string.IsNullOrWhiteSpace(json)) return messages;

//            using (JsonDocument doc = JsonDocument.Parse(json))
//            {
//                JsonElement root = doc.RootElement;
//                if (!root.TryGetProperty("messages", out JsonElement array) || array.ValueKind != JsonValueKind.Array)
//                    return messages;

//                foreach (JsonElement element in array.EnumerateArray())
//                {
//                    PrivateChatMessage message = ParsePrivateChatMessage(element);
//                    if (message != null) messages.Add(message);
//                }
//            }

//            return messages;
//        }

//        private static PrivateChatMessage ParsePrivateChatMessage(JsonElement element)
//        {
//            if (element.ValueKind != JsonValueKind.Object) return null;

//            string recipientFriendCode = PrivateReadString(element, "recipient_friend_code");
//            return new PrivateChatMessage
//            {
//                Id = PrivateReadLong(element, "id", 0L),
//                SenderFriendCode = PrivateReadString(element, "sender_friend_code"),
//                SenderName = PrivateReadString(element, "sender_name"),
//                RecipientFriendCode = recipientFriendCode,
//                RecipientName = PrivateReadString(element, "recipient_name"),
//                IsPrivate = PrivateReadBool(element, "is_private", !string.IsNullOrWhiteSpace(recipientFriendCode)),
//                PendingDelivery = PrivateReadBool(element, "pending_delivery", false),
//                Message = PrivateReadString(element, "message"),
//                CreatedAt = PrivateReadLong(element, "created_at", 0L)
//            };
//        }

//        private static string PrivateChatError(string json, string fallback)
//        {
//            if (string.IsNullOrWhiteSpace(json)) return fallback;
//            try
//            {
//                using (JsonDocument doc = JsonDocument.Parse(json))
//                {
//                    JsonElement root = doc.RootElement;
//                    string error = PrivateReadString(root, "error");
//                    string message = PrivateReadString(root, "message");
//                    string reason = PrivateReadString(root, "reason");
//                    string result = !string.IsNullOrWhiteSpace(error)
//                        ? error
//                        : (!string.IsNullOrWhiteSpace(message) ? message : fallback);
//                    if (!string.IsNullOrWhiteSpace(reason) &&
//                        result.IndexOf(reason, StringComparison.OrdinalIgnoreCase) < 0)
//                        result += " Reason: " + reason + ".";
//                    return result;
//                }
//            }
//            catch
//            {
//                return fallback;
//            }
//        }

//        private static string PrivateReadString(JsonElement root, string name)
//        {
//            if (root.ValueKind == JsonValueKind.Object &&
//                root.TryGetProperty(name, out JsonElement value) &&
//                value.ValueKind == JsonValueKind.String)
//                return value.GetString() ?? "";
//            return "";
//        }

//        private static long PrivateReadLong(JsonElement root, string name, long fallback)
//        {
//            if (root.ValueKind != JsonValueKind.Object ||
//                !root.TryGetProperty(name, out JsonElement value))
//                return fallback;

//            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number))
//                return number;
//            if (value.ValueKind == JsonValueKind.String &&
//                long.TryParse(value.GetString(), out long parsed))
//                return parsed;
//            return fallback;
//        }

//        private static bool PrivateReadBool(JsonElement root, string name, bool fallback)
//        {
//            if (root.ValueKind != JsonValueKind.Object ||
//                !root.TryGetProperty(name, out JsonElement value))
//                return fallback;
//            if (value.ValueKind == JsonValueKind.True) return true;
//            if (value.ValueKind == JsonValueKind.False) return false;
//            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
//                return number != 0;
//            if (value.ValueKind == JsonValueKind.String &&
//                bool.TryParse(value.GetString(), out bool parsed))
//                return parsed;
//            return fallback;
//        }

//        private static string PrivateJsonString(string value)
//        {
//            value = value ?? "";
//            return "\"" + value
//                .Replace("\\", "\\\\")
//                .Replace("\"", "\\\"")
//                .Replace("\n", "\\n")
//                .Replace("\r", "\\r")
//                .Replace("\t", "\\t") + "\"";
//        }

//        private static long PrivateUnixNowSeconds()
//        {
//            try { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
//            catch { return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }
//        }

//        private static bool PrivateChatIsInLobbyOrGame()
//        {
//            try
//            {
//                if (AmongUsClient.Instance == null) return false;
//                string state = AmongUsClient.Instance.GameState.ToString();
//                return state.IndexOf("Joined", StringComparison.OrdinalIgnoreCase) >= 0 ||
//                       state.IndexOf("Started", StringComparison.OrdinalIgnoreCase) >= 0;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private static void ClearRectChildren(RectTransform content)
//        {
//            if (content == null) return;
//            try
//            {
//                for (int i = content.childCount - 1; i >= 0; i--)
//                {
//                    Transform child = content.GetChild(i);
//                    if (child != null)
//                        UnityEngine.Object.Destroy(child.gameObject);
//                }
//            }
//            catch { }
//        }

//        private static void StartRoutine(IEnumerator routine)
//        {
//            if (routine == null) return;
//            if (_mainMenuOwner != null && _mainMenuOwner.gameObject != null && _mainMenuOwner.gameObject.activeInHierarchy)
//            {
//                _mainMenuOwner.StartCoroutine(routine);
//                return;
//            }

//            try
//            {
//                if (AmongUsClient.Instance != null)
//                {
//                    AmongUsClient.Instance.StartCoroutine(routine.WrapToIl2Cpp());
//                    return;
//                }
//            }
//            catch { }

//            Debug.LogError("[BANMOD Community] Coroutine host unavailable.");
//        }

//        private static UnityWebRequest CreateRequest(string method, string url, string json)
//        {
//            try
//            {
//                UnityWebRequest request;
//                if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
//                {
//                    request = UnityWebRequest.Get(url);
//                    request.downloadHandler = new DownloadHandlerBuffer();
//                }
//                else
//                {
//                    request = new UnityWebRequest(url, method);
//                    if (json != null)
//                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
//                    request.downloadHandler = new DownloadHandlerBuffer();
//                    request.SetRequestHeader("Content-Type", "application/json");
//                }

//                request.timeout = 30;
//                request.SetRequestHeader("Accept", "application/json");
//                BanModApiTokenManager.ApplyAuthHeader(request);



//                string playerName = HeaderSafe(BanModIdentity.GetPlayerName());
//                if (!string.IsNullOrWhiteSpace(playerName))
//                    request.SetRequestHeader("X-BANMOD-PlayerName", playerName);

//                string platform = HeaderSafe(BanModIdentity.GetPlatform());
//                if (!string.IsNullOrWhiteSpace(platform))
//                    request.SetRequestHeader("X-BANMOD-Platform", platform);

//                return request;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError("[BANMOD Community] Request creation failed: " + ex.Message);
//                return null;
//            }
//        }

//        private static string HeaderSafe(string value)
//        {
//            if (string.IsNullOrWhiteSpace(value)) return "";
//            StringBuilder safe = new StringBuilder(value.Length);
//            for (int i = 0; i < value.Length; i++)
//            {
//                char c = value[i];
//                if (c >= 0x20 && c <= 0x7E) safe.Append(c);
//            }
//            return safe.ToString().Trim();
//        }


//        private static IEnumerator SendRequest(string method, string url, object payload, Action<bool, string> callback)
//        {
//            bool identityReady = false;
//            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) =>
//            {
//                identityReady = success && !string.IsNullOrWhiteSpace(token);
//            });

//            if (!identityReady)
//            {
//                callback?.Invoke(false, "FriendCode unavailable. Please wait for BanMod login to finish.");
//                yield break;
//            }

//            string json = payload == null ? null : JsonSerializer.Serialize(payload, JsonOptions);
//            UnityWebRequest request = CreateRequest(method, url, json);
//            if (request == null)
//            {
//                callback?.Invoke(false, "Could not create request.");
//                yield break;
//            }

//            yield return request.SendWebRequest();

//            string body = "";
//            try { body = request.downloadHandler != null ? (request.downloadHandler.text ?? "") : ""; } catch { }
//            long code = 0;
//            try { code = request.responseCode; } catch { }
//            bool ok = request.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;


//            if (!ok)
//            {
//                string error = request.error ?? "request failed";
//                if (string.IsNullOrWhiteSpace(body))
//                    body = code > 0 ? ("HTTP " + code + ": " + error) : error;
//                Debug.LogWarning("[BANMOD Community] " + method + " " + url + " failed: HTTP=" + code + " result=" + request.result + " error=" + error);
//            }

//            try { request.Dispose(); } catch { }
//            callback?.Invoke(ok, body);
//        }


//        private static List<CommunityPost> ParsePosts(string json)
//        {
//            var result = new List<CommunityPost>();
//            using (var doc = JsonDocument.Parse(json))
//            {
//                var root = doc.RootElement;
//                if (!root.TryGetProperty("posts", out var posts) || posts.ValueKind != JsonValueKind.Array) return result;
//                foreach (var item in posts.EnumerateArray())
//                {
//                    var p = new CommunityPost();
//                    if (item.TryGetProperty("id", out var id)) p.Id = id.GetInt32();
//                    if (item.TryGetProperty("kind", out var k)) p.Kind = k.GetString() ?? "suggestion";
//                    if (item.TryGetProperty("title", out var t)) p.Title = t.GetString() ?? "";
//                    if (item.TryGetProperty("message", out var m)) p.Message = m.GetString() ?? "";
//                    if (item.TryGetProperty("status", out var st)) p.Status = st.GetString() ?? "open";
//                    if (item.TryGetProperty("created_at", out var c)) p.CreatedAt = c.GetInt64();
//                    if (item.TryGetProperty("updated_at", out var u)) p.UpdatedAt = u.GetInt64();
//                    if (item.TryGetProperty("stars", out var stars)) p.Stars = stars.GetInt32();
//                    if (item.TryGetProperty("reply_count", out var rc)) p.ReplyCount = rc.GetInt32();
//                    if (item.TryGetProperty("my_star", out var ms)) p.MyStar = ms.GetBoolean();
//                    if (item.TryGetProperty("my_post", out var mp)) p.MyPost = mp.GetBoolean();
//                    if (item.TryGetProperty("author", out var author)) p.Author = author.GetString() ?? "BanModUser";
//                    if (p.Id > 0) result.Add(p);
//                }
//            }
//            return result;
//        }

//        private static List<CommunityReply> ParseReplies(string json)
//        {
//            var result = new List<CommunityReply>();
//            using (var doc = JsonDocument.Parse(json))
//            {
//                var root = doc.RootElement;
//                if (!root.TryGetProperty("replies", out var replies) || replies.ValueKind != JsonValueKind.Array) return result;
//                foreach (var item in replies.EnumerateArray())
//                {
//                    var r = new CommunityReply();
//                    if (item.TryGetProperty("id", out var id)) r.Id = id.GetInt32();
//                    if (item.TryGetProperty("post_id", out var pid)) r.PostId = pid.GetInt32();
//                    if (item.TryGetProperty("message", out var m)) r.Message = m.GetString() ?? "";
//                    if (item.TryGetProperty("created_at", out var c)) r.CreatedAt = c.GetInt64();
//                    if (item.TryGetProperty("updated_at", out var u)) r.UpdatedAt = u.GetInt64();
//                    if (item.TryGetProperty("my_reply", out var mr)) r.MyReply = mr.GetBoolean();
//                    if (item.TryGetProperty("author", out var author)) r.Author = author.GetString() ?? "BanModUser";
//                    if (r.Id > 0) result.Add(r);
//                }
//            }
//            return result;
//        }

//        private static int HighestPostId(List<CommunityPost> posts)
//        {
//            int max = 0; for (int i = 0; i < posts.Count; i++) max = Math.Max(max, posts[i].Id); return max;
//        }

//        private static string ErrorText(string body)
//        {
//            if (string.IsNullOrWhiteSpace(body)) return "Server error.";
//            try { using (var doc = JsonDocument.Parse(body)) { var r = doc.RootElement; if (r.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String) return e.GetString() ?? "Server error."; if (r.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) return m.GetString() ?? "Server error."; } } catch { }
//            return Short(body, 220);
//        }

//        private static string CommunityUrl(string path) { return CommunityApiBase.TrimEnd('/') + "/api/community" + path; }
//        private static void SetStatus(string text) { if (_statusText != null) _statusText.text = text ?? ""; }
//        private static string Short(string text, int max) { text = (text ?? "").Replace("\r", " ").Replace("\n", " ").Trim(); return text.Length <= max ? text : text.Substring(0, Math.Max(1, max - 1)) + "…"; }
//        private static string FormatTimestamp(long value) { try { return DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().ToString("yyyy-MM-dd HH:mm"); } catch { return ""; } }
//        private static void SetInputText(TMP_InputField input, string value) { if (input == null) return; input.text = value ?? ""; try { input.MoveTextEnd(false); } catch { } }
//        private static void SetButtonText(Button button, string value) { if (button == null) return; var t = button.GetComponentInChildren<TextMeshProUGUI>(true); if (t != null) { t.richText = false; t.text = value ?? ""; } }


//        private static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
//        {
//            GameObject go = new GameObject(name);
//            go.transform.SetParent(parent, false);
//            RectTransform rt = go.AddComponent<RectTransform>();
//            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//            rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.anchoredPosition = pos;
//            rt.sizeDelta = size;
//            Image img = go.AddComponent<Image>();
//            ApplyModernImage(img, color, true);
//            img.raycastTarget = true;
//            return go;
//        }

//        private static void ApplyModernImage(Image image, Color color, bool addOutline)
//        {
//            if (image == null) return;
//            image.sprite = BanModUiStyles.RoundedSprite;
//            image.type = Image.Type.Sliced;
//            image.color = color;
//            if (addOutline)
//            {
//                Outline outline = image.GetComponent<Outline>();
//                if (outline == null) outline = image.gameObject.AddComponent<Outline>();
//                outline.effectColor = new Color(BanModUiStyles.AccentColor.r, BanModUiStyles.AccentColor.g, BanModUiStyles.AccentColor.b, 0.20f);
//                outline.effectDistance = new Vector2(1f, -1f);
//                outline.useGraphicAlpha = true;
//            }
//        }


//        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Color color, Vector2 pos, Vector2 size)
//        {
//            var go = new GameObject(name); go.transform.SetParent(parent, false);
//            var rt = go.AddComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
//            var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = text ?? ""; tmp.fontSize = fontSize; tmp.alignment = alignment; tmp.color = color; tmp.richText = false; tmp.enableWordWrapping = true; tmp.raycastTarget = false;
//            try { if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset; } catch { }
//            return tmp;
//        }

//        private static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color color, Action action)
//        {
//            GameObject go = new GameObject(name);
//            go.transform.SetParent(parent, false);
//            RectTransform rt = go.AddComponent<RectTransform>();
//            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//            rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.anchoredPosition = pos;
//            rt.sizeDelta = size;
//            Image img = go.AddComponent<Image>();
//            ApplyModernImage(img, color, false);
//            img.raycastTarget = true;
//            Button btn = go.AddComponent<Button>();
//            btn.targetGraphic = img;
//            btn.transition = Selectable.Transition.ColorTint;
//            ColorBlock colors = btn.colors;
//            colors.normalColor = Color.white;
//            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
//            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
//            colors.selectedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
//            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
//            colors.fadeDuration = 0.08f;
//            btn.colors = colors;
//            if (action != null) btn.onClick.AddListener((UnityAction)delegate { action(); });
//            TextMeshProUGUI label = CreateLabel(go.transform, "Text", text, 21, TextAlignmentOptions.Center, Color.white, Vector2.zero, size);
//            label.enableAutoSizing = true;
//            label.fontSizeMin = 14f;
//            label.fontSizeMax = 21f;
//            RectTransform textRect = label.GetComponent<RectTransform>();
//            textRect.anchorMin = Vector2.zero;
//            textRect.anchorMax = Vector2.one;
//            textRect.offsetMin = Vector2.zero;
//            textRect.offsetMax = Vector2.zero;
//            label.enableWordWrapping = false;
//            return btn;
//        }


//        private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, bool multiline, Vector2 pos, Vector2 size, int limit)
//        {
//            GameObject go = new GameObject(name);
//            go.transform.SetParent(parent, false);
//            RectTransform rt = go.AddComponent<RectTransform>();
//            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//            rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.anchoredPosition = pos;
//            rt.sizeDelta = size;
//            Image bg = go.AddComponent<Image>();
//            ApplyModernImage(bg, new Color(0.10f, 0.11f, 0.14f, 0.98f), true);
//            bg.raycastTarget = true;
//            TMP_InputField input = go.AddComponent<TMP_InputField>();
//            input.targetGraphic = bg;
//            input.characterLimit = limit;
//            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
//            input.contentType = TMP_InputField.ContentType.Standard;
//            input.inputType = TMP_InputField.InputType.Standard;
//            input.richText = false;
//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(go.transform, false);
//            RectTransform vrt = viewport.AddComponent<RectTransform>();
//            StretchFull(vrt, new Vector4(10f, 6f, 10f, 6f));
//            Image vpImage = viewport.AddComponent<Image>();
//            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
//            vpImage.raycastTarget = false;
//            viewport.AddComponent<RectMask2D>();
//            GameObject textGo = new GameObject("Text");
//            textGo.transform.SetParent(viewport.transform, false);
//            RectTransform trt = textGo.AddComponent<RectTransform>();
//            StretchFull(trt);
//            TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
//            text.fontSize = multiline ? 16 : 18;
//            text.color = Color.white;
//            text.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
//            text.enableWordWrapping = multiline;
//            text.overflowMode = multiline ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
//            text.richText = false;
//            text.raycastTarget = false;
//            GameObject phGo = new GameObject("Placeholder");
//            phGo.transform.SetParent(viewport.transform, false);
//            RectTransform prt = phGo.AddComponent<RectTransform>();
//            StretchFull(prt);
//            TextMeshProUGUI ph = phGo.AddComponent<TextMeshProUGUI>();
//            ph.text = placeholder;
//            ph.fontSize = multiline ? 16 : 18;
//            ph.color = new Color(1f, 1f, 1f, 0.45f);
//            ph.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
//            ph.enableWordWrapping = multiline;
//            ph.overflowMode = multiline ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
//            ph.richText = false;
//            ph.raycastTarget = false;
//            try { if (TMP_Settings.defaultFontAsset != null) { text.font = TMP_Settings.defaultFontAsset; ph.font = TMP_Settings.defaultFontAsset; } } catch { }
//            input.textViewport = vrt;
//            input.textComponent = text;
//            input.placeholder = ph;
//            return input;
//        }


//        private static TextMeshProUGUI CreateScrollableText(Transform parent, string name, Vector2 pos, Vector2 size, out ScrollRect scroll, out RectTransform content)
//        {
//            var root = new GameObject(name); root.transform.SetParent(parent, false); var rrt = root.AddComponent<RectTransform>(); rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0.5f); rrt.pivot = new Vector2(0.5f, 0.5f); rrt.anchoredPosition = pos; rrt.sizeDelta = size;
//            var bg = root.AddComponent<Image>(); ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
//            scroll = root.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30f;
//            var vp = new GameObject("Viewport"); vp.transform.SetParent(root.transform, false); var vrt = vp.AddComponent<RectTransform>(); StretchFull(vrt, new Vector4(10f, 10f, 10f, 10f)); vp.AddComponent<Image>().color = new Color(0, 0, 0, 0.001f); vp.AddComponent<RectMask2D>();
//            var cgo = new GameObject("Content"); cgo.transform.SetParent(vp.transform, false); content = cgo.AddComponent<RectTransform>(); content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f); content.pivot = new Vector2(0.5f, 1f); content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0f, 400f);
//            var tgo = new GameObject("Text"); tgo.transform.SetParent(cgo.transform, false); var tr = tgo.AddComponent<RectTransform>(); tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f); tr.anchoredPosition = Vector2.zero; tr.sizeDelta = new Vector2(-12f, 400f);
//            var text = tgo.AddComponent<TextMeshProUGUI>(); text.fontSize = 17; text.color = new Color(0.94f, 0.95f, 1f, 1f); text.alignment = TextAlignmentOptions.TopLeft; text.enableWordWrapping = true; text.richText = false; text.overflowMode = TextOverflowModes.Overflow; text.raycastTarget = false; try { if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset; } catch { }
//            scroll.viewport = vrt; scroll.content = content;
//            return text;
//        }

//        private static void StretchFull(RectTransform rt) { StretchFull(rt, Vector4.zero); }
//        private static void StretchFull(RectTransform rt, Vector4 pad)
//        {
//            if (rt == null) return; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f); rt.offsetMin = new Vector2(pad.x, pad.y); rt.offsetMax = new Vector2(-pad.z, -pad.w);
//        }

//    }

//    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
//    internal static class BanModCommunityMainMenuStartPatch
//    {
//        private static void Postfix(MainMenuManager __instance)
//        {
//            BanModCommunityBoard.InstallMainMenuButton(__instance);
//        }
//    }

//    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
//    internal static class BanModCommunityRuntimePatch
//    {
//        private static void Postfix()
//        {
//            BanModCommunityBoard.RuntimeTick();
//        }
//    }



//    [HarmonyPatch(typeof(BanModCommunicationUi), "get_IsUiOpen")]
//    internal static class BanModCommunityCommunicationIsOpenPatch
//    {
//        private static bool Prefix(ref bool __result)
//        {
//            if (!BanModCommunityBoard.IsOpen && !BanModSymbolPicker.IsOpen) return true;
//            __result = true;
//            return false;
//        }
//    }



//    [HarmonyPatch(typeof(BanModCommunicationUi), nameof(BanModCommunicationUi.OpenMenu))]
//    internal static class BanModCommunityCommunicationOpenPatch
//    {
//        private static bool Prefix()
//        {
//            BanModCommunityBoard.ToggleBugReportFromF3();
//            return false;
//        }
//    }



//    [HarmonyPatch(typeof(BanModCommunicationUi), nameof(BanModCommunicationUi.SetUnreadCount))]
//    internal static class BanModCommunityCommunicationUnreadPatch
//    {
//        private static bool Prefix(int count)
//        {
//            BanModCommunityBoard.SetBugReportUnreadCount(count);
//            return false;
//        }
//    }



//    [HarmonyPatch(typeof(BanModCommunicationUi), nameof(BanModCommunicationUi.ShowReportChatPopup))]
//    internal static class BanModCommunityCommunicationReportPopupPatch
//    {
//        private static bool Prefix(BanModCommunicationManager.ReportSummary report, Action onClose)
//        {
//            BanModCommunityBoard.OpenBugReportNotification(report, onClose);
//            return false;
//        }
//    }
//}
//credits and licenses in the resources folder/
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace BanMod
{
    internal static class BanModCommunityBoard
    {
        private const string RootName = "BanMod_StandaloneCommunity";
        private const int PageSize = 6;
        private const int CommunityChatCharacterLimit = 1000;


        private const float CommunityButtonYOffsetFallback = 0.54f;
        private const string CommunityLastSeenPostKey = "BANMOD_Community_LastSeenPostId_v2";
        private const string CommunityLastSeenReplyKey = "BANMOD_Community_LastSeenReplyId_v2";
        private const string CommunityLastSeenChatKey = "BANMOD_Community_LastSeenChatId_v1";
        private const string CommunityDisplayNameCacheKey = "BANMOD_Community_DisplayName_v1";
        private const float CommunityMentionPollIntervalSeconds = 10f;
        private const string CommunityApiBase = BanModApiConfig.ApiBaseUrl;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private static MainMenuManager _mainMenuOwner;
        private static GameObject _mainMenuButton;
        private static GameObject _communityUnreadDot;
        private static bool _mainMenuInstallRunning;
        private static int _latestKnownCommunityPostId;
        private static int _latestKnownCommunityReplyId;
        private static int _latestKnownCommunityChatId;


        private static readonly HashSet<int> CommunityMentionNotifiedIds = new HashSet<int>();
        private static bool _communityMentionInitialSyncDone;
        private static bool _communityMentionPollRunning;
        private static int _communityMentionLastMessageId;
        private static float _communityNextMentionPollTime;
        private static float _communityMentionFailureBackoff = 10f;


        private static GameObject _root;
        private static GameObject _window;
        private static GameObject _listPanel;
        private static GameObject _detailPanel;
        private static GameObject _composePanel;
        private static GameObject _chatPanel;
        private static RectTransform _chatContent;
        private static ScrollRect _chatScroll;
        private static GameObject _chatListRoot;
        private static readonly List<GameObject> ChatRowObjects = new List<GameObject>();
        private static TMP_InputField _chatInput;
        private static Button _chatSendButton;
        private static int _editingChatMessageId;
        private static string _chatDraftBeforeEdit = "";
        private static Button _chatTranslateButton;
        private static Button _chatTagButton;
        private static Button _chatReplyButton;
        private static TMP_InputField _profileInput;
        private static TextMeshProUGUI _profileNameText;
        private static TextMeshProUGUI _profileHeaderText;
        private static Button _profileActionButton;
        private static Button _profileMenuButton;
        private static GameObject _profileMenuPanel;
        private static string _communityDisplayName = "";
        private static bool _profileNameLocked;
        private static string _profilePendingName = "";
        private static bool _profileStateLoaded;
        private static bool _profileDefaultAttempted;
        private static bool _profileDefaultRequestRunning;
        private static string _privateLoginUsername = "";
        private static GameObject _managePanel;
        private static TMP_InputField _manageTitleInput;
        private static TMP_InputField _manageMessageInput;
        private static TextMeshProUGUI _manageHeading;
        private static string _manageMode = "";
        private static int _manageTargetId;
        private static Button _newPostButton;
        private static Button _tabChatButton;
        private static Button _tabPrivateChatButton;
        private static Button _tabBugReportButton;
        private static Button _tabSuggestionsButton;
        private static TextMeshProUGUI _privateChatTabUnreadBadgeText;
        private static TextMeshProUGUI _bugTabUnreadBadgeText;
        private static bool _showingChat = true;
        private static bool _showingPrivateChat;
        private static bool _showingBugReport;
        private static bool _chatPolling;
        private static bool _backgroundProfileRequested;




        private static GameObject _bugReportPanel;
        private static RectTransform _bugReportsContent;
        private static ScrollRect _bugReportsScroll;
        private static TextMeshProUGUI _bugConversationText;
        private static RectTransform _bugConversationContent;
        private static ScrollRect _bugConversationScroll;
        private static TextMeshProUGUI _bugSelectedHeader;
        private static TMP_InputField _bugTitleInput;
        private static TMP_InputField _bugGameModeInput;
        private static TMP_InputField _bugCustomRolesWhichInput;
        private static TMP_InputField _bugOtherModsWhichInput;
        private static TMP_InputField _bugDescriptionInput;
        private static TMP_InputField _bugReplyInput;
        private static Button _bugCustomYesButton;
        private static Button _bugCustomNoButton;
        private static Button _bugOtherYesButton;
        private static Button _bugOtherNoButton;
        private static Button _bugSendButton;
        private static Button _bugReplyButton;
        private static Button _bugCloseButton;
        private static Button _bugDeleteButton;
        private static bool _bugCustomRolesEnabled;
        private static bool _bugOtherModsInstalled;
        private static bool _bugRequestRunning;
        private static int _selectedBugReportId;
        private static int _bugReportUnreadCount;
        private static readonly List<BanModCommunicationManager.ReportSummary> BugReports =
            new List<BanModCommunicationManager.ReportSummary>();



        private const float PrivateChatPresenceIntervalSeconds = 20f;
        private const float PrivateChatOpenPollIntervalSeconds = 2f;
        private const float PrivateChatClosedPollIntervalSeconds = 5f;
        private const float PrivateChatPlayersPollIntervalSeconds = 10f;
        private const long PrivateChatOnlineThresholdSeconds = 90L;

        private static GameObject _privateChatPanel;
        private static RectTransform _privatePlayersContent;
        private static ScrollRect _privatePlayersScroll;
        private static RectTransform _privateMessagesContent;
        private static ScrollRect _privateMessagesScroll;
        private static TMP_InputField _privateChatInput;
        private static TextMeshProUGUI _privateConversationTitle;
        private static Button _privateReportButton;
        private static Button _privateTranslateButton;
        private static readonly List<PrivateChatPlayer> PrivateChatPlayers = new List<PrivateChatPlayer>();
        private static readonly List<PrivateChatMessage> PrivateChatMessages = new List<PrivateChatMessage>();
        private static readonly Dictionary<long, PrivateChatTranslationEntry> PrivateChatTranslations =
            new Dictionary<long, PrivateChatTranslationEntry>();
        private static readonly Dictionary<int, PrivateChatTranslationEntry> CommunityChatTranslations =
            new Dictionary<int, PrivateChatTranslationEntry>();
        private static readonly Dictionary<int, PrivateChatTranslationEntry> SuggestionTitleTranslations =
            new Dictionary<int, PrivateChatTranslationEntry>();
        private static readonly Dictionary<int, PrivateChatTranslationEntry> SuggestionMessageTranslations =
            new Dictionary<int, PrivateChatTranslationEntry>();
        private static readonly Dictionary<int, PrivateChatTranslationEntry> SuggestionReplyTranslations =
            new Dictionary<int, PrivateChatTranslationEntry>();
        private static readonly HashSet<long> PrivateKnownMessageIds = new HashSet<long>();
        private static readonly HashSet<long> PrivateUnreadMessageIds = new HashSet<long>();
        private static readonly HashSet<long> PrivatePendingDeliveryAckIds = new HashSet<long>();
        private static string _privateActiveFriendCode = "";
        private static string _privateActiveName = "";
        private static int _selectedPrivateMessageIndex = -1;
        private static string _privateBanModToken = "";
        private static long _privateChatSessionStartedAt;
        private static long _privateLastMessageId;
        private static bool _privateInitialSyncDone;
        private static bool _privateSessionRequestRunning;
        private static bool _privatePresenceRequestRunning;
        private static bool _privatePlayersRequestRunning;
        private static bool _privateMessagesRequestRunning;
        private static bool _privateAckRunning;
        private static bool _privateSendRunning;
        private static bool _privateReportRunning;
        private static float _privateNextSessionAttemptTime;
        private static float _privateNextPresenceTime;
        private static float _privateNextPlayersPollTime;
        private static float _privateNextMessagesPollTime;



        private static GameObject _launcherRoot;
        private static GameObject _launcherButton;
        private static RectTransform _launcherButtonRect;
        private static TextMeshProUGUI _launcherUnreadDot;
        private static bool _launcherDragging;
        private static bool _launcherMoved;
        private static Vector2 _launcherDragOffset;
        private static Vector2 _launcherDragStartMouse;
        private static float _launcherSuppressClickUntil;
        private const string LauncherPosXKey = "BANMOD_COMMUNITY_ICON_X_V2";
        private const string LauncherPosYKey = "BANMOD_COMMUNITY_ICON_Y_V2";
        private static bool _publicCommunityUnread;
        private static readonly List<BoxCollider2D> BlockedMainMenuColliders = new List<BoxCollider2D>();
        private static readonly List<bool> BlockedMainMenuColliderStates = new List<bool>();
        private static readonly List<PassiveButton> BlockedMainMenuButtons = new List<PassiveButton>();
        private static readonly List<bool> BlockedMainMenuButtonStates = new List<bool>();
        private static bool _mainMenuInputBlocked;
        private static TextMeshProUGUI _statusText;
        private static TextMeshProUGUI _detailText;
        private static RectTransform _detailContent;
        private static ScrollRect _detailScroll;
        private static RectTransform _suggestionsListContent;
        private static ScrollRect _suggestionsListScroll;
        private static TMP_InputField _titleInput;
        private static TMP_InputField _messageInput;
        private static TMP_InputField _replyInput;
        private static Button _starButton;
        private static Button _suggestionTranslateButton;
        private static readonly List<GameObject> ListButtons = new List<GameObject>();
        private static readonly List<CommunityPost> Posts = new List<CommunityPost>();
        private static readonly List<CommunityReply> Replies = new List<CommunityReply>();
        private static CommunityPost _selected;
        private static bool _requestRunning;


        private static bool _manageRequestRunning;

        private sealed class CommunityPost
        {
            public int Id;
            public string Kind = "suggestion";
            public string Title = "";
            public string Message = "";
            public string Status = "open";
            public long CreatedAt;
            public long UpdatedAt;
            public int Stars;
            public int ReplyCount;
            public bool MyStar;
            public bool MyPost;
            public string Author = "";
        }

        private sealed class CommunityReply
        {
            public int Id;
            public int PostId;
            public string Message = "";
            public long CreatedAt;
            public long UpdatedAt;
            public bool MyReply;
            public string Author = "";
        }

        private sealed class CommunityChatMessage
        {
            public int Id;
            public string Message = "";
            public long CreatedAt;
            public long UpdatedAt;
            public bool MyMessage;
            public string Author = "";
        }

        private sealed class PrivateChatPlayer
        {
            public string FriendCode = "";
            public string PlayerName = "";
            public bool IsOnline;
            public bool InLobby;
            public long LastSeen;
        }

        private sealed class PrivateChatMessage
        {
            public long Id;
            public string SenderFriendCode = "";
            public string SenderName = "";
            public string RecipientFriendCode = "";
            public string RecipientName = "";
            public bool IsPrivate;
            public bool PendingDelivery;
            public string Message = "";
            public long CreatedAt;
        }

        private sealed class PrivateChatTranslationEntry
        {
            public string SettingsKey = "";
            public string Original = "";
            public string Translation = "";
            public bool Attempted;
            public bool Manual;
        }

        private static readonly List<CommunityChatMessage> ChatMessages = new List<CommunityChatMessage>();
        private static int _selectedChatIndex = -1;
        private static int _selectedReplyIndex = -1;
        private static Button _reportChatButton;
        private static Button _reportReplyButton;

        internal static bool IsOpen { get { return _root != null && _root.activeSelf; } }

        internal static void OpenFromMainMenu()
        {
            try
            {
                EnsureStandaloneUi();
                if (_root == null) return;
                BlockMainMenuInput(true);
                _root.SetActive(true);
                RefreshLauncherVisibility();
                if (_composePanel != null) _composePanel.SetActive(false);
                SetStatus("Connecting to Community...");
                FetchProfile();
                ShowChatTab();
            }
            catch (Exception ex)
            {
                BlockMainMenuInput(false);
                Debug.LogError("[BANMOD Community] Cannot open standalone Community: " + ex);
            }
        }
        internal static void InstallMainMenuButton(MainMenuManager menu)
        {
            if (menu == null) return;
            if (_mainMenuButton != null) return;
            if (_mainMenuInstallRunning) return;

            _mainMenuInstallRunning = true;
            _mainMenuOwner = menu;
            menu.StartCoroutine(CoInstallMainMenuButton(menu));
        }

        private static IEnumerator CoInstallMainMenuButton(MainMenuManager menu)
        {


            PassiveButton stockNews = null;
            for (int attempt = 0; attempt < 180 && menu != null; attempt++)
            {
                stockNews = FindStockNewsButton(menu);
                if (stockNews != null && stockNews.gameObject != null && stockNews.gameObject.activeInHierarchy)
                    break;

                if (attempt == 0)
                    Debug.Log("[BANMOD Community] Waiting for the stock News button to become available...");

                yield return new WaitForSeconds(0.25f);
            }

            _mainMenuInstallRunning = false;
            if (menu == null) yield break;

            if (stockNews == null || stockNews.gameObject == null)
            {
                Debug.LogError("[BANMOD Community] Stock News button was still not found after MainMenu startup.");
                DumpMainMenuButtons(menu);
                yield break;
            }

            try
            {
                // BuildCommunityMainMenuButton(menu, stockNews); // disabilitato: niente pulsante COMMUNITY accanto a NEWS
            }
            catch (Exception ex)
            {
                Debug.LogError("[BANMOD Community] Failed to build main-menu Community button: " + ex);
                yield break;
            }


            if (_mainMenuButton != null)
                menu.StartCoroutine(CommunityUnreadPollingLoop(menu));
        }

        private static PassiveButton FindStockNewsButton(MainMenuManager menu)
        {
            if (menu == null) return null;

            try
            {
                var buttons = menu.GetComponentsInChildren<PassiveButton>(true);
                if (buttons != null)
                {

                    for (int i = 0; i < buttons.Length; i++)
                    {
                        var button = buttons[i];
                        if (button == null || button.gameObject == null) continue;
                        string objectName = (button.gameObject.name ?? "").ToUpperInvariant();
                        if (objectName.Contains("NEWS")) return button;
                    }



                    for (int i = 0; i < buttons.Length; i++)
                    {
                        var button = buttons[i];
                        if (button == null || button.gameObject == null) continue;
                        if (ButtonHasAnyText(button, "NOVIT", "NEWS", "NOUVE", "NOVED", "NOTIC", "NEUIG", "НОВОСТ"))
                            return button;
                    }
                }
            }
            catch { }



            try
            {
                var objects = Resources.FindObjectsOfTypeAll(Il2CppType.Of<PassiveButton>());
                if (objects != null)
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        var button = objects[i] as PassiveButton;
                        if (button == null || button.gameObject == null || button.transform == null) continue;
                        if (menu.transform != null && !button.transform.IsChildOf(menu.transform)) continue;

                        string objectName = (button.gameObject.name ?? "").ToUpperInvariant();
                        if (objectName.Contains("NEWS") ||
                            ButtonHasAnyText(button, "NOVIT", "NEWS", "NOUVE", "NOVED", "NOTIC", "NEUIG", "НОВОСТ"))
                            return button;
                    }
                }
            }
            catch { }

            return null;
        }

        private static bool ButtonHasAnyText(PassiveButton button, params string[] tokens)
        {
            if (button == null) return false;
            try
            {
                var labels = button.GetComponentsInChildren<TMP_Text>(true);
                if (labels == null) return false;
                for (int i = 0; i < labels.Length; i++)
                {
                    var label = labels[i];
                    if (label == null) continue;
                    string value = (label.text ?? "").Trim().ToUpperInvariant();
                    if (value.Length == 0) continue;
                    for (int t = 0; t < tokens.Length; t++)
                    {
                        if (value.Contains(tokens[t])) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static PassiveButton FindMenuButton(MainMenuManager menu, string[] objectTokens, string[] textTokens)
        {
            if (menu == null) return null;
            try
            {
                var buttons = menu.GetComponentsInChildren<PassiveButton>(true);
                if (buttons == null) return null;

                for (int i = 0; i < buttons.Length; i++)
                {
                    var button = buttons[i];
                    if (button == null || button.gameObject == null) continue;
                    string name = (button.gameObject.name ?? "").ToUpperInvariant();
                    for (int t = 0; t < objectTokens.Length; t++)
                    {
                        if (name.Contains(objectTokens[t])) return button;
                    }
                }

                for (int i = 0; i < buttons.Length; i++)
                {
                    var button = buttons[i];
                    if (button == null) continue;
                    if (ButtonHasAnyText(button, textTokens)) return button;
                }
            }
            catch { }
            return null;
        }

        private static void BuildCommunityMainMenuButton(MainMenuManager menu, PassiveButton originalPassive)
        {
            if (menu == null || originalPassive == null || originalPassive.gameObject == null) return;
            if (_mainMenuButton != null) return;

            const float halfRatio = 0.475f;
            const float gapRatio = 0.050f;
            GameObject original = originalPassive.gameObject;
            Transform parent = original.transform.parent;
            if (parent == null) return;

            Debug.Log("[BANMOD Community] Stock News button found: " + original.name);
            Vector2 stockSize = GetPassiveButtonSize(originalPassive);
            float fullWidthInParent = GetButtonWidthInParent(originalPassive, parent);
            if (fullWidthInParent <= 0.05f)
                fullWidthInParent = stockSize.x * Mathf.Abs(original.transform.localScale.x);
            if (fullWidthInParent <= 0.05f)
            {
                Debug.LogError("[BANMOD Community] Could not determine the stock News button width.");
                return;
            }

            Vector3 stockPosition = original.transform.localPosition;
            Vector3 stockScale = original.transform.localScale;
            float centerOffset = fullWidthInParent * ((halfRatio + gapRatio) * 0.5f);

            GameObject clone = UnityEngine.Object.Instantiate(original, parent);
            clone.name = "BanMod_CommunityNewsButton";
            PassiveButton passive = clone.GetComponent<PassiveButton>();
            if (passive == null) passive = clone.GetComponentInChildren<PassiveButton>(true);
            if (passive == null)
            {
                UnityEngine.Object.Destroy(clone);
                Debug.LogError("[BANMOD Community] Cloned News object has no PassiveButton.");
                return;
            }

            DisableAspectPositionsUnder(original.transform);
            DisableAspectPositionsUnder(clone.transform);




            original.transform.localScale = new Vector3(stockScale.x * halfRatio, stockScale.y, stockScale.z);
            clone.transform.localScale = new Vector3(stockScale.x * halfRatio, stockScale.y, stockScale.z);
            original.transform.localPosition = stockPosition + new Vector3(-centerOffset, 0f, 0f);
            clone.transform.localPosition = stockPosition + new Vector3(centerOffset, 0f, 0f);

            try
            {
                NewsCountButton[] copiedNews = clone.GetComponentsInChildren<NewsCountButton>(true);
                if (copiedNews != null)
                    for (int i = 0; i < copiedNews.Length; i++)
                        if (copiedNews[i] != null)
                        {
                            copiedNews[i].enabled = false;
                            UnityEngine.Object.Destroy(copiedNews[i]);
                        }
            }
            catch { }



            TMP_Text stockNewsSource = FindNewsMainLabel(original);
            string stockNewsText = stockNewsSource == null ? "NEWS" : (stockNewsSource.text ?? "NEWS").Trim();
            if (string.IsNullOrWhiteSpace(stockNewsText)) stockNewsText = "NEWS";
            TMP_Text newsLabel = InstallCleanSplitLabel(originalPassive, stockNewsText, 2f);


            HideCommunityDecorativeSprites(passive);
            TMP_Text communityLabel = InstallCleanSplitLabel(passive, "COMMUNITY", 2f);
            RemoveCopiedNewsBadge(clone, communityLabel);
            RepositionNumericBadge(original, 0.20f);

            passive.OnClick = new Button.ButtonClickedEvent();
            passive.OnClick.AddListener((UnityAction)delegate { OpenFromMainMenu(); });
            _mainMenuButton = clone;
            CreateCommunityUnreadDot(clone, communityLabel);
            clone.SetActive(true);
            EnforceCommunityButtonVisuals();
            RefreshCombinedUnread();
            Debug.Log("[BANMOD Community] News row split: separate scaled hit-boxes, 47.5% | 5% | 47.5%.");
        }

        private static float GetButtonWidthInParent(PassiveButton button, Transform parent)
        {
            if (button == null || parent == null) return 0f;
            try
            {
                BoxCollider2D col = button.GetComponent<BoxCollider2D>();
                if (col != null && col.size.x > 0.05f)
                {
                    Vector3 leftWorld = button.transform.TransformPoint(new Vector3(col.offset.x - col.size.x * 0.5f, col.offset.y, 0f));
                    Vector3 rightWorld = button.transform.TransformPoint(new Vector3(col.offset.x + col.size.x * 0.5f, col.offset.y, 0f));
                    float left = parent.InverseTransformPoint(leftWorld).x;
                    float right = parent.InverseTransformPoint(rightWorld).x;
                    return Mathf.Abs(right - left);
                }
            }
            catch { }
            return 0f;
        }

        private static void CounterScaleForegroundX(PassiveButton button, float ratio)
        {
            if (button == null || ratio <= 0.01f) return;
            try
            {
                var labels = button.GetComponentsInChildren<TMP_Text>(true);
                if (labels != null)
                    for (int i = 0; i < labels.Length; i++)
                        if (labels[i] != null)
                        {
                            Vector3 s = labels[i].transform.localScale;
                            labels[i].transform.localScale = new Vector3(s.x / ratio, s.y, s.z);
                        }
            }
            catch { }
            try
            {
                Transform inactive = button.inactiveSprites == null ? null : button.inactiveSprites.transform;
                Transform active = button.activeSprites == null ? null : button.activeSprites.transform;
                var sprites = button.GetComponentsInChildren<SpriteRenderer>(true);
                if (sprites != null)
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sr = sprites[i];
                        if (sr == null) continue;
                        Transform tr = sr.transform;
                        if ((inactive != null && (tr == inactive || tr.IsChildOf(inactive))) ||
                            (active != null && (tr == active || tr.IsChildOf(active)))) continue;
                        Vector3 s = tr.localScale;
                        tr.localScale = new Vector3(s.x / ratio, s.y, s.z);
                    }
            }
            catch { }
        }

        private static void HideCommunityDecorativeSprites(PassiveButton button)
        {
            if (button == null) return;
            try
            {
                var sprites = button.GetComponentsInChildren<SpriteRenderer>(true);
                if (sprites == null || sprites.Length == 0) return;





                float largestArea = 0f;
                for (int i = 0; i < sprites.Length; i++)
                {
                    SpriteRenderer sr = sprites[i];
                    if (sr == null) continue;
                    float area = Mathf.Abs(sr.bounds.size.x * sr.bounds.size.y);
                    if (area > largestArea) largestArea = area;
                }

                if (largestArea <= 0.0001f) return;
                float keepThreshold = largestArea * 0.62f;
                for (int i = 0; i < sprites.Length; i++)
                {
                    SpriteRenderer sr = sprites[i];
                    if (sr == null) continue;
                    float area = Mathf.Abs(sr.bounds.size.x * sr.bounds.size.y);
                    if (area < keepThreshold) sr.enabled = false;
                }
            }
            catch { }
        }

        private static TMP_Text InstallCleanSplitLabel(PassiveButton button, string text, float fontRatio)
        {
            if (button == null || button.gameObject == null) return null;
            try
            {
                GameObject root = button.gameObject;
                TMP_Text source = FindNewsMainLabel(root) ?? FindLargestTextLabelFallback(root);
                if (source == null) return null;

                Vector3 localPoint = root.transform.InverseTransformPoint(source.transform.position);
                float sourceFontSize = Math.Max(1f, source.fontSize);
                Color sourceColor = source.color;
                RectTransform sourceRect = source.GetComponent<RectTransform>();
                Vector2 sourceSize = sourceRect != null ? sourceRect.sizeDelta : Vector2.zero;

                TMP_Text[] labels = root.GetComponentsInChildren<TMP_Text>(true);
                if (labels != null)
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        TMP_Text existing = labels[i];
                        if (existing == null) continue;
                        string value = (existing.text ?? "").Trim();
                        bool numericBadge = value.Length > 0;
                        for (int c = 0; c < value.Length && numericBadge; c++)
                        {
                            char ch = value[c];
                            if (!char.IsDigit(ch) && ch != '+' && ch != ' ') numericBadge = false;
                        }
                        if (!numericBadge) existing.gameObject.SetActive(false);
                    }
                }

                GameObject labelGo = UnityEngine.Object.Instantiate(source.gameObject, root.transform);
                labelGo.name = "BanMod_SplitLabel_" + text;
                labelGo.SetActive(true);
                TMP_Text label = labelGo.GetComponent<TMP_Text>();
                if (label == null) label = labelGo.GetComponentInChildren<TMP_Text>(true);
                if (label == null) return null;

                try { label.DestroyTranslator(); } catch { }
                label.richText = false;
                label.text = text;
                label.alignment = TextAlignmentOptions.Center;
                label.color = sourceColor;
                label.enableAutoSizing = true;
                label.fontSizeMax = Math.Max(1f, sourceFontSize * fontRatio);
                label.fontSizeMin = Math.Max(1f, sourceFontSize * 0.28f);
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Truncate;
                label.raycastTarget = false;

                Transform tr = label.transform;
                localPoint.x = 0f;
                tr.localPosition = localPoint;
                tr.localRotation = source.transform.localRotation;


                Vector3 sourceScale = source.transform.localScale;
                tr.localScale = new Vector3(sourceScale.x / 0.475f, sourceScale.y, sourceScale.z);

                RectTransform rect = label.GetComponent<RectTransform>();
                if (rect != null && sourceRect != null)
                {
                    rect.anchorMin = sourceRect.anchorMin;
                    rect.anchorMax = sourceRect.anchorMax;
                    rect.pivot = sourceRect.pivot;
                    Vector2 size = sourceSize;
                    if (Math.Abs(size.x) > 0.01f) size.x *= 0.46f;
                    rect.sizeDelta = size;
                }

                return label;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[BANMOD Community] Could not install split label '" + text + "': " + ex.Message);
                return null;
            }
        }


        private static void CenterBranchUnderRoot(Transform root, Transform leaf)
        {
            if (root == null || leaf == null) return;
            try
            {
                Transform branch = leaf;
                while (branch.parent != null && branch.parent != root) branch = branch.parent;
                if (branch.parent == root)
                {
                    Vector3 p = branch.localPosition;
                    p.x = 0f;
                    branch.localPosition = p;
                }
            }
            catch { }
        }

        private static Vector2 GetPassiveButtonSize(PassiveButton button)
        {
            if (button == null) return Vector2.zero;
            try
            {
                var collider = button.GetComponent<BoxCollider2D>();
                if (collider != null && collider.size.x > 0.05f && collider.size.y > 0.05f)
                    return collider.size;
            }
            catch { }

            try
            {
                if (button.inactiveSprites != null)
                {
                    var sr = button.inactiveSprites.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.size.x > 0.05f && sr.size.y > 0.05f)
                        return sr.size;
                }
            }
            catch { }

            try
            {
                var sprites = button.GetComponentsInChildren<SpriteRenderer>(true);
                SpriteRenderer best = null;
                float area = 0f;
                if (sprites != null)
                {
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sr = sprites[i];
                        if (sr == null) continue;
                        float a = Mathf.Abs(sr.size.x * sr.size.y);
                        if (a > area) { area = a; best = sr; }
                    }
                }
                if (best != null) return best.size;
            }
            catch { }
            return Vector2.zero;
        }

        private static void ResizePassiveButton(PassiveButton button, Vector2 stockSize, float widthRatio)
        {
            if (button == null) return;
            float newWidth = stockSize.x * widthRatio;

            try
            {
                BoxCollider2D[] colliders = button.GetComponentsInChildren<BoxCollider2D>(true);
                if (colliders != null)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        BoxCollider2D collider = colliders[i];
                        if (collider == null) continue;
                        if (collider.size.x >= stockSize.x * 0.70f)
                            collider.size = new Vector2(newWidth, collider.size.y);
                    }
                }
            }
            catch { }

            ResizeButtonBackgroundBranch(button.inactiveSprites, newWidth, widthRatio);
            ResizeButtonBackgroundBranch(button.activeSprites, newWidth, widthRatio);
        }

        private static void ResizeButtonBackgroundBranch(GameObject branch, float newWidth, float widthRatio)
        {
            if (branch == null) return;
            try
            {
                SpriteRenderer[] sprites = branch.GetComponentsInChildren<SpriteRenderer>(true);
                if (sprites == null) return;
                for (int i = 0; i < sprites.Length; i++)
                {
                    SpriteRenderer sr = sprites[i];
                    if (sr == null) continue;
                    try
                    {
                        if (sr.drawMode != SpriteDrawMode.Simple)
                            sr.size = new Vector2(newWidth, sr.size.y);
                        else
                        {
                            Vector3 scale = sr.transform.localScale;
                            sr.transform.localScale = new Vector3(scale.x * widthRatio, scale.y, scale.z);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }


        private static void RepositionNumericBadge(GameObject root, float xRatio)
        {
            if (root == null) return;
            try
            {
                TMP_Text main = FindNewsMainLabel(root);
                var labels = root.GetComponentsInChildren<TMP_Text>(true);
                if (labels == null) return;
                for (int i = 0; i < labels.Length; i++)
                {
                    var label = labels[i];
                    if (label == null || label == main) continue;
                    string value = (label.text ?? "").Trim();
                    if (value.Length == 0) continue;
                    bool numeric = true;
                    for (int c = 0; c < value.Length; c++)
                    {
                        char ch = value[c];
                        if (!char.IsDigit(ch) && ch != '+' && ch != ' ') { numeric = false; break; }
                    }
                    if (!numeric) continue;
                    Transform badge = label.transform.parent != null ? label.transform.parent : label.transform;
                    Vector3 p = badge.localPosition;
                    p.x *= xRatio;
                    badge.localPosition = p;
                }
            }
            catch { }
        }

        private static TMP_Text FindNewsMainLabel(GameObject root)
        {
            if (root == null) return null;
            try
            {
                var labels = root.GetComponentsInChildren<TMP_Text>(true);
                if (labels != null)
                {
                    string[] tokens = { "COMMUNITY", "NOVIT", "NEWS", "NOUVE", "NOVED", "NOTIC", "NEUIG", "НОВОСТ" };
                    for (int i = 0; i < labels.Length; i++)
                    {
                        var label = labels[i];
                        if (label == null) continue;
                        string value = (label.text ?? "").Trim().ToUpperInvariant();
                        for (int t = 0; t < tokens.Length; t++)
                            if (value.Contains(tokens[t])) return label;
                    }
                }
            }
            catch { }
            return FindLargestTextLabelFallback(root);
        }

        private static TMP_Text FindLargestTextLabelFallback(GameObject root)
        {
            if (root == null) return null;
            try
            {
                var labels = root.GetComponentsInChildren<TMP_Text>(true);
                TMP_Text best = null;
                float bestScore = -1f;
                if (labels != null)
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        var label = labels[i];
                        if (label == null) continue;
                        string value = (label.text ?? "").Trim();
                        if (value.Length == 0) continue;
                        float score = label.fontSize + value.Length * 0.1f;
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = label;
                        }
                    }
                }
                return best;
            }
            catch { return null; }
        }

        private static void ShiftUpperMainMenuSection(MainMenuManager menu, PassiveButton newsButton, float shiftY)
        {
            if (menu == null || newsButton == null) return;

            var play = FindMenuButton(menu,
                new[] { "PLAYONLINEBUTTON", "PLAYONLINE" },
                new[] { "GIOCA", "PLAY", "SPIELEN", "JOUER", "JUGAR" });
            var inventory = FindMenuButton(menu,
                new[] { "INVENTORY" },
                new[] { "INVENTARIO", "INVENTORY", "INVENTAR", "INVENTAIRE" });
            var shop = FindMenuButton(menu,
                new[] { "SHOP", "STORE" },
                new[] { "NEGOZIO", "SHOP", "STORE", "BOUTIQUE", "TIENDA" });






            var movedRoots = new List<Transform>();
            Transform upperRoot = FindUpperButtonsRoot(play, inventory, shop, newsButton);
            if (upperRoot != null)
            {
                DisableAspectPosition(upperRoot);
                upperRoot.localPosition += new Vector3(0f, shiftY, 0f);
                movedRoots.Add(upperRoot);
                Debug.Log("[BANMOD Community] Shifted upper-menu group once: " + upperRoot.gameObject.name);
            }
            else
            {


                AddIndependentMoveRoot(movedRoots, play == null ? null : play.transform);
                AddIndependentMoveRoot(movedRoots, inventory == null ? null : inventory.transform);
                AddIndependentMoveRoot(movedRoots, shop == null ? null : shop.transform);
                for (int i = 0; i < movedRoots.Count; i++)
                {
                    DisableAspectPosition(movedRoots[i]);
                    movedRoots[i].localPosition += new Vector3(0f, shiftY, 0f);
                    Debug.Log("[BANMOD Community] Shifted independent upper root: " + movedRoots[i].gameObject.name);
                }
            }

            var moved = new HashSet<int>();
            var logo = FindMainMenuLogo(menu, play);
            if (!IsInsideAnyRoot(logo, movedRoots))
                MoveTransformOnce(logo, shiftY, moved, "Among Us logo");
            TryMoveDivider(menu, newsButton, shop, moved, shiftY);
        }

        private static Transform FindUpperButtonsRoot(PassiveButton play, PassiveButton inventory, PassiveButton shop, PassiveButton news)
        {
            Transform[] items = { play == null ? null : play.transform, inventory == null ? null : inventory.transform, shop == null ? null : shop.transform };
            Transform first = null;
            for (int i = 0; i < items.Length; i++) if (items[i] != null) { first = items[i]; break; }
            if (first == null) return null;

            Transform candidate = first;
            while (candidate != null)
            {
                bool containsAll = true;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;
                    if (items[i] != candidate && !items[i].IsChildOf(candidate)) { containsAll = false; break; }
                }
                bool containsNews = news != null && news.transform != null &&
                    (news.transform == candidate || news.transform.IsChildOf(candidate));
                if (containsAll && !containsNews) return candidate;
                candidate = candidate.parent;
            }



            Transform common = CommonAncestor(items);
            if (common == null) return null;
            Transform branch = DirectChildUnder(common, first);
            if (branch == null) return null;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                Transform other = DirectChildUnder(common, items[i]);
                if (other != branch) return null;
            }
            if (news != null && news.transform != null &&
                (news.transform == branch || news.transform.IsChildOf(branch))) return null;
            return branch;
        }

        private static Transform CommonAncestor(Transform[] items)
        {
            Transform first = null;
            for (int i = 0; i < items.Length; i++) if (items[i] != null) { first = items[i]; break; }
            if (first == null) return null;
            Transform c = first;
            while (c != null)
            {
                bool all = true;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;
                    if (items[i] != c && !items[i].IsChildOf(c)) { all = false; break; }
                }
                if (all) return c;
                c = c.parent;
            }
            return null;
        }

        private static Transform DirectChildUnder(Transform ancestor, Transform item)
        {
            if (ancestor == null || item == null || item == ancestor) return item;
            Transform current = item;
            while (current.parent != null && current.parent != ancestor) current = current.parent;
            return current.parent == ancestor ? current : null;
        }

        private static bool IsInsideAnyRoot(Transform candidate, List<Transform> roots)
        {
            if (candidate == null || roots == null) return false;
            for (int i = 0; i < roots.Count; i++)
            {
                var root = roots[i];
                if (root != null && (candidate == root || candidate.IsChildOf(root))) return true;
            }
            return false;
        }

        private static void AddIndependentMoveRoot(List<Transform> roots, Transform candidate)
        {
            if (candidate == null) return;


            for (int i = 0; i < roots.Count; i++)
                if (candidate == roots[i] || candidate.IsChildOf(roots[i])) return;

            for (int i = roots.Count - 1; i >= 0; i--)
                if (roots[i].IsChildOf(candidate)) roots.RemoveAt(i);
            roots.Add(candidate);
        }

        private static float GetStockLowerButtonStep(MainMenuManager menu, PassiveButton news)
        {
            float fallback = CommunityButtonYOffsetFallback;
            if (menu == null || news == null) return fallback;
            try
            {
                var account = FindMenuButton(menu,
                    new[] { "ACCOUNT" },
                    new[] { "IL MIO ACCOUNT", "MY ACCOUNT", "ACCOUNT", "KONTO", "COMPTE", "CUENTA" });
                if (account != null && account.transform != null && news.transform != null &&
                    account.transform.parent == news.transform.parent)
                {
                    float step = Math.Abs(news.transform.localPosition.y - account.transform.localPosition.y);
                    if (step >= 0.30f && step <= 0.90f)
                    {
                        Debug.Log("[BANMOD Community] Using stock News/Account spacing: " + step.ToString("0.000"));
                        return step;
                    }
                }
            }
            catch { }
            Debug.Log("[BANMOD Community] Using fallback Community spacing: " + fallback.ToString("0.000"));
            return fallback;
        }

        private static void DisableAspectPosition(Transform transform)
        {
            if (transform == null) return;
            try
            {
                var aspect = transform.GetComponent<AspectPosition>();
                if (aspect != null)
                {
                    aspect.enabled = false;
                    UnityEngine.Object.Destroy(aspect);
                }
            }
            catch { }
        }

        private static void DisableAspectPositionsUnder(Transform root)
        {
            if (root == null) return;
            try
            {
                var aspects = root.GetComponentsInChildren<AspectPosition>(true);
                if (aspects == null) return;
                for (int i = 0; i < aspects.Length; i++)
                {
                    var aspect = aspects[i];
                    if (aspect == null) continue;
                    aspect.enabled = false;
                    UnityEngine.Object.Destroy(aspect);
                }
            }
            catch { DisableAspectPosition(root); }
        }

        private static void MoveMainButtonOnce(PassiveButton button, float deltaY, HashSet<int> moved, string label)
        {
            if (button == null || button.transform == null) return;
            DisableAspectPosition(button.transform);
            MoveTransformOnce(button.transform, deltaY, moved, label);
        }

        private static void MoveTransformOnce(Transform transform, float deltaY, HashSet<int> moved, string label)
        {
            if (transform == null) return;
            try
            {
                int id = transform.gameObject.GetInstanceID();
                if (moved.Contains(id)) return;
                transform.localPosition += new Vector3(0f, deltaY, 0f);
                moved.Add(id);
                Debug.Log("[BANMOD Community] Shifted " + label + " upward: " + transform.gameObject.name);
            }
            catch { }
        }

        private static Transform FindMainMenuLogo(MainMenuManager menu, PassiveButton play)
        {
            if (menu == null) return null;



            Transform named = FindLikelyAmongUsLogoByName(menu.transform);
            if (named != null) return named;



            try
            {
                if (play != null)
                {
                    var menuSprites = menu.GetComponentsInChildren<SpriteRenderer>(true);
                    SpriteRenderer bestLocal = null;
                    float bestLocalScore = 0f;
                    if (menuSprites != null)
                    {
                        for (int i = 0; i < menuSprites.Length; i++)
                        {
                            var sprite = menuSprites[i];
                            if (sprite == null || sprite.gameObject == null) continue;
                            if (sprite.transform.position.y <= play.transform.position.y + 0.25f) continue;
                            float h = Math.Max(0.001f, sprite.bounds.size.y);
                            float ratio = sprite.bounds.size.x / h;
                            if (ratio < 1.4f) continue;
                            float score = sprite.bounds.size.x * sprite.bounds.size.y * ratio;
                            if (score > bestLocalScore)
                            {
                                bestLocalScore = score;
                                bestLocal = sprite;
                            }
                        }
                    }
                    if (bestLocal != null)
                    {
                        Transform root = bestLocal.transform;
                        while (root.parent != null && root.parent != menu.transform)
                            root = root.parent;
                        return root;
                    }
                }
            }
            catch { }


            try
            {
                var sprites = Resources.FindObjectsOfTypeAll(Il2CppType.Of<SpriteRenderer>());
                SpriteRenderer best = null;
                float bestArea = 0f;
                float playY = play == null ? float.MinValue : play.transform.position.y;
                if (sprites != null)
                {
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sprite = sprites[i] as SpriteRenderer;
                        if (sprite == null || sprite.gameObject == null || !sprite.gameObject.activeInHierarchy) continue;
                        string n = (sprite.gameObject.name ?? "").ToUpperInvariant();
                        if (!n.Contains("LOGO") || n.Contains("LOGOUT")) continue;
                        if (sprite.transform.position.y < playY) continue;
                        float area = sprite.bounds.size.x * sprite.bounds.size.y;
                        if (area > bestArea)
                        {
                            bestArea = area;
                            best = sprite;
                        }
                    }
                }
                if (best != null) return best.transform;
            }
            catch { }

            return null;
        }

        private static Transform FindLikelyAmongUsLogoByName(Transform root)
        {
            if (root == null) return null;
            try
            {
                string own = (root.gameObject.name ?? "").ToUpperInvariant();
                bool logoLike = own.Contains("LOGO") || own.Contains("TITLE");
                bool rejected = own.Contains("DISCORD") || own.Contains("GITHUB") || own.Contains("STEAM") ||
                                own.Contains("SOCIAL") || own.Contains("BANMOD") || own.Contains("LOGIN") ||
                                own.Contains("LOGOUT");
                if (logoLike && !rejected) return root;
                for (int i = 0; i < root.childCount; i++)
                {
                    var found = FindLikelyAmongUsLogoByName(root.GetChild(i));
                    if (found != null) return found;
                }
            }
            catch { }
            return null;
        }

        private static Transform FindTransformNameContains(Transform root, string token)
        {
            if (root == null) return null;
            try
            {
                string own = (root.gameObject.name ?? "").ToUpperInvariant();
                if (own.Contains(token)) return root;
                for (int i = 0; i < root.childCount; i++)
                {
                    var result = FindTransformNameContains(root.GetChild(i), token);
                    if (result != null) return result;
                }
            }
            catch { }
            return null;
        }

        private static void TryMoveDivider(MainMenuManager menu, PassiveButton news, PassiveButton shop, HashSet<int> moved, float shiftY)
        {
            if (menu == null || news == null) return;
            try
            {
                float newsY = news.transform.position.y;
                float shopY = shop == null ? newsY + 3f : shop.transform.position.y;
                var renderers = menu.GetComponentsInChildren<SpriteRenderer>(true);
                if (renderers == null) return;

                SpriteRenderer best = null;
                float bestRatio = 0f;
                for (int i = 0; i < renderers.Length; i++)
                {
                    var sr = renderers[i];
                    if (sr == null || sr.gameObject == null) continue;
                    float y = sr.transform.position.y;
                    if (y <= newsY || y >= shopY) continue;
                    float h = Math.Max(0.001f, sr.bounds.size.y);
                    float ratio = sr.bounds.size.x / h;
                    if (ratio > 8f && ratio > bestRatio)
                    {
                        bestRatio = ratio;
                        best = sr;
                    }
                }

                if (best != null)
                    MoveTransformOnce(best.transform, shiftY, moved, "separator");
            }
            catch { }
        }

        private static void RemoveCopiedNewsBadge(GameObject clone, TMP_Text mainLabel)
        {
            if (clone == null) return;
            try
            {
                var labels = clone.GetComponentsInChildren<TMP_Text>(true);
                if (labels == null) return;
                for (int i = 0; i < labels.Length; i++)
                {
                    var label = labels[i];
                    if (label == null || label == mainLabel) continue;
                    string value = (label.text ?? "").Trim();
                    bool numericBadge = value.Length > 0;
                    for (int c = 0; c < value.Length && numericBadge; c++)
                    {
                        char ch = value[c];
                        if (!char.IsDigit(ch) && ch != '+' && ch != ' ') numericBadge = false;
                    }
                    if (!numericBadge) continue;

                    GameObject badgeRoot = label.gameObject;
                    if (label.transform.parent != null && label.transform.parent.gameObject != clone)
                        badgeRoot = label.transform.parent.gameObject;
                    if (badgeRoot != null) badgeRoot.SetActive(false);
                }
            }
            catch { }
        }

        private static void CreateCommunityUnreadDot(GameObject buttonRoot, TMP_Text sourceLabel)
        {
            if (buttonRoot == null || sourceLabel == null) return;
            try
            {
                var dotObject = UnityEngine.Object.Instantiate(sourceLabel.gameObject, buttonRoot.transform);
                dotObject.name = "BanMod_CommunityUnreadDot";
                var dot = dotObject.GetComponent<TMP_Text>();
                if (dot == null)
                {
                    UnityEngine.Object.Destroy(dotObject);
                    return;
                }

                dot.richText = false;
                dot.text = "●";
                dot.color = new Color(0.96f, 0.06f, 0.06f, 1f);
                dot.alignment = TextAlignmentOptions.Center;
                dot.fontSize = Math.Max(18f, sourceLabel.fontSize * 0.58f);



                Vector3 localPos = new Vector3(2.35f, 0.28f, sourceLabel.transform.localPosition.z - 0.1f);
                var sprites = buttonRoot.GetComponentsInChildren<SpriteRenderer>(true);
                SpriteRenderer largest = null;
                float largestArea = 0f;
                if (sprites != null)
                {
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sr = sprites[i];
                        if (sr == null) continue;
                        float area = sr.bounds.size.x * sr.bounds.size.y;
                        if (area > largestArea)
                        {
                            largestArea = area;
                            largest = sr;
                        }
                    }
                }
                if (largest != null)
                {
                    Vector3 corner = new Vector3(largest.bounds.max.x, largest.bounds.max.y, largest.bounds.center.z);
                    Vector3 converted = buttonRoot.transform.InverseTransformPoint(corner);
                    localPos.x = converted.x - 0.18f;
                    localPos.y = converted.y - 0.12f;
                }

                dotObject.transform.localPosition = localPos;
                dotObject.transform.localRotation = Quaternion.identity;
                dotObject.transform.localScale = sourceLabel.transform.localScale * 0.60f;
                dotObject.SetActive(false);
                _communityUnreadDot = dotObject;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[BANMOD Community] Could not create unread dot: " + ex.Message);
            }
        }

        private static void EnforceCommunityButtonVisuals()
        {
            if (_mainMenuButton == null) return;
            try
            {
                TMP_Text mainLabel = null;
                TMP_Text[] labels = _mainMenuButton.GetComponentsInChildren<TMP_Text>(true);
                if (labels != null)
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        TMP_Text label = labels[i];
                        if (label == null) continue;

                        if (string.Equals(label.gameObject.name, "BanMod_SplitLabel_COMMUNITY", StringComparison.Ordinal))
                        {
                            mainLabel = label;
                            continue;
                        }

                        if (string.Equals(label.gameObject.name, "BanMod_CommunityUnreadDot", StringComparison.Ordinal))
                            continue;

                        string value = (label.text ?? "").Trim();
                        bool numericBadge = value.Length > 0;
                        for (int c = 0; c < value.Length && numericBadge; c++)
                        {
                            char ch = value[c];
                            if (!char.IsDigit(ch) && ch != '+' && ch != ' ') numericBadge = false;
                        }



                        if (!numericBadge) label.gameObject.SetActive(false);
                    }
                }

                if (mainLabel != null)
                {
                    try { mainLabel.DestroyTranslator(); } catch { }
                    mainLabel.richText = false;
                    mainLabel.text = "COMMUNITY";
                    mainLabel.alignment = TextAlignmentOptions.Center;
                    mainLabel.gameObject.SetActive(true);
                }

                RemoveCopiedNewsBadge(_mainMenuButton, mainLabel);
            }
            catch { }
        }


        private static IEnumerator CommunityUnreadPollingLoop(MainMenuManager menu)
        {
            yield return new WaitForSeconds(1f);
            while (menu != null && _mainMenuButton != null)
            {
                EnforceCommunityButtonVisuals();
                yield return CheckCommunityUnread();
                yield return new WaitForSeconds(20f);
            }
        }

        private static IEnumerator CheckCommunityUnread()
        {
            bool ok = false;
            string body = "";
            yield return SendRequest("GET", CommunityUrl("/activity"), null, delegate (bool success, string response)
            {
                ok = success;
                body = response ?? "";
            });
            if (!ok) yield break;
            try
            {
                using (var doc = JsonDocument.Parse(body))
                {
                    var root = doc.RootElement;
                    int postId = root.TryGetProperty("latest_post_id", out var p) ? p.GetInt32() : 0;
                    int replyId = root.TryGetProperty("latest_reply_id", out var r) ? r.GetInt32() : 0;
                    int chatId = root.TryGetProperty("latest_chat_id", out var c) ? c.GetInt32() : 0;
                    _latestKnownCommunityPostId = Math.Max(_latestKnownCommunityPostId, postId);
                    _latestKnownCommunityReplyId = Math.Max(_latestKnownCommunityReplyId, replyId);
                    _latestKnownCommunityChatId = Math.Max(_latestKnownCommunityChatId, chatId);

                    if (!_communityMentionInitialSyncDone)
                    {
                        _communityMentionInitialSyncDone = true;
                        _communityMentionLastMessageId = chatId;
                    }

                    int seenPost = PlayerPrefs.GetInt(CommunityLastSeenPostKey, 0);
                    int seenReply = PlayerPrefs.GetInt(CommunityLastSeenReplyKey, 0);
                    int seenChat = PlayerPrefs.GetInt(CommunityLastSeenChatKey, 0);
                    SetCommunityUnreadVisible(postId > seenPost || replyId > seenReply || chatId > seenChat);
                }
            }
            catch { }
        }

        private static void MarkCommunityActivitySeen()
        {
            try
            {
                if (_latestKnownCommunityPostId > PlayerPrefs.GetInt(CommunityLastSeenPostKey, 0))
                    PlayerPrefs.SetInt(CommunityLastSeenPostKey, _latestKnownCommunityPostId);
                if (_latestKnownCommunityReplyId > PlayerPrefs.GetInt(CommunityLastSeenReplyKey, 0))
                    PlayerPrefs.SetInt(CommunityLastSeenReplyKey, _latestKnownCommunityReplyId);
                if (_latestKnownCommunityChatId > PlayerPrefs.GetInt(CommunityLastSeenChatKey, 0))
                    PlayerPrefs.SetInt(CommunityLastSeenChatKey, _latestKnownCommunityChatId);
                PlayerPrefs.Save();
            }
            catch { }
            SetCommunityUnreadVisible(false);
        }

        private static void SetCommunityUnreadVisible(bool visible)
        {
            _publicCommunityUnread = visible;
            RefreshCombinedUnread();
        }

        private static void RefreshCombinedUnread()
        {
            bool visible = _publicCommunityUnread || PrivateUnreadMessageIds.Count > 0 || _bugReportUnreadCount > 0;
            try { if (_communityUnreadDot != null) _communityUnreadDot.SetActive(visible); } catch { }
            try { if (_launcherUnreadDot != null) _launcherUnreadDot.gameObject.SetActive(visible); } catch { }
            RefreshPrivateChatUnreadVisuals();
            RefreshBugUnreadVisuals();
        }

        private static void RefreshPrivateChatUnreadVisuals()
        {
            bool visible = PrivateUnreadMessageIds.Count > 0;
            try
            {
                if (_privateChatTabUnreadBadgeText != null)
                {
                    _privateChatTabUnreadBadgeText.text = visible ? "●" : "";
                    _privateChatTabUnreadBadgeText.gameObject.SetActive(visible);
                }
            }
            catch { }
        }
        private static void DumpMainMenuButtons(MainMenuManager menu)
        {
            if (menu == null) return;
            try
            {
                var buttons = menu.GetComponentsInChildren<PassiveButton>(true);
                if (buttons == null) return;
                for (int i = 0; i < buttons.Length; i++)
                {
                    var button = buttons[i];
                    if (button == null || button.gameObject == null) continue;
                    string text = "";
                    var labels = button.GetComponentsInChildren<TMP_Text>(true);
                    if (labels != null)
                    {
                        for (int j = 0; j < labels.Length; j++)
                        {
                            if (labels[j] == null || string.IsNullOrWhiteSpace(labels[j].text)) continue;
                            if (text.Length > 0) text += " | ";
                            text += labels[j].text.Replace("\n", " ").Replace("\r", " ");
                        }
                    }
                    Debug.Log("[BANMOD Community] MainMenu PassiveButton: " + button.gameObject.name + " text=[" + text + "]");
                }
            }
            catch { }
        }


        internal static void ForceClose()
        {
            _chatPolling = false;
            _showingChat = false;
            _showingPrivateChat = false;
            _showingBugReport = false;

            if (_composePanel != null) _composePanel.SetActive(false);
            if (_managePanel != null) _managePanel.SetActive(false);
            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);
            if (_chatPanel != null) _chatPanel.SetActive(false);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
            if (_listPanel != null) _listPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);

            if (_root != null) _root.SetActive(false);

            BlockMainMenuInput(false);
            RefreshLauncherVisibility();
        }

        private static void BlockMainMenuInput(bool block)
        {
            try
            {
                if (block)
                {
                    if (_mainMenuInputBlocked)
                        return;

                    _mainMenuInputBlocked = true;
                    BlockedMainMenuColliders.Clear();
                    BlockedMainMenuColliderStates.Clear();
                    BlockedMainMenuButtons.Clear();
                    BlockedMainMenuButtonStates.Clear();
                    if (_mainMenuOwner == null)
                    {
                        _mainMenuInputBlocked = false;
                        return;
                    }

                    var buttons = _mainMenuOwner.GetComponentsInChildren<PassiveButton>(true);
                    if (buttons != null)
                    {
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            var b = buttons[i];
                            if (b == null) continue;
                            BlockedMainMenuButtons.Add(b);
                            BlockedMainMenuButtonStates.Add(b.enabled);
                            b.enabled = false;
                        }
                    }

                    var colliders = _mainMenuOwner.GetComponentsInChildren<BoxCollider2D>(true);
                    if (colliders != null)
                    {
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            var c = colliders[i];
                            if (c == null) continue;
                            BlockedMainMenuColliders.Add(c);
                            BlockedMainMenuColliderStates.Add(c.enabled);
                            c.enabled = false;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < BlockedMainMenuButtons.Count; i++)
                    {
                        var b = BlockedMainMenuButtons[i];
                        if (b != null && i < BlockedMainMenuButtonStates.Count)
                            b.enabled = BlockedMainMenuButtonStates[i];
                    }
                    for (int i = 0; i < BlockedMainMenuColliders.Count; i++)
                    {
                        var c = BlockedMainMenuColliders[i];
                        if (c != null && i < BlockedMainMenuColliderStates.Count)
                            c.enabled = BlockedMainMenuColliderStates[i];
                    }
                    BlockedMainMenuButtons.Clear();
                    BlockedMainMenuButtonStates.Clear();
                    BlockedMainMenuColliders.Clear();
                    BlockedMainMenuColliderStates.Clear();
                    _mainMenuInputBlocked = false;
                }
            }
            catch
            {
                if (!block)
                    _mainMenuInputBlocked = false;
            }
        }

        private static void EnsureStandaloneUi()
        {
            if (_root != null) return;

            EnsureEventSystem();

            _root = new GameObject(RootName);
            UnityEngine.Object.DontDestroyOnLoad(_root);
            Canvas canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30000;
            CanvasScaler scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            _root.AddComponent<GraphicRaycaster>();

            CreateStandaloneInputBlocker();

            _window = CreatePanel(_root.transform, "CommunityWindow", Vector2.zero, new Vector2(1460f, 920f), BanModUiStyles.WindowColor);
            CreateLabel(_window.transform, "Title", "BANMOD COMMUNITY", 34, TextAlignmentOptions.Center, BanModUiStyles.AccentHoverColor, new Vector2(0f, 418f), new Vector2(980f, 48f));
            CreateLabel(_window.transform, "Subtitle", "Private messages, mod help, bug reports, and suggestions in one place.", 15, TextAlignmentOptions.Center, new Color(0.82f, 0.84f, 0.90f, 1f), new Vector2(0f, 382f), new Vector2(960f, 28f));
            CreateButton(_window.transform, "Close", "CLOSE", new Vector2(635f, 418f), new Vector2(150f, 46f), BanModUiStyles.DangerColor, ForceClose);



            _tabPrivateChatButton = CreateButton(_window.transform, "TabPrivateChat", "PRIVATE CHAT", new Vector2(-465f, 318f), new Vector2(250f, 52f), new Color(0.22f, 0.25f, 0.48f, 1f), ShowPrivateChatTab);
            _tabChatButton = CreateButton(_window.transform, "TabCommunity", "COMMUNITY", new Vector2(-155f, 318f), new Vector2(250f, 52f), new Color(0.12f, 0.36f, 0.58f, 1f), ShowChatTab);
            _tabBugReportButton = CreateButton(_window.transform, "TabBugReport", "BUG REPORT", new Vector2(155f, 318f), new Vector2(250f, 52f), new Color(0.46f, 0.28f, 0.10f, 1f), ShowBugReportTab);
            _tabSuggestionsButton = CreateButton(_window.transform, "TabSuggestions", "SUGGESTIONS", new Vector2(465f, 318f), new Vector2(250f, 52f), new Color(0.18f, 0.42f, 0.18f, 1f), ShowSuggestionsTab);
            _privateChatTabUnreadBadgeText = CreateLabel(_window.transform, "PrivateChatTabUnread", "●", 18, TextAlignmentOptions.Center, new Color(1f, 0.12f, 0.12f, 1f), new Vector2(-360f, 336f), new Vector2(26f, 26f));
            if (_privateChatTabUnreadBadgeText != null) _privateChatTabUnreadBadgeText.gameObject.SetActive(false);
            _bugTabUnreadBadgeText = CreateLabel(_window.transform, "BugTabUnread", "●", 18, TextAlignmentOptions.Center, new Color(1f, 0.12f, 0.12f, 1f), new Vector2(260f, 336f), new Vector2(26f, 26f));
            if (_bugTabUnreadBadgeText != null) _bugTabUnreadBadgeText.gameObject.SetActive(false);

            _profileHeaderText = CreateLabel(_window.transform, "ProfileHeader", "", 15, TextAlignmentOptions.Right, new Color(0.78f, 0.82f, 0.92f, 1f), new Vector2(480f, 370f), new Vector2(300f, 30f));
            _profileMenuButton = CreateButton(_window.transform, "ProfileMenuButton", "...", new Vector2(660f, 370f), new Vector2(62f, 38f), new Color(0.18f, 0.20f, 0.28f, 1f), ToggleProfileMenu);

            _statusText = CreateLabel(_window.transform, "Status", "", 14, TextAlignmentOptions.Center, new Color(0.78f, 0.82f, 0.92f, 1f), new Vector2(0f, -431f), new Vector2(1180f, 26f));

            BuildChatPanel();
            BuildPrivateChatPanel();
            BuildBugReportPanel();
            BuildListPanel();
            BuildDetailPanel();
            BuildComposePanel();
            BuildManagePanel();
            BuildProfileMenu();

            _showingChat = true;
            _showingPrivateChat = false;
            _showingBugReport = false;
            if (_chatPanel != null) _chatPanel.SetActive(true);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
            if (_listPanel != null) _listPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);
            if (_composePanel != null) _composePanel.SetActive(false);
            if (_managePanel != null) _managePanel.SetActive(false);
            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);

            _root.SetActive(false);
            EnsureLauncherUi();
            RefreshLauncherVisibility();
            RefreshBugUnreadVisuals();
        }


        private static void EnsureEventSystem()
        {



        }


        private static void CreateStandaloneInputBlocker()
        {
            GameObject blocker = new GameObject("InputBlocker");
            blocker.transform.SetParent(_root.transform, false);
            RectTransform rect = blocker.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = blocker.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.70f);
            image.raycastTarget = true;
            Button button = blocker.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener((UnityAction)delegate { });
            CanvasGroup group = blocker.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
            blocker.transform.SetAsFirstSibling();
        }


        private static void BuildProfileMenu()
        {
            _profileMenuPanel = CreatePanel(_window.transform, "CommunityNameMenu", new Vector2(380f, 125f), new Vector2(610f, 290f), new Color(0.07f, 0.08f, 0.11f, 0.995f));
            CreateLabel(_profileMenuPanel.transform, "NameMenuTitle", "COMMUNITY NAME", 24, TextAlignmentOptions.Left, Color.white, new Vector2(-145f, 105f), new Vector2(270f, 36f));
            CreateButton(_profileMenuPanel.transform, "NameMenuClose", "×", new Vector2(270f, 108f), new Vector2(48f, 40f), new Color(0.28f, 0.28f, 0.34f, 1f), CloseProfileMenu);
            _profileNameText = CreateLabel(_profileMenuPanel.transform, "ProfileNameLabel", "", 14, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, 57f), new Vector2(540f, 34f));
            _profileInput = CreateInput(_profileMenuPanel.transform, "ProfileNameInput", "Choose / request a name...", false, new Vector2(-55f, 5f), new Vector2(390f, 48f), 20);
            _profileActionButton = CreateButton(_profileMenuPanel.transform, "ProfileSave", "SET NAME", new Vector2(205f, 5f), new Vector2(130f, 48f), new Color(0.18f, 0.42f, 0.18f, 1f), SaveProfileName);
            CreateLabel(_profileMenuPanel.transform, "ProfileRule", "After the first name is set, changes require admin approval.", 13, TextAlignmentOptions.Left, new Color(0.62f, 0.67f, 0.78f, 1f), new Vector2(0f, -48f), new Vector2(540f, 34f));
            CreateLabel(_profileMenuPanel.transform, "ProfileLoginDefault", "Default: your BanMod Login username.", 13, TextAlignmentOptions.Left, new Color(0.62f, 0.67f, 0.78f, 1f), new Vector2(0f, -78f), new Vector2(540f, 30f));
            _profileMenuPanel.SetActive(false);
        }

        private static void ToggleProfileMenu()
        {
            if (_profileMenuPanel == null) return;
            bool show = !_profileMenuPanel.activeSelf;
            _profileMenuPanel.SetActive(show);
            if (show)
            {
                _profileMenuPanel.transform.SetAsLastSibling();
                FetchProfile();
            }
        }

        private static void CloseProfileMenu()
        {
            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);
        }

        private static void BuildBugReportPanel()
        {
            _bugReportPanel = CreatePanel(_window.transform, "BugReportPanel", new Vector2(0f, -55f), new Vector2(1320f, 660f), BanModUiStyles.PanelColor);

            GameObject formPanel = CreatePanel(_bugReportPanel.transform, "NewBugPanel", new Vector2(-370f, 0f), new Vector2(540f, 610f), new Color(0.075f, 0.085f, 0.12f, 1f));
            CreateLabel(formPanel.transform, "BugFormHeading", "NEW BUG REPORT", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-105f, 264f), new Vector2(280f, 36f));
            CreateLabel(formPanel.transform, "BugPrivateInfo", "Visible only to you and BanMod administration.", 12, TextAlignmentOptions.Left, new Color(0.60f, 0.66f, 0.78f, 1f), new Vector2(0f, 235f), new Vector2(480f, 26f));

            CreateLabel(formPanel.transform, "BugTitleLabel", "TITLE", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, 196f), new Vector2(480f, 24f));
            _bugTitleInput = CreateInput(formPanel.transform, "BugTitleInput", "Short bug title...", false, new Vector2(0f, 162f), new Vector2(480f, 46f), 160);

            CreateLabel(formPanel.transform, "BugGameModeLabel", "GAME MODE", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, 119f), new Vector2(480f, 24f));
            _bugGameModeInput = CreateInput(formPanel.transform, "BugGameModeInput", "Classic, Hide N Seek, Custom...", false, new Vector2(0f, 85f), new Vector2(480f, 46f), 100);

            CreateLabel(formPanel.transform, "BugCustomLabel", "CUSTOM ROLES?", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(-118f, 39f), new Vector2(245f, 24f));
            _bugCustomYesButton = CreateButton(formPanel.transform, "BugCustomYes", "YES", new Vector2(100f, 39f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugCustomRoles(true); });
            _bugCustomNoButton = CreateButton(formPanel.transform, "BugCustomNo", "NO ✓", new Vector2(202f, 39f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugCustomRoles(false); });
            _bugCustomRolesWhichInput = CreateInput(formPanel.transform, "BugCustomWhich", "Which custom roles?", false, new Vector2(0f, -3f), new Vector2(480f, 42f), 180);
            _bugCustomRolesWhichInput.gameObject.SetActive(false);

            CreateLabel(formPanel.transform, "BugModsLabel", "OTHER MODS?", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(-118f, -50f), new Vector2(245f, 24f));
            _bugOtherYesButton = CreateButton(formPanel.transform, "BugModsYes", "YES", new Vector2(100f, -50f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugOtherMods(true); });
            _bugOtherNoButton = CreateButton(formPanel.transform, "BugModsNo", "NO ✓", new Vector2(202f, -50f), new Vector2(90f, 38f), new Color(0.20f, 0.22f, 0.28f, 1f), delegate { SetBugOtherMods(false); });
            _bugOtherModsWhichInput = CreateInput(formPanel.transform, "BugModsWhich", "Which other mods?", false, new Vector2(0f, -92f), new Vector2(480f, 42f), 180);
            _bugOtherModsWhichInput.gameObject.SetActive(false);

            CreateLabel(formPanel.transform, "BugDescLabel", "DESCRIPTION", 13, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(0f, -139f), new Vector2(480f, 24f));
            _bugDescriptionInput = CreateInput(formPanel.transform, "BugDescriptionInput", "Describe what happened and how to reproduce it...", true, new Vector2(0f, -205f), new Vector2(480f, 100f), 4000);
            _bugSendButton = CreateButton(formPanel.transform, "BugSend", "SEND BUG REPORT", new Vector2(0f, -270f), new Vector2(250f, 48f), new Color(0.14f, 0.44f, 0.25f, 1f), SubmitBugReport);

            GameObject reportsPanel = CreatePanel(_bugReportPanel.transform, "BugReportsPanel", new Vector2(310f, 0f), new Vector2(740f, 610f), new Color(0.075f, 0.085f, 0.12f, 1f));
            CreateLabel(reportsPanel.transform, "BugReportsHeading", "MY BUG REPORTS", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-170f, 264f), new Vector2(320f, 36f));
            CreateButton(reportsPanel.transform, "BugRefresh", "REFRESH", new Vector2(285f, 264f), new Vector2(130f, 42f), BanModUiStyles.ButtonColor, LoadBugReports);

            CreateBugReportsList(reportsPanel.transform, new Vector2(0f, 157f), new Vector2(680f, 155f));
            _bugSelectedHeader = CreateLabel(reportsPanel.transform, "BugSelectedHeader", "Select a bug report to view the private conversation.", 14, TextAlignmentOptions.Left, new Color(0.70f, 0.75f, 0.86f, 1f), new Vector2(0f, 56f), new Vector2(680f, 34f));
            _bugConversationText = CreateScrollableText(reportsPanel.transform, "BugConversation", new Vector2(0f, -72f), new Vector2(680f, 220f), out _bugConversationScroll, out _bugConversationContent);
            _bugReplyInput = CreateInput(reportsPanel.transform, "BugReplyInput", "Message BanMod administration...", false, new Vector2(-80f, -222f), new Vector2(500f, 50f), 2500);
            _bugReplyButton = CreateButton(reportsPanel.transform, "BugReplySend", "SEND", new Vector2(245f, -222f), new Vector2(130f, 50f), new Color(0.14f, 0.44f, 0.25f, 1f), SendBugReportReply);
            _bugCloseButton = CreateButton(reportsPanel.transform, "BugClose", "CLOSE RESOLVED", new Vector2(-150f, -274f), new Vector2(210f, 42f), new Color(0.45f, 0.32f, 0.08f, 1f), CloseSelectedBugReport);
            _bugDeleteButton = CreateButton(reportsPanel.transform, "BugDelete", "DELETE", new Vector2(95f, -274f), new Vector2(150f, 42f), BanModUiStyles.DangerColor, DeleteSelectedBugReport);

            UpdateBugReportButtons();
        }

        private static void CreateBugReportsList(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject root = new GameObject("BugReportsList");
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = pos;
            rootRect.sizeDelta = size;
            Image bg = root.AddComponent<Image>();
            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);

            ScrollRect scroll = root.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            StretchFull(viewportRect, new Vector4(8f, 8f, 8f, 8f));
            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
            vpImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            _bugReportsContent = contentGo.AddComponent<RectTransform>();
            _bugReportsContent.anchorMin = new Vector2(0f, 1f);
            _bugReportsContent.anchorMax = new Vector2(1f, 1f);
            _bugReportsContent.pivot = new Vector2(0.5f, 1f);
            _bugReportsContent.anchoredPosition = Vector2.zero;
            _bugReportsContent.sizeDelta = new Vector2(0f, 140f);

            scroll.viewport = viewportRect;
            scroll.content = _bugReportsContent;
            _bugReportsScroll = scroll;
        }

        private static void SetBugCustomRoles(bool enabled)
        {
            _bugCustomRolesEnabled = enabled;
            SetButtonText(_bugCustomYesButton, enabled ? "YES ✓" : "YES");
            SetButtonText(_bugCustomNoButton, enabled ? "NO" : "NO ✓");
            if (_bugCustomRolesWhichInput != null) _bugCustomRolesWhichInput.gameObject.SetActive(enabled);
        }

        private static void SetBugOtherMods(bool enabled)
        {
            _bugOtherModsInstalled = enabled;
            SetButtonText(_bugOtherYesButton, enabled ? "YES ✓" : "YES");
            SetButtonText(_bugOtherNoButton, enabled ? "NO" : "NO ✓");
            if (_bugOtherModsWhichInput != null) _bugOtherModsWhichInput.gameObject.SetActive(enabled);
        }

        private static void SubmitBugReport()
        {
            if (_bugRequestRunning) return;

            string title = _bugTitleInput != null ? (_bugTitleInput.text ?? "").Trim() : "";
            string gameMode = _bugGameModeInput != null ? (_bugGameModeInput.text ?? "").Trim() : "";
            string customWhich = _bugCustomRolesWhichInput != null ? (_bugCustomRolesWhichInput.text ?? "").Trim() : "";
            string modsWhich = _bugOtherModsWhichInput != null ? (_bugOtherModsWhichInput.text ?? "").Trim() : "";
            string description = _bugDescriptionInput != null ? (_bugDescriptionInput.text ?? "").Trim() : "";

            if (title.Length == 0) { SetStatus("Bug report title is required."); return; }
            if (gameMode.Length == 0) { SetStatus("Game mode is required."); return; }
            if (_bugCustomRolesEnabled && customWhich.Length == 0) { SetStatus("Specify which custom roles are enabled."); return; }
            if (_bugOtherModsInstalled && modsWhich.Length == 0) { SetStatus("Specify which other mods are installed."); return; }
            if (description.Length == 0) { SetStatus("Bug description is required."); return; }

            _bugRequestRunning = true;
            UpdateBugReportButtons();
            SetStatus("Sending bug report and logs...");

            StartRoutine(BanModCommunicationManager.SendBugReportCoroutine(
                title,
                gameMode,
                _bugCustomRolesEnabled,
                customWhich,
                _bugOtherModsInstalled,
                modsWhich,
                description,
                delegate (bool success, string result)
                {
                    _bugRequestRunning = false;
                    SetStatus(result ?? (success ? "Bug report sent." : "Could not send bug report."));
                    if (success)
                    {
                        ClearBugReportForm();
                        LoadBugReports();
                    }
                    UpdateBugReportButtons();
                }));
        }

        private static void ClearBugReportForm()
        {
            SetInputText(_bugTitleInput, "");
            SetInputText(_bugGameModeInput, "");
            SetInputText(_bugCustomRolesWhichInput, "");
            SetInputText(_bugOtherModsWhichInput, "");
            SetInputText(_bugDescriptionInput, "");
            SetBugCustomRoles(false);
            SetBugOtherMods(false);
        }

        private static void LoadBugReports()
        {
            if (_bugRequestRunning) return;
            _bugRequestRunning = true;
            UpdateBugReportButtons();
            SetStatus("Loading bug reports...");

            int keepId = _selectedBugReportId;
            StartRoutine(BanModCommunicationManager.GetMyReportsCoroutine(
                delegate (List<BanModCommunicationManager.ReportSummary> reports, string error)
                {
                    _bugRequestRunning = false;
                    BugReports.Clear();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        SetStatus(error);
                        _selectedBugReportId = 0;
                        RenderBugReportsList();
                        RenderSelectedBugReport();
                        UpdateBugReportButtons();
                        return;
                    }

                    if (reports != null)
                    {
                        for (int i = 0; i < reports.Count; i++)
                        {
                            BanModCommunicationManager.ReportSummary report = reports[i];
                            if (report == null) continue;
                            string type = report.Type ?? "";
                            if (type.IndexOf("bug", StringComparison.OrdinalIgnoreCase) >= 0)
                                BugReports.Add(report);
                        }
                    }

                    _bugReportUnreadCount = 0;
                    for (int i = 0; i < BugReports.Count; i++)
                    {
                        BanModCommunicationManager.ReportSummary report = BugReports[i];
                        if (report == null) continue;
                        if (report.UnreadCount > 0) _bugReportUnreadCount += report.UnreadCount;
                        else if (report.IsUnread) _bugReportUnreadCount++;
                    }

                    _selectedBugReportId = FindBugReport(keepId) != null
                        ? keepId
                        : (BugReports.Count > 0 ? BugReports[0].Id : 0);

                    RenderBugReportsList();
                    RenderSelectedBugReport();
                    RefreshBugUnreadVisuals();
                    RefreshCombinedUnread();
                    SetStatus(BugReports.Count + " bug report" + (BugReports.Count == 1 ? "" : "s"));
                    UpdateBugReportButtons();
                }));
        }

        private static BanModCommunicationManager.ReportSummary FindBugReport(int reportId)
        {
            if (reportId <= 0) return null;
            for (int i = 0; i < BugReports.Count; i++)
            {
                BanModCommunicationManager.ReportSummary report = BugReports[i];
                if (report != null && report.Id == reportId) return report;
            }
            return null;
        }

        private static void RenderBugReportsList()
        {
            if (_bugReportsContent == null) return;
            ClearRectChildren(_bugReportsContent);

            if (BugReports.Count == 0)
            {
                CreateLabel(_bugReportsContent, "NoBugReports", "No bug reports yet.", 14, TextAlignmentOptions.Center,
                    new Color(0.68f, 0.72f, 0.80f, 1f), new Vector2(0f, -28f), new Vector2(620f, 40f));
                _bugReportsContent.sizeDelta = new Vector2(0f, 140f);
                return;
            }

            float top = 6f;
            for (int i = 0; i < BugReports.Count; i++)
            {
                BanModCommunicationManager.ReportSummary report = BugReports[i];
                if (report == null) continue;
                int id = report.Id;
                bool selected = id == _selectedBugReportId;
                bool unread = report.IsUnread || report.UnreadCount > 0;
                string status = string.IsNullOrWhiteSpace(report.Status) ? "open" : report.Status;
                string label = (unread ? "● " : "") + "#" + id + "  " + Short(report.Title, 38) + "  [" + status + "]";
                Button row = CreateButton(
                    _bugReportsContent,
                    "BugReport_" + id,
                    label,
                    new Vector2(0f, -(top + 22f)),
                    new Vector2(640f, 44f),
                    selected ? new Color(0.12f, 0.34f, 0.50f, 0.98f) : new Color(0.13f, 0.16f, 0.22f, 0.96f),
                    delegate { SelectBugReport(id); });
                RectTransform rowRect = row != null ? row.GetComponent<RectTransform>() : null;
                if (rowRect != null)
                {
                    rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 1f);
                    rowRect.pivot = new Vector2(0.5f, 1f);
                    rowRect.anchoredPosition = new Vector2(0f, -top);
                }
                TextMeshProUGUI rowText = row != null ? row.GetComponentInChildren<TextMeshProUGUI>(true) : null;
                if (rowText != null)
                {
                    rowText.alignment = TextAlignmentOptions.MidlineLeft;
                    rowText.margin = new Vector4(12f, 0f, 8f, 0f);
                    rowText.fontSizeMax = 18f;
                }
                top += 48f;
            }

            _bugReportsContent.sizeDelta = new Vector2(0f, Math.Max(140f, top + 8f));
        }

        private static void SelectBugReport(int reportId)
        {
            BanModCommunicationManager.ReportSummary report = FindBugReport(reportId);
            if (report == null) return;
            _selectedBugReportId = reportId;
            report.IsUnread = false;
            report.UnreadCount = 0;
            SetInputText(_bugReplyInput, "");
            try { BanModMessagePoller.MarkReportReadFromUi(reportId); } catch { }
            StartRoutine(BanModCommunicationManager.MarkReportReadCoroutine(reportId));
            RecountBugUnread();
            RenderBugReportsList();
            RenderSelectedBugReport();
            RefreshBugUnreadVisuals();
            RefreshCombinedUnread();
            UpdateBugReportButtons();
        }

        private static void RecountBugUnread()
        {
            int count = 0;
            for (int i = 0; i < BugReports.Count; i++)
            {
                BanModCommunicationManager.ReportSummary report = BugReports[i];
                if (report == null) continue;
                if (report.UnreadCount > 0) count += report.UnreadCount;
                else if (report.IsUnread) count++;
            }
            _bugReportUnreadCount = Math.Max(0, count);
        }

        private static void RenderSelectedBugReport()
        {
            BanModCommunicationManager.ReportSummary report = FindBugReport(_selectedBugReportId);
            if (report == null)
            {
                if (_bugSelectedHeader != null) _bugSelectedHeader.text = "Select a bug report to view the private conversation.";
                if (_bugConversationText != null) _bugConversationText.text = "";
                if (_bugConversationContent != null) _bugConversationContent.sizeDelta = new Vector2(0f, 220f);
                UpdateBugReportButtons();
                return;
            }

            string status = string.IsNullOrWhiteSpace(report.Status) ? "open" : report.Status;
            if (_bugSelectedHeader != null)
                _bugSelectedHeader.text = "#" + report.Id + " · " + (report.Title ?? "") + " · " + status.ToUpperInvariant();

            StringBuilder builder = new StringBuilder();
            builder.Append("Private bug conversation — only you and BanMod administration.\n");
            if (!string.IsNullOrWhiteSpace(report.GameMode))
                builder.Append("Game mode: ").Append(report.GameMode).Append("\n");

            if (report.Chat != null && report.Chat.Count > 0)
            {
                for (int i = 0; i < report.Chat.Count; i++)
                {
                    BanModCommunicationManager.ReportChatMessage message = report.Chat[i];
                    if (message == null || string.IsNullOrWhiteSpace(message.Message)) continue;
                    string author = string.Equals(message.AuthorType ?? "", "admin", StringComparison.OrdinalIgnoreCase)
                        ? "BANMOD ADMIN"
                        : "YOU";
                    builder.Append("\n").Append(author);
                    string when = FormatBugTimestamp(message.CreatedAt);
                    if (!string.IsNullOrWhiteSpace(when)) builder.Append(" · ").Append(when);
                    builder.Append("\n").Append(message.Message.Trim()).Append("\n");
                }
            }
            else if (!string.IsNullOrWhiteSpace(report.Message))
            {
                builder.Append("\nYOU\n").Append(report.Message.Trim()).Append("\n");
                if (!string.IsNullOrWhiteSpace(report.AdminReply))
                    builder.Append("\nBANMOD ADMIN\n").Append(report.AdminReply.Trim()).Append("\n");
            }

            if (_bugConversationText != null)
            {
                _bugConversationText.text = builder.ToString();
                try
                {
                    _bugConversationText.ForceMeshUpdate();
                    float height = Math.Max(220f, _bugConversationText.preferredHeight + 28f);
                    if (_bugConversationContent != null) _bugConversationContent.sizeDelta = new Vector2(0f, height);
                    RectTransform rt = _bugConversationText.GetComponent<RectTransform>();
                    if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
                    Canvas.ForceUpdateCanvases();
                    if (_bugConversationScroll != null) _bugConversationScroll.verticalNormalizedPosition = 0f;
                }
                catch { }
            }

            UpdateBugReportButtons();
        }

        private static string FormatBugTimestamp(double value)
        {
            try
            {
                long seconds = Convert.ToInt64(value);
                if (seconds <= 0) return "";
                return DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }
            catch { return ""; }
        }

        private static void SendBugReportReply()
        {
            if (_bugRequestRunning || _selectedBugReportId <= 0 || _bugReplyInput == null) return;
            string message = (_bugReplyInput.text ?? "").Trim();
            if (message.Length == 0) { SetStatus("Write a message first."); return; }

            _bugRequestRunning = true;
            UpdateBugReportButtons();
            SetStatus("Sending message...");
            int id = _selectedBugReportId;

            StartRoutine(BanModCommunicationManager.SendReportMessageCoroutine(
                id,
                message,
                delegate (bool success, string result)
                {
                    _bugRequestRunning = false;
                    SetStatus(result ?? (success ? "Message sent." : "Could not send message."));
                    if (success)
                    {
                        SetInputText(_bugReplyInput, "");
                        _selectedBugReportId = id;
                        LoadBugReports();
                    }
                    else
                    {
                        UpdateBugReportButtons();
                    }
                }));
        }

        private static void CloseSelectedBugReport()
        {
            if (_bugRequestRunning || _selectedBugReportId <= 0) return;
            _bugRequestRunning = true;
            UpdateBugReportButtons();
            SetStatus("Closing bug report as resolved...");
            int id = _selectedBugReportId;

            StartRoutine(CloseBugReportDirectCoroutine(
                id,
                delegate (bool success, string result)
                {
                    _bugRequestRunning = false;
                    SetStatus(result ?? (success ? "Bug report closed as resolved." : "Could not close bug report."));
                    _selectedBugReportId = id;

                    if (success)
                    {
                        BanModCommunicationManager.ReportSummary local = FindBugReport(id);
                        if (local != null)
                            local.Status = "closed";
                        RenderBugReportsList();
                        RenderSelectedBugReport();
                        UpdateBugReportButtons();
                        LoadBugReports();
                    }
                    else
                    {
                        UpdateBugReportButtons();
                    }
                }));
        }

        private static IEnumerator CloseBugReportDirectCoroutine(int reportId, Action<bool, string> callback)
        {
            bool hasToken = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine(delegate (bool success, string token)
            {
                hasToken = success;
            });

            if (!hasToken)
            {
                callback?.Invoke(false, "Token unavailable.");
                yield break;
            }

            UnityWebRequest request = new UnityWebRequest(BanModCommunicationConfig.ReportCloseUrl(reportId), "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;
            request.SetRequestHeader("Content-Type", "application/json");
            BanModApiTokenManager.ApplyAuthHeader(request);

            yield return request.SendWebRequest();

            long code = request.responseCode;
            string body = request.downloadHandler != null ? request.downloadHandler.text : "";
            string transportError = request.error ?? "";

            if (code == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                request.Dispose();
                callback?.Invoke(false, "Unauthorized. Token cleared, try again.");
                yield break;
            }

            if (code == 404 || code == 405)
            {
                request.Dispose();
                yield return BanModCommunicationManager.CloseReportCoroutine(reportId, callback);
                yield break;
            }

            bool successHttp = code >= 200 && code < 300;
            bool success = successHttp;
            string message = successHttp ? "Report closed as resolved." : "Could not close report.";

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        JsonElement root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("success", out JsonElement successElement) &&
                                (successElement.ValueKind == JsonValueKind.True || successElement.ValueKind == JsonValueKind.False))
                                success = successHttp && successElement.GetBoolean();

                            if (root.TryGetProperty("message", out JsonElement msgElement) && msgElement.ValueKind == JsonValueKind.String)
                                message = msgElement.GetString() ?? message;
                            else if (!success && root.TryGetProperty("error", out JsonElement errElement) && errElement.ValueKind == JsonValueKind.String)
                                message = errElement.GetString() ?? message;
                        }
                    }
                }
                catch { }
            }

            if (!success && !string.IsNullOrWhiteSpace(transportError))
                message = "HTTP " + code + ": " + transportError;
            else if (!success && code > 0 && message.IndexOf("HTTP ", StringComparison.OrdinalIgnoreCase) < 0)
                message = "HTTP " + code + ": " + message;

            request.Dispose();
            callback?.Invoke(success, message);
        }

        private static void DeleteSelectedBugReport()
        {
            if (_bugRequestRunning || _selectedBugReportId <= 0) return;
            _bugRequestRunning = true;
            UpdateBugReportButtons();
            SetStatus("Deleting bug report...");
            int id = _selectedBugReportId;

            StartRoutine(BanModCommunicationManager.DeleteReportCoroutine(
                id,
                delegate (bool success, string result)
                {
                    _bugRequestRunning = false;
                    SetStatus(result ?? (success ? "Bug report deleted." : "Could not delete bug report."));
                    if (success) _selectedBugReportId = 0;
                    LoadBugReports();
                }));
        }

        private static void UpdateBugReportButtons()
        {
            BanModCommunicationManager.ReportSummary report = FindBugReport(_selectedBugReportId);
            bool selected = report != null;
            string status = selected ? (report.Status ?? "") : "";
            bool open = selected &&
                !string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "resolved", StringComparison.OrdinalIgnoreCase);

            if (_bugSendButton != null) _bugSendButton.interactable = !_bugRequestRunning;
            if (_bugReplyButton != null) _bugReplyButton.interactable = !_bugRequestRunning && open;
            if (_bugCloseButton != null) _bugCloseButton.interactable = !_bugRequestRunning && open;
            if (_bugDeleteButton != null) _bugDeleteButton.interactable = !_bugRequestRunning && selected;
            if (_bugReplyInput != null) _bugReplyInput.interactable = !_bugRequestRunning && open;
        }

        internal static void SetBugReportUnreadCount(int count)
        {
            _bugReportUnreadCount = Math.Max(0, count);
            RefreshBugUnreadVisuals();
            RefreshCombinedUnread();
        }

        private static void RefreshBugUnreadVisuals()
        {
            bool visible = _bugReportUnreadCount > 0;
            try
            {
                if (_bugTabUnreadBadgeText != null)
                {
                    _bugTabUnreadBadgeText.text = visible ? "●" : "";
                    _bugTabUnreadBadgeText.gameObject.SetActive(visible);
                }
            }
            catch { }
        }

        internal static void ToggleBugReportFromF3()
        {
            EnsureStandaloneUi();
            if (_root == null) return;

            if (_root.activeSelf && _showingBugReport)
            {
                ForceClose();
                return;
            }

            BlockMainMenuInput(true);
            _root.SetActive(true);
            FetchProfile();
            ShowBugReportTab();
            RefreshLauncherVisibility();
        }

        internal static void OpenBugReportNotification(BanModCommunicationManager.ReportSummary report, Action onClose)
        {
            EnsureStandaloneUi();
            if (_root == null) return;

            BlockMainMenuInput(true);
            _root.SetActive(true);
            FetchProfile();

            if (report != null && report.Id > 0)
            {
                bool replaced = false;
                for (int i = 0; i < BugReports.Count; i++)
                {
                    if (BugReports[i] != null && BugReports[i].Id == report.Id)
                    {
                        BugReports[i] = report;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) BugReports.Insert(0, report);
                _selectedBugReportId = report.Id;
            }

            _showingChat = false;
            _showingPrivateChat = false;
            _showingBugReport = true;
            HideSectionOverlays();
            if (_chatPanel != null) _chatPanel.SetActive(false);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(true);
            if (_listPanel != null) _listPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);

            if (report != null)
            {
                report.IsUnread = false;
                report.UnreadCount = 0;
                try { BanModMessagePoller.MarkReportReadFromUi(report.Id); } catch { }
                RecountBugUnread();
                RenderBugReportsList();
                RenderSelectedBugReport();
            }

            RefreshCombinedUnread();
            RefreshLauncherVisibility();
            LoadBugReports();

            try { onClose?.Invoke(); } catch { }
        }

        private static void TryUseLoginUsernameAsDefault()
        {
            if (!_profileStateLoaded || _profileNameLocked || _profileDefaultAttempted || _profileDefaultRequestRunning)
                return;

            string loginName = (_privateLoginUsername ?? "").Trim();
            if (loginName.Length < 3 || loginName.Length > 20)
                return;

            _profileDefaultAttempted = true;
            _profileDefaultRequestRunning = true;
            var payload = new Dictionary<string, string> { { "display_name", loginName } };

            StartRoutine(SendRequest("POST", CommunityUrl("/profile"), payload, delegate (bool ok, string body)
            {
                _profileDefaultRequestRunning = false;
                if (ok)
                {
                    SetStatus("Community name set from your BanMod Login username.");
                    FetchProfile();
                    if (_showingChat) FetchChat();
                }
                else
                {


                    SetStatus("Could not use your Login username as the Community name: " + ErrorText(body));
                    FetchProfile();
                }
            }));
        }


        private static void BuildChatPanel()
        {
            _chatPanel = CreatePanel(_window.transform, "HelpDiscussionPanel", new Vector2(0f, -55f), new Vector2(1320f, 660f), BanModUiStyles.PanelColor);
            CreateLabel(_chatPanel.transform, "ChatHeading", "MOD HELP & DISCUSSION", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-455f, 286f), new Vector2(360f, 36f));
            CreateLabel(_chatPanel.transform, "ChatRetention", "Ask questions about BanMod, get help, and talk with the community. Messages are kept for 3 days.", 13, TextAlignmentOptions.Right, new Color(0.58f, 0.64f, 0.76f, 1f), new Vector2(235f, 286f), new Vector2(650f, 28f));
            CreateButton(_chatPanel.transform, "CommunityRefresh", "REFRESH", new Vector2(545f, 286f), new Vector2(130f, 42f), BanModUiStyles.ButtonColor, FetchChat);
            CreateChatMessageList(_chatPanel.transform, new Vector2(0f, 42f), new Vector2(1210f, 425f));
            _reportChatButton = CreateButton(_chatPanel.transform, "ReportChat", "REPORT SELECTED", new Vector2(-485f, -208f), new Vector2(210f, 44f), new Color(0.45f, 0.14f, 0.16f, 1f), ReportSelectedChatMessage);
            _chatTranslateButton = CreateButton(_chatPanel.transform, "TranslateChat", "TRANSLATE", new Vector2(-275f, -208f), new Vector2(180f, 44f), new Color(0.14f, 0.34f, 0.52f, 1f), TranslateSelectedCommunityChatMessage);
            _chatTagButton = CreateButton(_chatPanel.transform, "TagChat", "TAG", new Vector2(-100f, -208f), new Vector2(130f, 44f), new Color(0.24f, 0.32f, 0.48f, 1f), TagSelectedChatMessage);
            _chatReplyButton = CreateButton(_chatPanel.transform, "ReplyChat", "REPLY", new Vector2(45f, -208f), new Vector2(150f, 44f), new Color(0.18f, 0.38f, 0.28f, 1f), ReplySelectedChatMessage);
            CreateLabel(_chatPanel.transform, "SelectHint", "Select a message, then TAG / REPLY / TRANSLATE.", 12, TextAlignmentOptions.Left, new Color(0.62f, 0.68f, 0.80f, 1f), new Vector2(350f, -208f), new Vector2(400f, 30f));
            _chatInput = CreateInput(_chatPanel.transform, "ChatInput", "Write a message...", true, new Vector2(-110f, -274f), new Vector2(850f, 56f), CommunityChatCharacterLimit);
            if (_chatInput != null)
                _chatInput.lineType = TMP_InputField.LineType.MultiLineSubmit;
            BindSubmitAction(_chatInput, SubmitChat);
            BanModSymbolPicker.AttachToTmpInput(
                _chatPanel.transform,
                "CommunitySymbolPicker",
                _chatInput,
                new Vector2(350f, -274f),
                new Vector2(56f, 56f),
                true);
            _chatSendButton = CreateButton(_chatPanel.transform, "ChatSend", "SEND", new Vector2(515f, -274f), new Vector2(150f, 56f), new Color(0.15f, 0.42f, 0.28f, 1f), SubmitChat);
            if (_reportChatButton != null) _reportChatButton.interactable = false;
            if (_chatTranslateButton != null) _chatTranslateButton.interactable = false;
            if (_chatTagButton != null) _chatTagButton.interactable = false;
            if (_chatReplyButton != null) _chatReplyButton.interactable = false;
        }

        private static void CreateChatMessageList(Transform parent, Vector2 pos, Vector2 size)
        {
            _chatListRoot = new GameObject("ChatMessageList");
            _chatListRoot.transform.SetParent(parent, false);
            RectTransform rootRect = _chatListRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = pos;
            rootRect.sizeDelta = size;
            Image bg = _chatListRoot.AddComponent<Image>();
            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
            _chatScroll = _chatListRoot.AddComponent<ScrollRect>();
            _chatScroll.horizontal = false;
            _chatScroll.vertical = true;
            _chatScroll.movementType = ScrollRect.MovementType.Clamped;
            _chatScroll.inertia = true;
            _chatScroll.decelerationRate = 0.12f;


            _chatScroll.scrollSensitivity = 0f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(_chatListRoot.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            StretchFull(viewportRect, new Vector4(10f, 10f, 10f, 10f));
            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
            vpImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            _chatContent = contentGo.AddComponent<RectTransform>();
            _chatContent.anchorMin = new Vector2(0f, 1f);
            _chatContent.anchorMax = new Vector2(1f, 1f);
            _chatContent.pivot = new Vector2(0.5f, 1f);
            _chatContent.anchoredPosition = Vector2.zero;
            _chatContent.sizeDelta = new Vector2(0f, 410f);
            _chatScroll.viewport = viewportRect;
            _chatScroll.content = _chatContent;
        }

        private static void BuildPrivateChatPanel()
        {
            _privateChatPanel = CreatePanel(_window.transform, "PrivateChatPanel", new Vector2(0f, -55f), new Vector2(1320f, 660f), BanModUiStyles.PanelColor);
            CreateLabel(_privateChatPanel.transform, "PrivateHeading", "PRIVATE CHAT", 23, TextAlignmentOptions.Left, Color.white, new Vector2(-500f, 286f), new Vector2(300f, 36f));
            CreateLabel(_privateChatPanel.transform, "PrivateHint", "Select an online user. Conversations are private between the two users.", 13, TextAlignmentOptions.Right, new Color(0.58f, 0.64f, 0.76f, 1f), new Vector2(230f, 286f), new Vector2(610f, 28f));
            CreateButton(_privateChatPanel.transform, "PrivateRefresh", "REFRESH", new Vector2(545f, 286f), new Vector2(130f, 42f), BanModUiStyles.ButtonColor, RefreshPrivateChat);

            CreateLabel(_privateChatPanel.transform, "PrivatePlayersHeading", "ONLINE USERS", 15, TextAlignmentOptions.Left, new Color(0.72f, 0.77f, 0.88f, 1f), new Vector2(-475f, 246f), new Vector2(250f, 28f));
            CreatePrivatePlayersList(_privateChatPanel.transform, new Vector2(-465f, -15f), new Vector2(300f, 475f));

            _privateConversationTitle = CreateLabel(_privateChatPanel.transform, "PrivateConversationTitle", "SELECT AN ONLINE USER", 17, TextAlignmentOptions.Left, Color.white, new Vector2(125f, 246f), new Vector2(760f, 30f));
            CreatePrivateMessagesList(_privateChatPanel.transform, new Vector2(150f, 35f), new Vector2(800f, 370f));

            _privateChatInput = CreateInput(_privateChatPanel.transform, "PrivateInput", "Select an online user...", false, new Vector2(35f, -205f), new Vector2(550f, 54f), 300);
            BindSubmitAction(_privateChatInput, SubmitPrivateChatMessage);
            BanModSymbolPicker.AttachToTmpInput(
                _privateChatPanel.transform,
                "PrivateSymbolPicker",
                _privateChatInput,
                new Vector2(350f, -205f),
                new Vector2(54f, 54f),
                true);
            CreateButton(_privateChatPanel.transform, "PrivateSend", "SEND", new Vector2(490f, -205f), new Vector2(140f, 54f), new Color(0.15f, 0.42f, 0.28f, 1f), SubmitPrivateChatMessage);

            CreateLabel(
                _privateChatPanel.transform,
                "PrivateSelectHint",
                "Select a message. TRANSLATE also works when LiveTranslator is OFF.",
                12,
                TextAlignmentOptions.Left,
                new Color(0.58f, 0.62f, 0.72f, 1f),
                new Vector2(-90f, -270f),
                new Vector2(430f, 28f));

            _privateTranslateButton = CreateButton(
                _privateChatPanel.transform,
                "PrivateTranslate",
                "TRANSLATE",
                new Vector2(215f, -270f),
                new Vector2(170f, 42f),
                new Color(0.14f, 0.34f, 0.52f, 1f),
                TranslateSelectedPrivateMessage);

            _privateReportButton = CreateButton(
                _privateChatPanel.transform,
                "PrivateReport",
                "REPORT",
                new Vector2(430f, -270f),
                new Vector2(190f, 42f),
                new Color(0.45f, 0.14f, 0.16f, 1f),
                ReportSelectedPrivateMessage);

            if (_privateChatInput != null) _privateChatInput.interactable = false;
            if (_privateTranslateButton != null) _privateTranslateButton.interactable = false;
            if (_privateReportButton != null) _privateReportButton.interactable = false;
        }

        private static void RefreshPrivateChat()
        {
            RequestPrivateChatImmediateRefresh();
            SetStatus("Refreshing Private Chat...");
        }

        private static void CreatePrivatePlayersList(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject root = new GameObject("PrivatePlayersList");
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = pos;
            rootRect.sizeDelta = size;
            Image bg = root.AddComponent<Image>();
            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
            ScrollRect scroll = root.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            StretchFull(viewportRect, new Vector4(8f, 8f, 8f, 8f));
            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
            vpImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            _privatePlayersContent = contentGo.AddComponent<RectTransform>();
            _privatePlayersContent.anchorMin = new Vector2(0f, 1f);
            _privatePlayersContent.anchorMax = new Vector2(1f, 1f);
            _privatePlayersContent.pivot = new Vector2(0.5f, 1f);
            _privatePlayersContent.anchoredPosition = Vector2.zero;
            _privatePlayersContent.sizeDelta = new Vector2(0f, 440f);

            scroll.viewport = viewportRect;
            scroll.content = _privatePlayersContent;
            _privatePlayersScroll = scroll;
        }

        private static void CreatePrivateMessagesList(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject root = new GameObject("PrivateMessagesList");
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = pos;
            rootRect.sizeDelta = size;
            Image bg = root.AddComponent<Image>();
            ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
            ScrollRect scroll = root.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            StretchFull(viewportRect, new Vector4(10f, 10f, 10f, 10f));
            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
            vpImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            _privateMessagesContent = contentGo.AddComponent<RectTransform>();
            _privateMessagesContent.anchorMin = new Vector2(0f, 1f);
            _privateMessagesContent.anchorMax = new Vector2(1f, 1f);
            _privateMessagesContent.pivot = new Vector2(0.5f, 1f);
            _privateMessagesContent.anchoredPosition = Vector2.zero;
            _privateMessagesContent.sizeDelta = new Vector2(0f, 340f);

            scroll.viewport = viewportRect;
            scroll.content = _privateMessagesContent;
            _privateMessagesScroll = scroll;
        }

        private static void HideSectionOverlays()
        {
            if (_composePanel != null) _composePanel.SetActive(false);
            if (_managePanel != null) _managePanel.SetActive(false);
            if (_profileMenuPanel != null) _profileMenuPanel.SetActive(false);
        }

        private static void ShowChatTab()
        {
            _showingChat = true;
            _showingPrivateChat = false;
            _showingBugReport = false;
            HideSectionOverlays();
            if (_chatPanel != null) _chatPanel.SetActive(true);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
            if (_listPanel != null) _listPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);
            FetchChat();
        }

        private static void ShowPrivateChatTab()
        {
            _showingChat = false;
            _showingPrivateChat = true;
            _showingBugReport = false;
            HideSectionOverlays();
            if (_chatPanel != null) _chatPanel.SetActive(false);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(true);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
            if (_listPanel != null) _listPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);
            RequestPrivateChatImmediateRefresh();
            MarkActivePrivateConversationRead();
            RenderPrivatePlayers();
            RenderPrivateMessages(true);
            SetStatus(string.IsNullOrWhiteSpace(_privateActiveFriendCode)
                ? "Private Chat · select an online user."
                : "Private Chat with " + _privateActiveName);
        }

        private static void ShowBugReportTab()
        {
            _showingChat = false;
            _showingPrivateChat = false;
            _showingBugReport = true;
            HideSectionOverlays();
            if (_chatPanel != null) _chatPanel.SetActive(false);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(true);
            if (_listPanel != null) _listPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);
            LoadBugReports();
        }

        private static void ShowSuggestionsTab()
        {
            _showingChat = false;
            _showingPrivateChat = false;
            _showingBugReport = false;
            HideSectionOverlays();
            if (_chatPanel != null) _chatPanel.SetActive(false);
            if (_privateChatPanel != null) _privateChatPanel.SetActive(false);
            if (_bugReportPanel != null) _bugReportPanel.SetActive(false);
            if (_listPanel != null) _listPanel.SetActive(true);
            if (_detailPanel != null) _detailPanel.SetActive(true);
            FetchPosts();
        }

        private static void RefreshActive()
        {
            FetchProfile();
            if (_showingPrivateChat)
            {
                RefreshPrivateChat();
            }
            else if (_showingBugReport)
            {
                LoadBugReports();
            }
            else if (_showingChat)
            {
                FetchChat();
            }
            else
            {
                FetchPosts();
            }
        }

        private static void BuildListPanel()
        {
            _listPanel = CreatePanel(_window.transform, "ListPanel", new Vector2(-385f, -55f), new Vector2(520f, 660f), BanModUiStyles.PanelColor);
            CreateLabel(_listPanel.transform, "ListHeading", "SUGGESTIONS", 22, TextAlignmentOptions.Left, Color.white, new Vector2(-135f, 286f), new Vector2(230f, 36f));
            CreateButton(_listPanel.transform, "SuggestionsRefresh", "REFRESH", new Vector2(55f, 286f), new Vector2(110f, 42f), BanModUiStyles.ButtonColor, FetchPosts);
            _newPostButton = CreateButton(_listPanel.transform, "NewSuggestion", "+ NEW", new Vector2(185f, 286f), new Vector2(110f, 42f), new Color(0.18f, 0.42f, 0.18f, 1f), ShowCompose);
            CreateSuggestionsListScroll(_listPanel.transform, new Vector2(0f, -12f), new Vector2(470f, 520f));
        }

        private static void CreateSuggestionsListScroll(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject root = new GameObject("SuggestionsListScroll");
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = pos;
            rootRect.sizeDelta = size;
            Image bg = root.AddComponent<Image>();
            ApplyModernImage(bg, new Color(0.075f, 0.085f, 0.12f, 0.96f), true);

            ScrollRect scroll = root.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 34f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            StretchFull(viewportRect, new Vector4(8f, 8f, 8f, 8f));
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            _suggestionsListContent = contentGo.AddComponent<RectTransform>();
            _suggestionsListContent.anchorMin = new Vector2(0f, 1f);
            _suggestionsListContent.anchorMax = new Vector2(1f, 1f);
            _suggestionsListContent.pivot = new Vector2(0.5f, 1f);
            _suggestionsListContent.anchoredPosition = Vector2.zero;
            _suggestionsListContent.sizeDelta = new Vector2(0f, 500f);

            scroll.viewport = viewportRect;
            scroll.content = _suggestionsListContent;
            _suggestionsListScroll = scroll;
        }

        private static void BuildDetailPanel()
        {
            _detailPanel = CreatePanel(_window.transform, "DetailPanel", new Vector2(305f, -55f), new Vector2(740f, 660f), BanModUiStyles.PanelColor);
            CreateLabel(_detailPanel.transform, "DetailHeading", "DISCUSSION", 22, TextAlignmentOptions.Left, Color.white, new Vector2(-205f, 286f), new Vector2(280f, 36f));
            _detailText = CreateScrollableText(_detailPanel.transform, "DiscussionScroll", new Vector2(0f, 80f), new Vector2(680f, 360f), out _detailScroll, out _detailContent);
            _replyInput = CreateInput(_detailPanel.transform, "ReplyInput", "Write a reply...", true, new Vector2(-75f, -145f), new Vector2(480f, 82f), 2500);
            CreateButton(_detailPanel.transform, "ReplySend", "REPLY", new Vector2(260f, -145f), new Vector2(130f, 54f), new Color(0.15f, 0.42f, 0.28f, 1f), SubmitReply);
            CreateButton(_detailPanel.transform, "ReplyPrev", "<", new Vector2(-315f, -238f), new Vector2(54f, 42f), new Color(0.15f, 0.17f, 0.25f, 1f), SelectPreviousReply);
            CreateButton(_detailPanel.transform, "ReplyNext", ">", new Vector2(-250f, -238f), new Vector2(54f, 42f), new Color(0.15f, 0.17f, 0.25f, 1f), SelectNextReply);
            _reportReplyButton = CreateButton(_detailPanel.transform, "ReportReply", "REPORT COMMENT", new Vector2(-110f, -238f), new Vector2(185f, 42f), new Color(0.45f, 0.14f, 0.16f, 1f), ReportSelectedReply);
            _starButton = CreateButton(_detailPanel.transform, "Star", "☆ STAR", new Vector2(105f, -238f), new Vector2(155f, 42f), new Color(0.46f, 0.36f, 0.10f, 1f), ToggleStar);
            _suggestionTranslateButton = CreateButton(_detailPanel.transform, "TranslateSuggestion", "TRANSLATE POST", new Vector2(285f, -238f), new Vector2(160f, 42f), new Color(0.14f, 0.34f, 0.52f, 1f), TranslateSelectedSuggestionContent);
            CreateLabel(_detailPanel.transform, "ReportHint", "Use < / > to select a comment. TRANSLATE works even when LiveTranslator is OFF.", 12, TextAlignmentOptions.Left, new Color(0.58f, 0.62f, 0.72f, 1f), new Vector2(0f, -294f), new Vector2(660f, 26f));
            _starButton.interactable = false;
            if (_reportReplyButton != null) _reportReplyButton.interactable = false;
            if (_suggestionTranslateButton != null) _suggestionTranslateButton.interactable = false;
        }

        private static void BuildComposePanel()
        {
            _composePanel = CreatePanel(_window.transform, "ComposePanel", new Vector2(0f, -55f), new Vector2(1180f, 660f), BanModUiStyles.PanelColor);
            CreateLabel(_composePanel.transform, "ComposeTitle", "NEW SUGGESTION", 30, TextAlignmentOptions.Left, Color.white, new Vector2(-325f, 280f), new Vector2(520f, 46f));
            _titleInput = CreateInput(_composePanel.transform, "TitleInput", "Suggestion title...", false, new Vector2(0f, 195f), new Vector2(900f, 58f), 120);
            _messageInput = CreateInput(_composePanel.transform, "MessageInput", "Describe your suggestion...", true, new Vector2(0f, 20f), new Vector2(900f, 240f), 4000);
            CreateLabel(_composePanel.transform, "AnonInfo", "Your Community name is shown publicly. Your FriendCode/account identity is visible only to moderators.", 14, TextAlignmentOptions.Left, new Color(0.72f, 0.76f, 0.86f, 1f), new Vector2(0f, -135f), new Vector2(900f, 42f));
            CreateButton(_composePanel.transform, "Publish", "PUBLISH", new Vector2(125f, -265f), new Vector2(210f, 54f), new Color(0.14f, 0.44f, 0.25f, 1f), SubmitPost);
            CreateButton(_composePanel.transform, "Cancel", "CANCEL", new Vector2(-125f, -265f), new Vector2(210f, 54f), new Color(0.27f, 0.28f, 0.34f, 1f), delegate { _composePanel.SetActive(false); });
            _composePanel.SetActive(false);
        }

        private static void BuildManagePanel()
        {
            _managePanel = CreatePanel(_window.transform, "ManageOwnContent", new Vector2(0f, -55f), new Vector2(980f, 620f), new Color(0.075f, 0.085f, 0.12f, 1f));
            _manageHeading = CreateLabel(_managePanel.transform, "ManageHeading", "EDIT", 28, TextAlignmentOptions.Left, Color.white, new Vector2(-225f, 250f), new Vector2(420f, 44f));
            _manageTitleInput = CreateInput(_managePanel.transform, "ManageTitle", "Title...", false, new Vector2(0f, 165f), new Vector2(720f, 56f), 120);
            _manageMessageInput = CreateInput(_managePanel.transform, "ManageMessage", "Message...", true, new Vector2(0f, 10f), new Vector2(720f, 260f), 4000);
            if (_manageMessageInput != null)
            {
                _manageMessageInput.onValueChanged.AddListener((UnityAction<string>)delegate (string value)
                {
                    if (_manageMode != "chat") return;
                    int length = (value ?? "").Length;
                    SetStatus("Editing message: " + length + "/" + CommunityChatCharacterLimit + " characters.");
                });
            }
            CreateLabel(_managePanel.transform, "ManageInfo", "Only your messages can be edited or deleted. Community limit: 1000 characters.", 13, TextAlignmentOptions.Left, new Color(0.68f, 0.73f, 0.84f, 1f), new Vector2(0f, -155f), new Vector2(720f, 30f));
            CreateButton(_managePanel.transform, "ManageSave", "SAVE", new Vector2(190f, -245f), new Vector2(170f, 52f), new Color(0.14f, 0.44f, 0.25f, 1f), SaveManagedContent);
            CreateButton(_managePanel.transform, "ManageDelete", "DELETE", new Vector2(0f, -245f), new Vector2(170f, 52f), BanModUiStyles.DangerColor, DeleteManagedContent);
            CreateButton(_managePanel.transform, "ManageCancel", "CANCEL", new Vector2(-190f, -245f), new Vector2(170f, 52f), new Color(0.27f, 0.28f, 0.34f, 1f), CloseManagePanel);
            _managePanel.SetActive(false);
        }

        private static void ShowCompose()
        {
            if (_composePanel == null) return;
            SetInputText(_titleInput, "");
            SetInputText(_messageInput, "");
            _composePanel.SetActive(true);
        }

        private static void FetchProfile()
        {
            StartRoutine(SendRequest("GET", CommunityUrl("/profile"), null, delegate (bool ok, string body)
            {
                if (!ok) return;
                try
                {
                    using (var doc = JsonDocument.Parse(body))
                    {
                        JsonElement profile;
                        if (!doc.RootElement.TryGetProperty("profile", out profile)) return;
                        JsonElement name;
                        _communityDisplayName = profile.TryGetProperty("display_name", out name) ? (name.GetString() ?? "") : "";

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(_communityDisplayName))
                            {
                                PlayerPrefs.SetString(CommunityDisplayNameCacheKey, _communityDisplayName.Trim());
                                PlayerPrefs.Save();
                            }
                        }
                        catch { }

                        JsonElement locked;
                        _profileNameLocked = profile.TryGetProperty("name_locked", out locked) && locked.ValueKind == JsonValueKind.True;
                        JsonElement pending;
                        _profilePendingName = profile.TryGetProperty("pending_display_name", out pending) && pending.ValueKind == JsonValueKind.String
                            ? (pending.GetString() ?? "") : "";
                        _profileStateLoaded = true;

                        if (_profileHeaderText != null)
                            _profileHeaderText.text = string.IsNullOrWhiteSpace(_communityDisplayName)
                                ? "COMMUNITY NAME"
                                : _communityDisplayName;

                        if (!_profileNameLocked)
                        {
                            if (_profileNameText != null)
                                _profileNameText.text = "Choose your Community name once. Your Login username is used by default.";
                            if (_profileInput != null) _profileInput.interactable = true;
                            SetInputText(_profileInput,
                                !string.IsNullOrWhiteSpace(_privateLoginUsername) ? _privateLoginUsername : _communityDisplayName);
                            SetButtonText(_profileActionButton, "SET NAME");
                            if (_profileActionButton != null) _profileActionButton.interactable = true;
                        }
                        else if (!string.IsNullOrWhiteSpace(_profilePendingName))
                        {
                            if (_profileNameText != null)
                                _profileNameText.text = "CURRENT: " + _communityDisplayName + " · CHANGE REQUEST PENDING";
                            SetInputText(_profileInput, _profilePendingName);
                            if (_profileInput != null) _profileInput.interactable = false;
                            SetButtonText(_profileActionButton, "PENDING");
                            if (_profileActionButton != null) _profileActionButton.interactable = false;
                        }
                        else
                        {
                            if (_profileNameText != null)
                                _profileNameText.text = "CURRENT: " + _communityDisplayName + " · LOCKED";
                            SetInputText(_profileInput, "");
                            if (_profileInput != null) _profileInput.interactable = true;
                            SetButtonText(_profileActionButton, "REQUEST CHANGE");
                            if (_profileActionButton != null) _profileActionButton.interactable = true;
                        }

                        TryUseLoginUsernameAsDefault();
                    }
                }
                catch { }
            }));
        }

        private static void SaveProfileName()
        {
            if (_requestRunning || _profileInput == null) return;
            string name = (_profileInput.text ?? "").Trim();
            if (name.Length < 3) { SetStatus("Community name must be at least 3 characters."); return; }
            _requestRunning = true;
            var payload = new Dictionary<string, string> { { "display_name", name } };
            string endpoint = _profileNameLocked ? "/profile/change-request" : "/profile";
            StartRoutine(SendRequest("POST", CommunityUrl(endpoint), payload, delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok)
                {
                    SetStatus((_profileNameLocked ? "Name-change request failed: " : "Name setup failed: ") + ErrorText(body));
                    FetchProfile();
                    return;
                }
                SetStatus(_profileNameLocked ? "Name-change request sent for admin approval." : "Community name saved and locked.");
                FetchProfile();
                if (_showingChat) FetchChat();
                else if (!_showingPrivateChat && !_showingBugReport) FetchPosts();
            }));
        }

        private static void SetChatSendButtonText(string text)
        {
            if (_chatSendButton == null) return;
            try
            {
                TextMeshProUGUI label = _chatSendButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null) label.text = text ?? "SEND";
            }
            catch { }
        }

        private static void BeginInlineChatEdit(CommunityChatMessage message)
        {
            if (message == null || !message.MyMessage || _chatInput == null)
                return;

            if (_editingChatMessageId <= 0)
                _chatDraftBeforeEdit = _chatInput.text ?? "";

            _editingChatMessageId = message.Id;
            _selectedChatIndex = -1;

            _chatInput.characterLimit = CommunityChatCharacterLimit;
            _chatInput.lineType = TMP_InputField.LineType.MultiLineSubmit;
            _chatInput.contentType = TMP_InputField.ContentType.Standard;
            _chatInput.inputType = TMP_InputField.InputType.Standard;
            _chatInput.richText = false;
            _chatInput.enabled = true;
            _chatInput.interactable = true;
            _chatInput.readOnly = false;

            SetInputText(_chatInput, message.Message ?? "");
            SetChatSendButtonText("SAVE");

            if (_chatTagButton != null) _chatTagButton.interactable = false;
            if (_chatReplyButton != null) _chatReplyButton.interactable = false;
            if (_chatTranslateButton != null) _chatTranslateButton.interactable = false;

            SetStatus("EDIT MODE · " +
                (_chatInput.text ?? "").Length + "/" +
                CommunityChatCharacterLimit +
                " characters · SAVE to apply changes");

            StartRoutine(FocusInlineChatEditRoutine());
        }

        private static IEnumerator FocusInlineChatEditRoutine()
        {
            yield return null;

            if (_editingChatMessageId <= 0 || _chatInput == null)
                yield break;

            try
            {
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_chatInput.gameObject);
                }
            }
            catch { }

            try
            {
                _chatInput.enabled = true;
                _chatInput.interactable = true;
                _chatInput.readOnly = false;
                _chatInput.ActivateInputField();
                _chatInput.ForceLabelUpdate();

                int end = (_chatInput.text ?? "").Length;
                _chatInput.caretPosition = end;
                _chatInput.selectionAnchorPosition = end;
                _chatInput.selectionFocusPosition = end;
                try { _chatInput.MoveTextEnd(false); } catch { }
            }
            catch { }

            yield return null;

            if (_editingChatMessageId <= 0 || _chatInput == null)
                yield break;

            try
            {
                _chatInput.ActivateInputField();
                int end = (_chatInput.text ?? "").Length;
                _chatInput.caretPosition = end;
                _chatInput.selectionAnchorPosition = end;
                _chatInput.selectionFocusPosition = end;
            }
            catch { }
        }

        private static void EndInlineChatEdit(bool restoreDraft)
        {
            string draft = restoreDraft ? (_chatDraftBeforeEdit ?? "") : "";

            _editingChatMessageId = 0;
            _chatDraftBeforeEdit = "";

            SetChatSendButtonText("SEND");

            if (_chatInput != null)
            {
                _chatInput.characterLimit = CommunityChatCharacterLimit;
                _chatInput.lineType = TMP_InputField.LineType.MultiLineSubmit;
                SetInputText(_chatInput, draft);
            }

            RenderChat(false);
        }

        private static void SaveInlineChatEdit()
        {
            if (_editingChatMessageId <= 0 || _chatInput == null || _manageRequestRunning)
                return;

            string message = (_chatInput.text ?? "").Trim();

            if (message.Length == 0)
            {
                SetStatus("Message cannot be empty.");
                return;
            }

            if (message.Length > CommunityChatCharacterLimit)
            {
                SetStatus("Community messages are limited to " +
                    CommunityChatCharacterLimit +
                    " characters. Current length: " + message.Length + ".");
                return;
            }

            int messageId = _editingChatMessageId;
            var payload = new Dictionary<string, string> { { "message", message } };

            _manageRequestRunning = true;
            SetStatus("Saving edited message...");

            StartRoutine(SendRequest(
                "PATCH",
                CommunityUrl("/chat/" + messageId),
                payload,
                delegate (bool ok, string body)
                {
                    _manageRequestRunning = false;

                    if (!ok)
                    {
                        SetStatus("Edit failed: " + ErrorText(body));
                        return;
                    }

                    EndInlineChatEdit(true);
                    SetStatus("Message edited.");
                    FetchChat(false);
                }));
        }

        private static void OpenManageChat(CommunityChatMessage message)
        {
            if (message == null || !message.MyMessage || _managePanel == null)
                return;

            _manageMode = "chat";
            _manageTargetId = message.Id;
            _manageRequestRunning = false;

            if (_manageHeading != null)
                _manageHeading.text = "EDIT YOUR MESSAGE";

            if (_manageTitleInput != null)
                _manageTitleInput.gameObject.SetActive(false);

            _managePanel.SetActive(true);
            _managePanel.transform.SetAsLastSibling();

            if (_manageMessageInput != null)
            {
                _manageMessageInput.gameObject.SetActive(true);
                _manageMessageInput.enabled = true;
                _manageMessageInput.interactable = true;
                _manageMessageInput.readOnly = false;

                _manageMessageInput.characterLimit = CommunityChatCharacterLimit;
                _manageMessageInput.contentType = TMP_InputField.ContentType.Standard;
                _manageMessageInput.inputType = TMP_InputField.InputType.Standard;
                _manageMessageInput.lineType = TMP_InputField.LineType.MultiLineNewline;
                _manageMessageInput.richText = false;

                if (_manageMessageInput.textComponent != null)
                {
                    _manageMessageInput.textComponent.richText = false;
                    _manageMessageInput.textComponent.enableWordWrapping = true;
                    _manageMessageInput.textComponent.overflowMode = TextOverflowModes.Overflow;
                }

                SetInputText(_manageMessageInput, message.Message ?? "");
                _manageMessageInput.ForceLabelUpdate();

                SetStatus("Editing message: " +
                    (_manageMessageInput.text ?? "").Length + "/" +
                    CommunityChatCharacterLimit + " characters.");

                StartRoutine(FocusManagePopupInputRoutine());
            }
        }

        private static IEnumerator FocusManagePopupInputRoutine()
        {
            yield return null;
            yield return null;

            if (_managePanel == null ||
                !_managePanel.activeSelf ||
                _manageMode != "chat" ||
                _manageMessageInput == null)
                yield break;

            try
            {
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(
                        _manageMessageInput.gameObject);
                }
            }
            catch { }

            try
            {
                _manageMessageInput.enabled = true;
                _manageMessageInput.interactable = true;
                _manageMessageInput.readOnly = false;
                _manageMessageInput.ActivateInputField();
                _manageMessageInput.ForceLabelUpdate();

                int end = (_manageMessageInput.text ?? "").Length;
                _manageMessageInput.caretPosition = end;
                _manageMessageInput.selectionAnchorPosition = end;
                _manageMessageInput.selectionFocusPosition = end;

                try { _manageMessageInput.MoveTextEnd(false); } catch { }
            }
            catch { }
        }

        private static void OpenManagePost(CommunityPost post)
        {
            if (post == null || !post.MyPost || _managePanel == null) return;
            _manageMode = "post";
            _manageTargetId = post.Id;
            if (_manageHeading != null) _manageHeading.text = "EDIT YOUR SUGGESTION";
            if (_manageTitleInput != null)
            {
                _manageTitleInput.gameObject.SetActive(true);
                SetInputText(_manageTitleInput, post.Title);
            }
            if (_manageMessageInput != null)
            {
                _manageMessageInput.gameObject.SetActive(true);
                _manageMessageInput.characterLimit = 4000;
                SetInputText(_manageMessageInput, post.Message);
            }
            _managePanel.SetActive(true);
            _managePanel.transform.SetAsLastSibling();
        }

        private static void CloseManagePanel()
        {
            _manageMode = "";
            _manageTargetId = 0;
            _manageRequestRunning = false;
            if (_managePanel != null) _managePanel.SetActive(false);
        }

        private static void SaveManagedContent()
        {
            if (_manageRequestRunning || _manageTargetId <= 0) return;
            object payload;
            string url;
            if (_manageMode == "chat")
            {
                string message = _manageMessageInput == null ? "" : (_manageMessageInput.text ?? "").Trim();

                if (message.Length == 0)
                {
                    SetStatus("Message cannot be empty.");
                    return;
                }

                if (message.Length > CommunityChatCharacterLimit)
                {
                    SetStatus("Community messages are limited to " +
                        CommunityChatCharacterLimit +
                        " characters. Current length: " + message.Length + ".");
                    return;
                }

                payload = new Dictionary<string, string> { { "message", message } };
                url = CommunityUrl("/chat/" + _manageTargetId);
            }
            else if (_manageMode == "post")
            {
                string title = _manageTitleInput == null ? "" : (_manageTitleInput.text ?? "").Trim();
                string message = _manageMessageInput == null ? "" : (_manageMessageInput.text ?? "").Trim();
                if (title.Length == 0 || message.Length == 0) { SetStatus("Title and message are required."); return; }
                payload = new Dictionary<string, string> { { "title", title }, { "message", message } };
                url = CommunityUrl("/posts/" + _manageTargetId);
            }
            else return;

            string mode = _manageMode;
            string requestedChatText =
                mode == "chat" && _manageMessageInput != null
                    ? (_manageMessageInput.text ?? "").Trim()
                    : "";

            _manageRequestRunning = true;
            SetStatus("Saving...");

            StartRoutine(SendRequest("PATCH", url, payload, delegate (bool ok, string body)
            {
                _manageRequestRunning = false;

                if (!ok)
                {
                    SetStatus("Edit failed: " + ErrorText(body));
                    return;
                }

                if (mode == "chat")
                {
                    int serverLength = -1;
                    string serverMessage = "";

                    try
                    {
                        using (var doc = JsonDocument.Parse(body ?? "{}"))
                        {
                            JsonElement messageObj;
                            if (doc.RootElement.TryGetProperty("message", out messageObj) &&
                                messageObj.ValueKind == JsonValueKind.Object)
                            {
                                JsonElement messageText;
                                if (messageObj.TryGetProperty("message", out messageText) &&
                                    messageText.ValueKind == JsonValueKind.String)
                                {
                                    serverMessage = messageText.GetString() ?? "";
                                    serverLength = serverMessage.Length;
                                }
                            }
                        }
                    }
                    catch { }




                    if (serverLength >= 0 &&
                        requestedChatText.Length > serverLength &&
                        !string.Equals(requestedChatText, serverMessage, StringComparison.Ordinal))
                    {
                        SetStatus(
                            "SERVER TRUNCATED EDIT: sent " +
                            requestedChatText.Length +
                            " characters, saved " +
                            serverLength +
                            ". The Community PATCH backend limit must be raised to " +
                            CommunityChatCharacterLimit + ".");


                        if (_manageMessageInput != null)
                        {
                            SetInputText(_manageMessageInput, requestedChatText);
                            StartRoutine(FocusManagePopupInputRoutine());
                        }

                        FetchChat(false);
                        return;
                    }
                }

                CloseManagePanel();
                SetStatus("Saved.");

                if (mode == "chat")
                    FetchChat(false);
                else
                    FetchPosts();
            }));
        }

        private static void DeleteManagedContent()
        {
            if (_manageRequestRunning || _manageTargetId <= 0) return;
            string mode = _manageMode;
            string url = mode == "chat" ? CommunityUrl("/chat/" + _manageTargetId) : CommunityUrl("/posts/" + _manageTargetId);
            if (mode != "chat" && mode != "post") return;
            _manageRequestRunning = true;
            StartRoutine(SendRequest("DELETE", url, null, delegate (bool ok, string body)
            {
                _manageRequestRunning = false;
                if (!ok) { SetStatus("Delete failed: " + ErrorText(body)); return; }
                CloseManagePanel();
                SetStatus("Deleted.");
                if (mode == "chat") FetchChat(); else { _selected = null; FetchPosts(); }
            }));
        }

        private static IEnumerator ChatPollingLoop()
        {
            _chatPolling = true;
            while (_chatPolling && _root != null)
            {
                bool editingPublicMessage =
                    _editingChatMessageId > 0 ||
                    (_managePanel != null &&
                     _managePanel.activeSelf &&
                     _manageMode == "chat");

                if (_root.activeSelf && _showingChat && !editingPublicMessage)
                {
                    bool activityOk = false;
                    int latestChatId = 0;

                    yield return GetLatestCommunityChatIdCoroutine(
                        delegate (bool ok, int latestId)
                        {
                            activityOk = ok;
                            latestChatId = latestId;
                        });

                    // Fetch the heavier chat payload only when the server reports
                    // a newer message. If activity polling fails, keep the existing
                    // UI instead of hammering /chat as a fallback.
                    if (activityOk && latestChatId > _latestKnownCommunityChatId)
                        FetchChat();
                }

                yield return new WaitForSeconds(4f);
            }
            _chatPolling = false;
        }

        private static bool IsScrollNearBottom(ScrollRect scroll)
        {
            if (scroll == null || scroll.content == null || scroll.viewport == null)
                return true;

            try
            {
                Canvas.ForceUpdateCanvases();


                if (scroll.content.rect.height <= scroll.viewport.rect.height + 1f)
                    return true;


                return scroll.verticalNormalizedPosition <= 0.08f;
            }
            catch
            {
                return true;
            }
        }

        private static void NotifyIncomingPrivateMessage()
        {
            try
            {
                ChatController chat = null;

                try
                {
                    chat = HudManager.Instance != null
                        ? HudManager.Instance.Chat
                        : null;
                }
                catch { }

                if (chat == null)
                {
                    try
                    {
                        var chats = Resources.FindObjectsOfTypeAll(
                            Il2CppType.Of<ChatController>());

                        if (chats != null)
                        {
                            for (int i = 0; i < chats.Length; i++)
                            {
                                ChatController candidate = chats[i] as ChatController;

                                if (candidate == null)
                                    continue;

                                chat = candidate;
                                break;
                            }
                        }
                    }
                    catch { }
                }

                if (chat != null &&
                    chat.messageSound != null &&
                    SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySoundImmediate(
                        chat.messageSound,
                        false,
                        1f,
                        1f,
                        SoundManager.Instance.SfxChannel
                    );
                }
            }
            catch (Exception ex)
            {
                try
                {
                    BMLogger.Warn(
                        $"[PrivateChat] NotifyIncomingPrivateMessage sound failed: {ex}",
                        "PrivateChat");
                }
                catch { }
            }

            try
            {
                BanMod.FlashColor(
                    new Color(0f, 0.45f, 1f, 0.30f),
                    1.1f);
            }
            catch { }
        }

        private static string GetLocalCommunityNotificationName()
        {
            string name = (_communityDisplayName ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            try
            {
                name = (PlayerPrefs.GetString(CommunityDisplayNameCacheKey, "") ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            catch { }



            name = (_privateLoginUsername ?? "").Trim();
            return name;
        }

        private static bool CommunityMessageMentionsName(string message, string displayName)
        {
            message = message ?? "";
            displayName = (displayName ?? "").Trim();

            if (message.Length == 0 || displayName.Length == 0)
                return false;

            string needle = "@" + displayName;
            int start = 0;

            while (start < message.Length)
            {
                int index = message.IndexOf(needle, start, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    return false;

                int after = index + needle.Length;

                bool beforeOk =
                    index == 0 ||
                    char.IsWhiteSpace(message[index - 1]) ||
                    "↪([{\"'".IndexOf(message[index - 1]) >= 0;

                bool afterOk =
                    after >= message.Length ||
                    char.IsWhiteSpace(message[after]) ||
                    ":,.;!?)]}\"'".IndexOf(message[after]) >= 0;

                if (beforeOk && afterOk)
                    return true;

                start = index + 1;
            }

            return false;
        }

        private static void ProcessIncomingCommunityMentions(List<CommunityChatMessage> messages)
        {
            if (messages == null)
                return;

            int highestId = _communityMentionLastMessageId;
            for (int i = 0; i < messages.Count; i++)
            {
                CommunityChatMessage message = messages[i];
                if (message != null && message.Id > highestId)
                    highestId = message.Id;
            }


            if (!_communityMentionInitialSyncDone)
            {
                _communityMentionInitialSyncDone = true;
                _communityMentionLastMessageId = highestId;
                return;
            }

            string localName = GetLocalCommunityNotificationName();
            if (string.IsNullOrWhiteSpace(localName))
            {
                _communityMentionLastMessageId = Math.Max(_communityMentionLastMessageId, highestId);
                return;
            }

            bool notify = false;

            for (int i = 0; i < messages.Count; i++)
            {
                CommunityChatMessage message = messages[i];
                if (message == null ||
                    message.Id <= _communityMentionLastMessageId ||
                    message.MyMessage)
                    continue;



                if (CommunityMessageMentionsName(message.Message, localName) &&
                    !CommunityMentionNotifiedIds.Contains(message.Id))
                {
                    CommunityMentionNotifiedIds.Add(message.Id);
                    notify = true;

                }
            }

            _communityMentionLastMessageId = Math.Max(_communityMentionLastMessageId, highestId);


            if (CommunityMentionNotifiedIds.Count > 512)
            {
                CommunityMentionNotifiedIds.Clear();
            }

            if (notify)
            {


                NotifyIncomingPrivateMessage();
            }
        }

        private static void TickCommunityMentionNotifications()
        {


            if (_root != null && _root.activeSelf && _showingChat)
                return;

            float now = Time.unscaledTime;
            if (_communityMentionPollRunning || now < _communityNextMentionPollTime)
                return;

            _communityNextMentionPollTime = now + CommunityMentionPollIntervalSeconds;
            StartRuntimeRoutine(PollCommunityMentionNotificationsCoroutine());
        }

        private static IEnumerator GetLatestCommunityChatIdCoroutine(Action<bool, int> callback)
        {
            bool ok = false;
            string body = "";

            yield return SendRequest(
                "GET",
                CommunityUrl("/activity"),
                null,
                delegate (bool success, string response)
                {
                    ok = success;
                    body = response ?? "";
                });

            if (!ok)
            {
                callback?.Invoke(false, 0);
                yield break;
            }

            try
            {
                using (var doc = JsonDocument.Parse(body))
                {
                    JsonElement root = doc.RootElement;
                    int latestChatId =
                        root.TryGetProperty("latest_chat_id", out JsonElement chatId) &&
                        chatId.ValueKind == JsonValueKind.Number
                            ? chatId.GetInt32()
                            : 0;

                    callback?.Invoke(true, latestChatId);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning(
                        "[BANMOD Community] Activity poll parse failed: " +
                        ex.Message
                    );
                }
                catch { }

                callback?.Invoke(false, 0);
            }
        }

        private static IEnumerator PollCommunityMentionNotificationsCoroutine()
        {
            if (_communityMentionPollRunning)
                yield break;

            _communityMentionPollRunning = true;

            bool activityOk = false;
            int latestChatId = 0;

            yield return GetLatestCommunityChatIdCoroutine(
                delegate (bool ok, int latestId)
                {
                    activityOk = ok;
                    latestChatId = latestId;
                });

            if (!activityOk)
            {
                _communityNextMentionPollTime =
                    Time.unscaledTime + _communityMentionFailureBackoff;

                _communityMentionFailureBackoff =
                    Mathf.Min(_communityMentionFailureBackoff * 2f, 60f);

                _communityMentionPollRunning = false;
                yield break;
            }

            // No new Community chat message: the lightweight /activity request is
            // enough. Avoid downloading/parsing the chat history every 10 seconds.
            if (_communityMentionInitialSyncDone &&
                latestChatId <= _communityMentionLastMessageId)
            {
                _communityMentionFailureBackoff = 10f;
                _communityMentionPollRunning = false;
                yield break;
            }

            bool chatOk = false;
            string body = "";

            yield return SendRequest(
                "GET",
                CommunityUrl("/chat?limit=20"),
                null,
                delegate (bool success, string response)
                {
                    chatOk = success;
                    body = response ?? "";
                });

            if (!chatOk)
            {
                _communityNextMentionPollTime =
                    Time.unscaledTime + _communityMentionFailureBackoff;

                _communityMentionFailureBackoff =
                    Mathf.Min(_communityMentionFailureBackoff * 2f, 60f);

                _communityMentionPollRunning = false;
                yield break;
            }

            _communityMentionFailureBackoff = 10f;

            try
            {
                List<CommunityChatMessage> messages = ParseChatMessages(body);
                ProcessIncomingCommunityMentions(messages);

                int highest = HighestChatId(messages);
                _latestKnownCommunityChatId =
                    Math.Max(_latestKnownCommunityChatId, highest);

                int seenChat = 0;
                try
                {
                    seenChat = PlayerPrefs.GetInt(
                        CommunityLastSeenChatKey,
                        0
                    );
                }
                catch { }

                if (highest > seenChat)
                    SetCommunityUnreadVisible(true);
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning(
                        "[BANMOD Community] Mention poll parse failed: " +
                        ex.Message
                    );
                }
                catch { }
            }

            _communityMentionPollRunning = false;
        }

        private static void FetchChat()
        {
            FetchChat(false);
        }

        private static void FetchChat(bool forceScrollToBottom)
        {
            if (_requestRunning) return;
            _requestRunning = true;
            StartRoutine(SendRequest("GET", CommunityUrl("/chat?limit=120"), null, delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok) { SetStatus("Chat load failed: " + ErrorText(body)); return; }
                try
                {



                    bool keepAtBottom =
                        forceScrollToBottom ||
                        ChatMessages.Count == 0 ||
                        IsScrollNearBottom(_chatScroll);

                    ChatMessages.Clear();
                    ChatMessages.AddRange(ParseChatMessages(body));

                    ProcessIncomingCommunityMentions(ChatMessages);

                    _latestKnownCommunityChatId = Math.Max(_latestKnownCommunityChatId, HighestChatId(ChatMessages));
                    if (ChatMessages.Count == 0) _selectedChatIndex = -1;
                    else if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count) _selectedChatIndex = ChatMessages.Count - 1;
                    RenderChat(keepAtBottom);
                    SetStatus("General Chat · " + ChatMessages.Count + " messages from the current 3-day window");
                    MarkCommunityActivitySeen();
                }
                catch (Exception ex) { SetStatus("Invalid chat response: " + ex.Message); }
            }));
        }

        private static void SubmitChat()
        {
            if (_requestRunning) return;
            string message = (_chatInput == null ? "" : (_chatInput.text ?? "")).Trim();
            if (message.Length == 0) { SetStatus("Write a message first."); return; }

            message = TranslateOutgoingCommunityText(message, "Community Chat", CommunityChatCharacterLimit);

            _requestRunning = true;
            var payload = new Dictionary<string, string> { { "message", message } };
            StartRoutine(SendRequest("POST", CommunityUrl("/chat"), payload, delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok) { SetStatus("Send failed: " + ErrorText(body)); return; }
                SetInputText(_chatInput, "");

                FetchChat(true);
            }));
        }

        private static List<CommunityChatMessage> ParseChatMessages(string json)
        {
            var result = new List<CommunityChatMessage>();
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("messages", out var arr) || arr.ValueKind != JsonValueKind.Array) return result;
                foreach (var item in arr.EnumerateArray())
                {
                    var m = new CommunityChatMessage();
                    if (item.TryGetProperty("id", out var id)) m.Id = id.GetInt32();
                    if (item.TryGetProperty("message", out var text)) m.Message = text.GetString() ?? "";
                    if (item.TryGetProperty("created_at", out var created)) m.CreatedAt = created.GetInt64();
                    if (item.TryGetProperty("updated_at", out var updated)) m.UpdatedAt = updated.GetInt64();
                    if (item.TryGetProperty("my_message", out var mine)) m.MyMessage = mine.GetBoolean();
                    if (item.TryGetProperty("author", out var author)) m.Author = author.GetString() ?? "BanModUser";
                    if (m.Id > 0) result.Add(m);
                }
            }
            return result;
        }

        private static int HighestChatId(List<CommunityChatMessage> messages)
        {
            int max = 0;
            for (int i = 0; i < messages.Count; i++) if (messages[i] != null && messages[i].Id > max) max = messages[i].Id;
            return max;
        }

        private static void RenderChat(bool scrollToBottom = false)
        {
            if (_chatContent == null) return;



            float previousOffsetFromTop = 0f;
            try
            {
                previousOffsetFromTop = Mathf.Max(0f, _chatContent.anchoredPosition.y);
            }
            catch { previousOffsetFromTop = 0f; }
            for (int i = 0; i < ChatRowObjects.Count; i++)
                if (ChatRowObjects[i] != null) UnityEngine.Object.Destroy(ChatRowObjects[i]);
            ChatRowObjects.Clear();

            if (ChatMessages.Count == 0)
            {
                TextMeshProUGUI empty = CreateLabel(_chatContent, "EmptyChat", "No messages yet. Start the conversation.", 17, TextAlignmentOptions.Center, new Color(0.76f, 0.80f, 0.90f, 1f), new Vector2(0f, -70f), new Vector2(900f, 60f));
                RectTransform er = empty.GetComponent<RectTransform>();
                er.anchorMin = er.anchorMax = new Vector2(0.5f, 1f); er.pivot = new Vector2(0.5f, 1f);
                ChatRowObjects.Add(empty.gameObject);
                _chatContent.sizeDelta = new Vector2(0f, 410f);
            }
            else
            {
                float top = 8f;
                for (int i = 0; i < ChatMessages.Count; i++)
                {
                    int capturedIndex = i;
                    CommunityChatMessage m = ChatMessages[i];
                    bool selected = i == _selectedChatIndex;
                    string author = string.IsNullOrWhiteSpace(m.Author) ? "BanModUser" : m.Author;
                    string edited = m.UpdatedAt > m.CreatedAt ? " · edited" : "";

                    if (!m.MyMessage && LiveTranslator.GetEnabled())
                        CacheAutomaticCommunityTranslation(CommunityChatTranslations, m.Id, m.Message, "Community Chat");

                    string displayedMessage = GetCommunityDisplayedText(CommunityChatTranslations, m.Id, m.Message);
                    string rowTextRaw = author + (m.MyMessage ? " (you)" : "") + " · " + FormatTimestamp(m.CreatedAt) + edited + "\n" + displayedMessage;
                    string rowText = BanModSymbolPicker.ToSafeRichText(rowTextRaw);

                    float rowHeight = 70f;
                    Button row = CreateButton(_chatContent, "ChatRow_" + m.Id, rowText, Vector2.zero, new Vector2(1010f, rowHeight), selected ? new Color(0.23f, 0.31f, 0.50f, 1f) : new Color(0.12f, 0.14f, 0.20f, 1f), delegate
                    {
                        _selectedChatIndex = capturedIndex;
                        RenderChat(false);
                    });

                    TextMeshProUGUI label = row.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (label != null)
                    {
                        label.richText = true;
                        label.alignment = TextAlignmentOptions.TopLeft;
                        label.enableWordWrapping = true;



                        label.enableAutoSizing = false;
                        label.fontSize = 15f;
                        label.overflowMode = TextOverflowModes.Overflow;

                        RectTransform lr = label.GetComponent<RectTransform>();
                        if (lr != null)
                        {
                            lr.offsetMin = new Vector2(14f, 10f);
                            lr.offsetMax = new Vector2(-14f, -10f);
                        }

                        try
                        {


                            label.ForceMeshUpdate();
                            float preferred = label.GetPreferredValues(rowText, 980f, 100000f).y;

                            if (float.IsNaN(preferred) || float.IsInfinity(preferred) || preferred < 1f)
                                preferred = 50f;

                            rowHeight = Mathf.Max(70f, preferred + 28f);
                        }
                        catch
                        {

                            int explicitLines = 1;
                            string rawText = rowTextRaw ?? "";
                            for (int c = 0; c < rawText.Length; c++)
                                if (rawText[c] == '\n') explicitLines++;

                            rowHeight = Mathf.Max(70f, 28f + explicitLines * 24f);
                        }
                    }

                    RectTransform rr = row.GetComponent<RectTransform>();
                    rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 1f);
                    rr.pivot = new Vector2(0.5f, 1f);
                    rr.sizeDelta = new Vector2(1010f, rowHeight);
                    rr.anchoredPosition = new Vector2(-28f, -top);
                    ChatRowObjects.Add(row.gameObject);

                    if (m.MyMessage)
                    {
                        CommunityChatMessage captured = m;
                        Button more = CreateButton(_chatContent, "ChatMore_" + m.Id, "...", Vector2.zero, new Vector2(58f, rowHeight), new Color(0.22f, 0.23f, 0.30f, 1f), delegate
                        {
                            OpenManageChat(captured);
                        });
                        RectTransform mr = more.GetComponent<RectTransform>();
                        mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 1f);
                        mr.pivot = new Vector2(0.5f, 1f);
                        mr.sizeDelta = new Vector2(58f, rowHeight);
                        mr.anchoredPosition = new Vector2(520f, -top);
                        ChatRowObjects.Add(more.gameObject);
                    }

                    top += rowHeight + 12f;
                }

                _chatContent.sizeDelta = new Vector2(0f, Math.Max(410f, top + 24f));
            }

            if (_reportChatButton != null)
            {
                bool canReport = _selectedChatIndex >= 0 && _selectedChatIndex < ChatMessages.Count && !ChatMessages[_selectedChatIndex].MyMessage;
                _reportChatButton.interactable = canReport && !_requestRunning;
            }

            if (_chatTranslateButton != null)
            {
                bool canTranslate = _selectedChatIndex >= 0 && _selectedChatIndex < ChatMessages.Count;
                _chatTranslateButton.interactable = canTranslate && !_requestRunning;
            }

            bool hasSelectedMessage = _selectedChatIndex >= 0 && _selectedChatIndex < ChatMessages.Count;

            if (_chatTagButton != null)
                _chatTagButton.interactable =
                    _editingChatMessageId <= 0 &&
                    hasSelectedMessage &&
                    !_requestRunning;

            if (_chatReplyButton != null)
                _chatReplyButton.interactable =
                    _editingChatMessageId <= 0 &&
                    hasSelectedMessage &&
                    !_requestRunning;

            try
            {
                Canvas.ForceUpdateCanvases();

                if (_chatScroll != null && _chatScroll.viewport != null && _chatScroll.content != null)
                {
                    _chatScroll.StopMovement();

                    float viewportHeight = Mathf.Max(1f, _chatScroll.viewport.rect.height);
                    float contentHeight = Mathf.Max(viewportHeight, _chatScroll.content.rect.height);
                    float maxOffset = Mathf.Max(0f, contentHeight - viewportHeight);

                    if (scrollToBottom)
                    {
                        _chatScroll.verticalNormalizedPosition = 0f;
                    }
                    else if (maxOffset <= 0.5f)
                    {
                        _chatScroll.verticalNormalizedPosition = 1f;
                    }
                    else
                    {
                        float restoredOffset = Mathf.Clamp(previousOffsetFromTop, 0f, maxOffset);
                        _chatScroll.verticalNormalizedPosition = 1f - (restoredOffset / maxOffset);
                    }

                    Canvas.ForceUpdateCanvases();
                }
            }
            catch { }
        }

        private static void SelectPreviousChatMessage()
        {
            if (ChatMessages.Count == 0) { _selectedChatIndex = -1; RenderChat(); return; }
            if (_selectedChatIndex < 0) _selectedChatIndex = ChatMessages.Count - 1;
            else _selectedChatIndex = Math.Max(0, _selectedChatIndex - 1);
            RenderChat();
        }

        private static void SelectNextChatMessage()
        {
            if (ChatMessages.Count == 0) { _selectedChatIndex = -1; RenderChat(); return; }
            if (_selectedChatIndex < 0) _selectedChatIndex = 0;
            else _selectedChatIndex = Math.Min(ChatMessages.Count - 1, _selectedChatIndex + 1);
            RenderChat();
        }

        private static void TagSelectedChatMessage()
        {
            if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count || _chatInput == null)
            {
                SetStatus("Select a message first.");
                return;
            }

            CommunityChatMessage message = ChatMessages[_selectedChatIndex];
            if (message == null)
            {
                SetStatus("Select a valid message first.");
                return;
            }

            string author = string.IsNullOrWhiteSpace(message.Author) ? "BanModUser" : message.Author.Trim();
            string current = _chatInput.text ?? "";
            string separator = current.Length > 0 && !char.IsWhiteSpace(current[current.Length - 1]) ? " " : "";
            string addition = separator + "@" + author + " ";

            if (current.Length + addition.Length > CommunityChatCharacterLimit)
            {
                SetStatus("TAG would exceed the " + CommunityChatCharacterLimit + " character Community limit.");
                return;
            }

            SetInputText(_chatInput, current + addition);
            FocusChatInputAtEnd();
        }

        private static void ReplySelectedChatMessage()
        {
            if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count || _chatInput == null)
            {
                SetStatus("Select a message first.");
                return;
            }

            CommunityChatMessage message = ChatMessages[_selectedChatIndex];
            if (message == null)
            {
                SetStatus("Select a valid message first.");
                return;
            }

            string author = string.IsNullOrWhiteSpace(message.Author) ? "BanModUser" : message.Author.Trim();
            string preview = (message.Message ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
            const int maxPreviewLength = 120;
            if (preview.Length > maxPreviewLength)
                preview = preview.Substring(0, maxPreviewLength) + "...";

            string prefix = "↪ @" + author + ": \"" + preview + "\"\n";
            string current = _chatInput.text ?? "";
            int availableForCurrentText = CommunityChatCharacterLimit - prefix.Length;

            if (availableForCurrentText < 0)
            {
                SetStatus("The selected message is too long to quote.");
                return;
            }

            if (current.Length > availableForCurrentText)
                current = current.Substring(0, availableForCurrentText);

            SetInputText(_chatInput, prefix + current);
            FocusChatInputAtEnd();
        }

        private static void FocusChatInputAtEnd()
        {
            if (_chatInput == null) return;
            try
            {
                _chatInput.ActivateInputField();
                int end = (_chatInput.text ?? "").Length;
                _chatInput.caretPosition = end;
                _chatInput.selectionAnchorPosition = end;
                _chatInput.selectionFocusPosition = end;
            }
            catch { }
        }

        private static void ReportSelectedChatMessage()
        {
            if (_requestRunning || _selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count) return;
            var message = ChatMessages[_selectedChatIndex];
            if (message == null || message.MyMessage) { SetStatus("You cannot report your own message."); return; }
            _requestRunning = true;
            if (_reportChatButton != null) _reportChatButton.interactable = false;
            int messageId = message.Id;
            var payload = new Dictionary<string, string> { { "reason", "inappropriate" } };
            StartRoutine(SendRequest("POST", CommunityUrl("/chat/" + messageId + "/report"), payload, delegate (bool ok, string body)
            {
                _requestRunning = false;
                SetStatus(ok ? "Message reported to moderators." : "Report failed: " + ErrorText(body));
                RenderChat();
            }));
        }

        private static void FetchPosts()
        {
            if (_requestRunning) return;
            _requestRunning = true;
            SetStatus("Loading...");
            StartRoutine(SendRequest("GET", CommunityUrl("/posts?kind=suggestion&limit=100&sort=latest"), null, delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok) { SetStatus("Load failed: " + ErrorText(body)); return; }
                try
                {
                    Posts.Clear();
                    Posts.AddRange(ParsePosts(body));
                    _latestKnownCommunityPostId = Math.Max(_latestKnownCommunityPostId, HighestPostId(Posts));
                    _selected = Posts.Count > 0 ? Posts[0] : null;
                    RenderList();
                    if (_selected != null) FetchReplies(_selected.Id); else { Replies.Clear(); RenderSelected(); }
                    SetStatus(Posts.Count + " suggestions");
                    MarkCommunityActivitySeen();
                }
                catch (Exception ex) { SetStatus("Invalid server response: " + ex.Message); }
            }));
        }

        private static void FetchReplies(int postId)
        {
            Replies.Clear();
            RenderSelected();
            StartRoutine(SendRequest("GET", CommunityUrl("/posts/" + postId + "/replies"), null, delegate (bool ok, string body)
            {
                if (_selected == null || _selected.Id != postId) return;
                if (!ok) { SetStatus("Reply load failed: " + ErrorText(body)); return; }
                try
                {
                    Replies.Clear();
                    Replies.AddRange(ParseReplies(body));
                    for (int i = 0; i < Replies.Count; i++) _latestKnownCommunityReplyId = Math.Max(_latestKnownCommunityReplyId, Replies[i].Id);
                    _selectedReplyIndex = -1;
                    RenderSelected();
                    MarkCommunityActivitySeen();
                }
                catch (Exception ex) { SetStatus("Invalid replies response: " + ex.Message); }
            }));
        }

        private static void SelectPost(CommunityPost post)
        {
            _selected = post;
            Replies.Clear();
            _selectedReplyIndex = -1;
            SetInputText(_replyInput, "");
            RenderList();
            RenderSelected();
            if (post != null) FetchReplies(post.Id);
        }

        private static void SubmitPost()
        {
            if (_requestRunning) return;
            string title = (_titleInput == null ? "" : (_titleInput.text ?? "")).Trim();
            string message = (_messageInput == null ? "" : (_messageInput.text ?? "")).Trim();
            if (title.Length == 0 || message.Length == 0) { SetStatus("Title and message are required."); return; }

            title = TranslateOutgoingCommunityText(title, "Suggestion title", 120);
            message = TranslateOutgoingCommunityText(message, "Suggestion", 4000);

            var payload = new Dictionary<string, string> { { "kind", "suggestion" }, { "title", title }, { "message", message } };
            _requestRunning = true;
            StartRoutine(SendRequest("POST", CommunityUrl("/posts"), payload, delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok) { SetStatus("Publish failed: " + ErrorText(body)); return; }
                if (_composePanel != null) _composePanel.SetActive(false);
                SetStatus("Suggestion published.");
                FetchPosts();
            }));
        }

        private static void SubmitReply()
        {
            if (_selected == null || _requestRunning) return;
            string message = (_replyInput == null ? "" : (_replyInput.text ?? "")).Trim();
            if (message.Length == 0) { SetStatus("Write a reply first."); return; }

            message = TranslateOutgoingCommunityText(message, "Suggestion reply", 2500);

            int postId = _selected.Id;
            var payload = new Dictionary<string, string> { { "message", message } };
            _requestRunning = true;
            StartRoutine(SendRequest("POST", CommunityUrl("/posts/" + postId + "/replies"), payload, delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok) { SetStatus("Reply failed: " + ErrorText(body)); return; }
                SetInputText(_replyInput, "");
                SetStatus("Reply published.");
                FetchReplies(postId);
                FetchPosts();
            }));
        }

        private static void ToggleStar()
        {
            if (_selected == null || _selected.MyPost || _requestRunning) return;
            int id = _selected.Id;
            _requestRunning = true;
            StartRoutine(SendRequest("POST", CommunityUrl("/posts/" + id + "/star"), new Dictionary<string, string>(), delegate (bool ok, string body)
            {
                _requestRunning = false;
                if (!ok) { SetStatus("Star failed: " + ErrorText(body)); return; }
                try
                {
                    using (var doc = JsonDocument.Parse(body))
                    {
                        var root = doc.RootElement;
                        _selected.MyStar = root.TryGetProperty("starred", out var s) && s.GetBoolean();
                        if (root.TryGetProperty("stars", out var n)) _selected.Stars = n.GetInt32();
                    }
                    RenderList(); RenderSelected();
                }
                catch { }
            }));
        }

        private static void RenderList()
        {
            for (int i = 0; i < ListButtons.Count; i++)
                if (ListButtons[i] != null) UnityEngine.Object.Destroy(ListButtons[i]);
            ListButtons.Clear();
            if (_suggestionsListContent == null) return;

            ClearRectChildren(_suggestionsListContent);

            if (Posts.Count == 0)
            {
                TextMeshProUGUI empty = CreateLabel(_suggestionsListContent, "NoSuggestions", "No suggestions yet.", 15,
                    TextAlignmentOptions.Center, new Color(0.68f, 0.72f, 0.80f, 1f), Vector2.zero, new Vector2(420f, 50f));
                RectTransform emptyRect = empty != null ? empty.GetComponent<RectTransform>() : null;
                if (emptyRect != null)
                {
                    emptyRect.anchorMin = emptyRect.anchorMax = new Vector2(0.5f, 1f);
                    emptyRect.pivot = new Vector2(0.5f, 1f);
                    emptyRect.anchoredPosition = new Vector2(0f, -24f);
                }
                _suggestionsListContent.sizeDelta = new Vector2(0f, 500f);
                return;
            }

            float top = 8f;
            for (int i = 0; i < Posts.Count; i++)
            {
                CommunityPost post = Posts[i];
                if (post == null) continue;
                string resolved = post.Status == "resolved" ? " ✓" : "";
                string author = string.IsNullOrWhiteSpace(post.Author) ? "BanModUser" : post.Author;
                string text = author + " · " + Short(post.Title, 30) + resolved + "   ★" + post.Stars + "  💬" + post.ReplyCount;
                CommunityPost captured = post;
                float rowHeight = 68f;
                float width = post.MyPost ? 370f : 430f;
                float x = post.MyPost ? -28f : 0f;

                Button b = CreateButton(_suggestionsListContent, "Post_" + post.Id, text, Vector2.zero,
                    new Vector2(width, rowHeight), post == _selected
                        ? new Color(0.24f, 0.31f, 0.52f, 1f)
                        : new Color(0.13f, 0.15f, 0.22f, 1f),
                    delegate { SelectPost(captured); });
                RectTransform br = b != null ? b.GetComponent<RectTransform>() : null;
                if (br != null)
                {
                    br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
                    br.pivot = new Vector2(0.5f, 1f);
                    br.anchoredPosition = new Vector2(x, -top);
                }
                TextMeshProUGUI label = b != null ? b.GetComponentInChildren<TextMeshProUGUI>(true) : null;
                if (label != null)
                {
                    label.fontSize = 14f;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 10f;
                    label.fontSizeMax = 14f;
                }
                if (b != null) ListButtons.Add(b.gameObject);

                if (post.MyPost)
                {
                    Button more = CreateButton(_suggestionsListContent, "PostMore_" + post.Id, "...", Vector2.zero,
                        new Vector2(56f, rowHeight), new Color(0.22f, 0.23f, 0.30f, 1f), delegate { OpenManagePost(captured); });
                    RectTransform mr = more != null ? more.GetComponent<RectTransform>() : null;
                    if (mr != null)
                    {
                        mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 1f);
                        mr.pivot = new Vector2(0.5f, 1f);
                        mr.anchoredPosition = new Vector2(190f, -top);
                    }
                    if (more != null) ListButtons.Add(more.gameObject);
                }

                top += rowHeight + 10f;
            }

            _suggestionsListContent.sizeDelta = new Vector2(0f, Math.Max(500f, top + 8f));
            try
            {
                Canvas.ForceUpdateCanvases();
                if (_suggestionsListScroll != null)
                    _suggestionsListScroll.verticalNormalizedPosition = 1f;
            }
            catch { }
        }

        private static void RenderSelected()
        {
            if (_detailText == null) return;
            if (_selected == null)
            {
                SetDetailText("No public posts yet.");
                if (_starButton != null) _starButton.interactable = false;
                if (_replyInput != null) _replyInput.interactable = false;
                if (_reportReplyButton != null) _reportReplyButton.interactable = false;
                if (_suggestionTranslateButton != null) _suggestionTranslateButton.interactable = false;
                return;
            }

            if (_replyInput != null) _replyInput.interactable = true;

            if (!_selected.MyPost && LiveTranslator.GetEnabled())
            {
                CacheAutomaticCommunityTranslation(SuggestionTitleTranslations, _selected.Id, _selected.Title, "Suggestion title");
                CacheAutomaticCommunityTranslation(SuggestionMessageTranslations, _selected.Id, _selected.Message, "Suggestion");
            }

            string displayedTitle = GetCommunityDisplayedText(SuggestionTitleTranslations, _selected.Id, _selected.Title);
            string displayedMessage = GetCommunityDisplayedText(SuggestionMessageTranslations, _selected.Id, _selected.Message);

            string kind = _selected.Kind.ToUpperInvariant();
            string state = _selected.Status == "resolved" ? " · RESOLVED" : "";
            var sb = new System.Text.StringBuilder();
            sb.Append(kind).Append(state).Append("\n").Append(displayedTitle).Append("\n\n").Append(displayedMessage);
            sb.Append("\n\n").Append(string.IsNullOrWhiteSpace(_selected.Author) ? "BanModUser" : _selected.Author).Append(_selected.MyPost ? " (you)" : "").Append(" · ").Append(FormatTimestamp(_selected.CreatedAt)).Append(" · ★ ").Append(_selected.Stars).Append(" · 💬 ").Append(_selected.ReplyCount);
            sb.Append("\n\n────────────────────────\nCOMMENTS\n");
            if (Replies.Count == 0) sb.Append("\nNo comments yet. You can be the first to reply.");

            for (int i = 0; i < Replies.Count; i++)
            {
                var r = Replies[i];

                if (r != null && !r.MyReply && LiveTranslator.GetEnabled())
                    CacheAutomaticCommunityTranslation(SuggestionReplyTranslations, r.Id, r.Message, "Suggestion comment");

                string displayedReply = r == null ? "" : GetCommunityDisplayedText(SuggestionReplyTranslations, r.Id, r.Message);
                bool selectedReply = i == _selectedReplyIndex;
                if (selectedReply) sb.Append("\n▶ ");
                else sb.Append("\n");
                sb.Append("#").Append(i + 1).Append(" · ").Append(string.IsNullOrWhiteSpace(r.Author) ? "BanModUser" : r.Author).Append(r.MyReply ? " (you)" : "").Append(" · ").Append(FormatTimestamp(r.CreatedAt));
                if (selectedReply) sb.Append(" · SELECTED");
                sb.Append("\n").Append(displayedReply).Append("\n");
            }

            SetDetailText(sb.ToString());
            bool canAct = !_selected.MyPost;
            if (_starButton != null) { _starButton.interactable = canAct; SetButtonText(_starButton, (_selected.MyStar ? "★ STARRED" : "☆ STAR") + " (" + _selected.Stars + ")"); }
            if (_reportReplyButton != null)
            {
                bool canReport = _selectedReplyIndex >= 0 && _selectedReplyIndex < Replies.Count && !Replies[_selectedReplyIndex].MyReply;
                _reportReplyButton.interactable = canReport && !_requestRunning;
            }
            if (_suggestionTranslateButton != null)
            {
                bool commentSelected = _selectedReplyIndex >= 0 && _selectedReplyIndex < Replies.Count;
                SetButtonText(_suggestionTranslateButton, commentSelected ? "TRANSLATE COMMENT" : "TRANSLATE POST");
                _suggestionTranslateButton.interactable = !_requestRunning;
            }
        }

        private static void SelectPreviousReply()
        {
            if (Replies.Count == 0) { _selectedReplyIndex = -1; RenderSelected(); return; }
            if (_selectedReplyIndex < 0) _selectedReplyIndex = Replies.Count - 1;
            else _selectedReplyIndex = Math.Max(0, _selectedReplyIndex - 1);
            RenderSelected();
        }

        private static void SelectNextReply()
        {
            if (Replies.Count == 0) { _selectedReplyIndex = -1; RenderSelected(); return; }
            if (_selectedReplyIndex < 0) _selectedReplyIndex = 0;
            else _selectedReplyIndex = Math.Min(Replies.Count - 1, _selectedReplyIndex + 1);
            RenderSelected();
        }

        private static void ReportSelectedReply()
        {
            if (_requestRunning || _selectedReplyIndex < 0 || _selectedReplyIndex >= Replies.Count) return;
            var reply = Replies[_selectedReplyIndex];
            if (reply == null || reply.MyReply) { SetStatus("You cannot report your own comment."); return; }
            _requestRunning = true;
            if (_reportReplyButton != null) _reportReplyButton.interactable = false;
            int replyId = reply.Id;
            var payload = new Dictionary<string, string> { { "reason", "inappropriate" } };
            StartRoutine(SendRequest("POST", CommunityUrl("/replies/" + replyId + "/report"), payload, delegate (bool ok, string body)
            {
                _requestRunning = false;
                SetStatus(ok ? "Comment reported to moderators." : "Report failed: " + ErrorText(body));
                RenderSelected();
            }));
        }

        private static void SetDetailText(string value)
        {
            if (_detailText == null) return;
            _detailText.text = value ?? "";
            try
            {
                _detailText.ForceMeshUpdate();
                float h = Math.Max(300f, _detailText.preferredHeight + 30f);
                if (_detailContent != null) _detailContent.sizeDelta = new Vector2(_detailContent.sizeDelta.x, h);
                if (_detailScroll != null) _detailScroll.verticalNormalizedPosition = 1f;
            }
            catch { }
        }


        internal static void RuntimeTick()
        {
            try
            {
                EnsureLauncherUi();
                RefreshLauncherVisibility();
                HandleLauncherDrag();
                HandleUnifiedScrollInput();

                if (!_backgroundProfileRequested)
                {
                    string friendCode = "";

                    try
                    {
                        friendCode = (BanModIdentity.GetFriendCode() ?? "").Trim();
                    }
                    catch { }

                    if (friendCode.Length >= 3)
                    {
                        _backgroundProfileRequested = true;
                        FetchProfile();
                    }
                }

                TickCommunityMentionNotifications();
                TickPrivateChat();
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning("[BANMOD Community] Runtime tick failed: " + ex.Message);
                }
                catch { }
            }
        }

        private static void BindSubmitAction(TMP_InputField input, Action action)
        {
            if (input == null || action == null)
                return;






            try
            {
                input.onSubmit.AddListener((UnityAction<string>)delegate (string _)
                {
                    bool isCommunityChatInput = _chatInput != null && input == _chatInput;
                    bool ctrlPressed = false;

                    if (isCommunityChatInput)
                    {
                        try
                        {
                            ctrlPressed =
                                Input.GetKey(KeyCode.LeftControl) ||
                                Input.GetKey(KeyCode.RightControl);
                        }
                        catch { ctrlPressed = false; }
                    }

                    if (isCommunityChatInput && ctrlPressed)
                    {
                        InsertNewLineAtCaret(input);
                        return;
                    }

                    action();
                });
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning("[BANMOD Community] Could not bind Enter submit for " +
                        (input.gameObject != null ? input.gameObject.name : "input") + ": " + ex.Message);
                }
                catch { }
            }
        }

        private static void InsertNewLineAtCaret(TMP_InputField input)
        {
            if (input == null)
                return;

            try
            {
                string current = input.text ?? "";

                int anchor = Mathf.Clamp(input.selectionAnchorPosition, 0, current.Length);
                int focus = Mathf.Clamp(input.selectionFocusPosition, 0, current.Length);
                int selectionStart = Math.Min(anchor, focus);
                int selectionEnd = Math.Max(anchor, focus);

                string result =
                    current.Substring(0, selectionStart) +
                    "\n" +
                    current.Substring(selectionEnd);

                int limit = input.characterLimit;
                if (limit > 0 && result.Length > limit)
                {
                    SetStatus("Community messages are limited to " + limit + " characters.");
                    input.ActivateInputField();
                    return;
                }

                SetInputText(input, result);

                int newCaret = Math.Min(selectionStart + 1, result.Length);
                input.caretPosition = newCaret;
                input.selectionAnchorPosition = newCaret;
                input.selectionFocusPosition = newCaret;
                input.ActivateInputField();
            }
            catch (Exception ex)
            {
                try { Debug.LogWarning("[BANMOD Community] Could not insert Community newline: " + ex.Message); }
                catch { }
            }
        }

        private static void HandleUnifiedScrollInput()
        {
            if (_root == null || !_root.activeSelf)
                return;

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) < 0.001f)
            {
                try { wheel = Input.GetAxis("Mouse ScrollWheel") * 10f; }
                catch { wheel = 0f; }
            }
            if (Mathf.Abs(wheel) < 0.001f)
                return;

            if (_showingPrivateChat)
            {
                if (TryManualScroll(_privatePlayersScroll, wheel)) return;
                TryManualScroll(_privateMessagesScroll, wheel);
                return;
            }

            if (_showingBugReport)
            {
                if (TryManualScroll(_bugReportsScroll, wheel)) return;
                TryManualScroll(_bugConversationScroll, wheel);
                return;
            }

            if (_showingChat)
            {
                TryManualScroll(_chatScroll, wheel);
                return;
            }

            if (TryManualScroll(_suggestionsListScroll, wheel)) return;
            TryManualScroll(_detailScroll, wheel);
        }

        private static bool TryManualScroll(ScrollRect scroll, float wheel)
        {
            if (scroll == null || scroll.content == null || scroll.viewport == null || !scroll.gameObject.activeInHierarchy)
                return false;

            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            if (scrollRect == null)
                return false;

            bool inside = false;
            try { inside = RectTransformUtility.RectangleContainsScreenPoint(scrollRect, Input.mousePosition, null); }
            catch { }
            if (!inside)
                return false;

            float viewportHeight = scroll.viewport.rect.height;
            float contentHeight = scroll.content.rect.height;
            if (contentHeight <= viewportHeight + 1f)
                return true;

            try
            {
                scroll.StopMovement();

                float scrollableHeight = Mathf.Max(1f, contentHeight - viewportHeight);



                const float pixelsPerWheelUnit = 52f;
                float pixelDelta = wheel * pixelsPerWheelUnit;
                float normalizedDelta = pixelDelta / scrollableHeight;

                scroll.verticalNormalizedPosition =
                    Mathf.Clamp01(scroll.verticalNormalizedPosition + normalizedDelta);

                Canvas.ForceUpdateCanvases();
            }
            catch { }
            return true;
        }

        private static void EnsureLauncherUi()
        {
            if (_launcherRoot != null) return;

            try
            {
                EnsureEventSystem();

                _launcherRoot = new GameObject("BanMod_CommunityLauncher");
                UnityEngine.Object.DontDestroyOnLoad(_launcherRoot);
                RectTransform rootRect = _launcherRoot.AddComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;

                Canvas canvas = _launcherRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.overrideSorting = true;
                canvas.sortingOrder = 29950;

                CanvasScaler scaler = _launcherRoot.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                _launcherRoot.AddComponent<GraphicRaycaster>();

                Vector2 pos = LoadLauncherPosition();
                Button launcherButton = CreateButton(
                    _launcherRoot.transform,
                    "CommunityLauncherButton",
                    "C",
                    pos,
                    new Vector2(58f, 42f),
                    new Color(1f, 1f, 1f, 0.34f),
                    OnLauncherClicked);
                _launcherButton = launcherButton != null ? launcherButton.gameObject : null;

                _launcherButtonRect = _launcherButton == null ? null : _launcherButton.GetComponent<RectTransform>();
                if (_launcherButton != null)
                {
                    TextMeshProUGUI textLabel = _launcherButton.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (textLabel != null)
                    {
                        textLabel.fontSize = 19f;
                        textLabel.fontStyle = FontStyles.Bold;
                        textLabel.color = new Color(0f, 0f, 0f, 0.78f);
                    }

                    _launcherUnreadDot = CreateLabel(
                        _launcherButton.transform,
                        "UnreadDot",
                        "●",
                        22f,
                        TextAlignmentOptions.Center,
                        new Color(0.96f, 0.06f, 0.06f, 1f),
                        new Vector2(24f, 15f),
                        new Vector2(26f, 26f));
                    if (_launcherUnreadDot != null)
                        _launcherUnreadDot.gameObject.SetActive(false);
                }

                RefreshCombinedUnread();
                RefreshLauncherVisibility();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[BANMOD Community] Launcher creation failed: " + ex.Message);
            }
        }

        private static void RefreshLauncherVisibility()
        {
            if (_launcherRoot == null || _launcherButton == null) return;
            try
            {
                bool communityOpen = _root != null && _root.activeSelf;
                _launcherRoot.SetActive(true);
                _launcherButton.SetActive(!communityOpen);
                if (!communityOpen)
                    _launcherButton.transform.SetAsLastSibling();
            }
            catch { }
        }

        private static void OnLauncherClicked()
        {
            if (Time.unscaledTime < _launcherSuppressClickUntil)
                return;
            OpenFromMainMenu();
        }

        private static Vector2 LoadLauncherPosition()
        {
            try
            {
                return new Vector2(
                    PlayerPrefs.GetFloat(LauncherPosXKey, -920f),
                    PlayerPrefs.GetFloat(LauncherPosYKey, 508f));
            }
            catch
            {
                return new Vector2(-920f, 508f);
            }
        }

        private static void SaveLauncherPosition()
        {
            if (_launcherButtonRect == null) return;
            try
            {
                PlayerPrefs.SetFloat(LauncherPosXKey, _launcherButtonRect.anchoredPosition.x);
                PlayerPrefs.SetFloat(LauncherPosYKey, _launcherButtonRect.anchoredPosition.y);
                PlayerPrefs.Save();
            }
            catch { }
        }

        private static void HandleLauncherDrag()
        {
            if (_launcherButtonRect == null || _launcherButton == null || !_launcherButton.activeSelf)
            {
                _launcherDragging = false;
                return;
            }

            Vector2 mouse = Input.mousePosition;
            try
            {
                if (Input.GetMouseButtonDown(0) &&
                    RectTransformUtility.RectangleContainsScreenPoint(_launcherButtonRect, mouse, null))
                {
                    RectTransform canvasRect = _launcherRoot == null ? null : _launcherRoot.GetComponent<RectTransform>();
                    if (canvasRect != null &&
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouse, null, out Vector2 local))
                    {
                        _launcherDragging = true;
                        _launcherMoved = false;
                        _launcherDragStartMouse = mouse;
                        _launcherDragOffset = _launcherButtonRect.anchoredPosition - local;
                    }
                }

                if (_launcherDragging && Input.GetMouseButton(0))
                {
                    RectTransform canvasRect = _launcherRoot == null ? null : _launcherRoot.GetComponent<RectTransform>();
                    if (canvasRect != null &&
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouse, null, out Vector2 local))
                    {
                        Vector2 target = local + _launcherDragOffset;
                        Vector2 half = _launcherButtonRect.sizeDelta * 0.5f;
                        Rect bounds = canvasRect.rect;
                        target.x = Mathf.Clamp(target.x, bounds.xMin + half.x + 6f, bounds.xMax - half.x - 6f);
                        target.y = Mathf.Clamp(target.y, bounds.yMin + half.y + 6f, bounds.yMax - half.y - 6f);
                        _launcherButtonRect.anchoredPosition = target;

                        if ((mouse - _launcherDragStartMouse).sqrMagnitude > 36f)
                            _launcherMoved = true;
                    }
                }

                if (_launcherDragging && Input.GetMouseButtonUp(0))
                {
                    _launcherDragging = false;
                    if (_launcherMoved)
                    {
                        _launcherSuppressClickUntil = Time.unscaledTime + 0.30f;
                        SaveLauncherPosition();
                    }
                }
            }
            catch
            {
                _launcherDragging = false;
            }
        }

        private static void TickPrivateChat()
        {
            float now = Time.unscaledTime;

            if (string.IsNullOrWhiteSpace(_privateBanModToken))
            {
                if (!_privateSessionRequestRunning && now >= _privateNextSessionAttemptTime)
                {
                    if (StartRuntimeRoutine(EnsurePrivateChatSessionCoroutine()))
                        _privateNextSessionAttemptTime = now + 5f;
                    else
                        _privateNextSessionAttemptTime = now + 1f;
                }

                return;
            }

            if (!_privatePresenceRequestRunning && now >= _privateNextPresenceTime)
            {
                if (StartRuntimeRoutine(SendPrivatePresenceCoroutine()))
                    _privateNextPresenceTime = now + PrivateChatPresenceIntervalSeconds;
                else
                    _privateNextPresenceTime = now + 1f;
            }

            if (!_privatePlayersRequestRunning && now >= _privateNextPlayersPollTime)
            {
                if (StartRuntimeRoutine(PollPrivatePlayersCoroutine()))
                    _privateNextPlayersPollTime = now + PrivateChatPlayersPollIntervalSeconds;
                else
                    _privateNextPlayersPollTime = now + 1f;
            }

            float messageInterval =
                (_root != null && _root.activeSelf && _showingPrivateChat)
                ? PrivateChatOpenPollIntervalSeconds
                : PrivateChatClosedPollIntervalSeconds;

            if (!_privateMessagesRequestRunning && now >= _privateNextMessagesPollTime)
            {
                if (StartRuntimeRoutine(PollPrivateMessagesCoroutine()))
                    _privateNextMessagesPollTime = now + messageInterval;
                else
                    _privateNextMessagesPollTime = now + 1f;
            }
        }

        private static void RequestPrivateChatImmediateRefresh()
        {
            _privateNextPlayersPollTime = 0f;
            _privateNextMessagesPollTime = 0f;
            _privateNextPresenceTime = 0f;
        }

        private static bool StartRuntimeRoutine(IEnumerator routine)
        {
            if (routine == null) return false;
            try
            {
                if (AmongUsClient.Instance != null)
                {
                    AmongUsClient.Instance.StartCoroutine(routine.WrapToIl2Cpp());
                    return true;
                }
            }
            catch { }

            try
            {
                if (_mainMenuOwner != null && _mainMenuOwner.gameObject != null)
                {
                    _mainMenuOwner.StartCoroutine(routine);
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static IEnumerator EnsurePrivateChatSessionCoroutine()
        {
            try
            {
                if (PlayerPrefs.HasKey("CHAT_INSTALLATION_ID"))
                {
                    PlayerPrefs.DeleteKey("CHAT_INSTALLATION_ID");
                    PlayerPrefs.Save();
                }
            }
            catch { }

            if (_privateSessionRequestRunning || !string.IsNullOrWhiteSpace(_privateBanModToken))
                yield break;

            string friendCode = "";
            try { friendCode = BanModIdentity.GetFriendCode() ?? ""; } catch { }
            friendCode = friendCode.Trim();

            if (friendCode.Length < 3)
            {
                _privateNextSessionAttemptTime = Time.unscaledTime + 5f;
                yield break;
            }

            bool tokenReady = false;
            string clientToken = "";
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) =>
            {
                tokenReady = success;
                clientToken = token ?? "";
            });

            if (!tokenReady || string.IsNullOrWhiteSpace(clientToken))
            {
                _privateNextSessionAttemptTime = Time.unscaledTime + 5f;
                yield break;
            }

            _privateSessionRequestRunning = true;
            string json = "{" +
                "\"friend_code\":" + PrivateJsonString(friendCode) + "," +
                "\"platform\":" + PrivateJsonString(Application.platform.ToString()) +
                "}";

            UnityWebRequest request = new UnityWebRequest(CommunityApiBase.TrimEnd('/') + "/api/chat/session", "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 25;
            request.SetRequestHeader("Content-Type", "application/json");
            BanModApiTokenManager.ApplyAuthHeader(request);
            yield return request.SendWebRequest();

            string body = request.downloadHandler != null ? request.downloadHandler.text : "";
            if (request.responseCode >= 200 && request.responseCode < 300)
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        JsonElement root = doc.RootElement;
                        string loginUsername = PrivateReadString(root, "username");
                        if (!string.IsNullOrWhiteSpace(loginUsername))
                            _privateLoginUsername = loginUsername.Trim();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[BANMOD Community] Private Chat session parse failed: " + ex.Message);
                }

                _privateBanModToken = clientToken;
                _privateChatSessionStartedAt = PrivateUnixNowSeconds();
                _privateLastMessageId = 0;
                _privateInitialSyncDone = false;

                PrivateKnownMessageIds.Clear();
                PrivateChatMessages.Clear();
                PrivateUnreadMessageIds.Clear();
                PrivatePendingDeliveryAckIds.Clear();
                PrivateChatTranslations.Clear();

                _privateNextPresenceTime = 0f;
                _privateNextPlayersPollTime = 0f;
                _privateNextMessagesPollTime = 0f;

                RequestPrivateChatImmediateRefresh();
                StartRuntimeRoutine(SendPrivatePresenceCoroutine());
                StartRuntimeRoutine(PollPrivatePlayersCoroutine());
                StartRuntimeRoutine(PollPrivateMessagesCoroutine());
                RefreshCombinedUnread();
                TryUseLoginUsernameAsDefault();
            }
            else
            {
                if (request.responseCode == 401)
                    BanModApiTokenManager.ClearToken(request);
                _privateNextSessionAttemptTime = Time.unscaledTime +
                    ((request.responseCode == 403 || request.responseCode == 409) ? 15f : 5f);
            }

            request.Dispose();
            _privateSessionRequestRunning = false;
        }

        private static void ClearPrivateChatSession()
        {
            _privateBanModToken = "";
            _privateChatSessionStartedAt = 0;
            _privateLastMessageId = 0;
            _privateInitialSyncDone = false;
            PrivatePendingDeliveryAckIds.Clear();
            _privateAckRunning = false;
            _privateNextSessionAttemptTime = 0f;
        }

        private static IEnumerator SendPrivatePresenceCoroutine()
        {
            if (_privatePresenceRequestRunning || string.IsNullOrWhiteSpace(_privateBanModToken))
                yield break;

            _privatePresenceRequestRunning = true;
            string json = "{\"in_lobby\":" + (PrivateChatIsInLobbyOrGame() ? "true" : "false") + "}";
            UnityWebRequest request = CreatePrivateChatRequest(
                CommunityApiBase.TrimEnd('/') + "/api/chat/presence",
                "POST",
                json);

            yield return request.SendWebRequest();
            HandlePrivateChatAuthFailure(request);
            request.Dispose();
            _privatePresenceRequestRunning = false;
        }

        private static IEnumerator PollPrivatePlayersCoroutine()
        {
            if (_privatePlayersRequestRunning || string.IsNullOrWhiteSpace(_privateBanModToken))
                yield break;

            _privatePlayersRequestRunning = true;
            UnityWebRequest request = CreatePrivateChatRequest(
                CommunityApiBase.TrimEnd('/') + "/api/chat/online",
                "GET",
                null);

            yield return request.SendWebRequest();
            if (!HandlePrivateChatAuthFailure(request) &&
                request.responseCode >= 200 && request.responseCode < 300)
            {
                string body = request.downloadHandler != null ? request.downloadHandler.text : "";
                try
                {
                    List<PrivateChatPlayer> parsed = ParsePrivateChatPlayers(body);
                    PrivateChatPlayers.Clear();
                    PrivateChatPlayers.AddRange(parsed);
                    RenderPrivatePlayers();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[BANMOD Community] Private players parse failed: " + ex.Message);
                }
            }

            request.Dispose();
            _privatePlayersRequestRunning = false;
        }

        private static IEnumerator PollPrivateMessagesCoroutine()
        {
            if (_privateMessagesRequestRunning || string.IsNullOrWhiteSpace(_privateBanModToken))
                yield break;

            _privateMessagesRequestRunning = true;

            long now = PrivateUnixNowSeconds();
            long privateSince = Math.Max(0L, _privateChatSessionStartedAt);
            string url = CommunityApiBase.TrimEnd('/') + "/api/chat/messages" +
                "?general_since=" + now +
                "&private_since=" + privateSince +
                "&after_id=" + Math.Max(0L, _privateLastMessageId);

            UnityWebRequest request = CreatePrivateChatRequest(url, "GET", null);
            yield return request.SendWebRequest();

            if (!HandlePrivateChatAuthFailure(request) &&
                request.responseCode >= 200 && request.responseCode < 300)
            {
                string body = request.downloadHandler != null ? request.downloadHandler.text : "";
                try
                {
                    List<PrivateChatMessage> parsed = ParsePrivateChatMessages(body);
                    string ownFriendCode = "";
                    try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

                    bool shouldNotify = _privateInitialSyncDone;
                    bool playIncomingNotification = false;


                    bool keepPrivateAtBottom =
                        PrivateChatMessages.Count == 0 ||
                        IsScrollNearBottom(_privateMessagesScroll);

                    for (int i = 0; i < parsed.Count; i++)
                    {
                        PrivateChatMessage message = parsed[i];
                        if (message == null || message.Id <= 0) continue;

                        if (message.Id > _privateLastMessageId)
                            _privateLastMessageId = message.Id;

                        if (!message.IsPrivate)
                            continue;

                        bool fromMe = string.Equals(message.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
                        bool toMe = string.Equals(message.RecipientFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
                        if (!fromMe && !toMe)
                            continue;

                        bool queuedIncoming = toMe && !fromMe && message.PendingDelivery;
                        if (queuedIncoming)
                            PrivatePendingDeliveryAckIds.Add(message.Id);

                        if (PrivateKnownMessageIds.Contains(message.Id))
                            continue;

                        PrivateKnownMessageIds.Add(message.Id);
                        PrivateChatMessages.Add(message);

                        if (!fromMe && LiveTranslator.GetEnabled())
                            CacheAutomaticPrivateTranslation(message);



                        if (!fromMe && (shouldNotify || queuedIncoming))
                        {


                            playIncomingNotification = true;

                            bool conversationVisible =
                                _root != null && _root.activeSelf && _showingPrivateChat &&
                                string.Equals(_privateActiveFriendCode, message.SenderFriendCode, StringComparison.OrdinalIgnoreCase);

                            if (!conversationVisible)
                                PrivateUnreadMessageIds.Add(message.Id);
                        }
                    }

                    PrivateChatMessages.Sort(delegate (PrivateChatMessage a, PrivateChatMessage b)
                    {
                        return a.Id.CompareTo(b.Id);
                    });

                    _privateInitialSyncDone = true;

                    if (playIncomingNotification)
                        NotifyIncomingPrivateMessage();

                    if (_showingPrivateChat && _root != null && _root.activeSelf)
                    {
                        MarkActivePrivateConversationRead();
                        RenderPrivateMessages(keepPrivateAtBottom);
                        RenderPrivatePlayers();
                    }
                    RefreshCombinedUnread();
                    StartPendingPrivateDeliveryAck();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[BANMOD Community] Private messages parse failed: " + ex.Message);
                }
            }

            request.Dispose();
            _privateMessagesRequestRunning = false;
        }

        private static void StartPendingPrivateDeliveryAck()
        {
            if (_privateAckRunning ||
                PrivatePendingDeliveryAckIds.Count == 0 ||
                string.IsNullOrWhiteSpace(_privateBanModToken))
            {
                return;
            }

            _privateAckRunning = true;
            if (!StartRuntimeRoutine(AckPendingPrivateMessagesCoroutine()))
                _privateAckRunning = false;
        }

        private static IEnumerator AckPendingPrivateMessagesCoroutine()
        {
            List<long> ids = new List<long>(PrivatePendingDeliveryAckIds);
            if (ids.Count == 0)
            {
                _privateAckRunning = false;
                yield break;
            }

            StringBuilder json = new StringBuilder();
            json.Append("{\"message_ids\":[");
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) json.Append(',');
                json.Append(ids[i]);
            }
            json.Append("]}");

            UnityWebRequest request = CreatePrivateChatRequest(
                CommunityApiBase.TrimEnd('/') + "/api/chat/messages/ack",
                "POST",
                json.ToString());

            yield return request.SendWebRequest();

            bool authFailed = HandlePrivateChatAuthFailure(request);
            if (!authFailed && request.responseCode >= 200 && request.responseCode < 300)
            {
                for (int i = 0; i < ids.Count; i++)
                    PrivatePendingDeliveryAckIds.Remove(ids[i]);
            }

            request.Dispose();
            _privateAckRunning = false;
        }

        private static void SelectPrivatePlayer(string friendCode, string playerName)
        {
            friendCode = (friendCode ?? "").Trim();
            if (friendCode.Length == 0) return;

            _privateActiveFriendCode = friendCode;
            _privateActiveName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
            _selectedPrivateMessageIndex = -1;
            if (_privateChatInput != null)
            {
                _privateChatInput.interactable = true;
                if (_privateChatInput.placeholder is TextMeshProUGUI placeholder)
                    placeholder.text = "Message " + _privateActiveName + "...";
            }
            if (_privateConversationTitle != null)
                _privateConversationTitle.text = "PRIVATE CHAT — " + _privateActiveName;

            MarkActivePrivateConversationRead();
            RenderPrivatePlayers();
            RenderPrivateMessages(true);
            SetStatus("Private Chat with " + _privateActiveName);
        }

        private static void MarkActivePrivateConversationRead()
        {
            if (string.IsNullOrWhiteSpace(_privateActiveFriendCode))
                return;

            string ownFriendCode = "";
            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

            for (int i = PrivateChatMessages.Count - 1; i >= 0; i--)
            {
                PrivateChatMessage message = PrivateChatMessages[i];
                if (message == null) continue;

                bool inboundFromActive =
                    string.Equals(message.SenderFriendCode, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(message.RecipientFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);

                if (inboundFromActive)
                    PrivateUnreadMessageIds.Remove(message.Id);
            }

            RefreshCombinedUnread();
        }

        private static int PrivateUnreadCountForPlayer(string friendCode)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(friendCode)) return 0;

            for (int i = 0; i < PrivateChatMessages.Count; i++)
            {
                PrivateChatMessage message = PrivateChatMessages[i];
                if (message == null || !PrivateUnreadMessageIds.Contains(message.Id)) continue;
                if (string.Equals(message.SenderFriendCode, friendCode, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        private static void RenderPrivatePlayers()
        {
            if (_privatePlayersContent == null) return;

            ClearRectChildren(_privatePlayersContent);

            List<PrivateChatPlayer> online = new List<PrivateChatPlayer>();
            string ownFriendCode = "";
            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

            for (int i = 0; i < PrivateChatPlayers.Count; i++)
            {
                PrivateChatPlayer p = PrivateChatPlayers[i];
                if (p == null || !p.IsOnline) continue;
                if (string.Equals(p.FriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase)) continue;
                online.Add(p);
            }

            online.Sort(delegate (PrivateChatPlayer a, PrivateChatPlayer b)
            {
                int unreadA = PrivateUnreadCountForPlayer(a == null ? "" : a.FriendCode);
                int unreadB = PrivateUnreadCountForPlayer(b == null ? "" : b.FriendCode);
                if ((unreadA > 0) != (unreadB > 0))
                    return unreadA > 0 ? -1 : 1;
                return string.Compare(a == null ? "" : a.PlayerName, b == null ? "" : b.PlayerName, StringComparison.OrdinalIgnoreCase);
            });

            float top = 8f;
            if (online.Count == 0)
            {
                CreateLabel(_privatePlayersContent, "NoOnline", "No users online.", 14, TextAlignmentOptions.Center,
                    new Color(0.68f, 0.72f, 0.80f, 1f), new Vector2(0f, -30f), new Vector2(240f, 40f));
                _privatePlayersContent.sizeDelta = new Vector2(0f, 440f);
                return;
            }

            for (int i = 0; i < online.Count; i++)
            {
                PrivateChatPlayer player = online[i];
                string fc = player.FriendCode;
                string name = string.IsNullOrWhiteSpace(player.PlayerName) ? "Player" : player.PlayerName;
                int unread = PrivateUnreadCountForPlayer(fc);
                string label = (unread > 0 ? "● " : "") + name;
                if (unread > 1) label += "  (" + unread + ")";

                bool selected = string.Equals(fc, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase);
                Button row = CreateButton(
                    _privatePlayersContent,
                    "PrivatePlayer_" + i,
                    label,
                    new Vector2(0f, -(top + 22f)),
                    new Vector2(240f, 44f),
                    selected ? new Color(0.10f, 0.34f, 0.48f, 0.98f) : new Color(0.12f, 0.24f, 0.18f, 0.95f),
                    delegate { SelectPrivatePlayer(fc, name); });

                RectTransform playerRowRect = row == null ? null : row.GetComponent<RectTransform>();
                if (playerRowRect != null)
                {
                    playerRowRect.anchorMin = playerRowRect.anchorMax = new Vector2(0.5f, 1f);
                    playerRowRect.pivot = new Vector2(0.5f, 1f);
                    playerRowRect.anchoredPosition = new Vector2(0f, -top);
                }
                TextMeshProUGUI labelText = row == null ? null : row.GetComponentInChildren<TextMeshProUGUI>(true);
                if (labelText != null)
                {
                    labelText.alignment = TextAlignmentOptions.MidlineLeft;
                    labelText.margin = new Vector4(12f, 0f, 8f, 0f);
                }
                top += 48f;
            }

            _privatePlayersContent.sizeDelta = new Vector2(0f, Math.Max(440f, top + 10f));
        }

        private static void RenderPrivateMessages(bool scrollToBottom = false)
        {
            if (_privateMessagesContent == null) return;

            float previousScroll = 0f;
            try
            {
                if (_privateMessagesScroll != null)
                    previousScroll = _privateMessagesScroll.verticalNormalizedPosition;
            }
            catch { }

            ClearRectChildren(_privateMessagesContent);

            if (string.IsNullOrWhiteSpace(_privateActiveFriendCode))
            {
                CreateLabel(_privateMessagesContent, "SelectPlayerHint", "Select an online user from the list.", 16,
                    TextAlignmentOptions.Center, new Color(0.72f, 0.76f, 0.86f, 1f),
                    new Vector2(0f, -40f), new Vector2(680f, 50f));
                _privateMessagesContent.sizeDelta = new Vector2(0f, 340f);
                if (_privateTranslateButton != null) _privateTranslateButton.interactable = false;
                if (_privateReportButton != null) _privateReportButton.interactable = false;
                return;
            }

            string ownFriendCode = "";
            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

            float top = 8f;
            int visibleIndex = 0;

            for (int i = 0; i < PrivateChatMessages.Count; i++)
            {
                PrivateChatMessage message = PrivateChatMessages[i];
                if (message == null || !message.IsPrivate) continue;

                bool fromActiveToMe =
                    string.Equals(message.SenderFriendCode, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(message.RecipientFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
                bool fromMeToActive =
                    string.Equals(message.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(message.RecipientFriendCode, _privateActiveFriendCode, StringComparison.OrdinalIgnoreCase);

                if (!fromActiveToMe && !fromMeToActive)
                    continue;

                bool own = fromMeToActive;
                string sender = own
                    ? (string.IsNullOrWhiteSpace(_communityDisplayName) ? "You" : _communityDisplayName)
                    : (string.IsNullOrWhiteSpace(message.SenderName) ? _privateActiveName : message.SenderName);




                if (fromActiveToMe && LiveTranslator.GetEnabled())
                    CacheAutomaticPrivateTranslation(message);

                string displayedMessage = GetPrivateDisplayedMessage(message);
                string lineRaw = sender + "  ·  " + FormatTimestamp(message.CreatedAt) + "\n" + displayedMessage;
                string line = BanModSymbolPicker.ToSafeRichText(lineRaw);
                int originalIndex = i;
                bool selected = originalIndex == _selectedPrivateMessageIndex;

                Button row = CreateButton(
                    _privateMessagesContent,
                    "PrivateMessage_" + visibleIndex,
                    line,
                    new Vector2(0f, -(top + 30f)),
                    new Vector2(700f, 60f),
                    selected
                        ? new Color(0.25f, 0.30f, 0.44f, 0.98f)
                        : (own ? new Color(0.10f, 0.24f, 0.34f, 0.95f) : new Color(0.16f, 0.16f, 0.18f, 0.95f)),
                    delegate { SelectPrivateMessage(originalIndex); });

                float rowHeight = 60f;
                TextMeshProUGUI label = row == null ? null : row.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.richText = true;
                    label.alignment = TextAlignmentOptions.TopLeft;
                    label.margin = new Vector4(10f, 7f, 10f, 7f);
                    label.enableWordWrapping = true;
                    label.overflowMode = TextOverflowModes.Overflow;
                    try
                    {
                        float preferred = label.GetPreferredValues(line, 680f, 0f).y;
                        rowHeight = Mathf.Clamp(preferred + 18f, 60f, 320f);
                    }
                    catch { }
                }

                RectTransform rowRect = row == null ? null : row.GetComponent<RectTransform>();
                if (rowRect != null)
                {
                    rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 1f);
                    rowRect.pivot = new Vector2(0.5f, 1f);
                    rowRect.sizeDelta = new Vector2(700f, rowHeight);
                    rowRect.anchoredPosition = new Vector2(0f, -top);
                }

                top += rowHeight + 6f;
                visibleIndex++;
            }

            if (visibleIndex == 0)
            {
                CreateLabel(_privateMessagesContent, "NoMessages", "No private messages in this session yet.", 15,
                    TextAlignmentOptions.Center, new Color(0.68f, 0.72f, 0.80f, 1f),
                    new Vector2(0f, -40f), new Vector2(680f, 50f));
                top = 330f;
            }

            _privateMessagesContent.sizeDelta = new Vector2(0f, Math.Max(340f, top + 12f));

            bool hasSelectedMessage =
                _selectedPrivateMessageIndex >= 0 &&
                _selectedPrivateMessageIndex < PrivateChatMessages.Count &&
                PrivateChatMessages[_selectedPrivateMessageIndex] != null;

            if (_privateTranslateButton != null)
                _privateTranslateButton.interactable = hasSelectedMessage;

            if (_privateReportButton != null)
            {
                bool canReport = false;
                if (_selectedPrivateMessageIndex >= 0 && _selectedPrivateMessageIndex < PrivateChatMessages.Count)
                {
                    PrivateChatMessage selected = PrivateChatMessages[_selectedPrivateMessageIndex];
                    canReport = selected != null &&
                        !string.Equals(selected.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase);
                }
                _privateReportButton.interactable = canReport;
            }

            if (_privateMessagesScroll != null)
            {
                try
                {
                    Canvas.ForceUpdateCanvases();
                    _privateMessagesScroll.StopMovement();
                    _privateMessagesScroll.verticalNormalizedPosition =
                        scrollToBottom ? 0f : previousScroll;
                }
                catch { }
            }
        }

        private static void SelectPrivateMessage(int index)
        {
            if (index < 0 || index >= PrivateChatMessages.Count) return;
            _selectedPrivateMessageIndex = index;
            RenderPrivateMessages(false);
        }

        private static void CacheAutomaticCommunityTranslation(
            Dictionary<int, PrivateChatTranslationEntry> cache,
            int id,
            string original,
            string context)
        {
            if (cache == null ||
                id <= 0 ||
                string.IsNullOrWhiteSpace(original) ||
                !LiveTranslator.GetEnabled())
            {
                return;
            }

            string settingsKey = GetPrivateTranslationSettingsKey();

            PrivateChatTranslationEntry existing;
            if (cache.TryGetValue(id, out existing) &&
                existing != null &&
                existing.Attempted &&
                string.Equals(existing.SettingsKey, settingsKey, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.Original, original, StringComparison.Ordinal))
            {
                return;
            }

            PrivateChatTranslationEntry entry = new PrivateChatTranslationEntry
            {
                SettingsKey = settingsKey,
                Original = original ?? "",
                Translation = "",
                Attempted = true,
                Manual = false
            };
            cache[id] = entry;

            try
            {
                string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
                    original,
                    delegate (string part) { return LiveTranslator.TranslateExternalIncomingForRead(part, false); });
                if (!string.IsNullOrWhiteSpace(translated) &&
                    !string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
                {
                    entry.Translation = translated;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning("[BANMOD Community] " + context + " automatic translation failed: " + ex.Message);
                }
                catch { }
            }
        }

        private static string GetCommunityDisplayedText(
            Dictionary<int, PrivateChatTranslationEntry> cache,
            int id,
            string original)
        {
            original = original ?? "";
            if (cache == null || id <= 0)
                return original;

            PrivateChatTranslationEntry entry;
            if (!cache.TryGetValue(id, out entry) ||
                entry == null ||
                string.IsNullOrWhiteSpace(entry.Translation) ||
                !string.Equals(entry.SettingsKey, GetPrivateTranslationSettingsKey(), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(entry.Original, original, StringComparison.Ordinal))
            {
                return original;
            }

            if (string.Equals(entry.Translation, original, StringComparison.OrdinalIgnoreCase))
                return original;

            if (!LiveTranslator.GetEnabled() && !entry.Manual)
                return original;

            if (!LiveTranslator.GetShowOriginalIncoming())
                return entry.Translation;

            return original + "\n────────────\n[TR] " + entry.Translation;
        }

        private static bool TryCacheManualCommunityTranslation(
            Dictionary<int, PrivateChatTranslationEntry> cache,
            int id,
            string original,
            string context)
        {
            if (cache == null || id <= 0 || string.IsNullOrWhiteSpace(original))
                return false;

            string translated = "";
            try
            {
                translated = BanModSymbolPicker.TranslatePreservingPickerColors(
                    original,
                    delegate (string part) { return LiveTranslator.TranslateExternalSelectedForRead(part); });
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning("[BANMOD Community] Manual " + context + " translation failed: " + ex.Message);
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(translated) ||
                string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cache[id] = new PrivateChatTranslationEntry
            {
                SettingsKey = GetPrivateTranslationSettingsKey(),
                Original = original,
                Translation = translated,
                Attempted = true,
                Manual = true
            };
            return true;
        }

        private static string TranslateOutgoingCommunityText(string value, string context, int maxLength)
        {
            string result = (value ?? "").Trim();
            if (result.Length == 0)
                return result;

            if (LiveTranslator.GetEnabled())
            {
                try
                {
                    string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
                        result,
                        delegate (string part) { return LiveTranslator.TranslateExternalOutgoingForSend(part); });
                    if (!string.IsNullOrWhiteSpace(translated))
                        result = translated.Trim();
                }
                catch (Exception ex)
                {
                    try
                    {
                        Debug.LogWarning("[BANMOD Community] " + context + " outgoing translation failed: " + ex.Message);
                    }
                    catch { }
                }
            }

            if (maxLength > 0 && result.Length > maxLength)
                result = result.Substring(0, maxLength);

            return result;
        }

        private static void TranslateSelectedCommunityChatMessage()
        {
            if (_selectedChatIndex < 0 || _selectedChatIndex >= ChatMessages.Count)
            {
                SetStatus("Select a Community Chat message first.");
                return;
            }

            CommunityChatMessage message = ChatMessages[_selectedChatIndex];
            if (message == null || string.IsNullOrWhiteSpace(message.Message))
            {
                SetStatus("The selected Community Chat message is empty.");
                return;
            }

            if (!TryCacheManualCommunityTranslation(
                    CommunityChatTranslations,
                    message.Id,
                    message.Message,
                    "Community Chat"))
            {
                SetStatus("No translation available, or the message is already in the selected language.");
                return;
            }

            RenderChat(false);
            SetStatus("Selected Community Chat message translated.");
        }

        private static void TranslateSelectedSuggestionContent()
        {
            if (_selected == null || _selected.Id <= 0)
            {
                SetStatus("Select a suggestion first.");
                return;
            }

            if (_selectedReplyIndex >= 0 && _selectedReplyIndex < Replies.Count)
            {
                CommunityReply reply = Replies[_selectedReplyIndex];
                if (reply == null || string.IsNullOrWhiteSpace(reply.Message))
                {
                    SetStatus("The selected comment is empty.");
                    return;
                }

                if (!TryCacheManualCommunityTranslation(
                        SuggestionReplyTranslations,
                        reply.Id,
                        reply.Message,
                        "suggestion comment"))
                {
                    SetStatus("No translation available, or the comment is already in the selected language.");
                    return;
                }

                RenderSelected();
                SetStatus("Selected suggestion comment translated.");
                return;
            }

            bool translatedAnything = false;
            if (!string.IsNullOrWhiteSpace(_selected.Title))
            {
                translatedAnything |= TryCacheManualCommunityTranslation(
                    SuggestionTitleTranslations,
                    _selected.Id,
                    _selected.Title,
                    "suggestion title");
            }

            if (!string.IsNullOrWhiteSpace(_selected.Message))
            {
                translatedAnything |= TryCacheManualCommunityTranslation(
                    SuggestionMessageTranslations,
                    _selected.Id,
                    _selected.Message,
                    "suggestion");
            }

            if (!translatedAnything)
            {
                SetStatus("No translation available, or the suggestion is already in the selected language.");
                return;
            }

            RenderSelected();
            SetStatus("Selected suggestion translated.");
        }

        private static string GetPrivateTranslationSettingsKey()
        {
            try
            {
                return
                    (LiveTranslator.GetReadFromLang() ?? "auto").Trim().ToLowerInvariant() + "->" +
                    (LiveTranslator.GetReadToLang() ?? "among").Trim().ToLowerInvariant() +
                    "|among=" + (LiveTranslator.GetAmongUserLangCode() ?? "en").Trim().ToLowerInvariant() +
                    "|provider=" + (LiveTranslator.GetProvider() ?? "").Trim().ToLowerInvariant();
            }
            catch
            {
                return "auto->among|among=en";
            }
        }

        private static void CacheAutomaticPrivateTranslation(PrivateChatMessage message)
        {
            if (message == null ||
                message.Id <= 0 ||
                string.IsNullOrWhiteSpace(message.Message) ||
                !LiveTranslator.GetEnabled())
            {
                return;
            }

            string settingsKey = GetPrivateTranslationSettingsKey();
            string original = message.Message ?? "";

            PrivateChatTranslationEntry existing;
            if (PrivateChatTranslations.TryGetValue(message.Id, out existing) &&
                existing != null &&
                existing.Attempted &&
                string.Equals(existing.SettingsKey, settingsKey, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.Original, original, StringComparison.Ordinal))
            {
                return;
            }



            PrivateChatTranslationEntry entry = new PrivateChatTranslationEntry
            {
                SettingsKey = settingsKey,
                Original = original,
                Translation = "",
                Attempted = true,
                Manual = false
            };
            PrivateChatTranslations[message.Id] = entry;

            try
            {
                string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
                    original,
                    delegate (string part) { return LiveTranslator.TranslateExternalIncomingForRead(part, false); });
                if (!string.IsNullOrWhiteSpace(translated) &&
                    !string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
                {
                    entry.Translation = translated;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning(
                        "[BANMOD Community] Private automatic translation failed: " + ex.Message);
                }
                catch { }
            }
        }

        private static string GetPrivateDisplayedMessage(PrivateChatMessage message)
        {
            if (message == null)
                return "";

            string original = message.Message ?? "";
            if (message.Id <= 0)
                return original;

            PrivateChatTranslationEntry entry;
            if (!PrivateChatTranslations.TryGetValue(message.Id, out entry) ||
                entry == null ||
                string.IsNullOrWhiteSpace(entry.Translation) ||
                !string.Equals(entry.SettingsKey, GetPrivateTranslationSettingsKey(), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(entry.Original, original, StringComparison.Ordinal))
            {
                return original;
            }

            if (string.Equals(entry.Translation, original, StringComparison.OrdinalIgnoreCase))
                return original;



            if (!LiveTranslator.GetEnabled() && !entry.Manual)
                return original;

            if (!LiveTranslator.GetShowOriginalIncoming())
                return entry.Translation;

            return original + "\n────────────\n[TR] " + entry.Translation;
        }

        private static void TranslateSelectedPrivateMessage()
        {
            if (_selectedPrivateMessageIndex < 0 ||
                _selectedPrivateMessageIndex >= PrivateChatMessages.Count)
            {
                SetStatus("Select a private message first.");
                return;
            }

            PrivateChatMessage message = PrivateChatMessages[_selectedPrivateMessageIndex];
            if (message == null || string.IsNullOrWhiteSpace(message.Message))
            {
                SetStatus("The selected private message is empty.");
                return;
            }

            string original = message.Message ?? "";
            string translated = "";

            try
            {

                translated = BanModSymbolPicker.TranslatePreservingPickerColors(
                    original,
                    delegate (string part) { return LiveTranslator.TranslateExternalSelectedForRead(part); });
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning(
                        "[BANMOD Community] Manual private translation failed: " + ex.Message);
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(translated) ||
                string.Equals(translated, original, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("No translation available, or the message is already in the selected language.");
                return;
            }

            PrivateChatTranslations[message.Id] = new PrivateChatTranslationEntry
            {
                SettingsKey = GetPrivateTranslationSettingsKey(),
                Original = original,
                Translation = translated,
                Attempted = true,
                Manual = true
            };

            RenderPrivateMessages(false);
            SetStatus("Selected private message translated.");
        }

        private static void SubmitPrivateChatMessage()
        {
            if (_privateSendRunning) return;
            if (string.IsNullOrWhiteSpace(_privateActiveFriendCode))
            {
                SetStatus("Select an online user first.");
                return;
            }

            string message = (_privateChatInput == null ? "" : (_privateChatInput.text ?? "")).Trim();
            if (message.Length == 0)
            {
                SetStatus("Write a private message first.");
                return;
            }



            if (LiveTranslator.GetEnabled())
            {
                try
                {
                    string translated = BanModSymbolPicker.TranslatePreservingPickerColors(
                        message,
                        delegate (string part) { return LiveTranslator.TranslateExternalOutgoingForSend(part); });
                    if (!string.IsNullOrWhiteSpace(translated))
                        message = translated;
                }
                catch (Exception ex)
                {
                    try
                    {
                        Debug.LogWarning(
                            "[BANMOD Community] Private outgoing translation failed: " + ex.Message);
                    }
                    catch { }
                }
            }



            if (message.Length > 300)
                message = message.Substring(0, 300);

            if (!StartRuntimeRoutine(SendPrivateChatMessageCoroutine(message, _privateActiveFriendCode)))
                SetStatus("Private Chat runtime is unavailable.");
        }

        private static IEnumerator SendPrivateChatMessageCoroutine(string message, string recipientFriendCode)
        {
            if (_privateSendRunning) yield break;
            _privateSendRunning = true;

            if (string.IsNullOrWhiteSpace(_privateBanModToken))
            {
                yield return EnsurePrivateChatSessionCoroutine();
                if (string.IsNullOrWhiteSpace(_privateBanModToken))
                {
                    _privateSendRunning = false;
                    SetStatus("Private Chat session is unavailable.");
                    yield break;
                }
            }

            string json = "{" +
                "\"message\":" + PrivateJsonString(message) + "," +
                "\"recipient_friend_code\":" + PrivateJsonString(recipientFriendCode) +
                "}";

            UnityWebRequest request = CreatePrivateChatRequest(
                CommunityApiBase.TrimEnd('/') + "/api/chat/messages",
                "POST",
                json);

            yield return request.SendWebRequest();
            string body = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (HandlePrivateChatAuthFailure(request))
            {
                SetStatus("Private Chat session expired. Try again.");
            }
            else if (request.responseCode >= 200 && request.responseCode < 300)
            {
                if (_privateChatInput != null) _privateChatInput.text = "";
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("message", out JsonElement msg) && msg.ValueKind == JsonValueKind.Object)
                        {
                            PrivateChatMessage parsed = ParsePrivateChatMessage(msg);
                            if (parsed != null && parsed.Id > 0 && !PrivateKnownMessageIds.Contains(parsed.Id))
                            {
                                PrivateKnownMessageIds.Add(parsed.Id);
                                PrivateChatMessages.Add(parsed);
                                if (parsed.Id > _privateLastMessageId) _privateLastMessageId = parsed.Id;
                            }
                        }
                    }
                }
                catch { }

                PrivateChatMessages.Sort(delegate (PrivateChatMessage a, PrivateChatMessage b)
                {
                    return a.Id.CompareTo(b.Id);
                });
                RenderPrivateMessages(true);
                SetStatus("Private message sent.");
                _privateNextMessagesPollTime = 0f;
                _privateNextPresenceTime = 0f;
            }
            else
            {
                SetStatus("Private send failed: " + PrivateChatError(body, "HTTP " + request.responseCode));
            }

            request.Dispose();
            _privateSendRunning = false;
        }

        private static void ReportSelectedPrivateMessage()
        {
            if (_privateReportRunning) return;
            if (_selectedPrivateMessageIndex < 0 || _selectedPrivateMessageIndex >= PrivateChatMessages.Count)
            {
                SetStatus("Select a private message first.");
                return;
            }

            PrivateChatMessage message = PrivateChatMessages[_selectedPrivateMessageIndex];
            string ownFriendCode = "";
            try { ownFriendCode = (BanModIdentity.GetFriendCode() ?? "").Trim(); } catch { }

            if (message == null || string.Equals(message.SenderFriendCode, ownFriendCode, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("You cannot report your own message.");
                return;
            }

            StartRuntimeRoutine(ReportPrivateMessageCoroutine(message.Id));
        }

        private static IEnumerator ReportPrivateMessageCoroutine(long messageId)
        {
            if (_privateReportRunning || messageId <= 0) yield break;
            _privateReportRunning = true;

            string json = "{\"message_id\":" + messageId + ",\"reason\":\"inappropriate\"}";
            UnityWebRequest request = CreatePrivateChatRequest(
                CommunityApiBase.TrimEnd('/') + "/api/chat/report",
                "POST",
                json);

            yield return request.SendWebRequest();
            string body = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (HandlePrivateChatAuthFailure(request))
                SetStatus("Private Chat session expired.");
            else if (request.responseCode >= 200 && request.responseCode < 300)
                SetStatus("Private message reported.");
            else
                SetStatus("Private report failed: " + PrivateChatError(body, "HTTP " + request.responseCode));

            request.Dispose();
            _privateReportRunning = false;
        }

        private static UnityWebRequest CreatePrivateChatRequest(string url, string method, string json)
        {
            UnityWebRequest request;
            if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                request = UnityWebRequest.Get(url);
                request.downloadHandler = new DownloadHandlerBuffer();
            }
            else
            {
                request = new UnityWebRequest(url, method);
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json ?? "{}"));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.timeout = 25;
            BanModApiTokenManager.ApplyAuthHeader(request);
            return request;
        }

        private static bool HandlePrivateChatAuthFailure(UnityWebRequest request)
        {
            if (request == null) return false;
            if (request.responseCode == 401)
            {
                BanModApiTokenManager.ClearToken(request);
                ClearPrivateChatSession();
                return true;
            }
            if (request.responseCode == 403)
            {
                string body = request.downloadHandler != null ? request.downloadHandler.text : "";
                SetStatus(PrivateChatError(body, "Private Chat access denied."));
                return true;
            }
            return false;
        }

        private static List<PrivateChatPlayer> ParsePrivateChatPlayers(string json)
        {
            List<PrivateChatPlayer> players = new List<PrivateChatPlayer>();
            if (string.IsNullOrWhiteSpace(json)) return players;

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("players", out JsonElement array) || array.ValueKind != JsonValueKind.Array)
                    return players;

                foreach (JsonElement element in array.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object) continue;
                    long lastSeen = PrivateReadLong(element, "last_seen", 0L);
                    bool inferredOnline = lastSeen > 0 && lastSeen >= PrivateUnixNowSeconds() - PrivateChatOnlineThresholdSeconds;
                    players.Add(new PrivateChatPlayer
                    {
                        FriendCode = PrivateReadString(element, "friend_code"),
                        PlayerName = PrivateReadString(element, "player_name"),
                        IsOnline = PrivateReadBool(element, "online", inferredOnline),
                        InLobby = PrivateReadBool(element, "in_lobby", false),
                        LastSeen = lastSeen
                    });
                }
            }

            return players;
        }

        private static List<PrivateChatMessage> ParsePrivateChatMessages(string json)
        {
            List<PrivateChatMessage> messages = new List<PrivateChatMessage>();
            if (string.IsNullOrWhiteSpace(json)) return messages;

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("messages", out JsonElement array) || array.ValueKind != JsonValueKind.Array)
                    return messages;

                foreach (JsonElement element in array.EnumerateArray())
                {
                    PrivateChatMessage message = ParsePrivateChatMessage(element);
                    if (message != null) messages.Add(message);
                }
            }

            return messages;
        }

        private static PrivateChatMessage ParsePrivateChatMessage(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;

            string recipientFriendCode = PrivateReadString(element, "recipient_friend_code");
            return new PrivateChatMessage
            {
                Id = PrivateReadLong(element, "id", 0L),
                SenderFriendCode = PrivateReadString(element, "sender_friend_code"),
                SenderName = PrivateReadString(element, "sender_name"),
                RecipientFriendCode = recipientFriendCode,
                RecipientName = PrivateReadString(element, "recipient_name"),
                IsPrivate = PrivateReadBool(element, "is_private", !string.IsNullOrWhiteSpace(recipientFriendCode)),
                PendingDelivery = PrivateReadBool(element, "pending_delivery", false),
                Message = PrivateReadString(element, "message"),
                CreatedAt = PrivateReadLong(element, "created_at", 0L)
            };
        }

        private static string PrivateChatError(string json, string fallback)
        {
            if (string.IsNullOrWhiteSpace(json)) return fallback;
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;
                    string error = PrivateReadString(root, "error");
                    string message = PrivateReadString(root, "message");
                    string reason = PrivateReadString(root, "reason");
                    string result = !string.IsNullOrWhiteSpace(error)
                        ? error
                        : (!string.IsNullOrWhiteSpace(message) ? message : fallback);
                    if (!string.IsNullOrWhiteSpace(reason) &&
                        result.IndexOf(reason, StringComparison.OrdinalIgnoreCase) < 0)
                        result += " Reason: " + reason + ".";
                    return result;
                }
            }
            catch
            {
                return fallback;
            }
        }

        private static string PrivateReadString(JsonElement root, string name)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty(name, out JsonElement value) &&
                value.ValueKind == JsonValueKind.String)
                return value.GetString() ?? "";
            return "";
        }

        private static long PrivateReadLong(JsonElement root, string name, long fallback)
        {
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty(name, out JsonElement value))
                return fallback;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number))
                return number;
            if (value.ValueKind == JsonValueKind.String &&
                long.TryParse(value.GetString(), out long parsed))
                return parsed;
            return fallback;
        }

        private static bool PrivateReadBool(JsonElement root, string name, bool fallback)
        {
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty(name, out JsonElement value))
                return fallback;
            if (value.ValueKind == JsonValueKind.True) return true;
            if (value.ValueKind == JsonValueKind.False) return false;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
                return number != 0;
            if (value.ValueKind == JsonValueKind.String &&
                bool.TryParse(value.GetString(), out bool parsed))
                return parsed;
            return fallback;
        }

        private static string PrivateJsonString(string value)
        {
            value = value ?? "";
            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t") + "\"";
        }

        private static long PrivateUnixNowSeconds()
        {
            try { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
            catch { return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }
        }

        private static bool PrivateChatIsInLobbyOrGame()
        {
            try
            {
                if (AmongUsClient.Instance == null) return false;
                string state = AmongUsClient.Instance.GameState.ToString();
                return state.IndexOf("Joined", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       state.IndexOf("Started", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static void ClearRectChildren(RectTransform content)
        {
            if (content == null) return;
            try
            {
                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    Transform child = content.GetChild(i);
                    if (child != null)
                        UnityEngine.Object.Destroy(child.gameObject);
                }
            }
            catch { }
        }

        private static void StartRoutine(IEnumerator routine)
        {
            if (routine == null) return;
            if (_mainMenuOwner != null && _mainMenuOwner.gameObject != null && _mainMenuOwner.gameObject.activeInHierarchy)
            {
                _mainMenuOwner.StartCoroutine(routine);
                return;
            }

            try
            {
                if (AmongUsClient.Instance != null)
                {
                    AmongUsClient.Instance.StartCoroutine(routine.WrapToIl2Cpp());
                    return;
                }
            }
            catch { }

            Debug.LogError("[BANMOD Community] Coroutine host unavailable.");
        }

        private static UnityWebRequest CreateRequest(string method, string url, string json)
        {
            try
            {
                UnityWebRequest request;
                if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    request = UnityWebRequest.Get(url);
                    request.downloadHandler = new DownloadHandlerBuffer();
                }
                else
                {
                    request = new UnityWebRequest(url, method);
                    if (json != null)
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                request.timeout = 30;
                request.SetRequestHeader("Accept", "application/json");
                BanModApiTokenManager.ApplyAuthHeader(request);



                string playerName = HeaderSafe(BanModIdentity.GetPlayerName());
                if (!string.IsNullOrWhiteSpace(playerName))
                    request.SetRequestHeader("X-BANMOD-PlayerName", playerName);

                string platform = HeaderSafe(BanModIdentity.GetPlatform());
                if (!string.IsNullOrWhiteSpace(platform))
                    request.SetRequestHeader("X-BANMOD-Platform", platform);

                return request;
            }
            catch (Exception ex)
            {
                Debug.LogError("[BANMOD Community] Request creation failed: " + ex.Message);
                return null;
            }
        }

        private static string HeaderSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            StringBuilder safe = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c >= 0x20 && c <= 0x7E) safe.Append(c);
            }
            return safe.ToString().Trim();
        }


        private static IEnumerator SendRequest(string method, string url, object payload, Action<bool, string> callback)
        {
            bool identityReady = false;
            yield return BanModApiTokenManager.EnsureTokenCoroutine((success, token) =>
            {
                identityReady = success && !string.IsNullOrWhiteSpace(token);
            });

            if (!identityReady)
            {
                callback?.Invoke(false, "FriendCode unavailable. Please wait for BanMod login to finish.");
                yield break;
            }

            string json = payload == null ? null : JsonSerializer.Serialize(payload, JsonOptions);
            UnityWebRequest request = CreateRequest(method, url, json);
            if (request == null)
            {
                callback?.Invoke(false, "Could not create request.");
                yield break;
            }

            yield return request.SendWebRequest();

            string body = "";
            try { body = request.downloadHandler != null ? (request.downloadHandler.text ?? "") : ""; } catch { }
            long code = 0;
            try { code = request.responseCode; } catch { }
            bool ok = request.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;


            if (!ok)
            {
                string error = request.error ?? "request failed";
                if (string.IsNullOrWhiteSpace(body))
                    body = code > 0 ? ("HTTP " + code + ": " + error) : error;
                Debug.LogWarning("[BANMOD Community] " + method + " " + url + " failed: HTTP=" + code + " result=" + request.result + " error=" + error);
            }

            try { request.Dispose(); } catch { }
            callback?.Invoke(ok, body);
        }


        private static List<CommunityPost> ParsePosts(string json)
        {
            var result = new List<CommunityPost>();
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("posts", out var posts) || posts.ValueKind != JsonValueKind.Array) return result;
                foreach (var item in posts.EnumerateArray())
                {
                    var p = new CommunityPost();
                    if (item.TryGetProperty("id", out var id)) p.Id = id.GetInt32();
                    if (item.TryGetProperty("kind", out var k)) p.Kind = k.GetString() ?? "suggestion";
                    if (item.TryGetProperty("title", out var t)) p.Title = t.GetString() ?? "";
                    if (item.TryGetProperty("message", out var m)) p.Message = m.GetString() ?? "";
                    if (item.TryGetProperty("status", out var st)) p.Status = st.GetString() ?? "open";
                    if (item.TryGetProperty("created_at", out var c)) p.CreatedAt = c.GetInt64();
                    if (item.TryGetProperty("updated_at", out var u)) p.UpdatedAt = u.GetInt64();
                    if (item.TryGetProperty("stars", out var stars)) p.Stars = stars.GetInt32();
                    if (item.TryGetProperty("reply_count", out var rc)) p.ReplyCount = rc.GetInt32();
                    if (item.TryGetProperty("my_star", out var ms)) p.MyStar = ms.GetBoolean();
                    if (item.TryGetProperty("my_post", out var mp)) p.MyPost = mp.GetBoolean();
                    if (item.TryGetProperty("author", out var author)) p.Author = author.GetString() ?? "BanModUser";
                    if (p.Id > 0) result.Add(p);
                }
            }
            return result;
        }

        private static List<CommunityReply> ParseReplies(string json)
        {
            var result = new List<CommunityReply>();
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("replies", out var replies) || replies.ValueKind != JsonValueKind.Array) return result;
                foreach (var item in replies.EnumerateArray())
                {
                    var r = new CommunityReply();
                    if (item.TryGetProperty("id", out var id)) r.Id = id.GetInt32();
                    if (item.TryGetProperty("post_id", out var pid)) r.PostId = pid.GetInt32();
                    if (item.TryGetProperty("message", out var m)) r.Message = m.GetString() ?? "";
                    if (item.TryGetProperty("created_at", out var c)) r.CreatedAt = c.GetInt64();
                    if (item.TryGetProperty("updated_at", out var u)) r.UpdatedAt = u.GetInt64();
                    if (item.TryGetProperty("my_reply", out var mr)) r.MyReply = mr.GetBoolean();
                    if (item.TryGetProperty("author", out var author)) r.Author = author.GetString() ?? "BanModUser";
                    if (r.Id > 0) result.Add(r);
                }
            }
            return result;
        }

        private static int HighestPostId(List<CommunityPost> posts)
        {
            int max = 0; for (int i = 0; i < posts.Count; i++) max = Math.Max(max, posts[i].Id); return max;
        }

        private static string ErrorText(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return "Server error.";
            try { using (var doc = JsonDocument.Parse(body)) { var r = doc.RootElement; if (r.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String) return e.GetString() ?? "Server error."; if (r.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) return m.GetString() ?? "Server error."; } } catch { }
            return Short(body, 220);
        }

        private static string CommunityUrl(string path) { return CommunityApiBase.TrimEnd('/') + "/api/community" + path; }
        private static void SetStatus(string text) { if (_statusText != null) _statusText.text = text ?? ""; }
        private static string Short(string text, int max) { text = (text ?? "").Replace("\r", " ").Replace("\n", " ").Trim(); return text.Length <= max ? text : text.Substring(0, Math.Max(1, max - 1)) + "…"; }
        private static string FormatTimestamp(long value) { try { return DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().ToString("yyyy-MM-dd HH:mm"); } catch { return ""; } }
        private static void SetInputText(TMP_InputField input, string value) { if (input == null) return; input.text = value ?? ""; try { input.MoveTextEnd(false); } catch { } }
        private static void SetButtonText(Button button, string value) { if (button == null) return; var t = button.GetComponentInChildren<TextMeshProUGUI>(true); if (t != null) { t.richText = false; t.text = value ?? ""; } }


        private static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            ApplyModernImage(img, color, true);
            img.raycastTarget = true;
            return go;
        }

        private static void ApplyModernImage(Image image, Color color, bool addOutline)
        {
            if (image == null) return;
            image.sprite = BanModUiStyles.RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            if (addOutline)
            {
                Outline outline = image.GetComponent<Outline>();
                if (outline == null) outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(BanModUiStyles.AccentColor.r, BanModUiStyles.AccentColor.g, BanModUiStyles.AccentColor.b, 0.20f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = true;
            }
        }


        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = text ?? ""; tmp.fontSize = fontSize; tmp.alignment = alignment; tmp.color = color; tmp.richText = false; tmp.enableWordWrapping = true; tmp.raycastTarget = false;
            try { if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset; } catch { }
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color color, Action action)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            ApplyModernImage(img, color, false);
            img.raycastTarget = true;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            if (action != null) btn.onClick.AddListener((UnityAction)delegate { action(); });
            TextMeshProUGUI label = CreateLabel(go.transform, "Text", text, 21, TextAlignmentOptions.Center, Color.white, Vector2.zero, size);
            label.enableAutoSizing = true;
            label.fontSizeMin = 14f;
            label.fontSizeMax = 21f;
            RectTransform textRect = label.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            label.enableWordWrapping = false;
            return btn;
        }


        private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, bool multiline, Vector2 pos, Vector2 size, int limit)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image bg = go.AddComponent<Image>();
            ApplyModernImage(bg, new Color(0.10f, 0.11f, 0.14f, 0.98f), true);
            bg.raycastTarget = true;
            TMP_InputField input = go.AddComponent<TMP_InputField>();
            input.targetGraphic = bg;
            input.characterLimit = limit;
            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Standard;
            input.inputType = TMP_InputField.InputType.Standard;
            input.richText = false;
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(go.transform, false);
            RectTransform vrt = viewport.AddComponent<RectTransform>();
            StretchFull(vrt, new Vector4(10f, 6f, 10f, 6f));
            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
            vpImage.raycastTarget = false;
            viewport.AddComponent<RectMask2D>();
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(viewport.transform, false);
            RectTransform trt = textGo.AddComponent<RectTransform>();
            StretchFull(trt);
            TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
            text.fontSize = multiline ? 16 : 18;
            text.color = Color.white;
            text.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            text.enableWordWrapping = multiline;
            text.overflowMode = multiline ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            text.richText = false;
            text.raycastTarget = false;
            GameObject phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(viewport.transform, false);
            RectTransform prt = phGo.AddComponent<RectTransform>();
            StretchFull(prt);
            TextMeshProUGUI ph = phGo.AddComponent<TextMeshProUGUI>();
            ph.text = placeholder;
            ph.fontSize = multiline ? 16 : 18;
            ph.color = new Color(1f, 1f, 1f, 0.45f);
            ph.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            ph.enableWordWrapping = multiline;
            ph.overflowMode = multiline ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            ph.richText = false;
            ph.raycastTarget = false;
            try { if (TMP_Settings.defaultFontAsset != null) { text.font = TMP_Settings.defaultFontAsset; ph.font = TMP_Settings.defaultFontAsset; } } catch { }
            input.textViewport = vrt;
            input.textComponent = text;
            input.placeholder = ph;
            return input;
        }


        private static TextMeshProUGUI CreateScrollableText(Transform parent, string name, Vector2 pos, Vector2 size, out ScrollRect scroll, out RectTransform content)
        {
            var root = new GameObject(name); root.transform.SetParent(parent, false); var rrt = root.AddComponent<RectTransform>(); rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0.5f); rrt.pivot = new Vector2(0.5f, 0.5f); rrt.anchoredPosition = pos; rrt.sizeDelta = size;
            var bg = root.AddComponent<Image>(); ApplyModernImage(bg, new Color(0.09f, 0.10f, 0.13f, 0.96f), true);
            scroll = root.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30f;
            var vp = new GameObject("Viewport"); vp.transform.SetParent(root.transform, false); var vrt = vp.AddComponent<RectTransform>(); StretchFull(vrt, new Vector4(10f, 10f, 10f, 10f)); vp.AddComponent<Image>().color = new Color(0, 0, 0, 0.001f); vp.AddComponent<RectMask2D>();
            var cgo = new GameObject("Content"); cgo.transform.SetParent(vp.transform, false); content = cgo.AddComponent<RectTransform>(); content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f); content.pivot = new Vector2(0.5f, 1f); content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0f, 400f);
            var tgo = new GameObject("Text"); tgo.transform.SetParent(cgo.transform, false); var tr = tgo.AddComponent<RectTransform>(); tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f); tr.anchoredPosition = Vector2.zero; tr.sizeDelta = new Vector2(-12f, 400f);
            var text = tgo.AddComponent<TextMeshProUGUI>(); text.fontSize = 17; text.color = new Color(0.94f, 0.95f, 1f, 1f); text.alignment = TextAlignmentOptions.TopLeft; text.enableWordWrapping = true; text.richText = false; text.overflowMode = TextOverflowModes.Overflow; text.raycastTarget = false; try { if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset; } catch { }
            scroll.viewport = vrt; scroll.content = content;
            return text;
        }

        private static void StretchFull(RectTransform rt) { StretchFull(rt, Vector4.zero); }
        private static void StretchFull(RectTransform rt, Vector4 pad)
        {
            if (rt == null) return; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f); rt.offsetMin = new Vector2(pad.x, pad.y); rt.offsetMax = new Vector2(-pad.z, -pad.w);
        }

    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    internal static class BanModCommunityMainMenuStartPatch
    {
        private static void Postfix(MainMenuManager __instance)
        {
            BanModCommunityBoard.InstallMainMenuButton(__instance);
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    internal static class BanModCommunityRuntimePatch
    {
        private static void Postfix()
        {
            BanModCommunityBoard.RuntimeTick();
        }
    }



    [HarmonyPatch(typeof(BanModCommunicationUi), "get_IsUiOpen")]
    internal static class BanModCommunityCommunicationIsOpenPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!BanModCommunityBoard.IsOpen && !BanModSymbolPicker.IsOpen) return true;
            __result = true;
            return false;
        }
    }



    [HarmonyPatch(typeof(BanModCommunicationUi), nameof(BanModCommunicationUi.OpenMenu))]
    internal static class BanModCommunityCommunicationOpenPatch
    {
        private static bool Prefix()
        {
            BanModCommunityBoard.ToggleBugReportFromF3();
            return false;
        }
    }



    [HarmonyPatch(typeof(BanModCommunicationUi), nameof(BanModCommunicationUi.SetUnreadCount))]
    internal static class BanModCommunityCommunicationUnreadPatch
    {
        private static bool Prefix(int count)
        {
            BanModCommunityBoard.SetBugReportUnreadCount(count);
            return false;
        }
    }



    [HarmonyPatch(typeof(BanModCommunicationUi), nameof(BanModCommunicationUi.ShowReportChatPopup))]
    internal static class BanModCommunityCommunicationReportPopupPatch
    {
        private static bool Prefix(BanModCommunicationManager.ReportSummary report, Action onClose)
        {
            BanModCommunityBoard.OpenBugReportNotification(report, onClose);
            return false;
        }
    }
}
