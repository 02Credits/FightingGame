using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class RemoteInputsManager
    {
        public readonly ConcurrentQueue<InputState> RecievedStates = new ConcurrentQueue<InputState>();
    }
}
