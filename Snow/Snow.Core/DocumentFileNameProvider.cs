using System;
using System.CodeDom;
using System.IO;

namespace Snow.Core
{
    public interface IDocumentFileNameProvider
    {
        DirectoryInfo DatabaseDirectory { get; }
        DirectoryInfo DatabaseTransactionRootDirectory { get; }
        DirectoryInfo GetTransactionDirectory(Guid resourceManagerGuid);
        IDocumentFile GetDocumentFile<TDocument>(string key) where TDocument : class;
        IDocumentFile GetDocumentTransactionBackupFile<TDocument>(string key, Guid resourceManagerGuid) where TDocument : class;

    }

    public class DocumentFileNameProvider : IDocumentFileNameProvider
    {
        private readonly DirectoryInfo _databaseDirectory;
        private readonly DirectoryInfo _transactionRootDirectory;
        private const string FileExtension = "json";
        private const string TransactionDirectoryName = "trx";
        private static readonly IDateTimeNow DateTimeNow = new DateTimeNow();

        public DocumentFileNameProvider(string dataLocation, string databaseName)
        {
            _databaseDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName);
            _transactionRootDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName + "\\" +TransactionDirectoryName);
        }

        public DirectoryInfo DatabaseDirectory { get { return _databaseDirectory; } }
        public DirectoryInfo DatabaseTransactionRootDirectory { get { return _transactionRootDirectory; } }

        public DirectoryInfo GetTransactionDirectory(Guid resourceManagerGuid)
        {
            return new DirectoryInfo(_transactionRootDirectory.FullName + "\\" + resourceManagerGuid);
        }


        public IDocumentFile GetDocumentFile<TDocument>(string key) where TDocument : class
        {
            return new DocumentFile(_databaseDirectory.FullName + "\\" + GetFileName(typeof (TDocument), key), DateTimeNow);
        }

        public IDocumentFile GetDocumentTransactionBackupFile<TDocument>(string key, Guid resourceManagerGuid) where TDocument : class
        {
            return new DocumentFile(GetTransactionDirectory(resourceManagerGuid).FullName + "\\" + GetFileName(typeof (TDocument), key), DateTimeNow);
        }

        private static string GetFileName(Type type, string key)
        {
            return String.Format("{0}.{1}.{2}", type.Name, key, FileExtension);
        }
    }
}