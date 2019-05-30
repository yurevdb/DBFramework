using System;
using System.Collections.Generic;

namespace DBF
{
    /// <summary>
    /// Tracks any changes between the <see cref="DBContext"/> and the snapshots captured in the <see cref="ChangeTracker"/>
    /// </summary>
    internal class ChangeTracker
    {
        #region Private Members

        /// <summary>
        /// Private singleton instance of the ChangeTracker
        /// </summary>
        private static readonly ChangeTracker _Instance = new ChangeTracker();

        #endregion

        #region Public Properties

        /// <summary>
        /// The singleton instance of the <see cref="ChangeTracker"/> object
        /// </summary>
        internal static ChangeTracker Instance => _Instance;

        /// <summary>
        /// A <see cref="Queue{DBChange}"/> that holds all of the changes made to the <see cref="DBContext"/> compared to the remote database
        /// </summary>
        internal Queue<DBChange> Changes => new Queue<DBChange>();

        /// <summary>
        /// Indicates if the <see cref="ChangeTracker"/> has any changes stored
        /// </summary>
        internal bool HasChanges => Changes.Count > 0;

        /// <summary>
        /// The most current snapshot of the <see cref="DBSet{TModel}"/> of the <see cref="DBContext"/>
        /// </summary>
        internal HashSet<object> SnapShot { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Private Constructor used to instantiate the singleton instance
        /// </summary>
        private ChangeTracker()
        {
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Tries to detect any changes between the current state of the <see cref="DBContext"/> and the Snapshot of the latest synchronization
        /// </summary>
        public bool DetectChanges(DBContext context)
        {

            // Return wether there are changes in the context
            return Changes.Count > 0;
        }

        /// <summary>
        /// Take a snapshot of the given <see cref="DBContext"/>
        /// </summary>
        /// <param name="context">The <see cref="DBContext"/> to take a snapshot of</param>
        public void TakeSnapShot(DBContext context)
        {
            // Create a new snapshot
            HashSet<object> hashSet = new HashSet<object>();

            // Iterate over the properties in the given context
            foreach(var prop in context.GetType().GetProperties())
            {
                // If the property has no generic type arguments, then it's definitely not a DBSet
                if (prop.PropertyType.GenericTypeArguments.Length <= 0) continue;

                // We know the property is a dbset at this point
                // so is 1 generic type argument
                var model = prop.PropertyType.GenericTypeArguments[0];

                // Create a DBSet of the model
                var dbSet = Activator.CreateInstance(typeof(DBSet<>).MakeGenericType(model));

                // Check if the property is a DBSet
                if (!(prop.PropertyType == dbSet.GetType())) continue;

                // Set the value of the created DBSet to that of the corresponding DBSet in the given context
                var temp = prop.GetValue(context);

                // Dereference the DBSet and set it to the value of the property
                dbSet = typeof(DBSet<>).MakeGenericType(model).GetConstructor(new Type[] { typeof(DBSet<>).MakeGenericType(model) }).Invoke(new object[] { temp });

                // Add the created DBSet to the snapshot
                var added = hashSet.Add(dbSet);
            }

            // Set the snapshot to the created hashset
            SnapShot = hashSet;
        }

        #endregion
    }
}