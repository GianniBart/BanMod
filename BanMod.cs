//credits and licenses in the resources folder
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Runtime.Remoting.Messaging;
using InnerNet;
using Sentry.Unity.NativeUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static BanMod.BanMenuButtonsPatch;
using static BanMod.Utils;
using Object = UnityEngine.Object;

namespace BanMod;

[BepInPlugin(PluginGuid, "BanMod", PluginVersion)]
[BepInProcess("Among Us.exe")]
public partial class BanMod : BasePlugin
{
    public static BanMod Instance;
    public Harmony Harmony { get; } = new(PluginGuid);
    public static string modVersion = "4.5.3";
    public const string PluginGuid = "com.GianniBart.BanMod";
    public const string PluginVersion = "4.5.3";
    public const string VersionRequired = PluginVersion;
    public static Version version = Version.Parse(PluginVersion);
    public static List<string> supportedAU = new List<string> { "2026.9.29" };
    public static readonly string ModName = "BanMod";
    public static NormalGameOptionsV12 NormalOptions => GameOptionsManager.Instance != null ? GameOptionsManager.Instance.currentNormalGameOptions : null;
    public static ManualLogSource PluginLogger;
    public static KeyBindOptions keyBindOptions;
    public static HostControl hostControl;
    public static ModeratorUi moderatorUi;
    public static MsgMenu msgMenu;
    public static PlayerUI playerUI;
    public static SetPlayerUi setplayerUI;
    public static SkinUI skinUI;
    public static NameUI nameUI;
    public static VisualOptions visualOptions;
    public static PlayerTaskManager playerTaskManager;

    public static Dictionary<byte, bool> originalIsDeadStates = new Dictionary<byte, bool>();
    public static Dictionary<byte, float> playerDeathTimes = new Dictionary<byte, float>();
    public static readonly HashSet<byte> ShieldedPlayers = new HashSet<byte>();
    public static readonly HashSet<byte> UnreportableBodies = [];
    public static Dictionary<byte, PlayerState> PlayerStates = [];
    public static bool IntroDestroyed;
    public static int UpdateTime;
    public static string credentialsText;
    public static int ProtectedPlayerId = -1;
    public static OptionBackupData RealOptionsData;
    public static string InitiallyProtectedFriendCode = null;
    public static string FirstDeadFriendCode = null;
    public static bool IsFirstRound = true;
    public static byte ProtectedPlayerIdThisMatch = 255;
    public static string FriendCodeToRemoveShield = null;
    public static readonly HashSet<int> ModdedClients = new();
    public static List<DateTime> HostSelfSetTimes = new List<DateTime>();
    public static List<byte> forcedImpostorIds = new List<byte>();
    public static bool forceImpostor = false;
    public static bool _initialized = false;
    public static bool EveryRandomActive = false;
    public static bool EndGameForced = false;
    public static float everyRandomTimer = 0f;
    public static float rainbowPlayerTimer = 0f;
    public static PlayerControl RainbowTarget = null;
    private readonly List<UnityEngine.Component> _banModComponents = new();
    public static bool instantstart = false;

    private static readonly string saveFilePath = Path.Combine(Application.persistentDataPath, "host_setimp_times.txt");

    public static bool IsProtected(ClientData client)
    {
        if (client == null)
            return false;

        string friendCode = client.FriendCode;

        if (string.IsNullOrWhiteSpace(friendCode))
            return false;

        if (AllowedManager.IsModCreator(friendCode))
            return true;

        return Utils.IsVip(friendCode) ||
               Utils.IsModerator(friendCode);
    }

