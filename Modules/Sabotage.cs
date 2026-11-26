//credits and licenses in the resources folder
using AmongUs.GameOptions;
using BanMod;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BanMod.BanMod;
using static BanMod.Utils;

namespace BanMod
{

    [HarmonyPatch(typeof(MapBehaviour))]
    [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap))]
    public static class MapBehaviour_ShowSabotageMap_CustomButtonsPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            GameModeType gameMode = Options.GameMode.Selected;
            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            Transform parentTransform = __instance.infectedOverlay?.transform;

            if (parentTransform == null)
            {
                return;
            }
            if (!Options.Enablesabotage.GetBool())
            {
                return;
            }
            if (mapId != 0)
            {
                return;
            }
            CreateButton(__instance, parentTransform, "CustomSabotageButton_1", new Vector2(-0.0f, 2.3f), "BanMod.Resources.image.SabotageIcon1.png", () =>
            {
                SabotageManager.TryActivateSabotage(SystemTypes.Comms, 128);
                __instance.Close();
            });

            CreateButton(__instance, parentTransform, "CustomSabotageButton_2", new Vector2(-0.75f, 2.3f), "BanMod.Resources.image.SabotageIcon2.png", () =>
            {
                SabotageManager.TryActivateSabotage(SystemTypes.LifeSupp, 128);
                __instance.Close();
            });

            CreateButton(__instance, parentTransform, "CustomSabotageButton_3", new Vector2(0.75f, 2.3f), "BanMod.Resources.image.SabotageIcon3.png", () =>
            {
                SabotageManager.TryActivateSabotage(SystemTypes.Reactor, 128);
                __instance.Close();
            });

            CreateButton(__instance, parentTransform, "CustomSabotageButton_4", new Vector2(1.50f, 2.3f), "BanMod.Resources.image.SabotageIcon4.png", () =>
            {
                byte id = 4;
                for (int i = 0; i < 5; i++) id |= (byte)(1 << i);
                id |= 128;
                SabotageManager.TryActivateSabotage(SystemTypes.Electrical, id);
                __instance.Close();
            });
        }

        private static void CreateButton(MapBehaviour __instance, Transform parent, string name, Vector2 position, string spritePath, Action onClick)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.layer = LayerMask.NameToLayer("UI");

            Canvas canvas = buttonGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999;

            buttonGO.AddComponent<CanvasScaler>();
            buttonGO.AddComponent<GraphicRaycaster>();

            CanvasGroup canvasGroup = buttonGO.AddComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.localPosition = position;
            rectTransform.sizeDelta = new Vector2(100f, 100f);
            rectTransform.localScale = Vector3.one * 0.01f;

            buttonGO.AddComponent<CanvasRenderer>();
            Image buttonImage = buttonGO.AddComponent<Image>();

            Sprite sprite = Utils.LoadSprite(spritePath, 100f);
            if (sprite != null)
            {
                buttonImage.sprite = sprite;
                buttonImage.SetNativeSize();
            }
            else
            {
                buttonImage.color = Color.gray;
            }

            Button button = buttonGO.AddComponent<Button>();
            Action value = () => onClick.Invoke();
            button.onClick.AddListener(value);

        }
    }
}
[HarmonyPatch(typeof(SabotageSystemType))]
public static class SabotageSystemTypePatch
{
    [HarmonyPatch(nameof(SabotageSystemType.UpdateSystem))]
    [HarmonyPostfix]
    public static void Postfix_UpdateSystem(SabotageSystemType __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        if (__instance.Timer > 0f && !SabotageManager.IsSabotageActive)
        {
            SabotageManager.SetSabotageActiveState(true);
        }

        SabotageManager.SetGameSabotageCooldown(__instance.Timer);
    }

    [HarmonyPatch(nameof(SabotageSystemType.Deteriorate))]
    [HarmonyPostfix]
    public static void Postfix_Deteriorate(SabotageSystemType __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        SabotageManager.SetGameSabotageCooldown(__instance.Timer);

        if (__instance.Timer <= 0f && !__instance.AnyActive && SabotageManager.IsSabotageActive)
        {
            SabotageManager.SetSabotageActiveState(false);
        }
    }

