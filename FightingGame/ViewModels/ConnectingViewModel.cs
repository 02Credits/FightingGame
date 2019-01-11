using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public class ConnectingViewModel : Screen
    {
        public bool IsHosting { get; }
        public string ConnectingMessage => IsHosting ? "Waiting for Connection..." : "Connecting...";

        public ConnectingViewModel(bool isHosting)
        {
            IsHosting = isHosting;
        }
    }
}
