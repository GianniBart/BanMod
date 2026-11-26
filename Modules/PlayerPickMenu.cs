//credits and licenses in the resources folder
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Sentry.Internal.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BanMod
{
    public static class PlayerPickMenu
    {
        public static ShapeshifterMinigame playerpickMenu;
        public static bool IsActive;
        public static NetworkedPlayerInfo targetPlayerData;
        public static System.Action customAction;
        public static List<NetworkedPlayerInfo> customPlayerList;

        public static ShapeshifterMinigame getShapeshifterMenu()
        {
            var rolePrefab = getBehaviourByRoleType(RoleTypes.Shapeshifter);
            return Object.Instantiate(rolePrefab?.Cast<ShapeshifterRole>(), GameData.Instance.transform).ShapeshifterMenu;
        }

        public static RoleBehaviour getBehaviourByRoleType(RoleTypes roleType)
        {
            foreach (var role in RoleManager.Instance.AllRoles)
                if (role && role.Role == roleType)
                    return role;

            return null;
        }

        public static void openPlayerPickMenu(List<NetworkedPlayerInfo> playerList, System.Action action)
        {
            try
            {
                if (playerpickMenu != null)
                {
                    try { playerpickMenu.Close(); }
                    catch { }
                    try { Object.Destroy(playerpickMenu.gameObject); }
                    catch { }
                }
            }
            catch { }

            IsActive = true;
            customPlayerList = playerList;
            customAction = action;

            playerpickMenu = Object.Instantiate(getShapeshifterMenu());
            try { playerpickMenu.gameObject.name = "Custom_PlayerPickMenu"; } catch { }

            playerpickMenu.transform.SetParent(Camera.main.transform, false);
            playerpickMenu.transform.localPosition = new Vector3(0f, 0f, -50f);
            playerpickMenu.Begin(null);
        }

    }


    [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Shapeshift))]
    public static class PlayerPickMenu_ShiftPatch
    {
        public static bool Prefix(ShapeshifterMinigame __instance, PlayerControl target)
        {
            if (!PlayerPickMenu.IsActive)
                return true;

            PlayerPickMenu.targetPlayerData = target.Data;
            PlayerPickMenu.customAction?.Invoke();
            PlayerPickMenu.IsActive = false;

            try
            {
                __instance.Close();
            }
            catch { }

            return false; 
        }
    }

    [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
    public static class ShapeshifterMinigame_Begin
    {
        public static bool Prefix(ShapeshifterMinigame __instance)
        {
            if (!PlayerPickMenu.IsActive)
                return true;


            var list = PlayerPickMenu.customPlayerList;

            __instance.potentialVictims = new Il2CppSystem.Collections.Generic.List<ShapeshifterPanel>();
            var uiList = new Il2CppSystem.Collections.Generic.List<UiElement>();

            for (int i = 0; i < list.Count; i++)
            {
                NetworkedPlayerInfo playerData = list[i];

                int num = i % 3;
                int num2 = i / 3;

                ShapeshifterPanel shapeshifterPanel =
                    Object.Instantiate(__instance.PanelPrefab, __instance.transform);

                shapeshifterPanel.transform.localPosition =
                    new Vector3(__instance.XStart + num * __instance.XOffset,
                                __instance.YStart + num2 * __instance.YOffset,
                                -1f);

                shapeshifterPanel.SetPlayer(
                    i,
                    playerData,
                    (Il2CppSystem.Action)(() =>
                    {
                        PlayerPickMenu.targetPlayerData = playerData;
                        PlayerPickMenu.customAction.Invoke();
                        __instance.Close();
                    })
                );

                if (playerData.Object != null)
                {
                    shapeshifterPanel.NameText.text = playerData.PlayerName;

                    shapeshifterPanel.NameText.transform.localPosition =
                        new Vector3(0.3384f, 0.0311f, -0.1f);

                    shapeshifterPanel.NameText.transform.localScale =
                        new Vector3(0.9f, 1f, 1f);
                }

                __instance.potentialVictims.Add(shapeshifterPanel);
                uiList.Add(shapeshifterPanel.Button);
            }

            ControllerManager.Instance.OpenOverlayMenu(
                __instance.name,
                __instance.BackButton,
                __instance.DefaultButtonSelected,
                uiList,
                false
            );

            PlayerPickMenu.IsActive = false;
            return false;
        }
    }


    [HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
    public static class ShapeshifterPanel_SetPlayer
    {
        public static bool Prefix(ShapeshifterPanel __instance, int index, NetworkedPlayerInfo playerInfo, Il2CppSystem.Action onShift)
        {
            if (!PlayerPickMenu.IsActive)
                return true;

            if (__instance == null)
                return false;

            if (playerInfo == null || playerInfo.Disconnected)
                return false;

            __instance.shapeshift = onShift;
            __instance.PlayerIcon.SetFlipX(false);
            __instance.PlayerIcon.ToggleName(false);

            SpriteRenderer[] componentsInChildren = __instance.GetComponentsInChildren<SpriteRenderer>();
            foreach (var t in componentsInChildren)
            {
                t.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
            }

            __instance.PlayerIcon.SetMaskLayer(index + 2);
            __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(
                playerInfo,
                PlayerOutfitType.Default,
                PlayerMaterial.MaskType.ComplexUI,
                false,
                null
            );

            __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);

            __instance.NameText.text = playerInfo.PlayerName;

            DataManager.Settings.Accessibility.OnColorBlindModeChanged +=
                (Il2CppSystem.Action)__instance.SetColorblindText;

            __instance.SetColorblindText();

            return false;
        }
    }
}


