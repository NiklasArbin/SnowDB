using System.Configuration;
using System.Data.SqlClient;

namespace Snow.Core
{
    public class Configuration : ConfigurationSection
    {
        public static Configuration Configure()
        {
            var config = ConfigurationManager.GetSection("snowDbConfiguration") as Configuration;
            return config ?? new Configuration();
        }

        [ConfigurationProperty("datalocation", IsRequired = true)]
        public string DataLocation
        {
            get { return this["datalocation"] as string; }
        }
    }
}
