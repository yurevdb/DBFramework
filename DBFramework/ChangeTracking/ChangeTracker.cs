﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        internal Queue<DBChange> Changes { get; private set; } = new Queue<DBChange>();

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
            // Parse the changes and add them to the queue
            foreach (var change in ParseChanges(context))
                Changes.Enqueue(change);

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
                var type = typeof(DBSet<>).MakeGenericType(model);
                var ctor = type.GetConstructor(new Type[] { type });
                dbSet = ctor.Invoke(new object[] { temp });

                // Add the created DBSet to the snapshot
                var added = hashSet.Add(dbSet);
            }

            // Set the snapshot to the created hashset
            SnapShot = hashSet;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        ///     Parse the <see cref="DBContext"/> to detect any changes with the <see cref="SnapShot"/> and yield those changes as <see cref="DBChange"/>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     The use of dynamic variables is to be able to use anything I would like because I know the sets will be of type <see cref="DBSet{TModel}"/>.
        ///     But the compiler wants a strongly typed type to work with and won't cast to a <see cref="DBSet{TModel}"/> without knowing the TModel. 
        ///     The TModel will be unknown to us, but we don't need it at all. So if it's of an <see cref="object"/> type, 
        ///     that's good enough to signal that there was a change with a set and to return the item that was changed.
        /// </para>
        /// <para>
        ///     For these reason I will use dynamic variables substituting for <see cref="DBSet{TModel}"/> because all the other checks in place
        ///     can guarantee that the sets will be of the correct types and have the capabilities to get the Count, iterate over the elements and
        ///     get the element of a specified index.
        /// </para>
        /// </remarks>
        /// <param name="context">The <see cref="DBContext"/> to compare the snapshot with</param>
        /// <returns></returns>
        private IEnumerable<DBChange> ParseChanges(DBContext context)
        {
            // Iterate over the DBSets
            foreach(dynamic localSet in context.GetType().GetProperties().Where(prop => prop.PropertyType.GetGenericArguments().Length > 0 && prop.PropertyType == typeof(DBSet<>).MakeGenericType(prop.PropertyType.GetGenericArguments().First())).Select(prop => prop.GetValue(context)))
            {
                // Get the remote set corresponding to the local set
                dynamic remoteSet = SnapShot.Where(set => set.GetType() == localSet.GetType()).First();

                // If an item or items were added or removed
                if (localSet.Count != remoteSet.Count)
                {
                    // If item/items were added
                    if (localSet.Count > remoteSet.Count)
                    {
                        foreach(var item in localSet)
                        {
                            // Check if the remote set contains the item of the local set based on the primary key
                            // This is to guarantee that no updated items would be considered as new and therefor should be pushed instead of updated
                            if (!remoteSet.Contains(item, context.Schema))
                                yield return new DBChange { Action = DBAction.Push, Value = item };
                        }
                    }

                    // If item/items were removed
                    if (localSet.Count < remoteSet.Count)
                    {
                        foreach (var item in remoteSet)
                        {
                            if (!localSet.Contains(item, context.Schema))
                                yield return new DBChange { Action = DBAction.Remove, Value = item };
                        }
                    }
                }

                // There might be an update to an item
                // Iterate over every item in the remote set and if the local set item is different
                // Add it as a DBChange
                for (int i = 0; i < remoteSet.Count; i++)
                {
                    var remoteItem = remoteSet[i];
                    var localItem = localSet[i];

                    if (remoteItem != localItem)
                    {
                        yield return new DBChange { Action = DBAction.Update, Value = localItem };
                    }
                }
            }

            // Stop the function
            yield break;
        }

        #endregion
    }
}