using System;
using System.IO;

namespace Snow.Core.Transactions
{
    public interface ITransactionCounter : IDisposable
    {
        int Current();
        int Next();
    }

    internal class TransactionCounter : ITransactionCounter
    {
        private readonly FileStream _fileStream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly object _lock = new object();

        private static ITransactionCounter _instance;

        private TransactionCounter(string directory)
        {
            lock (_lock)
            {
                _fileStream = File.Open(String.Format("{0}\\{1}", directory, "snow.transaction.counter"), mode: FileMode.OpenOrCreate, access: FileAccess.ReadWrite, share: FileShare.None);
                _reader = new StreamReader(_fileStream);
                _writer = new StreamWriter(_fileStream);
            }
        }

        public static ITransactionCounter GetInstance(string directory)
        {
            if (_instance == null)
                _instance = new TransactionCounter(directory);
            return _instance;
        }

        public int Current()
        {
            lock (_lock)
            {
                _fileStream.Position = 0;
                var read = _reader.ReadLine();
                return int.Parse(read);
            }
        }

        public int Next()
        {
            lock (_lock)
            {
                _fileStream.Position = 0;
                var next = Current() + 1;
                _writer.WriteLine(next);
                return next;
            }
        }

        public void Dispose()
        {
            if (_fileStream != null)
                _fileStream.Close();

        }
    }
}
