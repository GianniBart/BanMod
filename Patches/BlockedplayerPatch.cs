using AmongUs.Data;
using Assets.InnerNet;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanMod;

public static class BlockedPlayersBulkManager
{
    private const string NameKeyPrefix = "BanMod.BlockedLastName.";

    public enum ControlAction
    {
        SelectAll,
        UnblockSelected
    }

    private sealed class PlayerRowInfo
    {
        public string Puid;
        public string FriendCode;
    }

    public static readonly HashSet<string> SelectedPuids =
        new HashSet<string>(StringComparer.Ordinal);

    private static readonly Dictionary<BlockedPlayerBar, ControlAction> ControlBars =
        new Dictionary<BlockedPlayerBar, ControlAction>();

    private static readonly Dictionary<BlockedPlayerBar, PlayerRowInfo> PlayerBars =
        new Dictionary<BlockedPlayerBar, PlayerRowInfo>();

    private static readonly List<BlockedPlayerBar> SpawnedBars =
        new List<BlockedPlayerBar>();

    public static bool BulkRunning { get; private set; }

    private static bool CancelRequested;
    private static int BulkProcessed;
    private static int BulkTotal;


    public static void RememberName(
        string puid,
        string playerName)
    {
        if (string.IsNullOrWhiteSpace(puid))
            return;

        if (string.IsNullOrWhiteSpace(playerName))
            return;

        PlayerPrefs.SetString(
            NameKeyPrefix + puid,
            playerName.Trim()
        );
    }

    public static string GetLastKnownName(
        string puid)
    {
        if (string.IsNullOrWhiteSpace(puid))
            return null;

        string key =
            NameKeyPrefix + puid;

        if (PlayerPrefs.HasKey(key))
        {
            string saved =
                PlayerPrefs.GetString(key);

            if (!string.IsNullOrWhiteSpace(saved))
                return saved;
        }

        try
        {
            string vanillaName =
                DataManager.Player.Friends.GetCachedName(puid);

            if (!string.IsNullOrWhiteSpace(vanillaName))
            {
                RememberName(
                    puid,
                    vanillaName
                );

                return vanillaName;
            }
        }
        catch
        {
        }

        return null;
    }


    public static string GetDisplayText(
        string puid,
        string friendCode)
    {
        bool selected =
            !string.IsNullOrWhiteSpace(puid) &&
            SelectedPuids.Contains(puid);

        string check =
            selected
                ? "[X]"
                : "[ ]";

        string code =
            string.IsNullOrWhiteSpace(friendCode)
                ? "(unknown FriendCode)"
                : friendCode;

        string name =
            GetLastKnownName(puid);

        if (string.IsNullOrWhiteSpace(name))
            name = "unknown name";

        return check +
               " " +
               code +
               "   |   <color=#000000>" +
               name +
               "</color>";
    }

    private static bool AreAllBlockedSelected()
    {
        FriendsListManager manager =
            DestroyableSingleton<FriendsListManager>.Instance;

        if (manager == null ||
            manager.BlockedPlayers == null ||
            manager.BlockedPlayers.Count == 0)
        {
            return false;
        }

        bool found = false;

        for (int i = 0;
             i < manager.BlockedPlayers.Count;
             i++)
        {
            ResponseBlockedPlayer player =
                manager.BlockedPlayers[i];

            if (player == null)
                continue;

            if (string.IsNullOrWhiteSpace(player.FriendPuid))
                continue;

            found = true;

            if (!SelectedPuids.Contains(player.FriendPuid))
                return false;
        }

        return found;
    }

    private static void UpdateControlText(
        BlockedPlayerBar bar,
        ControlAction action)
    {
        if (bar == null ||
            bar.SenderName == null)
        {
            return;
        }

        bar.SenderName.richText = true;

        switch (action)
        {
            case ControlAction.SelectAll:
                {
                    bar.SenderName.text =
                        AreAllBlockedSelected()
                            ? "[X] DESELECT ALL"
                            : "[ ] SELECT ALL";

                    break;
                }

            case ControlAction.UnblockSelected:
                {
                    if (BulkRunning)
                    {
                        if (CancelRequested)
                        {
                            bar.SenderName.text =
                                "STOPPING...";
                        }
                        else
                        {
                            bar.SenderName.text =
                                "STOP UNBLOCKING (" +
                                BulkProcessed +
                                "/" +
                                BulkTotal +
                                ")";
                        }
                    }
                    else
                    {
                        bar.SenderName.text =
                            "UNBLOCK SELECTED (" +
                            SelectedPuids.Count +
                            ")";
                    }

                    break;
                }
        }
    }

