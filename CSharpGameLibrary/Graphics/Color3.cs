using System;
using System.Runtime.InteropServices;

namespace CSGL.Graphics {
    public struct Color3 : IEquatable<Color3> {
        public float r, g, b;

        public Color3(float r, float g, float b) {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color3(Color3b color) {
            r = color.r / 255f;
            g = color.g / 255f;
            b = color.b / 255f;
        }

        public bool Equals(Color3 other) {
            return r == other.r &&
                g == other.g &&
                b == other.b;
        }

        public static bool operator ==(Color3 a, Color3 b) {
            return a.Equals(b);
        }

        public static bool operator !=(Color3 a, Color3 b) {
            return !a.Equals(b);
        }

        public override bool Equals(object other) {
            if (other is Color3) {
                return Equals((Color3)other);
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
