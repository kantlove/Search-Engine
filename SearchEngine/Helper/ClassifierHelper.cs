using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    /// <summary>
    /// This class help running the classify process and draw graph
    /// </summary>
    public class ClassifierHelper
    {
        // Classification result
        // Contains size of classifier and precision
        public Dictionary<int, float> Result;

        public ClassifierHelper()
        {
            Result = new Dictionary<int, float>();
        }

        public void Run()
        {
            for (int size = 1000; size <= 15000; size += 2000)
            {
                StatusWriter.PrintTitle("Classifying, size = " + size);

                Classifier classifier = new Classifier(size);
                int correct = 0, cnt = 0;
                Parallel.ForEach(Parser.TestDataset, doc =>
                {
                    // doc.Value is array of terms
                    int doc_category = classifier.Classify(doc.Value);
                    string category_name = Parser.CategoryId.First(x => x.Value == doc_category).Key;

                    int correct_category = Convert.ToInt32(doc.Key.Split('_')[0]);

                    // Ok, correct
                    if (correct_category == doc_category)
                        ++correct;

                    StatusWriter.Printf("{0} docs\n{1} is {2}", cnt++, doc.Key, category_name);
                });

                float precision = correct * 1.0f / Parser.TestDataset.Count;

                Result.Add(size, precision);

                
            }


            // Draw graph
            Pair<float, float> []data = new Pair<float, float>[Result.Count];
            int i = 0;
            foreach (var entry in Result)
                data[i++] = new Pair<float, float>(entry.Value * 100, entry.Key);
            Array.Sort(data);
            GraphDrawer.Draw(data);
        }
    }
}
