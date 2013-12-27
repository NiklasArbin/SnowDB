using System;
using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    [TestFixture]
    public class SessionTests
    {
        [Test]
        public void Get_should_throw_DocumentNotFoundException_for_non_existing_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var key = Guid.Parse("98CD87C0-1631-4CB9-BFC6-306CCBD15C8F");
            using (var session = store.OpenSession())
            {
                session.Invoking(x => x.Get<TestDocument>(key.ToString())).ShouldThrow<DocumentNotFoundException>();
            }
        }

        [Test]
        public void TryGet_should_return_false_for_a_non_existing_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var nonExistingKey = "193BEFBE-911E-433D-82EF-D22F1C6D43F2";
            using (var session = store.OpenSession())
            {
                TestDocument document;
                session.TryGet<TestDocument>(nonExistingKey, out document).Should().BeFalse();
            }
        }

        [Test]
        public void Save_should_save_a_new_document_if_it_does_not_exist()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var key = "77874B42-8093-450D-ADF1-861C55FB232C";
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument>(key).FullName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass { SomeDouble = 1.1 }
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, key);
                session.SaveChanges();
            }

            fileNameProvider.GetDocumentFile<TestDocument>(key).Exists.Should().BeTrue();
            fileNameProvider.GetDocumentFile<TestDocument>(key).Delete();
        }

        [Test]
        public void Delete_should_delete_the_document()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var key = "C8D16157-AEEF-461F-A9B0-673CA24E0F64";
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument>(key).FullName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling"
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, key);
                session.SaveChanges();
            }

            fileNameProvider.GetDocumentFile<TestDocument>(key).Exists.Should().BeTrue();


            using (var session = store.OpenSession())
            {
                session.Delete<TestDocument>(key);
                session.SaveChanges();
            }


            fileNameProvider.GetDocumentFile<TestDocument>(key).Exists.Should().BeFalse();
        }

        [Test]
        public void Get_should_retrieve_the_document_with_data_intact()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var key = "C4F3112E-03F6-4F73-8EA8-D93058D5F8B4";
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument>(key).FullName);
            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass()
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, key);
                session.SaveChanges();
            }

            TestDocument retrievedDocument;
            using (var session = store.OpenSession())
            {
                retrievedDocument = session.Get<TestDocument>(key);
            }

            retrievedDocument.SomeInt.Should().Be(1);
            retrievedDocument.SomeString.Should().Be("Stringaling");
            retrievedDocument.SomeSubClassProperty.Should().BeOfType<TestDocument.SomeSubClass>();
        }

        [Test]
        public void Save_should_be_able_to_save_two_different_types_of_documents_with_the_same_key()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var key = "B4996416-DF48-4F6F-B9F5-04AD151974D6";
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument>(key).FullName);
            TestSetup.SafeDeleteDocument(fileNameProvider.GetDocumentFile<TestDocument2>(key).FullName);

            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass()
            };
            var document2 = new TestDocument2 {AString = "AString"};

            using (var session = store.OpenSession())
            {
                session.Save(document, key);
                session.Save(document2, key);
                session.SaveChanges();
            }

            TestDocument readDocument;
            TestDocument2 reaDocument2;

            using (var session = store.OpenSession()) 
            {
                readDocument = session.Get<TestDocument>(key);
                reaDocument2 = session.Get<TestDocument2>(key);
            }

            readDocument.Should().BeOfType<TestDocument>();
            reaDocument2.Should().BeOfType<TestDocument2>();
        }



    }
}