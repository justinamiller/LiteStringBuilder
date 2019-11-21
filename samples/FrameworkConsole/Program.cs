using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
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
            RunTests();

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

        private static LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
        private static LiteStringBuilder m_strCustom1 = new LiteStringBuilder(64);
        private static LiteStringBuilder m_strCustom2 = new LiteStringBuilder(64);
        private static System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);
        private  delegate string Test();

        private static string String_Added()
        {
            string str = "PI=" + Math.PI + "_373=" + 373 + true + short.MaxValue;
            return str.Replace("373", "5428");
        }
        private static string String_Concat()
        {
            return string.Concat("PI=", Math.PI, "_373=", 373,true,short.MaxValue).Replace("373", "5428");
        }
        private static string StringBuilder()
        {
            m_strBuilder.Length = 0;
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Replace("373", "5428").Append((float)1.23);
            return m_strBuilder.ToString();
        }
        private static string CustomCString()
        {
            m_strCustom.Clear();
            m_strCustom.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Replace("373", "5428").Append(float.MaxValue);
            return m_strCustom.ToString();
        }

        //private static string CustomCStringFast()
        //{
        //    m_strCustom1.Clear();
        //    m_strCustom1.AppendFast("PI=").Append(Math.PI).AppendFast("_373=").Append(373).Append(true).Append(short.MaxValue).Replace("373", "5428").Append(float.MaxValue);
        //    return m_strCustom1.ToString();
        //}

        //private static string CustomCStringFastFloat()
        //{
        //    m_strCustom2.Clear();
        //    m_strCustom2.AppendFast("PI=").Append(Math.PI).AppendFast("_373=").Append(373).Append(true).Append(short.MaxValue).Replace("373", "5428").AppendFast(float.MaxValue);
        //    return m_strCustom2.ToString();
        //}

        //private static string CustomCString1()
        //{
        //    m_strCustom.Clear();
        //    m_strCustom.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Replace("373", "5428");
        //    return m_strCustom.ToString1();
        //}

        //private static string CustomCStringFast1()
        //{
        //    m_strCustom.Clear();
        //    m_strCustom.AppendFast("PI=").Append(Math.PI).AppendFast("_373=").Append(373).Append(true).Replace("373", "5428");
        //    return m_strCustom.ToString1();
        //}

        //private static string CustomCString2()
        //{
        //    m_strCustom.Clear();
        //    m_strCustom.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Replace("373", "5428");
        //    return m_strCustom.ToString2();
        //}

        //private static string CustomCStringFast2()
        //{
        //    m_strCustom.Clear();
        //    m_strCustom.AppendFast("PI=").Append(Math.PI).AppendFast("_373=").Append(373).Append(true).Replace("373", "5428");
        //    return m_strCustom.ToString2();
        //}

        private static Stopwatch _sw = Stopwatch.StartNew();
        private static void RunTest(string testName, Test test)
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForFullGCComplete();
          var current=  GC.GetTotalMemory(true);
            _sw.Restart();
            string lastResult = null;
            for (int i = 0; i < 1000; i++)
                lastResult = test();
            _sw.Stop();
            var now = GC.GetTotalMemory(false);

            Console.WriteLine($"{ test.Method.Name}  Time: { _sw.Elapsed.TotalMilliseconds} Bytes:  {(now-current).ToString("#,##0") } Result: {lastResult} ");
            //Debug.Log( "Check test result: test=" + testName + " result='" + lastResult + "' (" + lastResult.Length + ")" );
        }

        private static void RunTests()
        {
            //warm up;
            String_Added();
            String_Concat();
            StringBuilder();
            CustomCString();
       //     CustomCStringFast();

            //  Debug.Log("=================");
            Console.WriteLine("RUnTEST");
            RunTest("Test #1: string (+)       ", String_Added);
            RunTest("Test #2: string (.concat) ", String_Concat);
            RunTest("Test #3: StringBuilder    ", StringBuilder);
            RunTest("Test #4: custom StringFast", CustomCString);
      //      RunTest("Test #5: custom StringFastv2", CustomCStringFast);


            //RunTest("Test #6: custom StringFastReplace1", CustomCStringFastFloat);

            Console.ReadLine();
            //RunTest("Test #7: custom StringFast1v2", CustomCStringFast1);

            //RunTest("Test #8: custom StringFast2", CustomCString2);
            //RunTest("Test #9: custom StringFast2v2", CustomCStringFast2);
        }
    }
}
