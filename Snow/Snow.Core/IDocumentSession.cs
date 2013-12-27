using System;
using System.Transactions;

namespace Snow.Core
{
    public interface IDocumentSession: IDisposable
    {
        TDocument Get<TDocument>(string key);
        bool TryGet<TDocument>(string key, out TDocument document);
        void Save<TDocument>(TDocument document, string key);
        void Delete(string key);
        void SaveChanges();
    }
}