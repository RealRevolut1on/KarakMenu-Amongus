using HarmonyLib;

namespace KarakMenu;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatus_FixedUpdate
{
    public static void Postfix(ShipStatus __instance)
    {
        KarakSabotageCheats.Process(__instance);
        KarakCheats.OpenSabotageMapCheat();

        KarakCheats.CloseMeetingCheat();
        KarakCheats.SkipMeetingCheat();
        KarakCheats.CallMeetingCheat();
        KarakCheats.WalkInVentCheat();
        KarakCheats.KickVentsCheat();

        KarakPPMCheats.ReportBodyPPM();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class FungleShipStatus_FixedUpdate
{
    public static void Postfix(FungleShipStatus __instance)
    {
        KarakSabotageCheats.ProcessFungle(__instance);
    }
}
