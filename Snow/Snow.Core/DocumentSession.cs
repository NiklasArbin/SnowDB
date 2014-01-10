using System;
using System.Transactions;
using log4net;
using Snow.Core.Lucene;
using Snow.Core.Operation;
using Snow.Core.Serializers;

namespace Snow.Core
{

    public class DocumentSession : IDocumentSession
    {
        private ILog _log = LogManager.GetLogger(typeof(DocumentSession));

        private readonly IDocumentStore _store;
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private readonly IDocumentSerializer _serializer;
        private readonly ISessionIndexer _sessionIndexer;
        public Guid SessionGuid { get; private set; }
        private readonly TransactionalResourceManager _resourceManager;
        private TransactionScope _trx;

        public DocumentSession(IDocumentStore store, IDocumentSerializer serializer, IDocumentFileNameProvider fileNameProvider)
        {
            _store = store;
            _serializer = serializer;
            _fileNameProvider = fileNameProvider;
            SessionGuid = Guid.NewGuid();
            _sessionIndexer = new SessionIndexer(SessionGuid, fileNameProvider);
            _resourceManager = new TransactionalResourceManager(fileNameProvider, SessionGuid);

            _trx = new TransactionScope(TransactionScopeOption.Required);
            _resourceManager.Enlist(); 
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
            _resourceManager.AddOperation<TDocument>(new WriteOperation<TDocument>(document, key, _serializer, SessionGuid, _sessionIndexer, _fileNameProvider));
        }

        public void Delete<TDocument>(string key) where TDocument : class
        {
            _resourceManager.AddOperation<TDocument>(new DeleteOperation<TDocument>(_fileNameProvider, key, SessionGuid));
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