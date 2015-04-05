using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchEngine
{
    public static class Utility
    {
        /// <summary>
        /// Extract unique words from text
        /// Using regular expression
        /// </summary>
        public static string[] toWords(this string text, HashSet<String> stopWords = null, bool distinct = false)
        {
            HashSet<string> words = new HashSet<string>(text.Split(' '));
            words.RemoveWhere(w => stopWords.Contains(w));
            return words.ToArray();

            // return text.Split(' ');
        }

        /// <summary>
        /// Remove all non-word characters
        /// </summary>
        /// <param name="s"></param>
        public static string preProcess(this string s)
        {
            string pattern = @"\W"; // non word characters
            string replacement = " ";
            string result = Regex.Replace(s, pattern, replacement, RegexOptions.None);
            pattern = @"[A-Z][A-Z]";
            result = Regex.Replace(result, pattern, replacement, RegexOptions.None);
            result = result.Replace("  ", " ");
            return result;
        }

        public static Record toRecord(this string s)
        {
            string[] parts = s.Split(' ');
            return new Record(parts[0], (int)(parts[1][0] - '0')); // parse term and termID
        }

        public static Record[] toRecordArray(this string[] s, string docId)
        {
            Record[] rs = new Record[s.Length];
            for (int i = 0; i < s.Length; ++i)
                rs[i] = new Record(s[i], i, docId);
            return rs;
        }

        public static void print<T>(T s)
        {
            System.Console.WriteLine("{0}", s);
        }

        public static bool checkFileExist(string fileName)
        {
            string dir = @"output\" + fileName + ".txt";
            return System.IO.File.Exists(dir);
        }

        public static void writeTextToFile(string fileName, string text)
        {
            string path = "output", dir = @"output\" + fileName + ".txt";
            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);

            // Append new record to an existing file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dir, true, new UTF8Encoding(false, true), 65536))
            {
                file.Write(text);
            }
        }

        public static void writeTextToFile(string fileName, string[] lines)
        {
            string dir = @"output\" + fileName + ".txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dir))
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    if (!String.IsNullOrEmpty(lines[i]))
                    {
                        file.Write(lines[i]);
                        if (i < lines.Length - 1)
                            file.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Write to file in \output folder
        /// </summary>
        public static void writeToFile(string fileName, Record[] r)
        {
            string dir = @"output\" + fileName + ".txt";
            bool addNewLine = true;
            if (!checkFileExist(fileName))
            {
                addNewLine = false;
            }

            // Append new record to an existing file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dir, true, new UTF8Encoding(false, true), 65536))
            {
                if (addNewLine)
                    file.WriteLine();
                int n = r.Length;
                for (int i = 0; i < n; ++i)
                {
                    if (r[i] == null) break;
                    string text = r[i].termId + " " + r[i].docId;
                    if (i + 1 == n)
                        file.Write(text); //avoid carriage return at the end
                    else
                        file.WriteLine(text);
                }
            }
        }

        /// <summary>
        /// Write term and termID
        /// </summary>
        public static void writeMapFile(string fileName, Record[] r, bool noReturn = true)
        {
            string dir = @"output\" + fileName + ".txt";
            bool addNewLine = true;
            if (!checkFileExist(fileName))
            {
                addNewLine = false;
            }

            // Append new record to an existing file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dir, true, new UTF8Encoding(false, true), 65536))
            {
                if (addNewLine)
                    file.WriteLine();
                int n = r.Length;
                for (int i = 0; i < n; ++i)
                {
                    if (r[i] == null) break;
                    string text = r[i].term + " " + r[i].termId;
                    if (i + 1 == n && noReturn)
                        file.Write(text); //avoid carriage return at the end
                    else
                        file.WriteLine(text);
                }
            }
        }

        /// <summary>
        /// Write term and docID
        /// </summary>
        public static void writeTermToFile(string fileName, Record[] r, bool noReturn = true)
        {
            string dir = @"output\" + fileName + ".txt";
            bool addNewLine = true;
            if (!checkFileExist(fileName))
            {
                addNewLine = false;
            }

            // Append new record to an existing file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dir, true, new UTF8Encoding(false, true), 65536))
            {
                if (addNewLine)
                    file.WriteLine();
                int n = r.Length;
                for (int i = 0; i < n; ++i)
                {
                    if (r[i] == null) break;
                    string text = r[i].term + " " + r[i].docId;
                    if (i + 1 == n && noReturn)
                        file.Write(text); //avoid carriage return at the end
                    else
                        file.WriteLine(text);
                }
            }
        }

        public static void WriteDictToFile <TKey, TValue> (string filePath, IDictionary<TKey, TValue> dict)
        {
            if(dict == null || dict.Count == 0)
                throw new Exception("Why do u want to write an empty Dictionary?");

            // Check if value is a list
            bool isList = dict.Values.First() is System.Collections.IEnumerable;
            Type t_key = typeof(TKey);
            Type t_value = typeof(TValue);

            using (BinaryWriter bWrite = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
            {
                foreach(var entry in dict)
                {
                    if (t_key.IsPrimitive)
                        bWrite.Write(Convert.ToString(entry.Key));
                    else // object type
                        bWrite.Write(entry.Key.ToString());


                    if(isList)
                    {
                        var list = (System.Collections.ICollection)entry.Value;

                        bWrite.Write(Convert.ToString(list.Count)); // write the size
                        foreach (var item in list)
                        {
                            if (item.GetType().IsPrimitive)
                                bWrite.Write(Convert.ToString(item));
                            else // object type
                                bWrite.Write(item.ToString());
                        }
                    }
                    else
                    {
                        if (t_value.IsPrimitive)
                            bWrite.Write(Convert.ToString(entry.Value));
                        else
                            bWrite.Write(entry.Value.ToString());
                    }
                }
            }
        }

        public static void fileCopy(string source, string target)
        {
            string source_dir = @"output\" + source + ".txt";
            string target_dir = @"output\" + target + ".txt";
            System.IO.File.Copy(source_dir, target_dir);
        }

        public static void Empty(this System.IO.DirectoryInfo directory)
        {
            try
            {
                foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            }
            catch (Exception) { }
            try
            {
                foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception) { }
        }

        public static Dictionary<string, PostingList> LoadInvertedFile(string dir)
        {
            Dictionary<string, PostingList> IF = new Dictionary<string, PostingList>();

            StreamReader reader = new StreamReader(dir);

            while (reader.Peek() != -1)
            {
                PostingList pl = new PostingList(reader.ReadLine());
                IF.Add(pl.term, pl);
            }

            reader.Close();

            return IF;
        }

        public static int log2(long num)
        {
            int rs = 0;
            while ((1 << (rs + 1)) <= num) rs++;
            return rs;
        }

        /// <summary>
        /// Rapidly write message on Console
        /// </summary>
        public static void rapidWrite(string message)
        {
            Console.Write(message);
            Console.SetCursorPosition(Console.CursorLeft - message.Length, Console.CursorTop);
        }

        /// <summary>
        /// Remove carriage return at the end of a file
        /// </summary>
        public static void removeCarriageReturn(string fileName)
        {
            string dir = @"output\" + fileName + ".txt";
            string myFileData;
            myFileData = System.IO.File.ReadAllText(dir);

            // Remove last CR/LF
            if (myFileData.EndsWith(Environment.NewLine))
            {
                System.IO.File.WriteAllText(dir, myFileData.TrimEnd(Environment.NewLine.ToCharArray())); // Removes ALL CR/LF from the end!
            }
        }

        public static HashSet<int> ToHashSet(this string text)
        {
            HashSet<int> result = new HashSet<int>();

            string[] parts = text.Split(' ');
            for (int i = 0; i < parts.Length; ++i)
                result.Add(Convert.ToInt32(parts[i]));

            return result;
        }

        public static void RemoveOldFiles()
        {
            //DirectoryInfo directory = new DirectoryInfo(@"output\frequency\");
            //directory.Empty();
            DirectoryInfo directory = new DirectoryInfo(@"output\result\");
            directory.Empty();
            FileInfo[] files = new DirectoryInfo(@"output\").GetFiles("*dict_*");
            for (int i = 0; i < files.Length; ++i)
                files[i].Delete();
        }

        public static void RemoveAll()
        {
            DirectoryInfo directory = new DirectoryInfo(@"output\");
            directory.Empty();
        }
    }
}
