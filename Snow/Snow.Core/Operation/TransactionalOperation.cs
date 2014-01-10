using System;
using System.IO;

namespace Snow.Core.Operation
{
    //internal abstract class TransactionalOperation<TDocument> : IOperation
    //    where TDocument : class
    //{
    //    protected IDocumentFileNameProvider FileNameProvider;

    //    private IDocumentFile _docFile;
    //    protected IDocumentFile DocumentFile
    //    {
    //        get { return _docFile ?? (_docFile = FileNameProvider.GetDocumentFile<TDocument>(Key)); }
    //    }

    
    //    public Guid SessionGuid { get; private set; }
    //    public string Key { get; set; }

    //    protected TransactionalOperation(Guid sessionGuid)
    //    {
    //        SessionGuid = sessionGuid;
    //    }

    //    protected abstract void Commit(IDocumentFile documentFile);

        

    //    public virtual void Prepare()
    //    {
    //        if (DocumentFile.Exists)
    //        {
    //            File.Copy(FileNameProvider.GetDocumentFile<TDocument>(Key).FullName, FileNameProvider.GetDocumentTransactionBackupFile<TDocument>(Key, SessionGuid).FullName);
    //        }
    //    }

    //    public abstract void Commit();
       

    //    void IOperation.Rollback()
    //    {
    //        Rollback();
    //    }
    //    protected abstract void Rollback();
      
    //}
}