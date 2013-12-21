using System.IO;
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
    }
}