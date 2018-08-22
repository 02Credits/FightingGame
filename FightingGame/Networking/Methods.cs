using FightingGame.Systems;
using Microsoft.Xna.Framework;
using Serilog;
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
            Log.Information("Recieved Input {frameNumber}", inputFrame);
            Game.GetSystem<InputManager>().RecievedStates.Enqueue((playerId, inputFrame, inputState));
        }
    }
}
