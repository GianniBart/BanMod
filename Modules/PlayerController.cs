//credits and licenses in the resources folder
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using AmongUs.QuickChat;
using BanMod;
using BepInEx.Unity.IL2CPP.Utils;
using Epic.OnlineServices.RTC;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BanMod.Utils;
using IntPtr = System.IntPtr;

namespace BanMod
{
    public class PlayerMouseController : MonoBehaviour
    {
        private PlayerControl selectedPlayer;
        private static readonly Dictionary<byte, float> HostScales = new();
        private static readonly Dictionary<byte, byte> HostBodies = new();

        private static bool LocalVisualOverride = false;
        private static float LocalOverrideScale = 0.7f;
        private static byte LocalOverrideBody = (byte)PlayerBodyTypes.Normal;
        private int fixedCounter;
        private int FixedSkipRate => BanMod.Instance?.MoveRateLimit?.Value ?? 0;

        private float lastRightClickTime;
        private const float RightClickCooldown = 0.2f;
        public static bool zoomkey = false;
        private float lastMiddleClickTime;
        private const float doubleClickThreshold = 0.3f;
        void Update()
        {
            if (!CanRun())
                return;

            HandleSelection();
            HandleActions();
            HandleResizing();
        }

        void FixedUpdate()
        {
            if (BanMod.IsBanModDisabled) return;

            if (!CanRun())
                return;

            fixedCounter++;
            if (FixedSkipRate > 1 && fixedCounter < FixedSkipRate)
                return;

            fixedCounter = 0;

        }


        private bool CanRun()
        {
            return AmongUsClient.Instance != null
                && PlayerControl.LocalPlayer != null;
        }

        private Vector2 GetMouseWorld()
        {
            var cam = Camera.main;
            return cam != null
                ? cam.ScreenToWorldPoint(Input.mousePosition)
                : Vector2.zero;
        }

        private static void ApplyVisual(PlayerControl pc, float scale, byte bodyType)
        {
            if (pc == null || pc.Data == null)
                return;

            pc.transform.localScale = new Vector3(scale, scale, 1f);
            Utils.SetPlayerBodyType(pc, (PlayerBodyTypes)bodyType);
        }

        private static void SendVisualRpc(byte targetPlayerId, float scale, byte bodyType, bool selfOverride)
        {
            BanModSyncApi.SendSyncPlayerVisual(
                targetPlayerId,
                scale,
                bodyType,
                selfOverride);
        }

        private void SetSelectedPlayerVisual(float scale, PlayerBodyTypes bodyType)
        {
            if (selectedPlayer == null)
                return;

            byte targetId = selectedPlayer.PlayerId;
            byte bodyByte = (byte)bodyType;
            float safeScale = Mathf.Clamp(scale, 0.25f, 2.0f);

            bool amHost = AmongUsClient.Instance.AmHost;
            bool isMe = selectedPlayer == PlayerControl.LocalPlayer;

            ApplyVisual(selectedPlayer, safeScale, bodyByte);

            if (amHost)
            {
                HostScales[targetId] = safeScale;
                HostBodies[targetId] = bodyByte;

                SendVisualRpc(targetId, safeScale, bodyByte, false);
            }
            else
            {
                if (isMe)
                {
                    LocalVisualOverride = true;
                    LocalOverrideScale = safeScale;
                    LocalOverrideBody = bodyByte;

                    SendVisualRpc(targetId, safeScale, bodyByte, true);
                }
                else
                {
                    BMLogger.Info("[VisualRPC] Client non-host: modifica altri ignorata, prevale host.");

                    if (HostScales.TryGetValue(targetId, out float hostScale) &&
                        HostBodies.TryGetValue(targetId, out byte hostBody))
                    {
                        ApplyVisual(selectedPlayer, hostScale, hostBody);
                    }
                }
            }
        }

        private void SetSelectedPlayerScale(float newScale)
        {
            if (selectedPlayer == null)
                return;

            PlayerBodyTypes currentBody = Utils.GetPlayerBodyType(selectedPlayer);
            SetSelectedPlayerVisual(newScale, currentBody);
        }
        private void HandleSelection()
        {
            bool combinedClick = (Input.GetMouseButtonDown(1) && Input.GetMouseButton(0)) ||
                                 (Input.GetMouseButtonDown(0) && Input.GetMouseButton(1));

            if (!combinedClick)
                return;

            if (Time.time - lastRightClickTime < RightClickCooldown)
                return;

            lastRightClickTime = Time.time;

            Vector2 mouseWorld = GetMouseWorld();

            PlayerControl closest = BanMod.AllPlayerControls
                .Where(p => p != null && p.Data != null)
                .OrderBy(p => Vector2.Distance(p.transform.position, mouseWorld))
                .FirstOrDefault();

            if (closest == null)
                return;

            if (selectedPlayer == closest)
            {
                Deselect();
                BMLogger.Info("[PlayerMouseController] Deselezionato tramite combo");
                return;
            }

            Select(closest);
        }

