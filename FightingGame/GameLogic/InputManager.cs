using FightingGame.GameLogic.Systems;
using FightingGame.Networking;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FightingGame.GameLogic
{
    public class InputManager
    {
        public readonly string LocalPlayerId = Guid.NewGuid().ToString();
        private readonly Dictionary<string, Dictionary<long, InputState>> _recordedInputs = new Dictionary<string, Dictionary<long, InputState>>();

        public RemoteInputsManager RemoteInputsManager { get; set; }
        public NetworkManager NetworkManager { get; set; }
        public WpfKeyboard Keyboard { get; set; }

        public long EarliestFrameUpdate(long currentFrame)
        {
            StoreLocalState(currentFrame);

            long frameToRewindTo = currentFrame;
            while (RemoteInputsManager.RecievedStates.Any())
            {
                if (RemoteInputsManager.RecievedStates.TryDequeue(out InputState inputState))
                {
                    if (!StoreInputState(inputState))
                    {
                        throw new Exception($"Recieved multiple inputs for {inputState.Frame}.");
                    }
                    frameToRewindTo = Math.Min(frameToRewindTo, inputState.Frame);
                }
            }

            foreach (var playerID in _recordedInputs.Keys)
            {
                _recordedInputs[playerID].Remove(currentFrame - SystemManager.BufferSize);
            }

            return frameToRewindTo;
        }

        public void ClearHistory()
        {
            _recordedInputs.Clear();
        }

        public IEnumerable<InputState> GetInputStates(long frame)
        {
            foreach (string playerID in _recordedInputs.Keys)
            {
                yield return GetInputState(playerID, frame);
            }
        }

        public InputState GetInputState(string playerId, long frame)
        {
            if (_recordedInputs.TryGetValue(playerId, out var playerStates))
            {
                for (long i = frame; i > frame - SystemManager.BufferSize; i--)
                {
                    if (playerStates.TryGetValue(i, out var state))
                    {
                        return state;
                    }
                }
            }
            return new InputState(playerId, frame);
        }

        public InputState GetLocalInputState(long frame) => GetInputState(LocalPlayerId, frame);

        private void StoreLocalState(long frame)
        {
            var localState = BuildLocalState(frame);
            if (StoreInputState(localState))
            {
                BroadcastInputState(localState);
            }
        }

        private void BroadcastInputState(InputState inputState)
        {
            foreach (var proxy in NetworkManager.ConnectedClients)
            {
                proxy.NewInput(inputState);
            }
        }

        private bool StoreInputState(InputState inputState)
        {
            Dictionary<long, InputState> playerInputs;
            if (!_recordedInputs.TryGetValue(inputState.PlayerID, out playerInputs))
            {
                playerInputs = new Dictionary<long, InputState>();
                _recordedInputs[inputState.PlayerID] = playerInputs;
            }

            if (playerInputs.ContainsKey(inputState.Frame))
            {
                return false;
            }
            playerInputs[inputState.Frame] = inputState;
            return true;
        }

        private KeyStatus GetKeyStatus(Keys key, KeyboardState currentState, KeyboardState previousState)
        {
            if (previousState != null && previousState.IsKeyDown(key))
            {
                if (currentState != null && currentState.IsKeyDown(key))
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
                if (currentState != null && currentState.IsKeyDown(key))
                {
                    return KeyStatus.Pressed;
                }
                else
                {
                    return KeyStatus.Free;
                }
            }
        }

        private KeyboardState _previousLocalState;
        private InputState BuildLocalState(long frame)
        {
            var currentLocalState = Keyboard.GetState();
            var state = new InputState(
                LocalPlayerId,
                frame,
                GetKeyStatus(Keys.W, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.A, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.S, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.D, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Up, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Right, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Down, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Left, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Enter, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Space, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.LeftShift, currentLocalState, _previousLocalState),
                GetKeyStatus(Keys.Tab, currentLocalState, _previousLocalState)
            );
            _previousLocalState = currentLocalState;
            return state;
        }
    }

    public static class KeyStatusExtensions
    {
        public static bool IsFree(this KeyStatus status) => status.HasFlag(KeyStatus.Free);
        public static bool IsReleased(this KeyStatus status) => status.HasFlag(KeyStatus.Released);
        public static bool IsPressed(this KeyStatus status) => status.HasFlag(KeyStatus.Pressed);
        public static bool IsHeld(this KeyStatus status) => status.HasFlag(KeyStatus.Held);
        public static bool IsDown(this KeyStatus status) => KeyStatus.Down.HasFlag(status);
        public static bool IsUp(this KeyStatus status) => KeyStatus.Up.HasFlag(status);
    }
}
