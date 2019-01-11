using Caliburn.Micro;
using FightingGame.GameLogic;
using FightingGame.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public class StartEvent : IPageChangeEvent { }

    public class GameViewModel : Screen
    {
        Game _game;

        public GameViewModel(Game game)
        {
            _game = game;
            ViewAttached += GameViewModel_ViewAttached;
        }

        private void GameViewModel_ViewAttached(object sender, ViewAttachedEventArgs e)
        {
            var view = GetView() as GameView;
            view.SizeChanged += View_SizeChanged;
            _game.Width = view.Width;
            _game.Height = view.Height;
            view.GameContainer.Child = _game;
        }

        private void View_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var view = GetView() as GameView;
            _game.Width = view.Width;
            _game.Height = view.Height;
        }
    }
}
