using System;
using System.Transactions;
using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    [TestFixture]
    public class IsolationTests
    {
        [Test]
        public void A_new_document_saved_in_a_nested_transaction_should_not_be_visible_in_the_outer_transaction()
        {
            var store = TestBootStrapper.Container.Resolve<IDocumentStore>();

            var key = "D0AE63DC-F497-430F-B8ED-73B64F2863C2";
            TestSetup.SafeDeleteDocument<TestDocument>(key);

            using (var outer = new TransactionScope())
            {
                using (var session1 = store.OpenSession())
                {
                    session1.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, key);
                }

                using (var inner = new TransactionScope())
                {
                    using (var session2 = store.OpenSession())
                    {
                        session2.Invoking(s => s.Get<TestDocument>(key)).ShouldThrow<DocumentNotFoundException>();
                    }
                    inner.Complete();
                }
                outer.Complete();
            }
        }

        [Test]
        public void A_new_document_saved_in_a_seperate_transaction_should_not_be_visible_in_a_previously_started_transaction()
        {
            var store = TestBootStrapper.Container.Resolve<IDocumentStore>();

            var key = "E014CF2F-F77B-4F28-A90D-1BF53F10B57E";
            TestSetup.SafeDeleteDocument<TestDocument>(key);

            using (var outer = new TransactionScope())
            {
                

                using (var nestedButSeperate = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (var session1 = store.OpenSession())
                    {
                        session1.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, key);
                    }
                    nestedButSeperate.Complete();
                }

                using (var session2 = store.OpenSession())
                {
                    session2.Invoking(s => s.Get<TestDocument>(key)).ShouldThrow<DocumentNotFoundException>();
                }

                outer.Complete();
            }
        }

        [Test]
        public void A_document_deleted_in_a_seperate_transaction_should_not_be_deleted_in_a_previously_started_transaction()
        {
            var store = TestBootStrapper.Container.Resolve<IDocumentStore>();

            var key = "D0AE63DC-F497-430F-B8ED-73B64F2863C3";
            TestSetup.SafeDeleteDocument<TestDocument>(key);

            using (var session = store.OpenSession())
            {
                session.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, key);
            }

            using (var outer = new TransactionScope())
            {
                using (var nestedButSeperate = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (var session1 = store.OpenSession())
                    {
                        session1.Delete<TestDocument>(key);
                    }
                    nestedButSeperate.Complete();
                }

                using (var session2 = store.OpenSession())
                {
                    session2.Invoking(s => s.Get<TestDocument>(key)).ShouldNotThrow<DocumentNotFoundException>();
                }
                outer.Complete();
            }
        }

        [Test]
        public void A_document_changed_in_a_seperate_transaction_should_not_be_deleted_in_a_previously_started_transaction()
        {
            var store = TestBootStrapper.Container.Resolve<IDocumentStore>();

            var key = "D0AE63DC-F497-430F-B8ED-73B64F2863C4";
            TestSetup.SafeDeleteDocument<TestDocument>(key);

            using (var session = store.OpenSession())
            {
                session.Save(new TestDocument { SomeInt = 1, SomeString = "1" }, key);
            }

            using (var outer = new TransactionScope())
            {
                using (var nestedButSeperate = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (var session1 = store.OpenSession())
                    {
                        var doc = session1.Get<TestDocument>(key);
                        doc.SomeInt = 2;
                        session1.Save(doc, key);
                    }
                    nestedButSeperate.Complete();
                }

                using (var session2 = store.OpenSession())
                {
                    var docThatShouldNotHaveBeenChanged = session2.Get<TestDocument>(key);
                    docThatShouldNotHaveBeenChanged.SomeInt.Should().Be(1);
                }
                outer.Complete();
            }
        }

       
    }
}
