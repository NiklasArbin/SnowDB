using System;
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
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling"
            };
            var key = "B8343419-F8CE-4993-A2E8-6DB763D5AEF1";
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument>(key).FullName);

            using (var session = store.OpenSession())
            {
                using (var trx = new TransactionScope())
                {
                    session.Save(document, key);
                    session.SaveChanges();
                    Transaction.Current.Rollback(); 
                }
            }

            fileNameProvider.GetDocumentFile<TestDocument>(key).Exists.Should().BeFalse();

        }

        [Test]
        public void Rollback_of_a_delete_should_not_have_deleted_the_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling"
            };
            var key = "2A4E55A8-B01B-4EB5-9EC5-C2DD49304098";
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument>(key).FullName);

            using (var session = store.OpenSession())
            {
                session.Save(document, key);
                session.SaveChanges();
            }

            using (var session = store.OpenSession())
            {
                using (var trx = new TransactionScope())
                {
                    session.Delete<TestDocument>(key);
                    session.SaveChanges();
                    Transaction.Current.Rollback();
                }
            }

            fileNameProvider.GetDocumentFile<TestDocument>(key).Exists.Should().BeTrue();
        }
        
        [Test]
        public void Save_multiple_documents_in_the_same_transaction_should_save_all_documents()
        {
            using (var trx = new TransactionScope())
            {
                var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
                using (var session = store.OpenSession())
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var document = new TestDocument { SomeString = "blaha" };
                        session.Save(document, Guid.NewGuid().ToString());
                    }

                    session.SaveChanges();
                }
                trx.Complete();
            }
        }
    }
}