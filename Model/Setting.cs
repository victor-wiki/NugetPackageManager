using System.Collections.Generic;

namespace NugetPackageManager
{
    public class Setting
    {
        public string KeyFilePath { get; set; }
        public string KeyItemDelimiter { get; set; } = ":";
        public List<KeyPattern> KeyPatterns { get; set; } = new List<KeyPattern>();
    }

    public class KeyPattern
    {
        public string KeyName { get; set; }
        public string Pattern { get; set; }
    }
}
