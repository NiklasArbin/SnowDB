using System;
using System.Configuration;
using System.IO;
using System.Transactions;
using Snow.Core.Extensions;
using Snow.Core.Serializers;

namespace Snow.Core
{
    public class DocumentStore : IDocumentStore
    {
        public string DatabaseName { get; set; }
        public string DataLocation { get; set; }
        private IDocumentFileNameProvider _fileNameProvider;

        public DocumentStore()
        {

        }

        public DocumentStore(string connectionStringName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionString == null)
                throw new ArgumentException("Could not find '{0}' as named connection string".FormatWith(connectionStringName));
            var connectionStringBuilder = new SnowConnectionStringBuilder(connectionString.ConnectionString);

            DataLocation = connectionStringBuilder.DataLocation;
            DatabaseName = connectionStringBuilder.DatabaseName;
        }

        private void Initialize()
        {
            if (string.IsNullOrEmpty(DataLocation))
                throw new ArgumentException("DataLocation cannot be empty");
            if (string.IsNullOrEmpty(DatabaseName))
                throw new ArgumentException("DatabaseName cannot be empty");

            _fileNameProvider = new DocumentFileNameProvider(DataLocation, DatabaseName);

            if (!Directory.Exists(DataLocation))
                throw new DirectoryNotFoundException(String.Format("The directory '{0}' doesn't exist", DataLocation));

            var dataDirectory = new DirectoryInfo(DataLocation);
            string dbDir = dataDirectory.FullName + "\\" + DatabaseName;
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir + "\\trx");
            }
            var lucene = _fileNameProvider.GetLuceneRootDirectory();
            if (!lucene.Exists)
            {
                lucene.Create();
            }
        }

        public IDocumentSession OpenSession()
        {
            Initialize();
            return new DocumentSession(this, new JsonNetSerializer(), _fileNameProvider);
        }
    }
}