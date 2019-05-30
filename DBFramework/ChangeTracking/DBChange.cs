namespace DBF
{
    /// <summary>
    /// Information about a change that can instruct the <see cref="DBContext"/> to call the <see cref="IDBActionProvider"/> with the stored <see cref="DBAction"/>
    /// </summary>
    internal class DBChange
    {
        /// <summary>
        /// The action the <see cref="DBContext"/> should take to synchronized the change with the remote database
        /// </summary>
        public DBAction Action { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Value { get; set; }
    }
}
