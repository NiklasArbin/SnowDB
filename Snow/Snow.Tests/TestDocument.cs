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

    public class TestDocument2
    {
        public string AString { get; set; }
    }
}