    public float Timer { get; set; }
    public static readonly List<(string Message, byte ReceiverID)> MessagesToSend = [];
    public static readonly Dictionary<byte, Color32> PlayerColors = [];
    public static ConfigEntry<int> MessageWait { get; private set; }
    public static ConfigEntry<string> WebhookUrl { get; private set; }
    public static bool CheckBanPlayer;
    public static readonly Dictionary<byte, string> AllPlayerNames = [];
    public static ConfigEntry<string> HideColor { get; private set; }
    public const string ModColor = "#FFA500";
    public static readonly Dictionary<int, int> SayStartTimes = [];
    public static readonly Dictionary<int, int> SayBanwordsTimes = [];
    public static readonly bool ShowinfoButton = true;
    public static readonly bool ShowWebsiteButton = true;
    public static readonly bool ShowGitButton = true;
    public static readonly bool premiumButton = true;
    public static readonly bool ShowlobbyButton = true;
    public static readonly bool ShowKaitoButton = true;
    public static readonly bool ShowDiscordButton = true;
    public static bool ShowCommunityButton = true;
    public static readonly string GitsiteUrl = "https://github.com/GianniBart/BanMod";
    public static readonly string LobbysiteUrl = "https://banmod.online/";
    public static readonly string DiscordInviteUrl = "https://discord.gg/HGEBsm3vps";
    public static readonly string KaitositeUrl = "https://telegra.ph/KaitoRun-Fungle-Lobby-11-16";
    public static readonly string DiscordUrl = "https://discord.com/users/1537337616775651389";
    public static bool hasSentHackWarning = false;
    public static bool hasKilled = false;
    public static RoomZoneManager RoomZoneManagerInstance = new RoomZoneManager();
    private static Color? _unityModColor;
    public static void FlashColor(Color color, float duration = 1f)
    {
        HudManager hud = FastDestroyableSingleton<HudManager>.Instance;
        if (hud.FullScreen == null) return;

        GameObject obj = hud.transform.FindChild("FlashColor_FullScreen")?.gameObject;

        if (obj == null)
        {
            obj = Object.Instantiate(hud.FullScreen.gameObject, hud.transform);
            obj.name = "FlashColor_FullScreen";
        }

        hud.StartCoroutine(Effects.Lerp(duration, new Action<float>(t =>
        {
            obj.SetActive(Math.Abs(t - 1f) > 0.1f);
            obj.GetComponent<SpriteRenderer>().color = new(color.r, color.g, color.b, Mathf.Clamp01(((-2f * Mathf.Abs(t - 0.5f)) + 1) * color.a / 2));
        })));
    }
    public static void PlayPrivateMessageSound(byte playerId)
    {
        SoundManager.Instance.PlaySound(DestroyableSingleton<ChatController>.Instance.messageSound, false, 1f);
    }
    public static void SendModDetectionRPC()
    {
        if (PlayerControl.LocalPlayer == null) return;

        uint[] detectionCodes = { 420, 42069, 250 };

        foreach (var code in detectionCodes)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)code,
                SendOption.Reliable
            );
            writer.EndMessage();
        }
    }
    public static bool chatOpen
    {
        get
        {
            try
            {
                var hud = DestroyableSingleton<HudManager>.Instance;

                bool vanillaChatOpen =
                    hud != null &&
                    hud.Chat != null &&
                    hud.Chat.IsOpenOrOpening;

                bool playerUiEditing =
                    PlayerUI.Instance != null &&
                    PlayerUI.Instance.editingInput;

                return vanillaChatOpen || playerUiEditing;
            }
            catch
            {
                return false;
            }
        }
    }
    public static void ApplyPresetAutomatically()
    {
        try
        {
            var gameOptionsMenu = UnityEngine.Object.FindObjectOfType<GameOptionsMenu>();

            if (gameOptionsMenu != null)
            {
                gameOptionsMenu.ClickPresetButton(RulesPresets.Standard);
            }
        }
        catch
        {
        }
    }
    public static PlayerControl[] AllPlayerControls
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            var i = 0;

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.PlayerId == 255) continue;

                result[i++] = pc;
            }

            if (i == 0) return [];

            Array.Resize(ref result, i);
            return result;
        }
    }
    public static PlayerControl[] AllCrewmates
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            var i = 0;

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.PlayerId == 255) continue;

                if (pc.Data != null && !Utils.ImpostorTeam(pc))
                {
                    result[i++] = pc;
                }
            }

            if (i == 0) return Array.Empty<PlayerControl>();

            Array.Resize(ref result, i);
            return result;
        }
    }
    public static ClientData[] AllClients
    {
        get
        {
            if (AmongUsClient.Instance == null || AmongUsClient.Instance.allClients == null)
                return System.Array.Empty<ClientData>();

            var all = AmongUsClient.Instance.allClients;
            int count = all.Count;
            var result = new ClientData[count];
            int i = 0;

            foreach (var client in all)
            {
                if (client == null) continue;
                if (client.Id == 255) continue;
                if (client.Character == null) continue;

                result[i++] = client;
            }

            if (i == 0)
                return System.Array.Empty<ClientData>();

            System.Array.Resize(ref result, i);
            return result;
        }
    }
    public static PlayerControl GetPlayerControlFromClientId(int clientId)
    {
        if (clientId < 0) return null;

        ClientData clientData = null;
        foreach (var c in AmongUsClient.Instance.allClients)
        {
            if (c.Id == clientId)
            {
                clientData = c;
                break;
            }
        }
        if (clientData != null && clientData.Character != null)
            return clientData.Character;

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc != null && pc.PlayerId != 255 && clientData != null && clientData.Id == clientId)
                return pc;
        }

        return null;
    }
    public static string GetRealPlayerName(ClientData client)
    {
        if (client == null)
            return "";

        try
        {
            PlayerControl pc = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrDefault(p => p.OwnerId == client.Id);

            NetworkedPlayerInfo info = pc != null
                ? GameData.Instance?.GetPlayerById(pc.PlayerId)
                : null;

            string realName = info?.DefaultOutfit?.PlayerName;

            if (!string.IsNullOrWhiteSpace(realName))
                return realName;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BanMod] GetRealPlayerName fallback: {ex}");
        }

        return client.PlayerName ?? "";
    }
    public static List<PlayerControl> AllAlivePlayerControls
    {
        get
        {
            if (AllPlayerControls == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.IsGameStarted)
                return new List<PlayerControl>();

            return AllPlayerControls
                .Where(p => p != null && p.Data != null && !p.Data.IsDead)
                .ToList();
        }
    }
    public static Color UnityModColor
    {
        get
        {
            if (!_unityModColor.HasValue)
            {
                if (ColorUtility.TryParseHtmlString(ModColor, out var unityColor))
                {
                    _unityModColor = unityColor;
                }
                else
                {
                    return Color.gray;
                }
            }
            return _unityModColor.Value;
        }
    }

    public static void LoadHostSetTimes()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                HostSelfSetTimes = File.ReadAllLines(saveFilePath)
                    .Select(line => DateTime.Parse(line))
                    .ToList();
            }
        }
        catch (Exception)
        {
            HostSelfSetTimes = new List<DateTime>();
        }
    }
    public static ConfigEntry<bool> EnableMatchLog { get; private set; }
    public static ConfigEntry<bool> EnableChatLog { get; private set; }
    public static ConfigEntry<bool> ShowFPS { get; private set; }
    public static ConfigEntry<bool> GM { get; private set; }
    public static ConfigEntry<bool> DarkTheme { get; private set; }
    public static ConfigEntry<bool> DisableLobbyMusic { get; private set; }
    public static ConfigEntry<bool> AktiveLobby { get; private set; }
    public static ConfigEntry<bool> HalloweenDecorations { get; private set; }
    public static ConfigEntry<bool> AnniversaryDecorations { get; private set; }
    public static ConfigEntry<bool> AktiveChat { get; private set; }
    public static ConfigEntry<bool> EnableGhostChat { get; private set; }
    
    public static ConfigEntry<bool> ChatOffIfImpostor { get; private set; }
    public static ConfigEntry<bool> Resize_Player { get; private set; }
    public static ConfigEntry<bool> ExcludeFriends { get; private set; }
    public static ConfigEntry<bool> AddBanToList { get; private set; }
    public static ConfigEntry<bool> NoGameEnd { get; private set; }
    public static ConfigEntry<bool> EnableZoom { get; private set; }
    public static ConfigEntry<bool> SwitchVanilla { get; private set; }
    public static ConfigEntry<bool> SeeRoleMeeting { get; private set; }
    public static ConfigEntry<bool> VoteLockEnabled { get; private set; }
    public static ConfigEntry<string> spoofLevel { get; set; }
    public static ConfigEntry<string> menuHtmlColor { get; set; }
    public static ConfigEntry<string> FriendCode { get; set; }
    public static ConfigEntry<string> spoofPlatform { get; set; }
    public ConfigEntry<int> MoveRateLimit { get; set; }
    public static bool ShowColorName { get; set; }
    public static bool ShowNoName { get; set; }
    public static bool ShowVipModTag { get; set; }
    public static bool namewithid { get; set; }
    public static bool level { get; set; }
    public static bool Taskremain { get; set; }
    public static bool ShowMsgAlert { get; set; }
    public static bool SharedAllTasks { get; set; }
    public static bool Enablesabotage { get; set; }
    public static bool ShowInfo { get; set; }
    public static bool UseCustomNames { get; set; }
    public static ConfigEntry<bool> CustomMouse;
    public static bool IsBanModDisabled { get; private set; } = false;
    public static void DisableMod()
    {
        BanMod.Instance.Unload();
        Harmony.UnpatchID("com.GianniBart.BanMod");
        IsBanModDisabled = true;
    }
    public static void DisableAllRoles()
    {
        if (!Options.DisableRole.GetBool()) return;

        OptionItem[] rolesToDisable = {
            Options.Guess, Options.EnableImmortal,
            Options.EngineerFixer, Options.ViperGuess, Options.PhantomGuess,
            Options.ShapeGuess, Options.ImpostorGuess, Options.ScientistTime,
            Options.ExilerExe, Options.Jester
        };

        foreach (var role in rolesToDisable)
        {
            if (role != null && role.GetValue() != 0)
            {
                role.SetValue(0);
            }
        }
    }
    public static void EnableAllRoles()
    {
        if (Options.DisableRole.GetBool()) return;

        OptionItem[] rolesToEnable = {
            Options.Guess, Options.EnableImmortal,
            Options.EngineerFixer, Options.ViperGuess, Options.PhantomGuess,
            Options.ExilerExe, Options.Jester
        };

        foreach (var role in rolesToEnable)
        {
            if (role != null && role.GetValue() == 0)
            {
                role.SetValue(1);
            }
        }
    }
    public override void Load()
    {
        Instance = this;
        PluginLogger = Log;
        BMLogger.Init(PluginLogger);
        EnableMatchLog = Config.Bind("Client Options", "EnableMatchLog", true);
        EnableChatLog = Config.Bind("Client Options", "EnableChatLog", true);
        ShowFPS = Config.Bind("Client Options", "ShowFPS", false);
        GM = Config.Bind("Client Options", "GM", false);
        DarkTheme = Config.Bind("Client Options", "DarkTheme", false);
        DisableLobbyMusic = Config.Bind("Client Options", "DisableLobbyMusic", false);
        AktiveLobby = Config.Bind("Client Options", "EnableCustomDecorations", true);
        AktiveChat = Config.Bind("Client Options", "AktiveChat", false);
        EnableGhostChat = Config.Bind("Client Options", "EnableGhostChat", false);
        ChatOffIfImpostor = Config.Bind("Client Options", "ChatOffIfImpostor", true);
        Resize_Player = Config.Bind("Client Options", "Resize_Player", false);
        ExcludeFriends = Config.Bind("Client Options", "ExcludeFriends", false);
        AddBanToList = Config.Bind("Client Options", "AddBanToList", true);
        NoGameEnd = Config.Bind("Client Options", "NoGameEnd", false);
        EnableZoom = Config.Bind("Client Options", "EnableZoom", false);
        SeeRoleMeeting = Config.Bind("Client Options", "SeeRoleMeeting", true);
        VoteLockEnabled = Config.Bind("Client Options", "VoteLockEnabled", true);
        SwitchVanilla = Config.Bind("Client Options", "SwitchVanilla", false);
        HalloweenDecorations = Config.Bind("Decorations", "HalloweenDecorations", false, "Enable Halloween decorations");
        AnniversaryDecorations = Config.Bind("Decorations", "AnniversaryDecorations", false, "Enable Anniversary decorations");
        SeasonalModeController.Normalize();

        CustomMouse = Config.Bind("Client Options", "CustomMouse", false, "Enable or Disable Custom_Cursor");
        spoofLevel = Config.Bind("Client Options", "Level", "");
        spoofPlatform = Config.Bind("Client Options", "Platform", "", "Unknown, StandaloneEpicPC, StandaloneSteamPC, StandaloneMac, StandaloneWin10, StandaloneItch, IPhone, Android, Switch, Xbox, Playstation");
        MoveRateLimit = Config.Bind("General", "MoveRateLimit", 0,
                        "Controls how often networked movement logic runs relative to FixedUpdate calls.\n" +
                        "0 or 1 means the logic runs every FixedUpdate (no rate limiting).\n" +
                        "Values greater than 1 run the logic once every N FixedUpdate calls, reducing update frequency.");

        ClassInjector.RegisterTypeInIl2Cpp<KeyBindOptions>();
        ClassInjector.RegisterTypeInIl2Cpp<HostControl>();
        ClassInjector.RegisterTypeInIl2Cpp<ModeratorUi>();
        ClassInjector.RegisterTypeInIl2Cpp<MsgMenu>();
        ClassInjector.RegisterTypeInIl2Cpp<PlayerUI>();
        ClassInjector.RegisterTypeInIl2Cpp<SkinUI>();
        ClassInjector.RegisterTypeInIl2Cpp<NameUI>();
        ClassInjector.RegisterTypeInIl2Cpp<VisualOptions>();
        ClassInjector.RegisterTypeInIl2Cpp<PlayerTaskManager>();
        ClassInjector.RegisterTypeInIl2Cpp<BanMenuButtonsPatch>();
        ClassInjector.RegisterTypeInIl2Cpp<CustomButtonHandler>();
        ClassInjector.RegisterTypeInIl2Cpp<SpawnProtectionChecker>();
        ClassInjector.RegisterTypeInIl2Cpp<SpawnProtectionChecker1>();
        ClassInjector.RegisterTypeInIl2Cpp<PlayerPositionUpdater>();
        ClassInjector.RegisterTypeInIl2Cpp<PlayerMouseController>();
        ClassInjector.RegisterTypeInIl2Cpp<BanModUpdateHandler>();
        ClassInjector.RegisterTypeInIl2Cpp<MainMenuManagerPatch.PullingWorker>();
        ClassInjector.RegisterTypeInIl2Cpp<AnimatedSprite>();
        ClassInjector.RegisterTypeInIl2Cpp<BanModGUI>();
        ClassInjector.RegisterTypeInIl2Cpp<PremiumChatUI>();
        ClassInjector.RegisterTypeInIl2Cpp<PreviousMatchSummaryUi>();
        ClassInjector.RegisterTypeInIl2Cpp<SetPlayerUi>();
        ClassInjector.RegisterTypeInIl2Cpp<BanModCommunicationUi>();
        ClassInjector.RegisterTypeInIl2Cpp<BanModLoginUi>();
        ClassInjector.RegisterTypeInIl2Cpp<PresetMenuUi>();
        ClassInjector.RegisterTypeInIl2Cpp<ServerSelectionMenu>();
        ClassInjector.RegisterTypeInIl2Cpp<AutoFriendInviteUi>();
        ClassInjector.RegisterTypeInIl2Cpp<LiveTranslatorMenu>();
        ClassInjector.RegisterTypeInIl2Cpp<EvidenceMenu>();


        TemplateLoader.InitTemplates();
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplate");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateSns");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateKaitoRun");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateTaskRun");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateJBMode");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplatePns");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateFFA");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateFFATEAM");
        TemplateLoader.LoadWelcomeTemplate("WelcomeTemplateZombieMode");
        TemplateLoader.LoadRulesTemplate("RulesInfo");
        TemplateLoader.LoadRulesTemplate("RulesInfoSns");
        TemplateLoader.LoadRulesTemplate("RulesInfoPns");
        TemplateLoader.LoadRulesTemplate("RulesInfoKaitoRun");
        TemplateLoader.LoadRulesTemplate("RulesInfoTaskRun");
        TemplateLoader.LoadRulesTemplate("RulesInfoJBMode");
        TemplateLoader.LoadRulesTemplate("RulesInfoFFA");
        TemplateLoader.LoadRulesTemplate("RulesInfoFFATEAM");
        TemplateLoader.LoadRulesTemplate("RulesInfoZombieMode");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateSnsModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateKaitoRunModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateTaskRunModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateJBModeModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplatePnsModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateFFAModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateFFATEAMModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateKillRaceModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateAssassinModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateHotPotatoModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateRoomRushModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateTargetRushModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateDeathRunModded");
        TemplateLoader.LoadWelcomeModdedTemplate("WelcomeTemplateZombieModeModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoSnsModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoPnsModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoKaitoRunModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoTaskRunModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoJBModeModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoFFAModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoFFATEAMModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoKillRaceModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoHotPotatoModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoAssassinModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoRoomRushModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoTargetRushModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoDeathRunModded");
        TemplateLoader.LoadRulesModdedTemplate("RulesInfoZombieModeModded");

        Translator.Initialize();
        SpamManager.Initialize();
        BanManager.Initialize();
        MsgMenu.Initialize();
        OptionSaver.Initialize();
        AllowedManager.Initialize();
        LobbyInfoBar.Init(Config);
        try { CheaterManager.Initialize(); } catch (Exception ex) { try { BMLogger.LogError("[BANMOD] CheaterManager.Initialize failed: " + ex.Message); } catch { } }
        try { TeamerManager.Initialize(); } catch (Exception ex) { try { BMLogger.LogError("[BANMOD] TeamerManager.Initialize failed: " + ex.Message); } catch { } }
        SetRecommendationsPatch.LoadUserPresetFile(1);
        SetRecommendationsPatch.LoadUserPresetFile(2);
        SetRecommendationsPatch.LoadUserPresetFile(3);
        SetRecommendationsPatch.LoadUserPresetFile(4);
        keyBindOptions = AddComponent<KeyBindOptions>();
        hostControl = AddComponent<HostControl>();
        moderatorUi = AddComponent<ModeratorUi>();
        msgMenu = AddComponent<MsgMenu>();
        skinUI = AddComponent<SkinUI>();
        nameUI = AddComponent<NameUI>();
        visualOptions = AddComponent<VisualOptions>();
        playerTaskManager = AddComponent<PlayerTaskManager>();
        playerUI = AddComponent<PlayerUI>();
        setplayerUI = AddComponent<SetPlayerUi>();
        AddComponent<SpawnProtectionChecker>();
        AddComponent<SpawnProtectionChecker1>();
        AddComponent<PlayerPositionUpdater>();
        AddComponent<PlayerMouseController>();
        AddComponent<BanModUpdateHandler>();
        AddComponent<BanModGUI>();
        AddComponent<PremiumChatUI>();
        AddComponent<PreviousMatchSummaryUi>();
        AddComponent<BanModCommunicationUi>();
        AddComponent<BanModLoginUi>();
        AddComponent<PresetMenuUi>();
        AddComponent<ServerSelectionMenu>();
        AddComponent<AutoFriendInviteUi>();
        AddComponent<LiveTranslatorMenu>();
        AddComponent<EvidenceMenu>();
        LiveTranslator.Initialize(Config);

        BMLogger.LogInfo("[BanMod] BanModManager creato e avviato correttamente ✅");
        //TracersHandler.ArrowSprite = LoadSprite("BanMod.Resources.image.Arrow.png", 100f);
        if (BanMod.RoomZoneManagerInstance == null)
            BanMod.RoomZoneManagerInstance = new RoomZoneManager();

        Options.Load();
        OptionSaver.Load();
        LoadHostSetTimes();
        FixedUpdateUnifiedPatch.LoadCustomNames();
        Harmony.PatchAll();
        BMLogger.LogInfo("BanMod loaded successfully!");
        visualOptions.LoadSettings();
        BMLogger.LogInfo("BanMod loaded and settings synchronized!");
        IsBanModDisabled = false;

    }
    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    class ModManagerLateUpdatePatch
    {
        public static void Prefix(ModManager __instance)
        {
            LateTask.Update(Time.deltaTime);
        }
    }

    public class BanModUpdateHandler : MonoBehaviour
    {
        private float _nextNoGameEndWarning;
        void Update()
        {
            try
            {
                bool modOptionsOpen =
                    GameSettingMenuPatch.SettingsTab != null &&
                    GameSettingMenuPatch.SettingsTab.gameObject != null &&
                    GameSettingMenuPatch.SettingsTab.gameObject.activeInHierarchy;

                if (BanMod.NoGameEnd.Value && (GameStates.isLobby || GameStates.IsInTask))
                {
                    if (Time.realtimeSinceStartup >= _nextNoGameEndWarning &&
                        HudManager.Instance?.Notifier != null)
                    {
                        NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
                            HudManager.Instance.Notifier,
                            "WARNING: No Game End option is active"
                        );

                        _nextNoGameEndWarning = Time.realtimeSinceStartup + 60f;
                    }
                }
                else
                {
                    _nextNoGameEndWarning = 0f;
                }
                if (GameStates.isLobby)
                {
                    DleksEhtController.Apply();
                }
                if (GameStates.isLobby &&
                    Options.GameMode != null &&
                    !Options.GameMode.GetValue(GameModeType.Default) &&
                    GameManager.Instance != null &&
                    GameManager.Instance.IsHideAndSeek())
                {
                    Options.GameMode.SetValue(GameModeType.Default);
                    Options.ReOpenSettings();
                    return;
                }

                if (modOptionsOpen &&
                    GameStates.isLobby &&
                    Options.GameMode != null &&
                    (Options.GameMode.GetValue(GameModeType.FFA) || Options.GameMode.GetValue(GameModeType.FFATeam)
                    || Options.GameMode.GetValue(GameModeType.Assassin)
                    || Options.GameMode.GetValue(GameModeType.KillRace)
                    || Options.GameMode.GetValue(GameModeType.HotPotatoModded)) &&
                    !FfaExternalBridge.IsAvailable())
                {
                    Options.GameMode.SetValue(GameModeType.Default);
                    Options.ReOpenSettings();
                    return;
                }

                if (Options.Jester != null &&
                    Options.Jester.GetBool())
                {
                    Jester.Update();
                }

                if ((Input.GetKey(KeyCode.LeftControl) ||
                     Input.GetKey(KeyCode.RightControl)) &&
                    Input.GetKeyDown(KeyCode.L))
                {
                    ReconnectHandler.TryRejoin();
                }

                if ((Input.GetKey(KeyCode.LeftControl) ||
                     Input.GetKey(KeyCode.RightControl)) &&
                    Input.GetKeyDown(KeyCode.G))
                {
                    ReconnectHandler.TryNewGame();
                }

                if (IsAnyBanModMenuOpen())
                {
                    Input.ResetInputAxes();
                    return;
                }

                //if (Options.TrackImpostorTeammate == null ||
                //    !Options.TrackImpostorTeammate.GetBool())
                //{
                //    TracersHandler.HideAllArrows();
                //    return;
                //}

                //var allPlayers = BanMod.AllPlayerControls;

                //if (allPlayers == null)
                //    return;

                //foreach (var player in allPlayers)
                //{
                //    try
                //    {
                //        if (player == null ||
                //            player.Data == null ||
                //            player.Data.Disconnected)
                //        {
                //            continue;
                //        }

                //        TracersHandler.drawPlayerArrow(player);
                //    }
                //    catch (Exception e)
                //    {
                //        Debug.LogWarning(
                //            "[BanMod] Errore drawPlayerArrow: " + e
                //        );
                //    }
                //}
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "[BanMod] Errore BanModUpdateHandler.Update: " + e
                );
            }
        }

        private static bool IsAnyBanModMenuOpen()
        {
            try
            {
                if (PlayerTaskManager.Instance != null && PlayerTaskManager.Instance.showMenu)
                    return true;

                if (HostControl.Instance != null && HostControl.Instance.showMenu)
                    return true;

                if (KeyBindOptions.Instance != null && KeyBindOptions.Instance.showMenu)
                    return true;

                if (ModeratorUi.Instance != null && ModeratorUi.Instance.showMenu)
                    return true;

                if (MsgMenu.Instance != null && MsgMenu.Instance.showMenu)
                    return true;

                if (VisualOptions.Instance != null && VisualOptions.Instance.showMenu)
                    return true;

                if (BanModLoginUi.IsOpen)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BanMod] Errore IsAnyBanModMenuOpen: " + e);
                return false;
            }
        }
    }
}

