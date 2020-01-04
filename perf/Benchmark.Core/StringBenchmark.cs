using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using StringHelper;

namespace Benchmark.Core
{
    [RankColumn, HtmlExporter, CsvExporter]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class StringBenchmark
    {


        #region Normal
        [Benchmark]
        public string String_Interpolated()
        {
            string str = $"PI= { Math.PI} _373= { 373 } {true} {short.MaxValue}{'z'}";
            return str.Replace("373", "5428");
        }


        [Benchmark]
        public string String_Added()
        {
            string str = "PI=" + Math.PI + "_373=" + 373.ToString() + true.ToString() + short.MaxValue.ToString() + 'z';
            return str.Replace("373", "5428").Replace("St Paul", "HOT");
        }

        [Benchmark]
        public string String_Concat()
        {
            string str = string.Concat("PI=", Math.PI, "_373=", 373, true, short.MaxValue, 'z');
            return str.Replace("373", "5428").Replace("St Paul", "HOT");
        }


        [Benchmark]
        public string StringBuilder()
        {
            System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
            return m_strBuilder.ToString();
        }
        [Benchmark]
        public string LiteStringBuilder()
        {
            var m_strBuilder = new LiteStringBuilder(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
            return m_strBuilder.ToString();
        }

        [Benchmark]
        public string LiteStringBuilder13()
        {
            var m_strBuilder = new LiteStringBuilder13(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
            return m_strBuilder.ToString();
        }

      //  [Benchmark]
        //public string LiteStringBuilder13_replace2()
        //{
        //    var m_strBuilder = new LiteStringBuilder13(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace2("373", "5428").Replace2("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        //[Benchmark]
        //public string LiteStringBuilder13_replace3()
        //{
        //    var m_strBuilder = new LiteStringBuilder13(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace3("373", "5428").Replace3("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        //[Benchmark]
        //public string LiteStringBuilder13_replace5()
        //{
        //    var m_strBuilder = new LiteStringBuilder13(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace5("373", "5428").Replace5("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        //[Benchmark]
        //public string LiteStringBuilder13_replaceREAL()
        //{
        //    var m_strBuilder = new LiteStringBuilder13(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace_REAL("PI","").Replace_REAL("373", "5428").Replace_REAL("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        [Benchmark]
        public string LiteStringBuilder13_replaceSLOW()
        {
            var m_strBuilder = new LiteStringBuilder13(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace_Slow("373", "5428").Replace_Slow("St Paul", "HOT");
            return m_strBuilder.ToString();
        }

        [Benchmark]
        public string LiteStringBuilder13_Replace_Try()
        {
            var m_strBuilder = new LiteStringBuilder13(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace_Try("373", "5428").Replace_Try("St Paul", "HOT");
            return m_strBuilder.ToString();
        }

        [Benchmark]
        public string LiteStringBuilder13_Replace_Trycache()
        {
            var m_strBuilder = new LiteStringBuilder13(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace_TryCache("373", "5428").Replace_TryCache("St Paul", "HOT");
            return m_strBuilder.ToString();
        }


        [Benchmark]
        public string LiteStringBuilder13_Replace_Replace_TryCacheRework()
        {
            var m_strBuilder = new LiteStringBuilder13(1);
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace_TryCacheRework("373", "5428").Replace_TryCacheRework("St Paul", "HOT");
            return m_strBuilder.ToString();
        }
        


        //[Benchmark]
        //public string LiteStringBuilder13_Base()
        //{
        //    var m_strBuilder = new LiteStringBuilder13(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace4("373", "5428").Replace4("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string LiteStringBuilder7()
        //{
        //    var m_strBuilder = new LiteStringBuilder7(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        //[Benchmark]
        //public string LiteStringBuilder8()
        //{
        //    var m_strBuilder = new LiteStringBuilder8(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        //[Benchmark]
        //public string LiteStringBuilder9()
        //{
        //    var m_strBuilder = new LiteStringBuilder9(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}

        //[Benchmark]
        //public string LiteStringBuilder10()
        //{
        //    var m_strBuilder = new LiteStringBuilder10(1);
        //    m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}


        #endregion

        //#region BIGString

        //private readonly static string str1 = new string('a', 1234);
        //private readonly static string str2 = new string('b', 32);
        //private readonly static string str3 = new string('c', 2341);
        //private readonly static string str4 = new string('d', 5000);

        ////[Benchmark]
        ////public string Large_String_Interpolated()
        ////{
        ////    string str = $"{str1} {str2}{str3}{str4}";
        ////    return str.Replace("c", "z");
        ////}

        ////[Benchmark]
        ////public string Large_String_Added()
        ////{
        ////    string str = str1 + str2 + str3 + str4;
        ////    return str.Replace("c", "z");
        ////}

        ////[Benchmark]
        ////public string Large_String_Concat()
        ////{
        ////    string str = string.Concat(str1, str2, str3, str4);
        ////    return str.Replace("c", "z");
        ////}


        //[Benchmark]
        //public string Large_StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
        //    //m_strBuilder.Length = 0;
        //    m_strBuilder.Append(str1).Append(str2).Append(str3).Append(str4);
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string Large_LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        //    return m_strCustom.ToString();
        //}

        ////[Benchmark]
        ////public string Large_LiteStringBuilder6()
        ////{
        ////    var m_strCustom = new LiteStringBuilder6(1);
        ////    // m_strCustom.Clear();
        ////    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        ////    return m_strCustom.ToString();
        ////}

        ////[Benchmark]
        ////public string Large_LiteStringBuilder7()
        ////{
        ////    var m_strCustom = new LiteStringBuilder7(1);
        ////    // m_strCustom.Clear();
        ////    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        ////    return m_strCustom.ToString();
        ////}

        ////[Benchmark]
        ////public string Large_LiteStringBuilder8()
        ////{
        ////    var m_strCustom = new LiteStringBuilder8(1);
        ////    // m_strCustom.Clear();
        ////    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        ////    return m_strCustom.ToString();
        ////}

        ////[Benchmark]
        ////public string Large_LiteStringBuilder9()
        ////{
        ////    var m_strCustom = new LiteStringBuilder9(1);
        ////    // m_strCustom.Clear();
        ////    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        ////    return "";
        ////    return m_strCustom.ToString();
        ////}

        //[Benchmark]
        //public string Large_LiteStringBuilder10()
        //{
        //    var m_strCustom = new LiteStringBuilder10(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        //    return m_strCustom.ToString();
        //}

        //[Benchmark]
        //public string Large_LiteStringBuilder13()
        //{
        //    var m_strCustom = new LiteStringBuilder13(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        //    return m_strCustom.ToString();
        //}

        //[Benchmark]
        //public string Large_LiteStringBuilder2()
        //{
        //    var m_strCustom = new LiteStringBuilder2(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4);
        //    return m_strCustom.ToString();

        //}
        //#endregion

        //#region PrimativeTypes


        //[Benchmark]
        //public string Primative_String_Interpolated()
        //{
        //    string str = $"{char.MaxValue} {Int16.MaxValue}{Int32.MaxValue}{Int64.MaxValue}{DateTime.MaxValue}{double.MaxValue}{decimal.MaxValue}{float.MaxValue}{true}{byte.MaxValue}{sbyte.MaxValue} HELLOWORLD";
        //    return str;
        //}

        //[Benchmark]
        //public string Primative_String_Added()
        //{
        //    string str = char.MaxValue + Int16.MaxValue.ToString() + Int32.MaxValue.ToString() + Int64.MaxValue.ToString() + DateTime.MaxValue.ToString() + double.MaxValue.ToString() + decimal.MaxValue.ToString() + float.MaxValue.ToString() + true.ToString() + byte.MaxValue.ToString() + sbyte.MaxValue.ToString() + "HELLOWORLD";
        //    return str;
        //}

        //[Benchmark]
        //public string Primative_String_Concat()
        //{
        //    string str = string.Concat(char.MaxValue, Int16.MaxValue, Int32.MaxValue, Int64.MaxValue, DateTime.MaxValue, double.MaxValue, float.MaxValue, true, byte.MaxValue, sbyte.MaxValue,"HELLOWORLD");
        //    return str.Replace("c", "z");
        //}


        //[Benchmark]
        //public string Primative_StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(16);
        //    //m_strBuilder.Length = 0;
        //    m_strBuilder.Append(char.MaxValue).Append(Int16.MaxValue).Append(Int32.MaxValue).Append(Int64.MaxValue).Append(DateTime.MaxValue).Append(double.MaxValue).Append(float.MaxValue).Append(true).Append(byte.MaxValue).Append(sbyte.MaxValue).Append("HELLOWORLD");
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string Primative_LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(16);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(char.MaxValue).Append(Int16.MaxValue).Append(Int32.MaxValue).Append(Int64.MaxValue).Append(DateTime.MaxValue).Append(double.MaxValue).Append(float.MaxValue).Append(true).Append(byte.MaxValue).Append(sbyte.MaxValue).Append("HELLOWORLD");
        //    return m_strCustom.ToString();
        //}

        //[Benchmark]
        //public string Primative_LiteStringBuilderStacks()
        //{
        //    LiteStringBuilderStack m_strCustom = new LiteStringBuilderStack(16);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(char.MaxValue).Append(Int16.MaxValue).Append(Int32.MaxValue).Append(Int64.MaxValue).Append(DateTime.MaxValue).Append(double.MaxValue).Append(float.MaxValue).Append(true).Append(byte.MaxValue).Append(sbyte.MaxValue).Append("HELLOWORLD");
        //    return m_strCustom.ToString();
        //}

        //[Benchmark]
        //public string Primative_LiteStringBuilder2()
        //{
        //    LiteStringBuilder2 m_strCustom = new LiteStringBuilder2(16);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(char.MaxValue).Append(Int16.MaxValue).Append(Int32.MaxValue).Append(Int64.MaxValue).Append(DateTime.MaxValue).Append(double.MaxValue).Append(float.MaxValue).Append(true).Append(byte.MaxValue).Append(sbyte.MaxValue).Append("HELLOWORLD");
        //    return m_strCustom.ToString();
        //}

        //#endregion 

        //#region BIGArray

        //private readonly static char[] _bigArray1;
        //private readonly static char[] _bigArray2;
        //private readonly static char[] _bigArray3;



        //static StringBenchmark()
        //{


        //    _bigArray1 = new char[char.MaxValue];
        //    _bigArray2 = new char[char.MaxValue];
        //    _bigArray3 = new char[char.MaxValue];
        //    for (var i = 0; i < char.MaxValue; i++)
        //    {
        //        _bigArray1[i] = (char)i;
        //        _bigArray2[i] = (char)i;
        //        _bigArray3[i] = (char)i;
        //    }
        //}


        //[Benchmark]
        //public string LargeArray_StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
        //    //m_strBuilder.Length = 0;
        //    m_strBuilder.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string LargeArray_LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
        //    return m_strCustom.ToString();
        //}

        //[Benchmark]
        //public string LargeArray_LiteStringBuilder10()
        //{
        //    var m_strCustom = new LiteStringBuilder10(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
        //    return m_strCustom.ToString();
        //}

        //[Benchmark]
        //public string LargeArray_LiteStringBuilder13()
        //{
        //    var m_strCustom = new LiteStringBuilder13(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
        //    return m_strCustom.ToString();
        //}


        ////[Benchmark]
        ////public string LargeArray_LiteStringBuilderStack()
        ////{
        ////    LiteStringBuilderStack m_strCustom = new LiteStringBuilderStack(1);
        ////    // m_strCustom.Clear();
        ////    m_strCustom.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
        ////    return m_strCustom.ToString();
        ////}


        ////[Benchmark]
        ////public string LargeArray_LiteStringBuilder2()
        ////{
        ////    LiteStringBuilder2 m_strCustom = new LiteStringBuilder2(1);
        ////    // m_strCustom.Clear();
        ////    m_strCustom.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);
        ////    return m_strCustom.ToString();
        ////}
        //#endregion


        //[Benchmark]
        //public string LargeArray_StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
        //    //m_strBuilder.Length = 0;
        //    m_strBuilder.Append(_bigArray1);
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string LargeArray_LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(_bigArray1);
        //    return m_strCustom.ToString();
        //}
    }
}
