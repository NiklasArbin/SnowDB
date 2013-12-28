using System;
using System.IO;

namespace Snow.Core.Extensions
{
    internal class DeleteOperation<TDocument> : TransactionalOperation<TDocument> where TDocument : class
    {   
        public DeleteOperation(IDocumentFileNameProvider fileNameProvider, Guid resourceManagerGuid)
        {
            FileNameProvider = fileNameProvider;
            ResourceManagerGuid = resourceManagerGuid;
        }

        public override void Execute()
        {
            DocumentFile = FileNameProvider.GetDocumentFile<TDocument>(Key);
            if (!DocumentFile.Exists)
                throw new DocumentNotFoundException("Document {0} does not exist".FormatWith(Key));

            base.Execute();
        }

        protected override void Commit(FileStream lockedFileStream)
        {
            lockedFileStream.Close();
            DocumentFile.Delete();
        }

        protected override void Rollback()
        {
            if (DocumentFile.Exists)
            {
                return;
            }

            FileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, ResourceManagerGuid).CopyTo(DocumentFile.FullName, true);
        }
    }
}