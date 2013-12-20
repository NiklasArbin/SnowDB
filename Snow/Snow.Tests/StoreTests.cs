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
        private const string DataDir = @"c:\temp";
       
        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
        }

        [Test]
        public void OpenSession_should_return_a_new_session()
        {
            var store = new DocumentStore{DataLocation = DataDir, DatabaseName = "TestDB"};
            var session = store.OpenSession();

            session.Should().NotBeNull();
            session.Should().BeAssignableTo<IDocumentSession>();
        }

        [Test]
        public void OpenSession_should_throw_exception_if_datadirectory_is_not_specified()
        {
            var store = new DocumentStore { DatabaseName = "TestDB" };
            store.Invoking(x => x.OpenSession()).ShouldThrow<ArgumentException>();
        }

        [Test]
        public void OpenSession_should_throw_exception_if_databaseName_is_not_specified()
        {
            var store = new DocumentStore() { DataLocation = DataDir };
            store.Invoking(x => x.OpenSession()).ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Initialize_should_throw_exception_when_datadirectory_does_not_exist()
        {
            const string invalidPath = @"c:\invalid_path";
            var store = new DocumentStore { DataLocation = invalidPath, DatabaseName = "TestDB" };
            store.Invoking(x => x.OpenSession()).ShouldThrow<DirectoryNotFoundException>();
        }
    }


}