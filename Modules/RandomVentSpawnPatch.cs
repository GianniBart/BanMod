
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanMod;

internal static class RandomVentSpawnManager
{
    private static bool AlreadyTriggered;
    private static bool Running;


    // ============================================================
    // RESET
    // ============================================================

    public static void Reset()
    {
        AlreadyTriggered = false;
        Running = false;
    }


    // ============================================================
    // START
    // ============================================================

    public static void TryStart()
    {
        try
        {
            if (AlreadyTriggered || Running)
                return;


            bool randomVentSpawnEnabled =
                (
                    Options.RandomVentSpawn != null &&
                    Options.RandomVentSpawn.GetBool()
                )
                ||
                (
                    Options.RandomVentSpawn1 != null &&
                    Options.RandomVentSpawn1.GetBool()
                );


            bool randomVentSpawnFfaEnabled =
                (
                    Options.RandomVentSpawnffa != null &&
                    Options.RandomVentSpawnffa.GetBool() &&
                    Options.GameMode.GetValue(GameModeType.FFA)
                )
                ||
                (
                    Options.RandomVentSpawnffa1 != null &&
                    Options.RandomVentSpawnffa1.GetBool() &&
                    Options.GameMode.GetValue(GameModeType.FFATeam)
                );


            if (!randomVentSpawnEnabled &&
                !randomVentSpawnFfaEnabled)
            {
                return;
            }


            if (AmongUsClient.Instance == null ||
                !AmongUsClient.Instance.AmHost)
            {
                return;
            }


            if (ShipStatus.Instance == null ||
                HudManager.Instance == null)
            {
                return;
            }


            // ====================================================
            // VANILLA SERVER
            // ====================================================

            if (BanModServerSelection.IsVanilla)
            {
                if (ShipStatus.Instance.AllVents == null ||
                    ShipStatus.Instance.AllVents.Length <= 0)
                {
                    return;
                }


                VentilationSystem system =
                    GetVentSystem();


                if (system == null ||
                    system.PlayersInsideVents == null)
                {
                    return;
                }


                AlreadyTriggered = true;
                Running = true;


                HudManager.Instance.StartCoroutine(
                    CoBootPlayersFromRandomVents()
                );


                return;
            }


            // ====================================================
            // MODDED SERVER
            // ====================================================

            if (BanModServerSelection.IsModded25)
            {
                List<Vector2> mapPoints =
                    GetSavedSpawnPointsForCurrentMap();


                if (mapPoints == null ||
                    mapPoints.Count == 0)
                {
                    Debug.LogWarning(
                        "[RandomSpawn] Nessun punto configurato " +
                        "per questa mappa."
                    );

                    return;
                }


                AlreadyTriggered = true;
                Running = true;


                HudManager.Instance.StartCoroutine(
                    CoTeleportPlayersToSavedMapPositions()
                );


                return;
            }
        }
        catch (Exception ex)
        {
            Running = false;

            Debug.LogError(
                "[RandomSpawn] Start failed: " +
                ex
            );
        }
    }


    // ============================================================
    // MODDED TELEPORT COROUTINE
    // ============================================================

    private static IEnumerator CoTeleportPlayersToSavedMapPositions()
    {
        try
        {
            yield return null;
            yield return null;


            List<PlayerControl> players =
                GetValidPlayers();


            if (players == null ||
                players.Count == 0)
            {
                yield break;
            }


            bool isFfa =
                Options.GameMode.GetValue(
                    GameModeType.FFA
                );


            bool isFfaTeam =
                Options.GameMode.GetValue(
                    GameModeType.FFATeam
                );


            if (!isFfa &&
                !isFfaTeam)
            {
                TeleportPlayersFarApart(
                    players
                );
            }
            else
            {
                TeleportFfaPlayersByColor(
                    players
                );
            }


            yield return null;
        }
        finally
        {
            Running = false;
        }
    }


    // ============================================================
    // NORMAL MODE
    // ============================================================

