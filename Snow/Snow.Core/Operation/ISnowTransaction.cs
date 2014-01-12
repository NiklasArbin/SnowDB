namespace Snow.Core.Operation
{
    internal interface ISnowTransaction
    {
        void Prepare();
        void Commit();
        void Rollback();
    }
}
