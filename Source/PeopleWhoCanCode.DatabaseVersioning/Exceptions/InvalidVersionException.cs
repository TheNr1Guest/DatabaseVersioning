using System;

namespace PeopleWhoCanCode.DatabaseVersioning.Exceptions
{
    public class InvalidVersionException : Exception
    {
        public string InvalidVersion { get; set; }

        public InvalidVersionException(string invalidVersion)
        {
            InvalidVersion = invalidVersion;
        }
    }
}
