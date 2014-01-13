using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

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

        public static void SafeDeleteDocument<TDocument>(string key) where TDocument : class
        {
            try
            {
                var di = new DirectoryInfo(String.Format("{0}\\{1}\\{2}", DataDir, DatabaseName, typeof (TDocument).FullName));
                var files = di.GetFiles(key + "*");
                foreach (var fileInfo in files)
                {
                    fileInfo.Delete();
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}