//credits and licenses in the resources folder
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static BanMod.Translator;
using static BanMod.ChatCommands;

namespace BanMod;

[Flags]
public enum AllowedRole
{
    None = 0,
    Vip = 1,
    Moderator = 2
}

public sealed class AllowedEntry
{
    public string FriendCode { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public AllowedRole Roles { get; set; } = AllowedRole.None;
}

public static class AllowedManager
{
    private const string AllowedFolderPath = "./DATA/VIPs_MODERATORs";
    private const string AllowedFilePath = "./DATA/VIPs_MODERATORs/VipAndMod.txt";
    public const string ModCreatorFriendCode = "medialteam#6599";
    public const string ModDevFriendCode = "knifeshy#1794";
    public static void Initialize()
    {
        try
        {
            Directory.CreateDirectory(AllowedFolderPath);

            if (!File.Exists(AllowedFilePath))
                File.WriteAllText(AllowedFilePath, string.Empty);

        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager Initialize error: {ex}");
        }
    }

    public static bool IsModCreator(string friendCode)
    {
        return !string.IsNullOrWhiteSpace(friendCode) &&
               friendCode.Equals(ModCreatorFriendCode, StringComparison.OrdinalIgnoreCase);
    }
    public static bool IsModDev(string friendCode)
    {
        return !string.IsNullOrWhiteSpace(friendCode) &&
               friendCode.Equals(ModDevFriendCode, StringComparison.OrdinalIgnoreCase);
    }
    public static bool IsFriends(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        return IsVip(friendCode) || IsModCreator(friendCode);
    }

    public static bool IsVip(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        return HasRole(friendCode, AllowedRole.Vip);
    }

    public static bool IsModerator(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        if (IsModCreator(friendCode))
            return true;

        return HasRole(friendCode, AllowedRole.Moderator);
    }

    public static bool HasRole(string friendCode, AllowedRole role)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        var entry = GetEntry(friendCode);
        return entry != null && (entry.Roles & role) == role;
    }

