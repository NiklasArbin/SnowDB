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
        private readonly SortedDictionary<string, IOperation> _pendingChanges;

        public TransactionalResourceManager(IDocumentFileNameProvider fileNameProvider)
        {
            _fileNameProvider = fileNameProvider;

            _pendingChanges = new SortedDictionary<string, IOperation>();
        }

        public void Enlist(Guid sessionGuid)
        {
            _sessionGuid = sessionGuid;

            Transaction.Current.EnlistDurable(_sessionGuid, this, EnlistmentOptions.EnlistDuringPrepareRequired);
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            var dir = _fileNameProvider.GetTransactionDirectory(_sessionGuid);
            dir.Create();

            foreach (var pendingChange in _pendingChanges.Values)
            {
                pendingChange.Prepare();
            }
            preparingEnlistment.Prepared();
        }

        public void Commit()
        {
            
            foreach (var pendingChange in _pendingChanges.Values)
            {
                pendingChange.Commit();
            }
            
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            _pendingChanges.Clear();
            var dir = _fileNameProvider.GetTransactionDirectory(_sessionGuid);
            dir.Delete(true);
            enlistment.Done();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            foreach (var pendingChange in _pendingChanges.Values)
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

            if (!_pendingChanges.ContainsKey(fileName))
            {
                _pendingChanges.Add(fileName, operation);
            }
            else
            {
                _pendingChanges[fileName] = operation;
            }
        }
    }
}
