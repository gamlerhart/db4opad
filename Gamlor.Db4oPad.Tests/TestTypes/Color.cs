using System;

namespace Gamlor.Db4oPad.Tests.TestTypes
{
    public sealed class Color : IEquatable<Color>
    {
        private static readonly Random RandomGenerator = new Random();
        private const byte GroundColor = byte.MaxValue / 4;

        public Color(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public static Color RandomColor()
        {
            lock (RandomGenerator)
            {
                return new Color(ColorComponent(), ColorComponent(), ColorComponent());
            }
        }

        private static byte ColorComponent()
        {
            return (byte)(RandomGenerator.Next((byte.MaxValue - GroundColor)) + GroundColor);
        }

        public byte Red { get; private set; }

        public byte Green { get; private set; }

        public byte Blue { get; private set; }

        public bool Equals(Color other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Red == Red && other.Green == Green && other.Blue == Blue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Color)) return false;
            return Equals((Color)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Red.GetHashCode();
                result = (result * 397) ^ Green.GetHashCode();
                result = (result * 397) ^ Blue.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Color left, Color right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !Equals(left, right);
        }
    }

}