    private static void TeleportPlayersFarApart(
        List<PlayerControl> players)
    {
        if (players == null ||
            players.Count == 0)
        {
            return;
        }


        List<Vector2> availablePoints =
            GetSavedSpawnPointsForCurrentMap();


        if (availablePoints == null ||
            availablePoints.Count == 0)
        {
            Debug.LogWarning(
                "[RandomSpawn] Nessun punto spawn " +
                "configurato per questa mappa."
            );

            return;
        }


        ShufflePlayers(
            players
        );


        List<Vector2> assignedPositions =
            new List<Vector2>();


        for (int i = 0;
             i < players.Count;
             i++)
        {
            PlayerControl player =
                players[i];


            if (player == null ||
                player.Data == null ||
                player.Data.Disconnected ||
                player.Data.IsDead)
            {
                continue;
            }


            if (availablePoints.Count == 0)
            {
                Debug.LogWarning(
                    "[RandomSpawn] Terminati i punti spawn."
                );

                break;
            }


            if (!TryGetFarthestSavedSpawnPoint(
                    availablePoints,
                    assignedPositions,
                    out Vector2 spawnPosition))
            {
                Debug.LogWarning(
                    "[RandomSpawn] Nessun punto disponibile per " +
                    player.Data.PlayerName
                );

                continue;
            }


            assignedPositions.Add(
                spawnPosition
            );


            availablePoints.Remove(
                spawnPosition
            );


            Debug.Log(
                "[RandomSpawn] MODDED: " +
                player.Data.PlayerName +
                " -> " +
                spawnPosition
            );


            player.NetTransform.RpcSnapTo(
                spawnPosition
            );
        }
    }


    // ============================================================
    // FFA / FFA TEAM
    // ============================================================

    private static void TeleportFfaPlayersByColor(
        List<PlayerControl> players)
    {
        if (players == null ||
            players.Count == 0)
        {
            return;
        }


        Dictionary<byte, List<PlayerControl>> playersByColor =
            BuildPlayerGroupsByColor(
                players
            );


        if (playersByColor == null ||
            playersByColor.Count == 0)
        {
            return;
        }


        List<Vector2> availablePoints =
            GetSavedSpawnPointsForCurrentMap();


        if (availablePoints == null ||
            availablePoints.Count == 0)
        {
            Debug.LogWarning(
                "[RandomSpawn] Nessun punto configurato " +
                "per questa mappa."
            );

            return;
        }


        List<byte> colors =
            new List<byte>(
                playersByColor.Keys
            );


        ShuffleColorIds(
            colors
        );


        List<Vector2> assignedTeamCenters =
            new List<Vector2>();


        for (int i = 0;
             i < colors.Count;
             i++)
        {
            byte colorId =
                colors[i];


            if (!playersByColor.TryGetValue(
                    colorId,
                    out List<PlayerControl> team) ||
                team == null ||
                team.Count == 0)
            {
                continue;
            }


            if (availablePoints.Count == 0)
                break;


            // Primo punto del team:
            // scegliamo il più lontano dagli altri team.
            if (!TryGetFarthestSavedSpawnPoint(
                    availablePoints,
                    assignedTeamCenters,
                    out Vector2 teamCenter))
            {
                continue;
            }


            assignedTeamCenters.Add(
                teamCenter
            );


            ShufflePlayers(
                team
            );


            List<Vector2> usedTeamPositions =
                new List<Vector2>();


            for (int p = 0;
                 p < team.Count;
                 p++)
            {
                PlayerControl player =
                    team[p];


                if (player == null ||
                    player.Data == null ||
                    player.Data.Disconnected ||
                    player.Data.IsDead)
                {
                    continue;
                }


                if (availablePoints.Count == 0)
                {
                    Debug.LogWarning(
                        "[RandomSpawn] Terminati i punti per il team " +
                        colorId
                    );

                    break;
                }


                Vector2 finalPosition;


                // Primo membro = centro team.
                if (usedTeamPositions.Count == 0)
                {
                    finalPosition =
                        teamCenter;
                }
                else
                {
                    // Gli altri membri ricevono uno degli
                    // spawn registrati più vicini al centro team.
                    if (!TryGetNearestSavedSpawnPoint(
                            availablePoints,
                            teamCenter,
                            out finalPosition))
                    {
                        break;
                    }
                }


                usedTeamPositions.Add(
                    finalPosition
                );


                availablePoints.Remove(
                    finalPosition
                );


                Debug.Log(
                    "[RandomSpawn] FFA TEAM: " +
                    player.Data.PlayerName +
                    " (Color " +
                    colorId +
                    ") -> " +
                    finalPosition
                );


                player.NetTransform.RpcSnapTo(
                    finalPosition
                );
            }
        }
    }


