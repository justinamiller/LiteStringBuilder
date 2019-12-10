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

        private readonly static char[] _bigArray1;
        private readonly static char[] _bigArray2;
        private readonly static char[] _bigArray3;

        static  Program()
        {
            _bigArray1 = new char[char.MaxValue];
            _bigArray2 = new char[char.MaxValue];
            _bigArray3 = new char[char.MaxValue];
            for (var i = 0; i < char.MaxValue; i++)
            {
                _bigArray1[i] = (char)i;
                _bigArray2[i] = (char)i;
                _bigArray3[i] = (char)i;
            }
        }


        public static string LargeArray_StringBuilder()
        {
            System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
            //m_strBuilder.Length = 0;
            m_strBuilder.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
            return m_strBuilder.ToString();
        }


        public static string LargeArray_LiteStringBuilder()
        {
            LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
            // m_strCustom.Clear();
            m_strCustom.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
            return m_strCustom.ToString();
        }

        static void Main(string[] args)
        {
            for (var i = 0; i < 10000; i++)
            {
                LargeArray_StringBuilder();
                LargeArray_LiteStringBuilder();
            }
            return;

            var fs = new LiteStringBuilder(1);
            //fs.Append(new string('a', 100));
            //fs.Append((float)((float)60.0 / (float)7.0));
            //fs.Append("Hello World");
            //fs.ToString();

            var l = new List<char>();
            for(var i = 0; i < char.MaxValue; i++)
            {
                l.Add((char)i);
            }
            fs.Append(l.ToArray());
            fs.Append(l.ToArray());
            fs.Append(l.ToArray());
            fs.Append(l.ToArray());
            fs.Append(l.ToArray());
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
