
//credits and licenses in the resources folder
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static BanMod.BanMod;
using GameStates = BanMod.GameStates;

namespace BanMod
{
    public static class TaskVisibilityController
    {
        private static readonly Dictionary<MapNames, HashSet<string>> MapTaskNames = new()
        {
            {
                MapNames.Skeld,
                new HashSet<string>
                {
                    "DisableFixWiring",
                    "DisableSwipeCard",
                    "DisableChartCourse",
                    "DisableStabilizeSteering",
                    "DisableCleanO2Filter",
                    "DisablePrimeShields",
                    "DisableAlignEngineOutput",
                    "DisableEmptyChute",
                    "DisableClearAsteroids",
                    "DisableEmptyGarbage",
                    "DisableDivertPower",
                    "DisableSubmitScan",
                    "DisableStartReactor",
                    "DisableInspectSample",
                    "DisableUploadData",
                    "DisableFuelEngines"
                }
            },
            {
                MapNames.MiraHQ,
                new HashSet<string>
                {
                    "DisableFixWiring",
                    "DisableEnterIdCode",
                    "DisableCleanO2Filter",
                    "DisableProcessData",
                    "DisableDecontaminate",
                    "DisableWaterPlants",
                    "DisableReplaceWaterJug",
                    "DisableDivertPower",
                    "DisableSubmitScan",
                    "DisableRebootWifi",
                    "DisableUploadData"
                }
            },
            {
                MapNames.Polus,
                new HashSet<string>
                {
                    "DisableFixWiring",
                    "DisableSwipeCard",
                    "DisableInsertKeys",
                    "DisableScanBoardingPass",
                    "DisableUnlockManifolds",
                    "DisableMeasureWeather",
                    "DisableAssembleArtifact",
                    "DisableSortSamples",
                    "DisableRunDiagnostics",
                    "DisableRepairDrill",
                    "DisableAlignTelescope",
                    "DisableRecordTemperature",
                    "DisableEmptyChute",
                    "DisableClearAsteroids",
                    "DisableSubmitScan",
                    "DisableActivateWeatherNodes"
                }
            },
            {
                MapNames.Airship,
                new HashSet<string>
                {
                    "DisableFixWiring",
                    "DisableStoreArtifacts",
                    "DisablePutAwayPistols",
                    "DisablePutAwayRifles",
                    "DisableCleanToilet",
                    "DisableSortRecords",
                    "DisableFixShower",
                    "DisablePickUpTowels",
                    "DisablePolishRuby",
                    "DisableDressMannequin",
                    "DisableUnlockSafe",
                    "DisableResetBreaker",
                    "DisableEmptyGarbage",
                    "DisableSubmitScan",
                    "DisableOpenWaterways",
                    "DisableUploadData"
                }
            },
            {
                MapNames.Fungle,
                new HashSet<string>
                {
                    "DisableFixWiring",
                    "DisableBuyBeverage",
                    "DisableMonitorTree",
                    "DisableMakeBurger",
                    "DisableCleanToilet",
                    "DisableDevelopPhotos",
                    "DisableRewindTapes",
                    "DisableStartFans",
                    "DisableRoastMarshmallow",
                    "DisableFindSignal",
                    "DisableThrowFisbee",
                    "DisableLiftWeights",
                    "DisableCollectShells",
                    "DisableSubmitScan",
                    "DisableUploadData"
                }
            }
        };

        public static void UpdateTaskVisibility()
        {
            UpdateTaskVisibility(GetActiveMapName());
        }

        public static void UpdateTaskVisibility(MapNames currentMap)
        {
            if (currentMap < 0)
                return;

            if (!MapTaskNames.TryGetValue(currentMap, out var visibleNames))
                return;

            foreach (var task in OptionItem.ShortOptions)
                task.SetEnabled(visibleNames.Contains(task.Name));

            foreach (var task in OptionItem.CommonOptions)
                task.SetEnabled(visibleNames.Contains(task.Name));

            foreach (var task in OptionItem.LongOptions)
                task.SetEnabled(visibleNames.Contains(task.Name));

            BMLogger.Info(
                $"Visibilità task aggiornata per mappa: {currentMap} ({visibleNames.Count} visibili)",
                "TaskVisibility"
            );
        }

        public static MapNames GetActiveMapName()
        {
            if (GameOptionsManager.Instance == null ||
                GameOptionsManager.Instance.CurrentGameOptions == null)
            {
                return (MapNames)(-1);
            }

            return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
        }
    }
    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            taskOptionsLoad = Task.Run(() =>
            {
                try
                {
                    Load();
                }
                catch (System.Exception)
                {
                }
            });
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        public static void WaitOptionsLoad()
        {
            taskOptionsLoad.Wait();
        }
        public static OptionItem nocountdown;
        public static OptionItem randomMap;
        public static OptionItem Protection10Sec;
        public static OptionItem ShareLobbyCode;
        public static OptionItem NoKillMeeting;
        public static OptionItem ChatLeft;
        public static OptionItem AllowColorChangeAll;
        public static OptionItem AllowColorChangeModerator;
        public static OptionItem DisableRole;
        public static OptionItem DisableMeetingsAndReports;
        //public static OptionItem TrackImpostorTeammate;
        //public static OptionItem revealVotes;
        public static OptionItem EnableGameTimer;
        public static OptionItem PauseGameTimerDuringMeetings;
        public static FloatOptionItem GameTimerMinutes;
        public static OptionItem GameTimerMessage;
        public static OptionItem DisableDeviceCam;
        public static OptionItem DisableDeviceAdminPanel;
        public static OptionItem DisableDeviceVitals;

