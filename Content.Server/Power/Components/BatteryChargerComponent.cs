using Content.Server.Power.NodeGroups;

namespace Content.Server.Power.Components
{
    /// <summary>
    ///     Connects the loading side of a <see cref="BatteryComponent"/> to a non-APC power network.
    /// </summary>
    [RegisterComponent]
    public sealed partial class BatteryChargerComponent : BasePowerNetComponent
    {
        protected override void AddSelfToNet(EntityUid owner, IPowerNet net)
        {
            net.AddCharger((owner, this));
        }

        protected override void RemoveSelfFromNet(EntityUid owner, IPowerNet net)
        {
            net.RemoveCharger((owner, this));
        }
    }
}
