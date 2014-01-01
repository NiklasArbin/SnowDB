namespace Snow.Core
{
    public interface IDocumentStore
    {
        IDocumentSession OpenSession();
        string DatabaseName { get; }
        string DataLocation { get; }
    }
}