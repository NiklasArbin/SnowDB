namespace Snow.Core.Serializers
{
    public class FastJsonSerializer : IDocumentSerializer
    {
        public string Serialize<TDocument>(TDocument value)
        {
            return fastJSON.JSON.Instance.ToJSON(value);
        }

        public TDocument Deserialize<TDocument>(string serialized)
        {
            return fastJSON.JSON.Instance.ToObject<TDocument>(serialized);
        }
    }
}