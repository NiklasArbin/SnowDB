using System;
using Snow.Core.Lucene;
using Snow.Core.Serializers;

namespace Snow.Core.Operation
{
    internal class WriteOperation<TDocument> : IOperation where TDocument : class
    {
        private readonly TDocument _document;
        private readonly IDocumentFile _documentFile;
        private readonly IDocumentSerializer _serializer;
        private readonly ISessionIndexer _snowIndexer;
        private readonly IDocumentFileNameProvider _fileNameProvider;

        public string Key { get; set; }
        public Guid SessionGuid { get; private set; }

        public WriteOperation(TDocument document, string key, IDocumentSerializer serializer, Guid sessionGuid, ISessionIndexer snowIndexer, IDocumentFileNameProvider fileNameProvider)
        {
            SessionGuid = sessionGuid;
            Key = key;

            _document = document;
            _documentFile = fileNameProvider.GetDocumentFile<TDocument>(Key);
            _serializer = serializer;
            _snowIndexer = snowIndexer;
            _fileNameProvider = fileNameProvider;
        }

        public void Prepare()
        {
            _documentFile.Lock();
            var json = _serializer.Serialize(_document);
            _documentFile.Write(json);
        }

        public void Commit()
        {
            _documentFile.Unlock();
        }

        public void Rollback()
        {
            //TODO:Rollback....determine if new or existing
            //_snowIndexer.Rollback();
        }
    }
}