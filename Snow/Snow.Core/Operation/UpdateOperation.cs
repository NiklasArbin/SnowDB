using System;
using Snow.Core.Lucene;
using Snow.Core.Serializers;

namespace Snow.Core.Operation
{
    internal class UpdateOperation<TDocument> : IOperation where TDocument : class
    {
        private readonly TDocument _document;
        private readonly IDocumentFile<TDocument> _documentFile;
        private readonly IDocumentSerializer _serializer;
        private readonly ISessionIndexer _snowIndexer;
        private readonly IDocumentFileNameProvider _fileNameProvider;

        public string Key { get; set; }
        public Guid SessionGuid { get; private set; }

        public UpdateOperation(TDocument document, string key, IDocumentSerializer serializer, Guid sessionGuid, ISessionIndexer snowIndexer, IDocumentFileNameProvider fileNameProvider, DateTime sessionStamp
            )
        {
            SessionGuid = sessionGuid;
            Key = key;

            _document = document;
            _documentFile = fileNameProvider.GetDocumentFile<TDocument>(Key, sessionStamp);
            _serializer = serializer;
            _snowIndexer = snowIndexer;
            _fileNameProvider = fileNameProvider;
        }

        public void Prepare()
        {
            _documentFile.Lock();
            var json = _serializer.Serialize(_document);
            _documentFile.Write(json);
            _documentFile.Unlock();
        }

        public void Commit()
        {
            
        }

        public void Rollback()
        {
            //TODO:Rollback....determine if new or existing
            //_snowIndexer.Rollback();
        }
    }
}