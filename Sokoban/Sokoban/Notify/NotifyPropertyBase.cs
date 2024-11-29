using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Sokoban.Notify
{
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseChanged<T>(ref T propervalue, T newvalue, [CallerMemberName]string propertyName = "")
        {
            if (object.ReferenceEquals(propervalue, newvalue))
            {
                return;
            }

            propervalue = newvalue;
            OnPropertyChanged(propertyName);
        }
    }

    internal static class NotifyPropertyBaseEx
    {
        public static string GetPropertyName<T, U>(Expression<Func<T, U>> exp)
        {
            string _pName = "";
            if (exp.Body is MemberExpression)
            {
                _pName = (exp.Body as MemberExpression).Member.Name;
            }
            else if (exp.Body is UnaryExpression)
            {
                _pName = ((exp.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            }
            return _pName;
        }
    }

    internal class PropertyNotifyObject : NotifyPropertyBase, IDisposable
    {
        public PropertyNotifyObject() { }

        private readonly Dictionary<object, object> _ValueDictionary = new();

        public T GetPropertyValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("invalid " + propertyName);
            }

            object _propertyValue;
            if (!_ValueDictionary.TryGetValue(propertyName, out _propertyValue))
            {
                _propertyValue = default(T);
                _ValueDictionary.Add(propertyName, _propertyValue);
            }
            return (T)_propertyValue;
        }

        public void SetPropertyValue<T>(string propertyName, T value)
        {
            if (!_ValueDictionary.ContainsKey(propertyName) || _ValueDictionary[propertyName] != (object)value)
            {
                _ValueDictionary[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }

        public void Dispose()
        {
            DoDispose();
        }

        ~PropertyNotifyObject()
        {
            DoDispose();
        }

        void DoDispose()
        {
            _ValueDictionary?.Clear();
        }
    }

    internal static class PropertyNotifyObjectEx
    {
        public static U GetValue<T, U>(this T t, Expression<Func<T, U>> exp) where T : PropertyNotifyObject
        {
            string _pN = NotifyPropertyBaseEx.GetPropertyName(exp);
            return t.GetPropertyValue<U>(_pN);
        }

        public static void SetValue<T, U>(this T t, Expression<Func<T, U>> exp, U value) where T : PropertyNotifyObject
        {
            string _pN = NotifyPropertyBaseEx.GetPropertyName(exp);
            t.SetPropertyValue<U>(_pN, value);
        }
    }
}
