using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using log4net;
using Snow.Core.Serializers;

namespace Snow.Core
{
    public class SnowDocument : IObservable<SnowDocument>
    {
        public string Key { get; set; }
        public string Content { get; set; }

        public IDisposable Subscribe(IObserver<SnowDocument> observer)
        {
            return Disposable.Empty;
        }
    }

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

        private void SubscribeToDocument(IObservable<SnowDocument> document)
        {
            document.Subscribe(x => _log.DebugFormat("Subscribe triggered for document with key {0}", x.Key));
        }

        public TDocument Get<TDocument>(string key)
        {
            var file = _fileNameProvider.GetDocumentFile(key);
            if (!file.Exists)
                throw new DocumentNotFoundException(String.Format("Document {0} does not exist", key));
            var content = string.Empty;
            using (var sr = file.OpenText())
            {
                content = sr.ReadToEnd();
            }
            return _serializer.Deserialize<TDocument>(content);
        }

        public void Save<TDocument>(TDocument document, string key)
        {
            lock (_lock)
            {
                var file = _fileNameProvider.GetDocumentFile(key);
                using (var stream = new StreamWriter(file.Open(FileMode.OpenOrCreate, FileAccess.Write), _encoding))
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
    }
}