using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Systems
{
    public struct Focusable
    {
        public bool Focused { get; set; }
        public SpriteSheet FocusedSheet { get; set; }
        public SpriteSheet UnfocusedSheet { get; set; }
        public Dictionary<Keys, Entity> NextEntities { get; set; }
        public Action Activate { get; set; }
    }

    public class UIManager : IUpdatedSystem
    {
        public void Update()
        {
            Entity focusedEntity = Game.Entities.SingleOrDefault(entity => entity.Has<Focusable>() && entity.Get<Focusable>().Focused);
            if (focusedEntity != null)
            {
                var focusableComponent = focusedEntity.Get<Focusable>();
                var inputManager = Game.GetSystem<InputManager>();
                var localState = inputManager.GetLocalInputState();

                foreach (Keys key in focusableComponent.NextEntities.Keys)
                {
                    if (localState[key] == KeyStatus.Pressed)
                    {
                        focusableComponent.Focused = false;
                        focusedEntity.Set(focusableComponent);
                        focusedEntity.Set(focusableComponent.UnfocusedSheet);

                        var newFocusedEntity = focusableComponent.NextEntities[key];
                        var newFocusableComponent = newFocusedEntity.Get<Focusable>();
                        newFocusableComponent.Focused = true;
                        newFocusedEntity.Set(newFocusableComponent);
                        newFocusedEntity.Set(newFocusableComponent.FocusedSheet);
                        focusedEntity = newFocusedEntity;
                        focusableComponent = newFocusableComponent;
                        break;
                    }
                }

                if (localState.Enter == KeyStatus.Pressed)
                {
                    focusableComponent.Activate();
                }
            }
        }
    }
}
