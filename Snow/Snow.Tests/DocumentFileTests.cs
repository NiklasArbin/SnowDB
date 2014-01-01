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



            IDocumentFile documentFile1 = new DocumentFile(tempFile.FullName, new FakeDateTime());
            IDocumentFile documentFile2 = new DocumentFile(tempFile.FullName, new FakeDateTime());

            documentFile1.Lock();
            documentFile2.Invoking(x => x.Lock()).ShouldThrow<DocumentFileTimeoutException>();
        }
    }

    class FakeDateTime : IDateTimeNow
    {
        private int _counter = 0;
        private DateTime _dateTime;

        public FakeDateTime()
        {
            _dateTime = DateTime.Now;
        }

        public DateTime Now
        {
            get
            {
                if (_counter > 1)
                    return _dateTime.AddSeconds(30);
                _counter++;
                return _dateTime;
            }
        }
    }
}
