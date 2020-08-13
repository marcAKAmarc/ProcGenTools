using PuzzleBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PuzzleBuilder;
using System.Configuration;
using System.Drawing.Imaging;

namespace Test.OpenProcessTesting.Display
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, iDisplayer
    {
        public MainWindow()
        {
            InitializeComponent();
            Console.SetOut(new ControlWriter(FakeConsole));
            var back = new BackgroundWorker();
            back.DoWork += new DoWorkEventHandler(back_DoWork);
            back.RunWorkerAsync();
            mainWindow.UseLayoutRounding = true;
            //CreateItAll();

        }
        void back_DoWork(object sender, DoWorkEventArgs e)
        {
            CreateItAll();
        }

        void iDisplayer.Display(Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                Action action = delegate { wcfImageControl.Source = bitmapImage; };
                wcfImageControl.Dispatcher.Invoke(action);
            }
        }

        public void CreateItAll()
        {
            List<int> seeds = new List<int>() { 11 };
            var factory = new ProcessFactory<PuzzleBuilder.Process.AdvancedCircuitProcess.PuzzleProcess>(9, 12, "..//..//WfcDebug//Current//", "..//..//TilesetsDebug//Current//");
            factory.SetProcessDisplayer(this);
            var failures = 0;
            foreach (var i in seeds)
            {
                var result = factory.GetPuzzle(i, 
                    new List<System.Drawing.Point>() { new System.Drawing.Point(7,11 ) }, 
                    new List<System.Drawing.Point>() { new System.Drawing.Point(0, 1), new System.Drawing.Point(1, 11) });
                factory.SaveResult(
                    "..\\Output\\"
                );
                failures += factory.currentAttempt;
            }

            //int failedAttempts = 0;

            //var grid = new IntentionGrid(11, 11);//reuse
            //var random = new marcRandom(10);
            //var TilesConfig = new TilesetConfiguration(
            //                    "..//..//WfcDebug//Current//",
            //                    "..//..//TilesetsDebug//Current//"
            //                );
            //AdvancedCircuitProcess.PuzzleProcess processor = null; 
            //foreach (var i in seeds)
            //{
            //    var seed = i;
            //    PuzzleInfo output = null;
            //    //var attempt = 0;
            //    //var maxAttempt = 10;
            //    //while ((output == null || output.Success == false) && attempt < maxAttempt)
            //    //{

            //    //    random = new marcRandom(seed);
            //    //    if (processor == null)
            //    //        processor = new AdvancedCircuitProcess.PuzzleProcess(random, grid, TilesConfig);
            //    //    else //reuse
            //    //        processor.ClearForReuse(random);

            //    //    output = processor.CreateIt(new List<Point>() { new Point(0, 5) }, new List<Point>() { new Point(grid.Width-1, 5) });

            //    //    if (output == null || output.Success == false)
            //    //    {
            //    //        seed += 1000;
            //    //        failedAttempts += 1;
            //    //    }
            //    //}
            //    if (output != null)
            //        BitmapOperations.SaveBitmapToFile(
            //            ConfigurationManager.AppSettings["Output"].ToString()
            //                .Replace(".bmp", i.ToString() + ".bmp")
            //            , output.TileMap
            //        );

            //}
            Console.WriteLine("Puzzle.01 Created.  Total Failed Attampts: " + failures.ToString());
            //Console.ReadKey();
        }
    }

    public class ControlWriter : TextWriter
    {
        private TextBox textbox;
        public ControlWriter(TextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            Action action = delegate { textbox.Text += value; };
            textbox.Dispatcher.Invoke(action);
            //scroll();
        }

        public override void Write(string value)
        {
            Action action = delegate { textbox.Text += value; };
            textbox.Dispatcher.Invoke(action);
            //scroll();
        }

        public void scroll()
        {
            Action action = delegate { textbox.ScrollToEnd(); };
            textbox.Dispatcher.Invoke(action);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
