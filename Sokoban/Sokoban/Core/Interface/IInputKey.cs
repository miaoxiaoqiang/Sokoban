using System.Windows.Input;

namespace Sokoban.Core
{
    internal interface IInputKey
    {
        public void GetInputKey(object obj, KeyEventArgs e);
    }
}
