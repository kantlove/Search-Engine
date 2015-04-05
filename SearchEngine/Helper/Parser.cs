using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class Parser
    {
        static int TEST_LIMIT = Parameter.DOCS_LIMIT; // set to a small number to test
        public static int Count;
        public static SortedDictionary<string, int> DocTerms = new SortedDictionary<string, int>(); // number of terms of each doc
        public static Dictionary<string, int> CategoryId = new Dictionary<string, int>(); // category name and its ID
        public static Dictionary<int, List<string>> DocsOfCategory = new Dictionary<int, List<string>>(); // all docs of each category
        // Test dataset. DocId and terms in this doc
        public static Dictionary<string, string[]> TestDataset = new Dictionary<string, string[]>();

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public static long Parse(string dir, string filePath, HashSet<string> stopWords)
        {
            if (File.Exists(Parameter.TermInfoFile)) // already done no need to load again
            { 
                // return LoadInfo();
                long entries = LoadInfo();
                LoadClassifierData();
                return entries;
            }
            else
            {
                string[] categories = Directory.GetDirectories(@"docs\"); // get all sub-folders
                long entries = 0; // number of records created
                int done = 0;

                for (int i = 0; i < categories.Length; ++i)
                {
                    string category_path = categories[i];
                    string category_name = category_path.Split('\\').Last();
                    CategoryId.Add(category_name, i);
                }

                for (int c_id = 0; c_id < categories.Length; ++c_id)
                {
                    string category_path = categories[c_id];
                    // Frequency of a term in this category
                    Dictionary<string, int> term_frequency = new Dictionary<string, int>();

                    // Now read all file in this category
                    string[] filePaths = Directory.GetFiles(category_path);
                    for (int i = 0; i < filePaths.Length; ++i)
                    {
                        entries += ReadFromFile(dir, filePaths[i], stopWords, c_id, term_frequency);

                        StatusWriter.Print(++done + " docs");

                        // only take a certain number of docs to test
                        if (done == TEST_LIMIT) break;
                    }

                    // Save the frequency to file for the Classifier to use
                    SaveTermFrequency(c_id, term_frequency);
                }

                Count = DocTerms.Count; // update this!
                WriteInfo(entries); // save important info to file so we dont have to load all the terms again

                // Save all data for later use
                SaveClassifierData();

                return entries;
            }
        }

        public static void CreateTestDataset(HashSet<String> stopWords)
        {
            string[] categories = Directory.GetDirectories(@"docs\"); // get all sub-folders
            int done = 0;

            // Only take some docs
            int limit = Parameter.TEST_SIZE / CategoryId.Count;

            foreach (string category_path in categories)
            {
                string category_name = category_path.Split('\\').Last();

                // Now read all file in this category
                string[] filePaths = Directory.GetFiles(category_path);
                // Take some docs for test data
                for (int i = 0; i < Math.Min(limit, filePaths.Length); ++i)
                {
                    string docId = filePaths[i].Split('\\').Last().Replace("(", "").Replace(")", "");
                    // attach category id to the begining because there are many docs with the same id 
                    docId = CategoryId[category_name] + "_" + docId;

                    const int BufferSize = 4096; // Cluster size in NTFS
                    using (StreamReader fin = new StreamReader(filePaths[i], Encoding.UTF8, true, BufferSize))
                    {
                        while (!fin.ReadLine().Contains(": ")) { } // Skip the non-necessary part
                        string text = fin.ReadToEnd();
                        string[] words = Process(text, stopWords);

                        TestDataset.Add(docId, words);
                    }

                    StatusWriter.Print("Building test data", ++done + " docs");
                }
            }
        }

        static void SaveTermFrequency(int category_id, Dictionary<string, int> frequency)
        {
            string fileName = category_id + "_frequency";
            string path = Parameter.TermFrequencyFile + fileName;
            Directory.CreateDirectory(Parameter.TermFrequencyFile); // create folder if not exist

            using (BinaryWriter bWrite = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {
                foreach (var entry in frequency)
                {
                    bWrite.Write(entry.Key); // write term
                    bWrite.Write(entry.Value); // write frequency
                }
            }
        }

        static void SaveClassifierData()
        {
            Utility.WriteDictToFile(@"output\_DOCSTERM_", DocTerms);
            Utility.WriteDictToFile(@"output\_CategoryID_", CategoryId);
            Utility.WriteDictToFile(@"output\_DocsOfCategory_", DocsOfCategory);
        }

        static void LoadClassifierData()
        {
            LoadDocTerms();
            LoadCategoryID();
            LoadDocsOfCategory();
        }

        static void LoadDocTerms()
        {
            if (DocTerms == null)
                DocTerms = new SortedDictionary<string, int>();

            using (BinaryReader bRead = new BinaryReader(new FileStream(@"output\_DOCSTERM_", FileMode.Open, FileAccess.Read, FileShare.Read, 4096)))
            {
                while (bRead.PeekChar() != -1)
                {
                    string key = bRead.ReadString();
                    int value = Convert.ToInt32(bRead.ReadString());
                    DocTerms.Add(key, value);
                }
            }
        }

        static void LoadCategoryID()
        {
            if (CategoryId == null)
                CategoryId = new Dictionary<string, int>();

            using (BinaryReader bRead = new BinaryReader(new FileStream(@"output\_CategoryID_", FileMode.Open, FileAccess.Read, FileShare.Read, 4096)))
            {
                while (bRead.PeekChar() != -1)
                {
                    string key = bRead.ReadString();
                    int value = Convert.ToInt32(bRead.ReadString());
                    CategoryId.Add(key, value);
                }
            }
        }

        static void LoadDocsOfCategory()
        {
            if (DocsOfCategory == null)
                DocsOfCategory = new Dictionary<int, List<string>>();

            using (BinaryReader bRead = new BinaryReader(new FileStream(@"output\_DocsOfCategory_", FileMode.Open, FileAccess.Read, FileShare.Read, 4096)))
            {
                while (bRead.PeekChar() != -1)
                {
                    int key = Convert.ToInt32(bRead.ReadString());
                    int size = Convert.ToInt32(bRead.ReadString());
                    List<string> list = new List<string>();
                    for(int i = 0; i < size; ++i)
                    {
                        string item = bRead.ReadString();
                        list.Add(item);
                    }

                    DocsOfCategory.Add(key, list);
                }
            }
        }

        static long LoadInfo()
        {
            StreamReader fin = new StreamReader(Parameter.TermInfoFile);
            Count = Convert.ToInt32(fin.ReadLine());
            long entries = Convert.ToInt32(fin.ReadLine());

            fin.Close();
            return entries;
        }

        /// <summary>
        /// Read data from source file into docId and terms
        /// </summary>
        /// <returns>Number of terms</returns>
        public static long ReadFromFile(string dir, string filePath, HashSet<string> stopWords, int category_id, Dictionary<string, int> term_frequency)
        {
            //BinaryWriter bWrite = new BinaryWriter(new FileStream(dir, FileMode.OpenOrCreate));
            const int BufferSize = 4096; // Cluster size in NTFS
            StreamReader fin = new StreamReader(filePath, Encoding.UTF8, true, BufferSize);

            //ConsoleWriter cs_writer = new ConsoleWriter(Console.CursorLeft, Console.CursorTop);
            List<Record[]> listOfRecords = new List<Record[]>();

            long entries = 0;
            string text = "";
            string docId = filePath.Split('\\').Last().Replace("(", "").Replace(")", "");
            // attach category id to the begining because there are many docs with the same id 
            docId = category_id + "_" + docId;

            while (fin.ReadLine().Contains(": ")) { } // Skip the non-necessary part

            text = fin.ReadToEnd();

            // Convert to records. Each record contains a term and a doc_id
            Record[] records = Process(docId, text, stopWords);
            //listOfRecords.Add(records);
            entries += records.Length;

            fin.Close();

            // Calculate frequency of terms
            foreach (var record in records)
            {
                if (term_frequency.ContainsKey(record.term))
                    ++term_frequency[record.term];
                else
                    term_frequency.Add(record.term, 1);
            }

            // Now save all records to file
            //SaveRecordToFile(listOfRecords, bWrite);

            //bWrite.Close();

            // Save this doc to its category
            if (DocsOfCategory.ContainsKey(category_id))
                DocsOfCategory[category_id].Add(docId);
            else
                DocsOfCategory.Add(category_id, new List<string> { docId });

            return entries;
        }

        static void SaveRecordToFile(List<Record[]> listOfRecords, BinaryWriter bWrite)
        {
            for (int i = 0; i < listOfRecords.Count; ++i)
            {
                Record[] r = listOfRecords[i];
                for (int j = 0; j < r.Length; ++j)
                    if (!string.IsNullOrEmpty(r[j].term))
                        bWrite.Write(r[j].ToString());
            }
        }

        /// <summary>
        /// Read and process a document
        /// </summary>
        /// <returns>Number of chosen words in this document</returns>
        static Record[] Process(string docId, string doc, HashSet<String> stopWords)
        {
            // Remove all non-word character (ex: , . ! ...)
            // ----------------------------------------------
            string text = doc.preProcess();

            // Remove stopwords and lowercase
            // ------------------------------
            string[] words = text.toWords(stopWords);
            for (int i = 0; i < words.Length; ++i)
                words[i] = words[i].ToLower();

            // Count the term frequency and save to file
            SortedList<string, int> frequency = TermCount(docId, words);

            // Save the number of terms of this doc
            DocTerms[docId] = frequency.Count;

            // Now we can remove duplicates to continue
            words = words.Distinct().ToArray();

            // Remove empty strings
            words = words.Where(w => !string.IsNullOrEmpty(w)).ToArray();

            // Sort by frequency
            Array.Sort(words, (w1, w2) => frequency[w2].CompareTo(frequency[w1]));

            // Only take some words
            if (words.Length > Parameter.TERMS_LIMIT)
                words = words.Take(Parameter.TERMS_LIMIT).ToArray();

            // Convert
            Record[] records = words.toRecordArray(docId);

            frequency.Clear();

            return records;
        }

        /// <summary>
        /// Extract words from a document
        /// </summary>
        static string[] Process(string doc, HashSet<String> stopWords)
        {
            // Remove all non-word character (ex: , . ! ...)
            // ----------------------------------------------
            string text = doc.preProcess();

            // Remove stopwords and lowercase
            // ------------------------------
            string[] words = text.toWords(stopWords);
            for (int i = 0; i < words.Length; ++i)
                words[i] = words[i].ToLower();

            // Count the term frequency and save to file
            SortedList<string, int> frequency = TermCount(null, words);

            // Now we can remove duplicates to continue
            words = words.Distinct().ToArray();

            // Remove empty strings
            words = words.Where(w => !string.IsNullOrEmpty(w)).ToArray();

            // Sort by frequency
            Array.Sort(words, (w1, w2) => frequency[w2].CompareTo(frequency[w1]));

            // Only take some words
            if (words.Length > Parameter.TERMS_LIMIT)
                words = words.Take(Parameter.TERMS_LIMIT).ToArray();

            return words;
        }

        /// <summary>
        /// Count the term frequency and save to file
        /// </summary>
        static SortedList<string, int> TermCount(string docId, string[] words)
        {
            StringBuilder sb = new StringBuilder();
            SortedList<string, int> frequency = new SortedList<string, int>();
            for (int i = 0; i < words.Length; ++i)
            {
                string term = words[i];
                if (string.IsNullOrEmpty(term)) continue;

                if (frequency.ContainsKey(term))
                    ++frequency[term];
                else
                    frequency.Add(term, 1);
            }

            foreach (var entry in frequency)
                sb.Append(entry.Key + ' ' + entry.Value + Environment.NewLine);

            // Save the result to file
            //Directory.CreateDirectory(@"output\frequency\");
            //string dir = @"output\frequency\termCount_" + docId + ".txt";
            //StreamWriter file = new System.IO.StreamWriter(dir, true, new UTF8Encoding(false, true), 65536);
            //file.Write(sb.ToString());
            //file.Close();

            return frequency;
        }

        /// <summary>
        /// Save neccessary information to file so we don't need to load every term again
        /// </summary>
        static void WriteInfo(long entries)
        {
            StreamWriter fout = new StreamWriter(Parameter.TermInfoFile, true, new UTF8Encoding(false, true), 0x10000);
            fout.WriteLine(Count);
            fout.WriteLine(entries);
            fout.Close();
        }

        

        /// <summary>
        /// Clear all data to save memory. 
        /// Should be called after we have Vector Space
        /// </summary>
        public static void Clean()
        {
            DocTerms.Clear();
        }

    }
}
