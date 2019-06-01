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
        /// Determines wether the <see cref="DBContext"/> should be synchronized at all times with the database.
        /// I.e. after each push, remove or update run the fetch action to the database and set the <see cref="DBSet{T}"/> to its current values.
        /// </summary>
        public bool SynchronizeDB { get; set; } = true;

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
        /// 
        /// </summary>
        internal DBSchema DBSchema { private get; set; }

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
        /// Default constructor
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
        /// 
        /// </summary>
        /// <typeparam name="TActionProvider"></typeparam>
        /// <param name="actionProvider"></param>
        public void Use<TActionProvider>() where TActionProvider : DBActionProvider, new()
        {
            TActionProvider provider = (TActionProvider)Activator.CreateInstance(typeof(TActionProvider), _ConnectionString);
            provider.Schema = DBSchema;
            DBActionProvider = provider;
        }

        #endregion
    }
}
