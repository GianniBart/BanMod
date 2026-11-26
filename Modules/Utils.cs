//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Diagnostics;
using Il2CppSystem.IO.Ports;
using Il2CppSystem.Security.Cryptography;
using InnerNet;
using LibCpp2IL.Elf;
using MonoMod.Cil;
using MS.Internal.Xml.XPath;
using Rewired.Utils.Classes.Data;
using Rewired.Utils.Platforms.Windows;
using Sentry.Internal.Extensions;
using Sentry.Protocol;
using Sentry.Unity.NativeUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ProBuilder;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.UIR;
using static BanMod.ChatCommands;
using static BanMod.ExtendedPlayerControl;
using static BanMod.Scientist;
using static BanMod.SpamManager;
using static BanMod.Translator;
using static BanMod.Utils;
using static FilterPopUp.FilterInfoUI;
using static Il2CppMono.Security.X509.X520;
using static Il2CppSystem.Net.Mail.SmtpClient;
using static InnerNet.InnerNetClient;
using static NetworkedPlayerInfo;
using static PlayerOutfit;
using static Rewired.Utils.Classes.Utility.ObjectInstanceTracker;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;
using static UnityEngine.UIElements.UIR.Allocator2D;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using Math = System.Math;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;
using Timer = System.Timers.Timer;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace BanMod;

public static partial class Utils
{
    
