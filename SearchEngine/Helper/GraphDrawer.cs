using Sparrow.Chart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace SearchEngine
{
    public class GraphDrawer
    {
        public static ObservableCollection<PointXY> Data { get; set; }
        /// <summary>
        /// Create a chart and display a Form
        /// </summary>
        /// <param name="type">Normal or Global or Local ?</param>
        public static void Draw(Pair<float, float> []data, int type = 1)
        {
            if (Data == null)
                Data = new ObservableCollection<PointXY>();

            int decimal_place = 5;
            int e = (int)Math.Pow(10, decimal_place);

            PointsCollection points = new PointsCollection();
            // Add new data
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != null)
                {
                    double x = Math.Round(data[i].A * e) / e; // reduce decimal places
                    double y = Math.Round(data[i].B * e) / e; // reduce decimal places
                    points.Add(new DoublePoint() { Data = y, Value = x});
                }
            }
            

            // Hide the cover
            Service.window.Dispatcher.Invoke((Action)(() =>
            {
                Service.window.lineSeries.Points = points;
                Service.window.areaSeries.Points = points;
                Service.window.gridCover.Opacity = 0;
            }));
        }
    }

    public class PointXY
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointXY(double x, double y)
        {
            X = x;
            Y = y;           
        }
    }
}
