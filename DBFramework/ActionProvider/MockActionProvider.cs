using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBF
{
    /// <summary>
    /// 
    /// </summary>
    public class MockActionProvider : DBActionProvider
    {
        #region Private Members

        /// <summary>
        /// Private mock database
        /// </summary>
        private readonly List<object> _list = new List<object>();

        /// <summary>
        /// 
        /// </summary>
        private static readonly MockActionProvider _Instance = new MockActionProvider();

        #endregion

        #region Public Properties

        /// <summary>
        /// Singleton instance
        /// </summary>
        public MockActionProvider Instance => _Instance;

        #endregion

        #region Constructor

        public MockActionProvider() : base() { }

        #endregion

        #region DBActionProvider Implementations

        public override Task<DBSet<TModel>> Fetch<TModel>(Action<TModel> predicate)
        {
            return Task.Run(() =>
            {
                var list = new DBSet<TModel>();
                foreach (TModel item in _list)
                    list.Add(item);
                return list;
            });
        }

        public override Task Push<TModel>(TModel item)
        {
            return Task.Run(() => _list.Add(item));
        }

        public override Task Remove<TModel>(Action<TModel> predicate)
        {
            return Task.Run(() =>
            {
                TModel item = new TModel();
                predicate(item);

                foreach (TModel it in _list)
                    if (Schema.GetPrimaryKeyValue(it) == Schema.GetPrimaryKeyValue(item))
                        _list.Remove(it);
            });
        }

        public override Task Update<TModel>(Action<TModel> predicate)
        {
            return Task.Run(() =>
            {
                TModel item = new TModel();
                predicate(item);

                var oldItem = _list.Where(i => Schema.GetPrimaryKeyValue(item) == Schema.GetPrimaryKeyValue(i));

                foreach (var prop in oldItem.GetType().GetProperties())
                    prop.SetValue(oldItem, prop.GetValue(item));
            });
        }

        #endregion
    }
}