    public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            foreach (var playerState in meetingHud.playerStates)
            {
                var data = GameData.Instance.GetPlayerById(playerState.PlayerId);

                if (data.IsNull() || data.Disconnected || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);

                {
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }
        }
        catch { }
    }
    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);

            chatBubble.NameText.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.2f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);

        }
        catch { }
    }
    public static string GetNameTag(NetworkedPlayerInfo playerInfo, string playerName, bool isChat = false)
    {
        var nameTag = playerName;

        if (playerInfo.Role.IsNull() || playerInfo.IsNull() || playerInfo.Disconnected ||
            playerInfo.Object.CurrentOutfit.IsNull()) return nameTag;

        var roleColor = ColorUtility.ToHtmlStringRGB(playerInfo.Role.TeamColor);

        if (isChat)
        {
            nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{GetRoleName(playerInfo)}</size></color>";
            return nameTag;
        }

        nameTag = $"<color=#{roleColor}><size=70%>{GetRoleName(playerInfo)}</size>\r\n{nameTag}</color>";

        return nameTag;
    }
    public static string GetRoleName(NetworkedPlayerInfo playerData)
    {
        var translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(playerData.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        if (translatedRole != "STRMISS") return translatedRole;

        translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(GetBehaviourByTeamType(playerData.Role.TeamType).StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        return translatedRole;
    }
    public static RoleBehaviour GetBehaviourByRoleType(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == roleType);
    }
    public static RoleBehaviour GetBehaviourByTeamType(RoleTeamTypes roleTeamType)
    {
        RoleTypes roleType = (RoleTypes)Enum.Parse(typeof(RoleTypes), roleTeamType.ToString(), true);
        RoleBehaviour role = GetBehaviourByRoleType(roleType);

        return role;
    }
    public static void ClearTasks(PlayerControl player)
    {
        if (player.Data == null)
            return;

        try
        {
            player.ClearTasks();

            if (player.Data.Tasks != null)
                player.Data.Tasks.Clear();

            player.Data.MarkDirty();

            player.Data.RpcSetTasks(new byte[0]);

            if (GameData.Instance != null)
                GameData.Instance.RecomputeTaskCounts();

            BMLogger.Info(
                $"Task FORZATAMENTE rimosse: PlayerId={player.PlayerId}, nome={player.Data.PlayerName}");
        }
        catch (Exception exception)
        {
            BMLogger.Info(
                $"Errore rimozione task PlayerId={player.PlayerId}: {exception}");
        }
    }
    public static class MeetingVoteCloser
    {
        private static readonly MethodInfo ForceSkipAllMethod =
            AccessTools.Method(typeof(MeetingHud), "ForceSkipAll");

        public static void CloseVoteNow()
        {
            MeetingHud meeting = MeetingHud.Instance;
            if (meeting == null) return;

            if (!AmongUsClient.Instance.AmHost)
                return;

            if (meeting.CurrentState != MeetingHud.MeetingStates.NotVoted &&
                meeting.CurrentState != MeetingHud.MeetingStates.Voted)
                return;

            ForceSkipAllMethod.Invoke(meeting, null);
        }
    }
    public static bool seeGhosts = false;
    public static PlayerControl GetTarget(string input)
    {
        if (byte.TryParse(input, out byte id)) return GetPlayerById(id);

        byte colorId = MsgToColor(input);
        if (colorId != byte.MaxValue)
            return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data?.DefaultOutfit.ColorId == colorId);

        string norm = NameNormalizer.NormalizeInputName(input);
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p =>
            NameNormalizer.NormalizeInputName(p.Data.PlayerName).Equals(norm, StringComparison.OrdinalIgnoreCase));
    }
    public static void SetPlayerRole(PlayerControl target, RoleTypes role)
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            Debug.LogWarning("[RoleCheats] Solo l'host può impostare i ruoli.");
            return;
        }

        if (target == null || target.Data == null)
        {
            Debug.LogWarning("[RoleCheats] Player nullo.");
            return;
        }

        try
        {
            RoleManager.Instance.SetRole(target, role);
        }
        catch (Exception e)
        {
            BMLogger.Info($"[RoleCheats] SetRole locale fallito: {e}");
        }

        try
        {
            target.RpcSetRole(role, true);
        }
        catch (Exception e)
        {
            BMLogger.Info($"[RoleCheats] RpcSetRole fallito: {e}");
        }
    }

    internal static string GetPlatformName(PlayerControl player, bool useTag = false)
    {
        if (player?.GetClient()?.PlatformData == null) return string.Empty;
        return GetPlatformName(player.GetClient().PlatformData.Platform, useTag);
    }
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
    internal static string GetPlatformName(Platforms platform, bool useTag = false)
    {
        var (platformName, tag) = platform switch
        {
            Platforms.StandaloneSteamPC => ("Steam", "PC"),
            Platforms.StandaloneEpicPC => ("Epic Games", "PC"),
            Platforms.StandaloneWin10 => ("Microsoft Store", "PC"),
            Platforms.StandaloneMac => ("Mac OS", "PC"),
            Platforms.StandaloneItch => ("Itch.io", "PC"),
            Platforms.Xbox => ("Xbox", "Console"),
            Platforms.Playstation => ("Playstation", "Console"),
            Platforms.Switch => ("Switch", "Console"),
            Platforms.Android => ("Android", "Mobile"),
            Platforms.IPhone => ("IPhone", "Mobile"),
            Platforms.Unknown => ("None", ""),
            _ => (string.Empty, string.Empty)
        };

        if (string.IsNullOrEmpty(platformName))
            return string.Empty;

        return useTag && !string.IsNullOrEmpty(tag) ? $"{tag}: {platformName}" : platformName;
    }
    public static PlayerControl FindPlayerByNetId(uint netId)
    {
        if (PlayerControl.AllPlayerControls == null)
            return null;

        for (int i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
        {
            var pc = PlayerControl.AllPlayerControls[i];
            if (pc != null && pc.NetId == netId)
                return pc;
        }

        return null;
    }
    public static List<NetworkedPlayerInfo> GetAllPlayerData()
    {
        var playerDataList = new List<NetworkedPlayerInfo>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null)
            {
                playerDataList.Add(player.Data);
            }
        }

        return playerDataList;
    }
    public class SavedOutfit
    {
        public string PlayerName;
        public int ColorId;
        public string HatId;
        public string SkinId;
        public string VisorId;
        public string PetId;
        public string NamePlateId;
    }
    public static SavedOutfit OriginalOutfit = null;
    public static void SaveOriginalOutfit(PlayerControl player)
    {
        if (player == null || player.Data == null) return;

        if (OriginalOutfit != null) return;

        var o = PlayerControl.LocalPlayer.Data.DefaultOutfit;

        OriginalOutfit = new SavedOutfit()
        {
            ColorId = o.ColorId,
            HatId = o.HatId,
            SkinId = o.SkinId,
            VisorId = o.VisorId,
            PetId = o.PetId,
            NamePlateId = o.NamePlateId
        };

        BMLogger.Info("[BanMod] Outfit originale salvato con successo.");
    }
    public static void CopyOutfit(PlayerControl source, PlayerControl target)
    {
        if (source == null || target == null) return;

        var o = source.Data.DefaultOutfit;


        if (target == AmongUsClient.Instance.AmHost)
        {
            target.RpcSetColor((byte)o.ColorId);
            target.RpcSetHat(o.HatId);
            target.RpcSetSkin(o.SkinId);
            target.RpcSetVisor(o.VisorId);
            target.RpcSetPet(o.PetId);
            target.RpcSetNamePlate(o.NamePlateId);

            var t = target.Data.DefaultOutfit;
            t.ColorId = o.ColorId;
            t.HatId = o.HatId;
            t.SkinId = o.SkinId;
            t.VisorId = o.VisorId;
            t.PetId = o.PetId;
            t.NamePlateId = o.NamePlateId;
        }
        if (target != AmongUsClient.Instance.AmHost)
        {
            target.RpcSetHat(o.HatId);
            target.RpcSetSkin(o.SkinId);
            target.RpcSetVisor(o.VisorId);
            target.RpcSetPet(o.PetId);
            target.RpcSetNamePlate(o.NamePlateId);

            var t = target.Data.DefaultOutfit;
            t.HatId = o.HatId;
            t.SkinId = o.SkinId;
            t.VisorId = o.VisorId;
            t.PetId = o.PetId;
            t.NamePlateId = o.NamePlateId;
        }

        target.Data.MarkDirty();
    }
    public static void CopyOutfit1(PlayerControl source, PlayerControl target)
    {
        if (source == null || target == null) return;

        var o = source.Data.DefaultOutfit;

        if (target == AmongUsClient.Instance.AmHost)
        {
            target.RpcSetColor((byte)o.ColorId);
            target.RpcSetHat(o.HatId);
            target.RpcSetSkin(o.SkinId);
            target.RpcSetVisor(o.VisorId);

            var t = target.Data.DefaultOutfit;
            t.ColorId = o.ColorId;
            t.HatId = o.HatId;
            t.SkinId = o.SkinId;
            t.VisorId = o.VisorId;
        }
    }
    public static void RestoreOriginalOutfit(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        if (OriginalOutfit == null) return;

        var o = OriginalOutfit;

        if (AmongUsClient.Instance.AmHost)
        {
            player.RpcSetColor((byte)o.ColorId);
            player.RpcSetHat(o.HatId);
            player.RpcSetSkin(o.SkinId);
            player.RpcSetVisor(o.VisorId);
            player.RpcSetPet(o.PetId);
            player.RpcSetNamePlate(o.NamePlateId);

            var info = player.Data.DefaultOutfit;
            info.ColorId = o.ColorId;
            info.HatId = o.HatId;
            info.SkinId = o.SkinId;
            info.VisorId = o.VisorId;
            info.PetId = o.PetId;
            info.NamePlateId = o.NamePlateId;
        }
        else
        {
            player.RpcSetHat(o.HatId);
            player.RpcSetSkin(o.SkinId);
            player.RpcSetVisor(o.VisorId);
            player.RpcSetPet(o.PetId);
            player.RpcSetNamePlate(o.NamePlateId);

            var info = player.Data.DefaultOutfit;
            info.HatId = o.HatId;
            info.SkinId = o.SkinId;
            info.VisorId = o.VisorId;
            info.PetId = o.PetId;
            info.NamePlateId = o.NamePlateId;
        }

        player.Data.MarkDirty();
    }

    public static bool stringToPlatformType(string platformStr, out Platforms? platform)
    {

        if (!string.IsNullOrEmpty(platformStr))
        {

            try
            {
                platform = (Platforms)System.Enum.Parse(typeof(Platforms), platformStr, true);

                return true;

            }
            catch { }

        }

        platform = null;
        return false;
    }

    public static string PlatformTypeToString(Platforms platform)
    {
        return platform switch
        {
            Platforms.StandaloneEpicPC => "Epic",
            Platforms.StandaloneSteamPC => "Steam",
            Platforms.StandaloneMac => "Mac",
            Platforms.StandaloneWin10 => "Microsoft Store",
            Platforms.StandaloneItch => "Itch.io",
            Platforms.IPhone => "iPhone / iPad",
            Platforms.Android => "Android",
            Platforms.Switch => "Nintendo Switch",
            Platforms.Xbox => "Xbox",
            Platforms.Playstation => "PlayStation",
            _ => "Unknown"
        };
    }
    
    public static void ResetLobby()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var gsm = DestroyableSingleton<GameStartManager>.Instance;
        if (gsm == null || gsm.LobbyPrefab == null) return;

        var oldLobby = LobbyBehaviour.Instance;

        if (oldLobby != null)
        {
            (oldLobby as InnerNetObject)?.Despawn();
            LobbyBehaviour.Instance = null;
        }

        var newLobby = UnityEngine.Object.Instantiate(gsm.LobbyPrefab);
        LobbyBehaviour.Instance = newLobby;
        AmongUsClient.Instance.Spawn(newLobby, -2, SpawnFlags.None);
    }

    public static class CheatUtils
    {
        public static void CompletaTutteLeTask()
        {
            var player = PlayerControl.LocalPlayer;
            var client = AmongUsClient.Instance;

            if (player == null || player.Data == null)
            {
                Debug.LogWarning("Giocatore locale non disponibile.");
                return;
            }

            if (client == null)
            {
                Debug.LogWarning("AmongUsClient non inizializzato.");
                return;
            }

            var taskList = player.Data.Tasks;
            if (taskList == null || taskList.Count == 0)
            {
                Debug.LogWarning("Nessuna task da completare.");
                return;
            }

            HudManager.Instance.StartCoroutine(CompletaTutteLeTaskConDelay(1.5f));
        }
        public static IEnumerator CompletaTutteLeTaskConDelay(float delayPerTask = 1.5f)
        {
            var player = PlayerControl.LocalPlayer;
            var client = AmongUsClient.Instance;

            if (player == null || player.Data == null || client == null) yield break;

            bool isHost = client.AmHost;
            var taskList = player.Data.Tasks;

            if (taskList == null || taskList.Count == 0) yield break;

            var idsDaCompletare = new List<int>(taskList.Count);
            foreach (var taskInfo in taskList)
            {
                if (taskInfo != null && !taskInfo.Complete)
                    idsDaCompletare.Add((int)taskInfo.Id);
            }

            foreach (int id in idsDaCompletare)
            {
                var listaCorrente = player.Data.Tasks;
                if (listaCorrente == null) break;

                TaskInfo task = null;
                for (int i = 0; i < listaCorrente.Count; i++)
                {
                    if (listaCorrente[i] != null && (int)listaCorrente[i].Id == id)
                    {
                        task = listaCorrente[i];
                        break;
                    }
                }

                if (task == null) continue;

                var writer = client.StartRpcImmediately(
                    player.NetId,
                    (byte)RpcCalls.CompleteTask,
                    SendOption.Reliable,
                    client.HostId
                );

                writer.WritePacked(id);
                client.FinishRpcImmediately(writer);

                task.Complete = true;

                yield return new WaitForSeconds(Mathf.Max(1.5f, delayPerTask));
            }

            var taskSnapshot = player.myTasks.ToArray();
            foreach (var t in taskSnapshot)
            {
                if (t == null || t.IsComplete) continue;

                if (t is NormalPlayerTask normal)
                {
                    while (normal.TaskStep < normal.MaxStep)
                    {
                        normal.NextStep();
                        yield return new WaitForSeconds(1.25f);
                    }
                }

                t.Complete();
                yield return new WaitForSeconds(0.05f);
            }

            player.Data.MarkDirty();

        }

        public static void BypassScanner(bool value)
        {
            try
            {
                if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null)
                    return;

                byte scannerCount = (byte)(PlayerControl.LocalPlayer.scannerCount + 1);
                PlayerControl.LocalPlayer.scannerCount = scannerCount;
                PlayerControl.LocalPlayer.SetScanner(value, scannerCount);

                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(
                        PlayerControl.LocalPlayer.NetId, 15, SendOption.Reliable, p.OwnerId);

                    writer.Write(value);
                    writer.Write(scannerCount);

                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Errore in BypassScanner: " + e.Message);
            }
        }

        public static void HandleScannerRPC(PlayerControl instance, byte callId, MessageReader reader)
        {
            if (callId != 15) return;

            try
            {
                bool scanValue = reader.ReadBoolean();
                byte scanCount = reader.ReadByte();

                instance.SetScanner(scanValue, scanCount);
            }
            catch (Exception e)
            {
                Debug.LogError("Errore in HandleScannerRPC: " + e.Message);
            }
        }

        public static IEnumerator BypassScannerWithTimeout(float duration)
        {
            BypassScanner(true);
            yield return new WaitForSeconds(duration);
            BypassScanner(false);
        }
        public static void PlayAnimation(byte animationType)
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) return;

            player.PlayAnimation(animationType);

            RpcPlayAnimationMessage rpc = new(player.NetId, animationType);
            AmongUsClient.Instance.LateBroadcastUnreliableMessage(Unsafe.As<IGameDataMessage>(rpc));
        }

        public static void AnimShields() => PlayAnimation((byte)TaskTypes.PrimeShields);
        public static void AnimAsteroids() => PlayAnimation((byte)TaskTypes.ClearAsteroids);
        public static void AnimEmptyGarbage() => PlayAnimation((byte)TaskTypes.EmptyGarbage);

        public static void CamsOn()
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 1);
        }

        public static void CamsOff()
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 0);
        }
    }
    public static class MainMenuInfo
    {
        private class InfoMessage
        {
            public GameObject Root;
            public float ExpireAt;
        }

        private static readonly List<InfoMessage> Messages = new();

        private const float StartX = 3.2f;
        private const float StartY = -2.3f;
        private const float Spacing = 0.45f;
        private const float Duration = 3f;
        private const float BoxWidth = 5.7f;
        private const float BoxHeight = 0.75f;

        private static readonly Color BackgroundColor =
            new Color(0.055f, 0.060f, 0.075f, 0.98f);

        private static readonly Color BorderColor =
            new Color(0.15f, 0.55f, 0.90f, 0.65f);

        private static readonly Color AccentColor =
            new Color(0.20f, 0.70f, 1.00f, 1f);

        private static readonly Color TextColor =
            new Color(0.90f, 0.95f, 1.00f, 1f);

        private static readonly Color ShadowColor =
            new Color(0f, 0f, 0f, 0.55f);

        private static Sprite RoundedSprite;


        public static void Show(string message)
        {
            var menu = Object.FindObjectOfType<MainMenuManager>();

            if (menu == null)
                return;

            Cleanup();
            EnsureResources();

            var root = new GameObject(
                $"MainMenuInfo_{Messages.Count}"
            );

            root.transform.SetParent(
                menu.transform,
                false
            );
            root.transform.localScale = new Vector3(
                0.5f,
                0.5f,
                1f
            );

            var shadow = new GameObject("Shadow");

            shadow.transform.SetParent(
                root.transform,
                false
            );

            var shadowRenderer =
                shadow.AddComponent<SpriteRenderer>();

            shadowRenderer.sprite = RoundedSprite;
            shadowRenderer.color = ShadowColor;
            shadowRenderer.sortingOrder = 998;

            shadow.transform.localScale =
                new Vector3(
                    BoxWidth + 0.08f,
                    BoxHeight + 0.08f,
                    1f
                );

            shadow.transform.localPosition =
                new Vector3(
                    0.05f,
                    -0.05f,
                    0.08f
                );



            var border = new GameObject("Border");

            border.transform.SetParent(
                root.transform,
                false
            );

            var borderRenderer =
                border.AddComponent<SpriteRenderer>();

            borderRenderer.sprite = RoundedSprite;
            borderRenderer.color = BorderColor;
            borderRenderer.sortingOrder = 999;

            border.transform.localScale =
                new Vector3(
                    BoxWidth + 0.045f,
                    BoxHeight + 0.045f,
                    1f
                );

            border.transform.localPosition =
                Vector3.zero;


            var background =
                new GameObject("Background");

            background.transform.SetParent(
                root.transform,
                false
            );

            var backgroundRenderer =
                background.AddComponent<SpriteRenderer>();

            backgroundRenderer.sprite =
                RoundedSprite;

            backgroundRenderer.color =
                BackgroundColor;

            backgroundRenderer.sortingOrder =
                1000;

            background.transform.localScale =
                new Vector3(
                    BoxWidth,
                    BoxHeight,
                    1f
                );

            background.transform.localPosition =
                Vector3.zero;


            var accent =
                new GameObject("Accent");

            accent.transform.SetParent(
                root.transform,
                false
            );

            var accentRenderer =
                accent.AddComponent<SpriteRenderer>();

            accentRenderer.sprite =
                RoundedSprite;

            accentRenderer.color =
                AccentColor;

            accentRenderer.sortingOrder =
                1001;

            accent.transform.localScale =
                new Vector3(
                    0.075f,
                    BoxHeight - 0.14f,
                    1f
                );

            accent.transform.localPosition =
                new Vector3(
                    -(BoxWidth / 2f) + 0.14f,
                    0f,
                    -0.03f
                );


            var textObject =
                new GameObject("Text");

            textObject.transform.SetParent(
                root.transform,
                false
            );

            var text =
                textObject.AddComponent<TextMeshPro>();

            text.text = message;

            text.fontSize = 2.45f;

            text.fontStyle =
                FontStyles.Bold;

            text.alignment =
                TextAlignmentOptions.Center;

            text.enableWordWrapping =
                false;

            text.richText = true;

            text.color =
                TextColor;

            text.renderer.sortingOrder =
                1002;

            text.outlineWidth = 0.12f;

            text.outlineColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.75f
                );

            textObject.transform.localPosition =
                new Vector3(
                    0.10f,
                    0f,
                    -0.10f
                );


            Messages.Add(
                new InfoMessage
                {
                    Root = root,

                    ExpireAt =
                        Time.time + Duration
                }
            );

            Reposition();
        }


        public static void Update()
        {
            bool changed = false;

            for (
                int i = Messages.Count - 1;
                i >= 0;
                i--
            )
            {
                var message =
                    Messages[i];

                if (
                    message.Root == null ||
                    Time.time >= message.ExpireAt
                )
                {
                    if (message.Root != null)
                    {
                        Object.Destroy(
                            message.Root
                        );
                    }

                    Messages.RemoveAt(i);

                    changed = true;
                }
            }

            if (changed)
            {
                Reposition();
            }
        }

        private static void Cleanup()
        {
            for (
                int i = Messages.Count - 1;
                i >= 0;
                i--
            )
            {
                if (Messages[i].Root == null)
                {
                    Messages.RemoveAt(i);
                }
            }
        }

        private static void Reposition()
        {
            for (
                int i = 0;
                i < Messages.Count;
                i++
            )
            {
                if (Messages[i].Root == null)
                    continue;

                float y =
                    StartY +
                    (i * Spacing);

                Messages[i]
                    .Root
                    .transform
                    .localPosition =
                    new Vector3(
                        StartX,
                        y,
                        -5f
                    );
            }
        }

        private static void EnsureResources()
        {
            if (RoundedSprite != null)
                return;

            const int size = 64;
            const int radius = 15;

            Texture2D texture =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false
                );

            texture.hideFlags =
                HideFlags.DontUnloadUnusedAsset;

            texture.wrapMode =
                TextureWrapMode.Clamp;

            texture.filterMode =
                FilterMode.Bilinear;

            Color32[] pixels =
                new Color32[size * size];

            Color32 fill =
                Color.white;

            float r =
                Mathf.Max(
                    1f,
                    radius
                );

            float leftCenter =
                r - 0.5f;

            float rightCenter =
                size - r - 0.5f;

            float bottomCenter =
                r - 0.5f;

            float topCenter =
                size - r - 0.5f;


            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x;
                    float cy = y;

                    if (x < radius)
                    {
                        cx =
                            leftCenter;
                    }
                    else if (
                        x >= size - radius
                    )
                    {
                        cx =
                            rightCenter;
                    }

                    if (y < radius)
                    {
                        cy =
                            bottomCenter;
                    }
                    else if (
                        y >= size - radius
                    )
                    {
                        cy =
                            topCenter;
                    }


                    float dx =
                        x - cx;

                    float dy =
                        y - cy;

                    float distance =
                        Mathf.Sqrt(
                            dx * dx +
                            dy * dy
                        );


                    bool inCorner =
                        (
                            x < radius ||
                            x >= size - radius
                        )
                        &&
                        (
                            y < radius ||
                            y >= size - radius
                        );


                    float alphaFactor =
                        1f;

                    if (inCorner)
                    {
                        alphaFactor =
                            Mathf.Clamp01(
                                r +
                                0.5f -
                                distance
                            );
                    }


                    Color32 pixel =
                        fill;

                    pixel.a =
                        (byte)Mathf.RoundToInt(
                            fill.a *
                            alphaFactor
                        );

                    pixels[
                        y * size + x
                    ] = pixel;
                }
            }


            texture.SetPixels32(
                pixels
            );

            texture.Apply(
                false,
                false
            );


            RoundedSprite =
                Sprite.Create(
                    texture,

                    new Rect(
                        0,
                        0,
                        size,
                        size
                    ),

                    new Vector2(
                        0.5f,
                        0.5f
                    ),

                    size
                );

            RoundedSprite.hideFlags =
                HideFlags.DontUnloadUnusedAsset;
        }

        public static void Clear()
        {
            foreach (
                var message in Messages
            )
            {
                if (message.Root != null)
                {
                    Object.Destroy(
                        message.Root
                    );
                }
            }

            Messages.Clear();
        }
    }
    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", string.Empty);
    public static string ColorString(Color32 color, string str) => $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
    private static readonly Dictionary<string, Sprite> CachedSprites = [];
    private static readonly DateTime TimeStampStartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static NetworkedPlayerInfo GetPlayerInfoById(int PlayerId) =>
       GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => info.PlayerId == PlayerId);

    public static class SpamDetector
    {
        private class PlayerSpamData
        {
            public int MessageCount;
            public float LastResetTime;
        }

        private static Dictionary<int, PlayerSpamData> _playerData = new Dictionary<int, PlayerSpamData>();
        private const int MaxMessagesPerSecond = 60;

        public static bool IsSpamming(int playerId)
        {
            if (!_playerData.TryGetValue(playerId, out var data))
            {
                data = new PlayerSpamData { MessageCount = 0, LastResetTime = Time.time };
                _playerData[playerId] = data;
            }

            float now = Time.time;

            if (now - data.LastResetTime > 1f)
            {
                data.MessageCount = 0;
                data.LastResetTime = now;
            }

            data.MessageCount++;
            return data.MessageCount > MaxMessagesPerSecond;
        }

        public static void ResetPlayer(int playerId)
        {
            _playerData.Remove(playerId);
        }

        public static void ResetAll()
        {
            _playerData.Clear();
        }
    }
    public static void BanPlayer(int playerId)
    {
        NotificationPopper_AddInfoMessagePatch.AddInfoMessage(HudManager.Instance.Notifier, $"[NetworkBan] Player {playerId} banned for spamming.");
        BMLogger.LogWarning($"[NetworkBan] Player {playerId} banned for spamming.");
        AmongUsClient.Instance.KickPlayer(playerId, true);
        SpamDetector.ResetPlayer(playerId);
    }
    public static class RpcMap
    {
        public static readonly Dictionary<byte, string> RpcNames = new Dictionary<byte, string>()
    {
        { 0, "PlayAnimation" },
        { 1, "CompleteTask" },
        { 2, "SyncSettings" },
        { 3, "SetInfected" },
        { 4, "Exiled" },
        { 5, "CheckName" },
        { 6, "SetName" },
        { 7, "CheckColor" },
        { 8, "SetColor" },
        { 9, "SetHat_Deprecated" },
        { 10, "SetSkin_Deprecated" },
        { 11, "ReportDeadBody" },
        { 12, "MurderPlayer" },
        { 13, "SendChat" },
        { 14, "StartMeeting" },
        { 15, "SetScanner" },
        { 16, "SendChatNote" },
        { 17, "SetPet_Deprecated" },
        { 18, "SetStartCounter" },
        { 19, "EnterVent" },
        { 20, "ExitVent" },
        { 21, "SnapTo" },
        { 22, "CloseMeeting" },
        { 23, "VotingComplete" },
        { 24, "CastVote" },
        { 25, "ClearVote" },
        { 26, "AddVote" },
        { 27, "CloseDoorsOfType" },
        { 29, "SetTasks" },
        { 31, "ClimbLadder" },
        { 32, "UsePlatform" },
        { 33, "SendQuickChat" },
        { 34, "BootFromVent" },
        { 35, "UpdateSystem" },
        { 36, "SetVisor_Deprecated" },
        { 37, "SetNamePlate_Deprecated" },
        { 38, "SetLevel" },
        { 39, "SetHatStr" },
        { 40, "SetSkinStr" },
        { 41, "SetPetStr" },
        { 42, "SetVisorStr" },
        { 43, "SetNamePlateStr" },
        { 44, "SetRole" },
        { 45, "ProtectPlayer" },
        { 46, "Shapeshift" },
        { 47, "CheckMurder" },
        { 48, "CheckProtect" },
        { 49, "Pet" },
        { 50, "CancelPet" },
        { 51, "CheckZipline" },
        { 52, "UseZipline" },
        { 53, "TriggerSpores" },
        { 54, "CheckSpore" },
        { 55, "CheckShapeshift" },
        { 56, "RejectShapeshift" },
        { 60, "LobbyTimeExpiring" },
        { 61, "ExtendLobbyTimer" },
        { 62, "CheckVanish" },
        { 63, "StartVanish" },
        { 64, "CheckAppear" },
        { 65, "StartAppear" }
    };

        public static string GetRpcName(byte id)
        {
            return RpcNames.TryGetValue(id, out var name)
                ? name
                : $"UnknownRpc_{id}";
        }
    }
    public static void AddChatBypass(PlayerControl sourcePlayer, string chatText, bool censor = true)
    {
        if (sourcePlayer == null || PlayerControl.LocalPlayer == null) return;

        ChatController chat = DestroyableSingleton<ChatController>.Instance;
        if (chat == null) return;

        var dataSelf = PlayerControl.LocalPlayer.Data;
        var dataSource = sourcePlayer.Data;
        if (dataSelf == null || dataSource == null) return;

        ChatBubble bubble = chat.GetPooledBubble();
        try
        {
            bubble.transform.SetParent(chat.scroller.Inner);
            bubble.transform.localScale = Vector3.one;

            bool isSelf = (sourcePlayer == PlayerControl.LocalPlayer);
            if (isSelf) bubble.SetRight();
            else bubble.SetLeft();

            bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);

            bubble.SetCosmetics(dataSource);
            chat.SetChatBubbleName(bubble, dataSource, dataSource.IsDead, didVote, PlayerNameColor.Get(dataSource), null);

            if (censor && DataManager.Settings.Multiplayer.CensorChat)
                chatText = BlockedWords.CensorWords(chatText, false);

            bubble.SetText(chatText);
            bubble.AlignChildren();
            chat.AlignAllBubbles();

            if (!chat.IsOpenOrOpening && chat.notificationRoutine == null)
            {
                chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
            }

            if (!isSelf && !chat.IsOpenOrOpening)
            {
                SoundManager.Instance.PlaySound(chat.messageSound, false, 1f, null)
                    .pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
                chat.chatNotification.SetUp(sourcePlayer, chatText);
            }
        }
        catch (Exception)
        {
            BMLogger.Error(null);
            chat.chatBubblePool.Reclaim(bubble);
        }
    }
    public static int GetCurrentLobbyPlayerCount()
    {
        int count = 0;
        for (int i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            var player = GameData.Instance.AllPlayers[i];
            if (player != null && !player.Disconnected)
            {
                count++;
            }
        }
        return count;
    }

    public static string GetCurrentLobbyMode()
    {
        string lobbymode = "";
        GameModeType gameMode = Options.GameMode.Selected;
        bool hideAndSeek = GameManager.Instance.IsHideAndSeek();
        bool Normal = GameManager.Instance.IsNormal();
        if (hideAndSeek)
        {
            lobbymode = "HideAndSeek";
        }
        else if (Normal && gameMode == GameModeType.KaitoRun)
        {
            lobbymode = "KaitoRun";
        }
        else if (Normal && gameMode == GameModeType.SnS)
        {
            lobbymode = "SnS";
        }
        else if (Normal && gameMode == GameModeType.PnS)
        {
            lobbymode = "PnS";
        }
        else if (Normal && gameMode == GameModeType.Default)
        {
            lobbymode = "Default";
        }
        else if (Normal && gameMode == GameModeType.TaskRun)
        {
            lobbymode = "TaskRun";
        }
        else if (Normal && gameMode == GameModeType.FFA)
        {
            lobbymode = "FFA";
        }
        else if (Normal && gameMode == GameModeType.RoomRush)
        {
            lobbymode = "RoomRush";
        }
        else if (Normal && gameMode == GameModeType.TargetRush)
        {
            lobbymode = "TargetRush";
        }
        else if (Normal && gameMode == GameModeType.DeathRun)
        {
            lobbymode = "DeathRun";
        }
        else if (Normal && gameMode == GameModeType.FFATeam)
        {
            lobbymode = "FFA_Team";
        }
        else if (Normal && gameMode == GameModeType.ZombieMode)
        {
            lobbymode = "ZombieMode";
        }
        else if (Normal && gameMode == GameModeType.HotPotatoModded)
        {
            lobbymode = "HotPotato";
        }
        else if (Normal && gameMode == GameModeType.Assassin)
        {
            lobbymode = "FFA_Assassin";
        }
        else if (Normal && gameMode == GameModeType.KillRace)
        {
            lobbymode = "FFA_KillRace";
        }
        else
        {
            lobbymode = "UnkNow";
        }
        return lobbymode;
    }
    public static string GetCurrentStatus()
    {
        string status = "";

        if (GameStates.InGame)
        {
            status = "In_Game";
        }
        else if (GameStates.isLobby)
        {
            status = "In_Lobby";
        }

        return status;
    }
    public static MapNames GetActiveMapName() => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
    public static MapNames GetCurrentMap()
    {
        if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.CurrentGameOptions == null)
        {
            return (MapNames)(-1);
        }

        byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;

        switch (mapId)
        {
            case 0: return MapNames.Skeld;
            case 1: return MapNames.MiraHQ;
            case 2: return MapNames.Polus;
            case 3: return MapNames.Dleks;
            case 4: return MapNames.Airship;
            case 5: return MapNames.Fungle;
            default:
                return (MapNames)(-1);
        }
    }
    public static string GetRegionName(IRegionInfo region = null)
    {
        region ??= ServerManager.Instance.CurrentRegion;

        string name = region.Name;

        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
        {
            name = "Local Games";
            return name;
        }

        if (region.PingServer.EndsWith("among.us", StringComparison.Ordinal))
        {
            if (name == "North America") name = "NA";
            else if (name == "Europe") name = "EU";
            else if (name == "Asia") name = "AS";

            return name;
        }
        return name;
    }

    public static class LanguageUtils
    {
        public static string GetLanguageName(IGameOptions options)
        {
            if (options == null) return "Unknown";

            GameKeywords keywords = (GameKeywords)options.Keywords;

            if (keywords.HasFlag(GameKeywords.English)) return "English";
            if (keywords.HasFlag(GameKeywords.Italian)) return "Italian";
            if (keywords.HasFlag(GameKeywords.French)) return "French";
            if (keywords.HasFlag(GameKeywords.German)) return "German";
            if (keywords.HasFlag(GameKeywords.SpanishLA)) return "Spanish (LA)";
            if (keywords.HasFlag(GameKeywords.SpanishEU)) return "Spanish (EU)";
            if (keywords.HasFlag(GameKeywords.Brazilian)) return "Brazilian Portuguese";
            if (keywords.HasFlag(GameKeywords.Portuguese)) return "Portuguese";
            if (keywords.HasFlag(GameKeywords.Korean)) return "Korean";
            if (keywords.HasFlag(GameKeywords.Russian)) return "Russian";
            if (keywords.HasFlag(GameKeywords.Dutch)) return "Dutch";
            if (keywords.HasFlag(GameKeywords.Filipino)) return "Filipino";
            if (keywords.HasFlag(GameKeywords.Japanese)) return "Japanese";
            if (keywords.HasFlag(GameKeywords.Arabic)) return "Arabic";
            if (keywords.HasFlag(GameKeywords.Polish)) return "Polish";
            if (keywords.HasFlag(GameKeywords.SChinese)) return "Simplified Chinese";
            if (keywords.HasFlag(GameKeywords.TChinese)) return "Traditional Chinese";
            if (keywords.HasFlag(GameKeywords.Irish)) return "Irish";

            if (keywords == GameKeywords.All) return "All";
            if (keywords == GameKeywords.Other) return "Other";

            return "Unknown";
        }

        public static IGameOptions GetCurrentGameOptions()
        {
            if (GameOptionsManager.Instance == null)
                return null;

            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                return GameOptionsManager.Instance.GameHostOptions;
            }
            else
            {
                return GameOptionsManager.Instance.GameSearchOptions;
            }
        }
    }

    public class ButtonData
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
    public static string ColorString(UnityEngine.Color c, string s)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{s}</color>";
    }
    public static class FancyText
    {
        private static readonly Dictionary<char, string> fancyMap = new Dictionary<char, string>
    {
        {'A', "卂"}, {'B', "乃"}, {'C', "匚"}, {'D', "刀"}, {'E', "乇"},
        {'F', "下"}, {'G', "厶"}, {'H', "卄"}, {'I', "丨"}, {'J', "ﾌ"},
        {'K', "ズ"}, {'L', "ㄥ"}, {'M', "爪"}, {'N', "𠘨"}, {'O', "ㄖ"},
        {'P', "卩"}, {'Q', "㔿"}, {'R', "尺"}, {'S', "丂"}, {'T', "ㄒ"},
        {'U', "ㄩ"}, {'V', "ᐯ"}, {'W', "山"}, {'X', "メ"}, {'Y', "ㄚ"}, {'Z', "乙"}
    };

        public static string ToFancy(string input)
        {
            var sb = new StringBuilder();

            foreach (char c in input)
            {
                char upper = char.ToUpper(c);

                if (fancyMap.ContainsKey(upper))
                    sb.Append(fancyMap[upper]);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
    private static readonly System.Random Rand = new System.Random();
    public static readonly List<string> InsultiDisponibili = new List<string>();

    public static string PrendiInsulto()
    {
        if (InsultiDisponibili.Count == 0)
        {
            InsultiDisponibili.AddRange(Insulti);

            for (int i = InsultiDisponibili.Count - 1; i > 0; i--)
            {
                int j = Rand.Next(i + 1);
                string temp = InsultiDisponibili[i];
                InsultiDisponibili[i] = InsultiDisponibili[j];
                InsultiDisponibili[j] = temp;
            }
        }

        string insulto = InsultiDisponibili[InsultiDisponibili.Count - 1];
        InsultiDisponibili.RemoveAt(InsultiDisponibili.Count - 1);

        return insulto;
    }
    public static readonly string[] Insulti = new[]
    {
        "Non mi aspettavo niente da te e sono rimasto comunque deluso",
        "sei cosi brutto/a che se ti vede il gatto nero si gratta le palle e gira l'angolo",
        "sei così vacca che in India ti fanno sacra",
        "sei talmente zoccola che se ti dicono batti il 5 controlli subito l'agenda",
        "sei fastidioso/a come un chiodo nel culo",
        "sei talmente sfigato che se ti cade l'uccello rimbalza e ti picchia nel culo",
        "sei simpatico come un dito in culo e puzzi pure peggio",
        "fai talmente schifo che devi dare il viagra al tuo vibratore",
        "non ti picchio solo perchè la merda schizza!",
        "Sei raro come una figura mitologica: il corpo di uomo e la testa di cazzo",
        "Sei come Unieuro: batti, forte, sempre",
        "sei come una nuvola, se ti levi dalle palle è una bella giornata.",
        "cagati in mano e poi prenditi a sberle.",
        "E' vero che la natura fa brutti scherzi, ma a te t'ha preso proprio per il culo.",
        "Sei talmente stupido che riusciresti a farti investire da un auto parcheggiata.",
        "Se sei in giro con la bici e ti senti felice, guarda bene, forse hai dimenticato il sellino!",
        "Vorrei poterti entrare nella testa, per provare l'ebbrezza del vuoto",
        "Sei utile come un culo senza il buco",
        "Sei cosi scemo che se vai al cinema e leggi vietato ai minori di 18 torni con 17 amici",
        "Non prendertela se ti considerano mezzo scemo. Si vede che ti conoscono solo a metà.",
        "Sei così stupido che non troveresti una spina in una foresta di cactus.",
        "Ti farei tanti applausi… ma con la tua faccia in mezzo!",
        "Oggi ho sentito che è stato ritrovato un corpo senza cervello, ti prego dimmi che stai bene!",
        "Ti darei cinque minuti di intelligenza solo per farti capire quanto sei idiota!",
        "Sei talmente stupido che accenderesti la luce per vedere se è buio.",
        "Se ti ho offeso con queste battute, ti chiedo scusa. Non pensavo sapessi leggere.",
        "Non sei un problema da risolvere: sei proprio un errore da ignorare.",
        "Non serve insultarti: ti ci pensi da solo ogni volta che apri bocca.",
        "Vorrei cimentarmi in un duello intellettuale con te, ma vedo che sei disarmato.",
        "quando eri piccolo la tua altalena era probabilmente troppo vicina al muro!",
        "Uno di noi due è più stupido di me.",
        "Ricordo di essere stato al tuo livello di ignoranza, ero solo un bambino all'epoca.",
        "fai un clistere nelle orecchie perchè è il tuo cervello che è pieno di cacca non l'intestino.",
        "sei così sfigato che se compri un boomerang non torna per scelta",
        "hai la simpatia di una diarrea prima di un colloquio",
        "Sei così lento che Internet Explorer ti manda a cagare perché deve aspettarti.",
        "sei più inutile di un preservativo bucato",
        "Sei così coglione che se ti mettono il cervello nel culo caghi idee migliori.",
"Sei talmente rincoglionito che pure il tuo cazzo ti guarda e scuote la testa.",
"Sei così inutile che se ti infilassero nel culo a qualcuno saresti comunque d'intralcio.",
"Sei così stronzo che quando scorreggi chiedi scusa alla merda.",
"Sei talmente idiota che se avessi un neurone in più sarebbe comunque in ferie.",
"Sei così coglione che potresti inciampare nel tuo stesso cazzo anche senza averlo.",
"Sei una testa di cazzo con così tanta esperienza che ormai dovresti avere la partita IVA.",
"Sei così stupido che quando ti masturbi perdi contro te stesso.",
"Sei talmente merda che le mosche ti vedono e ordinano da asporto.",
"Sei così inutile che anche il buco del culo ha più responsabilità di te.",
"Sei così rincoglionito che se ti dicono 'vai a cagare' chiedi l'indirizzo.",
"Sei talmente coglione che se il cervello fosse merda avresti comunque la stitichezza.",
"Sei così brutto che quando ti fai una sega chiudi gli occhi per non rovinarti il momento.",
"Sei così stronzo che persino il tuo culo vorrebbe cambiare proprietario.",
"Sei così coglione che se ti sparano in testa il proiettile esce chiedendo scusa per il disturbo.",
"Sei talmente idiota che il tuo cervello ha messo il cartello 'affittasi'.",
"Sei così inutile che persino una scorreggia ha più impatto sociale di te.",
"Sei così coglione che se ti fai un clistere rischi di lavarti anche il cervello.",
"Sei così merda che quando vai al cesso il water tira lo sciacquone prima che ti siedi.",
"Sei così stronzo che persino la diarrea ti considera troppo liquido.",
"Sei talmente rincoglionito che quando ti dicono 'fatti fottere' chiedi giorno e ora.",
"Sei così stupido che il tuo cazzo ha più memoria muscolare del tuo cervello.",
"Sei così inutile che il preservativo rotto almeno ogni tanto crea qualcosa.",
"Sei così coglione che se ti mettono un dito nel culo pensi sia un aggiornamento firmware.",
"Sei così stronzo che se ti caghi addosso migliori il profumo.",
"Sei talmente testa di cazzo che quando pensi ti viene un'erezione al cervello, ma dura due secondi.",
"Sei così idiota che se ti dessero un cervello nuovo chiederesti lo scontrino per cambiarlo.",
"Sei così brutto che il tuo specchio ha chiesto il trasferimento.",
"Sei così coglione che se scopi una bambola gonfiabile è lei che finge l'orgasmo.",
"Sei talmente inutile che se fossi carta igienica saresti già sporca prima dell'uso.",
"Sei così scemo che se ti infili una lampadina nel culo probabilmente ti si accende un'idea.",
"Sei così stronzo che il tuo culo ti denuncia per concorrenza sleale.",
"Sei così rincoglionito che potresti perderti dentro un monolocale.",
"Sei così coglione che se ti mettono davanti a un bivio riesci a sbagliare tre volte.",
"Sei così stupido che quando caghi perdi il 90% della tua massa cerebrale.",
"Sei talmente merda che persino lo scopino del cesso ti guarda dall'alto in basso.",
"Sei così coglione che hai probabilmente bisogno di un tutorial per farti una sega.",
"Sei così inutile che se fossi un vibratore avresti le pile scariche dalla fabbrica.",
"Sei così stupido che hai più probabilità di trovare il tuo cervello in un uovo Kinder.",
"Sei talmente stronzo che quando sei nato il medico ha schiaffeggiato tua madre per vendetta.",
"Sei così rincoglionito che quando senti 'testa di cazzo' ti giri pensando ti abbiano chiamato per nome.",
"Sei così coglione che potresti venire bocciato a un test della personalità.",
"Sei talmente brutto che quando ti spogli il tuo cazzo prova a rientrare.",
"Sei così inutile che se ti mettono nel bidone dell'umido arriva una multa per raccolta differenziata sbagliata.",
"Sei così coglione che il tuo angelo custode ha chiesto il reddito di cittadinanza.",
"Sei così stupido che anche i tuoi spermatozoi probabilmente nuotano verso il culo.",
"Sei così stronzo che se ti lavi il culo l'acqua presenta denuncia.",
"Sei talmente rincoglionito che quando dici una cosa intelligente probabilmente stai citando qualcun altro.",
"Sei così coglione che se il cervello fosse un muscolo saresti paraplegico dalla fronte in su.",
"Sei così inutile che se fossi una supposta ti cagherebbero fuori per principio.",
"Sei così merda che se cadi nel cesso il water ti sputa fuori.",
"Sei talmente coglione che anche il tuo culo pensa più in profondità di te.",
"Sei così stronzo che quando vai a cagare fai sembrare il bagno più pulito prima di entrare.",
"Sei così rincoglionito che il tuo cervello gira su Windows Vista piratato.",
"Sei così coglione che il tuo albero genealogico dovrebbe essere una linea retta.",
"Sei così inutile che se fossi un cazzo non riusciresti neanche a stare duro.",
"Sei così idiota che potresti essere battuto a scacchi da un preservativo usato.",
"Sei così stronzo che la carta igienica si rifiuta di toccarti.",
"Sei talmente coglione che quando ti togli le mutande perdi metà del tuo quoziente intellettivo.",
"Sei così rincoglionito che se ti infilano una chiavetta USB nel culo provi a leggere i file.",
"Sei così merda che il letame ti considera un collega poco qualificato.",
"Sei così coglione che se ti chiedono sesso orale inizi a leggere ad alta voce.",
"Sei così stupido che quando senti parlare di sega circolare pensi sia un'orgia.",
"Sei così inutile che il tuo cazzo probabilmente viene usato solo per pisciare, e pure male.",
"Sei talmente coglione che il tuo cervello potrebbe stare comodamente dentro una supposta.",
"Sei così stronzo che quando caghi crei una copia di backup.",
"Sei così rincoglionito che se ti infilano un dito nel culo chiedi se prende il Wi-Fi.",
"Sei così brutto che Pornhub ti chiede la verifica per assicurarsi che tu non sia un contenuto violento.",
"Sei così coglione che persino una sega a due mani sarebbe troppo multitasking per te.",
"Sei così inutile che in un'orgia ti userebbero per tenere la porta.",
"Sei così stupido che se ti dicono 'testicolo' probabilmente rispondi 'presente'.",
"Sei talmente testa di cazzo che se ti metti un preservativo in testa finalmente sembri vestito bene.",
"Sei così rincoglionito che il tuo cervello ha bisogno del Viagra per avere un pensiero.",
"Sei così merda che quando passi vicino a un bagno pubblico migliori l'atmosfera.",
"Sei così coglione che se ti infilano una scopa nel culo finalmente hai una spina dorsale.",
"Sei così inutile che persino il tuo cazzo probabilmente ti usa solo come supporto logistico.",
"Sei talmente stronzo che se fossi carta igienica saresti quella trasparente da autogrill.",
"Sei così stupido che potresti farti inculare da una porta girevole.",
"Sei così rincoglionito che se ti dicono 'mettici la testa' probabilmente abbassi i pantaloni.",
"Sei così coglione che se avessi un cervello nel culo finalmente penseresti prima di cagare.",
"Sei così brutto che persino una glory hole chiederebbe di vedere prima la faccia.",
"Sei così inutile che se fossi un preservativo saresti taglia XS e pure bucato.",
"Sei così coglione che il tuo cervello ha meno attività del tuo cazzo dopo cinque seghe.",
"Sei talmente merda che quando entri in bagno la tavoletta del cesso si alza da sola per rispetto.",
"Sei così stupido che se ti dicono 'non fare il coglione' vai in crisi d'identità.",
"Sei così stronzo che potresti cagare in un letamaio e abbassarne il valore immobiliare.",
"Sei così rincoglionito che persino le tue scoregge escono con più idee di te.",
"Sei così coglione che se il buon senso fosse una malattia saresti immune.",
"Sei così inutile che il tuo culo probabilmente è la parte più produttiva del tuo corpo.",
"Sei talmente testa di cazzo che quando ti metti un cappello sembra un preservativo.",
"Sei così stupido che la tua sega più riuscita probabilmente è quella con cui ti hanno costruito.",
"Sei così coglione che se ti mettessero il cervello in vendita avrebbero paura dell'accusa di truffa.",
"Sei così stronzo che persino la merda ti considera tossico.",
"Sei così rincoglionito che il tuo unico pensiero profondo è quando ti siedi sul cesso.",
"Sei così inutile che anche un cazzo moscio ha più possibilità di concludere qualcosa.",
"Sei così coglione che se ti infilano una matita nel culo finalmente produci qualcosa di scritto.",
"Sei così brutto che quando apri Pornhub il sito passa automaticamente alla modalità audio.",
"Sei così stronzo che quando scoreggi il tuo culo ti chiede di abbassare i toni.",
"Sei così coglione che se ti mandano affanculo probabilmente sbagli strada.",
"Sei così rincoglionito che hai bisogno delle rotelle anche per andare a cagare.",
"Sei così inutile che un buco nel preservativo ha più prospettive future di te.",
"Sei così testa di cazzo che se ti facessero una TAC troverebbero due palle e nessun cervello."
    };

    public static void Exile(NetworkedPlayerInfo playerToExileInfo)
    {
        VoteContextManager.IsForcedVote = true;
        List<MeetingHud.VoterState> statesList = new();
        MeetingHud.Instance.RpcVotingComplete(statesList.ToArray(), playerToExileInfo, false, false, 0);
        MeetingHud.Instance.Close();
        MeetingHud.Instance.RpcClose();
        VoteContextManager.IsForcedVote = false;
        playerToExileInfo.IsDead = true;
        playerToExileInfo.MarkDirty();

    }

    public static void KillPlayer(PlayerControl target)
    {
        if (GameStates.isLobby) return;

        var killer = PlayerControl.LocalPlayer;

        PreviousMatchPopupTracker.MarkMirrorKillTarget(target);

        if (!BanMod.UnreportableBodies.Contains(target.PlayerId))
        {
            BanMod.UnreportableBodies.Add(target.PlayerId);
        }
        if (Utils.IsShapeshifted(killer))
        {
            killer.CmdCheckRevertShapeshift(false);
        }
        if (target != PlayerControl.LocalPlayer && Utils.IsShapeshifted(target))
        {
            target.CmdCheckRevertShapeshift(false);
        }
        killer.MyPhysics.StartCoroutine(CoMirrorKill(killer, target));

        target.Data.IsDead = true;
        target.Data.MarkDirty();
    }

    private static IEnumerator CoMirrorKill(PlayerControl killer, PlayerControl target)
    {
        Utils.SaveOriginalOutfit(PlayerControl.LocalPlayer);

        Vector3 originalPosition = killer.transform.position;

        CopyOutfit(target, killer);

        yield return new WaitForSeconds(0.15f);

        killer.MurderPlayer(target, MurderResultFlags.Succeeded);
        target.Data.IsDead = true;
        target.Data.MarkDirty();
        DeadBody[] allBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        DeadBody body = allBodies.FirstOrDefault(b => b.ParentId == target.PlayerId);

        if (body != null)
            UnityEngine.Object.Destroy(body.gameObject);
        yield return new WaitForSeconds(0.06f);

        if (killer.NetTransform != null)
        {
            killer.NetTransform.RpcSnapTo(originalPosition);
            killer.transform.position = originalPosition;
        }

        RestoreOriginalOutfit(killer);

    }
    public static MapNames GetCurrentMapFromOptions()
    {
        if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.CurrentGameOptions == null)
        {
            return (MapNames)(-1);
        }

        byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;

        switch (mapId)
        {
            case 0: return MapNames.Skeld;
            case 1: return MapNames.MiraHQ;
            case 2: return MapNames.Polus;
            case 3: return MapNames.Dleks;
            case 4: return MapNames.Airship;
            case 5: return MapNames.Fungle;
            default:
                return (MapNames)(-1);
        }
    }
    public static class NameNormalizer
    {
        public static readonly Dictionary<char, char> SpecialCharMap = new Dictionary<char, char>
    {
        { '卂', 'A' }, { '卄', 'H' }, { '卩', 'P' },
        { '乃', 'B' }, { '丨', 'I' }, { 'Ɋ', 'Q' },
        { '匚', 'C' }, { 'ㄥ', 'L' }, { '尺', 'R' },
        { 'ᗪ', 'D' }, { '爪', 'M' }, { '丂', 'S' },
        { '乇', 'E' }, { '几', 'N' }, { 'ᐯ', 'V' },
        { '千', 'F' }, { 'ㄒ', 'T' }, { '乙', 'Z' },
        { 'Ꮆ', 'G' }, { 'ㄖ', 'O' }, { 'ㄩ', 'U' },

        { '4', 'a' }, { '3', 'e' }, { '5', 's' },
        { '1', 'i' }, { '7', 't' }, { '0', 'o' },

        { 'Ö', 'O' }, { 'Ø', 'O' }, { 'ö', 'o' }, { 'ø', 'o' },
        { 'à', 'a' }, { 'á', 'a' }, { 'â', 'a' }, { 'ã', 'a' },
        { 'è', 'e' }, { 'é', 'e' }, { 'ê', 'e' }, { 'ë', 'e' },
        { 'ì', 'i' }, { 'í', 'i' }, { 'î', 'i' }, { 'ï', 'i' },
        { 'ò', 'o' }, { 'ó', 'o' }, { 'ô', 'o' }, { 'õ', 'o' },
        { 'ù', 'u' }, { 'ú', 'u' }, { 'û', 'u' }, { 'ü', 'u' },
        { 'ç', 'c' }, { 'ñ', 'n' }, { 'ﾑ', 'a' }, { 'ﾉ', 'i' },
        { 'À', 'A' }, { 'Á', 'A' }, { 'Â', 'A' }, { 'Ã', 'A' },
        { 'È', 'E' }, { 'É', 'E' }, { 'Ê', 'E' }, { 'Ë', 'E' },
        { 'Ì', 'I' }, { 'Í', 'I' }, { 'Î', 'I' }, { 'Ï', 'I' },
        { 'Ò', 'O' }, { 'Ó', 'O' }, { 'Ô', 'O' }, { 'Õ', 'O' },
        { 'Ù', 'U' }, { 'Ú', 'U' }, { 'Û', 'U' }, { 'Ü', 'U' },
        { 'Ç', 'C' }, { 'Ñ', 'N' }, { 'ä', 'a' }, { 'å', 'a' },

        { 'а', 'a' }, { 'б', 'b' }, { 'в', 'v' }, { 'г', 'g' }, { 'д', 'd' }, { 'е', 'e' }, { 'ё', 'e' },
        { 'з', 'z' }, { 'и', 'i' }, { 'й', 'y' }, { 'к', 'k' }, { 'л', 'l' }, { 'м', 'm' },
        { 'н', 'n' }, { 'о', 'o' }, { 'п', 'p' }, { 'р', 'r' }, { 'с', 's' }, { 'т', 't' }, { 'у', 'u' },
        { 'ф', 'f' }, { 'х', 'h' }, { 'Ä', 'A' }, { 'Å', 'A' }, { 'ы', 'y' }, { 'э', 'e' },
        { 'А', 'A' }, { 'Б', 'B' }, { 'В', 'V' }, { 'Г', 'G' }, { 'Д', 'D' }, { 'Е', 'E' }, { 'Ё', 'E' },
        { 'З', 'Z' }, { 'И', 'I' }, { 'Й', 'Y' }, { 'К', 'K' }, { 'Л', 'L' }, { 'М', 'M' },
        { 'Н', 'N' }, { 'О', 'O' }, { 'П', 'P' }, { 'Р', 'R' }, { 'С', 'S' }, { 'Т', 'T' }, { 'У', 'U' },
        { 'Ф', 'F' }, { 'Х', 'H' },
        { 'Ы', 'Y' }, { 'Э', 'E' },

        { 'Ð', 'D' }, { 'ð', 'd' }
    };

        public static string NormalizeInputName(string inputName)
        {
            if (string.IsNullOrEmpty(inputName))
            {
                return inputName;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(inputName.Length);

            foreach (char c in inputName)
            {
                if (SpecialCharMap.TryGetValue(c, out char normalizedChar))
                {
                    sb.Append(normalizedChar);
                }
                else if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }
            return sb.ToString();
        }
    }
    public static Sprite CreateSprite(Color color, int width = 64, int height = 64)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, rect, pivot);
    }

    private static Stack<NetworkedPlayerInfo.PlayerOutfit> savedOutfits = new Stack<NetworkedPlayerInfo.PlayerOutfit>();
    private static Stack<string> savedNames = new Stack<string>();
    private static DateTime _lastRulesSendTime = DateTime.MinValue;

    public static void SendRules()
    {
        bool isHost = AmongUsClient.Instance.AmHost;

        if (!isHost)
        {
            TimeSpan elapsed = DateTime.UtcNow - _lastRulesSendTime;

            if (elapsed < TimeSpan.FromMinutes(1))
            {
                return;
            }
        }

        string suffix = TemplateLoader.GetCurrentTemplateSuffix();

        string templateName =
            "RulesInfo" + suffix;

        string templateName1 =
            "RulesInfo" + suffix + "Modded";

        string title = TemplateLoader.FormatRulesTemplate(templateName);
        string title1 = TemplateLoader.FormatRulesModdedTemplate(templateName1);

        bool sent = false;

        if (BanModServerSelection.IsVanilla)
        {
            if (isHost && PlayerControl.LocalPlayer.Data.IsDead)
            {
                Utils.RequestProxyMessage(title);
                MessageBlocker.UpdateLastMessageTime();
            }
            else
            {
                Utils.SendMessage(title);
                MessageBlocker.UpdateLastMessageTime();
            }

            sent = true;
        }
        else if (BanModServerSelection.IsModded25)
        {
            Utils.SendMessage(title1);
            sent = true;
        }

        if (sent)
        {
            _lastRulesSendTime = DateTime.UtcNow;
        }
    }
    public static string GradientText(string text, Color start, Color end)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (text.Length == 1)
            return ColorString(start, text);

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            float t = (float)i / (text.Length - 1);
            Color c = Color.Lerp(start, end, t);
            sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{text[i]}</color>");
        }

        return sb.ToString();
    }
    private static readonly System.Random RainbowNameRandom = new System.Random();

    public static string MakeRainbowName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        Color[][] palettes =
        {
        new Color[]
        {
            new Color(1f, 0f, 0f),
            new Color(1f, 0.45f, 0f),
            new Color(1f, 1f, 0f),
            new Color(0f, 1f, 0.35f),
            new Color(0f, 0.75f, 1f),
            new Color(0.65f, 0f, 1f)
        },
        new Color[]
        {
            new Color(1f, 0f, 0.45f),
            new Color(1f, 0.25f, 0.85f),
            new Color(0.65f, 0f, 1f),
            new Color(0f, 0.55f, 1f),
            new Color(0f, 1f, 0.85f)
        },
        new Color[]
        {
            new Color(1f, 0.9f, 0f),
            new Color(1f, 0.45f, 0f),
            new Color(1f, 0.1f, 0.1f),
            new Color(1f, 0f, 0.65f),
            new Color(0.75f, 0f, 1f)
        },
        new Color[]
        {
            new Color(0f, 1f, 0.55f),
            new Color(0f, 0.85f, 1f),
            new Color(0.05f, 0.25f, 1f),
            new Color(0.55f, 0f, 1f),
            new Color(1f, 0f, 0.85f)
        },
        new Color[]
        {
            new Color(1f, 0.25f, 0.25f),
            new Color(1f, 0.75f, 0.15f),
            new Color(0.65f, 1f, 0.15f),
            new Color(0.15f, 1f, 0.75f),
            new Color(0.15f, 0.75f, 1f)
        }
    };

        Color[] palette = palettes[RainbowNameRandom.Next(palettes.Length)];

        int offset = RainbowNameRandom.Next(palette.Length);
        bool reverse = RainbowNameRandom.Next(0, 2) == 1;

        string result = "<b>";

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (char.IsWhiteSpace(ch))
            {
                result += ch;
                continue;
            }

            int index = reverse
                ? (offset - i + palette.Length * 10) % palette.Length
                : (offset + i) % palette.Length;

            string size = i == 0 ? "120%" : "100%";

            result += "<size=" + size + ">";
            result += ColorString(palette[index], ch.ToString());
            result += "</size>";
        }

        result += "</b>";

        return result;
    }
    public static PlayerBodyTypes GetPlayerBodyType(PlayerControl target)
    {
        if (target?.Data == null || target.MyPhysics == null)
            return PlayerBodyTypes.Normal;

        return target.MyPhysics.bodyType;
    }

    public static void SetPlayerBodyType(PlayerControl target, PlayerBodyTypes bodyType)
    {
        if (target?.Data == null || target.MyPhysics == null)
            return;

        target.MyPhysics.SetBodyType(bodyType);
    }

    public static PlayerBodyTypes GetNextBodyType(PlayerControl target)
    {
        if (target?.Data == null || target.MyPhysics == null)
            return PlayerBodyTypes.Normal;

        return target.MyPhysics.bodyType switch
        {
            PlayerBodyTypes.Normal => PlayerBodyTypes.Horse,
            PlayerBodyTypes.Horse => PlayerBodyTypes.Seeker,
            PlayerBodyTypes.Seeker => PlayerBodyTypes.Normal,
            _ => PlayerBodyTypes.Normal
        };
    }

    public static void RefreshPlayerBodyType(PlayerControl target)
    {
        SetPlayerBodyType(target, GetNextBodyType(target));
    }
    public static void CloseMeeting()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            return;

        var meeting = MeetingHud.Instance;
        if (meeting == null)
            return;

        // The approved callback in UtilsPoll.cs performs the actual close.
        SondaggioChiusuraMeeting(meeting);
    }

    public static IEnumerator DelayedCloseMeeting()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            yield break;

        var meeting = MeetingHud.Instance;
        if (meeting == null)
            yield break;

        while (meeting != null && meeting == MeetingHud.Instance &&
               !meeting.gameObject.activeInHierarchy)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                yield break;
            yield return null;
        }

        yield return null;
        yield return new WaitForSeconds(0.1f);

        if (meeting == null || meeting != MeetingHud.Instance ||
            AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            yield break;

        CloseMeeting();
    }

    public static bool isClientModded = false;
    public static void SendModdedHandshake()
    {
        if (PlayerControl.LocalPlayer == null)
            return;

        BanModSyncApi.SendModdedHandshake();
    }
    public static void Explosion(
    this PlayerControl target,
    float seconds,
    float interval = 0.12f)
    {
        if (target == null || target.Data == null)
            return;

        if (seconds <= 0f)
            return;

        if (!AmongUsClient.Instance.AmHost)
            return;

        if (target.Data.RoleType != RoleTypes.Phantom)
        {
            target.RpcSetRole(RoleTypes.Phantom, true);
        }

        // IMPORTANTE:
        // la coroutine la esegue l'host
        PlayerControl.LocalPlayer.StartCoroutine(
            ExplosionCoroutine(target, seconds, interval)
                .WrapToIl2Cpp()
        );
    }

    private static IEnumerator ExplosionCoroutine(
        PlayerControl target,
        float seconds,
        float interval)
    {
        float endTime = Time.time + seconds;

        byte originalColor = (byte)target.Data.DefaultOutfit.ColorId;

        int colorCount = Palette.PlayerColors.Length;
        int colorIndex = originalColor;

        bool vanish = false;

        while (Time.time < endTime)
        {
            if (target == null || target.Data == null)
                yield break;

            colorIndex++;

            if (colorIndex >= colorCount)
                colorIndex = 0;

            target.RpcSetColor((byte)colorIndex);

            vanish = !vanish;

            if (vanish)
            {
                ForceVanish(target);
            }
            else
            {
                ForceAppear(target);
            }

            yield return new WaitForSeconds(interval);
        }

        if (target != null && target.Data != null)
        {
            target.RpcSetColor(originalColor);
            ForceAppear(target);
        }
    }

    private static void ForceVanish(PlayerControl target)
    {
        if (target == null)
            return;

        // viene chiamato sul PlayerControl del TARGET
        target.CmdCheckVanish(1);
    }

    private static void ForceAppear(PlayerControl target)
    {
        if (target == null)
            return;

        // viene chiamato sul PlayerControl del TARGET
        target.CmdCheckAppear(true);
    }
    public static class HostOptionStatus
    {
        public static bool ImmortalAdded { get; set; } = false;
        public static bool ImmortalEnabled { get; set; } = false;
        public static bool EngineerEnabled { get; set; } = false;

        public static void UpdateHostRules(bool added, bool enabled, bool engineer)
        {
            ImmortalAdded = added;
            ImmortalEnabled = enabled;
            EngineerEnabled = engineer;
        }
    }
    public static void SendHostTripleBoolRpc()
    {
        if (PlayerControl.LocalPlayer == null ||
            AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost ||
            !GameStates.isOnlineGame)
        {
            return;
        }

        bool bool1_ImmortalAdded = ImmortalManager.immortalAssigned;
        bool bool2_ImmortalEnabled = Options.EnableImmortal.GetBool();
        bool bool3_EngineerEnabled = Options.EngineerFixer.GetBool();

        BanModSyncApi.SendHostTripleBoolUpdate(
            bool1_ImmortalAdded,
            bool2_ImmortalEnabled,
            bool3_EngineerEnabled);

        HostOptionStatus.UpdateHostRules(
            bool1_ImmortalAdded,
            bool2_ImmortalEnabled,
            bool3_EngineerEnabled);
    }
    public static class TemplateLoader
    {
        private static readonly string TemplatesWelcomeFolder = "./DATA/TEMPLATE/Welcome";
        private static readonly string TemplatesRulesFolder = "./DATA/TEMPLATE/Rules";
        private static readonly string TemplatesWelcomeModdedFolder = "./DATA/TEMPLATE/WelcomeModded";
        private static readonly string TemplatesRulesModdedFolder = "./DATA/TEMPLATE/RulesModded";
        // Replace only TemplateLoader.InitTemplates() in Modules/Utils.cs.
        // Keep the four existing Create...Template helper methods.
        public static void InitTemplates()
        {
            Directory.CreateDirectory(TemplatesWelcomeFolder);
            Directory.CreateDirectory(TemplatesRulesFolder);
            Directory.CreateDirectory(TemplatesWelcomeModdedFolder);
            Directory.CreateDirectory(TemplatesRulesModdedFolder);

            CreateWelcomeTemplate("WelcomeTemplate",
                "Welcome {player}! This lobby uses BanMod. Respect the rules and other players. No insults, teaming or hacks. Breaking the rules may result in a temporary or permanent lobby ban.");

            CreateRulesTemplate("RulesInfo",
                "This lobby uses BanMod. Respect the rules and other players. No insults, teaming or hacks. Breaking the rules may result in a temporary or permanent lobby ban.");

            CreateWelcomeModdedTemplate("WelcomeTemplateModded",
                "<size=120%><b><color=#3F7CC4>BANMOD</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>This lobby uses BanMod. Respect the rules and other players. No insults, teaming or hacks. Breaking the rules may result in a temporary or permanent lobby ban.</b></color>\n\nPlay fairly and respect everyone. Do not cooperate unfairly with opponents or use external cheats. The host may enforce the rules with temporary or permanent bans.");

            CreateRulesModdedTemplate("RulesInfoModded",
                "<size=120%><b><color=#3F7CC4>BANMOD</color></b></size>\n\n<color=#3E9B63><b>This lobby uses BanMod. Respect the rules and other players. No insults, teaming or hacks. Breaking the rules may result in a temporary or permanent lobby ban.</b></color>\n\nPlay fairly and respect everyone. Do not cooperate unfairly with opponents or use external cheats. The host may enforce the rules with temporary or permanent bans.");

            CreateWelcomeTemplate("WelcomeTemplateSns",
                "Welcome {player}! SNS: Copy a player, then kill them. Wrong kills: penalty/death. Crew: finish tasks. Imps: no vents. No meetings/reports. Comms only. Host may ban kills during Comms/downloads.");

            CreateRulesTemplate("RulesInfoSns",
                "SNS: Copy a player, then kill them. Wrong kills: penalty/death. Crew: finish tasks. Imps: no vents. No meetings/reports. Comms only. Host may ban kills during Comms/downloads.");

            CreateWelcomeModdedTemplate("WelcomeTemplateSnsModded",
                "<size=120%><b><color=#3F7CC4>SNS</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>SNS: Copy a player, then kill them. Wrong kills: penalty/death. Crew: finish tasks. Imps: no vents. No meetings/reports. Comms only. Host may ban kills during Comms/downloads.</b></color>\n\n<b>IMPOSTORS:</b> Shapeshift into a player, then kill that same player. Wrong kills cause penalties; reaching the configured mistake limit causes death. Impostors cannot vent.\n\n<b>CREWMATES:</b> Complete all your tasks.\n\n<b>RESTRICTIONS:</b> Meetings and body reports are disabled. Comms is the only available sabotage; all others are disabled. The host may forbid kills during Comms sabotage or while a crewmate is downloading. Check these optional bans before starting.");

            CreateRulesModdedTemplate("RulesInfoSnsModded",
                "<size=120%><b><color=#3F7CC4>SNS</color></b></size>\n\n<color=#3E9B63><b>SNS: Copy a player, then kill them. Wrong kills: penalty/death. Crew: finish tasks. Imps: no vents. No meetings/reports. Comms only. Host may ban kills during Comms/downloads.</b></color>\n\n<b>IMPOSTORS:</b> Shapeshift into a player, then kill that same player. Wrong kills cause penalties; reaching the configured mistake limit causes death. Impostors cannot vent.\n\n<b>CREWMATES:</b> Complete all your tasks.\n\n<b>RESTRICTIONS:</b> Meetings and body reports are disabled. Comms is the only available sabotage; all others are disabled. The host may forbid kills during Comms sabotage or while a crewmate is downloading. Check these optional bans before starting.");

            CreateWelcomeTemplate("WelcomeTemplatePns",
                "Welcome {player}! PNS: Move invisible; kill visible. Moving visible: penalty. Killing during punishment: death. Crew: do tasks. Imps: no vents. No meetings/reports. Comms only. Ask host about Comms/download kills.");

            CreateRulesTemplate("RulesInfoPns",
                "PNS: Move invisible; kill visible. Moving visible: penalty. Killing during punishment: death. Crew: do tasks. Imps: no vents. No meetings/reports. Comms only. Ask host about Comms/download kills.");

            CreateWelcomeModdedTemplate("WelcomeTemplatePnsModded",
                "<size=120%><b><color=#2F8FA3>PNS</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>PNS: Move invisible; kill visible. Moving visible: penalty. Killing during punishment: death. Crew: do tasks. Imps: no vents. No meetings/reports. Comms only. Ask host about Comms/download kills.</b></color>\n\n<b>IMPOSTORS:</b> Use Phantom invisibility to move and become visible to kill. Moving while visible causes penalties. Killing during punishment eliminates the impostor. Impostors cannot vent.\n\n<b>CREWMATES:</b> Complete all your tasks.\n\n<b>RESTRICTIONS:</b> Meetings and body reports are disabled. Comms is the only available sabotage; all others are disabled. The host may forbid kills during Comms sabotage or while a crewmate is downloading. Confirm these optional bans before starting.");

            CreateRulesModdedTemplate("RulesInfoPnsModded",
                "<size=120%><b><color=#2F8FA3>PNS</color></b></size>\n\n<color=#3E9B63><b>PNS: Move invisible; kill visible. Moving visible: penalty. Killing during punishment: death. Crew: do tasks. Imps: no vents. No meetings/reports. Comms only. Ask host about Comms/download kills.</b></color>\n\n<b>IMPOSTORS:</b> Use Phantom invisibility to move and become visible to kill. Moving while visible causes penalties. Killing during punishment eliminates the impostor. Impostors cannot vent.\n\n<b>CREWMATES:</b> Complete all your tasks.\n\n<b>RESTRICTIONS:</b> Meetings and body reports are disabled. Comms is the only available sabotage; all others are disabled. The host may forbid kills during Comms sabotage or while a crewmate is downloading. Confirm these optional bans before starting.");

            CreateWelcomeTemplate("WelcomeTemplateKaitoRun",
                "Welcome {player}! KaitoRun Fungle: Never fix Reactor. Only crew report. No emergency meetings. Wait 10s to kill in Fire/Meeting circles; elsewhere kill anytime. Reactor needs a body. Asking to start: auto-kick.");

            CreateRulesTemplate("RulesInfoKaitoRun",
                "KaitoRun Fungle: Never fix Reactor. Only crew report. No emergency meetings. Wait 10s to kill in Fire/Meeting circles; elsewhere kill anytime. Reactor needs a body. Asking to start: auto-kick.");

            CreateWelcomeModdedTemplate("WelcomeTemplateKaitoRunModded",
                "<size=120%><b><color=#C56A32>KAITORUN</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>KaitoRun Fungle: Never fix Reactor. Only crew report. No emergency meetings. Wait 10s to kill in Fire/Meeting circles; elsewhere kill anytime. Reactor needs a body. Asking to start: auto-kick.</b></color>\n\n<b>REACTOR:</b> Never repair it. Crew must report a body or finish tasks. Reactor is a failsafe, not a free win: impostors must provide a body before or during the sabotage. No body within 60s may lead to kick/ban. Angels must not protect continuously and prevent a reportable death.\n\n<b>SPAWN:</b> In Fire/Meeting circles, wait 10s before killing. Outside them, kill anytime. Only crew may report; no emergency meetings. Asking to start: auto-kick.\n\n<b>ALLOWED:</b> Grouping, camping, following, hiding, Vitals and Telescope. Chasing during MMU is optional; hide in vents or report to escape. Crew should ignore Comms and focus on tasks; check task locations during the first 10s without sabotages.\n\n<b>PRESET:</b> Kill/protect CD 0s; vent CD 5s; visible votes; no Shapeshifter; 2 tasks.");

            CreateRulesModdedTemplate("RulesInfoKaitoRunModded",
                "<size=120%><b><color=#C56A32>KAITORUN</color></b></size>\n\n<color=#3E9B63><b>KaitoRun Fungle: Never fix Reactor. Only crew report. No emergency meetings. Wait 10s to kill in Fire/Meeting circles; elsewhere kill anytime. Reactor needs a body. Asking to start: auto-kick.</b></color>\n\n<b>REACTOR:</b> Never repair it. Crew must report a body or finish tasks. Reactor is a failsafe, not a free win: impostors must provide a body before or during the sabotage. No body within 60s may lead to kick/ban. Angels must not protect continuously and prevent a reportable death.\n\n<b>SPAWN:</b> In Fire/Meeting circles, wait 10s before killing. Outside them, kill anytime. Only crew may report; no emergency meetings. Asking to start: auto-kick.\n\n<b>ALLOWED:</b> Grouping, camping, following, hiding, Vitals and Telescope. Chasing during MMU is optional; hide in vents or report to escape. Crew should ignore Comms and focus on tasks; check task locations during the first 10s without sabotages.\n\n<b>PRESET:</b> Kill/protect CD 0s; vent CD 5s; visible votes; no Shapeshifter; 2 tasks.");

            CreateWelcomeTemplate("WelcomeTemplateTaskRun",
                "Welcome {player}! Task Run: No impostors, sabotages or meetings. Run and complete all your tasks as fast as possible. The first player to finish wins.");

            CreateRulesTemplate("RulesInfoTaskRun",
                "Task Run: No impostors, sabotages or meetings. Run and complete all your tasks as fast as possible. The first player to finish wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateTaskRunModded",
                "<size=120%><b><color=#3E9B63>TASK RUN</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Task Run: No impostors, sabotages or meetings. Run and complete all your tasks as fast as possible. The first player to finish wins.</b></color>\n\nEveryone races on their own task list. Plan your route and keep moving: only completing all your tasks wins the race.");

            CreateRulesModdedTemplate("RulesInfoTaskRunModded",
                "<size=120%><b><color=#3E9B63>TASK RUN</color></b></size>\n\n<color=#3E9B63><b>Task Run: No impostors, sabotages or meetings. Run and complete all your tasks as fast as possible. The first player to finish wins.</b></color>\n\nEveryone races on their own task list. Plan your route and keep moving: only completing all your tasks wins the race.");

            CreateWelcomeTemplate("WelcomeTemplateJBMode",
                "Welcome {player}! JBMode: Play with the host-defined objectives and restrictions. Confirm allowed abilities, actions and victory conditions before starting.");

            CreateRulesTemplate("RulesInfoJBMode",
                "JBMode: Play with the host-defined objectives and restrictions. Confirm allowed abilities, actions and victory conditions before starting.");

            CreateWelcomeModdedTemplate("WelcomeTemplateJBModeModded",
                "<size=120%><b><color=#8A5CC2>JBMODE</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>JBMode: Play with the host-defined objectives and restrictions. Confirm allowed abilities, actions and victory conditions before starting.</b></color>\n\nThe host defines the round objectives and restrictions. Confirm the rules before starting; do not assume every standard Classic rule applies.");

            CreateRulesModdedTemplate("RulesInfoJBModeModded",
                "<size=120%><b><color=#8A5CC2>JBMODE</color></b></size>\n\n<color=#3E9B63><b>JBMode: Play with the host-defined objectives and restrictions. Confirm allowed abilities, actions and victory conditions before starting.</b></color>\n\nThe host defines the round objectives and restrictions. Confirm the rules before starting; do not assume every standard Classic rule applies.");

            CreateWelcomeTemplate("WelcomeTemplateFFA",
                "Welcome {player}! FFA: Everyone is an opponent. Eliminate other players and avoid their attacks. The last player alive wins.");

            CreateRulesTemplate("RulesInfoFFA",
                "FFA: Everyone is an opponent. Eliminate other players and avoid their attacks. The last player alive wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateFFAModded",
                "<size=120%><b><color=#C94A4A>FREE FOR ALL</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>FFA: Everyone is an opponent. Eliminate other players and avoid their attacks. The last player alive wins.</b></color>\n\nUse the abilities enabled in this lobby and watch your kill cooldown. There are no teammates in this mode.");

            CreateRulesModdedTemplate("RulesInfoFFAModded",
                "<size=120%><b><color=#C94A4A>FREE FOR ALL</color></b></size>\n\n<color=#3E9B63><b>FFA: Everyone is an opponent. Eliminate other players and avoid their attacks. The last player alive wins.</b></color>\n\nUse the abilities enabled in this lobby and watch your kill cooldown. There are no teammates in this mode.");

            CreateWelcomeTemplate("WelcomeTemplateFFATEAM",
                "Welcome {player}! Team FFA: Work with your assigned teammates to eliminate enemy teams. Coordinate attacks and survive. The last team alive wins.");

            CreateRulesTemplate("RulesInfoFFATEAM",
                "Team FFA: Work with your assigned teammates to eliminate enemy teams. Coordinate attacks and survive. The last team alive wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateFFATEAMModded",
                "<size=120%><b><color=#3F7CC4>TEAM FFA</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Team FFA: Work with your assigned teammates to eliminate enemy teams. Coordinate attacks and survive. The last team alive wins.</b></color>\n\nCheck your team assignment before starting. A team can remain in the match while at least one of its members survives.");

            CreateRulesModdedTemplate("RulesInfoFFATEAMModded",
                "<size=120%><b><color=#3F7CC4>TEAM FFA</color></b></size>\n\n<color=#3E9B63><b>Team FFA: Work with your assigned teammates to eliminate enemy teams. Coordinate attacks and survive. The last team alive wins.</b></color>\n\nCheck your team assignment before starting. A team can remain in the match while at least one of its members survives.");

            CreateWelcomeTemplate("WelcomeTemplateZombieMode",
                "Welcome {player}! Zombie Mode: Infected players spread the infection. Crewmates avoid infection and complete tasks. Use the abilities enabled in this lobby.");

            CreateRulesTemplate("RulesInfoZombieMode",
                "Zombie Mode: Infected players spread the infection. Crewmates avoid infection and complete tasks. Use the abilities enabled in this lobby.");

            CreateWelcomeModdedTemplate("WelcomeTemplateZombieModeModded",
                "<size=120%><b><color=#3E9B63>ZOMBIE MODE</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Zombie Mode: Infected players spread the infection. Crewmates avoid infection and complete tasks. Use the abilities enabled in this lobby.</b></color>\n\nInfected players hunt the remaining crew. Crewmates should keep their distance and make progress on tasks.");

            CreateRulesModdedTemplate("RulesInfoZombieModeModded",
                "<size=120%><b><color=#3E9B63>ZOMBIE MODE</color></b></size>\n\n<color=#3E9B63><b>Zombie Mode: Infected players spread the infection. Crewmates avoid infection and complete tasks. Use the abilities enabled in this lobby.</b></color>\n\nInfected players hunt the remaining crew. Crewmates should keep their distance and make progress on tasks.");

            CreateWelcomeTemplate("WelcomeTemplateHotPotato",
                "Welcome {player}! Hot Potato: Use the kill button to pass the potato. The holder is eliminated when time expires. Survive the rounds; the last player alive wins.");

            CreateRulesTemplate("RulesInfoHotPotato",
                "Hot Potato: Use the kill button to pass the potato. The holder is eliminated when time expires. Survive the rounds; the last player alive wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateHotPotatoModded",
                "<size=120%><b><color=#C56A32>HOT POTATO</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Hot Potato: Use the kill button to pass the potato. The holder is eliminated when time expires. Survive the rounds; the last player alive wins.</b></color>\n\nPassing the potato transfers it instead of killing the recipient. Get close enough to another player to pass it before it explodes.");

            CreateRulesModdedTemplate("RulesInfoHotPotatoModded",
                "<size=120%><b><color=#C56A32>HOT POTATO</color></b></size>\n\n<color=#3E9B63><b>Hot Potato: Use the kill button to pass the potato. The holder is eliminated when time expires. Survive the rounds; the last player alive wins.</b></color>\n\nPassing the potato transfers it instead of killing the recipient. Get close enough to another player to pass it before it explodes.");

            CreateWelcomeTemplate("WelcomeTemplateRoomRush",
                "Welcome {player}! Room Rush: Reach the indicated room. The last to arrive is eliminated. At timeout, anyone who has not arrived is eliminated. The last survivor wins.");

            CreateRulesTemplate("RulesInfoRoomRush",
                "Room Rush: Reach the indicated room. The last to arrive is eliminated. At timeout, anyone who has not arrived is eliminated. The last survivor wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateRoomRushModded",
                "<size=120%><b><color=#A88420>ROOM RUSH</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Room Rush: Reach the indicated room. The last to arrive is eliminated. At timeout, anyone who has not arrived is eliminated. The last survivor wins.</b></color>\n\nA new room is selected each round. Being inside a vent does not count as reaching the room. If nobody survives, there is no individual winner.");

            CreateRulesModdedTemplate("RulesInfoRoomRushModded",
                "<size=120%><b><color=#A88420>ROOM RUSH</color></b></size>\n\n<color=#3E9B63><b>Room Rush: Reach the indicated room. The last to arrive is eliminated. At timeout, anyone who has not arrived is eliminated. The last survivor wins.</b></color>\n\nA new room is selected each round. Being inside a vent does not count as reaching the room. If nobody survives, there is no individual winner.");

            CreateWelcomeTemplate("WelcomeTemplateTargetRush",
                "Welcome {player}! Target Rush: At timeout, be in your assigned room with exactly the required number of players. Too many or too few means elimination. The last survivor wins.");

            CreateRulesTemplate("RulesInfoTargetRush",
                "Target Rush: At timeout, be in your assigned room with exactly the required number of players. Too many or too few means elimination. The last survivor wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateTargetRushModded",
                "<size=120%><b><color=#2F8FA3>TARGET RUSH</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Target Rush: At timeout, be in your assigned room with exactly the required number of players. Too many or too few means elimination. The last survivor wins.</b></color>\n\nCheck your personal room and its required player count every round. You must be outside vents when the timer expires. Arriving earlier does not guarantee safety.");

            CreateRulesModdedTemplate("RulesInfoTargetRushModded",
                "<size=120%><b><color=#2F8FA3>TARGET RUSH</color></b></size>\n\n<color=#3E9B63><b>Target Rush: At timeout, be in your assigned room with exactly the required number of players. Too many or too few means elimination. The last survivor wins.</b></color>\n\nCheck your personal room and its required player count every round. You must be outside vents when the timer expires. Arriving earlier does not guarantee safety.");

            CreateWelcomeTemplate("WelcomeTemplateDeathRun",
                "Welcome {player}! Death Run: Visit the rooms in order and stay until each step completes. Leaving interrupts the hold. The last to finish is eliminated. The last survivor wins.");

            CreateRulesTemplate("RulesInfoDeathRun",
                "Death Run: Visit the rooms in order and stay until each step completes. Leaving interrupts the hold. The last to finish is eliminated. The last survivor wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateDeathRunModded",
                "<size=120%><b><color=#C56A32>DEATH RUN</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Death Run: Visit the rooms in order and stay until each step completes. Leaving interrupts the hold. The last to finish is eliminated. The last survivor wins.</b></color>\n\nFollow the displayed route. Stay in each target room for the required hold time, then move to the next step. Entering a vent also interrupts the hold.");

            CreateRulesModdedTemplate("RulesInfoDeathRunModded",
                "<size=120%><b><color=#C56A32>DEATH RUN</color></b></size>\n\n<color=#3E9B63><b>Death Run: Visit the rooms in order and stay until each step completes. Leaving interrupts the hold. The last to finish is eliminated. The last survivor wins.</b></color>\n\nFollow the displayed route. Stay in each target room for the required hold time, then move to the next step. Entering a vent also interrupts the hold.");

            CreateWelcomeTemplate("WelcomeTemplateKillRace",
                "Welcome {player}! Kill Race: Score kills before time runs out. Defeated players return after the respawn delay. The highest kill score wins; tied leaders share victory.");

            CreateRulesTemplate("RulesInfoKillRace",
                "Kill Race: Score kills before time runs out. Defeated players return after the respawn delay. The highest kill score wins; tied leaders share victory.");

            CreateWelcomeModdedTemplate("WelcomeTemplateKillRaceModded",
                "<size=120%><b><color=#C94A4A>KILL RACE</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Kill Race: Score kills before time runs out. Defeated players return after the respawn delay. The highest kill score wins; tied leaders share victory.</b></color>\n\nA defeat temporarily disables you instead of permanently eliminating you. Match duration, respawn delay and killer role are configured by the host.");

            CreateRulesModdedTemplate("RulesInfoKillRaceModded",
                "<size=120%><b><color=#C94A4A>KILL RACE</color></b></size>\n\n<color=#3E9B63><b>Kill Race: Score kills before time runs out. Defeated players return after the respawn delay. The highest kill score wins; tied leaders share victory.</b></color>\n\nA defeat temporarily disables you instead of permanently eliminating you. Match duration, respawn delay and killer role are configured by the host.");

            CreateWelcomeTemplate("WelcomeTemplateAssassin",
                "Welcome {player}! Assassin: Kill only your assigned target. A wrong attack eliminates you and disables the victim for 5 seconds. Targets change after eliminations. The last survivor wins.");

            CreateRulesTemplate("RulesInfoAssassin",
                "Assassin: Kill only your assigned target. A wrong attack eliminates you and disables the victim for 5 seconds. Targets change after eliminations. The last survivor wins.");

            CreateWelcomeModdedTemplate("WelcomeTemplateAssassinModded",
                "<size=120%><b><color=#8A5CC2>ASSASSIN</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Assassin: Kill only your assigned target. A wrong attack eliminates you and disables the victim for 5 seconds. Targets change after eliminations. The last survivor wins.</b></color>\n\nCheck the displayed target again after an elimination or a player disconnects. You are also being hunted. A wrong victim is only temporarily disabled by that attack.");

            CreateRulesModdedTemplate("RulesInfoAssassinModded",
                "<size=120%><b><color=#8A5CC2>ASSASSIN</color></b></size>\n\n<color=#3E9B63><b>Assassin: Kill only your assigned target. A wrong attack eliminates you and disables the victim for 5 seconds. Targets change after eliminations. The last survivor wins.</b></color>\n\nCheck the displayed target again after an elimination or a player disconnects. You are also being hunted. A wrong victim is only temporarily disabled by that attack.");

            CreateWelcomeTemplate("WelcomeTemplateHideAndSeek",
                "Welcome {player}! Hide and Seek: Seekers hunt the crew. Crewmates hide and complete tasks to reduce the timer. Seekers win by eliminating the crew; crewmates win when time runs out.");

            CreateRulesTemplate("RulesInfoHideAndSeek",
                "Hide and Seek: Seekers hunt the crew. Crewmates hide and complete tasks to reduce the timer. Seekers win by eliminating the crew; crewmates win when time runs out.");

            CreateWelcomeModdedTemplate("WelcomeTemplateHideAndSeekModded",
                "<size=120%><b><color=#C56A32>HIDE AND SEEK</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>Hide and Seek: Seekers hunt the crew. Crewmates hide and complete tasks to reduce the timer. Seekers win by eliminating the crew; crewmates win when time runs out.</b></color>\n\nUse the escape abilities available in the lobby. These are the standard rules; SNS and FFA variants have additional objectives.");

            CreateRulesModdedTemplate("RulesInfoHideAndSeekModded",
                "<size=120%><b><color=#C56A32>HIDE AND SEEK</color></b></size>\n\n<color=#3E9B63><b>Hide and Seek: Seekers hunt the crew. Crewmates hide and complete tasks to reduce the timer. Seekers win by eliminating the crew; crewmates win when time runs out.</b></color>\n\nUse the escape abilities available in the lobby. These are the standard rules; SNS and FFA variants have additional objectives.");

            CreateWelcomeTemplate("WelcomeTemplateSnsHideAndSeek",
                "Welcome {player}! SNS Hide and Seek: Seekers copy a player and kill that player. Wrong attacks cause penalties or death. Crew survive until timeout; seekers eliminate the crew.");

            CreateRulesTemplate("RulesInfoSnsHideAndSeek",
                "SNS Hide and Seek: Seekers copy a player and kill that player. Wrong attacks cause penalties or death. Crew survive until timeout; seekers eliminate the crew.");

            CreateWelcomeModdedTemplate("WelcomeTemplateSnsHideAndSeekModded",
                "<size=120%><b><color=#3F7CC4>SNS - HIDE AND SEEK</color></b></size>\n\nWelcome <b><color=#8A5CC2>{player}</color></b>!\n\n<color=#3E9B63><b>SNS Hide and Seek: Seekers copy a player and kill that player. Wrong attacks cause penalties or death. Crew survive until timeout; seekers eliminate the crew.</b></color>\n\nShapeshifts have a limited duration. Mistake limits and punishment duration depend on the host settings. Crewmates can complete tasks to reduce the timer.");

            CreateRulesModdedTemplate("RulesInfoSnsHideAndSeekModded",
                "<size=120%><b><color=#3F7CC4>SNS - HIDE AND SEEK</color></b></size>\n\n<color=#3E9B63><b>SNS Hide and Seek: Seekers copy a player and kill that player. Wrong attacks cause penalties or death. Crew survive until timeout; seekers eliminate the crew.</b></color>\n\nShapeshifts have a limited duration. Mistake limits and punishment duration depend on the host settings. Crewmates can complete tasks to reduce the timer.");
        }

        public static string GetCurrentTemplateSuffix()
        {
            var gameOptions =
                GameOptionsManager.Instance?.CurrentGameOptions;

            if (gameOptions != null &&
                gameOptions.GameMode == GameModes.HideNSeek)
            {
                if (Options.EnableFFA.GetBool())
                    return "FFA";

                if (Options.EnableSNS.GetBool())
                    return "SnsHideAndSeek";

                return "HideAndSeek";
            }

            return Options.GameMode.Selected switch
            {
                GameModeType.Default => "",
                GameModeType.KaitoRun => "KaitoRun",
                GameModeType.TaskRun => "TaskRun",
                GameModeType.SnS => "Sns",
                GameModeType.PnS => "Pns",
                GameModeType.ZombieMode => "ZombieMode",
                GameModeType.RoomRush => "RoomRush",
                GameModeType.TargetRush => "TargetRush",
                GameModeType.DeathRun => "DeathRun",
                GameModeType.FFA => "FFA",
                GameModeType.FFATeam => "FFATEAM",
                GameModeType.HotPotatoModded => "HotPotato",
                GameModeType.Assassin => "Assassin",
                GameModeType.KillRace => "KillRace",
                _ => ""
            };
        }
        private static void CreateWelcomeTemplate(string name, string content)
        {
            string path = Path.Combine(TemplatesWelcomeFolder, name + ".txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, content);
            }
        }
        private static void CreateWelcomeModdedTemplate(string name, string content)
        {
            string path = Path.Combine(TemplatesWelcomeModdedFolder, name + ".txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, content);
            }
        }
        private static void CreateRulesTemplate(string name, string content)
        {
            string path = Path.Combine(TemplatesRulesFolder, name + ".txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, content);
            }
        }
        private static void CreateRulesModdedTemplate(string name, string content)
        {
            string path = Path.Combine(TemplatesRulesModdedFolder, name + ".txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, content);
            }
        }

        public static string LoadWelcomeTemplate(string templateName)
        {
            string filePath = Path.Combine(TemplatesWelcomeFolder, templateName + ".txt");

            if (!File.Exists(filePath))
            {
                InitTemplates();

                if (!File.Exists(filePath))
                {
                    return $"Missing template: {templateName}";
                }
            }

            return File.ReadAllText(filePath).Replace("\\n", "\n");
        }
        public static string LoadWelcomeModdedTemplate(string templateName)
        {
            string filePath = Path.Combine(TemplatesWelcomeModdedFolder, templateName + ".txt");

            if (!File.Exists(filePath))
            {
                InitTemplates();

                if (!File.Exists(filePath))
                {
                    return $"Missing template: {templateName}";
                }
            }

            return File.ReadAllText(filePath).Replace("\\n", "\n");
        }
        public static string LoadRulesTemplate(string templateName)
        {
            string filePath = Path.Combine(TemplatesRulesFolder, templateName + ".txt");

            if (!File.Exists(filePath))
            {
                InitTemplates();

                if (!File.Exists(filePath))
                {
                    return $"Missing template: {templateName}";
                }
            }

            return File.ReadAllText(filePath).Replace("\\n", "\n");
        }
        public static string LoadRulesModdedTemplate(string templateName)
        {
            string filePath = Path.Combine(TemplatesRulesModdedFolder, templateName + ".txt");

            if (!File.Exists(filePath))
            {
                InitTemplates();

                if (!File.Exists(filePath))
                {
                    return $"Missing template: {templateName}";
                }
            }

            return File.ReadAllText(filePath).Replace("\\n", "\n");
        }

        private static string ApplyPlaceholders(string template, string playerName = "")
        {
            int level = Options.KickLevelLevel.GetInt();

            return template
                .Replace("{player}", playerName)
                .Replace("{level}", level.ToString());
        }

        public static string FormatWelcomeTemplate(string templateName, string playerName)
        {
            string template = LoadWelcomeTemplate(templateName);
            return ApplyPlaceholders(template, playerName);
        }
        public static string FormatWelcomeModdedTemplate(string templateName, string playerName)
        {
            string template = LoadWelcomeModdedTemplate(templateName);
            return ApplyPlaceholders(template, playerName);
        }
        public static string FormatRulesTemplate(string templateName, string playerName)
        {
            string template = LoadRulesTemplate(templateName);
            return ApplyPlaceholders(template, playerName);
        }
        public static string FormatRulesModdedTemplate(string templateName, string playerName)
        {
            string template = LoadRulesModdedTemplate(templateName);
            return ApplyPlaceholders(template, playerName);
        }
        public static string FormatWelcomeTemplate(string templateName)
        {
            string template = LoadWelcomeTemplate(templateName);
            return ApplyPlaceholders(template);
        }
        public static string FormatWelcomeModdedTemplate(string templateName)
        {
            string template = LoadWelcomeModdedTemplate(templateName);
            return ApplyPlaceholders(template);
        }
        public static string FormatRulesTemplate(string templateName)
        {
            string template = LoadRulesTemplate(templateName);
            return ApplyPlaceholders(template);
        }
        public static string FormatRulesModdedTemplate(string templateName)
        {
            string template = LoadRulesModdedTemplate(templateName);
            return ApplyPlaceholders(template);
        }
    }




    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            BMLogger.Error($"Error loading texture from: {path}", "LoadImage");
        }
        return null;
    }
    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream?.CopyTo(ms);
            texture.LoadImage(ms.ToArray(), false);
            return texture;
        }
        catch
        {
            BMLogger.Error($"读入Texture失败：{path}", "LoadImage");
        }
        return null;
    }
    public static Texture2D LoadExternalTexture(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                BMLogger.Warn($"Cursor file not found: {filePath}");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            if (!texture.LoadImage(fileData))
            {
                BMLogger.Error("LoadImage failed");
                return null;
            }

            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return texture;
        }
        catch (Exception ex)
        {
            BMLogger.Error($"Failed to load external texture: {ex}");
        }

        return null;
    }

    //public static class TracersHandler
    //{
    //    private static Dictionary<byte, GameObject> arrows = new();

    //    public static void drawPlayerArrow(PlayerControl target)
    //    {
    //        try
    //        {
    //            if (PlayerControl.LocalPlayer == null || target == null || target.Data == null)
    //                return;

    //            if (target == PlayerControl.LocalPlayer) return;

    //            if (!target.Data.Role.IsImpostor || target.Data.IsDead)
    //                return;
    //            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead)
    //                return;

    //            if (!arrows.TryGetValue(target.PlayerId, out var arrowObj) || arrowObj == null)
    //            {
    //                arrowObj = new GameObject("ImpostorArrow");
    //                var spriteRenderer = arrowObj.AddComponent<SpriteRenderer>();

    //                spriteRenderer.sprite = ArrowSprite;
    //                spriteRenderer.color = target.Data.Role.TeamColor;
    //                spriteRenderer.sortingOrder = 9999;

    //                arrows[target.PlayerId] = arrowObj;
    //            }

    //            Vector3 localPos = PlayerControl.LocalPlayer.transform.position;
    //            Vector3 targetPos = target.transform.position;

    //            Vector3 direction = (targetPos - localPos).normalized;

    //            Vector3 offset = direction * 0.7f;

    //            arrowObj.transform.position = localPos + offset;

    //            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //            arrowObj.transform.rotation = UnityEngine.Quaternion.Euler(0f, 0f, angle);

    //            arrowObj.SetActive(true);
    //        }
    //        catch { }
    //    }

    //    public static void HideAllArrows()
    //    {
    //        foreach (var arrow in arrows.Values)
    //        {
    //            if (arrow != null)
    //                arrow.SetActive(false);
    //        }
    //    }

    //    public static Sprite ArrowSprite;


    //}
    public static void ShowCommand()
    {
        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, (
            Translator.GetString("CommandList")
            + $"\n  ○ <color=#FF0000><b>/dn (name)</b></color> {Translator.GetString("Command.dn")}"
            + $"\n  ○ <color=#FF0000><b>/ddn (name)</b></color> {Translator.GetString("Command.ddn")}"
            + $"\n  ○ <color=#FF0000><b>/dw (word)</b></color> {Translator.GetString("Command.dw")}"
            + $"\n  ○ <color=#FF0000><b>/ddw (word)</b></color> {Translator.GetString("Command.ddw")}"
            + $"\n  ○ <color=#FF0000><b>/ds (start)</b></color> {Translator.GetString("Command.ds")}"
            + $"\n  ○ <color=#FF0000><b>/dds (start)</b></color> {Translator.GetString("Command.dds")}"
            + $"\n  ○ <color=#FF0000><b>/f (id)</b></color> {Translator.GetString("Command.f")}"
            + $"\n  ○ <color=#FF0000><b>/df (id)</b></color> {Translator.GetString("Command.df")}"
            ));

    }
    public static void ShowCommand3()
    {
        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, (
            Translator.GetString("CommandList")
            + $"\n  ○ <color=#FF0000><b>/level (num)</b></color> {Translator.GetString("Command.level")}"
            + $"\n  ○ <color=#FF0000><b>/id</b></color> {Translator.GetString("Command.id")}"
            + $"\n  ○ <color=#FF0000><b>/m</b></color> {Translator.GetString("Command.role")}"
            + $"\n  ○ <color=#FF0000><b>/bm (playercolor)</b></color> {Translator.GetString("Command.bm")}"
            + $"\n  ○ <color=#FF0000><b>/role</b></color> {Translator.GetString("Command.roles")}"
            ));

    }
    public static void ShowCommand4()
    {
        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, (
            Translator.GetString("CommandList")
            + $"\n  <color=#3535cc><b>Host Command</b></color>"
            + $"\n  ○ <color=#83b325><b>/copy</color><color=#b02eb0> (playercolor)</b></color> : copy outfit"
            + $"\n  ○ <color=#83b325><b>/reset</b></color> : reset outfit"
            + $"\n  ○ <color=#83b325><b>/every</color><color=#b02eb0> (color)</b></color> : all player same color"
            + $"\n  ○ <color=#83b325><b>/setname</color><color=#b02eb0> (id) (name)</b></color> : save the name in customnames.txt"
            + $"\n  ○ <color=#83b325><b>/start</color><color=#b02eb0> (sec)</b></color> : start with sec countdown"
            + $"\n  <color=#3535cc><b>Moderator Command</b></color>"
            + $"\n  ○ <color=#83b325><b>/instantstart</b></color> : start without countdown"
            + $"\n  ○ <color=#83b325><b>/start</color><color=#b02eb0> (sec)</b></color> : start with sec countdown"
            + $"\n  ○ <color=#83b325><b>/meeting</b></color> : call meeting"
            + $"\n  ○ <color=#83b325><b>/every</color><color=#b02eb0> (color)</b></color> : all player same color"
            + $"\n  ○ <color=#83b325><b>/setname</color><color=#b02eb0> (id) (name)</b></color>: save the name in customnames.txt"
            + $"\n  ○ <color=#83b325><b>/destroy</b></color> : destroy lobby"
            + $"\n  ○ <color=#83b325><b>/lobby</b></color> : recreate lobby"
            + $"\n  ○ <color=#83b325><b>/endgame</b></color> : Force GameEnd"
            + $"\n  ○ <color=#83b325><b>/endmeeting</b></color> : Close Meeting"
            + $"\n  ○ <color=#83b325><b>/ban</color><color=#b02eb0> (id)</b></color> : Ban"
            + $"\n  ○ <color=#83b325><b>/Kick</color><color=#b02eb0> (id)</b></color> : Kick"

            + $"\n  ○ <color=#83b325><b>/all</b></color> {Translator.GetString("Command.all")}"
            ));

    }

    public static readonly Dictionary<string, Vector2> DevicePos = new()
    {
        ["SkeldAdmin"] = new(3.48f, -8.62f),
        ["SkeldCamera"] = new(-13.06f, -2.45f),
        ["MiraHQAdmin"] = new(21.02f, 19.09f),
        ["MiraHQDoorLog"] = new(16.22f, 5.82f),
        ["PolusLeftAdmin"] = new(22.80f, -21.52f),
        ["PolusRightAdmin"] = new(24.66f, -21.52f),
        ["PolusCamera"] = new(2.96f, -12.74f),
        ["PolusVital"] = new(26.70f, -15.94f),
        ["DleksAdmin"] = new(-3.48f, -8.62f),
        ["DleksCamera"] = new(13.06f, -2.45f),
        ["AirshipCockpitAdmin"] = new(-22.32f, 0.91f),
        ["AirshipRecordsAdmin"] = new(19.89f, 12.60f),
        ["AirshipCamera"] = new(8.10f, -9.63f),
        ["AirshipVital"] = new(25.24f, -7.94f),
        ["FungleCamera"] = new(6.20f, 0.10f),
        ["FungleVital"] = new(-2.50f, -9.80f)
    };
    public static readonly Dictionary<string, string> colorMap = new()
{
    { "gold2",       "#FFD700" },
    { "silver",      "#C0C0C0" },
    { "bronze",      "#CD7F32" },
    { "copper",      "#B87333" },
    { "platinum",    "#E5E4E2" },
    { "steel",       "#71797E" },
    { "rose_gold",   "#B76E79" },
    { "gunmetal",    "#2A3439" },
    { "chrome",      "#E5E4E2" },

    { "neonyellow",  "#CCFF00" },
    { "electriclime","#CCFF00" },
    { "neongreen",   "#39FF14" },
    { "neonblue",    "#1F51FF" },
    { "neonpink",    "#FF10F0" },
    { "neonorange",  "#FF5F1F" },
    { "neonpurple",  "#BC13FE" },
    { "neonred",     "#FF073A" },
    { "neoncyan",    "#0FF0FC" },
    { "hotpink",     "#FF69B4" },

    { "null",        "#FFFFFF00" },
    { "gold",        "#C6A25A" },
    { "brown2",      "#7F582D" },
    { "beige",       "#B6AA9E" },
    { "bluegreen",   "#00CEC8" },
    { "maize",       "#FBEC5D" },
    { "candy",       "#FF004D" },
    { "wine",        "#722F37" },

    { "white",       "#FFFFFF" },
    { "bianco",      "#FFFFFF" },
    { "weiß",        "#FFFFFF" },
    { "белый",       "#FFFFFF" },
    { "blanc",       "#FFFFFF" },

    { "blu",         "#0000FF" },
    { "blue",        "#0000FF" },
    { "blau",        "#0000FF" },
    { "синий",       "#0000FF" },
    { "bleu",        "#0000FF" },

    { "verde",       "#00FF00" },
    { "green",       "#00FF00" },
    { "grün",        "#00FF00" },
    { "зелёный",     "#00FF00" },
    { "vert",        "#00FF00" },

    { "fucsia",      "#FF00FF" },
    { "fuchsia",     "#FF00FF" },
    { "fuchsie",     "#FF00FF" },
    { "фуксия",      "#FF00FF" },

    { "pink",        "#FFC0CB" },
    { "rosa",        "#FFC0CB" },
    { "confetto",    "#FFC0CB" },
    { "розовый",     "#FFC0CB" },
    { "rose",        "#FFC0CB" },

    { "arancio",     "#FFA500" },
    { "arancione",   "#FFA500" },
    { "orange",      "#FFA500" },
    { "оранжевый",   "#FFA500" },

    { "giallo",      "#FFFF00" },
    { "gialla",      "#FFFF00" },
    { "yellow",      "#FFFF00" },
    { "gelb",        "#FFFF00" },
    { "жёлтый",      "#FFFF00" },
    { "jaune",       "#FFFF00" },

    { "nero",        "#000000" },
    { "nera",        "#000000" },
    { "black",       "#000000" },
    { "schwarz",     "#000000" },
    { "чёрный",      "#000000" },
    { "noir",        "#000000" },

    { "viola",       "#800080" },
    { "purple",      "#800080" },
    { "lila",        "#800080" },
    { "фиолетовый",  "#800080" },
    { "violet",      "#800080" },

    { "marrone",     "#8B4513" },
    { "brown",       "#8B4513" },
    { "braun",       "#8B4513" },
    { "коричневый",  "#8B4513" },
    { "marron",      "#8B4513" },

    { "ciano",       "#00FFFF" },
    { "azzurro",     "#00FFFF" },
    { "azzurra",     "#00FFFF" },
    { "cyan",        "#00FFFF" },
    { "hellblau",    "#00FFFF" },
    { "голубой",     "#00FFFF" },
    { "bleu clair",  "#00FFFF" },

    { "bordo",       "#800000" },
    { "bordeaux",    "#800000" },
    { "maroon",      "#800000" },
    { "kastanienbraun", "#800000" },
    { "бордовый",    "#800000" },

    { "crema",       "#FFFACD" },
    { "cream",       "#FFFACD" },
    { "creme",       "#FFFACD" },
    { "кремовый",    "#FFFACD" },
    { "crème",       "#FFFACD" },
    { "banana",      "#FFFACD" },
    { "banan",       "#FFFACD" },

    { "lime",        "#BFFF00" },
    { "limette",     "#BFFF00" },
    { "лайм",        "#BFFF00" },
    { "citron vert", "#BFFF00" },

    { "grigio",      "#808080" },
    { "grigia",      "#808080" },
    { "gray",        "#808080" },
    { "grau",        "#808080" },
    { "серый",       "#808080" },
    { "gris",        "#808080" },

    { "tortora",     "#D2B48C" },
    { "taupe",       "#D2B48C" },
    { "таупе",       "#D2B48C" },
    { "tan",         "#D2B48C" },

    { "corallo",     "#FF7F50" },
    { "coral",       "#FF7F50" },
    { "koralle",     "#FF7F50" },
    { "коралловый",  "#FF7F50" },
    { "corail",      "#FF7F50" },

    { "rosso",       "#FF0000" },
    { "rossa",       "#FF0000" },
    { "red",         "#FF0000" },
    { "rot",         "#FF0000" },
    { "красный",     "#FF0000" },
    { "rouge",       "#FF0000" }
};
    public static readonly Dictionary<string, string> colorMap1 = new(StringComparer.OrdinalIgnoreCase)
{
    { "white", "white" }, { "bianco", "white" }, { "weiß", "white" },
    { "белый", "white" }, { "blanc", "white" },

    { "black", "black" }, { "nero", "black" }, { "nera", "black" },
    { "schwarz", "black" }, { "чёрный", "black" }, { "noir", "black" },

    { "red", "red" }, { "rosso", "red" }, { "rossa", "red" },
    { "rot", "red" }, { "красный", "red" }, { "rouge", "red" },

    { "green", "green" }, { "verde", "green" }, { "grün", "green" },
    { "зелёный", "green" }, { "vert", "green" },

    { "yellow", "yellow" }, { "giallo", "yellow" }, { "gialla", "yellow" },
    { "gelb", "yellow" }, { "жёлтый", "yellow" }, { "jaune", "yellow" },

    { "blue", "blue" }, { "blu", "blue" }, { "blau", "blue" },
    { "синий", "blue" }, { "bleu", "blue" },

    { "cyan", "cyan" }, { "ciano", "cyan" }, { "azzurro", "cyan" },
    { "azzurra", "cyan" }, { "hellblau", "cyan" }, { "голубой", "cyan" }, { "bleu clair", "cyan" },

    { "magenta", "magenta" }, { "fucsia", "magenta" }, { "fuchsia", "magenta" },
    { "fuchsie", "magenta" }, { "фуксия", "magenta" },

    { "gray", "gray" }, { "grey", "gray" }, { "grigio", "gray" },
    { "grigia", "gray" }, { "grau", "gray" }, { "серый", "gray" }, { "gris", "gray" }
};


    public static readonly Dictionary<string, string> symbolMap = new()
{
    { "umbrella", "☂" },
    { "lines", "ミ" },
    { "lines1", "彡" },
    { "bracket", "『" },
    { "bracket1", "』" },
    { "spades", "♤" },
    { "spades1", "♠︎" },
    { "diamonds1", "♢" },
    { "bracket2", "〘" },
    { "bracket3", "〙" },
    { "cross", "†" },
    { "heart", "♥" },
    { "heart1", "♡" },
    { "infinity", "∞" },
    { "note", "♫" },
    { "note1", "♪" },
    { "star", "★" },
    { "star1", "☆" },
    { "true", "✓" },
    { "ying", "☯" },
    { "warning", "⚠" },
    { "clubs", "♣" },
    { "diamonds", "♦" },
    { "cloud", "☁" },
    { "divider", "┇" },
    { "flower", "✿" },
    { "flower1", "❀" },
    { "sun", "☀" },
    { "smile", "㋡" },
    { "smilea", "㋛" },
    { "smileb", "ッ" },
    { "smilec", "シ" },
    { "smiled", "ツ" },
    { "smilee", "ヅ" },
    { "smilef", "웃" }
};


    private static int _messageSplitCounter = 0;
    private static string NormalizeChatMessagePreserveLines(string message, bool removeHtml = true)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "";

        if (removeHtml)
            message = message.RemoveHtmlTags();

        message = message
            .Replace("\\r\\n", "\n")
            .Replace("\\n", "\n")
            .Replace("\\r", "\n")
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        return message.Trim();
    }

    private static bool IsAliveAvailableModdedSender(PlayerControl p)
    {
        try
        {
            return p != null &&
                   p.Data != null &&
                   !p.Data.Disconnected &&
                   !p.Data.IsDead &&
                   UnifiedRPCHandlerPatch.IsClientModded(p.PlayerId);
        }
        catch
        {
            return false;
        }
    }

    private static List<PlayerControl> GetAliveModdedMessageSenders()
    {
        List<PlayerControl> senders = new List<PlayerControl>();

        try
        {
            PlayerControl local = PlayerControl.LocalPlayer;

            if (IsAliveAvailableModdedSender(local))
                senders.Add(local);

            if (BanMod.AllPlayerControls != null)
            {
                foreach (PlayerControl p in BanMod.AllPlayerControls
                    .Where(p => IsAliveAvailableModdedSender(p))
                    .OrderBy(p => p.PlayerId))
                {
                    if (local != null && p.PlayerId == local.PlayerId)
                        continue;

                    senders.Add(p);
                }
            }
        }
        catch
        {
        }

        return senders;
    }

    private static PlayerControl GetAvailableModdedProxy()
    {
        try
        {
            return GetAliveModdedMessageSenders()
                .FirstOrDefault(p =>
                    p != null &&
                    PlayerControl.LocalPlayer != null &&
                    p.PlayerId != PlayerControl.LocalPlayer.PlayerId);
        }
        catch
        {
            return null;
        }
    }

    private static PlayerControl GetSelectedModdedProxy()
    {
        return GetAvailableModdedProxy();
    }

    private static bool ShouldSendWithProxy(out PlayerControl proxy)
    {
        proxy = null;

        if (BanModServerSelection.IsVanilla)
            return false;

        try
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                return false;

            List<PlayerControl> senders = GetAliveModdedMessageSenders();

            if (senders == null || senders.Count <= 1)
                return false;

            int index = _messageSplitCounter % senders.Count;
            _messageSplitCounter = (_messageSplitCounter + 1) % 1000000;

            PlayerControl selected = senders[index];

            if (selected == null)
                return false;

            if (PlayerControl.LocalPlayer != null &&
                selected.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                return false;
            }

            proxy = selected;
            return true;
        }
        catch
        {
            proxy = null;
            return false;
        }
    }
    private static IEnumerable<string> SplitTemplateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            yield break;

        message = message
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        int offset = 0;
        int count = 0;

        while (offset < message.Length && count < 3)
        {
            // Evita spazi e righe vuote all'inizio del blocco.
            while (offset < message.Length &&
                   char.IsWhiteSpace(message[offset]))
            {
                offset++;
            }

            if (offset >= message.Length)
                yield break;

            int length = Math.Min(120, message.Length - offset);

            if (offset + length < message.Length)
            {
                int cut = -1;

                // Preferisce un cambio di riga entro il limite.
                for (int i = length - 1; i > 0; i--)
                {
                    if (message[offset + i] == '\n')
                    {
                        cut = i;
                        break;
                    }
                }

                // Se manca, cerca uno spazio tra le parole.
                // Se il carattere successivo è già uno spazio,
                // il blocco termina già su una parola completa.
                if (cut < 0 &&
                    !char.IsWhiteSpace(message[offset + length]))
                {
                    for (int i = length - 1; i > 0; i--)
                    {
                        if (char.IsWhiteSpace(message[offset + i]))
                        {
                            cut = i;
                            break;
                        }
                    }
                }

                if (cut > 0)
                    length = cut;

                // Protegge le coppie UTF-16 quando il taglio è obbligato.
                if (char.IsHighSurrogate(message[offset + length - 1]) &&
                    char.IsLowSurrogate(message[offset + length]))
                {
                    length--;
                }
            }

            string part = message.Substring(offset, length).TrimEnd();
            offset += length;

            if (string.IsNullOrWhiteSpace(part))
                continue;

            count++;
            yield return part;
        }
    }

    private static readonly Queue<(
    PlayerControl sender,
    string message,
    byte target)> TemplateQueue = new();

    public static bool TemplateQueueRunning { get; private set; }

    public static void SendVanillaTemplateBatch(
    PlayerControl sender,
    string message,
    byte target = byte.MaxValue)
    {
        if (sender == null || sender.Data == null ||
            sender.Data.Disconnected ||
            string.IsNullOrWhiteSpace(message) ||
            HudManager.Instance == null)
        {
            return;
        }

        TemplateQueue.Enqueue((sender, message, target));

        if (TemplateQueueRunning)
            return;

        TemplateQueueRunning = true;

        HudManager.Instance.StartCoroutine(ProcessTemplateQueue());
    }
    private static IEnumerator ProcessTemplateQueue()
    {
        try
        {
            while (TemplateQueue.Count > 0)
            {
                // Aspetta il timer dell'ultimo messaggio inviato.
                // Non usa CanSendMessage: durante questa coroutine
                // TemplateQueueRunning è true.
                while (Time.time - MessageBlocker.lastMessageTime <
                       MessageBlocker.timeToWait)
                {
                    if (!BanModServerSelection.IsVanilla ||
                        AmongUsClient.Instance == null ||
                        HudManager.Instance == null)
                    {
                        TemplateQueue.Clear();
                        yield break;
                    }

                    yield return null;
                }

                if (!BanModServerSelection.IsVanilla ||
                    AmongUsClient.Instance == null ||
                    HudManager.Instance == null)
                {
                    TemplateQueue.Clear();
                    yield break;
                }

                var item = TemplateQueue.Dequeue();

                if (item.sender == null || item.sender.Data == null ||
                    item.sender.Data.Disconnected)
                {
                    continue;
                }

                // Aspetta che il template finisca tutti i suoi blocchi.
                yield return HudManager.Instance.StartCoroutine(
                    SendVanillaTemplateBatchCoroutine(
                        item.sender,
                        item.message,
                        item.target));

                // L'attesa per il prossimo template parte da qui.
                MessageBlocker.UpdateLastMessageTime();
            }
        }
        finally
        {
            TemplateQueueRunning = false;
        }
    }
    private static IEnumerator SendVanillaTemplateBatchCoroutine(
        PlayerControl sender,
        string message,
        byte target)
    {
        var parts = SplitTemplateMessage(message).ToList();

        for (int i = 0; i < parts.Count; i++)
        {
            while (Time.time - MessageBlocker.lastMessageTime <
                   MessageBlocker.timeToWait)
            {
                if (!BanModServerSelection.IsVanilla ||
                    AmongUsClient.Instance == null ||
                    sender == null ||
                    sender.Data == null ||
                    sender.Data.Disconnected)
                {
                    yield break;
                }

                yield return null;
            }

            if (!BanModServerSelection.IsVanilla ||
                sender == null || sender.Data == null ||
                sender.Data.Disconnected ||
                AmongUsClient.Instance == null)
            {
                yield break;
            }

            bool broadcast = target == byte.MaxValue;

            bool localOnly =
                PlayerControl.LocalPlayer != null &&
                target == PlayerControl.LocalPlayer.PlayerId;

            int clientId = -1;

            if (!broadcast && !localOnly)
            {
                var targetPlayer = GetPlayerById(target);

                if (targetPlayer == null || targetPlayer.Data == null ||
                    targetPlayer.Data.Disconnected)
                {
                    yield break;
                }

                clientId = targetPlayer.GetClientId();

                if (clientId < 0)
                    yield break;
            }

            if (!localOnly)
            {
                var writer = CustomRpcSender.Create(
                    "VanillaTemplatePart",
                    SendOption.Reliable);

                writer.StartMessage(clientId);

                writer.StartRpc(sender.NetId, (byte)RpcCalls.SendChat)
                    .Write(parts[i])
                    .EndRpc();

                writer.EndMessage();
                writer.SendMessage();
            }

            if (broadcast || localOnly)
                ShowProxyMessageLocally(sender, parts[i]);

            MessageBlocker.UpdateLastMessageTime();
        }
    }
    public static void RequestProxyMessage(string message, byte target = byte.MaxValue, PlayerControl forcedProxy = null)
    {
        try
        {
            message = NormalizeChatMessagePreserveLines(message, false);

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (AmongUsClient.Instance == null)
                return;

            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
                return;

            //if (message.Length > 120 && BanModServerSelection.IsVanilla)
            //{
            //    try
            //    {
            //        NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
            //            HudManager.Instance.Notifier,
            //            $"Messaggio troppo lungo! Max 120 caratteri. Messaggio digitato: \"{message}\" (Lunghezza: {message.Length})");
            //    }
            //    catch
            //    {
            //    }

            //    return;
            //}

            PlayerControl proxy = forcedProxy;

            if (proxy == null ||
                proxy.Data == null ||
                proxy.Data.Disconnected ||
                proxy.Data.IsDead ||
                proxy == PlayerControl.LocalPlayer)
            {
                proxy = GetAvailableModdedProxy();
            }

            if (proxy == null)
                return;

            int proxyClientId = proxy.GetClientId();

            if (proxyClientId < 0)
                return;

            BanModSyncApi.SendProxySendChat(
                message,
                target,
                proxy.PlayerId);

        }
        catch
        {
        }
    }

    public static class ProxyMessageQueue
    {
        private static readonly Queue<(string msg, int targetClientId, bool sendToAll)> queue = new();

        public static void Enqueue(string msg, int targetClientId, bool sendToAll = false)
        {
            msg = NormalizeChatMessagePreserveLines(msg);

            if (string.IsNullOrWhiteSpace(msg))
                return;

            queue.Enqueue((msg, targetClientId, sendToAll));
        }

        public static void TrySendNext()
        {
            if (queue.Count == 0)
                return;

            if (!MessageBlocker.CanSendMessage())
                return;

            var localPlayer = PlayerControl.LocalPlayer;

            if (localPlayer == null ||
                localPlayer.Data == null ||
                localPlayer.Data.IsDead ||
                localPlayer.Data.Disconnected)
            {
                return;
            }

            var (msg, targetClientId, sendToAll) = queue.Peek();

            msg = NormalizeChatMessagePreserveLines(msg);

            if (string.IsNullOrWhiteSpace(msg))
            {
                queue.Dequeue();
                return;
            }

            if (msg.Length > 120 && BanModServerSelection.IsVanilla)
            {
                queue.Dequeue();
                return;
            }

            try
            {
                var writer = CustomRpcSender.Create("ProxySendChatDirect", SendOption.Reliable);

                writer.StartMessage(targetClientId);
                writer.StartRpc(localPlayer.NetId, (byte)RpcCalls.SendChat)
                    .Write(msg)
                    .EndRpc();
                writer.EndMessage();
                writer.SendMessage();

                if (sendToAll)
                    ShowProxyMessageLocally(localPlayer, msg);

                queue.Dequeue();

                MessageBlocker.UpdateLastMessageTime();
            }
            catch
            {
            }
        }

        public static void ClearQueue()
        {
            queue.Clear();
        }
    }

    private static void ShowProxyMessageLocally(PlayerControl localPlayer, string message)
    {
        try
        {
            if (localPlayer == null)
                return;

            message = NormalizeChatMessagePreserveLines(message);

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (HudManager.Instance != null && HudManager.Instance.Chat != null)
            {
                HudManager.Instance.Chat.AddChat(localPlayer, message);
                return;
            }
        }
        catch
        {
        }

        try
        {
            if (localPlayer == null)
                return;

            message = NormalizeChatMessagePreserveLines(message);

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (DestroyableSingleton<HudManager>.Instance != null &&
                DestroyableSingleton<HudManager>.Instance.Chat != null)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(localPlayer, message);
            }
        }
        catch
        {
        }
    }
    public static void HandleProxySendChatRequest(string message, byte target)
    {
        try
        {
            PlayerControl localPlayer = PlayerControl.LocalPlayer;

            if (localPlayer == null || localPlayer.Data == null)
                return;

            if (localPlayer.Data.IsDead || localPlayer.Data.Disconnected)
                return;

            message = NormalizeChatMessagePreserveLines(message);

            if (string.IsNullOrWhiteSpace(message))
                return;

            //if (message.Length > 120 && BanModServerSelection.IsVanilla)
            //    message = message.Substring(0, 120);
            if (BanModServerSelection.IsVanilla && message.Length > 120)
            {
                SendVanillaTemplateBatch(localPlayer, message, target);
                return;
            }

            bool sendToAll = target == byte.MaxValue || target == 255;
            bool targetIsProxySelf = target == localPlayer.PlayerId;

            int targetClientId = -1;

            if (!sendToAll && !targetIsProxySelf)
            {
                PlayerControl targetPlayer = Utils.GetPlayerById(target);

                if (targetPlayer == null || targetPlayer.Data == null || targetPlayer.Data.Disconnected)
                    return;

                targetClientId = targetPlayer.GetClientId();

                if (targetClientId < 0)
                    return;
            }

            if (targetIsProxySelf)
            {
                ShowProxyMessageLocally(localPlayer, message);
                MessageBlocker.UpdateLastMessageTime();
                return;
            }

            if (!MessageBlocker.CanSendMessage())
            {
                ProxyMessageQueue.Enqueue(message, targetClientId, sendToAll);
                return;
            }

            try
            {
                var writer = CustomRpcSender.Create("ProxySendChatDirect", SendOption.Reliable);

                writer.StartMessage(targetClientId);
                writer.StartRpc(localPlayer.NetId, (byte)RpcCalls.SendChat)
                    .Write(message)
                    .EndRpc();
                writer.EndMessage();
                writer.SendMessage();

                if (sendToAll)
                    ShowProxyMessageLocally(localPlayer, message);

                MessageBlocker.UpdateLastMessageTime();
            }
            catch
            {
            }
        }
        catch
        {
        }
    }

    //public static void SendMessage(string text, byte sendTo = byte.MaxValue)
    //{
    //    text = NormalizeChatMessagePreserveLines(text, false);

    //    if (string.IsNullOrWhiteSpace(text))
    //        return;

    //    if (text.Length > 120 && BanModServerSelection.IsVanilla)
    //    {
    //        NotificationPopper_AddInfoMessagePatch.AddInfoMessage(
    //            HudManager.Instance.Notifier,
    //            $"Messaggio troppo lungo! Max 120 caratteri. Messaggio digitato: \"{text}\" (Lunghezza: {text.Length})");

    //        Debug.LogError($"Messaggio troppo lungo! Max 120 caratteri. Messaggio digitato: \"{text}\" (Lunghezza: {text.Length})");
    //        return;
    //    }

    //    if (sendTo != byte.MaxValue)
    //    {
    //        var targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
    //            p != null &&
    //            p.PlayerId == sendTo &&
    //            p.Data != null &&
    //            !p.Data.Disconnected);

    //        if (targetPlayer == null)
    //            return;
    //    }

    //    if (ShouldSendWithProxy(out PlayerControl proxy) && proxy != null)
    //    {
    //        RequestProxyMessage(text, sendTo, proxy);
    //        return;
    //    }

    //    if (!MessageBlocker.CanSendMessage())
    //    {
    //        MessageRetryHandler.QueueMessage(text, sendTo);
    //        MessageRetryHandler.TrySendPending();
    //        return;
    //    }

    //    BanMod.MessagesToSend.Add((text, sendTo));
    //    MessageBlocker.UpdateLastMessageTime();
    //}
    public static void SendMessage(string text, byte sendTo = byte.MaxValue)
    {
        text = NormalizeChatMessagePreserveLines(text, false);

        if (string.IsNullOrWhiteSpace(text))
            return;

        if (sendTo != byte.MaxValue)
        {
            var targetPlayer = BanMod.AllPlayerControls.FirstOrDefault(p =>
                p != null &&
                p.PlayerId == sendTo &&
                p.Data != null &&
                !p.Data.Disconnected);

            if (targetPlayer == null)
                return;
        }

        bool splitTemplate =
            BanModServerSelection.IsVanilla &&
            text.Length > 120;

        // Se il mittente è morto, il template pubblico o privato
        // deve essere inviato da un proxy vivo.
        if (splitTemplate &&
            PlayerControl.LocalPlayer?.Data != null &&
            PlayerControl.LocalPlayer.Data.IsDead &&
            sendTo != PlayerControl.LocalPlayer.PlayerId)
        {
            RequestProxyMessage(text, sendTo);
            return;
        }

        if (ShouldSendWithProxy(out PlayerControl proxy) && proxy != null)
        {
            RequestProxyMessage(text, sendTo, proxy);
            return;
        }

        if (splitTemplate)
        {
            BanMod.MessagesToSend.Add((text, sendTo));
            return;
        }

        if (!MessageBlocker.CanSendMessage())
        {
            MessageRetryHandler.QueueMessage(text, sendTo);
            MessageRetryHandler.TrySendPending();
            return;
        }

        BanMod.MessagesToSend.Add((text, sendTo));
        MessageBlocker.UpdateLastMessageTime();
    }
    public static class MessageBlocker
    {
        public static float lastMessageTime = BanModServerSelection.IsVanilla ? -1.5f : 0f;
        public static float timeToWait = BanModServerSelection.IsVanilla ? 1.5f : 0f;

        //public static bool CanSendMessage()
        //{
        //    if (BanModServerSelection.IsVanilla)
        //    {
        //        return Time.time - lastMessageTime >= timeToWait;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        public static bool CanSendMessage()
        {
            if (!BanModServerSelection.IsVanilla)
                return true;

            if (TemplateQueueRunning)
                return false;

            return Time.time - lastMessageTime >= timeToWait;
        }
        public static void UpdateLastMessageTime()
        {
            lastMessageTime = Time.time;
        }

        public static void Reset()
        {
            lastMessageTime = -timeToWait;
        }
    }

    public static class MessageRetryHandler
    {
        private static readonly object queueLock = new object();
        private static Queue<(string text, byte sendTo)> pendingMessages = new();

        public static void ClearQueue()
        {
            lock (queueLock)
            {
                pendingMessages.Clear();
            }
        }

        public static void TrySendPending()
        {
            int safetyCounter = 50;

            lock (queueLock)
            {
                while (pendingMessages.Count > 0 && safetyCounter-- > 0)
                {
                    var msg = pendingMessages.Peek();

                    msg.text = NormalizeChatMessagePreserveLines(msg.text, false);

                    if (string.IsNullOrWhiteSpace(msg.text))
                    {
                        pendingMessages.Dequeue();
                        continue;
                    }

                    if (msg.text.Length > 120 && BanModServerSelection.IsVanilla)
                    {
                        pendingMessages.Dequeue();
                        continue;
                    }

                    if (msg.sendTo != byte.MaxValue)
                    {
                        var player = BanMod.AllPlayerControls.FirstOrDefault(p =>
                            p != null &&
                            p.PlayerId == msg.sendTo &&
                            p.Data != null &&
                            !p.Data.Disconnected);

                        if (player == null)
                        {
                            pendingMessages.Dequeue();
                            continue;
                        }
                    }

                    if (ShouldSendWithProxy(out PlayerControl proxy) && proxy != null)
                    {
                        pendingMessages.Dequeue();
                        RequestProxyMessage(msg.text, msg.sendTo, proxy);
                        continue;
                    }

                    if (!MessageBlocker.CanSendMessage())
                        break;

                    pendingMessages.Dequeue();
                    BanMod.MessagesToSend.Add((msg.text, msg.sendTo));
                    MessageBlocker.UpdateLastMessageTime();
                }
            }
        }

        public static void QueueMessage(string text, byte sendTo)
        {
            text = NormalizeChatMessagePreserveLines(text, false);

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text.Length > 120 && BanModServerSelection.IsVanilla)
                return;

            lock (queueLock)
            {
                pendingMessages.Enqueue((text, sendTo));
            }
        }
    }

    public static void AbortAllMessages()
    {
        TemplateQueue.Clear();

        MessageRetryHandler.ClearQueue();

        ProxyMessageQueue.ClearQueue();

        BanMod.MessagesToSend.Clear();

        _messageSplitCounter = 0;

        MessageBlocker.Reset();
    }

    public static string NumberToWords(int number)
    {
        if (number < 0) return Translator.GetString("minus") + " " + NumberToWords(Math.Abs(number));
        if (number == 0) return Translator.GetString("zero");

        SupportedLangs currentLang = Translator.GetUserTrueLang();

        string[] words = {
            Translator.GetString("zero"), Translator.GetString("one"), Translator.GetString("two"),
            Translator.GetString("three"), Translator.GetString("four"), Translator.GetString("five"),
            Translator.GetString("six"), Translator.GetString("seven"), Translator.GetString("eight"),
            Translator.GetString("nine"), Translator.GetString("ten"), Translator.GetString("eleven"),
            Translator.GetString("twelve"), Translator.GetString("thirteen"), Translator.GetString("fourteen"),
            Translator.GetString("fifteen"), Translator.GetString("sixteen"), Translator.GetString("seventeen"),
            Translator.GetString("eighteen"), Translator.GetString("nineteen")
        };

        string[] tens = {
            "", "", Translator.GetString("twenty"), Translator.GetString("thirty"),
            Translator.GetString("forty"), Translator.GetString("fifty"), Translator.GetString("sixty"),
            Translator.GetString("seventy"), Translator.GetString("eighty"), Translator.GetString("ninety")
        };

        if (number < 20)
        {
            if (currentLang == SupportedLangs.French && number >= 17)
            {
                string unitPartKey = "";
                switch (number % 10)
                {
                    case 7: unitPartKey = "seven"; break;
                    case 8: unitPartKey = "eight"; break;
                    case 9: unitPartKey = "nine"; break;
                }
                return Translator.GetString("ten") + "-" + Translator.GetString(unitPartKey);
            }
            return words[number];
        }

        if (number < 100)
        {
            int tensDigit = number / 10;
            int unitDigit = number % 10;
            string result = tens[tensDigit];

            if (unitDigit > 0)
            {
                switch (currentLang)
                {
                    case SupportedLangs.Italian:
                        if (unitDigit == 1) result = result.Substring(0, result.Length - 1) + Translator.GetString("one");
                        else if (unitDigit == 8) result = result.Substring(0, result.Length - 1) + Translator.GetString("eight");
                        else if (unitDigit == 3) result = result.Substring(0, result.Length - 1) + Translator.GetString("three");
                        else result += words[unitDigit];
                        break;

                    case SupportedLangs.English:
                        result += "-" + words[unitDigit];
                        break;

                    case SupportedLangs.French:
                        if (tensDigit == 7)
                        {
                            result = tens[6];
                            if (unitDigit == 1) result += " " + Translator.GetString("french_and_eleven");
                            else result += "-" + words[10 + unitDigit];
                        }
                        else if (tensDigit == 8 && unitDigit == 0)
                        {
                        }
                        else if (tensDigit == 8)
                        {
                            result += "-" + words[unitDigit];
                        }
                        else if (tensDigit == 9)
                        {
                            result = tens[8];
                            result += "-" + words[10 + unitDigit];
                        }
                        else
                        {
                            if (unitDigit == 1) result += " " + Translator.GetString("french_and_one");
                            else result += "-" + words[unitDigit];
                        }
                        break;

                    case SupportedLangs.German:
                        if (unitDigit == 0)
                        {
                        }
                        else
                        {
                            string unitWord = (unitDigit == 1) ? Translator.GetString("german_unit_one") : words[unitDigit];
                            result = unitWord + Translator.GetString("german_and") + tens[tensDigit];
                        }
                        break;

                    case SupportedLangs.Russian:
                        result += " " + words[unitDigit];
                        break;

                    default:
                        result = number.ToString();
                        break;
                }
            }
            return result;
        }

        return number.ToString();
    }

    public static Dictionary<byte, float> playerDeathTimes = new Dictionary<byte, float>();
    public static List<PlayerControl> AllPlayerControls;
    public static float MeetingStartTime = 0f;
    public static bool Scientist(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Scientist;
    }
    public static bool Angel(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.GuardianAngel;
    }
    public static bool Engineer(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Engineer;
    }
    public static bool Detective(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Detective;
    }
    public static bool SpiritGuide(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.SpiritGuide;
    }
    public static bool Noisemaker(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Noisemaker;
    }
    public static bool Cobra(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Viper;
    }
    public static bool Tracker(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Tracker;
    }
    public static bool Impostor(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Impostor;
    }
    public static bool IsShapeshifted(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player != null && player.shapeshiftTargetPlayerId != -1;
    }
    public static bool Shapeshifter(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Shapeshifter;
    }
    public static bool Phantom(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Phantom;
    }
    public static bool Crewmate(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Crewmate;
    }
    public static bool ImpostorTeam(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.Role.TeamType == RoleTeamTypes.Impostor;
    }
    public static bool CrewTeam(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.Role.TeamType == RoleTeamTypes.Crewmate;
    }
    public static void OnPlayerDeath(PlayerControl player)
    {
        if (player != null)
        {
            BanMod.playerDeathTimes[player.PlayerId] = Time.time;
        }
    }
    public static bool Judge(PlayerControl player)
    {
        if (player == null) return false;
        if (player.Data == null) return false;
        if (player.Data.Role == null) return false;
        return player.Data.RoleType == RoleTypes.Judge;
    }
    public static class SabotageManager
    {
        public static bool IsSabotageActive = false;

        public static float GameSabotageCooldownRemaining = 0f;

        public static bool TryActivateSabotage(SystemTypes sabotageType, byte value, bool closeCafeteriaDoors = true)
        {
            if (IsSabotageActive)
            {
                return false;
            }

            IsSabotageActive = true;


            if (closeCafeteriaDoors)
            {
                ShipStatus.Instance.RpcCloseDoorsOfType(SystemTypes.Cafeteria);
            }

            ShipStatus.Instance.RpcUpdateSystem(sabotageType, value);

            return true;
        }
        public static void SetSabotageActiveState(bool active)
        {
            IsSabotageActive = active;
        }

        public static void SetGameSabotageCooldown(float remainingTime)
        {
            GameSabotageCooldownRemaining = remainingTime;
        }
    }

    public static void Exeme()
    {
        PlayerControl playerToExile = PlayerControl.LocalPlayer;

        if (playerToExile == null || !AmongUsClient.Instance.AmHost) return;

        NetworkedPlayerInfo playerToExileInfo = GameData.Instance.GetPlayerById(playerToExile.PlayerId);

        List<MeetingHud.VoterState> statesList = new();

        MeetingHud.Instance.RpcVotingComplete(statesList.ToArray(), playerToExileInfo, false, false, 0);
        MeetingHud.Instance.Close();
        MeetingHud.Instance.RpcClose();
    }
    public static ClientData GetClientById(int id)
    {
        try { return AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Id == id); }
        catch { return null; }
    }
    public static unsafe class FastDestroyableSingleton<T> where T : MonoBehaviour
    {
        private static readonly IntPtr FieldPtr;
        private static readonly Func<IntPtr, T> CreateObject;
        static FastDestroyableSingleton()
        {
            FieldPtr = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<DestroyableSingleton<T>>.NativeClassPtr, nameof(DestroyableSingleton<T>._instance));
            var constructor = typeof(T).GetConstructor([typeof(IntPtr)]);
            var ptr = Expression.Parameter(typeof(IntPtr));
            var create = Expression.New(constructor!, ptr);
            var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
            CreateObject = lambda.Compile();
        }
        public static T Instance
        {
            get
            {
                IntPtr objectPointer;
                IL2CPP.il2cpp_field_static_get_value(FieldPtr, &objectPointer);
                return objectPointer == IntPtr.Zero ? DestroyableSingleton<T>.Instance : CreateObject(objectPointer);
            }
        }
    }
    public static bool fullBrightActive()
    {

        return GameStates.IsDead || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }
    public static bool IsPlayerActive(byte playerId)
    {
        return BanMod.AllPlayerControls.Any(p => p.PlayerId == playerId && !p.Data.IsDead);
    }
    public static bool chatUiActive()
    {
        try
        {
            bool isImpostor = PlayerControl.LocalPlayer != null &&
                              PlayerControl.LocalPlayer.Data != null &&
                              PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor;

            bool chatOffIfImpostorEnabled = BanMod.ChatOffIfImpostor.Value;

            bool activeChatValue = BanMod.AktiveChat.Value;

            if (isImpostor && chatOffIfImpostorEnabled)
            {
                activeChatValue = false;
            }

            return activeChatValue || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead;
        }
        catch
        {
            return false;
        }
    }
    public static void openChat()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }
        }

    }
    public static void closeChat()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
        }

    }

    public static string getColoredPingText(int ping)
    {

        if (ping <= 100)
        {
            return $"<color=#00ff00ff>PING: {ping} ms</color>";
        }
        else if (ping < 400)
        {
            return $"<color=#ffff00ff>PING: {ping} ms</color>";
        }
        else
        {
            return $"<color=#ff0000ff>PING: {ping} ms</color>";
        }
    }
    public static bool IsVip(string friendCode)
    {
        return AllowedManager.IsVip(friendCode);
    }

    public static bool IsModerator(string friendCode)
    {
        return AllowedManager.IsModerator(friendCode);
    }
    public class PlayerState(byte playerId)
    {
        public readonly byte PlayerId = playerId;
        public bool IsDead { get; set; } = false;
        public bool Disconnected { get; set; } = false;
        public NetworkedPlayerInfo.PlayerOutfit NormalOutfit { get; internal set; }
    }
    public static PlayerControl GetPlayerById(int PlayerId, bool fast = true)
    {

        if (PlayerId is > byte.MaxValue or < byte.MinValue) return null;
        return BanMod.AllPlayerControls.FirstOrDefault(x => x.PlayerId == PlayerId);
    }

    public static byte MsgToColor(string text, bool isHost = false)
    {
        text = text.ToLowerInvariant();
        text = text.Replace("色", string.Empty);
        int color;
        try { color = int.Parse(text); } catch { color = -1; }
        switch (text)
        {
            case "0":
            case "rosso":
            case "Rosso":
            case "rouge":
            case "Rouge":
            case "red":
            case "Red":
            case "rot":
            case "Rot":
            case "красный":
            case "Красный":
                color = 0; break;
            case "1":
            case "blu":
            case "Blu":
            case "bleu":
            case "Bleu":
            case "blue":
            case "Blue":
            case "blau":
            case "Blau":
            case "синий":
            case "Синий":
                color = 1; break;
            case "2":
            case "verde":
            case "Verde":
            case "vert":
            case "Vert":
            case "green":
            case "Green":
            case "grün":
            case "Grün":
            case "зеленый":
            case "Зеленый":
                color = 2; break;
            case "3":
            case "rosa":
            case "Rosa":
            case "pink":
            case "Pink":
            case "fucsia":
            case "Fucsia":
            case "розовый":
            case "Розовый":
                color = 3; break;
            case "4":
            case "arancione":
            case "Arancione":
            case "orange":
            case "Orange":
            case "arancio":
            case "Arancio":
            case "оранжевый":
            case "Оранжевый":
                color = 4; break;
            case "5":
            case "giallo":
            case "Giallo":
            case "jaune":
            case "Jaune":
            case "yellow":
            case "Yellow":
            case "gelb":
            case "Gelb":
            case "желтый":
            case "Желтый":
                color = 5; break;
            case "6":
            case "nero":
            case "Nero":
            case "noir":
            case "Noir":
            case "black":
            case "Black":
            case "schwarz":
            case "Schwarz":
            case "черный":
            case "Черный":
                color = 6; break;
            case "7":
            case "bianco":
            case "Bianco":
            case "blanc":
            case "Blanc":
            case "white":
            case "White":
            case "weiss":
            case "Weiss":
            case "белый":
            case "Белый":
                color = 7; break;
            case "8":
            case "viola":
            case "Viola":
            case "violet":
            case "Violet":
            case "purple":
            case "Purple":
            case "lila":
            case "Lila":
            case "фиолетовый":
            case "Фиолетовый":
                color = 8; break;
            case "9":
            case "marrone":
            case "Marrone":
            case "marron":
            case "Marron":
            case "brown":
            case "Brown":
            case "braun":
            case "Braun":
            case "коричневый":
            case "Коричневый":
                color = 9; break;
            case "10":
            case "ciano":
            case "Ciano":
            case "cyan":
            case "Cyan":
            case "голубой":
            case "Голубой":
                color = 10; break;
            case "11":
            case "lime":
            case "Lime":
            case "лайм":
            case "Лайм":
                color = 11; break;
            case "12":
            case "bordeaux":
            case "Bordeaux":
            case "bordo":
            case "Bordo":
            case "maroon":
            case "Maroon":
            case "бордовый":
            case "Бордовый":
                color = 12; break;
            case "13":
            case "confetto":
            case "Confetto":
            case "rose":
            case "Rose":
                color = 13; break;
            case "14":
            case "banana":
            case "Banana":
            case "banane":
            case "Banane":
            case "crema":
            case "Crema":
            case "банановый":
            case "Банановый":
                color = 14; break;
            case "15":
            case "grigio":
            case "Grigio":
            case "gris":
            case "Gris":
            case "gray":
            case "Gray":
            case "grau":
            case "Grau":
            case "серый":
            case "Серый":
                color = 15; break;
            case "16":
            case "beige":
            case "Beige":
            case "Tortora":
            case "tortora":
            case "tan":
            case "Tan":
            case "бежевый":
            case "Бежевый":
                color = 16; break;
            case "17":
            case "corallo":
            case "Corallo":
            case "corail":
            case "Corail":
            case "coral":
            case "Coral":
            case "koralle":
            case "Koralle":
            case "коралловый":
            case "Коралловый":
                color = 17; break;

            case "18": case "隐藏": case "?": color = 18; break;
        }
        if (color == 18)
            return byte.MaxValue;
        return !isHost && color == 18 ? byte.MaxValue : color is < 0 or > 18 ? byte.MaxValue : Convert.ToByte(color);
    }

    public static string ColorIdToName(int colorId)
    {
        string key = colorId switch
        {
            0 => "ColorRed",
            1 => "ColorBlue",
            2 => "ColorGreen",
            3 => "ColorPink",
            4 => "ColorOrange",
            5 => "ColorYellow",
            6 => "ColorBlack",
            7 => "ColorWhite",
            8 => "ColorPurple",
            9 => "ColorBrown",
            10 => "ColorCyan",
            11 => "ColorLime",
            12 => "ColorBordeaux",
            13 => "ColorConfetto",
            14 => "ColorBanana",
            15 => "ColorGray",
            16 => "ColorBeige",
            17 => "ColorCoral",
            18 => "ColorHidden",
            _ => "ColorRed"
        };

        return Translator.GetString(key);
    }
    public static Color ColorIdToColor(int colorId)
    {
        return colorId switch
        {
            0 => Color.red,
            1 => Color.blue,
            2 => Color.green,
            3 => new Color(1f, 0.4f, 0.7f),
            4 => new Color(1f, 0.5f, 0f),
            5 => Color.yellow,
            6 => Color.black,
            7 => Color.white,
            8 => new Color(0.5f, 0f, 0.5f),
            9 => new Color(0.36f, 0.26f, 0.2f),
            10 => Color.cyan,
            11 => new Color(0.5f, 1f, 0f),
            12 => new Color(0.5f, 0f, 0f),
            13 => new Color(1f, 0.8f, 0.9f),
            14 => new Color(1f, 1f, 0.5f),
            15 => Color.gray,
            16 => new Color(0.96f, 0.96f, 0.86f),
            17 => new Color(1f, 0.5f, 0.31f),
            18 => new Color(0.2f, 0.2f, 0.2f),
            _ => Color.red
        };
    }
    public static void SendEngineerMessage()
    {
        var engineerPlayer = BanMod.AllPlayerControls
            .FirstOrDefault(p => p.Data != null && p.Data.RoleType == RoleTypes.Engineer);

        if (engineerPlayer == null)
            return;

        byte engineerId = engineerPlayer.PlayerId;

        string msg = GetString("EngineerMessage");

        if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
        {
            Utils.RequestProxyMessage(msg, engineerId);
            MessageBlocker.UpdateLastMessageTime();
        }
        else
        {
            Utils.SendMessage(msg, engineerId);
            MessageBlocker.UpdateLastMessageTime();
        }

    }
    public static void SendShapeshifterMessage()
    {
        var ShapeshifterPlayer = BanMod.AllPlayerControls
            .FirstOrDefault(p => p.Data != null && p.Data.RoleType == RoleTypes.Shapeshifter);

        if (ShapeshifterPlayer == null)
            return;

        byte ShapeshifterId = ShapeshifterPlayer.PlayerId;

        string msg = string.Format(GetString("ShapeshifterMessage"));
        if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
        {
            Utils.RequestProxyMessage(msg, ShapeshifterId);
            MessageBlocker.UpdateLastMessageTime();
        }
        else
        {
            Utils.SendMessage(msg, ShapeshifterId);
            MessageBlocker.UpdateLastMessageTime();
        }

    }
    public static void SendPhantomMessage()
    {
        var PhantomPlayer = BanMod.AllPlayerControls
            .FirstOrDefault(p => p.Data != null && p.Data.RoleType == RoleTypes.Phantom);

        if (PhantomPlayer == null)
            return;

        byte PhantomId = PhantomPlayer.PlayerId;
        string msg = string.Format(GetString("PhantomMessage"));
        if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
        {
            Utils.RequestProxyMessage(msg, PhantomId);
            MessageBlocker.UpdateLastMessageTime();
        }
        else
        {
            Utils.SendMessage(msg, PhantomId);
            MessageBlocker.UpdateLastMessageTime();
        }

    }
    public static void SendPhantomNBMessage()
    {
        var PhantomPlayer = BanMod.AllPlayerControls
            .FirstOrDefault(p => p.Data != null && p.Data.RoleType == RoleTypes.Phantom);

        if (PhantomPlayer == null)
            return;

        byte PhantomId = PhantomPlayer.PlayerId;
        string msg = string.Format(GetString("PhantomNBMessage"));
        if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer.Data.IsDead && BanModServerSelection.IsVanilla)
        {
            Utils.RequestProxyMessage(msg, PhantomId);
            MessageBlocker.UpdateLastMessageTime();
        }
        else
        {
            Utils.SendMessage(msg, PhantomId);
            MessageBlocker.UpdateLastMessageTime();
        }

    }
    public static bool AnySabotageIsActive()
   => IsActive(SystemTypes.Electrical)
      || IsActive(SystemTypes.Comms)
      || IsActive(SystemTypes.MushroomMixupSabotage)
      || IsActive(SystemTypes.Laboratory)
      || IsActive(SystemTypes.LifeSupp)
      || IsActive(SystemTypes.Reactor)
      || IsActive(SystemTypes.HeliSabotage);

    public static bool IsActive(SystemTypes type)
    {

        if (!ShipStatus.Instance.Systems.ContainsKey(type))
        {
            return false;
        }

        switch (type)
        {
            case SystemTypes.Electrical:
                {
                    var SwitchSystem = ShipStatus.Instance.Systems[type].TryCast<SwitchSystem>();
                    return SwitchSystem != null && SwitchSystem.IsActive;
                }
            case SystemTypes.Reactor:
                {
                    {
                        var ReactorSystemType = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                        return ReactorSystemType != null && ReactorSystemType.IsActive;
                    }
                }
            case SystemTypes.Laboratory:
                {
                    var ReactorSystemType = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                    return ReactorSystemType != null && ReactorSystemType.IsActive;
                }
            case SystemTypes.HeliSabotage:
                {
                    var HeliSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<HeliSabotageSystem>();
                    return HeliSabotageSystem != null && HeliSabotageSystem.IsActive;
                }
            case SystemTypes.LifeSupp:
                {
                    var LifeSuppSystemType = ShipStatus.Instance.Systems[type].TryCast<LifeSuppSystemType>();
                    return LifeSuppSystemType != null && LifeSuppSystemType.IsActive;
                }
            case SystemTypes.Comms:
                {
                    var hqHud = ShipStatus.Instance.Systems[type].TryCast<HqHudSystemType>();
                    var hudOverride = ShipStatus.Instance.Systems[type].TryCast<HudOverrideSystemType>();

                    return (hqHud != null && hqHud.IsActive) || (hudOverride != null && hudOverride.IsActive);
                }
            case SystemTypes.MushroomMixupSabotage:
                {
                    var MushroomMixupSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<MushroomMixupSabotageSystem>();
                    return MushroomMixupSabotageSystem != null && MushroomMixupSabotageSystem.IsActive;
                }
            default:
                return false;
        }

    }

    public static class VoteContextManager
    {
        public static bool IsForcedVote = false;
    }
    public static string ToFullWidthNumbers(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return new string(input.Select(c =>
        {
            if (c >= '0' && c <= '9')
                return (char)('０' + (c - '0'));

            return c;
        }).ToArray());
    }


}

