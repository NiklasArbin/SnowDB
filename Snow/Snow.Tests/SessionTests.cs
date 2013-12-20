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
            var store = new DocumentStore{DataLocation = @"c:\temp\", DatabaseName = "TestDB"};
            var key = Guid.Parse("98CD87C0-1631-4CB9-BFC6-306CCBD15C8F");
            using (var session = store.OpenSession())
            {
                session.Invoking(x => x.Get<TestDocument>(key.ToString())).ShouldThrow<DocumentNotFoundException>();
            }
        }
    }
}