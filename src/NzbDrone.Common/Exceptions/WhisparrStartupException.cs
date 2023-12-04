using System;

namespace NzbDrone.Common.Exceptions
{
    public class WhisparrStartupException : NzbDroneException
    {
        public WhisparrStartupException(string message, params object[] args)
            : base("Whisparr failed to start: " + string.Format(message, args))
        {
        }

        public WhisparrStartupException(string message)
            : base("Whisparr failed to start: " + message)
        {
        }

        public WhisparrStartupException()
            : base("Whisparr failed to start")
        {
        }

        public WhisparrStartupException(Exception innerException, string message, params object[] args)
            : base("Whisparr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public WhisparrStartupException(Exception innerException, string message)
            : base("Whisparr failed to start: " + message, innerException)
        {
        }

        public WhisparrStartupException(Exception innerException)
            : base("Whisparr failed to start: " + innerException.Message)
        {
        }
    }
}
