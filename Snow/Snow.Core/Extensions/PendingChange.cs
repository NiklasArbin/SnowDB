namespace Snow.Core.Extensions
{

    internal interface IOperation
    {
        string Key { get; set; }
        void Execute();
    }
}