public static class MatchSummary1
{
    public static bool ZombieWin = false;
    public static bool ZombieCrewmateWin = false;
    public static bool HotPotatoWin = false;
    public static string HotPotatoWinnerName = "";

    public static bool ImpostorWin = false;
    public static bool CrewmateWin = false;
    public static bool JesterWin = false;
    public static bool TaskWin = false;

    public static bool GameTimerWasEnabled = false;
    public static bool GameEndedByTimer = false;

    public static GameTimeLimit.TimerWinner GameTimerWinner =
        GameTimeLimit.TimerWinner.None;

    public static bool GameTimerPausedDuringMeetings = false;

    public static float GameTimerTotal = 0f;
    public static float GameTimerElapsed = 0f;
    public static float GameTimerRemaining = 0f;
    public static float GameTimerMeetingTime = 0f;

    public static List<string> ReportHistory =
        new List<string>();

    private static readonly List<string>
        LastSavedSummaryMessages =
            new List<string>();

    public static string ImpostorName = "";
    public static string TaskWinnerName = "";
    public static string JesterName = "";

    private static float MatchStartTime = 0f;
    private static float MatchEndTime = 0f;
    private static bool MatchTimerRunning = false;

    public static void StartMatchTimer()
    {
        MatchStartTime =
            UnityEngine.Time.realtimeSinceStartup;

        MatchEndTime = 0f;
        MatchTimerRunning = true;
    }

