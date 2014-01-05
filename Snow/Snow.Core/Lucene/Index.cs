using System;
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
    internal interface ISnowIndex : IDisposable, ISnowTransaction
    {
        void Add<TDocument>(string key, string json);
        void Delete<TDocument>(string key);
    }

    internal class SnowIndex : ISnowIndex
    {
        private const string SnowDbKeyName = "SnowDBKey";

        private readonly IndexWriter _writer;
        private readonly FSDirectory _fsDirectory;
        private readonly Analyzer _analyser = new StandardAnalyzer(Version.LUCENE_30);

        private SnowIndex(IDocumentFileNameProvider fileNameProvider)
        {
            _fsDirectory = FSDirectory.Open(fileNameProvider.GetLuceneDirectory().FullName);
            _writer = new IndexWriter(_fsDirectory, _analyser, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public static SnowIndex Open(IDocumentFileNameProvider fileNameProvider)
        {
            return new SnowIndex(fileNameProvider);
        }

        public void Add<TDocument>(string key, string json)
        {
            var doc = new Document();
            doc.Add(new Field(SnowDbKeyName, GetKey<TDocument>(key), Field.Store.YES, Field.Index.ANALYZED));
            var fields = LuceneSerializer.Serialize(json);
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
        }

        public void Rollback()
        {
            _writer.Rollback();
        }

        public void Dispose()
        {
            _writer.Dispose();
            _fsDirectory.Dispose();
        }

        private static string GetKey<TDocument>(string key)
        {
            return "{0}.{1}".FormatWith(typeof(TDocument).Name, key);
        }
    }
}
