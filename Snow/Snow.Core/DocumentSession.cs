using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Transactions;
using log4net;
using Snow.Core.Serializers;

namespace Snow.Core
{
   
    public class DocumentSession : IDocumentSession
    {
        private ILog _log = LogManager.GetLogger(typeof(DocumentSession));

        private readonly IDocumentStore _store;
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private readonly IDocumentSerializer _serializer;


        private readonly Encoding _encoding = new UTF8Encoding();

        private Queue<KeyValuePair<string, object>> _pendingChanges;
        private object _lock = new object();

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer)
        {
            _store = store;
            _fileNameProvider = new DocumentFileNameProvider(store.DataLocation, store.DatabaseName);
            _serializer = serializer;
            _pendingChanges = new Queue<KeyValuePair<string, object>>();
        }

        public TDocument Get<TDocument>(string key)
        {
            var file = _fileNameProvider.GetDocumentFile(key);
            if (!file.Exists)
                throw new DocumentNotFoundException(String.Format("Document {0} does not exist", key));
            var content = string.Empty;
            using (var sr = new StreamReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                content = sr.ReadToEnd();
            }
            return _serializer.Deserialize<TDocument>(content);
        }

        public bool TryGet<TDocument>(string key, out TDocument document)
        {
            try
            {
                document = Get<TDocument>(key);
                return true;
            }
            catch (DocumentNotFoundException)
            {
                document = default(TDocument);
                return false;
            }
        }

        public void Save<TDocument>(TDocument document, string key)
        {
            lock (_lock)
            {
                var file = _fileNameProvider.GetDocumentFile(key);
                using (var stream = new StreamWriter(file.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None), _encoding))
                {
                    stream.Write(_serializer.Serialize(document));
                }
            }
        }

        public void SaveChanges()
        {
            lock (_lock)
            {
                while (_pendingChanges.Any())
                {
                    var change = _pendingChanges.Dequeue();

                    using (var fileStream = new FileStream(_fileNameProvider.GetDocumentFile(change.Key).FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (var writer = new StreamWriter(fileStream))
                        {
                            writer.Write(_serializer.Serialize((dynamic)change.Value));
                        }
                    }
                }
            }
        }

        private void EnsureFileLocks()
        {

        }


        public void Dispose()
        {

        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            throw new NotImplementedException();
        }

        public void Commit(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        public void Rollback(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        public void InDoubt(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }
    }
}