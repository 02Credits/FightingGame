using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FightingGame.Networking
{
    [Flags]
    public enum KeyStatus
    {
        None = 0, // unknown
        Free = 1, // isn't and wasn't pressed
        Released = 2, // was pressed, now isn't
        Pressed = 4, // wasn't pressed, now is
        Held = 8, // was and is pressed
        Down = Pressed | Held, // is currently pressed
        Up = Free | Released // is currently unpressed
    }

    public class InputState
    {
        public static readonly IReadOnlyList<Keys> TrackedKeys = new[]
        {
            Keys.W,
            Keys.A,
            Keys.S,
            Keys.D,
            Keys.Up,
            Keys.Right,
            Keys.Down,
            Keys.Left,
            Keys.Enter,
            Keys.Space,
            Keys.LeftShift,
            Keys.Tab
        };

        public string PlayerID { get; }

        public long Frame { get; }

        public KeyStatus W { get; }
        public KeyStatus A { get; }
        public KeyStatus S { get; }
        public KeyStatus D { get; }
        public KeyStatus Up { get; }
        public KeyStatus Right { get; }
        public KeyStatus Down { get; }
        public KeyStatus Left { get; }
        public KeyStatus Enter { get; }
        public KeyStatus Space { get; }
        public KeyStatus Shift { get; }
        public KeyStatus Tab { get; }

        public InputState(
            string playerID, long frame,
            KeyStatus w = KeyStatus.Up, KeyStatus a = KeyStatus.Up, KeyStatus s = KeyStatus.Up, KeyStatus d = KeyStatus.Up, 
            KeyStatus up = KeyStatus.Up, KeyStatus right = KeyStatus.Up, KeyStatus down = KeyStatus.Up, KeyStatus left = KeyStatus.Up, 
            KeyStatus enter = KeyStatus.Up, KeyStatus space = KeyStatus.Up, KeyStatus shift = KeyStatus.Up, KeyStatus tab = KeyStatus.Up)
        {
            PlayerID = playerID;
            Frame = frame;
            W = w;
            A = a;
            S = s;
            D = d;
            Up = up;
            Right = right;
            Down = down;
            Left = left;
            Enter = enter;
            Space = space;
            Shift = shift;
            Tab = tab;
        }

        [JsonIgnore]
        public IEnumerable<Keys> PressedKeys => TrackedKeys.Where(key => (this[key] & KeyStatus.Down) == this[key]);

        public KeyStatus this[Keys key]
        {
            get
            {
                switch (key)
                {
                    case Keys.W: return W;
                    case Keys.A: return A;
                    case Keys.S: return S;
                    case Keys.D: return D;
                    case Keys.Up: return Up;
                    case Keys.Right: return Right;
                    case Keys.Down: return Down;
                    case Keys.Left: return Left;
                    case Keys.Enter: return Enter;
                    case Keys.Space: return Space;
                    case Keys.LeftShift: return Shift;
                    case Keys.Tab: return Tab;
                    default:
                        throw new ArgumentException($"Input state does not record key {key}.");
                }
            }
        }
    }
}
