using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Systems
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
        public static readonly InputState AllUp = new InputState(
            KeyStatus.Up, KeyStatus.Up, KeyStatus.Up, KeyStatus.Up, 
            KeyStatus.Up, KeyStatus.Up, KeyStatus.Up, KeyStatus.Up, 
            KeyStatus.Up, KeyStatus.Up, KeyStatus.Up, KeyStatus.Up);

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
            KeyStatus w, KeyStatus a, KeyStatus s, KeyStatus d, 
            KeyStatus up, KeyStatus right, KeyStatus down, KeyStatus left, 
            KeyStatus enter, KeyStatus space, KeyStatus shift, KeyStatus tab)
        {
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

        public override bool Equals(object obj)
        {
            var state = obj as InputState;
            return state != null &&
                   W == state.W &&
                   A == state.A &&
                   S == state.S &&
                   D == state.D &&
                   Up == state.Up &&
                   Right == state.Right &&
                   Down == state.Down &&
                   Left == state.Left &&
                   Enter == state.Enter &&
                   Space == state.Space &&
                   Shift == state.Shift &&
                   Tab == state.Tab;
        }

        public override int GetHashCode()
        {
            var hashCode = 2056565879;
            hashCode = hashCode * -1521134295 + W.GetHashCode();
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + S.GetHashCode();
            hashCode = hashCode * -1521134295 + D.GetHashCode();
            hashCode = hashCode * -1521134295 + Up.GetHashCode();
            hashCode = hashCode * -1521134295 + Right.GetHashCode();
            hashCode = hashCode * -1521134295 + Down.GetHashCode();
            hashCode = hashCode * -1521134295 + Left.GetHashCode();
            hashCode = hashCode * -1521134295 + Enter.GetHashCode();
            hashCode = hashCode * -1521134295 + Space.GetHashCode();
            hashCode = hashCode * -1521134295 + Shift.GetHashCode();
            hashCode = hashCode * -1521134295 + Tab.GetHashCode();
            return hashCode;
        }
    }

    public class InputManager : IUpdatedSystem
    {
        public string LocalPlayerId = Guid.NewGuid().ToString();

        public Dictionary<string, Dictionary<int, InputState>> PlayerInputHistory = new Dictionary<string, Dictionary<int, InputState>>();

        public readonly Queue<(string, int, InputState)> RecievedStates = new Queue<(string, int, InputState)>();

        private Game _game;

        private KeyboardState _previousLocalState;
        private KeyboardState _currentLocalState;

        public InputManager(Game game)
        {
            _game = game;
            ClearHistory();
        }

        public void Update()
        {
            if (Game.Rewinding) return;
            _previousLocalState = _currentLocalState;
            _currentLocalState = Keyboard.GetState();

            var currentState = BuildState();
            PlayerInputHistory[LocalPlayerId][Game.Frame] = currentState;

            foreach (var proxy in Game.GetSystem<NetworkManager>().ConnectedClients)
            {
                proxy.NewInput(LocalPlayerId, Game.Frame, currentState);
            }

            foreach (string playerId in PlayerInputHistory.Keys)
            {
                Dictionary<int, InputState> inputHistory = PlayerInputHistory[playerId];

                int deadInputFrame = Game.Frame - Game.BufferSize;
                if (inputHistory.ContainsKey(deadInputFrame))
                {
                    inputHistory.Remove(deadInputFrame);
                }
            }

            int frameToRewindTo = Game.Frame;

            while (RecievedStates.Any())
            {
                (string playerId, int inputFrame, InputState inputState) = RecievedStates.Dequeue();

                Dictionary<int, InputState> inputHistory;
                if (!PlayerInputHistory.TryGetValue(playerId, out inputHistory))
                {
                    inputHistory = new Dictionary<int, InputState>();
                    PlayerInputHistory[playerId] = inputHistory;
                }
                
                if (!inputHistory.ContainsKey(inputFrame) || !inputHistory[inputFrame].Equals(inputState))
                {
                    if (inputFrame < frameToRewindTo) frameToRewindTo = inputFrame;
                    inputHistory[inputFrame] = inputState;
                }
            }

            _game.ResimulateFrom(frameToRewindTo);
        }

        public Dictionary<string, InputState> GetInputStates()
        {
            var returnStates = new Dictionary<string, InputState>();

            foreach (string playerId in PlayerInputHistory.Keys)
            {
                if (PlayerInputHistory[playerId].TryGetValue(Game.Frame, out var state)) {
                    returnStates[playerId] = state;
                }
                else
                {
                    bool set = false;
                    for (int i = Game.Frame; i >= 0; i--)
                    {
                        if (PlayerInputHistory[playerId].TryGetValue(i, out var oldState))
                        {
                            returnStates[playerId] = oldState;
                            set = true;
                            break;
                        }
                    }

                    if (!set)
                    {
                        returnStates[playerId] = InputState.AllUp;
                    }
                }
            }

            return returnStates;
        }

        public InputState GetLocalInputState() => GetInputStates()[LocalPlayerId];

        public void ClearHistory()
        {
            PlayerInputHistory.Clear();
            PlayerInputHistory[LocalPlayerId] = new Dictionary<int, InputState>();
        }

        private KeyStatus GetKeyStatus(Keys key)
        {
            if (_previousLocalState != null && _previousLocalState.IsKeyDown(key))
            {
                if (_currentLocalState != null && _currentLocalState.IsKeyDown(key))
                {
                    return KeyStatus.Held;
                }
                else
                {
                    return KeyStatus.Released;
                }
            }
            else
            {
                if (_currentLocalState != null && _currentLocalState.IsKeyDown(key))
                {
                    return KeyStatus.Pressed;
                }
                else
                {
                    return KeyStatus.Free;
                }
            }
        }

        private InputState BuildState()
        {
            return new InputState(
                GetKeyStatus(Keys.W),
                GetKeyStatus(Keys.A),
                GetKeyStatus(Keys.S),
                GetKeyStatus(Keys.D),
                GetKeyStatus(Keys.Up),
                GetKeyStatus(Keys.Right),
                GetKeyStatus(Keys.Down),
                GetKeyStatus(Keys.Left),
                GetKeyStatus(Keys.Enter),
                GetKeyStatus(Keys.Space),
                GetKeyStatus(Keys.LeftShift),
                GetKeyStatus(Keys.Tab)
            );
        }
    }
}
