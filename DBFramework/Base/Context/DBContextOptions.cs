using System;

namespace DBF
{
    /// <summary>
    /// Sets the options for the <see cref="DBContext"/>
    /// </summary>
    public class DBContextOptions
    {
        #region Private Members

        /// <summary>
        /// The Connection string
        /// </summary>
        private readonly string _ConnectionString;

        #endregion

        #region Public Settings Properties

        /// <summary>
        /// <para>
        ///     When set to true will allow potentially unsafe ways of handling data with a database according to the framework.
        ///     I.e. mutating a primary key of a database entry would be considered unsafe because the primary key is seen as immutable by the framework.
        ///     But to allow the user to use the framework as wanted, this setting can allow the use of unsafe data handling.
        /// </para>
        /// </summary>
        public bool AllowUnsafe { get; set; } = false;

        /// <summary>
        /// <para>
        ///     Allows the synchronization of the datacontext with the database run asynchronously.
        ///     If the <see cref="SynchronizeDB"/> is set to true will run the synchronization asynchrounously, 
        ///     even while setting up the datacontext. This means that the <see cref="DBSet{T}"/> will not be availlable on the next line most likely
        /// </para>
        /// <para>
        ///     Setting this to <see cref="false"/> will make sure that the synchronization will run synchronously so that the <see cref="DBSet{T}"/>
        ///     will be availlable on the next line if needed.
        /// </para>
        /// </summary>
        public bool AsyncSynchronization { get; set; } = false;

        #endregion

        #region Public Objects Properties

        /// <summary>
        /// The <see cref="DBF.DBActionProvider"/> that implements the database actions
        /// </summary>
        internal DBActionProvider DBActionProvider { get; private set; }

        /// <summary>
        /// The schema for the <see cref="DBContext"/>
        /// </summary>
        public DBSchema DBSchema { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// <para>
        ///     Initializes the <see cref="DBContextOptions"/> with the most used options set to the correst values.
        ///     If the options have to be set differently per <see cref="DBContext"/>, the user has all the possibility to do so.
        /// </para>
        /// </summary>
        public DBContextOptions()
        {
        }

        /// <summary>
        /// Default constructor with connectionstring to be able to initialize the <see cref="DBF.DBActionProvider"/>
        /// <para>
        ///     Initializes the <see cref="DBContextOptions"/> with the most used options set to the correst values.
        ///     If the options have to be set differently per <see cref="DBContext"/>, the user has all the possibility to do so.
        /// </para>
        /// </summary>
        public DBContextOptions(string ConnectionString)
        {
            _ConnectionString = ConnectionString;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Sets the <see cref="DBF.DBActionProvider"/> for the <see cref="DBContext"/>.
        /// </summary>
        /// <typeparam name="TActionProvider">The implementation of the <see cref="DBF.DBActionProvider"/> to use for the <see cref="DBContext"/></typeparam>
        public void Use<TActionProvider>(bool IsSingleton = false) where TActionProvider : DBActionProvider, new()
        {
            // Generate the provider
            TActionProvider provider = null;
            TActionProvider provvy;

            // Try to create the given DBActionProvider
            try
            {
                // If the connectionstring is set, create the provider with the connection string else
                // Create the provider without a parameter in the constructor
                if (_ConnectionString != null && typeof(TActionProvider).GetConstructor(new Type[] { typeof(string) }) != null) //&& typeof(TActionProvider).GetConstructors().Length > 1 
                    provvy = (TActionProvider)Activator.CreateInstance(typeof(TActionProvider), _ConnectionString);
                else
                    provvy = (TActionProvider)Activator.CreateInstance(typeof(TActionProvider));
            }
            catch (Exception ex)
            {
                throw new DBActionProviderException("The DBActionProvider could not be instantiated", ex);
            }

            // If singleton is true, get a property in the Action Provider that is of it's own type (signifies a singleton instance)
            if (IsSingleton)
            {
                foreach (var prop in typeof(TActionProvider).GetProperties())
                    if (prop.PropertyType == typeof(TActionProvider))
                        provider = (TActionProvider)prop.GetValue(provvy);
            }
            else
                provider = provvy;

            // If the DBActionProvider was succesfully created...
            if (provider != null)
            {
                // Store the provider locally
                DBActionProvider = provider;

                if (DBSchema != null)
                    // Assign the DBSchema
                    provider.Schema = DBSchema;
                else
                    throw new DBActionProviderException("There was no database schema created in the action provider");
            }
        }

        #endregion
    }
}
