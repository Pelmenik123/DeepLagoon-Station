using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.NodeGroups;

namespace Content.Server.Power.Components
{
    // TODO find a way to just remove this or turn it into one component.
    // Component interface queries require enumerating over ALL of an entities components.
    // So BaseNetConnectorNodeGroup<TNetType> is slow as shit.
    public interface IBaseNetConnectorComponent<in TNetType>
    {
        void SetNet(EntityUid owner, TNetType? net);
        Voltage Voltage { get; }
        string? NodeId { get; }
    }

    public abstract partial class BaseNetConnectorComponent<TNetType> : Component, IBaseNetConnectorComponent<TNetType>
        where TNetType : class
    {
        [Dependency] private IEntityManager _entMan = default!;

        [ViewVariables(VVAccess.ReadWrite)]
        public Voltage Voltage { get => _voltage; set => SetVoltage(value); }
        [DataField("voltage")]
        private Voltage _voltage = Voltage.High;

        [ViewVariables]
        public TNetType? Net => _net;
        private TNetType? _net;

        [ViewVariables] public bool NeedsNet => _net != null;

        [DataField("node")] public string? NodeId { get; set; }

        public void TryFindAndSetNet(EntityUid owner)
        {
            if (TryFindNet(owner, out var net))
            {
                SetNet(owner, net);
            }
        }

        public void ClearNet(EntityUid owner)
        {
            if (_net != null)
            {
                RemoveSelfFromNet(owner, _net);
                _net = null;
            }
        }

        protected abstract void AddSelfToNet(EntityUid owner, TNetType net);

        protected abstract void RemoveSelfFromNet(EntityUid owner, TNetType net);

        private bool TryFindNet(EntityUid owner, [NotNullWhen(true)] out TNetType? foundNet)
        {
            if (_entMan.TryGetComponent(owner, out NodeContainerComponent? container))
            {
                var compatibleNet = container.Nodes.Values
                    .Where(node => (NodeId == null || NodeId == node.Name) && node.NodeGroupID == (NodeGroupID)Voltage)
                    .Select(node => node.NodeGroup)
                    .OfType<TNetType>()
                    .FirstOrDefault();

                if (compatibleNet != null)
                {
                    foundNet = compatibleNet;
                    return true;
                }
            }
            foundNet = default;
            return false;
        }

        public void SetNet(EntityUid owner, TNetType? newNet)
        {
            if (_net != null)
                RemoveSelfFromNet(owner, _net);

            if (newNet != null)
                AddSelfToNet(owner, newNet);

            _net = newNet;
        }

        private void SetVoltage(Voltage newVoltage)
        {
#pragma warning disable CS0618 // Matches upstream SS14: the VV setter has no EntityUid; system paths now pass the owner explicitly.
            var owner = Owner;
#pragma warning restore CS0618
            ClearNet(owner);
            _voltage = newVoltage;
            TryFindAndSetNet(owner);
        }
    }

    public enum Voltage
    {
        High = NodeGroupID.HVPower,
        Medium = NodeGroupID.MVPower,
        Apc = NodeGroupID.Apc,
    }
}
