using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkConsole
{
    class PerfTest
    {
        private static LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
        private static LiteStringBuilder m_strCustom1 = new LiteStringBuilder(64);
        private static LiteStringBuilder m_strCustom2 = new LiteStringBuilder(64);
        private static System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);
        private delegate string Test();

        private static string String_Added()
        {
            string str = "PI=" + Math.PI + "_373=" + 373 + true + short.MaxValue;
            return str.Replace("373", "5428");
        }
        private static string String_Concat()
        {
            return string.Concat("PI=", Math.PI, "_373=", 373, true, short.MaxValue).Replace("373", "5428");
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



        private static Stopwatch _sw = Stopwatch.StartNew();
        private static void RunTest(string testName, Test test)
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForFullGCComplete();
            var current = GC.GetTotalMemory(true);
            _sw.Restart();
            string lastResult = null;
            for (int i = 0; i < 1000; i++)
                lastResult = test();
            _sw.Stop();
            var now = GC.GetTotalMemory(false);

            Console.WriteLine($"{ test.Method.Name}  Time: { _sw.Elapsed.TotalMilliseconds} Bytes:  {(now - current).ToString("#,##0") } Result: {lastResult} ");
            //Debug.Log( "Check test result: test=" + testName + " result='" + lastResult + "' (" + lastResult.Length + ")" );
        }

        public static void RunTests()
        {
            //warm up;
            String_Added();
            String_Concat();
            StringBuilder();
            CustomCString();
            //     CustomCStringFast();
            GC.Collect(GC.MaxGeneration);
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
