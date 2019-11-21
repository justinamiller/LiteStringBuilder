

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    //[ShortRunJob]
    [RankColumn, HtmlExporter, CsvExporter]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Arabic)]
    [RankColumn(NumeralSystem.Stars)]
    public class StringBenchmark
    {
        //private static LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
        //private static System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);

        [Benchmark]
        public  string String_Added()
        {
            string str = "PI=" + Math.PI + "_373=" + 373 + true + short.MaxValue;
            return str.Replace("373", "5428");
        }

        [Benchmark]
        public  string String_Concat()
        {
            return string.Concat("PI=", Math.PI, "_373=", 373, true, short.MaxValue).Replace("373", "5428");
        }

        [Benchmark]
        public  string StringBuilder()
        {
            System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(64);
            m_strBuilder.Length = 0;
            m_strBuilder.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Replace("373", "5428");
            return m_strBuilder.ToString();
        }

        [Benchmark]
        public  string LiteStringBuilder()
        {
            LiteStringBuilder m_strCustom = new LiteStringBuilder(64);
            m_strCustom.Clear();
            m_strCustom.Append("PI=").Append(Math.PI).Append("_373=").Append(373).Append(true).Append(short.MaxValue).Replace("373", "5428");
            return m_strCustom.ToString();
        }
    }
}

