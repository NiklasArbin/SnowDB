using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Snow.Core
{
    public class DocumentDocumentStore : IDocumentStore
    {
        private DirectoryInfo _dataDirectory;

        public void Initialize()
        {
            _dataDirectory = new DirectoryInfo(DataLocation);
            if(!_dataDirectory.Exists)
                throw new DirectoryNotFoundException(String.Format("The DataLocation '{0}' doesn't exist", DataLocation));
            _dataDirectory.MoveTo(Name);
            if(!_dataDirectory.Exists)
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

    public class DocumentSession : IDocumentSession
    {
        public void Get<TDocument>(object key)
        {
            throw new NotImplementedException();
        }

        public void Store<TDocument>(TDocument document)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }

    public interface IDocumentSession
    {
        void Get<TDocument>(object key);
        void Store<TDocument>(TDocument document);
        void SaveChanges();
    }
}
