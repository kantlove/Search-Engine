using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class Evaluator
    {
        public Pair<float, float> []Result;
        public List<float> AverPs; // list of average precisions of queries
        public List<float> FMs; // list of F-Measures of queries

        public Evaluator()
        {
            AverPs = new List<float>();
            FMs = new List<float>();
        }

        public void Evaluate(Pair<string, float>[] rankList, HashSet<string> solution)
        {
            // This is an array of Precision and F-Measure
            Result = new Pair<float, float>[rankList.Length];
            if (rankList.Length == 0)
                return;

            int right_result = 0;
            int total_solutions = solution.Count;
            float total_precision = 0; // save this to calculate avarage precision
            bool isRelevant = false;

            int n = rankList.Length;
            for(int i = 0; i < n; ++i)
            {
                if (rankList[i] == null)
                    continue;
                // if this result is relevant
                if (solution.Contains(rankList[i].A))
                {
                    right_result++;
                    isRelevant = true;
                }
                
                // precision
                float P = right_result * 1.0f / (i + 1); 
                // recall
                float R = right_result * 1.0f / total_solutions;

                Result[i] = new Pair<float, float>(P, R);

                if(isRelevant)
                    total_precision += Result[i].A;
                isRelevant = false;
            }

            this.AverPs.Add(total_precision / right_result);
            this.FMs.Add(F_Measure());
        }

        public float F_Measure()
        {
            int i = this.Result.Length - 1;
            while (this.Result[i] == null)
                i--;
            float precision = this.Result[i].A;
            float recall = this.Result[i].B;
            float f = 2 * precision * recall / (precision + recall);

            return f;
        }

        public float MAP()
        {
            if (AverPs.Count == 0)
                return 0;
            return this.AverPs.Average();
        }

        public void WriteResult(string dir)
        {
            StreamWriter writer = new StreamWriter(dir, true, new UTF8Encoding(false, true), 0x10000);
            for (int i = 0; i < Result.Length; ++i)
            {
                if (Result[i] == null) continue;
                writer.WriteLine(Result[i].A + ", " + Result[i].B);
            }
            writer.Close();
        }
    }
}