    public static void StopMatchTimer()
    {
        CaptureGameTimer();

        if (!MatchTimerRunning)
            return;

        MatchEndTime =
            UnityEngine.Time.realtimeSinceStartup;

        MatchTimerRunning = false;
    }

    public static string GetMatchTime()
    {
        float totalTime =
            MatchTimerRunning
                ? UnityEngine.Time.realtimeSinceStartup -
                  MatchStartTime
                : MatchEndTime -
                  MatchStartTime;

        totalTime =
            UnityEngine.Mathf.Max(
                0f,
                totalTime
            );

        int minutes =
            (int)(totalTime / 60f);

        int seconds =
            (int)(totalTime % 60f);

        return $"{minutes:D2}:{seconds:D2}";
    }
    public enum SnsHnsEndReason
    {
        None,
        ImpostorsDead,
        ImpostorsWin,
        TasksCompleted,
        TimerExpired,
        ImpostorsDisconnected
    }

    public static SnsHnsEndReason SnsHnsReason =
        SnsHnsEndReason.None;
    public static void Reset()
    {
        ZombieWin = false;
        ZombieCrewmateWin = false;

        HotPotatoWin = false;
        HotPotatoWinnerName = "";

        ImpostorWin = false;
        CrewmateWin = false;
        JesterWin = false;
        TaskWin = false;

        ImpostorName = "";
        TaskWinnerName = "";
        JesterName = "";

        ReportHistory.Clear();

        MatchStartTime = 0f;
        MatchEndTime = 0f;
        MatchTimerRunning = false;

        GameTimerWasEnabled = false;
        GameEndedByTimer = false;

        GameTimerWinner =
            GameTimeLimit.TimerWinner.None;

        GameTimerPausedDuringMeetings = false;

        GameTimerTotal = 0f;
        GameTimerElapsed = 0f;
        GameTimerRemaining = 0f;
        GameTimerMeetingTime = 0f;

        SnsHnsReason = SnsHnsEndReason.None;
    }

