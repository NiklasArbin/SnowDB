using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Transactions;
using log4net;
using Snow.Core.Extensions;
using Snow.Core.Serializers;

namespace Snow.Core
{

    public class DocumentSession : IDocumentSession, IEnlistmentNotification
    {
        private readonly Guid _resourceGuid;

        private ILog _log = LogManager.GetLogger(typeof(DocumentSession));

        private readonly IDocumentStore _store;
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private readonly IDocumentSerializer _serializer;


        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly Dictionary<string, IOperation> _pendingChanges;

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer)
        {
            _store = store;
            _fileNameProvider = new DocumentFileNameProvider(store.DataLocation, store.DatabaseName);
            _serializer = serializer;
            _pendingChanges = new Dictionary<string, IOperation>();
            _resourceGuid = Guid.NewGuid();
        }

        public TDocument Get<TDocument>(string key) where TDocument : class
        {
            var file = _fileNameProvider.GetDocumentFile<TDocument>(key);
            if (!file.Exists)
                throw new DocumentNotFoundException(String.Format("Document {0} does not exist", key));

            string content;
            using (var sr = new StreamReader(file.GetStream()))
            {
                content = sr.ReadToEnd();
            }
            return _serializer.Deserialize<TDocument>(content);
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
            var currentTransaction = Transaction.Current;
            if (currentTransaction != null)
                currentTransaction.EnlistDurable(_resourceGuid, this, EnlistmentOptions.None);

            foreach (var pendingChange in _pendingChanges.Values)
            {
                pendingChange.Execute();
            }
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

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            _fileNameProvider.GetTransactionDirectory(_resourceGuid).Create();
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            var dir = _fileNameProvider.GetTransactionDirectory(_resourceGuid);
            if (dir.Exists)
                dir.Delete(true);
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}