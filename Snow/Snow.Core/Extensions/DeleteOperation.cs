using System;

namespace Snow.Core.Extensions
{
    internal class DeleteOperation : TransactionalOperation
    {   
        public DeleteOperation(IDocumentFileNameProvider fileNameProvider, Guid resourceManagerGuid)
        {
            FileNameProvider = fileNameProvider;
            ResourceManagerGuid = resourceManagerGuid;
        }

        public override void Execute()
        {
            DocumentFile = FileNameProvider.GetDocumentFile(Key);
            if (!DocumentFile.Exists)
                throw new DocumentNotFoundException("Document {0} does not exist".FormatWith(Key));

            base.Execute();
        }

        protected override void Commit()
        {
            DocumentFile.Delete();
        }

        protected override void Rollback()
        {
            if (DocumentFile.Exists)
            {
                return;
            }

            FileNameProvider.GetDocumentTransactionBackupFile(Key, ResourceManagerGuid).CopyTo(DocumentFile.FullName, true);
        }
    }
}