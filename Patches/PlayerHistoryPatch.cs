//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using AmongUs.Data;
//using HarmonyLib;
//using InnerNet;
//using TMPro;
//using UnityEngine;

//namespace BanMod;

//internal static class PlayerHistory
//{
//    internal sealed class HistoryEntry
//    {
//        internal FriendsListManager.RecentPlayedWithPlayer Recent { get; }
//        internal Platforms Platform { get; }
//        internal string PlatformName { get; }
//        internal string ProductUserId { get; }
//        internal bool IsProtected { get; }

//        internal HistoryEntry(
//            FriendsListManager.RecentPlayedWithPlayer recent,
//            Platforms platform,
//            string platformName,
//            string productUserId,
//            bool isProtected)
//        {
//            Recent = recent;
//            Platform = platform;
//            PlatformName = platformName;
//            ProductUserId = productUserId;
//            IsProtected = isProtected;
//        }
//    }

//    internal static readonly Dictionary<string, HistoryEntry> LeftLobby = new();
//    internal static readonly Dictionary<string, HistoryEntry> LeftDuringRound = new();
//    internal static readonly Dictionary<string, HistoryEntry> PreviousMatch = new();

//    internal static int ScopedGameId = int.MinValue;
//    internal static bool RoundRunning;

//    internal static void ScopeToCurrentLobby()
//    {
//        int gameId = AmongUsClient.Instance != null ? AmongUsClient.Instance.GameId : 0;
//        if (gameId == ScopedGameId)
//            return;

//        ScopedGameId = gameId;
//        LeftLobby.Clear();
//        LeftDuringRound.Clear();
//        RoundRunning = false;
//    }

//    internal static void StartRound()
//    {
//        ScopeToCurrentLobby();
//        LeftLobby.Clear();
//        LeftDuringRound.Clear();
//        RoundRunning = true;
//    }

//    internal static void RememberLeaver(ClientData client)
//    {
//        NetworkedPlayerInfo player = client?.Character?.Data;
//        if (player == null || IsLocalPlayer(player))
//            return;

//        ScopeToCurrentLobby();

//        HistoryEntry entry = MakeHistoryEntry(player, client);
//        Dictionary<string, HistoryEntry> target =
//            RoundRunning ? LeftDuringRound : LeftLobby;

//        target[GetKey(entry.Recent)] = entry;
//    }

//    internal static void CapturePreviousMatch()
//    {
//        PreviousMatch.Clear();

//        foreach (HistoryEntry entry in LeftDuringRound.Values)
//            PreviousMatch[GetKey(entry.Recent)] = entry;

//        if (GameData.Instance != null)
//        {
//            for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
//            {
//                NetworkedPlayerInfo player = GameData.Instance.AllPlayers[i];
//                if (player == null || IsLocalPlayer(player))
//                    continue;

//                HistoryEntry entry = MakeHistoryEntry(player);
//                string key = GetKey(entry.Recent);

//                if (!PreviousMatch.TryGetValue(key, out HistoryEntry existing) ||
//                    existing.Platform == Platforms.Unknown)
//                {
//                    PreviousMatch[key] = entry;
//                }
//            }
//        }

//        LeftDuringRound.Clear();
//        LeftLobby.Clear();
//        RoundRunning = false;
//    }

//    private static HistoryEntry MakeHistoryEntry(
//        NetworkedPlayerInfo player,
//        ClientData client = null)
//    {
//        FriendsListManager.RecentPlayedWithPlayer recent = MakeRecentPlayer(player);

//        if (client == null && AmongUsClient.Instance != null)
//        {
//            client = AmongUsClient.Instance.GetClient(player.ClientId) ??
//                     AmongUsClient.Instance.GetRecentClient(player.ClientId);
//        }

//        Platforms platform =
//            client?.PlatformData?.Platform ?? Platforms.Unknown;

//        string platformName = GetPlatformDisplayName(
//            platform,
//            client?.PlatformData?.PlatformName);

//        string productUserId = client?.ProductUserId;
//        if (string.IsNullOrWhiteSpace(productUserId))
//            productUserId = recent.Puid ?? string.Empty;

//        bool isProtected = false;
//        if (client != null)
//        {
//            try
//            {
//                isProtected = BanMod.IsProtected(client);
//            }
//            catch
//            {
//                isProtected = false;
//            }
//        }

//        return new HistoryEntry(
//            recent,
//            platform,
//            platformName,
//            productUserId,
//            isProtected);
//    }

//    private static string GetPlatformDisplayName(
//        Platforms platform,
//        string originalName)
//    {
//        if (!string.IsNullOrWhiteSpace(originalName))
//            return originalName;

