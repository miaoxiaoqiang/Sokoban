using Sokoban.Componets;
using System.Windows.Controls;

namespace Sokoban.Core
{
    internal interface IWindow
    {
        public void ShowWindow(string window);

        public void ShowMessage(MessageType type, string text, int duratime);

        public void HideMessage();

        public void Navigate(string pagename);

        public Page GetPage(string pagename);
    }
}
