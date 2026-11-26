//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using InnerNet;
using LibCpp2IL.Elf;
using MS.Internal.Xml.XPath;
using Rewired;
using Rewired.Utils.Classes.Data;
using Rewired.Utils.Platforms.Windows;
using Sentry.Unity.NativeUtils;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using static BanMod.BMImage;
using static BanMod.ExtendedPlayerControl;
using static BanMod.GameStartManagerPatch;
using static BanMod.RoomZoneManager;
using static BanMod.SpamManager;
using static BanMod.SpawnZoneManager;
using static BanMod.Translator;
using static BanMod.Utils;
using static FilterPopUp.FilterInfoUI;
using static Il2CppMono.Security.X509.X520;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;
using static Il2CppSystem.Net.Http.Headers.Parser;
using static Il2CppSystem.Xml.Schema.FacetsChecker.FacetsCompiler;
using static InnerNet.ClientData;
using static Rewired.Data.UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo;
using static UnityEngine.GraphicsBuffer;
using Color = UnityEngine.Color;
using Convert = System.Convert;
using DateTime = System.DateTime;
using StringComparison = System.StringComparison;
using StringSplitOptions = System.StringSplitOptions;

namespace BanMod;


[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class ChatCommands
{
    public static List<string> ChatHistory = [];

    private const string Modded25CommandPrefix = "/cmd";

    private static bool IsChatCommand(string text)
    {
        return !string.IsNullOrWhiteSpace(text) &&
               text.TrimStart().StartsWith("/", StringComparison.Ordinal);
    }

    private static bool IsWrappedModded25Command(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string trimmed = text.TrimStart();

        return trimmed.Equals(
                   Modded25CommandPrefix,
                   StringComparison.OrdinalIgnoreCase
               ) ||
               trimmed.StartsWith(
                   Modded25CommandPrefix + " ",
                   StringComparison.OrdinalIgnoreCase
               );
    }

    private static string WrapModded25Command(string text)
    {
        if (!IsChatCommand(text) || IsWrappedModded25Command(text))
            return text;

        string command = text.TrimStart();

        return Modded25CommandPrefix + " " + command.Substring(1);
    }

    private static string UnwrapModded25Command(string text)
    {
        if (!IsWrappedModded25Command(text))
            return text;

        string trimmed = text.TrimStart();

        if (trimmed.Length <= Modded25CommandPrefix.Length)
            return text;

        string command = trimmed
            .Substring(Modded25CommandPrefix.Length)
            .TrimStart();

        if (string.IsNullOrEmpty(command))
            return text;

        return command.StartsWith("/", StringComparison.Ordinal)
            ? command
            : "/" + command;
    }

    [HarmonyPriority(Priority.First)]
    public static bool Prefix(ChatController __instance)
    {
        if (__instance == null) return false;
        string text = __instance.freeChatField?.textArea?.text ?? "";
        if (ChatHistory.Count == 0 || ChatHistory[^1] != text) ChatHistory.Add(text);

        ChatControllerUpdatePatch.CurrentHistorySelection = ChatHistory.Count;

        bool useModded25CommandEnvelope =
            BanModServerSelection.IsModded25 && IsChatCommand(text);

        string commandText = useModded25CommandEnvelope
            ? UnwrapModded25Command(text)
            : text;

        // Handle locally before command wrapping; never send host-only poll commands as chat.
        if (BanModPollCommands.TryHandle(commandText))
        {
            __instance.freeChatField.textArea.SetText("");
            return false;
        }

        // Votes and ordinary conversation do not enter the slash-command dispatcher.
        // ChatSendPatch and the original game still validate manual chat and cooldowns.
        if (!IsChatCommand(commandText))
            return true;

        if (__instance.timeSinceLastMessage < 1.5f && BanModServerSelection.IsVanilla)
            return false;

        if (useModded25CommandEnvelope &&
            !AmongUsClient.Instance.AmHost)
        {
            text = WrapModded25Command(commandText);
            __instance.freeChatField.textArea.text = text;
        }

        string[] args = commandText.Split(' ');
        if (args.Length == 0) return true;

        string command = args[0].ToLowerInvariant();
        string subArgs = args.Length > 1 ? args[1] : "";
        bool isPmCommand = command == "/pm" || command == "/pmall";
        if (AmongUsClient.Instance.AmHost ||
            (isPmCommand && !useModded25CommandEnvelope))
        {
            bool canceled = HandleCommand(command, args, subArgs);
            return !canceled;
        }

        return true;
    }


    public static bool insulta = true;
    public static bool superban = false;

    public static bool HandleCommand(string command, string[] args, string subArgs)
    {
        var player = PlayerControl.LocalPlayer;
        string playerName = player.Data.PlayerName;
        bool isModerator = Utils.IsModerator(player.FriendCode);
        bool IsVip = Utils.IsVip(player.FriendCode);
        var match1 = MsgMenu.buttonDataList.FirstOrDefault(b => $"/{b.Title.ToLowerInvariant()}" == command);
        byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        if (match1 != null)
        {
            string msg = match1.Message.Replace("\\n", "\n");
            Utils.SendMessage(msg);

            return true;
        }
        string lowerMsg = command.ToLower();

        switch (command)
        {
            case "/test":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;

                    if (args.Length < 2)
                    {
                        ShowChat("Uso: /test <id|color>");
                        return true;
                    }

                    PlayerControl targetPlayer = FindPlayerByIdOrColor(args[1]);

                    if (targetPlayer == null)
                    {
                        ShowChat($"Player '{args[1]}' not found.");
                        return true;
                    }
                    Utils.Explosion(targetPlayer, 3);

                    return true;
                }

            case "/rename":
                if (!AmongUsClient.Instance.AmHost)
                {
                    return true;
                }
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    StoreOriginalName(player.PlayerId);
                    player.RpcSetName(subArgs);
                    StoreModdedName(player.PlayerId);
                }
                return true;

            case "/n":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    string baseName = Regex.Replace(playerName, "<.*?>", "");
                    string friendCode = player.FriendCode;

                    if (subArgs.Equals("rainbow", StringComparison.OrdinalIgnoreCase) ||
                        subArgs.Equals("raimbow", StringComparison.OrdinalIgnoreCase))
                    {
                        StoreOriginalName(player.PlayerId);

                        string newName = RandomRainbowName(baseName);

                        player.RpcSetName(newName);
                        StoreModdedName(player.PlayerId);

                        return true;
                    }

                    string nameColorHex = GetColorHex(subArgs);

                    if (nameColorHex != null)
                    {
                        StoreOriginalName(player.PlayerId);

                        string newName = $"<color={nameColorHex}>{baseName}</color>";

                        player.RpcSetName(newName);
                        StoreModdedName(player.PlayerId);
                    }
                }
                return true;

            case "/fade":
                ApplyFade(player, args);
                return true;

            case "/host":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    string baseName = Regex.Replace(playerName, "<.*?>", "");
                    string friendCode = player.FriendCode;

                    {
                        string nameColorHex = colorMap.ContainsKey(subArgs)
                            ? colorMap[subArgs]
                            : (Regex.IsMatch(subArgs, "^#([0-9A-Fa-f]{6})$") ? subArgs : null);

                        if (nameColorHex != null)
                        {

                            StoreOriginalName(player.PlayerId);
                            string newName = $"<color={nameColorHex}>{"BanMod"}</color>";
                            player.RpcSetName(newName);
                            StoreModdedName(player.PlayerId);
                        }
                    }
                }
                return true;

            case "/host1":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    StoreOriginalName(player.PlayerId);

                    string newName =
                        "<b>" +
                        "<color=#FF5A00>B</color>" +
                        "<color=#FF9A00>A</color>" +
                        "<color=#FFD500>N</color>" +
                        "<color=#7ED957>M</color>" +
                        "<color=#3D8BFF>O</color>" +
                        "<color=#9B5CFF>D</color>" +
                        "</b>";

                    player.RpcSetName(newName);
                    StoreModdedName(player.PlayerId);
                }
                return true;

            case "/символ":
            case "/symbole":
            case "/simboli":
            case "/symbol":
                {
                    string symbolmsg1 = GetString("symbolcm1");
                    string symbolmsg2 = GetString("symbolcm2");
                    Utils.SendMessage(symbolmsg1);
                    Utils.SendMessage(symbolmsg2);
                }
                return true;

            case "/s":
            case "/d":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                    if (args.Length > 1)
                    {
                        string symbolKey = subArgs;
                        string colorArg = args.Length > 2 ? args[2].ToLowerInvariant() : "";

                        if (symbolMap.TryGetValue(symbolKey, out string rawSymbol))
                        {
                            string colorHex = colorMap.ContainsKey(colorArg)
                                ? colorMap[colorArg]
                                : (Regex.IsMatch(colorArg, "^#([0-9A-Fa-f]{6})$") ? colorArg : "#FFFFFF");

                            StoreOriginalName(player.PlayerId);
                            string coloredSymbol = $"<color={colorHex}>{rawSymbol}</color>";
                            string newName = command == "/s"
                                ? $"{coloredSymbol}{playerName}"
                                : $"{playerName}{coloredSymbol}";
                            player.RpcSetName(newName);
                            StoreModdedName(player.PlayerId);

                        }
                    }
                return true;

            case "/reset":
            case "/resetname":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;

                    if (!GameStates.isLobby || !BanModServerSelection.IsModded25)
                        return true;

                    if (args.Length == 1)
                    {
                        ResetPlayerName(player);
                        return true;
                    }

                    string targetArg = args[1].Trim();

                    if (targetArg.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var target in BanMod.AllPlayerControls.ToArray())
                        {
                            if (target == null || target.Data == null)
                                continue;

                            ResetPlayerName(target);
                        }

                        ShowChat("Nomi di tutti i player ripristinati.");
                        return true;
                    }

                    PlayerControl targetPlayer = Utils.GetTarget(targetArg);

                    if (targetPlayer == null)
                    {
                        ShowChat($"Player con colore '{targetArg}' non trovato.");
                        return true;
                    }

                    ResetPlayerName(targetPlayer);

                    return true;
                }

            case "/tpout":
            case "/esci":
                if (GameStates.isLobby) player.RpcTeleport(new Vector2(0.1f, 3.8f));
                return true;

            case "/tpin":
            case "/entra":
                if (GameStates.isLobby) player.RpcTeleport(new Vector2(-0.2f, 1.3f));
                return true;

            case "/insultaon":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    insulta = true;
                }
                return true;

            case "/insultaoff":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    insulta = false;
                }
                return true;

            case "/superbanon":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    superban = true;
                }
                return true;

            case "/superban":
                {
                    if (!AmongUsClient.Instance.AmHost) return true;
                    if (!superban) return true;
                    if (args.Length < 2)
                    {
                        return true;
                    }
                    string colorInput = args[1];
                    byte colorId = MsgToColor(colorInput);
                    if (colorId == byte.MaxValue)
                    {
                        return true;
                    }
                    PlayerControl targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                        p != null &&
                        p.Data != null &&
                        p.Data.DefaultOutfit != null &&
                        p.Data.DefaultOutfit.ColorId == colorId);
                    if (targetPlayer == null)
                    {
                        return true;
                    }
                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
                    if (client == null)
                    {
                        return true;
                    }
                    string friendCode = targetPlayer.Data?.FriendCode;
                    if (string.IsNullOrWhiteSpace(friendCode))
                        friendCode = client.FriendCode;
                    if (string.IsNullOrWhiteSpace(friendCode))
                    {
                        return true;
                    }
                    SilentPermanentFriendCodeBan.Initialize();
                    SilentPermanentFriendCodeBan.AddDeferred(friendCode);
                    superban = false;
                }
                return true;

            case "/skipmeeting":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    MeetingVoteCloser.CloseVoteNow();
                }
                return true;

            case "/infogame":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    PreviousMatchSummaryUi.ShowMenu();
                    DestroyableSingleton<ChatController>.Instance.Close();
                }
                return true;


            case "/customlobby":
            case "/cl":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    FakeMapLobbyUtility.Disable();
                    if (mapId == 0) FakeMapLobbyUtility.Enable(0);
                    else if (mapId == 1) FakeMapLobbyUtility.Enable(1);
                    else if (mapId == 2) FakeMapLobbyUtility.Enable(2);
                    else if (mapId == 3) FakeMapLobbyUtility.Enable(3);
                    else if (mapId == 4) FakeMapLobbyUtility.Enable(4);
                    else if (mapId == 5) FakeMapLobbyUtility.Enable(5);
                    return true;
                }

            case "/disablemap":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;
                    if (GameStates.IsInGameplay)
                        return true;
                    FakeMapLobbyUtility.Disable();
                    return true;
                }
            case "/setrole":
                {
                    if (AmongUsClient.Instance == null ||
                        !AmongUsClient.Instance.AmHost)
                        return true;

                    if (args.Length < 3)
                    {
                        ShowChat("Uso: /setrole <playerId> <role>");
                        return true;
                    }

                    if (!byte.TryParse(args[1], out byte playerId))
                    {
                        ShowChat("PlayerId non valido.");
                        return true;
                    }

                    if (!Enum.TryParse(args[2], true, out RoleTypes role))
                    {
                        ShowChat("Ruolo non valido.");
                        return true;
                    }

                    var target = GetPlayerById(playerId);

                    if (target == null)
                    {
                        ShowChat($"Player {playerId} non trovato.");
                        return true;
                    }

                    SetPlayerRole(target, role);

                    ShowChat(
                        $"SetRole: {target.Data.PlayerName} -> {role}"
                    );

                    return true;
                }

            case "/afk":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (!HostAfkManager.IsHostAfk)
                {
                    HostAfkManager.IsHostAfk = true;
                    ShowChat("AFK_ON");
                }
                else
                {
                    HostAfkManager.IsHostAfk = false;
                    ShowChat("AFK_OFF");
                }
                return true;

            case "/banall":
                if (!AmongUsClient.Instance.AmHost)
                    return true;

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.AmOwner) continue;

                    bool targetIsModerator = Utils.IsModerator(pc.FriendCode);
                    bool targetIsVip = Utils.IsVip(pc.FriendCode);

                    var client = pc.GetClient();

                    if (!ModeratorAuthority.CanManuallyRemove(player, client))
                        continue;

                    if (client != null)
                    {
                        BanMod.AddBanToList.Value = false;
                        AmongUsClient.Instance.KickPlayer(client.Id, true);
                        BanMod.AddBanToList.Value = true;
                    }
                }
                return true;

            case "/kickall":

                if (!AmongUsClient.Instance.AmHost)
                    return true;

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.AmOwner) continue;

                    bool targetIsModerator = Utils.IsModerator(pc.FriendCode);
                    bool targetIsVip = Utils.IsVip(pc.FriendCode);

                    var client = pc.GetClient();

                    if (!ModeratorAuthority.CanManuallyRemove(player, client))
                        continue;

                    if (client != null)
                    {
                        AmongUsClient.Instance.KickPlayer(client.Id, false);
                    }
                }
                return true;

            case "/every":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                {
                    subArgs = args.Length < 2 ? "" : args[1];
                    byte color1 = Utils.MsgToColor(subArgs, true);
                    if (color1 == byte.MaxValue)
                    {
                        return true;
                    }
                    foreach (var allplayer in PlayerControl.AllPlayerControls)
                    {
                        allplayer.RpcSetColor(color1);
                    }
                    return true;
                }

            case "/rainbowall":
                if (!AmongUsClient.Instance.AmHost) return true;

                BanMod.EveryRandomActive = !BanMod.EveryRandomActive;

                string erStatus = BanMod.EveryRandomActive ? "<color=green>ON</color>" : "<color=red>OFF</color>";
                BMLogger.SendInGame($"EveryRandom: {erStatus}");
                return false;

            case "/rainbow":
                if (!AmongUsClient.Instance.AmHost) return true;

                if (args.Length < 2)
                {
                    BanMod.EveryRandomActive = !BanMod.EveryRandomActive;
                    BanMod.RainbowTarget = null;
                    string status = BanMod.EveryRandomActive ? "<color=green>TUTTI ON</color>" : "<color=red>OFF</color>";
                    BMLogger.SendInGame($"Rainbow Mode: {status}");
                    return false;
                }

                if (byte.TryParse(args[1], out byte targetId5))
                {
                    if (BanMod.RainbowTarget != null && BanMod.RainbowTarget.PlayerId == targetId5)
                    {
                        BanMod.RainbowTarget = null;
                        BMLogger.SendInGame($"Rainbow OFF per Player {targetId5}");
                    }
                    else
                    {
                        var p = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == targetId5);
                        if (p != null)
                        {
                            BanMod.RainbowTarget = p;
                            BanMod.EveryRandomActive = false;
                            BMLogger.SendInGame($"Rainbow ON per {p.Data.PlayerName}");
                        }
                    }
                }
                return false;

            case "/setname":
                if (args.Length >= 3)
                {
                    string targetInput = args[1];
                    PlayerControl target = null;

                    if (byte.TryParse(targetInput, out byte id))
                    {
                        target = Utils.GetPlayerById(id);
                    }

                    if (target == null)
                    {
                        byte colorId = MsgToColor(targetInput);
                        target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p =>
                            p.Data != null && p.Data.DefaultOutfit.ColorId == colorId);
                    }

                    if (target != null)
                    {
                        string rawName = string.Join(" ", args, 2, args.Length - 2);
                        string newCustomName = Utils.MakeRainbowName(rawName);
                        string fCode = target.Data.FriendCode;

                        FixedUpdateUnifiedPatch.CustomNames[fCode] = newCustomName;

                        {
                            List<string> lines = new List<string>();
                            foreach (var kvp in FixedUpdateUnifiedPatch.CustomNames)
                            {
                                lines.Add($"{kvp.Key}:{kvp.Value}");
                            }

                            File.WriteAllLines("DATA/OTHER/NAME/CustomNames.txt", lines);

                            ShowChat($"{target.Data.PlayerName} changed to: {newCustomName}");
                        }
                    }
                }
                return true;

            case "/start":
                {
                    bool oldNocountdown = Options.nocountdown.GetBool();
                    var manager = UnityEngine.Object.FindObjectOfType<GameStartManager>();
                    if (manager != null)
                    {
                        manager.BeginGame();
                    }
                    return true;
                }

            case "/instantstart":
                {
                    var manager = UnityEngine.Object.FindObjectOfType<GameStartManager>();
                    BanMod.instantstart = true;
                    if (manager != null)
                    {
                        manager.BeginGame();
                    }
                    BanMod.instantstart = false;
                    return true;
                }

            case "/deleterole":
                {
                    BanMod.forcedImpostorIds.Clear();
                    ForcedRoleSystem.Clear();
                    BanMod.forceImpostor = false;
                    Jester.JesterId = 255;
                    Jester.JesterSelected = false;
                    Guesser.SpecialKillerId = 255;
                    Guesser.SpecialKillerSelected = false;
                    Exiler.ExilerId = 255;
                    Exiler.ExilerSelected = false;
                    ChatCommands.ShowChat("<color=#00ffff>Forced roles cleared for new game.</color>");

                    return true;
                }
            case "/insulta":
                {
                    if (args.Length < 2)
                    {
                        ShowChat("Uso corretto: /insulta <nome>");
                        return true;
                    }

                    string target = args[1];

                    string insulto = PrendiInsulto();

                    string msg = $"{target}, {insulto}";

                    Utils.SendMessage(msg);

                    return false;
                }

            case "/bbm":
                {
                    if (!AmongUsClient.Instance.AmHost || GameStates.isLobby || player.Data.IsDead)
                        return true;

                    if (args.Length >= 2)
                    {
                        PlayerControl targetPlayer = Utils.GetTarget(args[1]);

                        if (targetPlayer != null)
                        {
                            bool success = RolesCommand.Cmd(player.PlayerId, targetPlayer.PlayerId);

                            if (!success)
                            {
                                string msg = string.Format(GetString("NeutralInfo"));
                                Utils.SendMessage(msg, player.PlayerId);
                            }
                        }
                    }
                    return false;
                }
            case "/bm":
                {
                    if (!AmongUsClient.Instance.AmHost || GameStates.isLobby || player.Data.IsDead)
                        return true;

                    if (args.Length >= 2)
                    {
                        PlayerControl targetPlayer = Utils.GetTarget(args[1]);

                        if (targetPlayer != null)
                        {
                            bool success = RolesCommand.Cmd(player.PlayerId, targetPlayer.PlayerId);

                            if (!success)
                            {
                                string msg = string.Format(GetString("NeutralInfo"));
                                Utils.SendMessage(msg, player.PlayerId);
                            }
                        }
                    }
                    return false;
                }

            case "/scanner":
                {
                    if (args.Length < 2)
                    {
                        ShowChat("Uso: /scanner on|off [durata_in_secondi]");
                        return true;
                    }

                    string state = args[1].ToLowerInvariant();
                    float duration = 5f;

                    if (args.Length >= 3 && float.TryParse(args[2], out float parsedDuration))
                        duration = parsedDuration;

                    if (state == "on")
                    {
                        HudManager.Instance.StartCoroutine(CheatUtils.BypassScannerWithTimeout(duration));
                        ShowChat($"<color=#00ff00>Scanner bypass attivato per {duration} secondi.</color>");
                    }
                    else if (state == "off")
                    {
                        CheatUtils.BypassScanner(false);
                        ShowChat("<color=#ff0000>Scanner bypass disattivato.</color>");
                    }
                    else
                    {
                        ShowChat("Valore non valido! Usa 'on' o 'off'.");
                    }

                    return true;
                }

            case "/fix":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, 69);
                ShipStatus_FixedUpdate_Patch.FixAllSabotages(ShipStatus.Instance);
                return true;


            case "/endgame":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                BanMod.EndGameForced = true;
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
                return true;

            case "/t":
            case "/r":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                SendRules();
                return true;

            case "/ban":
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        return true;
                    }

                    if (args.Length < 2)
                    {
                        ShowChat("Usage: /ban <id|name|color> [reason]");
                        return true;
                    }

                    string targetInput = args[1];
                    string normalizedTargetInput = NameNormalizer.NormalizeInputName(targetInput);


                    PlayerControl targetPlayer = null;

                    if (int.TryParse(targetInput, out int targetId))
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p => p.PlayerId == targetId);
                    }

                    if (targetPlayer == null)
                    {
                        byte colorId = MsgToColor(targetInput);
                        if (colorId != byte.MaxValue)
                        {
                            targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                                p != null && p.Data != null && p.Data.DefaultOutfit.ColorId == colorId);
                        }
                    }

                    if (targetPlayer == null)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null &&
                            NameNormalizer.NormalizeInputName(p.Data.PlayerName)
                                .Equals(normalizedTargetInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (targetPlayer == null)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null &&
                            PlayerControlStartUnifiedPatch.PlayerNamesByFriendCode.TryGetValue(p.Data.FriendCode, out string originalName) &&
                            NameNormalizer.NormalizeInputName(originalName)
                                .Equals(normalizedTargetInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (targetPlayer == null)
                    {
                        ShowChat($"Player '{targetInput}' not found.");
                        return true;
                    }

                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
                    if (client == null)
                    {
                        ShowChat("Client data not found for target player.");
                        return true;
                    }
                    if (!ModeratorAuthority.CanManuallyRemove(player, client))
                    {
                        return true;
                    }

                    string reason = args.Length >= 3 ? string.Join(" ", args.Skip(2)).Trim() : "No reason provided";
                    string name1 = targetPlayer.name;

                    BanManager.AddBanPlayer(client, reason, false);
                    AmongUsClient.Instance.KickPlayer(client.Id, true);

                    NotificationPopper_AddInfoMessagePatch.AddInfoMessage(HudManager.Instance.Notifier, $"{name1} {GetString("banned")} {GetString("Reason")}: {reason}");
                    Utils.SendMessage($"{name1} {GetString("banned")}\n{GetString("Reason")}: {reason}");

                    return true;
                }
            case "/team":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;

                    if (args.Length < 2)
                    {
                        ShowChat("Uso: /team <id|color> [reason]");
                        return true;
                    }

                    string reason = args.Length >= 3 ? string.Join(" ", args.Skip(2)).Trim() : "Teaming";

                    PlayerControl targetPlayer = FindPlayerByIdOrColor(args[1]);

                    if (targetPlayer == null)
                    {
                        ShowChat($"Player '{args[1]}' not found.");
                        return true;
                    }

                    if (!BanAndAddTeamer(targetPlayer, reason))
                        ShowChat("Impossible bann/add teamer.");

                    return true;
                }
            case "/unban":
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        return true;
                    }

                    if (args.Length < 2)
                    {
                        ShowChat("Usage: /ban <id|name|color> [reason]");
                        return true;
                    }

                    string targetInput = args[1];
                    string normalizedTargetInput = NameNormalizer.NormalizeInputName(targetInput);


                    PlayerControl targetPlayer = null;

                    if (int.TryParse(targetInput, out int targetId))
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p => p.PlayerId == targetId);
                    }

                    if (targetPlayer == null)
                    {
                        byte colorId = MsgToColor(targetInput);
                        if (colorId != byte.MaxValue)
                        {
                            targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                                p != null && p.Data != null && p.Data.DefaultOutfit.ColorId == colorId);
                        }
                    }

                    if (targetPlayer == null)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null &&
                            NameNormalizer.NormalizeInputName(p.Data.PlayerName)
                                .Equals(normalizedTargetInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (targetPlayer == null)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null &&
                            PlayerControlStartUnifiedPatch.PlayerNamesByFriendCode.TryGetValue(p.Data.FriendCode, out string originalName) &&
                            NameNormalizer.NormalizeInputName(originalName)
                                .Equals(normalizedTargetInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (targetPlayer == null)
                    {
                        ShowChat($"Player '{targetInput}' not found.");
                        return true;
                    }

                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
                    if (client == null)
                    {
                        ShowChat("Client data not found for target player.");
                        return true;
                    }


                    string name1 = targetPlayer.name;
                    BanManager.RemoveBanPlayerFromBanList(client);
                    NotificationPopper_AddInfoMessagePatch.AddInfoMessage(HudManager.Instance.Notifier, $"{name1} Unbanned");


                    return true;
                }
            case "/kick":
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        return true;
                    }
                    if (args.Length < 2)
                    {
                        ShowChat("Usage: /kick <id|name|color> [reason]");
                        return true;
                    }

                    string targetInput = args[1];
                    string normalizedTargetInput = NameNormalizer.NormalizeInputName(targetInput);


                    PlayerControl targetPlayer = null;

                    if (int.TryParse(targetInput, out int targetId))
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p => p.PlayerId == targetId);
                    }

                    if (targetPlayer == null)
                    {
                        byte colorId = MsgToColor(targetInput);
                        if (colorId != byte.MaxValue)
                        {
                            targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                                p != null && p.Data != null && p.Data.DefaultOutfit.ColorId == colorId);
                        }
                    }

                    if (targetPlayer == null)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null &&
                            NameNormalizer.NormalizeInputName(p.Data.PlayerName)
                                .Equals(normalizedTargetInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (targetPlayer == null)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null &&
                            PlayerControlStartUnifiedPatch.PlayerNamesByFriendCode.TryGetValue(p.Data.FriendCode, out string originalName) &&
                            NameNormalizer.NormalizeInputName(originalName)
                                .Equals(normalizedTargetInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (targetPlayer == null)
                    {
                        ShowChat($"Player '{targetInput}' not found.");
                        return true;
                    }

                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
                    if (client == null)
                    {
                        ShowChat("Client data not found for target player.");
                        return true;
                    }
                    if (!ModeratorAuthority.CanManuallyRemove(player, client))
                    {
                        return true;
                    }

                    string reason = args.Length >= 3 ? string.Join(" ", args.Skip(2)).Trim() : "No reason provided";
                    string name1 = targetPlayer.name;

                    AmongUsClient.Instance.KickPlayer(client.Id, false);

                    NotificationPopper_AddInfoMessagePatch.AddInfoMessage(HudManager.Instance.Notifier, $"{name1} {GetString("HasBeenKicked")} {GetString("Reason")}: {reason}");
                    Utils.SendMessage($"{name1} {GetString("HasBeenKicked")}\n{GetString("Reason")}: {reason}");

                    return true;
                }

            case "/endmeeting":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                PlayerControl.LocalPlayer.StartCoroutine(Utils.DelayedCloseMeeting());
                return true;

            case "/lp":
            case "/livelli":
                {
                    if (!AmongUsClient.Instance.AmHost)
                        return true;

                    string levelList = GetString("PlayerLevelsTitle");

                    var allPlayers = GameData.Instance.AllPlayers;

                    for (int i = 0; i < allPlayers.Count; i++)
                    {
                        var playerInfo = allPlayers[i];
                        if (playerInfo == null) continue;

                        byte id = playerInfo.PlayerId;
                        string name = playerInfo.PlayerName ?? "???";
                        uint playerLevel = playerInfo.PlayerLevel;

                        string levelStr = (playerLevel == uint.MaxValue) ? "??? (not_sync)" : playerLevel.ToString();

                        levelList += $"{name} → {GetString("Level")} : <color=#00ff00>{levelStr}</color>\n";
                    }

                    ShowChat(levelList);
                    return true;
                }


            case "/exeme":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                Utils.Exeme();
                return true;

            case "/meeting":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                player.CmdReportDeadBody(null);
                return true;

            case "/close":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                MeetingHud.Instance.Close();
                MeetingHud.Instance.RpcClose();
                return true;

            case "/killme":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                Utils.KillPlayer(player);
                return true;


            case "/time":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                Scientist.ScientistCommandHost();
                return true;


            case "/summary":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                {
                    string report1 = MatchSummary1.GetSummaryReport();
                    {
                        Utils.SendMessage(report1, 255);
                    }
                    return true;
                }
            case "/info":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                subArgs = args.Length < 2 ? "" : args[1];
                subArgs = args.Length < 2 ? "" : args[1].ToLowerInvariant();
                switch (subArgs)
                {
                    case "giustiziere":
                    case "guesser":
                    case "guess":
                    case "giustiz":
                    case "g":
                    case "devin":
                    case "vermuten":
                    case "Предсказатель":
                        bool isGuessEnabled = Options.Guess.GetBool();
                        string statoGuess = isGuessEnabled ? "On" : "Off";
                        string msgGuess =
                            $"{GetString("GuesserDescription")}\n" +
                            $"{GetString("ModEnabled")} {statoGuess}";

                        Utils.SendMessage(msgGuess, 255);
                        MessageBlocker.UpdateLastMessageTime();

                        return true;

                    case "presidente":
                    case "president":
                    case "exiler":
                    case "p":
                    case "président":
                    case "präsident":
                    case "президент":
                        bool isExilerEnabled = Options.ExilerExe.GetBool();
                        bool isExilerKilled = Options.killexiler.GetBool();
                        string action = Options.ExilerAction.GetString();
                        string statoExiler = isExilerEnabled ? "On" : "Off";
                        string statoExilerK = isExilerKilled ? "On" : "Off";
                        string msgExiler =
                            $"{GetString("ModEnabled")}: {statoExiler}\n" +
                            $"{GetString("Consequence")}: {statoExilerK}\n" +
                            $"{GetString("Action")} {action}";

                        Utils.SendMessage(msgExiler, 255);
                        MessageBlocker.UpdateLastMessageTime();
                        Utils.SendMessage(GetString("exiler.cm"), 255);
                        MessageBlocker.UpdateLastMessageTime();
                        return true;

                    case "spettro":
                    case "fantasma":
                    case "phantom":
                    case "ph":
                    case "fantôme":
                    case "geist":
                    case "призрак":
                        {
                            var optionsPha = GameOptionsManager.Instance.CurrentGameOptions;
                            float PhantomCooldown = 1f;
                            float PhantomDuration = 1f;
                            float killCooldown = 1f;
                            int phantomCount = optionsPha.RoleOptions.GetNumPerGame(RoleTypes.Phantom);
                            int phantomChance = optionsPha.RoleOptions.GetChancePerGame(RoleTypes.Phantom);

                            if (optionsPha != null)
                            {
                                optionsPha.TryGetFloat(FloatOptionNames.PhantomCooldown, out PhantomCooldown);
                                optionsPha.TryGetFloat(FloatOptionNames.PhantomDuration, out PhantomDuration);
                                optionsPha.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                                phantomCount = optionsPha.RoleOptions.GetNumPerGame(RoleTypes.Phantom);
                                phantomChance = optionsPha.RoleOptions.GetChancePerGame(RoleTypes.Phantom);
                            }

                            bool isPhantomEnabled = Options.PhantomGuess.GetBool();

                            if (isPhantomEnabled)
                            {
                                string msgPha =
                                $"{GetString("MaxPerGame")}: {phantomCount}\n" +
                                $"{GetString("Probability")}: {phantomChance}%\n" +
                                $"{GetString("Cooldown")}: {PhantomCooldown}s\n" +
                                $"{GetString("DurationPhantom")}: {PhantomDuration}s\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";
                                Utils.SendMessage(msgPha, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("PhantomDescription"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgPha =
                                $"{GetString("MaxPerGame")}: {phantomCount}\n" +
                                $"{GetString("Probability")}: {phantomChance}%\n" +
                                $"{GetString("Cooldown")}: {PhantomCooldown}s\n" +
                                $"{GetString("DurationPhantom")}: {PhantomDuration}s\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";
                                Utils.SendMessage(msgPha, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return true;
                        }

                    case "immortale":
                    case "immortal":
                    case "imm":
                    case "immortel":
                    case "unsterblich":
                    case "бессмертный":
                        bool isImmortalEnabled = Options.EnableImmortal.GetBool();
                        bool isImmortalesentEnabled = Options.Immortalesentvote.GetBool();
                        string statoImmortal = isImmortalEnabled ? "On" : "Off";
                        string statoesent = isImmortalesentEnabled ? "On" : "Off";
                        string msgImmortal =
                            $"{GetString("ModEnabled")}: {statoImmortal}\n" +
                            $"{GetString("VoteEsent")}: {statoesent}";

                        Utils.SendMessage(msgImmortal, 255);
                        MessageBlocker.UpdateLastMessageTime();
                        Utils.SendMessage(GetString("ImmortalDescription"), 255);
                        MessageBlocker.UpdateLastMessageTime();
                        return true;

                    case "ing":
                    case "ingegnere":
                    case "engineer":
                    case "eng":
                    case "ingénieur":
                    case "ingenieur":
                    case "инженер":
                        {
                            var optionsIng = GameOptionsManager.Instance.CurrentGameOptions;
                            float engineerCooldown = 1f;
                            float engineerInVentTime = 1f;
                            int engineerCount = optionsIng.RoleOptions.GetNumPerGame(RoleTypes.Engineer);
                            int engineerChance = optionsIng.RoleOptions.GetChancePerGame(RoleTypes.Engineer);
                            if (optionsIng != null)
                            {
                                optionsIng.TryGetFloat(FloatOptionNames.EngineerCooldown, out engineerCooldown);
                                optionsIng.TryGetFloat(FloatOptionNames.EngineerInVentMaxTime, out engineerInVentTime);
                                engineerCount = optionsIng.RoleOptions.GetNumPerGame(RoleTypes.Engineer);
                                engineerChance = optionsIng.RoleOptions.GetChancePerGame(RoleTypes.Engineer);
                            }

                            bool isEngineerFixerEnabled = Options.EngineerFixer.GetBool();
                            int ventFixAttempts = Options.VentTimes.GetInt();
                            string FormatVentTime(float time)
                            {
                                return time == 0f ? "∞" : $"{time:0.0}s";
                            }
                            if (isEngineerFixerEnabled)
                            {
                                string msg =
                                $"{GetString("MaxPerGame")}: {engineerCount}\n" +
                                $"{GetString("Probability")}: {engineerChance}%\n" +
                                $"{GetString("Cooldown")}: {engineerCooldown:0.0}s\n" +
                                $"{GetString("VentTime")}: {FormatVentTime(engineerInVentTime)}";
                                string msg2 =
                                $"{GetString("EngineerDescription")}\n\n" +
                                $"{GetString("AvailableFixes")}: {ventFixAttempts}";

                                Utils.SendMessage(msg, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(msg2, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msg =
                                $"{GetString("MaxPerGame")}: {engineerCount}\n" +
                                $"{GetString("Probability")}: {engineerChance}%\n" +
                                $"{GetString("Cooldown")}: {engineerCooldown:0.0}s\n" +
                                $"{GetString("VentTime")}: {FormatVentTime(engineerInVentTime)}";
                                Utils.SendMessage(msg, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return true;
                        }


                    case "scienziato":
                    case "scientist":
                    case "sci":
                    case "scientifique":
                    case "wissenschaftler":
                    case "учёный":
                        {
                            var optionsScie = GameOptionsManager.Instance.CurrentGameOptions;
                            float ScientistCooldown = 1f;
                            float ScientistBatteryCharge = 1f;
                            int scientistCount = optionsScie.RoleOptions.GetNumPerGame(RoleTypes.Scientist);
                            int scientistChance = optionsScie.RoleOptions.GetChancePerGame(RoleTypes.Scientist);

                            if (optionsScie != null)
                            {
                                optionsScie.TryGetFloat(FloatOptionNames.ScientistCooldown, out ScientistCooldown);
                                optionsScie.TryGetFloat(FloatOptionNames.ScientistBatteryCharge, out ScientistBatteryCharge);
                                scientistCount = optionsScie.RoleOptions.GetNumPerGame(RoleTypes.Scientist);
                                scientistChance = optionsScie.RoleOptions.GetChancePerGame(RoleTypes.Scientist);
                            }

                            bool isScientistEnabled = Options.ScientistTime.GetBool();
                            if (isScientistEnabled)
                            {
                                string msgScie =
                                $"{GetString("MaxPerGame")}: {scientistCount}\n" +
                                $"{GetString("Probability")}: {scientistChance}%\n" +
                                $"{GetString("Cooldown")}: {ScientistCooldown:0.0}s\n" +
                                $"{GetString("VitalsTime")}: {ScientistBatteryCharge:0.0}s";
                                Utils.SendMessage(msgScie, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("ScientistDescription"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgScie =
                                $"{GetString("MaxPerGame")}: {scientistCount}\n" +
                                $"{GetString("Probability")}: {scientistChance}%\n" +
                                $"{GetString("Cooldown")}: {ScientistCooldown:0.0}s\n" +
                                $"{GetString("VitalsTime")}: {ScientistBatteryCharge:0.0}s";
                                Utils.SendMessage(msgScie, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return true;
                        }

                    case "lobby":
                        {
                            var options = GameOptionsManager.Instance.CurrentGameOptions;

                            bool confirmImpostorValue = false;
                            bool visualTasks = false;
                            bool anonymousVotes = false;
                            float crewLightMod = 1f;
                            float impostorLightMod = 1f;
                            float killCooldown = 1f;

                            if (options != null)
                            {
                                options.TryGetBool(BoolOptionNames.ConfirmImpostor, out confirmImpostorValue);
                                options.TryGetBool(BoolOptionNames.VisualTasks, out visualTasks);
                                options.TryGetBool(BoolOptionNames.AnonymousVotes, out anonymousVotes);
                                options.TryGetFloat(FloatOptionNames.CrewLightMod, out crewLightMod);
                                options.TryGetFloat(FloatOptionNames.ImpostorLightMod, out impostorLightMod);
                                options.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                            }

                            string onOff(bool val) => val ? "On" : "Off";
                            string msgLobby =
                                $"{GetString("ConfirmImpostor")}:{onOff(confirmImpostorValue)}\n" +
                                $"{GetString("VisualTasks")}:{onOff(visualTasks)}\n" +
                                $"{GetString("AnonymousVotes")}:{onOff(anonymousVotes)}\n" +
                                $"{GetString("CrewmateVision")}:{crewLightMod}\n" +
                                $"{GetString("ImpostorVision")}:{impostorLightMod}\n" +
                                $"{GetString("KillCooldown")}:{killCooldown}";

                            Utils.SendMessage(msgLobby);
                            MessageBlocker.UpdateLastMessageTime();
                            return true;
                        }

                    case "shapeshifter":
                    case "shape":
                    case "ss":
                    case "mutaforma":
                    case "muta":
                        {
                            var optionsShape = GameOptionsManager.Instance.CurrentGameOptions;
                            float ShapeshifterCooldown = 1f;
                            float ShapeshifterDuration = 1f;
                            bool ShapeshifterLeaveSkin = false;
                            float killCooldown = 1f;
                            int shapeCount = optionsShape.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter);
                            int shapeChance = optionsShape.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter);
                            string FormatDurationTime(float time)
                            {
                                return time == 0f ? "∞" : $"{time:0.0}s";
                            }
                            if (optionsShape != null)
                            {
                                optionsShape.TryGetFloat(FloatOptionNames.ShapeshifterCooldown, out ShapeshifterCooldown);
                                optionsShape.TryGetFloat(FloatOptionNames.ShapeshifterDuration, out ShapeshifterDuration);
                                optionsShape.TryGetBool(BoolOptionNames.ShapeshifterLeaveSkin, out ShapeshifterLeaveSkin);
                                optionsShape.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                                shapeCount = optionsShape.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter);
                                shapeChance = optionsShape.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter);
                            }

                            bool isShapeEnabled = Options.ShapeGuess.GetBool();
                            string statoShapevisible = ShapeshifterLeaveSkin ? "On" : "Off";
                            if (isShapeEnabled)
                            {
                                string msgShape =
                                $"{GetString("MaxPerGame")}: {shapeCount}\n" +
                                $"{GetString("Probability")}: {shapeChance}%\n" +
                                $"{GetString("Cooldown")}: {ShapeshifterCooldown}s\n" +
                                $"{GetString("DurationShape")}: {FormatDurationTime(ShapeshifterDuration)}\n" +
                                $"{GetString("SkinShape")}: {ShapeshifterLeaveSkin}\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";

                                Utils.SendMessage(msgShape, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("ShapeshifterDescription"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgShape =
                                $"{GetString("MaxPerGame")}: {shapeCount}\n" +
                                $"{GetString("Probability")}: {shapeChance}%\n" +
                                $"{GetString("Cooldown")}: {ShapeshifterCooldown}s\n" +
                                $"{GetString("DurationShape")}: {FormatDurationTime(ShapeshifterDuration)}\n" +
                                $"{GetString("SkinShape")}: {ShapeshifterLeaveSkin}\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";
                                Utils.SendMessage(msgShape, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return true;
                        }

                    case "detective":
                        {
                            var optionsDet = GameOptionsManager.Instance.CurrentGameOptions;
                            float DetectiveSuspectLimit = 1f;
                            int detectiveCount = optionsDet.RoleOptions.GetNumPerGame(RoleTypes.Detective);
                            int detectiveChance = optionsDet.RoleOptions.GetChancePerGame(RoleTypes.Detective);

                            if (optionsDet != null)
                            {
                                optionsDet.TryGetFloat(FloatOptionNames.DetectiveSuspectLimit, out DetectiveSuspectLimit);
                                detectiveCount = optionsDet.RoleOptions.GetNumPerGame(RoleTypes.Detective);
                                detectiveChance = optionsDet.RoleOptions.GetChancePerGame(RoleTypes.Detective);
                            }

                            string msgDet =
                                $"{GetString("MaxPerGame")}: {detectiveCount}\n" +
                                $"{GetString("Probability")}: {detectiveChance}%\n" +
                                $"{GetString("DetectiveSuspectLimit")}: {DetectiveSuspectLimit}";

                            Utils.SendMessage(msgDet, 255);
                            MessageBlocker.UpdateLastMessageTime();
                            return true;
                        }

                    case "cobra":
                    case "viper":
                        {
                            var optionsCo = GameOptionsManager.Instance.CurrentGameOptions;
                            float ViperDissolveTime = 1f;
                            float killCooldown = 1f;
                            int viperCount = optionsCo.RoleOptions.GetNumPerGame(RoleTypes.Viper);
                            int viperChance = optionsCo.RoleOptions.GetChancePerGame(RoleTypes.Viper);

                            if (optionsCo != null)
                            {
                                optionsCo.TryGetFloat(FloatOptionNames.ViperDissolveTime, out ViperDissolveTime);
                                optionsCo.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                                viperCount = optionsCo.RoleOptions.GetNumPerGame(RoleTypes.Viper);
                                viperChance = optionsCo.RoleOptions.GetChancePerGame(RoleTypes.Viper);
                            }

                            bool isViperEnabled = Options.ViperGuess.GetBool();

                            if (isViperEnabled)
                            {
                                string msgVip =
                                $"{GetString("MaxPerGame")}:{viperCount}\n" +
                                $"{GetString("Probability")}:{viperChance}%\n" +
                                $"{GetString("ViperDissolveTime")}:{ViperDissolveTime}s\n" +
                                $"{GetString("KillCooldown")}:{killCooldown}s";
                                Utils.SendMessage(msgVip, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("viper.cm"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgVip1 =
                                $"{GetString("MaxPerGame")}:{viperCount}\n" +
                                $"{GetString("Probability")}:{viperChance}%\n" +
                                $"{GetString("ViperDissolveTime")}:{ViperDissolveTime}s\n" +
                                $"{GetString("KillCooldown")}:{killCooldown}s";
                                Utils.SendMessage(msgVip1, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return true;
                        }

                    case "starnazzatore":
                    case "noisemaker":
                        {
                            var optionsNoi = GameOptionsManager.Instance.CurrentGameOptions;
                            float NoisemakerAlertDuration = 1f;
                            bool NoisemakerImpostorAlert = false;
                            int noisemakerCount = optionsNoi.RoleOptions.GetNumPerGame(RoleTypes.Noisemaker);
                            int noisemakerChance = optionsNoi.RoleOptions.GetChancePerGame(RoleTypes.Noisemaker);
                            if (optionsNoi != null)
                            {
                                optionsNoi.TryGetFloat(FloatOptionNames.NoisemakerAlertDuration, out NoisemakerAlertDuration);
                                optionsNoi.TryGetBool(BoolOptionNames.NoisemakerImpostorAlert, out NoisemakerImpostorAlert);
                                noisemakerCount = optionsNoi.RoleOptions.GetNumPerGame(RoleTypes.Noisemaker);
                                noisemakerChance = optionsNoi.RoleOptions.GetChancePerGame(RoleTypes.Noisemaker);
                            }

                            string msgNoi =
                                $"{GetString("MaxPerGame")}: {noisemakerCount}\n" +
                                $"{GetString("Probability")}: {noisemakerChance}%\n" +
                                $"{GetString("NoisemakerAlertDuration")}: {NoisemakerAlertDuration}s\n" +
                                $"{GetString("NoisemakerImpostorAlert")}: {NoisemakerImpostorAlert}";

                            Utils.SendMessage(msgNoi, 255);
                            MessageBlocker.UpdateLastMessageTime();
                            return true;
                        }

                    case "guardian":
                    case "angel":
                    case "angelo":
                        {
                            var optionsAng = GameOptionsManager.Instance.CurrentGameOptions;
                            float GuardianAngelCooldown = 1f;
                            float ProtectionDurationSeconds = 1f;
                            int angelCount = optionsAng.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel);
                            int angelChance = optionsAng.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel);

                            if (optionsAng != null)
                            {
                                optionsAng.TryGetFloat(FloatOptionNames.GuardianAngelCooldown, out GuardianAngelCooldown);
                                optionsAng.TryGetFloat(FloatOptionNames.ProtectionDurationSeconds, out ProtectionDurationSeconds);
                                angelCount = optionsAng.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel);
                                angelChance = optionsAng.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel);
                            }

                            string msgAng =
                                $"{GetString("MaxPerGame")}: {angelCount}\n" +
                                $"{GetString("Probability")}: {angelChance}%\n" +
                                $"{GetString("Cooldown")}: {GuardianAngelCooldown}s\n" +
                                $"{GetString("ProtectionDurationSeconds")}: {ProtectionDurationSeconds}s";

                            Utils.SendMessage(msgAng, 255);
                            MessageBlocker.UpdateLastMessageTime();
                            return true;
                        }

                    default:
                        return true;
                }



            case "/m":
                if (GameStates.isLobby) return true;
                bool isSpecialKiller1 = Options.Guess.GetBool() && PlayerControl.LocalPlayer.PlayerId == Guesser.SpecialKillerId;
                bool isJester1 = Options.Jester.GetBool() && PlayerControl.LocalPlayer.PlayerId == Jester.JesterId;
                bool isPresident1 = Options.ExilerExe.GetBool() && PlayerControl.LocalPlayer.PlayerId == Exiler.ExilerId;
                bool isScientist1 = Options.ScientistTime.GetBool() && Scientist(PlayerControl.LocalPlayer);
                bool isPhantom1 = Options.PhantomGuess.GetBool() && Phantom(PlayerControl.LocalPlayer);
                bool isEngineer1 = Options.EngineerFixer.GetBool() && Engineer(PlayerControl.LocalPlayer) && (!isJester1);
                bool isImmortal1 = Options.EnableImmortal.GetBool() && ImmortalManager.IsImmortal(PlayerControl.LocalPlayer.PlayerId);
                bool Shapeshifter1 = Options.ShapeGuess.GetBool() && Shapeshifter(PlayerControl.LocalPlayer);
                bool isCobra1 = Options.ViperGuess.GetBool() && Cobra(PlayerControl.LocalPlayer);
                bool isImpostor1 = Options.ImpostorGuess.GetBool() && Impostor(PlayerControl.LocalPlayer);

                if (isEngineer1)
                {
                    Engineer.SendEngineerMessage();
                }
                if (Shapeshifter1)
                {
                    ImpostorGuesser.SendShapePlayerMessage();
                }
                if (isPhantom1)
                {
                    ImpostorGuesser.SendPhantomPlayerMessage();
                }
                if (isImpostor1)
                {
                    ImpostorGuesser.SendImpostorPlayerMessage();
                }
                if (isCobra1)
                {
                    ImpostorGuesser.SendViperPlayerMessage();
                }
                if (isScientist1)
                {
                    Scientist.SendScientistMessage();
                }
                if (isSpecialKiller1)
                {
                    Guesser.SendKillerMessage();
                }
                if (isJester1)
                {
                    Jester.SendJesterMessage();
                }
                if (isPresident1)
                {
                    Exiler.SendExilerMessage();
                }
                if (isImmortal1)
                {
                    string msg = GetString("ImmortalSelfMessage");
                    if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
                    {
                        Utils.RequestProxyMessage(msg, player.PlayerId);
                        MessageBlocker.UpdateLastMessageTime();
                    }
                    else
                    {
                        Utils.SendMessage(msg, player.PlayerId);
                        MessageBlocker.UpdateLastMessageTime();
                    }
                }
                if (!isSpecialKiller1 && !isJester1 && !isPresident1 && !isScientist1 && !isPhantom1 && !isEngineer1 && !isImmortal1 && !Shapeshifter1 && !isCobra1 && !isImpostor1)
                {
                    string msg = string.Format(GetString("NeutralInfo"));
                    Utils.SendMessage(msg, PlayerControl.LocalPlayer.PlayerId);
                    MessageBlocker.UpdateLastMessageTime();

                }
                return true;

            case "/role":
                if (!AmongUsClient.Instance.AmHost)
                    return true;

                if (Options.EngineerFixer.GetBool())
                {
                    Engineer.SendEngineerMessage();
                }
                if (Options.PhantomGuess.GetBool())
                {
                    ImpostorGuesser.SendPhantomPlayerMessage();
                }
                if (Options.ViperGuess.GetBool())
                {
                    ImpostorGuesser.SendViperPlayerMessage();
                }
                if (Options.ShapeGuess.GetBool())
                {
                    ImpostorGuesser.SendShapePlayerMessage();
                }
                if (Options.ScientistTime.GetBool())
                {
                    Scientist.SendScientistMessage();
                }
                if (Options.Guess.GetBool())
                {
                    Guesser.SendKillerMessage();
                }
                if (Options.Jester.GetBool())
                {
                    Jester.SendJesterMessage();
                }
                if (Options.ExilerExe.GetBool())
                {
                    Exiler.SendExilerMessage();
                }
                return true;

            case "/coms":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (PlayerControl.LocalPlayer != null && (
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Phantom ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Viper ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Shapeshifter))
                {
                    SabotageManager.TryActivateSabotage(SystemTypes.Comms, 128);
                }
                return true;


            case "/o2":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (PlayerControl.LocalPlayer != null && (
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Phantom ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Viper ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Shapeshifter))
                {
                    SabotageManager.TryActivateSabotage(SystemTypes.LifeSupp, 128);
                }
                return true;

            case "/reactor":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (PlayerControl.LocalPlayer != null && (
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Phantom ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Viper ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Shapeshifter))
                {
                    SabotageManager.TryActivateSabotage(SystemTypes.Reactor, 128);
                }
                return true;

            case "/light":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (PlayerControl.LocalPlayer != null && (
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Phantom ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Viper ||
                    PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Shapeshifter))
                {
                    byte electricalSabotageId = 4;
                    for (int i = 0; i < 5; i++)
                    {
                        electricalSabotageId |= (byte)(1 << i);

                    }
                    electricalSabotageId |= 128;

                    SabotageManager.TryActivateSabotage(SystemTypes.Electrical, electricalSabotageId);
                }
                return true;


            case "/colour":
            case "/color":
            case "/colore":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                subArgs = args.Length < 2 ? "" : args[1];
                var color = Utils.MsgToColor(subArgs, true);
                if (color == byte.MaxValue)
                {
                    return true;
                }
                PlayerControl.LocalPlayer.RpcSetColor(color);
                return true;

            case "/sg":
            case "/setspecialkiller":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (args.Length == 2 && byte.TryParse(subArgs, out byte targetId1))
                {
                    var target = BanMod.AllPlayerControls.FirstOrDefault(p =>
                        p.PlayerId == targetId1 &&
                        p.Data != null &&
                        !p.Data.IsDead &&
                        p.Data.Role?.TeamType != RoleTeamTypes.Impostor);

                    if (target != null)
                    {
                        Guesser.SpecialKillerId = targetId1;
                        Guesser.SpecialKillerSelected = true;
                        ShowChat($"Special Killer set to {target.name} (ID: {targetId1}).");

                        if (AmongUsClient.Instance.AmHost)
                        {
                            BanModSyncApi.SendSetSpecialKiller(targetId1);
                        }
                        else
                        {
                            ShowChat("Only the host can set the Special Killer.");
                        }
                    }
                    else
                    {
                        ShowChat($"Player with ID {targetId1} is invalid (dead or an impostor).");
                    }
                }
                else
                {
                    ShowChat("Correct use: /setsk ");
                }
                return true;

            case "/sj":
            case "/setjester":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (args.Length == 2 && byte.TryParse(subArgs, out byte targetId3))
                {
                    var target = BanMod.AllPlayerControls.FirstOrDefault(p =>
                        p.PlayerId == targetId3 &&
                        p.Data != null &&
                        !p.Data.IsDead &&
                        p.Data.Role?.TeamType != RoleTeamTypes.Impostor);

                    if (target != null)
                    {
                        Jester.JesterId = targetId3;
                        Jester.JesterSelected = true;
                        ShowChat($"Jester set to {target.name} (ID: {targetId3}).");

                        if (AmongUsClient.Instance.AmHost)
                        {
                            BanModSyncApi.SendSetJester(targetId3);
                        }
                        else
                        {
                            ShowChat("Only the host can set the Jester.");
                        }
                    }
                    else
                    {
                        ShowChat($"Player with ID {targetId3} is invalid (dead or an impostor).");
                    }
                }
                else
                {
                    ShowChat("Correct use: /setjester ");
                }
                return true;

            case "/se":
            case "/setexiler":
                if (!AmongUsClient.Instance.AmHost)
                    return true;
                if (args.Length == 2 && byte.TryParse(subArgs, out byte targetId2))
                {
                    var target = BanMod.AllPlayerControls.FirstOrDefault(p =>
                        p.PlayerId == targetId2 &&
                        p.Data != null &&
                        !p.Data.IsDead);

                    if (target != null)
                    {
                        Exiler.ExilerId = targetId2;
                        Exiler.ExilerSelected = true;
                        ShowChat($"Exiler set to {target.name} (ID: {targetId2}).");

                        if (AmongUsClient.Instance.AmHost)
                        {
                            BanModSyncApi.SendSetExiler(targetId2);
                        }
                        else
                        {
                            ShowChat("Only the host can set the Exiler.");
                        }
                    }
                    else
                    {
                        ShowChat($"Player with ID {targetId2} is invalid");
                    }
                }
                else
                {
                    ShowChat("Correct use: /setexiler ");
                }
                return true;

            case "/all":
                if (!AmongUsClient.Instance.AmHost)
                {
                    return true;
                }
                Utils.ShowCommand();
                Utils.ShowCommand3();
                return true;

            case "/help":
            case "/aiuto":
            case "/hilfe":
            case "/aide":
            case "/помощь":
                if (!AmongUsClient.Instance.AmHost)
                {
                    return true;
                }
                Utils.ShowCommand4();
                return true;

            case "/dn":
                {
                    string value = string.Join(" ", args.Skip(1)).Trim();

                    bool result = AppendToFile(
                        "DenyName.txt",
                        value,
                        "AddedtoDenynamelist"
                    );

                    FixedUpdateUnifiedPatch.RefreshDeniedNamesNow();

                    return result;
                }

            case "/ddn":
                {
                    string value = string.Join(" ", args.Skip(1)).Trim();

                    bool result = RemoveFromFile(
                        "DenyName.txt",
                        value,
                        "DeletedtoDenynamelist"
                    );

                    FixedUpdateUnifiedPatch.RefreshDeniedNamesNow();

                    return result;
                }
            case "/dw": return AppendToFile("BanWords.txt", string.Join(" ", args.Skip(1)), "AddedtoDenyWordlist");
            case "/ddw": return RemoveFromFile("BanWords.txt", string.Join(" ", args.Skip(1)), "DeletedtoDenyWord");
            case "/ds": return AppendToFile("SpamStart.txt", string.Join(" ", args.Skip(1)), "AddedtoDenystartlistlist");
            case "/dds": return RemoveFromFile("SpamStart.txt", string.Join(" ", args.Skip(1)), "DeletedtoDenystartlist");
            case "/addvip": return AllowedManager.ManageVip(subArgs, add: true);
            case "/deletevip": return AllowedManager.ManageVip(subArgs, add: false);
            case "/addmod": return AllowedManager.ManageModerator(subArgs, add: true);
            case "/deletemod": return AllowedManager.ManageModerator(subArgs, add: false);

            case "/id":
                if (!AmongUsClient.Instance.AmHost)
                {
                    return true;
                }
                string msg5 = GetString("PlayerIdList") + string.Join("\n", BanMod.AllPlayerControls
                    .Where(p => p != null)
                    .Select(p => $"{p.PlayerId} ({NumberToWords(p.PlayerId)}) → {p.Data.PlayerName}"));
                ShowChat(msg5);
                return true;


            case "/level":
                if (int.TryParse(subArgs, out int level) && level is >= 1 and <= 99999)
                {
                    uint lvl = Convert.ToUInt32(level - 1);

                    player.RpcSetLevel(lvl);
                    DataManager.Player.stats.level = lvl;
                    DataManager.Player.Save();

                    BanMod.spoofLevel.Value = level.ToString();
                    BanMod.Instance.Config.Save();
                    ShowChat("Livello impostato a " + subArgs);
                }
                else
                {
                    ShowChat("Livello troppo alto");
                }
                return true;

            case "/say":
            case "/scrivi":
                return PrivateMessage1(args);

            case "/chat":
                return ChatColorManager.SetColoredText(subArgs);

        }

        return false;
    }

    static bool AppendToFile(string file, string value, string msgKey)
    {
        value = value?.Trim();

        if (string.IsNullOrWhiteSpace(value))
            return true;

        string path = $"./DATA/DENIED/{file}";

        if (file.Equals("BanWords.txt", StringComparison.OrdinalIgnoreCase))
        {
            if (!SpamManager.BanWords.Any(x =>
                x.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                File.AppendAllText(
                    path,
                    Environment.NewLine + value,
                    Encoding.UTF8
                );

                SpamManager.BanWords.Add(value);
            }
        }

        else if (file.Equals("SpamStart.txt", StringComparison.OrdinalIgnoreCase))
        {
            if (!SpamManager.SpamStart.Any(x =>
                x.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                File.AppendAllText(
                    path,
                    Environment.NewLine + value,
                    Encoding.UTF8
                );

                SpamManager.SpamStart.Add(value);
            }
        }

        else
        {
            File.AppendAllText(
                path,
                Environment.NewLine + value,
                Encoding.UTF8
            );
        }

        ShowChat(value + GetString(msgKey));
        return true;
    }

    static bool RemoveFromFile(string file, string value, string msgKey)
    {
        value = value?.Trim();

        if (string.IsNullOrWhiteSpace(value))
            return true;

        string path = $"./DATA/DENIED/{file}";

        if (File.Exists(path))
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8)
                .Where(line =>
                    !line.Trim().Equals(
                        value,
                        StringComparison.OrdinalIgnoreCase
                    ))
                .ToList();

            File.WriteAllLines(path, lines, Encoding.UTF8);
        }

        if (file.Equals("BanWords.txt", StringComparison.OrdinalIgnoreCase))
        {
            SpamManager.BanWords.RemoveAll(x =>
                x.Equals(value, StringComparison.OrdinalIgnoreCase));
        }
        else if (file.Equals("SpamStart.txt", StringComparison.OrdinalIgnoreCase))
        {
            SpamManager.SpamStart.RemoveAll(x =>
                x.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        ShowChat(value + GetString(msgKey));
        return true;
    }
    public static bool ComandoExeUsed = false;
    public static bool ComandoRoomUsed = false;


    static bool PrivateMessage1(string[] args)
    {
        if (args.Length < 2) return false;

        string msg;
        byte id = byte.MaxValue;

        if (byte.TryParse(args[1], out byte parsedId) && args.Length >= 3)
        {
            id = parsedId;
            msg = string.Join(" ", args.Skip(2));
        }
        else
        {
            msg = string.Join(" ", args.Skip(1));
        }

        var target = id == byte.MaxValue ? null : GetPlayerById(id);
        if (id != byte.MaxValue && target == null) return false;
        if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
        {
            Utils.RequestProxyMessage(msg, id);
            MessageBlocker.UpdateLastMessageTime();
        }
        else
        {
            Utils.SendMessage(msg, id);
            MessageBlocker.UpdateLastMessageTime();
        }

        string recipient = id == byte.MaxValue ? GetString("Everyone") : target.Data.PlayerName;
        ShowChat($"{GetString("MessageSentTo")} {recipient}");
        return true;
    }
    private static Stack<NetworkedPlayerInfo.PlayerOutfit> savedOutfits2 = new Stack<NetworkedPlayerInfo.PlayerOutfit>();
    private static Stack<string> savedNames2 = new Stack<string>();
    public static readonly Dictionary<string, string> originalNamesByFriendCode = new();
    public static readonly Dictionary<string, string> moddedNamesByFriendCode = new();

    public static void StoreOriginalName(int playerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var player = BanMod.AllPlayerControls
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);

        if (player == null || player.Data == null) return;

        string friendCode = player.FriendCode;

        if (!string.IsNullOrEmpty(friendCode) &&
            !originalNamesByFriendCode.ContainsKey(friendCode))
        {
            originalNamesByFriendCode[friendCode] = player.Data.PlayerName;
        }
    }

    public static void StoreModdedName(int playerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var player = BanMod.AllPlayerControls
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);

        if (player == null || player.Data == null) return;

        string friendCode = player.FriendCode;

        if (!string.IsNullOrEmpty(friendCode))
        {
            moddedNamesByFriendCode[friendCode] = player.Data.PlayerName;
        }
    }

    public static void DeleteModdedName(int playerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var player = BanMod.AllPlayerControls
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);

        if (player == null) return;

        string friendCode = player.FriendCode;

        if (!string.IsNullOrEmpty(friendCode))
        {
            moddedNamesByFriendCode.Remove(friendCode);
        }
    }

    public static void RestoreOriginalName(int playerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var player = BanMod.AllPlayerControls
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);

        if (player == null) return;

        if ((Utils.Shapeshifter(player) &&
             player.CurrentOutfitType == PlayerOutfitType.Shapeshifted) ||
            Utils.Phantom(player))
        {
            return;
        }

        string friendCode = player.FriendCode;

        if (!string.IsNullOrEmpty(friendCode) &&
            originalNamesByFriendCode.TryGetValue(friendCode, out string originalName))
        {
            player.RpcSetName(originalName);
        }
    }
    public static void RestoreModdedNameIfNeeded(int playerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!BanModServerSelection.IsModded25) return;

        var player = BanMod.AllPlayerControls
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);

        if (player == null || player.Data == null) return;

        if ((Utils.Shapeshifter(player) &&
             player.CurrentOutfitType == PlayerOutfitType.Shapeshifted) ||
            Utils.Phantom(player))
        {
            return;
        }

        string friendCode = player.FriendCode;

        if (string.IsNullOrEmpty(friendCode))
            return;

        if (!moddedNamesByFriendCode.TryGetValue(friendCode, out string moddedName))
            return;

        if (!originalNamesByFriendCode.TryGetValue(friendCode, out string originalName))
            return;

        string currentName = player.Data.PlayerName;

        if (currentName == originalName)
        {
            player.RpcSetName(moddedName);
        }
    }
    public static string GetBanModTagName()
    {
        return
            "<b>" +
            "<color=#FF5A00>B</color>" +
            "<color=#FF9A00>a</color>" +
            "<color=#FFD500>n</color>" +
            "<color=#7ED957>M</color>" +
            "<color=#3D8BFF>o</color>" +
            "<color=#9B5CFF>d</color>" +
            "</b>";
    }

    public static string GetDisplayName(PlayerControl player)
    {
        if (player == null || player.Data == null)
            return "";

        string friendCode = player.FriendCode;

        string realName;

        if (!string.IsNullOrEmpty(friendCode) &&
            moddedNamesByFriendCode.TryGetValue(friendCode, out string moddedName))
        {
            realName = StripNameTags(moddedName);
        }
        else if (!string.IsNullOrEmpty(friendCode) &&
                 originalNamesByFriendCode.TryGetValue(friendCode, out string originalName))
        {
            realName = StripNameTags(originalName);
        }
        else
        {
            realName = StripNameTags(player.Data.PlayerName);
        }

        if (!Options.TagName.GetBool())
        {
            if (!string.IsNullOrEmpty(friendCode) &&
                moddedNamesByFriendCode.TryGetValue(friendCode, out string moddedName1))
            {
                return moddedName1;
            }
            else if (!string.IsNullOrEmpty(friendCode) &&
                     originalNamesByFriendCode.TryGetValue(friendCode, out string originalName1))
            {
                return originalName1;
            }

            return player.Data.PlayerName;
        }

        string mode = Options.GameMode.GetString();

        string modeName = mode switch
        {
            "Default" => "BNM",
            "BanMod" => "BNM",
            "SnS" => "SNS",
            "PnS" => "PNS",
            "KaitoRun" => "KAITO",
            "TaskRun" => "TASKRUN",
            "JBMode" => "JB",
            "FFA" => "FFA",
            "RoomRush" => "ROOMRUSH",
            "TargetRush" => "TARGETRUSH",
            "DeathRun" => "DEATHRUN",
            "ZombieMode" => "ZOMBIE",
            "HotPotatoModded" => "HOT-POTATO",
            _ => "BNM"
        };

        string hostText = Options.TagGameMod.GetBool()
            ? $"{modeName}-HOST"
            : "BNM-HOST";

        return FadeTagAndName(
            hostText,
            realName,
            "#800080",
            "#FF0000"
        );
    }
    public static void ClearLocalPlayerNameData()
    {
        var localPlayer = PlayerControl.LocalPlayer;

        if (localPlayer == null)
            return;

        string friendCode = localPlayer.FriendCode;

        if (string.IsNullOrEmpty(friendCode))
            return;

        moddedNamesByFriendCode.Remove(friendCode);
        originalNamesByFriendCode.Remove(friendCode);
    }
    public static class ChatColorManager
    {
        public static readonly Dictionary<string, string> colorMap = new()
    {
        { "gold", "#C6A25A" },{ "brown2", "#7F582D" },{ "beige", "#B6AA9E" },{ "bluegreen", "#00CEC8" },{ "maize", "#FBEC5D" },
        { "candy", "#FF004D" },{ "wine", "#722F37" },
        { "white", "#FFFFFF" }, { "bianco", "#FFFFFF" }, { "weiß", "#FFFFFF" }, { "белый", "#FFFFFF" }, { "blanc", "#FFFFFF" },
        { "blu", "#0000FF" }, { "blue", "#0000FF" }, { "blau", "#0000FF" }, { "синий", "#0000FF" }, { "bleu", "#0000FF" },
        { "verde", "#00FF00" }, { "green", "#00FF00" }, { "grün", "#00FF00" }, { "зелёный", "#00FF00" }, { "vert", "#00FF00" },
        { "fucsia", "#FF00FF" }, { "fuchsia", "#FF00FF" }, { "fuchsie", "#FF00FF" }, { "фуксия", "#FF00FF" }, { "pink", "#FFC0CB" },
        { "arancio", "#FFA500" }, { "arancione", "#FFA500" }, { "orange", "#FFA500" }, { "оранжевый", "#FFA500" },
        { "giallo", "#FFFF00" }, { "gialla", "#FFFF00" }, { "yellow", "#FFFF00" }, { "gelb", "#FFFF00" }, { "жёлтый", "#FFFF00" }, { "jaune", "#FFFF00" },
        { "nero", "#000000" }, { "nera", "#000000" }, { "black", "#000000" }, { "schwarz", "#000000" }, { "чёрный", "#000000" }, { "noir", "#000000" },
        { "viola", "#800080" }, { "purple", "#800080" }, { "lila", "#800080" }, { "фиолетовый", "#800080" }, { "violet", "#800080" },
        { "marrone", "#8B4513" }, { "brown", "#8B4513" }, { "braun", "#8B4513" }, { "коричневый", "#8B4513" }, { "marron", "#8B4513" },
        { "ciano", "#00FFFF" }, { "azzurro", "#00FFFF" }, { "azzurra", "#00FFFF" }, { "cyan", "#00FFFF" }, { "hellblau", "#00FFFF" }, { "голубой", "#00FFFF" }, { "bleu clair", "#00FFFF" },
        { "bordo", "#800000" }, { "bordeaux", "#800000" }, { "maroon", "#800000" }, { "kastanienbraun", "#800000" }, { "бордовый", "#800000" },
        { "rosa", "#FFC0CB" }, { "confetto", "#FFC0CB" }, { "розовый", "#FFC0CB" }, { "rose", "#FFC0CB" },
        { "crema", "#FFFACD" }, { "cream", "#FFFACD" }, { "creme", "#FFFACD" }, { "кремовый", "#FFFACD" }, { "crème", "#FFFACD" },{ "banana", "#FFFACD" },{ "banan", "#FFFACD" },
        { "lime", "#BFFF00" }, { "limette", "#BFFF00" }, { "лайм", "#BFFF00" }, { "citron vert", "#BFFF00" },
        { "grigio", "#808080" }, { "grigia", "#808080" }, { "gray", "#808080" }, { "grau", "#808080" }, { "серый", "#808080" }, { "gris", "#808080" },
        { "tortora", "#D2B48C" }, { "taupe", "#D2B48C" }, { "таупе", "#D2B48C" }, { "tan", "#D2B48C" },
        { "corallo", "#FF7F50" }, { "coral", "#FF7F50" }, { "koralle", "#FF7F50" }, { "коралловый", "#FF7F50" }, { "corail", "#FF7F50" },
        { "rosso", "#FF0000" }, { "rossa", "#FF0000" }, { "red", "#FF0000" }, { "rot", "#FF0000" }, { "красный", "#FF0000" }, { "rouge", "#FF0000" }
    };

        public static Color? currentChatColor = null;

        public static bool SetColoredText(string colorKey)
        {
            string hex = null;
            if (colorMap.TryGetValue(colorKey.ToLower(), out hex) || Regex.IsMatch(colorKey, "^#([0-9A-Fa-f]{6})$"))
            {
                string colorString = hex ?? colorKey;

                if (ColorUtility.TryParseHtmlString(colorString, out Color newColor))
                {
                    currentChatColor = newColor;
                    return true;
                }
            }

            currentChatColor = null;
            return false;
        }
    }
    private static readonly System.Random NameRandom = new System.Random();

    private static string GetColorHex(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim().ToLowerInvariant();

        if (ChatColorManager.colorMap.TryGetValue(input, out string hex))
            return hex;

        if (Regex.IsMatch(input, "^#([0-9A-Fa-f]{6})$"))
            return input.StartsWith("#") ? input : "#" + input;

        return null;
    }

    private static string StripNameTags(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "";

        return Regex.Replace(name, "<[^>]*>", "");
    }

    private static void ResetPlayerName(PlayerControl target)
    {
        if (target == null || target.Data == null)
            return;

        RestoreOriginalName(target.PlayerId);
        DeleteModdedName(target.PlayerId);
    }

    private static string RandomRainbowName(string name)
    {
        StringBuilder result = new StringBuilder();

        foreach (char c in name)
        {
            if (char.IsWhiteSpace(c))
            {
                result.Append(c);
                continue;
            }

            int r = NameRandom.Next(80, 256);
            int g = NameRandom.Next(80, 256);
            int b = NameRandom.Next(80, 256);

            string hex = $"#{r:X2}{g:X2}{b:X2}";

            result.Append($"<color={hex}>{c}</color>");
        }

        return result.ToString();
    }

    public static string FadeName(string name, string startHex, string endHex)
    {
        startHex = startHex.TrimStart('#');
        endHex = endHex.TrimStart('#');

        int startR = Convert.ToInt32(startHex.Substring(0, 2), 16);
        int startG = Convert.ToInt32(startHex.Substring(2, 2), 16);
        int startB = Convert.ToInt32(startHex.Substring(4, 2), 16);

        int endR = Convert.ToInt32(endHex.Substring(0, 2), 16);
        int endG = Convert.ToInt32(endHex.Substring(2, 2), 16);
        int endB = Convert.ToInt32(endHex.Substring(4, 2), 16);

        StringBuilder result = new StringBuilder();

        int visibleChars = name.Count(c => !char.IsWhiteSpace(c));
        int currentIndex = 0;

        foreach (char c in name)
        {
            if (char.IsWhiteSpace(c))
            {
                result.Append(c);
                continue;
            }

            float t = visibleChars <= 1
                ? 0f
                : (float)currentIndex / (visibleChars - 1);

            int r = (int)(startR + (endR - startR) * t);
            int g = (int)(startG + (endG - startG) * t);
            int b = (int)(startB + (endB - startB) * t);

            string hex = $"#{r:X2}{g:X2}{b:X2}";

            result.Append($"<color={hex}>{c}</color>");

            currentIndex++;
        }

        return result.ToString();
    }
    private static string FadeTagAndName(
    string tag,
    string realName,
    string startHex,
    string endHex)
    {
        if (string.IsNullOrEmpty(tag))
            tag = "";

        if (string.IsNullOrEmpty(realName))
            realName = "";

        startHex = startHex.TrimStart('#');
        endHex = endHex.TrimStart('#');

        int startR = Convert.ToInt32(startHex.Substring(0, 2), 16);
        int startG = Convert.ToInt32(startHex.Substring(2, 2), 16);
        int startB = Convert.ToInt32(startHex.Substring(4, 2), 16);

        int endR = Convert.ToInt32(endHex.Substring(0, 2), 16);
        int endG = Convert.ToInt32(endHex.Substring(2, 2), 16);
        int endB = Convert.ToInt32(endHex.Substring(4, 2), 16);

        string completeText = tag + realName;

        int visibleChars = completeText.Count(c => !char.IsWhiteSpace(c));

        if (visibleChars <= 0)
            return "<color=#FFD700>★</color>";

        int currentIndex = 0;

        string ColorChar(char c)
        {
            if (char.IsWhiteSpace(c))
                return c.ToString();

            float t = visibleChars <= 1
                ? 0f
                : (float)currentIndex / (visibleChars - 1);

            int r = (int)Math.Round(startR + (endR - startR) * t);
            int g = (int)Math.Round(startG + (endG - startG) * t);
            int b = (int)Math.Round(startB + (endB - startB) * t);

            currentIndex++;

            return $"<color=#{r:X2}{g:X2}{b:X2}>{c}</color>";
        }

        StringBuilder result = new StringBuilder();

        foreach (char c in tag)
            result.Append(ColorChar(c));

        result.Append("<color=#FFD700>★</color>");

        foreach (char c in realName)
            result.Append(ColorChar(c));

        return result.ToString();
    }
    private static string RandomFadeColor()
    {
        int r = NameRandom.Next(50, 256);
        int g = NameRandom.Next(50, 256);
        int b = NameRandom.Next(50, 256);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static string MakeDarkerColor(string hex)
    {
        hex = hex.TrimStart('#');

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

        r = (int)(r * 0.35f);
        g = (int)(g * 0.35f);
        b = (int)(b * 0.35f);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static string MakeLighterColor(string hex)
    {
        hex = hex.TrimStart('#');

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

        r = (int)(r + (255 - r) * 0.65f);
        g = (int)(g + (255 - g) * 0.65f);
        b = (int)(b + (255 - b) * 0.65f);

        return $"#{r:X2}{g:X2}{b:X2}";
    }
    public static void ApplyFade(PlayerControl player, string[] args)
    {
        if (player == null || player.Data == null)
            return;

        if (!GameStates.isLobby || !BanModServerSelection.IsModded25)
            return;

        string startColor;
        string endColor;

        if (args.Length == 1)
        {
            startColor = RandomFadeColor();
            endColor = RandomFadeColor();

            int attempts = 0;

            while (startColor.Equals(endColor, StringComparison.OrdinalIgnoreCase)
                   && attempts < 10)
            {
                endColor = RandomFadeColor();
                attempts++;
            }
        }

        else if (args.Length == 2)
        {
            string selectedColor = GetColorHex(args[1]);

            if (selectedColor == null)
            {
                ShowChat($"Colore '{args[1]}' non valido.");
                return;
            }

            startColor = MakeDarkerColor(selectedColor);
            endColor = MakeLighterColor(selectedColor);
        }

        else
        {
            startColor = GetColorHex(args[1]);
            endColor = GetColorHex(args[2]);

            if (startColor == null || endColor == null)
            {
                ShowChat("Uno dei colori non è valido.");
                return;
            }
        }

        string baseName = StripNameTags(player.Data.PlayerName);

        StoreOriginalName(player.PlayerId);

        string newName = FadeName(
            baseName,
            startColor,
            endColor
        );

        player.RpcSetName(newName);

        StoreModdedName(player.PlayerId);
    }
    private static void ReportModChat(
    PlayerControl moderator,
    string action,
    PlayerControl target = null)
    {
        ModeratorAuthority.ReportModeratorUsage(
            moderator,
            action,
            target?.PlayerId ?? byte.MaxValue,
            "CHAT"
        );
    }
    private static PlayerControl FindPlayerByIdOrColor(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (byte.TryParse(input, out byte playerId))
        {
            PlayerControl byId = BanMod.AllPlayerControls.FirstOrDefault(p =>
                p != null &&
                p.Data != null &&
                p.PlayerId == playerId);

            if (byId != null)
                return byId;
        }

        byte colorId = MsgToColor(input);
        if (colorId != byte.MaxValue)
        {
            PlayerControl byColor = BanMod.AllPlayerControls.FirstOrDefault(p =>
                p != null &&
                p.Data != null &&
                p.Data.DefaultOutfit != null &&
                p.Data.DefaultOutfit.ColorId == colorId);

            if (byColor != null)
                return byColor;
        }

        return null;
    }

    private static bool BanAndAddTeamer(PlayerControl targetPlayer, string reason)
    {
        if (targetPlayer == null)
            return false;

        ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
        if (client == null)
            return false;

        if (AllowedManager.IsModCreator(client.FriendCode))
            return false;

        if (BanMod.IsProtected(client))
            return false;

        string finalReason = string.IsNullOrWhiteSpace(reason) ? "Teaming" : reason;
        string targetName = targetPlayer.Data?.PlayerName ?? targetPlayer.name ?? "Player";

        BanManager.AddBanPlayer(client, finalReason, false);

        TeamerManager.AddPlayer(client, finalReason);

        AmongUsClient.Instance.KickPlayer(client.Id, true);

        NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
            HudManager.Instance.Notifier,
            $"{targetName} banned and added to Teamers. Reason: {finalReason}"
        );

        ShowChat($"{targetName} banned and added to Teamers.\nReason: {finalReason}");

        return true;
    }
    public static void CmdPrivate_Public()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        DestroyableSingleton<GameStartManager>.Instance.MakePublic();
    }
    public static void ShowChat(string msg)
    {
        try
        {
            if (!DestroyableSingleton<HudManager>.InstanceExists)
                return;

            HudManager hud = DestroyableSingleton<HudManager>.Instance;

            if (hud == null || hud.Chat == null || PlayerControl.LocalPlayer == null)
                return;

            hud.Chat.AddChat(PlayerControl.LocalPlayer, msg);
        }
        catch
        {
        }
    }
    public static void OnReceiveChat(PlayerControl player, string text, out bool canceled)
    {
        canceled = false;


        if (!AmongUsClient.Instance.AmHost)
            return;

        if (BanModServerSelection.IsModded25)
            text = UnwrapModded25Command(text);

        string[] args = text.Split(' ');
        string command = args[0].ToLowerInvariant();
        string subArg = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        string subArgs = args.Length > 1 ? args[1] : "";
        string playerName = player.Data.PlayerName;
        bool isVip = Utils.IsVip(player.FriendCode);
        bool isModerator = Utils.IsModerator(player.FriendCode);
        string lowerMsg = text.ToLower();

        switch (command)
        {
            case "/imp":
                {
                    canceled = true;

                    if (!BanModServerSelection.IsModded25)
                        return;

                    if (player == null || player.Data == null)
                        return;

                    if (args.Length < 2)
                        return;

                    if (player.Data.Role?.TeamType != RoleTeamTypes.Impostor)
                        return;

                    string message = string.Join(" ", args.Skip(1)).Trim();

                    if (string.IsNullOrWhiteSpace(message))
                        return;

                    string senderName = player.Data.PlayerName ?? "Impostor";

                    string impMessage =
                        $"<color=#ff1919>[IMPOSTOR]</color> " +
                        $"<color=#ff6666>{senderName}</color>: {message}";

                    foreach (var target in BanMod.AllPlayerControls.ToArray())
                    {
                        if (target == null || target.Data == null)
                            continue;

                        if (target.PlayerId == player.PlayerId)
                            continue;

                        if (target.Data.Role?.TeamType != RoleTeamTypes.Impostor)
                            continue;

                        Utils.SendMessage(impMessage, target.PlayerId);
                        MessageBlocker.UpdateLastMessageTime();
                    }

                    return;
                }

            case "/sbanon":
                {
                    canceled = true;

                    if (!isModerator)
                        return;

                    superban = true;

                    ReportModChat(
                        player,
                        "Enable SuperBan (/sbanon)"
                    );

                    return;
                }
            case "/sban":
                {
                    canceled = true;

                    if (!superban) return;
                    if (args.Length < 2) return;
                    if (!isModerator) return;

                    string colorInput = args[1];

                    byte colorId = MsgToColor(colorInput);

                    if (colorId == byte.MaxValue)
                        return;

                    PlayerControl targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                        p != null &&
                        p.Data != null &&
                        p.Data.DefaultOutfit != null &&
                        p.Data.DefaultOutfit.ColorId == colorId);

                    if (targetPlayer == null)
                        return;

                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);

                    if (client == null)
                        return;

                    string friendCode = targetPlayer.Data?.FriendCode;

                    if (string.IsNullOrWhiteSpace(friendCode))
                        friendCode = client.FriendCode;

                    if (string.IsNullOrWhiteSpace(friendCode))
                        return;

                    SilentPermanentFriendCodeBan.Initialize();
                    SilentPermanentFriendCodeBan.AddDeferred(friendCode);
                    superban = false;
                    ReportModChat(
                        player,
                        "SuperBan (/sban)",
                        targetPlayer
                    );
                    return;
                }

            case "/tpout":
            case "/esci":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                    player.RpcTeleport(new Vector2(0.1f, 3.8f));
                return;

            case "/tpin":
            case "/entra":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                    player.RpcTeleport(new Vector2(-0.2f, 1.3f));
                return;

            case "/rename":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    StoreOriginalName(player.PlayerId);
                    player.RpcSetName(subArgs);
                    StoreModdedName(player.PlayerId);
                }
                return;


            case "/n":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    string baseName = Regex.Replace(playerName, "<.*?>", "");
                    string friendCode = player.FriendCode;

                    if (subArgs.Equals("rainbow", StringComparison.OrdinalIgnoreCase) ||
                        subArgs.Equals("raimbow", StringComparison.OrdinalIgnoreCase))
                    {
                        StoreOriginalName(player.PlayerId);

                        string newName = RandomRainbowName(baseName);

                        player.RpcSetName(newName);
                        StoreModdedName(player.PlayerId);

                        return;
                    }

                    string nameColorHex = GetColorHex(subArgs);

                    if (nameColorHex != null)
                    {
                        StoreOriginalName(player.PlayerId);

                        string newName = $"<color={nameColorHex}>{baseName}</color>";

                        player.RpcSetName(newName);
                        StoreModdedName(player.PlayerId);
                    }
                }
                break;

            case "/fade":
                ApplyFade(player, args);
                return;

            case "/s":
            case "/d":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                    if (args.Length > 1)
                    {
                        string symbolKey = subArg;
                        string colorArg = args.Length > 2 ? args[2].ToLowerInvariant() : "";

                        if (symbolMap.TryGetValue(symbolKey, out string rawSymbol))
                        {
                            string colorHex = colorMap.ContainsKey(colorArg)
                                ? colorMap[colorArg]
                                : (Regex.IsMatch(colorArg, "^#([0-9A-Fa-f]{6})$") ? colorArg : "#FFFFFF");

                            StoreOriginalName(player.PlayerId);
                            string coloredSymbol = $"<color={colorHex}>{rawSymbol}</color>";
                            string newName = command == "/s"
                                ? $"{coloredSymbol}{playerName}"
                                : $"{playerName}{coloredSymbol}";
                            player.RpcSetName(newName);
                            StoreModdedName(player.PlayerId);
                        }
                    }
                break;

            case "/reset":
            case "/resetname":
                if (GameStates.isLobby && BanModServerSelection.IsModded25)
                {
                    RestoreOriginalName(player.PlayerId);
                    DeleteModdedName(player.PlayerId);
                }
                break;

            case "/public":
            case "/private":
                {
                    if (!isModerator)
                        return;

                    ReportModChat(
                        player,
                        command == "/public"
                            ? "Public Lobby (/public)"
                            : "Private Lobby (/private)"
                    );

                    CmdPrivate_Public();

                    return;
                }

            case "/instantstart":
                {
                    if (!isModerator)
                        return;

                    var manager =
                        UnityEngine.Object.FindObjectOfType<GameStartManager>();

                    if (manager != null)
                    {
                        ReportModChat(
                            player,
                            "Instant Start (/instantstart)"
                        );
                        BanMod.instantstart = true;
                        manager.BeginGame();
                        BanMod.instantstart = false;
                    }

                    return;
                }

            case "/start":
                {
                    if (!isModerator)
                        return;

                    bool oldNocountdown = Options.nocountdown.GetBool();

                    var manager =
                        UnityEngine.Object.FindObjectOfType<GameStartManager>();

                    if (manager != null)
                    {
                        ReportModChat(
                            player,
                            "Start Game (/start)"
                        );

                        manager.BeginGame();
                    }

                    return;
                }

            case "/meeting":
                {
                    if (!isModerator)
                        return;

                    ReportModChat(
                        player,
                        "Call Meeting (/meeting)"
                    );

                    player.CmdReportDeadBody(null);

                    return;
                }

            case "/endgame":
                {
                    if (!isModerator)
                        return;

                    if (GameManager.Instance == null)
                        return;

                    ReportModChat(
                        player,
                        "End Game (/endgame)"
                    );
                    BanMod.EndGameForced = true;
                    GameManager.Instance.RpcEndGame(
                        GameOverReason.CrewmatesByTask,
                        false
                    );

                    return;
                }

            case "/endmeeting":
                {
                    if (!isModerator)
                        return;

                    if (MeetingHud.Instance == null)
                        return;

                    ReportModChat(
                        player,
                        "End Meeting (/endmeeting)"
                    );

                    PlayerControl.LocalPlayer.StartCoroutine(
                        Utils.DelayedCloseMeeting()
                    );

                    return;
                }

            case "/every":
                {
                    if (!isModerator)
                        return;

                    subArgs = args.Length < 2 ? "" : args[1];

                    byte color = Utils.MsgToColor(
                        subArgs,
                        true
                    );

                    if (color == byte.MaxValue)
                        return;

                    ReportModChat(
                        player,
                        $"Set Everyone Color (/every {subArgs})"
                    );

                    foreach (var allplayer in PlayerControl.AllPlayerControls)
                    {
                        allplayer.RpcSetColor(color);
                    }

                    return;
                }

            case "/ban":
                {
                    if (!isModerator) return;

                    if (args.Length < 2)
                    {
                        return;
                    }

                    string targetInput = args[1];
                    PlayerControl targetPlayer = null;

                    byte colorId = MsgToColor(targetInput);
                    if (colorId != byte.MaxValue)
                    {
                        targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null && p.Data != null && p.Data.DefaultOutfit.ColorId == colorId);
                    }

                    if (targetPlayer == null)
                    {
                        return;
                    }

                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
                    if (client == null)
                    {
                        return;
                    }

                    if (!ModeratorAuthority.CanManuallyRemove(player, client))
                        return;

                    ReportModChat(
                        player,
                        "Ban (/ban)",
                        targetPlayer
                    );

                    BanManager.AddBanPlayer(
                        client,
                        "ModeratorBan",
                        true
                    );

                    try
                    {
                        BanMod.AddBanToList.Value = false;

                        AmongUsClient.Instance.KickPlayer(
                            client.Id,
                            true
                        );
                    }
                    finally
                    {
                        BanMod.AddBanToList.Value = true;
                    }

                    return;
                }

            case "/kick":
                {
                    if (!isModerator) return;
                    if (args.Length < 2)
                    {
                        return;
                    }
                    string targetInput = args[1];
                    PlayerControl targetPlayer = null;
                    if (targetPlayer == null)
                    {
                        byte colorId = MsgToColor(targetInput);
                        if (colorId != byte.MaxValue)
                        {
                            targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                                p != null && p.Data != null && p.Data.DefaultOutfit.ColorId == colorId);
                        }
                    }

                    if (targetPlayer == null)
                    {
                        return;
                    }
                    ClientData client = AmongUsClient.Instance?.GetClient(targetPlayer.OwnerId);
                    if (client == null)
                    {
                        return;
                    }
                    if (!ModeratorAuthority.CanManuallyRemove(player, client))
                        return;

                    ReportModChat(
                        player,
                        "Kick (/kick)",
                        targetPlayer
                    );

                    AmongUsClient.Instance.KickPlayer(
                        client.Id,
                        false
                    );

                    return;
                }

            case "/summary":
                {
                    if (!isModerator)
                        return;

                    string report1 =
                        MatchSummary1.GetSummaryReport();

                    ReportModChat(
                        player,
                        "Match Summary (/summary)"
                    );

                    Utils.SendMessage(
                        report1,
                        255
                    );

                    return;
                }

            case "/colour":
            case "/color":
            case "/colore":
                {
                    subArgs = args.Length < 2 ? "" : args[1];
                    var color = Utils.MsgToColor(subArgs, true);
                    if (color == byte.MaxValue)
                        break;
                    if (!GameStates.isLobby) return;
                    if (Options.AllowColorChangeModerator.GetBool() && (isModerator || isVip))
                        player.RpcSetColor(color);
                    else if (Options.AllowColorChangeAll.GetBool())
                        player.RpcSetColor(color);
                    else return;

                    break;
                }

            case "/bm":
                {
                    if (GameStates.isLobby || player.Data.IsDead) return;

                    if (args.Length >= 2)
                    {
                        PlayerControl targetPlayer = Utils.GetTarget(args[1]);

                        if (targetPlayer != null)
                        {
                            RolesCommand.Cmd(player.PlayerId, targetPlayer.PlayerId);
                        }
                    }
                    return;
                }

            case "/m":
                if (GameStates.isLobby) return;
                bool isSpecialKiller1 = Options.Guess.GetBool() && player.PlayerId == Guesser.SpecialKillerId;
                bool isJester1 = Options.Jester.GetBool() && player.PlayerId == Jester.JesterId;
                bool isPresident1 = Options.ExilerExe.GetBool() && player.PlayerId == Exiler.ExilerId;
                bool isScientist1 = Options.ScientistTime.GetBool() && Scientist(player);
                bool isPhantom1 = Options.PhantomGuess.GetBool() && Phantom(player);
                bool isCobra1 = Options.ViperGuess.GetBool() && Cobra(player);
                bool isImpostor1 = Options.ImpostorGuess.GetBool() && Impostor(player);
                bool isEngineer1 = Options.EngineerFixer.GetBool() && Engineer(player) && (!isJester1); ;
                bool isImmortal1 = Options.EnableImmortal.GetBool() && ImmortalManager.IsImmortal(player.PlayerId);
                bool Shapeshifter1 = Options.ShapeGuess.GetBool() && Shapeshifter(player);

                if (isEngineer1)
                {
                    Engineer.SendEngineerMessage();
                }
                if (Shapeshifter1)
                {
                    ImpostorGuesser.SendShapePlayerMessage();
                }
                if (isPhantom1)
                {
                    ImpostorGuesser.SendPhantomPlayerMessage();
                }
                if (isImpostor1)
                {
                    ImpostorGuesser.SendImpostorPlayerMessage();
                }
                if (isCobra1)
                {
                    ImpostorGuesser.SendViperPlayerMessage();
                }
                if (isScientist1)
                {
                    Scientist.SendScientistMessage();
                }
                if (isSpecialKiller1)
                {
                    Guesser.SendKillerMessage();
                }
                if (isJester1)
                {
                    Jester.SendJesterMessage();
                }
                if (isPresident1)
                {
                    Exiler.SendExilerMessage();
                }
                if (isImmortal1)
                {
                    string msg = GetString("ImmortalSelfMessage");
                    if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
                    {
                        Utils.RequestProxyMessage(msg, player.PlayerId);
                        MessageBlocker.UpdateLastMessageTime();
                    }
                    else
                    {
                        Utils.SendMessage(msg, player.PlayerId);
                        MessageBlocker.UpdateLastMessageTime();
                    }
                }
                if (!isSpecialKiller1 && !isJester1 && !isPresident1 && !isScientist1 && !isPhantom1 && !isEngineer1 && !isImmortal1 && !Shapeshifter1 && !isCobra1 && !isImpostor1)
                {
                    string msg = string.Format(GetString("NeutralInfo"));
                    Utils.SendMessage(msg, player.PlayerId);
                    MessageBlocker.UpdateLastMessageTime();

                }
                return;

            case "/insulta":
                bool IsVip = Utils.IsVip(player.FriendCode);
                if (!GameStates.isLobby) return;
                if (!insulta) return;
                if (!IsVip) return;
                {
                    if (args.Length < 2)
                    {
                        ShowChat("Uso corretto: /insulta <nome>");
                        return;
                    }

                    string target = args[1];

                    string insulto = PrendiInsulto();

                    string msg = $"{target}, {insulto}";

                    Utils.SendMessage(msg);

                    return;
                }

            case "/info":
                subArgs = args.Length < 2 ? "" : args[1];
                subArgs = args.Length < 2 ? "" : args[1].ToLowerInvariant();
                switch (subArgs)
                {
                    case "giustiziere":
                    case "guesser":
                    case "guess":
                    case "giustiz":
                    case "g":
                    case "devin":
                    case "vermuten":
                    case "Предсказатель":
                        bool isGuessEnabled = Options.Guess.GetBool();
                        string statoGuess = isGuessEnabled ? "On" : "Off";
                        string msgGuess =
                            $"{GetString("GuesserDescription")}\n" +
                            $"{GetString("ModEnabled")} {statoGuess}";

                        Utils.SendMessage(msgGuess, 255);
                        MessageBlocker.UpdateLastMessageTime();

                        return;

                    case "presidente":
                    case "president":
                    case "exiler":
                    case "p":
                    case "président":
                    case "präsident":
                    case "президент":
                        bool isExilerEnabled = Options.ExilerExe.GetBool();
                        bool isExilerKilled = Options.killexiler.GetBool();
                        string action = Options.ExilerAction.GetString();
                        string statoExiler = isExilerEnabled ? "On" : "Off";
                        string statoExilerK = isExilerKilled ? "On" : "Off";
                        string msgExiler =
                            $"{GetString("ModEnabled")}: {statoExiler}\n" +
                            $"{GetString("Consequence")}: {statoExilerK}\n" +
                            $"{GetString("Action")} {action}";

                        Utils.SendMessage(msgExiler, 255);
                        MessageBlocker.UpdateLastMessageTime();
                        Utils.SendMessage(GetString("exiler.cm"), 255);
                        MessageBlocker.UpdateLastMessageTime();
                        return;


                    case "spettro":
                    case "fantasma":
                    case "phantom":
                    case "ph":
                    case "fantôme":
                    case "geist":
                    case "призрак":
                        {
                            var optionsPha = GameOptionsManager.Instance.CurrentGameOptions;
                            float PhantomCooldown = 1f;
                            float PhantomDuration = 1f;
                            float killCooldown = 1f;
                            int phantomCount = optionsPha.RoleOptions.GetNumPerGame(RoleTypes.Phantom);
                            int phantomChance = optionsPha.RoleOptions.GetChancePerGame(RoleTypes.Phantom);

                            if (optionsPha != null)
                            {
                                optionsPha.TryGetFloat(FloatOptionNames.PhantomCooldown, out PhantomCooldown);
                                optionsPha.TryGetFloat(FloatOptionNames.PhantomDuration, out PhantomDuration);
                                optionsPha.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                                phantomCount = optionsPha.RoleOptions.GetNumPerGame(RoleTypes.Phantom);
                                phantomChance = optionsPha.RoleOptions.GetChancePerGame(RoleTypes.Phantom);
                            }

                            bool isPhantomEnabled = Options.PhantomGuess.GetBool();

                            if (isPhantomEnabled)
                            {
                                string msgPha =
                                $"{GetString("MaxPerGame")}: {phantomCount}\n" +
                                $"{GetString("Probability")}: {phantomChance}%\n" +
                                $"{GetString("Cooldown")}: {PhantomCooldown}s\n" +
                                $"{GetString("DurationPhantom")}: {PhantomDuration}s\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";
                                Utils.SendMessage(msgPha, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("PhantomDescription"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgPha =
                                $"{GetString("MaxPerGame")}: {phantomCount}\n" +
                                $"{GetString("Probability")}: {phantomChance}%\n" +
                                $"{GetString("Cooldown")}: {PhantomCooldown}s\n" +
                                $"{GetString("DurationPhantom")}: {PhantomDuration}s\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";
                                Utils.SendMessage(msgPha, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return;
                        }

                    case "immortale":
                    case "immortal":
                    case "imm":
                    case "immortel":
                    case "unsterblich":
                    case "бессмертный":
                        bool isImmortalEnabled = Options.EnableImmortal.GetBool();
                        bool isImmortalesentEnabled = Options.Immortalesentvote.GetBool();
                        string statoImmortal = isImmortalEnabled ? "On" : "Off";
                        string statoesent = isImmortalesentEnabled ? "On" : "Off";
                        string msgImmortal =
                            $"{GetString("ModEnabled")}: {statoImmortal}\n" +
                            $"{GetString("VoteEsent")}: {statoesent}";

                        Utils.SendMessage(msgImmortal, 255);
                        MessageBlocker.UpdateLastMessageTime();
                        Utils.SendMessage(GetString("ImmortalDescription"), 255);
                        MessageBlocker.UpdateLastMessageTime();
                        return;

                    case "ing":
                    case "ingegnere":
                    case "engineer":
                    case "eng":
                    case "ingénieur":
                    case "ingenieur":
                    case "инженер":
                        {
                            var optionsIng = GameOptionsManager.Instance.CurrentGameOptions;
                            float engineerCooldown = 1f;
                            float engineerInVentTime = 1f;
                            int engineerCount = optionsIng.RoleOptions.GetNumPerGame(RoleTypes.Engineer);
                            int engineerChance = optionsIng.RoleOptions.GetChancePerGame(RoleTypes.Engineer);
                            if (optionsIng != null)
                            {
                                optionsIng.TryGetFloat(FloatOptionNames.EngineerCooldown, out engineerCooldown);
                                optionsIng.TryGetFloat(FloatOptionNames.EngineerInVentMaxTime, out engineerInVentTime);
                                engineerCount = optionsIng.RoleOptions.GetNumPerGame(RoleTypes.Engineer);
                                engineerChance = optionsIng.RoleOptions.GetChancePerGame(RoleTypes.Engineer);
                            }

                            bool isEngineerFixerEnabled = Options.EngineerFixer.GetBool();
                            int ventFixAttempts = Options.VentTimes.GetInt();
                            string FormatVentTime(float time)
                            {
                                return time == 0f ? "∞" : $"{time:0.0}s";
                            }
                            if (isEngineerFixerEnabled)
                            {
                                string msg =
                                $"{GetString("MaxPerGame")}: {engineerCount}\n" +
                                $"{GetString("Probability")}: {engineerChance}%\n" +
                                $"{GetString("Cooldown")}: {engineerCooldown:0.0}s\n" +
                                $"{GetString("VentTime")}: {FormatVentTime(engineerInVentTime)}";
                                string msg2 =
                                $"{GetString("EngineerDescription")}\n\n" +
                                $"{GetString("AvailableFixes")}: {ventFixAttempts}";

                                Utils.SendMessage(msg, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(msg2, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msg =
                                $"{GetString("MaxPerGame")}: {engineerCount}\n" +
                                $"{GetString("Probability")}: {engineerChance}%\n" +
                                $"{GetString("Cooldown")}: {engineerCooldown:0.0}s\n" +
                                $"{GetString("VentTime")}: {FormatVentTime(engineerInVentTime)}";
                                Utils.SendMessage(msg, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return;
                        }


                    case "scienziato":
                    case "scientist":
                    case "sci":
                    case "scientifique":
                    case "wissenschaftler":
                    case "учёный":
                        {
                            var optionsScie = GameOptionsManager.Instance.CurrentGameOptions;
                            float ScientistCooldown = 1f;
                            float ScientistBatteryCharge = 1f;
                            int scientistCount = optionsScie.RoleOptions.GetNumPerGame(RoleTypes.Scientist);
                            int scientistChance = optionsScie.RoleOptions.GetChancePerGame(RoleTypes.Scientist);

                            if (optionsScie != null)
                            {
                                optionsScie.TryGetFloat(FloatOptionNames.ScientistCooldown, out ScientistCooldown);
                                optionsScie.TryGetFloat(FloatOptionNames.ScientistBatteryCharge, out ScientistBatteryCharge);
                                scientistCount = optionsScie.RoleOptions.GetNumPerGame(RoleTypes.Scientist);
                                scientistChance = optionsScie.RoleOptions.GetChancePerGame(RoleTypes.Scientist);
                            }

                            bool isScientistEnabled = Options.ScientistTime.GetBool();
                            if (isScientistEnabled)
                            {
                                string msgScie =
                                $"{GetString("MaxPerGame")}: {scientistCount}\n" +
                                $"{GetString("Probability")}: {scientistChance}%\n" +
                                $"{GetString("Cooldown")}: {ScientistCooldown:0.0}s\n" +
                                $"{GetString("VitalsTime")}: {ScientistBatteryCharge:0.0}s";
                                Utils.SendMessage(msgScie, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("ScientistDescription"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgScie =
                                $"{GetString("MaxPerGame")}: {scientistCount}\n" +
                                $"{GetString("Probability")}: {scientistChance}%\n" +
                                $"{GetString("Cooldown")}: {ScientistCooldown:0.0}s\n" +
                                $"{GetString("VitalsTime")}: {ScientistBatteryCharge:0.0}s";
                                Utils.SendMessage(msgScie, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return;
                        }

                    case "lobby":
                        {
                            var options = GameOptionsManager.Instance.CurrentGameOptions;

                            bool confirmImpostorValue = false;
                            bool visualTasks = false;
                            bool anonymousVotes = false;
                            float crewLightMod = 1f;
                            float impostorLightMod = 1f;
                            float killCooldown = 1f;

                            if (options != null)
                            {
                                options.TryGetBool(BoolOptionNames.ConfirmImpostor, out confirmImpostorValue);
                                options.TryGetBool(BoolOptionNames.VisualTasks, out visualTasks);
                                options.TryGetBool(BoolOptionNames.AnonymousVotes, out anonymousVotes);
                                options.TryGetFloat(FloatOptionNames.CrewLightMod, out crewLightMod);
                                options.TryGetFloat(FloatOptionNames.ImpostorLightMod, out impostorLightMod);
                                options.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                            }

                            string onOff(bool val) => val ? "On" : "Off";
                            string msgLobby =
                                $"{GetString("ConfirmImpostor")}:{onOff(confirmImpostorValue)}\n" +
                                $"{GetString("VisualTasks")}:{onOff(visualTasks)}\n" +
                                $"{GetString("AnonymousVotes")}:{onOff(anonymousVotes)}\n" +
                                $"{GetString("CrewmateVision")}:{crewLightMod}\n" +
                                $"{GetString("ImpostorVision")}:{impostorLightMod}\n" +
                                $"{GetString("KillCooldown")}:{killCooldown}";

                            Utils.SendMessage(msgLobby);
                            MessageBlocker.UpdateLastMessageTime();
                            return;
                        }

                    case "shapeshifter":
                    case "shape":
                    case "ss":
                    case "mutaforma":
                    case "muta":
                        {
                            var optionsShape = GameOptionsManager.Instance.CurrentGameOptions;
                            float ShapeshifterCooldown = 1f;
                            float ShapeshifterDuration = 1f;
                            bool ShapeshifterLeaveSkin = false;
                            float killCooldown = 1f;
                            int shapeCount = optionsShape.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter);
                            int shapeChance = optionsShape.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter);
                            string FormatDurationTime(float time)
                            {
                                return time == 0f ? "∞" : $"{time:0.0}s";
                            }
                            if (optionsShape != null)
                            {
                                optionsShape.TryGetFloat(FloatOptionNames.ShapeshifterCooldown, out ShapeshifterCooldown);
                                optionsShape.TryGetFloat(FloatOptionNames.ShapeshifterDuration, out ShapeshifterDuration);
                                optionsShape.TryGetBool(BoolOptionNames.ShapeshifterLeaveSkin, out ShapeshifterLeaveSkin);
                                optionsShape.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                                shapeCount = optionsShape.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter);
                                shapeChance = optionsShape.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter);
                            }

                            bool isShapeEnabled = Options.ShapeGuess.GetBool();
                            string statoShapevisible = ShapeshifterLeaveSkin ? "On" : "Off";
                            if (isShapeEnabled)
                            {
                                string msgShape =
                                $"{GetString("MaxPerGame")}: {shapeCount}\n" +
                                $"{GetString("Probability")}: {shapeChance}%\n" +
                                $"{GetString("Cooldown")}: {ShapeshifterCooldown}s\n" +
                                $"{GetString("DurationShape")}: {FormatDurationTime(ShapeshifterDuration)}\n" +
                                $"{GetString("SkinShape")}: {ShapeshifterLeaveSkin}\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";

                                Utils.SendMessage(msgShape, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("ShapeshifterDescription"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgShape =
                                $"{GetString("MaxPerGame")}: {shapeCount}\n" +
                                $"{GetString("Probability")}: {shapeChance}%\n" +
                                $"{GetString("Cooldown")}: {ShapeshifterCooldown}s\n" +
                                $"{GetString("DurationShape")}: {FormatDurationTime(ShapeshifterDuration)}\n" +
                                $"{GetString("SkinShape")}: {ShapeshifterLeaveSkin}\n" +
                                $"{GetString("KillCooldown")}: {killCooldown}s";
                                Utils.SendMessage(msgShape, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return;
                        }

                    case "detective":
                        {
                            var optionsDet = GameOptionsManager.Instance.CurrentGameOptions;
                            float DetectiveSuspectLimit = 1f;
                            int detectiveCount = optionsDet.RoleOptions.GetNumPerGame(RoleTypes.Detective);
                            int detectiveChance = optionsDet.RoleOptions.GetChancePerGame(RoleTypes.Detective);

                            if (optionsDet != null)
                            {
                                optionsDet.TryGetFloat(FloatOptionNames.DetectiveSuspectLimit, out DetectiveSuspectLimit);
                                detectiveCount = optionsDet.RoleOptions.GetNumPerGame(RoleTypes.Detective);
                                detectiveChance = optionsDet.RoleOptions.GetChancePerGame(RoleTypes.Detective);
                            }

                            string msgDet =
                                $"{GetString("MaxPerGame")}: {detectiveCount}\n" +
                                $"{GetString("Probability")}: {detectiveChance}%\n" +
                                $"{GetString("DetectiveSuspectLimit")}: {DetectiveSuspectLimit}";

                            Utils.SendMessage(msgDet, 255);
                            MessageBlocker.UpdateLastMessageTime();
                            return;
                        }

                    case "cobra":
                    case "viper":
                        {
                            var optionsCo = GameOptionsManager.Instance.CurrentGameOptions;
                            float ViperDissolveTime = 1f;
                            float killCooldown = 1f;
                            int viperCount = optionsCo.RoleOptions.GetNumPerGame(RoleTypes.Viper);
                            int viperChance = optionsCo.RoleOptions.GetChancePerGame(RoleTypes.Viper);

                            if (optionsCo != null)
                            {
                                optionsCo.TryGetFloat(FloatOptionNames.ViperDissolveTime, out ViperDissolveTime);
                                optionsCo.TryGetFloat(FloatOptionNames.KillCooldown, out killCooldown);
                                viperCount = optionsCo.RoleOptions.GetNumPerGame(RoleTypes.Viper);
                                viperChance = optionsCo.RoleOptions.GetChancePerGame(RoleTypes.Viper);
                            }

                            bool isViperEnabled = Options.ViperGuess.GetBool();

                            if (isViperEnabled)
                            {
                                string msgVip =
                                $"{GetString("MaxPerGame")}:{viperCount}\n" +
                                $"{GetString("Probability")}:{viperChance}%\n" +
                                $"{GetString("ViperDissolveTime")}:{ViperDissolveTime}s\n" +
                                $"{GetString("KillCooldown")}:{killCooldown}s";
                                Utils.SendMessage(msgVip, 255);
                                MessageBlocker.UpdateLastMessageTime();
                                Utils.SendMessage(GetString("viper.cm"), 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            else
                            {
                                string msgVip1 =
                                $"{GetString("MaxPerGame")}:{viperCount}\n" +
                                $"{GetString("Probability")}:{viperChance}%\n" +
                                $"{GetString("ViperDissolveTime")}:{ViperDissolveTime}s\n" +
                                $"{GetString("KillCooldown")}:{killCooldown}s";
                                Utils.SendMessage(msgVip1, 255);
                                MessageBlocker.UpdateLastMessageTime();
                            }
                            return;
                        }

                    case "starnazzatore":
                    case "noisemaker":
                        {
                            var optionsNoi = GameOptionsManager.Instance.CurrentGameOptions;
                            float NoisemakerAlertDuration = 1f;
                            bool NoisemakerImpostorAlert = false;
                            int noisemakerCount = optionsNoi.RoleOptions.GetNumPerGame(RoleTypes.Noisemaker);
                            int noisemakerChance = optionsNoi.RoleOptions.GetChancePerGame(RoleTypes.Noisemaker);
                            if (optionsNoi != null)
                            {
                                optionsNoi.TryGetFloat(FloatOptionNames.NoisemakerAlertDuration, out NoisemakerAlertDuration);
                                optionsNoi.TryGetBool(BoolOptionNames.NoisemakerImpostorAlert, out NoisemakerImpostorAlert);
                                noisemakerCount = optionsNoi.RoleOptions.GetNumPerGame(RoleTypes.Noisemaker);
                                noisemakerChance = optionsNoi.RoleOptions.GetChancePerGame(RoleTypes.Noisemaker);
                            }

                            string msgNoi =
                                $"{GetString("MaxPerGame")}: {noisemakerCount}\n" +
                                $"{GetString("Probability")}: {noisemakerChance}%\n" +
                                $"{GetString("NoisemakerAlertDuration")}: {NoisemakerAlertDuration}s\n" +
                                $"{GetString("NoisemakerImpostorAlert")}: {NoisemakerImpostorAlert}";

                            Utils.SendMessage(msgNoi, 255);
                            MessageBlocker.UpdateLastMessageTime();
                            return;
                        }

                    case "guardian":
                    case "angel":
                    case "angelo":
                        {
                            var optionsAng = GameOptionsManager.Instance.CurrentGameOptions;
                            float GuardianAngelCooldown = 1f;
                            float ProtectionDurationSeconds = 1f;
                            int angelCount = optionsAng.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel);
                            int angelChance = optionsAng.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel);

                            if (optionsAng != null)
                            {
                                optionsAng.TryGetFloat(FloatOptionNames.GuardianAngelCooldown, out GuardianAngelCooldown);
                                optionsAng.TryGetFloat(FloatOptionNames.ProtectionDurationSeconds, out ProtectionDurationSeconds);
                                angelCount = optionsAng.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel);
                                angelChance = optionsAng.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel);
                            }

                            string msgAng =
                                $"{GetString("MaxPerGame")}: {angelCount}\n" +
                                $"{GetString("Probability")}: {angelChance}%\n" +
                                $"{GetString("Cooldown")}: {GuardianAngelCooldown}s\n" +
                                $"{GetString("ProtectionDurationSeconds")}: {ProtectionDurationSeconds}s";

                            Utils.SendMessage(msgAng, 255);
                            MessageBlocker.UpdateLastMessageTime();
                            return;
                        }

                    default:
                        return;
                }


            case "/r":
            case "/t":
                if (!Options.TcommandforAll.GetBool())
                    return;
                if (player.Data.IsDead)
                    return;
                SendRules();
                return;

            default:
                if (SpamManager.CheckStart(player, text) ||
                    SpamManager.CheckWord(player, text)) return;

                return;

        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static class ChatUpdatePatch_SendMessage
    {
        public static float lastMessageTime = BanModServerSelection.IsVanilla ? -1.5f : 0f;
        public static float timeToWait = BanModServerSelection.IsVanilla ? 1.5f : 0f;

        public static void Postfix(ChatController __instance)
        {
            if (BanMod.IsBanModDisabled) return;
            if (BanModServerSelection.IsVanilla && Utils.TemplateQueueRunning)
                return;
            if (BanMod.MessagesToSend.Count == 0) return;

            var firstMessage = BanMod.MessagesToSend[0];

            if (BanModServerSelection.IsVanilla &&
                firstMessage.Item1.Length > 120)
            {
                var sender = PlayerControl.LocalPlayer;

                if (sender == null || sender.Data == null ||
                    sender.Data.Disconnected)
                {
                    return;
                }

                if (sender.Data.IsDead &&
                    firstMessage.Item2 != sender.PlayerId)
                {
                    Utils.RequestProxyMessage(
                        firstMessage.Item1,
                        firstMessage.Item2);
                }
                else
                {
                    Utils.SendVanillaTemplateBatch(
                        sender,
                        firstMessage.Item1,
                        firstMessage.Item2);
                }

                BanMod.MessagesToSend.RemoveAt(0);
                lastMessageTime = Time.time;
                return;
            }

            if (BanModServerSelection.IsVanilla)
            {
                if (!Utils.MessageBlocker.CanSendMessage())
                    return;
            }
            else if (Time.time - lastMessageTime < timeToWait)
            {
                return;
            }
            var realLocalPlayer = PlayerControl.LocalPlayer;

            var localPlayer =
                !BanModServerSelection.IsVanilla &&
                realLocalPlayer != null &&
                realLocalPlayer.Data != null &&
                realLocalPlayer.Data.IsDead
                    ? BanMod.AllPlayerControls
                        .Where(p =>
                            p != null &&
                            p.Data != null &&
                            !p.Data.Disconnected &&
                            !p.Data.IsDead)
                        .OrderBy(_ => UnityEngine.Random.value)
                        .FirstOrDefault() ?? realLocalPlayer
                    : realLocalPlayer;

            var (msg, sendTo) = BanMod.MessagesToSend[0];

            if (sendTo != byte.MaxValue)
            {
                var player = BanMod.AllPlayerControls.FirstOrDefault(p =>
                    p != null && p.PlayerId == sendTo && p.Data != null && !p.Data.Disconnected);

                if (player == null)
                {
                    BanMod.MessagesToSend.RemoveAt(0);
                    Debug.LogWarning($"[SendMessage] Messaggio per PlayerId {sendTo} annullato: giocatore disconnesso.");
                    return;
                }

            }
            else
            {
                foreach (var player in BanMod.AllPlayerControls.Where(p => p != null && p.Data != null && !p.Data.Disconnected))
                {
                    if (player.PlayerId == localPlayer.PlayerId) continue;

                }
            }

            BanMod.MessagesToSend.RemoveAt(0);
            lastMessageTime = Time.time;

            int clientId = sendTo == byte.MaxValue ? -1 : Utils.GetPlayerById(sendTo)?.GetClientId() ?? -1;

            var writer = CustomRpcSender.Create("MessagesToSend", SendOption.Reliable);
            bool usingOtherPlayer =
                localPlayer != null &&
                PlayerControl.LocalPlayer != null &&
                localPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId;

            string originalName = localPlayer.Data.PlayerName;

            writer.StartMessage(clientId);

            if (usingOtherPlayer)
            {
                writer.StartRpc(localPlayer.NetId, (byte)RpcCalls.SetName)
                    .Write("<color=#f3f315><b>System</b></color>")
                    .EndRpc();
            }
            writer.StartRpc(localPlayer.NetId, (byte)RpcCalls.SendChat)
                .Write(msg)
                .EndRpc();

            if (usingOtherPlayer)
            {
                writer.StartRpc(localPlayer.NetId, (byte)RpcCalls.SetName)
                    .Write(originalName)
                    .EndRpc();
            }
            writer.EndMessage();
            writer.SendMessage();

            if (clientId == -1)
            {
                if (usingOtherPlayer)
                {
                    localPlayer.Data.PlayerName = "<color=#f3f315><b>System</b></color>";

                    FastDestroyableSingleton<HudManager>.Instance.Chat
                        .AddChat(localPlayer, msg);

                    localPlayer.Data.PlayerName = originalName;
                }
                else
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat
                        .AddChat(localPlayer, msg);
                }
            }

            lastMessageTime = Time.time;
            Utils.MessageBlocker.UpdateLastMessageTime();
        }
    }
}
