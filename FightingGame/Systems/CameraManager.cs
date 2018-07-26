using FightingGame.Components;
using FightingGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public class CameraManager : ILoadedSystem, IUpdatedSystem
    {
        public const float WORLD_WIDTH = 300;
        public const float CAMERA_MOVEMENT_SPEED = 0.1f;

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public float CameraWorldHeight { get; set; }

        public Vector2 CameraPosition { get; set; }

        GraphicsDevice graphics;

        public CameraManager(GraphicsDevice graphics)
        {
            this.graphics = graphics;
        }

        public void Load()
        {
            CameraWorldHeight = WORLD_WIDTH * graphics.Viewport.Height / graphics.Viewport.Width;

            Projection = Matrix.CreateOrthographic(
                WORLD_WIDTH,
                CameraWorldHeight,
                0,
                10);
        }

        public void Update()
        {
            var positionedTargets =
                Game.Entities.Where((t) => t.HasComponent<CameraTarget>())
                             .Select((t) => t.GetComponent<Position>().Value);
            if (positionedTargets.Any())
            {
                var averageTargetPosition =
                    positionedTargets.Aggregate(Vector2.Zero, (acc, position) => position + acc) / positionedTargets.Count();

                var targetLeftOfCamera = averageTargetPosition.X - WORLD_WIDTH / 2;
                var targetPosition =
                    new Vector2(
                        (int)Math.Floor(targetLeftOfCamera / WORLD_WIDTH) * WORLD_WIDTH + WORLD_WIDTH,
                        averageTargetPosition.Y);

                CameraPosition += (targetPosition - CameraPosition) * CAMERA_MOVEMENT_SPEED;
            }

            View = Matrix.CreateTranslation(new Vector3(-CameraPosition, 0));
        }
    }
}