    public static void CaptureGameTimer()
    {
        if (!Options.EnableGameTimer.GetBool())
            return;

        if (GameTimeLimit.TotalTime <= 0f)
            return;

        GameTimerWasEnabled = true;

        GameEndedByTimer =
            GameEndedByTimer ||
            GameTimeLimit.EndedByTimer;

        if (GameTimeLimit.WinnerByTimer !=
            GameTimeLimit.TimerWinner.None)
        {
            GameTimerWinner =
                GameTimeLimit.WinnerByTimer;
        }

        GameTimerPausedDuringMeetings =
            Options
                .PauseGameTimerDuringMeetings
                .GetBool();

        GameTimerTotal =
            GameTimeLimit.TotalTime;

        GameTimerElapsed =
            GameTimeLimit.GetElapsedTime();

        GameTimerRemaining =
            GameTimeLimit.RemainingTime;

        GameTimerMeetingTime =
            GameTimeLimit.GetMeetingTime();
    }

    private static string BuildBaseSummaryReport()
    {
        var report =
            new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(
                ImmortalManager
                    .LastImmortalPlayerName))
        {
            report.AppendLine(
                string.Format(
                    GetString(
                        "ImmortalPlayerReport"
                    ),
                    ImmortalManager
                        .LastImmortalPlayerName
                )
            );
        }
        if (SnsHnsReason != SnsHnsEndReason.None)
        {
            if (SnsHnsReason == SnsHnsEndReason.ImpostorsWin)
            {
                report.AppendLine(GetString("ImpostorWins"));
            }
            else
            {
                report.AppendLine(GetString("CrewmateWins"));

                switch (SnsHnsReason)
                {
                    case SnsHnsEndReason.ImpostorsDead:
                        report.AppendLine(
                            GetString("SnsHnsImpostorsDead"));
                        break;

                    case SnsHnsEndReason.TasksCompleted:
                        report.AppendLine(
                            GetString("SnsHnsTasksCompleted"));
                        break;

                    case SnsHnsEndReason.TimerExpired:
                        report.AppendLine(
                            GetString("SnsHnsTimerExpired"));
                        break;

                    case SnsHnsEndReason.ImpostorsDisconnected:
                        report.AppendLine(
                            GetString("SnsHnsImpostorsDisconnected"));
                        break;
                }
            }

            AppendImpostors(report);
        }
        else if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek && !Options.EnableFFA.GetBool())
        {
            if (CrewmateWin)
            {
                report.AppendLine(
                    GetString(
                        "CrewmateWins"
                    )
                );

                AppendImpostors(report);
            }
            else if (ImpostorWin)
            {
                {
                    report.AppendLine(
                        GetString(
                            "ImpostorWins"
                        )
                    );
                }

                AppendImpostors(report);
            }
        }
        else if (Options.GameMode.Selected ==
            GameModeType.RoomRush)
        {
            report.AppendLine(
                $"{GetString("WinnerIs")}: " +
                RoomRush.WinnerName
                );
        }
        else if (Options.GameMode.Selected ==
            GameModeType.TargetRush)
        {
            report.AppendLine(
                $"{GetString("WinnerIs")}: " +
                TargetRush.WinnerName
                );
        }
        else if (Options.GameMode.Selected ==
            GameModeType.DeathRun)
        {
            report.AppendLine(
                $"{GetString("WinnerIs")}: " +
                DeathRun.WinnerName
                );
        }
        else if (
            ZombieWin &&
            Options.GameMode.Selected ==
            GameModeType.ZombieMode)
        {
            report.AppendLine(
                GetString("ZombieWins")
            );

            AppendTaskProgress(report);
        }
        else if (
            ZombieCrewmateWin &&
            Options.GameMode.Selected ==
            GameModeType.ZombieMode)
        {
            report.AppendLine(
                GetString("CrewmateWins")
            );

            AppendTaskProgress(report);
        }
        else if (TaskWin)
        {
            string winnerName =
                TaskManager.WinnerName;

            float winnerTime =
                TaskManager.WinnerTotalTime;

            report.AppendLine(
                GetString("TaskWins")
            );

            report.AppendLine(
                $"{GetString("WinnerIs")}: " +
                $"{winnerName}"
            );

            int minutes =
                (int)(winnerTime / 60f);

            int seconds =
                (int)(winnerTime % 60f);

            string timeFormatted =
                $"{minutes:D2}:{seconds:D2}";

            report.AppendLine(
                $"{GetString("ScientistPlayerDiedTime")}: " +
                $"{ToFullWidthNumbers(timeFormatted)}"
            );
        }
        else if (JesterWin)
        {
            string jesterName =
                PreviousMatchPopupTracker
                    .JesterName;

            report.AppendLine(
                $"{GetString("JesterWins")}: " +
                $"{jesterName}"
            );

            AppendTaskProgress(report);
        }
        else if (ImpostorWin)
        {
            if (GameEndedByTimer &&
                GameTimerWinner ==
                GameTimeLimit
                    .TimerWinner
                    .Impostors)
            {
                report.AppendLine(
                    GetString(
                        "ImpostorWinsTimer"
                    )
                );
            }
            else
            {
                report.AppendLine(
                    GetString(
                        "ImpostorWins"
                    )
                );
            }

            AppendImpostors(report);
            AppendTaskProgress(report);
        }
        else if (CrewmateWin)
        {
            report.AppendLine(
                GetString(
                    "CrewmateWins"
                )
            );

            AppendImpostors(report);
            AppendTaskProgress(report);
        }

        return report
            .ToString()
            .TrimEnd();
    }

    private static void AppendImpostors(
        System.Text.StringBuilder report)
    {
        var impostors =
            ImpostorTracker
                .GetImpostors();

        //if (impostors.Count == 0)
        //{
        //    report.AppendLine(
        //        GetString(
        //            "NoImpostorsPresent"
        //        )
        //    );

        //    return;
        //}

        if (impostors.Count == 0)
        {
            return;
        }

        foreach (var imp in impostors)
        {
            int kills =
                KillTracker.GetKills(
                    imp.PlayerId
                );

            report.AppendLine(
                $"{imp.PlayerName}: " +
                $"{ToFullWidthNumbers(kills.ToString())} kill"
            );
        }
    }

    private static void AppendTaskProgress(
        System.Text.StringBuilder report)
    {
        int totalDone = 0;
        int totalTasks = 0;

        foreach (
            var data in
            TaskTracker.GetAllTaskData())
        {
            totalDone += data.Done;
            totalTasks += data.Total;
        }

        report.AppendLine(
            $"Task: " +
            $"{ToFullWidthNumbers(totalDone.ToString())} / " +
            $"{ToFullWidthNumbers(totalTasks.ToString())}"
        );
    }

    private static string BuildTimerMessage()
    {
        string gameTime =
            ToFullWidthNumbers(
                GameTimeLimit.FormatTime(
                    GameTimerElapsed
                )
            );

        if (!GameTimerPausedDuringMeetings)
        {
            return
                $"Game Time: {gameTime}";
        }

        string configuredTimer =
            ToFullWidthNumbers(
                GameTimeLimit.FormatTime(
                    GameTimerTotal
                )
            );

        string meetingTime =
            ToFullWidthNumbers(
                GameTimeLimit.FormatTime(
                    GameTimerMeetingTime
                )
            );

        float totalMatchSeconds =
            GameTimerElapsed +
            GameTimerMeetingTime;

        string totalMatchTime =
            ToFullWidthNumbers(
                GameTimeLimit.FormatTime(
                    totalMatchSeconds
                )
            );

        return
            $"Timer: {configuredTimer}\n" +
            $"Game Time: {gameTime}\n" +
            $"Meeting Time: {meetingTime}\n" +
            $"Total Match Time: {totalMatchTime}";
    }

    private static List<string> SplitMessage(
        string message,
        int maxLength = 120)
    {
        var result =
            new List<string>();

        if (string.IsNullOrWhiteSpace(
                message))
        {
            return result;
        }

        string normalized =
            message
                .Replace(
                    "\r\n",
                    "\n"
                )
                .Replace(
                    "\r",
                    "\n"
                )
                .Trim();

        string[] lines =
            normalized.Split('\n');

        var current =
            new System.Text
                .StringBuilder();

        foreach (
            string rawLine in lines)
        {
            string line =
                rawLine.TrimEnd();

            if (line.Length >
                maxLength)
            {
                if (current.Length > 0)
                {
                    result.Add(
                        current.ToString()
                    );

                    current.Clear();
                }

                int index = 0;

                while (
                    index <
                    line.Length)
                {
                    int length =
                        System.Math.Min(
                            maxLength,
                            line.Length -
                            index
                        );

                    result.Add(
                        line.Substring(
                            index,
                            length
                        )
                    );

                    index += length;
                }

                continue;
            }

            int additionalLength =
                line.Length +
                (
                    current.Length > 0
                        ? 1
                        : 0
                );

            if (
                current.Length +
                additionalLength >
                maxLength)
            {
                if (current.Length > 0)
                {
                    result.Add(
                        current.ToString()
                    );

                    current.Clear();
                }
            }

            if (current.Length > 0)
            {
                current.Append('\n');
            }

            current.Append(line);
        }

        if (current.Length > 0)
        {
            result.Add(
                current.ToString()
            );
        }

        return result;
    }

    public static List<string>
        GetSummaryMessages()
    {
        CaptureGameTimer();

        string resultMessage =
            BuildBaseSummaryReport();

        string completeMessage;

        if (!GameTimerWasEnabled)
        {
            string matchTime =
                ToFullWidthNumbers(
                    GetMatchTime()
                );

            completeMessage =
                $"{resultMessage}\n" +
                $"Match Time: {matchTime}";
        }
        else
        {
            string timerMessage =
                BuildTimerMessage();

            completeMessage =
                $"{resultMessage}\n" +
                $"{timerMessage}";
        }

        return SplitMessage(
            completeMessage,
            120
        );
    }

    public static string GetSummaryReport()
    {
        return string.Join(
            "\n\n",
            GetSummaryMessages()
        );
    }

    public static void SaveToHistory()
    {
        List<string> messages =
            GetSummaryMessages();

        LastSavedSummaryMessages
            .Clear();

        foreach (
            string message in messages)
        {
            if (string.IsNullOrWhiteSpace(
                    message))
            {
                continue;
            }

            ReportHistory.Add(
                message
            );

            LastSavedSummaryMessages
                .Add(
                    message
                );
        }
    }

    public static string GetLastSavedReport()
    {
        return string.Join(
            "\n\n",
            LastSavedSummaryMessages
        );
    }

    public static List<string>
        GetLastSavedReports()
    {
        return new List<string>(
            LastSavedSummaryMessages
        );
    }
}
public static class TaskTracker
{
    public class TaskData
    {
        public byte PlayerId;
        public string Name;
        public int Done;
        public int Total;
    }

