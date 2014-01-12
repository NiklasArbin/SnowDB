using System;
using Snow.Core.Serializers;

namespace Snow.Core.Operation
{
    internal class CreateOperation<TDocument> : IOperation where TDocument : class
    {
        private readonly TDocument _document;
        private readonly IDocumentFile<TDocument> _documentFile;
        private readonly IDocumentSerializer _serializer;
        private readonly DateTime _sessionDateStamp;

        public string Key { get; set; }
        public Guid SessionGuid { get; private set; }

        public CreateOperation(TDocument document, string key, IDocumentSerializer serializer, Guid sessionGuid, IDocumentFileNameProvider fileNameProvider, DateTime sessionDateStamp)
        {
            SessionGuid = sessionGuid;
            Key = key;

            _document = document;
            _documentFile = fileNameProvider.GetDocumentFile<TDocument>(Key, sessionDateStamp);
            _serializer = serializer;
            _sessionDateStamp = sessionDateStamp;
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