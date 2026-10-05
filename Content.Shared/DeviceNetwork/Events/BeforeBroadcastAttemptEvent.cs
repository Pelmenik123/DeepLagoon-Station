using Content.Shared.DeviceNetwork.Components;

namespace Content.Shared.DeviceNetwork.Events;

/// <summary>
/// Sent to the sending entity before broadcasting network packets to recipients
/// </summary>
public sealed class BeforeBroadcastAttemptEvent(IReadOnlySet<Device> recipients) : CancellableEntityEventArgs
{
    public readonly IReadOnlySet<Device> Recipients = recipients;
    public HashSet<Device>? ModifiedRecipients;
}
