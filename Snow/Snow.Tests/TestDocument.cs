using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snow.Tests
{
    public class TestDocument
    {
        
        public string SomeString { get; set; }
        public int SomeInt { get; set; }
        public SomeSubClass SomeSubClassProperty { get; set; }

        public class SomeSubClass
        {
            public double SomeDouble { get; set; }
        }
    }
}
