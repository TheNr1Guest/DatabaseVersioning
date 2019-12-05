using System;

namespace PeopleWhoCanCode.DatabaseVersioning.Models
{
    public class ChangeScript
    {
        public Version Version { get; set; }
        public int Number { get; set; }
        public string Content { get; set; }

        public ChangeScript(Version version, int number, string content)
        {
            Version = version;
            Number = number;
            Content = content;
        }
    }
}
