using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SearchEngine
{
    public class StatusWriter
    {
        public static string Title = "";
        public static string Text = "";

        public static void Print(string title, string text)
        {
            Service.window.Dispatcher.Invoke((Action)(() =>
            {
                Service.window.tbTitle.Text = title;
                Service.window.tbSubtitle.Text = text;
            }));

        }

        public static void Print(string text)
        {
            Print(Title, text);
        }

        public static void Print()
        {
            Print(Title, "");
        }

        public static void PrintTitle(string title)
        {
            Title = title;
            Print();
        }

        public static void Printf(string format, params object[] args)
        {
            Print(string.Format(format, args));
        }
    }
}
