using System;

namespace DBF
{
    /// <summary>
    /// 
    /// </summary>
    public class DBAccessException : Exception
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public DBAccessException() : base() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        public DBAccessException(string Message) : base(Message) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="exception"></param>
        public DBAccessException(string Message, Exception exception) : base(Message, exception) { }

        #endregion
    }
}
