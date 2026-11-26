//credits and licenses in the resources folder/
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BanMod;

public static class FfaExternalBridge
{
    private static Assembly FfaAssembly;

    private static FieldInfo ActiveModeField;
    private static FieldInfo EnabledField;
    private static FieldInfo TeamsEnabledField;
    private static FieldInfo TeamCountField;
    private static FieldInfo HotPotatoEnabledField;
    private static FieldInfo HotPotatoExplosionSecondsField;
    private static FieldInfo HotPotatoFirstDelaySecondsField;
    private static FieldInfo KillerRoleField;
    private static FieldInfo KillRaceDurationSecondsField;
    private static FieldInfo KillRaceRespawnSecondsField;

    private static FieldInfo MaxVentSecondsField;
    private static FieldInfo VentBootModeField;

    private static MethodInfo GetWinnerNameMethod;
    private static MethodInfo GetSummaryMethod;
    private static MethodInfo GetModeNameMethod;

    private static float nextAutoSyncAt;

    public static bool IsAvailable()
    {
        return TryResolve();
    }

    private static bool TryResolve()
    {
        try
        {
            if (FfaAssembly != null &&
                ActiveModeField != null)
            {
                return true;
            }

            ResetCache();

            FfaAssembly =
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(assembly =>
                        assembly.GetType(
                            "FFA.FFAOptions",
                            false
                        ) != null &&
                        assembly.GetType(
                            "FFA.FfaExternalModePublicApi",
                            false
                        ) != null
                    );

            if (FfaAssembly == null)
                return false;

            Type optionsType =
                FfaAssembly.GetType(
                    "FFA.FFAOptions",
                    false
                );

            Type ventOptionsType =
                FfaAssembly.GetType(
                    "FFA.FfaVentLimitPatch+FfaVentOptions",
                    false
                ) ??
                FfaAssembly.GetType(
                    "FFA.FfaVentOptions",
                    false
                );

            Type ventBootType =
                FfaAssembly.GetType(
                    "FFA.FfaVentBootOptions",
                    false
                );

            Type apiType =
                FfaAssembly.GetType(
                    "FFA.FfaExternalModePublicApi",
                    false
                );

            ActiveModeField =
                FindField(optionsType, "ActiveMode");

            EnabledField =
                FindField(optionsType, "Enabled");

            TeamsEnabledField =
                FindField(optionsType, "TeamsEnabled");

            TeamCountField =
                FindField(optionsType, "TeamCount");

            HotPotatoEnabledField =
                FindField(optionsType, "HotPotatoEnabled");

            HotPotatoExplosionSecondsField =
                FindField(
                    optionsType,
                    "HotPotatoExplosionSeconds"
                );

            HotPotatoFirstDelaySecondsField =
                FindField(
                    optionsType,
                    "HotPotatoFirstDelaySeconds"
                );

            KillerRoleField =
                FindField(optionsType, "KillerRole");

            KillRaceDurationSecondsField =
                FindField(
                    optionsType,
                    "KillRaceDurationSeconds"
                );

            KillRaceRespawnSecondsField =
                FindField(
                    optionsType,
                    "KillRaceRespawnSeconds"
                );

            MaxVentSecondsField =
                FindField(
                    ventOptionsType,
                    "MaxVentSeconds"
                );

            VentBootModeField =
                FindField(
                    ventBootType,
                    "Mode"
                );

            GetWinnerNameMethod =
                FindMethod(
                    apiType,
                    "GetWinnerName"
                );

            GetSummaryMethod =
                FindMethod(
                    apiType,
                    "GetSummary"
                );

            GetModeNameMethod =
                FindMethod(
                    apiType,
                    "GetModeName"
                );

            return ActiveModeField != null;
        }
        catch
        {
            ResetCache();
            return false;
        }
    }

    private static FieldInfo FindField(
        Type type,
        string name)
    {
        return type?.GetField(
            name,
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static
        );
    }

    private static MethodInfo FindMethod(
        Type type,
        string name)
    {
        return type?.GetMethod(
            name,
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static
        );
    }

    private static int GetSelectedExternalMode()
    {
        if (Options.GameMode == null)
            return 0;

        try
        {
            if (Options.GameMode.GetValue(
                    GameModeType.FFA))
                return 1;

            if (Options.EnableFFA.GetBool() && GameManager.Instance.IsHideAndSeek())
                return 1;

            if (Options.GameMode.GetValue(
                    GameModeType.FFATeam))
                return 2;

            if (Options.GameMode.GetValue(
                    GameModeType.HotPotatoModded))
                return 3;

            if (Options.GameMode.GetValue(
                    GameModeType.Assassin))
                return 4;

            if (Options.GameMode.GetValue(
                    GameModeType.KillRace))
                return 5;
        }
        catch
        {
        }

        return 0;
    }

