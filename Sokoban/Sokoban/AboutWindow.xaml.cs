using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Sokoban
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            Utils.IconHelper.RemoveIcon(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)).Dispose();
        }
    }
}
