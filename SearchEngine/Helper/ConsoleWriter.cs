using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class ConsoleWriter
    {
        private int origin_x, origin_y;

        public ConsoleWriter() { }

        public ConsoleWriter(int x, int y)
        {
            this.origin_x = x;
            this.origin_y = y;
        }

        public void RefreshOrigin()
        {
            origin_x = Console.CursorLeft;
            origin_y = Console.CursorTop;
        }

        /// <summary>
        /// Continuously write text on console without moving the cursor
        /// </summary>
        /// <param name="text"></param>
        public void RapidWrite(string text)
        {
            Console.WriteLine(text);
            Console.SetCursorPosition(origin_x, origin_y);
        }

        /// <summary>
        /// Play the loading animation
        /// </summary>
        public void showLoading()
        {
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                ConsoleSpinner spinner = new ConsoleSpinner();
                while (true)
                    spinner.Turn();
            }).Start();
        }
    }
}
