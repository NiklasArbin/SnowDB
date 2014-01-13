using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Snow.Core.Extensions;

namespace Snow.Core
{
    public interface IDocumentFile<TDocument> : IDisposable
    {
        void Lock();
        void Unlock();
        string Read();
        void Write(string value);
        bool Exists { get; }
        void Delete();
    }

    public class DocumentFile<TDocument> : IDocumentFile<TDocument>
    {
        private readonly string _key;
        private readonly IDateTimeNow _dateTimeNow;
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private readonly DateTime _sessionStamp;
        private FileStream _fileStream;
        private readonly DirectoryInfo _documentDirectory;
        private static readonly TimeSpan MaxWaitForFile = TimeSpan.FromSeconds(30);
        private bool IsLocked { get; set; }

        public DocumentFile(string key, IDateTimeNow dateTimeNow, IDocumentFileNameProvider fileNameProvider, DateTime sessionStamp)
        {
            _key = key;
            _dateTimeNow = dateTimeNow;
            _fileNameProvider = fileNameProvider;
            _sessionStamp = sessionStamp;
            _documentDirectory = new DirectoryInfo(String.Format("{0}\\{1}", fileNameProvider.DatabaseDirectory, typeof(TDocument).FullName));
        }

        private FileInfo GetFileInfoForReadAccess(string key)
        {
            var searchPattern = String.Format("{0}.*", key);
            var fileInfo =
                _documentDirectory.GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(x => x.LastWriteTime <= _sessionStamp)
                    .OrderByDescending(x => x.LastWriteTime)
                    .FirstOrDefault();
            if (fileInfo != null)
            {
                return fileInfo;
            }

            return new FileInfo(String.Format("{0}\\{1}.0.{2}", _documentDirectory.FullName, key, "json"));
        }

        private FileStream GetFileStreamForWriteAccess(string key)
        {
            var searchPattern = String.Format("{0}.*.{1}", key, "json");
            var fileInfo =
                _documentDirectory.GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
                    .OrderByDescending(x => x.LastWriteTime)
                    .FirstOrDefault();
            if (fileInfo != null)
            {
                var nextVersion = GetVersionFromFileName(fileInfo.Name) + 1;
                return OpenFileForWriteAccess(_key, nextVersion, "json");
            }

            return OpenFileForWriteAccess(_key, 0, "json");
        }

        private FileStream GetFileInfoForDelete(string key)
        {
            var searchPattern = String.Format("{0}.*", key);
            _documentDirectory.Refresh();
            var fileInfo =
                _documentDirectory.GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
                    .OrderByDescending(x => x.LastWriteTime)
                    .FirstOrDefault();
            if (fileInfo != null)
            {
                var nextVersion = GetVersionFromFileName(fileInfo.Name) + 1;
                return OpenFileForWriteAccess(_key, nextVersion, "deleted");
            }

            return OpenFileForWriteAccess(_key, 0, "deleted");
        }

        private int GetVersionFromFileName(string fileName)
        {
            //TODO: Regex?
            var firstIndex = fileName.IndexOf('.');
            var lastIndex = fileName.IndexOf('.', firstIndex + 1);
            var substring = fileName.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
            return int.Parse(substring);
        }



        private FileStream OpenFileStreamOrWait(FileInfo file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var initalAccessTime = _dateTimeNow.Now;
            while ((_dateTimeNow.Now - initalAccessTime) < MaxWaitForFile)
            {
                try
                {
                    return file.Open(fileMode, fileAccess, fileShare);
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
            throw new DocumentFileTimeoutException("Timeout occured when trying to access the file {0}".FormatWith(file.FullName));
        }

        private FileStream OpenFileForReadAccess(FileInfo fileInfo)
        {
            return OpenFileStreamOrWait(fileInfo, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private FileStream OpenFileForWriteAccess(string key, int version, string extension)
        {
            var file = new FileInfo(String.Format("{0}\\{1}.{2}.{3}", _documentDirectory.FullName, key, version, extension));

            try
            {
                return file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
            }
            catch (IOException e)
            {
                if (!e.Message.EndsWith("exists."))
                {
                    throw;
                }
                return OpenFileForWriteAccess(key, ++version, extension);
            }
        }

        public void Lock()
        {
            if (IsLocked)
                throw new InvalidOperationException("The document is already locked.");
            //_fileStream = OpenFileForReadAccess();
            //IsLocked = true;
        }

        public void Unlock()
        {
            //if (_fileStream != null)
            //{
            //    _fileStream.Dispose();
            //    _fileStream = null;
            //}
            //IsLocked = false;
        }

        public string Read()
        {
            String content;
            var fileToRead = GetFileInfoForReadAccess(_key);
            using (var sr = new StreamReader(OpenFileForReadAccess(fileToRead)))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }

        public void Write(string value)
        {
            using (var stream = new StreamWriter(GetFileStreamForWriteAccess(_key)))
            {
                stream.Write(value);
            }
        }


        public bool Exists
        {
            get
            {
                var fileInfo = GetFileInfoForReadAccess(_key);
                if (fileInfo.Exists)
                {
                    return fileInfo.Extension != ".deleted";
                }
                return false;
            }
        }

        public void Delete()
        {
            using (var stream = GetFileInfoForDelete(_key))
            {
                //Just touch the file.
            }
        }


        public void Dispose()
        {
            Unlock();
        }
    }
}
