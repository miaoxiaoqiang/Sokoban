using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Sokoban.Core;
using Sokoban.Componets;

namespace Sokoban
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class MainWindow : Window, IWindow
    {
        private readonly IDictionary<string, Page> _pages;
        private readonly AboutWindow aboutWindow;
        private readonly MapEditWindow mapEditWindow;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += Window_Loaded;
            AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(WindowKeyDown), true);

            _pages = new Dictionary<string, Page>
            {
                { "Home", new HomePage() },
                { "Stage", new StagePage() },
                { "Game", new GamePage() }
            };

            aboutWindow = new AboutWindow();
            mapEditWindow = new MapEditWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //Window[] childArray = Application.Current.Windows.Cast<Window>().ToArray();
            base.OnClosing(e);
            Application.Current.Shutdown();
            
            //Environment.Exit(0);
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    var collections = Application.Current.Windows;

        //    foreach (Window window in collections)
        //    {
        //        if (window != this)
        //        {
        //            window.Close();
        //        }
        //    }

        //    base.OnClosed(e);
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "map\\finished\\");
            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "logs\\");

            ContentFrame.Content = GetPage("Home");
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Left = (SystemParameters.PrimaryScreenWidth / 2) - (ActualWidth / 2);
            Top = (SystemParameters.PrimaryScreenHeight / 2) - (ActualHeight / 2);
        }

        public void WindowKeyDown(object obj, KeyEventArgs e)
        {
            if (ContentFrame.Content is IInputKey inputkey)
            {
                inputkey.GetInputKey(obj, e);
            }
        }

        public void Navigate(string pagename)
        {
            ContentFrame.Content = GetPage(pagename);
        }

        public Page GetPage(string pagename)
        {
            if (!_pages.ContainsKey(pagename))
            {
                return null;
            }
            return _pages[pagename];
        }

        public void ShowMessage(MessageType type, string text, int duratime)
        {
            HideMessage();

            mask.Visibility = Visibility.Visible;
            MessageManage.Show(text, type, "MainMessageToken", duratime);

            if (duratime > 0)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(duratime);
                    this.Dispatcher.Invoke(() =>
                    {
                        HideMessage();
                    });
                });
            }
        }

        public void HideMessage()
        {
            mask.Visibility = Visibility.Collapsed;
            MessageManage.Hide("MainMessageToken");
        }

        public void ShowWindow(string window)
        {
            if (string.Compare(window, "Desc", StringComparison.Ordinal) == 0)
            {
                aboutWindow.ShowDialog();
            }
            else if (string.Compare(window, "Map", StringComparison.Ordinal) == 0)
            {
                Visibility = Visibility.Hidden;
                mapEditWindow.Owner = this;
                mapEditWindow.ShowDialog();
            }
        }
    }
}