        public static OptionItem EnableDetector;
        public static OptionItem EnableAfkKick;
        public static OptionItem EnableShield;
        public static IntegerOptionItem TimeToActivate;
        public static IntegerOptionItem DetectionDelay;
        public static OptionItem EnableCamDetector;
        public static OptionItem EnableCamKick;
        public static IntegerOptionItem TimeToCamActivate;
        public static IntegerOptionItem DetectionCamDelay;
        public static IntegerOptionItem MaxCam;
        public static OptionItem EnableCamTaskDetector;
        public static OptionItem EnableCamTaskKick;
        public static IntegerOptionItem TimeToCamTaskActivate;
        public static IntegerOptionItem DetectionCamTaskDelay;
        public static IntegerOptionItem MinTasksToUseCamCrew;
        public static IntegerOptionItem MinTasksToUseCamImp;
        public static IntegerOptionItem MinKillsToUseCamImp;
        public static StringOptionItem ImpostorCamCondition;
        public static OptionItem EnableProximityMonitor;
        public static IntegerOptionItem ProximityDistance;
        public static IntegerOptionItem ProximityTimeSeconds;
        public static StringOptionItem ProximityAction;
        public static OptionItem AutoKickStopWords;
        public static StringOptionItem AutoKickStopWordsAction;
        public static IntegerOptionItem AutoKickStopWordsTimes;
        public static OptionItem SendAutoKickStopWordsMsg;
        public static OptionItem AutoKickStart;
        public static StringOptionItem AutoKickStartAction;
        public static IntegerOptionItem AutoKickStartTimes;
        public static OptionItem SendAutoKickStartMsg;
        public static OptionItem ApplyDenyNameList;
        public static OptionItem CheckBanList;
        public static OptionItem CheckBlockList;
        public static OptionItem CheckFriendCode;
        public static OptionItem KickLevel;
        public static IntegerOptionItem KickLevelLevel;
        public static IntegerOptionItem SubmitScanMax;
        public static OptionItem DisableCleanVent;
        public static OptionItem SharedAllTasks;
        public static OptionItem DisableCalibrateDistributor;
        public static OptionItem DisableChartCourse;
        public static OptionItem DisableStabilizeSteering;
        public static OptionItem DisableCleanO2Filter;
        public static OptionItem DisableUnlockManifolds;
        public static OptionItem DisablePrimeShields;
        public static OptionItem DisableMeasureWeather;
        public static OptionItem DisableBuyBeverage;
        public static OptionItem DisableAssembleArtifact;
        public static OptionItem DisableSortSamples;
        public static OptionItem DisableProcessData;
        public static OptionItem DisableRunDiagnostics;
        public static OptionItem DisableRepairDrill;
        public static OptionItem DisableAlignTelescope;
        public static OptionItem DisableRecordTemperature;
        public static OptionItem DisableFillCanisters;
        public static OptionItem DisableMonitorTree;
        public static OptionItem DisableStoreArtifacts;
        public static OptionItem DisablePutAwayPistols;
        public static OptionItem DisablePutAwayRifles;
        public static OptionItem DisableMakeBurger;
        public static OptionItem DisableCleanToilet;
        public static OptionItem DisableDecontaminate;
        public static OptionItem DisableSortRecords;
        public static OptionItem DisableFixShower;
        public static OptionItem DisablePickUpTowels;
        public static OptionItem DisablePolishRuby;
        public static OptionItem DisableDressMannequin;
        public static OptionItem DisableSwipeCard;
        public static OptionItem DisableFixWiring;
        public static OptionItem DisableEnterIdCode;
        public static OptionItem DisableInsertKeys;
        public static OptionItem DisableScanBoardingPass;
        public static OptionItem DisableSubmitScan;
        public static OptionItem DisableUnlockSafe;
        public static OptionItem DisableStartReactor;
        public static OptionItem DisableResetBreaker;
        public static OptionItem DisableAlignEngineOutput;
        public static OptionItem DisableInspectSample;
        public static OptionItem DisableEmptyChute;
        public static OptionItem DisableClearAsteroids;
        public static OptionItem DisableWaterPlants;
        public static OptionItem DisableOpenWaterways;
        public static OptionItem DisableReplaceWaterJug;
        public static OptionItem DisableRebootWifi;
        public static OptionItem DisableDevelopPhotos;
        public static OptionItem DisableRewindTapes;
        public static OptionItem DisableStartFans;
        public static OptionItem DisableUploadData;
        public static OptionItem DisableEmptyGarbage;
        public static OptionItem DisableFuelEngines;
        public static OptionItem DisableDivertPower;
        public static OptionItem DisableActivateWeatherNodes;
        public static OptionItem DisableRoastMarshmallow;
        public static OptionItem DisableCollectSamples;
        public static OptionItem DisableReplaceParts;
        public static OptionItem DisableCollectVegetables;
        public static OptionItem DisableMineOres;
        public static OptionItem DisableExtractFuel;
        public static OptionItem DisableCatchFish;
        public static OptionItem DisablePolishGem;
        public static OptionItem DisableHelpCritter;
        public static OptionItem DisableHoistSupplies;
        public static OptionItem DisableFixAntenna;
        public static OptionItem DisableBuildSandcastle;
        public static OptionItem DisableCrankGenerator;
        public static OptionItem DisableMonitorMushroom;
        public static OptionItem DisablePlayVideoGame;
        public static OptionItem DisableFindSignal;
        public static OptionItem DisableThrowFisbee;
        public static OptionItem DisableLiftWeights;
        public static OptionItem DisableCollectShells;
        public static OptionItem DisableUnknown;
        public static IntegerOptionItem AutoVoteTime;
        //public static OptionItem AutoVote;
        public static StringOptionItem AutoVoteAction;
        public static OptionItem sendInfocomand;
        //public static OptionItem buttonvisibile;
        public static OptionItem EngineerFixer;
        public static StringOptionItem ExilerAction;
        public static StringOptionItem KickLevelAction;
        public static StringOptionItem ActionTeamers;
        public static StringOptionItem GuesserAction;
        public static OptionItem ResetDoorsEveryTurns;
        public static StringOptionItem DoorsResetMode;
        public static IntegerOptionItem VentTimes;
        public static OptionItem sendwelcome;
        public static OptionItem ScientistTime;
        public static OptionItem Guess;
        public static OptionItem Jester;
        public static OptionItem JesterVent;
        public static OptionItem PhantomGuess;
        public static OptionItem ShapeGuess;
        public static OptionItem ViperGuess;
        public static OptionItem ImpostorGuess;
        public static OptionItem ExilerExe;
        public static OptionItem killexiler;
        public static OptionItem SendSummary;
        public static OptionItem Immortalesentvote;
        public static OptionItem aktive_notimmplayer;
        public static OptionItem ProtectFirstDead;
        public static OptionItem EnableImmortal;
        public static OptionItem sendtoimmortal;
        public static OptionItem sendtoAll;
        public static OptionItem BlockSwitches;
        public static OptionItem Enablesabotage;
        public static OptionItem DisableAllSabotages;
        public static OptionItem DisableReactorSabotage;
        public static OptionItem DisableCommsSabotage;
        public static OptionItem DisableO2Sabotage;
        public static OptionItem DisableElectricalSabotage;
        public static OptionItem DisableLaboratorySabotage;
        public static OptionItem DisableHeliSabotage;
        public static OptionItem DisableMushroomSabotage;
        public static OptionItem DisableDoorSabotage;
        public static OptionItem DisableDeadImpostorSabotage;
        public static IntegerOptionItem FloatSismic;
        public static IntegerOptionItem FloatReactor;
        public static IntegerOptionItem FloatCrashCourse;
        public static OptionItem EnableAntiCheat;
        public static StringOptionItem ActionCheater;
        public static OptionItem SentWarning;
        public static OptionItem AutoStart;
        public static IntegerOptionItem AutoStartTime;
        public static IntegerOptionItem AutoStartCount;
        public static IntegerOptionItem AutoStartDelay;
        public static OptionItem AutoRejoin;
        public static OptionItem ClooseDoorsCheat;
        public static OptionItem CompleteTaskCheat;
        public static OptionItem KickVentCheat;
        public static OptionItem SabotageCheat2;
        public static OptionItem UseVentCheat;
        public static GameModeOptionItem GameMode;
        public static StringOptionItem PresetSelection;
        public static OptionItem TcommandforAll;
        public static OptionItem MisfiresToSuicideSns;
        public static OptionItem CantKillTimeSns;
        public static OptionItem MisfiresToSuicidePns;
        public static OptionItem CantKillTimePns;
        public static OptionItem ProtectFirstHost;
        public static IntegerOptionItem NumSeekers;
        public static IntegerOptionItem DecontaminationTime;
        public static OptionItem MoreImp;
        public static IntegerOptionItem NumImpostor;
        public static IntegerOptionItem SabotageCooldown;
        public static IntegerOptionItem FfaVentMaxSeconds;
        public static StringOptionItem FFAVentTeleportMode;
        public static IntegerOptionItem FfaVentMaxSeconds1;
        public static StringOptionItem FFAVentTeleportMode1;
        public static List<StringOptionItem> SeekerSelections = new List<StringOptionItem>();
        public static OptionItem DisableDevice;
        public static bool ForceOwnLanguage = true;
        public static StringOptionItem ProfilerHintMode;
        public static StringOptionItem ProtectFirstPlayer;
        public static StringOptionItem FfaKillerRole;
        public static StringOptionItem FfaTeamKillerRole;
        public static StringOptionItem KillRaceKillerRole;
        public static StringOptionItem AssassinKillerRole;
        public static StringOptionItem FfaTeamCount;
        public static IntegerOptionItem HotPotatoModdedFirstDelaySeconds;
        public static IntegerOptionItem HotPotatoModdedExplosionSeconds;
        public static IntegerOptionItem KillRaceDurationSeconds;
        public static IntegerOptionItem KillRaceRespawnSeconds;
        public static OptionItem RandomVentSpawn;
        public static OptionItem RandomVentSpawnffa;
        public static OptionItem NoVent;
        public static OptionItem RandomVentSpawn1;
        public static OptionItem RandomVentSpawnffa1;
        public static OptionItem NoVent1;
        public static IntegerOptionItem ZombieTouchesToInfect;
        public static FloatOptionItem ZombieTouchDurationSeconds;
        public static IntegerOptionItem ZombieInitialInfectionDelay;
        public static FloatOptionItem ZombieSpeedMultiplier;
        public static FloatOptionItem ZombieVisionMultiplier;
        public static IntegerOptionItem HotPotatoExplosionSeconds;
        public static IntegerOptionItem HotPotatoFirstDelaySeconds;
        public static StringOptionItem LobbyGameMode;
        public static OptionItem TagName;
        public static OptionItem TagGameMod;
        public static bool IsLoaded = false;
        private static bool _reOpenSettingsScheduled = false;
        private static bool syncingFirstSeeker = false;
        //public static OptionItem AutoMuteForDiscord;
        public static OptionItem EnableFFA;
        public static OptionItem EnableSNS;
        public static IntegerOptionItem Shapetimer;
        public static IntegerOptionItem SnSPunizione;
        public static IntegerOptionItem SnSError;
        public static OptionItem EnableAllChat;
        public static OptionItem NokillComsSNS;
        public static OptionItem NokillComsPNS;
        public static OptionItem EnableAntiBotSystem;
        public static bool IsZombieMode => GameMode != null && GameMode.Selected == GameModeType.ZombieMode;

        public static readonly GameModeType[] GameModeOrder =
        {
            GameModeType.Default,
            GameModeType.KaitoRun,
            GameModeType.TaskRun,
            GameModeType.SnS,
            GameModeType.PnS,
            GameModeType.ZombieMode,
            GameModeType.RoomRush,
            GameModeType.TargetRush,
            GameModeType.DeathRun,
            GameModeType.FFA,
            GameModeType.FFATeam,
            GameModeType.HotPotatoModded,
            GameModeType.Assassin,
            GameModeType.KillRace
        };
        public static readonly GameModeType[] VanillaGameModeOrder =
        {
            GameModeType.Default,
            GameModeType.KaitoRun,
            GameModeType.TaskRun,
            GameModeType.SnS,
            GameModeType.PnS,
            GameModeType.ZombieMode,
            GameModeType.FFA,
            GameModeType.FFATeam
        };
        public static void UpdateGameModesForServer()
        {
            if (GameMode == null)
                return;

            GameModeType[] modes =
                BanModServerSelection.IsModded25
                    ? GameModeOrder
                    : VanillaGameModeOrder;

            GameMode.SetGameModes(
                modes,
                true
            );

            FfaExternalBridge.SyncGameMode();

            if (GameStates.isLobby)
                ReOpenSettings();
        }

        private static bool syncingLobbyGameMode = false;

        public static void SyncLobbyGameModeFromVanilla()
        {
            if (LobbyGameMode == null)
                return;

            var manager = GameOptionsManager.Instance;

            if (manager == null ||
                manager.CurrentGameOptions == null)
            {
                return;
            }

            int wantedValue =
                manager.CurrentGameOptions.GameMode == GameModes.HideNSeek
                    ? 1
                    : 0;

            if (LobbyGameMode.GetInt() == wantedValue)
                return;

            try
            {
                syncingLobbyGameMode = true;

                LobbyGameMode.SetValue(wantedValue, false);
            }
            finally
            {
                syncingLobbyGameMode = false;
            }
        }

