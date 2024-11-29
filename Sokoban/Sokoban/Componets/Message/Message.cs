using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sokoban.Componets
{
    public enum MessageType
    {
        [Description("默认")]
        Default,
        [Description("成功")]
        Success,
        [Description("警告")]
        Warning,
        [Description("错误")]
        Error,
        [Description("问号")]
        Question,
        [Description("说明")]
        Info,
        [Description("等待")]
        Loading
    }

    internal sealed class Message : ContentControl
    {
        public int Time { get; set; }

        [Bindable(true)]
        public MessageType MessageType
        {
            get
            {
                return (MessageType)GetValue(MessageTypeProperty);
            }
            set
            {
                SetValue(MessageTypeProperty, value);
            }
        }

        public static readonly DependencyProperty MessageTypeProperty = DependencyProperty.Register("MessageType", typeof(MessageType), typeof(Message), new PropertyMetadata(default(MessageType)));

        internal Message()
        {
            Loaded += Message_Loaded;
        }

        private void Message_Loaded(object sender, RoutedEventArgs e)
        {
            //if (Time > 0)
            //{
            //    if (Parent is MessageHost host)
            //    {
            //        await Task.Delay(Time);

            //        //这块代码先写死，以后再修改
            //        UIElement element = Utils.FindVisualParent<Grid>(this);
            //        if (element != null)
            //        {
            //            UIElement maskElement = Utils.GetChildObject<Border>(element, "mask");
            //            if (maskElement != null)
            //            {
            //                maskElement.Visibility = Visibility.Collapsed;
            //            }
            //        }

            //        host.Items.Remove(this);
            //    }
            //}
        }
    }

    public sealed class MessageHost : ItemsControl
    {
        public string Token
        {
            get
            {
                return (string)GetValue(TokenProperty);
            }
            set
            {
                SetValue(TokenProperty, value);
            }
        }

        public static readonly DependencyProperty TokenProperty = DependencyProperty.Register("Token", typeof(string), typeof(MessageHost), new PropertyMetadata(OnTokenChanged));

        private static void OnTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageHost host)
            {
                var token = e.NewValue.ToString();
                if (MessageManage.MessageHosts.ContainsKey(token))
                {
                    MessageManage.MessageHosts.Remove(token);
                }
                MessageManage.MessageHosts.Add(token, host);
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Message;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Message();
        }
    }

    public sealed class MessageManage
    {
        static MessageManage()
        {
            MessageHosts = new Dictionary<string, MessageHost>();
        }

        internal static Dictionary<string, MessageHost> MessageHosts
        {
            get;
            set;
        }

        public static void Show(string message, MessageType messageType, string token, int time)
        {
            if (!MessageHosts.ContainsKey(token))
            {
                return;
            }

            var messageHost = MessageHosts[token];
            messageHost.Dispatcher.Invoke((Action)(() =>
            {
                messageHost.Items.Add(new Message()
                {
                    MessageType = messageType,
                    Content = message,
                    Time = time,
                    Uid = Guid.NewGuid().ToString()
                });
            }));
        }

        public static void Hide(string token)
        {
            if (!MessageHosts.ContainsKey(token))
            {
                return;
            }

            var messageHost = MessageHosts[token];
            messageHost.Items.Clear();
        }
    }
}
