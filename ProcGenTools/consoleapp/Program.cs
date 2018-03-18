using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.DataStructures;
namespace consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            var tl = new OpinionatedItem<String>("┌", "top-left");
            var tr = new OpinionatedItem<String>("┐", "top-right");
            var bl = new OpinionatedItem<String>("└", "bot-left");
            var br = new OpinionatedItem<String>("┘", "bot-right");
            var em = new OpinionatedItem<String>("O", "empty");
            var all = new List<IOpinionatedItem> { tl, tr, br, bl};
            /*
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {tl, bl}, 1, 0, 0);
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {tr,br }, -1, 0, 0);
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {tl, tr }, 0, 1, 0);
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {bl, br }, 0, -1, 0);
            */
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, 1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, -1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, 1, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, -1, 0);

            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, 1, 0, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, -1, 0, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, 1, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, -1, 0);

            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, 1, 0, 0);
            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, -1, 0, 0);
            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, 1, 0);
            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, -1, 0);

            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, 1, 0, 0);
            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, -1, 0, 0);
            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, 1, 0);
            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, -1, 0);

            /*var tl = new OpinionatedItem<String>("_", "top-left");
            var tr = new OpinionatedItem<String>(" ", "top-right");
            var t = new OpinionatedItem<String>(".", "terminator");
            tl.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { tr, t, tl });
            tr.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { tr, t, tl });
            t.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { tr, t, tl });

            tl.ClearAcceptableInDirection(1, 0, 0);
            tl.ClearAcceptableInDirection(-1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, t }, 1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, t }, -1, 0, 0);

            tr.ClearAcceptableInDirection(1, 0, 0);
            tr.ClearAcceptableInDirection(-1, 0, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { t, tr },1,0,0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { t, tr }, -1, 0, 0);

            var wcf = new WcfGrid(0);
            wcf.Init(10,10,1,new List<IOpinionatedItem>() { tl, tr, t });*/

            var keyVal = ' ';
            var seed = 0;
            while (keyVal != 'Q' && keyVal != 'q')
            {
                var wcf = new WcfGrid(seed++);
                wcf.Init(20, 10, 1, all);
                var shape = new List<WcfVector>().Cross3dShape();
                wcf.SetInfluenceShape(shape);

                wcf.CollapseAll();

                wcf.PrintStatesToConsole2d();
                Console.WriteLine("Press any key to quit.");
                keyVal = Console.ReadKey().KeyChar;
            }
        }
    }
}
