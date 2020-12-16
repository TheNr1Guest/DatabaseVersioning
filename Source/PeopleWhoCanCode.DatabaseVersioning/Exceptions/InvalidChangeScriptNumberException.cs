using System;

namespace PeopleWhoCanCode.DatabaseVersioning.Exceptions
{
    public class InvalidChangeScriptNumberException : Exception
    {
        public string InvalidNumber { get; }

        public InvalidChangeScriptNumberException(string invalidNumber)
        {
            InvalidNumber = invalidNumber;
        }
    }
}
