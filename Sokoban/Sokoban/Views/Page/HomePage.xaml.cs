using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sokoban.Core;

namespace Sokoban
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();

            BtnPlayGame.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(PlayGame), true);
            BtnExitApp.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ExitApp), true);
            BtnDesc.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ShowDesc), true);
            BtnEditMap.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(EditMap), true);
        }

        private void EditMap(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is IWindow window)
            {
                window.ShowWindow("Map");
            }
        }

        private void PlayGame(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is IWindow window)
            {
                window.Navigate("Stage");
            }
        }

        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Close();
        }

        private void ShowDesc(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is IWindow window)
            {
                window.ShowWindow("Desc");
            }
        }
    }
}
