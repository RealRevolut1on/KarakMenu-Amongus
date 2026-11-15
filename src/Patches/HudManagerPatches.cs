using HarmonyLib;
using System;

namespace KarakMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManager_Start
{
	// Postfix patch of HudManager.Start to give minimap access to impostors too
	public static void Postfix(HudManager __instance)
	{
		__instance.MapButton.OnClick.RemoveAllListeners(); // Remove previous OnClick action

		// Always open normal map when map button is clicked
		// To access sabotage map, sabotage button can be used
		__instance.MapButton.OnClick.AddListener((Action) (() =>
        {
			__instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.Normal
			});

		}));
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
	public static void Postfix(HudManager __instance)
    {
		try
		{
			__instance.ShadowQuad.gameObject.SetActive(!KarakESP.IsFullbrightActive()); // Fullbright

			if (Utils.IsChatUiActive()) // AlwaysChat
			{
				__instance.Chat.gameObject.SetActive(true);
			}
			else
			{
				Utils.CloseChat();
				__instance.Chat.gameObject.SetActive(false);
			}

			KarakCheats.UseVentCheat(__instance);
			KarakESP.ZoomOut(__instance);
			KarakESP.FreecamCheat();

			// Close PlayerPickMenu if there is no PPM cheat enabled
			if (PlayerPickMenu.playerpickMenu != null && CheatToggles.ShouldPPMClose())
			{
				PlayerPickMenu.playerpickMenu.Close();
			}
		}
		catch { }
    }
}
