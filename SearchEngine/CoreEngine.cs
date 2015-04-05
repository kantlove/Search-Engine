using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    /// <summary>
    /// Main program is here!
    /// </summary>
    public class CoreEngine
    {
        const int MAX_BUFFER = 16384; // bytes
        static int N_PORTIONS = 32; // number of portions to be divided at the beginning

        public static void Run()
        {
            var watch = Stopwatch.StartNew();
            Console.CursorVisible = false;

            // Get all paths of document in folder \docs
            // -------------------------------------------
            string filePath = Parameter.DataFile;
            string queryPath = Parameter.QueryFile;
            string solutionPath = Parameter.SolutionFile;
            List<string> docs = new List<string>();

            // Read list of stop words
            // ---------------------------
            Console.WriteLine("Reading stop words\n");
            StatusWriter.Title = "Reading stop words";
            StatusWriter.Print();
            HashSet<String> stopWords = new HashSet<string>(System.IO.File.ReadAllLines(@"stopwords_en.txt"));
            
            // Remove old files
            // ------------------
            Console.WriteLine("Removing old files\n");
            StatusWriter.Title = "Removing old files";
            StatusWriter.Print();
            Utility.RemoveOldFiles();
            if (Parameter.RESET)
                Utility.RemoveAll();

            // Read and process each document
            // -------------------------------------
            Console.Write("Reading documents\t");
            StatusWriter.Title = "Reading documents";
            StatusWriter.Print();
            long entries = Parser.Parse(Parameter.TermFile, filePath, stopWords);
            Parser.CreateTestDataset(stopWords);
            Console.WriteLine();

            #region Classify Code

            Console.Write("Running Classifier\t");
            StatusWriter.PrintTitle("Running Classifier");
            ClassifierHelper helper = new ClassifierHelper();
            helper.Run();

            #endregion

            #region Query Code
            //// SPIMI 
            //// Result is in file _SPIMI_.txt
            //// -----------------------------
            //Console.Write("\n\nSPIMI\t\t");
            //StatusWriter.Title = "SPIMI";
            //StatusWriter.Print();
            //SPIMI spimi = new SPIMI(Parameter.TermFile, entries / N_PORTIONS);
            //spimi.cs_writer = new ConsoleWriter(Console.CursorLeft, Console.CursorTop);
            //spimi.SinglePassIndexing(entries);
            
            //// Load the SPIMI file back
            //// -------------------------
            //Dictionary<string, PostingList> IF = Utility.LoadInvertedFile(Parameter.SpimiFile);
            
            //// Build vector space
            //// -----------------------------
            //Console.Write("\n\nVector space\t\t");
            //StatusWriter.Title = "Vector Space";
            //StatusWriter.Print();
            //VectorSpace vSpace = new VectorSpace(1);
            //// ***NOTE: After calling Init(IF), Parser.DocTerms is cleared!!!
            //vSpace.Init(IF); 

            //// Create Evaluator
            //// ----------------
            //Console.Write("\n\nQuerying Plaese wait...\t\t");
            //StatusWriter.Title = "Querying";    
            //StatusWriter.Print();
            //Evaluator evaluator = new Evaluator();
            //QueryManager.evaluator = evaluator;
            //QueryManager.stopWords = stopWords;
            //QueryManager.Parse(queryPath, solutionPath, @"queries\queries.txt", IF, Parameter.QUERY_ID);
            //QueryManager.Query(vSpace, IF, Parameter.QUERY_ID);

            //// Print F-Measure & MAP
            //// ----------------------
            //Console.Write("\n\nMeasures");
            //string fMeasure = string.Join(", ", evaluator.FMs);
            //string MAP = evaluator.MAP() + "";
            //Console.WriteLine("\n\tF-Measures = {0}", fMeasure);
            //Console.WriteLine("\n\tMAP = {0}", MAP);
            //StatusWriter.Print("Measurements", string.Format("F-Measures = {0}\nMAP = {1}", fMeasure, MAP));

            #endregion

            // Done
            // -----
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("\n\nComplete in {0}", TimeSpan.FromMilliseconds(elapsedMs));
        }
    }
}
