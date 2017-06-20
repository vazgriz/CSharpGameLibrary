using System;

namespace CSGL.GL4 {
    public struct GLsync : IEquatable<GLsync> {
        IntPtr ptr;

        public bool Equals(GLsync other) {
            return ptr == other.ptr;
        }

        public static bool operator == (GLsync a, GLsync b) {
            return a.Equals(b);
        }

        public static bool operator != (GLsync a, GLsync b) {
            return !a.Equals(b);
        }

        public override bool Equals(object obj) {
            if (obj is GLsync) {
                return Equals((GLsync)obj);
            }
            return false;
        }

        public override int GetHashCode() {
            return ptr.GetHashCode();
        }
    }
}
