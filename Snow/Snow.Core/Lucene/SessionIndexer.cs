using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Snow.Core.Extensions;
using Snow.Core.Operation;
using Version = Lucene.Net.Util.Version;

namespace Snow.Core.Lucene
{
    internal interface ISessionIndexer : IDisposable, ISnowTransaction
    {
        void Add<TDocument>(string key, string json);
        void Delete<TDocument>(string key);
        ISessionIndexer Open();
    }

    internal class SessionIndexer : ISessionIndexer
    {
        private readonly IDocumentFileNameProvider _fileNameProvider;
        private const string SnowDbKeyName = "SnowDBKey";

        private IndexWriter _writer;
        private FSDirectory _fsDirectory;
        private readonly DirectoryInfo _sessionDirectory;

        private readonly Analyzer _analyser = new StandardAnalyzer(Version.LUCENE_29);

        public SessionIndexer(Guid sessionId, IDocumentFileNameProvider fileNameProvider)
        {
            _fileNameProvider = fileNameProvider;
            _sessionDirectory = fileNameProvider.GetLuceneSessionDirectory(sessionId);
        }

        public ISessionIndexer Open()
        {
            _sessionDirectory.Create();
            _fsDirectory = FSDirectory.Open(_sessionDirectory);
            _writer = new IndexWriter(_fsDirectory, _analyser, IndexWriter.MaxFieldLength.UNLIMITED);
            return this;
        }

        public void Add<TDocument>(string key, string json)
        {
            var doc = new Document();
            doc.Add(new Field(SnowDbKeyName, GetKey<TDocument>(key), Field.Store.YES, Field.Index.ANALYZED));
            var fields = JsonToLuceneConverter.Serialize(json);
            foreach (var field in fields)
            {
                doc.Add(field);
            }
            _writer.AddDocument(doc);
        }

        public void Delete<TDocument>(string key)
        {
            var term = new Term(SnowDbKeyName, GetKey<TDocument>(key));
            _writer.DeleteDocuments(term);
        }

        public void Prepare()
        {
            _writer.PrepareCommit();
        }

        public void Commit()
        {
            _writer.Commit();
            IndexSyncronizer.Syncronize(_sessionDirectory, _fileNameProvider);
        }

        public void Rollback()
        {
            //_writer.Rollback();
        }

        public void Dispose()
        {
            if (_writer != null)
                _writer.Dispose();
            if (_fsDirectory != null)
                _fsDirectory.Dispose();
            if (_sessionDirectory.Exists)
                _sessionDirectory.Delete(true);
        }

        private static string GetKey<TDocument>(string key)
        {
            return "{0}.{1}".FormatWith(typeof(TDocument).Name, key);
        }
    }
}
