using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.DataStructures;
using System.Configuration;
namespace Test.HierarchicalMaps
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Presse Enter");
            Console.ReadKey();
            HierarchicalMap.RelativeScales = new int[] { 4,4 };
            var map = new HierarchicalMap(10, 10, new Random(4));
            //map.SetTestPortals();
            //map.CreatePaths();
            //map.CoverPathsWithZones(3,2);
            map.SpawnZone(3,2);
            map.SpawnZone(3, 2);
            map.SpawnZone(3, 2);
            map.SpawnZone(3, 2);
            map.CreateSubMaps();
            map.PrintMasterToConsole();
            map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
        }
    }
}
