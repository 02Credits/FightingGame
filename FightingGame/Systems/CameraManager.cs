using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public struct CameraTarget { }

    public struct Camera { }

    public class CameraManager : ILoadedSystem, IUpdatedSystem
    {
        public const float CAMERA_MOVEMENT_SPEED = 0.1f;

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public void Load()
        {
            Projection = Matrix.CreateOrthographic(
                Game.ScreenWidth,
                Game.ScreenWidth * (float)Game.ScreenAspectRatio,
                0,
                10);
        }

        public void Update()
        {
            var targetPositions = Game.Entities
                .Where(entity => entity.Has<CameraTarget>())
                .Select(entity => entity.Get<Position>().Value);

            var camera = Game.Entities.First(entity => entity.Has<Camera>());
            var cameraPositionComponent = camera.Get<Position>();

            if (targetPositions.Any())
            {
                var averageTargetPosition = targetPositions.Aggregate(Vector2.Zero, (acc, position) => position + acc) / targetPositions.Count();
                var targetLeftOfCamera = averageTargetPosition.X - Game.ScreenWidth / 2;
                var targetPosition =
                    new Vector2(
                        (int)Math.Floor(targetLeftOfCamera / Game.ScreenWidth) * Game.ScreenWidth + Game.ScreenWidth,
                        averageTargetPosition.Y);

                cameraPositionComponent.Value += (targetPosition - cameraPositionComponent.Value) * CAMERA_MOVEMENT_SPEED;
                camera.Set(cameraPositionComponent);
            }

            View = Matrix.CreateTranslation(new Vector3(-cameraPositionComponent.Value, 0));
        }
    }
}
