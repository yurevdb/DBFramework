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
        /// The context builder instance
        /// </summary>
        internal readonly DBContextBuilder _DBContextBuilder = new DBContextBuilder();

        /// <summary>
        /// The user set DBActionProvider
        /// </summary>
        internal IDBActionProvider DBActionProvider => Options.DBActionProvider;

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
        /// <param name="ConnectionString">The connectionstring for the <see cref="DBContext"/></param>
        public DBContext(string ConnectionString)
        {
            // Set the DBContextOptions
            Options = new DBContextOptions(ConnectionString);

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
        public async Task Commit()
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
                var method = DBActionProvider.GetType().GetMethod(nameof(DBActionProvider.Fetch));

                // Generate the fetch method with the generic type we got earlier
                var genMethod = method.MakeGenericMethod(genericType);

                // Execute the generic fetch method for the specified type
                if (Options.AsyncSynchronization)
                    Task.Run(() =>
                    {
                        var p = typeof(Task<>).MakeGenericType(typeof(DBSet<>).MakeGenericType(prop.PropertyType.GetGenericArguments().First()));
                        var t = Convert.ChangeType(genMethod.Invoke(DBActionProvider, new object[] { null }), p);
                        prop.SetValue(this, t.GetType().GetProperty("Result")?.GetValue(t));
                    });
                else
                {
                    var p = typeof(Task<>).MakeGenericType(typeof(DBSet<>).MakeGenericType(prop.PropertyType.GetGenericArguments().First()));
                    var t = Convert.ChangeType(genMethod.Invoke(DBActionProvider, new object[] { null }), p);
                    prop.SetValue(this, t.GetType().GetProperty("Result")?.GetValue(t));
                }
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
            if(HasChanges)
                Discard().Wait();
        }

        #endregion
    }
}
