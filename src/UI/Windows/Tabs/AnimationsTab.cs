using UnityEngine;

namespace KarakMenu;

public class AnimationsTab : ITab
{
    public string name => "Animations";

    public void Draw()
    {
        DrawGeneral();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.animShields = KarakUI.Toggle(CheatToggles.animShields, "Shields");
        CheatToggles.animAsteroids = KarakUI.Toggle(CheatToggles.animAsteroids, "Asteroids");
        CheatToggles.animEmptyGarbage = KarakUI.Toggle(CheatToggles.animEmptyGarbage, "Empty Garbage");
        CheatToggles.animMedScan = KarakUI.Toggle(CheatToggles.animMedScan, "Medbay Scan");
        CheatToggles.animCamsInUse = KarakUI.Toggle(CheatToggles.animCamsInUse, "Cams In Use");
        CheatToggles.moonWalk = KarakUI.Toggle(CheatToggles.moonWalk, "Moonwalk");
        KarakUI.EndSection();
    }
}
