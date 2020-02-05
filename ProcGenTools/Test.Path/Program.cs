using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.DataStructures;
using System.Drawing;
namespace Test.Paths
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                var path = new Path(new Point(19,9), new Point(0,5), 20 , 10);

                for(var y = 0; y < 10; y++)
                {
                    for(var x = 0; x < 20; x++)
                    {
                        if(path._pathPoints.Any(pp=>pp.point == new Point(x, y)))
                        {
                            Console.Write('X');
                        }
                        else
                        {
                            Console.Write('-');
                        }
                    }
                    Console.Write(Environment.NewLine);
                }
                Console.WriteLine("Press Q to quit.  Press any other key to display another path.");
            }
            while (Console.ReadKey().Key != ConsoleKey.Q);

        }
    }
}
