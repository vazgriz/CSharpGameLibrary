using System;
using System.Runtime.InteropServices;

namespace CSGL.Graphics {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Color : IEquatable<Color> {
        public float r, g, b, a;

        public Color(float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(Color32 color) {
            r = color.r / 255f;
            g = color.g / 255f;
            b = color.b / 255f;
            a = color.a / 255f;
        }

        public bool Equals(Color other) {
            return r == other.r &&
                g == other.g &&
                b == other.b &&
                a == other.a;
        }

        public static bool operator ==(Color a, Color b) {
            return a.Equals(b);
        }

        public static bool operator !=(Color a, Color b) {
            return !a.Equals(b);
        }

        public override bool Equals(object other) {
            if (other is Color) {
                return Equals((Color)other);
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