    public static Dictionary<byte, TaskData> TaskState = new Dictionary<byte, TaskData>();

    public static void UpdatePlayerTask(PlayerControl player)
    {
        GameModeType gameMode = Options.GameMode.Selected;

        if (player?.Data == null || player.Data.Tasks == null)
            return;
        if (player.Data.Role?.TeamType == RoleTeamTypes.Impostor)
            return;
        if (gameMode == GameModeType.FFA) return;
        if (gameMode == GameModeType.HotPotatoModded) return;
        int total = player.Data.Tasks.Count;
        int done = 0;
        foreach (var task in player.Data.Tasks)
        {
            if (task != null && task.Complete)
                done++;
        }

        TaskState[player.PlayerId] = new TaskData
        {
            PlayerId = player.PlayerId,
            Name = player.Data.PlayerName,
            Done = done,
            Total = total
        };
        PreviousMatchPopupTracker.UpdatePlayerTask(player);
    }

    public static void Clear() => TaskState.Clear();

    public static List<TaskData> GetAllTaskData() => TaskState.Values.ToList();
}
public static class ImpostorTracker
{
    public class ImpostorData
    {
        public byte PlayerId;
        public string PlayerName;
    }

    private static readonly List<ImpostorData> impostors = new();
    private static readonly List<ImpostorData> snsInitialImpostors = new();

