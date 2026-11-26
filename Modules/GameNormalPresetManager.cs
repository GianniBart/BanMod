//credits and licenses in the resources folder
using System;
using System.IO;
using System.Text.Json;
using System.Reflection;
using AmongUs;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace BanMod
{
    public class RoleOption
    {
        public int Count { get; set; } = 1;
        public int Chance { get; set; } = 100;
    }

    public class RoleSettings
    {
        public RoleOption Shapeshifter { get; set; } = new RoleOption();
        public RoleOption Phantom { get; set; } = new RoleOption();
        public RoleOption Scientist { get; set; } = new RoleOption();
        public RoleOption GuardianAngel { get; set; } = new RoleOption();
        public RoleOption Engineer { get; set; } = new RoleOption();
        public RoleOption Noisemaker { get; set; } = new RoleOption();
        public RoleOption Tracker { get; set; } = new RoleOption();
        public RoleOption Detective { get; set; } = new RoleOption();
        public RoleOption Judge { get; set; } = new RoleOption();
        public RoleOption Viper { get; set; } = new RoleOption();
        public RoleOption SpiritGuide { get; set; } = new RoleOption();

        public float viperDissolveTime { get; set; } = 10f;
        public float SpiritGuideCooldownSeconds { get; set; } = 20f;
        public float DetectiveSuspectLimit { get; set; } = 3f;
        public float JudgeTaskRequirementPercentage { get; set; } = 50f;

        public float EngineerCooldown { get; set; } = 5f;
        public float EngineerInVentMaxTime { get; set; } = 30f;

        public float GuardianAngelCooldown { get; set; } = 35f;
        public float GuardianAngelDuration { get; set; } = 25f;

        public float ScientistCooldown { get; set; } = 10f;
        public float ScientistBattery { get; set; } = 30f;

        public float TrackerCooldown { get; set; } = 10f;
        public float TrackerDelay { get; set; } = 0f;
        public float TrackerDuration { get; set; } = 30f;

        public bool NoisemakerAlert { get; set; } = true;
        public float NoisemakerDuration { get; set; } = 10f;

        public bool ShapeshifterLeaveSkin { get; set; } = false;
        public float ShapeshifterCooldown { get; set; } = 10f;
        public float ShapeshifterDuration { get; set; } = 30f;

        public float PhantomCooldown { get; set; } = 10f;
        public float PhantomDuration { get; set; } = 30f;
    }

    public class CustomOptions
    {
        public string PresetName { get; set; } = "";
        public int MaxPlayers { get; set; } = 15;
        public int NumImpostors { get; set; } = 2;
        public float PlayerSpeedMod { get; set; } = 1.0f;
        public float CrewLightMod { get; set; } = 1.0f;
        public float ImpostorLightMod { get; set; } = 1.0f;
        public float KillCooldown { get; set; } = 15f;
        public int NumCommonTasks { get; set; } = 1;
        public int NumLongTasks { get; set; } = 1;
        public int NumShortTasks { get; set; } = 2;
        public int NumEmergencyMeetings { get; set; } = 1;
        public bool AnonymousVotes { get; set; } = false;
        public AmongUs.GameOptions.TaskBarMode TaskBarMode { get; set; } = AmongUs.GameOptions.TaskBarMode.Normal;
        public int KillDistance { get; set; } = 1;
        public int EmergencyCooldown { get; set; } = 20;
        public int DiscussionTime { get; set; } = 30;
        public int VotingTime { get; set; } = 45;
        public bool IsDefaults { get; set; } = true;
        public bool ConfirmImpostor { get; set; } = true;
        public bool VisualTasks { get; set; } = true;

        public RoleSettings Roles { get; set; } = new RoleSettings();
    }

    public enum GameModeType
    {
        SnS = 0,
        KaitoRun = 2,
        Default = 3,
        TaskRun = 4,
        FFA = 6,
        ZombieMode = 7,
        PnS = 8,
        HotPotatoModded = 9,
        RoomRush = 10,
        TargetRush = 11,
        FFATeam = 12,
        Assassin = 13,
        KillRace = 14,
        DeathRun = 15
    }

    public enum PresetSelectionType
    {
        Default = 0,
        Preset1 = 1,
        Preset2 = 2,
        Preset3 = 3
    }

    [HarmonyPatch(typeof(NormalGameOptionsV12), nameof(NormalGameOptionsV12.SetRecommendations), new Type[] { typeof(int), typeof(bool), typeof(RulesPresets) })]
    public static class SetRecommendationsPatch
    {
        public static bool Prefix(NormalGameOptionsV12 __instance, int numPlayers, bool isOnline, RulesPresets rulesPresets)
        {
            try
            {
                if (BanMod.IsBanModDisabled)
                    return true;

                if (AmongUsClient.Instance == null)
                    return true;

                if (PlayerControl.LocalPlayer?.Data == null)
                    return true;

                if (!GameStates.isLobby)
                    return true;

                if (__instance == null)
                    return true;

                if (Options.GameMode == null ||
                    Options.PresetSelection == null)
                {
                    return true;
                }

                if (GameOptionsManager.Instance == null ||
                    GameOptionsManager.Instance.CurrentGameOptions == null)
                {
                    return true;
                }

                if (GameStates.isHideNSeek)
                    return true;

                switch (rulesPresets)
                {
                    case RulesPresets.Standard:
                        SetStandardRecommendations(__instance, numPlayers, isOnline);
                        return false;

                    default:
                        return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BanMod] SetRecommendations Prefix skipped: " +
                    ex.GetType().Name + " - " + ex.Message
                );

                return true;
            }
        }

        public static void SetStandardRecommendations(NormalGameOptionsV12 __instance, int numPlayers, bool isOnline)
        {
            if (__instance == null)
                return;

            if (AmongUsClient.Instance == null)
                return;

            if (PlayerControl.LocalPlayer?.Data == null)
                return;

            if (!GameStates.isLobby)
                return;

            if (Options.GameMode == null ||
                Options.PresetSelection == null)
            {
                return;
            }

            GameModeType gameMode =
                Options.GameMode.Selected;

            PresetSelectionType presetSelection =
                (PresetSelectionType)Options.PresetSelection.GetValue();

            CustomOptions options =
                ResolveOptions(gameMode, presetSelection);

            if (options == null)
                return;

            ApplyToInstance(__instance, options);
        }

        private static CustomOptions ResolveOptions(GameModeType gameMode, PresetSelectionType presetSelection)
        {
            switch (presetSelection)
            {
                case PresetSelectionType.Preset1:
                    return LoadUserPresetFile(1);
                case PresetSelectionType.Preset2:
                    return LoadUserPresetFile(2);
                case PresetSelectionType.Preset3:
                    return LoadUserPresetFile(3);
                case PresetSelectionType.Default:
                default:
                    return LoadGameModePreset(gameMode);
            }
        }

        public static CustomOptions LoadGameModePreset(GameModeType gameMode)
        {
            switch (gameMode)
            {
                case GameModeType.SnS:
                    return LoadOrCreateGameModePreset("SnS.json", CreateSnSDefaults());

                case GameModeType.PnS:
                    return LoadOrCreateGameModePreset("PnS.json", CreatePnSDefaults());

                case GameModeType.ZombieMode:
                    return LoadOrCreateGameModePreset("ZombieMode.json", CreateZombieModeDefaults());

                case GameModeType.HotPotatoModded:
                    return LoadOrCreateGameModePreset("HotPotato.json", CreateHotPotatoDefaults());

                case GameModeType.KaitoRun:
                    return LoadOrCreateGameModePreset("KaitoRun.json", CreateKaitoRunDefaults());

                case GameModeType.Default:
                    return LoadOrCreateGameModePreset("Default.json", CreateDefaultDefaults());

                case GameModeType.TaskRun:
                    return LoadOrCreateGameModePreset("TaskRun.json", CreateTaskRunDefaults());

                case GameModeType.FFA:
                    return LoadOrCreateGameModePreset("FFA.json", CreateFFAModeDefaults());

                case GameModeType.FFATeam:
                    return LoadOrCreateGameModePreset("FFATEAM.json", CreateFFATEAMModeDefaults());

                case GameModeType.KillRace:
                    return LoadOrCreateGameModePreset("KillRace.json", CreateKillRaceModeDefaults());

                case GameModeType.Assassin:
                    return LoadOrCreateGameModePreset("Assassin.json", CreateAssassinModeDefaults());

                case GameModeType.RoomRush:
                    return LoadOrCreateGameModePreset("RoomRush.json", CreateRoomRushModeDefaults());

                case GameModeType.TargetRush:
                    return LoadOrCreateGameModePreset("TargetRush.json", CreateTargetRushModeDefaults());

                case GameModeType.DeathRun:
                    return LoadOrCreateGameModePreset("DeathRun.json", CreateDeathRunModeDefaults());

                default:
                    return new CustomOptions();
            }
        }

        public static void ApplyToInstance(NormalGameOptionsV12 __instance, CustomOptions options)
        {
            __instance.MaxPlayers = options.MaxPlayers;
            __instance.NumImpostors = options.NumImpostors;
            __instance.PlayerSpeedMod = options.PlayerSpeedMod;
            __instance.CrewLightMod = options.CrewLightMod;
            __instance.ImpostorLightMod = options.ImpostorLightMod;
            __instance.KillCooldown = options.KillCooldown;
            __instance.NumCommonTasks = options.NumCommonTasks;
            __instance.NumLongTasks = options.NumLongTasks;
            __instance.NumShortTasks = options.NumShortTasks;
            __instance.NumEmergencyMeetings = options.NumEmergencyMeetings;
            __instance.AnonymousVotes = options.AnonymousVotes;
            __instance.TaskBarMode = options.TaskBarMode;
            __instance.KillDistance = options.KillDistance;
            __instance.EmergencyCooldown = options.EmergencyCooldown;
            __instance.DiscussionTime = options.DiscussionTime;
            __instance.VotingTime = options.VotingTime;
            __instance.IsDefaults = options.IsDefaults;
            __instance.ConfirmImpostor = options.ConfirmImpostor;
            __instance.VisualTasks = options.VisualTasks;

            var roles = options.Roles;

            __instance.roleOptions.SetRoleRate(RoleTypes.Shapeshifter, roles.Shapeshifter.Count, roles.Shapeshifter.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Phantom, roles.Phantom.Count, roles.Phantom.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Scientist, roles.Scientist.Count, roles.Scientist.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.GuardianAngel, roles.GuardianAngel.Count, roles.GuardianAngel.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Engineer, roles.Engineer.Count, roles.Engineer.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Noisemaker, roles.Noisemaker.Count, roles.Noisemaker.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Tracker, roles.Tracker.Count, roles.Tracker.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Viper, roles.Viper.Count, roles.Viper.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Detective, roles.Detective.Count, roles.Detective.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.Judge, roles.Judge.Count, roles.Judge.Chance);
            __instance.roleOptions.SetRoleRate(RoleTypes.SpiritGuide, roles.SpiritGuide.Count, roles.SpiritGuide.Chance);


            if (__instance.roleOptions.TryGetRoleOptions<ViperRoleOptionsV12>(RoleTypes.Viper, out var viperOptions))
            {
                viperOptions.ViperDissolveTime = roles.viperDissolveTime;
            }

            if (__instance.roleOptions.TryGetRoleOptions<DetectiveRoleOptionsV12>(RoleTypes.Detective, out var detectiveOptions))
            {
                detectiveOptions.DetectiveSuspectLimit = roles.DetectiveSuspectLimit;
            }

            if (__instance.roleOptions.TryGetRoleOptions<SpiritGuideRoleOptionsV12>(RoleTypes.SpiritGuide, out var SpiritGuideOptions))
            {
                SpiritGuideOptions.SpiritGuideCooldownSeconds = roles.SpiritGuideCooldownSeconds;
            }

            if (__instance.roleOptions.TryGetRoleOptions<JudgeRoleOptionsV12>(RoleTypes.Judge, out var judgeOptions))
            {
                judgeOptions.JudgeTaskRequirementPercentage = roles.JudgeTaskRequirementPercentage;
            }

            if (__instance.roleOptions.TryGetRoleOptions<EngineerRoleOptionsV12>(RoleTypes.Engineer, out var engineerOptions))
            {
                engineerOptions.EngineerCooldown = roles.EngineerCooldown;
                engineerOptions.EngineerInVentMaxTime = roles.EngineerInVentMaxTime;
            }

            if (__instance.roleOptions.TryGetRoleOptions<GuardianAngelRoleOptionsV12>(RoleTypes.GuardianAngel, out var guardianAngelOptions))
            {
                guardianAngelOptions.GuardianAngelCooldown = roles.GuardianAngelCooldown;
                guardianAngelOptions.ProtectionDurationSeconds = roles.GuardianAngelDuration;
            }

            if (__instance.roleOptions.TryGetRoleOptions<ScientistRoleOptionsV12>(RoleTypes.Scientist, out var scientistOptions))
            {
                scientistOptions.ScientistCooldown = roles.ScientistCooldown;
                scientistOptions.ScientistBatteryCharge = roles.ScientistBattery;
            }

            if (__instance.roleOptions.TryGetRoleOptions<TrackerRoleOptionsV12>(RoleTypes.Tracker, out var trackerOptions))
            {
                trackerOptions.TrackerCooldown = roles.TrackerCooldown;
                trackerOptions.TrackerDelay = roles.TrackerDelay;
                trackerOptions.TrackerDuration = roles.TrackerDuration;
            }

            if (__instance.roleOptions.TryGetRoleOptions<NoisemakerRoleOptionsV12>(RoleTypes.Noisemaker, out var noisemakerOptions))
            {
                noisemakerOptions.NoisemakerImpostorAlert = roles.NoisemakerAlert;
                noisemakerOptions.NoisemakerAlertDuration = roles.NoisemakerDuration;
            }

            if (__instance.roleOptions.TryGetRoleOptions<ShapeshifterRoleOptionsV12>(RoleTypes.Shapeshifter, out var shapeshifterOptions))
            {
                shapeshifterOptions.ShapeshifterLeaveSkin = roles.ShapeshifterLeaveSkin;
                shapeshifterOptions.ShapeshifterCooldown = roles.ShapeshifterCooldown;
                shapeshifterOptions.ShapeshifterDuration = roles.ShapeshifterDuration;
            }

            if (__instance.roleOptions.TryGetRoleOptions<PhantomRoleOptionsV12>(RoleTypes.Phantom, out var phantomOptions))
            {
                phantomOptions.PhantomCooldown = roles.PhantomCooldown;
                phantomOptions.PhantomDuration = roles.PhantomDuration;
            }
        }


        public static CustomOptions CaptureFromInstance(NormalGameOptionsV12 instance)
        {
            if (instance == null)
                return null;

            CustomOptions fallback = null;

            try
            {
                GameModeType gameMode = Options.GameMode.Selected;
                PresetSelectionType presetSelection = (PresetSelectionType)Options.PresetSelection.GetValue();
                fallback = ResolveOptions(gameMode, presetSelection);
            }
            catch
            {
                fallback = new CustomOptions();
            }

            if (fallback == null)
                fallback = new CustomOptions();

            CustomOptions result = new CustomOptions
            {
                MaxPlayers = instance.MaxPlayers,
                NumImpostors = instance.NumImpostors,
                PlayerSpeedMod = instance.PlayerSpeedMod,
                CrewLightMod = instance.CrewLightMod,
                ImpostorLightMod = instance.ImpostorLightMod,
                KillCooldown = instance.KillCooldown,
                NumCommonTasks = instance.NumCommonTasks,
                NumLongTasks = instance.NumLongTasks,
                NumShortTasks = instance.NumShortTasks,
                NumEmergencyMeetings = instance.NumEmergencyMeetings,
                AnonymousVotes = instance.AnonymousVotes,
                TaskBarMode = instance.TaskBarMode,
                KillDistance = instance.KillDistance,
                EmergencyCooldown = instance.EmergencyCooldown,
                DiscussionTime = instance.DiscussionTime,
                VotingTime = instance.VotingTime,
                IsDefaults = instance.IsDefaults,
                ConfirmImpostor = instance.ConfirmImpostor,
                VisualTasks = instance.VisualTasks,
                Roles = new RoleSettings()
            };

            result.Roles.Shapeshifter = CaptureRoleOption(instance, RoleTypes.Shapeshifter, fallback.Roles.Shapeshifter);
            result.Roles.Phantom = CaptureRoleOption(instance, RoleTypes.Phantom, fallback.Roles.Phantom);
            result.Roles.Scientist = CaptureRoleOption(instance, RoleTypes.Scientist, fallback.Roles.Scientist);
            result.Roles.GuardianAngel = CaptureRoleOption(instance, RoleTypes.GuardianAngel, fallback.Roles.GuardianAngel);
            result.Roles.Engineer = CaptureRoleOption(instance, RoleTypes.Engineer, fallback.Roles.Engineer);
            result.Roles.Noisemaker = CaptureRoleOption(instance, RoleTypes.Noisemaker, fallback.Roles.Noisemaker);
            result.Roles.Tracker = CaptureRoleOption(instance, RoleTypes.Tracker, fallback.Roles.Tracker);
            result.Roles.Detective = CaptureRoleOption(instance, RoleTypes.Detective, fallback.Roles.Detective);
            result.Roles.Judge = CaptureRoleOption(instance, RoleTypes.Judge, fallback.Roles.Judge);
            result.Roles.Viper = CaptureRoleOption(instance, RoleTypes.Viper, fallback.Roles.Viper);
            result.Roles.SpiritGuide = CaptureRoleOption(instance, RoleTypes.SpiritGuide, fallback.Roles.SpiritGuide);


            if (instance.roleOptions.TryGetRoleOptions<ViperRoleOptionsV12>(RoleTypes.Viper, out var viperOptions))
                result.Roles.viperDissolveTime = viperOptions.ViperDissolveTime;
            else
                result.Roles.viperDissolveTime = fallback.Roles.viperDissolveTime;

            if (instance.roleOptions.TryGetRoleOptions<DetectiveRoleOptionsV12>(RoleTypes.Detective, out var detectiveOptions))
                result.Roles.DetectiveSuspectLimit = detectiveOptions.DetectiveSuspectLimit;
            else
                result.Roles.DetectiveSuspectLimit = fallback.Roles.DetectiveSuspectLimit;

            if (instance.roleOptions.TryGetRoleOptions<JudgeRoleOptionsV12>(RoleTypes.Judge, out var judgeOptions))
                result.Roles.JudgeTaskRequirementPercentage = judgeOptions.JudgeTaskRequirementPercentage;
            else
                result.Roles.JudgeTaskRequirementPercentage = fallback.Roles.JudgeTaskRequirementPercentage;

            if (instance.roleOptions.TryGetRoleOptions<EngineerRoleOptionsV12>(RoleTypes.Engineer, out var engineerOptions))
            {
                result.Roles.EngineerCooldown = engineerOptions.EngineerCooldown;
                result.Roles.EngineerInVentMaxTime = engineerOptions.EngineerInVentMaxTime;
            }
            else
            {
                result.Roles.EngineerCooldown = fallback.Roles.EngineerCooldown;
                result.Roles.EngineerInVentMaxTime = fallback.Roles.EngineerInVentMaxTime;
            }

            if (instance.roleOptions.TryGetRoleOptions<GuardianAngelRoleOptionsV12>(RoleTypes.GuardianAngel, out var guardianAngelOptions))
            {
                result.Roles.GuardianAngelCooldown = guardianAngelOptions.GuardianAngelCooldown;
                result.Roles.GuardianAngelDuration = guardianAngelOptions.ProtectionDurationSeconds;
            }
            else
            {
                result.Roles.GuardianAngelCooldown = fallback.Roles.GuardianAngelCooldown;
                result.Roles.GuardianAngelDuration = fallback.Roles.GuardianAngelDuration;
            }

            if (instance.roleOptions.TryGetRoleOptions<SpiritGuideRoleOptionsV12>(RoleTypes.SpiritGuide, out var SpiritGuideOptions))
            {
                result.Roles.SpiritGuideCooldownSeconds = SpiritGuideOptions.SpiritGuideCooldownSeconds;
            }
            else
            {
                result.Roles.SpiritGuideCooldownSeconds = fallback.Roles.SpiritGuideCooldownSeconds;
            }

            if (instance.roleOptions.TryGetRoleOptions<ScientistRoleOptionsV12>(RoleTypes.Scientist, out var scientistOptions))
            {
                result.Roles.ScientistCooldown = scientistOptions.ScientistCooldown;
                result.Roles.ScientistBattery = scientistOptions.ScientistBatteryCharge;
            }
            else
            {
                result.Roles.ScientistCooldown = fallback.Roles.ScientistCooldown;
                result.Roles.ScientistBattery = fallback.Roles.ScientistBattery;
            }

            if (instance.roleOptions.TryGetRoleOptions<TrackerRoleOptionsV12>(RoleTypes.Tracker, out var trackerOptions))
            {
                result.Roles.TrackerCooldown = trackerOptions.TrackerCooldown;
                result.Roles.TrackerDelay = trackerOptions.TrackerDelay;
                result.Roles.TrackerDuration = trackerOptions.TrackerDuration;
            }
            else
            {
                result.Roles.TrackerCooldown = fallback.Roles.TrackerCooldown;
                result.Roles.TrackerDelay = fallback.Roles.TrackerDelay;
                result.Roles.TrackerDuration = fallback.Roles.TrackerDuration;
            }

            if (instance.roleOptions.TryGetRoleOptions<NoisemakerRoleOptionsV12>(RoleTypes.Noisemaker, out var noisemakerOptions))
            {
                result.Roles.NoisemakerAlert = noisemakerOptions.NoisemakerImpostorAlert;
                result.Roles.NoisemakerDuration = noisemakerOptions.NoisemakerAlertDuration;
            }
            else
            {
                result.Roles.NoisemakerAlert = fallback.Roles.NoisemakerAlert;
                result.Roles.NoisemakerDuration = fallback.Roles.NoisemakerDuration;
            }

            if (instance.roleOptions.TryGetRoleOptions<ShapeshifterRoleOptionsV12>(RoleTypes.Shapeshifter, out var shapeshifterOptions))
            {
                result.Roles.ShapeshifterLeaveSkin = shapeshifterOptions.ShapeshifterLeaveSkin;
                result.Roles.ShapeshifterCooldown = shapeshifterOptions.ShapeshifterCooldown;
                result.Roles.ShapeshifterDuration = shapeshifterOptions.ShapeshifterDuration;
            }
            else
            {
                result.Roles.ShapeshifterLeaveSkin = fallback.Roles.ShapeshifterLeaveSkin;
                result.Roles.ShapeshifterCooldown = fallback.Roles.ShapeshifterCooldown;
                result.Roles.ShapeshifterDuration = fallback.Roles.ShapeshifterDuration;
            }

            if (instance.roleOptions.TryGetRoleOptions<PhantomRoleOptionsV12>(RoleTypes.Phantom, out var phantomOptions))
            {
                result.Roles.PhantomCooldown = phantomOptions.PhantomCooldown;
                result.Roles.PhantomDuration = phantomOptions.PhantomDuration;
            }
            else
            {
                result.Roles.PhantomCooldown = fallback.Roles.PhantomCooldown;
                result.Roles.PhantomDuration = fallback.Roles.PhantomDuration;
            }

            return result;
        }

        private static RoleOption CaptureRoleOption(
            NormalGameOptionsV12 instance,
            RoleTypes role,
            RoleOption fallback)
        {
            int count = fallback != null ? fallback.Count : 0;
            int chance = fallback != null ? fallback.Chance : 100;

            TryReadRoleInt(
                instance.roleOptions,
                role,
                new[]
                {
                    "GetNumPerGame",
                    "GetNumPerGameForRole",
                    "GetRoleCount",
                    "GetCountPerGame",
                    "GetCount"
                },
                ref count
            );

            TryReadRoleInt(
                instance.roleOptions,
                role,
                new[]
                {
                    "GetChancePerGame",
                    "GetChancePerGameForRole",
                    "GetRoleChance",
                    "GetChance"
                },
                ref chance
            );

            return new RoleOption
            {
                Count = count,
                Chance = chance
            };
        }

        private static void TryReadRoleInt(
            object target,
            RoleTypes role,
            string[] methodNames,
            ref int value)
        {
            if (target == null)
                return;

            try
            {
                Type type = target.GetType();
                BindingFlags flags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;

                MethodInfo[] methods = type.GetMethods(flags);

                foreach (string methodName in methodNames)
                {
                    foreach (MethodInfo method in methods)
                    {
                        if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
                            continue;

                        ParameterInfo[] parameters = method.GetParameters();

                        if (parameters.Length != 1)
                            continue;

                        object argument;

                        Type parameterType = parameters[0].ParameterType;

                        if (parameterType == typeof(RoleTypes))
                            argument = role;
                        else if (parameterType == typeof(int))
                            argument = (int)role;
                        else if (parameterType == typeof(byte))
                            argument = (byte)role;
                        else
                            continue;

                        object raw = method.Invoke(target, new[] { argument });

                        if (raw == null)
                            continue;

                        value = Convert.ToInt32(raw);
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        public static NormalGameOptionsV12 GetCurrentGameOptions()
        {
            try
            {
                if (GameOptionsManager.Instance == null)
                    return null;

                if (GameOptionsManager.Instance.CurrentGameOptions == null)
                    return null;

                return GameOptionsManager.Instance
                    .CurrentGameOptions
                    .Cast<NormalGameOptionsV12>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BanMod] Could not get NormalGameOptionsV12: " +
                    ex.GetType().Name + " - " + ex.Message
                );

                return null;
            }
        }

        public static void SyncCurrentOptions(NormalGameOptionsV12 options)
        {
            if (options == null)
                return;

            try
            {
                object manager = GameOptionsManager.Instance;

                TryInvokeNoArg(manager, "SaveNormalHostOptions");
                TryInvokeNoArg(manager, "SaveHostOptions");
                TryInvokeNoArg(manager, "SaveOptions");
            }
            catch
            {
            }

            try
            {
                object localPlayer = PlayerControl.LocalPlayer;

                if (localPlayer != null)
                {
                    if (!TryInvokeOneArg(localPlayer, "RpcSyncSettings", options))
                        TryInvokeOneArg(localPlayer, "SyncSettings", options);
                }
            }
            catch
            {
            }
        }

        private static object GetMemberValue(object target, string name)
        {
            if (target == null)
                return null;

            try
            {
                Type type = target.GetType();

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

                if (property != null)
                    return property.GetValue(target);

                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

                if (field != null)
                    return field.GetValue(target);
            }
            catch
            {
            }

            return null;
        }

        private static bool TryInvokeNoArg(object target, string methodName)
        {
            if (target == null)
                return false;

            try
            {
                MethodInfo method = target.GetType().GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null
                );

                if (method == null)
                    return false;

                method.Invoke(target, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryInvokeOneArg(object target, string methodName, object argument)
        {
            if (target == null || argument == null)
                return false;

            try
            {
                BindingFlags flags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;

                foreach (MethodInfo method in target.GetType().GetMethods(flags))
                {
                    if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
                        continue;

                    ParameterInfo[] parameters = method.GetParameters();

                    if (parameters.Length != 1)
                        continue;

                    Type parameterType = parameters[0].ParameterType;

                    if (!parameterType.IsInstanceOfType(argument) &&
                        !parameterType.IsAssignableFrom(argument.GetType()))
                        continue;

                    method.Invoke(target, new[] { argument });
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public static CustomOptions LoadUserPresetFile(int number)
        {
            if (number < 1 || number > 3)
                return new CustomOptions();

            string fileName = $"Preset_{number}.json";
            string fullPath = Path.Combine("DATA", "GAMEMODES_PRESETS", "PRESET", fileName);

            try
            {
                string dir = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(fullPath))
                {
                    CustomOptions defaultOptions = new CustomOptions
                    {
                        PresetName = $"Preset {number}"
                    };

                    SaveUserPresetFile(number, defaultOptions);
                    return defaultOptions;
                }

                string content = File.ReadAllText(fullPath);

                CustomOptions loaded =
                    JsonSerializer.Deserialize<CustomOptions>(content)
                    ?? new CustomOptions();

                bool shouldSave = false;

                if (string.IsNullOrWhiteSpace(loaded.PresetName))
                {
                    loaded.PresetName = $"Preset {number}";
                    shouldSave = true;
                }

                if (loaded.Roles == null)
                {
                    loaded.Roles = new RoleSettings();
                    shouldSave = true;
                }

                if (!HasJudgeSettings(content))
                {
                    loaded.Roles.Judge =
                        new RoleOption { Count = 0, Chance = 100 };

                    loaded.Roles.JudgeTaskRequirementPercentage = 50f;
                    shouldSave = true;
                }

                if (shouldSave)
                    SaveUserPresetFile(number, loaded);

                return loaded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[BanMod] Could not load Preset_{number}.json: " +
                    ex.GetType().Name + " - " + ex.Message
                );

                return new CustomOptions
                {
                    PresetName = $"Preset {number}"
                };
            }
        }

        public static bool SaveUserPresetFile(int number, CustomOptions options)
        {
            if (number < 1 || number > 3 || options == null)
                return false;

            string fileName = $"Preset_{number}.json";
            string fullPath = Path.Combine("DATA", "GAMEMODES_PRESETS", "PRESET", fileName);

            try
            {
                string dir = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (string.IsNullOrWhiteSpace(options.PresetName))
                    options.PresetName = $"Preset {number}";

                string json = JsonSerializer.Serialize(
                    options,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

                File.WriteAllText(fullPath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[BanMod] Could not save Preset_{number}.json: " +
                    ex.GetType().Name + " - " + ex.Message
                );

                return false;
            }
        }

        public static string GetUserPresetName(int number)
        {
            if (number < 1 || number > 3)
                return "Preset";

            CustomOptions options = LoadUserPresetFile(number);

            if (options == null || string.IsNullOrWhiteSpace(options.PresetName))
                return $"Preset {number}";

            return options.PresetName;
        }

        public static bool RenameUserPreset(int number, string newName)
        {
            if (number < 1 || number > 3)
                return false;

            if (string.IsNullOrWhiteSpace(newName))
                return false;

            string cleanName = newName.Trim();

            if (cleanName.Length > 32)
                cleanName = cleanName.Substring(0, 32);

            CustomOptions options = LoadUserPresetFile(number);

            if (options == null)
                return false;

            options.PresetName = cleanName;

            bool saved =
                SaveUserPresetFile(number, options);

            if (saved)
                UpdatePresetSelectionNames();

            return saved;
        }

        public static void UpdatePresetSelectionNames()
        {
            try
            {
                if (Options.PresetSelection == null)
                    return;

                GameModeType currentMode =
                    GetCurrentSelectedGameMode();

                string[] names =
                {
                    GetGameModePresetName(currentMode),
                    GetUserPresetName(1),
                    GetUserPresetName(2),
                    GetUserPresetName(3)
                };

                Options.PresetSelection.Selections = names;
                Options.PresetSelection.Rule = (0, names.Length - 1, 1);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BanMod] Could not update preset display names: " +
                    ex.GetType().Name + " - " + ex.Message
                );
            }
        }

        public static string GetDefaultGameModeName(GameModeType gameMode)
        {
            switch (gameMode)
            {
                case GameModeType.SnS: return "SnS";
                case GameModeType.PnS: return "PnS";
                case GameModeType.HotPotatoModded: return "HotPotato";
                case GameModeType.ZombieMode: return "ZombieMode";
                case GameModeType.KaitoRun: return "KaitoRun";
                case GameModeType.Default: return "Default";
                case GameModeType.TaskRun: return "TaskRun";
                case GameModeType.FFA: return "FFA";
                case GameModeType.FFATeam: return "FFATeam";
                case GameModeType.Assassin: return "Assassin";
                case GameModeType.KillRace: return "KillRace";
                case GameModeType.RoomRush: return "RoomRush";
                case GameModeType.DeathRun: return "DeathRun";
                case GameModeType.TargetRush: return "TargetRush";
                default: return gameMode.ToString();
            }
        }

        public static string GetGameModePresetName(GameModeType gameMode)
        {
            string fallback = GetDefaultGameModeName(gameMode) + " Preset";
            string fileName = GetGameModePresetFileName(gameMode);

            if (string.IsNullOrWhiteSpace(fileName))
                return fallback;

            string fullPath =
                Path.Combine("DATA", "GAMEMODES_PRESETS", "GAMEMODES", fileName);

            try
            {
                if (!File.Exists(fullPath))
                    LoadGameModePreset(gameMode);

                if (!File.Exists(fullPath))
                    return fallback;

                string json = File.ReadAllText(fullPath);

                CustomOptions loaded =
                    JsonSerializer.Deserialize<CustomOptions>(json);

                if (loaded == null ||
                    string.IsNullOrWhiteSpace(loaded.PresetName))
                {
                    return fallback;
                }

                return loaded.PresetName.Trim();
            }
            catch
            {
                return fallback;
            }
        }

        public static bool RenameGameModePreset(
            GameModeType gameMode,
            string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return false;

            string cleanName = newName.Trim();

            if (cleanName.Length > 32)
                cleanName = cleanName.Substring(0, 32);

            CustomOptions current =
                LoadGameModePreset(gameMode);

            if (current == null)
                return false;

            current.PresetName = cleanName;

            bool saved =
                SaveGameModePreset(gameMode, current);

            if (saved)
                UpdatePresetSelectionNames();

            return saved;
        }

        public static void UpdateGameModeSelectionNames()
        {
            try
            {
                if (Options.GameMode == null)
                    return;

                string[] names =
                {
                    GetGameModePresetName(GameModeType.SnS),
                    GetGameModePresetName(GameModeType.PnS),
                    GetGameModePresetName(GameModeType.HotPotatoModded),
                    GetGameModePresetName(GameModeType.ZombieMode),
                    GetGameModePresetName(GameModeType.KaitoRun),
                    GetGameModePresetName(GameModeType.Default),
                    GetGameModePresetName(GameModeType.TaskRun),
                    GetGameModePresetName(GameModeType.RoomRush),
                    GetGameModePresetName(GameModeType.DeathRun),
                    GetGameModePresetName(GameModeType.TargetRush),
                    GetGameModePresetName(GameModeType.KillRace),
                    GetGameModePresetName(GameModeType.Assassin),
                    GetGameModePresetName(GameModeType.FFATeam),
                    GetGameModePresetName(GameModeType.FFA)
                };

                Options.GameMode.Selections = names;
                Options.GameMode.Rule = (0, names.Length - 1, 1);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BanMod] Could not update GameMode display names: " +
                    ex.GetType().Name + " - " + ex.Message
                );
            }
        }

        public static GameModeType GetCurrentSelectedGameMode()
        {
            try
            {
                if (Options.GameMode != null)
                    return Options.GameMode.Selected;
            }
            catch
            {
            }

            return GameModeType.Default;
        }

        public static string GetGameModePresetFileName(GameModeType gameMode)
        {
            switch (gameMode)
            {
                case GameModeType.SnS:
                    return "SnS.json";

                case GameModeType.PnS:
                    return "PnS.json";

                case GameModeType.HotPotatoModded:
                    return "HotPotato.json";

                case GameModeType.ZombieMode:
                    return "ZombieMode.json";

                case GameModeType.KaitoRun:
                    return "KaitoRun.json";

                case GameModeType.Default:
                    return "Default.json";

                case GameModeType.TaskRun:
                    return "TaskRun.json";

                case GameModeType.RoomRush:
                    return "RoomRush.json";

                case GameModeType.TargetRush:
                    return "TargetRush.json";

                case GameModeType.DeathRun:
                    return "DeathRun.json";

                case GameModeType.FFA:
                    return "FFA.json";

                case GameModeType.FFATeam:
                    return "FFATeam.json";

                case GameModeType.KillRace:
                    return "KillRace.json";

                case GameModeType.Assassin:
                    return "Assassin.json";

                default:
                    return null;
            }
        }

        public static bool SaveGameModePreset(
            GameModeType gameMode,
            CustomOptions options)
        {
            if (options == null)
                return false;

            string fileName =
                GetGameModePresetFileName(gameMode);

            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string fullPath =
                Path.Combine(
                    "DATA",
                    "GAMEMODES_PRESETS",
                    "GAMEMODES",
                    fileName
                );

            try
            {
                string dir = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(dir) &&
                    !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (string.IsNullOrWhiteSpace(options.PresetName))
                {
                    options.PresetName =
                        GetGameModePresetName(gameMode);
                }

                string json =
                    JsonSerializer.Serialize(
                        options,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }
                    );

                File.WriteAllText(fullPath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[BanMod] Could not save {fileName}: " +
                    ex.GetType().Name + " - " + ex.Message
                );

                return false;
            }
        }

        public static CustomOptions LoadOrCreateGameModePreset(string fileName, CustomOptions defaultOptions)
        {
            string fullPath = Path.Combine("DATA", "GAMEMODES_PRESETS", "GAMEMODES", fileName);

            try
            {
                string dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(fullPath))
                {
                    string json = JsonSerializer.Serialize(defaultOptions, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(fullPath, json);
                    return defaultOptions;
                }

                string content = File.ReadAllText(fullPath);

                CustomOptions loaded =
                    JsonSerializer.Deserialize<CustomOptions>(content)
                    ?? defaultOptions;

                if (loaded.Roles == null)
                    loaded.Roles = defaultOptions.Roles ?? new RoleSettings();

                if (!HasJudgeSettings(content))
                {
                    RoleOption defaultJudge =
                        defaultOptions.Roles?.Judge;

                    loaded.Roles.Judge = new RoleOption
                    {
                        Count = defaultJudge?.Count ?? 0,
                        Chance = defaultJudge?.Chance ?? 100
                    };

                    loaded.Roles.JudgeTaskRequirementPercentage =
                        defaultOptions.Roles?.JudgeTaskRequirementPercentage
                        ?? 50f;

                    string migratedJson = JsonSerializer.Serialize(
                        loaded,
                        new JsonSerializerOptions { WriteIndented = true }
                    );

                    File.WriteAllText(fullPath, migratedJson);
                }

                return loaded;
            }
            catch (Exception)
            {
                return defaultOptions;
            }
        }

        private static bool HasJudgeSettings(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty("Roles", out JsonElement roles) ||
                    roles.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                return roles.TryGetProperty("Judge", out _) &&
                       roles.TryGetProperty("JudgeTaskRequirementPercentage", out _);
            }
            catch
            {
                return false;
            }
        }

        private static CustomOptions CreateSnSDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 3, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }

        private static CustomOptions CreatePnSDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 3, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }

        private static CustomOptions CreateZombieModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 1,
                PlayerSpeedMod = 2f,
                CrewLightMod = 1.5f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 3, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }

        private static CustomOptions CreateRoomRushModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 1,
                PlayerSpeedMod = 2f,
                CrewLightMod = 1f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 0,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 10f,
                    EngineerInVentMaxTime = 5f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }

        private static CustomOptions CreateDeathRunModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 1,
                PlayerSpeedMod = 2f,
                CrewLightMod = 1f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 0,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 10f,
                    EngineerInVentMaxTime = 5f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateTargetRushModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 1,
                PlayerSpeedMod = 2f,
                CrewLightMod = 1f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 0,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 10f,
                    EngineerInVentMaxTime = 5f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateHotPotatoDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 1,
                PlayerSpeedMod = 2f,
                CrewLightMod = 1.5f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 0.001f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = true,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 15, Chance = 100 },
                    Phantom = new RoleOption { Count = 3, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateBanModDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 17.5f,
                NumCommonTasks = 1,
                NumLongTasks = 0,
                NumShortTasks = 4,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 1, Chance = 100 },
                    Phantom = new RoleOption { Count = 1, Chance = 100 },
                    Scientist = new RoleOption { Count = 1, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 2, Chance = 100 },
                    Engineer = new RoleOption { Count = 1, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 1, Chance = 100 },
                    Tracker = new RoleOption { Count = 1, Chance = 100 },
                    Viper = new RoleOption { Count = 1, Chance = 100 },
                    Detective = new RoleOption { Count = 1, Chance = 100 },
                    Judge = new RoleOption { Count = 1, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }

        private static CustomOptions CreateKaitoRunDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 2,
                PlayerSpeedMod = 2.0f,
                CrewLightMod = 1.0f,
                ImpostorLightMod = 1.0f,
                KillCooldown = 0.1f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 1,
                NumEmergencyMeetings = 3,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)0,
                KillDistance = 0,
                EmergencyCooldown = 60,
                DiscussionTime = 0,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Phantom = new RoleOption { Count = 2, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 13, Chance = 100 },
                    Engineer = new RoleOption { Count = 7, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 3, Chance = 100 },
                    Tracker = new RoleOption { Count = 3, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 1, Chance = 100 },
                    Judge = new RoleOption { Count = 1, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 5f,
                    GuardianAngelCooldown = 0f,
                    GuardianAngelDuration = 255f,
                    ScientistCooldown = 0f,
                    ScientistBattery = 255f,
                    TrackerCooldown = 0f,
                    TrackerDelay = 0f,
                    TrackerDuration = 90f,
                    NoisemakerAlert = false,
                    NoisemakerDuration = 3f,
                    ShapeshifterLeaveSkin = true,
                    ShapeshifterCooldown = 0f,
                    ShapeshifterDuration = 255f,
                    PhantomCooldown = 5f,
                    PhantomDuration = 10f
                }
            };
        }
        private static CustomOptions CreateDefaultDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 17.5f,
                NumCommonTasks = 1,
                NumLongTasks = 0,
                NumShortTasks = 4,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 1, Chance = 100 },
                    Phantom = new RoleOption { Count = 1, Chance = 100 },
                    Scientist = new RoleOption { Count = 1, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 2, Chance = 100 },
                    Engineer = new RoleOption { Count = 1, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 1, Chance = 100 },
                    Tracker = new RoleOption { Count = 1, Chance = 100 },
                    Viper = new RoleOption { Count = 1, Chance = 100 },
                    Detective = new RoleOption { Count = 1, Chance = 100 },
                    Judge = new RoleOption { Count = 1, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateTaskRunDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 1,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 17.5f,
                NumCommonTasks = 1,
                NumLongTasks = 2,
                NumShortTasks = 4,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 0,
                VotingTime = 90,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 0, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 10f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateJBModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 2.25f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 10f,
                NumCommonTasks = 1,
                NumLongTasks = 0,
                NumShortTasks = 3,
                NumEmergencyMeetings = 2,
                AnonymousVotes = true,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 30,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 1, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 0 },
                    Scientist = new RoleOption { Count = 0, Chance = 0 },
                    GuardianAngel = new RoleOption { Count = 2, Chance = 100 },
                    Engineer = new RoleOption { Count = 3, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 2, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 0 },
                    Viper = new RoleOption { Count = 2, Chance = 100 },
                    Detective = new RoleOption { Count = 1, Chance = 100 },
                    Judge = new RoleOption { Count = 1, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 5f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 65f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 30f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 5f,
                    ShapeshifterDuration = 0f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateFFAModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 10f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 0,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 15, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 1f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateFFATEAMModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 10f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 0,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 15, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 1f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateAssassinModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 10f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 0,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 15, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 1f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        private static CustomOptions CreateKillRaceModeDefaults()
        {
            return new CustomOptions
            {
                MaxPlayers = 15,
                NumImpostors = 3,
                PlayerSpeedMod = 1.75f,
                CrewLightMod = 0.75f,
                ImpostorLightMod = 2.0f,
                KillCooldown = 10f,
                NumCommonTasks = 0,
                NumLongTasks = 0,
                NumShortTasks = 0,
                NumEmergencyMeetings = 2,
                AnonymousVotes = false,
                TaskBarMode = (AmongUs.GameOptions.TaskBarMode)1,
                KillDistance = 0,
                EmergencyCooldown = 15,
                DiscussionTime = 45,
                VotingTime = 60,
                IsDefaults = true,
                ConfirmImpostor = true,
                VisualTasks = false,
                Roles = new RoleSettings
                {
                    Shapeshifter = new RoleOption { Count = 0, Chance = 100 },
                    Phantom = new RoleOption { Count = 0, Chance = 100 },
                    Scientist = new RoleOption { Count = 0, Chance = 100 },
                    GuardianAngel = new RoleOption { Count = 0, Chance = 100 },
                    Engineer = new RoleOption { Count = 0, Chance = 100 },
                    Noisemaker = new RoleOption { Count = 0, Chance = 100 },
                    Tracker = new RoleOption { Count = 0, Chance = 100 },
                    Viper = new RoleOption { Count = 15, Chance = 100 },
                    Detective = new RoleOption { Count = 0, Chance = 100 },
                    Judge = new RoleOption { Count = 0, Chance = 100 },
                    SpiritGuide = new RoleOption { Count = 0, Chance = 100 },

                    SpiritGuideCooldownSeconds = 20f,
                    viperDissolveTime = 1f,
                    DetectiveSuspectLimit = 3f,
                    JudgeTaskRequirementPercentage = 50f,
                    EngineerCooldown = 5f,
                    EngineerInVentMaxTime = 30f,
                    GuardianAngelCooldown = 35f,
                    GuardianAngelDuration = 25f,
                    ScientistCooldown = 10f,
                    ScientistBattery = 30f,
                    TrackerCooldown = 10f,
                    TrackerDelay = 0f,
                    TrackerDuration = 30f,
                    NoisemakerAlert = true,
                    NoisemakerDuration = 10f,
                    ShapeshifterLeaveSkin = false,
                    ShapeshifterCooldown = 10f,
                    ShapeshifterDuration = 30f,
                    PhantomCooldown = 10f,
                    PhantomDuration = 30f
                }
            };
        }
        
    }


    public class PresetMenuUi : MonoBehaviour
    {
        public static PresetMenuUi Instance;

        public bool showMenu = false;

        private static readonly Vector2 WindowSize = new Vector2(760f, 680f);

        private static Rect _windowRect =
            new Rect(100f, 100f, WindowSize.x, WindowSize.y);

        private static Vector2 _scrollPosition = Vector2.zero;

        private static readonly string[] EditNames =
        {
            "Preset 1",
            "Preset 2",
            "Preset 3"
        };

        private static string _status = "";

        private static int _editingSlot = 0;
        private static GameModeType _editingGameMode =
            (GameModeType)(-1);
        private static string _renameBuffer = "";

        private static GUIStyle _titleStyle;
        private static GUIStyle _sectionStyle;
        private static GUIStyle _slotTitleStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _closeButtonStyle;
        private static GUIStyle _statusStyle;
        private static GUIStyle _boxStyle;

        private void Awake()
        {
            Instance = this;
            showMenu = false;
        }

        private void OnEnable()
        {
            MenuRouter.OnPanelChanged += HandlePanelChanged;
        }

        private void OnDisable()
        {
            MenuRouter.OnPanelChanged -= HandlePanelChanged;

            showMenu = false;
            _editingSlot = 0;
            _editingGameMode = (GameModeType)(-1);
            _renameBuffer = "";
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void HandlePanelChanged(MenuRouter.Panel panel)
        {
            showMenu =
                panel == MenuRouter.Panel.Presets;

            if (showMenu)
            {
                CenterWindow();
                RefreshNames();
                _scrollPosition = Vector2.zero;
                _status = "";
            }
            else
            {
                _editingSlot = 0;
                _editingGameMode = (GameModeType)(-1);
                _renameBuffer = "";
            }
        }

        private void Update()
        {
            if (BanMod.IsBanModDisabled)
                return;

            if (AmongUsClient.Instance == null)
                return;

            if (PlayerControl.LocalPlayer?.Data == null)
                return;

            if (!GameStates.isLobby)
                return;

            if (Options.GameMode == null ||
                Options.PresetSelection == null)
            {
                return;
            }

            if (KeyBindOptions.IsBindingActive)
                return;

            if (BanMod.chatOpen)
                return;

            if (!Input.GetKeyDown(KeyBindOptions.K15))
                return;

            if (MenuRouter.Current == MenuRouter.Panel.Presets)
                MenuRouter.Open(MenuRouter.Panel.None);
            else
                MenuRouter.Open(MenuRouter.Panel.Presets);
        }

        private void OnGUI()
        {
            if (!showMenu)
                return;

            if (AmongUsClient.Instance == null)
                return;

            if (PlayerControl.LocalPlayer?.Data == null)
                return;

            if (!GameStates.isLobby)
                return;

            EnsureStyles();

            Event currentEvent = Event.current;

            HandleRenameKeyboard(currentEvent);

            bool consumeMouse =
                currentEvent != null &&
                currentEvent.isMouse;

            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;

            _windowRect = GUI.Window(
                315,
                _windowRect,
                (GUI.WindowFunction)DrawWindow,
                "",
                BanModUiStyles.BlackWindow
            );

            GUI.backgroundColor = oldBackground;

            if (consumeMouse &&
                currentEvent != null &&
                currentEvent.type != EventType.Used)
            {
                currentEvent.Use();
            }
        }

        private static void CenterWindow()
        {
            _windowRect = new Rect(
                Screen.width / 2f - WindowSize.x / 2f,
                Screen.height / 2f - WindowSize.y / 2f,
                WindowSize.x,
                WindowSize.y
            );
        }

        private static void RefreshNames()
        {
            for (int slot = 1; slot <= 3; slot++)
            {
                EditNames[slot - 1] =
                    SetRecommendationsPatch.GetUserPresetName(slot);
            }

            SetRecommendationsPatch.UpdatePresetSelectionNames();
        }

        private static void DrawWindow(int id)
        {
            GUILayout.Label("PRESET MANAGER", _titleStyle);

            GUILayout.Space(6);

            GUILayout.Label(
                "Save the current settings into the default preset of the selected GameMode or into one of the three custom presets. " +
                "The first entry in the normal Preset option always shows the current GameMode preset name.",
                _labelStyle
            );

            GUILayout.Space(10);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawSection("USER PRESETS");

            GUILayout.Space(6);

            DrawSlot(1);
            GUILayout.Space(8);
            DrawSlot(2);
            GUILayout.Space(8);
            DrawSlot(3);

            GUILayout.Space(18);

            DrawSection("CURRENT GAME MODE PRESET");

            GUILayout.Space(6);

            GameModeType currentMode =
                SetRecommendationsPatch.GetCurrentSelectedGameMode();

            DrawCurrentGameModePreset(currentMode);

            GUILayout.Space(12);

            GUILayout.EndScrollView();

            if (!string.IsNullOrWhiteSpace(_status))
            {
                GUILayout.Space(6);
                GUILayout.Label(_status, _statusStyle);
            }

            GUILayout.Space(6);

            if (GUILayout.Button(
                "CLOSE",
                _closeButtonStyle,
                GUILayout.Height(42f)
            ))
            {
                MenuRouter.Open(MenuRouter.Panel.None);
            }

            GUI.DragWindow();
        }

        private static void DrawSection(string title)
        {
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label(title, _sectionStyle);
            GUILayout.EndVertical();
        }

        private static void DrawSlot(int slot)
        {
            int index = slot - 1;
            bool isEditing = (_editingSlot == slot);

            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label(
                $"SLOT {slot} - {EditNames[index]}",
                _slotTitleStyle
            );

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();

            string visibleName = isEditing
                ? "> " + _renameBuffer + "_"
                : EditNames[index];

            GUILayout.Label(
                visibleName,
                _labelStyle,
                GUILayout.Height(32f)
            );

            if (GUILayout.Button(
                isEditing ? "SAVE NAME" : "RENAME",
                _buttonStyle,
                GUILayout.Width(125f),
                GUILayout.Height(32f)
            ))
            {
                if (isEditing)
                    CommitRename(slot);
                else
                    BeginRename(slot);
            }

            if (isEditing)
            {
                if (GUILayout.Button(
                    "CANCEL",
                    _buttonStyle,
                    GUILayout.Width(90f),
                    GUILayout.Height(32f)
                ))
                {
                    CancelRename();
                }
            }

            GUILayout.EndHorizontal();

            if (isEditing)
            {
                GUILayout.Label(
                    "Type the new name. ENTER = save, ESC = cancel, BACKSPACE = delete.",
                    _labelStyle
                );
            }

            GUILayout.Space(6);

            bool oldEnabled = GUI.enabled;

            if (isEditing)
                GUI.enabled = false;

            if (GUILayout.Button(
                "SAVE CURRENT SETTINGS",
                _buttonStyle,
                GUILayout.Height(36f)
            ))
            {
                SaveCurrent(slot);
            }

            GUI.enabled = oldEnabled;

            GUILayout.EndVertical();
        }

        private static void DrawCurrentGameModePreset(
            GameModeType gameMode)
        {
            string modeName =
                SetRecommendationsPatch.GetDefaultGameModeName(gameMode);

            string presetName =
                SetRecommendationsPatch.GetGameModePresetName(gameMode);

            bool isEditing =
                _editingGameMode == gameMode;

            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label(
                modeName + " -> " + presetName,
                _slotTitleStyle
            );

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();

            string visibleName = isEditing
                ? "> " + _renameBuffer + "_"
                : presetName;

            GUILayout.Label(
                visibleName,
                _labelStyle,
                GUILayout.Height(32f)
            );

            if (GUILayout.Button(
                isEditing ? "SAVE NAME" : "RENAME",
                _buttonStyle,
                GUILayout.Width(125f),
                GUILayout.Height(32f)
            ))
            {
                if (isEditing)
                    CommitGameModeRename(gameMode);
                else
                    BeginGameModeRename(gameMode);
            }

            if (isEditing)
            {
                if (GUILayout.Button(
                    "CANCEL",
                    _buttonStyle,
                    GUILayout.Width(90f),
                    GUILayout.Height(32f)
                ))
                {
                    CancelRename();
                }
            }

            GUILayout.EndHorizontal();

            if (isEditing)
            {
                GUILayout.Label(
                    "Type the new name. ENTER = save, ESC = cancel, BACKSPACE = delete.",
                    _labelStyle
                );
            }

            GUILayout.Space(6);

            bool oldEnabled = GUI.enabled;

            if (isEditing)
                GUI.enabled = false;

            if (GUILayout.Button(
                "SAVE CURRENT SETTINGS",
                _buttonStyle,
                GUILayout.Height(40f)
            ))
            {
                SaveCurrentSelectedGameModePreset();
            }

            GUI.enabled = oldEnabled;

            GUILayout.EndVertical();
        }

        private static void BeginRename(int slot)
        {
            if (slot < 1 || slot > 3)
                return;

            _editingGameMode = (GameModeType)(-1);
            _editingSlot = slot;
            _renameBuffer = EditNames[slot - 1] ?? "";

            _status =
                $"Renaming slot {slot}: type the name and press ENTER.";
        }

        private static void BeginGameModeRename(
            GameModeType gameMode)
        {
            _editingSlot = 0;
            _editingGameMode = gameMode;

            _renameBuffer =
                SetRecommendationsPatch.GetGameModePresetName(gameMode);

            _status =
                $"Renaming the default preset for {SetRecommendationsPatch.GetDefaultGameModeName(gameMode)}.";
        }

        private static void CancelRename()
        {
            _editingSlot = 0;
            _editingGameMode = (GameModeType)(-1);
            _renameBuffer = "";

            RefreshNames();

            _status = "Rename cancelled.";
        }

        private static void CommitRename(int slot)
        {
            if (slot < 1 || slot > 3)
                return;

            string newName = (_renameBuffer ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                _status = "The preset name cannot be empty.";
                return;
            }

            if (newName.Length > 32)
                newName = newName.Substring(0, 32);

            if (SetRecommendationsPatch.RenameUserPreset(
                slot,
                newName
            ))
            {
                _editingSlot = 0;
                _editingGameMode = (GameModeType)(-1);
                _renameBuffer = "";

                RefreshNames();

                _status =
                    $"Slot {slot} renamed to " +
                    $"\"{SetRecommendationsPatch.GetUserPresetName(slot)}\".";
            }
            else
            {
                _status =
                    $"Could not rename slot {slot}.";
            }
        }

        private static void CommitGameModeRename(
            GameModeType gameMode)
        {
            string newName = (_renameBuffer ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                _status = "The GameMode name cannot be empty.";
                return;
            }

            if (newName.Length > 32)
                newName = newName.Substring(0, 32);

            if (SetRecommendationsPatch.RenameGameModePreset(
                gameMode,
                newName
            ))
            {
                _editingSlot = 0;
                _editingGameMode = (GameModeType)(-1);
                _renameBuffer = "";

                RefreshNames();

                _status =
                    $"Default preset for {SetRecommendationsPatch.GetDefaultGameModeName(gameMode)} " +
                    $"renamed to " +
                    $"\"{SetRecommendationsPatch.GetGameModePresetName(gameMode)}\".";
            }
            else
            {
                _status =
                    $"Could not rename " +
                    $"{SetRecommendationsPatch.GetDefaultGameModeName(gameMode)}.";
            }
        }

        private static void HandleRenameKeyboard(Event e)
        {
            bool editingUser =
                _editingSlot >= 1 &&
                _editingSlot <= 3;

            bool editingMode =
                (int)_editingGameMode >= 0 &&
                (int)_editingGameMode <= 6;

            if (!editingUser && !editingMode)
                return;

            if (e == null ||
                e.type != EventType.KeyDown)
            {
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                CancelRename();
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                if (editingUser)
                    CommitRename(_editingSlot);
                else
                    CommitGameModeRename(_editingGameMode);

                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Backspace)
            {
                if (!string.IsNullOrEmpty(_renameBuffer))
                {
                    _renameBuffer =
                        _renameBuffer.Substring(
                            0,
                            _renameBuffer.Length - 1
                        );
                }

                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Delete)
            {
                _renameBuffer = "";
                e.Use();
                return;
            }

            char c = e.character;

            if (c == '\0' ||
                char.IsControl(c))
            {
                return;
            }

            if (_renameBuffer == null)
                _renameBuffer = "";

            if (_renameBuffer.Length >= 32)
            {
                e.Use();
                return;
            }

            _renameBuffer += c;
            e.Use();
        }

        private static void SaveCurrent(int slot)
        {
            NormalGameOptionsV12 current =
                SetRecommendationsPatch.GetCurrentGameOptions();

            if (current == null)
            {
                _status =
                    "Could not read the current game settings.";
                return;
            }

            CustomOptions captured =
                SetRecommendationsPatch.CaptureFromInstance(current);

            if (captured == null)
            {
                _status =
                    "Could not create a preset from the current settings.";
                return;
            }

            string name = EditNames[slot - 1];

            if (string.IsNullOrWhiteSpace(name))
            {
                name =
                    SetRecommendationsPatch.GetUserPresetName(slot);
            }

            captured.PresetName = name.Trim();

            if (SetRecommendationsPatch.SaveUserPresetFile(
                slot,
                captured
            ))
            {
                RefreshNames();

                _status =
                    $"Saved to slot {slot}: " +
                    $"\"{SetRecommendationsPatch.GetUserPresetName(slot)}\".";
            }
            else
            {
                _status =
                    $"Could not save slot {slot}.";
            }
        }

        private static void SaveCurrentSelectedGameModePreset()
        {
            GameModeType gameMode =
                SetRecommendationsPatch.GetCurrentSelectedGameMode();

            string displayName =
                SetRecommendationsPatch.GetGameModePresetName(gameMode);

            NormalGameOptionsV12 current =
                SetRecommendationsPatch.GetCurrentGameOptions();

            if (current == null)
            {
                _status =
                    "Could not read the current game settings.";
                return;
            }

            CustomOptions captured =
                SetRecommendationsPatch.CaptureFromInstance(current);

            if (captured == null)
            {
                _status =
                    $"Could not capture the current settings for {displayName}.";
                return;
            }

            captured.PresetName = displayName;

            if (SetRecommendationsPatch.SaveGameModePreset(
                gameMode,
                captured
            ))
            {
                SetRecommendationsPatch.UpdatePresetSelectionNames();

                _status =
                    $"Saved as the default preset for {displayName} " +
                    $"({SetRecommendationsPatch.GetGameModePresetFileName(gameMode)}).";
            }
            else
            {
                _status =
                    $"Could not save the default preset for {displayName}.";
            }
        }

        private static void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle =
                new GUIStyle(BanModUiStyles.TitleLabel)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

            _sectionStyle =
                new GUIStyle(BanModUiStyles.HeaderLabel)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

            _slotTitleStyle =
                new GUIStyle(BanModUiStyles.HeaderLabel)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };

            _labelStyle =
                new GUIStyle(BanModUiStyles.BodyLabel)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };

            _buttonStyle =
                new GUIStyle(BanModUiStyles.ButtonDark)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

            _closeButtonStyle =
                new GUIStyle(BanModUiStyles.DangerButton)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

            _statusStyle =
                new GUIStyle(BanModUiStyles.MutedLabel)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };

            _boxStyle =
                new GUIStyle(BanModUiStyles.DarkBox);

            _boxStyle.padding = new RectOffset();
            _boxStyle.padding.left = 12;
            _boxStyle.padding.right = 12;
            _boxStyle.padding.top = 10;
            _boxStyle.padding.bottom = 10;
        }
    }


    [HarmonyPatch(typeof(Options), nameof(Options.Load))]
    public static class PresetNamesOptionsLoadPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (BanMod.IsBanModDisabled)
                    return;

                if (AmongUsClient.Instance == null)
                    return;

                if (PlayerControl.LocalPlayer?.Data == null)
                    return;

                if (!GameStates.isLobby)
                    return;

                if (Options.GameMode == null ||
                    Options.PresetSelection == null)
                {
                    return;
                }

                SetRecommendationsPatch.UpdatePresetSelectionNames();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BanMod] PresetNamesOptionsLoadPatch skipped: " +
                    ex.GetType().Name + " - " + ex.Message
                );
            }
        }
    }

    [HarmonyPatch(typeof(Options), nameof(Options.ReOpenSettings))]
    public static class PresetNamesGameModeChangedPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (BanMod.IsBanModDisabled)
                    return;

                if (AmongUsClient.Instance == null)
                    return;

                if (PlayerControl.LocalPlayer?.Data == null)
                    return;

                if (!GameStates.isLobby)
                    return;

                if (Options.GameMode == null ||
                    Options.PresetSelection == null)
                {
                    return;
                }

                SetRecommendationsPatch.UpdatePresetSelectionNames();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[BanMod] PresetNamesGameModeChangedPatch skipped: " +
                    ex.GetType().Name + " - " + ex.Message
                );
            }
        }
    }

}