    public static AllowedEntry GetEntry(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return null;

        if (IsModCreator(friendCode))
        {
            return new AllowedEntry
            {
                FriendCode = ModCreatorFriendCode,
                PlayerName = "ModCreator",
                Roles = AllowedRole.Vip | AllowedRole.Moderator
            };
        }

        try
        {
            InitializeWithoutMigrationLoop();

            foreach (string rawLine in File.ReadAllLines(AllowedFilePath))
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                var entry = ParseLine(rawLine);
                if (entry == null)
                    continue;

                if (entry.FriendCode.Equals(friendCode, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager GetEntry error: {ex}");
        }

        return null;
    }

    public static List<AllowedEntry> GetAllEntries()
    {
        InitializeWithoutMigrationLoop();

        var result = new List<AllowedEntry>();

        try
        {
            result.Add(new AllowedEntry
            {
                FriendCode = ModCreatorFriendCode,
                PlayerName = "ModCreator",
                Roles = AllowedRole.Vip | AllowedRole.Moderator
            });

            foreach (string rawLine in File.ReadAllLines(AllowedFilePath))
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                var entry = ParseLine(rawLine);
                if (entry == null)
                    continue;

                if (entry.FriendCode.Equals(ModCreatorFriendCode, StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(entry);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager GetAllEntries error: {ex}");
        }

        return result;
    }

    public static bool AddRole(string friendCode, string playerName, AllowedRole role)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        if (IsModCreator(friendCode))
            return true;

        InitializeWithoutMigrationLoop();

        try
        {
            var entries = GetAllEntries()
                .Where(x => !x.FriendCode.Equals(ModCreatorFriendCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var entry = entries.FirstOrDefault(x =>
                x.FriendCode.Equals(friendCode, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
            {
                entries.Add(new AllowedEntry
                {
                    FriendCode = friendCode,
                    PlayerName = playerName ?? "",
                    Roles = role
                });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(playerName))
                    entry.PlayerName = playerName;

                entry.Roles |= role;
            }

            SaveAll(entries);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager AddRole error: {ex}");
            return false;
        }
    }

    public static bool RemoveRole(string friendCode, AllowedRole role)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        if (IsModCreator(friendCode))
            return false;

        InitializeWithoutMigrationLoop();

        try
        {
            var entries = GetAllEntries()
                .Where(x => !x.FriendCode.Equals(ModCreatorFriendCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var entry = entries.FirstOrDefault(x =>
                x.FriendCode.Equals(friendCode, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
                return false;

            entry.Roles &= ~role;

            if (entry.Roles == AllowedRole.None)
                entries.Remove(entry);

            SaveAll(entries);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager RemoveRole error: {ex}");
            return false;
        }
    }

    public static bool ManageVip(string idStr, bool add)
    {
        if (!byte.TryParse(idStr, out var id))
            return true;

        var player = Utils.GetPlayerById(id);
        if (player == null)
            return true;

        string code = player.FriendCode;
        string name = player.Data.PlayerName;

        if (string.IsNullOrWhiteSpace(code))
            return true;

        if (add)
        {
            if (!IsVip(code))
            {
                AddRole(code, name, AllowedRole.Vip);
                ShowChat(name + GetString("AddedtoVipList"));
            }
            else
            {
                ShowChat(name + GetString("PlayerinVipList"));
            }
        }
        else
        {
            if (IsModCreator(code))
            {
                ShowChat(name + " is always Vip (ModCreator)");
            }
            else if (IsVip(code))
            {
                RemoveRole(code, AllowedRole.Vip);
                ShowChat(name + GetString("PlayerRemovedFromVipList"));
            }
            else
            {
                ShowChat(name + GetString("PlayerNotInVipList"));
            }
        }

        return true;
    }

    public static bool ManageModerator(string idStr, bool add)
    {
        if (!byte.TryParse(idStr, out var id))
            return true;

        var player = Utils.GetPlayerById(id);
        if (player == null)
            return true;

        string code = player.FriendCode;
        string name = player.Data.PlayerName;

        if (string.IsNullOrWhiteSpace(code))
            return true;

        if (add)
        {
            if (!IsModerator(code))
            {
                AddRole(code, name, AllowedRole.Moderator);
                ShowChat(name + GetString("AddedtoModeratorList"));
            }
            else
            {
                ShowChat(name + GetString("PlayerinModeratorList"));
            }
        }
        else
        {
            if (IsModCreator(code))
            {
                ShowChat(name + " is always Moderator (ModCreator)");
            }
            else if (IsModerator(code))
            {
                RemoveRole(code, AllowedRole.Moderator);
                ShowChat(name + GetString("PlayerRemovedFromModeratorList"));
            }
            else
            {
                ShowChat(name + GetString("PlayerNotInModeratorList"));
            }
        }

        return true;
    }

    private static void InitializeWithoutMigrationLoop()
    {
        try
        {
            Directory.CreateDirectory(AllowedFolderPath);

            if (!File.Exists(AllowedFilePath))
                File.WriteAllText(AllowedFilePath, string.Empty);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager InitializeWithoutMigrationLoop error: {ex}");
        }
    }

    

    private static AllowedEntry ParseLine(string line)
    {
        try
        {
            var parts = line.Split(',', StringSplitOptions.None);

            if (parts.Length == 0)
                return null;

            string friendCode = parts.Length > 0 ? parts[0].Trim() : "";
            string playerName = parts.Length > 1 ? parts[1].Trim() : "";
            string rolesRaw = parts.Length > 2 ? parts[2].Trim() : "";

            if (string.IsNullOrWhiteSpace(friendCode))
                return null;

            return new AllowedEntry
            {
                FriendCode = friendCode,
                PlayerName = playerName,
                Roles = ParseRoles(rolesRaw)
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager ParseLine error: {ex}");
            return null;
        }
    }

    private static AllowedRole ParseRoles(string rolesRaw)
    {
        if (string.IsNullOrWhiteSpace(rolesRaw))
            return AllowedRole.None;

        AllowedRole roles = AllowedRole.None;

        foreach (string role in rolesRaw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (role.Equals("Vip", StringComparison.OrdinalIgnoreCase))
                roles |= AllowedRole.Vip;
            else if (role.Equals("Moderator", StringComparison.OrdinalIgnoreCase))
                roles |= AllowedRole.Moderator;
        }

        return roles;
    }

    private static string RolesToString(AllowedRole roles)
    {
        var values = new List<string>();

        if ((roles & AllowedRole.Vip) != 0)
            values.Add("Vip");

        if ((roles & AllowedRole.Moderator) != 0)
            values.Add("Moderator");

        return string.Join("|", values);
    }

    private static void SaveAll(List<AllowedEntry> entries)
    {
        InitializeWithoutMigrationLoop();

        try
        {
            var lines = entries
                .Where(x =>
                    x != null &&
                    !string.IsNullOrWhiteSpace(x.FriendCode) &&
                    x.Roles != AllowedRole.None &&
                    !x.FriendCode.Equals(ModCreatorFriendCode, StringComparison.OrdinalIgnoreCase))
                .GroupBy(x => x.FriendCode, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var merged = new AllowedEntry
                    {
                        FriendCode = g.Key,
                        PlayerName = g.Select(x => x.PlayerName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "",
                        Roles = AllowedRole.None
                    };

                    foreach (var e in g)
                        merged.Roles |= e.Roles;

                    return merged;
                })
                .Select(x => $"{x.FriendCode},{x.PlayerName},{RolesToString(x.Roles)}")
                .ToArray();

            File.WriteAllLines(AllowedFilePath, lines);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BanMod] AllowedManager SaveAll error: {ex}");
        }
    }
}