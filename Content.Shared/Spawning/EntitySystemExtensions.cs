using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Spawning
{
    public static class EntitySystemExtensions
    {
        public static EntityUid? SpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            EntityCoordinates coordinates,
            CollisionGroup collisionLayer,
            in Box2? box = null,
            SharedPhysicsSystem? physicsManager = null)
        {
            physicsManager ??= entityManager.System<SharedPhysicsSystem>();
            var mapCoordinates = entityManager.System<SharedTransformSystem>().ToMapCoordinates(coordinates);

            return entityManager.SpawnIfUnobstructed(prototypeName, mapCoordinates, collisionLayer, box, physicsManager);
        }

        public static EntityUid? SpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            MapCoordinates coordinates,
            CollisionGroup collisionLayer,
            in Box2? box = null,
            SharedPhysicsSystem? collision = null)
        {
            var boxOrDefault = box.GetValueOrDefault(Box2.UnitCentered).Translated(coordinates.Position);
            collision ??= entityManager.System<SharedPhysicsSystem>();

            var lookup = entityManager.System<EntityLookupSystem>();
            var bodies = new HashSet<PhysicsComponent>();
            lookup.ForEachFixtureIntersecting(
                coordinates.MapId,
                boxOrDefault,
                ref bodies,
                new CollectPhysicsBodiesCallback(),
                new FixtureQueryArgs(
                    new QueryFilter
                    {
                        LayerBits = -1L,
                        MaskBits = -1L,
                        Flags = QueryFlags.Dynamic | QueryFlags.Static | QueryFlags.Sensors,
                    },
                    Approximate: true));

            foreach (var body in bodies)
            {
                if (!body.Hard)
                {
                    continue;
                }

                // TODO: wtf fix this
                if (collisionLayer == 0 || (body.CollisionMask & (int)collisionLayer) == 0)
                {
                    continue;
                }

                return null;
            }

            return entityManager.SpawnEntity(prototypeName, coordinates);
        }

        private readonly struct CollectPhysicsBodiesCallback : IFixtureQueryCallback<HashSet<PhysicsComponent>>
        {
            public bool Invoke(ref HashSet<PhysicsComponent> state, in FixtureProxy fixture)
            {
                state.Add(fixture.Body);
                return true;
            }
        }

        public static bool TrySpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            EntityCoordinates coordinates,
            CollisionGroup collisionLayer,
            [NotNullWhen(true)] out EntityUid? entity,
            Box2? box = null,
            SharedPhysicsSystem? physicsManager = null)
        {
            entity = entityManager.SpawnIfUnobstructed(prototypeName, coordinates, collisionLayer, box, physicsManager);

            return entity != null;
        }

        public static bool TrySpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            MapCoordinates coordinates,
            CollisionGroup collisionLayer,
            [NotNullWhen(true)] out EntityUid? entity,
            in Box2? box = null,
            SharedPhysicsSystem? physicsManager = null)
        {
            entity = entityManager.SpawnIfUnobstructed(prototypeName, coordinates, collisionLayer, box, physicsManager);

            return entity != null;
        }
    }
}
