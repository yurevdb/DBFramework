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
        where TModel : class
    {
        #region Private Members

        /// <summary>
        /// Private instance of the list
        /// </summary>
        private readonly IList<TModel> _list = new List<TModel>();

        #endregion

        #region Public Modifiers

        /// <summary>
        /// Adds the given item to the <see cref="DBSet{TModel}"/>
        /// </summary>
        /// <param name="item">The item of type <typeparamref name="TModel"/></param>
        public void Add(TModel item)
        {
            _list.Add(item);
        }

        /// <summary>
        /// Inserts the given item to the <see cref="DBSet{TModel}"/> at the provided index
        /// </summary>
        /// <param name="index">The item of type <typeparamref name="TModel"/></param>
        /// <param name="item">The index to insert the item at</param>
        public void Insert(int index, TModel item)
        {
            _list.Insert(index, item);
        }

        /// <summary>
        /// Clears the <see cref="DBSet{TModel}"/>
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Removes the given item from the <see cref="DBSet{TModel}"/>
        /// </summary>
        /// <param name="item">The item of type <typeparamref name="TModel"/></param>
        /// <returns></returns>
        public bool Remove(TModel item)
        {
            return _list.Remove(item);
        }

        /// <summary>
        /// Removes the item from the <see cref="DBSet{TModel}"/> at the specified index
        /// </summary>
        /// <param name="index">The index to remove the item from</param>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        #endregion

        #region Public Accessors

        /// <summary>
        /// Get the item from the specified index
        /// </summary>
        /// <param name="index">The index to get the item from</param>
        /// <returns></returns>
        public TModel this[int index] { get => _list[index]; set => _list[index] = value; }

        /// <summary>
        /// The amount of items that exist in the <see cref="DBSet{TModel}"/>
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Indicates wether the <see cref="DBSet{TModel}"/> is readonly
        /// </summary>
        public bool IsReadOnly => _list.IsReadOnly;

        /// <summary>
        /// Indicates wether the <see cref="DBSet{TModel}"/> contains the given item
        /// </summary>
        /// <param name="item">The item of type <typeparamref name="TModel"/></param>
        /// <returns></returns>
        public bool Contains(TModel item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Indicates wether the <see cref="DBSet{TModel}"/> contains the given item based on the primary key obtained by providing a <see cref="DBSchema"/>
        /// </summary>
        /// <param name="item">The item of type <typeparamref name="TModel"/></param>
        /// <param name="schema">The <see cref="DBSchema"/> holding the primary key</param>
        /// <returns></returns>
        public bool Contains(TModel item, DBSchema schema)
        {
            foreach (var i in _list)
                if (schema.GetPrimaryKeyValue(i).Equals(schema.GetPrimaryKeyValue(item)))
                    return true;

            return false;
        }

        /// <summary>
        /// Copies an array of <typeparamref name="TModel"/>, starting and the provided index
        /// </summary>
        /// <param name="array">The array to inject</param>
        /// <param name="arrayIndex">The index to start from</param>
        public void CopyTo(TModel[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the <see cref="IEnumerator{TModel}"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TModel> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Gets the index of a given item in the <see cref="DBSet{TModel}"/>
        /// </summary>
        /// <param name="item">The item of type <typeparamref name="TModel"/></param>
        /// <returns></returns>
        public int IndexOf(TModel item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Gets the <see cref="IEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DBSet() { }

        /// <summary>
        /// Parameterized constructor for internal use only (must be public for reflection to be able to create a new instance of this)
        /// Used to create a dereferenced set based on a DBSet to create a snapshot of the dbcontext
        /// </summary>
        /// <param name="set">The dbset to dereference</param>
        public DBSet(DBSet<TModel> set)
        {
            foreach (var item in set)
            {
                var newItem = (TModel)Activator.CreateInstance(typeof(TModel));
                foreach(var prop in newItem.GetType().GetProperties())
                    prop.SetValue(newItem, prop.GetValue(item));
                _list.Add(newItem);
            }
        }

        #endregion
    }
}
