using Robust.Shared.Random;
using Content.Shared.EntityTable.EntitySelectors;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityTable.ValueSelector;

/// <summary>
/// Used for implementing custom value selection for <see cref="EntityTableSelector"/>
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class NumberSelector
{
    public abstract float Get(IRobustRandom rand,
        IEntityManager entMan,
        IPrototypeManager proto);
}
