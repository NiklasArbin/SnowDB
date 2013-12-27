using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Snow.Tests
{
    [SetUpFixture]
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

        public static void SafeDeleteDocument(string path)
        {
            if(File.Exists(path))
                File.Delete(path);
        }
    }
}