// IMPORTANT NOTICE
// BANMOD - Part of the Among Us modding ecosystem.
// This file may contain code derived from other mods and their respective contributors, licensed under the GNU GPL v3.0.
//
// Credits
// Many parts of the code were created by drawing inspiration from the aforementioned mods, rewriting and implementing new functions to fit the goals of this project.
//
// - Town of Host (https://github.com/tukasa0001/TownOfHost)
// - TownofHost-Enhanced (https://github.com/EnhancedNetwork/TownofHost-Enhanced)
// - EndlessHostRoles (https://github.com/Gurge44/EndlessHostRoles)
// - AmongUsRevamped (https://github.com/ApeMV/AmongUsRevamped)
// - MalumMenu (https://github.com/scp222thj/MalumMenu)
// - TheotherRoles (https://github.com/TheOtherRolesAU/TheOtherRoles)
// - BetterAmongUs (https://github.com/D1GQ/BetterAmongUs-Public)
// - GameLogger (https://github.com/whichtwix/GameLogger)
//
// Since this mod was developed through reverse engineering (extracting, modifying, and patching code from the game's original DLL file),
// the logical structure of certain functions is constrained by the base architecture of Among Us. For this reason,
// some code segments may appear similar to those of other mods not explicitly mentioned, even though they were independently developed or adapted.
//
// License
// This project is distributed under the GNU GPL v3.0.
// In compliance with the license, the source code is available for review and modification.
// All original copyrights belong to their respective owners.
// Use of this mod implies acceptance of the GPL v3.0 terms, ensuring that derivative works also remain free and open source.
//
// This Mod includes code from:
// Copyright(c) 2018 Mark Heath, Andrew Ward & Contributors
// Licensed under the MIT License (Nlayer fold for MusicPlayer)
//
// The custom skin images are copyrighted and owned by GianniBart
