using System;
using System.Threading.Tasks;

namespace DBF
{
    /// <summary>
    /// <para>
    ///     Defines the action that DBF can do on a database.
    /// </para>
    /// <para>
    ///     This needs to be implemented for each specific database that exists.
    ///     To allow for the <see cref="DBContext"/> to call these functions and
    ///     execute queries on the database of your choice.
    /// </para>
    /// </summary>
    public abstract class DBActionProvider
    {
        #region Protected Members

        /// <summary>
        /// The connectionstring to access the database 
        /// </summary>
        protected readonly string _ConnectionString = null;

        #endregion

        #region Internal Properties

        /// <summary>
        /// The <see cref="DBSchema"/> holding all the info about every property for the <see cref="DBContext"/>.
        /// I.e. Constraints, Primary key, etc.
        /// </summary>
        internal protected DBSchema Schema { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DBActionProvider() { }

        /// <summary>
        /// Paramerized constructor
        /// <paramref name="ConnectionString">The Connectionstring to access the database</paramref>
        /// </summary>
        public DBActionProvider(string ConnectionString)
        {
            _ConnectionString = ConnectionString;
        }

        #endregion

        #region Internal Functions

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="action"></param>
        /// <param name="item"></param>
        /// <param name="schema"></param>
        internal void RunAction<TModel>(DBAction action, TModel item, DBSchema schema) where TModel : class, new()
        {
            // Get primary key property info
            var pk = schema.GetPrimaryKey(item.GetType());

            switch (action)
            {
                case DBAction.Push:
                    Push(item);
                    break;
                case DBAction.Remove:
                    var rmethod = GetType().GetMethod(nameof(Remove));
                    var rgenMethod = rmethod.MakeGenericMethod(item.GetType());
                    Action<TModel> rwhere = i => pk.SetValue(i, pk.GetValue(item));
                    rgenMethod.Invoke(this, new object[] { rwhere });
                    break;
                case DBAction.Update:
                    var umethod = GetType().GetMethod(nameof(Update));
                    var ugenMethod = umethod.MakeGenericMethod(item.GetType());
                    Action<TModel> uwhere = i =>
                    {
                        pk.SetValue(i, pk.GetValue(item));
                        foreach (var prop in i.GetType().GetProperties())
                            if (prop.GetValue(item) != null && prop != pk)
                                prop.SetValue(i, prop.GetValue(item));
                    };
                    ugenMethod.Invoke(this, new object[] { uwhere });
                    break;
                default: break;
            }
        }

        #endregion

        #region Abstract Database interaction

        /// <summary>
        /// Used to get the values from the database as a <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="TModel">The model for the corresponding database table</typeparam>
        /// <param name="predicate">Used to set the where clause for fetching items from the database</param>
        /// <returns><see cref="List{T}"/> of items of <typeparamref name="TModel"/></returns>
        public abstract Task<DBSet<TModel>> Fetch<TModel>(Action<TModel> predicate) where TModel : class, new();

        /// <summary>
        /// Adds the given item to the database
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to the database table</typeparam>
        /// <param name="item">The item to push to the database</param>
        /// <returns></returns>
        public abstract Task Push<TModel>(TModel item);

        /// <summary>
        /// Removes item/items from the database based on an <see cref="Action"/> that defines the where clause for the item/items to remove
        /// </summary>
        /// <typeparam name="TModel">The model of the table for wich to remove an item</typeparam>
        /// <param name="predicate">An <see cref="Action"/> that defines the where clause to remove an item/items from the database</param>
        /// <returns></returns>
        public abstract Task Remove<TModel>(Action<TModel> predicate) where TModel : class, new();

        /// <summary>
        /// Update an item in the database based on the predicate with the new values coming from the update <see cref="Action"/>
        /// <para>
        ///     TODO: update the update statement to require a primary key and use that to identify the item to update
        ///           and then also use 1 predicate that has at least 2 values (1 primary key and 1 value to update)
        /// </para>
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to the database table to update</typeparam>
        /// <param name="predicate">The <see cref="Action"/> defining the where clause for the update</param>
        /// <param name="schema">The <see cref="DBSchema"/> holding the info about properties </param>
        /// <returns></returns>
        public abstract Task Update<TModel>(Action<TModel> predicate) where TModel : class, new();

        #endregion

        #region Abstract Database Modifications



        #endregion
    }
}
