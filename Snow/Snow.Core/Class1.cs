using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Snow.Core
{
    public class DocumentStore : IDocumentStore
    {
        private DirectoryInfo _dataDirectory;

        public void Initialize()
        {
            _dataDirectory = new DirectoryInfo(DataLocation);
            if (!_dataDirectory.Exists)
                throw new DirectoryNotFoundException(String.Format("The DataLocation '{0}' doesn't exist", DataLocation));
            _dataDirectory.MoveTo(Name);
            if (!_dataDirectory.Exists)
                _dataDirectory.Create();
        }

        public IDocumentSession OpenSession()
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }
        public string DataLocation { get; set; }
    }

    public interface IDocumentStore
    {
        void Initialize();
        IDocumentSession OpenSession();
        string Name { get; set; }
        string DataLocation { get; set; }
    }

    public interface IDocumentSession
    {
        TDocument Get<TDocument>(object key);
        void Store<TDocument>(TDocument document, object key);
        void SaveChanges();
    }

    public class JsonNet : IDocumentSerializer
    {
        public string Serialize<TDocument>(TDocument value)
        {
           return JsonConvert.SerializeObject(value);
        }

        public TDocument Deserialize<TDocument>(string serialized)
        {
            return JsonConvert.DeserializeObject<TDocument>(serialized);
        }
    }

    public interface IDocumentSerializer
    {
        string Serialize<TDocument>(TDocument value);
        TDocument Deserialize<TDocument>(string serialized);
    }
}
