using System;
using Robust.Client.UserInterface;

namespace Content.Client;

public static class ControlDisposeExt
{
    /// <summary>
    /// Disposes a control through <see cref="IDisposable.Dispose"/>, keeping the legacy lifecycle
    /// contract (<see cref="Control.Disposed"/> becomes true) without referencing the obsolete
    /// <see cref="Control.Dispose"/> member directly. Upstream SS14 uses <c>Control.Dispose()</c>
    /// in these places; <c>Orphan()</c> does not set <see cref="Control.Disposed"/> and breaks
    /// code that relies on it as a "this control is dead" flag.
    /// </summary>
    public static void DisposeControl(this Control control)
    {
        ((IDisposable)control).Dispose();
    }
}
