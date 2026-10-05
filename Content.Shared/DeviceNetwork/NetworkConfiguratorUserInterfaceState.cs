using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeviceNetwork;

[Serializable, NetSerializable]
public sealed class NetworkConfiguratorUserInterfaceState(HashSet<(string, string)> deviceList) : BoundUserInterfaceState
{
    public readonly HashSet<(string address, string name)> DeviceList = deviceList;
}

[Serializable, NetSerializable]
public sealed class DeviceListUserInterfaceState(HashSet<(string address, string name)> deviceList) : BoundUserInterfaceState
{
    public readonly HashSet<(string address, string name)> DeviceList = deviceList;
}

[Serializable, NetSerializable]
public sealed class DeviceLinkUserInterfaceState(
    ProtoId<SourcePortPrototype>[] sources,
    ProtoId<SinkPortPrototype>[] sinks,
    HashSet<(ProtoId<SourcePortPrototype> source, ProtoId<SinkPortPrototype> sink)> links,
    string sourceAddress,
    string sinkAddress,
    List<(string source, string sink)>? defaults = default,
    Dictionary<string, string>? sourcePortNames = default) : BoundUserInterfaceState
{
    public readonly ProtoId<SourcePortPrototype>[] Sources = sources;
    public readonly ProtoId<SinkPortPrototype>[] Sinks = sinks;
    public readonly HashSet<(ProtoId<SourcePortPrototype> source, ProtoId<SinkPortPrototype> sink)> Links = links;
    public readonly List<(string source, string sink)>? Defaults = defaults;
    public readonly string SourceAddress = sourceAddress;
    public readonly string SinkAddress = sinkAddress;
    public readonly Dictionary<string, string>? SourcePortNames = sourcePortNames;
}
