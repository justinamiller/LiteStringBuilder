

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using StringHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    [RankColumn, HtmlExporter, CsvExporter]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class StringBenchmark
    {
        private DateTime _dt = DateTime.Now;

        //static StringBenchmark()
        //{
        //    var sb = new StringBuilder();
        //    sb.Append("test");
        //    var sb1 = new LiteStringBuilder();
        //    sb1.Append("test");
        //    sb.ToString();
        //    sb1.ToString();
        //}


        //#region Normal
        //[Benchmark]
        //public string StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
        //    m_strBuilder.Append("PI=").Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
        //    m_strCustom.Append("PI=").Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
        //    return m_strCustom.ToString();
        //}

        //#endregion
        #region Normal
        //[Benchmark]
        //public string String_Interpolated()
        //{
        //    string str = $"PI= { Math.PI} _373= { 373 } {true} {short.MaxValue}{'z'}";
        //    return str.Replace("373", "5428");
        //}


        //[Benchmark]
        //public string String_Added()
        //{
        //    string str = "PI=" + Math.PI + "_373=" + 373.ToString() + true.ToString() + short.MaxValue.ToString() + 'z';
        //    return str.Replace("373", "5428").Replace("St Paul", "HOT");
        //}

        //[Benchmark]
        //public string String_Concat()
        //{
        //    string str = string.Concat("PI=", Math.PI, "_373=", 373, true, short.MaxValue, 'z');
        //    return str.Replace("373", "5428").Replace("St Paul", "HOT");
        //}


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
            LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
            m_strCustom.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append('z').Replace("373", "5428").Replace("St Paul", "HOT");
            return m_strCustom.ToString();
        }

        #endregion

        //#region BIGString

        //private readonly static string str1 = new string('a', 1000);
        //private readonly static string str2 = new string('b', 1000);
        //private readonly static string str3 = new string('c', 1000);
        //private readonly static string str4 = new string('d', 1000);

        //[Benchmark]
        //public string Large_String_Interpolated()
        //{
        //    string str = $"{str1} {str2}{str3}{str4}";
        //    return str.Replace("c", "z");
        //}

        //[Benchmark]
        //public string Large_String_Added()
        //{
        //    string str = str1 + str2 + str3 + str4;
        //    return str.Replace("c", "z");
        //}

        //[Benchmark]
        //public string Large_String_Concat()
        //{
        //    string str = string.Concat(str1, str2, str3, str4);
        //    return str.Replace("c", "z");
        //}


        //[Benchmark]
        //public string Large_StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);
        //    //m_strBuilder.Length = 0;
        //    m_strBuilder.Append(str1).Append(str2).Append(str3).Append(str4).Replace("c", "z");
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string Large_LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4).Replace("c", "z");
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


        //#endregion

        //#region PrimativeTypes


        //[Benchmark]
        //public string Primative_String_Interpolated()
        //{
        //    string str = $"{char.MaxValue} {Int16.MaxValue}{Int32.MaxValue}{Int64.MaxValue}{DateTime.MaxValue}{double.MaxValue}{decimal.MaxValue}{float.MaxValue}{true}{byte.MaxValue}{sbyte.MaxValue}";
        //    return str;
        //}

        //[Benchmark]
        //public string Primative_String_Added()
        //{
        //    string str = char.MaxValue + Int16.MaxValue.ToString() + Int32.MaxValue.ToString() + Int64.MaxValue.ToString() + DateTime.MaxValue.ToString() + double.MaxValue.ToString() + decimal.MaxValue.ToString() + float.MaxValue.ToString() + true.ToString() + byte.MaxValue.ToString() + sbyte.MaxValue.ToString();
        //    return str;
        //}

        //[Benchmark]
        //public string Primative_String_Concat()
        //{
        //    string str = string.Concat(char.MaxValue, Int16.MaxValue, Int32.MaxValue, Int64.MaxValue, DateTime.MaxValue, double.MaxValue, float.MaxValue, true, byte.MaxValue, sbyte.MaxValue);
        //    return str.Replace("c", "z");
        //}


        //[Benchmark]
        //public string Primative_StringBuilder()
        //{
        //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);
        //    //m_strBuilder.Length = 0;
        //    m_strBuilder.Append(char.MaxValue).Append(Int16.MaxValue).Append(Int32.MaxValue).Append(Int64.MaxValue).Append(DateTime.MaxValue).Append(double.MaxValue).Append(float.MaxValue).Append(true).Append(byte.MaxValue).Append(sbyte.MaxValue);
        //    return m_strBuilder.ToString();
        //}


        //[Benchmark]
        //public string Primative_LiteStringBuilder()
        //{
        //    LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
        //    // m_strCustom.Clear();
        //    m_strCustom.Append(char.MaxValue).Append(Int16.MaxValue).Append(Int32.MaxValue).Append(Int64.MaxValue).Append(DateTime.MaxValue).Append(double.MaxValue).Append(float.MaxValue).Append(true).Append(byte.MaxValue).Append(sbyte.MaxValue);
        //    return m_strCustom.ToString();
        //}


        //#endregion 


    }
}

