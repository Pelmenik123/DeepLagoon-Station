using Content.Server.Power.NodeGroups;

namespace Content.Server.Power.Components
{
    [RegisterComponent]
    public sealed partial class BatteryDischargerComponent : BasePowerNetComponent
    {
        protected override void AddSelfToNet(EntityUid owner, IPowerNet net)
        {
            net.AddDischarger((owner, this));
        }

        protected override void RemoveSelfFromNet(EntityUid owner, IPowerNet net)
        {
            net.RemoveDischarger((owner, this));
        }
    }
}
