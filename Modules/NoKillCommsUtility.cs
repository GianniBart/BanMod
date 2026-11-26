namespace BanMod;

internal static class NoKillCommsUtility
{
    public static bool IsMisfire(PlayerControl killer)
    {
        if (Options.GameMode == null)
            return false;

        GameModeType gameMode = Options.GameMode.Selected;
        bool nokillcoms = ((gameMode == GameModeType.SnS && Options.NokillComsSNS.GetBool()) || (gameMode == GameModeType.PnS && Options.NokillComsPNS.GetBool()));
        if (!nokillcoms || killer == null || ShipStatus.Instance == null || !Utils.IsActive(SystemTypes.Comms))
        {
            return false;
        }

        foreach (PlainShipRoom room in ShipStatus.Instance.AllRooms)
        {
            if (room == null ||
                room.RoomId != SystemTypes.Comms ||
                room.roomArea == null)
            {
                continue;
            }

            if (room.roomArea.OverlapPoint(killer.GetTruePosition()))
            {
                return true;
            }
        }

        return false;
    }
}