using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class QueryVector : MyVector
    {
        int WordCount;
        public string QueryText;

        public QueryVector(string query_text, Dictionary<string, PostingList> terms) : base(terms.Count)
        {
            QueryText = query_text;
            Dictionary<string, int> frequency = CalculateFrequency(query_text);
            this.WordCount = frequency.Count;

            // Build a vector with size equal to total terms
            int i = 0;
            float eps = 0.00000001f;
            foreach(var entry in terms)
            {
                float value = 0;
                if(frequency.ContainsKey(entry.Key))
                    value = TF(frequency, entry.Key);

                if (Math.Abs(value - 0) > eps)
                    Coordinates.Add(i, value);
                ++i;
            }
        }

        Dictionary<string, int> CalculateFrequency(string query_text)
        {
            string[] words = query_text.Split(' ');
            Dictionary<string, int> rs = new Dictionary<string,int>();

            for(int i = 0; i < words.Length; ++i)
            {
                if (string.IsNullOrEmpty(words[i]))
                    continue;
                if (rs.ContainsKey(words[i]))
                    rs[words[i]]++;
                else
                    rs.Add(words[i], 1);
            }

            return rs;
        }

        float TF(Dictionary<string, int> frequency, string term)
        {
            return frequency[term] * 1.0f / WordCount;
        }
    }
}
