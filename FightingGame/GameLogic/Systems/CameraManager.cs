using FightingGame.GameLogic.Systems.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FightingGame.GameLogic.Systems
{
    public struct CameraTarget { }

    public struct Camera { }

    public class CameraManager : SystemBase, ILoadableSystem, IUpdatableSystem
    {
        public const float CAMERA_WIDTH = 600;
        public const float CAMERA_MOVEMENT_SPEED = 0.1f;

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public void Load(IEditableWorld _)
        {
            Projection = Matrix.CreateOrthographic(
                CAMERA_WIDTH,
                CAMERA_WIDTH / Game.ASPECT_RATIO,
                0,
                10);
        }

        public void Update(IEditableWorld editableWorld, long frame)
        {
            var targetPositions = editableWorld.Entities
                .Where(entity => entity.Has<CameraTarget>())
                .Select(entity => entity.Get<Position>());

            var camera = editableWorld.Entities.FirstOrDefault(entity => entity.Has<Camera>());
            if (camera == null)
            {
                camera = new Entity(
                    Position.Zero,
                    new Camera());
            }
            var cameraPositionComponent = camera.Get<Position>();

            if (targetPositions.Any())
            {
                var averageTargetPosition = targetPositions.Aggregate(Vector2.Zero, (acc, position) => position.ToVector() + acc) / targetPositions.Count();
                var targetLeftOfCamera = averageTargetPosition.X - CAMERA_WIDTH / 2;
                var targetPosition =
                    new Vector2(
                        (int)Math.Floor(targetLeftOfCamera / CAMERA_WIDTH) * CAMERA_WIDTH + CAMERA_WIDTH,
                        averageTargetPosition.Y);

                var newPosition = cameraPositionComponent.ToVector() + ((targetPosition - cameraPositionComponent.ToVector()) * CAMERA_MOVEMENT_SPEED);
                camera = camera.Set(new Position((int)newPosition.X, (int)newPosition.Y));
            }

            editableWorld.UpdateEntity(camera);

            View = Matrix.CreateTranslation(new Vector3(-cameraPositionComponent.ToVector(), 0));
        }
    }
}
