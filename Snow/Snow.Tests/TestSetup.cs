using System;
using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    public class TestSetup
    {
        public const string DataDir = @"c:\temp";
        public const string DatabaseName = "TestDB";

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
        }

        public static void SafeDeleteDocument<TDocument>(string key)
        {
            var documentFileNameProvider = new DocumentFileNameProvider(TestSetup.DataDir, TestSetup.DatabaseName);
            var file = new DocumentFile<TDocument>(key, new DateTimeNow(), documentFileNameProvider, DateTime.Now);
            file.Delete();
        }
    }
}