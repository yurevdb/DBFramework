using System;

namespace DBF
{
    public class DBSetChangedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
