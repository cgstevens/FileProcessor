using System.IO;
using Akka.Configuration;

namespace Shared.Helpers
{
    public static class HoconLoader
    {
        public static Config ParseConfig(string hoconPath)
        {
            return ConfigurationFactory.ParseString(File.ReadAllText(hoconPath));
        }
    }
}
