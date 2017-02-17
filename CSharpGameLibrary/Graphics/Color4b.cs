using System;
using System.Runtime.InteropServices;

namespace CSGL.Graphics {
    public struct Color4b : IEquatable<Color4b> {
        public byte r, g, b, a;

        public Color4b(byte r, byte g, byte b, byte a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color4b(int r, int g, int b, int a) : this((byte)r, (byte)g, (byte)b, (byte)a) { }

        public Color4b(Color4 color) {
            r = (byte)(System.Math.Min(System.Math.Max(color.r * 255, 0), 255));
            g = (byte)(System.Math.Min(System.Math.Max(color.g * 255, 0), 255));
            b = (byte)(System.Math.Min(System.Math.Max(color.b * 255, 0), 255));
            a = (byte)(System.Math.Min(System.Math.Max(color.a * 255, 0), 255));
        }

        public Color4b(Color3b color, byte a) : this(color.r, color.g, color.b, a) { }
        public Color4b(Color3b color, int a) : this(color, (byte)a) { }

        public Color4b(Color3 color, byte a) {
            r = (byte)(System.Math.Min(System.Math.Max(color.r * 255, 0), 255));
            g = (byte)(System.Math.Min(System.Math.Max(color.g * 255, 0), 255));
            b = (byte)(System.Math.Min(System.Math.Max(color.b * 255, 0), 255));
            this.a = a;
        }

        public Color4b(Color3 color, int a) : this(color, (byte)a) { }

        public uint ToUint() {
            uint result = 0;
            result &= r;
            result &= (uint)g << 8;
            result &= (uint)b << 16;
            result &= (uint)a << 24;
            return result;
        }

        public bool Equals(Color4b other) {
            return r == other.r &&
                g == other.g &&
                b == other.b &&
                a == other.a;
        }

        public static bool operator == (Color4b a, Color4b b) {
            return a.Equals(b);
        }

        public static bool operator != (Color4b a, Color4b b) {
            return !a.Equals(b);
        }

        public override bool Equals(object other) {
            if (other is Color4b) {
                return Equals((Color4b)other);
            }
            return false;
        }

        public override int GetHashCode() {
            return r.GetHashCode() ^
                g.GetHashCode() ^
                b.GetHashCode() ^
                a.GetHashCode();
        }
    }
}