        public static void Load()
        {
            if (IsLoaded) return;
            LobbyGameMode = (StringOptionItem)StringOptionItem.Create("LobbyGameMode", new[] { "Classic", "Hide N Seek" }, 0, OptionCategory.GameMode1, false, false).SetColor(new Color32(255, 80, 80, 255));
            LobbyGameMode.RegisterUpdateValueEvent((sender, args) =>
            {
                if (syncingLobbyGameMode)
                    return;

                if (!GameStates.isLobby)
                    return;

                if (AmongUsClient.Instance == null ||
                    !AmongUsClient.Instance.AmHost)
                {
                    return;
                }

                int value = LobbyGameMode.GetInt();

                GameModes target =
                    value == 0
                        ? GameModes.Normal
                        : GameModes.HideNSeek;

                if (GameOptionsManager.Instance?.CurrentGameOptions != null &&
                    GameOptionsManager.Instance.CurrentGameOptions.GameMode == target)
                {
                    return;
                }

                if (HudManager.Instance != null)
                {
                    HudManager.Instance.StartCoroutine(
                        LobbyGameModeOptionPatch.SwitchGameMode(target)
                    );
                }
            });
            EnableFFA = BooleanOptionItem.Create("EnableFFA", false, OptionCategory.Seeker, false).SetColor(new Color32(0, 153, 255, 255));
            EnableSNS = BooleanOptionItem.Create("EnableSNS", false, OptionCategory.Seeker, false).SetColor(new Color32(0, 153, 255, 255));
            Shapetimer = (IntegerOptionItem)IntegerOptionItem.Create("Shapetimer", new(3, 120, 1), 10, OptionCategory.Seeker, true).SetParent(EnableSNS).SetColor(new Color32(255, 128, 0, 255));
            SnSPunizione = (IntegerOptionItem)IntegerOptionItem.Create("SnSPunizione", new(0, 120, 5), 30, OptionCategory.Seeker, true).SetParent(EnableSNS).SetColor(new Color32(255, 128, 0, 255));
            SnSError = (IntegerOptionItem)IntegerOptionItem.Create("SnSError", new(0, 2, 1), 1, OptionCategory.Seeker, true).SetParent(EnableSNS).SetColor(new Color32(255, 128, 0, 255));
            bool syncingFfaSns = false;

            EnableFFA.RegisterUpdateValueEvent((sender, args) =>
            {
                if (syncingFfaSns)
                    return;

                if (EnableFFA.GetBool())
                {
                    syncingFfaSns = true;
                    EnableSNS.SetValue(0, false);
                    syncingFfaSns = false;
                }

                ReOpenSettings();
            });

            EnableSNS.RegisterUpdateValueEvent((sender, args) =>
            {
                if (syncingFfaSns)
                    return;

                if (EnableSNS.GetBool())
                {
                    syncingFfaSns = true;
                    EnableFFA.SetValue(0, false);
                    syncingFfaSns = false;
                }

                ReOpenSettings();
            });
            NumSeekers = (IntegerOptionItem)IntegerOptionItem.Create("NumSeekers", new(1, 14, 1), 1, OptionCategory.Seeker, true).SetColor(new Color32(0, 153, 255, 255));
            SeekerSelections.Clear();

            for (int i = 1; i <= 14; i++)
            {
                var opt = (StringOptionItem)StringOptionItem.Create(
                    $"SetSeeker {i}",
                    new[] { "Round-robin" },
                    0,
                    OptionCategory.Seeker,
                    true,
                    false
                )
                .SetColor(new Color32(0, 153, 255, 255));

                SeekerSelections.Add(opt);
            }
            SeekerSelections[0].RegisterUpdateValueEvent((sender, args) =>
            {
                if (syncingFirstSeeker)
                    return;

                try
                {
                    if (AmongUsClient.Instance == null ||
                        !AmongUsClient.Instance.AmHost)
                        return;

                    if (!GameStates.isHideNSeek)
                        return;

                    if (GameData.Instance == null)
                        return;

                    var hnsOptions =
                        GameOptionsManager.Instance?
                            .CurrentGameOptions?
                            .Cast<HideNSeekGameOptionsV12>();

                    if (hnsOptions == null)
                        return;

                    syncingFirstSeeker = true;

                    string selectedName =
                        ((StringOptionItem)sender).GetString();

                    if (string.IsNullOrWhiteSpace(selectedName) ||
                        selectedName == "Round-robin")
                    {
                        hnsOptions.ImpostorPlayerID = -1;
                        return;
                    }

                    foreach (var player in GameData.Instance.AllPlayers)
                    {
                        if (player == null ||
                            player.Disconnected)
                            continue;

                        if (player.PlayerName == selectedName)
                        {
                            hnsOptions.ImpostorPlayerID =
                                player.PlayerId;

                            break;
                        }
                    }
                }
                finally
                {
                    syncingFirstSeeker = false;
                }
            });
            void UpdateSeekerOptions()
            {
                int current = NumSeekers.GetInt();

                for (int i = 0; i < SeekerSelections.Count; i++)
                    SeekerSelections[i].SetEnabled(i < current);
            }

            NumSeekers.RegisterUpdateValueEvent((sender, args) =>
            {
                UpdateSeekerOptions();
            });

            UpdateSeekerOptions();

            GameMode = new GameModeOptionItem(
                "GameMode",
                GameModeOrder,
                GameModeType.Default,
                OptionCategory.GameMode,
                true,
                true,
                BanMod.ApplyPresetAutomatically
            );

            GameMode.SetColor(new Color32(255, 204, 0, 255));
            UpdateGameModesForServer();

            GameMode.RegisterUpdateValueEvent((sender, args) =>
            {
                FfaExternalBridge.SyncGameMode();
                ReOpenSettings();
            });

            PresetSelection = (StringOptionItem)StringOptionItem.Create("Preset", new[] { "Default", "Preset1", "Preset2", "Preset3" }, 0,
                OptionCategory.GameMode,
                true,
                false,
                BanMod.ApplyPresetAutomatically
            ).SetColor(new Color32(255, 204, 0, 255));

            PresetSelection.RegisterUpdateValueEvent((sender, args) =>
            {
                FfaExternalBridge.SyncAll();
                ReOpenSettings();
            });

            RandomVentSpawnffa = BooleanOptionItem.Create("RandomVentSpawn", false, OptionCategory.FFA, true).SetColor(new Color32(0, 153, 255, 255));
            NoVent = BooleanOptionItem.Create("BlockVent", false, OptionCategory.FFA, false).SetColor(new Color32(0, 153, 255, 255));
            FfaVentMaxSeconds = (IntegerOptionItem)IntegerOptionItem.Create("FfaVentMaxSeconds", new(1, 30, 1), 5, OptionCategory.FFA, true).SetColor(new Color32(0, 153, 255, 255));
            FFAVentTeleportMode = (StringOptionItem)StringOptionItem.Create("FFAVentTeleportMode", new[] { "Always", "RandomEvery15Seconds", "Never" }, 0, OptionCategory.FFA, true, true).SetColor(new Color32(255, 204, 0, 255));
            FfaKillerRole = (StringOptionItem)StringOptionItem.Create("FfaKillerRole", new[] { "Viper", "Impostor" }, 0, OptionCategory.FFA, true, false).SetColor(new Color32(255, 204, 0, 255));

            void UpdateFfaVentOptions(bool noVent)
            {
                FfaVentMaxSeconds.SetEnabled(!noVent);
                FFAVentTeleportMode.SetEnabled(!noVent);
            }

            UpdateFfaVentOptions(NoVent.GetBool());

            NoVent.RegisterUpdateValueEvent((sender, args) =>
            {
                bool noVent = ((OptionItem)sender).GetBool();
                UpdateFfaVentOptions(noVent);
                FfaExternalBridge.SyncAll();
                ReOpenSettings();
            });

            FfaVentMaxSeconds.RegisterUpdateValueEvent((sender, args) =>
            {
                FfaExternalBridge.SyncAll();
            });

            FFAVentTeleportMode.RegisterUpdateValueEvent((sender, args) =>
            {
                FfaExternalBridge.SyncAll();
            });

            KillRaceKillerRole = (StringOptionItem)StringOptionItem.Create("FfaKillerRole", new[] { "Viper", "Impostor" }, 0, OptionCategory.KillRace, true, false).SetColor(new Color32(255, 204, 0, 255));
            AssassinKillerRole = (StringOptionItem)StringOptionItem.Create("FfaKillerRole", new[] { "Viper", "Impostor" }, 0, OptionCategory.Assassin, true, false).SetColor(new Color32(255, 204, 0, 255));

            FfaTeamCount = (StringOptionItem)StringOptionItem.Create("FfaTeamCount", new[] { "2 Team", "3 Team", "4 Team", "5 Team" }, 0, OptionCategory.FFATEAM, true, false ).SetColor(new Color32(255, 80, 80, 255));
            RandomVentSpawnffa1 = BooleanOptionItem.Create("RandomVentSpawn", false, OptionCategory.FFATEAM, true).SetColor(new Color32(0, 153, 255, 255));
            NoVent1 = BooleanOptionItem.Create("BlockVent", false, OptionCategory.FFATEAM, false).SetColor(new Color32(0, 153, 255, 255));
            FfaVentMaxSeconds1 = (IntegerOptionItem)IntegerOptionItem.Create("FfaVentMaxSeconds", new(1, 30, 1), 5, OptionCategory.FFATEAM, true).SetColor(new Color32(0, 153, 255, 255));
            FFAVentTeleportMode1 = (StringOptionItem)StringOptionItem.Create("FFAVentTeleportMode", new[] { "Always", "RandomEvery15Seconds", "Never" }, 0, OptionCategory.FFATEAM, true, true).SetColor(new Color32(255, 204, 0, 255));
            FfaTeamKillerRole = (StringOptionItem)StringOptionItem.Create("FfaKillerRole", new[] { "Viper", "Impostor" }, 0, OptionCategory.FFATEAM, true, false).SetColor(new Color32(255, 204, 0, 255));

            void UpdateFfaVentOptions1(bool noVent1)
            {
                FfaVentMaxSeconds1.SetEnabled(!noVent1);
                FFAVentTeleportMode1.SetEnabled(!noVent1);
            }

            UpdateFfaVentOptions1(NoVent1.GetBool());

            NoVent1.RegisterUpdateValueEvent((sender, args) =>
            {
                bool noVent1 = ((OptionItem)sender).GetBool();
                UpdateFfaVentOptions1(noVent1);
                FfaExternalBridge.SyncAll();
                ReOpenSettings();
            });

            FfaVentMaxSeconds1.RegisterUpdateValueEvent((sender, args) =>
            {
                FfaExternalBridge.SyncAll();
            });

            FFAVentTeleportMode1.RegisterUpdateValueEvent((sender, args) =>
            {
                FfaExternalBridge.SyncAll();
            });

            HotPotatoModdedFirstDelaySeconds = (IntegerOptionItem)IntegerOptionItem.Create("HotPotatoFirstDelaySeconds", new(0, 30, 1), 3, OptionCategory.HotPotato, true).SetColor(new Color32(255, 128, 0, 255));

            HotPotatoModdedExplosionSeconds = (IntegerOptionItem)IntegerOptionItem.Create("HotPotatoExplosionSeconds", new(5, 120, 5), 15, OptionCategory.HotPotato, true).SetColor(new Color32(255, 128, 0, 255));

            KillRaceDurationSeconds = (IntegerOptionItem)IntegerOptionItem.Create("KillRaceDurationSeconds",new(30, 1800, 30),180,OptionCategory.KillRace, true).SetColor(new Color32(0, 153, 255, 255));

            KillRaceRespawnSeconds = (IntegerOptionItem)IntegerOptionItem.Create("KillRaceRespawnSeconds", new(1, 15, 1), 3, OptionCategory.KillRace, true).SetColor(new Color32(0, 153, 255, 255));


            void UpdateExternalFfaOptions()
            {
                GameModeType selected =
                    GameMode.Selected;

                bool HnS =
                    Options.EnableFFA.GetBool();

                bool ffa =
                    selected == GameModeType.FFA;

                bool team =
                    selected == GameModeType.FFATeam;

                bool hpModded =
                    selected == GameModeType.HotPotatoModded;

                bool assassin =
                    selected == GameModeType.Assassin;

                bool killRace =
                    selected == GameModeType.KillRace;

                bool hotpotato =
                    selected == GameModeType.HotPotatoModded;

                FfaKillerRole.SetEnabled(
                    ffa 
                );

                FfaTeamKillerRole.SetEnabled(
                    team
                );

                AssassinKillerRole.SetEnabled(
                    assassin
                );

                KillRaceKillerRole.SetEnabled(
                    killRace
                );

                FfaTeamCount.SetEnabled(
                    team
                );

                HotPotatoModdedFirstDelaySeconds.SetEnabled(
                    hpModded
                );

                HotPotatoModdedExplosionSeconds.SetEnabled(
                    hpModded
                );

                KillRaceDurationSeconds.SetEnabled(
                    killRace
                );

                KillRaceRespawnSeconds.SetEnabled(
                    killRace
                );
            }

            FfaKillerRole.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            FfaTeamKillerRole.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            KillRaceKillerRole.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            AssassinKillerRole.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            FfaTeamCount.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            HotPotatoModdedFirstDelaySeconds.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            HotPotatoModdedExplosionSeconds.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            KillRaceDurationSeconds.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            KillRaceRespawnSeconds.RegisterUpdateValueEvent(
                (sender, args) =>
                    FfaExternalBridge.SyncAll(true)
            );

            GameMode.RegisterUpdateValueEvent(
                (sender, args) =>
                {
                    UpdateExternalFfaOptions();
                    FfaExternalBridge.SyncAll(true);
                }
            );

            UpdateExternalFfaOptions();


            MisfiresToSuicideSns = (IntegerOptionItem)IntegerOptionItem.Create("SuicideAfterMisfiresAmount", new(1, 10, 1), 2, OptionCategory.SNS, false).SetColor(new Color32(0, 153, 255, 255));
            CantKillTimeSns = (IntegerOptionItem)IntegerOptionItem.Create("MisfireKillCooldown", new(0, 60, 5), 20, OptionCategory.SNS, false).SetColor(new Color32(0, 153, 255, 255));
            NokillComsSNS = BooleanOptionItem.Create("NokillComs", false, OptionCategory.SNS, true).SetColor(new Color32(0, 153, 255, 255));

            MisfiresToSuicidePns = (IntegerOptionItem)IntegerOptionItem.Create("SuicideAfterMisfiresAmountPns", new(1, 10, 1), 2, OptionCategory.PNS, false).SetColor(new Color32(0, 153, 255, 255));
            CantKillTimePns = (IntegerOptionItem)IntegerOptionItem.Create("MisfireKillCooldownPns", new(0, 60, 5), 20, OptionCategory.PNS, false).SetColor(new Color32(0, 153, 255, 255));
            NokillComsPNS = BooleanOptionItem.Create("NokillComs", false, OptionCategory.PNS, true).SetColor(new Color32(0, 153, 255, 255));

            ZombieInitialInfectionDelay = (IntegerOptionItem)IntegerOptionItem.Create("ZombieInitialInfectionDelay",new(5, 180, 5),30,OptionCategory.Zombie,true).SetColor(new Color32(0, 153, 255, 255));
            ZombieSpeedMultiplier = (FloatOptionItem)FloatOptionItem.Create("ZombieSpeedMultiplier",new(0f, 100f, 5f),90f,OptionCategory.Zombie,true).SetColor(new Color32(0, 153, 255, 255));
            ZombieVisionMultiplier = (FloatOptionItem)FloatOptionItem.Create("ZombieVisionMultiplier",new(0f, 100f, 5f),90f,OptionCategory.Zombie,true).SetColor(new Color32(0, 153, 255, 255));
            ZombieTouchesToInfect = (IntegerOptionItem)IntegerOptionItem.Create("ZombieTouchesToInfect",new(1, 10, 1),1,OptionCategory.Zombie,true).SetColor(new Color32(0, 153, 255, 255));
            ZombieTouchDurationSeconds = (FloatOptionItem)FloatOptionItem.Create("ZombieTouchDurationSeconds", new(0.5f, 10f, 0.5f),1f,OptionCategory.Zombie,true).SetColor(new Color32(0, 153, 255, 255));

            nocountdown = BooleanOptionItem.Create("nocountdown", false, OptionCategory.Lobby, true).SetColor(new Color32(0, 153, 255, 255));
            ShareLobbyCode = BooleanOptionItem.Create("ShareLobbyCode", false, OptionCategory.Lobby, true).SetColor(new Color32(0, 153, 255, 255));
            AutoRejoin = BooleanOptionItem.Create("AutoRejoin", false, OptionCategory.Lobby, true).SetColor(new Color32(0, 153, 255, 255));
            AutoStart = BooleanOptionItem.Create("AutoStart", false, OptionCategory.Lobby, true).SetColor(new Color32(0, 153, 255, 255));
            AutoStartTime = (IntegerOptionItem)IntegerOptionItem.Create("AutoStartTime", new(0, 600, 10), 60, OptionCategory.Lobby, true).SetParent(AutoStart).SetColor(new Color32(0, 153, 255, 255));
            AutoStartCount = (IntegerOptionItem)IntegerOptionItem.Create("AutoStartCount", new(1, 15, 1), 14, OptionCategory.Lobby, true).SetParent(AutoStart).SetColor(new Color32(0, 153, 255, 255));
            AutoStartDelay = (IntegerOptionItem)IntegerOptionItem.Create("AutoStarDelay", new(0, 60, 5), 30, OptionCategory.Lobby, true).SetParent(AutoStart).SetColor(new Color32(0, 153, 255, 255));

            TagName = BooleanOptionItem.Create("ModdedName", true, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            TagGameMod = BooleanOptionItem.Create("TagGameMod", true, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRole = BooleanOptionItem.Create("DisableRole", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            EnableAllChat = BooleanOptionItem.Create("EnableAllChat", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            RandomVentSpawn = BooleanOptionItem.Create("RandomVentSpawn", false,OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            randomMap = BooleanOptionItem.Create("Opt_RandomMap", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            //TrackImpostorTeammate = BooleanOptionItem.Create("TrackImpostorTeammate", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            MoreImp = BooleanOptionItem.Create("MoreImpostors", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            NumImpostor = (IntegerOptionItem)IntegerOptionItem.Create("NumImpostorMax", new(4, 7, 1), 4, OptionCategory.Gameplay, false).SetParent(MoreImp).SetColor(new Color32(0, 153, 255, 255));
            DisableDevice = BooleanOptionItem.Create("DisableDevice", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDeviceCam = BooleanOptionItem.Create("DisableDeviceCam", false, OptionCategory.Gameplay, true).SetParent(DisableDevice).SetColor(new Color32(0, 153, 255, 255));
            DisableDeviceAdminPanel = BooleanOptionItem.Create("DisableDeviceAdminPanel", false, OptionCategory.Gameplay, true).SetParent(DisableDevice).SetColor(new Color32(0, 153, 255, 255));
            DisableDeviceVitals = BooleanOptionItem.Create("DisableDeviceVitals", false, OptionCategory.Gameplay, true).SetParent(DisableDevice).SetColor(new Color32(0, 153, 255, 255));
            EnableGameTimer = (BooleanOptionItem)BooleanOptionItem.Create("EnableGameTimer", false, OptionCategory.Gameplay, true).SetColor(new Color32(0, 153, 255, 255));
            GameTimerMinutes = (FloatOptionItem)FloatOptionItem.Create("GameTimerMinutes",new(1f, 60f, 0.5f),30f,OptionCategory.Gameplay,true).SetParent(EnableGameTimer).SetColor(new Color32(0, 153, 255, 255));
            GameTimerMessage = BooleanOptionItem.Create("GameTimerMessage", false, OptionCategory.Gameplay, true).SetParent(EnableGameTimer).SetColor(new Color32(0, 153, 255, 255));
            PauseGameTimerDuringMeetings = BooleanOptionItem.Create("PauseGameTimerDuringMeetings",true,OptionCategory.Gameplay,true).SetParent(EnableGameTimer).SetColor(new Color32(0, 153, 255, 255));

            NoKillMeeting = BooleanOptionItem.Create("NoKillMeeting", false, OptionCategory.Meetings, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMeetingsAndReports = BooleanOptionItem.Create("Opt_DisableMeetingsAndReports", false, OptionCategory.Meetings, true).SetColor(new Color32(0, 153, 255, 255));
            //revealVotes = BooleanOptionItem.Create("revealVotes", false, OptionCategory.Meetings, true).SetColor(new Color32(0, 153, 255, 255));
            //AutoVote = BooleanOptionItem.Create("AutoVote", false, OptionCategory.Meetings, true).SetColor(new Color32(0, 153, 255, 255));
            //AutoVoteTime = (IntegerOptionItem)IntegerOptionItem.Create("AutoVoteTime", new(0, 120, 10), 30, OptionCategory.Meetings, true).SetParent(AutoVote).SetColor(new Color32(0, 153, 255, 255));
            //AutoVoteAction = (StringOptionItem)StringOptionItem.Create("AutoVoteAction", new[] { "Always", "OnlyAfk" }, 0, OptionCategory.Meetings, true).SetParent(AutoVote).SetColor(new Color32(0, 153, 255, 255));

            ChatLeft = BooleanOptionItem.Create("ChatLeft", false, OptionCategory.Chat, true).SetColor(new Color32(0, 153, 255, 255));
            TcommandforAll = BooleanOptionItem.Create("EnableTCommandForAll", false, OptionCategory.Chat, true).SetColor(new Color32(0, 153, 255, 255));
            sendwelcome = BooleanOptionItem.Create("SendWelcomeMessage", false, OptionCategory.Chat, true).SetColor(new Color32(0, 153, 255, 255));
            SendSummary = BooleanOptionItem.Create("SendSummary1", false, OptionCategory.Chat, true).SetColor(new Color32(0, 153, 255, 255));
            sendInfocomand = BooleanOptionItem.Create("SendCommandInfo", false, OptionCategory.Chat, true).SetColor(new Color32(0, 153, 255, 255));


            AllowColorChangeAll = BooleanOptionItem.Create("AllowColorChangeAll", false, OptionCategory.Appearance, true).SetColor(new Color32(0, 153, 255, 255));
            AllowColorChangeModerator = BooleanOptionItem.Create("AllowColorChangeModerator", false, OptionCategory.Appearance, true).SetColor(new Color32(0, 153, 255, 255));
            //buttonvisibile = BooleanOptionItem.Create("QuickMenusVisible", false, OptionCategory.Appearance, true).SetColor(new Color32(0, 153, 255, 255));
            //CustomSkin = BooleanOptionItem.Create("CustomSkin", true, OptionCategory.Appearance, true).SetColor(new Color32(0, 153, 255, 255));

            Protection10Sec = BooleanOptionItem.Create("Protection10Sec", false, OptionCategory.Protection, true).SetColor(new Color32(0, 153, 255, 255));
            ProtectFirstHost = BooleanOptionItem.Create("ProtectFirstHost", false, OptionCategory.Protection, true).SetColor(new Color32(0, 153, 255, 255));
            ProtectFirstDead = BooleanOptionItem.Create("ProtectFirstDead", false, OptionCategory.Protection, true).SetColor(new Color32(0, 153, 255, 255));
            ProtectFirstPlayer = (StringOptionItem)StringOptionItem.Create("ProtectFirstPlayer", new[] { "None" }, 0, OptionCategory.Protection, true, false).SetColor(new Color32(0, 153, 255, 255));

            SubmitScanMax = (IntegerOptionItem)IntegerOptionItem.Create("SubmitScanMax", new(-1, 15, 1), -1, OptionCategory.Task, true).SetColor(new Color32(0, 153, 255, 255));
            SharedAllTasks = BooleanOptionItem.Create("SharedAllTasks", false, OptionCategory.Task, true).SetColor(new Color32(0, 153, 255, 255));
            DisableFixWiring = BooleanOptionItem.Create("DisableFixWiring", false, OptionCategory.Common, true).SetColor(new Color32(0, 153, 255, 255));
            DisableSwipeCard = BooleanOptionItem.Create("DisableSwipeCard", false, OptionCategory.Common, true).SetColor(new Color32(0, 153, 255, 255));
            DisableEnterIdCode = BooleanOptionItem.Create("DisableEnterIdCode", false, OptionCategory.Common, true).SetColor(new Color32(0, 153, 255, 255));
            DisableInsertKeys = BooleanOptionItem.Create("DisableInsertKeys", false, OptionCategory.Common, true).SetColor(new Color32(0, 153, 255, 255));
            DisableScanBoardingPass = BooleanOptionItem.Create("DisableScanBoardingPass", false, OptionCategory.Common, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCalibrateDistributor = BooleanOptionItem.Create("DisableCalibrateDistributor", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableChartCourse = BooleanOptionItem.Create("DisableChartCourse", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableStabilizeSteering = BooleanOptionItem.Create("DisableStabilizeSteering", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCleanO2Filter = BooleanOptionItem.Create("DisableCleanO2Filter", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableUnlockManifolds = BooleanOptionItem.Create("DisableUnlockManifolds", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePrimeShields = BooleanOptionItem.Create("DisablePrimeShields", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMeasureWeather = BooleanOptionItem.Create("DisableMeasureWeather", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableBuyBeverage = BooleanOptionItem.Create("DisableBuyBeverage", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableAssembleArtifact = BooleanOptionItem.Create("DisableAssembleArtifact", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableSortSamples = BooleanOptionItem.Create("DisableSortSamples", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableProcessData = BooleanOptionItem.Create("DisableProcessData", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRunDiagnostics = BooleanOptionItem.Create("DisableRunDiagnostics", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRepairDrill = BooleanOptionItem.Create("DisableRepairDrill", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableAlignTelescope = BooleanOptionItem.Create("DisableAlignTelescope", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRecordTemperature = BooleanOptionItem.Create("DisableRecordTemperature", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableFillCanisters = BooleanOptionItem.Create("DisableFillCanisters", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMonitorTree = BooleanOptionItem.Create("DisableMonitorTree", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableStoreArtifacts = BooleanOptionItem.Create("DisableStoreArtifacts", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePutAwayPistols = BooleanOptionItem.Create("DisablePutAwayPistols", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePutAwayRifles = BooleanOptionItem.Create("DisablePutAwayRifles", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMakeBurger = BooleanOptionItem.Create("DisableMakeBurger", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCleanToilet = BooleanOptionItem.Create("DisableCleanToilet", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDecontaminate = BooleanOptionItem.Create("DisableDecontaminate", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableSortRecords = BooleanOptionItem.Create("DisableSortRecords", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableFixShower = BooleanOptionItem.Create("DisableFixShower", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePickUpTowels = BooleanOptionItem.Create("DisablePickUpTowels", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePolishRuby = BooleanOptionItem.Create("DisablePolishRuby", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDressMannequin = BooleanOptionItem.Create("DisableDressMannequin", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableUnlockSafe = BooleanOptionItem.Create("DisableUnlockSafe", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableResetBreaker = BooleanOptionItem.Create("DisableResetBreaker", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableAlignEngineOutput = BooleanOptionItem.Create("DisableAlignEngineOutput", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableEmptyChute = BooleanOptionItem.Create("DisableEmptyChute", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableClearAsteroids = BooleanOptionItem.Create("DisableClearAsteroids", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableWaterPlants = BooleanOptionItem.Create("DisableWaterPlants", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableReplaceWaterJug = BooleanOptionItem.Create("DisableReplaceWaterJug", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDevelopPhotos = BooleanOptionItem.Create("DisableDevelopPhotos", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRewindTapes = BooleanOptionItem.Create("DisableRewindTapes", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableStartFans = BooleanOptionItem.Create("DisableStartFans", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableEmptyGarbage = BooleanOptionItem.Create("DisableEmptyGarbage", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDivertPower = BooleanOptionItem.Create("DisableDivertPower", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRoastMarshmallow = BooleanOptionItem.Create("DisableRoastMarshmallow", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCollectSamples = BooleanOptionItem.Create("DisableCollectSamples", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableReplaceParts = BooleanOptionItem.Create("DisableReplaceParts", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCollectVegetables = BooleanOptionItem.Create("DisableCollectVegetables", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMineOres = BooleanOptionItem.Create("DisableMineOres", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableExtractFuel = BooleanOptionItem.Create("DisableExtractFuel", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCatchFish = BooleanOptionItem.Create("DisableCatchFish", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePolishGem = BooleanOptionItem.Create("DisablePolishGem", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableHelpCritter = BooleanOptionItem.Create("DisableHelpCritter", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableHoistSupplies = BooleanOptionItem.Create("DisableHoistSupplies", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableFixAntenna = BooleanOptionItem.Create("DisableFixAntenna", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableBuildSandcastle = BooleanOptionItem.Create("DisableBuildSandcastle", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCrankGenerator = BooleanOptionItem.Create("DisableCrankGenerator", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMonitorMushroom = BooleanOptionItem.Create("DisableMonitorMushroom", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisablePlayVideoGame = BooleanOptionItem.Create("DisablePlayVideoGame", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableFindSignal = BooleanOptionItem.Create("DisableFindSignal", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableThrowFisbee = BooleanOptionItem.Create("DisableThrowFisbee", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableLiftWeights = BooleanOptionItem.Create("DisableLiftWeights", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCollectShells = BooleanOptionItem.Create("DisableCollectShells", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCleanVent = BooleanOptionItem.Create("DisableCleanVent", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableSubmitScan = BooleanOptionItem.Create("DisableSubmitScan", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableStartReactor = BooleanOptionItem.Create("DisableStartReactor", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableInspectSample = BooleanOptionItem.Create("DisableInspectSample", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableOpenWaterways = BooleanOptionItem.Create("DisableOpenWaterways", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableRebootWifi = BooleanOptionItem.Create("DisableRebootWifi", false, OptionCategory.Short, true).SetColor(new Color32(0, 153, 255, 255));
            DisableUploadData = BooleanOptionItem.Create("DisableUploadData", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableFuelEngines = BooleanOptionItem.Create("DisableFuelEngines", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));
            DisableActivateWeatherNodes = BooleanOptionItem.Create("DisableActivateWeatherNodes", false, OptionCategory.Long, true).SetColor(new Color32(0, 153, 255, 255));

            Jester = BooleanOptionItem.Create("Jester", false, OptionCategory.Jester, true).SetColor(new Color32(0, 153, 255, 255));
            JesterVent = BooleanOptionItem.Create("JesterVent", false, OptionCategory.Jester, true).SetParent(Jester).SetColor(new Color32(0, 153, 255, 255));

            Guess = BooleanOptionItem.Create("Guess", false, OptionCategory.Guesser, true).SetColor(new Color32(0, 153, 255, 255));
            GuesserAction = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Kill", "Exile" }, 0, OptionCategory.Guesser, true).SetParent(Guess).SetColor(new Color32(0, 153, 255, 255));

            EnableImmortal = BooleanOptionItem.Create("EnableImmortal", false, OptionCategory.Immortal, true).SetColor(new Color32(0, 153, 255, 255));
            sendtoAll = BooleanOptionItem.Create("NotifyAll", false, OptionCategory.Immortal, true).SetParent(EnableImmortal).SetColor(new Color32(0, 153, 255, 255));
            sendtoimmortal = BooleanOptionItem.Create("NotifyImmortal", false, OptionCategory.Immortal, true).SetParent(EnableImmortal).SetColor(new Color32(0, 153, 255, 255));
            Immortalesentvote = BooleanOptionItem.Create("Immortalesentvote", false, OptionCategory.Immortal, true).SetParent(EnableImmortal).SetColor(new Color32(0, 153, 255, 255));

            EngineerFixer = BooleanOptionItem.Create("EnginerFixer", false, OptionCategory.Engineer, true).SetColor(new Color32(0, 153, 255, 255));
            VentTimes = (IntegerOptionItem)IntegerOptionItem.Create("VentTimes", new(0, 5, 1), 3, OptionCategory.Engineer, true).SetParent(EngineerFixer).SetColor(new Color32(0, 153, 255, 255));

            PhantomGuess = BooleanOptionItem.Create("PhantomGuess", false, OptionCategory.Impostor, true).SetColor(new Color32(0, 153, 255, 255));
            ShapeGuess = BooleanOptionItem.Create("ShapeGuess", false, OptionCategory.Impostor, true).SetColor(new Color32(0, 153, 255, 255));
            ViperGuess = BooleanOptionItem.Create("ViperGuess", false, OptionCategory.Impostor, true).SetColor(new Color32(0, 153, 255, 255));
            ImpostorGuess = BooleanOptionItem.Create("ImpostorGuess1", false, OptionCategory.Impostor, true).SetColor(new Color32(0, 153, 255, 255));
            aktive_notimmplayer = BooleanOptionItem.Create("aktive_notimmplayer", false, OptionCategory.Impostor, true).SetColor(new Color32(0, 153, 255, 255));

            ScientistTime = BooleanOptionItem.Create("ScientistTime", false, OptionCategory.Scientist, true).SetColor(new Color32(0, 153, 255, 255));

            ExilerExe = BooleanOptionItem.Create("ExilerExe", false, OptionCategory.Exiler, true).SetColor(new Color32(0, 153, 255, 255));
            ExilerAction = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Kill", "Exile" }, 0, OptionCategory.Exiler, true).SetParent(ExilerExe).SetColor(new Color32(0, 153, 255, 255));
            killexiler = BooleanOptionItem.Create("DiesAfterCommand", false, OptionCategory.Exiler, true).SetParent(ExilerExe).SetColor(new Color32(0, 153, 255, 255));

            EnableAntiBotSystem = BooleanOptionItem.Create("EnableAntiBotSystem", false, OptionCategory.AntiBot, true).SetColor(new Color32(0, 153, 255, 255));

            KickLevel = BooleanOptionItem.Create("KickLevel", false, OptionCategory.Levels, true).SetColor(new Color32(0, 153, 255, 255));
            KickLevelLevel = (IntegerOptionItem)IntegerOptionItem.Create("KickLevelLevel", new(5, 100, 5), 50, OptionCategory.Levels, true).SetParent(KickLevel).SetColor(new Color32(0, 153, 255, 255));
            KickLevelAction = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Ban", "Kick" }, 0, OptionCategory.Levels, true).SetParent(KickLevel).SetColor(new Color32(0, 153, 255, 255));

            ApplyDenyNameList = BooleanOptionItem.Create("ApplyDenyNameList", false, OptionCategory.Blocklist, true).SetColor(new Color32(0, 153, 255, 255));
            CheckBanList = BooleanOptionItem.Create("CheckBanList", false, OptionCategory.Blocklist, true).SetColor(new Color32(0, 153, 255, 255));
            CheckBlockList = BooleanOptionItem.Create("CheckBlockList", false, OptionCategory.Blocklist, true).SetColor(new Color32(0, 153, 255, 255));
            CheckFriendCode = BooleanOptionItem.Create("CheckFriendCode", false, OptionCategory.Blocklist, true).SetColor(new Color32(0, 153, 255, 255));
            ActionTeamers = (StringOptionItem)StringOptionItem.Create("ActionTeamers", new[] { "OnlyWarm", "Kick", "Ban" }, 0, OptionCategory.Blocklist, true).SetColor(new Color32(0, 153, 255, 255));

            EnableAntiCheat = BooleanOptionItem.Create("EnableAntiCheat", false, OptionCategory.Cheat, true).SetColor(new Color32(0, 153, 255, 255));
            ClooseDoorsCheat = BooleanOptionItem.Create("CloseDoorsCheat", false, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));
            CompleteTaskCheat = BooleanOptionItem.Create("CompleteTaskCheat", false, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));
            KickVentCheat = BooleanOptionItem.Create("KickVentCheat", false, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));
            SabotageCheat2 = BooleanOptionItem.Create("SabotageCheat2", false, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));
            UseVentCheat = BooleanOptionItem.Create("UseVentCheat", false, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));
            SentWarning = BooleanOptionItem.Create("SentWarning", false, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));
            ActionCheater = (StringOptionItem)StringOptionItem.Create("ActionCheaterlist", new[] { "OnlyWarm", "Kick", "Ban" }, 0, OptionCategory.Cheat, true).SetParent(EnableAntiCheat).SetColor(new Color32(0, 153, 255, 255));

            EnableDetector = BooleanOptionItem.Create("EnableAFKDetector", false, OptionCategory.Afk, true).SetColor(new Color32(0, 153, 255, 255));
            EnableAfkKick = BooleanOptionItem.Create("EnableAfkKick", false, OptionCategory.Afk, true).SetParent(EnableDetector).SetColor(new Color32(0, 153, 255, 255));
            EnableShield = BooleanOptionItem.Create("EnableShield", false, OptionCategory.Afk, true).SetParent(EnableDetector).SetColor(new Color32(0, 153, 255, 255));
            DetectionDelay = (IntegerOptionItem)IntegerOptionItem.Create("AFKDetectionDelay(sec)", new(5, 60, 5), 45, OptionCategory.Afk, true).SetParent(EnableDetector).SetColor(new Color32(0, 153, 255, 255));
            TimeToActivate = (IntegerOptionItem)IntegerOptionItem.Create("TimeToActivate(min)", new(1, 10, 1), 1, OptionCategory.Afk, true).SetParent(EnableDetector).SetColor(new Color32(0, 153, 255, 255));

            EnableCamDetector = BooleanOptionItem.Create("EnableCamDetector", false, OptionCategory.Cam, true).SetColor(new Color32(0, 153, 255, 255));
            EnableCamKick = BooleanOptionItem.Create("EnableCamKick", false, OptionCategory.Cam, true).SetParent(EnableCamDetector).SetColor(new Color32(0, 153, 255, 255));
            DetectionCamDelay = (IntegerOptionItem)IntegerOptionItem.Create("CamDetectionDelaySeconds", new(0, 60, 5), 20, OptionCategory.Cam, true).SetParent(EnableCamDetector).SetColor(new Color32(0, 153, 255, 255));
            TimeToCamActivate = (IntegerOptionItem)IntegerOptionItem.Create("CamKickDelaySeconds", new(0, 60, 5), 30, OptionCategory.Cam, true).SetParent(EnableCamDetector).SetColor(new Color32(0, 153, 255, 255));
            MaxCam = (IntegerOptionItem)IntegerOptionItem.Create("MaxCam", new(0, 5, 1), 3, OptionCategory.Cam, true).SetParent(EnableCamDetector).SetColor(new Color32(0, 153, 255, 255));

            EnableCamTaskDetector = BooleanOptionItem.Create("EnableCamTaskDetector", false, OptionCategory.CamTask, true).SetColor(new Color32(0, 153, 255, 255));
            EnableCamTaskKick = BooleanOptionItem.Create("EnableCamKick", false, OptionCategory.CamTask, true).SetColor(new Color32(0, 153, 255, 255)).SetParent(EnableCamTaskDetector);
            DetectionCamTaskDelay = (IntegerOptionItem)IntegerOptionItem.Create("CamDetectionDelaySeconds", new(0, 60, 5), 20, OptionCategory.CamTask, true).SetParent(EnableCamTaskDetector).SetColor(new Color32(0, 153, 255, 255));
            TimeToCamTaskActivate = (IntegerOptionItem)IntegerOptionItem.Create("CamKickDelaySeconds", new(0, 60, 5), 30, OptionCategory.CamTask, true).SetParent(EnableCamTaskDetector).SetColor(new Color32(0, 153, 255, 255));
            MinTasksToUseCamCrew = (IntegerOptionItem)IntegerOptionItem.Create("MinTasksToUseCamCrew", new(1, 10, 1), 3, OptionCategory.CamTask, true).SetParent(EnableCamTaskDetector).SetColor(new Color32(0, 153, 255, 255));
            ImpostorCamCondition = (StringOptionItem)StringOptionItem.Create("ImpostorCamCondition", new[] { "Task", "KillMin", "Both", "Either" }, 0, OptionCategory.CamTask, true).SetParent(EnableCamTaskDetector).SetColor(new Color32(0, 153, 255, 255));
            MinKillsToUseCamImp = (IntegerOptionItem)IntegerOptionItem.Create("KillsLabel", new(1, 10, 1), 3, OptionCategory.CamTask, true).SetParent(EnableCamTaskDetector).SetColor(new Color32(0, 153, 255, 255));
            MinTasksToUseCamImp = (IntegerOptionItem)IntegerOptionItem.Create("MinTasksToUseCamImp", new(1, 10, 1), 3, OptionCategory.CamTask, true).SetParent(EnableCamTaskDetector).SetColor(new Color32(0, 153, 255, 255));

            EnableProximityMonitor = BooleanOptionItem.Create("EnableProximityMonitor", false, OptionCategory.Follow, true).SetColor(new Color32(0, 153, 255, 255));
            ProximityDistance = (IntegerOptionItem)IntegerOptionItem.Create("ProximityDistance", new(0, 10, 1), 5, OptionCategory.Follow, true).SetParent(EnableProximityMonitor).SetColor(new Color32(0, 153, 255, 255));
            ProximityTimeSeconds = (IntegerOptionItem)IntegerOptionItem.Create("ProximityTimeSeconds", new(0, 120, 5), 30, OptionCategory.Follow, true).SetParent(EnableProximityMonitor).SetColor(new Color32(0, 153, 255, 255));
            ProximityAction = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Warm", "Kick" }, 0, OptionCategory.Follow, true).SetParent(EnableProximityMonitor).SetColor(new Color32(0, 153, 255, 255));

            AutoKickStart = BooleanOptionItem.Create("AutoKickStart", false, OptionCategory.Spamlist, true).SetColor(new Color32(0, 153, 255, 255));
            AutoKickStartTimes = (IntegerOptionItem)IntegerOptionItem.Create("WarningsBeforeKick", new(0, 5, 1), 2, OptionCategory.Spamlist, true).SetParent(AutoKickStart).SetColor(new Color32(0, 153, 255, 255));
            SendAutoKickStartMsg = BooleanOptionItem.Create("SendWarningMessageToPlayer", false, OptionCategory.Spamlist, true).SetParent(AutoKickStart).SetColor(new Color32(0, 153, 255, 255));
            AutoKickStartAction = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Kick", "Ban" }, 0, OptionCategory.Spamlist, true).SetParent(AutoKickStart).SetColor(new Color32(0, 153, 255, 255));

            AutoKickStopWords = BooleanOptionItem.Create("AutoKickStopWords", false, OptionCategory.Wordlist, true).SetColor(new Color32(0, 153, 255, 255));
            AutoKickStopWordsTimes = (IntegerOptionItem)IntegerOptionItem.Create("WarningsBeforeKick", new(0, 5, 1), 1, OptionCategory.Wordlist, true).SetParent(AutoKickStopWords).SetColor(new Color32(0, 153, 255, 255));
            SendAutoKickStopWordsMsg = BooleanOptionItem.Create("SendWarningMessageToPlayer", false, OptionCategory.Wordlist, true).SetParent(AutoKickStopWords).SetColor(new Color32(0, 153, 255, 255));
            AutoKickStopWordsAction = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Kick", "Ban" }, 0, OptionCategory.Wordlist, true).SetParent(AutoKickStopWords).SetColor(new Color32(0, 153, 255, 255));

            Enablesabotage = BooleanOptionItem.Create("Enablesabotage", false, OptionCategory.SabotageOption, true).SetColor(new Color32(0, 153, 255, 255));
            DecontaminationTime = (IntegerOptionItem)IntegerOptionItem.Create("DecontaminationTime", new(3, 30, 1), 9, OptionCategory.SabotageOption, true).SetColor(new Color32(0, 153, 255, 255));
            SabotageCooldown = (IntegerOptionItem)IntegerOptionItem.Create("SabotageCooldown", new(1, 60, 1), 30, OptionCategory.SabotageOption, true).SetColor(new Color32(0, 153, 255, 255));
            ResetDoorsEveryTurns = BooleanOptionItem.Create("ResetDoorsEveryTurns", false, OptionCategory.SabotageOption, true).SetColor(new Color32(0, 153, 255, 255));
            DoorsResetMode = (StringOptionItem)StringOptionItem.Create("Action", new[] { "Open", "Close" }, 0, OptionCategory.SabotageOption, true).SetParent(ResetDoorsEveryTurns).SetColor(new Color32(0, 153, 255, 255));
            BlockSwitches = BooleanOptionItem.Create("BlockSwitches", false, OptionCategory.SabotageOption, true).SetColor(new Color32(0, 153, 255, 255));

            DisableAllSabotages = BooleanOptionItem.Create("DisableAllSabotages", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDeadImpostorSabotage = BooleanOptionItem.Create("DisableAllSabotagesForDeadImpostor", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableReactorSabotage = BooleanOptionItem.Create("DisableReactorSabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableCommsSabotage = BooleanOptionItem.Create("DisableCommsSabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableO2Sabotage = BooleanOptionItem.Create("DisableO2Sabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableElectricalSabotage = BooleanOptionItem.Create("DisableElectricalSabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableLaboratorySabotage = BooleanOptionItem.Create("DisableLaboratorySabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableHeliSabotage = BooleanOptionItem.Create("DisableHeliSabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableMushroomSabotage = BooleanOptionItem.Create("DisableMushroomSabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            DisableDoorSabotage = BooleanOptionItem.Create("DisableDoorSabotage", false, OptionCategory.Sabotage, true).SetColor(new Color32(0, 153, 255, 255));
            void UpdateSabotageOptions(bool disable)
            {
                DisableReactorSabotage.SetEnabled(!disable);
                DisableCommsSabotage.SetEnabled(!disable);
                DisableO2Sabotage.SetEnabled(!disable);
                DisableElectricalSabotage.SetEnabled(!disable);
                DisableLaboratorySabotage.SetEnabled(!disable);
                DisableHeliSabotage.SetEnabled(!disable);
                DisableMushroomSabotage.SetEnabled(!disable);

                if (disable)
                {
                    DisableReactorSabotage.SetValue(0);
                    DisableCommsSabotage.SetValue(0);
                    DisableO2Sabotage.SetValue(0);
                    DisableElectricalSabotage.SetValue(0);
                    DisableLaboratorySabotage.SetValue(0);
                    DisableHeliSabotage.SetValue(0);
                    DisableMushroomSabotage.SetValue(0);
                }
            }
            DisableAllSabotages.RegisterUpdateValueEvent((sender, args) =>
            {
                bool disable = ((OptionItem)sender).GetBool();
                UpdateSabotageOptions(disable);
            });
            TaskVisibilityController.UpdateTaskVisibility();
            IsLoaded = true;
            FfaExternalBridge.SyncAll();
        }
        public static void ReOpenSettings()
        {
            if (!IsLoaded)
                return;

            if (!GameStates.isLobby)
                return;

            if (AmongUsClient.Instance == null)
                return;

            if (PlayerControl.LocalPlayer == null)
                return;

            if (_reOpenSettingsScheduled)
                return;

            _reOpenSettingsScheduled = true;

            OptionItem.SyncAllOptions();

            var menu = GameObject.FindObjectOfType<GameSettingMenu>();

            if (menu != null)
                menu.Close();

            LateTask.New(() =>
            {
                if (!GameStates.isLobby)
                    return;

                var hostButtons = GameObject.Find("Host Buttons");

                var editBtn =
                    hostButtons?.transform.FindChild("Edit")?.GetComponent<PassiveButton>() ??
                    hostButtons?.transform.FindChild("EditButton")?.GetComponent<PassiveButton>();

                if (editBtn != null)
                    editBtn.ReceiveClickDown();

            }, 0.01f);

            LateTask.New(() =>
            {
                try
                {
                    var newMenu = GameObject.Find("PlayerOptionsMenu(Clone)");

                    if (newMenu == null)
                        return;

                    if (GameSettingMenuPatch.GameModesButton != null)
                    {
                        GameSettingMenuPatch.GameModesButton.ReceiveClickDown();
                        GameSettingMenuPatch.GameModesButton.OnClick.Invoke();
                    }
                }
                finally
                {
                    _reOpenSettingsScheduled = false;
                }

            }, 0.18f);
        }
    }
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public static class GameOptionsMenu_Update_ForceFFA_HnSOnly_Patch
    {
        private static float seekerSyncTimer = 0f;

        public static void Postfix()
        {
            try
            {

                GameModeType gameMode = Options.GameMode.Selected;

                if (GameStates.isHideNSeek)
                {
                    if (gameMode == GameModeType.FFA)
                    {
                        Options.GameMode.SetValue(GameModeType.Default);
                    }

                    seekerSyncTimer += Time.deltaTime;

                    if (seekerSyncTimer >= 2f)
                    {
                        seekerSyncTimer = 0f;
                        UpdateSeekerSelectionsAndSyncFirstSeeker();
                    }
                }
                else
                {
                    seekerSyncTimer = 0f;
                }
            }
            catch
            {
            }
        }

        private static void UpdateSeekerSelectionsAndSyncFirstSeeker()
        {
            try
            {
                if (AmongUsClient.Instance == null || GameData.Instance == null)
                    return;

                if (Options.SeekerSelections == null || Options.SeekerSelections.Count <= 0)
                    return;

                var namesList = new System.Collections.Generic.List<string>();
                namesList.Add("Round-robin");

                foreach (var player in GameData.Instance.AllPlayers)
                {
                    if (player != null && !player.Disconnected)
                        namesList.Add(player.PlayerName);
                }

                string[] namesArray = namesList.ToArray();

                foreach (var opt in Options.SeekerSelections)
                {
                    if (opt == null)
                        continue;

                    opt.Selections = namesArray;
                    opt.Rule = (0, namesArray.Length - 1, 1);
                }

                SyncVanillaSeekerToSetSeeker1(namesArray);
            }
            catch
            {
            }
        }

        private static void SyncVanillaSeekerToSetSeeker1(string[] namesArray)
        {
            try
            {
                if (namesArray == null || namesArray.Length <= 1)
                    return;

                var hnsOptions = GameOptionsManager.Instance.CurrentGameOptions.Cast<HideNSeekGameOptionsV12>();

                if (hnsOptions == null)
                    return;

                int vanillaSeekerId = hnsOptions.ImpostorPlayerID;

                if (vanillaSeekerId < 0)
                    return;

                string vanillaSeekerName = GetPlayerNameById(vanillaSeekerId);

                if (string.IsNullOrEmpty(vanillaSeekerName))
                    return;

                int index = -1;

                for (int i = 0; i < namesArray.Length; i++)
                {
                    if (namesArray[i] == vanillaSeekerName)
                    {
                        index = i;
                        break;
                    }
                }

                if (index <= 0)
                    return;

                StringOptionItem setSeeker1 = Options.SeekerSelections[0];

                if (setSeeker1 == null)
                    return;

                try
                {
                    if (setSeeker1.GetString() == vanillaSeekerName)
                        return;
                }
                catch
                {
                }

                setSeeker1.SetValue(index);
            }
            catch
            {
            }
        }

        private static string GetPlayerNameById(int playerId)
        {
            try
            {
                if (GameData.Instance != null)
                {
                    foreach (var player in GameData.Instance.AllPlayers)
                    {
                        if (player == null || player.Disconnected)
                            continue;

                        if (player.PlayerId == playerId)
                            return player.PlayerName;
                    }
                }
            }
            catch
            {
            }

            try
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.Data == null)
                        continue;

                    if (player.PlayerId == playerId)
                        return player.Data.PlayerName;
                }
            }
            catch
            {
            }

            return "";
        }
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        public static class UpdateFirstMeetingProtectedPlayerPatch
        {
            private static float updateTimer;

            public static void Postfix()
            {

                try
                {
                    if (!GameStates.isLobby)
                    {
                        updateTimer = 0f;
                        return;
                    }

                    updateTimer += Time.deltaTime;

                    if (updateTimer < 1f)
                        return;

                    updateTimer = 0f;

                    UpdatePlayerNames();
                }
                catch (System.Exception exception)
                {
                    BMLogger.LogWarning(
                        $"[FirstMeetingPlayerOption] Errore aggiornamento nomi: " +
                        $"{exception.Message}"
                    );
                }
            }

            private static void UpdatePlayerNames()
            {
                if (AmongUsClient.Instance == null ||
                    GameData.Instance == null ||
                    Options.ProtectFirstPlayer == null)
                {
                    return;
                }

                string previouslySelected = "None";

                string[] currentSelections =
                    Options.ProtectFirstPlayer.Selections;

                int currentValue =
                    Options.ProtectFirstPlayer.GetValue();

                if (currentSelections != null &&
                    currentValue >= 0 &&
                    currentValue < currentSelections.Length)
                {
                    previouslySelected =
                        currentSelections[currentValue];
                }

                var namesList =
                    new List<string>
                    {
                "None"
                    };

                foreach (NetworkedPlayerInfo player
                         in GameData.Instance.AllPlayers)
                {
                    if (player == null ||
                        player.Disconnected ||
                        string.IsNullOrWhiteSpace(player.PlayerName))
                    {
                        continue;
                    }

                    namesList.Add(player.PlayerName);
                }

                string[] namesArray =
                    namesList.ToArray();

                if (currentSelections != null &&
                    currentSelections.SequenceEqual(namesArray))
                {
                    return;
                }

                Options.ProtectFirstPlayer.Selections =
                    namesArray;

                Options.ProtectFirstPlayer.Rule =
                    (
                        0,
                        namesArray.Length - 1,
                        1
                    );

                int selectedIndex =
                    System.Array.FindIndex(
                        namesArray,
                        name => string.Equals(
                            name,
                            previouslySelected,
                            System.StringComparison.Ordinal
                        )
                    );

                if (selectedIndex < 0)
                    selectedIndex = 0;

                Options.ProtectFirstPlayer.SetValue(
                    selectedIndex
                );

                BMLogger.Info(
                    $"[FirstMeetingPlayerOption] Lista aggiornata. " +
                    $"Selezione mantenuta: {namesArray[selectedIndex]}"
                );
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            OptionItem.SyncAllOptions();
        }
    }
}
[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
public static class NumberOptionLimitPatch
{
    private static readonly HashSet<StringNames> LockedOptions = new()
    {
        StringNames.GameNumImpostors,
        StringNames.CapacityLabel
    };

    public static void Postfix(NumberOption __instance)
    {
        if (__instance == null)
            return;

        if (__instance.Title == StringNames.GameKillCooldown)
        {
            __instance.ValidRange = new FloatRange(0.001f, 180f);
            __instance.Increment = 0.5f;
            return;
        }

        if (__instance.Title == StringNames.ViperDissolveTime)
        {
            __instance.ValidRange = new FloatRange(1f, 180f);
            __instance.Increment = 1f;
            return;
        }

        if (__instance.Title == StringNames.GamePlayerSpeed)
        {
            if (GameStates.isHideNSeek)
            {
                __instance.ValidRange = new FloatRange(0f, 999f);
                __instance.Increment = 0.1f;
            }
            else
            {
                __instance.ValidRange = new FloatRange(0.50f, 3f);
                __instance.Increment = 0.1f;
            }
            return;
        }

        if (__instance.Title == StringNames.GameDiscussTime)
        {
            __instance.ValidRange = new FloatRange(0f, 999f);
            __instance.Increment = 5f;
            return;
        }

        if (__instance.Title == StringNames.GameVotingTime)
        {
            __instance.ValidRange = new FloatRange(0f, 999f);
            __instance.Increment = 5f;
            return;
        }

        if (__instance.Title == StringNames.GameEmergencyCooldown)
        {
            __instance.ValidRange = new FloatRange(0f, 999f);
            __instance.Increment = 1f;
            return;
        }

        if (__instance.Title == StringNames.GameCrewLight)
        {
            __instance.ValidRange = new FloatRange(0f, 999f);
            __instance.Increment = 0.05f;
            return;
        }

        if (__instance.Title == StringNames.GameImpostorLight)
        {
            __instance.ValidRange = new FloatRange(0f, 999f);
            __instance.Increment = 0.05f;
            return;
        }
        if (LockedOptions.Contains(__instance.Title))
            return;

        __instance.ValidRange = new FloatRange(0f, 999f);
    }
}


[HarmonyPatch(typeof(ActionButton), nameof(ActionButton.SetCoolDown))]
public static class Patch_ActionButton_SetCoolDown
{
    static bool Prefix(ActionButton __instance, ref float timer, ref float maxTimer, ref bool __state)
    {
        __state = false;

        try
        {
            if (IsBanModDisabled)
                return true;

            if (__instance == null)
                return true;

            if (GameStates.isHideNSeek)
                return true;

            if (maxTimer > 0f)
                return true;

            __state = true;

            timer = 0f;
            maxTimer = 1f;

            return true;
        }
        catch
        {
            return true;
        }
    }

    static void Postfix(ActionButton __instance, bool __state)
    {
        try
        {
            if (!__state)
                return;

            if (IsBanModDisabled)
                return;

            if (__instance == null)
                return;

            __instance.SetCooldownFill(0.1f);
        }
        catch
        {
        }
    }
}