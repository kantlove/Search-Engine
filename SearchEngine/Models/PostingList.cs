using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    /// <summary>
    /// List of documents contain this term
    /// </summary>
    public class PostingList
    {
        public string term;
        public List<string> docs;

        public PostingList(string _term, List<string> _docs)
        {
            this.term = _term;
            this.docs = _docs;
        }

        public PostingList(string text)
        {
            string[] data = text.Split(' ');
            this.term = data[0];

            docs = new List<string>();
            for(int i = 1; i < data.Length; ++i)
            {
                docs.Add(data[i]);
            }
        }

        public void AddDocs(PostingList other)
        {
            this.docs.AddRange(other.docs);
            docs.Sort();
        }

        public override int GetHashCode()
        {
            return term.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return term.Equals(((PostingList)obj).term);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(term);
            for (int i = 0; i < docs.Count; ++i)
                sb.Append(" " + docs[i]);

            return sb.ToString();
        }
    }
}
