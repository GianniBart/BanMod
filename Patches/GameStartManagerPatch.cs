//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppSystem;
using InnerNet;
using Rewired.Utils.Platforms.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;


namespace BanMod;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        {
            __instance.MinPlayers = 1;
        }
    }
}

public static class GameStartManagerPatch
{

    public static float LastJoinLeaveTime = 0f;
    public static float ManualStopTime = 0f;

    public static float Timer { get; set; } = 600f;
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static TextMeshPro HideName;
        public static TextMeshPro GameCountdown;
        public static Vector3 StartButtonTextDefaultPosition;
        public static bool HasStartButtonTextDefaultPosition;

        public static void Postfix(GameStartManager __instance)
        {
            {
                if (__instance == null) return;

                var temp = __instance.PlayerCounter;
                GameCountdown = Object.Instantiate(temp, __instance.StartButton.transform);
                GameCountdown.text = string.Empty;

                if (__instance.StartButton.buttonText != null)
                {
                    StartButtonTextDefaultPosition = __instance.StartButton.buttonText.transform.localPosition;
                    HasStartButtonTextDefaultPosition = true;
                }


                if (AmongUsClient.Instance.AmHost)
                {
                    __instance.GameStartTextParent.GetComponent<SpriteRenderer>().sprite = null;
                    __instance.StartButton.ChangeButtonText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.StartLabel));
                    __instance.GameStartText.transform.localPosition = new(__instance.GameStartText.transform.localPosition.x, 2f, __instance.GameStartText.transform.localPosition.z);
                    __instance.StartButton.activeTextColor = __instance.StartButton.inactiveTextColor = Color.white;

                    __instance.EditButton.activeTextColor = __instance.EditButton.inactiveTextColor = Color.black;
                    __instance.EditButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0f, 0.647f, 1f, 1f);
                    __instance.EditButton.activeSprites.GetComponent<SpriteRenderer>().color = new(0f, 0.847f, 1f, 1f);
                    __instance.EditButton.inactiveSprites.transform.Find("Shine").GetComponent<SpriteRenderer>().color = new(0f, 1f, 1f, 0.5f);

