using System;
using System.Collections.Generic;
using System.Linq;
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

        private void Prepare()
        {
            if (PendingChanges.Any())
            {
                foreach (var pendingChange in PendingChanges.Values)
                {
                    pendingChange.Prepare();
                }
            }
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            Prepare();
            preparingEnlistment.Done();
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            PendingChanges.Clear();
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
            var uniqueKey = String.Format("{0}.{1}", typeof(TDocument).FullName, operation.Key);

            if (!PendingChanges.ContainsKey(uniqueKey))
            {
                PendingChanges.Add(uniqueKey, operation);
            }
            else
            {
                PendingChanges[uniqueKey] = operation;
            }
        }
    }
}