        private void HandleResizing()
        {
            if (!BanMod.Resize_Player.Value)
                return;

            if (Input.GetMouseButtonDown(2))
            {
                if (selectedPlayer != null)
                {
                    SetSelectedPlayerScale(0.7f);
                }

                BMLogger.Info("[BanMod] Reset scala a 0.7");
                lastMiddleClickTime = Time.time;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                float step = 0.5f;

                if (selectedPlayer != null)
                {
                    float currentScale = selectedPlayer.transform.localScale.x;

                    float newScale = Mathf.Clamp(
                        currentScale + (scroll > 0 ? step : -step),
                        0.25f,
                        2.0f
                    );

                    SetSelectedPlayerScale(newScale);
                }
            }
        }
        private void Select(PlayerControl pc)
        {
            Deselect();

            selectedPlayer = pc;
            SafeSetOutline(pc, true);
            if (selectedPlayer.Data.IsDead)
            {
                Utils.seeGhosts = true;
            }
            else
            {
                Utils.seeGhosts = false;
            }
            BMLogger.Info($"[PlayerMouseController] Selezionato {pc.Data.PlayerName} (Dead: {pc.Data.IsDead})");
        }

        private void Deselect()
        {
            if (selectedPlayer != null)
            {
                SafeSetOutline(selectedPlayer, false);
                Utils.seeGhosts = false;
                selectedPlayer = null;
            }
        }

        private void SafeSetOutline(PlayerControl pc, bool active)
        {
            try
            {
                if (pc == null)
                    return;

                var cosmetics = pc.cosmetics;
                if (cosmetics == null || cosmetics.Pointer == IntPtr.Zero)
                    return;

                if (active)
                {
                    cosmetics.SetOutline(
                        true,
                        new Il2CppSystem.Nullable<Color>(Color.yellow)
                    );
                }
                else
                {
                    cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(Color.white));

                    foreach (var rend in pc.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        if (rend == null) continue;

                        bool wasEnabled = rend.enabled;
                        rend.enabled = false;
                        rend.enabled = wasEnabled;
                    }
                }
            }
            catch
            {
            }
        }

        public static void ReceiveSyncPlayerVisual(MessageReader reader, int senderId)
        {
            if (reader == null)
                return;

            byte targetId = reader.ReadByte();
            float scale = reader.ReadSingle();
            byte bodyType = reader.ReadByte();
            bool selfOverride = reader.ReadBoolean();

            ReceiveSyncPlayerVisualServer(
                targetId,
                scale,
                bodyType,
                selfOverride,
                senderId);
        }

