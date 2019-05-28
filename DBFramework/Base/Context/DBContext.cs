using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBF
{
    /// <summary>
    /// <para>
    ///     Base class for any database context holding any data related to the database.
    ///     Will be inherited for any database possible and the main functions should be
    ///     overridden to work with that specific database.
    /// </para>
    /// <para>
    ///     I.e. for a sql database you would create a <see cref="DBContext"/> as a SqlContext
    ///     where all the core functions needed for <see cref="DBF"/> to work will be implemented
    ///     for an sql database
    /// </para>
    /// </summary>
    public abstract class DBContext : IDisposable
    {
        #region Private Members

        /// <summary>
        /// The connection string for the <see cref="DBContext"/>
        /// </summary>
        internal string _ConnectionString = null;

        /// <summary>
        /// 
        /// </summary>
        internal readonly DBContextBuilder _DBContextBuilder = new DBContextBuilder();

        #endregion

        #region Public Properties

        /// <summary>
        /// The options for the <see cref="DBContext"/>
        /// </summary>
        public DBContextOptions Options { get; private set; } = new DBContextOptions();

        /// <summary>
        /// The schema for the <see cref="DBContext"/>.
        /// This holds any constraint data for the database
        /// </summary>
        protected DBSchema Schema => _DBContextBuilder.Schema;

        /// <summary>
        /// Determines wether the <see cref="DBSet{T}"/> of this <see cref="DBContext"/> has any changes made to it since the last commit to the database
        /// </summary>
        internal bool HasChanges { get; set; } = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DBContext()
        {
            // Create the context
            OnContextCreating(_DBContextBuilder);
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="Options"><see cref="Action"/> to set the <see cref="DBContextOptions"/></param>
        public DBContext(Action<DBContextOptions> Options)
        {
            // Assign the user given options
            Options(this.Options);

            // Create the context
            OnContextCreating(_DBContextBuilder);
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="ConnectionString">The connectionstring for the <see cref="DBContext"/></param>
        public DBContext(string ConnectionString)
        {
            // Set the connectionstring
            _ConnectionString = ConnectionString;

            // Create the context
            OnContextCreating(_DBContextBuilder);
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="ConnectionString">The connectionstring for the <see cref="DBContext"/></param>
        /// <param name="Options"><see cref="Action"/> to set the <see cref="DBContextOptions"/></param>
        public DBContext(string ConnectionString, Action<DBContextOptions> Options)
        {
            // Set the connectionstring
            _ConnectionString = ConnectionString;

            // Assign the user given options
            Options(this.Options);

            // Create the context
            OnContextCreating(_DBContextBuilder);
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// <para>
        ///     If any <see cref="DBSet{T}"/> belonging to this <see cref="DBContext"/> has any significant changes within the data. 
        ///     This function will commit those changes to the database.
        /// </para>
        /// </summary>
        /// <returns></returns>
        internal async Task Commit()
        {
            await Task.Run(() =>
            {

            });
        }

        /// <summary>
        /// <para>
        ///     If any <see cref="DBSet{T}"/> belonging to this <see cref="DBContext"/> has any significant changes within the data. 
        ///     This function will discard those changes to the database.
        /// </para>
        /// </summary>
        /// <returns></returns>
        internal async Task Discard()
        {
            await Task.Run(() =>
            {

            });
        }

        #endregion

        #region Private Helpers

        private void DBSetHasChanged(DBSetChangedEventArgs e)
        {

        }

        #endregion

        #region Database interaction

        /// <summary>
        /// Used to get the values from the database as a <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="T">The model for the corresponding database table</typeparam>
        /// <param name="predicate">Used to set the where clause for fetching items from the database</param>
        /// <returns><see cref="List{T}"/> of items of <typeparamref name="T"/></returns>
        public abstract Task<DBSet<T>> Fetch<T>(Action<T> predicate) where T : new();

        /// <summary>
        /// Adds the given item to the database
        /// </summary>
        /// <typeparam name="T">The model corresponding to the database table</typeparam>
        /// <param name="item">The item to push to the database</param>
        /// <returns></returns>
        public abstract Task Push<T>(T item);

        /// <summary>
        /// Removes item/items from the database based on an <see cref="Action"/> that defines the where clause for the item/items to remove
        /// </summary>
        /// <typeparam name="T">The model of the table for wich to remove an item</typeparam>
        /// <param name="predicate">An <see cref="Action"/> that defines the where clause to remove an item/items from the database</param>
        /// <returns></returns>
        public abstract Task Remove<T>(Action<T> predicate) where T : new();

        /// <summary>
        /// Update an item in the database based on the predicate with the new values coming from the update <see cref="Action"/>
        /// <para>
        ///     TODO: update the update statement to require a primary key and use that to identify the item to update
        ///           and then also use 1 predicate that has at least 2 values (1 primary key and 1 value to update)
        /// </para>
        /// </summary>
        /// <typeparam name="T">The model corresponding to the database table to update</typeparam>
        /// <param name="predicate">The <see cref="Action"/> defining the where clause for the update</param>
        /// <param name="update">The new value for the model to update on the database</param>
        /// <returns></returns>
        public abstract Task Update<T>(Action<T> predicate) where T : new();

        #endregion

        #region Virtual Functions

        /// <summary>
        /// Get called after the main initialization of the constructor
        /// </summary>
        /// <param name="contextBuilder">The <see cref="DBContextBuilder"/> instance to use to set constraints etc.</param>
        public virtual void OnContextCreating(DBContextBuilder contextBuilder)
        {
            // Fetch all the data from the database
            foreach(var prop in GetType().GetProperties())
            {
                // Check to see if the property type has any generic arguments
                // The dbsets always need a generic argument and that's what we're trying to get
                if (prop.PropertyType.GetGenericArguments().Length < 1) continue;

                // Get the generic type (always should be just 1)
                Type genericType = prop.PropertyType.GetGenericArguments().First();

                // Check to see if the property was of type DBSet<genericType>
                if (!(prop.PropertyType == typeof(DBSet<>).MakeGenericType(genericType))) continue;

                // Get the fetch method
                var method = GetType().GetMethod("Fetch");

                // Generate the fetch method with the generic type we got earlier
                var genMethod = method.MakeGenericMethod(genericType);

                // Execute the generic fetch method for the specified type
                if (Options.AsyncSynchronization)
                    Task.Run(() => genMethod.Invoke(this, new object[] { null }));
                else
                    (genMethod.Invoke(this, new object[] { null }) as Task)?.Wait();
            }

            // Set the DBTables in the contextbuilder based on the DBSets set in the user created context
            foreach (var prop in GetType().GetProperties())
                if (prop.PropertyType.GenericTypeArguments.Length == 1)
                {
                    // Assuming the user did this right
                    var type = typeof(DBTable<>).MakeGenericType(prop.PropertyType.GenericTypeArguments.First());
                    var table = Activator.CreateInstance(type);
                    contextBuilder.Schema.DBTables.Add(table);
                }
        }

        #endregion

        #region Implementations

        /// <summary>
        /// Disposes the current <see cref="DBContext"/>
        /// </summary>
        public void Dispose()
        {
            
        }

        #endregion
    }
}
