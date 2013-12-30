using System;
using System.IO;
using System.Transactions;

namespace Snow.Core
{
    internal class Key<TDocument> where TDocument : class 
    {
        public string Value { get; set; }
    }

    internal abstract class TransactionalOperation<TDocument> : IOperation, IEnlistmentNotification
        where TDocument : class
    {
        protected IDocumentFileNameProvider FileNameProvider;
        protected Guid ResourceManagerGuid;
        protected IDocumentFile DocumentFile;

        public string Key { get; set; }

        public virtual void Execute()
        {
            DocumentFile = FileNameProvider.GetDocumentFile<TDocument>(Key);

            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistDurable(ResourceManagerGuid, this, EnlistmentOptions.None);
            }
            else
            {
                LockFile();
                Commit(DocumentFile);
            }
        }

        protected abstract void Commit(IDocumentFile lockedFileStream);

        private void LockFile()
        {
            DocumentFile.Lock();
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (DocumentFile.Exists)
            {
                File.Copy(FileNameProvider.GetDocumentFile<TDocument>(Key).FullName, FileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, ResourceManagerGuid).FullName);
            }

            LockFile();
            preparingEnlistment.Prepared();
        }



        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            Commit(DocumentFile);
            var backupfile = new DocumentFile(FileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, ResourceManagerGuid).FullName);
            if (backupfile.Exists)
            {
                backupfile.Delete();
            }

            enlistment.Done();
        }
        protected abstract void Rollback();
        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            Rollback();
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            //Not really sure what to do about this state yet...
            enlistment.Done();
        }
    }
}