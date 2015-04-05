using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class DocVector : MyVector
    {
        public string DocID;
        public int WordCount;

        public DocVector(int length) : base(length) { }

        public DocVector(string docId, int words, int n_docs, Dictionary<string, PostingList> terms) : base(terms.Count)
        {
            this.DocID = docId;
            this.WordCount = words;

            var frequency = LoadTermFrequency();

            // Build a vector for this document
            // This vector contain only index and value of elements != 0
            int i = 0;
            float eps = 0.00000001f;
            foreach (var entry in terms)
            {
                float value = 0;
                if (frequency.ContainsKey(entry.Key))
                {
                    // Calculate Term Frequency
                    value = TF(frequency, entry.Key);
                    // Calculate IDF
                    value *= IDF(n_docs, entry.Value);
                }

                // Only save value != 0
                if (Math.Abs(value - 0) > eps)
                    Coordinates.Add(i, value);
                ++i;
            }
        }

        float TF(Dictionary<string, int> frequency, string term)
        {
            return frequency[term] * 1.0f / WordCount;
        }

        float IDF(int n_docs, PostingList pl)
        {
            return 1 + (float)Math.Log10(n_docs * 1.0 / pl.docs.Count);
        }

        public Dictionary<string, int> LoadTermFrequency()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            string dir = string.Format(@"output\frequency\termCount_" + DocID + ".txt");
            const int BufferSize = 4096;
            StreamReader reader = new StreamReader(dir, Encoding.UTF8, true, BufferSize);
            string line = "";
            while (reader.Peek() != -1)
            {
                line = reader.ReadLine();
                string[] tmp = line.Split(' ');
                result.Add(tmp[0], Convert.ToInt32(tmp[1]));
            }

            reader.Close();
            return result;

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DocID + " ");
            sb.Append(WordCount + " ");
            sb.Append(Coordinates.Count + " ");

            // Format is 'index:value'
            foreach (var entry in Coordinates)
                sb.Append(entry.Key + ":" + entry.Value + " ");

            return sb.ToString();
        }
    }

}
