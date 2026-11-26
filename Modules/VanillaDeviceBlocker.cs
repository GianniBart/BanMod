//credits and licenses in the resources folder/

using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BanMod;

public static class VanillaDeviceBlocker
{
    private enum DeviceType
    {
        Cameras,
        Admin,
        Vitals
    }

    private sealed class DeviceLocation
    {
        public DeviceType Type;
        public Vector2 Position;

        public DeviceLocation(DeviceType type, Vector2 position)
        {
            Type = type;
            Position = position;
        }
    }

    private static readonly HashSet<byte> DesyncedCommsPlayers = new();

    private static byte _lastBlockMask;
    private static bool _localStateInitialized;

    private static readonly Dictionary<int, DeviceLocation[]> DevicePositions =
        new Dictionary<int, DeviceLocation[]>
        {
            {
                0,
                new[]
                {
                    // Admin
                    new DeviceLocation(
                        DeviceType.Admin,
                        new Vector2(3.48f, -8.62f)),

                    // Cameras
                    new DeviceLocation(
                        DeviceType.Cameras,
                        new Vector2(-13.06f, -2.45f))
                }
            },
            {
                1,
                new[]
                {
                    // Admin
                    new DeviceLocation(
                        DeviceType.Admin,
                        new Vector2(21.02f, 19.09f))
                }
            },
            {
                2,
                new[]
                {
                    // Left Admin
                    new DeviceLocation(
                        DeviceType.Admin,
                        new Vector2(22.80f, -21.52f)),

                    // Right Admin
                    new DeviceLocation(
                        DeviceType.Admin,
                        new Vector2(24.66f, -21.52f)),

                    // Cameras
                    new DeviceLocation(
                        DeviceType.Cameras,
                        new Vector2(2.96f, -12.74f)),

                    // Vitals
                    new DeviceLocation(
                        DeviceType.Vitals,
                        new Vector2(26.70f, -15.94f))
                }
            },
            {
                4,
                new[]
                {
                    // Cockpit Admin
                    new DeviceLocation(
                        DeviceType.Admin,
                        new Vector2(-22.32f, 0.91f)),

                    // Records Admin
                    new DeviceLocation(
                        DeviceType.Admin,
                        new Vector2(19.89f, 12.60f)),

                    // Cameras
                    new DeviceLocation(
                        DeviceType.Cameras,
                        new Vector2(8.10f, -9.63f)),

                    // Vitals
                    new DeviceLocation(
                        DeviceType.Vitals,
                        new Vector2(25.24f, -7.94f))
                }
            },
            {
                5,
                new[]
                {
                    // Vitals
                    new DeviceLocation(
                        DeviceType.Vitals,
                        new Vector2(-2.765f, -9.819f))
                }
            }
        };


    private static bool ShouldBlock(DeviceType type)
    {
        switch (type)
        {
            case DeviceType.Cameras:
                return Options.DisableDeviceCam != null &&
                       Options.DisableDeviceCam.GetBool();

            case DeviceType.Admin:
                return Options.DisableDeviceAdminPanel != null &&
                       Options.DisableDeviceAdminPanel.GetBool();

            case DeviceType.Vitals:
                return Options.DisableDeviceVitals != null &&
                       Options.DisableDeviceVitals.GetBool();

            default:
                return false;
        }
    }

    public static bool ShouldBlockCameras()
    {
        if (!ShouldBlock(DeviceType.Cameras))
            return false;

        int mapId = GetCurrentMapId();

        return mapId == 0 ||
               mapId == 2 ||
               mapId == 4;
    }

    public static bool ShouldBlockAdmin()
    {
        if (!ShouldBlock(DeviceType.Admin))
            return false;

        int mapId = GetCurrentMapId();

        return mapId == 0 ||
               mapId == 1 ||
               mapId == 2 ||
               mapId == 4;
    }

    public static bool ShouldBlockVitals()
    {
        if (!ShouldBlock(DeviceType.Vitals))
            return false;

        int mapId = GetCurrentMapId();

        return mapId == 2 ||
               mapId == 4 ||
               mapId == 5;
    }

