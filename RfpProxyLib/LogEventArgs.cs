using System;

namespace RfpProxyLib
{
    public enum LogDirection
    {
        Read,
        Written
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(LogDirection direction, string message)
        {
            Direction = direction;
            Message = message;
        }

        public LogDirection Direction { get; }

        public string Message { get; }
    }
}