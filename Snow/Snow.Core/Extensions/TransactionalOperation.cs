using System;
using System.IO;
using System.Transactions;

namespace Snow.Core.Extensions
{
    internal abstract class TransactionalOperation : IOperation, IEnlistmentNotification
    {
        protected IDocumentFileNameProvider FileNameProvider;
        protected Guid ResourceManagerGuid;
        protected FileInfo DocumentFile;
        private FileStream _fileStream;
        public string Key { get; set; }

        public virtual void Execute()
        {
            DocumentFile = FileNameProvider.GetDocumentFile(Key);

            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistDurable(ResourceManagerGuid, this, EnlistmentOptions.None);
            }
            else
            {
                Commit();
            }
        }

        protected abstract void Commit();

        private void LockFile()
        {
            _fileStream = DocumentFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        private void UnlockFile()
        {
            _fileStream.Close();
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (DocumentFile.Exists)
            {
                DocumentFile.CopyTo(FileNameProvider.GetDocumentTransactionBackupFile(Key, ResourceManagerGuid).FullName);
            }
                
            LockFile();
            preparingEnlistment.Prepared();
        }

        

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            UnlockFile();
            Commit();
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