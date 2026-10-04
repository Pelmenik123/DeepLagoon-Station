using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Client;

/// <summary>
/// Temporary bridges for the SpriteComponent to SpriteSystem migration.
/// </summary>
public static class SpriteComponentExt
{
    /// <summary>
    /// Resolves the client sprite system for call sites that have no injected reference.
    /// </summary>
    public static SpriteSystem Sys => IoCManager.Resolve<IEntityManager>().System<SpriteSystem>();
    /// <summary>
    /// Legacy single-post-shader id. The old <see cref="SpriteComponent.PostShader"/> property only ever
    /// supported a single shader; all legacy call sites share this id so they keep replacing each other.
    /// </summary>
    public const string LegacyPostShaderId = "legacy";

#pragma warning disable CS0618 // Bridge for hundreds of legacy SpriteComponent call sites that do not carry their EntityUid.
    public static Entity<SpriteComponent?> AsEntity(this SpriteComponent sprite)
    {
        return (sprite.Owner, sprite);
    }

    public static Entity<SpriteComponent> AsEntityComp(this SpriteComponent sprite)
    {
        return (sprite.Owner, sprite);
    }
#pragma warning restore CS0618

    public static void SetLegacyPostShader(this SpriteSystem sys, Entity<SpriteComponent?> sprite, ShaderInstance? shader,
        bool getScreenTexture = false, bool raiseShaderEvent = false)
    {
        if (shader == null)
        {
            sys.RemovePostShader(sprite, LegacyPostShaderId);
            return;
        }

        sys.SetPostShader(sprite, new SpriteComponent.PostShaderArgs(LegacyPostShaderId, shader)
        {
            GetScreenTexture = getScreenTexture,
            RaiseShaderEvent = raiseShaderEvent,
        });
    }

    public static ShaderInstance? GetLegacyPostShader(this SpriteSystem sys, Entity<SpriteComponent?> sprite)
    {
        return sys.TryGetPostShader(sprite, LegacyPostShaderId, out var entry) ? entry.Shader : null;
    }

    public static bool GetLegacyPostShaderGetScreenTexture(this SpriteSystem sys, Entity<SpriteComponent?> sprite)
    {
        return sys.TryGetPostShader(sprite, LegacyPostShaderId, out var entry) && entry.GetScreenTexture;
    }

    public static void SetLegacyPostShaderFlags(this SpriteSystem sys, Entity<SpriteComponent?> sprite,
        bool? getScreenTexture = null, bool? raiseShaderEvent = null)
    {
        if (!sys.TryGetPostShader(sprite, LegacyPostShaderId, out var entry))
            return;

        if (getScreenTexture.HasValue)
            entry.GetScreenTexture = getScreenTexture.Value;
        if (raiseShaderEvent.HasValue)
            entry.RaiseShaderEvent = raiseShaderEvent.Value;
    }
}
