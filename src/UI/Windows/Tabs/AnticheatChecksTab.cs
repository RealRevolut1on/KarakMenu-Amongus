using UnityEngine;

namespace KarakMenu;

public class AnticheatChecksTab : ITab
{
    public string name => "Checks";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawRpcChecks();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGameDataChecks();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    // One toggle per RPC, straight out of the check registry. No inner
    // scroll view on purpose: KarakUI.Toggle hit-tests Event.current
    // directly, which does not account for a nested scroll view's offset.
    // The menu's own content scroll handles the overflow.
    private void DrawRpcChecks()
    {
        KarakUI.BeginSection("RPC Checks");
        foreach (var entry in Anticheat.RpcHandlers)
            entry.Value.Enabled = KarakUI.Toggle(entry.Value.Enabled, entry.Key.ToString(), "ac-rpc-" + entry.Key);
        KarakUI.EndSection();
    }

    private void DrawGameDataChecks()
    {
        KarakUI.BeginSection("Game Data Checks");
        Anticheat.EnableGameDataChecks = KarakUI.Toggle(Anticheat.EnableGameDataChecks, "Enable", "ac-gamedata");
        foreach (var entry in Anticheat.GameDataHandlers)
            entry.Value.Enabled = KarakUI.Toggle(entry.Value.Enabled, entry.Key.ToString(), "ac-gd-" + entry.Key);
        KarakUI.EndSection();
    }
}
