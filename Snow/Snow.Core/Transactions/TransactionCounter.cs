using System;
using System.IO;

namespace Snow.Core.Transactions
{
    internal interface ITransactionCounter : IDisposable
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

        public TransactionCounter(string directory)
        {
            _fileStream = File.Open(String.Format("{0}\\{1}", directory, "snow.transaction.counter"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            _reader = new StreamReader(_fileStream);
            _writer = new StreamWriter(_fileStream);
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
            throw new NotImplementedException();
        }
    }
}