    // ============================================================
    // SELECT FARTHEST SAVED POINT
    // ============================================================

    private static bool TryGetFarthestSavedSpawnPoint(
        List<Vector2> availablePoints,
        List<Vector2> assignedPositions,
        out Vector2 result)
    {
        result =
            Vector2.zero;


        if (availablePoints == null ||
            availablePoints.Count == 0)
        {
            return false;
        }


        // Primo punto completamente casuale.
        if (assignedPositions == null ||
            assignedPositions.Count == 0)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    availablePoints.Count
                );


            result =
                availablePoints[
                    randomIndex
                ];


            return true;
        }


        int bestIndex =
            -1;


        float bestMinimumDistance =
            -1f;


        for (int i = 0;
             i < availablePoints.Count;
             i++)
        {
            Vector2 candidate =
                availablePoints[i];


            float minimumDistance =
                float.MaxValue;


            for (int j = 0;
                 j < assignedPositions.Count;
                 j++)
            {
                float distanceSquared =
                    (
                        candidate -
                        assignedPositions[j]
                    ).sqrMagnitude;


                if (distanceSquared <
                    minimumDistance)
                {
                    minimumDistance =
                        distanceSquared;
                }
            }


            if (minimumDistance >
                bestMinimumDistance)
            {
                bestMinimumDistance =
                    minimumDistance;


                bestIndex =
                    i;
            }
        }


        if (bestIndex < 0)
            return false;


        result =
            availablePoints[
                bestIndex
            ];


        return true;
    }


    // ============================================================
    // SELECT NEAREST POINT
    // ============================================================

    private static bool TryGetNearestSavedSpawnPoint(
        List<Vector2> availablePoints,
        Vector2 center,
        out Vector2 result)
    {
        result =
            Vector2.zero;


        if (availablePoints == null ||
            availablePoints.Count == 0)
        {
            return false;
        }


        int bestIndex =
            -1;


        float bestDistance =
            float.MaxValue;


        for (int i = 0;
             i < availablePoints.Count;
             i++)
        {
            Vector2 candidate =
                availablePoints[i];


            float distance =
                (
                    candidate -
                    center
                ).sqrMagnitude;


            if (distance <
                bestDistance)
            {
                bestDistance =
                    distance;


                bestIndex =
                    i;
            }
        }


        if (bestIndex < 0)
            return false;


        result =
            availablePoints[
                bestIndex
            ];


        return true;
    }


    // ============================================================
    // HARDCODED MAP SPAWN POINTS
    // ============================================================

    private static List<Vector2> GetSavedSpawnPointsForCurrentMap()
    {
        int mapId;


        try
        {
            mapId =
                Convert.ToInt32(
                    Utils.GetCurrentMap()
                );
        }
        catch
        {
            return new List<Vector2>();
        }


        switch (mapId)
        {
            // ====================================================
            // MAP 0 - THE SKELD
            // ====================================================

            case 0:
                return new List<Vector2>
                {
                    new Vector2(-4.211f, 5.635f),
                    new Vector2(4.744f, 2.722f),
                    new Vector2(6.545f, -3.935f),
                    new Vector2(16.745f, -4.678f),
                    new Vector2(9.760f, -13.135f),
                    new Vector2(3.927f, -16.378f),
                    new Vector2(-0.033f, -17.463f),
                    new Vector2(-8.198f, -8.971f),
                    new Vector2(-16.135f, -13.115f),
                    new Vector2(-13.026f, -4.411f),
                    new Vector2(-22.263f, -8.484f),
                    new Vector2(-21.926f, -3.097f),
                    new Vector2(-17.920f, 2.204f),
                    new Vector2(-9.437f, -4.415f),
                    new Vector2(6.243f, -8.363f),
                    new Vector2(-3.962f, -11.780f)
                };


            // ====================================================
            // MAP 1 - MIRA HQ
            // ====================================================

            case 1:
                return new List<Vector2>
                {
                    new Vector2(16.295f, -0.244f),
                    new Vector2(15.754f, 4.622f),
                    new Vector2(19.914f, 4.478f),
                    new Vector2(24.886f, -2.704f),
                    new Vector2(28.574f, 4.902f),
                    new Vector2(15.059f, 19.803f),
                    new Vector2(21.808f, 20.140f),
                    new Vector2(16.401f, 23.975f),
                    new Vector2(4.462f, 0.189f),
                    new Vector2(6.057f, 6.863f),
                    new Vector2(11.452f, 13.486f),
                    new Vector2(0.908f, 13.735f)
                };


            // ====================================================
            // MAP 2 - POLUS
            // ====================================================

            case 2:
                return new List<Vector2>
                {
                    new Vector2(16.640f, -1.460f),
                    new Vector2(4.426f, -4.269f),
                    new Vector2(2.910f, -12.192f),
                    new Vector2(1.497f, -17.243f),
                    new Vector2(9.292f, -25.751f),
                    new Vector2(12.426f, -16.123f),
                    new Vector2(12.689f, -24.179f),
                    new Vector2(21.315f, -16.817f),
                    new Vector2(31.244f, -7.883f),
                    new Vector2(39.593f, -9.869f),
                    new Vector2(36.911f, -22.474f),
                    new Vector2(25.775f, -25.272f),
                    new Vector2(23.748f, -3.088f),
                    new Vector2(4.189f, -11.584f),
                    new Vector2(32.680f, -16.096f)
                };


            // ====================================================
            // MAP 4 - AIRSHIP
            // ====================================================

            case 4:
                return new List<Vector2>
                {
                    new Vector2(-14.101f, -16.576f),
                    new Vector2(-13.468f, -5.512f),
                    new Vector2(-22.298f, -1.994f),
                    new Vector2(-8.555f, 7.946f),
                    new Vector2(8.421f, 14.845f),
                    new Vector2(12.008f, 8.306f),
                    new Vector2(12.142f, 1.938f),
                    new Vector2(30.364f, 7.488f),
                    new Vector2(36.647f, -3.739f),
                    new Vector2(18.491f, -4.130f),
                    new Vector2(10.533f, -16.762f),
                    new Vector2(0.953f, -1.537f),
                    new Vector2(21.769f, 2.478f),
                    new Vector2(28.174f, -2.397f),
                    new Vector2(25.621f, -8.500f)
                };


            // ====================================================
            // MAP 5 - FUNGLE
            // ====================================================

            case 5:
                return new List<Vector2>
                {
                    new Vector2(-21.782f, 2.736f),
                    new Vector2(-13.181f, -9.898f),
                    new Vector2(-4.581f, -9.102f),
                    new Vector2(23.980f, -8.196f),
                    new Vector2(-0.747f, -2.580f),
                    new Vector2(-8.373f, 10.795f),
                    new Vector2(23.748f, 13.665f),
                    new Vector2(20.131f, 7.254f),
                    new Vector2(7.351f, 0.705f),
                    new Vector2(11.239f, -1.089f),
                    new Vector2(10.089f, -10.627f),
                    new Vector2(-4.860f, -15.803f),
                    new Vector2(-10.390f, 2.789f),
                    new Vector2(-17.029f, 6.469f),
                    new Vector2(2.489f, 3.663f),
                    new Vector2(3.040f, 6.035f)
                };
        }


        return new List<Vector2>();
    }


    // ============================================================
    // PLAYER SHUFFLE
    // ============================================================

    private static void ShufflePlayers(
        List<PlayerControl> players)
    {
        if (players == null)
            return;


        for (int i = players.Count - 1;
             i > 0;
             i--)
        {
            int j =
                UnityEngine.Random.Range(
                    0,
                    i + 1
                );


            PlayerControl temp =
                players[i];


            players[i] =
                players[j];


            players[j] =
                temp;
        }
    }


    // ============================================================
    // AFTER MEETING
    // ============================================================

    public static void TryStartAfterMeeting()
    {
        try
        {
            GameModeType gameMode =
                Options.GameMode.Selected;


            if (gameMode != GameModeType.FFA &&
                gameMode != GameModeType.FFATeam)
            {
                return;
            }


            bool enabled =
                (
                    Options.RandomVentSpawnffa != null &&
                    Options.RandomVentSpawnffa.GetBool()
                )
                ||
                (
                    Options.RandomVentSpawnffa1 != null &&
                    Options.RandomVentSpawnffa1.GetBool()
                );


            if (!enabled)
                return;


            if (Running)
                return;


            AlreadyTriggered =
                false;


            TryStart();
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[RandomVentSpawn] After meeting failed: " +
                ex
            );
        }
    }


    // ============================================================
    // VANILLA - BOOT FROM VENTS
    // ============================================================

    private static IEnumerator CoBootPlayersFromRandomVents()
    {
        VentilationSystem system =
            null;


        List<byte> queuedPlayerIds =
            new List<byte>();


        try
        {
            yield return null;
            yield return null;


            system =
                GetVentSystem();


            if (system == null ||
                system.PlayersInsideVents == null)
            {
                yield break;
            }


            List<PlayerControl> players =
                GetValidPlayers();


            List<Vent> vents =
                GetRandomizedVents();


            if (players.Count <= 0 ||
                vents.Count <= 0)
            {
                yield break;
            }


            Dictionary<byte, List<PlayerControl>> playersByColor =
                BuildPlayerGroupsByColor(
                    players
                );


            Dictionary<byte, Vent> ventByColor =
                BuildVentAssignments(
                    playersByColor,
                    vents
                );


            List<int> ventsToBoot =
                new List<int>();


            for (int i = 0;
                 i < players.Count;
                 i++)
            {
                PlayerControl player =
                    players[i];


                if (player == null ||
                    player.Data == null ||
                    player.Data.Disconnected ||
                    player.Data.IsDead)
                {
                    continue;
                }


                byte colorId =
                    (byte)player.Data
                        .DefaultOutfit
                        .ColorId;


                if (!ventByColor.TryGetValue(
                        colorId,
                        out Vent vent))
                {
                    vent =
                        GetRandomVent(
                            vents
                        );
                }


                if (vent == null)
                    continue;


                string roomName =
                    GetVentRoomKey(
                        vent
                    );


                byte ventId =
                    (byte)vent.Id;


                system.PlayersInsideVents[
                    player.PlayerId
                ] = ventId;


                queuedPlayerIds.Add(
                    player.PlayerId
                );


                if (!ventsToBoot.Contains(
                        vent.Id))
                {
                    ventsToBoot.Add(
                        vent.Id
                    );
                }


                Debug.Log(
                    "[RandomVentSpawn] Queued " +
                    player.Data.PlayerName +
                    " (color " +
                    colorId +
                    ") from vent " +
                    vent.Id +
                    ", room " +
                    roomName
                );
            }


            for (int i = 0;
                 i < ventsToBoot.Count;
                 i++)
            {
                VentilationSystem.Update(
                    VentilationSystem.Operation.BootImpostors,
                    ventsToBoot[i]
                );
            }


            yield return null;
        }
        finally
        {
            if (system != null &&
                system.PlayersInsideVents != null)
            {
                for (int i = 0;
                     i < queuedPlayerIds.Count;
                     i++)
                {
                    try
                    {
                        if (system.PlayersInsideVents.ContainsKey(
                                queuedPlayerIds[i]))
                        {
                            system.PlayersInsideVents.Remove(
                                queuedPlayerIds[i]
                            );
                        }
                    }
                    catch
                    {
                    }
                }
            }


            Running =
                false;
        }
    }


    // ============================================================
    // GROUP PLAYERS BY COLOR
    // ============================================================

    private static Dictionary<byte, List<PlayerControl>>
        BuildPlayerGroupsByColor(
            List<PlayerControl> players)
    {
        Dictionary<byte, List<PlayerControl>> result =
            new Dictionary<byte, List<PlayerControl>>();


        if (players == null)
            return result;


        for (int i = 0;
             i < players.Count;
             i++)
        {
            PlayerControl player =
                players[i];


            if (player == null ||
                player.Data == null ||
                player.Data.Disconnected ||
                player.Data.IsDead)
            {
                continue;
            }


            byte colorId =
                (byte)player.Data
                    .DefaultOutfit
                    .ColorId;


            if (!result.TryGetValue(
                    colorId,
                    out List<PlayerControl> colorPlayers))
            {
                colorPlayers =
                    new List<PlayerControl>();


                result[colorId] =
                    colorPlayers;
            }


            colorPlayers.Add(
                player
            );
        }


        return result;
    }


    // ============================================================
    // VANILLA VENT ASSIGNMENT
    // ============================================================

    private static Dictionary<byte, Vent>
        BuildVentAssignments(
            Dictionary<byte, List<PlayerControl>> playersByColor,
            List<Vent> allVents)
    {
        Dictionary<byte, Vent> result =
            new Dictionary<byte, Vent>();


        List<Vent> assignedVents =
            new List<Vent>();


        List<byte> singleColors =
            new List<byte>();


        List<byte> duplicatedColors =
            new List<byte>();


        foreach (
            KeyValuePair<byte, List<PlayerControl>> entry
            in playersByColor)
        {
            if (entry.Value != null &&
                entry.Value.Count > 1)
            {
                duplicatedColors.Add(
                    entry.Key
                );

                continue;
            }


            singleColors.Add(
                entry.Key
            );
        }


        ShuffleColorIds(
            singleColors
        );


        ShuffleColorIds(
            duplicatedColors
        );


        AssignColorsToVents(
            singleColors,
            playersByColor,
            allVents,
            assignedVents,
            result
        );


        AssignColorsToVents(
            duplicatedColors,
            playersByColor,
            allVents,
            assignedVents,
            result
        );


        return result;
    }


    private static void AssignColorsToVents(
        List<byte> colors,
        Dictionary<byte, List<PlayerControl>> playersByColor,
        List<Vent> allVents,
        List<Vent> assignedVents,
        Dictionary<byte, Vent> result)
    {
        for (int i = 0;
             i < colors.Count;
             i++)
        {
            byte colorId =
                colors[i];


            Vent selectedVent =
                SelectFarthestAvailableVent(
                    allVents,
                    assignedVents
                );


            if (selectedVent == null)
                continue;


            result[colorId] =
                selectedVent;


            assignedVents.Add(
                selectedVent
            );


            int playerCount =
                playersByColor[
                    colorId
                ].Count;


            Debug.Log(
                "[RandomVentSpawn] Vent " +
                selectedVent.Id +
                " assigned to color " +
                colorId +
                " (" +
                playerCount +
                " player(s))"
            );
        }
    }


    private static Vent SelectFarthestAvailableVent(
        List<Vent> allVents,
        List<Vent> assignedVents)
    {
        if (allVents == null ||
            allVents.Count == 0)
        {
            return null;
        }


        bool hasFreeVent =
            HasFreeVent(
                allVents,
                assignedVents
            );


        if (assignedVents == null ||
            assignedVents.Count == 0)
        {
            return GetRandomFreeVent(
                allVents,
                assignedVents
            );
        }


        Vent bestVent =
            null;


        float bestMinimumDistance =
            -1f;


        for (int i = 0;
             i < allVents.Count;
             i++)
        {
            Vent candidate =
                allVents[i];


            if (candidate == null)
                continue;


            if (hasFreeVent &&
                IsVentAssigned(
                    candidate,
                    assignedVents
                ))
            {
                continue;
            }


            float minimumDistance =
                GetMinimumDistanceSquared(
                    candidate,
                    assignedVents
                );


            if (minimumDistance >
                bestMinimumDistance)
            {
                bestMinimumDistance =
                    minimumDistance;


                bestVent =
                    candidate;
            }
        }


        return bestVent ??
               GetRandomFreeVent(
                   allVents,
                   assignedVents
               );
    }


    private static bool HasFreeVent(
        List<Vent> allVents,
        List<Vent> assignedVents)
    {
        for (int i = 0;
             i < allVents.Count;
             i++)
        {
            Vent vent =
                allVents[i];


            if (vent != null &&
                !IsVentAssigned(
                    vent,
                    assignedVents
                ))
            {
                return true;
            }
        }


        return false;
    }


    private static bool IsVentAssigned(
        Vent vent,
        List<Vent> assignedVents)
    {
        if (vent == null ||
            assignedVents == null)
        {
            return false;
        }


        for (int i = 0;
             i < assignedVents.Count;
             i++)
        {
            Vent assignedVent =
                assignedVents[i];


            if (assignedVent != null &&
                assignedVent.Id == vent.Id)
            {
                return true;
            }
        }


        return false;
    }


    private static Vent GetRandomFreeVent(
        List<Vent> allVents,
        List<Vent> assignedVents)
    {
        List<Vent> freeVents =
            new List<Vent>();


        for (int i = 0;
             i < allVents.Count;
             i++)
        {
            Vent vent =
                allVents[i];


            if (vent != null &&
                !IsVentAssigned(
                    vent,
                    assignedVents
                ))
            {
                freeVents.Add(
                    vent
                );
            }
        }


        if (freeVents.Count > 0)
        {
            return GetRandomVent(
                freeVents
            );
        }


        return GetRandomVent(
            allVents
        );
    }


    private static Vent GetRandomVent(
        List<Vent> allVents)
    {
        if (allVents == null ||
            allVents.Count == 0)
        {
            return null;
        }


        return allVents[
            UnityEngine.Random.Range(
                0,
                allVents.Count
            )
        ];
    }


    // ============================================================
    // SHUFFLE COLORS
    // ============================================================

    private static void ShuffleColorIds(
        List<byte> colors)
    {
        if (colors == null)
            return;


        for (int i = colors.Count - 1;
             i > 0;
             i--)
        {
            int j =
                UnityEngine.Random.Range(
                    0,
                    i + 1
                );


            byte temp =
                colors[i];


            colors[i] =
                colors[j];


            colors[j] =
                temp;
        }
    }


    // ============================================================
    // VENT DISTANCE
    // ============================================================

    private static float GetMinimumDistanceSquared(
        Vent candidate,
        List<Vent> assignedVents)
    {
        if (candidate == null)
            return -1f;


        Vector2 candidatePosition =
            (Vector2)candidate.transform.position;


        float minimumDistance =
            float.MaxValue;


        for (int i = 0;
             i < assignedVents.Count;
             i++)
        {
            Vent assignedVent =
                assignedVents[i];


            if (assignedVent == null)
                continue;


            Vector2 assignedPosition =
                (Vector2)assignedVent
                    .transform
                    .position;


            float distanceSquared =
                (
                    candidatePosition -
                    assignedPosition
                ).sqrMagnitude;


            if (distanceSquared <
                minimumDistance)
            {
                minimumDistance =
                    distanceSquared;
            }
        }


        return minimumDistance;
    }


    // ============================================================
    // VENT ROOM NAME
    // ============================================================

    private static string GetVentRoomKey(
        Vent vent)
    {
        if (vent == null)
            return "Unknown";


        try
        {
            if (BanMod.RoomZoneManagerInstance != null)
            {
                var room =
                    BanMod.RoomZoneManagerInstance
                        .GetCurrentRoom(
                            (Vector2)vent.transform.position,
                            Utils.GetCurrentMap()
                        );


                if (room != null &&
                    !string.IsNullOrWhiteSpace(
                        room.RoomName))
                {
                    return room.RoomName;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[RandomVentSpawn] Could not detect " +
                "room for vent " +
                vent.Id +
                ": " +
                ex.Message
            );
        }


        return "Vent_" +
               vent.Id;
    }


    // ============================================================
    // VALID PLAYERS
    // ============================================================

    private static List<PlayerControl> GetValidPlayers()
    {
        List<PlayerControl> result =
            new List<PlayerControl>();


        try
        {
            for (int i = 0;
                 i < PlayerControl.AllPlayerControls.Count;
                 i++)
            {
                PlayerControl player =
                    PlayerControl.AllPlayerControls[i];


                if (player == null ||
                    player.Data == null)
                {
                    continue;
                }


                if (player.Data.Disconnected ||
                    player.Data.IsDead)
                {
                    continue;
                }


                if (player.isDummy)
                    continue;

                if (!Options.GameMode.GetValue(GameModeType.FFA) &&
                    !Options.GameMode.GetValue(GameModeType.FFATeam) &&
                    GameManager.Instance.IsHideAndSeek() &&
                    player.Data.Role != null &&
                    player.Data.Role.IsImpostor)
                {
                    continue;
                }

                result.Add(
                    player
                );
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[RandomSpawn] Could not collect players: " +
                ex
            );
        }


        result.Sort(
            (a, b) =>
                a.PlayerId.CompareTo(
                    b.PlayerId
                )
        );


        return result;
    }


    // ============================================================
    // RANDOMIZED VENTS
    // ============================================================

    private static List<Vent> GetRandomizedVents()
    {
        List<Vent> vents =
            new List<Vent>();


        try
        {
            if (ShipStatus.Instance == null ||
                ShipStatus.Instance.AllVents == null)
            {
                return vents;
            }


            foreach (
                Vent vent
                in ShipStatus.Instance.AllVents)
            {
                if (vent == null)
                    continue;


                if (vent.gameObject == null ||
                    !vent.gameObject.activeInHierarchy)
                {
                    continue;
                }


                vents.Add(
                    vent
                );
            }


            for (int i = vents.Count - 1;
                 i > 0;
                 i--)
            {
                int j =
                    UnityEngine.Random.Range(
                        0,
                        i + 1
                    );


                Vent temp =
                    vents[i];


                vents[i] =
                    vents[j];


                vents[j] =
                    temp;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[RandomVentSpawn] Could not collect vents: " +
                ex
            );
        }


        return vents;
    }


    // ============================================================
    // VENT SYSTEM
    // ============================================================

    private static VentilationSystem GetVentSystem()
    {
        try
        {
            if (ShipStatus.Instance == null ||
                ShipStatus.Instance.Systems == null)
            {
                return null;
            }


            ISystemType systemType;


            if (!ShipStatus.Instance.Systems.TryGetValue(
                    SystemTypes.Ventilation,
                    out systemType))
            {
                return null;
            }


            return systemType?
                .TryCast<VentilationSystem>();
        }
        catch
        {
            return null;
        }
    }
}


// ================================================================
// RESET PATCH
// ================================================================

[HarmonyPatch(
    typeof(GameStartManager),
    nameof(GameStartManager.Start)
)]
internal static class RandomVentSpawnResetPatch
{
    private static void Postfix()
    {
        RandomVentSpawnManager.Reset();
    }
}


// ================================================================
// INTRO END
// ================================================================

[HarmonyPatch(
    typeof(IntroCutscene),
    "OnDestroy"
)]
internal static class RandomVentSpawnIntroEndPatch
{
    private static void Postfix()
    {
        RandomVentSpawnManager.TryStart();
    }
}


// ================================================================
// AFTER MEETING
// ================================================================

[HarmonyPatch(
    typeof(ExileController),
    nameof(ExileController.WrapUp)
)]
internal static class RandomVentSpawnAfterMeetingPatch
{
    private static void Postfix()
    {
        RandomVentSpawnManager.TryStartAfterMeeting();
    }
}