    public static void RefreshVisibleLabels()
    {
        foreach (
            KeyValuePair<BlockedPlayerBar, PlayerRowInfo> entry
            in PlayerBars)
        {
            BlockedPlayerBar bar =
                entry.Key;

            PlayerRowInfo info =
                entry.Value;

            if (bar == null ||
                info == null ||
                bar.SenderName == null)
            {
                continue;
            }

            bar.SenderName.richText = true;

            bar.SenderName.text =
                GetDisplayText(
                    info.Puid,
                    info.FriendCode
                );
        }

        foreach (
            KeyValuePair<BlockedPlayerBar, ControlAction> entry
            in ControlBars)
        {
            if (entry.Key == null)
                continue;

            UpdateControlText(
                entry.Key,
                entry.Value
            );
        }
    }

    public static void TogglePlayer(
        string puid)
    {
        if (BulkRunning)
            return;

        if (string.IsNullOrWhiteSpace(puid))
            return;

        if (!SelectedPuids.Add(puid))
        {
            SelectedPuids.Remove(puid);
        }

        RefreshVisibleLabels();
    }

    public static void ToggleAll()
    {
        if (BulkRunning)
            return;

        FriendsListManager manager =
            DestroyableSingleton<FriendsListManager>.Instance;

        if (manager == null ||
            manager.BlockedPlayers == null)
        {
            return;
        }

        bool allSelected =
            AreAllBlockedSelected();

        SelectedPuids.Clear();

        if (!allSelected)
        {
            for (int i = 0;
                 i < manager.BlockedPlayers.Count;
                 i++)
            {
                ResponseBlockedPlayer player =
                    manager.BlockedPlayers[i];

                if (player == null)
                    continue;

                if (string.IsNullOrWhiteSpace(player.FriendPuid))
                    continue;

                SelectedPuids.Add(player.FriendPuid);
            }
        }

        RefreshVisibleLabels();
    }

    public static void HandleUnblockButton(
        FriendsListUI ui)
    {
        if (ui == null)
            return;

        if (BulkRunning)
        {
            CancelRequested = true;
            RefreshVisibleLabels();
            return;
        }

        if (SelectedPuids.Count == 0)
            return;

        CancelRequested = false;

        try
        {
            ui.StartCoroutine(
                UnblockSelectedCoroutine(ui)
            );
        }
        catch (Exception ex)
        {
            BulkRunning = false;
            CancelRequested = false;
            Debug.LogError("[BlockedPlayersBulk] Unable to start unblock coroutine: " + ex);
        }
    }

