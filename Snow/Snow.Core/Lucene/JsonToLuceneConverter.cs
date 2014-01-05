using System.Collections.Generic;
using Lucene.Net.Documents;
using Snow.Core.Extensions;

namespace Snow.Core.Lucene
{
    public class JsonToLuceneConverter
    {
        public static List<Field> Serialize(string input)
        {
            var luceneFields = new List<Field>();
            var dictionary = fastJSON.JSON.Instance.ToObject<Dictionary<string, object>>(input);
            FlattenDictionary(dictionary, string.Empty, luceneFields);
            return luceneFields;
        }

        private static void FlattenDictionary(Dictionary<string, object> dictionary, string parentName, ICollection<Field> luceneFields )
        {
            foreach (var kvp in dictionary)
            {
                if (kvp.Value is string)
                {
                    luceneFields.Add(GetField("{0}.{1}".FormatWith(parentName, kvp.Key), (string)kvp.Value));
                }
                else if (kvp.Value is Dictionary<string, object>)
                {
                    FlattenDictionary((Dictionary<string, object>)kvp.Value, "{0}.{1}".FormatWith(parentName, kvp.Key), luceneFields);
                }
                else if (kvp.Value is List<object>)
                {
                    var num = 0;
                    foreach (var o in (List<object>)kvp.Value)
                    {
                        FlattenDictionary((Dictionary<string, object>)o, "{0}.{1}[{2}]".FormatWith(parentName, kvp.Key, num++), luceneFields);
                    }
                }
            }
        }

        private static Field GetField(string key, string value)
        {
            return new Field(key, value, Field.Store.NO, Field.Index.ANALYZED);
        }
    }
}
