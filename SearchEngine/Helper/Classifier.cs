using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    /// <summary>
    /// Naive Bayes Classifier
    /// This class need data from Parser to run!
    /// </summary>
    public class Classifier
    {
        // Total number of docs for training
        public int Size { get; set; }

        // Size of vocabulary
        public int VocabSize { get; private set; }

        // Data from Parser but is limited by Size
        public Dictionary<int, List<string>> DocsOfCategory; // all docs of each category

        // Probability P(Ci) : chance of category Ci
        public Dictionary<int, float> P_Ci;

        public SortedList<int, Dictionary<string, int>> CategoryTermFrequency;

        public Classifier(int size)
        {
            Size = size;
            P_Ci = new Dictionary<int, float>();

            FetchData();
            Init();
        }

        public void Init()
        {
            // Calculate P(Ci)
            // ---------------
            int limit = Size / this.DocsOfCategory.Count; // only take some docs, DocsOfCategory.Count is number of Category
            foreach(var entry in this.DocsOfCategory)
            {
                // Number of docs of category Ci / total docs
                float p_ci = limit * 1.0f / Size;

                P_Ci.Add(entry.Key, p_ci);

                // Also calculate vocabulary size
                foreach(var doc_id in entry.Value)
                    VocabSize += Parser.DocTerms[doc_id];
            }

            // Calculate term frequency
            // -------------------------
            if (CategoryTermFrequency == null)
                CategoryTermFrequency = new SortedList<int, Dictionary<string, int>>();
            foreach(var category_id in Parser.CategoryId.Values)
            {
                var tmp = LoadTermFrequency(category_id);
                CategoryTermFrequency.Add(category_id, tmp);
            }
        }

        /// <summary>
        /// Fetch data from Parser but limited by Size
        /// </summary>
        public void FetchData()
        {
            if (this.DocsOfCategory == null)
                this.DocsOfCategory = new Dictionary<int, List<string>>();

            // limit = number of docs to be taken in each category
            int limit = Size / Parser.CategoryId.Count;

            DocsOfCategory = Parser.DocsOfCategory.ToDictionary(
                entry => entry.Key, entry => entry.Value.Take(limit).ToList());
        }

        /// <summary>
        /// Classify a document
        /// </summary>
        /// <param name="terms">terms of that document</param>
        public int Classify(string []terms)
        {
            // Probability P(Dx | Ci)
            Dictionary<int, float> P_Dx_Ci = new Dictionary<int, float>();
            float max = -1;
            int ans = -1; // answer = category id of this doc

            foreach (var category_id in Parser.CategoryId.Values)
            {
                int number_of_terms = TermsInCategory(category_id);
                // frequency of a term in this category
                var frequency = CategoryTermFrequency[category_id];

                float p = 0;
                foreach (string term in terms)
                    if (frequency.ContainsKey(term))
                        p += (frequency[term] + 1) * 1.0f / (number_of_terms + VocabSize);

                P_Dx_Ci.Add(category_id, p);
            }

            foreach (var entry in P_Ci)
            {
                int category_id = entry.Key;
                float p = entry.Value * P_Dx_Ci[category_id];

                // Update answer
                if (max == -1 || p > max)
                {
                    max = p;
                    ans = category_id;
                }
            }

            return ans;
        }

        /// <summary>
        /// Get number of terms in this category
        /// </summary>
        int TermsInCategory(int category_id)
        {
            var docs = this.DocsOfCategory[category_id];
            int ans = 0;
            foreach(var doc_id in docs)
                ans += Parser.DocTerms[doc_id];

            return ans;
        }

        Dictionary<string, int> LoadTermFrequency(int category_id)
        {
            Dictionary<string, int> result = new Dictionary<string,int>();

            string fileName = category_id + "_frequency";
            string path = Parameter.TermFrequencyFile + fileName;
            
            using(BinaryReader bRead = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096)))
            {
                int pos = 0;
                int fileLength = (int)bRead.BaseStream.Length;
                while (pos < fileLength)
                {
                    string term = bRead.ReadString();
                    int frequency = bRead.ReadInt32();
                    result.Add(term, frequency);

                    // update position
                    pos += term.Length * sizeof(char);
                    pos += sizeof(int);
                }
            }

            return result;
        }
    }
}
