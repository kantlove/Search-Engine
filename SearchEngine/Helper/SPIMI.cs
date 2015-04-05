using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

namespace SearchEngine
{
    public class SPIMI
    {
        public long BLOCK_SIZE;
        public ConsoleWriter cs_writer;
        public readonly string Source_dir;

        public SPIMI(string source, long block_size)
        {
            this.Source_dir = source;
            this.BLOCK_SIZE = block_size;
        }

        /// <summary>
        /// SPIMI
        /// Idea: build dictionary for basic blocks
        /// recursively merge 2 dictionaries of 2 blocks into 1
        /// </summary>
        public void SinglePassIndexing(long entries)
        {
            if (cs_writer == null) throw new Exception("Console Writer is null");
            if (File.Exists(Parameter.SpimiFile)) // already done
                return;

            long block_size = BLOCK_SIZE;

            Console.Write("\n\tBuilding dictionaries: ");
            cs_writer.RefreshOrigin();
            int n_dict = CreateDictionary(block_size);

            Console.Write("\n\tMerging: ");
            cs_writer.RefreshOrigin();
            MergeDictionary(block_size, n_dict);

            Console.Write("\n\tCleaning: ");
            cs_writer.RefreshOrigin();
            CleanJunkFiles();
        }

        /// <summary>
        /// Create dictionaries for all basic blocks
        /// </summary>
        /// <returns>Number of dictionaries</returns>
        int CreateDictionary(long block_size)
        {
            string dir = Source_dir;
            Dictionary<string, PostingList> dict = new Dictionary<string, PostingList>();

            int j = 0; // number of files created
            // StreamReader reader = new StreamReader(dir);
            BinaryReader bin = new BinaryReader(new FileStream(dir, FileMode.Open));
            string line = "";
            int i = 0;
            int pos = 0;
            int fileLength = (int)bin.BaseStream.Length;

            while (pos < fileLength)
            {
                line = bin.ReadString();
                PostingList pl = new PostingList(line);

                AddToDictionary(dict, pl);

                i++;
                if (i == block_size || pos + 1 == fileLength)
                {
                    string file = string.Format("dict_1_{0}", j);
                    WriteDictionaryToFile(dict, file);

                    i = 0; j++;
                    dict.Clear();

                    cs_writer.RapidWrite(string.Format("{0}", j));
                }

                pos += line.Length * sizeof(char);
            }
            //while (reader.Peek() != -1)
            //{
            //    line = reader.ReadLine();
            //    PostingList pl = new PostingList(line);

            //    AddToDictionary(dict, pl);

            //    i++;
            //    if (i == block_size || reader.Peek() == -1)
            //    {
            //        string file = string.Format("dict_1_{0}", j);
            //        WriteDictionaryToFile(dict, file);

            //        i = 0; j++;
            //        dict.Clear();

            //        cs_writer.RapidWrite(string.Format("{0}", j));
            //    }
            //}
            //reader.Close();
            return j;
        }

        void MergeDictionary(long block_size, int n)
        {
            string final = "";
            int j = 1, cnt = 0;
            while (cnt != 1) // if there is only 1 file, stop!
            {
                int k = 0;
                cnt = 0;
                for (int i = 0; i < n; i += 2)
                {
                    string file = string.Format("dict_{0}_{1}", j, i);
                    final = string.Format(@"dict_{0}_{1}", j + 1, k);

                    Dictionary<string, PostingList> dict1 = LoadPostingList(file);

                    if (i + 1 < n)
                    {
                        file = string.Format(@"output\dict_{0}_{1}.txt", j, i + 1);
                        StreamReader reader = new StreamReader(file);
                        while (reader.Peek() != -1)
                        {
                            PostingList p = new PostingList(reader.ReadLine());
                            AddToDictionary(dict1, p);
                        }
                        reader.Close();
                    }
                    WriteDictionaryToFile(dict1, final);
                    k++; cnt++;

                    cs_writer.RapidWrite(string.Format("{0}              ", i));
                }
                j++;
                n = cnt; // new number of files
            }

            // copy the final result to _SPIMI_.txt
            Utility.fileCopy(final, "_SPIMI_");
        }

        void AddToDictionary(Dictionary<string, PostingList> dict, PostingList pl)
        {
            if (pl.term.Equals("anticardiolipin"))
                Console.WriteLine();
            if (dict.ContainsKey(pl.term))
                dict[pl.term].AddDocs(pl);
            else
                dict.Add(pl.term, pl);
        }

        void WriteDictionaryToFile(HashSet<PostingList> set, string fileName)
        {
            string dir = string.Format(@"output\{0}.txt", fileName);
            using (StreamWriter writer = new StreamWriter(dir, true, new UTF8Encoding(false, true), 0x10000))
            {
                int i = 1;
                foreach (PostingList pl in set)
                {
                    if (i < set.Count)
                        writer.WriteLine(pl.ToString());
                    else
                        writer.Write(pl.ToString());
                    i++;
                }
            }
        }

        void WriteDictionaryToFile(Dictionary<string, PostingList> data, string fileName)
        {
            string dir = string.Format(@"output\{0}.txt", fileName);
            using (StreamWriter writer = new StreamWriter(dir, true, new UTF8Encoding(false, true), 0x10000))
            {
                int i = 1;
                foreach (var entry in data)
                {
                    PostingList pl = entry.Value;
                    if (i < data.Count)
                        writer.WriteLine(pl.ToString());
                    else
                        writer.Write(pl.ToString());
                    i++;
                }
            }
        }

        /// <summary>
        /// Load a file into PostingList collection
        /// </summary>
        Dictionary<string, PostingList> LoadPostingList(string fileName)
        {
            Dictionary<string, PostingList> result = new Dictionary<string, PostingList>();

            string dir = string.Format(@"output\{0}.txt", fileName);
            using (StreamReader reader = new StreamReader(dir))
            {
                while (reader.Peek() != -1)
                {
                    PostingList pl = new PostingList(reader.ReadLine());
                    result.Add(pl.term, pl);
                }
            }

            return result;
        }


        void CleanJunkFiles()
        {
            FileInfo[] files = new DirectoryInfo(@"output\").GetFiles("*dict_*");
            for (int i = 0; i < files.Length; ++i)
            {
                files[i].Delete();
                cs_writer.RapidWrite(string.Format("{0}/{1} files", i + 1, files.Length));
            }
        }
    }
}