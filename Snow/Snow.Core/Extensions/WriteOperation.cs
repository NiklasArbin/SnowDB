using System;
using System.IO;
using System.Text;
using Snow.Core.Serializers;

namespace Snow.Core.Extensions
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

        protected override void Commit(FileStream lockedFileStream)
        {
            using (var stream = new StreamWriter(lockedFileStream))
            {
                stream.Write(_serializer.Serialize(Document));
            }
        }

        protected override void Rollback()
        {
            //TODO:Rollback....determine if new or existing
        }
    }
}