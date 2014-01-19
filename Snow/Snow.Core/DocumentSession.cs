using System;
using System.Transactions;
using log4net;
using Snow.Core.Lucene;
using Snow.Core.Operation;
using Snow.Core.Serializers;
using Snow.Core.Transactions;

namespace Snow.Core
{

    public class DocumentSession : IDocumentSession
    {
        private ILog _log = LogManager.GetLogger(typeof(DocumentSession));

        private readonly IDocumentStore _store;
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private readonly ITransactionCounter _transactionCounter;
        private readonly IDocumentSerializer _serializer;
        private readonly ISessionIndexer _sessionIndexer;
        public Guid SessionGuid { get; private set; }
        private readonly TransactionalResourceManager _resourceManager;
        private TransactionScope _trx;
        private DateTime _sessionStamp;

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer, IDocumentFileNameProvider fileNameProvider, ITransactionCounter transactionCounter)
        {
            _store = store;
            _serializer = serializer;
            _fileNameProvider = fileNameProvider;
            _transactionCounter = transactionCounter;
            SessionGuid = Guid.NewGuid();
            _sessionIndexer = new SessionIndexer(SessionGuid, fileNameProvider);
            _resourceManager = new TransactionalResourceManager(fileNameProvider, SessionGuid);

            _trx = new TransactionScope(TransactionScopeOption.Required);
            _sessionStamp = Transaction.Current.TransactionInformation.CreationTime;
            _resourceManager.Enlist();
        }

        public TDocument Get<TDocument>(string key) where TDocument : class
        {
            var file = _fileNameProvider.GetDocumentFile<TDocument>(key, _sessionStamp);
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
            _resourceManager.AddOperation<TDocument>(new UpdateOperation<TDocument>(document, key, _serializer, SessionGuid, _sessionIndexer, _fileNameProvider, _sessionStamp));
        }

        public void Delete<TDocument>(string key) where TDocument : class
        {
            _resourceManager.AddOperation<TDocument>(new DeleteOperation<TDocument>(_fileNameProvider, key, SessionGuid, _sessionStamp));
        }

        private void SaveChanges()
        {
            //_sessionIndexer.Open();
            //_sessionIndexer.Prepare();
            //_sessionIndexer.Commit();
        }

        public void Dispose()
        {
            _trx.Complete();
            _trx.Dispose();
            //_resourceManager.Commit();
            //_sessionIndexer.Dispose();
        }
    }
}