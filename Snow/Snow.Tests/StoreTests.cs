using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    [TestFixture]
    public class StoreTests
    {

        [Test]
        public void OpenSession_should_return_a_new_session()
        {
            var store = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            var session = store.OpenSession();

            session.Should().NotBeNull();
            session.Should().BeAssignableTo<IDocumentSession>();
        }

        [Test]
        public void OpenSession_should_throw_exception_if_datadirectory_is_not_specified()
        {
            var store = new DocumentStore { DatabaseName = TestSetup.DatabaseName };
            store.Invoking(x => x.OpenSession()).ShouldThrow<ArgumentException>();
        }

        [Test]
        public void OpenSession_should_throw_exception_if_databaseName_is_not_specified()
        {
            var store = new DocumentStore() { DataLocation = TestSetup.DataDir };
            store.Invoking(x => x.OpenSession()).ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Initialize_should_throw_exception_when_datadirectory_does_not_exist()
        {
            const string invalidPath = @"c:\invalid_path";
            var store = new DocumentStore { DataLocation = invalidPath, DatabaseName = TestSetup.DatabaseName };
            store.Invoking(x => x.OpenSession()).ShouldThrow<DirectoryNotFoundException>();
        }
        [Test]
        public void Initialize_should_create_the_database_folder_if_it_does_not_exist()
        {
            var store = new DocumentStore() { DataLocation = TestSetup.DataDir, DatabaseName = "NonExistingDatabase" };
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, "NonExistingDatabase");
            using (var session = store.OpenSession())
            {

            }
            fileNameProvider.DatabaseDirectory.Exists.Should().BeTrue();
        }

        [TearDown]
        public void TearDown()
        {
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, "NonExistingDatabase");
            if (fileNameProvider.DatabaseDirectory.Exists)
                fileNameProvider.DatabaseDirectory.Delete(true);
        }
    }


}