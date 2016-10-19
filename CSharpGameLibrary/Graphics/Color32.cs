using System;
using System.Runtime.InteropServices;

namespace CSGL.Graphics {
    public struct Color32 : IEquatable<Color32> {
        public byte r, g, b, a;

        public Color32(byte r, byte g, byte b, byte a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color32(int r, int g, int b, int a) : this((byte)r, (byte)g, (byte)b, (byte)a) { }

        public Color32(Color color) {
            r = (byte)(color.r * 255);
            g = (byte)(color.g * 255);
            b = (byte)(color.b * 255);
            a = (byte)(color.a * 255);
        }

        public uint ToUint() {
            uint result = 0;
            result &= r;
            result &= (uint)g << 8;
            result &= (uint)b << 16;
            result &= (uint)a << 24;
            return result;
        }

        public bool Equals(Color32 other) {
            return r == other.r &&
                g == other.g &&
                b == other.b &&
                a == other.a;
        }

        public static bool operator == (Color32 a, Color32 b) {
            return a.Equals(b);
        }

        public static bool operator != (Color32 a, Color32 b) {
            return !a.Equals(b);
        }

        public override bool Equals(object other) {
            if (other is Color32) {
                return Equals((Color32)other);
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
