using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sokoban.Componets
{
    /// <summary>
    /// StageControl.xaml 的交互逻辑
    /// </summary>
    public sealed partial class StageControl : UserControl
    {
        public StageControl()
        {
            InitializeComponent();
        }

        //public event RoutedEventHandler ClickHandler
        //{
        //    add => AddHandler(ClickHandlerEvent, value);
        //    remove => RemoveHandler(ClickHandlerEvent, value);
        //}

        //public static readonly RoutedEvent ClickHandlerEvent = EventManager.RegisterRoutedEvent(
        //    nameof(ClickHandlerEvent), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StageControl));
    }
}
