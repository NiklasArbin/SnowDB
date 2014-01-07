using System.IO;
using System.Threading;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace Snow.Core.Lucene
{
    public static class IndexSyncronizer
    {
        private static readonly Mutex Mutex = new Mutex(false, "SnowDBMutex");
        private static readonly Analyzer Analyser = new StandardAnalyzer(Version.LUCENE_29);

        public static void Syncronize(DirectoryInfo sessionDirectory, IDocumentFileNameProvider fileNameProvider)
        {
            using (Mutex)
            {
                using (var fsDirectory = FSDirectory.Open(fileNameProvider.GetLuceneDirectory()))
                {
                    using (var writer = new IndexWriter(fsDirectory, Analyser, IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        using (var readerDirectory = FSDirectory.Open(sessionDirectory))
                        {
                            using (var reader = IndexReader.Open(readerDirectory, true))
                            {
                                writer.AddIndexes(reader);
                            }
                        }
                    }
                }
            }
        }
    }
}
