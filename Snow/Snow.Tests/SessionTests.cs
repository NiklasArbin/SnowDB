using System;
using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    [TestFixture]
    public class SessionTests
    {
        private const string NonExistingKey = "86DF4E87-35DC-489F-BA13-2A802AB9A693";

        
        [Test]
        public void Get_should_throw_DocumentNotFoundException_for_non_existing_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            
            var Key = "2B8EB412-2E39-4661-85E1-D1EAF06F74BF";
            TestSetup.SafeDeleteDocument<TestDocument>(Key);

            using (var session = store.OpenSession())
            {
                session.Invoking(x => x.Get<TestDocument>(NonExistingKey)).ShouldThrow<DocumentNotFoundException>();
            }
        }

        [Test]
        public void TryGet_should_return_false_for_a_non_existing_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            using (var session = store.OpenSession())
            {
                TestDocument document;
                session.TryGet(NonExistingKey, out document).Should().BeFalse();
            }
        }

        [Test]
        public void Save_should_save_a_new_document_if_it_does_not_exist()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);

            var Key = "2B8EB412-2E39-4661-85E1-D2EAF06F74BF";
            TestSetup.SafeDeleteDocument<TestDocument>(Key);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass { SomeDouble = 1.1 }
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, Key);
            }

            fileNameProvider.GetDocumentFile<TestDocument>(Key, DateTime.Now).Exists.Should().BeTrue();
        }

        [Test]
        public void Delete_should_delete_the_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);

            var Key = "2B8EB412-2E39-4661-85E1-D1EAF06F75BF";
            TestSetup.SafeDeleteDocument<TestDocument>(Key);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling"
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, Key);
            }

            fileNameProvider.GetDocumentFile<TestDocument>(Key, DateTime.Now).Exists.Should().BeTrue();


            using (var session = store.OpenSession())
            {
                session.Delete<TestDocument>(Key);
            }


            fileNameProvider.GetDocumentFile<TestDocument>(Key, DateTime.Now).Exists.Should().BeFalse();
        }

        [Test]
        public void Get_should_retrieve_the_document_with_data_intact()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };

            var Key = "B5550A31-B28F-4011-9240-A613D5DE82C1";
            TestSetup.SafeDeleteDocument<TestDocument>(Key);
            
            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass()
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, Key);
            }

            TestDocument retrievedDocument;
            using (var session = store.OpenSession())
            {
                retrievedDocument = session.Get<TestDocument>(Key);
            }

            retrievedDocument.SomeInt.Should().Be(1);
            retrievedDocument.SomeString.Should().Be("Stringaling");
            retrievedDocument.SomeSubClassProperty.Should().BeOfType<TestDocument.SomeSubClass>();
        }

        [Test]
        public void Save_should_be_able_to_save_two_different_types_of_documents_with_the_same_key()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };

            var Key = "D84A682F-8300-4F98-A459-D4C40CC8064C";
            TestSetup.SafeDeleteDocument<TestDocument>(Key);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass()
            };
            var document2 = new TestDocument2 { AString = "AString" };

            using (var session = store.OpenSession())
            {
                session.Save(document, Key);
                session.Save(document2, Key);
            }

            TestDocument readDocument;
            TestDocument2 reaDocument2;

            using (var session = store.OpenSession())
            {
                readDocument = session.Get<TestDocument>(Key);
                reaDocument2 = session.Get<TestDocument2>(Key);
            }

            readDocument.Should().BeOfType<TestDocument>();
            reaDocument2.Should().BeOfType<TestDocument2>();
        }



    }
}