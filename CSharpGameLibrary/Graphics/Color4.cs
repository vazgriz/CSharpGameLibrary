using System;
using System.Runtime.InteropServices;

namespace CSGL.Graphics {
    public struct Color4 : IEquatable<Color4> {
        public float r, g, b, a;

        public Color4(float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color4(Color32 color) {
            r = color.r / 255f;
            g = color.g / 255f;
            b = color.b / 255f;
            a = color.a / 255f;
        }

        public bool Equals(Color4 other) {
            return r == other.r &&
                g == other.g &&
                b == other.b &&
                a == other.a;
        }

        public static bool operator ==(Color4 a, Color4 b) {
            return a.Equals(b);
        }

        public static bool operator !=(Color4 a, Color4 b) {
            return !a.Equals(b);
        }

        public override bool Equals(object other) {
            if (other is Color4) {
                return Equals((Color4)other);
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
