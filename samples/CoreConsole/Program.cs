using StringHelper;
using System;
using System.Diagnostics;
using System.Text;

namespace CoreConsole
{
    class Program
    {

        #region BIGString

        private readonly static string str1 = new string('a', 1000);
        private readonly static string str2 = new string('b', 2000);
        private readonly static string str3 = new string('c', 1500);
        private readonly static string str4 = new string('d', 4000);

        public static string Large_LiteStringBuilder10()
        {
            var m_strCustom = new LiteStringBuilder10(1);
            // m_strCustom.Clear();
            m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
            return m_strCustom.ToString();
        }
        #endregion

        static Program()
        {
            new LiteStringBuilder10(1);
        }
        static void Main(string[] args)
        {
            for(var i=0;i<100;i++)
                Large_LiteStringBuilder10();

            return;
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