//        return platform switch
//        {
//            Platforms.StandaloneSteamPC => "PC (Steam)",
//            Platforms.StandaloneEpicPC => "PC (Epic Games)",
//            Platforms.StandaloneWin10 => "PC (Microsoft Store)",
//            Platforms.StandaloneItch => "PC (Itch.io)",
//            Platforms.StandaloneMac => "Mac",
//            Platforms.Android => "Android",
//            Platforms.IPhone => "iPhone / iPad",
//            Platforms.Switch => "Nintendo Switch",
//            Platforms.Xbox => "Xbox",
//            Platforms.Playstation => "PlayStation",
//            _ => string.Empty
//        };
//    }

//    internal static FriendsListManager.RecentPlayedWithPlayer MakeRecentPlayer(
//        NetworkedPlayerInfo player)
//    {
//        return new FriendsListManager.RecentPlayedWithPlayer(
//            player,
//            AmongUsClient.Instance.GameId,
//            GetServerAddress());
//    }

//    internal static string GetKey(
//        FriendsListManager.RecentPlayedWithPlayer player)
//    {
//        if (!string.IsNullOrWhiteSpace(player.Puid))
//            return "puid:" + player.Puid;
//        if (!string.IsNullOrWhiteSpace(player.FriendCode))
//            return "friend:" + player.FriendCode;
//        return "name:" + player.PlayerName;
//    }

//    private static bool IsLocalPlayer(NetworkedPlayerInfo player)
//    {
//        return PlayerControl.LocalPlayer != null &&
//               player.PlayerId == PlayerControl.LocalPlayer.PlayerId;
//    }

//    private static string GetServerAddress()
//    {
//        try
//        {
//            string host = AmongUsClient.Instance.GetNetworkAddress();

//            IPAddress ip = IPAddress.Parse(host);
//            byte[] bytes = ip.GetAddressBytes();

//            if (bytes.Length != 4)
//                return string.Empty;

//            uint address = BitConverter.ToUInt32(bytes, 0);

//            return $"{address}:{AmongUsClient.Instance.GetNetworkPort()}";
//        }
//        catch
//        {
//            return string.Empty;
//        }
//    }
//}

//[HarmonyPatch(typeof(ShipStatus), "Start")]
//internal static class ShipStatusStartPatch
//{
//    private static void Prefix()
//    {
//        PlayerHistory.StartRound();
//    }
//}

//[HarmonyPatch(typeof(EndGameManager), "Start")]
//internal static class EndGameManagerStartPatch
//{
//    private static void Prefix()
//    {
//        PlayerHistory.CapturePreviousMatch();
//    }
//}

//[HarmonyPatch(typeof(AmongUsClient), "OnPlayerLeft")]
//internal static class AmongUsClientOnPlayerLeftPatch
//{
//    private static void Prefix(ClientData data)
//    {
//        PlayerHistory.RememberLeaver(data);

//    }
//}

//[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.RefreshRecentlyPlayed))]
//internal static class RefreshRecentlyPlayedPatch
//{
//    private const string Marker = "RecentPlayersSections.";
//    private const string LeftTitle = "PLAYERS WHO LEFT";
//    private const string PreviousTitle = "PREVIOUS MATCH";

//    internal static readonly List<LobbyPlayerBar> InjectedBars = new();
//    private static Sprite BanButtonSprite;

//    private static void Postfix(FriendsListUI __instance)
//    {
//        PlayerHistory.ScopeToCurrentLobby();
//        RemoveOldInjectedObjects(__instance);

//        int row = CountOriginalRecentPlayers();
//        row = AddSection(__instance, LeftTitle, PlayerHistory.LeftLobby.Values, row);
//        row = AddSection(__instance, PreviousTitle, PlayerHistory.PreviousMatch.Values, row);

//        __instance.RecentlyPlayedScroller.SetYBoundsMax(
//            -(__instance.YStart - row * __instance.YOffset));
//    }

//    private static int AddSection(
//        FriendsListUI ui,
//        string title,
//        IEnumerable<PlayerHistory.HistoryEntry> players,
//        int row)
//    {
//        List<PlayerHistory.HistoryEntry> entries = players
//            .Where(p => p != null)
//            .OrderBy(p => p.Recent.PlayerName, StringComparer.OrdinalIgnoreCase)
//            .ToList();

//        CreateTitle(ui, title, row);
//        row++;

//        foreach (PlayerHistory.HistoryEntry entry in entries)
//        {
//            FriendsListManager.RecentPlayedWithPlayer recent = entry.Recent;
//            float y = ui.YStart - row * ui.YOffset;

