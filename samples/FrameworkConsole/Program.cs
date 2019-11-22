using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StringHelper;

namespace FrameworkConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(1000);
 
            var fs = new LiteStringBuilder();
            fs.Append((float)((float)60.0 / (float)7.0));
            fs.Append("Hello World");
            fs.ToString();
            fs = null;
            PerfTest.RunTests();

            Console.WriteLine("DOne");
                Console.ReadLine();

            for (var a = 0; a < 50; a++)
            {
                var sw = Stopwatch.StartNew();
                fs = new LiteStringBuilder();
                for (var i = 0; i < 10000; i++)
                {
                    fs.Append("Hello World");
                    fs.Append(true);
                    fs.Append(false);
                    fs.Append(12345);
                    fs.Append(int.MinValue);
                    fs.Append(int.MaxValue);
                }
                Console.WriteLine($"Test 1: {sw.Elapsed.TotalMilliseconds}");

                sw = Stopwatch.StartNew();
                fs = new LiteStringBuilder();
                for (var i = 0; i < 10000; i++)
                {
                    fs.Append(true);
                    fs.Append(false);
                    fs.Append(int.MinValue);
                    fs.Append(int.MaxValue);
                }
                Console.WriteLine($"Test 2: {sw.Elapsed.TotalMilliseconds}");


                var sb = new StringBuilder(32);
                sb.Append("Hello World");
                sw = Stopwatch.StartNew();
                sb = new StringBuilder(32);
                for (var i = 0; i < 10000; i++)
                {
                    sb.Append("Hello World");
                    sb.Append(true);
                    sb.Append(false);
                    sb.Append(int.MinValue);
                    sb.Append(int.MaxValue);
                }
                Console.WriteLine($"Test 3: {sw.Elapsed.TotalMilliseconds}");

   
                sw = Stopwatch.StartNew();
                for (var i = 0; i < 10000; i++)
                {
                    fs.ToString();
                }
                Console.WriteLine($"Test 4: {sw.Elapsed.TotalMilliseconds}");




                sw = Stopwatch.StartNew();
                for (var i = 0; i < 10000; i++)
                {
                    sb.ToString();
                }
                Console.WriteLine($"Test 5: {sw.Elapsed.TotalMilliseconds}");

                Console.WriteLine();
            }



           // Console.WriteLine(fs.ToString());
            Console.ReadLine();
        }

        
    }
}
