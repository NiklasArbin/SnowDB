namespace Snow.Core
{
    internal interface IOperation
    {
        string Key { get; set; }
        void Execute();
    }
}
