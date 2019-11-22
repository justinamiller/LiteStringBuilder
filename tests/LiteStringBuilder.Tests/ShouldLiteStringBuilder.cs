using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Diagnostics.CodeAnalysis;

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
        public void TestAppend()
        {
            var sb = StringHelper.LiteStringBuilder.Create();
            Assert.IsTrue(sb.Length == 0);
            sb.Append((string)null);
            Assert.IsTrue(sb.Length == 0);
            sb.Append(string.Empty);
            Assert.IsTrue(sb.Length == 0);


            sb.Append(new StringBuilder().Append('a', 5000).ToString());
            Assert.IsTrue(sb.Length == 5000);

            sb.Append(new StringBuilder().Append('a', 10000).ToString());
            Assert.IsTrue(sb.Length == 15000);

            sb.Append(new StringBuilder().Append('a', 1000).ToString());
            Assert.IsTrue(sb.Length == 16000);

            sb.Clear();
            Assert.IsTrue(sb.Length == 0);
            sb.Append(true);
            Assert.AreEqual(sb.ToString(), "True");

            sb.Clear();
            sb.Append(false);
            Assert.AreEqual(sb.ToString(), "False");


            sb.Clear();
            sb.Append(0);
            Assert.AreEqual(sb.ToString(), "0");

            sb.Clear();
            sb.Append(1000);
            Assert.AreEqual(sb.ToString(), "1000");

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
    }
}
