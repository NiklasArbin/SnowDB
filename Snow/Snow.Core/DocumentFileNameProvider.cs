using System;
using System.IO;

namespace Snow.Core
{
    public interface IDocumentFileNameProvider
    {
        DirectoryInfo DatabaseDirectory { get; }
        DirectoryInfo DatabaseTransactionRootDirectory { get; }
        //DirectoryInfo GetTransactionDirectory<TDocument>(Guid resourceManagerGuid) where TDocument : class;
        DirectoryInfo GetLuceneRootDirectory();
        DirectoryInfo GetLuceneSessionDirectory(Guid sessionId);
        DirectoryInfo GetLuceneDirectory();
        IDocumentFile GetDocumentFile<TDocument>(string key) where TDocument : class;
        //IDocumentFile GetDocumentTransactionBackupFile<TDocument>(string key, Guid resourceManagerGuid) where TDocument : class;

    }

    public class DocumentFileNameProvider : IDocumentFileNameProvider
    {
        private readonly DirectoryInfo _databaseDirectory;
        private readonly DirectoryInfo _transactionRootDirectory;
        private const string FileExtension = "json";
        private const string TransactionDirectoryName = "trx";
        private const string LuceneDirectoryName = "Lucene";
        private static readonly IDateTimeNow DateTimeNow = new DateTimeNow();

        public DocumentFileNameProvider(string dataLocation, string databaseName)
        {
            _databaseDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName);
            _transactionRootDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName + "\\" + TransactionDirectoryName);
        }

        public DirectoryInfo DatabaseDirectory { get { return _databaseDirectory; } }
        public DirectoryInfo DatabaseTransactionRootDirectory { get { return _transactionRootDirectory; } }

        //public DirectoryInfo GetTransactionDirectory<TDocument>(Guid resourceManagerGuid) where TDocument : class
        //{
        //    return new DirectoryInfo(GetDocumentDirectory(typeof(TDocument)) + "\\" + resourceManagerGuid);
        //}

        public DirectoryInfo GetLuceneRootDirectory()
        {
            return new DirectoryInfo(_databaseDirectory.FullName + "\\" + LuceneDirectoryName);
        }

        public DirectoryInfo GetLuceneSessionDirectory(Guid sessionId)
        {
            return new DirectoryInfo(_databaseDirectory.FullName + "\\" + LuceneDirectoryName + "\\" + sessionId);
        }

        public DirectoryInfo GetLuceneDirectory()
        {
            return new DirectoryInfo(_databaseDirectory.FullName + "\\" + LuceneDirectoryName + "\\main");
        }


        public IDocumentFile GetDocumentFile<TDocument>(string key) where TDocument : class
        {
            var documentTypeDirectory = GetDocumentDirectory(typeof(TDocument));
            if (!Directory.Exists(documentTypeDirectory))
            {
                Directory.CreateDirectory(documentTypeDirectory); 
            }

            return new DocumentFile(documentTypeDirectory + "\\" + GetFileName(key), DateTimeNow);
        }

        //public IDocumentFile GetDocumentTransactionBackupFile<TDocument>(string key, Guid resourceManagerGuid) where TDocument : class
        //{
        //    return new DocumentFile(GetTransactionDirectory<TDocument>(resourceManagerGuid).FullName + "\\" + GetFileName(key), DateTimeNow);
        //}

        private static string GetFileName(string key)
        {
            return String.Format("{0}.{1}", key, FileExtension);
        }

        private string GetDocumentDirectory(Type type)
        {
            return String.Format("{0}\\{1}", _databaseDirectory.FullName, type.FullName);
        }
    }
}