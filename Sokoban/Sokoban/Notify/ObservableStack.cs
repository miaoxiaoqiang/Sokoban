using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Sokoban.Notify
{
    public sealed class ObservableStack<T> : IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Stack<T> _stack;

        public ObservableStack()
        {
            _stack = new Stack<T>();
        }

        public ObservableStack(IEnumerable<T> collection)
        {
            _stack = new Stack<T>(collection);
        }

        public void Push(T item)
        {
            _stack.Push(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(nameof(Count));
        }

        public T Pop()
        {
            var item = _stack.Pop();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            OnPropertyChanged(nameof(Count));
            return item;
        }

        public T Peek()
        {
            return _stack.Peek();
        }

        public bool Contains(T item)
        {
            return _stack.Contains(item);
        }

        public void Clear()
        {
            _stack.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        public int Count => _stack.Count;
    }
}
