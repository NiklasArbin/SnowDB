using System;
using System.Security.Cryptography.X509Certificates;

namespace Snow.Core
{
    public interface IDocumentSession : IDisposable
    {
        Guid SessionGuid { get; }
        TDocument Get<TDocument>(string key) where TDocument : class;
        bool TryGet<TDocument>(string key, out TDocument document) where TDocument : class;
        void Save<TDocument>(TDocument document, string key) where TDocument : class;
        void Delete<TDocument>(string key) where TDocument : class;
    }
}