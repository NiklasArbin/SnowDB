using System;

namespace Snow.Core
{
    public interface IDocumentStore : IDisposable
    {
        IDocumentSession OpenSession();
        string DatabaseName { get; }
        string DataLocation { get; }
    }
}