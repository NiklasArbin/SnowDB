using System;
using FluentAssertions;
using Moq;
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
            IDocumentFile tempFile = fileNameProvider.GetDocumentFile<TestDocument>(key);
            
            
            var currentTime = DateTime.Now;
            var mockDateTime = new Mock<IDateTimeNow>();
            mockDateTime.Setup(x => x.Now).Returns(currentTime);

            IDocumentFile documentFile1 = new DocumentFile(tempFile.FullName, mockDateTime.Object);
            IDocumentFile documentFile2 = new DocumentFile(tempFile.FullName, mockDateTime.Object);

            documentFile1.Lock();
            documentFile2.Invoking(x => x.Lock()).ShouldThrow<DocumentFileTimeoutException>();
        }
    }
}