    public static bool IsExternalFfaModeSelected()
    {
        return GetSelectedExternalMode() != 0;
    }

    public static void SyncAll(
        bool force = false)
    {
        // Joined clients consume the host snapshot; never overwrite it with local presets.
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameId != 0 &&
            !AmongUsClient.Instance.AmHost)
            return;
        if (!TryResolve())
            return;

        int mode =
            GetSelectedExternalMode();

        bool legacyFfaEnabled =
            mode == 1 ||
            mode == 2 ||
            mode == 3;

        SetValue(
            ActiveModeField,
            mode
        );

        SetValue(
            EnabledField,
            legacyFfaEnabled
        );

        SetValue(
            TeamsEnabledField,
            mode == 2
        );

        SetValue(
            HotPotatoEnabledField,
            mode == 3
        );

        if (Options.FfaTeamCount != null)
        {
            SetValue(
                TeamCountField,
                Mathf.Clamp(
                    Options.FfaTeamCount.GetValue() + 2,
                    2,
                    5
                )
            );
        }

        if (Options.HotPotatoModdedExplosionSeconds != null)
        {
            SetValue(
                HotPotatoExplosionSecondsField,
                Mathf.Clamp(
                    Options.HotPotatoModdedExplosionSeconds.GetInt(),
                    5,
                    120
                )
            );
        }

        if (Options.HotPotatoModdedFirstDelaySeconds != null)
        {
            SetValue(
                HotPotatoFirstDelaySecondsField,
                Mathf.Clamp(
                    Options.HotPotatoModdedFirstDelaySeconds.GetInt(),
                    0,
                    30
                )
            );
        }

        int killerRole = 0;

        switch (mode)
        {
            case 1:
                killerRole =
                    Options.FfaKillerRole != null
                        ? Options.FfaKillerRole.GetValue()
                        : 0;
                break;

            case 2:
                killerRole =
                    Options.FfaTeamKillerRole != null
                        ? Options.FfaTeamKillerRole.GetValue()
                        : 0;
                break;

            case 4:
                killerRole =
                    Options.AssassinKillerRole != null
                        ? Options.AssassinKillerRole.GetValue()
                        : 0;
                break;

            case 5:
                killerRole =
                    Options.KillRaceKillerRole != null
                        ? Options.KillRaceKillerRole.GetValue()
                        : 0;
                break;
        }

        if (mode != 3)
        {
            SetValue(
                KillerRoleField,
                Mathf.Clamp(killerRole, 0, 1)
            );
        }

        if (Options.KillRaceDurationSeconds != null)
        {
            SetValue(
                KillRaceDurationSecondsField,
                Mathf.Clamp(
                    Options.KillRaceDurationSeconds.GetInt(),
                    30,
                    1800
                )
            );
        }

        if (Options.KillRaceRespawnSeconds != null)
        {
            SetValue(
                KillRaceRespawnSecondsField,
                Mathf.Clamp(
                    Options.KillRaceRespawnSeconds.GetInt(),
                    1,
                    15
                )
            );
        }

        if (Options.FfaVentMaxSeconds != null)
        {
            SetValue(
                MaxVentSecondsField,
                Mathf.Clamp(
                    Options.FfaVentMaxSeconds.GetInt(),
                    1,
                    30
                )
            );
        }

        if (Options.FFAVentTeleportMode != null)
        {
            SetValue(
                VentBootModeField,
                Mathf.Clamp(
                    Options.FFAVentTeleportMode.GetValue(),
                    0,
                    2
                )
            );
        }

        if (Options.FfaVentMaxSeconds1 != null)
        {
            SetValue(
                MaxVentSecondsField,
                Mathf.Clamp(
                    Options.FfaVentMaxSeconds1.GetInt(),
                    1,
                    30
                )
            );
        }

