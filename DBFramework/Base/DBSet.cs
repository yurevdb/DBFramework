using System;
using System.Collections;
using System.Collections.Generic;

namespace DBF
{
    /// <summary>
    /// A collection of items corresponding to a database table
    /// </summary>
    /// <typeparam name="T">The model of the database table</typeparam>
    public class DBSet<T> : IList<T>
    {
        #region Change Tracking

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DBSetChangedEventArgs> DBSetChanged;

        protected virtual void OnDBSetChanged(DBSetChangedEventArgs e)
        {
            DBSetChanged?.Invoke(this, e);
        }

        #endregion

        #region IList Implementation

        private readonly IList<T> _list = new List<T>();

        #region Change Tracking

        public void Add(T item)
        {
            _list.Add(item);
            OnDBSetChanged(new DBSetChangedEventArgs() { Message = "Added an item" });
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnDBSetChanged(new DBSetChangedEventArgs() { Message = "Inserted an item" });
        }

        public void Clear()
        {
            _list.Clear();
            OnDBSetChanged(new DBSetChangedEventArgs() { Message = "Cleared all the items" });
        }

        public bool Remove(T item)
        {
            OnDBSetChanged(new DBSetChangedEventArgs() { Message = "Removed an item" });
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            OnDBSetChanged(new DBSetChangedEventArgs() { Message = "Removed an item" });
        }

        #endregion

        #region Not Change Tracking

        public T this[int index] { get => _list[index]; set => _list[index] = value; }

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #endregion
    }
}
