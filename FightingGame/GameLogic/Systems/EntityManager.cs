using FightingGame.GameLogic.Systems.Interfaces;
using System.Linq;

namespace FightingGame.GameLogic.Systems
{
    public class EntityManager : SystemBase, IUpdatableSystem, IDrawableSystem
    {
        public SystemManager SystemManager { get; set; }
        public VertexManager VertexManager { get; set; }
        public VertexRenderer VertexRenderer { get; set; }

        public void Update(IEditableWorld editableWorld, long frame)
        {
            foreach (var entity in editableWorld.Entities.ToList())
            {
                var updatedEntity = entity;

                updatedEntity = updatedEntity.Update(editableEntity =>
                {
                    foreach (var system in SystemManager.EntityUpdaterSystems)
                    {
                        if (system.SubscribedUpdateComponents.Any(updatedEntity.Has))
                        {
                                system.Update(editableEntity, frame);
                        }
                    }
                });

                editableWorld.UpdateEntity(updatedEntity);
            }
        }

        public void Draw(World world, long frame)
        {
            foreach (var entity in world.Entities)
            {
                foreach (var system in SystemManager.EntityDrawerSystems)
                {
                    if (system.SubscribedDrawComponents.Any(entity.Has))
                    {
                        system.Draw(entity, frame);
                    }
                }
            }
        }
    }
}
