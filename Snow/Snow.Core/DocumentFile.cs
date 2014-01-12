using System;
using System.IO;
using System.Linq;
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
        private readonly IDateTimeNow _dateTimeNow;
        private FileStream _fileStream;
        private readonly FileInfo _fileInfo;
        private static readonly TimeSpan MaxWaitForFile = TimeSpan.FromSeconds(30);
        private bool IsLocked { get; set; }

        public DocumentFile(string fileName, IDateTimeNow dateTimeNow)
        {
            _dateTimeNow = dateTimeNow;
            _fileInfo = new FileInfo(fileName);
        }

        private FileStream OpenFileOrWait(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var initalAccessTime = _dateTimeNow.Now;
            while ((_dateTimeNow.Now - initalAccessTime) < MaxWaitForFile)
            {
                try
                {
                    return _fileInfo.Open(fileMode, fileAccess, fileShare);
                }
                catch (IOException e)
                {
                    if (e.Message.StartsWith("The process cannot access the file"))
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var t = _dateTimeNow.Now - initalAccessTime;
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
            return _fileStream ?? (_fileStream = _fileStream = OpenFileOrWait(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read));
        }

        private string GetValidFileVersion<TDocument>(string key, DateTime sessionStamp)
        {
            var searchPattern = String.Format("{0}.*.{1}", key, "json");
            var fileInfo =
                _fileInfo.Directory.GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(x => x.LastWriteTime <= sessionStamp)
                    .OrderByDescending(x => x.LastWriteTime)
                    .FirstOrDefault();
            return null;
        }

        public bool Exists
        {
            get
            {
                _fileInfo.Refresh();
                return _fileInfo.Exists;
            }
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
            var initalAccessTime = _dateTimeNow.Now;
            while ((_dateTimeNow.Now - initalAccessTime) < MaxWaitForFile)
            {
                try
                {
                    Unlock();
                    _fileInfo.Delete();
                    return;
                }
                catch (IOException e)
                {
                    if (e.Message.StartsWith("The process cannot access the file"))
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        throw;
                    }

                }
            }
            throw new DocumentFileTimeoutException("Timeout occured when trying to access the file {0}".FormatWith(_fileInfo.FullName));
        }

        public void Dispose()
        {
            Unlock();
        }
    }
}
