using System;
using System.Collections.Generic;

namespace DBF
{
    public class DBSchema
    {
        #region Internal Properties

        /// <summary>
        /// The tables for the <see cref="DBContext"/>
        /// </summary>
        public HashSet<object> DBTables { get; private set; } = new HashSet<object>();

        #endregion

        #region Public Functions

        /// <summary>
        /// Gets the model for a <see cref="DBTable{TModel}"/> to set the constraints
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to a database table of the database</typeparam>
        /// <returns></returns>
        public DBTable<TModel> Model<TModel>() where TModel : class, new()
        {
            foreach (var table in DBTables)
                if (table.GetType() == typeof(DBTable<TModel>))
                    return (DBTable<TModel>)table;

            // If the user tries to access a model that ins't availlable
            // throw an exception
            throw new Exception("The provided model isn't availlable in the ContextBuilder");
        }

        #endregion

    }
}
