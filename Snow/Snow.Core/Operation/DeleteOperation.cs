using System;
using System.IO;
using Snow.Core.Extensions;

namespace Snow.Core.Operation
{
    internal class DeleteOperation<TDocument> : IOperation where TDocument : class
    {
        private readonly IDocumentFileNameProvider _fileNameProvider;
        public string Key { get; set; }
        public Guid SessionGuid { get; private set; }
        private readonly IDocumentFile _documentFile;

        public DeleteOperation(IDocumentFileNameProvider fileNameProvider, string key, Guid sessionGuid)
        {
            Key = key;
            _fileNameProvider = fileNameProvider;
            SessionGuid = sessionGuid;
            _documentFile = fileNameProvider.GetDocumentFile<TDocument>(Key);
        }

        public void Prepare()
        {
            if (!_documentFile.Exists)
                throw new DocumentNotFoundException("Document {0} does not exist".FormatWith(Key));
            File.Copy(_documentFile.FullName, _fileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, SessionGuid).FullName, false);
            _documentFile.Delete();
        }

        public void Commit()
        {
            
        }

        public void Rollback()
        {
            if (_documentFile.Exists)
            {
                return;
            }

            File.Copy(_fileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, SessionGuid).FullName, _documentFile.FullName, true);
        }
    }
}