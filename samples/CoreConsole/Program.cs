using System;
using System.Diagnostics;
using System.Text;

namespace CoreConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var fs = new LiteStringBuilder();
            fs.Append("Hello World");
          

            for (var a = 0; a < 50; a++)
            {
                var sw = Stopwatch.StartNew();
                fs = new LiteStringBuilder();
                for (var i = 0; i < 10000; i++)
                {
                    fs.Append("Hello World");
                }
                Console.WriteLine($"Test 1: {sw.Elapsed.TotalMilliseconds}");

                //sw = Stopwatch.StartNew();
                //fs = new LiteStringBuilder();
                //for (var i = 0; i < 10000; i++)
                //{
                //    fs.AppendFast("Hello World");
                //}
                //Console.WriteLine($"Test 2: {sw.Elapsed.TotalMilliseconds}");

                var sb = new StringBuilder(32);
                sb.Append("Hello World");
                sw = Stopwatch.StartNew();
                sb = new StringBuilder(32);
                for (var i = 0; i < 10000; i++)
                {
                    sb.Append("Hello World");
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
