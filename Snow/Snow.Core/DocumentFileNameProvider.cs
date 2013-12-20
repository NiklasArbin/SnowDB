using System.IO;

namespace Snow.Core
{
    public interface IDocumentFileNameProvider
    {
        DirectoryInfo DatabaseDirectory { get; }
        FileInfo GetDocumentFile(string key);
    }

    public class DocumentFileNameProvider : IDocumentFileNameProvider
    {
        private readonly DirectoryInfo _databaseDirectory;
        private const string FileExtension = ".json";

        public DocumentFileNameProvider(string dataLocation, string databaseName)
        {
            _databaseDirectory = new DirectoryInfo(dataLocation + "\\" + databaseName);
        }

        public DirectoryInfo DatabaseDirectory { get { return _databaseDirectory; } }

        public FileInfo GetDocumentFile(string key)
        {
            return new FileInfo(_databaseDirectory.FullName + "\\" + key + FileExtension);
        }
    }
}