//            LobbyPlayerBar bar = UnityEngine.Object.Instantiate(
//                ui.LobbyPlayerBar,
//                ui.RecentlyPlayedArea.transform);

//            bar.gameObject.name = Marker + title + "." + row;
//            bar.SetRecentPlayer(recent);
//            bar.SetUp(recent.Puid, ui, recent.FriendCode, recent.PlayerName);
//            bar.transform.localPosition = new Vector3(-0.29f, y, -1f);

//            try
//            {
//                CreateBanButton(ui, bar, entry);
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(
//                    $"[PlayerHistory] CreateBanButton failed for " +
//                    $"{recent.PlayerName}: {ex}");
//            }

//            Action<PassiveButton> value = button =>
//            {
//                button.ClickMask = ui.RecentlyPlayedScroller.Hitbox;
//            };
//            bar.Buttons.ForEach(value);

//            if (entry.Platform != Platforms.Unknown)
//                bar.SetPlatform(entry.Platform, entry.PlatformName);
//            else
//                bar.GetAndSetPlatform();

//            InjectedBars.Add(bar);
//            foreach (PassiveButton selectable in bar.ControllerSelectable)
//                ControllerManager.Instance.AddSelectableUiElement(selectable, false);
//            row++;
//        }

//        return row;
//    }

//    private static void CreateBanButton(
//        FriendsListUI ui,
//        LobbyPlayerBar bar,
//        PlayerHistory.HistoryEntry entry)
//    {
//        List<SpriteRenderer> actionSprites = GetActionSprites(bar);
//        GetActionArea(
//            bar,
//            actionSprites,
//            out float centerX,
//            out float centerY,
//            out float totalWidth);

//        ShrinkAndRaiseActionButtons(actionSprites);

//        SpriteRenderer referenceRenderer = bar.ReportButton;
//        if (referenceRenderer == null && actionSprites.Count > 0)
//            referenceRenderer = actionSprites[0];

//        PassiveButton sourceButton = referenceRenderer != null
//            ? referenceRenderer.GetComponentInParent<PassiveButton>()
//            : null;

//        GameObject banObject;
//        PassiveButton button;

//        if (sourceButton != null)
//        {
//            banObject = UnityEngine.Object.Instantiate(
//                sourceButton.gameObject,
//                ui.RecentlyPlayedArea.transform);
//            button = banObject.GetComponent<PassiveButton>();

//            foreach (SpriteRenderer oldRenderer in
//                     banObject.GetComponentsInChildren<SpriteRenderer>(true))
//            {
//                oldRenderer.enabled = false;
//            }

//            foreach (Collider2D oldCollider in
//                     banObject.GetComponentsInChildren<Collider2D>(true))
//            {
//                oldCollider.enabled = false;
//            }
//        }
//        else
//        {
//            banObject = new GameObject();
//            banObject.transform.SetParent(
//                ui.RecentlyPlayedArea.transform,
//                false);
//            button = banObject.AddComponent<PassiveButton>();
//        }

//        Vector3 targetWorld = bar.transform.TransformPoint(
//            new Vector3(centerX, centerY - 0.22f, 0f));
//        Vector3 targetLocal = ui.RecentlyPlayedArea.transform
//            .InverseTransformPoint(targetWorld);
//        targetLocal.z = -3f;

//        banObject.transform.localPosition = targetLocal;
//        banObject.transform.localRotation = Quaternion.identity;
//        banObject.transform.localScale = Vector3.one;
//        banObject.name = Marker + "BanButton." + entry.Recent.PlayerName;
//        banObject.SetActive(true);

//        SpriteRenderer background = banObject.GetComponent<SpriteRenderer>();
//        if (background == null)
//            background = banObject.AddComponent<SpriteRenderer>();

//        background.enabled = true;
//        background.sprite = GetBanButtonSprite();
//        background.drawMode = SpriteDrawMode.Sliced;
//        background.size = new Vector2(totalWidth, 0.25f);
//        background.color = new Color(0.58f, 0.10f, 0.10f, 1f);

//        if (referenceRenderer != null)
//        {
//            background.sortingLayerID = referenceRenderer.sortingLayerID;
//            background.sortingOrder = referenceRenderer.sortingOrder + 1;
//            background.maskInteraction = referenceRenderer.maskInteraction;
//        }

//        BoxCollider2D collider = banObject.AddComponent<BoxCollider2D>();
//        collider.size = new Vector2(totalWidth, 0.25f);

//        TextMeshPro label = UnityEngine.Object.Instantiate(
//            ui.ViewRequestsText,
//            banObject.transform);

//        label.gameObject.name = Marker + "BanLabel";
//        label.gameObject.SetActive(true);
//        label.alignment = TextAlignmentOptions.Center;
//        label.enableAutoSizing = false;
//        label.fontStyle |= FontStyles.Bold;
//        label.fontSize = 1.05f;

//        RectTransform labelRect = label.rectTransform;
//        labelRect.pivot = new Vector2(0.5f, 0.5f);
//        labelRect.sizeDelta = new Vector2(totalWidth, 0.3f);
//        labelRect.localScale = Vector3.one;
//        labelRect.localPosition = new Vector3(0f, 0f, -0.1f);

//        Renderer labelRenderer = label.GetComponent<Renderer>();
//        if (labelRenderer != null)
//        {
//            labelRenderer.sortingLayerID = background.sortingLayerID;
//            labelRenderer.sortingOrder = background.sortingOrder + 1;
//        }

//        bool alreadyBanned = BanManager.IsHistoryPlayerBanned(
//            entry.Recent.FriendCode,
//            entry.ProductUserId);

//        bool canBan = !entry.IsProtected &&
//                      !alreadyBanned;

//        if (entry.IsProtected)
//            label.text = "PROTECTED PLAYER";
//        else if (alreadyBanned)
//            label.text = "ALREADY IN BANLIST";
//        else
//            label.text = "ADD TO BANLIST";

//        label.color = Color.black;

//        SetBanButtonColor(background, canBan, alreadyBanned);

//        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
//        button.OnClick.RemoveAllListeners();
//        button.OnClick.AddListener((Action)(() =>
//        {
//            bool added = BanManager.AddBanPlayerFromHistory(
//                entry.Recent.FriendCode,
//                entry.ProductUserId,
//                entry.Recent.PlayerName,
//                "RecentPlayersButton");

//            if (!added)
//                return;

//            label.text = "ADDED TO BANLIST";
//            label.color = new Color(0.05f, 0.42f, 0.05f, 1f);
//            SetBanButtonColor(background, false, true);
//            button.enabled = false;
//            collider.enabled = false;
//        }));

//        button.OnMouseOver = new UnityEngine.Events.UnityEvent();
//        button.OnMouseOver.RemoveAllListeners();
//        button.OnMouseOver.AddListener((Action)(() =>
//        {
//            if (canBan)
//                background.color = new Color(0.78f, 0.16f, 0.16f, 1f);
//        }));

//        button.OnMouseOut = new UnityEngine.Events.UnityEvent();
//        button.OnMouseOut.RemoveAllListeners();
//        button.OnMouseOut.AddListener((Action)(() =>
//        {
//            SetBanButtonColor(background, canBan, alreadyBanned);
//        }));

//        button.enabled = canBan;
//        button.ClickMask = ui.RecentlyPlayedScroller.Hitbox;
//        collider.enabled = canBan;

//        bar.Buttons.Add(button);
//        bar.ControllerSelectable.Add(button);
//    }

//    private static List<SpriteRenderer> GetActionSprites(LobbyPlayerBar bar)
//    {
//        List<SpriteRenderer> result = new();

//        AddUniqueSprite(result, bar.AddFriendButton);
//        AddUniqueSprite(result, bar.BlockButton);
//        AddUniqueSprite(result, bar.ReportButton);

//        return result;
//    }

//    private static void AddUniqueSprite(
//        List<SpriteRenderer> sprites,
//        SpriteRenderer sprite)
//    {
//        if (sprite == null || sprites.Contains(sprite))
//            return;

//        sprites.Add(sprite);
//    }

//    private static void GetActionArea(
//        LobbyPlayerBar bar,
//        List<SpriteRenderer> sprites,
//        out float centerX,
//        out float centerY,
//        out float totalWidth)
//    {
//        if (sprites.Count == 0)
//        {
//            centerX = 1.35f;
//            centerY = 0f;
//            totalWidth = 1.65f;
//            return;
//        }

//        float minX = float.MaxValue;
//        float maxX = float.MinValue;
//        float ySum = 0f;

//        foreach (SpriteRenderer sprite in sprites)
//        {
//            Vector3 local = bar.transform.InverseTransformPoint(
//                sprite.transform.position);

//            minX = Mathf.Min(minX, local.x);
//            maxX = Mathf.Max(maxX, local.x);
//            ySum += local.y;
//        }

//        centerX = (minX + maxX) * 0.5f;
//        centerY = ySum / sprites.Count;
//        totalWidth = Mathf.Clamp(maxX - minX + 0.5f, 1.5f, 2.4f);
//    }

//    private static void ShrinkAndRaiseActionButtons(
//        List<SpriteRenderer> sprites)
//    {
//        List<Transform> adjusted = new();

//        foreach (SpriteRenderer sprite in sprites)
//        {
//            PassiveButton parentButton =
//                sprite.GetComponentInParent<PassiveButton>();

//            Transform target = parentButton != null
//                ? parentButton.transform
//                : sprite.transform;

//            if (adjusted.Contains(target))
//                continue;

//            adjusted.Add(target);

//            Vector3 position = target.localPosition;
//            position.y += 0.12f;
//            target.localPosition = position;

//            Vector3 scale = target.localScale;
//            scale.x *= 0.72f;
//            scale.y *= 0.72f;
//            target.localScale = scale;
//        }
//    }

//    private static Sprite GetBanButtonSprite()
//    {
//        if (BanButtonSprite != null)
//            return BanButtonSprite;

//        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
//        texture.name = Marker + "BanButtonTexture";
//        texture.SetPixel(0, 0, Color.white);
//        texture.Apply();
//        texture.wrapMode = TextureWrapMode.Clamp;
//        texture.filterMode = FilterMode.Point;

//        BanButtonSprite = Sprite.Create(
//            texture,
//            new Rect(0f, 0f, 1f, 1f),
//            new Vector2(0.5f, 0.5f),
//            1f);
//        BanButtonSprite.name = Marker + "BanButtonSprite";

//        return BanButtonSprite;
//    }

//    private static void SetBanButtonColor(
//        SpriteRenderer background,
//        bool canBan,
//        bool alreadyBanned)
//    {
//        if (alreadyBanned)
//            background.color = new Color(0.18f, 0.42f, 0.18f, 1f);
//        else if (canBan)
//            background.color = new Color(0.58f, 0.10f, 0.10f, 1f);
//        else
//            background.color = new Color(0.28f, 0.28f, 0.28f, 1f);
//    }

//    private static void CreateTitle(FriendsListUI ui, string title, int row)
//    {
//        TextMeshPro label = UnityEngine.Object.Instantiate(
//            ui.ViewRequestsText,
//            ui.RecentlyPlayedArea.transform);

//        label.gameObject.name = Marker + "Title." + title;
//        label.gameObject.SetActive(true);
//        label.text = title;
//        label.alignment = TextAlignmentOptions.MidlineLeft;
//        label.enableAutoSizing = false;
//        label.fontStyle |= FontStyles.Bold;
//        label.fontSize = 2.35f;
//        label.color = Color.black;

//        RectTransform rect = label.rectTransform;
//        rect.pivot = new Vector2(0f, 0.5f);
//        rect.sizeDelta = new Vector2(4.65f, 0.55f);
//        rect.localPosition = new Vector3(
//            -0.29f,
//            ui.YStart - row * ui.YOffset,
//            -1f);
//    }

//    private static int CountOriginalRecentPlayers()
//    {
//        int count = 0;
//        var recent = DestroyableSingleton<FriendsListManager>.Instance.RecentlyPlayedWith;
//        string localName = DataManager.Player.Customization.Name;

//        for (int i = 0; i < recent.Count; i++)
//        {
//            if (recent[i].PlayerName != localName)
//                count++;
//        }

//        return count;
//    }

//    private static void RemoveOldInjectedObjects(FriendsListUI ui)
//    {
//        for (int i = InjectedBars.Count - 1; i >= 0; i--)
//        {
//            LobbyPlayerBar bar = InjectedBars[i];
//            if (bar != null)
//            {
//                foreach (PassiveButton selectable in bar.ControllerSelectable)
//                    ControllerManager.Instance.RemoveSelectableUiElement(selectable);
//            }
//        }
//        InjectedBars.Clear();

//        Transform parent = ui.RecentlyPlayedArea.transform;
//        for (int i = parent.childCount - 1; i >= 0; i--)
//        {
//            GameObject child = parent.GetChild(i).gameObject;
//            if (child.name.StartsWith(Marker, StringComparison.Ordinal))
//                UnityEngine.Object.DestroyImmediate(child);
//        }
//    }
//}

//[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.UpdatedReportedPlayers))]
//internal static class UpdatedReportedPlayersPatch
//{
//    private static void Postfix()
//    {
//        foreach (LobbyPlayerBar bar in RefreshRecentlyPlayedPatch.InjectedBars)
//        {
//            if (bar != null)
//                bar.CheckAlreadyReported();
//        }
//    }
//}
