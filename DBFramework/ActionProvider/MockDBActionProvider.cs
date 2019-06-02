using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBF
{
    /// <summary>
    /// 
    /// </summary>
    public class MockDBActionProvider : DBActionProvider
    {
        #region Private Members

        /// <summary>
        /// Private mock database
        /// </summary>
        private readonly List<object> list = new List<object>();

        #endregion

        #region DBActionProvider Implementations

        public override Task<DBSet<TModel>> Fetch<TModel>(Action<TModel> predicate)
        {
            throw new NotImplementedException();
        }

        public override Task Push<TModel>(TModel item)
        {
            throw new NotImplementedException();
        }

        public override Task Remove<TModel>(Action<TModel> predicate)
        {
            throw new NotImplementedException();
        }

        public override Task Update<TModel>(Action<TModel> predicate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
