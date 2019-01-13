using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
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
