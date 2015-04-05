using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class VectorSpace
    {
        public Dictionary<string, DocVector> Vectors;
        public Dictionary<string, long> Positions; // save the byte position of document
        public ConsoleWriter cs_writer;
        public int Total_Docs;

        public VectorSpace(int n_docs)
        {
            Vectors = new Dictionary<string, DocVector>();
            Positions = new Dictionary<string, long>();
            Total_Docs = n_docs;
        }

        /// <summary>
        /// Init the VectorSpace
        /// REMEMBER THAT CALLING THIS WILL CLEAR Parser.DocTerms
        /// </summary>
        public void Init(Dictionary<string, PostingList> inverted_file)
        {
            if (!File.Exists(Parameter.VectorFile)) // if not done yet
                Build(inverted_file);
            Parser.Clean(); // IMPORTANT!! Release memory
            if (Positions == null || Positions.Count == 0)
                LoadPosition(Parameter.VectorPosFile);
            // LoadFromFile(Parameter.VectorFile);
        }

        /// <summary>
        /// Build vector space but because of Memory limit, 
        /// vectors will be written to file immediately after creation
        /// </summary>
        void Build(Dictionary<string, PostingList> inverted_file)
        {
            // cs_writer.RefreshOrigin();

            BinaryWriter bWrite = new BinaryWriter(new FileStream(Parameter.VectorFile, FileMode.Create));
            // We also write the offset of each vector for fast retrieval
            BinaryWriter pos_out = new BinaryWriter(new FileStream(Parameter.VectorPosFile, FileMode.Create));
            
            List<DocVector> temp = new List<DocVector>();

            int n = Parser.Count, cnt = 0;
            foreach (var entry in Parser.DocTerms)
            {
                string id = entry.Key;
                DocVector v = new DocVector(id, entry.Value, Total_Docs, inverted_file);
                temp.Add(v);

                // cs_writer.RapidWrite(string.Format("{0}/{1} vectors built", cnt++, n));
                StatusWriter.Print(string.Format("{0} vectors built", cnt++));

                // After a certain amount of vector is created, write them to file
                if (temp.Count >= 10)
                {
                    WriteToFile(temp, bWrite, pos_out);
                    temp.Clear(); // Clear memory
                }
            }

            // write the remainings
            if (temp.Count > 0)
                WriteToFile(temp, bWrite, pos_out);
            temp.Clear();

            //fout.Close();
            bWrite.Close();
            pos_out.Close();
        }

        /// <summary>
        /// Write vectors to file
        /// </summary>
        void WriteToFile(List<DocVector> docs, BinaryWriter bWrite, BinaryWriter pos_out)
        {
            for (int j = 0; j < docs.Count; ++j) 
            {
                string text = docs[j].ToString();
                bWrite.Write(text);
                pos_out.Write(docs[j].DocID + " " + bWrite.BaseStream.Position);
            }
        }

        public void LoadFromFile(string source)
        {
            //cs_writer.RefreshOrigin();
            BinaryReader bRead = new BinaryReader(new FileStream(source, FileMode.Open));
            int cnt = 0; 
            long pos = 0, length = bRead.BaseStream.Length;
            string line = "";
            while (pos < length)
            {
                line = bRead.ReadString();
                DocVector v = ParseVector(line);

                Vectors.Add(v.DocID, v);

                //cs_writer.RapidWrite(string.Format("{0} vectors built", cnt++));
                StatusWriter.Print(string.Format("{0} vectors built", cnt++));

                pos += (line.Length * sizeof(char));
            }

            //fin.Close();
            bRead.Close();
        }

        /// <summary>
        /// Load the position (in bytes) of document in the file
        /// This is used to improve document searching speed in binary file
        /// </summary>
        public void LoadPosition(string source)
        {
            //cs_writer.RefreshOrigin();

            BinaryReader bRead = new BinaryReader(new FileStream(source, FileMode.Open));
            //StreamReader fin = new StreamReader(source);
            int cnt = 0;
            long pos = 0, length = bRead.BaseStream.Length;
            string line = "";
            string[] parts;
            while (pos < length)
            {
                line = bRead.ReadString();
                parts = line.Split(' ');
                string id = parts[0]; // doc id
                long byte_offset = Convert.ToInt32(parts[1]);  // position (in bytes)
                Positions.Add(id, byte_offset);

                //cs_writer.RapidWrite(string.Format("{0} vectors loaded", cnt++));
                StatusWriter.Print(string.Format("{0} vectors loaded", cnt++));

                pos += (line.Length * sizeof(char));
            }

            //fin.Close();
            bRead.Close();
        }

        /// <summary>
        /// Search for vector in binary file
        /// </summary>
        public DocVector GetVector(string doc_id, BinaryReader bRead)
        {
            if (!Positions.ContainsKey(doc_id))
                return null;

            long pos = Positions[doc_id];
            bRead.BaseStream.Seek(pos, SeekOrigin.Begin);
            string line = bRead.ReadString();
            DocVector rs = ParseVector(line);

            return rs;
        }

        /// <summary>
        /// parse Vector Object from a line of text
        /// </summary>
        DocVector ParseVector(string line)
        {
            string []parts = line.Split(' '), entry;

            parts = line.Split(' ');
            string id = parts[0]; // doc id
            int wordCount = Convert.ToInt32(parts[1]);  // number of words
            int n = Convert.ToInt32(parts[2]);  // vector length
            DocVector v = new DocVector(n);
            v.DocID = id;
            v.WordCount = wordCount;

            for (int i = 0; i < n; ++i)
            {
                entry = parts[i + 3].Split(':');
                v.Coordinates.Add(Convert.ToInt32(entry[0]), (float)Convert.ToDouble(entry[1]));
            }

            return v;
        }
    }
}
