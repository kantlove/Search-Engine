using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class QueryGenerator
    {
        const int QUERY_COUNT = 5;
        const int MAX_QUERY_LENGTH = 10;
        static Random _rand = new Random();
        static ConsoleWriter cs_writer = new ConsoleWriter();

        /// <summary>
        /// Randomly generate queries and write to queries/queries.txt
        /// </summary>
        /// <param name="filePaths">documents path</param>
        public static void Generate(string []filePaths, HashSet<string> stopWords, Dictionary<string, PostingList> IF)
        {
            cs_writer.RefreshOrigin();

            PostingList[] result = new PostingList[QUERY_COUNT];
            for(int i = 0; i < QUERY_COUNT; ++i)
            {
                int doc = _rand.Next(0, filePaths.Length);
                int length = _rand.Next(1, MAX_QUERY_LENGTH);

                List<string> solution = new List<string>(); // list of relevant documents

                StreamReader reader = new StreamReader(filePaths[doc]);
                string text = reader.ReadToEnd();
                
                string[] words = PreProcess(text, stopWords); // remove stop words,...
                
                string query = "";
                for (int j = 0; j < length; ++j)
                {
                    // randomly choose a word
                    string w = words[_rand.Next(0, words.Length)];
                    if(string.IsNullOrEmpty(w))
                    {
                        j--;
                        continue;
                    }

                    // find which document contain this term
                    if (IF.ContainsKey(w))
                        solution = solution.Union(IF[w].docs).ToList();

                    query += w;
                    if (j + 1 < length)
                        query += " ";
                }

                result[i] = new PostingList(query, solution);

                // Write progress on console
                cs_writer.RapidWrite((i + 1) + "/" + QUERY_COUNT);
                reader.Close();
            }

            // Now save the queries to file
            // -------------------------------
            WriteToFile(result);
        }

        static string[] PreProcess(string doc, HashSet<string> stopWords)
        {
            // Remove all non-word character (ex: , . ! ...)
            // ----------------------------------------------
            string text = doc.preProcess();

            // Read and extract unique words
            // ------------------------------
            string[] words = text.toWords(stopWords);

            return words;
        }

        /// <summary>
        /// Write the query and its solution to file
        /// </summary>
        static void WriteToFile(PostingList []queries)
        {
            Directory.CreateDirectory(@"queries/");

            // Clear the old file
            DirectoryInfo directory = new DirectoryInfo(@"queries\");
            directory.Empty();

            string query_dir = @"queries\queries.txt";
            StreamWriter writer = new StreamWriter(query_dir, true, new UTF8Encoding(false, true), 0x10000);
            
            for(int i = 0; i < queries.Length; ++i)
            {
                PostingList q = queries[i];
                writer.Write(q.term + " |"); // use '|' as a separator
                for (int j = 0; j < q.docs.Count; ++j)
                    writer.Write(" " + q.docs[j]);
                writer.WriteLine();
            }
            writer.Close();
        }
    }
}
