using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snow.Core.Operation
{
    internal interface ISnowTransaction
    {
        void Prepare();
        void Commit();
        void Rollback();
    }
}
