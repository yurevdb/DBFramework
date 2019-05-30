using System;
using System.Threading.Tasks;

namespace DBF
{
    public interface IDBActionProvider
    {
        #region Database interaction

        /// <summary>
        /// Used to get the values from the database as a <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="TModel">The model for the corresponding database table</typeparam>
        /// <param name="predicate">Used to set the where clause for fetching items from the database</param>
        /// <returns><see cref="List{T}"/> of items of <typeparamref name="TModel"/></returns>
        Task<DBSet<TModel>> Fetch<TModel>(Action<TModel> predicate) where TModel : class, new();

        /// <summary>
        /// Adds the given item to the database
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to the database table</typeparam>
        /// <param name="item">The item to push to the database</param>
        /// <returns></returns>
        Task Push<TModel>(TModel item);

        /// <summary>
        /// Removes item/items from the database based on an <see cref="Action"/> that defines the where clause for the item/items to remove
        /// </summary>
        /// <typeparam name="TModel">The model of the table for wich to remove an item</typeparam>
        /// <param name="predicate">An <see cref="Action"/> that defines the where clause to remove an item/items from the database</param>
        /// <returns></returns>
        Task Remove<TModel>(Action<TModel> predicate) where TModel : class, new();

        /// <summary>
        /// Update an item in the database based on the predicate with the new values coming from the update <see cref="Action"/>
        /// <para>
        ///     TODO: update the update statement to require a primary key and use that to identify the item to update
        ///           and then also use 1 predicate that has at least 2 values (1 primary key and 1 value to update)
        /// </para>
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to the database table to update</typeparam>
        /// <param name="predicate">The <see cref="Action"/> defining the where clause for the update</param>
        /// <param name="update">The new value for the model to update on the database</param>
        /// <returns></returns>
        Task Update<TModel>(Action<TModel> predicate) where TModel : class, new();

        #endregion
    }
}
