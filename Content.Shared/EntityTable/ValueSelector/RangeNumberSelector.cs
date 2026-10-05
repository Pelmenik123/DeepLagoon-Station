using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.EntityTable.ValueSelector;

/// <summary>
/// Gives a value between the two numbers specified, inclusive.
/// </summary>
/// <remarks>
/// Frontier: output must be floored to have this behaviour
/// </remarks>
public sealed partial class RangeNumberSelector : NumberSelector
{
    [DataField]
    public Vector2 Range = new(1, 1);

    public override float Get(IRobustRandom rand, IEntityManager entMan, IPrototypeManager proto)
    {
        return (float)rand.NextDouble() * (Range.Y + 1 - Range.X) + Range.X;
    }
}
