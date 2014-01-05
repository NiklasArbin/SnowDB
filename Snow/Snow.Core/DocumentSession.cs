using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using log4net;
using Snow.Core.Operation;
using Snow.Core.Serializers;

namespace Snow.Core
{

    public class DocumentSession : IDocumentSession
    {
        private readonly Guid _resourceGuid;

        private ILog _log = LogManager.GetLogger(typeof(DocumentSession));

        private readonly IDocumentStore _store;
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private readonly IDocumentSerializer _serializer;


        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly Dictionary<string, IOperation> _pendingChanges;

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer, IDocumentFileNameProvider fileNameProvider)
        {
            _store = store;
            _serializer = serializer;
            _fileNameProvider = fileNameProvider;
            _pendingChanges = new Dictionary<string, IOperation>();
            _resourceGuid = Guid.NewGuid();
        }

        public TDocument Get<TDocument>(string key) where TDocument : class
        {
            var file = _fileNameProvider.GetDocumentFile<TDocument>(key);
            if (!file.Exists)
                throw new DocumentNotFoundException(String.Format("Document {0} does not exist", key));

            
            return _serializer.Deserialize<TDocument>(file.Read());
        }

        public bool TryGet<TDocument>(string key, out TDocument document) where TDocument : class
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

        public void Save<TDocument>(TDocument document, string key) where TDocument : class
        {
            AddOperation<TDocument>(new WriteOperation<TDocument>(_fileNameProvider, _serializer, _encoding, _resourceGuid) { Key = key, Document = document });
        }

        public void Delete<TDocument>(string key) where TDocument : class
        {
            AddOperation<TDocument>(new DeleteOperation<TDocument>(_fileNameProvider, _resourceGuid) { Key = key });
        }

        public void SaveChanges()
        {
            var dir = _fileNameProvider.GetTransactionDirectory(_resourceGuid);
            if (Transaction.Current != null)
                dir.Create();

            foreach (var pendingChange in _pendingChanges.Values)
            {
                pendingChange.Execute();
            }

            _pendingChanges.Clear();

            if (dir.Exists)
                dir.Delete(true);
        }

        private void AddOperation<TDocument>(IOperation operation) where TDocument : class
        {
            var fileName = _fileNameProvider.GetDocumentFile<TDocument>(operation.Key).Name;

            if (!_pendingChanges.ContainsKey(fileName))
            {
                _pendingChanges.Add(fileName, operation);
            }
            else
            {
                _pendingChanges[fileName] = operation;
            }
        }

        public void Dispose()
        {

        }
    }
}