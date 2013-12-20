namespace Snow.Core
{
    public interface IDocumentStore
    {
        IDocumentSession OpenSession();
        string DatabaseName { get; set; }
        string DataLocation { get; set; }
    }
}