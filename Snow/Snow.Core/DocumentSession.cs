using System;
using System.Collections.Generic;
using System.IO;
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
        private Dictionary<string, IOperation> _pendingChanges;

        private object _lock = new object();

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer)
        {
            _store = store;
            _fileNameProvider = new DocumentFileNameProvider(store.DataLocation, store.DatabaseName);
            _serializer = serializer;
            _pendingChanges = new Dictionary<string, IOperation>();
            _resourceGuid = Guid.NewGuid();
        }

        public TDocument Get<TDocument>(string key)
        {
            var file = _fileNameProvider.GetDocumentFile(key);
            if (!file.Exists)
                throw new DocumentNotFoundException(String.Format("Document {0} does not exist", key));

            string content;
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
            AddOperation(new WriteOperation(_fileNameProvider, _serializer, _encoding, _resourceGuid) { Key = key, Document = document });
        }

        public void Delete(string key)
        {
            AddOperation(new DeleteOperation(_fileNameProvider, _resourceGuid) { Key = key });
        }

        public void SaveChanges()
        {
            var currentTransaction = Transaction.Current;
            if (currentTransaction != null)
                currentTransaction.EnlistDurable(_resourceGuid, this, EnlistmentOptions.EnlistDuringPrepareRequired);

            foreach (var pendingChange in _pendingChanges.Values)
            {
                pendingChange.Execute();
            }
        }

        private void AddOperation(IOperation operation)
        {
            if (!_pendingChanges.ContainsKey(operation.Key))
            {
                _pendingChanges.Add(operation.Key, operation);
            }
            else
            {
                _pendingChanges[operation.Key] = operation;
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