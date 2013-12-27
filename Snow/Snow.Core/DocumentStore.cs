using System;
using System.IO;
using Snow.Core.Serializers;

namespace Snow.Core
{
    public class DocumentStore : IDocumentStore
    {

        private void Initialize()
        {
            if (string.IsNullOrEmpty(DataLocation))
                throw new ArgumentException("DataLocation cannot be empty");
            if (string.IsNullOrEmpty(DatabaseName))
                throw new ArgumentException("DatabaseName cannot be empty");

            if (!Directory.Exists(DataLocation))
                throw new DirectoryNotFoundException(String.Format("The directory '{0}' doesn't exist", DataLocation));

            var dataDirectory = new DirectoryInfo(DataLocation);
            string dbDir = dataDirectory.FullName +"\\"+ DatabaseName;
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir).CreateSubdirectory("trx");
            }
                
        }

        public IDocumentSession OpenSession()
        {
            Initialize();
            return new DocumentSession(this, new JsonNetSerializer());
        }

        public string DatabaseName { get; set; }
        public string DataLocation { get; set; }
    }
}