using System;
using System.Collections.Generic;
using System.Transactions;
using Snow.Core.Operation;

namespace Snow.Core
{
    public class TransactionalResourceManager : IEnlistmentNotification
    {
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private Guid _sessionGuid;
        public readonly SortedDictionary<string, IOperation> PendingChanges;

        public TransactionalResourceManager(IDocumentFileNameProvider fileNameProvider, Guid sessionGuid)
        {
            _fileNameProvider = fileNameProvider;
            _sessionGuid = sessionGuid;

            PendingChanges = new SortedDictionary<string, IOperation>();
        }

        public void Enlist()
        {
            Transaction.Current.EnlistDurable(_sessionGuid, this, EnlistmentOptions.None);
        }

        public void Prepare()
        {
            var dir = _fileNameProvider.GetTransactionDirectory(_sessionGuid);
            dir.Create();

            foreach (var pendingChange in PendingChanges.Values)
            {
                pendingChange.Prepare();
            }
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            Prepare();
            preparingEnlistment.Prepared();
        }

        public void Commit()
        {
            foreach (var pendingChange in PendingChanges.Values)
            {
                pendingChange.Commit();
            }
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            PendingChanges.Clear();
            var dir = _fileNameProvider.GetTransactionDirectory(_sessionGuid);
            dir.Delete(true);
            enlistment.Done();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            foreach (var pendingChange in PendingChanges.Values)
            {
                pendingChange.Rollback();
            }
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void AddOperation<TDocument>(IOperation operation) where TDocument : class
        {
            var fileName = _fileNameProvider.GetDocumentFile<TDocument>(operation.Key).Name;

            if (!PendingChanges.ContainsKey(fileName))
            {
                PendingChanges.Add(fileName, operation);
            }
            else
            {
                PendingChanges[fileName] = operation;
            }
        }
    }
}
