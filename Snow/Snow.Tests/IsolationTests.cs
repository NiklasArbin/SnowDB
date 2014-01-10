using System.Transactions;
using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    [TestFixture]
    public class IsolationTests
    {
        private const string Key = "09915C3D-520A-4AB8-9286-C30CEF009C6C";

        [Test]
        public void A_new_document_saved_in_a_nested_transaction_should_not_be_visible_in_the_outer_transaction()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };

            using (var outer = new TransactionScope())
            {
                using (var session1 = store.OpenSession())
                {
                    session1.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, Key);
                }

                using (var inner = new TransactionScope())
                {
                    using (var session2 = store.OpenSession())
                    {
                        session2.Invoking(s => s.Get<TestDocument>(Key)).ShouldThrow<DocumentNotFoundException>();
                    }
                    inner.Complete();
                }
                outer.Complete();
            }
        }

        [Test]
        public void A_new_document_saved_in_a_seperate_transaction_should_not_be_visible_in_a_previously_started_transaction()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };

            using (var outer = new TransactionScope())
            {
                using (var nestedButSeperate = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (var session1 = store.OpenSession())
                    {
                        session1.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, Key);
                    }
                    nestedButSeperate.Complete();
                }

                using (var session2 = store.OpenSession())
                {
                    session2.Invoking(s => s.Get<TestDocument>(Key)).ShouldThrow<DocumentNotFoundException>();
                }

                outer.Complete();
            }
        }

        [Test]
        public void A_deleted_document_in_a_seperate_transaction_should_not_be_deleted_in_a_previously_started_transaction()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };

            using (var session = store.OpenSession())
            {
                session.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, Key);
            }

            using (var outer = new TransactionScope())
            {
                using (var nestedButSeperate = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (var session1 = store.OpenSession())
                    {
                        session1.Delete<TestDocument>(Key);
                    }
                    nestedButSeperate.Complete();
                }

                using (var session2 = store.OpenSession())
                {
                    session2.Invoking(s => s.Get<TestDocument>(Key)).ShouldNotThrow<DocumentNotFoundException>();
                }
                outer.Complete();
            }
        }

        [TearDown]
        public void TearDown()
        {
            var documentFileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            TestSetup.SafeDeleteDocument(documentFileNameProvider.GetDocumentFile<TestDocument>(Key).FullName);
        }
    }
}
