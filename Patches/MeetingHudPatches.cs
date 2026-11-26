//credits and licenses in the resources folder/
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BanMod;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update
{
    public static readonly HashSet<byte> VotedPlayers = new();

    public static void Prefix(MeetingHud __instance)
    {
        if (__instance == null)
            return;

        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (__instance.playerStates == null ||
            GameData.Instance == null)
        {
            return;
        }

        if (__instance.state >= MeetingHud.MeetingStates.Results)
            return;

        foreach (var playerVoteArea in __instance.playerStates)
        {
            if (!playerVoteArea)
                continue;

            byte voterId = playerVoteArea.PlayerId.Value;
            byte votedForId = playerVoteArea.VotedForId;

            var playerData = GameData.Instance.GetPlayerById(voterId);

            if (playerData == null || playerData.Disconnected)
                continue;

            if (votedForId == PlayerVoteArea.HasNotVoted ||
                votedForId == PlayerVoteArea.MissedVote ||
                votedForId == PlayerVoteArea.DeadVote)
            {
                continue;
            }

            if (!VotedPlayers.Add(voterId))
                continue;

            if (votedForId == PlayerVoteArea.SkippedVote)
            {
                if (__instance.SkippedVoting)
                {
                    __instance.BloopAVoteIcon(
                        playerData,
                        0,
                        __instance.SkippedVoting.transform
                    );
                }

                continue;
            }

            foreach (var votedForArea in __instance.playerStates)
            {
                if (!votedForArea)
                    continue;

                if (votedForArea.PlayerId != votedForId)
                    continue;

                if (votedForArea.transform != null)
                {
                    __instance.BloopAVoteIcon(
                        playerData,
                        0,
                        votedForArea.transform
                    );
                }

                break;
            }
        }

        foreach (var votedForArea in __instance.playerStates)
        {
            if (!votedForArea || votedForArea.transform == null)
                continue;

            var voteSpreader =
                votedForArea.transform.GetComponent<VoteSpreader>();

            if (!voteSpreader || voteSpreader.Votes == null)
                continue;

            foreach (var spriteRenderer in voteSpreader.Votes)
            {
                if (!spriteRenderer)
                    continue;

                if (spriteRenderer.gameObject != null)
                {
                    spriteRenderer.gameObject.SetActive((BanMod.GM.Value) || (ForcedRoleSystem.GM));
                }
            }
        }

        if (__instance.SkippedVoting)
        {
            var skipSpreader =
                __instance.SkippedVoting.transform
                    .GetComponent<VoteSpreader>();

            if (skipSpreader != null &&
                skipSpreader.Votes != null)
            {
                foreach (var spriteRenderer in skipSpreader.Votes)
                {
                    if (spriteRenderer &&
                        spriteRenderer.gameObject != null)
                    {
                        spriteRenderer.gameObject.SetActive((BanMod.GM.Value) || (ForcedRoleSystem.GM));
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHud_PopulateResults
{
    public static void Prefix(MeetingHud __instance)
    {
        if (__instance == null)
            return;

        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (__instance.playerStates != null)
        {
            foreach (var votedForArea in __instance.playerStates)
            {
                if (!votedForArea ||
                    votedForArea.transform == null)
                {
                    continue;
                }

                ClearVotes(
                    votedForArea.transform.GetComponent<VoteSpreader>()
                );
            }
        }

        if (__instance.SkippedVoting)
        {
            ClearVotes(
                __instance.SkippedVoting.transform
                    .GetComponent<VoteSpreader>()
            );
        }

        MeetingHud_Update.VotedPlayers.Clear();
    }

    private static void ClearVotes(VoteSpreader voteSpreader)
    {
        if (!voteSpreader || voteSpreader.Votes == null)
            return;

        foreach (var spriteRenderer in voteSpreader.Votes)
        {
            if (!spriteRenderer)
                continue;

            if (spriteRenderer.gameObject != null)
            {
                Object.Destroy(spriteRenderer.gameObject);
            }
        }

        voteSpreader.Votes.Clear();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHudVoteResetStartPatch
{
    public static void Prefix()
    {
        MeetingHud_Update.VotedPlayers.Clear();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
public static class MeetingHudVoteResetDestroyPatch
{
    public static void Prefix()
    {
        MeetingHud_Update.VotedPlayers.Clear();
    }
}