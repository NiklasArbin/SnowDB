using System;
using System.IO;

namespace Snow.Core
{
    public interface IDocumentFile : IDisposable
    {
        void Lock();
        void Unlock();
        Stream GetStream();
        bool Exists { get; }
        string FullName { get; }
        string Name { get;  }
        void Delete();
    }

    public class DocumentFile : IDocumentFile
    {
        private readonly string _path;
        private FileStream _fileStream;
        private bool IsLocked { get; set; }

        public DocumentFile(string path)
        {
            _path = path;
        }

        public void Lock()
        {
            if (IsLocked)
                throw new InvalidOperationException("The document is already locked.");
            _fileStream = File.Open(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            IsLocked = true;
        }

        public void Unlock()
        {
            if (!IsLocked)
                throw new InvalidOperationException("The document is not locked.");
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
            IsLocked = false;
        }

        public Stream GetStream()
        {
            if (_fileStream == null)
            {
                _fileStream = _fileStream = File.Open(_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            }
            return _fileStream;
        }

        public bool Exists
        {
            get
            {
                return (_fileStream != null || File.Exists(_path));
            }
        }

        public string FullName
        {
            get
            {
                return new FileInfo(_path).FullName;
            }
        }

        public string Name
        {
            get
            {
                return new FileInfo(_path).Name;
            }
        }

        public void Delete()
        {
            File.Delete(_path);
        }

        public void Dispose()
        {
            Unlock();
        }
    }
}
