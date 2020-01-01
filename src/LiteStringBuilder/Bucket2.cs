using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringHelper
{
    internal  sealed class Bucket2
    {
        public char[] Buffer;
        public int Length;
        public int Offset;
        public Bucket2(char[] buffer, int length, int offset)
        {
            this.Buffer = buffer;
            this.Length = length;
            this.Offset = offset;
        }

        public Bucket2(Bucket2 Bucket2, int offset)
        {
            this.Buffer = Bucket2.Buffer;
            this.Length = Bucket2.Length;
            this.Offset = offset;
        }

        public Span<char> Span
        {
            get
            {
                return new Span<char>(this.Buffer, 0, this.Length);
            }
        }

        public override string ToString()
        {
           return new Span<char>(this.Buffer, 0, this.Length).ToString();
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj is Bucket2)
        //    {
        //        return Equals((Bucket2)obj);
        //    }
        //    return false;
        //}
        //public bool Equals(Bucket2 other)
        //{
        //    // Check for same reference
        //    if (ReferenceEquals(this, other))
        //        return true;

        //    if (this.Offset != other.Offset || other.Length != this.Length)
        //    {
        //        return false;
        //    }

        //    for (var i = 0; i < this.Length; i++)
        //    {
        //        if (!this.Buffer[i].Equals(other.Buffer[i]))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;

        //}

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;
                for (var i = 0; i < this.Length; i++)
                {
                    hash += Buffer[i].GetHashCode();
                }
                return 31 * hash + this.Length + this.Offset;
            }
        }
    }
}
