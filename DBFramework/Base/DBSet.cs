using System;
using System.Collections;
using System.Collections.Generic;

namespace DBF
{
    /// <summary>
    /// A collection of items corresponding to a database table
    /// </summary>
    /// <typeparam name="TModel">The model of the database table</typeparam>
    public class DBSet<TModel> : IList<TModel>
    {
        #region Public Properties

        /// <summary>
        /// <para>
        ///     The local <see cref="List{T}"/> of elements contained within the <see cref="DBSet{TModel}"/>.
        ///     This list of elements can be committed to the database with the <see cref="DBContext.Commit"/> function.
        /// </para>
        /// </summary>
        public IList<TModel> Local { get; private set; } = new List<TModel>();

        #endregion

        #region IList Implementation

        private readonly IList<TModel> _list = new List<TModel>();

        #region Change Tracking

        public void Add(TModel item)
        {
            _list.Add(item);
        }

        public void Insert(int index, TModel item)
        {
            _list.Insert(index, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Remove(TModel item)
        {
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        #endregion

        #region Not Change Tracking

        public TModel this[int index] { get => _list[index]; set => _list[index] = value; }

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public bool Contains(TModel item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(TModel[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TModel> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(TModel item)
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
