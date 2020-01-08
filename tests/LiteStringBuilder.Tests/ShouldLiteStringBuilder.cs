﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using StringHelper;
using System.Collections.Generic;

namespace LiteStringBuilder.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ShouldLiteStringBuilder
    {
        [TestMethod]
        public void TestLiteStringBuilder()
        {
            var sb = new StringHelper.LiteStringBuilder();
            sb = new StringHelper.LiteStringBuilder(0);
            sb = new StringHelper.LiteStringBuilder(-32);
            sb = new StringHelper.LiteStringBuilder(null);
            Assert.AreEqual(sb.ToString(), "");
            sb = new StringHelper.LiteStringBuilder("");
            Assert.AreEqual(sb.ToString(), "");
            sb = new StringHelper.LiteStringBuilder("Hello");
            Assert.AreEqual(sb.ToString(), "Hello");
        }

        private static string GetString(char c)
        {
            return new string(c, 1000);
        }

        [TestMethod]
        public void TestCorruptStrings()
        {
            var chars = new char[char.MaxValue];
            for(var i = char.MinValue; i < chars.Length; i++)
            {
                chars[i] = i;
            }
            var str = new string(chars);
            var sb = new StringHelper.LiteStringBuilder();
            sb.Append(chars);

            Assert.AreEqual(sb.ToString(), str);

            Assert.AreEqual(sb.Length, str.Length);
            Assert.AreEqual(sb.ToString().Length, sb.Length);


            sb.Clear();
            var singleByteString = "s";
            var doubleByteString = "ß";
            var quadByteString = "𝟘";

            sb.Append(singleByteString);
            Assert.AreEqual(sb.ToString(), singleByteString);

            sb.Append(doubleByteString);
            Assert.AreEqual(sb.ToString(), singleByteString+ doubleByteString);

            sb.Append(quadByteString);
            Assert.AreEqual(sb.ToString(), singleByteString + doubleByteString+ quadByteString);
        }

        [TestMethod]
        public void TestLiteStringBuilderThreaded()
        {
            var list = new System.Collections.Concurrent.ConcurrentDictionary<int,string>();
            System.Threading.Tasks.Parallel.For(0, 1000, (i) =>
            {
                var fs = new StringHelper.LiteStringBuilder(1);
                fs.Append(GetString((char)(48 + (i % 10))));
                list.TryAdd(i, fs.ToString());
        });

            var dic = list.ToArray();
            for(var i = 0; i < 1000; i++)
            {
                var key = dic[i].Key;
                var value = dic[i].Value;
                var str = GetString((char)(48 + (key % 10)));
                if (string.Compare(str, value) != 0)
                {
                    Assert.Fail("Not thread safe");
                }
            }
        }

        [TestMethod]
        public void TestLength()
        {
            var sb = new StringHelper.LiteStringBuilder();
            Assert.IsTrue(sb.Length == 0);
            sb.Append("Hello");
            Assert.IsTrue(sb.Length == 5);
        }

        [TestMethod]
        public void TestIsEmpty()
        {
            var sb = new StringHelper.LiteStringBuilder();
            Assert.IsTrue(sb.IsEmpty());
            sb.Append("Hello");
            Assert.IsFalse(sb.IsEmpty());
        }

        [TestMethod]
        public void TestClear()
        {
            var sb = new StringHelper.LiteStringBuilder();
            Assert.IsTrue(sb.IsEmpty());
            sb.Append("Hello");
            Assert.IsFalse(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "Hello");
            sb.Clear();
            Assert.IsTrue(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "");
        }

        [TestMethod]
        public void TestCreate()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("Hello");
            Assert.IsFalse(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "Hello");
            sb = StringHelper.LiteStringBuilder.Create(32);
            sb.Append("Hello");
            Assert.IsFalse(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "Hello");
        }

        [TestMethod]
        public void TestEquals()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("Hello");

            var sb1 = StringHelper.LiteStringBuilder.Create();
            sb1.Append("Hello");

            Assert.IsTrue(sb1.Equals(sb));

            sb1 = StringHelper.LiteStringBuilder.Create();
            sb1.Append("hello");
            Assert.IsFalse(sb1.Equals(sb));


            sb1.Clear();
            sb1.Append("Hello");
            Assert.IsTrue(sb1.Equals(sb));

            Assert.IsFalse(sb1.Equals(null));

            var sb2 = sb;
            Assert.IsTrue(sb2.Equals(sb));

            sb1.Clear();
            sb1.Append("Hi");
            Assert.IsFalse(sb1.Equals(sb));

            Assert.IsFalse(sb1.Equals(new object()));
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("Hello");

            var sb1 = StringHelper.LiteStringBuilder.Create();
            sb1.Append("Hello");

            Assert.IsTrue(sb1.GetHashCode() == sb.GetHashCode());

            sb1 = StringHelper.LiteStringBuilder.Create();
            sb1.Append("hello");
            Assert.IsFalse(sb1.GetHashCode() == sb.GetHashCode());


            sb1.Clear();
            sb1.Append("Hello");
            Assert.IsTrue(sb1.GetHashCode() == sb.GetHashCode());


            var sb2 = sb;
            Assert.IsTrue(sb2.GetHashCode() == sb.GetHashCode());

            sb1.Clear();
            sb1.Append("Hi");
            Assert.IsFalse(sb1.GetHashCode() == sb.GetHashCode());

        }

        [TestMethod]
        public void TestIndex()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("abcd");
            try
            {
                var c = sb[5];
                Assert.Fail("Should have exception");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }

            //try
            //{
            //    sb[5] = 'c';
            //    Assert.Fail("Should have exception");
            //}
            //catch (Exception)
            //{
            //    Assert.IsTrue(true);
            //}


            Assert.AreEqual(sb[1], 'b');
            //sb[1] = 'z';
            //Assert.AreEqual(sb[1], 'z');
        }

        [TestMethod]
        public void TestAppendT()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append<int>(null);
            Assert.AreEqual(sb.Length, 0);
            sb.Append<string>("", null, "1");
            Assert.AreEqual(sb.Length, 1);
            sb.Clear();
            sb.Append<int>(1, 2, 3, 4, 5);
            Assert.AreEqual(sb.Length, 5);
            sb.Clear();

            sb.Append<int>(1);
            Assert.AreEqual(sb.Length, 1);
        }

        [TestMethod]
        public void TestAppendObjects()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("Test", 'c', true, double.MaxValue, long.MaxValue,ulong.MaxValue,uint.MaxValue,null, int.MaxValue, short.MaxValue, decimal.MaxValue, double.MaxValue, float.MaxValue, DateTime.MinValue, sbyte.MinValue, byte.MinValue, new char[2] { 'c', 'b' });
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestAppend()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            Assert.IsTrue(sb.Length == 0);
            sb.Append((string)null);
            Assert.IsTrue(sb.Length == 0);
            sb.Append(string.Empty);
            Assert.IsTrue(sb.Length == 0);

            sb.Append((char[])null);
            Assert.IsTrue(sb.Length == 0);

            sb.Clear();
            Assert.IsTrue(sb.Length == 0);
            sb.Append(true);
            Assert.AreEqual(sb.ToString(), "True");

            sb.Clear();
            sb.Append(false);
            Assert.AreEqual(sb.ToString(), "False");


            sb.Clear();
            sb.Append("a");
            Assert.AreEqual(sb.ToString(), "a");

            sb.Clear();
            sb.Append("ab");
            Assert.AreEqual(sb.ToString(), "ab");


            sb.Clear();
            sb.Append("abc");
            Assert.AreEqual(sb.ToString(), "abc");

            sb.Clear();
            sb.Append("abcd");
            Assert.AreEqual(sb.ToString(), "abcd");

            sb.Clear();
            sb.Append(0);
            Assert.AreEqual(sb.ToString(), "0");

            sb.Clear();
            sb.Append(Convert.ToByte('a'));
            Assert.AreEqual(sb.ToString(), Convert.ToByte('a').ToString());

            sb.Clear();
            sb.Append((sbyte)Convert.ToByte('a'));
            Assert.AreEqual(sb.ToString(), ((sbyte)Convert.ToByte('a')).ToString());

            var dt = DateTime.Now;
            sb.Clear();
            sb.Append(dt);
            Assert.AreEqual(sb.ToString(), dt.ToString());

            sb.Clear();
            sb.Append((float)1.1234);
            Assert.AreEqual(sb.ToString(), "1.1234");

            sb.Clear();
            sb.Append(1000);
            Assert.AreEqual(sb.ToString(), "1000");

            sb.Clear();
            sb.Append(-1000);
            Assert.AreEqual(sb.ToString(), "-1000");

            sb.Clear();
            sb.Append((decimal)0);
            Assert.AreEqual(sb.ToString(), "0");

            sb.Clear();
            sb.Append((decimal)1000);
            Assert.AreEqual(sb.ToString(), "1000");

            sb.Clear();
            sb.Append((decimal)-1000.123);
            Assert.AreEqual(sb.ToString(), "-1000.123");


            sb.Clear();
            sb.Append((decimal)100000.123456789);
            Assert.AreEqual(sb.ToString(), "100000.123456789");

            sb.Clear();
            sb.Append((double)100000.123456789);
            Assert.AreEqual(sb.ToString(), "100000.123456789");

            sb.Clear();
            sb.Append((double)-123412342.123);
            Assert.AreEqual(sb.ToString(), "-123412342.123");


            sb.Clear();
            sb.Append((long)0);
            Assert.AreEqual(sb.ToString(), "0");

            sb.Clear();
            sb.Append((long)1000);
            Assert.AreEqual(sb.ToString(), "1000");
            sb.Clear();
            sb.Append((long)-1000);
            Assert.AreEqual(sb.ToString(), "-1000");

            sb.Clear();
            sb.Append((short)1000);
            Assert.AreEqual(sb.ToString(), "1000");

            sb.Clear();
            sb.Append((ulong)1000);
            Assert.AreEqual(sb.ToString(), "1000");

            sb.Clear();
            sb.Append((uint)1000);
            Assert.AreEqual(sb.ToString(), "1000");

            sb.Clear();
            sb.Append((object)"Hello");
            Assert.AreEqual(sb.ToString(), "Hello");

            sb.Clear();
            sb.Append((object)null);
            Assert.AreEqual(sb.ToString(), "");

            sb.Clear();
            sb.Append(new char[] { 'h', 'i' });
            Assert.AreEqual(sb.ToString(), "hi");

            sb = StringHelper.LiteStringBuilder.Create();
            for(var i = 0; i < 1000; i++)
            {
                sb.Append('a');
            }
            Assert.IsTrue(sb.Length == 1000);
        }

        [TestMethod]
        public void TestAppendline()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            Assert.IsTrue(sb.Length == 0);
            sb.AppendLine();
            Assert.IsTrue(sb.Length == Environment.NewLine.Length);

            sb.Clear();
            sb.AppendLine("Hi");
            Assert.IsTrue(sb.Length == Environment.NewLine.Length + 2);

            sb.Clear();
            sb.AppendLine("");
            Assert.IsTrue(sb.Length == Environment.NewLine.Length);

            sb.Clear();
            sb.AppendLine(null);
            Assert.IsTrue(sb.Length == Environment.NewLine.Length);
        }

        [TestMethod]
        public void TestReplace()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("ABCabcABCdefgABC");
            sb.Replace("ABC", "123");
            Assert.AreEqual(sb.ToString(), "123abc123defg123");

            sb.Clear();
            sb.Append("ABCabcABCdefgABC");
            sb.Replace("A", "123");
            Assert.AreEqual(sb.ToString(), "123BCabc123BCdefg123BC");

            sb.Replace("1", "one");
            Assert.AreEqual(sb.ToString(), "one23BCabcone23BCdefgone23BC");

            sb.Replace("2", "");
            Assert.AreEqual(sb.ToString(), "one3BCabcone3BCdefgone3BC");

            sb.Replace("3", "4");
            Assert.AreEqual(sb.ToString(), "one4BCabcone4BCdefgone4BC");


            sb.Replace("one", "1");
            Assert.AreEqual(sb.ToString(), "14BCabc14BCdefg14BC");

            sb.Replace("1", null);
            Assert.AreEqual(sb.ToString(), "4BCabc4BCdefg4BC");


            sb.Clear();
            Assert.AreEqual(sb.Replace(null,"HI"), sb);

            Assert.AreEqual(sb.Replace("", "HI"), sb);

            Assert.AreEqual(sb.Replace("", null), sb);

            Assert.AreEqual(sb.Replace(null, null), sb);

            sb.Clear();
            sb.Append("abc");
            sb.Replace("", "justin");
            Assert.AreEqual(sb.ToString(), "abc");

            sb.Replace(null, "justin");
            Assert.AreEqual(sb.ToString(), "abc");

            sb.Replace("z", "justin");
            Assert.AreEqual(sb.ToString(), "abc");

            sb.Replace("z", "");
            Assert.AreEqual(sb.ToString(), "abc");

            sb.Replace("abc", "");
            Assert.AreEqual(sb.ToString(), "");

            sb.Replace("abc", null);
            Assert.AreEqual(sb.ToString(), "");
        }

        [TestMethod]
        public void TestSet()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            sb.Append("Hello World");
            sb.Set("Hi");
            Assert.IsFalse(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "Hi");


            sb.Set("123", "45");
            Assert.IsFalse(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "12345");

            sb.Set(null, "",123);
            Assert.IsFalse(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "123");

            sb.Set(null, "", null);
            Assert.IsTrue(sb.IsEmpty());
            Assert.AreEqual(sb.ToString(), "");
        }

        private static ulong Power(ulong value, int power)
        {
            ulong val = 0;
          for(int i = 0; i < power; i++)
            {
                val = val * value; 
            }
            return val;
        }


        [TestMethod]
        public void TestGetIntLength()
        {
            for(int i = 0; i < ulong.MaxValue.ToString("###0").Length; i++)
            {
                int a = Utilities.GetIntLength((ulong)Math.Pow(10, i));
                int b= Math.Pow(10, i).ToString("###0").Length;

                Assert.AreEqual(a, b);
            }
         
        }
    }
}
