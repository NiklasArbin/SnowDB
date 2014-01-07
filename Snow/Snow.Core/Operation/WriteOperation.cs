using System;
using Snow.Core.Lucene;
using Snow.Core.Serializers;

namespace Snow.Core.Operation
{
    internal class WriteOperation<TDocument> : TransactionalOperation<TDocument> where TDocument : class
    {
        private readonly IDocumentSerializer _serializer;
        private readonly ISessionIndexer _snowIndexer;
        public object Document { get; set; }

        public WriteOperation(IDocumentFileNameProvider fileNameProvider, IDocumentSerializer serializer, Guid resourceManagerGuid, ISessionIndexer snowIndexer)
        {
            FileNameProvider = fileNameProvider;
            _serializer = serializer;
            _snowIndexer = snowIndexer;
            ResourceManagerGuid = resourceManagerGuid;
        }

        protected override void Commit(IDocumentFile documentFile)
        {
            var json = _serializer.Serialize(Document);
            documentFile.Write(json);
            _snowIndexer.Add<TDocument>(Key, json);
        }

        protected override void Rollback()
        {
            _snowIndexer.Rollback();
            //TODO:Rollback....determine if new or existing
        }
    }
}