using System;

namespace DBF
{
    /// <summary>
    /// Helper object for creating the database context with all properties assigned
    /// </summary>
    public class DBContextBuilder
    {
        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        internal DBSchema Schema { get; private set; } = new DBSchema();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DBContextBuilder()
        {

        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Gets the model for a <see cref="DBTable{TModel}"/> to set the constraints
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to a database table of the database</typeparam>
        /// <returns></returns>
        public DBTable<TModel> Model<TModel>() where TModel : class, new()
        {
            foreach (var table in Schema.DBTables)
                if (table.GetType() == typeof(DBTable<TModel>))
                    return (DBTable<TModel>)table;

            // If the user tries to access a model that ins't availlable
            // throw an exception
            throw new Exception("The provided model isn't availlable in the ContextBuilder");
        }

        #endregion
    }
}
