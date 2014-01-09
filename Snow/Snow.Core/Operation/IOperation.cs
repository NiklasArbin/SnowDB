using System;

namespace Snow.Core.Operation
{
    public interface IOperation
    {
        string Key { get; set; }
        Guid SessionGuid { get; }
        void Prepare();
        void Commit();
        void Rollback();
    }
}