        public static void ReceiveSyncPlayerVisualServer(
            byte targetId,
            float scale,
            byte bodyType,
            bool selfOverride,
            int senderId)
        {
            if (AmongUsClient.Instance == null)
                return;

            scale = Mathf.Clamp(scale, 0.25f, 2.0f);

            PlayerControl target = BanMod.AllPlayerControls
                .FirstOrDefault(p => p != null && p.PlayerId == targetId);

            if (target == null)
                return;

            bool amHost = AmongUsClient.Instance.AmHost;
            bool isLocalTarget = target == PlayerControl.LocalPlayer;

            if (amHost && selfOverride)
            {
                PlayerControl senderPlayer = BanMod.AllPlayerControls
                    .FirstOrDefault(p => p != null && p.GetClientId() == senderId);

                if (senderPlayer == null || senderPlayer.PlayerId != targetId)
                    return;

                HostScales[targetId] = scale;
                HostBodies[targetId] = bodyType;

                ApplyVisual(target, scale, bodyType);

                // L'host ritrasmette via server agli altri BanMod.
                SendVisualRpc(targetId, scale, bodyType, true);
                return;
            }

            if (!amHost)
            {
                HostScales[targetId] = scale;
                HostBodies[targetId] = bodyType;

                if (isLocalTarget && LocalVisualOverride)
                {
                    ApplyVisual(
                        PlayerControl.LocalPlayer,
                        LocalOverrideScale,
                        LocalOverrideBody);
                    return;
                }

                ApplyVisual(target, scale, bodyType);
            }
        }
        private void HandleActions()
        {
            if (selectedPlayer == null || selectedPlayer.Data == null)
                return;

            if (AmongUsClient.Instance == null)
                return;

            var hud = DestroyableSingleton<HudManager>.Instance;

            if (hud == null)
                return;

            if (hud.Chat != null && hud.Chat.IsOpenOrOpening)
                return;

            if (Input.GetKeyDown(KeyBindOptions.K1) && AmongUsClient.Instance.AmHost)
            {
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        if (GameStates.isLobby)
                        {
                            if (selectedPlayer.Data.IsDead)
                            {
                                selectedPlayer.Revive();
                                selectedPlayer.Data.IsDead = false;
                                selectedPlayer.Data.MarkDirty();
                                selectedPlayer.RpcSetRole(RoleTypes.Crewmate, true);
                                selectedPlayer.Data.MarkDirty();
                            }
                            else
                            {
                                selectedPlayer.RpcSetRole(RoleTypes.CrewmateGhost, true);
                            }
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyBindOptions.K2))
            {
                if (selectedPlayer == null)
                    return;

                if (ModeratorAuthority.CanUseLocal)
                {
                    ModeratorAuthority.Request(
                        ModeratorAction.ChangeBody,
                        selectedPlayer.PlayerId);
                }
                else
                {
                    float scale = selectedPlayer.transform.localScale.x;
                    PlayerBodyTypes nextBody = Utils.GetNextBodyType(selectedPlayer);
                    SetSelectedPlayerVisual(scale, nextBody);
                }
            }

            if (Input.GetKeyDown(KeyBindOptions.K3) && ModeratorAuthority.CanUseLocal)
            {
                ModeratorAuthority.Request(
                    ModeratorAction.Ban,
                    selectedPlayer.PlayerId);
            }

            if (Input.GetKeyDown(KeyBindOptions.K4) && ModeratorAuthority.CanUseLocal)
            {
                ModeratorAuthority.Request(
                    ModeratorAction.Kick,
                    selectedPlayer.PlayerId);
            }

            if (Input.GetKeyDown(KeyBindOptions.K5) && ModeratorAuthority.CanUseLocal)
            {
                ModeratorAuthority.Request(
                    ModeratorAction.RandomFreeColor,
                    selectedPlayer.PlayerId);
            }
            if (Input.GetKeyDown(KeyBindOptions.K6))
            {
                if (zoomkey == false)
                    zoomkey = true;
                else if (zoomkey == true)
                    zoomkey = false;
            }
            if (Input.GetKeyDown(KeyBindOptions.K7) && ModeratorAuthority.CanUseLocal)
            {
                ModeratorAuthority.Request(ModeratorAction.ToggleLobbyObject);
            }
            if (Input.GetKeyDown(KeyBindOptions.K8) && AmongUsClient.Instance.AmHost)
            {
                DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage
                });
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                SimulatedArrowFollow.Toggle(selectedPlayer);
            }
            if (Input.GetKeyDown(KeyBindOptions.K9))
            {
                bool isHost = AmongUsClient.Instance.AmHost;
                bool canExecute = false;
                string reason = "Unknown";

                bool isDead = PlayerControl.LocalPlayer.Data.IsDead;

                if (isDead)
                {
                    canExecute = true;
                    BMLogger.LogInfo("[TaskLog] Player morto: bypass impostazioni, procedo.");
                }
                else if (isHost)
                {
                    bool immOpt = Options.EnableImmortal.GetBool();
                    bool immAssigned = ImmortalManager.immortalAssigned;
                    bool engFixer = Options.EngineerFixer.GetBool();
                    bool isEng = Utils.Engineer(PlayerControl.LocalPlayer);

                    BMLogger.LogInfo($"[TaskLog] HOST - ImmOpt: {immOpt}, Assigned: {immAssigned}, EngFix: {engFixer}");

                    if (!immOpt || (immOpt && immAssigned))
                    {
                        canExecute = true;

                        if (engFixer && isEng)
                        {
                            canExecute = false;
                            reason = "Engineer Fixer attivo (Host)";
                        }
                    }
                    else
                    {
                        reason = "Immortal non ancora assegnato (Host)";
                    }
                }
                else
                {
                    bool hImmEnabled = HostOptionStatus.ImmortalEnabled;
                    bool hImmAdded = HostOptionStatus.ImmortalAdded;
                    bool hEngEnabled = HostOptionStatus.EngineerEnabled;

                    BMLogger.LogInfo($"[TaskLog] CLIENT - ImmEnabled: {hImmEnabled}, ImmAdded: {hImmAdded}, EngEnabled: {hEngEnabled}");

                    if (!hImmEnabled)
                    {
                        canExecute = true;
                        BMLogger.LogInfo("[TaskLog] Lobby Vanilla o Opzione Off: Procedo.");
                    }
                    else
                    {
                        if (hImmAdded)
                        {
                            canExecute = true;
                        }
                        else
                        {
                            reason = "Immortal attivo ma non aggiunto (Client)";
                        }
                    }

                    if (canExecute && hEngEnabled && Utils.Engineer(PlayerControl.LocalPlayer))
                    {
                        canExecute = false;
                        reason = "Engineer Fixer attivo (Client)";
                    }
                }

                if (canExecute)
                {
                    BMLogger.LogInfo("[TaskLog] ESECUZIONE COROUTINE AVVIATA");
                    HudManager.Instance.StartCoroutine(CheatUtils.CompletaTutteLeTaskConDelay(1f));
                }
                else
                {
                    BMLogger.LogWarning($"[TaskLog] BLOCCHETTO: {reason}");
                }
            }
        }
    }
}

