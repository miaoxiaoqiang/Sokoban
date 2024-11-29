using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sokoban
{
    /// <summary>
    /// StageClearWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class StageClearWindow : Window
    {
        public event Action<bool> ContinueGameEvent;

        public StageClearWindow()
        {
            InitializeComponent();

            BtnCancelGame.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(CancelGame), true);
            BtnNextStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(NextStage), true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void NextStage(object sender, MouseButtonEventArgs e)
        {
            ContinueGameEvent?.Invoke(true);
        }

        private void CancelGame(object sender, MouseButtonEventArgs e)
        {
            ContinueGameEvent?.Invoke(false);
        }

        private void ChangeNumber(string number)
        {
            StageNumber.Text = number;
        }
    }
}