    public static bool IsEnabled()
    {
        return ShouldBlockCameras() ||
               ShouldBlockAdmin() ||
               ShouldBlockVitals();
    }

    private static byte GetBlockMask()
    {
        byte mask = 0;

        if (ShouldBlockCameras())
            mask |= 1;

        if (ShouldBlockAdmin())
            mask |= 2;

        if (ShouldBlockVitals())
            mask |= 4;

        return mask;
    }

    public static void OnShipStarted()
    {
        DesyncedCommsPlayers.Clear();

        _lastBlockMask = GetBlockMask();
        _localStateInitialized = true;

        UpdateLocalDeviceColliders();
    }

    public static void FixedUpdate()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (ShipStatus.Instance == null)
            return;

        byte blockMask = GetBlockMask();

        if (!_localStateInitialized ||
            blockMask != _lastBlockMask)
        {
            UpdateLocalDeviceColliders();

            _lastBlockMask = blockMask;
            _localStateInitialized = true;
        }

        if (blockMask == 0)
        {
            RestoreAllPlayers();
            return;
        }

        if (MeetingHud.Instance != null)
        {
            RestoreAllPlayers();
            return;
        }

        if (Utils.IsActive(SystemTypes.Comms))
        {
            DesyncedCommsPlayers.Clear();
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null ||
                player.Data == null)
            {
                continue;
            }

            if (player.Data.Disconnected)
            {
                DesyncedCommsPlayers.Remove(player.PlayerId);
                continue;
            }

            if (player.AmOwner)
                continue;

            bool shouldBlock =
                !player.Data.IsDead &&
                !player.inVent &&
                IsNearBlockedDevice(player.GetTruePosition());

