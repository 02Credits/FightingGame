using FightingGame.Components;
using FightingGame.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Systems
{
    public class PhysicsManager : IUpdatedSystem
    {
        public void Update()
        {
            foreach (var entity1 in Game.Entities.Where(entity => entity.HasComponent<Physics>()))
            {
                var entity1Position = entity1.GetComponent<Position>().Value;
                var entity1Dimensions = entity1.GetComponent<Textured>();
                foreach (var entity2 in Game.Entities.Where(entity => entity.HasComponent<Physics>()))
                {
                    var entity2Position = entity1.GetComponent<Position>().Value;
                    var entity2Dimensions = entity1.GetComponent<Textured>();

                    if (entity1Position.Value)
                }
            }
        }
    }
}