        if (Options.FFAVentTeleportMode1 != null)
        {
            SetValue(
                VentBootModeField,
                Mathf.Clamp(
                    Options.FFAVentTeleportMode1.GetValue(),
                    0,
                    2
                )
            );
        }
    }

    internal static void ApplyReceivedMode(int mode)
    {
        if (AmongUsClient.Instance == null || AmongUsClient.Instance.AmHost) return;
        UpdateModeSelection(mode);
    }

    internal static void ApplyHostMode(int mode)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
        UpdateModeSelection(mode);
    }

    private static void UpdateModeSelection(int mode)
    {
        if (Options.GameMode == null || mode < 0 || mode > 5) return;
        if (GameStates.isHideNSeek)
        {
            // Hide N Seek uses EnableFFA, not the Classic GameMode selector.
            // Selecting GameModeType.FFA here fights the HnS menu's Default reset
            // on every snapshot and repeatedly closes/reopens the settings UI.
            if ((mode == 0 || mode == 1) && Options.EnableFFA != null)
            {
                bool enabled = mode == 1;
                if (Options.EnableFFA.GetBool() != enabled)
                    Options.EnableFFA.SetValue(enabled ? 1 : 0, false);
            }
            return;
        }
        GameModeType selected = mode switch
        {
            1 => GameModeType.FFA,
            2 => GameModeType.FFATeam,
            3 => GameModeType.HotPotatoModded,
            4 => GameModeType.Assassin,
            5 => GameModeType.KillRace,
            _ => GameModeType.Default
        };
        // An inactive FFA snapshot must not replace another non-FFA game mode.
        if (mode == 0 && !IsExternalFfaModeSelected()) return;
        if (Options.GameMode.Selected == selected || !Options.GameMode.GetOrder().Contains(selected)) return;
        Options.GameMode.SetValue(selected, false);
    }

    public static void SyncGameMode()
    {
        SyncAll(true);
    }

    public static void SyncVentSeconds()
    {
        SyncAll(true);
    }

    public static void SyncVentMode()
    {
        SyncAll(true);
    }

    public static void SyncTeamMode()
    {
        SyncAll(true);
    }

    public static void SyncTeamCount()
    {
        SyncAll(true);
    }

    public static void SyncHotPotatoMode()
    {
        SyncAll(true);
    }

    public static void SyncHotPotatoExplosionSeconds()
    {
        SyncAll(true);
    }

    public static void SyncHotPotatoFirstDelaySeconds()
    {
        SyncAll(true);
    }

    public static string GetWinnerName()
    {
        return InvokeString(
            GetWinnerNameMethod
        );
    }

    public static string GetSummary()
    {
        return InvokeString(
            GetSummaryMethod
        );
    }

    public static string GetModeName()
    {
        return InvokeString(
            GetModeNameMethod
        );
    }

    private static string InvokeString(
        MethodInfo method)
    {
        try
        {
            if (!TryResolve() ||
                method == null)
            {
                return "";
            }

            return method.Invoke(
                null,
                null
            )?.ToString() ?? "";
        }
        catch
        {
            return "";
        }
    }

    private static void SetValue(
        FieldInfo field,
        object value)
    {
        if (field == null ||
            value == null)
        {
            return;
        }

        try
        {
            Type type =
                field.FieldType;

            object converted =
                type.IsEnum
                    ? Enum.ToObject(
                        type,
                        Convert.ToInt32(value)
                    )
                    : Convert.ChangeType(
                        value,
                        type
                    );

            field.SetValue(
                null,
                converted
            );
        }
        catch
        {
        }
    }

    public static void AutoSync()
    {
        if (Time.realtimeSinceStartup <
            nextAutoSyncAt)
        {
            return;
        }

        nextAutoSyncAt =
            Time.realtimeSinceStartup + 0.5f;

        SyncAll();
    }

    public static void ResetCache()
    {
        FfaAssembly = null;

        ActiveModeField = null;
        EnabledField = null;
        TeamsEnabledField = null;
        TeamCountField = null;
        HotPotatoEnabledField = null;
        HotPotatoExplosionSecondsField = null;
        HotPotatoFirstDelaySecondsField = null;
        KillerRoleField = null;
        KillRaceDurationSecondsField = null;
        KillRaceRespawnSecondsField = null;

        MaxVentSecondsField = null;
        VentBootModeField = null;

        GetWinnerNameMethod = null;
        GetSummaryMethod = null;
        GetModeNameMethod = null;
    }
}

[HarmonyPatch(
    typeof(AmongUsClient),
    nameof(AmongUsClient.Update)
)]
internal static class FfaExternalBridgeAutoSyncPatch
{
    private static void Postfix()
    {
        FfaExternalBridge.AutoSync();
    }
}

[HarmonyPatch(
    typeof(GameStartManager),
    nameof(GameStartManager.BeginGame)
)]
internal static class FfaExternalBridgeBeginGamePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix()
    {
        FfaExternalBridge.SyncAll(true);
    }
}
