using System;
using System.IO;
using System.Transactions;

namespace Snow.Core.Operation
{
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
                DocumentFile.Lock();
                Commit(DocumentFile);
            }
        }

        protected abstract void Commit(IDocumentFile documentFile);

        

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (DocumentFile.Exists)
            {
                File.Copy(FileNameProvider.GetDocumentFile<TDocument>(Key).FullName, FileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, ResourceManagerGuid).FullName);
            }

            DocumentFile.Lock();
            preparingEnlistment.Prepared();
        }



        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            Commit(DocumentFile);
            DocumentFile.Unlock();
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