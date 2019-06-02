using System;

namespace DBF
{
    /// <summary>
    /// 
    /// </summary>
    public class DBActionProviderException : Exception
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public DBActionProviderException() : base() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        public DBActionProviderException(string Message) : base(Message) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="exception"></param>
        public DBActionProviderException(string Message, Exception exception) : base(Message, exception) { }

        #endregion
    }
}
