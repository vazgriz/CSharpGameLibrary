using System;
using System.Runtime.InteropServices;

namespace CSGL.Graphics {
    public struct Color3b : IEquatable<Color3b> {
        public byte r, g, b;

        public Color3b(byte r, byte g, byte b) {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color3b(int r, int g, int b) : this((byte)r, (byte)g, (byte)b) { }

        public Color3b(Color3 color) {
            r = (byte)System.Math.Min(System.Math.Max(color.r * 255, 0), 255);
            g = (byte)System.Math.Min(System.Math.Max(color.g * 255, 0), 255);
            b = (byte)System.Math.Min(System.Math.Max(color.b * 255, 0), 255);
        }

        public uint ToUint() {
            uint result = 0;
            result &= r;
            result &= (uint)g << 8;
            result &= (uint)b << 16;
            return result;
        }

        public bool Equals(Color3b other) {
            return r == other.r &&
                g == other.g &&
                b == other.b;
        }

        public static bool operator ==(Color3b a, Color3b b) {
            return a.Equals(b);
        }

        public static bool operator !=(Color3b a, Color3b b) {
            return !a.Equals(b);
        }

        public override bool Equals(object other) {
            if (other is Color3b) {
                return Equals((Color3b)other);
            }
            return false;
        }

        public override int GetHashCode() {
            return r.GetHashCode() ^
                g.GetHashCode() ^
                b.GetHashCode();
        }
    }
}
