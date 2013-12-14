using System;
using System.IO;

namespace Snow.Core
{
    public class DocumentSession : IDocumentSession
    {
        private readonly IDocumentStore _store;
        private readonly IDocumentSerializer _serializer;
        private readonly string _databaseFilePath;
        private const string FileExtension = ".json";

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer)
        {
            _store = store;
            _serializer = serializer;
            _databaseFilePath = String.Format(@"{0}\{1}\", _store.DataLocation, store.Name);
        }

        public TDocument Get<TDocument>(object key)
        {
            return _serializer.Deserialize<TDocument>(File.ReadAllText(_databaseFilePath + key + FileExtension));
        }

        public void Store<TDocument>(TDocument document, object key)
        {
            using (var writer = new StreamWriter(File.Create(_databaseFilePath + key + FileExtension)))
            {
                writer.Write(_serializer.Serialize(document));
            }
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}