    private static IEnumerator UnblockSelectedCoroutine(
        FriendsListUI ui)
    {
        BulkRunning = true;
        CancelRequested = false;

        FriendsListManager manager =
            DestroyableSingleton<FriendsListManager>.Instance;

        if (manager == null)
        {
            BulkRunning = false;
            yield break;
        }

        List<string> targets =
            new List<string>();

        foreach (string puid in SelectedPuids)
        {
            if (!string.IsNullOrWhiteSpace(puid))
            {
                targets.Add(puid);
            }
        }

        BulkTotal = targets.Count;
        BulkProcessed = 0;

        RefreshVisibleLabels();

        float initialPause = 0.75f;

        while (initialPause > 0f)
        {
            if (CancelRequested || manager == null || ui == null)
                break;

            initialPause -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (manager == null || ui == null)
        {
            BulkRunning = false;
            CancelRequested = false;
            yield break;
        }

        for (int i = 0;
             i < targets.Count;
             i++)
        {
            if (CancelRequested || manager == null || ui == null)
                break;

            string puid =
                targets[i];

            bool completed = false;
            bool success = false;

            Action<ResponseState, Response<ResponseFriendsListRequest>> callback =
                (state, response) =>
                {
                    success =
                        state == ResponseState.Success;

                    completed = true;
                };

            try
            {
                manager.UnblockPlayer(
                    puid,
                    callback
                );
            }
            catch (Exception ex)
            {
                Debug.LogError("[BlockedPlayersBulk] UnblockPlayer failed: " + ex);
                completed = true;
                success = false;
            }

            float timeout = 10f;

            while (!completed &&
                   timeout > 0f)
            {
                if (manager == null || ui == null)
                    break;

                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (completed &&
                success)
            {
                RemoveBlockedPlayerFromLocalList(
                    manager,
                    puid
                );

                SelectedPuids.Remove(puid);
            }

            BulkProcessed++;

            RefreshVisibleLabels();

            float pause = 0.50f;

            while (pause > 0f)
            {
                if (CancelRequested || manager == null || ui == null)
                    break;

                pause -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        BulkRunning = false;
        CancelRequested = false;

        if (manager != null)
            CleanupSelection(manager);

        RefreshVisibleLabels();

        if (ui != null)
            RefreshFromServer(ui);
    }

    private static void RemoveBlockedPlayerFromLocalList(
        FriendsListManager manager,
        string puid)
    {
        if (manager == null ||
            manager.BlockedPlayers == null)
        {
            return;
        }

        for (int i =
                 manager.BlockedPlayers.Count - 1;
             i >= 0;
             i--)
        {
            ResponseBlockedPlayer player =
                manager.BlockedPlayers[i];

            if (player == null)
                continue;

            if (player.FriendPuid == puid)
            {
                manager.BlockedPlayers.RemoveAt(i);
            }
        }
    }

    private static void RefreshFromServer(
        FriendsListUI ui)
    {
        FriendsListManager manager =
            DestroyableSingleton<FriendsListManager>.Instance;

        if (manager == null ||
            ui == null)
        {
            return;
        }

        Action callback =
            () =>
            {
                if (manager == null || ui == null)
                    return;

                CleanupSelection(manager);
                ui.UpdateFriendBars();
            };

        try
        {
            ui.StartCoroutine(
                manager.RefreshFriendsList(
                    callback,
                    false
                )
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("[BlockedPlayersBulk] RefreshFriendsList failed: " + ex);
        }
    }

    private static void CleanupSelection(
        FriendsListManager manager)
    {
        HashSet<string> stillBlocked =
            new HashSet<string>(
                StringComparer.Ordinal
            );

        if (manager != null &&
            manager.BlockedPlayers != null)
        {
            for (int i = 0;
                 i < manager.BlockedPlayers.Count;
                 i++)
            {
                ResponseBlockedPlayer player =
                    manager.BlockedPlayers[i];

                if (player == null)
                    continue;

                if (string.IsNullOrWhiteSpace(player.FriendPuid))
                    continue;

                stillBlocked.Add(player.FriendPuid);
            }
        }

        List<string> remove =
            new List<string>();

        foreach (string puid in SelectedPuids)
        {
            if (!stillBlocked.Contains(puid))
            {
                remove.Add(puid);
            }
        }

        for (int i = 0;
             i < remove.Count;
             i++)
        {
            SelectedPuids.Remove(remove[i]);
        }
    }

    private static void DestroySpawnedBars()
    {
        for (int i = 0;
             i < SpawnedBars.Count;
             i++)
        {
            BlockedPlayerBar bar =
                SpawnedBars[i];

            if (bar == null)
                continue;

            if (bar.ControllerSelectable != null)
            {
                foreach (
                    PassiveButton button
                    in bar.ControllerSelectable)
                {
                    if (button == null)
                        continue;

                    try
                    {
                        ControllerManager.Instance
                            .RemoveSelectableUiElement(button);
                    }
                    catch
                    {
                    }
                }
            }

            try
            {
                UnityEngine.Object.DestroyImmediate(
                    bar.gameObject
                );
            }
            catch
            {
            }
        }

        SpawnedBars.Clear();
        PlayerBars.Clear();
        ControlBars.Clear();
    }

    public static bool ReplaceRefreshBlockedPlayers(
        FriendsListUI ui)
    {
        if (ui == null)
            return false;

        FriendsListManager manager =
            DestroyableSingleton<FriendsListManager>.Instance;

        if (manager == null ||
            manager.BlockedPlayers == null)
        {
            return false;
        }

        if (manager.RecentlyPlayedWith != null)
        {
            for (int i = 0;
                 i < manager.RecentlyPlayedWith.Count;
                 i++)
            {
                FriendsListManager.RecentPlayedWithPlayer recent =
                    manager.RecentlyPlayedWith[i];

                if (recent == null)
                    continue;

                RememberName(
                    recent.Puid,
                    recent.PlayerName
                );
            }
        }

        CleanupSelection(manager);
        RebuildListRows(ui);

        return true;
    }

    private static void RebuildListRows(
        FriendsListUI ui)
    {
        if (ui == null ||
            ui.BlockedPlayerBar == null ||
            ui.BlockedArea == null ||
            ui.BlockedScroller == null)
        {
            return;
        }

        FriendsListManager manager =
            DestroyableSingleton<FriendsListManager>.Instance;

        if (manager == null ||
            manager.BlockedPlayers == null)
        {
            return;
        }

        DestroySpawnedBars();

        int row = 0;

        CreateControlBar(
            ui,
            row++,
            ControlAction.SelectAll
        );

        CreateControlBar(
            ui,
            row++,
            ControlAction.UnblockSelected
        );

        List<ResponseBlockedPlayer> sorted =
            new List<ResponseBlockedPlayer>();

        for (int i = 0;
             i < manager.BlockedPlayers.Count;
             i++)
        {
            ResponseBlockedPlayer player =
                manager.BlockedPlayers[i];

            if (player != null)
            {
                sorted.Add(player);
            }
        }

        sorted.Sort(
            CompareBlockedPlayersByFriendCode
        );

        for (int i = 0;
             i < sorted.Count;
             i++)
        {
            ResponseBlockedPlayer player =
                sorted[i];

            float y =
                ui.YStart -
                row * ui.YOffset;

            BlockedPlayerBar bar =
                UnityEngine.Object.Instantiate(
                    ui.BlockedPlayerBar,
                    ui.BlockedArea.transform
                );

            if (bar == null)
                continue;

            string friendCode =
                player.FriendCode ??
                string.Empty;

            bar.SetUp(
                player.FriendPuid,
                ui,
                friendCode,
                null
            );

            PlayerRowInfo info =
                new PlayerRowInfo();

            info.Puid =
                player.FriendPuid;

            info.FriendCode =
                player.FriendCode;

            PlayerBars[bar] = info;
            SpawnedBars.Add(bar);

            if (bar.SenderName != null)
            {
                bar.SenderName.richText = true;

                bar.SenderName.text =
                    GetDisplayText(
                        player.FriendPuid,
                        player.FriendCode
                    );
            }

            bar.transform.localPosition =
                new Vector3(
                    -0.29f,
                    y,
                    -1f
                );

            ConfigureBarButtons(
                bar,
                ui
            );

            if (!string.IsNullOrWhiteSpace(player.FriendPuid))
            {
                bar.GetAndSetPlatform();
            }

            row++;
        }

        ui.BlockedScroller.SetYBoundsMax(
            -(
                ui.YStart -
                row * ui.YOffset
             )
        );

        RefreshVisibleLabels();
    }

    private static int CompareBlockedPlayersByFriendCode(
        ResponseBlockedPlayer a,
        ResponseBlockedPlayer b)
    {
        if (ReferenceEquals(a, b))
            return 0;

        if (a == null)
            return 1;

        if (b == null)
            return -1;

        bool aEmpty =
            string.IsNullOrWhiteSpace(a.FriendCode);

        bool bEmpty =
            string.IsNullOrWhiteSpace(b.FriendCode);

        if (aEmpty && !bEmpty)
            return 1;

        if (!aEmpty && bEmpty)
            return -1;

        return StringComparer
            .OrdinalIgnoreCase
            .Compare(
                a.FriendCode ?? string.Empty,
                b.FriendCode ?? string.Empty
            );
    }

    private static BlockedPlayerBar CreateControlBar(
        FriendsListUI ui,
        int row,
        ControlAction action)
    {
        if (ui == null ||
            ui.BlockedPlayerBar == null ||
            ui.BlockedArea == null)
        {
            return null;
        }

        float y =
            ui.YStart -
            row * ui.YOffset;

        BlockedPlayerBar bar =
            UnityEngine.Object.Instantiate(
                ui.BlockedPlayerBar,
                ui.BlockedArea.transform
            );

        if (bar == null)
            return null;

        ControlBars[bar] = action;
        SpawnedBars.Add(bar);

        bar.SetUp(
            string.Empty,
            ui,
            string.Empty,
            string.Empty
        );

        bar.transform.localPosition =
            new Vector3(
                -0.29f,
                y,
                -1f
            );

        ConfigureBarButtons(
            bar,
            ui
        );

        if (bar.PlatformIdentifier != null)
        {
            bar.PlatformIdentifier
                .gameObject
                .SetActive(false);
        }

        UpdateControlText(
            bar,
            action
        );

        return bar;
    }

    private static void ConfigureBarButtons(
        BlockedPlayerBar bar,
        FriendsListUI ui)
    {
        if (bar == null ||
            ui == null ||
            ui.BlockedScroller == null)
        {
            return;
        }

        if (bar.Buttons != null)
        {
            foreach (
                PassiveButton button
                in bar.Buttons)
            {
                if (button == null)
                    continue;

                button.ClickMask =
                    ui.BlockedScroller.Hitbox;
            }
        }

        if (bar.ControllerSelectable != null)
        {
            foreach (
                PassiveButton button
                in bar.ControllerSelectable)
            {
                if (button == null)
                    continue;

                try
                {
                    ControllerManager.Instance
                        .AddSelectableUiElement(
                            button,
                            false
                        );
                }
                catch
                {
                }
            }
        }
    }

    public static bool TryGetControlAction(
        BlockedPlayerBar bar,
        out ControlAction action)
    {
        return ControlBars.TryGetValue(
            bar,
            out action
        );
    }

    public static bool TryGetPlayerInfo(
        BlockedPlayerBar bar,
        out string puid,
        out string friendCode)
    {
        puid = null;
        friendCode = null;

        PlayerRowInfo info;

        if (!PlayerBars.TryGetValue(
                bar,
                out info))
        {
            return false;
        }

        if (info == null)
            return false;

        puid = info.Puid;
        friendCode = info.FriendCode;

        return true;
    }
}

[HarmonyPatch(
    typeof(FriendsListBar),
    nameof(FriendsListBar.SetUp))]
public static class FriendsListBarSetUpPatch
{
    public static void Prefix(
        string puid,
        string playerInGameName)
    {
        if (string.IsNullOrWhiteSpace(puid))
            return;

        if (string.IsNullOrWhiteSpace(playerInGameName))
            return;

        BlockedPlayersBulkManager.RememberName(
            puid,
            playerInGameName
        );
    }
}


[HarmonyPatch(
    typeof(FriendsListManager),
    nameof(FriendsListManager.AddRecentlyPlayed))]
public static class AddRecentlyPlayedBlockedNamePatch
{
    public static void Prefix(
        NetworkedPlayerInfo player)
    {
        if (player == null)
            return;

        BlockedPlayersBulkManager.RememberName(
            player.Puid,
            player.PlayerName
        );
    }
}


[HarmonyPatch(
    typeof(FriendsListUI),
    nameof(FriendsListUI.RefreshBlockedPlayers))]
public static class RefreshBlockedPlayersPatch
{
    public static bool Prefix(
        FriendsListUI __instance)
    {
        bool replaced =
            BlockedPlayersBulkManager
                .ReplaceRefreshBlockedPlayers(
                    __instance
                );

        return !replaced;
    }
}

[HarmonyPatch(
    typeof(BlockedPlayerBar),
    nameof(BlockedPlayerBar.CheckUnblockPlayer))]
public static class CheckUnblockPlayerBulkPatch
{
    public static bool Prefix(
        BlockedPlayerBar __instance)
    {
        if (__instance == null)
            return true;

        BlockedPlayersBulkManager.ControlAction action;

        if (BlockedPlayersBulkManager
            .TryGetControlAction(
                __instance,
                out action))
        {
            switch (action)
            {
                case BlockedPlayersBulkManager
                    .ControlAction.SelectAll:
                    {
                        BlockedPlayersBulkManager
                            .ToggleAll();

                        break;
                    }

                case BlockedPlayersBulkManager
                    .ControlAction.UnblockSelected:
                    {
                        BlockedPlayersBulkManager
                            .HandleUnblockButton(
                                FriendsListUI.Instance
                            );

                        break;
                    }
            }

            return false;
        }

        string puid;
        string friendCode;

        if (BlockedPlayersBulkManager
            .TryGetPlayerInfo(
                __instance,
                out puid,
                out friendCode))
        {
            BlockedPlayersBulkManager
                .TogglePlayer(puid);

            return false;
        }

        return true;
    }
}
