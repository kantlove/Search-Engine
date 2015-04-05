using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    /// <summary>
    /// A mapping between a term and a document that contains it
    /// </summary>
    public class Record: IComparable
    {
        public string term, docId;
        public long termId;

        public Record(string t, long id, string doc = "0")
        {
            term = t;
            termId = id;
            docId = doc;
        }

        public int CompareTo(object obj)
        {
            return String.Compare(term, ((Record)obj).term);
        }

        public override string ToString()
        {
            return term + " " + docId;
        }
    }
}
