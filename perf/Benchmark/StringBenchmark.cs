

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


        #region Normal
        [Benchmark]
        public string String_Interpolated()
        {
            string str = $"PI= { Math.PI} _373= { 373 } {true} {short.MaxValue} {_dt}";
            return str.Replace("373", "5428");
        }


        [Benchmark]
        public string String_Added()
        {
            string str = "PI=" + Math.PI + "_373=" + 373 + true + short.MaxValue + _dt;
            return str.Replace("373", "5428");
        }

        [Benchmark]
        public string String_Concat()
        {
            string str = string.Concat("PI=", Math.PI, "_373=", 373, true, short.MaxValue, _dt);
            return str.Replace("373", "5428");
        }


        [Benchmark]
        public string StringBuilder()
        {
            System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);
            //m_strBuilder.Length = 0;
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append(_dt).Replace("373", "5428");
            return m_strBuilder.ToString();
        }


        [Benchmark]
        public string LiteStringBuilder()
        {
            LiteStringBuilder m_strCustom = new LiteStringBuilder(1);
            // m_strCustom.Clear();
            m_strCustom.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Append(_dt).Replace("373", "5428");
            return m_strCustom.ToString();
        }
        #endregion

        #region BIGString

        private readonly static string str1 = new string('a', 1000);
        private readonly static string str2 = new string('b', 1000);
        private readonly static string str3 = new string('c', 1000);
        private readonly static string str4 = new string('d', 1000);

        [Benchmark]
        public string Large_String_Interpolated()
        {
            string str = $"{str1} {str2}{str3}{str4}";
            return str.Replace("c", "z");
        }

        [Benchmark]
        public string Large_String_Added()
        {
            string str = str1 + str2 + str3 + str4;
            return str.Replace("c", "z");
        }

        [Benchmark]
        public string Large_String_Concat()
        {
            string str = string.Concat(str1, str2, str3, str4);
            return str.Replace("c", "z");
        }


        [Benchmark]
        public string Large_StringBuilder()
        {
            System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);
            //m_strBuilder.Length = 0;
            m_strBuilder.Append(str1).Append(str2).Append(str3).Append(str4).Replace("c", "z");
            return m_strBuilder.ToString();
        }


        [Benchmark]
        public string Large_LiteStringBuilder()
        {
            LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
            // m_strCustom.Clear();
            m_strCustom.Append(str1).Append(str2).Append(str3).Append(str4).Replace("c", "z");
            return m_strCustom.ToString();
        }

        #endregion
    }
}

