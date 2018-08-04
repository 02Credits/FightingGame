using FightingGame.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Networking
{
    public static class Methods
    {
        public static void NewInput(string playerId, int inputFrame, InputState inputState)
        {
            Game.GetSystem<InputManager>().RecievedStates.Enqueue((playerId, inputFrame, inputState));
        }
    }
}
