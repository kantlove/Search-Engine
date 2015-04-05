using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class QueryManager
    {
        public static int MAX_RESULTS = Parameter.MAX_RESULTS; // maximum number of results to be returned
        public static Dictionary<string, string> Queries; // set of queries
        public static Dictionary<string, PostingList> Solutions; // set of queries
        public static Dictionary<string, Tuple<QueryVector, HashSet<string>>> FullQuery; // A complete query, contains Id, text, solution
        public static Evaluator evaluator; // evaluator of this system
        public static HashSet<String> stopWords;
        ConsoleWriter cs_writer;

        public static void Parse(string querySource, string solutionSource, string output, Dictionary<string, PostingList> inverted_file, string query_id)
        {
            ParseQuery(querySource, query_id, inverted_file);
            ParseSolution(solutionSource, query_id);

            // Combine query and solution into 1
            FullQuery = new Dictionary<string, Tuple<QueryVector, HashSet<string>>>();

            foreach (var entry in Queries)
            {
                string real_query = entry.Value;
                QueryVector query = new QueryVector(real_query, inverted_file);
                HashSet<string> solution_set = new HashSet<string>(Solutions[entry.Key].docs);
                FullQuery.Add(entry.Key, new Tuple<QueryVector, HashSet<string>>(query, solution_set));
            }

            Queries.Clear();
            Solutions.Clear();
        }

        public static long ParseQuery(string source, string query_id, Dictionary<string, PostingList> IF)
        {
            Queries = new Dictionary<string, string>();

            StreamReader fin = new StreamReader(source);

            long entries = 0;
            string line = "";
            string text = "";
            string queryId = "";

            while (fin.Peek() != -1)
            {
                line = fin.ReadLine(); // <top>
                if (line == "<top>")
                {
                    line = fin.ReadLine(); // <num>
                    queryId = line.Split(' ')[2];

                    if (queryId.Equals(query_id))
                    {
                        fin.ReadLine(); // <title>
                        fin.ReadLine(); // <desc>
                        text = fin.ReadLine(); // real desc
                        text = text.preProcess(); // remove non-word character
                        //text = GetFeedback(text, IF);
                        Queries[queryId] = text; // save it
                    }

                }
            }

            fin.Close();

            return entries;
        }

        public static void ParseSolution(string source, string query_id)
        {
            Solutions = new Dictionary<string, PostingList>();

            StreamReader fin = new StreamReader(source);

            string line = "";
            string queryId = "";
            string rel_doc = "0";
            while (fin.Peek() != -1)
            {
                line = fin.ReadLine(); // ID   DOCID
                string[] parts = line.Split('\t');
                queryId = parts[0];
                if (queryId.Equals(query_id))
                {
                    rel_doc = parts[1];

                    if (!Solutions.ContainsKey(queryId))
                        Solutions[queryId] = new PostingList(queryId, new List<string>() { rel_doc });
                    else
                        Solutions[queryId].docs.Add(rel_doc);
                }
            }

            fin.Close();
        }

        public static void Query(VectorSpace vSpace, Dictionary<string, PostingList> IF, string queryId)
        {
            ConsoleWriter cs_writer = new ConsoleWriter(Console.CursorLeft, Console.CursorTop);
            Console.WriteLine("\n\t Normal query\n");
            ExecuteQueries(vSpace, IF, queryId);
        }

        //static HashSet<int> relevantDocs = new HashSet<int>();
        //static HashSet<int> unrelevantDocs = new HashSet<int>();
        public static void ExecuteQueries(VectorSpace vSpace, Dictionary<string, PostingList> IF, string queryId, string extra_name = "", int feedbackType = 0)
        {
            // the first k_value documents are considered relevant
            //int k_value = (int)(MAX_RESULTS * Parameter.K_RATIO);

            QueryVector query = FullQuery[queryId].Item1;
            HashSet<string> solution = FullQuery[queryId].Item2;

            // add Pseudo Feedback
            //if(feedbackType > 0)
            //    query.QueryText = GetFeedback(query.QueryText, IF, feedbackType);

            int m = vSpace.Total_Docs;
            List<Pair<string, float>> rankList = new List<Pair<string, float>>(); // docId and angle

            BinaryReader bin = new BinaryReader(new FileStream(Parameter.VectorFile, FileMode.Open, FileAccess.Read, FileShare.Read));

            //int k = 0;
            float eps = 0.000001f; // a very small number
            string[] words = GetBestWordsOfQuery(query.QueryText);

            for (int i = 0; i < words.Length; ++i)
            {
                string w = words[i].ToLower();
                if (IF.ContainsKey(w)) // find this word
                {
                    List<string> list = IF[words[i].ToLower()].docs; // list of docs contain this word
                    List<DocVector> candidate_docs = new List<DocVector>();
                    // Get all the docs containing this term from file
                    foreach (string doc_id in list)
                    {
                        DocVector dv = vSpace.GetVector(doc_id, bin);
                        candidate_docs.Add(dv);
                    }

                    // Calculate angles between docs and query
                    Parallel.ForEach(candidate_docs, dv => {
                        if (dv != null)
                        {
                            float angle = query.Angle(dv);
                            // Ignore 90 degree angle
                            if (Math.Abs(angle - 1.57079637) > eps)
                                rankList.Add(new Pair<string, float>(dv.DocID, angle));

                            // save this doc
                            //if (k++ < k_value && !relevantDocs.Contains(dv.DocID))
                            //    relevantDocs.Add(dv.DocID);
                            //else if (!unrelevantDocs.Contains(dv.DocID))
                            //    unrelevantDocs.Add(dv.DocID);
                        }
                    });
                }
            }
            bin.Close();

            // Sort by angle
            rankList.Sort();

            // Only take MAX_RESULTS item
            Pair<string, float>[] finalRankList = rankList.ToArray();

            // Write result to file
            string result_dir = @"output/result/query_" + queryId + ".txt";
            WriteResult(result_dir, finalRankList);

            // Evaluate this query
            evaluator.Evaluate(rankList.ToArray(), solution);
            evaluator.WriteResult(@"output/result/pre_recall_" + queryId + ".csv");

            // Draw graph
            // GraphDrawer.Draw(evaluator.Result, queryId + extra_name);
            GraphDrawer.Draw(evaluator.Result, feedbackType + 1);

            rankList.Clear();
        }

        static string[] GetBestWordsOfQuery(string query)
        {
            string[] words = query.Split(' ');
            Dictionary<string, int> frequency = new Dictionary<string, int>();
            for(int i =0; i < words.Length; ++i)
            {
                if (frequency.ContainsKey(words[i]))
                    ++frequency[words[i]];
                else
                    frequency.Add(words[i], 1);
            }

            return frequency.OrderBy(x => x.Value).Take(Parameter.QUERY_LIMIT).ToDictionary(pair => pair.Key, pair => pair.Value).Keys.ToArray();
        }

        static void WriteResult(string dir, Pair<string, float>[] result)
        {
            Directory.CreateDirectory(@"output/result/");
            StreamWriter writer = new StreamWriter(dir, true, new UTF8Encoding(false, true), 0x10000);
            for (int i = 0; i < result.Length; ++i)
                if (result[i] != null)
                    writer.WriteLine(result[i].A + " " + result[i].B);
            writer.Close();
        }

        #region PseudoFeedback

        /// <summary>
        /// Modify the query
        /// </summary>
        /// <param name="type">1: Global    2: Local</param>
        //static string GetFeedback(string query_text, Dictionary<string, PostingList> IF, int type = 1)
        //{
        //    string[] parts = query_text.Split(' ');
        //    Parallel.ForEach(parts, word =>
        //    {
        //        string synonym = "";
        //        if (type == 1)
        //            synonym = GlobalAnalysis(word, IF);
        //        else
        //            synonym = LocalAnalysis(word, IF);

        //        if (!query_text.Contains(' ' + synonym) && !query_text.Contains(synonym + ' ')) // already added
        //            query_text += ' ' + synonym;
        //    });

        //    return query_text;
        //}

        //// Pseudo feedback using Global Analysis
        //static string GlobalAnalysis(string word, Dictionary<string, PostingList> IF)
        //{
        //    if (!IF.ContainsKey(word.ToLower()))
        //        return "";
        //    List<int> docs1 = IF[word.ToLower()].docs;
        //    int max = 0; // max score
        //    string result = ""; // final result (aka chosen term)

        //    Parallel.ForEach(IF, entry =>
        //    {
        //        string other_word = entry.Key;
        //        if (!other_word.Equals(word) && !stopWords.Contains(other_word))
        //        {
        //            List<int> docs2 = entry.Value.docs;
        //            List<int> docs = docs1.Intersect(docs2).ToList(); // take intersect

        //            int c_ij = 0;
        //            foreach (var doc in docs)
        //            {
        //                DocVector dv = new DocVector(0) { DocID = doc }; // create a dummy vector just to get the frequency
        //                Dictionary<string, int> frequency = dv.LoadTermFrequency();
        //                if (!frequency.ContainsKey(word) || !frequency.ContainsKey(other_word)) // over careful...
        //                    continue;
        //                c_ij += frequency[word] * frequency[other_word];
        //                if (c_ij > max)
        //                {
        //                    max = c_ij;
        //                    result = other_word;
        //                }
        //            }
        //        }
        //    });

        //    return result;
        //}

        //static string LocalAnalysis(string word, Dictionary<string, PostingList> IF)
        //{
        //    if (!IF.ContainsKey(word.ToLower()))
        //        return "";
        //    List<int> docs1 = IF[word.ToLower()].docs;
        //    int max = 0; // max score
        //    string result = ""; // final result (aka chosen term)

        //    Parallel.ForEach(IF, entry =>
        //    {
        //        string other_word = entry.Key;
        //        if (!other_word.Equals(word) && !stopWords.Contains(other_word))
        //        {
        //            List<int> docs2 = entry.Value.docs;
        //            List<int> docs = docs1.Intersect(docs2).ToList(); // take intersect

        //            int c_ij = 0;
        //            foreach (var doc in docs)
        //            {
        //                if (relevantDocs.Contains(doc)) // Local Analysis, only consider relevant docs
        //                {
        //                    DocVector dv = new DocVector(0) { DocID = doc }; // create a dummy vector just to get the frequency
        //                    Dictionary<string, int> frequency = dv.LoadTermFrequency();
        //                    if (!frequency.ContainsKey(word) || !frequency.ContainsKey(other_word)) // over careful...
        //                        continue;
        //                    c_ij += frequency[word] * frequency[other_word];
        //                    if (c_ij > max)
        //                    {
        //                        max = c_ij;
        //                        result = other_word;
        //                    }
        //                }
        //            }
        //        }
        //    });

        //    return result;
        //}

        #endregion
    }
}
