using System;
using System.IO;
using System.Threading;
using Snow.Core.Extensions;

namespace Snow.Core
{
    public interface IDocumentFile : IDisposable
    {
        void Lock();
        void Unlock();
        string Read();
        void Write(string value);
        bool Exists { get; }
        string FullName { get; }
        string Name { get; }
        void Delete();
    }

    public class DocumentFile : IDocumentFile
    {
        private FileStream _fileStream;
        private readonly FileInfo _fileInfo;
        private static readonly TimeSpan MaxWaitForFile = TimeSpan.FromSeconds(30);
        private bool IsLocked { get; set; }

        public DocumentFile(string fileName)
        {
            _fileInfo = new FileInfo(fileName);
        }

        private FileStream OpenFileOrWait(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var initalAccessTime = DateTime.Now;
            while ((DateTime.Now - initalAccessTime) < MaxWaitForFile)
            {
                try
                {
                    return _fileInfo.Open(fileMode, fileAccess, fileShare);
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }
            var t = DateTime.Now - initalAccessTime;
            throw new DocumentFileTimeoutException("Timeout occured when trying to access the file {0}".FormatWith(_fileInfo.FullName));
        }

        public void Lock()
        {
            if (IsLocked)
                throw new InvalidOperationException("The document is already locked.");
            _fileStream = OpenFileOrWait(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
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

        public string Read()
        {
            String content;
            using (var sr = new StreamReader(GetStream()))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }

        public void Write(string value)
        {
            using (var stream = new StreamWriter(GetStream()))
            {
                stream.Write(value);
            }
        }

        private Stream GetStream()
        {
            if (_fileStream == null)
            {
                _fileStream = _fileStream = OpenFileOrWait(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            }
            return _fileStream;
        }

        public bool Exists
        {
            get { return (_fileInfo.Exists); }
        }

        public string FullName
        {
            get { return _fileInfo.FullName; }
        }

        public string Name
        {
            get { return _fileInfo.Name; }
        }

        public void Delete()
        {
            _fileInfo.Delete();
        }

        public void Dispose()
        {
            Unlock();
        }
    }

    public class DocumentFileTimeoutException : Exception
    {
        public DocumentFileTimeoutException(string message)
            : base(message)
        { }
    }
}