    [HarmonyPatch(nameof(SabotageSystemType.SetInitialSabotageCooldown))]
    [HarmonyPostfix]
    public static void Postfix_InitialCooldown(SabotageSystemType __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        SabotageManager.SetGameSabotageCooldown(__instance.Timer);
    }
}
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatus_FixedUpdate_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(ShipStatus __instance)
    {
        GameModeType gameMode = Options.GameMode.Selected;

        if (!AmongUsClient.Instance.AmHost)
            return true;
        if (BanMod.BanMod.IsBanModDisabled)
            return true;
        if (__instance == null)
            return true;

        if (gameMode == GameModeType.SnS)
        {
            FixSabotage(__instance, SystemTypes.Reactor);
            FixSabotage(__instance, SystemTypes.Laboratory);
            FixSabotage(__instance, SystemTypes.HeliSabotage);
            FixSabotage(__instance, SystemTypes.LifeSupp);
            FixSabotage(__instance, SystemTypes.Electrical);
            return true;
        }
        if (gameMode == GameModeType.PnS)
        {
            FixSabotage(__instance, SystemTypes.Reactor);
            FixSabotage(__instance, SystemTypes.Laboratory);
            FixSabotage(__instance, SystemTypes.HeliSabotage);
            FixSabotage(__instance, SystemTypes.LifeSupp);
            FixSabotage(__instance, SystemTypes.Electrical);
            return true;
        }
        if (Options.DisableAllSabotages.GetBool())
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.FFA)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.FFATeam)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.HotPotatoModded)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.KillRace)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.Assassin)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.RoomRush)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.TargetRush)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (gameMode == GameModeType.DeathRun)
        {
            FixAllSabotages(__instance);
            return true;
        }
        if (Options.DisableReactorSabotage.GetBool())
            FixSabotage(__instance, SystemTypes.Reactor);

        if (Options.DisableCommsSabotage.GetBool())
            FixSabotage(__instance, SystemTypes.Comms);

        if (Options.DisableO2Sabotage.GetBool())
            FixSabotage(__instance, SystemTypes.LifeSupp);

        if (Options.DisableElectricalSabotage.GetBool())
            FixSabotage(__instance, SystemTypes.Electrical);

        if (Options.DisableLaboratorySabotage.GetBool())
            FixSabotage(__instance, SystemTypes.Laboratory);

        if (Options.DisableHeliSabotage.GetBool())
            FixSabotage(__instance, SystemTypes.HeliSabotage);


        return true;
    }

    public static void FixSabotage(ShipStatus shipStatus, SystemTypes systemType)
    {
        if (!Utils.IsActive(systemType)) return;

        switch (systemType)
        {
            case SystemTypes.HeliSabotage:
                BMLogger.Info("[FixSabotage] Fixing HeliSabotage by fixing both consoles.");
                shipStatus.RpcUpdateSystem(systemType, 16 | 0);
                shipStatus.RpcUpdateSystem(systemType, 16 | 1); 
                break;

            case SystemTypes.Laboratory:
            case SystemTypes.Reactor:
                BMLogger.Info($"[FixSabotage] Fixing reactor ({systemType})");
                shipStatus.RpcUpdateSystem(systemType, 16);
                break;

            case SystemTypes.Comms:
            case SystemTypes.LifeSupp:
                shipStatus.RpcUpdateSystem(systemType, 16);
                shipStatus.RpcUpdateSystem(systemType, 16 | 0); 
                shipStatus.RpcUpdateSystem(systemType, 16 | 1); 
                break;

            case SystemTypes.Electrical:
                shipStatus.RpcUpdateSystem(systemType, 16);
                if (shipStatus.Systems.TryGetValue(systemType, out var system))
                {
                    var elecSys = system.Cast<SwitchSystem>();
                    for (var i = 0; i < 5; i++)
                    {
                        int switchMask = 1 << i;
                        if ((elecSys.ActualSwitches & switchMask) != (elecSys.ExpectedSwitches & switchMask))
                        {
                            shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
                        }
                    }
                }
                break;

            default:
                Debug.LogWarning($"[FixSabotage] Unknown systemType: {systemType}, sending default repair.");
                shipStatus.RpcUpdateSystem(systemType, 16);
                break;
        }
    }

    public static void FixAllSabotages(ShipStatus shipStatus)
    {
        if (!Utils.AnySabotageIsActive()) return;

        FixSabotage(shipStatus, SystemTypes.Reactor);
        FixSabotage(shipStatus, SystemTypes.Laboratory);
        FixSabotage(shipStatus, SystemTypes.HeliSabotage);
        FixSabotage(shipStatus, SystemTypes.LifeSupp);
        FixSabotage(shipStatus, SystemTypes.Electrical);
        FixSabotage(shipStatus, SystemTypes.Comms);
    }
}
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
public static class BlockCloseDoorsPatch
{
    public static bool Prefix(SystemTypes room)
    {
        GameModeType gameMode1 = Options.GameMode.Selected;

        if (Options.DisableDoorSabotage.GetBool())
        {
            BMLogger.Info($"[BlockCloseDoorsPatch] Tentativo di chiudere porta in {room} bloccato.");
            return false; 
        }
        if (gameMode1 == GameModeType.FFA)
        {
            return false;
        }
        if (gameMode1 == GameModeType.FFATeam)
        {
            return false;
        }
        if (gameMode1 == GameModeType.KillRace)
        {
            return false;
        }
        if (gameMode1 == GameModeType.HotPotatoModded)
        {
            return false;
        }
        if (gameMode1 == GameModeType.Assassin)
        {
            return false;
        }
        if (gameMode1 == GameModeType.SnS)
        {
            return false;
        }
        if (gameMode1 == GameModeType.PnS)
        {
            return false;
        }
        if (gameMode1 == GameModeType.RoomRush)
        {
            return false;
        }
        if (gameMode1 == GameModeType.TargetRush)
        {
            return false;
        }
        if (gameMode1 == GameModeType.DeathRun)
        {
            return false;
        }
        return true;
        
    }
}
[HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.UpdateSystem))]
class Patch_MushroomMixupBlock
{
    static bool Prefix(ref MushroomMixupSabotageSystem __instance, PlayerControl player, MessageReader msgReader)
    {
        if (Options.DisableMushroomSabotage.GetBool() || Options.DisableAllSabotages.GetBool())
        {
            return false;
        }
        if (player.Data.IsDead && player.Data.Role.IsImpostor && Options.DisableDeadImpostorSabotage.GetBool())
        {
            return false;
        }
        return true; 
    }
}

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class SabotageSystemType_UpdateSystem_Patch1
{
    private static readonly HashSet<byte> ForcedCrewmateGhosts1 = new HashSet<byte>();

    public static bool Prefix(PlayerControl player, MessageReader msgReader)
    {
        if (!AmongUsClient.Instance.AmHost)
            return true;

        if (player == null || player.Data == null || player.Data.Role == null)
            return false;

        if (!Options.DisableDeadImpostorSabotage.GetBool())
            return true;

        if (player.Data.IsDead && player.Data.Role.IsImpostor)
        {
            if (msgReader != null && msgReader.BytesRemaining > 0)
                msgReader.ReadByte();

            ForcedCrewmateGhosts1.Add(player.PlayerId);

            player.RpcSetRole(RoleTypes.CrewmateGhost);

            return false;
        }

        return true;
    }

    public static void RestoreImpostorGhosts()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null)
                continue;

            if (!ForcedCrewmateGhosts1.Contains(pc.PlayerId))
                continue;

            pc.RpcSetRole(RoleTypes.ImpostorGhost);
        }
    }

    public static void Clear()
    {
        ForcedCrewmateGhosts1.Clear();
    }
}
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class LogicGameFlowNormal_CheckEndCriteria_Patch1
{
    public static void Prefix()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        if (!Options.DisableDeadImpostorSabotage.GetBool())
            return;

        SabotageSystemType_UpdateSystem_Patch1.RestoreImpostorGhosts();
    }
}
[HarmonyPatch(typeof(SwitchSystem), nameof(SwitchSystem.UpdateSystem))]
class SwitchUpdatePatch
{
    private static bool Prefix(SwitchSystem __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
    {

        byte amount;
        {
            var newReader = MessageReader.Get(msgReader);
            amount = newReader.ReadByte();
            newReader.Recycle();
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return true;
        }

        if (amount.HasBit(SwitchSystem.DamageSystem))
        {
            return true;
        }


        if (Options.BlockSwitches.GetBool())
        {
            var switchedKnob = (byte)(0b_00001 << amount);

            bool isAlreadyFixed =
                (__instance.ActualSwitches & switchedKnob) ==
                (__instance.ExpectedSwitches & switchedKnob);

            if (isAlreadyFixed)
            {
                BMLogger.Info($"[SWITCH BLOCKED] {player.Data.PlayerName} ha tentato di toccare uno switch già sistemato (index: {amount}).");
                return false;
            }
        }

        return true;
    }
}
