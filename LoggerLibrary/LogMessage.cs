using System;
using System.Collections.Generic;
using System.Text;

namespace LoggerLibrary
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(LogMessage left, LogMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LogMessage left, LogMessage right)
        {
            return !(left == right);
        }
    }
}