    private static bool snsSnapshotCaptured;

    public static void ResetSnsSnapshot()
    {
        snsInitialImpostors.Clear();
        snsSnapshotCaptured = false;
    }

    public static void CaptureSnsSnapshot()
    {
        if (snsSnapshotCaptured)
            return;

        DetectImpostors();

        foreach (var imp in impostors)
        {
            snsInitialImpostors.Add(new ImpostorData
            {
                PlayerId = imp.PlayerId,
                PlayerName = imp.PlayerName
            });
        }

        snsSnapshotCaptured = true;
    }
    public static void DetectImpostors()
    {
        impostors.Clear();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player?.Data?.Role?.TeamType != RoleTeamTypes.Impostor)
                continue;

            impostors.Add(new ImpostorData
            {
                PlayerId = player.PlayerId,
                PlayerName = player.Data.PlayerName
            });
        }
    }
    public static List<ImpostorData> GetImpostors()
    {
        return snsSnapshotCaptured
            ? new List<ImpostorData>(snsInitialImpostors)
            : new List<ImpostorData>(impostors);
    }

    public static void Clear() => impostors.Clear();

    public static bool IsImpostor(byte playerId) => impostors.Any(i => i.PlayerId == playerId);
}
public static class KillTracker
{
    private static readonly Dictionary<byte, int> kills = new();

