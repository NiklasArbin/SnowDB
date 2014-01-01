using System.Data.Common;

namespace Snow.Core
{
    public class SnowConnectionStringBuilder : DbConnectionStringBuilder
    {
        public SnowConnectionStringBuilder(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public string DataLocation
        {
            get { return this["datalocation"] as string; }
            set { this["datalocation"] = value; }
        }

        public string DatabaseName
        {
            get { return this["databasename"] as string; }
            set { this["databasename"] = value; }
        }
    }
}