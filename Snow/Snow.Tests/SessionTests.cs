using System;
using System.IO;
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
            var document = new TestDocument
            {
                SomeInt = 1,
                SomeString = "Stringaling",
                SomeSubClassProperty = new TestDocument.SomeSubClass { SomeDouble = 1.1 }
            };

            using (var session = store.OpenSession())
            {
                session.Save(document, key);
            }

            fileNameProvider.GetDocumentFile(key).Exists.Should().BeTrue();
        }
    }
}