using AmongUs.Data;

namespace KarakMenu;
public static class KarakSpoof
{
    private static uint _lastAppliedTarget;

    public static void SpoofLevel()
    {
        var playerData = DataManager.Player;
        if (playerData == null || playerData.Stats == null) return;

        if (!uint.TryParse(KarakMenu.spoofLevel.Value, out uint target) || target < 1)
        {
            _lastAppliedTarget = 0;
            return;
        }

        // 1. Keep the local stats the game reads from in sync. Stats.Level is
        //    the 1-based value the UI shows; the private `level` field behind
        //    it is 0-based, so probe both and keep whichever reads back right.
        if (!(target == _lastAppliedTarget && playerData.Stats.Level == target))
        {
            playerData.stats.level = target;
            if (playerData.Stats.Level != target)
                playerData.stats.level = target - 1;
        }

        // 2. What everyone else actually sees comes from
        //    NetworkedPlayerInfo.PlayerLevel, which is only ever set by the
        //    SetLevel RPC. Rewriting DataManager alone changes nothing for
        //    them — so push it once whenever the requested value changes.
        //    The wire value is 0-based (the nametag renders PlayerLevel + 1).
        if (_lastAppliedTarget != target)
        {
            _lastAppliedTarget = target;
            var local = PlayerControl.LocalPlayer;
            if (local != null)
                local.RpcSetLevel(target - 1);
        }
    }

    public static string SpoofFriendCode()
    {
        string friendCode = KarakMenu.guestFriendCode.Value;
        if (string.IsNullOrWhiteSpace(friendCode))
        {
            friendCode = DestroyableSingleton<AccountManager>.Instance.GetRandomName();
        }
        return friendCode;
    }
}
