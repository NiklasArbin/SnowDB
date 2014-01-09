using System;
using System.IO;
using Snow.Core.Extensions;

namespace Snow.Core.Operation
{
    internal class DeleteOperation<TDocument> : TransactionalOperation<TDocument> where TDocument : class
    {
        public DeleteOperation(IDocumentFileNameProvider fileNameProvider, Guid resourceManagerGuid) : base(resourceManagerGuid)
        {
            FileNameProvider = fileNameProvider;
        }

        public override void Prepare()
        {
            if (!DocumentFile.Exists)
                throw new DocumentNotFoundException("Document {0} does not exist".FormatWith(Key));

            base.Prepare();
        }

        protected override void Commit(IDocumentFile documentFile)
        {
            documentFile.Delete();
        }

        protected override void Rollback()
        {
            if (DocumentFile.Exists)
            {
                return;
            }

            File.Copy(FileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, SessionGuid).FullName, DocumentFile.FullName, true);
        }
    }
}