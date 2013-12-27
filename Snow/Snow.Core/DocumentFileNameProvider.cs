using System;
using System.IO;

namespace Snow.Core
{
    public interface IDocumentFileNameProvider
    {
        DirectoryInfo DatabaseDirectory { get; }
        DirectoryInfo DatabaseTransactionRootDirectory { get; }
        DirectoryInfo GetTransactionDirectory(Guid resourceManagerGuid);
        FileInfo GetDocumentFile(string key);
        FileInfo GetDocumentTransactionBackupFile(string key, Guid resourceManagerGuid);

    }

    public class DocumentFileNameProvider : IDocumentFileNameProvider
    {
        private readonly DirectoryInfo _databaseDirectory;
        private readonly DirectoryInfo _transactionRootDirectory;
        private const string FileExtension = ".json";

        public DocumentFileNameProvider(string dataLocation, string databaseName)
        {
            _databaseDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName);
            _transactionRootDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName + "\\trx");
        }

        public DirectoryInfo DatabaseDirectory { get { return _databaseDirectory; } }
        public DirectoryInfo DatabaseTransactionRootDirectory { get { return _transactionRootDirectory; } }

        public DirectoryInfo GetTransactionDirectory(Guid resourceManagerGuid)
        {
            return new DirectoryInfo(_transactionRootDirectory.FullName + "\\" + resourceManagerGuid);
        }

        public FileInfo GetDocumentFile(string key)
        {
            return new FileInfo(_databaseDirectory.FullName + "\\" + GetFileName(key));
        }

        public FileInfo GetDocumentTransactionBackupFile(string key, Guid resourceManagerGuid)
        {
            return new FileInfo(GetTransactionDirectory(resourceManagerGuid).FullName + "\\" + GetFileName(key));
        }

        private static string GetFileName(string key)
        {
            return key + FileExtension;
        }
    }
}