                    __instance.HostViewButton.activeTextColor = __instance.HostViewButton.inactiveTextColor = Color.black;
                    __instance.HostViewButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0f, 0.647f, 1f, 1f);
                    __instance.HostViewButton.activeSprites.GetComponent<SpriteRenderer>().color = new(0f, 0.847f, 1f, 1f);
                    __instance.HostViewButton.inactiveSprites.transform.Find("Shine").GetComponent<SpriteRenderer>().color = new(0f, 1f, 1f, 0.5f);
                }

                if (AmongUsClient.Instance == null || AmongUsClient.Instance.IsGameStarted || GameStates.InGame || __instance.startState == GameStartManager.StartingStates.Starting) return;

                Timer = 600f;

                if (!AmongUsClient.Instance.AmHost) return;
            }
        }

    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static int lastFlashSecond = -1;
        public static bool Prefix(GameStartManager __instance)
        {
            if (AmongUsClient.Instance.AmHost)
                VanillaUpdate(__instance);
            if (AmongUsClient.Instance == null || GameData.Instance == null || !AmongUsClient.Instance.AmHost || !GameData.Instance) return true;
            return false;
        }

        private static void VanillaUpdate(GameStartManager instance)
        {
            if (!GameData.Instance || !GameManager.Instance) return;

            try
            {
                instance.UpdateMapImage((MapNames)GameManager.Instance.LogicOptions.MapId);
            }
            catch (System.Exception)
            {
            }

            instance.CheckSettingsDiffs();
            instance.StartButton.gameObject.SetActive(true);
            instance.RulesPresetText.text = DestroyableSingleton<TranslationController>.Instance.GetString(GameOptionsManager.Instance.CurrentGameOptions.GetRulesPresetTitle());
            if (GameCode.IntToGameName(AmongUsClient.Instance.GameId) == null) instance.privatePublicPanelText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LocalButton);
            else if (AmongUsClient.Instance.IsGamePublic) instance.privatePublicPanelText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PublicHeader);
            else instance.privatePublicPanelText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PrivateHeader);
            instance.HostPrivateButton.gameObject.SetActive(!AmongUsClient.Instance.IsGamePublic);
            instance.HostPublicButton.gameObject.SetActive(AmongUsClient.Instance.IsGamePublic);
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
                ClipboardHelper.PutClipboardString(GameCode.IntToGameName(AmongUsClient.Instance.GameId));
            if (GameData.Instance.PlayerCount != instance.LastPlayerCount)
            {
                instance.LastPlayerCount = GameData.Instance.PlayerCount;
                string text = "<color=#FF0000FF>";
                if (instance.LastPlayerCount > instance.MinPlayers) text = "<color=#00FF00FF>";
                if (instance.LastPlayerCount == instance.MinPlayers) text = "<color=#FFFF00FF>";
                instance.PlayerCounter.text = $"{text}{instance.LastPlayerCount}/{(AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ? 15 : GameManager.Instance.LogicOptions.MaxPlayers)}";
                instance.StartButton.SetButtonEnableState(instance.LastPlayerCount >= instance.MinPlayers);
                ActionMapGlyphDisplay startButtonGlyph = instance.StartButtonGlyph;
                startButtonGlyph?.SetColor((instance.LastPlayerCount >= instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                if (DestroyableSingleton<DiscordManager>.InstanceExists)
                {
                    if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
                        DestroyableSingleton<DiscordManager>.Instance.SetInLobbyHost(instance.LastPlayerCount, GameManager.Instance.LogicOptions.MaxPlayers, AmongUsClient.Instance.GameId);
                    else DestroyableSingleton<DiscordManager>.Instance.SetInLobbyClient(instance.LastPlayerCount, GameManager.Instance.LogicOptions.MaxPlayers, AmongUsClient.Instance.GameId);
                }
            }

            if (AmongUsClient.Instance.AmHost)
            {
                if (instance.startState == GameStartManager.StartingStates.Countdown)
                {
                    if (GameStartManagerStartPatch.HasStartButtonTextDefaultPosition &&
                        instance.StartButton.buttonText != null)
                    {
                        instance.StartButton.buttonText.transform.localPosition =
                            GameStartManagerStartPatch.StartButtonTextDefaultPosition;
                    }

                    instance.StartButton.ChangeButtonText(string.Format("STOP"));
                    instance.StartButton.DestroyTranslator();
                    instance.StartButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0.8f, 0f, 0f, 1f);
                    instance.StartButton.activeSprites.GetComponent<SpriteRenderer>().color = Color.red;
                    instance.StartButton.inactiveSprites.transform.Find("Shine").GetComponent<SpriteRenderer>().color = new(0.8f, 0.4f, 0.4f, 1f);
                    instance.StartButton.activeTextColor = instance.StartButton.inactiveTextColor = Color.white;
                    int num = Mathf.CeilToInt(instance.countDownTimer);
                    instance.countDownTimer -= Time.deltaTime;
                    int num2 = Mathf.CeilToInt(instance.countDownTimer);
                    if (!instance.GameStartTextParent.activeSelf) SoundManager.Instance.PlaySound(instance.gameStartSound, false);
                    instance.GameStartTextParent.SetActive(true);
                    instance.GameStartText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameStarting, num2);
                    if (num != num2) PlayerControl.LocalPlayer.RpcSetStartCounter(num2);
                    if (num2 <= 0) instance.FinallyBegin();
                }
                else
                {
                    string startLabel = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.StartLabel);
                    string currentGameMode = Options.GameMode != null
                        ? Options.GameMode.GetString()
                        : string.Empty;

                    bool showGameMode =
                        Options.GameMode != null &&
                        !Options.GameMode.GetValue(GameModeType.Default);

                    string gameModeLine = showGameMode
                        ? $"\n<size=65%><color=#00FFFFFF>GameMode: <b>{currentGameMode}</b></color></size>"
                        : string.Empty;

                    if (GameStartManagerStartPatch.HasStartButtonTextDefaultPosition &&
                        instance.StartButton.buttonText != null)
                    {
                        instance.StartButton.buttonText.transform.localPosition =
                            GameStartManagerStartPatch.StartButtonTextDefaultPosition +
                            (showGameMode ? new Vector3(0f, 0.14f, 0f) : Vector3.zero);
                    }

                    instance.StartButton.ChangeButtonText(
                        $"{startLabel}{gameModeLine}"
                    );
                    instance.StartButton.DestroyTranslator();
                    instance.StartButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0.1f, 0.1f, 0.1f, 1f);
                    instance.StartButton.activeSprites.GetComponent<SpriteRenderer>().color = new(0.2f, 0.2f, 0.2f, 1f);
                    instance.StartButton.inactiveSprites.transform.Find("Shine").GetComponent<SpriteRenderer>().color = new(0.3f, 0.3f, 0.3f, 0.5f);
                    instance.StartButton.activeTextColor = instance.StartButton.inactiveTextColor = Color.white;
                    instance.GameStartTextParent.SetActive(false);
                    instance.GameStartText.text = string.Empty;
                }
            }

            if (instance.LobbyInfoPane.gameObject.activeSelf && DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening) instance.LobbyInfoPane.DeactivatePane();
            instance.LobbyInfoPane.gameObject.SetActive(!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening);
        }
        public static void Postfix(GameStartManager __instance)
        {
            if (AmongUsClient.Instance == null || AmongUsClient.Instance.IsGameStarted || GameStates.InGame || __instance == null || __instance.startState == GameStartManager.StartingStates.Starting)
                return;

            if (AmongUsClient.Instance.AmHost)
            {
                __instance.StartButton.gameObject.SetActive(true);

                GameStartManagerPatch.Timer = Mathf.Max(0f, GameStartManagerPatch.Timer - Time.deltaTime);

                int minutes = (int)GameStartManagerPatch.Timer / 60;
                int seconds = (int)GameStartManagerPatch.Timer % 60;
                string suffix = $"{minutes:00}:{seconds:00}";

                int currentSec = (int)GameStartManagerPatch.Timer;

                if (GameStartManagerPatch.Timer <= 5)
                {
                    suffix = Utils.ColorString(Color.red, suffix);

                    if (currentSec != lastFlashSecond)
                    {
                        lastFlashSecond = currentSec;
                        Color flashCol = new Color(1f, 0f, 0f, 0.8f);
                        Utils.FlashColor(flashCol, 1f);
                    }
                }
                else if (GameStartManagerPatch.Timer <= 30)
                {
                    suffix = Utils.ColorString(currentSec % 2 == 0 ? Color.yellow : Color.red, suffix);

                }
                else if (GameStartManagerPatch.Timer <= 60)
                {
                    suffix = Utils.ColorString(Color.yellow, suffix);

                    if (currentSec != lastFlashSecond)
                    {
                        Color flashCol = new Color(1f, 0f, 0f, 0.8f);
                        Utils.FlashColor(flashCol, 1.5f);
                    }
                }
                else
                {
                    lastFlashSecond = -1;
                }

                TextMeshPro tmp = GameStartManagerStartPatch.GameCountdown;

                if (tmp.text == string.Empty)
                {
                    tmp.name = "LobbyTimer";
                    tmp.fontSize = tmp.fontSizeMin = tmp.fontSizeMax = 5f;
                    tmp.autoSizeTextContainer = true;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.cyan;
                    tmp.outlineColor = Color.black;
                    tmp.outlineWidth = 0.4f;
                    tmp.transform.localPosition += new Vector3(-0.8f, -0.42f, 0f);
                    tmp.transform.localScale = new(0.5f, 0.5f, 1f);
                }

                tmp.text = suffix;
                if (HostAfkManager.IsHostAfk)
                {
                    bool isFull = GameData.Instance.PlayerCount >= GameManager.Instance.LogicOptions.MaxPlayers;
                    bool isClosing = Timer <= 10f;

                    if ((isFull || isClosing) && __instance.startState == GameStartManager.StartingStates.NotStarting && !GameStates.InGame)
                    {
                        __instance.BeginGame();
                        return;
                    }
                }
                if (Options.AutoStart.GetBool())
                {
                    if (__instance.startState == GameStartManager.StartingStates.NotStarting && !GameStates.InGame)
                    {
                        if (GameData.Instance == null || GameManager.Instance == null || GameData.Instance.AllPlayers == null)
                            return;

                        bool lobbyIsFull = GameData.Instance.PlayerCount >= Options.AutoStartCount.GetInt();
                        bool timerReachedThreshold1 = Timer > 0 && Timer < 10;
                        bool recentlyChangedLobby = (Time.realtimeSinceStartup - LastJoinLeaveTime) < 1f;
                        bool recentlyStoppedByHost = (Time.realtimeSinceStartup - ManualStopTime) <= Options.AutoStartDelay.GetInt();
                        float targetTime = Options.AutoStartTime.GetFloat();
                        float tolerance = 1f;
                        bool withinStartWindow = Timer >= (targetTime - tolerance) && Timer <= (targetTime + tolerance);

                        if (!recentlyChangedLobby && !recentlyStoppedByHost &&
                            (lobbyIsFull || withinStartWindow || timerReachedThreshold1))
                        {
                            __instance.StartCoroutine(DelayedSafeBegin(__instance));
                        }

                    }
                }
                else
                {
                    __instance.StartButton.gameObject.SetActive(true);
                }
            }
        }

        public static IEnumerator DelayedSafeBegin(GameStartManager __instance)
        {
            yield return new WaitForSeconds(0.5f);

            if (__instance == null || GameStates.InGame || __instance.startState != GameStartManager.StartingStates.NotStarting)
                yield break;

            try
            {
                __instance.BeginGame();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[AutoStart] Errore nell'avvio automatico: " + ex);
            }
        }
    }
}
[HarmonyPatch(typeof(GameData), nameof(GameData.AddPlayer))]
class PlayerJoinPatch
{
    public static void Prefix()
    {
        GameStartManagerPatch.LastJoinLeaveTime = Time.realtimeSinceStartup;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
public static class PlayerLeavePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        ModdedRegistry.ModdedPlayers.Remove(__instance.PlayerId);
    }
}
[HarmonyPatch(typeof(GameData), nameof(GameData.RemovePlayer))]
class PlayerLeavePatch1
{
    public static void Prefix()
    {
        GameStartManagerPatch.LastJoinLeaveTime = Time.realtimeSinceStartup;
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
class ResetStartStatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        SoundManager.Instance.StopSound(__instance.gameStartSound);

        if (AmongUsClient.Instance.AmHost)
        {
            GameStartManagerPatch.ManualStopTime = Time.realtimeSinceStartup;
        }
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
public class GameStartManagerBeginPatch
{
    public static bool Prefix(GameStartManager __instance)
    {
        VoteBanTracker.Reset();

        if (!AmongUsClient.Instance.AmHost)
            return true;
        FakeMapLobbyUtility.Disable();
        SelectRandomMap();

        if (__instance.startState == GameStartManager.StartingStates.Countdown)
        {
            __instance.ResetStartState();
            return false;
        }
        if (Options.nocountdown.GetBool())
        {
            __instance.countDownTimer = 0f;
        }
        else if (BanMod.instantstart)
        {
            __instance.countDownTimer = 0f;
        }
        else
        {
            __instance.countDownTimer = 5f;
        }
        __instance.startState = GameStartManager.StartingStates.Countdown;
        LobbyDepartureGuard.OnStart(__instance);
        __instance.GameSizePopup.SetActive(false);
        DataManager.Player.Onboarding.AlwaysShowMinPlayerWarning = false;
        DataManager.Player.Onboarding.ViewedMinPlayerWarning = true;
        DataManager.Player.Save();
        __instance.StartButton.gameObject.SetActive(false);
        __instance.StartButtonClient.gameObject.SetActive(false);
        __instance.GameStartTextParent.SetActive(false);

        AmongUsClient.Instance.KickNotJoinedPlayers();
        KillTracker.Clear();

        return false;
    }
    private static void SelectRandomMap()
    {
        if (Options.randomMap.GetBool())
        {
            if (IRandom.Instance == null)
                IRandom.SetInstance(new IRandom.NetRandomWrapper());

            var rand = IRandom.Instance;

            List<byte> randomMaps = new()
            {
                0,
                1,
                2,
                4,
                5
            };

            if (randomMaps.Count == 0)
                return;

            var mapsId = randomMaps[rand.Next(randomMaps.Count)];
            BanMod.NormalOptions.MapId = mapsId;
        }
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class VoteBanTrackerResetLobbyPatch
{
    public static void Prefix()
    {
        VoteBanTracker.Reset();

        BMLogger.LogInfo(
            "[VoteBan] Conteggio Kicks azzerato per la nuova lobby.");
    }
}