namespace DBF
{
    /// <summary>
    /// Defines an action on a database
    /// </summary>
    internal enum DBAction
    {
        /// <summary>
        /// Selects an item from a database
        /// </summary>
        Fetch,

        /// <summary>
        /// Inserts the item to a database
        /// </summary>
        Push,

        /// <summary>
        /// Deletes the item from a database
        /// </summary>
        Remove,

        /// <summary>
        /// Updates an item from a database
        /// </summary>
        Update
    }
}
