using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Engineering;

[Serializable, NetSerializable]
public sealed partial class DisassembleOnAltVerbDoAfterEvent : SimpleDoAfterEvent
{
}
