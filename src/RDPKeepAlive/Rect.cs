using System;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect : IEquatable<Rect>
    {
        internal int Left { get; set; }
        internal int Top { get; set; }
        internal int Right { get; set; }
        internal int Bottom { get; set; }

        public Rect(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Right = left + width;
            Bottom = top + height;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Left, Top, Right, Bottom);
        }

        public bool Equals(Rect other)
        {
            return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
        }

        public override bool Equals(object obj)
        {
            return obj is Rect other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Left.GetHashCode();
                hash = hash * 31 + Top.GetHashCode();
                hash = hash * 31 + Right.GetHashCode();
                hash = hash * 31 + Bottom.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rect left, Rect right)
        {
            return !left.Equals(right);
        }
    }
}
