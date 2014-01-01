using FluentAssertions;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    class DocumentFileTests
    {
        [Test]
        public void Lock_should_lock_the_file()
        {
            var fileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var key = "7966FBDA-D8A6-4F6D-AFD9-DE00968ED6D4";
            IDocumentFile documentFile1 = fileNameProvider.GetDocumentFile<TestDocument>(key);
            IDocumentFile documentFile2 = fileNameProvider.GetDocumentFile<TestDocument>(key);
            documentFile1.Lock();
            documentFile2.Invoking(x => x.Lock()).ShouldThrow<DocumentFileTimeoutException>();
        }
    }
}
