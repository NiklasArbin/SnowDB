using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;
using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    public class TransactionTests
    {

        [Test]
        public void Rollback_of_a_save_should_not_have_saved_a_new_document()
        {
            var store = TestBootStrapper.Container.Resolve<IDocumentStore>();
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling"
            };
            var key = "B8343419-F8CE-4993-A2E8-6DB763D5AEF1";
            TestSetup.SafeDeleteDocument<TestDocument>(key);

            using (var trx = new TransactionScope())
            {
                using (var session = store.OpenSession())
                {
                    session.Save(document, key);
                    Transaction.Current.Rollback();
                }
            }

            fileNameProvider.GetDocumentFile<TestDocument>(key, DateTime.Now).Exists.Should().BeFalse();

        }

        [Test]
        public void Rollback_of_a_delete_should_not_have_deleted_the_document()
        {
            var store = TestBootStrapper.Container.Resolve<IDocumentStore>();
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling"
            };
            var key = "2A4E55A8-B01B-4EB5-9EC5-C2DD49304098";


            using (var session = store.OpenSession())
            {
                session.Save(document, key);
            }

            fileNameProvider.GetDocumentFile<TestDocument>(key, DateTime.Now).Exists.Should().BeTrue();

            try
            {
                using (var trx = new TransactionScope())
                {
                    using (var session = store.OpenSession())
                    {
                        session.Delete<TestDocument>(key);
                        Transaction.Current.Rollback();
                    }
                    trx.Complete();
                }
            }
            catch (TransactionAbortedException)
            {

            }

            var fileThatShouldNotHaveBeenDeleted = fileNameProvider.GetDocumentFile<TestDocument>(key, DateTime.Now);
            fileThatShouldNotHaveBeenDeleted.Exists.Should().BeTrue();
            TestSetup.SafeDeleteDocument<TestDocument>(key);
        }

        [Test]
        public void Save_multiple_documents_in_the_same_transaction_should_save_all_documents()
        {
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var guids = new List<Guid>();
            for (int i = 0; i < 10; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            using (var trx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                var store = TestBootStrapper.Container.Resolve<IDocumentStore>();
                using (var session = store.OpenSession())
                {
                    foreach (var guid in guids)
                    {
                        var document = new TestDocument { SomeString = "blaha" };
                        session.Save(document, guid.ToString());
                    }
                }
                trx.Complete();
            }

            foreach (var guid in guids)
            {
                fileNameProvider.GetDocumentFile<TestDocument>(guid.ToString(), DateTime.Now).Exists.Should().BeTrue();
                fileNameProvider.GetDocumentFile<TestDocument>(guid.ToString(), DateTime.Now).Delete();
            }
        }
    }
}