using System;
using System.Text;
using Snow.Core.Serializers;

namespace Snow.Core.Operation
{
    internal class WriteOperation<TDocument> : TransactionalOperation<TDocument> where TDocument : class
    {
        private readonly IDocumentSerializer _serializer;
        private readonly Encoding _encoding;
        public object Document { get; set; }

        public WriteOperation(IDocumentFileNameProvider fileNameProvider, IDocumentSerializer serializer, Encoding encoding, Guid resourceManagerGuid)
        {
            FileNameProvider = fileNameProvider;
            _serializer = serializer;
            _encoding = encoding;
            ResourceManagerGuid = resourceManagerGuid;
        }

        protected override void Commit(IDocumentFile documentFile)
        {
            documentFile.Write(_serializer.Serialize(Document));
        }

        protected override void Rollback()
        {
            //TODO:Rollback....determine if new or existing
        }
    }
}