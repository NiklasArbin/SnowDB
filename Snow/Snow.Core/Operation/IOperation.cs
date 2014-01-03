namespace Snow.Core.Operation
{
    internal interface IOperation
    {
        string Key { get; set; }
        void Execute();
    }
}
