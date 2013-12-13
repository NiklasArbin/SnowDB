using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snow.Core
{
    public class DocumentDocumentStore : IDocumentStore
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }

    public interface IDocumentStore
    {
        void Initialize();
    }

    public class DocumentSession : IDocumentSession
    {
        public void Store(object document)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }

    public interface IDocumentSession
    {
        void Store(object document);
        void SaveChanges();
    }
}
