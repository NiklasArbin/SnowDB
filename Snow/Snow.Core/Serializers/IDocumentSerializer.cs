namespace Snow.Core.Serializers
{
    public interface IDocumentSerializer
    {
        string Serialize<TDocument>(TDocument value);
        TDocument Deserialize<TDocument>(string serialized);
    }
}
