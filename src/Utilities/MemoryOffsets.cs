namespace KarakMenu;

// Memory offsets from the 2026-10-10 Among Us dump (krxslaxy dumper, metadata v31).
// These are method RVAs into GameAssembly.dll, used by the Memory tab as
// reference anchors alongside the live pointer readouts.
// The dumper does not emit static-field RVAs, so these are per-class method
// anchors rather than the old static Instance pointers.
public static class MemoryOffsets
{
    public const string PlayerControlPointer = "GameAssembly.dll+6A66C0";
    public const string ShipStatusPointer = "GameAssembly.dll+72BC70";
    public const string AmongUsClientPointer = "GameAssembly.dll+61D5E0";
    public const string PlayerControlGetDataRva = "GameAssembly.dll+6B2300";
}
