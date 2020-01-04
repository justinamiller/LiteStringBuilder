using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringHelper
{
    internal readonly struct Bucket
    {
        public readonly char[] Buffer;
        public readonly int Length;
        public readonly int Offset;
        public readonly int OffsetLength;
        public Bucket(char[] buffer, int length, int offset)
        {
            this.Buffer = buffer;
            this.Length = length;
            this.Offset = offset;
            this.OffsetLength = length + offset;
        }

        public Bucket(Bucket bucket, int offset)
        {
            this.Buffer = bucket.Buffer;
            this.Length = bucket.Length;
            this.Offset = offset;

            this.OffsetLength = Length + offset;
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

        public override bool Equals(object obj)
        {
            if (obj is Bucket)
            {
                return Equals((Bucket)obj);
            }
            return false;
        }
        public bool Equals(Bucket other)
        {
            // Check for same reference
            if (ReferenceEquals(this, other))
                return true;

            if (this.Offset != other.Offset || other.Length != this.Length)
            {
                return false;
            }

            for (var i = 0; i < this.Length; i++)
            {
                if (!this.Buffer[i].Equals(other.Buffer[i]))
                {
                    return false;
                }
            }

            return true;
        }

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
