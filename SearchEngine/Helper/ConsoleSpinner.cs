using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class ConsoleSpinner
    {
        int counter;
        public ConsoleSpinner()
        {
            counter = 0;
        }
        public void Turn()
        {
            counter++;
            string text = "Loading ";
            Console.Write(text);
            switch (counter % 3)
            {
                case 0: Console.Write("[/]"); break;
                case 1: Console.Write("[-]"); break;
                case 2: Console.Write("[\\]"); break;
                case 3: Console.Write("[|]"); break;
            }
            Console.SetCursorPosition(Console.CursorLeft - 3 - text.Length, Console.CursorTop);
            System.Threading.Thread.Sleep(50);
        }
    }
}