    public static void RegisterKill(byte killerId)
    {

        if (!kills.ContainsKey(killerId))
            kills[killerId] = 0;

        kills[killerId]++;
    }

    public static int GetKills(byte playerId)
    {
        return kills.TryGetValue(playerId, out var count) ? count : 0;
    }

    public static void Clear() => kills.Clear();
}

public class SpawnProtectionChecker : MonoBehaviour
{
    private SpawnZoneManager zoneManager;
    public MapNames currentMap;

    private void Start()
    {
        zoneManager = new SpawnZoneManager();

        if (ShipStatus.Instance == null)
        {
            currentMap = (MapNames)(-1);
            return;
        }

        currentMap = GetCurrentMapFromOptions();
    }

    private void Update()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
        GameModeType gameMode = Options.GameMode.Selected;
        if (gameMode == GameModeType.KaitoRun)
        {
            if (currentMap == (MapNames)(-1) && ShipStatus.Instance != null)
            {
                currentMap = GetCurrentMapFromOptions();
            }

            if (Time.time > Block.ShieldEndTime) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.IsDead) continue;

                Vector2 pos = player.GetTruePosition();
                bool isInSpawnZone = zoneManager.IsPlayerInAnyZone(pos, currentMap);
                bool isProtected = BanMod.ShieldedPlayers.Contains(player.PlayerId);

                if (isInSpawnZone && !isProtected && !GameStates.isLobby)
                {
                    BanMod.ShieldedPlayers.Add(player.PlayerId);
                }
                else if (!isInSpawnZone && isProtected)
                {
                    BanMod.ShieldedPlayers.Remove(player.PlayerId);
                    player.RemoveProtection();
                    player.protectedByGuardianId = -1;
                    player.Data.MarkDirty();
                    AFKDetector.EnsureTrackedPlayers();
                }
            }
        }
    }

}
public class SpawnProtectionChecker1 : MonoBehaviour
{
    private SpawnZoneManager zoneManager;
    public MapNames currentMap;

    private void Start()
    {
        zoneManager = new SpawnZoneManager();

        if (ShipStatus.Instance == null)
        {
            currentMap = (MapNames)(-1);
            return;
        }

        currentMap = GetCurrentMapFromOptions();
    }

    private void Update()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
        if (!Options.Protection10Sec.GetBool()) return;
        if (Options.Protection10Sec.GetBool())
        {
            if (currentMap == (MapNames)(-1) && ShipStatus.Instance != null)
            {
                currentMap = GetCurrentMapFromOptions();
            }

            if (Time.time > Block.ShieldEndTime) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.IsDead) continue;

                Vector2 pos = player.GetTruePosition();
                bool isInSpawnZone = zoneManager.IsPlayerInAnyZone(pos, currentMap);
                bool isProtected = BanMod.ShieldedPlayers.Contains(player.PlayerId);
                bool hasLostProtection = Block.PlayersLostProtection.Contains(player.PlayerId);

                if (isInSpawnZone && !isProtected && !hasLostProtection)
                {
                    if (Block.InitialProtectedPlayers.Contains(player.PlayerId) && !GameStates.isLobby)
                    {
                        BanMod.ShieldedPlayers.Add(player.PlayerId);
                    }
                }
                else if (!isInSpawnZone && isProtected)
                {
                    BanMod.ShieldedPlayers.Remove(player.PlayerId);
                    player.RemoveProtection();
                    player.protectedByGuardianId = -1;
                    player.Data.MarkDirty();
                    Block.PlayersLostProtection.Add(player.PlayerId);

                    AFKDetector.EnsureTrackedPlayers();
                }
            }
        }
    }
}

public static class BMImage
{
    private static readonly Dictionary<string, Sprite> CachedSprites = new Dictionary<string, Sprite>();

    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            string key = path + "|" + pixelsPerUnit;
            if (CachedSprites.TryGetValue(key, out Sprite sprite)) return sprite;

            Texture2D texture = LoadTextureFromResources(path);
            if (texture == null)
            {
                BMLogger.Error("Texture is null: " + path, "LoadImage");
                return null;
            }

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );

            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            CachedSprites[key] = sprite;
            return sprite;
        }
        catch (Exception ex)
        {
            BMLogger.Error("Error loading texture from: " + path + " | " + ex, "LoadImage");
        }

        return null;
    }

    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = OpenEmbeddedResourceFlexible(assembly, path);

            if (stream == null)
            {
                BMLogger.Error("Embedded resource not found: " + path, "LoadImage");
                LogAvailableResources(assembly);
                return null;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                if (!texture.LoadImage(ms.ToArray(), false))
                {
                    BMLogger.Error("LoadImage failed for resource: " + path, "LoadImage");
                    return null;
                }
            }

            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return texture;
        }
        catch (Exception ex)
        {
            BMLogger.Error("读入Texture失败：" + path + " | " + ex, "LoadImage");
        }

        return null;
    }

    private static Stream OpenEmbeddedResourceFlexible(Assembly assembly, string path)
    {
        if (assembly == null || string.IsNullOrEmpty(path))
            return null;

        Stream exact = assembly.GetManifestResourceStream(path);
        if (exact != null)
            return exact;

        string normalized = NormalizeResourcePath(path);
        string fileName = Path.GetFileName(path.Replace('\\', '/'));

        string[] resources = assembly.GetManifestResourceNames();

        for (int i = 0; i < resources.Length; i++)
        {
            string resource = resources[i];
            string normalizedResource = NormalizeResourcePath(resource);

            if (normalizedResource.EndsWith(normalized, StringComparison.OrdinalIgnoreCase))
                return assembly.GetManifestResourceStream(resource);
        }

        string resourcesSuffix = normalized;
        int idx = normalized.IndexOf("Resources.", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            resourcesSuffix = normalized.Substring(idx);

        for (int i = 0; i < resources.Length; i++)
        {
            string resource = resources[i];
            string normalizedResource = NormalizeResourcePath(resource);

            if (normalizedResource.EndsWith(resourcesSuffix, StringComparison.OrdinalIgnoreCase))
                return assembly.GetManifestResourceStream(resource);
        }

        for (int i = 0; i < resources.Length; i++)
        {
            string resource = resources[i];

            if (resource.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase) ||
                resource.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                return assembly.GetManifestResourceStream(resource);
        }

        return null;
    }

    private static string NormalizeResourcePath(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\\", ".")
            .Replace("/", ".")
            .Replace("..", ".")
            .Trim('.');
    }

    private static void LogAvailableResources(Assembly assembly)
    {
        try
        {
            string[] names = assembly.GetManifestResourceNames();

            for (int i = 0; i < names.Length && i < 40; i++)
            {
                BMLogger.LogDebug("[LoadImage] Embedded resource: " + names[i]);
            }

            if (names.Length > 40)
            {
                BMLogger.LogDebug("[LoadImage] Showing first 40 of " + names.Length + " embedded resources.");
            }
        }
        catch (Exception ex)
        {
            BMLogger.Error("[LoadImage] Could not list embedded resources: " + ex);
        }
    }

    public static Sprite LoadExternalSprite(string filePath, float pixelsPerUnit = 1f)
    {
        try
        {
            string key = filePath + "|" + pixelsPerUnit;
            if (CachedSprites.TryGetValue(key, out Sprite sprite)) return sprite;

            Texture2D texture = LoadExternalTexture(filePath);
            if (texture == null)
                return null;

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );

            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            CachedSprites[key] = sprite;
            return sprite;
        }
        catch (Exception ex)
        {
            BMLogger.Error("Failed to load external sprite: " + ex);
        }

        return null;
    }

    public static Texture2D LoadExternalTexture(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            if (!texture.LoadImage(fileData))
            {
                BMLogger.Error("LoadImage failed");
                return null;
            }

            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return texture;
        }
        catch (Exception ex)
        {
            BMLogger.Error("Failed to load external texture: " + ex);
        }

        return null;
    }

    public static class PrivateNameUtility
    {
        public enum NameVisibility
        {
            Private,
            Everyone
        }

        private static readonly Dictionary<byte, Coroutine> ActiveTimers = new();

        public static void SetText(
            PlayerControl player,
            string text,
            NameVisibility visibility = NameVisibility.Private)
        {
            if (!IsValid(player))
                return;

            StopTimer(player, false);
            SendName(player, text, visibility);
        }

        public static void StartTimer(
            PlayerControl player,
            int seconds,
            string format = "{0}s",
            NameVisibility visibility = NameVisibility.Private)
        {
            if (!IsValid(player))
                return;

            if (seconds < 0)
                seconds = 0;

            StopTimer(player, false);

            Coroutine coroutine = player.StartCoroutine(
                TimerCoroutine(
                    player,
                    seconds,
                    format,
                    visibility
                )
            );

            ActiveTimers[player.PlayerId] = coroutine;
        }

        public static void StopTimer(
            PlayerControl player,
            bool restoreName = true,
            NameVisibility visibility = NameVisibility.Private)
        {
            if (player == null)
                return;

            if (ActiveTimers.TryGetValue(
                player.PlayerId,
                out Coroutine coroutine))
            {
                if (coroutine != null)
                    player.StopCoroutine(coroutine);

                ActiveTimers.Remove(player.PlayerId);
            }

            if (restoreName && IsValid(player))
            {
                SendName(
                    player,
                    player.Data.PlayerName,
                    visibility
                );
            }
        }

        public static void Reset(
            PlayerControl player,
            NameVisibility visibility = NameVisibility.Private)
        {
            if (!IsValid(player))
                return;

            StopTimer(player, false);

            SendName(
                player,
                player.Data.PlayerName,
                visibility
            );
        }

        private static IEnumerator TimerCoroutine(
            PlayerControl player,
            int seconds,
            string format,
            NameVisibility visibility)
        {
            for (int remaining = seconds; remaining >= 0; remaining--)
            {
                if (!IsValid(player))
                    break;

                string text;

                try
                {
                    text = string.Format(format, remaining);
                }
                catch
                {
                    text = remaining.ToString();
                }

                SendName(
                    player,
                    text,
                    visibility
                );

                if (remaining > 0)
                    yield return new WaitForSecondsRealtime(1f);
            }

            if (player != null)
                ActiveTimers.Remove(player.PlayerId);
        }

        private static void SendName(
            PlayerControl player,
            string text,
            NameVisibility visibility)
        {
            if (!IsValid(player))
                return;

            AmongUsClient client = AmongUsClient.Instance;

            int targetClientId;

            switch (visibility)
            {
                case NameVisibility.Private:
                    targetClientId =
                        client.GetClientIdFromCharacter(player);

                    if (targetClientId < 0)
                        return;

                    break;

                case NameVisibility.Everyone:
                    targetClientId = -1;
                    break;

                default:
                    return;
            }

            MessageWriter writer = client.StartRpcImmediately(
                player.NetId,
                (byte)RpcCalls.SetName,
                SendOption.Reliable,
                targetClientId
            );

            writer.Write(player.Data.NetId);
            writer.Write(text ?? string.Empty);
            writer.Write(false);

            client.FinishRpcImmediately(writer);
        }

        private static bool IsValid(PlayerControl player)
        {
            return player != null
                && player.Data != null
                && AmongUsClient.Instance != null
                && AmongUsClient.Instance.AmHost;
        }
    }
    public static class HostGameTools
    {
        public const float FreezeSyncInterval = 0.08f;

        private sealed class FrozenPlayer
        {
            public Vector2 Position;
            public float NextSync;

            public FrozenPlayer(Vector2 position)
            {
                Position = position;
                NextSync = 0f;
            }
        }

        private static readonly Dictionary<byte, FrozenPlayer> FrozenPlayers = new();

        public static bool IsHost =>
            AmongUsClient.Instance != null &&
            AmongUsClient.Instance.AmHost;

        private static bool IsValid(PlayerControl player)
        {
            return IsHost &&
                   player != null &&
                   player.Data != null &&
                   !player.Data.Disconnected;
        }

        public static bool FreezePlayer(PlayerControl player)
        {
            if (!IsValid(player))
                return false;

            byte id = player.PlayerId;

            Vector2 position = player.GetTruePosition();

            if (FrozenPlayers.TryGetValue(id, out FrozenPlayer frozen))
            {
                frozen.Position = position;
                frozen.NextSync = 0f;

                return true;
            }

            FrozenPlayers[id] = new FrozenPlayer(position);

            return true;
        }

        public static bool UnfreezePlayer(PlayerControl player)
        {
            if (player == null)
                return false;

            return FrozenPlayers.Remove(player.PlayerId);
        }

        public static bool IsFrozen(PlayerControl player)
        {
            if (player == null)
                return false;

            return FrozenPlayers.ContainsKey(
                player.PlayerId
            );
        }

        public static void UpdateFrozenPlayers()
        {
            if (!IsHost)
                return;

            if (FrozenPlayers.Count == 0)
                return;

            float now = Time.time;

            List<byte> invalidPlayers = null;

            foreach (
                KeyValuePair<byte, FrozenPlayer> pair
                in FrozenPlayers
            )
            {
                byte playerId = pair.Key;
                FrozenPlayer frozen = pair.Value;

                PlayerControl player =
                    FindPlayerById(playerId);

                if (!IsValid(player))
                {
                    invalidPlayers ??= new List<byte>();
                    invalidPlayers.Add(playerId);

                    continue;
                }

                if (now < frozen.NextSync)
                    continue;

                frozen.NextSync =
                    now + FreezeSyncInterval;

                if (player.NetTransform != null)
                {
                    player.NetTransform.RpcSnapTo(
                        frozen.Position
                    );
                }
            }

            if (invalidPlayers != null)
            {
                foreach (byte id in invalidPlayers)
                    FrozenPlayers.Remove(id);
            }
        }
        public static PlayerControl FindPlayerById(
            byte playerId)
        {
            foreach (
                PlayerControl player
                in PlayerControl.AllPlayerControls
            )
            {
                if (player == null)
                    continue;

                if (player.PlayerId == playerId)
                    return player;
            }

            return null;
        }
    }

    public sealed class HostGameToolsBehaviour : MonoBehaviour
    {
        public static HostGameToolsBehaviour Instance
        {
            get;
            private set;
        }

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            HostGameTools.UpdateFrozenPlayers();
        }
    }
}


