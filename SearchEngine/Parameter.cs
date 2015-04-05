using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public static class Parameter
    {
        public static bool RESET = false; // true = build everything again from scratch

        public static int DOCS_LIMIT = Int32.MaxValue; // set to Int32.MaxValue to use all documents
        public static int TERMS_LIMIT = Int32.MaxValue; // maximum terms to be chosen each document
        public static int QUERY_LIMIT = Int32.MaxValue; // maximum number of words in a query to be process
        public static int TEST_SIZE = 5000; // size of test dataset
        public static string QUERY_ID = "OHSU41";
        public static int MAX_RESULTS = Int32.MaxValue;
        public static float K_RATIO = 2f / 3; // this value is used to take relevant documents from rank list

        static string executableLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string DataFile = Path.Combine(executableLocation, @"docs\ohsumed.87");
        public static string QueryFile = Path.Combine(executableLocation, @"queries\query.ohsu.1-63");
        public static string SolutionFile = Path.Combine(executableLocation, @"queries\qrels.ohsu.batch.87");
        public static string SpimiFile = @"output\_SPIMI_.txt";
        public static string TermFile = @"output\_TERMS_";
        public static string TermInfoFile = @"output\_TERMS_INFO_.txt";
        public static string VectorFile = @"output\_VECTOR_";
        public static string VectorPosFile = @"output\_VECTOR_POS_";
        public static string TermFrequencyFile = @"output\category_term_frequency\"; // frequency of terms in each category
    }
}