            UpdatePlayerComms(player, shouldBlock);
        }
    }

    private static bool IsNearBlockedDevice(Vector2 playerPosition)
    {
        int mapId = GetCurrentMapId();

        if (!DevicePositions.TryGetValue(
                mapId,
                out DeviceLocation[] locations))
        {
            return false;
        }

        const float usableDistance = 1f;

        foreach (DeviceLocation location in locations)
        {
            if (!ShouldBlock(location.Type))
                continue;

            if (Vector2.Distance(
                    playerPosition,
                    location.Position) <= usableDistance)
            {
                return true;
            }
        }

        return false;
    }

    private static void UpdatePlayerComms(
        PlayerControl player,
        bool shouldBlock)
    {
        if (player == null ||
            player.AmOwner)
        {
            return;
        }

        if (shouldBlock)
        {
            if (!DesyncedCommsPlayers.Add(player.PlayerId))
                return;

            SendTargetedSystemUpdate(
                player,
                SystemTypes.Comms,
                128);

            return;
        }

        if (!DesyncedCommsPlayers.Remove(player.PlayerId))
            return;

        SendTargetedCommsRepair(player);
    }

    private static void SendTargetedCommsRepair(
        PlayerControl player)
    {
        if (player == null)
            return;

        SendTargetedSystemUpdate(
            player,
            SystemTypes.Comms,
            16);

        int mapId = GetCurrentMapId();

        if (mapId == 1 ||
            mapId == 5)
        {
            SendTargetedSystemUpdate(
                player,
                SystemTypes.Comms,
                17);
        }
    }

    public static void RestoreAllPlayers()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            DesyncedCommsPlayers.Clear();
            return;
        }

        if (ShipStatus.Instance == null ||
            Utils.IsActive(SystemTypes.Comms))
        {
            DesyncedCommsPlayers.Clear();
            return;
        }

        foreach (byte playerId in DesyncedCommsPlayers.ToArray())
        {
            PlayerControl player = Utils.GetPlayerById(playerId);

            if (player == null ||
                player.Data == null ||
                player.Data.Disconnected)
            {
                continue;
            }

            SendTargetedCommsRepair(player);
        }

        DesyncedCommsPlayers.Clear();
    }

    private static void SendTargetedSystemUpdate(
        PlayerControl target,
        SystemTypes systemType,
        byte amount)
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (ShipStatus.Instance == null ||
            target == null ||
            target.AmOwner)
        {
            return;
        }

        int clientId = GetClientId(target);

        if (clientId < 0)
            return;

        MessageWriter writer =
            AmongUsClient.Instance.StartRpcImmediately(
                ShipStatus.Instance.NetId,
                (byte)RpcCalls.UpdateSystem,
                SendOption.Reliable,
                clientId);

        writer.Write((byte)systemType);
        writer.WriteNetObject(target);
        writer.Write(amount);

        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    private static int GetClientId(PlayerControl player)
    {
        if (player == null ||
            AmongUsClient.Instance == null ||
            AmongUsClient.Instance.allClients == null)
        {
            return -1;
        }

        foreach (var client in
                 AmongUsClient.Instance.allClients.ToArray())
        {
            if (client == null ||
                client.Character == null)
            {
                continue;
            }

            if (client.Character.PlayerId == player.PlayerId)
                return client.Id;
        }

        return -1;
    }

    private static int GetCurrentMapId()
    {
        if (GameOptionsManager.Instance == null ||
            GameOptionsManager.Instance.CurrentGameOptions == null)
        {
            return -1;
        }

        return GameOptionsManager
            .Instance
            .CurrentGameOptions
            .MapId;
    }

    private static void UpdateLocalDeviceColliders()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        int mapId = GetCurrentMapId();

        // Admin
        bool mapHasAdmin =
            mapId == 0 ||
            mapId == 1 ||
            mapId == 2 ||
            mapId == 4;

        if (mapHasAdmin)
        {
            bool blockAdmin =
                ShouldBlock(DeviceType.Admin);

            foreach (var admin in
                     GameObject.FindObjectsOfType<MapConsole>(true))
            {
                if (admin == null)
                    continue;

                SetColliderBlocked(
                    admin.gameObject,
                    blockAdmin);
            }
        }

        foreach (var console in
                 GameObject.FindObjectsOfType<SystemConsole>(true))
        {
            if (console == null)
                continue;

            string consoleName =
                console.gameObject.name ?? "";

            if (!TryGetDeviceType(
                    mapId,
                    consoleName,
                    out DeviceType deviceType))
            {
                continue;
            }

            SetColliderBlocked(
                console.gameObject,
                ShouldBlock(deviceType));
        }
    }

    private static bool TryGetDeviceType(
        int mapId,
        string consoleName,
        out DeviceType deviceType)
    {
        deviceType = DeviceType.Admin;

        switch (mapId)
        {
            case 0:
                // The Skeld Cameras
                if (consoleName == "SurvConsole")
                {
                    deviceType = DeviceType.Cameras;
                    return true;
                }

                break;

            case 2:
                // Polus Cameras
                if (consoleName == "Surv_Panel")
                {
                    deviceType = DeviceType.Cameras;
                    return true;
                }

                // Polus Vitals
                if (consoleName == "panel_vitals")
                {
                    deviceType = DeviceType.Vitals;
                    return true;
                }

                break;

            case 4:
                // Airship Cameras
                if (consoleName == "task_cams")
                {
                    deviceType = DeviceType.Cameras;
                    return true;
                }

                // Airship Vitals
                if (consoleName == "panel_vitals")
                {
                    deviceType = DeviceType.Vitals;
                    return true;
                }

                break;

            case 5:
                // The Fungle Vitals
                if (consoleName == "VitalsConsole")
                {
                    deviceType = DeviceType.Vitals;
                    return true;
                }

                break;
        }

        return false;
    }

    private static void SetColliderBlocked(
        GameObject target,
        bool blocked)
    {
        if (target == null)
            return;

        Collider2D collider =
            target.GetComponent<Collider2D>();

        if (collider != null)
            collider.enabled = !blocked;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public static class VanillaDeviceBlocker_ShipStart_Patch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        VanillaDeviceBlocker.OnShipStarted();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class VanillaDeviceBlocker_FixedUpdate_Patch
{
    public static void Postfix()
    {
        VanillaDeviceBlocker.FixedUpdate();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class VanillaDeviceBlocker_MeetingStart_Patch
{
    public static void Prefix()
    {
        VanillaDeviceBlocker.RestoreAllPlayers();
    }
}
