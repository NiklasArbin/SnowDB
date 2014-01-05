using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Snow.Core.Lucene;
using Snow.Core.Serializers;

namespace Snow.Tests
{
    class FlattenJsonTests
    {








//        {"menu": {
//          "id": "file",
//          "value": "File",
//          "popup": {
//          "menuitem": [
//              {"value": "New", "onclick": "CreateNewDoc()"},
//              {"value": "Open", "onclick": "OpenDoc()"},
//              {"value": "Close", "onclick": "CloseDoc()"}
//                      ]
//                  }
//                  }
//        }
        private string testJson = "{\"menu\": {\"id\": \"file\",\"value\": \"File\",\"popup\": {\"menuitem\": [{\"value\": \"New\", \"onclick\": \"CreateNewDoc()\"},{\"value\": \"Open\", \"onclick\": \"OpenDoc()\"},{\"value\": \"Close\", \"onclick\": \"CloseDoc()\"}]}}}";

        [Test]
        public void Test()
        {
            var x = JsonToLuceneConverter.Serialize(testJson);

           var y= fastJSON.JSON.Instance.ToObject<Dictionary<string, object>>(testJson);
        }
    }
}
