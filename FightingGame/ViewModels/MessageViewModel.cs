using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FightingGame.ViewModels
{
    public class MessageViewModel
    {
        public string Message { get; }
        public string Username { get; }
        public SolidColorBrush MessageColor => Username == Environment.UserName ? Brushes.Green : Brushes.Blue; 

        [JsonConstructor]
        public MessageViewModel(string message, string username)
        {
            Message = message;
            Username = username;
        }

        public MessageViewModel(string message)
            : this(message, Environment.UserName) { }
    }
}
