using System;

namespace Snow.Core
{
    public interface IDocumentSession:IDisposable
    {
        TDocument Get<TDocument>(string key);
        void Store<TDocument>(TDocument document, string key);
        void SaveChanges();
    }
}