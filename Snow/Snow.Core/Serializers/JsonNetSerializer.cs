using Newtonsoft.Json;

namespace Snow.Core.Serializers
{
    public class JsonNetSerializer : IDocumentSerializer
    {
        public string Serialize<TDocument>(TDocument value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public TDocument Deserialize<TDocument>(string serialized)
        {
            return JsonConvert.DeserializeObject<TDocument>(serialized);
        }
    }
}