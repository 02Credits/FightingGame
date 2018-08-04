using System;
using System.Collections.Generic;
using System.Text;

namespace FightingGame.Networking
{
    public class IDManager
    {
        public long CurrentID;

        public IDManager() { }

        public IDManager(long id) { CurrentID = id; }

        public long GetNextID()
        {
            var id = CurrentID;
            CurrentID++;
            return id;
        }
    }
}
