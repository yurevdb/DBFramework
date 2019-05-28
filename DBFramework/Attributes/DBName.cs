using System;

namespace DBF
{
    /// <summary>
    /// Assigns a different name for the property to be found on a database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class DBName : Attribute
    {

        #region Public Properties

        /// <summary>
        /// The database name of the property
        /// </summary>
        public string Name {get;set;}

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the attribute with the given name for the property
        /// </summary>
        /// <param name="Name"></param>
        public DBName(string Name)
        {
            this.Name = Name;
        }

        #endregion

    }
}
